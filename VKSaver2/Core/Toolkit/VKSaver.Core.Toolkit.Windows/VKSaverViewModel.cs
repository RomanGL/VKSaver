#if WINDOWS_UWP
using Prism.Windows.Mvvm;
using NavigatedToEventArgs = Prism.Windows.Navigation.NavigatedToEventArgs;
using NavigatingFromEventArgs = Prism.Windows.Navigation.NavigatingFromEventArgs;
#else
using Microsoft.Practices.Prism.StoreApps;
using NavigatedToEventArgs = Microsoft.Practices.Prism.StoreApps.NavigatedToEventArgs;
using NavigatingFromEventArgs = Microsoft.Practices.Prism.StoreApps.NavigatingFromEventArgs;
#endif

using System.Collections.Generic;
using VKSaver.Core.Toolkit.Navigation;

namespace VKSaver.Core.Toolkit
{
    public abstract class VKSaverViewModel : ViewModelBase, IVKSaverViewModel
    {
        public virtual void OnNavigatedTo(Navigation.NavigatedToEventArgs e,
            Dictionary<string, object> viewModelState)
        {
        }

        public virtual void OnNavigatingFrom(Navigation.NavigatingFromEventArgs e,
            Dictionary<string, object> viewModelState, bool suspending)
        {
        }

        public sealed override void OnNavigatedTo(NavigatedToEventArgs e, 
            Dictionary<string, object> viewModelState)
        {
            var args = new Navigation.NavigatedToEventArgs((NavigationMode)(int)e.NavigationMode, e.Parameter);
            OnNavigatedTo(args, viewModelState);
            base.OnNavigatedTo(e, viewModelState);
        }

        public sealed override void OnNavigatingFrom(NavigatingFromEventArgs e, 
            Dictionary<string, object> viewModelState, bool suspending)
        {
            var args = new Navigation.NavigatingFromEventArgs((NavigationMode)(int)e.NavigationMode, e.Parameter);
            OnNavigatingFrom(args, viewModelState, suspending);

            e.Cancel = args.Cancel;
            base.OnNavigatingFrom(e, viewModelState, suspending);
        }
    }
}
