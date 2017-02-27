using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Windows.Navigation;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class DebugLaunchViewResolver : ILaunchViewResolver
    {
        public DebugLaunchViewResolver(
            ILaunchViewResolver launchViewResolver, 
            INavigationService navigationService)
        {
            _launchViewResolver = launchViewResolver;
            _navigationService = navigationService;
        }

        public string LaunchViewName
        {
            get { return _launchViewResolver.LaunchViewName; }
            set { _launchViewResolver.LaunchViewName = value; }
        }

        public List<string> AvailableLaunchViews => _launchViewResolver.AvailableLaunchViews;

        public void OpenDefaultView()
        {
            _navigationService.Navigate(AppConstants.DEFAULT_MAIN_VIEW, null);
        }

        public bool TryOpenSpecialViews()
        {
            return false;
        }

        public bool TryOpenPromoView()
        {
            return false;
        }

        public Task<bool> EnsureDatabaseUpdated()
        {
            return Task.FromResult(false);
        }

        private readonly ILaunchViewResolver _launchViewResolver;
        private readonly INavigationService _navigationService;
    }
}
