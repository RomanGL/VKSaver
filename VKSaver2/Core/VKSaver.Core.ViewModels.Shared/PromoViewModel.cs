#if WINDOWS_UWP
using Prism.Windows.Mvvm;
using Prism.Commands;
using Prism.Windows.Navigation;
#else
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#endif

using PropertyChanged;
using System;
using VKSaver.Core.Services;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class PromoViewModel : ViewModelBase
    {
        public PromoViewModel(INavigationService navigationService, ILocService locService, 
            IVKLoginService vkLoginService, ISettingsService settingsService)
        {
            _navigationService = navigationService;
            _locService = locService;
            _vkLoginService = vkLoginService;
            _settingsService = settingsService;

            _screenItems = new ScreenItem[]
            {
                new ScreenItem(_locService["PromoView_Screen1_Line1_Text"], 
                    _locService["PromoView_Screen1_Line2_Text"], 1),
                new ScreenItem(_locService["PromoView_Screen2_Line1_Text"],
                    _locService["PromoView_Screen2_Line2_Text"], 2),
                new ScreenItem(_locService["PromoView_Screen3_Line1_Text"],
                    _locService["PromoView_Screen3_Line2_Text"], 3),
                new ScreenItem(_locService["PromoView_Screen4_Line1_Text"],
                    _locService["PromoView_Screen4_Line2_Text"], 4),
                new ScreenItem(_locService["PromoView_Screen5_Line1_Text"],
                    _locService["PromoView_Screen5_Line2_Text"], 5),
                new ScreenItem(_locService["PromoView_Screen0_Line1_Text"],
                    _locService["PromoView_Screen0_Line2_Text"], 0, true)
                {
                    LetsGoCommand = new DelegateCommand(OnLetsGoCommand)
                }
            };
        }
        
        [DoNotNotify]
        public ScreenItem[] ScreenItems { get { return _screenItems; } }

        private void OnLetsGoCommand()
        {
            if (_vkLoginService.IsAuthorized)
                _navigationService.Navigate("MainView", null);
            else
                _navigationService.Navigate("LoginView", null);

            _navigationService.ClearHistory();
            _settingsService.Set(AppConstants.CURRENT_PROMO_INDEX_PARAMETER, AppConstants.CURRENT_PROMO_INDEX);
        }
        
        private readonly INavigationService _navigationService;
        private readonly ScreenItem[] _screenItems;
        private readonly ILocService _locService;
        private readonly IVKLoginService _vkLoginService;
        private readonly ISettingsService _settingsService;

        public sealed class ScreenItem
        {
            public ScreenItem(string line1, string line2, int imageIndex)
                : this(line1, line2, imageIndex, false) { }

            public ScreenItem(string line1, string line2, int imageIndex, bool isPurchaseItem)
            {
                Line1 = line1;
                Line2 = line2;
                _imageIndex = imageIndex;
                IsLetsGoItem = isPurchaseItem;
            }

            public string Line1 { get; private set; }
            public string Line2 { get; private set; }
            public string ImageSource { get { return String.Format(IMAGE_BASE_SOURCE, _imageIndex); } }
            public bool IsLetsGoItem { get; private set; }

            public DelegateCommand LetsGoCommand { get; set; }

            private readonly int _imageIndex;
            private const string IMAGE_BASE_SOURCE = "ms-appx:///Assets/Promo/{0}.png";
        }
    }
}
