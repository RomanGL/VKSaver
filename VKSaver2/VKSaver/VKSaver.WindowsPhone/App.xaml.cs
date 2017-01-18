using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Store;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Unity;
using System.Globalization;
using VKSaver.Core.Services;
using System.Threading.Tasks;
using VKSaver.Core.Services.Interfaces;
using Windows.UI.Xaml;
using VKSaver.Core.Models.Player;
using Microsoft.Practices.ServiceLocation;
using VKSaver.Core.LinksExtractor;
using ModernDev.InTouch;
using Windows.UI.Core;
using IF.Lastfm.Core.Api;
using Yandex.Metrica;
using System.Collections.Generic;
using Windows.Networking.PushNotifications;
using Newtonsoft.Json;
using VKSaver.Core;
using VKSaver.Core.Models;
using VKSaver.Core.Models.Common;
using VKSaver.Core.ViewModels.Common;
using Yandex.Metrica.Push;
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
                _appLoaderService = new AppLoaderService(this);
                _frame = new WithLoaderFrame(_appLoaderService, this);

                HardwareButtons.BackPressed += (s, e) =>
                {
                    if (NavigationService.CanGoBack())
                    {
                        NavigationService.GoBack();
                        e.Handled = true;
                    }
                };

                _frame.Navigated += (s, e) =>
                {
                    var dict = new Dictionary<string, string>(1);
                    dict[MetricaConstants.NAVIGATION_PAGE] = e.SourcePageType.Name;

                    _metricaService?.LogEvent(MetricaConstants.NAVIGATION_EVENT, JsonConvert.SerializeObject(dict));
                };
                return _frame;
            };
            
            this.UnhandledException += App_UnhandledException;
            this.Suspending += App_Suspending;
            this.Resuming += App_Resuming;

            YandexMetrica.Config.CrashTracking = false;
        }

        private void App_Resuming(object sender, object e)
        {
            ActivateMetrica();
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

#if !DEBUG
                YandexMetrica.ReportUnhandledException(e.Exception);
#endif
                var locService = _container.Resolve<ILocService>();
                var notificationsService = _container.Resolve<IAppNotificationsService>();
                notificationsService.SendNotification(new AppNotification
                {
                    Title = locService["AppNotifications_FatalError_Title"],
                    Content = locService["AppNotifications_TouchToInfo_Content"],
                    Type = AppNotificationType.Error,
                    DestinationView = "ErrorView",
                    NavigationParameter = e.Message,
                    IsImportant = true
                });
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

            int settingsVersion = settingsService.Get(AppConstants.SETTINGS_VERSION_PARAMETER, 0);
            if (settingsVersion < AppConstants.SETTINGS_VERSION)
            {
                settingsService.Clear();
                settingsService.Set(AppConstants.SETTINGS_VERSION_PARAMETER, AppConstants.SETTINGS_VERSION);
            }

            _metricaService = new MetricaService();

            var inTouch = new InTouch();
            var lastAuth = new LastAuth(LAST_FM_API_KEY, LAST_FM_API_SECRET);

            var vkLoginService = new VKLoginService(settingsService, inTouch);
            if (vkLoginService.IsAuthorized)
                vkLoginService.InitializeInTouch();

            _container.RegisterInstance<IMetricaService>(_metricaService);
            _container.RegisterInstance<IServiceResolver>(this);
            _container.RegisterInstance<ISettingsService>(settingsService);
            _container.RegisterInstance(this.NavigationService);
            _container.RegisterInstance(this.SessionStateService);
            _container.RegisterInstance<IDispatcherWrapper>(this);
            _container.RegisterInstance<IAppLoaderService>(_appLoaderService);            
            _container.RegisterInstance<IVKLoginService>(vkLoginService);
            _container.RegisterInstance<InTouch>(inTouch);
            _container.RegisterInstance<ILastAuth>(lastAuth);
            _container.RegisterInstance<IAppNotificationsPresenter>(_frame);

            _container.RegisterType<ILocService, LocService>(new ContainerControlledLifetimeManager());
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
            _container.RegisterType<IVksmExtractionService, VksmExtractionService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IAdsService, AdsService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<ILaunchViewResolver, LaunchViewResolver>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IEmailService, EmailService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IFeedbackService, FeedbackService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IDeviceVibrationService, DeviceVibrationService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<ISoundService, SoundService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IAppNotificationsService, AppNotificationsService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IHttpFileService, HttpFileService>(new ContainerControlledLifetimeManager());

            _container.RegisterType<LastfmClient>(new ContainerControlledLifetimeManager(), 
                new InjectionFactory(InstanceFactories.ResolveLastfmClient));

            var playerService = _container.Resolve<PlayerService>();
            var downloadsService = _container.Resolve<DownloadsService>();
            var uploadsService = _container.Resolve<UploadsService>();
            
            _container.RegisterInstance<ISuspendingService>("s1", playerService);
            _container.RegisterInstance<ISuspendingService>("s2", downloadsService);
            _container.RegisterInstance<ISuspendingService>("s3", uploadsService);
            _container.RegisterType<ISuspendingService, TransferNotificationsService>("s4", new ContainerControlledLifetimeManager());

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
                var launchViewResolver = _container.Resolve<ILaunchViewResolver>();
                NavigationService.ClearHistory();

                if (!launchViewResolver.TryOpenSpecialViews() && await launchViewResolver.EnsureDatabaseUpdated() == false)
                    launchViewResolver.OpenDefaultView();
#if FULL
                _container.Resolve<IBetaService>().ExecuteAppLaunch();
#endif
                ActivatePush(null);
            };
            vkLoginService.UserLogout += async (s, e) =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    NavigationService.Navigate(AppConstants.DEFAULT_LOGIN_VIEW, null);
                    NavigationService.ClearHistory();
                });
            };
            inTouch.AuthorizationFailed += async (s, e) =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    NavigationService.Navigate(AppConstants.DEFAULT_LOGIN_VIEW, null);
                    NavigationService.ClearHistory();
                });
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

            var locService = _container.Resolve<ILocService>();
            locService.ApplyAppLanguage();

            base.OnInitialize(args);            
        }

        protected override async Task OnLaunchApplication(LaunchActivatedEventArgs args)
        {
            ActivateMetrica();

            var playerService = _container.Resolve<IPlayerService>();
            var vkLoginService = _container.Resolve<IVKLoginService>();
            var downloadsService = _container.Resolve<IDownloadsService>();
            var launchViewResolver = _container.Resolve<ILaunchViewResolver>();

            StartSuspendingServices();

            if (args.PreviousExecutionState == ApplicationExecutionState.ClosedByUser ||
                args.PreviousExecutionState == ApplicationExecutionState.NotRunning)
            {
                if (!launchViewResolver.TryOpenPromoView())
                {
                    if (vkLoginService.IsAuthorized)
                    {
                        if (!launchViewResolver.TryOpenSpecialViews() && await launchViewResolver.EnsureDatabaseUpdated() == false)
                        {
                            launchViewResolver.OpenDefaultView();
                        }
                    }
                    else
                        NavigationService.Navigate(AppConstants.DEFAULT_LOGIN_VIEW, null);
                }
            }

            if (vkLoginService.IsAuthorized)
            {
                try
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        var state = playerService.CurrentState;
                        if (state == PlayerState.Playing)
                            NavigationService.Navigate("PlayerView", null);
                    });                    
                }
                catch (Exception) { }
#if FULL
                _container.Resolve<IBetaService>().ExecuteAppLaunch();
#endif

                _container.Resolve<IFeedbackService>().ActivateFeedbackNotifier();
                ActivatePush(args);
            }            
        }

        protected override async Task OnFileActivatedAsync(FileActivatedEventArgs args)
        {
            ActivateMetrica();

            var mediaFilesProcessService = _container.Resolve<IMediaFilesProcessService>();
            var downloadsService = _container.Resolve<IDownloadsService>();
            var launchViewResolver = _container.Resolve<ILaunchViewResolver>();

            StartSuspendingServices();

            if (args.PreviousExecutionState == ApplicationExecutionState.NotRunning ||
                args.PreviousExecutionState == ApplicationExecutionState.ClosedByUser)
            {
                var playerService = _container.Resolve<IPlayerService>();
                var vkLoginService = _container.Resolve<IVKLoginService>();

                if (!launchViewResolver.TryOpenPromoView())
                {
                    if (vkLoginService.IsAuthorized)
                    {
                        if (!launchViewResolver.TryOpenSpecialViews() && await launchViewResolver.EnsureDatabaseUpdated() == false)
                        {
                            launchViewResolver.OpenDefaultView();
                        }
                    }
                    else
                        NavigationService.Navigate(AppConstants.DEFAULT_LOGIN_VIEW, null);
                }

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

        private async void ActivateMetrica()
        {
            await Task.Run(() => YandexMetrica.Activate(AppConstants.YANDEX_METRICA_API_KEY));
        }

        private async void ActivatePush(LaunchActivatedEventArgs e)
        {
            await Task.Run(() =>
            {
                YandexMetricaPush.Activate("***REMOVED***");
                if (e != null)
                    YandexMetricaPush.ProcessApplicationLaunch(e);
            });
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
                NavigationService.Navigate("ErrorView", $"Resolution failed: {type.Name}");
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

            return (T) res;
        }

        private IUnityContainer _container;
        private IAppLoaderService _appLoaderService;
        private IMetricaService _metricaService;
        private UnityServiceLocator _unityServiceLocator;
        private WithLoaderFrame _frame;

        private const string LAST_FM_API_KEY = "***REMOVED***";
        private const string LAST_FM_API_SECRET = "***REMOVED***";

        private const string VIEW_MODEL_FORMAT = "VKSaver.Core.ViewModels.{0}Model, VKSaver.Core.ViewModels, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        private const string VIEW_MODEL_CONTROLS_FORMAT = "VKSaver.Core.ViewModels.{0}ViewModel, VKSaver.Core.ViewModels, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        private const string VIEW_FORMAT = "VKSaver.Views.{0}";
        private const int VKSAVER_APP_ID = ***REMOVED***;
    }
}