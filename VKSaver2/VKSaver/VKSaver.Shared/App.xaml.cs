using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml.Controls;
using Windows.UI.ViewManagement;
using Windows.ApplicationModel.Store;
using OneTeam.SDK.VK.Services.Interfaces;
using OneTeam.SDK.VK.Services;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Unity;
using System.Globalization;
using OneTeam.SDK.LastFm.Services.Interfaces;
using OneTeam.SDK.LastFm.Services;
using VKSaver.Core.Services;
using OneTeam.SDK.Core.Services.Interfaces;
using System.Threading.Tasks;
using VKSaver.Core.Services.Interfaces;
using Windows.UI.Xaml;
using Windows.UI.Popups;
using VKSaver.Core.Models.Player;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#if WINDOWS_PHONE_APP
using Windows.Phone.UI.Input;
using VKSaver.Controls;
#else
using Windows.UI.Popups;
using VKSaver.Controls;
#endif

namespace VKSaver
{
    public sealed partial class App : MvvmAppBase, IServiceResolver
    {
        public App()
        {
            this.InitializeComponent();

            this.FrameFactory = () =>
            {
                _appLoaderService = new AppLoaderService();
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
            _container.Resolve<IPlayerService>().StartService();
            _container.Resolve<IDownloadsService>().DiscoverActiveDownloadsAsync();
        }

        private void App_Suspending(object sender, SuspendingEventArgs e)
        {
            _container.Resolve<IPlayerService>().StopService();
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var logService = _container.Resolve<ILogService>();

            logService.LogException(e.Exception);
            NavigationService.Navigate("ErrorView", null);

            e.Handled = true;
        }

        public IUnityContainer Container { get { return _container; } }

        protected override void OnInitialize(IActivatedEventArgs args)
        {
#if DEBUG
            DebugSettings.EnableFrameRateCounter = true;
#endif

            _container = new UnityContainer();
            ViewModelLocator.SetDefaultViewTypeToViewModelTypeResolver(GetViewModelType);

            var settingsService = new SettingsService();
            var vkLoginService = new VKLoginService(VKSAVER_APP_ID, settingsService);

            _container.RegisterInstance<IServiceResolver>(this);
            _container.RegisterInstance<ISettingsService>(settingsService);
            _container.RegisterInstance(this.NavigationService);
            _container.RegisterInstance<IVKLoginService>(vkLoginService);            
            _container.RegisterInstance<ILFService>(new LFService("***REMOVED***"));
            _container.RegisterInstance<IAppLoaderService>(_appLoaderService);

            _container.RegisterType<ILogService, LogService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IVKService, VKService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IDialogsService, DialogsService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IPurchaseService, PurchaseService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<ITracksShuffleService, TracksShuffleService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IPlayerPlaylistService, PlayerPlaylistService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IPlayerService, PlayerService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<ICultureProvider, CultureProvider>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IGrooveMusicService, GrooveMusicService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IImagesCacheService, ImagesCacheService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<INetworkInfoService, NetworkInfoService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IDownloadsService, DownloadsService2>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IDownloadsServiceHelper, DownloadsServiceHelper>(new ContainerControlledLifetimeManager());

            vkLoginService.UserLogin += (s, e) => NavigationService.Navigate("MainView", null);
            vkLoginService.UserLogout += (s, e) =>
            {
                NavigationService.Navigate("LoginView", null);
                NavigationService.ClearHistory();
            };

            var playerService = _container.Resolve<IPlayerService>();
            playerService.StartService();

#if DEBUG
            LoadPurchaseFile();
#endif

            base.OnInitialize(args);            
        }

        protected override Task OnLaunchApplication(LaunchActivatedEventArgs args)
        {
            var playerService = _container.Resolve<IPlayerService>();

            if (args.PreviousExecutionState == ApplicationExecutionState.ClosedByUser ||
                args.PreviousExecutionState == ApplicationExecutionState.NotRunning)
            {
                if (_container.Resolve<IVKLoginService>().IsAuthorized)
                    NavigationService.Navigate("MainView", null);
                else
                    NavigationService.Navigate("LoginView", null);
            }

            try
            {
                var state = playerService.CurrentState;
                if (state == PlayerState.Playing)
                    NavigationService.Navigate("PlayerView", null);
            }
            catch (Exception) { }

            _container.Resolve<IDownloadsService>().DiscoverActiveDownloadsAsync();
            return Task.FromResult<object>(null);
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
            return _container.Resolve(type);
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
        private Frame _frame;

        private const string VIEW_MODEL_FORMAT = "VKSaver.Core.ViewModels.{0}Model, VKSaver.Core.ViewModels, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        private const string VIEW_MODEL_CONTROLS_FORMAT = "VKSaver.Core.ViewModels.{0}ViewModel, VKSaver.Core.ViewModels, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        private const string VIEW_FORMAT = "VKSaver.Views.{0}";
        private const int VKSAVER_APP_ID = ***REMOVED***;
    }
}