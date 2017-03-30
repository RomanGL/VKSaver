using System.Collections.Generic;
using GalaSoft.MvvmLight;
using VKSaver.Core.ViewModels.Common.Navigation;

namespace VKSaver.Core.ViewModels.Common
{
    public abstract class VKSaverViewModel : ViewModelBase, IVKSaverViewModel
    {
        public virtual void AppOnNavigatedTo(NavigatedToEventArgs e, 
            Dictionary<string, object> viewModelState)
        {
        }

        public virtual void AppOnNavigatingFrom(NavigatingFromEventArgs e, 
            Dictionary<string, object> viewModelState, bool suspending)
        {
        }
    }
}