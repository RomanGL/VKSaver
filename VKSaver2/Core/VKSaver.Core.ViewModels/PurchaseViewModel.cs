using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Services.Interfaces;
using Windows.ApplicationModel.Store;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class PurchaseViewModel : ViewModelBase
    {
        public PurchaseViewModel(IPurchaseService purchaseService, INavigationService navigationService,
            IDialogsService dialogsService)
        {
            _purchaseService = purchaseService;
            _navigationService = navigationService;
            _dialogsService = dialogsService;

            _screenItems = new ScreenItem[]
            {
                new ScreenItem("Только самые лучшие треки!", "В полной версии ВКачай", 1),
                new ScreenItem("Мировые чарты исполнителей и треков", "Будьте в тренде!", 2),
                new ScreenItem("Персональные рекомендации", "Слушайте то, что Вам нравится!", 3),
                new ScreenItem("Ни один документ не потеряется!", "Отправка и загрузка документов ВКонтакте", 4),
                new ScreenItem("Все видеозаписи под рукой", "Смотрите и загружайте любимые видео!", 5),
                new ScreenItem("ВКачай 2", "Все самое лучшее - только для Вас!", 0, true)
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
                _dialogsService.Show("Теперь вы счастливый владелец полной версии ВКачай 2!",
                    "Спасибо за покупку");

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
                _dialogsService.Show("Не удалось произвести покупку. Мы очень сожалеем, повторите попытку позднее.",
                    "Покупка не удалась");
            }
        }

        private string _destinationView;
        private string _destinationParameter;

        private readonly IPurchaseService _purchaseService;
        private readonly INavigationService _navigationService;
        private readonly IDialogsService _dialogsService;
        private readonly ScreenItem[] _screenItems;

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
