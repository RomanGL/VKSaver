using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using Newtonsoft.Json;
using OneTeam.SDK.Core;
using OneTeam.SDK.VK.Models.Audio;
using OneTeam.SDK.VK.Models.Common;
using OneTeam.SDK.VK.Models.Groups;
using OneTeam.SDK.VK.Models.Users;
using OneTeam.SDK.VK.Services.Interfaces;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.ViewModels.Collections;
using Windows.UI.Xaml.Navigation;
using OneTeam.SDK.VK.Models.Video;
using OneTeam.SDK.VK.Models.Docs;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels.Common;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class UserContentViewModel : ViewModelBase
    {
        public UserContentViewModel(IVKService vkService, INavigationService navigationService,
            IPlayerService playerService, IDownloadsServiceHelper downloadsServiceHelper)
        {
            _vkService = vkService;
            _navigationService = navigationService;
            _playerService = playerService;
            _downloadsServiceHelper = downloadsServiceHelper;

            ExecuteTracksListItemCommand = new DelegateCommand<object>(OnExecuteTracksListItemCommand);
            NotImplementedCommand = new DelegateCommand(() => _navigationService.Navigate("AccessDeniedView", null));
            DownloadItemCommand = new DelegateCommand<object>(OnDownloadItemCommand, CanExecuteDownloadItemCommand);
        }

        public string PageTitle { get; private set; }

        public IncrementalLoadingJumpListCollection AudioGroup { get; private set; }

        public IncrementalLoadingJumpListCollection VideoGroup { get; private set; }

        public PaginatedCollection<VKDocument> Documents { get; private set; }

        [DoNotNotify]
        public DelegateCommand<object> ExecuteTracksListItemCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<object> DownloadItemCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand NotImplementedCommand { get; private set; }

        public int LastPivotIndex { get; set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            var parameter = JsonConvert.DeserializeObject<KeyValuePair<string, long>>(e.Parameter.ToString());
            _userID = parameter.Value;

            if (e.NavigationMode == NavigationMode.New)
            {
                switch (parameter.Key)
                {
                    case "audios":
                        LastPivotIndex = 0;
                        break;
                    case "videos":
                        LastPivotIndex = 1;
                        break;
                    case "docs":
                        LastPivotIndex = 2;
                        break;
                    default:
                        LastPivotIndex = 0;
                        break;
                }
            }

            if (viewModelState.Count > 0)
            {
                if (e.NavigationMode != NavigationMode.New)
                    LastPivotIndex = (int)viewModelState[nameof(LastPivotIndex)];

                PageTitle = (string)viewModelState[nameof(PageTitle)];

                _audioAlbumsOffset = (uint)viewModelState[nameof(_audioAlbumsOffset)];
                _audiosOffset = (uint)viewModelState[nameof(_audiosOffset)];
                _videoAlbumsOffset = (uint)viewModelState[nameof(_videoAlbumsOffset)];
                _videosOffset = (uint)viewModelState[nameof(_videosOffset)];

                var audioAlbums = JsonConvert.DeserializeObject<List<VKAudioAlbum>>(
                    viewModelState["AudioAlbums"].ToString());
                var audios = JsonConvert.DeserializeObject<List<VKAudio>>(
                    viewModelState["Audios"].ToString());

                AudioGroup = new IncrementalLoadingJumpListCollection();
                AudioGroup.Add(new PaginatedJumpListGroup<object>(audioAlbums, LoadMoreAudioAlbums) { Key = "albums" });
                AudioGroup.Add(new PaginatedJumpListGroup<object>(audios, LoadMoreAudios) { Key = "audios" });

                var videoAlbums = JsonConvert.DeserializeObject<List<VKVideoAlbum>>(
                    viewModelState["VideoAlbums"].ToString());
                var videos = JsonConvert.DeserializeObject<List<VKVideo>>(
                    viewModelState["Videos"].ToString());

                VideoGroup = new IncrementalLoadingJumpListCollection();
                VideoGroup.Add(new PaginatedJumpListGroup<object>(videoAlbums, LoadMoreVideoAlbums) { Key = "albums" });
                VideoGroup.Add(new PaginatedJumpListGroup<object>(videos, LoadMoreVideos) { Key = "videos" });

                Documents = JsonConvert.DeserializeObject<PaginatedCollection<VKDocument>>(
                    viewModelState[nameof(Documents)].ToString());
                Documents.LoadMoreItems = LoadMoreDocuments;
            }
            else
            {
                AudioGroup = new IncrementalLoadingJumpListCollection();
                AudioGroup.Add(new PaginatedJumpListGroup<object>(LoadMoreAudioAlbums) { Key = "albums" });
                AudioGroup.Add(new PaginatedJumpListGroup<object>(LoadMoreAudios) { Key = "audios" });

                VideoGroup = new IncrementalLoadingJumpListCollection();
                VideoGroup.Add(new PaginatedJumpListGroup<object>(LoadMoreVideoAlbums) { Key = "albums" });
                VideoGroup.Add(new PaginatedJumpListGroup<object>(LoadMoreVideos) { Key = "videos" });

                Documents = new PaginatedCollection<VKDocument>(LoadMoreDocuments);

                _audiosOffset = 0;
                _audioAlbumsOffset = 0;
                _videosOffset = 0;
                _videoAlbumsOffset = 0;
                _docsOffset = 0;

                PageTitle = "ВКачай";
                LoadUserInfo(_userID);
            }

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                var audioAlbums = AudioGroup.FirstOrDefault(g => (string)g.Key == "albums");
                var audios = AudioGroup.FirstOrDefault(g => (string)g.Key == "audios");

                if (audios != null)
                    viewModelState["Audios"] = JsonConvert.SerializeObject(audios.ToList());
                if (audioAlbums != null)
                    viewModelState["AudioAlbums"] = JsonConvert.SerializeObject(audioAlbums.ToList());

                var videoAlbums = VideoGroup.FirstOrDefault(g => (string)g.Key == "albums");
                var videos = VideoGroup.FirstOrDefault(g => (string)g.Key == "videos");

                if (videos != null)
                    viewModelState["Videos"] = JsonConvert.SerializeObject(videos.ToList());
                if (videoAlbums != null)
                    viewModelState["VideoAlbums"] = JsonConvert.SerializeObject(videoAlbums.ToList());

                viewModelState[nameof(Documents)] = JsonConvert.SerializeObject(Documents.ToList());
                viewModelState[nameof(LastPivotIndex)] = LastPivotIndex;
                viewModelState[nameof(PageTitle)] = PageTitle;
                viewModelState[nameof(_audiosOffset)] = _audiosOffset;
                viewModelState[nameof(_audioAlbumsOffset)] = _audioAlbumsOffset;
                viewModelState[nameof(_videosOffset)] = _videosOffset;
                viewModelState[nameof(_videoAlbumsOffset)] = _videoAlbumsOffset;
                viewModelState[nameof(_docsOffset)] = _docsOffset;           
            }

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private async Task<IEnumerable<object>> LoadMoreAudios(uint page)
        {
            var parameters = new Dictionary<string, string>
            {
                { "count", "50" },
                { "offset", _audiosOffset.ToString() }
            };

            if (_userID != 0)
                parameters["owner_id"] = _userID.ToString();

            var request = new Request<VKCountedItemsObject<VKAudio>>("audio.get", parameters);
            var response = await _vkService.ExecuteRequestAsync(request);

            if (response.IsSuccess)
            {
                _audiosOffset += 50;
                return response.Response.Items;
            }
            else
                throw new Exception();
        }

        private async Task<IEnumerable<object>> LoadMoreAudioAlbums(uint page)
        {
            var parameters = new Dictionary<string, string>
            {
                { "count", "50" },
                { "offset", _audioAlbumsOffset.ToString() }
            };

            if (_userID != 0)
                parameters["owner_id"] = _userID.ToString();

            var request = new Request<VKCountedItemsObject<VKAudioAlbum>>("audio.getAlbums", parameters);
            var response = await _vkService.ExecuteRequestAsync(request);

            if (response.IsSuccess)
            {
                _audioAlbumsOffset += 50;
                return response.Response.Items;
            }
            else
                throw new Exception();
        }

        private async Task<IEnumerable<object>> LoadMoreVideos(uint page)
        {
            var parameters = new Dictionary<string, string>
            {
                { "count", "50" },
                { "offset", _videosOffset.ToString() }
            };

            if (_userID != 0)
                parameters["owner_id"] = _userID.ToString();

            var request = new Request<VKCountedItemsObject<VKVideo>>("video.get", parameters);
            var response = await _vkService.ExecuteRequestAsync(request);

            if (response.IsSuccess)
            {
                _videosOffset += 50;
                return response.Response.Items;
            }
            else
                throw new Exception();
        }

        private async Task<IEnumerable<object>> LoadMoreVideoAlbums(uint page)
        {
            var parameters = new Dictionary<string, string>
            {
                { "need_system", "1" },
                { "count", "50" },
                { "offset", _videoAlbumsOffset.ToString() }
            };

            if (_userID != 0)
                parameters["owner_id"] = _userID.ToString();

            var request = new Request<VKCountedItemsObject<VKVideoAlbum>>("video.getAlbums", parameters);
            var response = await _vkService.ExecuteRequestAsync(request);

            if (response.IsSuccess)
            {
                _videoAlbumsOffset += 50;
                return response.Response.Items;
            }
            else
                throw new Exception();
        }

        private async Task<IEnumerable<VKDocument>> LoadMoreDocuments(uint page)
        {
            var parameters = new Dictionary<string, string>
            {
                { "count", "50" },
                { "offset", _docsOffset.ToString() }
            };

            if (_userID != 0)
                parameters["owner_id"] = _userID.ToString();

            var request = new Request<VKCountedItemsObject<VKDocument>>("docs.get", parameters);
            var response = await _vkService.ExecuteRequestAsync(request);

            if (response.IsSuccess)
            {
                _docsOffset += 50;
                return response.Response.Items;
            }
            else
                throw new Exception(response.Error.ToString());
        }

        private async void LoadUserInfo(long userID)
        {
            var parameters = new Dictionary<string, string>();
            if (userID > 0)
                parameters["user_id"] = userID.ToString();
            else if (userID < 0)
                parameters["group_ids"] = (-userID).ToString();

            if (userID >= 0)
            {
                var request = new Request<List<VKUser>>("users.get", parameters);
                var response = await _vkService.ExecuteRequestAsync(request);

                if (response.IsSuccess && userID == _userID && response.Response.Any())
                    PageTitle = response.Response[0].Name;
            }
            else
            {
                var request = new Request<List<VKGroup>>("groups.getById", parameters);
                var response = await _vkService.ExecuteRequestAsync(request);

                if (response.IsSuccess && userID == _userID && response.Response.Any())
                    PageTitle = response.Response[0].Name;
            }
        } 

        private async void OnExecuteTracksListItemCommand(object item)
        {
            if (item is VKAudioAlbum)
            {
                _navigationService.Navigate("AudioAlbumView", JsonConvert.SerializeObject(item));
            }
            else if (item is VKAudio)
            {
                var audios = AudioGroup.FirstOrDefault(g => (string)g.Key == "audios");
                if (audios == null)
                    throw new Exception("Не найдена группа аудиозаписей.");

                await _playerService.PlayNewTracks(audios.Cast<VKAudio>().ToPlayerTracks(),
                    audios.IndexOf(item));

                _navigationService.Navigate("PlayerView", null);
            }
        }

        private async void OnDownloadItemCommand(object item)
        {
            if (item is VKAudio)
            {
                var audio = (VKAudio)item;
                await _downloadsServiceHelper.StartDownloadingAsync(audio.ToDownloadable());
            }
            else if (item is VKDocument)
            {
                var doc = (VKDocument)item;
                await _downloadsServiceHelper.StartDownloadingAsync(doc.ToDownloadable());
            }
        }

        private bool CanExecuteDownloadItemCommand(object item)
        {
            return item is VKAudio || item is VKDocument;
        }

        private long _userID;
        private uint _audiosOffset;
        private uint _videosOffset;
        private uint _audioAlbumsOffset;
        private uint _videoAlbumsOffset;
        private uint _docsOffset;

        private readonly IVKService _vkService;
        private readonly INavigationService _navigationService;
        private readonly IPlayerService _playerService;
        private readonly IDownloadsServiceHelper _downloadsServiceHelper;
    }
}
