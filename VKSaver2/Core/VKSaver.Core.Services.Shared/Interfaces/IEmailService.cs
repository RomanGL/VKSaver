using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VKSaver.Core.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendSupportEmail();
    }
}
