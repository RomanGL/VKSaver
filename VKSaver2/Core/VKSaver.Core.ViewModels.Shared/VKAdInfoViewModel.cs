#if WINDOWS_UWP
using Prism.Commands;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
using Windows.System;
#elif WINDOWS_PHONE_APP
using Microsoft.Practices.Prism.StoreApps;
using Windows.System;
#elif ANDROID
using VKSaver.Core.Toolkit.Commands;
#endif

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PropertyChanged;
using VKSaver.Core.Models;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Toolkit;
using VKSaver.Core.Toolkit.Navigation;
using NavigatedToEventArgs = VKSaver.Core.Toolkit.Navigation.NavigatedToEventArgs;
using NavigatingFromEventArgs = VKSaver.Core.Toolkit.Navigation.NavigatingFromEventArgs;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class VKAdInfoViewModel : VKSaverViewModel
    {
        public VKAdInfoViewModel(
            IVKAdService vkAdService,
            IAppLoaderService appLoaderService,
            ILogService logService)
        {
            _vkAdService = vkAdService;
            _appLoaderService = appLoaderService;
            _logService = logService;

            ReloadDataCommand = new DelegateCommand(OnReloadDataCommand);
            ActionCommand = new DelegateCommand(OnActionCommand);
        }

        public VKAdData Data { get; private set; }

        public ContentState DataState { get; private set; }

        [DoNotNotify]
        public DelegateCommand ReloadDataCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand ActionCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            _adId = e.Parameter.ToString();
            if (viewModelState.Count > 0)
            {
                Data = JsonConvert.DeserializeObject<VKAdData>(viewModelState["Data"].ToString());
                _isSuccess = (bool)viewModelState["IsSuccess"];
            }

            if (Data == null)
                LoadData(_adId);

            base.OnNavigatedTo(e, viewModelState);
        }

        public override async void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (e.NavigationMode == NavigationMode.New || suspending)
            {
                viewModelState["Data"] = JsonConvert.SerializeObject(Data);
                viewModelState["IsSuccess"] = _isSuccess;
            }
            else if (e.NavigationMode == NavigationMode.Back && Data != null && !_isSuccess)
            {
                await _vkAdService.ReportAdAsync(Data.AdName, false);
            }

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private async void LoadData(string adId)
        {
            DataState = ContentState.Loading;
            Data = await _vkAdService.GetAdDataAsync(adId);
            DataState = Data == null ? ContentState.Error : ContentState.Normal;
        }

        private async void OnActionCommand()
        {
            _appLoaderService.Show();

            try
            {
                _isSuccess = true;
                await _vkAdService.ReportAdAsync(Data.AdName, true);
                await Task.Delay(1000);

#if WINDOWS_UWP || WINDOWS_PHONE_APP
                await Launcher.LaunchUriAsync(new Uri(Data.ExternalLink));
#elif ANDROID
                throw new NotImplementedException();
#endif
            }
            catch (Exception e)
            {
                _logService.LogException(e);
            }

            _appLoaderService.Hide();
        }

        private void OnReloadDataCommand() => LoadData(_adId);

        private string _adId;
        private bool _isSuccess;

        private readonly IVKAdService _vkAdService;
        private readonly ILogService _logService;
        private readonly IAppLoaderService _appLoaderService;
    }
}
