using ModernDev.InTouch;
using System;
using System.Net.Http;
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

        public async Task<Tuple<IUpload, UploadsPreprocessorResultType>> ProcessUploadableAsync(IUploadable uploadable)
        {
            var result = await GetServerInfoForTypeAsync(uploadable.ContentType);
            if (result.Item1 == null)
                return new Tuple<IUpload, UploadsPreprocessorResultType>(null, result.Item2);

            return new Tuple<IUpload, UploadsPreprocessorResultType>(
                new Upload
                {
                    Uploadable = uploadable,
                    UploadUrl = result.Item1.UploadUrl
                }, 
                UploadsPreprocessorResultType.Success);
        }

        private async Task<Tuple<ServerInfo, UploadsPreprocessorResultType>> GetServerInfoForTypeAsync(FileContentType type)
        {
            try
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
                {
                    return new Tuple<ServerInfo, UploadsPreprocessorResultType>(null,
                        UploadsPreprocessorResultType.ServerError);
                }
                return new Tuple<ServerInfo, UploadsPreprocessorResultType>(response.Data,
                    UploadsPreprocessorResultType.Success);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null && ex.InnerException is HttpRequestException)
                {
                    return new Tuple<ServerInfo, UploadsPreprocessorResultType>(null,
                        UploadsPreprocessorResultType.ConnectionError);
                }
                else
                {
                    return new Tuple<ServerInfo, UploadsPreprocessorResultType>(null,
                        UploadsPreprocessorResultType.UnknownError);
                }
            }
        }

        private readonly InTouch _inTouch;
        private readonly IInTouchWrapper _inTouchWrapper;
    }
}
