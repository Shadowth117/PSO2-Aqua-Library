using AquaModelLibrary.Helpers.Readers;
using System.Runtime.CompilerServices;

namespace AquaModelLibrary.Data.BluePoint.CGPR
{
    public class CGPRMetadata
    {
        public BPEra era;

        public byte[] mainGuid;
        public BPStringMini name = null;
        public int unkInt0;
        public List<CGPRMetadataProperty> properties = new List<CGPRMetadataProperty>();
        public int unkInt1;

        public class CGPRMetadataProperty
        {
            public byte[] guid = null;
            public int value;
        }
        public CGPRMetadata() { }
        public CGPRMetadata(BufferedStreamReaderBE<MemoryStream> sr, BPEra newEra)
        {
            Read(sr);
        }

        private void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            mainGuid = sr.ReadBytesSeek(0x10);
            name = new BPStringMini(sr, era);
            unkInt0 = sr.Read<int>();
            var count = sr.Read<int>();
            for(int i = 0; i < count; i++)
            {
                properties.Add(new CGPRMetadataProperty() { guid = sr.ReadBytesSeek(0x10), value = sr.Read<int>()} );
            }
            unkInt1 = sr.Read<int>();
        }
    }
}
