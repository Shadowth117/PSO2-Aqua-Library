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

        /// <summary>
        /// Get full calculated variable length
        /// </summary>
        public int GetTrueLength()
        {
            return length + (lengthAddition - 1) * 0x80;
        }

        /// <summary>
        /// Determine the number of bytes this length data is composed of
        /// </summary>
        public int GetCLengthStructSize()
        {
            return length >= 0x80 ? 2 : 1;
        }
    }
}
