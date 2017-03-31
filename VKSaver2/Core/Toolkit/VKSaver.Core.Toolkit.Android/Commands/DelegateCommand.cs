using System;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Command;

namespace VKSaver.Core.Toolkit.Commands
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

        public Task Execute(object parameter)
        {
            return Task.Run(() => Execute(parameter));
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

        public Task Execute(T parameter)
        {
            return Task.Run(() => Execute(parameter));
        }
    }
}