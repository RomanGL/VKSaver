using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Controls
{
    public sealed partial class Shell : UserControl
    {
        public Shell(Frame frame)
            : this()
        {
            CurrentFrame = frame;
        }

        public Shell()
        {
            this.InitializeComponent();

            MenuButton.Click += (s, e) => ShellSplitView.IsPaneOpen = !ShellSplitView.IsPaneOpen;
            this.SizeChanged += Shell_SizeChanged;
            Window.Current.SetTitleBar(GrabRect);
        }

        public Frame CurrentFrame
        {
            get { return ContentBorder.Child as Frame; }
            set
            {
                var oldFrame = ContentBorder.Child as Frame;
                if (oldFrame != null)
                    oldFrame.Navigated -= CurentFrame_Navigated;

                value.Navigated += CurentFrame_Navigated;
                ContentBorder.Child = value;
            }
        }
        
        #region DependencyProperties
        
        public static string GetTitle(DependencyObject obj)
        {
            return (string)obj.GetValue(TitleProperty);
        }

        public static void SetTitle(DependencyObject obj, string value)
        {
            obj.SetValue(TitleProperty, value);
        }

        public static bool GetIsMenuButtonVisible(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsMenuButtonVisibleProperty);
        }

        public static void SetIsMenuButtonVisible(DependencyObject obj, bool value)
        {
            obj.SetValue(IsMenuButtonVisibleProperty, value);
        }

        public static bool GetIsTitleBarVisible(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsTitleBarVisibleProperty);
        }

        public static void SetIsTitleBarVisible(DependencyObject obj, bool value)
        {
            obj.SetValue(IsTitleBarVisibleProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.RegisterAttached("Title", typeof(string),
                typeof(Shell), new PropertyMetadata(default(string), (x, e) => GetCurrentShell()?.UpdatePageTitle()));

        public static readonly DependencyProperty IsMenuButtonVisibleProperty =
            DependencyProperty.RegisterAttached("IsMenuButtonVisible", typeof(bool),
                typeof(Shell), new PropertyMetadata(true, (s, e) => GetCurrentShell()?.UpdateMenuButton()));

        public static readonly DependencyProperty IsTitleBarVisibleProperty =
            DependencyProperty.RegisterAttached("IsTitleBarVisible", typeof(bool),
                typeof(Shell), new PropertyMetadata(true, (s, e) => GetCurrentShell()?.UpdateTitleBar()));

        #endregion

        private void CurentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            UpdatePageTitle();
            UpdateMenuButton();
            UpdateTitleBar();
            UpdateSplitViewState();
        }

        private void Shell_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSplitViewState();
        }

        private void UpdatePageTitle()
        {
            if (CurrentFrame.Content == null)
                return;
            
            PageTitle.Text = GetTitle(CurrentFrame.Content as DependencyObject) ?? String.Empty;
        }

        private void UpdateMenuButton()
        {
            if (CurrentFrame.Content == null)
                return;

            if (GetIsMenuButtonVisible(CurrentFrame.Content as DependencyObject))
            {
                ShowMenuButtonStoryboard.Begin();
            }
            else
            {
                HideMenuButtonStoryboard.Begin();
            }
        }

        private void UpdateTitleBar()
        {
            if (CurrentFrame.Content == null)
                return;

            if (GetIsTitleBarVisible(CurrentFrame.Content as DependencyObject))
                ShowTitleBarStoryboard.Begin();
            else
                HideTitleBarStoryboard.Begin();
        }

        private void UpdateSplitViewState()
        {
            if (CurrentFrame.Content == null)
                return;

            bool flag = GetIsMenuButtonVisible(CurrentFrame.Content as DependencyObject) &
                GetIsTitleBarVisible(CurrentFrame.Content as DependencyObject);

            if (flag && this.ActualWidth > 840)
            {
                ShellSplitView.IsPaneOpen = true;
                ShellSplitView.DisplayMode = SplitViewDisplayMode.Inline;
            }
            else
            {
                ShellSplitView.IsPaneOpen = false;
                ShellSplitView.DisplayMode = SplitViewDisplayMode.Overlay;
            }
        }

        private static Shell GetCurrentShell()
        {
            return Window.Current.Content as Shell;
        }
    }
}
