using System;
using System.Collections.Generic;
using VKSaver.Core.Models;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Transfer;
using VKSaver.Core.Services.Common;
using VKSaver.Core.Services.Interfaces;
using Windows.Networking.BackgroundTransfer;

namespace VKSaver.Core.Services
{
    public sealed class TransferNotificationsService : ISuspendingService
    {
        public TransferNotificationsService(
            IDownloadsService downloadsService,
            IUploadsService uploadsService,
            ILocService locService,
            IAppNotificationsService appNotificationsService)
        {
            _downloadsService = downloadsService;
            _uploadsService = uploadsService;
            _locService = locService;
            _appNotificationsService = appNotificationsService;

            _notifications = new Dictionary<Guid, AppNotification>(4);
        }

        public void StartService()
        {
            lock (_lockObject)
            {
                if (_isRunning)
                    return;
                _isRunning = true;
            }

            _downloadsService.ProgressChanged += DownloadsService_ProgressChanged;
            _uploadsService.ProgressChanged += UploadsService_ProgressChanged;
        }

        public void StopService()
        {
            lock (_lockObject)
            {
                if (!_isRunning)
                    return;
                _isRunning = false;
            }

            _downloadsService.ProgressChanged -= DownloadsService_ProgressChanged;
            _uploadsService.ProgressChanged -= UploadsService_ProgressChanged;
        }

        private void UploadsService_ProgressChanged(object sender, TransferItem e)
        {
            TransferProgressChanged(TransferType.Upload, e);
        }

        private void DownloadsService_ProgressChanged(object sender, TransferItem e)
        {
            TransferProgressChanged(TransferType.Download, e);
        }

        private void TransferProgressChanged(TransferType type, TransferItem e)
        {
            AppNotification notification = null;
            _notifications.TryGetValue(e.OperationGuid, out notification);

            if (notification == null)
            {
                if (e.Status == BackgroundTransferStatus.Running && e.ProcessedSize.Bytes > 0)
                {
                    notification = new AppNotification
                    {
                        Type = AppNotificationType.Default,
                        NoVibration = true,
                        NoSound = true
                    };

                    notification.ProgressPercent = e.TotalSize.Bytes == 0 ? 0 : e.ProcessedSize.Bytes / (e.TotalSize.Bytes / 100);                    
                    notification.Content = String.Format(_locService["TransferView_SizeMask_Text"],
                        e.ProcessedSize.ToFormattedString(_locService), e.TotalSize.ToFormattedString(_locService));
                    notification.Duration = TimeSpan.MaxValue;                    

                    if (type == TransferType.Download)
                    {
                        notification.Title = String.Format(_locService["TransferView_Downloading_Notification_Text"], e.Name);
                        notification.ImageUrl = "ms-appx:///Assets/Popups/Download.png";
                    }
                    else
                    {
                        notification.Title = String.Format(_locService["TransferView_Uploading_Notification_Text"], e.Name);
                        notification.ImageUrl = "ms-appx:///Assets/Popups/Upload.png";
                    }

                    _notifications[e.OperationGuid] = notification;
                    _appNotificationsService.SendNotification(notification);
                }
            }
            else
            {
                if (e.Status == BackgroundTransferStatus.Running)
                {
                    notification.ProgressPercent = e.TotalSize.Bytes == 0 ? 0 : e.ProcessedSize.Bytes / (e.TotalSize.Bytes / 100);
                    notification.Content = String.Format(_locService["TransferView_SizeMask_Text"],
                        e.ProcessedSize.ToFormattedString(_locService), e.TotalSize.ToFormattedString(_locService));
                }
                else
                {
                    _notifications.Remove(e.OperationGuid);
                    notification.Hide();
                }
            }

            if (e.Status == BackgroundTransferStatus.Error)
            {
                _appNotificationsService.SendNotification(new AppNotification
                {
                    Type = AppNotificationType.Error,
                    Title = type == TransferType.Download ? 
                         _locService["Toast_Downloads_Fail_Text"] : 
                         _locService["Toast_Uploads_Fail_Text"],
                    Content = e.Name
                });
                _notifications.Remove(e.OperationGuid);
            }
            else if (e.Status == BackgroundTransferStatus.Completed)
            {
                if (type == TransferType.Download)
                {
                    _appNotificationsService.SendNotification(new AppNotification
                    {
                        Type = AppNotificationType.Info,
                        Title = _locService["Toast_Downloads_Success_Text"],
                        Content = e.Name
                    });
                }

                _notifications.Remove(e.OperationGuid);
            }
        }

        private bool _isRunning;

        private readonly IDownloadsService _downloadsService;
        private readonly IUploadsService _uploadsService;
        private readonly ILocService _locService;
        private readonly IAppNotificationsService _appNotificationsService;

        private readonly Dictionary<Guid, AppNotification> _notifications;
        private readonly object _lockObject = new object();  

        private enum TransferType : byte
        {
            Download, 
            Upload
        };
    }
}
