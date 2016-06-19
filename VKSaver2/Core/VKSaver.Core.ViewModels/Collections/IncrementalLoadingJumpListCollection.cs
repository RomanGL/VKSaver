using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using VKSaver.Core.Models.Common;

namespace VKSaver.Core.ViewModels.Collections
{
    public class IncrementalLoadingJumpListCollection : StateSupportCollection<PaginatedJumpListGroup<object>>, 
        ISupportUpDownIncrementalLoading
    {
        public IncrementalLoadingJumpListCollection()
            : base()
        {
            HasMoreDownItems = true;
        }

        public bool HasMoreDownItems { get; set; }

        public bool HasMoreUpItems { get { return false; } }

        public override async void Load()
        {
            await LoadMore();
        }

        public override void Refresh()
        {
            for (int i = 0; i < this.Count; i++)
            {
                this[i].Refresh();
            }

            Load();
        }

        private async Task<LoadMoreItemsResult> LoadMore()
        {
            try
            {
                HasMoreDownItems = false;
                ContentState = ContentState.Loading;

                uint tempCount = 0;
                bool tempHasMore = false;
                ContentState tempState = ContentState.None;

                foreach (var group in this)
                {
                    if (group.HasMoreItems || group.ContentState == ContentState.Error)
                    {
                        var result = await group.LoadMore();
                        tempCount += result.Count;

                        if (group.HasMoreItems)
                            tempHasMore = true;

                        if ((byte)tempState < (int)group.ContentState)
                            tempState = group.ContentState;
                    }

                    if (tempState == ContentState.Error)
                    {
                        HasMoreDownItems = false;
                        ContentState = ContentState.Error;
                        return new LoadMoreItemsResult() { Count = tempCount };
                    }
                    else if (tempCount >= 50)
                    {
                        HasMoreDownItems = tempHasMore;
                        ContentState = ContentState.Normal;
                        return new LoadMoreItemsResult() { Count = tempCount };
                    }
                }

                ContentState = tempState;
                HasMoreDownItems = tempHasMore;
                return new LoadMoreItemsResult() { Count = tempCount };
            }
            catch (Exception)
            {
                HasMoreDownItems = false;
                ContentState = ContentState.Error;
                return new LoadMoreItemsResult() { Count = 0 };
            }
        }

        public Task<object> LoadUpAsync(uint count)
        {
            throw new NotImplementedException();
        }

        public async Task<object> LoadDownAsync(uint count)
        {
            await LoadMore();
            return null;
        }
    }
}
