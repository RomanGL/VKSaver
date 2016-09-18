using System;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;

namespace VKSaver.Core.Services.Interfaces
{
    public interface ILastFmLoginService
    {
        event TypedEventHandler<ILastFmLoginService, EventArgs> UserLogout;

        bool IsAuthorized { get; }

        void InitializeLastAuth();
        Task LoginAsync(string login, string password);
        void Logout();

        IServiceAuthorization GetServiceAuthorization();
    }
}
