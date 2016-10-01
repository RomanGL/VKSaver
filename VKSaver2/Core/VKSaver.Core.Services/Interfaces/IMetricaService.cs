using System;
namespace VKSaver.Core.Services.Interfaces
{
    public interface IMetricaService
    {
        void LogException(Exception ex);
        void LogEvent(string eventName);
        void LogEvent(string eventName, string json);
    }
}
