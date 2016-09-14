using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using ModernDev.InTouch;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels.Common;
using Windows.UI.Xaml.Controls;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public abstract class AudioViewModel : SelectionViewModel
    {
        protected AudioViewModel(
            IPlayerService playerService, 
            ILocService locService,
            INavigationService navigationService, 
            IAppLoaderService appLoaderService)
            : base(locService)
        {            
            _navigationService = navigationService;
            _playerService = playerService;
            _appLoaderService = appLoaderService;

            PlayTracksCommand = new DelegateCommand<Audio>(OnPlayTracksCommand); 
            PlaySelectedCommand = new DelegateCommand(OnPlaySelectedCommand, HasSelectedItems);
        }

        [DoNotNotify]
        public DelegateCommand<Audio> PlayTracksCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand PlaySelectedCommand { get; private set; }  

        protected abstract IList<Audio> GetAudiosList();

        protected override void CreateSelectionAppBarButtons()
        {      
            AppBarItems.Add(new AppBarButton
            {
                Label = _locService["AppBarButton_Play_Text"],
                Icon = new FontIcon { Glyph = "\uE102" },
                Command = PlaySelectedCommand
            });

            base.CreateSelectionAppBarButtons();
        }

        private async void OnPlayTracksCommand(Audio track)
        {
            _appLoaderService.Show();

            var audiosList = GetAudiosList();

            await _playerService.PlayNewTracks(audiosList.ToPlayerTracks(), audiosList.IndexOf(track));
            _navigationService.Navigate("PlayerView", null);

            _appLoaderService.Hide();
        }

        private async void OnPlaySelectedCommand()
        {
            _appLoaderService.Show();

            var toPlay = SelectedItems.Cast<Audio>().ToPlayerTracks();
            await _playerService.PlayNewTracks(toPlay, 0);
            _navigationService.Navigate("PlayerView", null);

            _appLoaderService.Hide();
        }

        protected override void OnSelectionChangedCommand()
        {            
            PlaySelectedCommand.RaiseCanExecuteChanged();
            base.OnSelectionChangedCommand();            
        }

        protected readonly INavigationService _navigationService;
        protected readonly IPlayerService _playerService;
        protected readonly IAppLoaderService _appLoaderService;
    }
}
