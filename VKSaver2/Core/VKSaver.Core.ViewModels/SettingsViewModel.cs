﻿using Microsoft.Practices.Prism.StoreApps;
using ModernDev.InTouch;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Common;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class SettingsViewModel : ViewModelBase
    {
        public SettingsViewModel(IVKLoginService vkLoginService, ISettingsService settingsService,
            ILastFmLoginService lastFmLoginSevice, IPurchaseService purchaseService,
            IInTouchWrapper inTouchWrapper, InTouch inTouch)
        {
            _vkLoginService = vkLoginService;
            _settingsService = settingsService;
            _lastFmLoginService = lastFmLoginSevice;
            _purchaseService = purchaseService;
            _inTouchWrapper = inTouchWrapper;
            _inTouch = inTouch;

            Authorizations = new ObservableCollection<IServiceAuthorization>();
        }

        public ObservableCollection<IServiceAuthorization> Authorizations { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            var auth = (VKAuthorization)_vkLoginService.GetServiceAuthorization();

            Authorizations.Add(auth);
            Authorizations.Add(_lastFmLoginService.GetServiceAuthorization());

            _lastFmLoginService.UserLogout += LastFmLoginService_UserLogout;

            LoadVKUserName(auth);
            base.OnNavigatedTo(e, viewModelState);
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
            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Users.Get());
            if (!response.IsError)
            {
                var user = response.Data[0];
                auth.UserName = $"{user.FirstName} {user.LastName}";
            }
        }

        private readonly IVKLoginService _vkLoginService;
        private readonly ISettingsService _settingsService;
        private readonly ILastFmLoginService _lastFmLoginService;
        private readonly IPurchaseService _purchaseService;
        private readonly IInTouchWrapper _inTouchWrapper;
        private readonly InTouch _inTouch;
    }
}
