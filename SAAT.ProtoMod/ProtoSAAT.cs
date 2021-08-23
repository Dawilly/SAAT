using System;
using System.IO;

using Microsoft.Xna.Framework.Audio;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace SAAT.ProtoMod {
    /// <summary>
    /// Implementation of <see cref="Mod"/>, providing audio functionality for the proof of concept.
    /// </summary>
    public class ProtoSAAT : Mod {
        private ICue sinWaveCue;
        private SoundEffectInstance sinWaveSfx;

        /// <summary>
        /// Entry Point.
        /// </summary>
        /// <param name="helper">Implementation of SMAPI's Mod Helper.</param>
        public override void Entry(IModHelper helper) {
            helper.Events.GameLoop.GameLaunched += this.LoadAudioAssets;
            helper.Events.Input.ButtonPressed += this.OnKeyPressed;
        }

        /// <summary>
        /// Event callback method that loads all audio assets prior to the first Game.Update tick.
        /// </summary>
        /// <param name="sender">The caller.</param>
        /// <param name="e">The event arguments.</param>
        private void LoadAudioAssets(object sender, GameLaunchedEventArgs e) {
            this.sinWaveCue = ProtoSAAT.CreateSoundEffect(Path.Combine(this.Helper.DirectoryPath, "assets", "SinWave.wav"), out var sfxInt);

            sfxInt.IsLooped = true;
            this.sinWaveSfx = sfxInt;
        }

        /// <summary>
        /// Event callback method that occurs when a key is pressed.
        /// </summary>
        /// <param name="sender">The caller.</param>
        /// <param name="args">The event arguments.</param>
        private void OnKeyPressed(object sender, ButtonPressedEventArgs args) {
            var key = args.Button;

            switch (key) {
                case SButton.D1:
                    this.sinWaveCue.Stop(AudioStopOptions.Immediate);
                    this.sinWaveSfx.Stop(true);
                    break;
                case SButton.D2:
                    this.sinWaveCue.Play();
                    break;
                case SButton.D3:
                    this.sinWaveSfx.Play();
                    break;
                case SButton.D4:
                    break;
                case SButton.D5:
                    break;
            }
        }

        /// <summary>
        /// Creates <see cref="SoundEffect"/> objects and stores it into a <see cref="ICue"/> instance.
        /// </summary>
        /// <param name="path">The path to the .wav audio file.</param>
        /// <param name="sfxInt">The <see cref="SoundEffectInstance"/> instance containing the <see cref="SoundEffect"/>.</param>
        /// <returns>A newly created <see cref="ICue"/> instance.</returns>
        private static ICue CreateSoundEffect(string path, out SoundEffectInstance sfxInt) {
            SoundEffect sfx;

            using (var stream = new FileStream(path, FileMode.Open)) {
                sfx = SoundEffect.FromStream(stream);
            }

            // A little awkward, but this is the instance that can perform loops.
            sfxInt = sfx.CreateInstance();

            var category = Game1.audioEngine.GetCategoryIndex("Sound");
            var cueBall = new CueDefinition("GiveMeASin", sfx, category);

            Game1.soundBank.AddCue(cueBall);

            return Game1.soundBank.GetCue("GiveMeASin");
        }
    }
}
