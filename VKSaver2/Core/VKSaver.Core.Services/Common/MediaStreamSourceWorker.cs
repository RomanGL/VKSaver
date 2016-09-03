using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VKSaver.Core.Models;
using Windows.Storage;
using System.IO;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.Web.Http;
using TagLib.Mpeg;
using System.Runtime.InteropServices.WindowsRuntime;
using VKSaver.Core.Models.Player;
using VKSaver.Core.Services.Interfaces;
using System.Diagnostics;
using FFmpegInterop;

namespace VKSaver.Core.Services.Common
{
    /// <summary>
    /// Представляет обработчик источника мультимедиа <see cref="MediaStreamSource"/>.
    /// </summary>
    public sealed class MediaStreamSourceWorker : IDisposable
    {        
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="MediaStreamSourceWorker"/>
        /// для указанного аудио трека.
        /// </summary>
        /// <param name="track">Аудио трек.</param>
        public MediaStreamSourceWorker(IPlayerTrack track, IMusicCacheService musicCacheService)
        {
            Track = track;
            _musicCacheService = musicCacheService;
        }

        /// <summary>
        /// Возвращает трек, для которого создан медиа источник.
        /// </summary>
        public IPlayerTrack Track { get; private set; }

        /// <summary>
        /// Получить медиа источник.
        /// </summary>
        public async Task<MediaStreamSource> GetSource()
        {
            if (_mediaSource != null)
                return _mediaSource;
            

            try
            {
                string cacheFileName = $"{Track.VKInfo.OwnerID} {Track.VKInfo.ID}.vksm";

                _fileData = await _musicCacheService.GetCachedFileData(cacheFileName);
                if (_fileData == null)
                    return null;
                                
                _fileStream = await _fileData.GetStream();
                _ffmpeg = FFmpegInteropMSS.CreateFFmpegInteropMSSFromStream(_fileStream.AsRandomAccessStream(), true, false);
                _mediaSource = _ffmpeg.GetMediaStreamSource();
                _mediaSource.BufferTime = TimeSpan.Zero;

                return _mediaSource;
            }
            catch (Exception) { return null; }
        }

        public void Dispose()
        {
            if (_fileData != null)
            {
                _fileData.Dispose();
                _fileData = null;
            }

            _ffmpeg?.Dispose();
        }

        private MediaStreamSource _mediaSource;
        private Stream _fileStream;
        private CachedFileData _fileData;
        private FFmpegInteropMSS _ffmpeg;
        private IMusicCacheService _musicCacheService;
    }
}
