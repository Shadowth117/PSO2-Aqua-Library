namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    public struct ARCHeader
    {
        public int fileSize;
        public int pof0Offset; //No POF0 tag, but same schema. Relative to 0x20, end of the ARC header
        public int pof0OffsetsSize; //Includes padding to 0x4. Sometimes there'll be data after this names of internal sections
        public int group1FileCount; //Names follow the pof0 sections

        public int group2FileCount;
        public int magic; //0100
        public int unkInt0;
        public int unkInt1;
    }
}
