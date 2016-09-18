using Microsoft.Practices.Prism.StoreApps;
using PropertyChanged;
using System;
using System.Threading.Tasks;
using VKSaver.Core.Services.Interfaces;
using Windows.System;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class LastFmLoginViewModel : ViewModelBase
    {
        public LastFmLoginViewModel(ILastFmLoginService lastFmLoginService, 
            IAppLoaderService appLoaderService)
        {
            _lastFmLoginService = lastFmLoginService;
            _appLoaderService = appLoaderService;

            JoinCommand = new DelegateCommand(async () => await Launcher.LaunchUriAsync(
                new Uri("https://www.last.fm/join")));
            RestoreCommand = new DelegateCommand(async () => await Launcher.LaunchUriAsync(
                new Uri("https://secure.last.fm/settings/lostpassword")));
        }

        public string LoginText { get; set; }

        public string PasswordText { get; set; }

        [DoNotNotify]
        public DelegateCommand JoinCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand RestoreCommand { get; private set; }

        [DoNotNotify]
        public bool IsAuthorized { get { return _lastFmLoginService.IsAuthorized; } }

        public async Task Login()
        {
            _appLoaderService.Show();
            await _lastFmLoginService.LoginAsync(LoginText, PasswordText);
            _appLoaderService.Hide();
        }

        private readonly ILastFmLoginService _lastFmLoginService;
        private readonly IAppLoaderService _appLoaderService;
    }
}
