using StardewModdingAPI;

namespace SAAT.API {
    /// <summary>
    /// Implementation of <see cref="Mod"/> that provides access to <see cref="IAudioManager"/> for other mods.
    /// </summary>
    public class SAAT : Mod {
        private IAudioManager audioManager;

        /// <inheritdoc/>
        public override void Entry(IModHelper helper) {
            this.audioManager = new AudioManager(this.Monitor);

            helper.ConsoleCommands.Add("audio_allocs", "Prints all memory allocations for audio, by the individual tracks.", this.ListMallocs);
        }

        /// <inheritdoc/>
        public override object GetApi() {
            return this.audioManager;
        }

        /// <summary>
        /// Callback method for the SMAPI Command.
        /// </summary>
        /// <param name="argc"></param>
        /// <param name="argv"></param>
        private void ListMallocs(string argc, string[] argv) {
            // This should be int, string[]. Reee.
            this.audioManager.PrintMemoryAllocationInfo();
        }
    }
}
