using System.Threading.Tasks;
using VKSaver.Core.Models.Common;

namespace VKSaver.Core.Services.Interfaces
{
    public interface IPurchaseService
    {
        bool IsFullVersionPurchased { get; }

        Task<ProductPurchaseStatus> BuyFullVersion(bool isPermanent);
    }
}
