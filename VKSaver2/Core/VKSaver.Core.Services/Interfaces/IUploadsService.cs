using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VKSaver.Core.Models.Transfer;

namespace VKSaver.Core.Services.Interfaces
{
    public interface IUploadsService
    {
        event EventHandler<TransferOperationErrorEventArgs> UploadError;
        event EventHandler<TransferItem> ProgressChanged;
        event EventHandler UploadsCompleted;

        bool IsLoading { get; }

        void DiscoverActiveUploads();
        void Cancel(Guid operationGuid);

        IEnumerable<TransferItem> GetAllUploads();
        int GetUploadsCount();

        Task<List<UploadInitError>> StartUploadingAsync(IList<IUpload> uploads);
        Task<UploadInitError> StartUploadingAsync(IUpload upload);
        Task CancelAllAsync();
    }
}
