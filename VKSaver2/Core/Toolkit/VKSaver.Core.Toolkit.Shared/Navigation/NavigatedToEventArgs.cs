using System;

namespace VKSaver.Core.Toolkit.Navigation
{
    public sealed class NavigatedToEventArgs : EventArgs
    {
        public NavigatedToEventArgs(NavigationMode mode, object parameter)
        {
            NavigationMode = mode;
            Parameter = parameter;
        }

        public NavigationMode NavigationMode { get; }
        public object Parameter { get; }
    }
}
