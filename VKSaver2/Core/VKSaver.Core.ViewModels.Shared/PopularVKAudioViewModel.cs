using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using ModernDev.InTouch;
using Newtonsoft.Json;
using PropertyChanged;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels.Collections;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class PopularVKAudioViewModel : VKAudioImplementedViewModel
    {
        public PopularVKAudioViewModel(
            InTouch inTouch,
            IAppLoaderService appLoaderService,
            IDialogsService dialogsService,
            IInTouchWrapper inTouchWrapper,
            IDownloadsServiceHelper downloadsServiceHelper,
            IPlayerService playerService,
            ILocService locService,
            INavigationService navigationService,
            ISettingsService settingsService)
            : base(inTouch, appLoaderService, dialogsService, inTouchWrapper,
                  downloadsServiceHelper, playerService, locService, navigationService)
        {
            _settingsService = settingsService;

            IsReloadButtonSupported = true;
            IsPlayButtonSupported = true;
            IsShuffleButtonSupported = true;

            ShowFilterFlyoutCommand = new DelegateCommand(OnShowFilterFlyoutCommand);
            FilterFlyoutClosedCommand = new DelegateCommand(OnFilterFlyoutClosedCommand);

            CreateFilters();
        }

        public PaginatedCollection<Audio> Audios { get; private set; }

        public List<AudioGenresFilterItem> Genres { get; private set; }

        public int SelectedGenresItem
        {
            get { return _settingsService.Get(SELECTED_GENRES_ITEM_PARAMETER_NAME, 0); }
            set
            {
                _settingsService.Set(SELECTED_GENRES_ITEM_PARAMETER_NAME, value < 0 ? 0 : value);
                _isFilterChanged = true;
            }
        }

        public bool OnlyEng
        {
            get { return _settingsService.Get(ONLY_ENG_PARAMETER_NAME, false); }
            set
            {
                _settingsService.Set(ONLY_ENG_PARAMETER_NAME, value);
                _isFilterChanged = true;
            }
        }

        [DoNotCheckEquality]
        public bool IsFilterFlyoutShowed { get; private set; }

        [DoNotNotify]
        public DelegateCommand ShowFilterFlyoutCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand FilterFlyoutClosedCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            if (viewModelState.Count > 0)
            {
                Audios = JsonConvert.DeserializeObject<PaginatedCollection<Audio>>
                    (viewModelState[nameof(Audios)].ToString());
                _audiosOffset = (int)viewModelState[nameof(_audiosOffset)];

                Audios.LoadMoreItems = LoadMoreAudios;
            }
            else
            {
                Audios = new PaginatedCollection<Audio>(LoadMoreAudios);
                _audiosOffset = 0;
            }

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (!IsSelectionMode && e.NavigationMode == NavigationMode.New)
            {
                viewModelState[nameof(Audios)] = JsonConvert.SerializeObject(Audios.ToList());
                viewModelState[nameof(_audiosOffset)] = _audiosOffset;
            }

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        protected override IList<Audio> GetAudiosList()
        {
            return Audios;
        }

        protected override IList GetSelectionList()
        {
            return Audios;
        }

        protected override void CreateDefaultAppBarButtons()
        {
            base.CreateDefaultAppBarButtons();

            AppBarItems.Add(new AppBarButton
            {
                Label = _locService["AppBarButton_Filter_Text"],
                Icon = new FontIcon { Glyph = "\uE16E", FontSize = 14 },
                Command = ShowFilterFlyoutCommand
            });
        }

        protected override void OnReloadContentCommand()
        {
            _audiosOffset = 0;
            Audios?.Refresh();
        }

        private void OnFilterFlyoutClosedCommand()
        {
            if (_isFilterChanged)
            {
                _isFilterChanged = false;
                OnReloadContentCommand();
            }
        }

        private void OnShowFilterFlyoutCommand()
        {
            IsFilterFlyoutShowed = !IsFilterFlyoutShowed;
        }

        private void CreateFilters()
        {
            Genres = new List<AudioGenresFilterItem>
            {
                new AudioGenresFilterItem(_locService["PopularVKAudioView_Genre_AllGenres"]),
                new AudioGenresFilterItem(_locService["PopularVKAudioView_Genre_Rock"], AudioGenres.Rock),
                new AudioGenresFilterItem(_locService["PopularVKAudioView_Genre_Pop"], AudioGenres.Pop),
                new AudioGenresFilterItem(_locService["PopularVKAudioView_Genre_RapAndHipHop"], AudioGenres.RapAndHipHop),
                new AudioGenresFilterItem(_locService["PopularVKAudioView_Genre_DanceAndHouse"], AudioGenres.DanceAndHouse),
                new AudioGenresFilterItem(_locService["PopularVKAudioView_Genre_Alternative"], AudioGenres.Alternative),
                new AudioGenresFilterItem(_locService["PopularVKAudioView_Genre_Instrumental"], AudioGenres.Instrumental),
                new AudioGenresFilterItem(_locService["PopularVKAudioView_Genre_EasyListening"], AudioGenres.EasyListening),
                new AudioGenresFilterItem(_locService["PopularVKAudioView_Genre_Metal"], AudioGenres.Metal),
                new AudioGenresFilterItem(_locService["PopularVKAudioView_Genre_Dubstep"], AudioGenres.Dubstep),
                new AudioGenresFilterItem(_locService["PopularVKAudioView_Genre_IndiePop"], AudioGenres.IndiePop),
                new AudioGenresFilterItem(_locService["PopularVKAudioView_Genre_JazzAndBlues"], AudioGenres.JazzAndBlues),
                new AudioGenresFilterItem(_locService["PopularVKAudioView_Genre_DrumAndBass"], AudioGenres.DrumAndBass),
                new AudioGenresFilterItem(_locService["PopularVKAudioView_Genre_Trance"], AudioGenres.Trance),
                new AudioGenresFilterItem(_locService["PopularVKAudioView_Genre_Ethnic"], AudioGenres.Ethnic),
                new AudioGenresFilterItem(_locService["PopularVKAudioView_Genre_AcousticAndVocal"], AudioGenres.AcousticAndVocal),
                new AudioGenresFilterItem(_locService["PopularVKAudioView_Genre_Reggae"], AudioGenres.Reggae),
                new AudioGenresFilterItem(_locService["PopularVKAudioView_Genre_Classical"], AudioGenres.Classical),
                new AudioGenresFilterItem(_locService["PopularVKAudioView_Genre_ElectropopAndDisco"], AudioGenres.ElectropopAndDisco),
            };
        }

        private async Task<IEnumerable<Audio>> LoadMoreAudios(uint page)
        {
            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Audio.GetPopular(
                OnlyEng, Genres[SelectedGenresItem].Genre, 50, _audiosOffset));

            if (response.IsError)
                throw new Exception(response.Error.ToString());
            else
            {
                if (!Audios.Any() && response.Data.Any())
                    SetDefaultMode();

                _audiosOffset += 50;
                return response.Data;
            }
        }

        private int _audiosOffset;
        private bool _isFilterChanged;

        private readonly ISettingsService _settingsService;

        private const string SELECTED_GENRES_ITEM_PARAMETER_NAME = "PopularVKGenresItem";
        private const string ONLY_ENG_PARAMETER_NAME = "PopularVKOnlyEng";

        public sealed class AudioGenresFilterItem
        {
            public AudioGenresFilterItem(string title, AudioGenres genre = 0)
            {
                Title = title;
                Genre = genre;
            }

            public string Title { get; }
            public AudioGenres Genre { get; }
        }
    }
}
