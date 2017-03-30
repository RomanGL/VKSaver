#if WINDOWS_UWP
using Prism.Commands;
using Windows.ApplicationModel;
#elif WINDOWS_PHONE_APP
using Microsoft.Practices.Prism.StoreApps;
using Windows.ApplicationModel;
#else
#endif

using PropertyChanged;
using System.Collections.Generic;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels.Common;
using NavigatedToEventArgs = VKSaver.Core.ViewModels.Common.Navigation.NavigatedToEventArgs;

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

        public override void AppOnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
#if WINDOWS_UWP || WINDOWS_PHONE_APP
            var version = Package.Current.Id.Version;
            Version = string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
#else
            Version = "TODO";
#endif

            base.AppOnNavigatedTo(e, viewModelState);
        }

        public async void OnSendEmailCommand()
        {
            await _emailService.SendSupportEmail();
        }

        private readonly IEmailService _emailService;
    }
}
