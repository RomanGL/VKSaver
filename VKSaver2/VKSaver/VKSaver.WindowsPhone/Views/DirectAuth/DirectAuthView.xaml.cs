using Microsoft.Practices.Prism.StoreApps;
using System;
using System.Linq;
using VKSaver.Core.Models.Common;
using VKSaver.Core.ViewModels;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace VKSaver.Views
{
    public partial class DirectAuthView : VisualStateAwarePage, IValidationSupport
    {
        public DirectAuthView()
        {
            this.InitializeComponent();
            LoginCommand = new DelegateCommand(OnLoginCommand, CanExecuteLoginCommand);
            OAuthLoginCommand = new DelegateCommand(() => Frame.Navigate(typeof(LoginView)));
        }

        private DirectAuthViewModel ViewModel { get { return DataContext as DirectAuthViewModel; } }

        public DelegateCommand LoginCommand { get; private set; }

        public DelegateCommand OAuthLoginCommand { get; private set; }

        public void StartValidation(string validationUrl)
        {
            browser.Navigate(new Uri(validationUrl));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.New)
            {
                ClearCookies();
                VisualStateManager.GoToState(this, "Normal", false);
                _isEnterDataState = false;
            }

            userNameBox.TextChanged += TextBox_TextChanged;
            passwordBox.PasswordChanged += TextBox_TextChanged;

            ViewModel.ValidationView = this;
            base.OnNavigatedTo(e);
        }

        private void TextBox_TextChanged(object sender, RoutedEventArgs e)
        {
            LoginCommand.RaiseCanExecuteChanged();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            //if (_isWorking)
            //{
            //    e.Cancel = true;
            //    return;
            //}

            if (!_isCompleted && (e.NavigationMode == NavigationMode.Back || e.NavigationMode == NavigationMode.Refresh))
            {
                if (_isEnterDataState)
                {
                    VisualStateManager.GoToState(this, "Normal", false);
                    _isEnterDataState = false;
                    LoginCommand.RaiseCanExecuteChanged();
                    e.Cancel = true;
                    return;
                }
            }

            userNameBox.TextChanged -= TextBox_TextChanged;
            passwordBox.PasswordChanged -= TextBox_TextChanged;

            base.OnNavigatingFrom(e);
        }

        private async void OnLoginCommand()
        {
            if (_isEnterDataState)
            {
                _isWorking = true;
                await ViewModel.Login();
                _isWorking = false;

                if (ViewModel.IsAuthorized)
                {
                    _isCompleted = true;
                }

                LoginCommand.RaiseCanExecuteChanged();
            }
            else
            {
                _isEnterDataState = true;
                VisualStateManager.GoToState(this, "EnterData", true);

                LoginCommand.RaiseCanExecuteChanged();
            }
        }

        private bool CanExecuteLoginCommand()
        {
            if (!_isEnterDataState)
                return true;

            if (_isWorking)
                return false;

            var vm = ViewModel;
            if (vm == null)
                return false;

            return !String.IsNullOrWhiteSpace(vm.LoginText) && !String.IsNullOrEmpty(vm.PasswordText);
        }

        private void userNameBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
                passwordBox.Focus(FocusState.Programmatic);
        }

        private void passwordBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter && LoginCommand.CanExecute())
            {
                LoginCommand.Execute();
                InputPane.GetForCurrentView().TryHide();
            }
        }

        private async void browser_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            _isWorking = true;
            _isCompleted = false;
            LoginCommand.RaiseCanExecuteChanged();
            VisualStateManager.GoToState(this, "Loading", true);

            if (args.Uri.AbsoluteUri.Contains("token="))
            {
                _isCompleted = true;
                var parts = args.Uri.Fragment.Substring(1).Split('&').ToArray();
                string token = parts[1].Split('=')[1];  // В отличие от LoginView, здесь первым имеется либо success=1 либо fail=1.
                int userID = int.Parse(parts[2].Split('=')[1]);

                ViewModel.LoginToken(userID, token);
            }
            else if (args.Uri.AbsoluteUri.Contains("error="))
            {
                var parts = args.Uri.Fragment.Substring(1).Split('&').ToArray();
                string error = parts[0].Split('=')[1];

                DirectAuthErrors errorName = DirectAuthErrors.unknown_error;
                Enum.TryParse(error, out errorName);

                ViewModel.ProcessError(errorName);
                _isCompleted = true;
                VisualStateManager.GoToState(this, "Normal", true);
            }
            else if (args.Uri.AbsoluteUri.EndsWith("blank.html?success=1"))
            {
                _isCompleted = true;
                await ViewModel.Login();
            }
            else if (args.Uri.AbsoluteUri.EndsWith("blank.html#fail=1"))
            {
                _isCompleted = true;
            }
        }

        private void browser_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (_isCompleted)
            {
                _isWorking = false;
                LoginCommand.RaiseCanExecuteChanged();
                VisualStateManager.GoToState(this, "Normal", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "Validation", true);
            }
        }

        private void browser_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            ViewModel.ProcessError(DirectAuthErrors.connection_error, e.WebErrorStatus.ToString());
            _isCompleted = true;
            _isWorking = false;
            LoginCommand.RaiseCanExecuteChanged();
            VisualStateManager.GoToState(this, "Normal", true);
        }

        private void ClearCookies()
        {
            var myFilter = new HttpBaseProtocolFilter();
            var cookieManager = myFilter.CookieManager;
            var myCookieJar = cookieManager.GetCookies(new Uri("https://oauth.vk.com/"));
            foreach (HttpCookie cookie in myCookieJar)
            {
                cookieManager.DeleteCookie(cookie);
            }
        }

        private bool _isEnterDataState;
        private bool _isWorking = false;
        private bool _isCompleted = false;
    }
}
