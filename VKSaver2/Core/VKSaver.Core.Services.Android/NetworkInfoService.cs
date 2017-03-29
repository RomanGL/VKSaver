using System;
using Android.Content;
using Android.Net;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class NetworkInfoService : INetworkInfoService
    {
        private readonly ISettingsService _settingsService;
        private readonly ConnectivityManager _manager;

        public NetworkInfoService(
            Context context, 
            ISettingsService settingsService)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            _manager = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
            _settingsService = settingsService;
        }

        public bool IsInternetAvailable => _manager.ActiveNetworkInfo.IsAvailable;

        public bool CanAppUseInternet
        {
            get
            {
                var type = _settingsService.Get(AppConstants.INTERNET_ACCESS_TYPE, InternetAccessType.All);
                switch (type)
                {
                    case InternetAccessType.WiFi:
                        return _manager.ActiveNetworkInfo.Type == ConnectivityType.Wifi;
                    case InternetAccessType.Disabled:
                        return false;
                    default:
                        return IsInternetAvailable;
                }
            }
        }
    }
}