using StardewValley;

namespace SAAT.API {
    /// <summary>
    /// API reference that provides access to the audio manager and its functionality.
    /// </summary>
    public interface IAudioManager {
        /// <summary>
        /// Loads an audio asset.
        /// </summary>
        /// <param name="owner">The unique mod identification, who is loading said audio asset.</param>
        /// <param name="name">The name of the audio asset.</param>
        /// <param name="path">The file path to the audio asset.</param>
        /// <param name="category">The category of the audio asset.</param>
        /// <returns>A newly created <see cref="ICue"/> instance.</returns>
        ICue Load(string owner, string name, string path, Category category);

        /// <summary>
        /// Details the memory allocations of audio, by each individual audio track that is
        /// currently in memory.
        /// </summary>
        void PrintMemoryAllocationInfo();
    }
}
