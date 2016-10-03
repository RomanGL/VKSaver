using ModernDev.InTouch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VKSaver.Core.Services.Interfaces
{
    public interface IInTouchWrapper
    {
        Task<Response<T>> ExecuteRequest<T>(Task<Response<T>> inTouchRequestTask);
    }
}