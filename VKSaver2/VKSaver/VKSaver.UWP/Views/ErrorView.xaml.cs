using Prism.Windows.Mvvm;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Navigation;

namespace VKSaver.Views
{
    public partial class ErrorView : SessionStateAwarePage
    {
        public ErrorView()
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
    }
}
