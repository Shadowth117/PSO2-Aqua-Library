namespace AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.SHADData
{
    //Struct containing details for the shadExtra area, including a count needed to read it.
    public struct SHADDetail
    {
        public int unk0;
        public int shadExtraCount; //Details how many entries will be in the associated shadExtra
        public int unk1;
        public int unkCount0; //A count for something within the shadExtra area, presumably

        public int unk2;
        public int unkCount1; //Another count. Seemingly shadExtraCount - unkCount0; the other types of entries in the shadExtra
        public int unk3;
        public int unk4;

        public SHADDetail(int _unk0, int _shadExtraCount, int _unk1, int _unkCount0, int _unk2, int _unkCount1, int _unk3, int _unk4)
        {
           unk0 = _unk0;
           shadExtraCount = _shadExtraCount;
           unk1 = _unk1;
           unkCount0 = _unkCount0;
           unk2 = _unk2;
           unkCount1 = _unkCount1;
           unk3 = _unk3;
           unk4 = _unk4;
        }
    }
}
