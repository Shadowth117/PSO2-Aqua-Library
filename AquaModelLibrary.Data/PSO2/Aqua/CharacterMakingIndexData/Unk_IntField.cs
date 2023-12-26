using AquaModelLibrary.Helpers;

namespace AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData
{
    //This may be color related. But I have no idea what it's supposed to do.
    public unsafe struct Unk_IntField
    {
        public fixed int unkIntField[0x79];

        public byte[] GetBytes()
        {
            return DataHelpers.ConvertStruct(this);
        }
    }
}
