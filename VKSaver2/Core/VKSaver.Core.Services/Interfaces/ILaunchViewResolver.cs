using System.Collections.Generic;
using System.Threading.Tasks;

namespace VKSaver.Core.Services.Interfaces
{
    public interface ILaunchViewResolver
    {
        string LaunchViewName { get; set; }
        List<string> AvailableLaunchViews { get; }

        void OpenDefaultView();
        bool TryOpenSpecialViews();
        bool TryOpenPromoView();

        Task<bool> EnsureDatabaseUpdated();
    }
}
