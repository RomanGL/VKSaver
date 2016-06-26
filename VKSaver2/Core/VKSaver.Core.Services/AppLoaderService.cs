using System.Collections.Generic;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class AppLoaderService : IAppLoaderService
    {
        public AppLoaderService(ILocService locService)
        {
            _locService = locService;
        }

        public void Show()
        {
            Show(_locService["AppLoader_Loading"]);
        }

        public void Show(string text)
        {
            lock (_lockObject)
            {
                foreach (var loader in _loaders)
                    loader.ShowLoader(text);
            }
        }

        public void Hide()
        {
            lock (_lockObject)
            {
                foreach (var loader in _loaders)
                    loader.HideLoader();
            }
        }

        public void AddLoader(ILoader loader)
        {
            lock (_lockObject)
                _loaders.Add(loader);
        }

        public void RemoveLoader(ILoader loader)
        {
            lock (_lockObject)
                _loaders.Remove(loader);
        }

        private readonly List<ILoader> _loaders = new List<ILoader>();
        private readonly object _lockObject = new object();
        private readonly ILocService _locService;
    }
}
