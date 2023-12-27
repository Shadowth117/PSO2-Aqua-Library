using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers;
using System.IO;
using AquaModelLibrary.Data.DataTypes.SetLengthStrings;

namespace AquaModelLibrary.Data.PSO2.MiscPSO2Structs
{
    public unsafe class TPNTexturePattern
    {
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

        public TPNTexturePattern() { }

        public TPNTexturePattern(byte[] bytes)
        {
            Read(bytes);
        }

        public TPNTexturePattern(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        public void Read(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                Read(sr);
            }
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            header = sr.Read<TpnHeader>();
            for (int i = 0; i < header.count; i++)
            {
                texSets.Add(sr.Read<TexSet>());
            }
        }

        public byte[] GetBytes()
        {
            List<byte> outBytes = new List<byte>();
            outBytes.AddRange(new byte[] { 0x74, 0x70, 0x6E, 0 });
            outBytes.AddRange(BitConverter.GetBytes(texSets.Count));
            foreach(var texSet in texSets)
            {
                outBytes.AddRange(DataHelpers.ConvertStruct(texSet));
            }
            outBytes.AlignFileEndWriter(0x10);

            return outBytes.ToArray();
        }
    }
}
