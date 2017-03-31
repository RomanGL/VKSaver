#if WINDOWS_UWP
using Prism.Commands;
using Prism.Windows.Navigation;
#elif WINDOWS_PHONE_APP
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#elif ANDROID
using VKSaver.Core.Toolkit.Commands;
#endif

using ModernDev.InTouch;
using Newtonsoft.Json;
using System.Collections.Generic;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Toolkit;

namespace VKSaver.Core.ViewModels
{
    public sealed class SelectSearchTypeViewModel : VKSaverViewModel
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
