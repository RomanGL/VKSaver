using IF.Lastfm.Core.Api;
using Microsoft.Practices.Unity;
using ModernDev.InTouch;
using OneTeam.SDK.LastFm.Services;
using OneTeam.SDK.LastFm.Services.Interfaces;
using Prism.Mvvm;
using Prism.Unity.Windows;
using System;
using System.Globalization;
using System.Threading.Tasks;
using VKSaver.Core.LinksExtractor;
using VKSaver.Core.Services;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Prism.Windows.AppModel;
using VKSaver.Controls;
using Windows.Storage;
using VKSaver.Core.Services.Database;
using VKSaver.Core.Models.Player;
using Yandex.Metrica;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media;
using Windows.Foundation.Metadata;

namespace VKSaver
{
    sealed partial class App : PrismUnityApplication, IServiceResolver, IDispatcherWrapper
    {
        public App()
        {
            this.InitializeComponent();
        }        

        public CoreDispatcher Dispatcher { get; private set; }
        
        protected override void ConfigureViewModelLocator()
        {
            ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver(GetViewModelType);
            base.ConfigureViewModelLocator();
        }

        protected override Frame OnCreateRootFrame()
        {
            _appLoaderService = new AppLoaderService(this);
            return new WithLoaderFrame(_appLoaderService);
        }

        protected override UIElement CreateShell(Frame rootFrame)
        {
            Dispatcher = Window.Current.Dispatcher;
            return base.CreateShell(rootFrame);
        }

        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            var view = ApplicationView.GetForCurrentView();

            view.TitleBar.BackgroundColor = ((SolidColorBrush)Resources["TitleBarBackgroundThemeBrush"]).Color;
            view.TitleBar.InactiveBackgroundColor = ((SolidColorBrush)Resources["TitleBarBackgroundThemeBrush"]).Color;
            view.TitleBar.ForegroundColor = ((SolidColorBrush)Resources["TitleBarForegroundThemeBrush"]).Color;
            view.TitleBar.InactiveForegroundColor = ((SolidColorBrush)Resources["TitleBarInactiveForegroundThemeBrush"]).Color;

            view.TitleBar.ButtonBackgroundColor = ((SolidColorBrush)Resources["TitleBarButtonBackgroundThemeBrush"]).Color;
            view.TitleBar.ButtonForegroundColor = ((SolidColorBrush)Resources["TitleBarButtonForegroundThemeBrush"]).Color;
            view.TitleBar.ButtonHoverBackgroundColor = ((SolidColorBrush)Resources["TitleBarButtonHoverBackgroundThemeBrush"]).Color;
            view.TitleBar.ButtonHoverForegroundColor = ((SolidColorBrush)Resources["TitleBarButtonForegroundThemeBrush"]).Color;
            view.TitleBar.ButtonPressedBackgroundColor = ((SolidColorBrush)Resources["TitleBarButtonPressedBackgroundThemeBrush"]).Color;
            view.TitleBar.ButtonPressedForegroundColor = ((SolidColorBrush)Resources["TitleBarButtonPressedForegroundThemeBrush"]).Color;

            view.TitleBar.ButtonInactiveBackgroundColor = ((SolidColorBrush)Resources["TitleBarButtonBackgroundThemeBrush"]).Color;
            view.TitleBar.ButtonInactiveForegroundColor = ((SolidColorBrush)Resources["TitleBarInactiveForegroundThemeBrush"]).Color;

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar.GetForCurrentView().HideAsync().AsTask().Wait();
            }

            base.OnWindowCreated(args);
        }

        protected override Task OnInitializeAsync(IActivatedEventArgs args)
        {
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

            Container.RegisterInstance<IMetricaService>(_metricaService);
            Container.RegisterInstance<IServiceResolver>(this);
            Container.RegisterInstance<ISettingsService>(settingsService);
            Container.RegisterInstance(this.NavigationService);
            Container.RegisterInstance(this.SessionStateService);
            Container.RegisterInstance<IDispatcherWrapper>(this);
            Container.RegisterInstance<ILFService>(new LFService(LAST_FM_API_KEY));
            Container.RegisterInstance<IAppLoaderService>(_appLoaderService);
            Container.RegisterInstance<IVKLoginService>(vkLoginService);
            Container.RegisterInstance<InTouch>(inTouch);
            Container.RegisterInstance<ILastAuth>(lastAuth);

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

            var playerService = Container.Resolve<PlayerService>();
            var downloadsService = Container.Resolve<DownloadsService>();
            var uploadsService = Container.Resolve<UploadsService>();

            Container.RegisterInstance<ISuspendingService>("s1", playerService);
            Container.RegisterInstance<ISuspendingService>("s2", downloadsService);
            Container.RegisterInstance<ISuspendingService>("s3", uploadsService);

            Container.RegisterInstance<IPlayerService>(playerService);
            Container.RegisterInstance<IDownloadsService>(downloadsService);
            Container.RegisterInstance<IUploadsService>(uploadsService);

            Container.RegisterType<IDownloadsServiceHelper, DownloadsServiceHelper>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IMediaFilesProcessService, MediaFilesProcessService>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IUploadsServiceHelper, UploadsServiceHelper>(new ContainerControlledLifetimeManager());

            vkLoginService.UserLogin += async (s, e) =>
            {
                if (await TryOpenFirstStartView() == false)
                    NavigationService.Navigate("MainView", null);
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

            var purchaseService = Container.Resolve<PurchaseService>();
            if (!purchaseService.IsFullVersionPurchased)
            {
                settingsService.Remove(PlayerConstants.PLAYER_SCROBBLE_MODE);
            }

            var lastFmLoginService = Container.Resolve<ILastFmLoginService>();
            if (lastFmLoginService.IsAuthorized)
                lastFmLoginService.InitializeLastAuth();

            StartSuspendingServices();

            var locService = Container.Resolve<ILocService>();
            locService.ApplyAppLanguage();

            var vk = Container.Resolve<LoginViewModel>();

            return base.OnInitializeAsync(args);
        }

        protected override async Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
        {
            ActivateMetrica();

            var playerService = Container.Resolve<IPlayerService>();
            var vkLoginService = Container.Resolve<IVKLoginService>();
            var downloadsService = Container.Resolve<IDownloadsService>();
            var settingsService = Container.Resolve<ISettingsService>();

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
        }
        
        protected override async void OnFileActivated(FileActivatedEventArgs args)
        {
            ActivateMetrica();

            var mediaFilesProcessService = Container.Resolve<IMediaFilesProcessService>();
            var downloadsService = Container.Resolve<IDownloadsService>();
            var settingsService = Container.Resolve<ISettingsService>();

            StartSuspendingServices();

            if (args.PreviousExecutionState == ApplicationExecutionState.NotRunning ||
                args.PreviousExecutionState == ApplicationExecutionState.ClosedByUser)
            {
                var playerService = Container.Resolve<IPlayerService>();
                var vkLoginService = Container.Resolve<IVKLoginService>();

                if (settingsService.Get(AppConstants.CURRENT_PROMO_INDEX_PARAMETER, 0) < AppConstants.CURRENT_PROMO_INDEX)
                    NavigationService.Navigate("PromoView", null);
                else if (vkLoginService.IsAuthorized)
                {
                    if (await TryOpenFirstStartView() == false)
                        NavigationService.Navigate("MainView", null);
                }
                else
                    NavigationService.Navigate("LoginView", null);
            }

            await mediaFilesProcessService.ProcessFiles(args.Files);
            base.OnFileActivated(args);
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

        private async Task<bool> TryOpenFirstStartView()
        {
            var settingsService = Container.Resolve<ISettingsService>();
            string currentFirstView = settingsService.Get<string>(AppConstants.CURRENT_FIRST_START_VIEW_PARAMETER, null);

            if (settingsService.Get(AppConstants.CURRENT_FIRST_START_INDEX_PARAMETER, 0) < AppConstants.CURRENT_FIRST_START_INDEX)
            {
                if (currentFirstView == null)
                    NavigationService.Navigate("FirstStartView", null);
                else
                    NavigationService.Navigate("FirstStartRetryView", null);

                return true;
            }
            else if (currentFirstView != null && currentFirstView != "Completed")
            {
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
            var suspendingServices = Container.ResolveAll<ISuspendingService>();
            foreach (var service in suspendingServices)
            {
                service.StartService();
            }
        }

        private void StopSuspendingServices()
        {
            var suspendingServices = Container.ResolveAll<ISuspendingService>();
            foreach (var service in suspendingServices)
            {
                service.StopService();
            }
        }

        private async void ActivateMetrica()
        {
            //await Task.Run(() => YandexMetrica.Activate(AppConstants.YANDEX_METRICA_API_KEY));
        }

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

        public new T Resolve<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public new T Resolve<T>(string name) where T : class
        {
            throw new NotImplementedException();
        }
        
        private IAppLoaderService _appLoaderService;
        private IMetricaService _metricaService;

        private const string LAST_FM_API_KEY = "***REMOVED***";
        private const string LAST_FM_API_SECRET = "***REMOVED***";

        private const string VIEW_MODEL_FORMAT = "VKSaver.Core.ViewModels.{0}Model, VKSaver.Core.ViewModels.UWP, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        private const string VIEW_MODEL_CONTROLS_FORMAT = "VKSaver.Core.ViewModels.{0}ViewModel, VKSaver.Core.ViewModels.UWP, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        private const string VIEW_FORMAT = "VKSaver.Views.{0}";
        private const int VKSAVER_APP_ID = ***REMOVED***;        
    }
}
