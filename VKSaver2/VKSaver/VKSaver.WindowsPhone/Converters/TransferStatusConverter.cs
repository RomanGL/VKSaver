using System;
using VKSaver.Core.Services.Interfaces;
using Windows.Networking.BackgroundTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace VKSaver.Converters
{
    /// <summary>
    /// Конвертирует статус операции в текстовое представление.
    /// </summary>
    public class TransferStatusConverter : IValueConverter
    {
        public TransferStatusConverter()
        {
            _locService = new Lazy<ILocService>(() => ((App)Application.Current).Resolve<ILocService>());
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var status = (BackgroundTransferStatus)value;

            switch (status)
            {
                case BackgroundTransferStatus.Running:
                    return _locService.Value["TransferStatus_Running_Text"];
                case BackgroundTransferStatus.PausedByApplication:
                    return _locService.Value["TransferStatus_PausedByApplication_Text"];
                case BackgroundTransferStatus.PausedCostedNetwork:
                    return _locService.Value["TransferStatus_PausedCostedNetwork_Text"];
                case BackgroundTransferStatus.PausedNoNetwork:
                    return _locService.Value["TransferStatus_PausedNoNetwork_Text"];
                case BackgroundTransferStatus.Completed:
                    return _locService.Value["TransferStatus_Completed_Text"];
                case BackgroundTransferStatus.Canceled:
                    return _locService.Value["TransferStatus_Canceled_Text"];
                case BackgroundTransferStatus.Error:
                    return _locService.Value["TransferStatus_Error_Text"];
                default:
                    return _locService.Value["TransferStatus_Idle_Text"];
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        private readonly Lazy<ILocService> _locService;
    }
}
