using AquaModelLibrary.Data.PSO2.Aqua.SetLengthStrings;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaCommonData
{
    //Used at the junction between two models as well as in AFPMain
    public unsafe struct AFPBase
    {
        public PSO2String fileName; //AQO filename
        public int paddingOffset; //Offset to padding, 0x10 from NEND start in NIFL
        public int afpOffsetAddition; //Amount added to offsets by the AFP? Always seems to be 0x30. If using afp however, 0x40 should be added, so perhaps this is something else.
        public int paddingOffsetAdjusted; //Offset to padding + 0x40 of afp header, 0x10 from NEND start 
        public int fileTypeCString; //aqo or tro for models. aqv or trv for texture/uv anims. The game doesn't seem to care what's here though, frankly.

        public byte[] GetBytes()
        {
            List<byte> outBytes = new List<byte>();
            outBytes.AddRange(fileName.GetBytes());
            outBytes.AddRange(BitConverter.GetBytes(paddingOffset));
            outBytes.AddRange(BitConverter.GetBytes(afpOffsetAddition));
            outBytes.AddRange(BitConverter.GetBytes(paddingOffsetAdjusted));
            outBytes.AddRange(BitConverter.GetBytes(fileTypeCString));

            return outBytes.ToArray();
        }
    }
}
