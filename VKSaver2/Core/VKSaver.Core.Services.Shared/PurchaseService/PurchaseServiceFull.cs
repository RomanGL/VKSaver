using System;
using System.Threading.Tasks;
using VKSaver.Core.Services.Interfaces;
using Windows.ApplicationModel.Store;

namespace VKSaver.Core.Services
{
#if FULL
    public sealed class PurchaseService : IPurchaseService
    {
        public bool IsFullVersionPurchased { get { return true; } }

        public Task<ProductPurchaseStatus> BuyFullVersion(bool isPernament)
        {
            throw new NotImplementedException("Это полная версия без возможности покупок.");
        }
    }
#endif
}
