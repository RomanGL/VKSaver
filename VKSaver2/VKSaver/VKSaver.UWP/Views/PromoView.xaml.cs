using Prism.Windows.Mvvm;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Views
{
    public sealed partial class PromoView : SessionStateAwarePage
    {
        public PromoView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            background.Start();
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            background.Stop();
            base.OnNavigatedFrom(e);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (rootFlipView.SelectedIndex + 1 == rootFlipView.Items.Count)
                rootFlipView.SelectedIndex = 0;
            else
                rootFlipView.SelectedIndex++;
        }
    }
}
