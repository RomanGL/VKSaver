using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Helpers;
using IF.Lastfm.Core.Objects;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Common;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class LastFmLoginService : ILastFmLoginService
    {
        public event TypedEventHandler<ILastFmLoginService, EventArgs> UserLogout;

        public LastFmLoginService(ISettingsService settingsService, ILastAuth lastAuth,
            INavigationService navigationService, IPlayerService playerService)
        {
            _settingsService = settingsService;
            _lastAuth = lastAuth;
            _navigationService = navigationService;
            _playerService = playerService;
        }

        private LastUserSession UserSession
        {
            get { return _settingsService.Get<LastUserSession>(LAST_FM_USER_SESSION_PARAMETER); }
            set { _settingsService.Set(LAST_FM_USER_SESSION_PARAMETER, value); }
        }

        public bool IsAuthorized { get { return UserSession != null; } }

        public void InitializeLastAuth()
        {
            if (!IsAuthorized)
                throw new InvalidOperationException("Авторизация не выполнена.");

            _lastAuth.LoadSession(UserSession);
        }

        public async Task LoginAsync(string login, string password)
        {
            var response = await _lastAuth.GetSessionTokenAsync(login, password);
            if (response.Success)
            {
                UserSession = _lastAuth.UserSession;
                _playerService.UpdateLastFm();
            }
        }

        public void Logout()
        {
            _settingsService.Remove(LAST_FM_USER_SESSION_PARAMETER);
            _settingsService.Remove(PlayerConstants.PLAYER_SCROBBLE_MODE);
            UserLogout?.Invoke(this, EventArgs.Empty);

            _playerService.UpdateLastFm();
        }

        public IServiceAuthorization GetServiceAuthorization()
        {
            var session = UserSession;
            return new LFAuthorization
            {
                IsAuthorized = session != null,
                UserName = session?.Username,
                SignInMethod = () => _navigationService.Navigate("LastFmLoginView", null),
                SignOutMethod = () => Logout()
            };
        }

        private readonly ISettingsService _settingsService;
        private readonly ILastAuth _lastAuth;
        private readonly INavigationService _navigationService;
        private readonly IPlayerService _playerService;

        private const string LAST_FM_USER_SESSION_PARAMETER = "LfUserSession";        
    }
}
