using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Models.Transfer;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class UploadsPostprocessor : IUploadsPostprocessor
    {
        public UploadsPostprocessor()
        {

        }

        public Task ProcessUploadAsync(IUpload upload)
        {
            throw new NotImplementedException();
        }        
    }
}
