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
    public class SplashScreenActivity : Activity
    {
        private IUnityContainer _container;

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

            _container.RegisterInstance<Context>(this);

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
        }

        private async Task OnLaunched()
        {
            var vkLoginService = _container.Resolve<IVKLoginService>();
            if (vkLoginService.IsAuthorized)
            {

            }
            else
            {
                var intent = new Intent(this, typeof(LoginActivity));
                StartActivity(intent);
            }
        }
    }
}

