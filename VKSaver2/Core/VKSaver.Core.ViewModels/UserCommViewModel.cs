using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using Newtonsoft.Json;
using OneTeam.SDK.Core;
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

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class UserCommViewModel : ViewModelBase
    {
        public UserCommViewModel(IVKService vkService, INavigationService navigationService)
        {
            _vkService = vkService;
            _navigationService = navigationService;

            NotImplementedCommand = new DelegateCommand(() => _navigationService.Navigate("AccessDeniedView", null));
        }

        public PaginatedCollection<VKUser> Friends { get; private set; }

        public PaginatedCollection<VKUserList> FriendsLists { get; private set; }

        public PaginatedCollection<VKGroup> Groups { get; private set; }

        public int LastPivotIndex { get; set; }

        [DoNotNotify]
        public DelegateCommand NotImplementedCommand { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            var parameter = JsonConvert.DeserializeObject<KeyValuePair<string, long>>(e.Parameter.ToString());
            _userID = parameter.Value;

            if (e.NavigationMode == NavigationMode.New)
            {
                switch (parameter.Key)
                {
                    case "friends":
                        LastPivotIndex = 0;
                        break;
                    case "fr_lists":
                        LastPivotIndex = 1;
                        break;
                    case "groups":
                        LastPivotIndex = 2;
                        break;
                    default:
                        LastPivotIndex = 0;
                        break;
                }
            }

            if (viewModelState.Count > 0)
            {

            }
            else
            {
                Friends = new PaginatedCollection<VKUser>(LoadMoreFriends);
                FriendsLists = new PaginatedCollection<VKUserList>(LoadMoreLists);
                Groups = new PaginatedCollection<VKGroup>(LoadMoreGroups);

                _friendsOffset = 0;
                _groupsOffset = 0;
            }

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private async Task<IEnumerable<VKUser>> LoadMoreFriends(uint page)
        {
            var parameters = new Dictionary<string, string>
            {
                { "user_id", _userID.ToString() },
                { "order", "hints" },
                { "fields", "photo_100" },
                { "count", "50" },
                { "offset", _friendsOffset.ToString() }
            };

            var request = new Request<VKCountedItemsObject<VKUser>>("friends.get", parameters);
            var response = await _vkService.ExecuteRequestAsync(request);

            if (response.IsSuccess)
            {
                _friendsOffset += 50;
                return response.Response.Items;
            }
            else
                throw new Exception();
        }

        private async Task<IEnumerable<VKUserList>> LoadMoreLists(uint page)
        {
            if (FriendsLists.Count > 0)
                return new List<VKUserList>();

            var parameters = new Dictionary<string, string>();
            parameters["return_system"] = "1";

            if (_userID > 0)
                parameters["user_id"] = _userID.ToString();

            var request = new Request<VKCountedItemsObject<VKUserList>>("friends.getLists", parameters);
            var response = await _vkService.ExecuteRequestAsync(request);
            
            if (response.IsSuccess)
                return response.Response.Items;
            else
                throw new Exception(response.Error.ToString());
        }

        private async Task<IEnumerable<VKGroup>> LoadMoreGroups(uint page)
        {
            var parameters = new Dictionary<string, string>
            {
                { "count", "50" },
                { "offset", _groupsOffset.ToString() },
                { "extended", "1" }
            };

            if (_userID > 0)
                parameters["user_id"] = _userID.ToString();

            var request = new Request<VKCountedItemsObject<VKGroup>>("groups.get", parameters);
            var response = await _vkService.ExecuteRequestAsync(request);

            if (response.IsSuccess)
            {
                _groupsOffset += 50;
                return response.Response.Items;
            }
            else
                throw new Exception(response.Error.ToString());
        }

        private long _userID;
        private uint _friendsOffset;
        private uint _groupsOffset;

        private readonly IVKService _vkService;
        private readonly INavigationService _navigationService;
    }
}
