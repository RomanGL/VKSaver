using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using PropertyChanged;
using System;
using System.Collections.Generic;
using VKSaver.Core.Services;
using VKSaver.Core.Services.Database;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class UpdatingDatabaseViewModel : ViewModelBase
    {
        public UpdatingDatabaseViewModel(
            INavigationService navigationService,
            ILibraryDatabaseService musicDatabaseService, 
            ISettingsService settingsService)
        {
            _navigationService = navigationService;
            _libraryDatabaseService = musicDatabaseService;
            _settingsService = settingsService;
        }

        public int Total { get; private set; }
        public int Current { get; private set; }
        public int TotalSteps { get; private set; }
        public int CurrentStep { get; private set; }
        public string OperationName { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            _settingsService.Set(AppConstants.CURRENT_FIRST_START_VIEW_PARAMETER, "UpdatingDatabaseView");            

            _libraryDatabaseService.UpdateProgressChanged += LibraryDatabaseService_UpdateProgressChanged;
            _libraryDatabaseService.Update();

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (!suspending)
            {
                _libraryDatabaseService.UpdateProgressChanged -= LibraryDatabaseService_UpdateProgressChanged;
                _settingsService.Set(AppConstants.CURRENT_FIRST_START_INDEX_PARAMETER, AppConstants.CURRENT_FIRST_START_INDEX);
                _settingsService.Set(AppConstants.CURRENT_LIBRARY_INDEX_PARAMETER, AppConstants.CURRENT_LIBRARY_INDEX);
            }

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private void LibraryDatabaseService_UpdateProgressChanged(ILibraryDatabaseService sender, DBUpdateProgressChangedEventArgs args)
        {
            TotalSteps = 2;
            Current = args.CurrentItem;
            Total = args.TotalItems;

            switch (args.Step)
            {
                case DatabaseUpdateStepType.Started:
                    OperationName = "Снаряжаем рыцарей...";
                    CurrentStep = 0;
                    break;
                case DatabaseUpdateStepType.SearchingFiles:
                    OperationName = "Ищем аудиофайлы...";
                    CurrentStep = 1;
                    break;
                case DatabaseUpdateStepType.PreparingDatabase:
                    OperationName = "Переписываем манускрипты...";
                    CurrentStep = 2;
                    break;
                case DatabaseUpdateStepType.SearchingArtists:
                    OperationName = "Фотографируем исполнителей...";
                    CurrentStep = 3;
                    break;
                case DatabaseUpdateStepType.SearchingAlbums:
                    OperationName = "Рисуем картинки альбомов...";
                    CurrentStep = 4;
                    break;
                case DatabaseUpdateStepType.Completed:
                    _settingsService.Set(AppConstants.CURRENT_FIRST_START_VIEW_PARAMETER, "Completed");
                    _navigationService.Navigate("MainView", null);
                    _navigationService.ClearHistory();
                    break;
                default:
                    OperationName = String.Empty;
                    break;
            }
        }

        private readonly INavigationService _navigationService;
        private readonly ILibraryDatabaseService _libraryDatabaseService;
        private readonly ISettingsService _settingsService;
    }
}
