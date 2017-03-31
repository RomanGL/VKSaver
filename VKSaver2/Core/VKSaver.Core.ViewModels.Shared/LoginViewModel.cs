#if WINDOWS_UWP
using Prism.Windows.Navigation;
using Prism.Commands;
using Windows.System;
#elif WINDOWS_PHONE_APP
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using Windows.System;
#elif ANDROID
using VKSaver.Core.Toolkit.Commands;
#endif

using ModernDev.InTouch;
using System;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Toolkit;

namespace VKSaver.Core.ViewModels
{
    public sealed partial class LoginViewModel : VKSaverViewModel
    {
        public LoginViewModel(
            IVKLoginService vkLoginService, 
            IDialogsService dialogsService,
            ILocService locService,
            INavigationService navigationService)
        {
            _vkLoginService = vkLoginService;
            _dialogsService = dialogsService;
            _locService = locService;
            _navigationService = navigationService;
            
            GoToDirectAuthCommand = new DelegateCommand(OnGoToDirectAuthCommand);

#if WINDOWS_UWP
            LoginCommand = new DelegateCommand(() => LoginUwp());
#endif

#if WINDOWS_UWP || WINDOWS_PHONE_APP
            JoinCommand = new DelegateCommand(async () => await Launcher.LaunchUriAsync(new Uri("https://vk.com/join")));
            RestoreCommand = new DelegateCommand(async () => await Launcher.LaunchUriAsync(new Uri("https://vk.com/restore")));
#elif ANDROID
            JoinCommand = new DelegateCommand(() => { throw new NotImplementedException(); });
            RestoreCommand = new DelegateCommand(() => { throw new NotImplementedException(); });
#endif
        }

        public DelegateCommand GoToDirectAuthCommand { get; private set; }
        /// <summary>
        /// Команда, запускающая процесс регистрации.
        /// </summary>
        public DelegateCommand JoinCommand { get; private set; }
        /// <summary>
        /// Команда, запускающая процесс восстановления пароля.
        /// </summary>
        public DelegateCommand RestoreCommand { get; private set; }

#if WINDOWS_UWP
        public DelegateCommand LoginCommand { get; private set; }
#endif

        /// <summary>
        /// Возвращает URL для проведения авторизации.
        /// </summary>
        public string AuthUrl { get { return _vkLoginService.GetOAuthUrl(); } }

        /// <summary>
        /// Завершить авторизацию по токену.
        /// </summary>
        public void LoginToken(int userID, string accessToken)
        {
            _vkLoginService.Login(new APISession(accessToken, userID));
        }

        /// <summary>
        /// Отобразить ошибку авторизации.
        /// </summary>
        /// <param name="error">Имя ошибки.</param>
        public void ShowError(string error)
        {
            switch (error)
            {
                case "connection_error":
                    _dialogsService.Show(_locService["Message_Login_ConnectionError_Text"],
                        _locService["Message_Login_AuthorizationFailed_Title"]);
                    break;
                case "access_denied":
                    _dialogsService.Show(_locService["Message_Login_AccessDenied_Text"],
                        _locService["Message_Login_AccessDenied_Title"]);
                    break;
                default:
                    _dialogsService.Show(String.Format(_locService["Message_Login_UnknownError_Text"], error),
                        _locService["Message_Login_AuthorizationFailed_Title"]);
                    break;
            }
        }

        private void OnGoToDirectAuthCommand()
        {
            _navigationService.Navigate("DirectAuthView", null);
            _navigationService.ClearHistory();
        }

        private readonly IVKLoginService _vkLoginService;
        private readonly IDialogsService _dialogsService;
        private readonly ILocService _locService;
        private readonly INavigationService _navigationService;
    }
}
