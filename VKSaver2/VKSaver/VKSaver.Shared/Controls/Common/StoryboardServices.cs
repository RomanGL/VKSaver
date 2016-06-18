using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace VKSaver.Controls.Common
{
    public static class StoryboardServices
    {
        public static DependencyObject GetTarget(Timeline timeline)
        {
            if (timeline == null)
                throw new ArgumentNullException("timeline");

            return timeline.GetValue(TargetProperty) as DependencyObject;
        }

        public static void SetTarget(Timeline timeline, DependencyObject value)
        {
            if (timeline == null)
                throw new ArgumentNullException("timeline");

            timeline.SetValue(TargetProperty, value);
        }

        public static readonly DependencyProperty TargetProperty =
                DependencyProperty.RegisterAttached(
                        "Target",
                        typeof(DependencyObject),
                        typeof(Timeline),
                        new PropertyMetadata(null, OnTargetPropertyChanged));

        private static void OnTargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Storyboard.SetTarget(d as Timeline, e.NewValue as DependencyObject);
        }



        public static Storyboard GetFadeOutStoryboard(DependencyObject obj)
        {
            return (Storyboard)obj.GetValue(FadeOutStoryboardProperty);
        }

        public static void SetFadeOutStoryboard(DependencyObject obj, Storyboard value)
        {
            obj.SetValue(FadeOutStoryboardProperty, value);
        }

        public static readonly DependencyProperty FadeOutStoryboardProperty =
            DependencyProperty.RegisterAttached("FadeOutStoryboard", typeof(Storyboard), typeof(Storyboard), new PropertyMetadata(null));        
    }
}
