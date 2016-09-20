using ModernDev.InTouch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Transfer;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class UploadsPreprocessor : IUploadsPreprocessor
    {
        public UploadsPreprocessor(
            InTouch inTouch,
            IInTouchWrapper inTouchWrapper)
        {
            _inTouch = inTouch;
            _inTouchWrapper = inTouchWrapper;
        }

        public async Task<IUpload> ProcessUploadableAsync(IUploadable uploadable)
        {
            var serverInfo = await GetServerInfoForTypeAsync(uploadable.ContentType);
            throw new NotImplementedException();
        }

        private async Task<ServerInfo> GetServerInfoForTypeAsync(FileContentType type)
        {
            Response<ServerInfo> response = null;
            switch (type)
            {
                case FileContentType.Music:
                    response = await _inTouchWrapper.ExecuteRequest(_inTouch.Audio.GetUploadServer());
                    break;
                default:
                    response = await _inTouchWrapper.ExecuteRequest(_inTouch.Docs.GetUploadServer());
                    break;
            }

            if (response.IsError)
                return null;
            return response.Data;
        }

        private readonly InTouch _inTouch;
        private readonly IInTouchWrapper _inTouchWrapper;
    }
}
