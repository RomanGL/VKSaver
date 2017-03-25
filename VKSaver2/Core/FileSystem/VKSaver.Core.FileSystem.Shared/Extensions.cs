#if WINDOWS_UWP || WINDOWS_PHONE_APP
using Windows.Storage.Streams;
#endif

using System;
using System.IO;
using System.Threading.Tasks;

namespace VKSaver.Core.FileSystem
{
    public static class Extensions
    {
        public static async Task<Stream> OpenStreamForReadAsync(this IFile file)
        {
            // TODO: IFile
            throw new NotImplementedException("IFile");
        }
    }
}
