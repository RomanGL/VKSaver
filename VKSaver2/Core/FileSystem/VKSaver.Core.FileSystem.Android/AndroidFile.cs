using System;
using System.Collections.Generic;
using Java.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.IO;

namespace VKSaver.Core.FileSystem
{
    public sealed class AndroidFile : IFile, IDisposable
    {
        private readonly Java.IO.File _currentFile;

        public AndroidFile(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
                throw new ArgumentException(nameof(path));

            _currentFile = new Java.IO.File(path);
            if (!_currentFile.IsFile)
                throw new ArgumentException("This is not file.");
        }

        internal AndroidFile(Java.IO.File file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));
            if (!file.IsFile)
                throw new ArgumentException("This is not file.");

            _currentFile = file;
        }

        public string DisplayName => _currentFile.Name;

        public string FileType => System.IO.Path.GetExtension(_currentFile.Name);

        public string Name => _currentFile.Name;

        public string Path => _currentFile.Path;

        public IFileProperties Properties
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Task DeleteAsync(bool isPermanent = true)
        {
            return Task.Run(() =>
            {
                bool result = _currentFile.Delete();
                if (!result)
                    throw new Exception("File can't be deleted.");
            });
        }

        public Task<Stream> OpenAsync(FileAccessMode accessMode)
        {
            throw new NotImplementedException();
        }

        public Task RenameAsync(string desiredName, NameCollisionOption option)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _currentFile.Dispose();
        }
    }
}