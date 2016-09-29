using System;
namespace VKSaver.Core.Services.Interfaces
{
    public interface IMetricaService
    {
        void LogException(Exception ex);
        void LogText(string text);
    }
}
