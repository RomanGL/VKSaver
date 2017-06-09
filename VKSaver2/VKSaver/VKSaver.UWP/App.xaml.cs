using IF.Lastfm.Core.Api;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using ModernDev.InTouch;
using Newtonsoft.Json;
using Prism.Mvvm;
using Prism.Unity.Windows;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using VKSaver.Controls;
using VKSaver.Core;
using VKSaver.Core.LinksExtractor;
using VKSaver.Core.Models;
using VKSaver.Core.Models.Player;
using VKSaver.Core.Services;
using VKSaver.Core.Services.Interfaces;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using VKSaver.Common;
using VKSaver.Core.ViewModels;
using Yandex.Metrica;
using Yandex.Metrica.Push;
using Windows.ApplicationModel.Store;
using Windows.ApplicationModel;
using Prism.Events;

namespace VKSaver
{
    public sealed partial class App : PrismUnityApplication, IServiceResolver, IDispatcherWrapper
    {
        public App()
        {
            this.InitializeComponent();
            this.UnhandledException += App_UnhandledException;
            //this.RequiresPointerMode = ApplicationRequiresPointerMode.WhenRequested;
            YandexMetrica.Config.CrashTracking = false;
        }

        protected override Frame OnCreateRootFrame()
        {
            _appLoaderService = new AppLoaderService(this);
            _frame = new Frame();

            _frame.Navigated += (s, e) =>
            {
                var dict = new Dictionary<string, string>(1);
                dict[MetricaConstants.NAVIGATION_PAGE] = e.SourcePageType.Name;

                _metricaService?.LogEvent(MetricaConstants.NAVIGATION_EVENT, JsonConvert.SerializeObject(dict));
            };

            return _frame;
        }

        protected override UIElement CreateShell(Frame rootFrame)
        {
            Dispatcher = Window.Current.Dispatcher;
            var compositor = ElementCompositionPreview.GetElementVisual(rootFrame).Compositor;
            SurfaceLoader.Initialize(compositor);
            Toolkit.Animations.SurfaceLoader.Initialize(compositor);
            
            _shell.CurrentFrame = rootFrame;
            return _shell;
        }

        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            var view = ApplicationView.GetForCurrentView();
            view.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            //view.SetPreferredMinSize(new Size(320, 500));

            //view.TitleBar.BackgroundColor = (Color)Resources["TitleBarBackgroundThemeColor"];
            //view.TitleBar.ForegroundColor = (Color)Resources["TitleBarForegroundThemeColor"];
            //view.TitleBar.InactiveBackgroundColor = (Color)Resources["TitleBarBackgroundThemeColor"];
            //view.TitleBar.InactiveForegroundColor = (Color)Resources["TitleBarInactiveForegroundThemeColor"];

            //view.TitleBar.ButtonForegroundColor = (Color)Resources["TitleBarForegroundThemeColor"];
            //view.TitleBar.ButtonHoverForegroundColor = (Color)Resources["TitleBarForegroundThemeColor"];
            //view.TitleBar.ButtonPressedForegroundColor = (Color)Resources["TitleBarForegroundThemeColor"];

            //view.TitleBar.ButtonHoverBackgroundColor = (Color)Resources["TitleBarButtonHoverBackgroundThemeColor"];
            //view.TitleBar.ButtonPressedBackgroundColor = (Color)Resources["TitleBarButtonPressedBackgroundThemeColor"];

            //view.TitleBar.ButtonInactiveForegroundColor = (Color)Resources["TitleBarInactiveForegroundThemeColor"];

            //view.TitleBar.ButtonBackgroundColor = (Color)Resources["TitleBarBackgroundThemeColor"];
            //view.TitleBar.ButtonInactiveBackgroundColor = (Color)Resources["TitleBarBackgroundThemeColor"];
            base.OnWindowCreated(args);
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var logService = Container.Resolve<ILogService>();
                logService.LogException(e.Exception);

#if !DEBUG
                YandexMetrica.ReportUnhandledException(e.Exception);
#endif
                var locService = Container.Resolve<ILocService>();
                var notificationsService = Container.Resolve<IAppNotificationsService>();
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
        
        public CoreDispatcher Dispatcher { get; private set; }

        protected override void ConfigureViewModelLocator()
        {
            _unityServiceLocator = new UnityServiceLocator(Container);
            ServiceLocator.SetLocatorProvider(() => _unityServiceLocator);
            ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver(GetViewModelType);
            base.ConfigureViewModelLocator();
        }

        protected override Task OnInitializeAsync(IActivatedEventArgs args)
        {
            Container.RegisterInstance<IServiceResolver>(this);
            Container.RegisterInstance<INavigationService>("PrismNavigationService", NavigationService);
            Container.RegisterInstance(this.SessionStateService);
            Container.RegisterInstance<IDispatcherWrapper>(this);
            Container.RegisterInstance<IAppLoaderService>(_appLoaderService);
            Container.RegisterInstance<IEventAggregator>(this.EventAggregator);

            Container.RegisterType<INavigationService, VKSaverNavigationService>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedParameter<INavigationService>("PrismNavigationService"),
                    new ResolvedParameter<IVKLoginService>(),
                    new ResolvedParameter<IMetricaService>(),
                    new ResolvedParameter<IDispatcherWrapper>(),
                    new ResolvedParameter<IServiceResolver>()));

            Container.RegisterType<ISettingsService, SettingsService>(new ContainerControlledLifetimeManager(),
                new InjectionFactory(InstanceFactories.ResolveSettingsService));
            Container.RegisterType<InTouch>(new ContainerControlledLifetimeManager(),
                new InjectionFactory(InstanceFactories.ResolveInTouch));
            Container.RegisterType<ILastAuth, LastAuth>(new ContainerControlledLifetimeManager(),
                new InjectionFactory(InstanceFactories.ResolveLastAuth));
            Container.RegisterType<LastfmClient>(new ContainerControlledLifetimeManager(),
                new InjectionFactory(InstanceFactories.ResolveLastfmClient));
            Container.RegisterType<IVKLoginService, VKLoginService>(new ContainerControlledLifetimeManager(),
                new InjectionFactory(InstanceFactories.ResolveVKLoginService));

            Container.RegisterType<IMetricaService, MetricaService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ILocService, LocService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IInTouchWrapper, InTouchWrapper>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ILogService, LogService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IDialogsService, DialogsService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IPurchaseService, PurchaseService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<INetworkInfoService, NetworkInfoService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ITracksShuffleService, TracksShuffleService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IPlayerPlaylistService, PlayerPlaylistService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ICultureProvider, CultureProvider>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IGrooveMusicService, GrooveMusicService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IImagesCacheService, ImagesCacheService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IMusicCacheService, MusicCacheService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IVideoLinksExtractor, VideoLinksExtractor>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ILastFmLoginService, LastFmLoginService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IUploadsPreprocessor, UploadsPreprocessor>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IUploadsPostprocessor, UploadsPostprocessor>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ILibraryDatabaseService, LibraryDatabaseService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IVksmExtractionService, VksmExtractionService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IAdsService, AdsService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IEmailService, EmailService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IFeedbackService, FeedbackService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IDeviceVibrationService, DeviceVibrationService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ISoundService, SoundService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IAppNotificationsService, AppNotificationsService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IProtocolHandler, ProtocolHandler>(new TransientLifetimeManager());
            Container.RegisterType<IHttpFileService, HttpFileService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IVKAdService, VKAdService>(new TransientLifetimeManager());
            Container.RegisterType<IPlayerService, PlayerService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IDownloadsService, DownloadsService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IUploadsService, UploadsService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IVKCaptchaHandler, VKCaptchaHandler>(new TransientLifetimeManager());
            Container.RegisterType<IVKBehaviorSimulator, VKBehaviorSimulator>(new ContainerControlledLifetimeManager());

            Container.RegisterType<ISuspendingService, TransferNotificationsService>("s1",
                new ContainerControlledLifetimeManager());
            Container.RegisterType<ISuspendingService>("s2",
                new InjectionFactory(c => c.Resolve<PlayerService>()));
            Container.RegisterType<ISuspendingService>("s3",
                new InjectionFactory(c => c.Resolve<DownloadsService>()));
            Container.RegisterType<ISuspendingService>("s4",
                new InjectionFactory(c => c.Resolve<UploadsService>()));

            Container.RegisterType<IDownloadsServiceHelper, DownloadsServiceHelper>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IMediaFilesProcessService, MediaFilesProcessService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IUploadsServiceHelper, UploadsServiceHelper>(new ContainerControlledLifetimeManager());
            Container.RegisterType<ISuspendingService, SuspendingServicesManager>(new TransientLifetimeManager(),
                new InjectionConstructor(
                    new ResolvedArrayParameter<ISuspendingService>(
                        new ResolvedParameter<ISuspendingService>("s1"),
                        new ResolvedParameter<ISuspendingService>("s2"),
                        new ResolvedParameter<ISuspendingService>("s3"),
                        new ResolvedParameter<ISuspendingService>("s4"))));

            Container.RegisterType<PlayerViewModel>(new ContainerControlledLifetimeManager());
            //#if DEBUG
            //            Container.RegisterType<ILaunchViewResolver, LaunchViewResolver>("l1", new ContainerControlledLifetimeManager());
            //            Container.RegisterType<ILaunchViewResolver, DebugLaunchViewResolver>(new ContainerControlledLifetimeManager(),
            //                new InjectionConstructor(
            //                    new ResolvedParameter<ILaunchViewResolver>("l1"),
            //                    new ResolvedParameter<INavigationService>()));
            //#else
            Container.RegisterType<ILaunchViewResolver, LaunchViewResolver>(new ContainerControlledLifetimeManager());
//#endif

#if DEBUG && !FULL
            LoadPurchaseFile();
#endif

            var purchaseService = Container.Resolve<PurchaseService>();
            if (!purchaseService.IsFullVersionPurchased)
            {
                Container.Resolve<ISettingsService>().Remove(PlayerConstants.PLAYER_SCROBBLE_MODE);
            }

            var lastFmLoginService = Container.Resolve<ILastFmLoginService>();
            if (lastFmLoginService.IsAuthorized)
                lastFmLoginService.InitializeLastAuth();

            _shell = Container.Resolve<Shell>();
            Container.RegisterInstance<IAppNotificationsPresenter>(_shell);
            _shell.PlayerVM = Container.Resolve<PlayerViewModel>(); ;

            StartSuspendingServices();

            var locService = Container.Resolve<ILocService>();
            locService.ApplyAppLanguage();

            return base.OnInitializeAsync(args);
        }

        protected override async Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
        {
            ActivateMetrica();

            var vkLoginService = Container.Resolve<IVKLoginService>();
            var launchViewResolver = Container.Resolve<ILaunchViewResolver>();

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
                        var state = Container.Resolve<IPlayerService>().CurrentState;
                        if (state == PlayerState.Playing)
                            NavigationService.Navigate("PlayerView", null);
                    });
                }
                catch (Exception) { }
//#if FULL
//                Container.Resolve<IBetaService>().ExecuteAppLaunch();
//#endif

                Container.Resolve<IFeedbackService>().ActivateFeedbackNotifier();
                ActivatePush(args);
            }

            //Container.Resolve<IProtocolHandler>().ProcessProtocol("?yamp_l=vksaver%3A%2F%2Fvkad%2F-138475410_3&yamp_i=m%3D49874%26cor%3Dddd8ae32-0322-4881-b526-c96c1a708ccc");
            if (args.Kind == ActivationKind.Protocol || (args.Kind == ActivationKind.Launch && !String.IsNullOrEmpty(args.Arguments)))
                Container.Resolve<IProtocolHandler>().ProcessProtocol(args.Arguments);
        }

        protected override async void OnFileActivated(FileActivatedEventArgs args)
        {
            await OnInitializeAsync(args);
            await OnFileActivatedAsync(args);
            base.OnFileActivated(args);
        }

        private async Task OnFileActivatedAsync(FileActivatedEventArgs args)
        {
            ActivateMetrica();

            var mediaFilesProcessService = Container.Resolve<IMediaFilesProcessService>();
            var launchViewResolver = Container.Resolve<ILaunchViewResolver>();

            StartSuspendingServices();

            if (args.PreviousExecutionState == ApplicationExecutionState.NotRunning ||
                args.PreviousExecutionState == ApplicationExecutionState.ClosedByUser)
            {
                var vkLoginService = Container.Resolve<IVKLoginService>();

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
                    Container.Resolve<IBetaService>().ExecuteAppLaunch();
#endif
                }
            }

            await mediaFilesProcessService.ProcessFiles(args.Files);
        }

        protected override Task OnSuspendingApplicationAsync()
        {
            StopSuspendingServices();
            return base.OnSuspendingApplicationAsync();
        }

        protected override Task OnResumeApplicationAsync(IActivatedEventArgs args)
        {
            ActivateMetrica();
            StartSuspendingServices();

            return base.OnResumeApplicationAsync(args);
        }

        //protected override void OnActivated(IActivatedEventArgs args)
        //{
        //    base.OnActivated(args);

        //    var rootFrame = _frame;
        //    var continuationManager = new ContinuationManager();

        //    var continuationEventArgs = args as IContinuationActivatedEventArgs;
        //    if (continuationEventArgs != null)
        //    {
        //        if (rootFrame != null)
        //        {
        //            continuationManager.Continue(continuationEventArgs, rootFrame);
        //        }
        //    }
        //}

        private void StartSuspendingServices()
        {
            Container.Resolve<ISuspendingService>().StartService();
        }

        private void StopSuspendingServices()
        {
            Container.Resolve<ISuspendingService>().StopService();
        }

        private async void ActivateMetrica()
        {
            //await Task.Run(() => YandexMetrica.Activate(AppConstants.YANDEX_METRICA_API_KEY));
        }

        private async void ActivatePush(LaunchActivatedEventArgs e)
        {
            //await Task.Run(() =>
            //{
            //    YandexMetricaPush.Activate("***REMOVED***");
            //    if (e != null)
            //        YandexMetricaPush.ProcessApplicationLaunch(e);
            //});
        }

#if DEBUG && !FULL
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
                return Container.Resolve(type);
            }
            catch (Exception)
            {
                NavigationService.Navigate("ErrorView", $"Resolution failed: {type.Name}");
                return null;
            }
        }

        public T Resolve<T>() where T : class
        {
            return Resolve<T>(null);
        }

        public T Resolve<T>(string name) where T : class
        {
            object res = null;
            if (name == null)
                res = Container.Resolve(typeof(T));
            else
                res = Container.Resolve(typeof(T), name);

            return (T)res;
        }
        
        private IAppLoaderService _appLoaderService;
        private IMetricaService _metricaService;
        private UnityServiceLocator _unityServiceLocator;
        private Frame _frame;
        private Shell _shell;

        private const string VIEW_MODEL_FORMAT = "VKSaver.Core.ViewModels.{0}Model, VKSaver.Core.ViewModels.UWP, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        private const string VIEW_MODEL_CONTROLS_FORMAT = "VKSaver.Core.ViewModels.{0}ViewModel, VKSaver.Core.ViewModels.UWP, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        private const string VIEW_FORMAT = "VKSaver.Views.{0}";
    }
}