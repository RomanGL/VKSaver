using System.Collections.Generic;
using PropertyChanged;
using VKSaver.Core.Toolkit;
using NavigatedToEventArgs = VKSaver.Core.Toolkit.Navigation.NavigatedToEventArgs;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public sealed class ErrorViewModel : VKSaverViewModel
    {
        public string ErrorDetails { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            ErrorDetails = e.Parameter.ToString();
            base.OnNavigatedTo(e, viewModelState);
        }
    }
}
