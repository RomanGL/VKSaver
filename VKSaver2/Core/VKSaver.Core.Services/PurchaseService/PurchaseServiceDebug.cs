using System;
using System.Threading.Tasks;
using VKSaver.Core.Services.Interfaces;
using Windows.ApplicationModel.Store;

namespace VKSaver.Core.Services
{
#if DEBUG && !FULL
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
                    status = (await CurrentAppSimulator.RequestProductPurchaseAsync(StoreConstants.FullVersionVKSaverPernament)).Status;
                else
                    status = (await CurrentAppSimulator.RequestProductPurchaseAsync(StoreConstants.FullVersionVKSaver)).Status;

                if (status == ProductPurchaseStatus.Succeeded ||
                    status == ProductPurchaseStatus.AlreadyPurchased)
                    _isFullVersionPurchased = true;
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
            if (_isFullVersionPurchased != null)
                return _isFullVersionPurchased.Value;

            var licenseInformation = CurrentAppSimulator.LicenseInformation;

            _isFullVersionPurchased = licenseInformation.ProductLicenses[StoreConstants.FullVersionVKSaver].IsActive;
            if (!_isFullVersionPurchased.Value)
                _isFullVersionPurchased = licenseInformation.ProductLicenses[StoreConstants.FullVersionVKSaverPernament].IsActive;

            return _isFullVersionPurchased.Value;
        }

        private bool? _isFullVersionPurchased;

        private readonly ILogService _logService;
    }
#endif
}
