using System.Threading.Tasks;

namespace VKSaver.Core.Services.Interfaces
{
    public interface INotificationsService
    {
        bool IsYandexPushActivated { get; }

        Task ActivateYandexPushAsync();
        Task DeactivateYandexPushAsync();
    }
}
