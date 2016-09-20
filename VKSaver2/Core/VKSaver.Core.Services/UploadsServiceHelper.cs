using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Models.Transfer;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class UploadsServiceHelper : IUploadsServiceHelper
    {
        public UploadsServiceHelper(IUploadsPreprocessor uploadsPreprocessor)
        {
            _uploadsPreprocessor = uploadsPreprocessor;
        }

        public async Task<bool> StartUploadingAsync(IUploadable item)
        {
            throw new NotImplementedException();
        }

        private readonly IUploadsPreprocessor _uploadsPreprocessor;
    }
}
