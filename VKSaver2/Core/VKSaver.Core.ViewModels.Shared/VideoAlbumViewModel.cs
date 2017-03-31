#if WINDOWS_UWP
using Prism.Windows.Mvvm;
using Prism.Commands;
using Prism.Windows.Navigation;
#elif WINDOWS_PHONE_APP
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
#elif ANDROID
using VKSaver.Core.Toolkit.Commands;
#endif

using ModernDev.InTouch;
using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels.Collections;
using VKSaver.Core.Toolkit;
using VKSaver.Core.Toolkit.Controls;
using VKSaver.Core.Toolkit.Navigation;
using NavigatedToEventArgs = VKSaver.Core.Toolkit.Navigation.NavigatedToEventArgs;
using NavigatingFromEventArgs = VKSaver.Core.Toolkit.Navigation.NavigatingFromEventArgs;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class VideoAlbumViewModel : VKSaverViewModel
    {
        public VideoAlbumViewModel(INavigationService navigationService, InTouch inTouch,
            IAppLoaderService appLoaderService, IDialogsService dialogsService, ILocService locService,
            IInTouchWrapper inTouchWrapper)
        {
            _navigationService = navigationService;
            _inTouch = inTouch;
            _dialogsService = dialogsService;
            _locService = locService;
            _appLoaderService = appLoaderService;
            _inTouchWrapper = inTouchWrapper;
            
            PrimaryItems = new ObservableCollection<IButtonElement>();
            SecondaryItems = new ObservableCollection<IButtonElement>();
            
            ReloadContentCommand = new DelegateCommand(OnReloadContentCommand);
            AddToMyVideosCommand = new DelegateCommand<Video>(OnAddToMyVideosCommand, CanAddToMyVideos);
            OpenVideoCommand = new DelegateCommand<Video>(OnOpenVideoCommand);
            OpenTransferManagerCommand = new DelegateCommand(OnOpenTransferManagerCommand);

            DeleteVideoCommand = new DelegateCommand<Video>(OnDeleteVideoCommand, CanDeleteVideo);
        }

        public PaginatedCollection<Video> Videos { get; private set; }

        public bool IsSelectionMode { get; private set; }

        public bool IsItemClickEnabled { get; private set; }

        [DoNotCheckEquality]
        public bool SelectAll { get; private set; }

        [DoNotNotify]
        public ObservableCollection<IButtonElement> PrimaryItems { get; private set; }

        [DoNotNotify]
        public ObservableCollection<IButtonElement> SecondaryItems { get; private set; }
        
        [DoNotNotify]
        public DelegateCommand<Video> OpenVideoCommand { get; private set; }
                                
        [DoNotNotify]
        public DelegateCommand ReloadContentCommand { get; private set; }
                
        [DoNotNotify]
        public DelegateCommand<Video> AddToMyVideosCommand { get; private set; }
        
        [DoNotNotify]
        public DelegateCommand<Video> DeleteVideoCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand OpenTransferManagerCommand { get; private set; }

        public VideoAlbum Album { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            Album = JsonConvert.DeserializeObject<VideoAlbum>(e.Parameter.ToString());

            if (viewModelState.Count > 0)
            {
                Videos = JsonConvert.DeserializeObject<PaginatedCollection<Video>>(
                    viewModelState[nameof(Videos)].ToString());
                _offset = (int)viewModelState[nameof(_offset)];

                Videos.LoadMoreItems = LoadMoreVideos;
            }
            else
            {
                _offset = 0;
                Videos = new PaginatedCollection<Video>(LoadMoreVideos);
            }

            if (Videos.Count > 0)
                SetDefaultMode();

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (e.NavigationMode == NavigationMode.Back && _appLoaderService.IsShowed)
            {
                e.Cancel = true;
                return;
            }

            if (e.NavigationMode == NavigationMode.Back && IsSelectionMode)
            {
                SetDefaultMode();
                e.Cancel = true;
                return;
            }

            if (e.NavigationMode == NavigationMode.New)
            {
                viewModelState[nameof(Videos)] = JsonConvert.SerializeObject(Videos.ToList());
                viewModelState[nameof(_offset)] = _offset;
            }

            PrimaryItems.Clear();
            SecondaryItems.Clear();
            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private async Task<IEnumerable<Video>> LoadMoreVideos(uint page)
        {
            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Videos.Get(
                Album.OwnerId, 
                albumId: (int)Album.Id, 
                count: 50, 
                offset: _offset));

            if (response.IsError)
                throw new Exception(response.Error.ToString());

            if (!Videos.Any() && response.Data.Items.Any())
                SetDefaultMode();

            _offset += 50;
            return response.Data.Items;
        }

        private void CreateDefaultAppBarButtons()
        {
            PrimaryItems.Clear();
            SecondaryItems.Clear();

            PrimaryItems.Add(new ButtonElement
            {
                Label = _locService["AppBarButton_Refresh_Text"],
                Icon = new FontButtonIcon { Glyph = "\uE117", FontSize = 14 },
                Command = ReloadContentCommand
            });

            SecondaryItems.Add(new ButtonElement
            {
                Label = _locService["AppBarButton_TransferManager_Text"],
                Command = OpenTransferManagerCommand
            });
        }

        private void SetDefaultMode()
        {
            CreateDefaultAppBarButtons();
        }

        private void OnOpenVideoCommand(Video video)
        {
            _navigationService.Navigate("VideoInfoView", JsonConvert.SerializeObject(video));
        }

        private void OnOpenTransferManagerCommand()
        {
            _navigationService.Navigate("TransferView", "downloads");
        }

        private void OnReloadContentCommand()
        {
            _offset = 0;
            Videos.Refresh();
        }

        private bool CanAddToMyVideos(Video video)
        {
            return video != null && video.OwnerId != _inTouch.Session.UserId;
        }

        private bool CanDeleteVideo(Video video)
        {
            return video != null && video.OwnerId == _inTouch.Session.UserId;
        }

        private async void OnDeleteVideoCommand(Video video)
        {
            _appLoaderService.Show(String.Format(_locService["AppLoader_DeletingItem"], video.ToString()));
            if (!await DeleteVideo(video))
            {
                _dialogsService.Show(_locService["Message_VideoDeleteError_Text"],
                    _locService["Message_VideoDeleteError_Title"]);
            }
            else
            {
                Videos.Remove(video);
            }

            _appLoaderService.Hide();
        }

        private async void OnAddToMyVideosCommand(Video video)
        {
            _appLoaderService.Show(String.Format(_locService["AppLoader_AddingItem"], video.ToString()));
            if (!await AddToMyVideos(video))
            {
                _dialogsService.Show(_locService["Message_VideoAddError_Text"],
                    _locService["Message_VideoAddError_Title"]);
            }            
            _appLoaderService.Hide();
        }
        
        private async Task<bool> AddToMyVideos(Video video)
        {
            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Videos.Add(
                (int)video.Id, video.OwnerId, _inTouch.Session.UserId));
            return !response.IsError;
        }

        private async Task<bool> DeleteVideo(Video video)
        {
            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Videos.Delete(
                (int)video.Id, video.OwnerId, _inTouch.Session.UserId));
            return !response.IsError;
        }

        private int _offset;

        private readonly INavigationService _navigationService;
        private readonly IAppLoaderService _appLoaderService;
        private readonly IDialogsService _dialogsService;
        private readonly ILocService _locService;
        private readonly InTouch _inTouch;
        private readonly IInTouchWrapper _inTouchWrapper;
    }
}
