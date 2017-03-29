using System;
using System.Threading.Tasks;

namespace VKSaver.Core.Services.Interfaces
{
    public interface IDispatcherWrapper
    {
        Task RunOnUIThreadAsync(Action action);
    }
}
