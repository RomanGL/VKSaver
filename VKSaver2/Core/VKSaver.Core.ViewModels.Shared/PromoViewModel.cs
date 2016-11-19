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
using System.Collections.Generic;
using VKSaver.Core.Services;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class PromoViewModel : ViewModelBase
    {
        public PromoViewModel(
            INavigationService navigationService, 
            ILocService locService, 
            IVKLoginService vkLoginService, 
            ISettingsService settingsService,
            ILaunchViewResolver launchViewResolver)
        {
            _navigationService = navigationService;
            _locService = locService;
            _vkLoginService = vkLoginService;
            _settingsService = settingsService;
            _launchViewResolver = launchViewResolver;

            FillScreenItems();
        }
        
        [DoNotNotify]
        public List<ScreenItem> ScreenItems { get; private set; }

        private void FillScreenItems()
        {
            ScreenItems = new List<ScreenItem>();
            ScreenItems.Add(new ScreenItem(
                _locService["PromoView_Screen1_Line1_Text"],
                _locService["PromoView_Screen1_Line2_Text"], 1));

            int promoIndex = _settingsService.Get(AppConstants.CURRENT_PROMO_INDEX_PARAMETER, AppConstants.DEFAULT_PROMO_INDEX);
            promoIndex++;

            while (promoIndex <= AppConstants.CURRENT_PROMO_INDEX)
            {
                int count = GetItemsCountFromPromoIndex(promoIndex);
                for (int i = 1; i <= count; i++)
                {
                    ScreenItems.Add(new ScreenItem(
                        _locService[$"PromoView_Screen{promoIndex}{i}_Line1_Text"],
                        _locService[$"PromoView_Screen{promoIndex}{i}_Line2_Text"], 
                        promoIndex * 10 + i));  // Получаем индекс картинки. Например, 2 * 10 + 2 = 22. Картинка 22.
                }

                promoIndex++;
            }

            ScreenItems.Add(new ScreenItem(
                _locService["PromoView_Screen0_Line1_Text"],
                _locService["PromoView_Screen0_Line2_Text"], 0, true)
                {
                    LetsGoCommand = new DelegateCommand(OnLetsGoCommand)
                });
        }

        private async void OnLetsGoCommand()
        {
            _navigationService.ClearHistory();

            if (_vkLoginService.IsAuthorized)
            {
                if (!_launchViewResolver.TryOpenSpecialViews() && await _launchViewResolver.EnsureDatabaseUpdated() == false)
                {
                    _launchViewResolver.OpenDefaultView();
                }
            }
            else
                _navigationService.Navigate(AppConstants.DEFAULT_LOGIN_VIEW, null);
            
            _settingsService.Set(AppConstants.CURRENT_PROMO_INDEX_PARAMETER, AppConstants.CURRENT_PROMO_INDEX);
        }
        
        private static int GetItemsCountFromPromoIndex(int promoIndex)
        {
            switch (promoIndex)
            {
                case 2:
                    return AppConstants.COUNT_OF_PROMO_INDEX_2;
                default:
                    return 0;
            }
        }

        private readonly INavigationService _navigationService;
        private readonly ILocService _locService;
        private readonly IVKLoginService _vkLoginService;
        private readonly ISettingsService _settingsService;
        private readonly ILaunchViewResolver _launchViewResolver;

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
