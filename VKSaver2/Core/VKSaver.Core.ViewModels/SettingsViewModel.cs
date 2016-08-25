using Microsoft.Practices.Prism.StoreApps;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class SettingsViewModel : ViewModelBase
    {
        public SettingsViewModel(IVKLoginService vkLoginService, ISettingsService settingsService)
        {
            _vkLoginService = vkLoginService;
            _settingsService = settingsService;

            Authorizations = new ObservableCollection<IServiceAuthorization>();
        }

        public ObservableCollection<IServiceAuthorization> Authorizations { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            Authorizations.Add(_vkLoginService.GetServiceAuthorization());

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            Authorizations.Clear();

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private readonly IVKLoginService _vkLoginService;
        private readonly ISettingsService _settingsService;
    }
}
