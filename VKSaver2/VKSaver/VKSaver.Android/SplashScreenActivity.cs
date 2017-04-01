using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Widget;
using Android.OS;
using IF.Lastfm.Core.Api;
using Microsoft.Practices.Unity;
using ModernDev.InTouch;
using VKSaver.Android.Views;
using VKSaver.Core;
using VKSaver.Core.FileSystem;
using VKSaver.Core.Services;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Android
{
    [Activity(Label = "ВКачай", 
        Icon = "@drawable/icon",
        Theme = "@android:style/Theme.DeviceDefault.NoActionBar.Fullscreen",
        MainLauncher = true, 
        NoHistory = true)]
    public class SplashScreenActivity : Activity, IServiceResolver, IActivityTypesResolver
    {
        private const string VIEW_MODEL_FORMAT = "VKSaver.Core.ViewModels.{0}Model, VKSaver.Core.ViewModels.Android, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        private const string VIEW_FORMAT = "VKSaver.Android.Views.{0}";

        private IUnityContainer _container;

        public static IUnityContainer Container { get; private set; }
        public T Resolve<T>() where T : class => _container.Resolve<T>();
        public T Resolve<T>(string name) where T : class => _container.Resolve<T>(name);

        public Type GetActivityType(string viewName)
            => Type.GetType(String.Format(CultureInfo.InvariantCulture, VIEW_FORMAT, viewName));
        public Type GetViewModelType(string activityName)
            => Type.GetType(String.Format(CultureInfo.InvariantCulture, VIEW_MODEL_FORMAT, activityName.Replace("Activity", "View")));

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SplashScreenActivity);

            await Task.Run(async () =>
            {
                InitializeServices();
                await OnLaunched();
            });
        }

        private void InitializeServices()
        {
            _container = new UnityContainer();
            Container = _container;

            _container.RegisterInstance<Context>(this);
            _container.RegisterInstance<IServiceResolver>(this);
            _container.RegisterInstance<IActivityTypesResolver>(this);
            _container.RegisterInstance<IUnityContainer>(_container);
            
            _container.RegisterType<ISettingsService, SettingsService>(
                new ContainerControlledLifetimeManager(),
                new InjectionFactory(InstanceFactories.ResolveSettingsService));
            _container.RegisterType<InTouch>(
                new ContainerControlledLifetimeManager(),
                new InjectionFactory(InstanceFactories.ResolveInTouch));
            _container.RegisterType<ILastAuth, LastAuth>(
                new ContainerControlledLifetimeManager(),
                new InjectionFactory(InstanceFactories.ResolveLastAuth));
            _container.RegisterType<LastfmClient>(
                new ContainerControlledLifetimeManager(),
                new InjectionFactory(InstanceFactories.ResolveLastfmClient));
            _container.RegisterType<IVKLoginService, VKLoginService>(
                new ContainerControlledLifetimeManager(),
                new InjectionFactory(InstanceFactories.ResolveVKLoginService));

            _container.RegisterType<INavigationService, NavigationService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<ILocService, LocService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IDialogsService, DialogsService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IAppLoaderService, AppLoaderService>(new ContainerControlledLifetimeManager());
            _container.RegisterType<IVKCaptchaHandler, VKCaptchaHandler>(new ContainerControlledLifetimeManager());
        }

        private async Task OnLaunched()
        {
            var vkLoginService = _container.Resolve<IVKLoginService>();
            if (vkLoginService.IsAuthorized)
            {
                _container.Resolve<INavigationService>().Navigate("MainView", null);
            }
            else
            {
                _container.Resolve<INavigationService>().Navigate("DirectAuthView", null);
            }
        }
    }
}

