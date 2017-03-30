#if WINDOWS_UWP
using Prism.Commands;
using Windows.UI.Xaml.Controls;
#else
using Microsoft.Practices.Prism.StoreApps;
using Windows.UI.Xaml.Controls;
#endif

using PropertyChanged;
using System.Collections;
using System.Collections.Generic;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.ViewModels.Common.Navigation;
using NavigatedToEventArgs = VKSaver.Core.ViewModels.Common.Navigation.NavigatedToEventArgs;
using NavigatingFromEventArgs = VKSaver.Core.ViewModels.Common.Navigation.NavigatingFromEventArgs;

namespace VKSaver.Core.ViewModels
{
    [ImplementPropertyChanged]
    public abstract class SelectionViewModel : WithAppBarViewModel
    {
        protected SelectionViewModel(ILocService locService)
        {
            _locService = locService;

            IsItemClickEnabled = true;
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
        public List<object> SelectedItems { get; private set; }

        [DoNotNotify]
        public DelegateCommand SelectionChangedCommand { get; private set; }

        [DoNotNotify]
        public DelegateCommand ActivateSelectionMode { get; private set; }

        [DoNotNotify]
        public DelegateCommand ReloadContentCommand { get; private set; }

        [DoNotNotify]
        protected bool IsReloadButtonSupported { get; set; }

        public override void AppOnNavigatedTo(NavigatedToEventArgs e, Dictionary<string, object> viewModelState)
        {
            var selectionList = GetSelectionList();
            if (selectionList != null && selectionList.Count > 0)
                SetDefaultMode();

            base.AppOnNavigatedTo(e, viewModelState);
        }

        public override void AppOnNavigatingFrom(NavigatingFromEventArgs e, Dictionary<string, object> viewModelState, bool suspending)
        {
            if (e.NavigationMode == NavigationMode.Back && IsSelectionMode)
            {
                SetDefaultMode();
                e.Cancel = true;
                return;
            }

            base.AppOnNavigatingFrom(e, viewModelState, suspending);
        }

        protected abstract IList GetSelectionList();

        protected override void CreateDefaultAppBarButtons()
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

            base.CreateDefaultAppBarButtons();
        }

        protected override void CreateSelectionAppBarButtons()
        {            
            AppBarItems.Add(new AppBarButton
            {
                Label = _locService["AppBarButton_SelectAll_Text"],
                Icon = new FontIcon { Glyph = "\uE0E7" },
                Command = new DelegateCommand(() => SelectAll = !SelectAll)
            });

            base.CreateSelectionAppBarButtons();
        }

        protected virtual void OnSelectionChangedCommand() { }

        protected virtual void OnReloadContentCommand() { }

        protected override void SetSelectionMode()
        {
            IsSelectionMode = true;
            IsItemClickEnabled = false;

            base.SetSelectionMode();
        }

        protected override void SetDefaultMode()
        {
            IsSelectionMode = false;
            IsItemClickEnabled = true;

            base.SetDefaultMode();
        }

        protected override void HideCommandBar()
        {
            IsSelectionMode = false;
            IsItemClickEnabled = true;

            base.HideCommandBar();
        }

        protected bool HasSelectedItems()
        {
            return SelectedItems.Count > 0;
        }

        protected readonly ILocService _locService;
    }
}
