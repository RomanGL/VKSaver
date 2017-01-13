using System;
using VKSaver.Core.Services.Interfaces;
using Yandex.Metrica;

namespace VKSaver.Core.Services
{
    public sealed class MetricaService : IMetricaService
    {
        public void LogException(Exception ex)
        {
#if !DEBUG
            try
            {
                YandexMetrica.ReportError(ex.Message, ex);
            }
            catch (Exception) { }
#endif
        }

        public void LogEvent(string eventName)
        {
#if !DEBUG
            try
            {
                YandexMetrica.ReportEvent(eventName);
            }
            catch (Exception) { }
#endif
        }

        public void LogEvent(string eventName, string json)
        {
#if !DEBUG
            try
            {
                YandexMetrica.ReportEvent(eventName, json);
            }
            catch (Exception) { }
#endif
        }
    }
}
