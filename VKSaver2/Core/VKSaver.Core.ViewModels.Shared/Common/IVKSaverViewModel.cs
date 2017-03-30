using System.Collections.Generic;
using VKSaver.Core.ViewModels.Common.Navigation;

namespace VKSaver.Core.ViewModels.Common
{
    public interface IVKSaverViewModel
    {
        void AppOnNavigatedTo(NavigatedToEventArgs e, 
            Dictionary<string, object> viewModelState);

        void AppOnNavigatingFrom(Navigation.NavigatingFromEventArgs e, 
            Dictionary<string, object> viewModelState, bool suspending);
    }
}
