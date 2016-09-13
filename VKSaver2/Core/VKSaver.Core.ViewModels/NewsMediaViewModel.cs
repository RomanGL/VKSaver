using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using ModernDev.InTouch;
using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels.Collections;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class NewsMediaViewModel : AudioViewModelBase
    {
        public NewsMediaViewModel(InTouch inTouch, INavigationService navigationService,
            IPlayerService playerService, IDownloadsServiceHelper downloadsServiceHelper,
            IAppLoaderService appLoaderService, IDialogsService dialogsService,
            ILocService locService, IInTouchWrapper inTouchWraper)
            : base(inTouch, navigationService, playerService, downloadsServiceHelper,
                 appLoaderService, dialogsService, locService, inTouchWraper)
        { }

        [DoNotNotify]
        public PaginatedCollection<Audio> MediaItems { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            if (viewModelState.Count > 0)
            {
                _startFrom = (string)viewModelState[nameof(_startFrom)];
                MediaItems = JsonConvert.DeserializeObject<PaginatedCollection<Audio>>(
                    viewModelState[nameof(MediaItems)].ToString());
                MediaItems.LoadMoreItems = LoadMoreMediaItems;
            }
            else
            {
                MediaItems = new PaginatedCollection<Audio>(LoadMoreMediaItems);
            }

            base.OnNavigatedTo(e, viewModelState);
        }

        protected override IList<Audio> GetAudiosList()
        {
            return MediaItems;
        }

        protected override void OnReloadContentCommand()
        {
            MediaItems?.Clear();
            _startFrom = null;
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {            
            if (!IsSelectionMode && e.NavigationMode == NavigationMode.New)
            {
                viewModelState[nameof(MediaItems)] = JsonConvert.SerializeObject(MediaItems);
                viewModelState[nameof(_startFrom)] = _startFrom;
            }

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private async Task<IEnumerable<Audio>> LoadMoreMediaItems(uint page)
        {
            if (page > 0)
                return new List<Audio>(0);

            var parameters = new NewsfeedGetParams();
            parameters.StartFrom = _startFrom;
            parameters.Filters = new List<NewsfeedFilters>(1)
            {
                NewsfeedFilters.Audio
            };
            
            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Newsfeed.Get(parameters));

            if (response.IsError)
                throw new Exception(response.Error.ToString());
            else
            {
                if (!MediaItems.Any() && response.Data.Items.Any())
                    SetDefaultMode();

                _startFrom = response.Data.NextFrom;
                var mediaItems = new List<Audio>();

                foreach (var post in response.Data.Items)
                {
                    if (post.Attachments == null)
                        continue;

                    foreach (var attachment in post.Attachments)
                    {
                        var audio = attachment as Audio;
                        if (audio != null)
                            mediaItems.Add(audio);
                    }
                }

                return mediaItems;
            }
        }

        private string _startFrom;
    }
}
