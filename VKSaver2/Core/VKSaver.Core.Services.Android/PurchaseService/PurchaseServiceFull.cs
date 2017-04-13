using System;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
#if FULL
    public sealed class PurchaseService : IPurchaseService
    {
        public bool IsFullVersionPurchased => true;

        public Task<ProductPurchaseStatus> BuyFullVersion(bool isPermanent)
        {
            throw new NotImplementedException();
        }
    }
#endif
}