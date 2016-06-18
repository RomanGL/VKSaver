using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Practices.Prism.StoreApps;
using System;
using Windows.UI.Xaml;
using VKSaver.Controls;

namespace VKSaver.Views
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class PurchaseView : VisualStateAwarePage
    {
        public PurchaseView()
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
        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (rootPivot.SelectedIndex)
            {
                case 0:
                    background.DefaultTheme = PlayerTheme.Magenta;
                    break;
                case 1:
                    background.DefaultTheme = PlayerTheme.Orange;
                    break;
                case 2:
                    background.DefaultTheme = PlayerTheme.Black;
                    break;
                case 3:
                    background.DefaultTheme = PlayerTheme.Gray;
                    break;
                case 4:
                    background.DefaultTheme = PlayerTheme.Blue;
                    break;
                case 5:
                    background.DefaultTheme = PlayerTheme.Magenta;
                    break;
                default:
                    background.DefaultTheme = PlayerTheme.None;
                    break;
            }
            background.NextTheme();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (rootPivot.SelectedIndex + 1 == rootPivot.Items.Count)
                rootPivot.SelectedIndex = 0;
            else
                rootPivot.SelectedIndex++;
        }

        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            rootPivot.SelectedIndex = rootPivot.Items.Count - 1;
        }
#endif
    }
}
