using System;
using System.Threading.Tasks;
using VKSaver.Core.Services.VksmExtraction;

namespace VKSaver.Core.Services.Interfaces
{
    public interface IVksmExtractionService
    {
        event TypedEventHandler<IVksmExtractionService, SearchingProgressChangedEventArgs> SearchingProgressChanged;
        event TypedEventHandler<IVksmExtractionService, ExtractingProgressChangedEventArgs> ExtractingProgressChanged;
        event TypedEventHandler<IVksmExtractionService, EventArgs> ExtractionCompleted;

        Task ExtractAsync();
    }
}
