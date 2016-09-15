using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using ModernDev.InTouch;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using VKSaver.Core.Models.Player;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels.Common;
using Windows.UI.Xaml.Controls;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public abstract class AudioViewModel<T> : SelectionViewModel
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

            PlayTracksCommand = new DelegateCommand<T>(OnPlayTracksCommand); 
            PlaySelectedCommand = new DelegateCommand(OnPlaySelectedCommand, HasSelectedItems);
        }

        [DoNotNotify]
        public DelegateCommand<T> PlayTracksCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand PlaySelectedCommand { get; private set; }  
        
        protected abstract IList<T> GetAudiosList();

        protected abstract IPlayerTrack ConvertToPlayerTrack(T track);

        protected virtual void PrepareTracksBeforePlay(IEnumerable<T> tracks) { }

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

        private async void OnPlayTracksCommand(T track)
        {
            _appLoaderService.Show();

            var audiosList = GetAudiosList();
            PrepareTracksBeforePlay(audiosList);

            await _playerService.PlayNewTracks(audiosList.Select(t => ConvertToPlayerTrack(t)), audiosList.IndexOf(track));
            _navigationService.Navigate("PlayerView", null);

            _appLoaderService.Hide();
        }

        private async void OnPlaySelectedCommand()
        {
            _appLoaderService.Show();

            var tracks = SelectedItems.Cast<T>();
            PrepareTracksBeforePlay(tracks);

            var toPlay = tracks.Select(t => ConvertToPlayerTrack(t));
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
