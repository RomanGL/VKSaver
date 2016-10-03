using Windows.UI.Core;

namespace VKSaver.Core.Services.Interfaces
{
    public interface IDispatcherWrapper
    {
        CoreDispatcher Dispatcher { get; }
    }
}
