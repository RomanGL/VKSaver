#if WINDOWS_UWP
using Prism.Windows.Mvvm;
using Prism.Commands;
using Prism.Windows.Navigation;
#else
using Microsoft.Practices.Prism.StoreApps;
#endif

using IF.Lastfm.Core.Api.Enums;
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
            IAppLoaderService appLoaderService, IDialogsService dialogsService,
            ILocService locService)
        {
            _lastFmLoginService = lastFmLoginService;
            _appLoaderService = appLoaderService;
            _dialogsService = dialogsService;
            _locService = locService;

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

        public async Task<bool> Login()
        {
            _appLoaderService.Show(_locService["AppLoader_PleaseWait"]);

            LastResponseStatus code = await _lastFmLoginService.LoginAsync(LoginText, PasswordText);
            switch (code)
            {                
                case LastResponseStatus.Successful:
                    _appLoaderService.Hide();
                    return true;             
                case LastResponseStatus.BadAuth:
                    _dialogsService.Show(_locService["Message_Login_InvalidAccount_Text"],
                        _locService["Message_Login_AuthorizationFailed_Title"]);
                    break;
                case LastResponseStatus.Unknown:
                    _dialogsService.Show(_locService["Message_Login_LfConnectionError_Text"],
                        _locService["Message_Login_AuthorizationFailed_Title"]);
                    break;              
                default:
                    _dialogsService.Show(String.Format(_locService["Message_Login_UnknownError_Text"], code),
                        _locService["Message_Login_AuthorizationFailed_Title"]);
                    break;
            }

            _appLoaderService.Hide();
            return false;
        }

        private readonly ILastFmLoginService _lastFmLoginService;
        private readonly IAppLoaderService _appLoaderService;
        private readonly IDialogsService _dialogsService;
        private readonly ILocService _locService;
    }
}
