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
using System.Linq;
using System.Threading.Tasks;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels.Collections;
using VKSaver.Core.Toolkit;
using VKSaver.Core.Toolkit.Navigation;
using NavigatedToEventArgs = VKSaver.Core.Toolkit.Navigation.NavigatedToEventArgs;
using NavigatingFromEventArgs = VKSaver.Core.Toolkit.Navigation.NavigatingFromEventArgs;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class UserCommViewModel : VKSaverViewModel
    {
        public UserCommViewModel(InTouch inTouch, INavigationService navigationService,
            IInTouchWrapper inTouchWrapper)
        {
            _inTouch = inTouch;
            _navigationService = navigationService;
            _inTouchWrapper = inTouchWrapper;

            ExecuteItemCommand = new DelegateCommand<object>(OnExecuteItemCommand);
            NotImplementedCommand = new DelegateCommand(() => _navigationService.Navigate("AccessDeniedView", null));
        }

        public string PageTitle { get; private set; }

        public PaginatedCollection<User> Friends { get; private set; }

        public SimpleStateSupportCollection<FriendsList> FriendsLists { get; private set; }

        public PaginatedCollection<Group> Groups { get; private set; }

        public int LastPivotIndex { get; set; }

        [DoNotNotify]
        public DelegateCommand NotImplementedCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand<object> ExecuteItemCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            var parameter = JsonConvert.DeserializeObject<KeyValuePair<string, int>>(e.Parameter.ToString());
            _userID = parameter.Value;

            if (e.NavigationMode == NavigationMode.New)
            {
                switch (parameter.Key)
                {
                    case "friends":
                        LastPivotIndex = 0;
                        break;
                    //case "fr_lists":
                    //    LastPivotIndex = 1;
                    //    break;
                    case "groups":
                        LastPivotIndex = 1;
                        break;
                    default:
                        LastPivotIndex = 0;
                        break;
                }
            }

            if (viewModelState.Count > 0)
            {
                Friends = JsonConvert.DeserializeObject<PaginatedCollection<User>>(
                    viewModelState[nameof(Friends)].ToString());
                FriendsLists = JsonConvert.DeserializeObject<SimpleStateSupportCollection<FriendsList>>(
                    viewModelState[nameof(FriendsLists)].ToString());
                Groups = JsonConvert.DeserializeObject<PaginatedCollection<Group>>(
                    viewModelState[nameof(Groups)].ToString());

                LastPivotIndex = (int)viewModelState[nameof(LastPivotIndex)];
                _friendsOffset = (int)viewModelState[nameof(_friendsOffset)];
                _groupsOffset = (int)viewModelState[nameof(_groupsOffset)];

                Friends.LoadMoreItems = LoadMoreFriends;
                FriendsLists.LoadItems = LoadLists;
                Groups.LoadMoreItems = LoadMoreGroups;
            }
            else
            {
                Friends = new PaginatedCollection<User>(LoadMoreFriends);
                FriendsLists = new SimpleStateSupportCollection<FriendsList>(LoadLists);
                Groups = new PaginatedCollection<Group>(LoadMoreGroups);

                _friendsOffset = 0;
                _groupsOffset = 0;

                PageTitle = "ВКачай";
                LoadUserInfo(_userID);
            }

            FriendsLists.Load();
            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                viewModelState[nameof(Friends)] = JsonConvert.SerializeObject(Friends.ToList());
                viewModelState[nameof(FriendsLists)] = JsonConvert.SerializeObject(FriendsLists.ToList());
                viewModelState[nameof(Groups)] = JsonConvert.SerializeObject(Groups.ToList());
                viewModelState[nameof(LastPivotIndex)] = LastPivotIndex;
                viewModelState[nameof(_friendsOffset)] = _friendsOffset;
                viewModelState[nameof(_groupsOffset)] = _groupsOffset;
            }

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private async Task<IEnumerable<User>> LoadMoreFriends(uint page)
        {
            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Friends.Get(
                _userID,
                count: 50, offset: _friendsOffset,
                fields: new List<UserProfileFields> { UserProfileFields.Photo100 }));

            if (response.IsError)
                throw new Exception(response.Error.ToString());

            _friendsOffset += 50;
            return response.Data.Items;
        }

        private async Task<IEnumerable<FriendsList>> LoadLists()
        {
            if (FriendsLists.Any())
                return new List<FriendsList>(0);

            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Friends.GetLists(_userID, true));
            if (response.IsError)
                throw new Exception(response.Error.ToString());

            return response.Data.Items;
        }

        private async Task<IEnumerable<Group>> LoadMoreGroups(uint page)
        {
            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Groups.Get(
                new GroupsGetParams
                {
                    UserId = _userID > 0 ? (int?)_userID : null,
                    Count = 50,
                    Offset = _groupsOffset
                }));

            if (response.IsError)
                throw new Exception(response.Error.ToString());

            _groupsOffset += 50;
            return response.Data.Items;
        }

        private async void LoadUserInfo(long userID)
        {
            try
            {
                var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Users.Get(
                    _userID == 0 ? null : new List<object> { _userID }));

                if (!response.IsError && response.Data.Any() && _userID == userID)
                {
                    var user = response.Data[0];
                    PageTitle = $"{user.FirstName} {user.LastName}";
                }
            }
            catch (Exception) { }
        }

        private void OnExecuteItemCommand(object item)
        {
            if (item is User)
            {
                var user = (User)item;
                string parameter = JsonConvert.SerializeObject(new KeyValuePair<string, int>(
                    "audios", user.Id));
                _navigationService.Navigate("UserContentView", parameter);
            }
            else if (item is Group)
            {
                var group = (Group)item;
                string parameter = JsonConvert.SerializeObject(new KeyValuePair<string, int>(
                    "audios", -group.Id));
                _navigationService.Navigate("UserContentView", parameter);
            }
            else if (item is FriendsList)
            {

            }
        }

        private int _userID;
        private int _friendsOffset;
        private int _groupsOffset;

        private readonly INavigationService _navigationService;
        private readonly InTouch _inTouch;
        private readonly IInTouchWrapper _inTouchWrapper;
    }
}
