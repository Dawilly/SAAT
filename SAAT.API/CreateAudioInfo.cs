namespace SAAT.API
{
    /// <summary>
    /// Parameter data for creating an audio cue.
    /// </summary>
    public struct CreateAudioInfo
    {
        public string Name;
        public Category Category;
        public bool Loop;
    }
}
