using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Prism.StoreApps;
using VKSaver.Core.ViewModels;
using ModernDev.InTouch;

namespace VKSaver.Views
{
    /// <summary>
    /// Представления страницы авторизации.
    /// </summary>
    public sealed partial class LoginView : VisualStateAwarePage
    {
        public LoginView()
        {
            this.InitializeComponent();
        }

        private LoginViewModel ViewModel { get { return DataContext as LoginViewModel; } }

        public DelegateCommand LoginCommand { get; private set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            LoginCommand = new DelegateCommand(
                () => browser.Source = new Uri(ViewModel.AuthUrl),
                () => !_isLoading);
                        
            if (e.NavigationMode == NavigationMode.New)
            {
                VisualStateManager.GoToState(this, "Normal", true);
            }

            base.OnNavigatedTo(e);
        }

        private void browser_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            _isLoading = true;
            _isCompleted = false;
            LoginCommand.RaiseCanExecuteChanged();
            VisualStateManager.GoToState(this, "Loading", true);

            if (args.Uri.AbsoluteUri.Contains("token="))
            {
                _isCompleted = true;
                var parts = args.Uri.Fragment.Substring(1).Split('&').ToArray();
                string token = parts[0].Split('=')[1];
                int userID = int.Parse(parts[2].Split('=')[1]);
                
                ViewModel.LoginToken(userID, token);
            }
            else if (args.Uri.AbsoluteUri.Contains("error="))
            {
                var parts = args.Uri.Fragment.Substring(1).Split('&').ToArray();
                string error = parts[0].Split('=')[1];

                ViewModel.ShowError(error);
                _isCompleted = true;
                VisualStateManager.GoToState(this, "Normal", true);
            }
        }

        private void browser_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (_isCompleted)
            {
                _isLoading = false;
                LoginCommand.RaiseCanExecuteChanged();
                VisualStateManager.GoToState(this, "Normal", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "Auth", true);
            }
        }

        private void browser_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            ViewModel.ShowError("connection_error");
            _isCompleted = true;
            _isLoading = false;
            LoginCommand.RaiseCanExecuteChanged();
            VisualStateManager.GoToState(this, "Normal", true);
        }

        private bool _isCompleted;
        private bool _isLoading;
    }
}
