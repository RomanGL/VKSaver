#if WINDOWS_UWP
using Prism.Commands;
using Prism.Windows.Mvvm;
using Prism.Windows.Navigation;
#else
using Microsoft.Practices.Prism.StoreApps;
#endif

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using PropertyChanged;
using VKSaver.Core.Models;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class VKAdInfoViewModel : ViewModelBase
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
                await Launcher.LaunchUriAsync(new Uri(Data.ExternalLink));
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
