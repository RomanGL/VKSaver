using System;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services.PurchaseService
{
#if FULL
    public sealed class PurchaseServiceFull : IPurchaseService
    {
        public bool IsFullVersionPurchased => true;

        public Task<ProductPurchaseStatus> BuyFullVersion(bool isPermanent)
        {
            throw new NotImplementedException();
        }
    }
#endif
}