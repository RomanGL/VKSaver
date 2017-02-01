using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using VKSaver.Controls;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class VKCaptchaHandler : IVKCaptchaHandler
    {
        public async Task<string> GetCaptchaUserInput(string captchaImg)
        {
#if WINDOWS_UWP
            return null;
#else
            var enterCaptcha = new EnterCaptcha(captchaImg);
            if (await enterCaptcha.ShowAsync() == ContentDialogResult.Primary)
            {
                return String.IsNullOrWhiteSpace(enterCaptcha.Captcha) ? "empty" : enterCaptcha.Captcha;

            }
            else
                return null;
#endif
        }
    }
}
