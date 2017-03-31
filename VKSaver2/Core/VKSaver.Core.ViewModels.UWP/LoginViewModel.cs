using System;
using VKSaver.Core.Services;
using Windows.Security.Authentication.Web;
using VKSaver.Core.Toolkit;

namespace VKSaver.Core.ViewModels
{
    public sealed partial class LoginViewModel : VKSaverViewModel
    {
        private async void LoginUwp()
        {
//#if DEBUG
//            LoginToken(69396347, "942c58d27b3c05c4ecc8b382b89a16eb25d6a49693eddb600707bd59251c4e60d71546709bde36fbf787f");
//            return;
//#endif
            try
            {
                var authResult = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, 
                    new Uri(_vkLoginService.GetOAuthUrl()), new Uri(VKLoginService.REDIRECT_URL));

                if (authResult.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
                {
                    ShowError(authResult.ResponseErrorDetail.ToString());
                }
                else if (authResult.ResponseStatus == WebAuthenticationStatus.Success)
                {
                    var response = authResult.ResponseData.Split(new char[] { '#' })[1].Split(new char[] { '&' });
                    string token = response[0].Split('=')[1];
                    int userID = int.Parse(response[2].Split('=')[1]);

                    LoginToken(userID, token);
                }
                else
                    ShowError("access_denied");
            }
            catch (Exception)
            {
                ShowError("connection_error");
            }
        }
    }
}
