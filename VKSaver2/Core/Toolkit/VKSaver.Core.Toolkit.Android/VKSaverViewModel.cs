using System.Collections.Generic;
using GalaSoft.MvvmLight;
using VKSaver.Core.Toolkit.Navigation;

namespace VKSaver.Core.Toolkit
{
    public abstract class VKSaverViewModel : ViewModelBase, IVKSaverViewModel
    {
        public virtual void OnNavigatedTo(NavigatedToEventArgs e, 
            Dictionary<string, object> viewModelState)
        {
        }

        public virtual void OnNavigatingFrom(NavigatingFromEventArgs e, 
            Dictionary<string, object> viewModelState, bool suspending)
        {
        }
    }
}