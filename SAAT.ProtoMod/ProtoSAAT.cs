using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework.Audio;

using SAAT.API;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace SAAT.ProtoMod {
    /// <summary>
    /// Implementation of <see cref="Mod"/>, providing proof of concept for audio functionality.
    /// </summary>
    public class ProtoSAAT : Mod {
        private const string ApiId = "Pickles.SAAT.API";

        private IAudioManager audioApi;
        private ICue sinWaveCue;
        private ICue sinOggCue;
        private string assetPath;

        /// <summary>
        /// SMAPI's entry point.
        /// </summary>
        /// <param name="helper">Implementation of SMAPI's Mod Helper.</param>
        public override void Entry(IModHelper helper) {
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Input.ButtonPressed += this.OnKeyPressed;

            this.assetPath = Path.Combine(helper.DirectoryPath, "assets");
        }

        /// <summary>
        /// Event callback method that loads all audio assets prior to the first Game.Update tick.
        /// </summary>
        /// <param name="sender">The caller.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e) {
            if (this.audioApi == null) {
                this.audioApi = this.Helper.ModRegistry.GetApi<IAudioManager>(ProtoSAAT.ApiId);
            }

            this.sinWaveCue = this.audioApi.Load("sinWav", Path.Combine(this.assetPath, "SinWave.wav"), Category.Sound);
            this.sinOggCue = this.audioApi.Load("sinOgg", Path.Combine(this.assetPath, "SinWave.ogg"), Category.Sound);
        }

        /// <summary>
        /// Event callback method that occurs when a key is pressed.
        /// </summary>
        /// <param name="sender">The caller.</param>
        /// <param name="args">The event arguments.</param>
        private void OnKeyPressed(object sender, ButtonPressedEventArgs args) {
            var key = args.Button;

            switch (key) {
                case SButton.D1: // Stop everything! Now!
                    this.sinWaveCue.Stop(AudioStopOptions.Immediate);
                    this.sinOggCue.Stop(AudioStopOptions.Immediate);
                    break;
                case SButton.D2: 
                    this.sinWaveCue.Play();
                    break;
                case SButton.D3:
                    this.sinOggCue.Play();
                    break;
                case SButton.D4:
                    break;
                case SButton.D5:
                    break;
            }
        }
    }
}
