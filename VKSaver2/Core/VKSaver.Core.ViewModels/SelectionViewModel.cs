using Microsoft.Practices.Prism.StoreApps;
using PropertyChanged;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using VKSaver.Core.Services.Interfaces;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public abstract class SelectionViewModel : ViewModelBase
    {
        protected SelectionViewModel(ILocService locService)
        {
            _locService = locService;

            IsItemClickEnabled = true;
            AppBarItems = new ObservableCollection<ICommandBarElement>();
            SecondaryItems = new ObservableCollection<ICommandBarElement>();
            SelectedItems = new List<object>();

            SelectionChangedCommand = new DelegateCommand(OnSelectionChangedCommand);
            ActivateSelectionMode = new DelegateCommand(SetSelectionMode);
            ReloadContentCommand = new DelegateCommand(OnReloadContentCommand);
        }

        public bool IsSelectionMode { get; private set; }

        public bool IsItemClickEnabled { get; private set; }

        [DoNotCheckEquality]
        public bool SelectAll { get; private set; }

        [DoNotNotify]
        public ObservableCollection<ICommandBarElement> AppBarItems { get; private set; }

        [DoNotNotify]
        public ObservableCollection<ICommandBarElement> SecondaryItems { get; private set; }

        [DoNotNotify]
        public List<object> SelectedItems { get; private set; }

        [DoNotNotify]
        public DelegateCommand SelectionChangedCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand ActivateSelectionMode { get; private set; }

        [DoNotNotify]
        public DelegateCommand ReloadContentCommand { get; private set; }

        protected bool IsReloadButtonSupported { get; set; }

        public override void OnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            if (GetSelectionList().Count > 0)
                SetDefaultMode();

            base.OnNavigatedTo(e, viewModelState);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (e.NavigationMode == NavigationMode.Back && IsSelectionMode)
            {
                SetDefaultMode();
                e.Cancel = true;
                return;
            }

            base.OnNavigatingFrom(e, viewModelState, suspending);
        }

        protected abstract IList GetSelectionList();

        protected virtual void CreateDefaultAppBarButtons()
        {
            if (IsReloadButtonSupported)
            {
                AppBarItems.Add(new AppBarButton
                {
                    Label = _locService["AppBarButton_Refresh_Text"],
                    Icon = new FontIcon { Glyph = "\uE117", FontSize = 14 },
                    Command = ReloadContentCommand
                });
            }

            AppBarItems.Add(new AppBarButton
            {
                Label = _locService["AppBarButton_Select_Text"],
                Icon = new FontIcon { Glyph = "\uE133", FontSize = 14 },
                Command = ActivateSelectionMode
            });
        }

        protected virtual void CreateSelectionAppBarButtons()
        {            
            AppBarItems.Add(new AppBarButton
            {
                Label = _locService["AppBarButton_SelectAll_Text"],
                Icon = new FontIcon { Glyph = "\uE0E7" },
                Command = new DelegateCommand(() => SelectAll = !SelectAll)
            });
        }

        protected virtual void OnSelectionChangedCommand() { }

        protected virtual void OnReloadContentCommand() { }

        protected void SetSelectionMode()
        {
            IsSelectionMode = true;
            IsItemClickEnabled = false;

            AppBarItems.Clear();
            SecondaryItems.Clear();

            CreateSelectionAppBarButtons();
        }

        protected void SetDefaultMode()
        {
            IsSelectionMode = false;
            IsItemClickEnabled = true;

            AppBarItems.Clear();
            SecondaryItems.Clear();

            CreateDefaultAppBarButtons();
        }

        protected void HideCommandBar()
        {
            IsSelectionMode = false;
            IsItemClickEnabled = true;

            AppBarItems.Clear();
            SecondaryItems.Clear();
        }

        protected bool HasSelectedItems()
        {
            return SelectedItems.Count > 0;
        }

        protected readonly ILocService _locService;
    }
}
