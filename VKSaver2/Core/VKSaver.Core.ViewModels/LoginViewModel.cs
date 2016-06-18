using Microsoft.Practices.Prism.StoreApps;
using OneTeam.SDK.VK.Models.Common;
using OneTeam.SDK.VK.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Services.Interfaces;
using Windows.System;

namespace VKSaver.Core.ViewModels
{
    public sealed class LoginViewModel : ViewModelBase
    {
        public LoginViewModel(IVKLoginService vkLoginService, IDialogsService dialogsService)
        {
            _vkLoginService = vkLoginService;
            _dialogsService = dialogsService;

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
        /// <param name="accessToken">Ключ доступа ВКонтакте.</param>
        public void LoginToken(VKAccessToken accessToken)
        {
            _vkLoginService.Login(accessToken);
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
                    _dialogsService.Show("Не удалось подключиться к ВКонтакте. Проверьте доступ к интернету и повторите попытку.",
                        "Авторизация не удалась");
                    break;
                case "access_denied":
                    _dialogsService.Show("Не удалось выполнить авторизацию, так как ВКачай было отказано в доступе. Повторите попытку позднее.",
                        "Ошибка доступа");
                    break;
                default:
                    _dialogsService.Show($"Не удалось выполнить авторизацию. Повторите попытку позднее.\nКод ошибки: {error}",
                        "Авторизация не удалась");
                    break;
            }
        }

        private readonly IVKLoginService _vkLoginService;
        private readonly IDialogsService _dialogsService;
    }
}
