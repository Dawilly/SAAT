namespace SAAT.API {
    /// <summary>
    /// Enumeration that represents the audio category a track belongs to.
    /// 
    /// Setting a track to a category allows the game settings to control the volume
    /// output based on the category's setting.
    /// </summary>
    public enum Category : int
    {
        Global = 0,
        Default = 1,

        /// <summary>A background music type.</summary>
        /// <remarks>All audio tracks, once heard by the player, are listed in the jukebox.</remarks>
        Music = 2,

        /// <summary>A general sound effect type. (I.e. sword swinging, bomb explosion, etc).</summary>
        Sound = 3,

        /// <summary>A ambient sound effect type. (I.e. Birds, wind, etc).</summary>
        Ambient = 4,

        /// <summary>A sound effect that resembles a particular foot-step.</summary>
        Footsteps = 5
    }

    internal enum AudioFileType
    {
        Wav,
        Ogg
    }
}
