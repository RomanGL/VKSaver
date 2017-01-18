using System;
using System.Net;
using System.Threading.Tasks;
using VKSaver.Core.Models.Common;
using VKSaver.Core.Services.Interfaces;

namespace VKSaver.Core.Services
{
    public sealed class HttpFileService : IHttpFileService
    {
        public async Task<FileSize> GetFileSize(string url)
        {
            try
            {
                var request = WebRequest.Create(url);
                request.Method = "HEAD";

                var response = await request.GetResponseAsync();
                return FileSize.FromBytes((ulong)response.ContentLength);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Не удалось получить данные о размере файла.", ex);
            }
        }
    }
}
