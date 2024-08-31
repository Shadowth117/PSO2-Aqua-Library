using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.CustomRoboBattleRevolution.Model.Common
{
    public class CRBRKeyframe
    {
        public List<byte> keyData = new List<byte>();

        public int nextFrameOffset;
        /// <summary>
        /// Key data will have one byte at the start, usually 11 or 13, possibly flags defining what it is. 
        /// This amount + padding to 0x4 fills the rest
        /// </summary>
        public int dataSize;
        public int int_08;
        public byte keyTime;
        public byte bt_0D;
        public byte bt_0E;
        public byte bt_0F;

        public int keyDataOffset;

        public CRBRKeyframe() { }

        /// <summary>
        /// Consecutive keyframes are stored in the keyframe root to keep them together so we don't have to recursively navigate them for access.
        /// Therefore, they're not read in the constructor
        /// </summary>
        public CRBRKeyframe(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            nextFrameOffset = sr.ReadBE<int>();
            dataSize = sr.ReadBE<int>();
            int_08 = sr.ReadBE<int>();
            keyTime = sr.ReadBE<byte>();
            bt_0D = sr.ReadBE<byte>();
            bt_0E = sr.ReadBE<byte>();
            bt_0F = sr.ReadBE<byte>();

            keyDataOffset = sr.ReadBE<int>();

            if(keyDataOffset != 0)
            {
                keyData.AddRange(sr.ReadBytes(keyDataOffset + offset, dataSize));
            }
        }
    }
}
