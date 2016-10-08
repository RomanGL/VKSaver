using System;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Interfaces;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace VKSaver.Converters
{
    /// <summary>
    /// Представляет конвертер для структуры размера файла.
    /// </summary>
    public class FileSizeConverter : IValueConverter
    {
        public FileSizeConverter()
        {
            _locService = new Lazy<ILocService>(() => ((App)Application.Current).Resolve<ILocService>());
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var size = (FileSize)value;

            if (size.Kilobytes < 1024)
                return String.Format(_locService.Value["FileSize_KB_Mask_Text"], size.Kilobytes);
            else if (size.Megabytes >= 1 && size.Megabytes < 1024)
                return String.Format(_locService.Value["FileSize_MB_Mask_Text"], Math.Round(size.Megabytes, 2));
            else if (size.Gigabytes >= 1 && size.Gigabytes < 1024)
                return String.Format(_locService.Value["FileSize_GB_Mask_Text"], Math.Round(size.Gigabytes, 2));
            else if (size.Terabytes >= 1 && size.Terabytes < 1024)
                return String.Format(_locService.Value["FileSize_TB_Mask_Text"], Math.Round(size.Terabytes, 2));
            else if (size.Petabytes >= 1 && size.Petabytes < 1024)
                return String.Format(_locService.Value["FileSize_PB_Mask_Text"], Math.Round(size.Petabytes, 2));
            else if (size.Exabytes >= 1 && size.Exabytes < 1024)
                return String.Format(_locService.Value["FileSize_EB_Mask_Text"], Math.Round(size.Exabytes, 2));
            else
                return String.Format(_locService.Value["FileSize_Bytes_Mask_Text"], size.Bytes);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        private readonly Lazy<ILocService> _locService;
    }
}
