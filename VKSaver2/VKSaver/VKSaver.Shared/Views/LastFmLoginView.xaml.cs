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
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Phone.UI.Input;

namespace VKSaver.Views
{
    /// <summary>
    /// Представления страницы авторизации.
    /// </summary>
    public sealed partial class LastFmLoginView : VisualStateAwarePage
    {
        public LastFmLoginView()
        {
            this.InitializeComponent();
            LoginCommand = new DelegateCommand(OnLoginCommand, CanExecuteLoginCommand);
        }

        private LastFmLoginViewModel ViewModel { get { return DataContext as LastFmLoginViewModel; } }

        public DelegateCommand LoginCommand { get; private set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            VisualStateManager.GoToState(this, "Normal", false);
            _isEnterDataState = false;

            userNameBox.TextChanged += TextBox_TextChanged;
            passwordBox.PasswordChanged += TextBox_TextChanged;

            base.OnNavigatedTo(e);
        }

        private void TextBox_TextChanged(object sender, RoutedEventArgs e)
        {
            LoginCommand.RaiseCanExecuteChanged();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (_isWorking)
            {
                e.Cancel = true;
                return;
            }

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
                    Frame.GoBack();
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

        private bool _isEnterDataState;
        private bool _isWorking = false;
        private bool _isCompleted = false;
    }
}
