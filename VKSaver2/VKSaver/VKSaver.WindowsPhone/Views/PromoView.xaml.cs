using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Practices.Prism.StoreApps;
using System;
using Windows.UI.Xaml;
using VKSaver.Controls;

namespace VKSaver.Views
{
    public sealed partial class PromoView : VisualStateAwarePage
    {
        public PromoView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
#if WINDOWS_PHONE_APP
            background.Start();
#endif
            base.OnNavigatedTo(e);
        }
        
#if WINDOWS_PHONE_APP
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (rootPivot.SelectedIndex + 1 == rootPivot.Items.Count)
                rootPivot.SelectedIndex = 0;
            else
                rootPivot.SelectedIndex++;
        }
#endif
    }
}
