using System.Threading.Tasks;
using Windows.ApplicationModel.Store;

namespace VKSaver.Core.Services.Interfaces
{
    public interface IPurchaseService
    {
        bool IsFullVersionPurchased { get; }

        Task<ProductPurchaseStatus> BuyFullVersion(bool isPermanent);
    }
}
