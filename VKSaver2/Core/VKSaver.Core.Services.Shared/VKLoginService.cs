using ModernDev.InTouch;
using System;
using Windows.Web.Http;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Common;
using VKSaver.Core.Services.Interfaces;
using System.Text;
using Newtonsoft.Json;

namespace VKSaver.Core.Services
{
    /// <summary>
    /// Представляет сервис авторизации ВКонтакте.
    /// </summary>
    public sealed class VKLoginService : IVKLoginService
    {
        public const string ACCESS_TOKEN_PARAMETER = "AccessToken";
        public const string REDIRECT_URL = "https://oauth.vk.com/blank.html";

        private const string AUTHORIZATION_URL = "https://oauth.vk.com/authorize";        
        private const string PARAMETERS_MASK = "{0}?client_id={1}&scope={2}&redirect_uri={3}&display=popup&v={4}&response_type=token";
        private const string SCOPE = "audio,friends,docs,groups,offline,status,video,wall";

        private const string DIRECT_AUTH_URL = "https://oauth.vk.com/token?grant_type=password&client_id={0}&client_secret={1}&username={2}&password={3}&scope={4}&v={5}";

        private const string AUTH_VERSION_PARAMETER = "AuthVersion";
        private const uint CURRENT_AUTH_VERSION = 1;

        //private const int CLIENT_ID = ***REMOVED***;  // ВКачай AppID
        private const int CLIENT_ID = ***REMOVED***;  // VK WP AppID
        private const string CLIENT_SECRET = "***REMOVED***";        

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

            InitializeInTouch();
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
        public bool IsAuthorized
        {
            get
            {
                return _settingsService.Get<uint>(AUTH_VERSION_PARAMETER, 0) >= CURRENT_AUTH_VERSION && 
                    AccessToken != null;
            }
        }

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
        /// Возвращает данные авторизации ВКонтакте в общем виде.
        /// </summary>
        public IServiceAuthorization GetServiceAuthorization()
        {
            var auth = new VKAuthorization
            {
                IsAuthorized = IsAuthorized,
                SignInMethod = () => UserLogout?.Invoke(this, EventArgs.Empty),
                SignOutMethod = Logout,
                UserName = AccessToken.UserID.ToString()
            };
            return auth;
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
            _settingsService?.Set(AUTH_VERSION_PARAMETER, CURRENT_AUTH_VERSION);
            _inTouch.SetSessionData(session);
            UserLogin?.Invoke(this, EventArgs.Empty);
        }

        public async Task<VKDirectAuthResponse> Login(string userName, string password, string captchaSid = null, string captchaKey = null, string code = null, bool forseSms = false)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var sb = new StringBuilder();
                    sb.AppendFormat(DIRECT_AUTH_URL, CLIENT_ID, CLIENT_SECRET, userName, password, SCOPE, InTouch.APIVersion);

                    if (!String.IsNullOrWhiteSpace(code))
                        sb.AppendFormat("&code={0}", code);
                    if (!String.IsNullOrEmpty(captchaSid))
                        sb.AppendFormat("captcha_sid={0}", captchaSid);
                    if (!String.IsNullOrEmpty(captchaKey))
                        sb.AppendFormat("captcha_key={0}", captchaKey);
                    if (forseSms)
                        sb.Append("&force_sms=1");

                    string response = await (await client.GetAsync(new Uri(sb.ToString()))).Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<VKDirectAuthResponse>(response);

                    return result;
                }
            }
            catch (Exception)
            {
                return new VKDirectAuthResponse { Error = DirectAuthErrors.connection_error };
            }
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
            _inTouch.AuthorizationFailed += InTouch_AuthorizationFailed;
            if (IsAuthorized)
            {
                var token = AccessToken;
                _inTouch.SetSessionData(new APISession(token.AccessToken, (int) token.UserID));
            }
        }

        private void InTouch_AuthorizationFailed(object sender, ResponseError e)
        {
            Logout();
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
