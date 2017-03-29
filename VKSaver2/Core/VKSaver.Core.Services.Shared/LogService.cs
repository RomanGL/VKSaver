using System;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class LogService : ILogService
    {
        public LogService(IMetricaService metricaService)
        {
            _metricaService = metricaService;
        }

        public void LogException(Exception ex)
        {
            _metricaService?.LogException(ex);
        }

        public void LogText(string text)
        {
//            var writter = await GetWritterAsync();
//            if (writter == null)
//                return;

//            string textToWrite = $"{DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")}: {text}\n";

//#if DEBUG
//            Debug.WriteLine(textToWrite);
//#endif

//            await writter.WriteLineAsync(textToWrite);
//            writter.Flush();
        }

        //private Task<StreamWriter> GetWritterAsync()
        //{
        //    return Task.Run(() =>
        //    {
        //        try
        //        {
        //            lock (_lockObject)
        //            {
        //                if (_writter != null)
        //                    return _writter;

        //                var logFile = ApplicationData.Current.LocalFolder.CreateFileAsync(
        //                    LOG_FILE_NAME, CreationCollisionOption.OpenIfExists).GetAwaiter().GetResult();
        //                var stream = logFile.OpenStreamForWriteAsync().GetAwaiter().GetResult();
        //                stream.Seek(0, SeekOrigin.End);

        //                _writter = new StreamWriter(stream, Encoding.UTF8);
        //                return _writter;
        //            }
        //        }
        //        catch (Exception)
        //        {
        //            return null;
        //        }
        //    });
        //}
        
        //private StreamWriter _writter;

        private readonly object _lockObject = new object();
        private readonly IMetricaService _metricaService;

        private const string LOG_FILE_NAME = "vkslog.txt";
    }
}
