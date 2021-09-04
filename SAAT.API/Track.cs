using System;

using StardewValley;

namespace SAAT.API {
    /// <summary>
    /// Data container class that details an audio track.
    /// </summary>
    internal class Track {
        /// <summary>Gets or sets the size of the audio data buffer, in bytes.</summary>
        public uint BufferSize { get; set; }

        /// <summary>Gets or sets the SoundBank category the track belongs to.</summary>
        public Category Category { get; set; }

        /// <summary>Gets or sets the identification of the track.</summary>
        public string Id { get; set; }

        /// <summary>Gets or sets the cue instance of the audio track.</summary>
        public ICue Instance { get; set; }

        /// <summary>Gets or sets the relative file path of the audio file.</summary>
        public string Filepath { get; set; }

        /// <summary>Gets or sets the unique identification of the owner (mod).</summary>
        public string Owner { get; set; }

        public static double BufferSizeInKilo(Track track) {
            return Math.Round((double)track.BufferSize / 1024, 2);
        }

        public static double BufferSizeInMega(Track track) {
            return Math.Round((double)track.BufferSize / 1048576, 2);
        }
    }
}
