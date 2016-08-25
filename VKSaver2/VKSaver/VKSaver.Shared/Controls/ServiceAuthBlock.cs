using System;
using System.Collections.Generic;
using System.Text;
using VKSaver.Core.Models.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VKSaver.Controls
{
    [TemplateVisualState(Name = SIGNED_IN_STATE_NAME)]
    [TemplateVisualState(Name = SIGNED_OUT_STATE_NAME)]
    public class ServiceAuthBlock : Control
    {
        public ServiceAuthBlock()
        {
            this.DefaultStyleKey = typeof(ServiceAuthBlock);
        }

        public object Authorization
        {
            get { return (object)GetValue(AuthorizationProperty); }
            set { SetValue(AuthorizationProperty, value); }
        }
        
        public static readonly DependencyProperty AuthorizationProperty =
            DependencyProperty.Register("Authorization", typeof(object), 
                typeof(ServiceAuthBlock), new PropertyMetadata(default(object), OnAuthorizationChanged));
        
        private static void OnAuthorizationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var auth = e.NewValue as IServiceAuthorization;
            if (auth != null && auth.IsAuthorized)
                VisualStateManager.GoToState((Control)obj, SIGNED_IN_STATE_NAME, true);
            else
                VisualStateManager.GoToState((Control)obj, SIGNED_OUT_STATE_NAME, true);
        }
        
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _loginButton = GetTemplateChild(LOGIN_BUTTON_NAME) as Button;
            _logoutButton = GetTemplateChild(LOGOUT_BUTTON_NAME) as Button;

            if (_loginButton != null)
                _loginButton.Click += LoginLogoutButton_Click;

            if (_logoutButton != null)
                _logoutButton.Click += LoginLogoutButton_Click;

            var auth = Authorization as IServiceAuthorization;
            if (auth != null && auth.IsAuthorized)
                VisualStateManager.GoToState(this, SIGNED_IN_STATE_NAME, true);
            else
                VisualStateManager.GoToState(this, SIGNED_OUT_STATE_NAME, true);
        }

        private void LoginLogoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender == _loginButton)
                (Authorization as IServiceAuthorization)?.SignInMethod?.Invoke();
            else if (sender == _logoutButton)
                (Authorization as IServiceAuthorization)?.SignOutMethod?.Invoke();
        }

        private Button _loginButton;
        private Button _logoutButton;

        private const string LOGIN_BUTTON_NAME = "LoginButton";
        private const string LOGOUT_BUTTON_NAME = "LogoutButton";
        private const string SIGNED_OUT_STATE_NAME = "SignedOutState";
        private const string SIGNED_IN_STATE_NAME = "SignedInState";
    }
}
