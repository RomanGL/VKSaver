using ModernDev.InTouch;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Common;
using VKSaver.Core.Services.Interfaces;
using System.Text;
using Windows.Web.Http;
using Newtonsoft.Json;
using HttpClient = Windows.Web.Http.HttpClient;

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
        //private const string SCOPE = "audio,friends,docs,groups,offline,status,video,wall";
        private const string SCOPE = "notify,friends,photos,audio,video,docs,notes,pages,status,wall,groups,messages,notifications,stats,market";

        private const string DIRECT_AUTH_URL = "https://oauth.vk.com/token?grant_type=password&scope={0}&client_id={1}&client_secret={2}&username={3}&password={4}&v={5}&lang=ru";
        private const string DIRECT_AUTH_URL_POST = "https://api.vk.com/oauth/token";

        private const string AUTH_VERSION_PARAMETER = "AuthVersion";
        private const uint CURRENT_AUTH_VERSION = 2;    // Была версия 1 до 18 мая.

        //private const int CLIENT_ID = ***REMOVED***;  // ВКачай AppID

        //private const int CLIENT_ID = ***REMOVED***;      // VK WP7 AppID
        //private const string CLIENT_SECRET = "***REMOVED***";    // VK WP7 ClientSecret

        private const int CLIENT_ID = ***REMOVED***;  // VK WP AppID
        private const string CLIENT_SECRET = "***REMOVED***";  // VK WP ClientSecret.

        //private const int CLIENT_ID = ***REMOVED***;                        // Windows AppID
        //private const string CLIENT_SECRET = "***REMOVED***";  // Windows ClientSecret.

        //private const int CLIENT_ID = ***REMOVED***;                          // Android AppID.
        //private const string CLIENT_SECRET = "***REMOVED***";    // Android ClientSecret.

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
        public VKLoginService(
            ISettingsService settingsService, 
            InTouch inTouch, 
            IVKBehaviorSimulator vkBehaviorSimulator)
        {
            _settingsService = settingsService;
            _inTouch = inTouch;
            _vkBehaviorSimulator = vkBehaviorSimulator;

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
                UserID = session.UserId,
                Secret = session.SessionSecret
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
                    var parameters = new Dictionary<string, string>();
                    parameters["grant_type"] = "password";
                    parameters["client_id"] = CLIENT_ID.ToString();
                    parameters["client_secret"] = CLIENT_SECRET;
                    parameters["username"] = userName;
                    parameters["password"] = password;
                    parameters["scope"] = SCOPE;

                    if (!String.IsNullOrWhiteSpace(code))
                        parameters["code"] = code;
                    if (!String.IsNullOrEmpty(captchaSid))
                        parameters["captcha_sid"] = captchaSid;
                    if (!String.IsNullOrEmpty(captchaKey))
                        parameters["captcha_key"] = captchaKey;
                    if (forseSms)
                        parameters["force_sms"] = "1";

                    parameters["v"] = InTouch.APIVersion;
                    parameters["lang"] = ToEnumString(_inTouch.DataLanguage);

                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("Accept", "*/*");
                    client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
                    client.DefaultRequestHeaders.Add("User-Agent", AppConstants.VK_USER_AGENT);

                    string response = await (await client.PostAsync(new Uri(DIRECT_AUTH_URL_POST), new HttpFormUrlEncodedContent(parameters)))
                        .Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<VKDirectAuthResponse>(response);

                    return result;
                }
            }
            catch (Exception)
            {
                return new VKDirectAuthResponse { Error = DirectAuthErrors.connection_error };
            }
        }

        //public async Task<VKDirectAuthResponse> Login(string userName, string password, string captchaSid = null, string captchaKey = null, string code = null, bool forseSms = false)
        //{
        //    try
        //    {
        //        using (var client = new HttpClient())
        //        {
        //            var sb = new StringBuilder();
        //            sb.AppendFormat(DIRECT_AUTH_URL, SCOPE, CLIENT_ID, CLIENT_SECRET, userName, password, InTouch.APIVersion);

        //            if (!String.IsNullOrWhiteSpace(code))
        //                sb.AppendFormat("&code={0}", code);
        //            if (!String.IsNullOrEmpty(captchaSid))
        //                sb.AppendFormat("captcha_sid={0}", captchaSid);
        //            if (!String.IsNullOrEmpty(captchaKey))
        //                sb.AppendFormat("captcha_key={0}", captchaKey);
        //            if (forseSms)
        //                sb.Append("&force_sms=1");

        //            client.DefaultRequestHeaders.Add("User-Agent", AppConstants.VK_USER_AGENT);
        //            string response = await (await client.GetAsync(new Uri(sb.ToString()))).Content.ReadAsStringAsync();
        //            var result = JsonConvert.DeserializeObject<VKDirectAuthResponse>(response);

        //            return result;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return new VKDirectAuthResponse { Error = DirectAuthErrors.connection_error };
        //    }
        //}

        /// <summary>
        /// Отменить авторизацию ВКонтакте.
        /// </summary>
        public void Logout()
        {
            _settingsService?.Remove(ACCESS_TOKEN_PARAMETER);
            _inTouch.SetSessionData(null);
            _vkBehaviorSimulator.IsSimulationComplete = false;
            UserLogout?.Invoke(this, EventArgs.Empty);
        }

        public void InitializeInTouch()
        {
            _inTouch.AuthorizationFailed += InTouch_AuthorizationFailed;
            if (IsAuthorized)
            {
                var token = AccessToken;
                _inTouch.SetSessionData(new APISession(token.AccessToken, (int) token.UserID, token.Secret));
            }
        }

        private void InTouch_AuthorizationFailed(object sender, ResponseError e)
        {
            Logout();
        }

        private static string ToEnumString<T>(T type)
            => type.GetType().GetRuntimeField(type.ToString())?.GetCustomAttribute<EnumMemberAttribute>()?.Value;

        private readonly ISettingsService _settingsService;
        private readonly InTouch _inTouch;
        private readonly IVKBehaviorSimulator _vkBehaviorSimulator;

        private sealed class VKAccessToken
        {
            public string AccessToken { get; set; }
            public int ExpiresIn { get; set; }
            public long UserID { get; set; }
            public string Secret { get; set; }
        }
    }
}
