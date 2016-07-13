namespace VKSaver.Core.ViewModels.Search
{
    internal sealed class SearchNavigationParameter
    {
        public int UserId { get; set; }
        
        public SearchSection Section { get; set; }

        public string Query { get; set; }

        public enum SearchSection
        {
            Ewerywhere = 0,
            InCollection = 1
        }
    }
}
