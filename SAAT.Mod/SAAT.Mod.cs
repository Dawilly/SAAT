﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using Microsoft.Xna.Framework.Audio;

using SAAT.API;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

using SmapiMod = StardewModdingAPI.Mod;

namespace SAAT.Mod {
    /// <summary>
    /// Implementation of <see cref="StardewModdingAPI.Mod"/>, providing proof of concept for audio functionality.
    /// </summary>
    public class SAATMod : SmapiMod
    {
        private const string ApiId = "ZeroMeters.SAAT.API";
        private const string ContentPackFilename = "tracks.json";

        private IAudioManager audioApi;
        private List<ICue> playList;
        private ICue activeTrack;
        private int index;

        public bool DebugMode { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="SAATMod"/> class, an implementation of <see cref="StardewModdingAPI.Mod"/>.
        /// </summary>
        public SAATMod()
        {
            this.playList = new List<ICue>();

            this.index = -1;

            this.DebugMode = false;
        }

        /// <summary>
        /// SMAPI's entry point.
        /// </summary>
        /// <param name="helper">Implementation of SMAPI's Mod Helper.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;

            if (this.DebugMode)
            {
                helper.Events.Input.ButtonPressed += this.OnKeyPressed;
            }

            helper.ConsoleCommands.Add("gen_track_json", "Generates an example JSON file. Results will be tracks.json in the SAAT.Mod folder.", this.GenerateSampleJson);
            helper.ConsoleCommands.Add("setdebug", "Sets the debugging state for SAAT.Mod, enabling playlist control for audio tracks.", this.EnableDebug);
        }

        /// <summary>
        /// Loads the content packs assigned to SAATMod.
        /// </summary>
        private void LoadContentPacks()
        {
            var packs = this.Helper.ContentPacks.GetOwned();

            foreach (var pack in packs)
            {
                var jsonData = pack.ReadJsonFile<AudioTrack[]>(SAATMod.ContentPackFilename);

                foreach (var track in jsonData)
                {
                    string path = Path.Combine(pack.DirectoryPath, track.Filepath);

                    var cue = this.audioApi.Load(pack.Manifest.UniqueID, track.Id, path, track.Category);

                    this.playList.Add(cue);
                }
            }
        }

        /// <summary>
        /// Event callback method that loads all audio assets prior to the first Game.Update tick.
        /// </summary>
        /// <param name="sender">The caller.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            if (this.audioApi == null)
            {
                this.audioApi = this.Helper.ModRegistry.GetApi<IAudioManager>(SAATMod.ApiId);
            }

            // Lets not block the UI/Main Thread. Spin off loading to another thread.
            var thread = new Thread(this.LoadContentPacks);
            thread.Start();
        }

        /// <summary>
        /// Event callback method that occurs when a key is pressed.
        /// </summary>
        /// <param name="sender">The caller.</param>
        /// <param name="args">The event arguments.</param>
        private void OnKeyPressed(object sender, ButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                // Stop
                case SButton.D1: 
                    this.StopCurrentTrack();
                    this.activeTrack = null;
                    this.index = -1;
                    break;

                // Forward on playlist
                case SButton.D2: 
                    this.StopCurrentTrack();
                    this.index = (this.index + 1) % this.playList.Count;
                    this.PlayNextTrack();
                    break;

                // Backward on playlist
                case SButton.D3: 
                    this.StopCurrentTrack();
                    this.index = this.index - 1 < 0 ? this.playList.Count - 1 : this.index - 1;
                    this.PlayNextTrack();
                    break;
            }
        }

        /// <summary>
        /// Command callback that enables/disables debug mode.
        /// </summary>
        /// <param name="command">The called command.</param>
        /// <param name="argv">The argument value(s).</param>
        private void EnableDebug(string command, string[] argv)
        {
            if (argv.Length < 1)
            {
                this.Monitor.Log("Insufficient command arguments. Syntax: setdebug <value>. Where <value> is either true or false.", LogLevel.Info);
                return;
            }

            if (!bool.TryParse(argv[0], out bool results))
            {
                this.Monitor.Log("Unable to determine argument. Syntax: setdebug <value>. Where <value> is either true or false.", LogLevel.Info);
                return;
            }

            if (results == this.DebugMode)
            {
                return;
            }

            if (results)
            {
                this.Helper.Events.Input.ButtonPressed += this.OnKeyPressed;
            }
            else
            {
                this.Helper.Events.Input.ButtonPressed -= this.OnKeyPressed;
            }

            this.DebugMode = results;
        }

        /// <summary>
        /// Command callback that generates an example json file for SAAT.
        /// </summary>
        /// <param name="command">The called command.</param>
        /// <param name="argv">The argument value(s).</param>
        private void GenerateSampleJson(string command, string[] argv)
        {
            var tracks = new AudioTrack[2];

            tracks[0] = new AudioTrack("ExampleOne", "one.ogg", Category.Music);
            tracks[1] = new AudioTrack("ExampleTwo", "two.wav", Category.Sound);

            this.Helper.Data.WriteJsonFile("tracks.json", tracks);
        }

        /// <summary>
        /// Plays the next track according to <see cref="SAATMod.index"/>.
        /// </summary>
        private void PlayNextTrack()
        {
            this.activeTrack = this.playList[this.index];

            this.activeTrack.Play();
        }

        /// <summary>
        /// Stops the current track from playing.
        /// </summary>
        private void StopCurrentTrack()
        {
            if (this.index == -1) return;

            this.activeTrack.Stop(AudioStopOptions.Immediate);
        }
    }
}