using System.Collections.Generic;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class SuspendingServicesManager : ISuspendingService
    {
        public SuspendingServicesManager(IEnumerable<ISuspendingService> services)
        {
            _services = services;
        }

        public void StartService()
        {
            foreach (var service in _services)
            {
                service.StartService();
            }
        }

        public void StopService()
        {
            foreach (var service in _services)
            {
                service.StopService();
            }
        }

        private readonly IEnumerable<ISuspendingService> _services;
    }
}
