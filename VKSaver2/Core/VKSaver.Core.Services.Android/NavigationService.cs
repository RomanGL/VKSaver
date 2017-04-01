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
using Microsoft.Practices.Unity;
using VKSaver.Core.Services.Interfaces;
using VKSaver.Core.Toolkit;
using VKSaver.Core.Toolkit.Controls;
using VKSaver.Core.Toolkit.Navigation;

namespace VKSaver.Core.Services
{
    public sealed class NavigationService : INavigationService
    {
        public NavigationService(
            Context context, 
            IUnityContainer unityContainer,
            IServiceResolver serviceResolver,
            IActivityTypesResolver activityTypesResolver)
        {
            _context = context;
            _serviceResolver = serviceResolver;
            _activityTypesResolver = activityTypesResolver;
            _unityContainer = unityContainer;

            ActivityBase.ActivityCreated += ActivityBase_ActivityCreated;
            ActivityBase.ActivityResumed += ActivityBase_ActivityResumed;
            ActivityBase.ActivityPaused += ActivityBase_ActivityPaused;
        }

        public bool CanGoBack()
        {
            return true;
        }

        public bool CanGoForward()
        {
            return true;
        }

        public void ClearHistory()
        {
        }

        public void GoBack()
        {
            ActivityBase.GoBack();
        }

        public void GoForward()
        {
        }

        public bool Navigate(string pageToken, object parameter)
        {
            string activityName = pageToken.Replace("View", "Activity");
            var type = _activityTypesResolver.GetActivityType(activityName);
            var intent = new Intent(_context, type);

            if (parameter != null)
                intent.PutExtra("parameter", parameter.ToString());

            _context.StartActivity(intent);
            return true;
        }

        public void RemoveAllPages(string pageToken = null, object parameter = null)
        {
        }

        public void RemoveFirstPage(string pageToken = null, object parameter = null)
        {
        }

        public void RemoveLastPage(string pageToken = null, object parameter = null)
        {
        }

        private void ActivityBase_ActivityCreated(object sender, EventArgs e)
        {
            var vm = _unityContainer.Resolve(_activityTypesResolver.GetViewModelType(sender.GetType().Name));
            ((ActivityBase)sender).ViewModel = (VKSaverViewModel)vm;
        }

        private void ActivityBase_ActivityResumed(object sender, string e)
        {
            var args = new NavigatedToEventArgs(NavigationMode.New, e);
            var vmState = new Dictionary<string, object>();

            ((ActivityBase)sender).ViewModel?.OnNavigatedTo(args, vmState);
        }

        private void ActivityBase_ActivityPaused(object sender, string e)
        {
            var args = new NavigatingFromEventArgs(NavigationMode.New, e);
            var vmState = new Dictionary<string, object>();

            ((ActivityBase)sender).ViewModel?.OnNavigatingFrom(args, vmState, false);
        }

        private static readonly Dictionary<string, object> _sessionState = new Dictionary<string, object>();

        private readonly Context _context;
        private readonly IUnityContainer _unityContainer;
        private readonly IServiceResolver _serviceResolver;
        private readonly IActivityTypesResolver _activityTypesResolver;
    }
}