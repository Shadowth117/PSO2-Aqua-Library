namespace AquaModelLibrary.Data.Nova.NOD
{
    public struct NODHeader
    {
        public int magic;
        public int int_04;
        public int entityCount0;
        public int entityCount1;

        public int entityCount2;
        public int entityOffset0; //Should always be 0x2C
        public int entityOffset1;
        public int entityOffset2;

        public int int_20;
        public int offset_24;
        public int offset_28;
    }
}
