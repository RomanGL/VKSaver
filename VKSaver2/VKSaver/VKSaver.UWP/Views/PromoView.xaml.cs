using Windows.UI.Xaml;
using Prism.Windows.Mvvm;

namespace VKSaver.Views
{
    public sealed partial class PromoView : SessionStateAwarePage
    {
        public PromoView()
        {
            this.InitializeComponent();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (rootPivot.SelectedIndex + 1 == rootPivot.Items.Count)
                rootPivot.SelectedIndex = 0;
            else
                rootPivot.SelectedIndex++;
        }
    }
}
