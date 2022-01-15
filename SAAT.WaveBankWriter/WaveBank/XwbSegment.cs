namespace SAAT.WaveBankWriter {
    /// <summary>
    /// Data structure that represents a single segment of a WaveBank (XWB).
    /// </summary>
    internal struct XwbSegment {
        /// <summary>The length of the section.</summary>
        public int Length;

        /// <summary>The address offset detailing the start of the section.</summary>
        public int Offset;
    }
}
