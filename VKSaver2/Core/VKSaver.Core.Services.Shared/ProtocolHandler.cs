#if WINDOWS_UWP
using Prism.Windows.Navigation;
#else
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#endif

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class ProtocolHandler : IProtocolHandler
    {
        public ProtocolHandler(
            ILogService logService,
            INavigationService navigationService,
            IDispatcherWrapper dispatcherWrapper)
        {
            _logService = logService;
            _navigationService = navigationService;
            _dispatcherWrapper = dispatcherWrapper;
        }

        public async void ProcessProtocol(string uri)
        {
            await Task.Run(() => ProcessProtocolInternal(uri));
        }

        private async void ProcessProtocolInternal(string uri)
        {
            try
            {
                var parts = uri.Split(new[] { "&yamp_i" }, StringSplitOptions.None);
                string protocolUri = Uri.UnescapeDataString(parts[0].Substring(8));

                if (protocolUri.StartsWith("vksaver://vkad/"))
                {
                    await _dispatcherWrapper.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                        () => _navigationService.Navigate("VKAdInfoView", protocolUri.Substring(15)));
                }
            }
            catch (Exception e)
            {
                _logService.LogException(e);
            }
        }

        private readonly ILogService _logService;
        private readonly INavigationService _navigationService;
        private readonly IDispatcherWrapper _dispatcherWrapper;
    }
}
