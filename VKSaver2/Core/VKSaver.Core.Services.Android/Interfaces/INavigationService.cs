namespace VKSaver.Core.Services.Interfaces
{
    public interface INavigationService
    {
        bool CanGoBack();
        bool CanGoForward();
        void ClearHistory();
        void GoBack();
        void GoForward();
        bool Navigate(string pageToken, object parameter);
        void RemoveAllPages(string pageToken = null, object parameter = null);
        void RemoveFirstPage(string pageToken = null, object parameter = null);
        void RemoveLastPage(string pageToken = null, object parameter = null);
    }
}