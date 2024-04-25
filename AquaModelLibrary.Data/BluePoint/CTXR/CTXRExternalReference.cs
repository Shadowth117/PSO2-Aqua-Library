using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BluePoint.CTXR
{
    public class CTXRExternalReference
    {
        public short mipLevel;
        public short unkSht0;
        public int bufferSize;
        public short unkSht1;
        public short unkSht2;
        public byte unkBt0;
        public string externalMipReference;

        public CTXRExternalReference() { }
        public CTXRExternalReference(BufferedStreamReaderBE<MemoryStream> sr)
        {
            mipLevel = sr.ReadBE<short>();
            unkSht0 = sr.ReadBE<short>();
            bufferSize = sr.ReadBE<int>();
            unkSht1 = sr.ReadBE<short>();
            unkSht2 = sr.ReadBE<short>();
            externalMipReference = sr.ReadCStringSeek();
        }
    }
}
