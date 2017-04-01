using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using GalaSoft.MvvmLight.Helpers;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Toolkit.Controls;
using VKSaver.Core.ViewModels;
using Microsoft.Practices.Unity;

namespace VKSaver.Android.Views
{
    [Activity(Label = "LoginActivity", Theme = "@android:style/Theme.DeviceDefault.NoActionBar", NoHistory = true)]
    public class DirectAuthActivity : ActivityBase, ILoader
    {
        private ProgressDialog _progressDialog;
        private EditText _loginText;
        private EditText _passwordText;
        private Button _loginButton;

        private DirectAuthViewModel VM => (DirectAuthViewModel)ViewModel;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.LoginActivity);

            GetElements();
            SetBindings();
        }

        protected override void OnResume()
        {
            SplashScreenActivity.Container.Resolve<IAppLoaderService>().AddLoader(this);
            base.OnResume();
        }

        protected override void OnPause()
        {
            SplashScreenActivity.Container.Resolve<IAppLoaderService>().RemoveLoader(this);
            base.OnPause();
        }

        private void GetElements()
        {
            _loginButton = FindViewById<Button>(Resource.Id.LoginButton);
            _loginText = FindViewById<EditText>(Resource.Id.LoginEditText);
            _passwordText = FindViewById<EditText>(Resource.Id.PasswordEditText);
        }

        private void SetBindings()
        {
            this.SetBinding(() => VM.LoginText, () => _loginText.Text, BindingMode.TwoWay);
            this.SetBinding(() => VM.PasswordText, () => _passwordText.Text, BindingMode.TwoWay);

            _loginButton.Click += _loginButton_Click;
        }

        private async void _loginButton_Click(object sender, EventArgs e)
        {
            await VM.Login();
        }

        public void ShowLoader(string text)
        {
            _progressDialog = ProgressDialog.Show(this, "", text, true);
        }

        public void HideLoader()
        {
            _progressDialog.Hide();
        }
    }
}