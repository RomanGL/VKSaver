using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace VKSaver.Core.Services.Interfaces
{
    public interface IDispatcherWrapper
    {
        CoreDispatcher Dispatcher { get; }
    }
}
