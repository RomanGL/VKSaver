using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml.Controls;
using Windows.UI.ViewManagement;
using Windows.ApplicationModel.Store;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Unity;
using System.Globalization;
using OneTeam.SDK.LastFm.Services.Interfaces;
using OneTeam.SDK.LastFm.Services;
using VKSaver.Core.Services;
using System.Threading.Tasks;
using VKSaver.Core.Services.Interfaces;
using Windows.UI.Xaml;
using Windows.UI.Popups;
using VKSaver.Core.Models.Player;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using Microsoft.Practices.ServiceLocation;
using VKSaver.Core.LinksExtractor;
using ModernDev.InTouch;
using ICSharpCode.SharpZipLib.Zip;
using Windows.Storage;
using System.IO;
using Windows.UI.Core;
using IF.Lastfm.Core.Api;
using VKSaver.Core.Services.Database;
#if WINDOWS_PHONE_APP
using Windows.Phone.UI.Input;
using VKSaver.Controls;
#else
using Windows.UI.Popups;
using VKSaver.Controls;
#endif

namespace VKSaver
{
    public sealed partial class App : MvvmAppBase, IServiceResolver, IDispatcherWrapper
    {
        public App()
        {
            this.InitializeComponent();

            this.FrameFactory = () =>
            {
                _locService = new LocService();
                _appLoaderService = new AppLoaderService(_locService);
#if WINDOWS_PHONE_APP
                _frame = new WithLoaderFrame(_appLoaderService);

                HardwareButtons.BackPressed += (s, e) =>
                {
                    if (NavigationService.CanGoBack())
                    {
                        NavigationService.GoBack();
                        e.Handled = true;
                    }
                };

                return _frame;
#else
                _frame = new CustomFrame();
                return _frame;
#endif
            };
            
            this.UnhandledException += App_UnhandledException;
            this.Suspending += App_Suspending;
            this.Resuming += App_Resuming;
        }

        private void App_Resuming(object sender, object e)
        {
            StartSuspendingServices();
        }

        private void App_Suspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            StopSuspendingServices();
            deferral.Complete();
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var logService = _container.Resolve<ILogService>();
                logService.LogException(e.Exception);

                NavigationService.Navigate("ErrorView", null);
            }
            catch { }

            e.Handled = true;
        }

        public IUnityContainer Container { get { return _container; } }

        public CoreDispatcher Dispatcher { get; private set; }

        protected override void OnInitialize(IActivatedEventArgs args)
        {
            Dispatcher = Window.Current.Dispatcher;

            _container = new UnityContainer();
            _unityServiceLocator = new UnityServiceLocator(_container);
            ServiceLocator.SetLocatorProvider(() => _unityServiceLocator);

            ViewModelLocator.SetDefaultViewTypeToViewModelTypeResolver(GetViewModelType);

            var settingsService = new SettingsService();
            var inTouch = new InTouch();
            var lastAuth = new LastAuth(LAST_FM_API_KEY, LAST_FM_API_SECRET);

            var vkLoginService = new VKLoginService(settingsService, inTouch);
            if (vkLoginService.IsAuthorized)
                vkLoginService.InitializeInTouch();            

            _container.RegisterInstance<IServiceResolver>(this);
            _container.RegisterInstance<ISettingsService>(settingsService);
            _container.RegisterInstance(this.NavigationService);
            _container.RegisterInstance(this.SessionStateService);
            _container.RegisterInstance<IDispatcherWrapper>(this);       
            _container.RegisterInstance<ILFService>(new LFService(LAST_FM_API_KEY));
            _container.RegisterInstance<IAppLoaderService>(_appLoaderService);
            _container.RegisterInstance<ILocService>(_locService);
            _container.RegisterInstance<IVKLoginService>(vkLoginService);
            _container.RegisterInstance<InTouch>(inTouch);
            _container.RegisterInstance<ILastAuth>(lastAuth);          

            _container.RegisterType<IInTouchWrapper, InTouchWrapper>(new ContainerControlledLifetimeManager());
            _container.RegisterType<ILogService, LogService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IDialogsService, DialogsService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IPurchaseService, PurchaseService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<INetworkInfoService, NetworkInfoService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<ITracksShuffleService, TracksShuffleService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IPlayerPlaylistService, PlayerPlaylistService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<ICultureProvider, CultureProvider>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IGrooveMusicService, GrooveMusicService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IImagesCacheService, ImagesCacheService>(new ContainerControlledLifetimeManager());            
            _container.RegisterType<IMusicCacheService, MusicCacheService>(new ContainerControlledLifetimeManager());            
            _container.RegisterType<IVideoLinksExtractor, VideoLinksExtractor>(new ContainerControlledLifetimeManager());
            _container.RegisterType<ILastFmLoginService, LastFmLoginService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IUploadsPreprocessor, UploadsPreprocessor>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IUploadsPostprocessor, UploadsPostprocessor>(new ContainerControlledLifetimeManager());
            _container.RegisterType<ILibraryDatabaseService, LibraryDatabaseService>(new ContainerControlledLifetimeManager());

            var playerService = _container.Resolve<PlayerService>();
            var downloadsService = _container.Resolve<DownloadsService>();
            var uploadsService = _container.Resolve<UploadsService>();
            
            _container.RegisterInstance<ISuspendingService>("s1", playerService);
            _container.RegisterInstance<ISuspendingService>("s2", downloadsService);
            _container.RegisterInstance<ISuspendingService>("s3", uploadsService);

            _container.RegisterInstance<IPlayerService>(playerService);
            _container.RegisterInstance<IDownloadsService>(downloadsService);
            _container.RegisterInstance<IUploadsService>(uploadsService);

            _container.RegisterType<IDownloadsServiceHelper, DownloadsServiceHelper>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IMediaFilesProcessService, MediaFilesProcessService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IUploadsServiceHelper, UploadsServiceHelper>(new ContainerControlledLifetimeManager());

#if FULL
            _container.RegisterType<IBetaService, BetaService>(new ContainerControlledLifetimeManager());
#endif

            vkLoginService.UserLogin += async (s, e) =>
            {
                if (await TryOpenFirstStartView() == false)
                    NavigationService.Navigate("MainView", null);
#if FULL
                _container.Resolve<IBetaService>().ExecuteAppLaunch();
#endif
            };
            vkLoginService.UserLogout += (s, e) =>
            {
                NavigationService.Navigate("LoginView", null);
                NavigationService.ClearHistory();
            };
            inTouch.AuthorizationFailed += (s, e) =>
            {
                NavigationService.Navigate("LoginView", null);
                NavigationService.ClearHistory();
            };
            
#if DEBUG
            LoadPurchaseFile();
#endif

            var purchaseService = _container.Resolve<PurchaseService>();
            if (!purchaseService.IsFullVersionPurchased)
            {
                settingsService.Remove(PlayerConstants.PLAYER_SCROBBLE_MODE);
            }
            
            var lastFmLoginService = _container.Resolve<ILastFmLoginService>();
            if (lastFmLoginService.IsAuthorized)
                lastFmLoginService.InitializeLastAuth();

            StartSuspendingServices();

            base.OnInitialize(args);            
        }

        protected override async Task OnLaunchApplication(LaunchActivatedEventArgs args)
        {
            var playerService = _container.Resolve<IPlayerService>();
            var vkLoginService = _container.Resolve<IVKLoginService>();
            var downloadsService = _container.Resolve<IDownloadsService>();
            var settingsService = _container.Resolve<ISettingsService>();

            StartSuspendingServices();

            if (args.PreviousExecutionState == ApplicationExecutionState.ClosedByUser ||
                args.PreviousExecutionState == ApplicationExecutionState.NotRunning)
            {
                if (settingsService.Get(AppConstants.CURRENT_PROMO_INDEX_PARAMETER, 0) < AppConstants.CURRENT_PROMO_INDEX)
                    NavigationService.Navigate("PromoView", null);
                else if (vkLoginService.IsAuthorized)
                {
                    if (await TryOpenFirstStartView() == false)
                    {
                        NavigationService.Navigate("MainView", null);
                        try
                        {
                            var state = playerService.CurrentState;
                            if (state == PlayerState.Playing)
                                NavigationService.Navigate("PlayerView", null);
                        }
                        catch (Exception) { }
                    }
                }
                else
                    NavigationService.Navigate("LoginView", null);
            }

            if (vkLoginService.IsAuthorized)
            {
#if FULL
                _container.Resolve<IBetaService>().ExecuteAppLaunch();
#endif
            }

            var logService = _container.Resolve<ILogService>();
            logService.LogText("App started");
        }

        protected override async Task OnFileActivatedAsync(FileActivatedEventArgs args)
        {
            var mediaFilesProcessService = _container.Resolve<IMediaFilesProcessService>();
            var downloadsService = _container.Resolve<IDownloadsService>();
            var settingsService = _container.Resolve<ISettingsService>();

            StartSuspendingServices();

            if (args.PreviousExecutionState == ApplicationExecutionState.NotRunning ||
                args.PreviousExecutionState == ApplicationExecutionState.ClosedByUser)
            {
                var playerService = _container.Resolve<IPlayerService>();
                var vkLoginService = _container.Resolve<IVKLoginService>();

                if (settingsService.Get(AppConstants.CURRENT_PROMO_INDEX_PARAMETER, 0) < AppConstants.CURRENT_PROMO_INDEX)
                    NavigationService.Navigate("PromoView", null);
                else if (vkLoginService.IsAuthorized)
                {
                    if (await TryOpenFirstStartView() == false)
                        NavigationService.Navigate("MainView", null);
                }
                else
                    NavigationService.Navigate("LoginView", null);

                if (vkLoginService.IsAuthorized)
                {
#if FULL
                    _container.Resolve<IBetaService>().ExecuteAppLaunch();
#endif
                }
            }

            await mediaFilesProcessService.ProcessFiles(args.Files);            
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);

            var rootFrame = _frame;
            var continuationManager = new ContinuationManager();

            var continuationEventArgs = args as IContinuationActivatedEventArgs;
            if (continuationEventArgs != null)
            {
                if (rootFrame != null)
                {
                    continuationManager.Continue(continuationEventArgs, rootFrame);
                }
            }
        }

        private async Task<bool> TryOpenFirstStartView()
        {
            var settingsService = _container.Resolve<ISettingsService>();
            if (settingsService.Get(AppConstants.CURRENT_FIRST_START_INDEX_PARAMETER, 0) < AppConstants.CURRENT_FIRST_START_INDEX)
            {
                string currentView = settingsService.Get<string>(AppConstants.CURRENT_FIRST_START_VIEW_PARAMETER);
                if (currentView == null)
                    NavigationService.Navigate("FirstStartView", null);
                else if (currentView != "Completed")
                    NavigationService.Navigate("FirstStartRetryView", null);

                return true;
            }
            else if (settingsService.Get(AppConstants.CURRENT_LIBRARY_INDEX_PARAMETER, 0) < AppConstants.CURRENT_LIBRARY_INDEX)
            {
                NavigationService.Navigate("UpdatingDatabaseView", null);
                return true;
            }
            else
            {
                try
                {
                    var libraryDb = await ApplicationData.Current.LocalFolder.GetFileAsync(LibraryDatabase.DATABASE_FILE_NAME);
                }
                catch (Exception)
                {
                    NavigationService.Navigate("UpdatingDatabaseView", null);
                    return true;
                }
            }

            return false;
        }

        private void StartSuspendingServices()
        {
            var suspendingServices = _container.ResolveAll<ISuspendingService>();
            foreach (var service in suspendingServices)
            {
                service.StartService();
            }
        }

        private void StopSuspendingServices()
        {
            var suspendingServices = _container.ResolveAll<ISuspendingService>();
            foreach (var service in suspendingServices)
            {
                service.StopService();
            }
        }

#if DEBUG
        /// <summary>
        /// Загружает файл списка покупок.
        /// </summary>
        private async void LoadPurchaseFile()
        {
            var proxyDataFolder = await Package.Current.InstalledLocation.GetFolderAsync("Data");
            var proxyFile = await proxyDataFolder.GetFileAsync("InAppPurchase.xml");
            await CurrentAppSimulator.ReloadSimulatorAsync(proxyFile);
        }
#endif
        
        /// <summary>
        /// Возвращает тип модели представления для типа представления.
        /// </summary>
        /// <param name="viewType">Тип представления.</param>
        private Type GetViewModelType(Type viewType)
        {
            string viewModelTypeName = null;
            if (viewType.Name.EndsWith("View"))
                viewModelTypeName = String.Format(CultureInfo.InvariantCulture, VIEW_MODEL_FORMAT, viewType.Name);
            else
                viewModelTypeName = String.Format(CultureInfo.InvariantCulture, VIEW_MODEL_CONTROLS_FORMAT, viewType.Name);

            return Type.GetType(viewModelTypeName);
        }

        protected override Type GetPageType(string pageToken)
        {
            var pageType = Type.GetType(String.Format(CultureInfo.InvariantCulture, VIEW_FORMAT, pageToken));
            if (pageType == null)
                return Type.GetType(String.Format(CultureInfo.InvariantCulture, VIEW_FORMAT, "ErrorView"));
            return pageType;
        }

        protected override object Resolve(Type type)
        {
            try
            {
                return _container.Resolve(type);
            }
            catch (Exception)
            {
                NavigationService.Navigate("ErrorView", null);
                return null;
            }
        }

        public T Resolve<T>() where T: class
        {
            return Resolve<T>(null);
        }

        public T Resolve<T>(string name) where T: class
        {
            object res = null;
            if (name == null)
                res = _container.Resolve(typeof(T));
            else
                res = _container.Resolve(typeof(T), name);

            if (res == null)
                return null;
            return (T)res;
        }

        private IUnityContainer _container;
        private IAppLoaderService _appLoaderService;
        private ILocService _locService;
        private UnityServiceLocator _unityServiceLocator;
        private Frame _frame;

        private const string LAST_FM_API_KEY = "***REMOVED***";
        private const string LAST_FM_API_SECRET = "***REMOVED***";

        private const string VIEW_MODEL_FORMAT = "VKSaver.Core.ViewModels.{0}Model, VKSaver.Core.ViewModels, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        private const string VIEW_MODEL_CONTROLS_FORMAT = "VKSaver.Core.ViewModels.{0}ViewModel, VKSaver.Core.ViewModels, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        private const string VIEW_FORMAT = "VKSaver.Views.{0}";
        private const int VKSAVER_APP_ID = ***REMOVED***;
    }
}