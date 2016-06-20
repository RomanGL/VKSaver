using System;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Interfaces;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace VKSaver.Controls
{
    public sealed class WithLoaderFrame : Frame, ILoader
    {
        public WithLoaderFrame()
        {
            this.DefaultStyleKey = typeof(WithLoaderFrame);
        }

        public WithLoaderFrame(IAppLoaderService appLoaderService)
        {
            this.DefaultStyleKey = typeof(WithLoaderFrame);
            AppLoaderService = appLoaderService;
        }

        public IAppLoaderService AppLoaderService { get; private set; }

        public string LoaderText
        {
            get { return (string)GetValue(LoaderTextProperty); }
            set { SetValue(LoaderTextProperty, value); }
        }
        
        public static readonly DependencyProperty LoaderTextProperty =
            DependencyProperty.Register("LoaderText", typeof(string), typeof(WithLoaderFrame), new PropertyMetadata(null));

        public void HideLoader()
        {
            VisualStateManager.GoToState(this, "Normal", true);

            var page = Content as Page;
            if (page != null && page.BottomAppBar != null && _appBarHided)
            {
                _appBarHided = false;
                page.BottomAppBar.Visibility = Visibility.Visible;
                page.BottomAppBar.Tag = null;
            }
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            var page = Content as Page;
            if (page != null && page.BottomAppBar != null && _appBarHided)
            {
                _appBarHided = false;
                page.BottomAppBar.Visibility = Visibility.Visible;
                page.BottomAppBar.Tag = null;
            }

            base.OnContentChanged(oldContent, newContent);
        }

        public void ShowLoader(string text)
        {
            LoaderText = text;
            VisualStateManager.GoToState(this, "Showed", true);

            var page = Content as Page;
            if (page != null && page.BottomAppBar != null && page.BottomAppBar.Visibility == Visibility.Visible)
            {
                page.BottomAppBar.Visibility = Visibility.Collapsed;
                page.BottomAppBar.Tag = true;
                _appBarHided = true;
            }
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (ContentTransitions == null)
            {
                ContentTransitions = new TransitionCollection
                {
                    new NavigationThemeTransition()
                };
            }

            this.Loaded += WithLoaderFrame_Loaded;
            this.Unloaded += WithLoaderFrame_Unloaded;

            VisualStateManager.GoToState(this, "Normal", true);
        }

        private void WithLoaderFrame_Loaded(object sender, RoutedEventArgs e)
        {
            AppLoaderService?.AddLoader(this);
        }

        private void WithLoaderFrame_Unloaded(object sender, RoutedEventArgs e)
        {
            AppLoaderService?.RemoveLoader(this);
        }

        private bool _appBarHided;
    }
}
