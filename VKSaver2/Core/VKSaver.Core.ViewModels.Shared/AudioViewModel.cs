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
using System.Linq;
using System.Threading.Tasks;
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
            IAppLoaderService appLoaderService,
            int maxPlayingTracks = -1)
            : base(locService)
        {            
            _navigationService = navigationService;
            _playerService = playerService;
            _appLoaderService = appLoaderService;

            PlayTracksCommand = new DelegateCommand<T>(OnPlayTracksCommand); 
            PlaySelectedCommand = new DelegateCommand(OnPlaySelectedCommand, HasSelectedItems);

            _maxPlayingTracks = maxPlayingTracks;
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

            await Task.Run(async () =>
            {
                var audiosList = GetAudiosList(); 
                if (audiosList == null)
                {
                    _appLoaderService.Hide();
                    return;
                }               

                int trackIndex = audiosList.IndexOf(track);
                if (_maxPlayingTracks != -1 && audiosList.Count > _maxPlayingTracks)
                {
                    int newIndex = 0;
                    var newTracks = audiosList.GetFromCentre(trackIndex, _maxPlayingTracks, out newIndex);

                    PrepareTracksBeforePlay(newTracks);
                    await _playerService.PlayNewTracks(newTracks.Select(t => ConvertToPlayerTrack(t)), newIndex);
                }
                else
                {
                    PrepareTracksBeforePlay(audiosList);
                    await _playerService.PlayNewTracks(audiosList.Select(t => ConvertToPlayerTrack(t)), trackIndex);
                }
            });

            _navigationService.Navigate("PlayerView", null);
            _appLoaderService.Hide();
        }

        private async void OnPlaySelectedCommand()
        {
            _appLoaderService.Show();

            await Task.Run(async () =>
            {
                var tracks = SelectedItems.Cast<T>().ToList();
                PrepareTracksBeforePlay(tracks);

                if (_maxPlayingTracks != -1 && tracks.Count > _maxPlayingTracks)
                {
                    int newIndex = 0;
                    var newTracks = tracks.GetFromCentre(0, _maxPlayingTracks, out newIndex);

                    PrepareTracksBeforePlay(newTracks);
                    await _playerService.PlayNewTracks(newTracks.Select(t => ConvertToPlayerTrack(t)), newIndex);
                }
                else
                {
                    PrepareTracksBeforePlay(tracks);
                    await _playerService.PlayNewTracks(tracks.Select(t => ConvertToPlayerTrack(t)), 0);
                }                 
            });

            _navigationService.Navigate("PlayerView", null);
            _appLoaderService.Hide();
        }

        protected override void OnSelectionChangedCommand()
        {            
            PlaySelectedCommand.RaiseCanExecuteChanged();
            base.OnSelectionChangedCommand();            
        }

        private readonly int _maxPlayingTracks;

        protected readonly INavigationService _navigationService;
        protected readonly IPlayerService _playerService;
        protected readonly IAppLoaderService _appLoaderService;        
    }
}
