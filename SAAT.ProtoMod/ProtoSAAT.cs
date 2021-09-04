using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

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
        private List<ICue> playList;
        private ICue activeTrack;
        private int index;

        /// <summary>
        /// Creates a new instance of the <see cref="ProtoSAAT"/> class, an implementation of <see cref="Mod"/>.
        /// </summary>
        public ProtoSAAT() {
            this.playList = new List<ICue>();

            this.index = -1;
        }

        /// <summary>
        /// SMAPI's entry point.
        /// </summary>
        /// <param name="helper">Implementation of SMAPI's Mod Helper.</param>
        public override void Entry(IModHelper helper) {
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.Input.ButtonPressed += this.OnKeyPressed;

            helper.ConsoleCommands.Add("gen_track_json", "Generates an example JSON file. Results will be tracks.json in the ProtoSAAT folder.", this.GenerateSampleJson);
        }

        /// <summary>
        /// Loads the content packs assigned to ProtoSAAT.
        /// </summary>
        private void LoadContentPacks() {
            var packs = this.Helper.ContentPacks.GetOwned();

            foreach (var pack in packs) {
                var jsonData = pack.ReadJsonFile<AudioTrack[]>("tracks.json");

                foreach (var track in jsonData) {
                    string path = Path.Combine(pack.DirectoryPath, track.Filepath);

                    var cue = this.audioApi.Load(track.Id, path, track.Category);

                    this.playList.Add(cue);
                }
            }
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

            // Lets not block the UI Thread. Spin off loading to another thread.
            var thread = new Thread(this.LoadContentPacks);
            thread.Start();
        }

        /// <summary>
        /// Event callback method that occurs when a key is pressed.
        /// </summary>
        /// <param name="sender">The caller.</param>
        /// <param name="args">The event arguments.</param>
        private void OnKeyPressed(object sender, ButtonPressedEventArgs args) {
            switch (args.Button) {
                // Stop
                case SButton.D1: 
                    StopCurrentTrack();
                    this.activeTrack = null;
                    this.index = -1;
                    break;

                // Forward on playlist
                case SButton.D2: 
                    StopCurrentTrack();
                    this.index = (this.index + 1) % this.playList.Count;
                    PlayNextTrack();
                    break;

                // Backward on playlist
                case SButton.D3: 
                    StopCurrentTrack();
                    this.index = this.index - 1 < 0 ? this.playList.Count - 1 : this.index - 1;
                    PlayNextTrack();
                    break;
            }
        }

        /// <summary>
        /// Command Callback that generates an example json file for SAAT.
        /// </summary>
        /// <param name="argc">Number of command arguments.</param>
        /// <param name="argv">The argument value(s).</param>
        private void GenerateSampleJson(string argc, string[] argv) {
            var tracks = new AudioTrack[2];

            tracks[0] = new AudioTrack("ExampleOne", "one.ogg", Category.Music);
            tracks[1] = new AudioTrack("ExampleTwo", "two.wav", Category.Sound);

            this.Helper.Data.WriteJsonFile("tracks.json", tracks);
        }

        /// <summary>
        /// Plays the next track according to <see cref="index"/>.
        /// </summary>
        private void PlayNextTrack() {
            this.activeTrack = this.playList[this.index];
            this.activeTrack.Play();
        }

        /// <summary>
        /// Stops the current track from playing.
        /// </summary>
        private void StopCurrentTrack() {
            if (this.index == -1) return;
            this.activeTrack.Stop(AudioStopOptions.Immediate);
        }
    }
}
