using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VKSaver.Core.Services.Interfaces;
using Windows.ApplicationModel.Store;
using static VKSaver.Core.Services.MetricaConstants;

namespace VKSaver.Core.Services
{
#if RELEASE && !FULL
    public sealed class PurchaseService : IPurchaseService
    {
        public PurchaseService(ILogService logService, IMetricaService metricaService)
        {
            _logService = logService;
            _metricaService = metricaService;
        }

        public bool IsFullVersionPurchased { get { return GetIsFullVersion(); } }

        public async Task<ProductPurchaseStatus> BuyFullVersion(bool isPermanent)
        {
            try
            {
                ProductPurchaseStatus status;

                if (isPermanent)
                    status = (await CurrentApp.RequestProductPurchaseAsync(StoreConstants.FullVersionVKSaverPernament)).Status;
                else
                    status = (await CurrentApp.RequestProductPurchaseAsync(StoreConstants.FullVersionVKSaver)).Status;

                var dict = new Dictionary<string, string>(1);
                if (status == ProductPurchaseStatus.Succeeded)
                {
                    dict["Purchased"] = isPermanent ? FULL_VERSION_PERMANENT : FULL_VERSION_MONTH;
                    _metricaService.LogEvent(FULL_VERSION_PURCHASED_EVENT, JsonConvert.SerializeObject(dict));
                }
                else if (status == ProductPurchaseStatus.AlreadyPurchased)
                {
                    dict["AlreadyPurchased"] = isPermanent ? FULL_VERSION_PERMANENT : FULL_VERSION_MONTH;
                    _metricaService.LogEvent(FULL_VERSION_PURCHASED_EVENT, JsonConvert.SerializeObject(dict));
                }

                return status;
            }
            catch (Exception ex)
            {
                _logService.LogException(ex);
                return ProductPurchaseStatus.NotPurchased;
            }
        }

        private bool GetIsFullVersion()
        {
            var licenseInformation = CurrentApp.LicenseInformation;

            bool isMonthyActive = licenseInformation.ProductLicenses[StoreConstants.FullVersionVKSaver].IsActive;
            if (!isMonthyActive)
                return licenseInformation.ProductLicenses[StoreConstants.FullVersionVKSaverPernament].IsActive;

            return isMonthyActive;
        }
        
        private readonly ILogService _logService;
        private readonly IMetricaService _metricaService;
    }
#endif
}
