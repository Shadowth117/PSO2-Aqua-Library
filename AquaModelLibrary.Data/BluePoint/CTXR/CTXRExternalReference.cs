using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers.Writers;
using System.Text;

namespace AquaModelLibrary.Data.BluePoint.CTXR
{
    public class CTXRExternalReference
    {
        public short mipLevel;
        public short unkSht0;
        public int bufferSize;
        public int lowerMipBufferSize;
        public string externalMipReference;

        public CTXRExternalReference() { }
        public CTXRExternalReference(BufferedStreamReaderBE<MemoryStream> sr)
        {
            mipLevel = sr.ReadBE<short>();
            unkSht0 = sr.ReadBE<short>();
            bufferSize = sr.ReadBE<int>();
            lowerMipBufferSize = sr.ReadBE<int>();
            externalMipReference = sr.ReadCStringSeek();
        }

        public byte[] GetBytes()
        {
            ByteListWriter outBytes = new();
            outBytes.AddValue(mipLevel);
            outBytes.AddValue(unkSht0);
            outBytes.AddValue(bufferSize);
            outBytes.AddValue(lowerMipBufferSize);
            outBytes.AddValue(Encoding.ASCII.GetBytes(externalMipReference));
            outBytes.Add(0);

            return outBytes.ToArray();
        }
    }
}
