using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using ModernDev.InTouch;
using Newtonsoft.Json;
using System.Collections.Generic;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.ViewModels
{
    public sealed class SelectSearchTypeViewModel : ViewModelBase
    {
        public SelectSearchTypeViewModel(INavigationService navigationService, InTouch inTouch,
            IPurchaseService purchaseService)
        {
            _navigationService = navigationService;
            _inTouch = inTouch;
            _purchaseService = purchaseService;

            GoToSearchCommand = new DelegateCommand<string>(OnGoToSearchCommand);
        }

        public DelegateCommand<string> GoToSearchCommand { get; private set; }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {     
            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private void OnGoToSearchCommand(string type)
        {
            if (type != "Audio" && type != "Video" && !_purchaseService.IsFullVersionPurchased)
            {
                _navigationService.Navigate("PurchaseView", JsonConvert.SerializeObject(
                    new KeyValuePair<string, string>($"{type}SearchView", null)));
            }
            else
                _navigationService.Navigate($"{type}SearchView", null);

            _navigationService.RemoveFirstPage("SelectSearchTypeView");
        }

        private readonly INavigationService _navigationService;
        private readonly InTouch _inTouch;
        private readonly IPurchaseService _purchaseService;
    }
}
