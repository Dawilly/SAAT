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
        }

        /// <inheritdoc/>
        public override object GetApi() {
            return this.audioManager;
        }
    }
}
