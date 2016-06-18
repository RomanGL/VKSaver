using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using Newtonsoft.Json;
using OneTeam.SDK.Core;
using OneTeam.SDK.VK.Models.Audio;
using OneTeam.SDK.VK.Services.Interfaces;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Player;
using VKSaver.Core.Services.Interfaces;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class TrackLyricsViewModel : ViewModelBase
    {
        public TrackLyricsViewModel(IVKService vkService, INavigationService navigationService,
            IDialogsService dialogService)
        {
            _vkService = vkService;
            _navigationService = navigationService;
            _dialogService = dialogService;

            ReloadLyricsCommand = new DelegateCommand(OnReloadLyricsCommand);
        }

        public string Lyrics { get; private set; }

        public ImageSource ArtistImage { get; private set; }

        public ContentState LyricsState { get; private set; }

        public IPlayerTrack Track { get; private set; }

        [DoNotNotify]
        public DelegateCommand ReloadLyricsCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            if (viewModelState.Count > 0)
            {
                Lyrics = viewModelState[nameof(Lyrics)].ToString();
                Track = JsonConvert.DeserializeObject<PlayerTrack>(
                    viewModelState[nameof(Track)].ToString());

            }
            else if (e.Parameter != null)
            {
                var data = JsonConvert.DeserializeObject<KeyValuePair<string, string>>(e.Parameter.ToString());                
                Track = JsonConvert.DeserializeObject<PlayerTrack>(data.Key);

                if (data.Key != null)
                    ArtistImage = new BitmapImage(new Uri(data.Value));
            }

            if (String.IsNullOrEmpty(Lyrics))
                LoadLyrics();

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                viewModelState[nameof(Track)] = JsonConvert.SerializeObject(Track);
                viewModelState[nameof(Lyrics)] = Lyrics;
            }

            LyricsState = ContentState.None;

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private void OnReloadLyricsCommand()
        {
            LoadLyrics();
        }

        private async void LoadLyrics()
        {
            if (Track.LyricsID == 0)
            {
                ShowError();
                return;
            }

            LyricsState = ContentState.Loading;
            var parameters = new Dictionary<string, string>
            {
                { "lyrics_id", Track.LyricsID.ToString() }
            };

            var request = new Request<VKAudioLyrics>("audio.getLyrics", parameters);
            var response = await _vkService.ExecuteRequestAsync(request);

            if (response.IsSuccess)
            {
                if (String.IsNullOrWhiteSpace(response.Response.Text))
                {
                    ShowError();
                    return;
                }

                Lyrics = response.Response.Text;
                LyricsState = ContentState.Normal;
            }
            else
                LyricsState = ContentState.Error;
        }

        private void ShowError()
        {
            _dialogService.Show("Не удалось найти текст для этой аудиозаписи.", "Ошибка");
            LyricsState = ContentState.NoData;
        }
        
        private readonly IVKService _vkService;
        private readonly INavigationService _navigationService;
        private readonly IDialogsService _dialogService;
    }
}
