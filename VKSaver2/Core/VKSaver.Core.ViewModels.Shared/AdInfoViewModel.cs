#if WINDOWS_UWP
using Prism.Commands;
using Windows.System;
#elif WINDOWS_PHONE_APP
using Microsoft.Practices.Prism.StoreApps;
using Windows.System;
#elif ANDROID
using VKSaver.Core.Toolkit.Commands;
#endif

using System;
using System.Collections.Generic;
using VKSaver.Core.Services.Interfaces;
using PropertyChanged;
using VKSaver.Core.Toolkit;
using NavigatedToEventArgs = VKSaver.Core.Toolkit.Navigation.NavigatedToEventArgs;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class AdInfoViewModel : VKSaverViewModel
    {
        public AdInfoViewModel(ILocService locService)
        {
            _locService = locService;
            ActionCommand = new DelegateCommand(OnActionCommand);
        }

        public string AdImage { get; private set; }
        public string AdTitle { get; private set; }
        public string AdText { get; private set; }

        public string ActionText { get; private set; }
        public DelegateCommand ActionCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            string adName = e.Parameter.ToString();

            AdImage = String.Format(AD_IMAGE_MASK, adName);

            AdTitle = _locService[String.Format(AD_TITLE_MASK, adName)];
            AdText = _locService[String.Format(AD_TEXT_MASK, adName)];
            ActionText = _locService[String.Format(AD_ACTION_TEXT_MASK, adName)];

            _adActionLink = _locService[String.Format(AD_ACTION_LINK_MASK, adName)];

            base.OnNavigatedTo(e, viewModelState);
        }

        private async void OnActionCommand()
        {
#if WINDOWS_UWP || WINDOWS_PHONE_APP
            await Launcher.LaunchUriAsync(new Uri(_adActionLink));
#else
            throw new NotImplementedException();
#endif
        }

        private string _adActionLink;

        private readonly ILocService _locService;

        private const string AD_TITLE_MASK = "Ads_{0}_Title";
        private const string AD_TEXT_MASK = "Ads_{0}_Text";
        private const string AD_ACTION_TEXT_MASK = "Ads_{0}_Action_Text";
        private const string AD_ACTION_LINK_MASK = "Ads_{0}_Action_Link";
        private const string AD_IMAGE_MASK = "ms-appx:///Assets/Ads/{0}.jpg";
    }
}
