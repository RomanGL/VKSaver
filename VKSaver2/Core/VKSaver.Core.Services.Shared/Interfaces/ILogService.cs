using System;

namespace VKSaver.Core.Services.Interfaces
{
    public interface ILogService
    {
        void LogException(Exception ex);
        void LogText(string text);
    }
}
