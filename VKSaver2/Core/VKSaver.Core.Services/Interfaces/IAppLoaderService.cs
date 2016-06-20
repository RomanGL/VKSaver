using VKSaver.Core.Models.Common;

namespace VKSaver.Core.Services.Interfaces
{
    public interface IAppLoaderService
    {
        void Show();
        void Show(string text);
        void Hide();

        void AddLoader(ILoader loader);
        void RemoveLoader(ILoader loader);
    }
}
