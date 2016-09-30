using System;
using System.Threading.Tasks;
using VKSaver.Core.Services.Interfaces;
using Windows.ApplicationModel.Store;

namespace VKSaver.Core.Services
{
#if RELEASE && !FULL
    public sealed class PurchaseService : IPurchaseService
    {
        public PurchaseService(ILogService logService)
        {
            _logService = logService;
        }

        public bool IsFullVersionPurchased { get { return GetIsFullVersion(); } }

        public async Task<ProductPurchaseStatus> BuyFullVersion(bool isPernament)
        {
            try
            {
                ProductPurchaseStatus status;

                if (isPernament)
                    status = (await CurrentApp.RequestProductPurchaseAsync(StoreConstants.FullVersionVKSaverPernament)).Status;
                else
                    status = (await CurrentApp.RequestProductPurchaseAsync(StoreConstants.FullVersionVKSaver)).Status;

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
    }
#endif
}
