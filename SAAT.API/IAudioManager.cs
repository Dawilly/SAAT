using Microsoft.Xna.Framework.Audio;

using StardewValley;

namespace SAAT.API {
    /// <summary>
    /// API reference that provides access to the audio manager and its functionality.
    /// </summary>
    public interface IAudioManager {
        /// <summary>
        /// Loads an audio asset.
        /// </summary>
        /// <param name="name">The name of the audio.</param>
        /// <param name="path">The path to the audio.</param>
        /// <returns>A newly created <see cref="SoundEffectInstance"/> with the audio data.</returns>
        ICue Load(string name, string path, Category category);
    }
}
