using System;
using VKSaver.Common;
using VKSaver.Core.Models;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace VKSaver.Controls
{
    /// <summary>
    /// Представляет элемент управления для отображения всплывающих сообщений в приложении.
    /// </summary>
    public class AppNotificationControl : Control
    {
        private const double MAX_MANIPULATION_X = 100;

        public AppNotificationControl()
        {
            this.DefaultStyleKey = typeof(AppNotificationControl);
        }

        private FrameworkElement parent;
        private FrameworkElement contentGrid;
        private FrameworkElement rootGrid;
        private FrameworkElement visualBorder;
        private ProgressBar progress;
        private CompositeTransform rootGridTransform;
        private Storyboard visibleStoryboard;
        private Storyboard collapsedStoryboard;
        private Storyboard manipulationResetStoryboard;
        private Storyboard manipulationCompletedStoryboard;
        private DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        private int number;
        private bool isTitleState;
        private bool hasTitle;

        public AppNotification Message
        {
            get { return (AppNotification)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }
        
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(AppNotification), 
                typeof(AppNotificationControl), new PropertyMetadata(default(AppNotification)));

        protected override void OnApplyTemplate()
        {
            if (Message == null)
                throw new ArgumentNullException("Message");

            base.OnApplyTemplate();
            contentGrid = GetTemplateChild("ContentGrid") as FrameworkElement;
            rootGrid = GetTemplateChild("RootGrid") as FrameworkElement;
            visualBorder = GetTemplateChild("VisualBorder") as FrameworkElement;
            visibleStoryboard = GetTemplateChild("VisibleStoryboard") as Storyboard;
            collapsedStoryboard = GetTemplateChild("CollapsedStoryboard") as Storyboard;
            manipulationResetStoryboard = GetTemplateChild("ManipulationResetStoryboard") as Storyboard;
            manipulationCompletedStoryboard = GetTemplateChild("ManipulationCompletedStoryboard") as Storyboard;
            progress = GetTemplateChild("Progress") as ProgressBar;
            progress.Value = Message.ProgressPercent;

            parent = this.Parent as FrameworkElement;
            rootGridTransform = rootGrid.RenderTransform as CompositeTransform;

            hasTitle = !String.IsNullOrWhiteSpace(Message.Title);
            if (hasTitle)
            {
                VisualStateManager.GoToState(this, "TitleState", true);
                isTitleState = true;
            }
            else VisualStateManager.GoToState(this, "ContentState", true);

            VisualStateManager.GoToState(this, Message.Type.ToString(), true);

            Message.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "ProgressPercent")
                    progress.Value = Message.ProgressPercent;
            };
        }

        /// <summary>
        /// Показывает уведомление.
        /// </summary>
        public void Show()
        {
            parent.SizeChanged += Parent_SizeChanged;
            rootGrid.ManipulationDelta += RootGrid_ManipulationDelta;
            rootGrid.ManipulationCompleted += RootGrid_ManipulationCompleted;
            timer.Tick += Timer_Tick;

            rootGrid.Width = contentGrid.DesiredSize.Width;
            ((DoubleAnimation)visibleStoryboard.Children[0]).To = contentGrid.DesiredSize.Width;
            visibleStoryboard.Begin();

            timer.Start();
        }

        private void RootGrid_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (rootGridTransform.TranslateX < MAX_MANIPULATION_X)
                manipulationResetStoryboard.Begin();
            else if (rootGridTransform.TranslateX >= MAX_MANIPULATION_X)
            {
                this.IsHitTestVisible = false;
                parent.SizeChanged -= Parent_SizeChanged;
                this.ManipulationDelta -= RootGrid_ManipulationDelta;
                this.ManipulationCompleted -= RootGrid_ManipulationCompleted;
                timer?.Stop();
                timer = null;

                manipulationCompletedStoryboard.Completed += delegate
                {
                    var panel = this.GetFirstAncestorOfType<Panel>();
                    panel.Children.Remove(this);
                };
                manipulationCompletedStoryboard.Begin();
            }
        }

        private void RootGrid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            double resultTransform = rootGridTransform.TranslateX + e.Delta.Translation.X;
            if (resultTransform > 0)
                rootGridTransform.TranslateX += e.Delta.Translation.X;
            else rootGridTransform.TranslateX = 0;
        }

        /// <summary>
        /// Вызывается при изменении родительского контейнера.
        /// </summary>
        private void Parent_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.rootGrid.Width = double.NaN;
            contentGrid.Measure(new Size(parent.ActualWidth - 40, 60));
            this.visualBorder.Width = contentGrid.DesiredSize.Width;
        }

        /// <summary>
        /// Скрывает уведомление.
        /// </summary>
        public void Hide()
        {
            if (timer == null) return;

            parent.SizeChanged -= Parent_SizeChanged;
            this.ManipulationDelta -= RootGrid_ManipulationDelta;
            this.ManipulationCompleted -= RootGrid_ManipulationCompleted;
            timer.Stop();
            timer = null;
            collapsedStoryboard.Completed += CollapsedStoryboard_Completed;            
            collapsedStoryboard.Begin();
        }

        private void Timer_Tick(object sender, object e)
        {
            number++;

            if (number >= Message.Duration.TotalSeconds)
            {
                Hide();
            }

            if (!hasTitle || number % 3 != 0) return;

            if (isTitleState) VisualStateManager.GoToState(this, "ContentState", true);
            else VisualStateManager.GoToState(this, "TitleState", true);

            isTitleState = !isTitleState;
        }

        private void CollapsedStoryboard_Completed(object sender, object e)
        {
            collapsedStoryboard.Completed -= CollapsedStoryboard_Completed;
            var panel = this.GetFirstAncestorOfType<Panel>();
            panel.Children.Remove(this);
        }
    }
}
