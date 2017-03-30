#if WINDOWS_UWP
using Prism.Commands;
using Windows.System;
#elif WINDOWS_PHONE_APP
using Microsoft.Practices.Prism.StoreApps;
using Windows.System;
#elif ANDROID
#endif

using IF.Lastfm.Core.Api.Enums;
using PropertyChanged;
using System;
using System.Threading.Tasks;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels.Common;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class LastFmLoginViewModel : VKSaverViewModel
    {
        public LastFmLoginViewModel(ILastFmLoginService lastFmLoginService, 
            IAppLoaderService appLoaderService, IDialogsService dialogsService,
            ILocService locService)
        {
            _lastFmLoginService = lastFmLoginService;
            _appLoaderService = appLoaderService;
            _dialogsService = dialogsService;
            _locService = locService;

#if WINDOWS_UWP || WINDOWS_PHONE_APP
            JoinCommand = new DelegateCommand(async () => await Launcher.LaunchUriAsync(
                new Uri("https://www.last.fm/join")));
            RestoreCommand = new DelegateCommand(async () => await Launcher.LaunchUriAsync(
                new Uri("https://secure.last.fm/settings/lostpassword")));
#elif ANDROID
            JoinCommand = new DelegateCommand(() => { throw new NotImplementedException(); });
            RestoreCommand = new DelegateCommand(() => { throw new NotImplementedException(); });
#endif
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
