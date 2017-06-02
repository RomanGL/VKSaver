#if WINDOWS_UWP
using Prism.Windows.Mvvm;
using Prism.Commands;
using Prism.Windows.Navigation;
#else
using Microsoft.Practices.Prism.StoreApps;
#endif

using System.Collections.Generic;
using PropertyChanged;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class ErrorViewModel : ViewModelBase
    {
        public string ErrorDetails { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            if (e.Parameter != null)
                ErrorDetails = e.Parameter.ToString();
            base.OnNavigatedTo(e, viewModelState);
        }
    }
}
