using System.Collections.Generic;
using VKSaver.Core.Toolkit.Navigation;

namespace VKSaver.Core.Toolkit
{
    public interface IVKSaverViewModel
    {
        void OnNavigatedTo(NavigatedToEventArgs e, 
            Dictionary<string, object> viewModelState);

        void OnNavigatingFrom(NavigatingFromEventArgs e, 
            Dictionary<string, object> viewModelState, bool suspending);
    }
}
