﻿using ModernDev.InTouch;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Services.Common;
using Windows.UI.Xaml.Controls;
using VKSaver.Controls;

namespace VKSaver.Core.Services
{
    public sealed class InTouchWrapper : IInTouchWrapper
    {
        public InTouchWrapper(InTouch inTouch, IVKBehaviorSimulator vkBehaviorSimulator)
        {
            _inTouch = inTouch;
            _vkBehaviorSimulator = vkBehaviorSimulator;
            
            _queue = new TaskQueue();
        }

        public async Task<Response<T>> ExecuteRequest<T>(Task<Response<T>> inTouchRequestTask)
        {
            await _semaphore.WaitAsync();
            try
            {
                await _vkBehaviorSimulator.StartSimulation();
                if (!_vkBehaviorSimulator.IsSimulationComplete)
                    return new Response<T>(new ResponseError(1, "Preparing fail."), default(T), null);
            }
            finally
            {
                _semaphore.Release();
            }

            var response = await inTouchRequestTask;
            if (response.IsError && response.Error.Code == 14)
                return await _queue.Enqueue(() => ProcessCaptcha(response));
            else
                return response;
        }

#if WINDOWS_UWP
        private Task<Response<T>> ProcessCaptcha<T>(Response<T> errorResponse)
        {
            return Task.FromResult(errorResponse);
        }
#endif

#if WINDOWS_PHONE_APP
        private async Task<Response<T>> ProcessCaptcha<T>(Response<T> errorResponse)
        {
            var response = errorResponse;

            while (true)
            {
                var enterCaptcha = new EnterCaptcha(errorResponse.Error.CaptchaImg);
                if (await enterCaptcha.ShowAsync() == ContentDialogResult.Primary)
                {
                    string captcha = String.IsNullOrWhiteSpace(enterCaptcha.Captcha) ? "empty" : enterCaptcha.Captcha;

                    response = await _inTouch.SendCaptcha<T>(captcha, response.Error);
                    if (response.IsError && response.Error.Code == 14)
                        return await ProcessCaptcha(response);
                    else
                        return response;
                }
                else
                    return response;
            }
        }
#endif

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private readonly TaskQueue _queue;

        private readonly InTouch _inTouch;
        private readonly IVKBehaviorSimulator _vkBehaviorSimulator;
    }
}
