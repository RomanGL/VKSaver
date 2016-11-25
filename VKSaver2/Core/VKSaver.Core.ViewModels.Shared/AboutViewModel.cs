﻿using Microsoft.Practices.Prism.StoreApps;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Text;
using VKSaver.Core.Services.Interfaces;
using Windows.ApplicationModel;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class AboutViewModel : ViewModelBase
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
            var version = Package.Current.Id.Version;
            Version = string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);

            base.OnNavigatedTo(e, viewModelState);
        }

        public async void OnSendEmailCommand()
        {
            await _emailService.SendSupportEmail();
        }

        private readonly IEmailService _emailService;
    }
}