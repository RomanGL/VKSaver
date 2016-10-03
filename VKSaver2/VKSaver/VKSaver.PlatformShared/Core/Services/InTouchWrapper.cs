using ModernDev.InTouch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Services.Common;
using Windows.UI.Xaml.Controls;

namespace VKSaver.Core.Services
{
    public sealed class InTouchWrapper : IInTouchWrapper
    {
        public InTouchWrapper(InTouch inTouch)
        {
            _inTouch = inTouch;
            _queue = new TaskQueue();
        }

        public async Task<Response<T>> ExecuteRequest<T>(Task<Response<T>> inTouchRequestTask)
        {
            var response = await inTouchRequestTask;
            if (response.IsError && response.Error.Code == 14)
                return await _queue.Enqueue(() => ProcessCaptcha(response));
            else
                return response;
        }

        private async Task<Response<T>> ProcessCaptcha<T>(Response<T> errorResponse)
        {
            var response = errorResponse;

            while (true)
            {
                //var enterCaptcha = new EnterCaptcha(errorResponse.Error.CaptchaImg);
                //if (await enterCaptcha.ShowAsync() == ContentDialogResult.Primary)
                //{
                //    string captcha = String.IsNullOrWhiteSpace(enterCaptcha.Captcha) ? "empty" : enterCaptcha.Captcha;

                //    response = await _inTouch.SendCaptcha<T>(captcha, response.Error);
                //    if (response.IsError && response.Error.Code == 14)
                //        return await ProcessCaptcha(response);
                //    else
                //        return response;
                //}
                //else
                    return response;
            }
        }

        private readonly TaskQueue _queue;
        private readonly InTouch _inTouch;
    }
}
