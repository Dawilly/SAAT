using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAAT.WaveBankWriter {
    internal class XwbFile {
        private const int LatestVersion = 46;
        private const int LatestSegmentCount = 5;

        public readonly string Magic = "WBND";

        public XwbHeader Header { get; }

        public XwbData Data { get; }

        public XwbFile() {
            this.Header = new XwbHeader {
                Version = XwbFile.LatestVersion,
                Segments = new XwbSegment[XwbFile.LatestSegmentCount]
            };


        }
    }
}
