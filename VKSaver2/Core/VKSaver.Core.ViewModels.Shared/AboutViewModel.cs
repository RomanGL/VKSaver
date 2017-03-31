#if WINDOWS_UWP
using Prism.Commands;
using Windows.ApplicationModel;
#elif WINDOWS_PHONE_APP
using Microsoft.Practices.Prism.StoreApps;
using Windows.ApplicationModel;
#elif ANDROID
using VKSaver.Core.Toolkit.Commands;
#endif

using System;
using PropertyChanged;
using System.Collections.Generic;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Toolkit;
using NavigatedToEventArgs = VKSaver.Core.Toolkit.Navigation.NavigatedToEventArgs;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class AboutViewModel : VKSaverViewModel
    {
        public AboutViewModel(IEmailService emailService)
        {
            _emailService = emailService;

            SendEmailCommand = new DelegateCommand(OnSendEmailCommand);
        }

        public string Version { get; private set; }

        [DoNotNotify]
        public DelegateCommand SendEmailCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
#if WINDOWS_UWP || WINDOWS_PHONE_APP
            var version = Package.Current.Id.Version;
            Version = String.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
#else
            Version = "TODO";
#endif

            base.OnNavigatedTo(e, viewModelState);
        }

        public async void OnSendEmailCommand()
        {
            await _emailService.SendSupportEmail();
        }

        private readonly IEmailService _emailService;
    }
}
