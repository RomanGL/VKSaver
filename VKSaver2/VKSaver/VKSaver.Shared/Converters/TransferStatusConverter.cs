using System;
using System.Collections.Generic;
using System.Text;
using Windows.Networking.BackgroundTransfer;
using Windows.UI.Xaml.Data;

namespace VKSaver.Converters
{
    /// <summary>
    /// Конвертирует статус операции в текстовое представление.
    /// </summary>
    public class TransferStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var status = (BackgroundTransferStatus)value;

            switch (status)
            {
                case BackgroundTransferStatus.Running:
                    return "Выполняется...";
                case BackgroundTransferStatus.PausedByApplication:
                    return "Приостановлено";
                case BackgroundTransferStatus.PausedCostedNetwork:
                    return "Платная сеть";
                case BackgroundTransferStatus.PausedNoNetwork:
                    return "Соединение отсутствует";
                case BackgroundTransferStatus.Completed:
                    return "Завершено";
                case BackgroundTransferStatus.Canceled:
                    return "Отменено";
                case BackgroundTransferStatus.Error:
                    return "Ошибка";
                default:
                    return "Ожидание...";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
