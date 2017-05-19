#if WINDOWS_UWP
using Prism.Windows.Mvvm;
using Prism.Commands;
using Prism.Windows.Navigation;
#else
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#endif

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Services.Interfaces;
using Windows.System;
using PropertyChanged;
using VKSaver.Core.Models.Common;
using ModernDev.InTouch;

namespace VKSaver.Core.ViewModels
{
    public class DirectAuthViewModel : ViewModelBase
    {
        public DirectAuthViewModel(
            IVKLoginService vkLoginService,
            IAppLoaderService appLoaderService, 
            IDialogsService dialogsService,
            ILocService locService,
            INavigationService navigationService,
            IVKCaptchaHandler vkCaptchaHandler)
        {
            _vkLoginService = vkLoginService;
            _appLoaderService = appLoaderService;
            _dialogsService = dialogsService;
            _locService = locService;
            _navigationService = navigationService;
            _vkCaptchaHandler = vkCaptchaHandler;

            JoinCommand = new DelegateCommand(async () => await Launcher.LaunchUriAsync(new Uri("https://vk.com/join")));
            RestoreCommand = new DelegateCommand(async () => await Launcher.LaunchUriAsync(new Uri("https://vk.com/restore")));
            GoToOAuthCommand = new DelegateCommand(OnGoToOAuthCommand);
        }

        public string LoginText { get; set; }

        public string PasswordText { get; set; }

        [DoNotNotify]
        public DelegateCommand GoToOAuthCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand JoinCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand RestoreCommand { get; private set; }

        [DoNotNotify]
        public bool IsAuthorized { get { return _vkLoginService.IsAuthorized; } }

        [DoNotNotify]
        public IValidationSupport ValidationView { get; set; }

        public async Task<bool> Login(string captchaSid = null, string captchaKey = null)
        {
            _appLoaderService.Show(_locService["AppLoader_PleaseWait"]);

            var result = await _vkLoginService.Login(LoginText, PasswordText, captchaSid, captchaKey);
            DirectAuthErrors error = result != null ? result.Error : DirectAuthErrors.unknown_error;

            switch (error)
            {
                case DirectAuthErrors.None:
                    _vkLoginService.Login(new APISession(result.AccessToken, result.UserID, result.Secret));                   
                    break;
                case DirectAuthErrors.need_captcha:
                    try
                    {
                        string key = await _vkCaptchaHandler.GetCaptchaUserInput(result.CaptchaImg);
                        if (key != null)
                            return await Login(result.CaptchaSid, key);
                    }
                    catch (Exception)
                    {
                    }
                    break;
                case DirectAuthErrors.need_validation:
                    ValidationView?.StartValidation(result.RedirectUri);
                    break;
                default:
                    ProcessError(error);
                    break;
            }

            _appLoaderService.Hide();
            return false;
        }

        public void LoginToken(int userID, string accessToken)
        {
            _vkLoginService.Login(new APISession(accessToken, userID));
        }

        public void ProcessError(DirectAuthErrors error, string otherError = null)
        {
            switch (error)
            {
                case DirectAuthErrors.None:
                case DirectAuthErrors.need_captcha:
                case DirectAuthErrors.need_validation:
                    break;
                case DirectAuthErrors.invalid_client:
                    _dialogsService.Show(_locService["Message_Login_InvalidAccount_Text"],
                        _locService["Message_Login_AuthorizationFailed_Title"]);
                    break;
                case DirectAuthErrors.connection_error:
                    _dialogsService.Show(_locService["Message_Login_ConnectionError_Text"],
                        _locService["Message_Login_AuthorizationFailed_Title"]);
                    break;
                case DirectAuthErrors.access_denied:
                    _dialogsService.Show(_locService["Message_Login_AccessDenied_Text"],
                        _locService["Message_Login_AccessDenied_Title"]);
                    break;
                default:
                    _dialogsService.Show(String.Format(_locService["Message_Login_UnknownError_Text"], 
                        otherError == null ? error.ToString() : otherError),
                        _locService["Message_Login_AuthorizationFailed_Title"]);
                    break;
            }
        }

        private async void OnGoToOAuthCommand()
        {
            // Подтверждение OAQuth авторизации.
            bool result = await _dialogsService.ShowYesNoAsync(
                _locService["Message_OAuth_Attention_Text"],
                _locService["Message_OAuth_Attention_Title"]);

            if (result)
            {
                _navigationService.ClearHistory();
                _navigationService.Navigate("LoginView", null);                
            }
        }

        private readonly IVKLoginService _vkLoginService;
        private readonly IAppLoaderService _appLoaderService;
        private readonly IDialogsService _dialogsService;
        private readonly ILocService _locService;
        private readonly INavigationService _navigationService;
        private readonly IVKCaptchaHandler _vkCaptchaHandler;
    }
}
