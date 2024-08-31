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
    }
}
