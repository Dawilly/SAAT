using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework.Audio;
using NVorbis;

using StardewModdingAPI;
using StardewValley;

namespace SAAT.API {
    /// <summary>
    /// Implementation of <see cref="IAudioManager"/>. Handles all operations regarding audio.
    /// </summary>
    public class AudioManager : IAudioManager {
        private readonly Dictionary<string, ICue> cueTable;

        private readonly IAudioEngine engine;
        private readonly ISoundBank soundBank;
        private readonly IMonitor monitor;

        /// <summary>
        /// Creates a new instance of the <see cref="AudioManager"/> class.
        /// </summary>
        public AudioManager(IMonitor monitor) {
            this.cueTable = new Dictionary<string, ICue>();

            this.engine = Game1.audioEngine;
            this.soundBank = Game1.soundBank;

            this.monitor = monitor;
        }

        /// <inheritdoc/>
        public ICue Load(string name, string path, Category category) {
            if (this.cueTable.ContainsKey(name)) {
                return this.cueTable[name];
            }

            var sfx = this.LoadFile(path);

            // Am I being funny yet?
            var cueBall = new CueDefinition(name, sfx, (int)category);

            this.soundBank.AddCue(cueBall);

            return this.soundBank.GetCue(name);
        }

        /// <summary>
        /// Loads an audio file into memory and creates a <see cref="SoundEffect"/> object to access
        /// the audio content.
        /// </summary>
        /// <param name="path">The path to the audio file.</param>
        /// <returns>A newly created <see cref="SoundEffect"/> object. <see cref="null"/> if it failed to load.</returns>
        private SoundEffect LoadFile(string path) {
            try {
                var type = Utilities.ParseAudioExtension(path);

                using var stream = new FileStream(path, FileMode.Open);

                switch (type) {
                    case AudioFileType.Wav:
                        return SoundEffect.FromStream(stream);
                    case AudioFileType.Ogg:
                        return AudioManager.OpenOggFile(stream);
                }
            } catch (Exception e) {
                this.monitor.Log($"Unable to load audio: {e.Message}\n{e.StackTrace}");
            }

            return null;
        }

        private static SoundEffect OpenOggFile(FileStream stream) {
            using var reader = new VorbisReader(stream, true);

            // At the moment, we're loading everything in. If the number of samples is greater than int.MaxValue, bail.
            if (reader.TotalSamples > int.MaxValue) {
                throw new Exception("TotalSample overflow");
            }

            int totalSamples = (int)reader.TotalSamples;
            int sampleRate = reader.SampleRate;
            var channels = (AudioChannels)reader.Channels;

            float[] vorbisBuffer = new float[totalSamples];
            byte[] buffer = new byte[SoundEffect.GetSampleSizeInBytes(reader.TotalTime, sampleRate, channels)];

            reader.ReadSamples(vorbisBuffer, 0, totalSamples);

            // TO-DO: Fix
            // Turns out we need to convert everything to uncompressed 16-bit PCM.
            // This doesn't properly convert. We can't throw float points into the data set. 
            for (int i = 0; i < totalSamples; i++) {
                int count = Math.Min(buffer.Length - (i * sizeof(float)), sizeof(float));
                var segment = new ArraySegment<byte>(buffer, i * sizeof(float), count);
                BitConverter.TryWriteBytes(segment, vorbisBuffer[i]);
            }

            return new SoundEffect(buffer, sampleRate, channels);
        }
    }
}
