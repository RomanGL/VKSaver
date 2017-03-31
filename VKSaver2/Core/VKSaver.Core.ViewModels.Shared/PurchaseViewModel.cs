#if WINDOWS_UWP
using Prism.Commands;
using Prism.Windows.Navigation;
#elif WINDOWS_PHONE_APP
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#elif ANDROID
using VKSaver.Core.Toolkit.Commands;
#endif

using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Toolkit;
using VKSaver.Core.Models.Common;
using NavigatedToEventArgs = VKSaver.Core.Toolkit.Navigation.NavigatedToEventArgs;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class PurchaseViewModel : VKSaverViewModel
    {
        public PurchaseViewModel(
            IPurchaseService purchaseService, 
            INavigationService navigationService,
            IDialogsService dialogsService, 
            ILocService locService)
        {
            _purchaseService = purchaseService;
            _navigationService = navigationService;
            _dialogsService = dialogsService;
            _locService = locService;

            _screenItems = new ScreenItem[]
            {
                new ScreenItem(_locService["PurchaseView_Screen1_Line1_Text"], 
                    _locService["PurchaseView_Screen1_Line2_Text"], 1),
                new ScreenItem(_locService["PurchaseView_Screen2_Line1_Text"],
                    _locService["PurchaseView_Screen2_Line2_Text"], 2),
                new ScreenItem(_locService["PurchaseView_Screen3_Line1_Text"],
                    _locService["PurchaseView_Screen3_Line2_Text"], 3),
                new ScreenItem(_locService["PurchaseView_Screen4_Line1_Text"],
                    _locService["PurchaseView_Screen4_Line2_Text"], 4),
                new ScreenItem(_locService["PurchaseView_Screen5_Line1_Text"],
                    _locService["PurchaseView_Screen5_Line2_Text"], 5),
                new ScreenItem(_locService["PurchaseView_Screen0_Line1_Text"],
                    _locService["PurchaseView_Screen0_Line2_Text"], 0, true)
                {
                    BuyPermanentCommand = new DelegateCommand(OnBuyPermanentCommand),
                    BuyMonthlyCommand = new DelegateCommand(OnBuyMonthlyCommand)
                }
            };
        }
        
        [DoNotNotify]
        public ScreenItem[] ScreenItems { get { return _screenItems; } }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            if (e.Parameter != null && e.Parameter is string)
            {
                var pair = JsonConvert.DeserializeObject<KeyValuePair<string, string>>(e.Parameter.ToString());
                _destinationView = pair.Key;
                _destinationParameter = pair.Value;
            }
            else
            {
                _destinationView = null;
                _destinationParameter = null;
            }

            base.OnNavigatedTo(e, viewModelState);
        }

        private void OnBuyPermanentCommand()
        {
            BuyFull(true);
        }

        private void OnBuyMonthlyCommand()
        {
            BuyFull(false);
        }

        private async void BuyFull(bool isPermanent)
        {
            var result = await _purchaseService.BuyFullVersion(isPermanent);

            if (result == ProductPurchaseStatus.Succeeded ||
                result == ProductPurchaseStatus.AlreadyPurchased)
            {
                _dialogsService.Show(_locService["Message_Purchase_Success_Text"],
                    _locService["Message_Purchase_Success_Title"]);

                if (_destinationView == null)
                    _navigationService.GoBack();
                else
                {
                    _navigationService.Navigate(_destinationView, _destinationParameter);
                    _navigationService.RemoveLastPage("PurchaseView");
                }
            }
            else
            {
                _dialogsService.Show(_locService["Message_Purchase_Failed_Text"],
                    _locService["Message_Purchase_Failed_Title"]);
            }
        }

        private string _destinationView;
        private string _destinationParameter;

        private readonly IPurchaseService _purchaseService;
        private readonly INavigationService _navigationService;
        private readonly IDialogsService _dialogsService;
        private readonly ScreenItem[] _screenItems;
        private readonly ILocService _locService;

        public sealed class ScreenItem
        {
            public ScreenItem(string line1, string line2, int imageIndex)
                : this(line1, line2, imageIndex, false) { }

            public ScreenItem(string line1, string line2, int imageIndex, bool isPurchaseItem)
            {
                Line1 = line1;
                Line2 = line2;
                _imageIndex = imageIndex;
                IsPurchaseItem = isPurchaseItem;
            }

            public string Line1 { get; private set; }
            public string Line2 { get; private set; }
            public string ImageSource { get { return String.Format(IMAGE_BASE_SOURCE, _imageIndex); } }
            public bool IsPurchaseItem { get; private set; }

            public DelegateCommand BuyMonthlyCommand { get; set; }
            public DelegateCommand BuyPermanentCommand { get; set; }

            private readonly int _imageIndex;
            private const string IMAGE_BASE_SOURCE = "ms-appx:///Assets/Purchase/{0}.png";
        }
    }
}
