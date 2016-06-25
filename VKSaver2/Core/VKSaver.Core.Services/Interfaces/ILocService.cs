using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VKSaver.Core.Services.Interfaces
{
    public interface ILocService
    {
        string this[string resName] { get; }

        string GetResource(string resName);
    }
}
