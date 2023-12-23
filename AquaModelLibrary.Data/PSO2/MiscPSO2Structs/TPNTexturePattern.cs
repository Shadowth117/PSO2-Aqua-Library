using AquaModelLibrary.Core.Extensions;
using AquaModelLibrary.Data.PSO2.Aqua.AquaCommonData;
using AquaModelLibrary.Data.PSO2.Aqua.SetLengthStrings;
using AquaModelLibrary.Helpers;

namespace AquaModelLibrary.Data.PSO2.MiscPSO2Structs
{
    public unsafe class TPNTexturePattern
    {
        public AFPBase tpnAFPBase = new AFPBase();
        public TpnHeader header = new TpnHeader();
        public List<TexSet> texSets = new List<TexSet>();
        //Header
        public struct TpnHeader
        {
            public int magic; //tpn
            public int count; //Amount of texture sets in the file
        }
        //Texture Set
        public struct TexSet
        {
            public PSO2String setName; //Reference for this texture set
            public PSO2String tex0Name; //Tex0
            public PSO2String tex1Name; //Tex1
            public PSO2String tex2Name; //Tex2
            public PSO2String tex3Name; //Tex3
        }

        public byte[] GetBytes()
        {
            List<byte> outBytes = new List<byte>();
            outBytes.AddRange(new byte[] { 0x74, 0x70, 0x6E, 0 });
            outBytes.AddRange(BitConverter.GetBytes(texSets.Count));
            foreach(var texSet in texSets)
            {
                outBytes.AddRange(MiscHelpers.ConvertStruct(texSet));
            }
            outBytes.AlignFileEndWriter(0x10);

            return outBytes.ToArray();
        }
    }
}
