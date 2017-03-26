#if WINDOWS_UWP
using Prism.Windows.Navigation;
#elif WINDOWS_PHONE_APP
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#endif

using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Enums;
using IF.Lastfm.Core.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Common;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class LastFmLoginService : ILastFmLoginService
    {
        public event TypedEventHandler<ILastFmLoginService, EventArgs> UserLogout;

        public LastFmLoginService(
            ISettingsService settingsService, 
            ILastAuth lastAuth,
            INavigationService navigationService, 
            IPlayerService playerService,
            IPurchaseService purchaseService)
        {
            _settingsService = settingsService;
            _lastAuth = lastAuth;
            _navigationService = navigationService;
            _playerService = playerService;
            _purchaseService = purchaseService;
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

        public async Task<LastResponseStatus> LoginAsync(string login, string password)
        {
            var response = await _lastAuth.GetSessionTokenAsync(login, password);
            if (response.Success)
            {
                UserSession = _lastAuth.UserSession;
                _playerService.UpdateLastFm();
            }

            return response.Status;
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
                SignInMethod = () =>
                {
                    if (_purchaseService.IsFullVersionPurchased)
                        _navigationService.Navigate("LastFmLoginView", null);
                    else
                        _navigationService.Navigate("PurchaseView",
                            JsonConvert.SerializeObject(new KeyValuePair<string, string>("LastFmLoginView", null)));
                },
                SignOutMethod = () => Logout()
            };
        }

        private readonly ISettingsService _settingsService;
        private readonly ILastAuth _lastAuth;
        private readonly INavigationService _navigationService;
        private readonly IPlayerService _playerService;
        private readonly IPurchaseService _purchaseService;

        private const string LAST_FM_USER_SESSION_PARAMETER = "LfUserSession";        
    }
}
