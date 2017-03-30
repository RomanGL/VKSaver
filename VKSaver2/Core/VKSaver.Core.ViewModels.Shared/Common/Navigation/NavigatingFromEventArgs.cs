using System;

namespace VKSaver.Core.ViewModels.Common.Navigation
{
    public sealed class NavigatingFromEventArgs : EventArgs
    {
        public NavigatingFromEventArgs(NavigationMode navigationMode, object parameter)
        {
            NavigationMode = navigationMode;
            Parameter = parameter;
        }

        public bool Cancel { get; set; }
        public NavigationMode NavigationMode { get; }
        public object Parameter { get; }
    }
}
