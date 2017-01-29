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
            
            _container.RegisterInstance<IServiceResolver>(this);
            _container.RegisterInstance(this.NavigationService);
            _container.RegisterInstance(this.SessionStateService);
            _container.RegisterInstance<IDispatcherWrapper>(this);
            _container.RegisterInstance<IAppLoaderService>(_appLoaderService);
            _container.RegisterInstance<IAppNotificationsPresenter>(_frame);

            _container.RegisterType<ISettingsService, SettingsService>(new ContainerControlledLifetimeManager(),
                new InjectionFactory(InstanceFactories.ResolveSettingsService));
            _container.RegisterType<InTouch>(new ContainerControlledLifetimeManager(),
                new InjectionFactory(InstanceFactories.ResolveInTouch));
            _container.RegisterType<ILastAuth, LastAuth>(new ContainerControlledLifetimeManager(),
                new InjectionFactory(InstanceFactories.ResolveLastAuth));
            _container.RegisterType<LastfmClient>(new ContainerControlledLifetimeManager(),
                new InjectionFactory(InstanceFactories.ResolveLastfmClient));
            _container.RegisterType<IVKLoginService, VKLoginService>(new ContainerControlledLifetimeManager(),
                new InjectionFactory(InstanceFactories.ResolveVKLoginService));

            _container.RegisterType<IMetricaService, MetricaService>(new ContainerControlledLifetimeManager());
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
            _container.RegisterType<IProtocolHandler, ProtocolHandler>(new TransientLifetimeManager());
            _container.RegisterType<IHttpFileService, HttpFileService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IVKAdService, VKAdService>(new TransientLifetimeManager());
            _container.RegisterType<IPlayerService, PlayerService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IDownloadsService, DownloadsService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IUploadsService, UploadsService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<INotificationsService, NotificationsService>(new ContainerControlledLifetimeManager());

            _container.RegisterType<ISuspendingService, TransferNotificationsService>("s1",
                new ContainerControlledLifetimeManager());
            _container.RegisterType<ISuspendingService>("s2",
                new InjectionFactory(c => c.Resolve<PlayerService>()));
            _container.RegisterType<ISuspendingService>("s3",
                new InjectionFactory(c => c.Resolve<DownloadsService>()));
            _container.RegisterType<ISuspendingService>("s4",
                new InjectionFactory(c => c.Resolve<UploadsService>()));

            _container.RegisterType<IDownloadsServiceHelper, DownloadsServiceHelper>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IMediaFilesProcessService, MediaFilesProcessService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IUploadsServiceHelper, UploadsServiceHelper>(new ContainerControlledLifetimeManager());
            _container.RegisterType<ISuspendingService, SuspendingServicesManager>(new TransientLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedArrayParameter<ISuspendingService>(
                        new ResolvedParameter<ISuspendingService>("s1"),
                        new ResolvedParameter<ISuspendingService>("s2"),
                        new ResolvedParameter<ISuspendingService>("s3"),
                        new ResolvedParameter<ISuspendingService>("s4"))));

#if FULL
            _container.RegisterType<IBetaService, BetaService>(new TransientLifetimeManager());
#endif

            _metricaService = _container.Resolve<IMetricaService>();
            var vkLoginService = _container.Resolve<IVKLoginService>();
            vkLoginService.UserLogin += async (s, e) =>
            {
                var launchViewResolver = _container.Resolve<ILaunchViewResolver>();
                NavigationService.ClearHistory();

                if (!launchViewResolver.TryOpenSpecialViews() && await launchViewResolver.EnsureDatabaseUpdated() == false)
                    launchViewResolver.OpenDefaultView();
#if FULL
                _container.Resolve<IBetaService>().ExecuteAppLaunch();
#endif
            };
            vkLoginService.UserLogout += async (s, e) =>
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
                _container.Resolve<ISettingsService>().Remove(PlayerConstants.PLAYER_SCROBBLE_MODE);
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
            
            var vkLoginService = _container.Resolve<IVKLoginService>();
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
                        var state = _container.Resolve<IPlayerService>().CurrentState;
                        if (state == PlayerState.Playing)
                            NavigationService.Navigate("PlayerView", null);
                    });                    
                }
                catch (Exception) { }
#if FULL
                _container.Resolve<IBetaService>().ExecuteAppLaunch();
#endif
                await _container.Resolve<INotificationsService>().ActivateYandexPushAsync();
                _container.Resolve<IFeedbackService>().ActivateFeedbackNotifier();
            } 
            
            if (_container.Resolve<INotificationsService>().IsYandexPushActivated)
                YandexMetricaPush.ProcessApplicationLaunch(args);

            //_container.Resolve<IProtocolHandler>().ProcessProtocol("?yamp_l=vksaver%3A%2F%2Fvkad%2F-138475410_3&yamp_i=m%3D49874%26cor%3Dddd8ae32-0322-4881-b526-c96c1a708ccc");
            if (args.Kind == ActivationKind.Protocol || (args.Kind == ActivationKind.Launch && !String.IsNullOrEmpty(args.Arguments)))
                _container.Resolve<IProtocolHandler>().ProcessProtocol(args.Arguments);    
        }

        protected override async Task OnFileActivatedAsync(FileActivatedEventArgs args)
        {
            ActivateMetrica();

            var mediaFilesProcessService = _container.Resolve<IMediaFilesProcessService>();
            var launchViewResolver = _container.Resolve<ILaunchViewResolver>();

            StartSuspendingServices();

            if (args.PreviousExecutionState == ApplicationExecutionState.NotRunning ||
                args.PreviousExecutionState == ApplicationExecutionState.ClosedByUser)
            {
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
            _container.Resolve<ISuspendingService>().StartService();
        }

        private void StopSuspendingServices()
        {
            _container.Resolve<ISuspendingService>().StopService();
        }

        private async void ActivateMetrica()
        {
            await Task.Run(() => YandexMetrica.Activate(AppConstants.YANDEX_METRICA_API_KEY));
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

        private const string VIEW_MODEL_FORMAT = "VKSaver.Core.ViewModels.{0}Model, VKSaver.Core.ViewModels, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        private const string VIEW_MODEL_CONTROLS_FORMAT = "VKSaver.Core.ViewModels.{0}ViewModel, VKSaver.Core.ViewModels, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        private const string VIEW_FORMAT = "VKSaver.Views.{0}";
    }
}