using ModernDev.InTouch;
using System;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    /// <summary>
    /// Представляет сервис авторизации ВКонтакте.
    /// </summary>
    public sealed class VKLoginService : IVKLoginService
    {
        public const string ACCESS_TOKEN_PARAMETER = "AccessToken";
        private const string AUTHORIZATION_URL = "https://oauth.vk.com/authorize";
        private const string REDIRECT_URL = "https://oauth.vk.com/blank.html";
        private const string PARAMETERS_MASK = "{0}?client_id={1}&scope={2}&redirect_uri={3}&display=popup&v={4}&response_type=token";
        private const string SCOPE = "audio,friends,docs,groups,offline,status,video,wall";
        private const int CLIENT_ID = ***REMOVED***;

        private VKAccessToken AccessToken { get { return _settingsService?.Get<VKAccessToken>(ACCESS_TOKEN_PARAMETER); } }

        /// <summary>
        /// Просиходит при успешной авторизации пользователя.
        /// </summary>
        public event TypedEventHandler<IVKLoginService, EventArgs> UserLogin;
        /// <summary>
        /// Происходит при выходе пользователя.
        /// </summary>
        public event TypedEventHandler<IVKLoginService, EventArgs> UserLogout;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="VKLoginService"/>.
        /// </summary>
        /// <param name="settingsService">Сервис настроек.</param>
        public VKLoginService(ISettingsService settingsService, InTouch inTouch)
        {
            _settingsService = settingsService;
            _inTouch = inTouch;
        }

        /// <summary>
        /// Возвращает идентификатор текущего авторизованного пользователя.
        /// </summary>
        public long UserID
        {
            get
            {
                if (AccessToken == null)
                    throw new InvalidOperationException("Авторизация не выполнена.");
                return AccessToken.UserID;
            }
        }

        /// <summary>
        /// Возвращает токен авторизованного пользователя.
        /// </summary>
        public string Token
        {
            get
            {
                if (AccessToken == null)
                    throw new InvalidOperationException("Авторизация не выполнена.");
                return AccessToken.AccessToken;
            }
        }

        /// <summary>
        /// Возвращает значение, выполнена ли авторизация.
        /// </summary>
        public bool IsAuthorized { get { return AccessToken != null; } }

        /// <summary>
        /// Возвращает адрес для oAuth-авторизации ВКонтакте.
        /// </summary>
        public string GetOAuthUrl()
        {
            return String.Format(PARAMETERS_MASK, AUTHORIZATION_URL, CLIENT_ID, SCOPE, REDIRECT_URL, InTouch.APIVersion);
        }

        /// <summary>
        /// Возвращает ключ доступа к ВКонтакте из redireted-пути oAuth.
        /// </summary>
        /// <param name="oAuthUrl">Путь.</param>
        public APISession GetAccessTokenFromUrl(string oAuthUrl)
        {
            var response = oAuthUrl.Split(new char[] { '#' })[1].Split(new char[] { '&' });
            string token = response[0].Split('=')[1];
            int userID = int.Parse(response[2].Split('=')[1]);

            return new APISession(token, userID);
        }

        /// <summary>
        /// Выполняет авторизацию по указанному токену.
        /// </summary>
        public void Login(APISession session)
        {
            _settingsService?.Set(ACCESS_TOKEN_PARAMETER, new VKAccessToken
            {
                AccessToken = session.AccessToken,
                UserID = session.UserId
            });
            _inTouch.SetSessionData(session);
            UserLogin?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Отменить авторизацию ВКонтакте.
        /// </summary>
        public void Logout()
        {
            _settingsService?.Remove(ACCESS_TOKEN_PARAMETER);
            _inTouch.SetSessionData(null);
            UserLogout?.Invoke(this, EventArgs.Empty);
        }
        public void InitializeInTouch()
        {
            if (!IsAuthorized)
                throw new InvalidOperationException("Авторизация не выполнена.");

            var token = AccessToken;
            _inTouch.SetSessionData(new APISession(token.AccessToken, (int)token.UserID));
        }

        private readonly ISettingsService _settingsService;
        private readonly InTouch _inTouch;
        
        private sealed class VKAccessToken
        {
            public string AccessToken { get; set; }
            public int ExpiresIn { get; set; }
            public long UserID { get; set; }
        }
    }
}
