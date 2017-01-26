using Microsoft.Practices.Prism.StoreApps;
using System;
using System.Collections.Generic;
using System.Text;
using PropertyChanged;
using VKSaver.Core.Models;
using VKSaver.Core.Services;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class VKAdInfoViewModel : ViewModelBase
    {
        public VKAdInfoViewModel(
            IVKAdService vkAdService,
            IMetricaService metricaService)
        {
            _vkAdService = vkAdService;
        }

        public VKAdData Data { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            LoadData("-138475410_2");
            base.OnNavigatedTo(e, viewModelState);
        }

        private async void LoadData(string adId)
        {
            Data = await _vkAdService.GetAdDataAsync(adId);
        }

        private readonly IVKAdService _vkAdService;
    }
}
