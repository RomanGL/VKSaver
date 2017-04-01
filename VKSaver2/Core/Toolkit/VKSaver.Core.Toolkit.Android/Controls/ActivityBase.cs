using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace VKSaver.Core.Toolkit.Controls
{
    public abstract class ActivityBase : Activity
    {
        private string _parameter;

        public static event EventHandler ActivityCreated;
        public static event EventHandler<string> ActivityPaused;
        public static event EventHandler<string> ActivityResumed;

        public VKSaverViewModel ViewModel { get; set; }
        public static ActivityBase CurrentActivity { get; private set; }

        public static void GoBack()
        {
            CurrentActivity?.OnBackPressed();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            ActivityCreated?.Invoke(this, EventArgs.Empty);
            _parameter = Intent.GetStringExtra("parameter");
        }

        protected override void OnResume()
        {
            CurrentActivity = this;
            ActivityResumed?.Invoke(this, _parameter);

            base.OnResume();
        }

        protected override void OnPause()
        {
            ActivityPaused?.Invoke(this, _parameter);
            base.OnPause();
        }
    }
}