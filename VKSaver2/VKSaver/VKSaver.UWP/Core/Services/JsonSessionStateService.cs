using Newtonsoft.Json;
using Prism.Windows.AppModel;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;

namespace VKSaver.Core.Services
{
    public sealed class JsonSessionStateService : ISessionStateService
    {
        private Dictionary<string, object> _sessionState = new Dictionary<string, object>();
        private readonly List<Type> _knownTypes = new List<Type>();
        private const string SessionStateFileName = "_uwpAppState.json";

        /// <summary>
        /// Provides access to global session state for the current session. This state is
        /// serialized by <see cref="SaveAsync"/> and restored by
        /// <see cref="RestoreSessionStateAsync"/>, so values must be serializable by
        /// <see cref="DataContractSerializer"/> and should be as compact as possible. Strings
        /// and other self-contained data types are strongly recommended.
        /// </summary>
        public Dictionary<string, object> SessionState
        {
            get { return _sessionState; }
        }

        /// <summary>
        /// Adds a type to the list of custom types provided to the <see cref="DataContractSerializer"/> when
        /// reading and writing session state. Initially empty, additional types may be
        /// added to customize the serialization process.
        /// </summary>
        public void RegisterKnownType(Type type)
        {
            _knownTypes.Add(type);
        }

        public async Task SaveAsync()
        {
            try
            {
                // Save the navigation state for all registered frames
                foreach (var weakFrameReference in _registeredFrames)
                {
                    IFrameFacade frame;
                    if (weakFrameReference.TryGetTarget(out frame))
                    {
                        SaveFrameNavigationState(frame);
                    }
                }

                JsonSerializer serializer = new JsonSerializer
                {
                    TypeNameHandling = TypeNameHandling.All,
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                };

                // Serialize the session state synchronously to avoid asynchronous access to shared
                // state
                using (MemoryStream sessionData = new MemoryStream())
                using (var jsonTextWriter = new JsonTextWriter(new StreamWriter(sessionData)))
                {
                    serializer.Serialize(jsonTextWriter, _sessionState);

                    // Flush to the underyling stream.
                    jsonTextWriter.Flush();

                    // Get an output stream for the SessionState file and write the state asynchronously
                    StorageFile file =
                        await
                            ApplicationData.Current.LocalFolder.CreateFileAsync(SessionStateFileName,
                                CreationCollisionOption.ReplaceExisting);
                    using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        sessionData.Seek(0, SeekOrigin.Begin);
                        var provider = new DataProtectionProvider("LOCAL=user");

                        // Encrypt the session data and write it to disk.
                        await provider.ProtectStreamAsync(sessionData.AsInputStream(), fileStream);
                        await fileStream.FlushAsync();
                    }
                }
            }
            catch (Exception e)
            {
                throw new SessionStateServiceException(e);
            }
        }

        /// <summary>
        /// Restores previously saved <see cref="SessionState"/>.
        /// </summary>
        /// <returns>An asynchronous task that reflects when session state has been read. The
        /// content of <see cref="SessionState"/> should not be relied upon until this task
        /// completes.</returns>
        public async Task RestoreSessionStateAsync()
        {
            _sessionState = new Dictionary<String, Object>();

            try
            {
                JsonSerializer serializer = new JsonSerializer
                {
                    TypeNameHandling = TypeNameHandling.All,
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                };

                // Get the input stream for the SessionState file
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(SessionStateFileName);

                using (IInputStream inStream = await file.OpenSequentialReadAsync())
                using (var memoryStream = new MemoryStream())
                {
                    var provider = new DataProtectionProvider("LOCAL=user");

                    // Decrypt the prevously saved session data.
                    await provider.UnprotectStreamAsync(inStream, memoryStream.AsOutputStream());
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    using (var reader = new JsonTextReader(new StreamReader(memoryStream)))
                    {
                        // Deserialize the Session State
                        _sessionState = serializer.Deserialize<Dictionary<string, object>>(reader);
                    }
                }
            }
            catch (Exception e)
            {
                throw new SessionStateServiceException(e);
            }
        }

        /// <summary>
        /// Any <see cref="Frame"/> instances registered with <see cref="RegisterFrame"/> will 
        /// restore their prior navigation state, which in turn gives their active <see cref="Page"/> 
        /// an opportunity restore its state.
        /// 
        /// This method requires that RestoreSessionStateAsync be called prior to this method.
        /// </summary>
        public void RestoreFrameState()
        {
            try
            {
                // Restore any registered frames to their saved state
                foreach (var weakFrameReference in _registeredFrames)
                {
                    IFrameFacade frame;
                    if (weakFrameReference.TryGetTarget(out frame))
                    {
                        frame.ClearValue(FrameSessionStateProperty);
                        RestoreFrameNavigationState(frame);
                    }
                }
            }
            catch (Exception e)
            {
                throw new SessionStateServiceException(e);
            }
        }

        private static DependencyProperty FrameSessionStateKeyProperty =
            DependencyProperty.RegisterAttached("_FrameSessionStateKey", typeof(String), typeof(SessionStateService), null);
        private static DependencyProperty FrameSessionStateProperty =
            DependencyProperty.RegisterAttached("_FrameSessionState", typeof(Dictionary<String, Object>), typeof(SessionStateService), null);
        private static List<WeakReference<IFrameFacade>> _registeredFrames = new List<WeakReference<IFrameFacade>>();
        
        /// <summary>
        /// Registers a <see cref="Frame"/> instance to allow its navigation history to be saved to
        /// and restored from <see cref="SessionState"/>. Frames should be registered once
        /// immediately after creation if they will participate in session state management. Upon
        /// registration, if state has already been restored for the specified key,
        /// the navigation history will immediately be restored. Subsequent invocations of
        /// <see cref="RestoreFrameState"/> will also restore navigation history.
        /// </summary>
        /// <param name="frame">An instance whose navigation history should be managed by
        /// <see cref="SessionStateServiceException"/></param>
        /// <param name="sessionStateKey">A unique key into <see cref="SessionState"/> used to
        /// store navigation-related information.</param>
        public void RegisterFrame(IFrameFacade frame, String sessionStateKey)
        {
            if (frame == null) throw new ArgumentNullException("frame");

            if (frame.GetValue(FrameSessionStateKeyProperty) != null)
            {
                throw new InvalidOperationException("FrameAlreadyRegisteredWithKey");
            }

            if (frame.GetValue(FrameSessionStateProperty) != null)
            {
                throw new InvalidOperationException("FrameRegistrationRequirement");
            }

            // Use a dependency property to associate the session key with a frame, and keep a list of frames whose
            // navigation state should be managed
            frame.SetValue(FrameSessionStateKeyProperty, sessionStateKey);
            _registeredFrames.Add(new WeakReference<IFrameFacade>(frame));

            // Check to see if navigation state can be restored
            RestoreFrameNavigationState(frame);
        }

        /// <summary>
        /// Disassociates a <see cref="Frame"/> previously registered by <see cref="RegisterFrame"/>
        /// from <see cref="SessionState"/>. Any navigation state previously captured will be
        /// removed.
        /// </summary>
        /// <param name="frame">An instance whose navigation history should no longer be
        /// managed.</param>
        public void UnregisterFrame(IFrameFacade frame)
        {
            // Remove session state and remove the frame from the list of frames whose navigation
            // state will be saved (along with any weak references that are no longer reachable)
            SessionState.Remove((String)frame.GetValue(FrameSessionStateKeyProperty));
            _registeredFrames.RemoveAll((weakFrameReference) =>
            {
                IFrameFacade testFrame;
                return !weakFrameReference.TryGetTarget(out testFrame) || testFrame == frame;
            });
        }

        /// <summary>
        /// Provides storage for session state associated with the specified <see cref="Frame"/>.
        /// Frames that have been previously registered with <see cref="RegisterFrame"/> have
        /// their session state saved and restored automatically as a part of the global
        /// <see cref="SessionState"/>. Frames that are not registered have transient state
        /// that can still be useful when restoring pages that have been discarded from the
        /// navigation cache.
        /// </summary>
        /// <remarks>Apps may choose to rely on <see cref="VisualStateAwarePage"/> to manage
        /// page-specific state instead of working with Frame session state directly.</remarks>
        /// <param name="frame">The instance for which session state is desired.</param>
        /// <returns>A collection of state, subject to the same serialization mechanism as
        /// <see cref="SessionState"/>.</returns>
        public Dictionary<String, Object> GetSessionStateForFrame(IFrameFacade frame)
        {
            if (frame == null) throw new ArgumentNullException("frame");

            var frameState = (Dictionary<String, Object>)frame.GetValue(FrameSessionStateProperty);

            if (frameState == null)
            {
                var frameSessionKey = (String)frame.GetValue(FrameSessionStateKeyProperty);
                if (frameSessionKey != null)
                {
                    // Registered frames reflect the corresponding session state
                    if (!_sessionState.ContainsKey(frameSessionKey))
                    {
                        _sessionState[frameSessionKey] = new Dictionary<String, Object>();
                    }
                    frameState = (Dictionary<String, Object>)_sessionState[frameSessionKey];
                }
                else
                {
                    // Frames that aren't registered have transient state
                    frameState = new Dictionary<String, Object>();
                }
                frame.SetValue(FrameSessionStateProperty, frameState);
            }
            return frameState;
        }

        private void RestoreFrameNavigationState(IFrameFacade frame)
        {
            var frameState = GetSessionStateForFrame(frame);
            if (frameState.ContainsKey("Navigation"))
            {
                frame.SetNavigationState((String)frameState["Navigation"]);
            }
        }

        private void SaveFrameNavigationState(IFrameFacade frame)
        {
            var frameState = GetSessionStateForFrame(frame);
            frameState["Navigation"] = frame.GetNavigationState();
        }
    }
}
