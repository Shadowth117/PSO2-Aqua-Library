namespace AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData
{
    //Contains information about the triangle strip sets
    public struct PSET
    {
        public int tag; //0xC6, type 0x9 //0x2100 in classic, 0x1000 in aqp 0xC33. May actually be two int16s since it's 0x140 for the second two bytes in .trp. May not mean much
        public int faceGroupCount; //0xBB, type 0x9 //Number of face groups. In classic, this seems to ALWAYS be 0x1. However in NGS, this seems to be used. These groups seem to link
                                   //faces to specific groups for some purpose. Notably, the weird Vector3 arrays linked in objc.

        public int faceCountOffset; //Offset for the beginning of the correlating face data structure. 
        public int psetFaceCount; //0xBC, type 0x9 //This is actually the same count as the one at the offset above. Perhaps one would be used for triangle count and one would be used for true face count with another faceType above?

        public int faceOffset; //This is an offset to the beginning of the strip data. Unknown purpose in 0xC33 variant
        public int stripStartCount; //0xC5, type 0x9 //Unused in classic. Provides starting id in global strip list for 0xC33.

        public PSET(Dictionary<int, object> psetRaw)
        {
            tag = (int)psetRaw[0xC6];
            faceGroupCount = (int)psetRaw[0xBB];
            psetFaceCount = (int)psetRaw[0xBC];
            stripStartCount = (int)psetRaw[0xC5];

            //Unused in vtbf
            faceCountOffset = 0;
            faceOffset = 0;
        }
    }
}
