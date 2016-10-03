namespace VKSaver.Core.Models.Common
{
    public static class ContentStateExtensions
    {
        public static bool IsLoaded(this ContentState state)
        {
            return state == ContentState.Normal || state == ContentState.NoData;
        }

        public static bool IsError(this ContentState state)
        {
            return state == ContentState.Error;
        }

        public static bool IsLoading(this ContentState state)
        {
            return state == ContentState.Loading;
        }
    }
}
