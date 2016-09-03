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
            if (_mediaSource != null) return _mediaSource;

            _isCanceled = false;
            _isCompleted = false;

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
                                                
                //_byteOffset = tagFile.InvariantStartPosition;
                _currentBitrate = (uint)tagFile.Properties.AudioBitrate * 1000;
                _currentChannels = (uint)tagFile.Properties.AudioChannels;

                _bufferSize = (int)(_currentBitrate / 8) * (BUFFERED_TIME_SECONDS + 1);
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
            _isCanceled = true;

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

        MediaStreamSourceSampleRequest request;

        /// <summary>
        /// Вызывается при запросе нового сэмпла.
        /// </summary>
        private void MediaSource_SampleRequested(MediaStreamSource sender, MediaStreamSourceSampleRequestedEventArgs args)
        {
            GC.Collect(1, GCCollectionMode.Forced);

            var privateRequest = args.Request;
            long newOffset = _byteOffset + BUFFER_SIZE;

            if (newOffset <= _fileStream.Length ||
                (_isCompleted && newOffset - _fileStream.Length < BUFFER_SIZE && newOffset - _fileStream.Length > 0))
            {
                int count = SetSample(privateRequest);

                if (count == 0)
                {
                    request = privateRequest;
                    request.ReportSampleProgress(0);
                    var deferral = request.GetDeferral();
                    SetSample(request);
                    deferral.Complete();
                    request = null;
                }
                else privateRequest.GetDeferral().Complete();
            }
            else if (!_isCanceled && !_isCompleted)
            {
                request = privateRequest;
                uint percent = (uint)((_fileStream.Length - _byteOffset) / BUFFER_SIZE);
                request.ReportSampleProgress(percent);
                var deferral = request.GetDeferral();
                SetSample(request);
                deferral.Complete();
                request = null;
            }
            else
            {
                privateRequest.GetDeferral().Complete();
            }

            GC.Collect(1, GCCollectionMode.Forced);
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
        /// Запускает процесс кэширования.
        /// </summary>
        //private async void StartCaching()
        //{
        //    if (httpStream == null || _fileStream == null) return;

        //    await Task.Run(async () =>
        //    {
        //        try
        //        {
        //            var readHttpStream = httpStream.AsStreamForRead();
        //            var writeFileStream = _fileStream.AsStreamForWrite();

        //            int count = 0;
        //            long currentPosition = writeFileStream.Length;

        //            do
        //            {
        //                if (_isCanceled)
        //                {
        //                    await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
        //                    return;
        //                }

        //                var buffer = new byte[BUFFER_SIZE];
        //                count = readHttpStream.Read(buffer, 0, BUFFER_SIZE);

        //                writeFileStream.Position = currentPosition;
        //                writeFileStream.Write(buffer, 0, count);

        //                currentPosition = writeFileStream.Position;
        //                //fileStream.Size = (ulong)currentPosition;

        //                if (request != null)
        //                {
        //                    if (_byteOffset + BUFFER_SIZE <= _fileStream.Size) request.ReportSampleProgress(100);
        //                    else
        //                    {
        //                        uint percent = (uint)((_fileStream.Size - _byteOffset) / BUFFER_SIZE);
        //                        request.ReportSampleProgress(percent);
        //                    }
        //                }
        //            } while (readHttpStream.CanRead && count > 0);
                    
        //            string fileName;
        //            if (Track.OwnerID != 0 && Track.ID != 0) fileName = Track.OwnerID + "_" + Track.ID + "_1.mp3";
        //            else fileName = Track.Artist + "_" + Track.Title + "_1.mp3";

        //            await file.RenameAsync(fileName);
        //        }
        //        catch (Exception) { _isCanceled = true; }

        //        _isCompleted = true;
        //    });
        //}

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

                _byteOffset += count;
                _timeOffset = _timeOffset.Add(_sampleDuration);

                request.Sample = sample;
                return count;
            }
            catch (Exception) { return 0; }
        }

        private const int BUFFER_SIZE = SAMPLE_SIZE;
        private const int SAMPLE_SIZE = 1152;
        private const int BUFFERED_TIME_SECONDS = 2;

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
        private bool _isCanceled;
        private bool _isCompleted;
        private IMusicCacheService _musicCacheService;
    }
}
