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
                var file = new MusicCacheFile(cacheFileName, _fileStream);

                TagLib.File tagFile = TagLib.File.Create(file, "audio/mpeg", TagLib.ReadStyle.Average);
                if (tagFile == null) return null;

                _startPosition = tagFile.InvariantStartPosition;                         
                _currentBitrate = (uint)tagFile.Properties.AudioBitrate * 1000;
                _currentChannels = (uint)tagFile.Properties.AudioChannels;

                _bufferSize = (int)(_currentBitrate / 8) * (BUFFERED_TIME_SECONDS + 3);
                _buffer = new byte[_bufferSize];
                
                _sampleDuration = TimeSpan.FromMilliseconds(SAMPLE_SIZE / (_currentBitrate / 1000 / 8));

                var audioProperties = AudioEncodingProperties.CreateMp3(
                    (uint)tagFile.Properties.AudioSampleRate, 
                    _currentChannels, _currentBitrate);

                var audioDescriptor = new AudioStreamDescriptor(audioProperties);
                _mediaSource = new MediaStreamSource(audioDescriptor);

                _mediaSource.CanSeek = true;
                _mediaSource.MusicProperties.Title = Track.Title;
                _mediaSource.MusicProperties.Artist = Track.Artist;
                _mediaSource.MusicProperties.Album = "ВКачай";
                _mediaSource.Duration = tagFile.Properties.Duration;
                _mediaSource.BufferTime = TimeSpan.FromSeconds(BUFFERED_TIME_SECONDS);

                _mediaSource.Starting += MediaSource_Starting;
                _mediaSource.SampleRequested += MediaSource_SampleRequested;
                _mediaSource.Closed += MediaSource_Closed;

                Debug.WriteLine($"\nTrack: {Track.Title}\nBitrate: {_currentBitrate} kbps\nChannels: {_currentChannels}\nSample rate: {tagFile.Properties.AudioSampleRate}\nBits per sample: {tagFile.Properties.BitsPerSample}\n");
                                    
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
        }

        /// <summary>
        /// Вызывается, когда источник мультимедиа готов запрашивать сэмплы.
        /// </summary>
        private void MediaSource_Starting(MediaStreamSource sender, MediaStreamSourceStartingEventArgs args)
        {            
            var request = args.Request;
            if (request.StartPosition != null && request.StartPosition.Value <= _mediaSource.Duration)
            {
                long sampleOffset = request.StartPosition.Value.Ticks / _sampleDuration.Ticks;
                _timeOffset = TimeSpan.FromTicks(sampleOffset * _sampleDuration.Ticks);
                _byteOffset = sampleOffset * BUFFER_SIZE;
            }

            request.SetActualStartPosition(_timeOffset);
        }
        
        /// <summary>
        /// Вызывается при запросе нового сэмпла.
        /// </summary>
        private void MediaSource_SampleRequested(MediaStreamSource sender, MediaStreamSourceSampleRequestedEventArgs args)
        {
            var privateRequest = args.Request;
            long newOffset = _byteOffset + BUFFER_SIZE;

            if (newOffset <= _fileStream.Length)
            {
                int count = SetSample(privateRequest);
            }            
            else
            {
                privateRequest.GetDeferral().Complete();
            }
        }

        /// <summary>
        /// Вызывается при завершении работы источника мультимедиа.
        /// </summary>
        private void MediaSource_Closed(MediaStreamSource sender, MediaStreamSourceClosedEventArgs args)
        {
            sender.Starting -= MediaSource_Starting;
            sender.SampleRequested -= MediaSource_SampleRequested;
            sender.Closed -= MediaSource_Closed;

            if (sender == _mediaSource)
                _mediaSource = null;

            Dispose();
        }

        /// <summary>
        /// Устанвливает следующий сэмпл для запроса.
        /// </summary>
        private int SetSample(MediaStreamSourceSampleRequest request)
        {
            try
            {
                Debug.WriteLine($"Length: {_fileStream.Length}, Offset: {_byteOffset}");

                _fileStream.Seek(_byteOffset, SeekOrigin.Begin);
                int count = _fileStream.Read(_buffer, _bufferOffset, BUFFER_SIZE);
                
                IBuffer buffer = _buffer.AsBuffer(_bufferOffset, count);
                _bufferOffset += count;
                if (_bufferOffset + BUFFER_SIZE > _bufferSize)
                    _bufferOffset = 0;
                
                var duration = TimeSpan.FromMilliseconds(count / (_currentBitrate / 1000 / 8));
                var sample = MediaStreamSample.CreateFromBuffer(buffer, _timeOffset);
                sample.Duration = duration;
                sample.KeyFrame = true;

                if (_byteOffset > _startPosition)
                    _timeOffset = _timeOffset.Add(_sampleDuration);

                _byteOffset += count;                

                request.Sample = sample;
                return count;
            }
            catch (Exception) { return 0; }
        }

        private const int BUFFER_SIZE = SAMPLE_SIZE;
        private const int SAMPLE_SIZE = 1152;
        private const int BUFFERED_TIME_SECONDS = 1;

        private int _bufferOffset;
        private int _bufferSize;
        private byte[] _buffer;
        private TimeSpan _sampleDuration;
        private MediaStreamSource _mediaSource;
        private Stream _fileStream;
        private TimeSpan _timeOffset;
        private CachedFileData _fileData;
        private long _byteOffset;
        private uint _currentChannels;
        private uint _currentBitrate;
        private long _startPosition;
        private IMusicCacheService _musicCacheService;
    }
}
