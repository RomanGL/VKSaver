#if WINDOWS_UWP || WINDOWS_PHONE_APP
using Windows.UI.Xaml.Controls;
#elif ANDROID
#endif

using System.Collections.ObjectModel;
using PropertyChanged;
using VKSaver.Core.Toolkit;
using VKSaver.Core.Toolkit.Controls;

namespace VKSaver.Core.ViewModels
{
    public abstract class WithAppBarViewModel : VKSaverViewModel
    {
        protected WithAppBarViewModel()
        {
            AppBarItems = new ObservableCollection<IButtonElement>();
            SecondaryItems = new ObservableCollection<IButtonElement>();
        }

        [DoNotNotify]
        public ObservableCollection<IButtonElement> AppBarItems { get; }

        [DoNotNotify]
        public ObservableCollection<IButtonElement> SecondaryItems { get; }

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
