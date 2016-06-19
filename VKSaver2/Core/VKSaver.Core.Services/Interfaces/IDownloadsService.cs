using System;
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

        bool IsLoading { get; }

        void DiscoverActiveDownloadsAsync();
        void CancelDownload(Guid operationGuid);
        void PauseResume(Guid operationGuid);
        DownloadItem[] GetAllDownloads();
        Task<List<DownloadInitError>> StartDownloadingAsync(IList<IDownloadable> items);        
    }
}