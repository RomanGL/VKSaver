using Prism.Mvvm;
using PropertyChanged;

namespace VKSaver.Controls
{
    [ImplementPropertyChanged]
    public class ShellNavigationItem : BindableBase
    {
        public string Icon { get; set; }
        public string Name { get; set; }
        public bool IsCurrent { get; set; }

        [DoNotNotify]
        public string DestinationView { get; set; }
        [DoNotNotify]
        public object NavigationParameter { get; set; }
    }
}
