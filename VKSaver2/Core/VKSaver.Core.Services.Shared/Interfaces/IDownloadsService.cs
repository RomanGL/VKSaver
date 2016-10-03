using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VKSaver.Core.Models.Transfer;

namespace VKSaver.Core.Services.Interfaces
{
    public interface IDownloadsService
    {
        event EventHandler<TransferOperationErrorEventArgs> DownloadError;
        event EventHandler<TransferItem> ProgressChanged;
        event EventHandler DownloadsCompleted;

        bool IsLoading { get; }

        void DiscoverActiveDownloads();
        void Cancel(Guid operationGuid);
        void PauseResume(Guid operationGuid);

        TransferItem[] GetAllDownloads();
        int GetDownloadsCount();
        Task<List<DownloadInitError>> StartDownloadingAsync(IList<IDownloadable> items);
        Task CancelAllAsync();    
    }
}