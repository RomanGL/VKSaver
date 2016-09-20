using ModernDev.InTouch;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Models.Transfer;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class UploadsPostprocessor : IUploadsPostprocessor
    {
        public UploadsPostprocessor(InTouch inTouch, IInTouchWrapper inTouchWrapper, ILogService logService)
        {
            _inTouch = inTouch;
            _inTouchWrapper = inTouchWrapper;
            _logService = logService;
        }

        public async Task<UploadsPostprocessorResultType> ProcessUploadAsync(ICompletedUpload upload)
        {
            try
            {
                switch (upload.ContentType)
                {
                    case FileContentType.Music:
                        return await SaveAudioAsync(upload.ServerResponse);
                    case FileContentType.Video:
                        break;
                    default:
                        return await SaveDocumentASync(upload.ServerResponse);
                }
            }
            catch (Exception ex)
            {
                _logService.LogException(ex);
                if (ex.InnerException != null && ex.InnerException is HttpRequestException)
                    return UploadsPostprocessorResultType.ConnectionError;
            }

            return UploadsPostprocessorResultType.Unknown;
        } 
        
        private async Task<UploadsPostprocessorResultType> SaveAudioAsync(string serverResponse)
        {
            var json = JObject.Parse(serverResponse);

            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Audio.Save(new AudioUploadResponse
            {
                Redirect = json["redirect"].Value<string>(),
                Server = json["server"].Value<string>(),
                Audio = json["audio"].Value<string>(),
                Hash = json["hash"].Value<string>()
            }));

            if (response.IsError)
                return UploadsPostprocessorResultType.ServerError;

            return UploadsPostprocessorResultType.Success;
        }

        private async Task<UploadsPostprocessorResultType> SaveDocumentASync(string serverResponse)
        {
            var json = JObject.Parse(serverResponse);

            var response = await _inTouchWrapper.ExecuteRequest(_inTouch.Docs.Save(
                json["file"].Value<string>()));

            if (response.IsError)
                return UploadsPostprocessorResultType.ServerError;

            return UploadsPostprocessorResultType.Success;
        }

        private readonly ILogService _logService;
        private readonly InTouch _inTouch;
        private readonly IInTouchWrapper _inTouchWrapper;
    }
}
