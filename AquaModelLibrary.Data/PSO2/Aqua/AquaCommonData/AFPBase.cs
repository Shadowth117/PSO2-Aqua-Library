using AquaModelLibrary.Data.DataTypes.SetLengthStrings;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaCommonData
{
    //Used at the junction between two models as well as in AFPMain
    public unsafe struct AFPBase
    {
        public PSO2String fileName; //AQO filename
        public int paddingOffset; //Offset to padding, 0x10 from NEND start in NIFL
        public int afpBaseSize; 
        public int totalSize; //Size of file with afpBase and end padding
        public int fileTypeCString; //aqo or tro for models. aqv or trv for texture/uv anims. The game doesn't seem to care what's here though, frankly.

        public byte[] GetBytes()
        {
            List<byte> outBytes = new List<byte>();
            outBytes.AddRange(fileName.GetBytes());
            outBytes.AddRange(BitConverter.GetBytes(paddingOffset));
            outBytes.AddRange(BitConverter.GetBytes(afpBaseSize));
            outBytes.AddRange(BitConverter.GetBytes(totalSize));
            outBytes.AddRange(BitConverter.GetBytes(fileTypeCString));

            return outBytes.ToArray();
        }
    }
}
