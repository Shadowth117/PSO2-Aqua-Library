using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BluePoint
{
    public class CLength
    {
        private byte length;
        public byte lengthAddition = 1;

        public CLength() { }
        public CLength(BufferedStreamReaderBE<MemoryStream> sr)
        {
            length = sr.Read<byte>();
            if (length >= 0x80)
            {
                lengthAddition = sr.Read<byte>();
            }
        }

        public int GetTrueLength()
        {
            return length + (lengthAddition - 1) * 0x80;
        }
    }
}
