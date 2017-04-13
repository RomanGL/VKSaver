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
using VKSaver.Core.Toolkit.Controls;
using VKSaver.Core.ViewModels;

namespace VKSaver.Android.Views
{
    [Activity(
        Label = "@string/ApplicationName",
        Theme = "@android:style/Theme.DeviceDefault")]
    public class MainActivity : ActivityBase
    {
        private MainViewModel VM => (MainViewModel)ViewModel;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.MainActivity);
        }
    }
}