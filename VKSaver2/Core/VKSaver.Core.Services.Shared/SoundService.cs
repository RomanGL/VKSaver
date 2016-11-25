using SharpDX.Multimedia;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VKSaver.Core.Services.Interfaces;
using Windows.Storage;

namespace VKSaver.Core.Services
{
    /// <summary>
    /// Предсталяет сервис для проигрывания звуковых эффектов в формате WAV.
    /// </summary>
    public sealed class SoundService : ISoundService
    {
        private readonly Dictionary<string, AudioBufferAndMetaData> cachedBuffers;
        private readonly MasteringVoice masteringVoice;
        private readonly XAudio2 xAudio;
        private readonly object lockObject;

        /// <summary>
        /// Инициализирует новый экзепляр класса <see cref="SoundService"/>.
        /// </summary>
        public SoundService()
        {
            cachedBuffers = new Dictionary<string, AudioBufferAndMetaData>();
            lockObject = new object();

            xAudio = new XAudio2();
            masteringVoice = new MasteringVoice(xAudio);
            masteringVoice.SetVolume(1, 0);
            xAudio.StartEngine();
        }

        /// <summary>
        /// Воспроизвести звуковой эффект.
        /// </summary>
        /// <param name="source">Путь к WAV файлу.</param>
        /// <param name="volume">Громкость (-224 <> 224)</param>
        public async Task PlaySound(string source, float volume = 1)
        {
            if (volume < -224 || volume > 224)
                throw new ArgumentOutOfRangeException("volume");

            var buffer = await GetBuffer(source);
            var sourceVoice = new SourceVoice(xAudio, buffer.WaveFormat, true);
            sourceVoice.SetVolume(volume, XAudio2.CommitNow);
            sourceVoice.SubmitSourceBuffer(buffer, buffer.DecodedPacketsInfo);
            sourceVoice.Start();
        }

        /// <summary>
        /// Возвращает аудио буфер для файла.
        /// </summary>
        /// <param name="source">Путь к файлу.</param>
        private async Task<AudioBufferAndMetaData> GetBuffer(string source)
        {
            if (cachedBuffers.ContainsKey(source)) return cachedBuffers[source];

            var stream = (await (await StorageFile.GetFileFromApplicationUriAsync(new Uri(source)))
                        .OpenReadAsync()).AsStreamForRead();

            lock (lockObject)
            {
                var soundstream = new SoundStream(stream);
                var buffer = new AudioBufferAndMetaData
                {
                    Stream = soundstream.ToDataStream(),
                    AudioBytes = (int)soundstream.Length,
                    Flags = BufferFlags.EndOfStream,
                    WaveFormat = soundstream.Format,
                    DecodedPacketsInfo = soundstream.DecodedPacketsInfo
                };

                cachedBuffers[source] = buffer;
                return buffer;
            }
        }

        /// <summary>
        /// Представляет аудио буфер с метаданными файла.
        /// </summary>
        private sealed class AudioBufferAndMetaData : AudioBuffer
        {
            /// <summary>
            /// Данные о формате.
            /// </summary>
            public WaveFormat WaveFormat { get; set; }
            /// <summary>
            /// Декодированная информация о пакетах.
            /// </summary>
            public uint[] DecodedPacketsInfo { get; set; }
        }

    }
}
