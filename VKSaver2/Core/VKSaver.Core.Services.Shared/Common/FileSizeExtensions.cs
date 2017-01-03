using System;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services.Common
{
    public static class FileSizeExtensions
    {
        public static string ToFormattedString(this FileSize size, ILocService locService)
        {
            if (size.Kilobytes < 1024)
                return String.Format(locService["FileSize_KB_Mask_Text"], size.Kilobytes);
            else if (size.Megabytes >= 1 && size.Megabytes < 1024)
                return String.Format(locService["FileSize_MB_Mask_Text"], Math.Round(size.Megabytes, 2));
            else if (size.Gigabytes >= 1 && size.Gigabytes < 1024)
                return String.Format(locService["FileSize_GB_Mask_Text"], Math.Round(size.Gigabytes, 2));
            else if (size.Terabytes >= 1 && size.Terabytes < 1024)
                return String.Format(locService["FileSize_TB_Mask_Text"], Math.Round(size.Terabytes, 2));
            else if (size.Petabytes >= 1 && size.Petabytes < 1024)
                return String.Format(locService["FileSize_PB_Mask_Text"], Math.Round(size.Petabytes, 2));
            else if (size.Exabytes >= 1 && size.Exabytes < 1024)
                return String.Format(locService["FileSize_EB_Mask_Text"], Math.Round(size.Exabytes, 2));
            else
                return String.Format(locService["FileSize_Bytes_Mask_Text"], size.Bytes);
        }
    }
}
