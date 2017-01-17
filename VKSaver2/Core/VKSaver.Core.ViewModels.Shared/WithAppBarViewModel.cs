using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using Microsoft.Practices.Prism.StoreApps;
using PropertyChanged;

namespace VKSaver.Core.ViewModels
{
    public abstract class WithAppBarViewModel : ViewModelBase
    {
        protected WithAppBarViewModel()
        {
            AppBarItems = new ObservableCollection<ICommandBarElement>();
            SecondaryItems = new ObservableCollection<ICommandBarElement>();
        }

        [DoNotNotify]
        public ObservableCollection<ICommandBarElement> AppBarItems { get; private set; }

        [DoNotNotify]
        public ObservableCollection<ICommandBarElement> SecondaryItems { get; private set; }

        protected virtual void CreateDefaultAppBarButtons()
        {
        }

        protected virtual void CreateSelectionAppBarButtons()
        {
        }

        protected virtual void SetSelectionMode()
        {
            AppBarItems.Clear();
            SecondaryItems.Clear();

            CreateSelectionAppBarButtons();
        }

        protected virtual void SetDefaultMode()
        {
            AppBarItems.Clear();
            SecondaryItems.Clear();

            CreateDefaultAppBarButtons();
        }

        protected virtual void HideCommandBar()
        {
            AppBarItems.Clear();
            SecondaryItems.Clear();
        }
    }
}
