#if WINDOWS_UWP
using Prism.Windows.Mvvm;
using Prism.Commands;
using Prism.Windows.Navigation;
#else
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#endif

using System;
using System.Collections.Generic;
using System.Text;
using VKSaver.Core.Services;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Services.VksmExtraction;
using Windows.UI.Core;
using PropertyChanged;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class VksmExtractionViewModel : ViewModelBase
    {
        public VksmExtractionViewModel(
            INavigationService navigationService, 
            IVksmExtractionService vksmExtractionService,
            ISettingsService settingsService,
            ILocService locService,
            IDispatcherWrapper dispatcherWrapper)
        {
            _navigationService = navigationService;
            _vksmExtractionService = vksmExtractionService;
            _settingsService = settingsService;
            _locService = locService;
            _dispatcherWrapper = dispatcherWrapper;
        }

        public string SearchingText { get; private set; }

        public long CompletedBytes { get; private set; }
        public long TotalBytes { get; private set; }

        public int CurrentFile { get; private set; }
        public int TotalFiles { get; private set; }

        public override async void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            _settingsService.Set(AppConstants.CURRENT_FIRST_START_VIEW_PARAMETER, "VksmExtractionView");

            TotalFiles = 1;
            TotalBytes = 0;

            _vksmExtractionService.ExtractionCompleted += ExtractionCompleted;
            _vksmExtractionService.SearchingProgressChanged += SearchingProgressChanged;
            _vksmExtractionService.ExtractingProgressChanged += ExtractingProgressChanged;

            base.OnNavigatedTo(e, viewModelState);

            await _vksmExtractionService.ExtractAsync();
        }
                
        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (!suspending)
            {
                _vksmExtractionService.ExtractionCompleted -= ExtractionCompleted;
                _vksmExtractionService.SearchingProgressChanged -= SearchingProgressChanged;
                _vksmExtractionService.ExtractingProgressChanged -= ExtractingProgressChanged;
            }

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private async void ExtractingProgressChanged(IVksmExtractionService sender, ExtractingProgressChangedEventArgs e)
        {
            await _dispatcherWrapper.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if ((e.TotalBytes == 0 && e.CompletedBytes == 0) || e.TotalBytes == e.CompletedBytes)
                {
                    TotalBytes = 1;
                    CompletedBytes = 0;
                }
                else
                {
                    TotalBytes = e.TotalBytes;
                    CompletedBytes = e.CompletedBytes;
                }
            });
        }

        private async void SearchingProgressChanged(IVksmExtractionService sender, SearchingProgressChangedEventArgs e)
        {
            await _dispatcherWrapper.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                TotalFiles = e.TotalFiles;
                CurrentFile = e.CurrentFile;

                SearchingText = String.Format(_locService["UpdatingDatabseView_SearchingFilesStep_Text"],
                            CurrentFile, TotalFiles);
            });
        }

        private void ExtractionCompleted(IVksmExtractionService sender, EventArgs e)
        {
            _navigationService.ClearHistory();
            _navigationService.Navigate("UpdatingDatabaseView", null);
        }

        private readonly INavigationService _navigationService;
        private readonly IVksmExtractionService _vksmExtractionService;
        private readonly ISettingsService _settingsService;
        private readonly ILocService _locService;
        private readonly IDispatcherWrapper _dispatcherWrapper;
    }
}
