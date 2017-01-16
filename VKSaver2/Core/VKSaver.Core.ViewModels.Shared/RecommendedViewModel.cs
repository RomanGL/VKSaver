#if WINDOWS_UWP
using Prism.Windows.Mvvm;
using Prism.Commands;
using Prism.Windows.Navigation;
#else
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#endif

using ModernDev.InTouch;
using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels.Collections;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class RecommendedViewModel : VKAudioImplementedViewModel
    {
        public RecommendedViewModel(
            InTouch inTouch,
            INavigationService navigationService,
            IPlayerService playerService,
            IDownloadsServiceHelper downloadsServiceHelper,
            IAppLoaderService appLoaderService,
            IDialogsService dialogsService,
            ILocService locService,
            IInTouchWrapper inTouchWrapper)
            : base(inTouch, appLoaderService, dialogsService, inTouchWrapper, downloadsServiceHelper,
                  playerService, locService, navigationService)
        {
            IsReloadButtonSupported = true;
        }

        public PaginatedCollection<Audio> Audios { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            if (e.Parameter != null)
                _userID = long.Parse(e.Parameter.ToString());

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

        protected override void OnReloadContentCommand()
        {
            Audios?.Refresh();
            _audiosOffset = 0;
        }

        private async Task<IEnumerable<Audio>> LoadMoreAudios(uint page)
        {
            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Audio.GetRecommendations(
                userId: _userID > 0 ? (int?)_userID : null,
                count: 50,
                offset: _audiosOffset));

            if (response.IsError)
                throw new Exception(response.Error.ToString());
            else
            {
                if (!Audios.Any() && response.Data.Items.Any())
                    SetDefaultMode();

                _audiosOffset += 50;
                return response.Data.Items;
            }
        }

        private int _audiosOffset;
        private long _userID;
    }
}
