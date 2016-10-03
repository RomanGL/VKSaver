#if WINDOWS_UWP
using Prism.Windows.Mvvm;
using Prism.Commands;
#else
using Microsoft.Practices.Prism.StoreApps;
#endif

using ModernDev.InTouch;
using System;
using VKSaver.Core.Services.Interfaces;
using Windows.System;

namespace VKSaver.Core.ViewModels
{
    public sealed class LoginViewModel : ViewModelBase
    {
        public LoginViewModel(IVKLoginService vkLoginService, IDialogsService dialogsService,
            ILocService locService)
        {
            _vkLoginService = vkLoginService;
            _dialogsService = dialogsService;
            _locService = locService;

            JoinCommand = new DelegateCommand(async () => await Launcher.LaunchUriAsync(new Uri("https://vk.com/join")));
            RestoreCommand = new DelegateCommand(async () => await Launcher.LaunchUriAsync(new Uri("https://vk.com/restore")));
        }

        /// <summary>
        /// Команда, запускающая процесс регистрации.
        /// </summary>
        public DelegateCommand JoinCommand { get; private set; }
        /// <summary>
        /// Команда, запускающая процесс восстановления пароля.
        /// </summary>
        public DelegateCommand RestoreCommand { get; private set; }

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

        private readonly IVKLoginService _vkLoginService;
        private readonly IDialogsService _dialogsService;
        private readonly ILocService _locService;
    }
}
