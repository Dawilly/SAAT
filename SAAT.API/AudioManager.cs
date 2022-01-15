using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework.Audio;
using NVorbis;

using StardewModdingAPI;
using StardewValley;

namespace SAAT.API
{
    /// <summary>
    /// Implementation of <see cref="IAudioManager"/>. Handles all operations regarding audio.
    /// </summary>
    public class AudioManager : IAudioManager
    {
        private readonly Dictionary<string, ICue> cueTable;
        private readonly Dictionary<string, Track> trackTable;

        //TO-DO: Implement an engine that handles memory management appropriately, instead of ad-hoc.
        //private readonly IAudioEngine engine;
        private readonly ISoundBank soundBank;
        private readonly IMonitor monitor;

        /// <summary>
        /// Creates a new instance of the <see cref="AudioManager"/> class.
        /// </summary>
        public AudioManager(IMonitor monitor)
        {
            this.cueTable = new Dictionary<string, ICue>();
            this.trackTable = new Dictionary<string, Track>();

            //this.engine = Game1.audioEngine;
            this.soundBank = Game1.soundBank;

            this.monitor = monitor;
        }

        /// <inheritdoc/>
        public ICue Load(string owner, string name, string path, Category category)
        {
            if (this.cueTable.ContainsKey(name))
            {
                return this.cueTable[name];
            }

            SoundEffect sfx;
            uint byteSize;

            try
            {
                sfx = AudioManager.LoadFile(path, out byteSize);
            }
            catch (Exception e)
            {
                this.monitor.Log($"Unable to load audio: {e.Message}\n{e.StackTrace}");
                return null;
            }

            // Am I being funny yet?
            var cueBall = new CueDefinition(name, sfx, (int)category);

            // Need to add the defition to the bank in order to generate a cue.
            this.soundBank.AddCue(cueBall);
            var cue = this.soundBank.GetCue(name);

            this.cueTable.Add(name, cue);

            var track = new Track {
                BufferSize = byteSize,
                Category = category,
                Id = name,
                Instance = cue,
                Filepath = path,
                Owner = owner
            };

            this.trackTable.Add(name, track);

            return cue;
        }

        /// <inheritdoc/>
        public void PrintMemoryAllocationInfo()
        {
            var subTotals = new Dictionary<string, uint>();

            string name = "Name";
            string size = "Size (In Bytes)";
            string owner = "Owner";

            this.monitor.Log($"##\t{name.PadRight(40)}{size.PadRight(40)}{owner}\t##", LogLevel.Info);

            foreach (var track in this.trackTable.Values)
            {
                if (!subTotals.ContainsKey(track.Owner))
                {
                    subTotals.Add(track.Owner, track.BufferSize);
                }
                else
                {
                    subTotals[track.Owner] += track.BufferSize;
                }

                string bufferSize = $"{Track.BufferSizeInKilo(track)} KB";
                this.monitor.Log($"  \t{track.Id.PadRight(40)}{bufferSize.PadRight(40)}{track.Owner}", LogLevel.Info);
            }

            uint total = 0;
            this.monitor.Log($"\n\n##\t {name.PadRight(40)}{size}\t##", LogLevel.Info);

            foreach (var kvp in subTotals)
            {
                total += kvp.Value;
                string bufferSize = $"{Track.BufferSizeInMega(kvp.Value)} MB";
                this.monitor.Log($"  \t{kvp.Key.PadRight(40)}{bufferSize}", LogLevel.Info);
            }

            this.monitor.Log($"Total Memory Usage: {Track.BufferSizeInMega(total)} MB", LogLevel.Info);
        }

        /// <summary>
        /// Loads an audio file into memory and creates a <see cref="SoundEffect"/> object to access
        /// the audio content.
        /// </summary>
        /// <param name="path">The path to the audio file.</param>
        /// <returns>A newly created <see cref="SoundEffect"/> object. <see cref="null"/> if it failed to load.</returns>
        private static SoundEffect LoadFile(string path, out uint byteSize)
        {
            byteSize = 0;

            var type = Utilities.ParseAudioExtension(path);

            using var stream = new FileStream(path, FileMode.Open);

            switch (type) {
                case AudioFileType.Wav:
                    return AudioManager.OpenWavFile(stream, out byteSize);

                case AudioFileType.Ogg:
                    return AudioManager.OpenOggFile(stream, out byteSize);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Loads the entire content of a .wav into memory.
        /// </summary>
        /// <param name="stream">The file stream pointing to the wav file.</param>
        /// <param name="byteSize">The number of bytes needed for the audio data.</param>
        /// <returns>A newly created <see cref="SoundEffect"/> object.</returns>
        private static SoundEffect OpenWavFile(FileStream stream, out uint byteSize)
        {
            byteSize = 0;

            // We're gonna peak at the number of bytes before we pass this off.
            using var reader = new BinaryReader(stream);
            long riffDataSize = 0;

            do {
                string chunkId = new string(reader.ReadChars(4));
                int chunkSize = reader.ReadInt32();

                switch (chunkId) {
                    case "RIFF":
                        // Set filesize and toss out "WAVE".
                        riffDataSize = chunkSize;
                        reader.ReadChars(4); 
                        break;

                    case "fmt ":
                        // Toss out.
                        reader.ReadBytes(chunkSize);
                        break;

                    case "data":
                        // Set byteSize, we're done.
                        byteSize = (uint)chunkSize;
                        break;

                    default:
                        reader.BaseStream.Seek((long)chunkSize, SeekOrigin.Current);
                        break;
                }
            } while (byteSize == 0 && reader.BaseStream.Position < riffDataSize);

            // Back to the top of the file.
            stream.Position = 0;

            return SoundEffect.FromStream(stream);
        }

        /// <summary>
        /// Loads the entire content of an .ogg into memory.
        /// </summary>
        /// <param name="stream">The file stream pointing to the ogg file.</param>
        /// <param name="byteSize">The number of bytes needed for the audio data.</param>
        /// <returns>A newly created <see cref="SoundEffect"/> object.</returns>
        private static SoundEffect OpenOggFile(FileStream stream, out uint byteSize)
        {
            using var reader = new VorbisReader(stream, true);

            // At the moment, we're loading everything in. If the number of samples is greater than int.MaxValue, bail.
            if (reader.TotalSamples > int.MaxValue)
            {
                throw new Exception("TotalSample overflow");
            }

            int totalSamples = (int)reader.TotalSamples;
            int sampleRate = reader.SampleRate;
            
            // SoundEffect.SampleSizeInBytes has a fault within it. In conjunction with a small amount of percision loss,
            // any decimal points are dropped instead of rounded up. For example: It will calculate the buffer size to be
            // 2141.999984, returning 2141. This should be 2142, as it violates block alignment below.
            int bufferSize = (int)Math.Ceiling(reader.TotalTime.TotalSeconds * (sampleRate * reader.Channels * 16d / 8d));
            byte[] buffer = new byte[bufferSize];
            float[] vorbisBuffer = new float[totalSamples];

            int sampleReadings = reader.ReadSamples(vorbisBuffer, 0, totalSamples);

            // This shouldn't occur. Check just incase and bail out if so.
            if (sampleReadings == 0)
            {
                throw new Exception("Unable to read samples from Ogg file.");
            }

            // Buffers within SoundEffect instances MUST be block aligned. By 2 for Mono, 4 for Stereo.
            int blockAlign = reader.Channels * 2;
            sampleReadings -= sampleReadings % blockAlign;

            // Must convert the audio data to 16-bit PCM, as this is the only format SoundEffect supports.
            for (int i = 0; i < sampleReadings; i++)
            {
                short sh = (short)Math.Max(Math.Min(short.MaxValue * vorbisBuffer[i], short.MaxValue), short.MinValue);
                buffer[i * 2] = (byte)(sh & 0xff);
                buffer[i * 2 + 1] = (byte)((sh >> 8) & 0xff);
            }

            byteSize = (uint)buffer.Length;

            return new SoundEffect(buffer, sampleRate, (AudioChannels)reader.Channels);
        }
    }
}
