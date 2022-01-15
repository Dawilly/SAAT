using System;
using System.IO;

namespace SAAT.API {
    internal static class Utilities
    {
        public static AudioFileType ParseAudioExtension(string path)
        {
            string ext = Path.GetExtension(path).ToLower();

            switch (ext) {
                case ".wav":
                    return AudioFileType.Wav;

                case ".ogg":
                    return AudioFileType.Ogg;

                default:
                    throw new ArgumentException("Audio File type not supported");
            }
        }
    }
}
