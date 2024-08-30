namespace AquaModelLibrary.Data.CustomRoboBattleRevolution.Model
{
    public class CRBRCharacterModelHeader : CRBRModelHeader
    {
        /// <summary>
        /// All offsets in the file are offset by this initial offset
        /// </summary>
        //public int offset;
        public int fileSize;
        public int offset0;
        public int offset1;

        public int offset2;
        public int unkCount0;
        public int offsetTableOffset;
        public int offsetTableCount;

        public int offset3;
        public int int_24;

        //Probably padding to 0x20 from here
        public int int_28;
        public int int_2C;

        public int int_30;
        public int int_34;
        public int int_38;
        public int int_3C;
    }
}
