using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.UserProfile;
using ModernDev.InTouch;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class VKBehaviorSimulator : IVKBehaviorSimulator
    {
        public VKBehaviorSimulator(
            InTouch inTouch,
            ILogService logService)
        {
            _inTouch = inTouch;
            _logService = logService;
        }

        public bool IsSimulationComplete { get; set; }

        public async Task StartSimulation()
        {
            if (IsSimulationComplete)
                return;

            try
            {
                var pars = GetParameters();
                var response = await _inTouch.Request<BaseDataResponse>("execute.getBaseData", pars);
                if (!response.IsError)
                    IsSimulationComplete = true;
            }
            catch (Exception e)
            {
                _logService.LogException(e);
            }
        }

        private Dictionary<string, string> GetParameters()
        {
            var deviceInfo = new EasClientDeviceInformation();

            var pars = new Dictionary<string, string>();
            pars["userId"] = _inTouch.Session.UserId.ToString();
            pars["device"] = deviceInfo.SystemSku;
            pars["os"] = "WindowsPhone_8.1";
            pars["version"] = "4.11.1";
            pars["locale"] = GlobalizationPreferences.Languages[0];
            pars["themeBackgroundMode"] = "dark";
            pars["themeActiveBackground"] = "dark";
            pars["func_v"] = "5";
            pars["loadStickers"] = "1";

            return pars;
        }
        
        private readonly InTouch _inTouch;
        private readonly ILogService _logService;

        private sealed class BaseDataResponse
        {
            public int Time { get; set; }
            public int GamesSectionEnabled { get; set; }
            public int NewStoreItemsCount { get; set; }
            public bool HasStickersUpdates { get; set; }
            public int DebugDisabled { get; set; }
        }
    }
}
