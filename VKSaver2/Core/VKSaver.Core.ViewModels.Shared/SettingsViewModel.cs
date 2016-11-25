#if WINDOWS_UWP
using Prism.Windows.Mvvm;
using Prism.Commands;
using Prism.Windows.Navigation;
#else
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#endif

using ModernDev.InTouch;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Models;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services;
using VKSaver.Core.Services.Common;
using VKSaver.Core.Services.Interfaces;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class SettingsViewModel : ViewModelBase
    {
        public SettingsViewModel(
            INavigationService navigationService,
            IVKLoginService vkLoginService, 
            ISettingsService settingsService,
            ILastFmLoginService lastFmLoginSevice, 
            IPurchaseService purchaseService,
            ILocService locService,
            IDialogsService dialogsService,
            IInTouchWrapper inTouchWrapper, 
            InTouch inTouch,
            ILaunchViewResolver launchViewResolver,
            IAppNotificationsService appNotificationsService)
        {
            _navigationService = navigationService;
            _vkLoginService = vkLoginService;
            _settingsService = settingsService;
            _lastFmLoginService = lastFmLoginSevice;
            _purchaseService = purchaseService;
            _inTouchWrapper = inTouchWrapper;
            _inTouch = inTouch;
            _locService = locService;
            _dialogsService = dialogsService;
            _launchViewResolver = launchViewResolver;
            _appNotificationsService = appNotificationsService;

            Authorizations = new ObservableCollection<IServiceAuthorization>();
            UpdateDatabaseCommand = new DelegateCommand(OnUpdateDatabaseCommand);
            ExtractMp3FromVksmCommand = new DelegateCommand(OnExtractMp3FromVksmCommand);
        }

        [DoNotNotify]
        public ObservableCollection<IServiceAuthorization> Authorizations { get; private set; }

        public List<LanguageItem> AvailableLanguages { get; private set; }

        public List<string> AvailableLaunchViews { get { return _launchViewResolver.AvailableLaunchViews; } }

        [AlsoNotifyFor(nameof(AvailableLanguages))]
        public int LanguageIndex
        {
            get
            {
                if (AvailableLanguages == null)
                    return -1;

                var currentLanguage = _locService.CurrentAppLanguage;
                return AvailableLanguages.FindIndex(l => l.Lang == currentLanguage);
            }
            set
            {
                if (AvailableLanguages == null)
                    return;

                _locService.CurrentAppLanguage = AvailableLanguages[value].Lang;
                OnLanguageChanged();

                //_navigationService.ClearHistory();
                //_navigationService.Navigate("MainView", null);
                //_navigationService.Navigate("SettingsView", null);
            }
        }

        [AlsoNotifyFor(nameof(AvailableLaunchViews))]
        public int SelectedLaunchViewIndex
        {
            get { return AvailableLaunchViews.IndexOf(_launchViewResolver.LaunchViewName); }
            set
            {
                _launchViewResolver.LaunchViewName = AvailableLaunchViews[value];
                OnLaunchViewChanged();
            }
        }

        [DoNotNotify]
        public DelegateCommand UpdateDatabaseCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand ExtractMp3FromVksmCommand { get; private set; }

        public int SelectedInternetAccessIndex
        {
            get { return (int)_settingsService.Get(AppConstants.INTERNET_ACCESS_TYPE, InternetAccessType.All); }
            set { _settingsService.Set(AppConstants.INTERNET_ACCESS_TYPE, (InternetAccessType)value); }
        }

        public bool DownloadsNotifications
        {
            get { return _settingsService.Get(AppConstants.DOWNLOADS_NOTIFICATIONS_PARAMETER, true); }
            set
            {
                _settingsService.Set(AppConstants.DOWNLOADS_NOTIFICATIONS_PARAMETER, value);
                OnDownloadsNotificationsChanged();
            }
        }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            var auth = (VKAuthorization)_vkLoginService.GetServiceAuthorization();

            Authorizations.Add(auth);
            Authorizations.Add(_lastFmLoginService.GetServiceAuthorization());

            _lastFmLoginService.UserLogout += LastFmLoginService_UserLogout;

            LoadVKUserName(auth);

            AvailableLanguages = _locService.GetAvailableLanguages();
            base.OnNavigatedTo(e, viewModelState);

            _appNotificationsService.SendNotification(new AppNotification
            {
                Title = "Настройки",
                Content = "Добро пожаловать в настройки!",
                Type = AppNotificationType.Warning
            });
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            Authorizations.Clear();
            _lastFmLoginService.UserLogout -= LastFmLoginService_UserLogout;

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private void LastFmLoginService_UserLogout(ILastFmLoginService sender, EventArgs e)
        {
            var item = Authorizations.FirstOrDefault(s => s.ServiceName == "last.fm");
            if (item == null)
                return;

            Authorizations.Remove(item);
            Authorizations.Add(_lastFmLoginService.GetServiceAuthorization());
        }

        private async void LoadVKUserName(VKAuthorization auth)
        {
            try
            {
                var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Users.Get());
                if (!response.IsError)
                {
                    var user = response.Data[0];
                    auth.UserName = $"{user.FirstName} {user.LastName}";
                }
            }
            catch (Exception) { }
        }

        private void OnUpdateDatabaseCommand()
        {
            _navigationService.ClearHistory();
            _navigationService.Navigate("UpdatingDatabaseView", null);
        }

        private void OnExtractMp3FromVksmCommand()
        {
            _navigationService.ClearHistory();
            _navigationService.Navigate("VksmExtractionView", null);
        }

        private void OnDownloadsNotificationsChanged()
        {
            _dialogsService.Show(_locService["Message_DownloadsNotificationsChanged_Text"],
                _locService["Message_DownloadsNotificationsChanged_Title"]);
        }

        private void OnLanguageChanged()
        {
            _dialogsService.Show(_locService["Message_RestartRequired_Text"],
                _locService["Message_RestartRequired_Title"]);
        }

        private void OnLaunchViewChanged()
        {
            _dialogsService.Show(_locService["Message_RestartRequired_Text"],
                _locService["Message_RestartRequired_Title"]);
        }

        private readonly InTouch _inTouch;
        private readonly INavigationService _navigationService;
        private readonly IVKLoginService _vkLoginService;
        private readonly ISettingsService _settingsService;
        private readonly ILastFmLoginService _lastFmLoginService;
        private readonly IPurchaseService _purchaseService;
        private readonly IInTouchWrapper _inTouchWrapper;
        private readonly ILocService _locService;
        private readonly IDialogsService _dialogsService;        
        private readonly ILaunchViewResolver _launchViewResolver;
        private readonly IAppNotificationsService _appNotificationsService;
    }
}
