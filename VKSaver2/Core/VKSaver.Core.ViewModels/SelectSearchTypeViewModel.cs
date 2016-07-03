using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using ModernDev.InTouch;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace VKSaver.Core.ViewModels
{
    public sealed class SelectSearchTypeViewModel : ViewModelBase
    {
        public SelectSearchTypeViewModel(INavigationService navigationService, InTouch inTouch)
        {
            _navigationService = navigationService;
            _inTouch = inTouch;

            GoToSearchCommand = new DelegateCommand<string>(OnGoToSearchCommand);
        }

        public DelegateCommand<string> GoToSearchCommand { get; private set; }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {     
            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private void OnGoToSearchCommand(string type)
        {
            _navigationService.Navigate($"{type}SearchView", null);
            _navigationService.RemoveFirstPage("SelectSearchTypeView");
        }

        private readonly INavigationService _navigationService;
        private readonly InTouch _inTouch;
    }
}
