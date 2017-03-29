using System;
using GalaSoft.MvvmLight.Command;

namespace VKSaver.Core.ViewModels.Common
{
    public sealed class DelegateCommand : RelayCommand
    {
        public DelegateCommand(Action execute) 
            : base(execute)
        {
        }

        public DelegateCommand(Action execute, Func<bool> canExecute) 
            : base(execute, canExecute)
        {
        }
    }

    public sealed class DelegateCommand<T> : RelayCommand<T>
    {
        public DelegateCommand(Action<T> execute) 
            : base(execute)
        {
        }

        public DelegateCommand(Action<T> execute, Func<T, bool> canExecute) 
            : base(execute, canExecute)
        {
        }
    }
}