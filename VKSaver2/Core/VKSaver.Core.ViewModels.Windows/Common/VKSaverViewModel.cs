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
using VKSaver.Core.ViewModels.Common.Navigation;

namespace VKSaver.Core.ViewModels.Common
{
    public abstract class VKSaverViewModel : ViewModelBase, IVKSaverViewModel
    {
        public virtual void AppOnNavigatedTo(Navigation.NavigatedToEventArgs e,
            Dictionary<string, object> viewModelState)
        {
        }

        public virtual void AppOnNavigatingFrom(Navigation.NavigatingFromEventArgs e,
            Dictionary<string, object> viewModelState, bool suspending)
        {
        }

        public sealed override void OnNavigatedTo(NavigatedToEventArgs e, 
            Dictionary<string, object> viewModelState)
        {
            var args = new Navigation.NavigatedToEventArgs((NavigationMode)(int)e.NavigationMode, e.Parameter);
            AppOnNavigatedTo(args, viewModelState);
            base.OnNavigatedTo(e, viewModelState);
        }

        public sealed override void OnNavigatingFrom(NavigatingFromEventArgs e, 
            Dictionary<string, object> viewModelState, bool suspending)
        {
            var args = new Navigation.NavigatingFromEventArgs((NavigationMode)(int)e.NavigationMode, e.Parameter);
            AppOnNavigatingFrom(args, viewModelState, suspending);

            e.Cancel = args.Cancel;
            base.OnNavigatingFrom(e, viewModelState, suspending);
        }
    }
}
