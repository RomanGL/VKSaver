using System;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Interfaces;
using Windows.Networking.Connectivity;

namespace VKSaver.Core.Services
{
    public sealed class NetworkInfoService : INetworkInfoService
    {
        public NetworkInfoService(ISettingsService settingsService)
        {
            this._settingsService = settingsService;
        }

        public bool IsInternetAvailable
        {
            get { return ConnectionProfile != null; }
        }

        public bool CanAppUseInternet
        {
            get
            {
                var type = _settingsService.Get(AppConstants.INTERNET_ACCESS_TYPE, InternetAccessType.All);
                switch (type)
                {
                    case InternetAccessType.WiFi:
                        var profile = ConnectionProfile;
                        if (profile == null)
                            return false;

                        return profile.IsWlanConnectionProfile;
                    case InternetAccessType.Disabled:
                        return false;
                    default:
                        return IsInternetAvailable;
                }
            }
        }

        private ConnectionProfile ConnectionProfile
        {
            get { return NetworkInformation.GetInternetConnectionProfile(); }
        }

        private ISettingsService _settingsService;
    }
}
