using System.Collections.Generic;
using Windows.Storage;

namespace VKSaver.Core.ViewModels.Common
{
    public interface IFileOpenPickerSupport
    {
        void StartFileOpenPicker();
        void ContinueFileOpenPicker(IReadOnlyList<StorageFile> files);
    }
}
