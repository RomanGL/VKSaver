using Prism.Windows.Mvvm;
using System;
using VKSaver.Core.Services;
using Windows.Security.Authentication.Web;

namespace VKSaver.Core.ViewModels
{
    public sealed partial class LoginViewModel : ViewModelBase
    {
        private async void LoginUwp()
        {
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
