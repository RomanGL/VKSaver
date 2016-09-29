using System;
using System.Collections.Generic;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class AppLoaderService : IAppLoaderService
    {
        public AppLoaderService(IServiceResolver serviceResolver)
        {
            _serviceResolver = serviceResolver;
            _locService = new Lazy<ILocService>(() => _serviceResolver.Resolve<ILocService>());
        }

        public bool IsShowed { get; private set; }

        public void Show()
        {
            Show(_locService.Value["AppLoader_Loading"]);
        }

        public void Show(string text)
        {
            lock (_lockObject)
            {
                IsShowed = true;
                foreach (var loader in _loaders)
                    loader.ShowLoader(text);
            }
        }

        public void Hide()
        {
            lock (_lockObject)
            {
                IsShowed = false;
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

        private readonly IServiceResolver _serviceResolver;        
        private readonly Lazy<ILocService> _locService;
    }
}
