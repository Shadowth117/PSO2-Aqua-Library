using System.Collections.Generic;
using Reloaded.Memory.Streams;

namespace AquaModelLibrary.Extra.Ninja.BillyHatcher
{
    /// <summary>
    /// ARC is similar to something like PSO2 NIFL where it's more of a wrapper for other formats. .lnd also uses a variant of this at times.
    /// </summary>
    public class ARC
    {
        public List<ARCFileRef> fileReferences = new List<ARCFileRef>();
        public List<string> fileNames = new List<string>();
        public List<byte[]> files = new List<byte[]>();
        public ARC() { }

        public ARC(BufferedStreamReader sr)
        {

        }

        public struct ARCHeader
        {
            public int fileSize;
            public int pof0Offset; //No POF0 tag, but same schema. Relative to 0x20, end of the ARC header
            public int pof0OffsetsSize; //Includes padding to 0x4. Sometimes there'll be data after this names of internal sections
            public int fileCount; //Names follow the pof0 sections

            public int unkCount;
            public int magic; //0100
            public int unkInt0;
            public int unkInt1;
        }

        //ARCLND
        /// <summary>
        /// These are placed directly after POF0 data. Not all ARCs have these.
        /// </summary>
        public struct ARCFileRef
        {
            public int modelOffset;
            public int relativeNameOffset;
        }
    }
}
