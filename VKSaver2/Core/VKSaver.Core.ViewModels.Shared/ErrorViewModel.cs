#if WINDOWS_UWP
using Prism.Windows.Mvvm;
using Prism.Commands;
using Prism.Windows.Navigation;
#elif WINDOWS_PHONE_APP
using Microsoft.Practices.Prism.StoreApps;
#elif ANDROID
#endif

using System.Collections.Generic;
using PropertyChanged;
using VKSaver.Core.ViewModels.Common;
using NavigatedToEventArgs = VKSaver.Core.ViewModels.Common.Navigation.NavigatedToEventArgs;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class ErrorViewModel : VKSaverViewModel
    {
        public string ErrorDetails { get; private set; }

        public override void AppOnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            ErrorDetails = e.Parameter.ToString();
            base.AppOnNavigatedTo(e, viewModelState);
        }
    }
}
