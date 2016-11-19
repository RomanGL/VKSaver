#if WINDOWS_UWP
using Prism.Windows.Mvvm;
using Prism.Commands;
using Prism.Windows.Navigation;
#else
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#endif

using PropertyChanged;
using System;
using System.Collections.Generic;
using VKSaver.Core.Services;
using VKSaver.Core.Services.Database;
using VKSaver.Core.Services.Interfaces;
using Windows.Storage;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class UpdatingDatabaseViewModel : ViewModelBase
    {
        public UpdatingDatabaseViewModel(
            INavigationService navigationService,
            ILibraryDatabaseService musicDatabaseService, 
            ISettingsService settingsService, 
            ILocService locService,
            ILaunchViewResolver launchViewResolver)
        {
            _navigationService = navigationService;
            _libraryDatabaseService = musicDatabaseService;
            _settingsService = settingsService;
            _locService = locService;
            _launchViewResolver = launchViewResolver;
        }
        
        public int Current { get; private set; }
        public int Total { get; private set; }
        public string StepText { get; private set; }
        public string OperationText { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            _settingsService.Set(AppConstants.CURRENT_FIRST_START_VIEW_PARAMETER, "UpdatingDatabaseView");
            _settingsService.Remove(AppConstants.CURRENT_LIBRARY_INDEX_PARAMETER);
            _totalSteps = 2;

            _libraryDatabaseService.UpdateProgressChanged += LibraryDatabaseService_UpdateProgressChanged;
            _libraryDatabaseService.Update();

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (!suspending)
            {
                _libraryDatabaseService.UpdateProgressChanged -= LibraryDatabaseService_UpdateProgressChanged;
                _settingsService.Set(AppConstants.CURRENT_LIBRARY_INDEX_PARAMETER, AppConstants.CURRENT_LIBRARY_INDEX);
            }

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private void LibraryDatabaseService_UpdateProgressChanged(ILibraryDatabaseService sender, DBUpdateProgressChangedEventArgs args)
        {
            Current = args.CurrentItem;
            Total = args.TotalItems;

            switch (args.Step)
            {
                case DatabaseUpdateStepType.Started:
                    OperationText = _locService["UpdatingDatabseView_StartedStep_Text"];
                    if (_currentStep != 0)
                    {
                        _currentStep = 0;
                        UpdateStepText();
                    }
                    break;
                case DatabaseUpdateStepType.SearchingFiles:
                    OperationText = String.Format(_locService["UpdatingDatabseView_SearchingFilesStep_Text"],
                        Current, Total);

                    if (_currentStep != 1)
                    {
                        _currentStep = 1;
                        UpdateStepText();
                    }
                    break;
                case DatabaseUpdateStepType.PreparingDatabase:
                    OperationText = String.Format(_locService["UpdatingDatabseView_PreparingDatabaseStep_Text"],
                        Current, Total);

                    if (_currentStep != 2)
                    {
                        _currentStep = 2;
                        UpdateStepText();
                    }
                    break;
                case DatabaseUpdateStepType.SearchingArtists:
                    OperationText = String.Format(_locService["UpdatingDatabaseView_SearchingArtistsStep_Text"],
                        Current, Total);

                    if (_currentStep != 3)
                    {
                        _currentStep = 3;
                        UpdateStepText();
                    }
                    break;
                case DatabaseUpdateStepType.SearchingAlbums:
                    OperationText = String.Format(_locService["UpdatingDatabaseView_SearchingAlbumsStep_Text"],
                        Current, Total);

                    if (_currentStep != 4)
                    {
                        _currentStep = 4;
                        UpdateStepText();
                    }
                    break;
                case DatabaseUpdateStepType.Completed:
                    _navigationService.ClearHistory();

                    if (_settingsService.Get(AppConstants.CURRENT_FIRST_START_INDEX_PARAMETER, 0) == AppConstants.CURRENT_FIRST_START_INDEX)
                    {
                        _settingsService.Set<string>(AppConstants.CURRENT_FIRST_START_VIEW_PARAMETER, null);
                        _launchViewResolver.OpenDefaultView();
                    }
                    else
                        _navigationService.Navigate("FirstSelectLaunchView", null);
                    break;
            }
        }

        private void UpdateStepText()
        {
            StepText = String.Format(_locService["UpdatingDatabaseView_StepMask_Text"],
                _currentStep, _totalSteps);
        }
        
        private int _totalSteps;
        private int _currentStep = -1;

        private readonly INavigationService _navigationService;
        private readonly ILibraryDatabaseService _libraryDatabaseService;
        private readonly ISettingsService _settingsService;
        private readonly ILocService _locService;
        private readonly ILaunchViewResolver _launchViewResolver;
    }
}
