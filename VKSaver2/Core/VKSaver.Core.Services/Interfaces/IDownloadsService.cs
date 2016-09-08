﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VKSaver.Core.Models.Transfer;
using VKSaver.Core.Transfer;

namespace VKSaver.Core.Services.Interfaces
{
    public interface IDownloadsService
    {
        event EventHandler<DownloadOperationErrorEventArgs> DownloadError;
        event EventHandler<DownloadItem> ProgressChanged;
        event EventHandler DownloadsCompleted;

        bool IsLoading { get; }

        void DiscoverActiveDownloadsAsync();
        void Cancel(Guid operationGuid);
        void PauseResume(Guid operationGuid);

        DownloadItem[] GetAllDownloads();
        int GetDownloadsCount();
        Task<List<DownloadInitError>> StartDownloadingAsync(IList<IDownloadable> items);
        Task CancelAll();    
    }
}