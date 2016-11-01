﻿using System;
using VKSaver.Core.Services.Interfaces;
using Yandex.Metrica;

namespace VKSaver.Core.Services
{
    public sealed class MetricaService : IMetricaService
    {
        public void LogException(Exception ex)
        {
            try
            {
                YandexMetrica.ReportError(ex.Message, ex);
            }
            catch (Exception) { }
        }

        public void LogEvent(string eventName)
        {
            try
            {
                YandexMetrica.ReportEvent(eventName);
            }
            catch (Exception) { }
        }

        public void LogEvent(string eventName, string json)
        {
            try
            {
                YandexMetrica.ReportEvent(eventName, json);
            }
            catch (Exception) { }
        }
    }
}