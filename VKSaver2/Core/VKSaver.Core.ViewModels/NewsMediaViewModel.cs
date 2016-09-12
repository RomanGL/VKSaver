using Microsoft.Practices.Prism.StoreApps;
using ModernDev.InTouch;
using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels.Collections;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class NewsMediaViewModel : ViewModelBase
    {
        public NewsMediaViewModel(InTouch inTouch, IInTouchWrapper inTouchWrapper)
        {
            _inTouch = inTouch;
            _inTouchWrapper = inTouchWrapper;
        }

        [DoNotNotify]
        public PaginatedCollection<NewsMediaItem> MediaItems { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            if (viewModelState.Count > 0)
            {
                _startFrom = (string)viewModelState[nameof(_startFrom)];
                MediaItems = JsonConvert.DeserializeObject<PaginatedCollection<NewsMediaItem>>(
                    viewModelState[nameof(MediaItems)].ToString());
                MediaItems.LoadMoreItems = LoadMoreMediaItems;
            }
            else
            {
                MediaItems = new PaginatedCollection<NewsMediaItem>(LoadMoreMediaItems);
            }

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                viewModelState[nameof(MediaItems)] = JsonConvert.SerializeObject(MediaItems);
                viewModelState[nameof(_startFrom)] = _startFrom;
            }

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        private async Task<IEnumerable<NewsMediaItem>> LoadMoreMediaItems(uint page)
        {
            var parameters = new NewsfeedGetParams();
            parameters.Count = 50;
            parameters.Filters = new List<NewsfeedFilters>
            {
                NewsfeedFilters.Audio
            };

            return null;
        }

        private string _startFrom;
          
        private readonly InTouch _inTouch;
        private readonly IInTouchWrapper _inTouchWrapper;
    }
}
