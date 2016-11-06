using Newtonsoft.Json;

namespace VKSaver.Core.Models.Common
{
    public sealed class VKDirectAuthResponse
    {
        [JsonProperty("error")]
        public DirectAuthErrors Error { get; set; }
        [JsonProperty("error_description")]
        public string ErrorDescription { get; set; }

        [JsonProperty("validation_type")]
        public string ValidationType { get; set; }
        [JsonProperty("validation_sid")]
        public string ValidationSid { get; set; }
        [JsonProperty("phone_mask")]
        public string PhoneMask { get; set; }
        [JsonProperty("redirect_uri")]
        public string RedirectUri { get; set; }

        [JsonProperty("captcha_sid")]
        public string CaptchaSid { get; set; }
        [JsonProperty("captcha_img")]
        public string CaptchaImg { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("user_id")]
        public int UserID { get; set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
