using OneTeam.SDK.Core.Services.Interfaces;
using VKSaver.Core.Services.Interfaces;
using Windows.Networking.Connectivity;

namespace VKSaver.Core.Services
{
    public sealed class NetworkInfoService : INetworkInfoService
    {
        private ISettingsService settingsService;

        public NetworkInfoService(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
        }

        public bool IsInternetAvailable
        {
            get { return ConnectionProfile == null; }
        }

        private ConnectionProfile ConnectionProfile
        {
            get { return NetworkInformation.GetInternetConnectionProfile(); }
        }
    }
}
