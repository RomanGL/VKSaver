using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace VKSaver.Core.FileSystem
{
    public sealed class File : IFile
    {
        public File(string path)
        {
        }

        public string DisplayName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string FileType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Name
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public string Path
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IFileProperties Properties
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Task DeleteAsync(bool isPermanent = true)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> OpenAsync(FileAccessMode accessMode)
        {
            throw new NotImplementedException();
        }

        public Task RenameAsync(string desiredName, NameCollisionOption option)
        {
            throw new NotImplementedException();
        }
    }
}