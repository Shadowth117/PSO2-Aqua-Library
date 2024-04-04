using AquaModelLibrary.Helpers.Readers;
using System.Diagnostics;
using System.Text;

namespace AquaModelLibrary.Data.BluePoint.CMAT
{
    public struct CMTLMeta0
    {
        public int unk0;
        public int unk1;
    }

    public class CMAT
    {
        public int hash;
        public int unk0;
        public int cmatFlags;
        public int int_0C;

        public int int_10;
        public int int_14;
        public int int_18;
        public int int_1C;

        public CFooter footerData;

        public List<string> shaderNames = new List<string>();
        public List<CMTLMeta0> meta0List = new List<CMTLMeta0>();
        public List<string> texNames = new List<string>();

        public CMAT(byte[] file)
        {
            file = CompressionHandler.CheckCompression(file);
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        private void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            sr.Seek(sr.BaseStream.Length - 0xC, SeekOrigin.Begin);
            footerData = sr.Read<CFooter>();

            sr.Seek(0, SeekOrigin.Begin);
            hash = sr.Read<int>();
            unk0 = sr.Read<int>();
            cmatFlags = sr.Read<int>();
            int_0C = sr.Read<int>();

            int_10 = sr.Read<int>();
            int_14 = sr.Read<int>();
            
            byte textLen;
            int texCount;
            switch (footerData.version)
            {
                case 0x39:  //SOTC

                    var shaderCount = sr.Read<int>();
                    for (int i = 0; i < shaderCount; i++)
                    {
                        meta0List.Add(sr.Read<CMTLMeta0>());
                        textLen = sr.Read<byte>();
                        shaderNames.Add(Encoding.UTF8.GetString(sr.ReadBytes(sr.Position, textLen)));
                        sr.Seek(textLen, System.IO.SeekOrigin.Current);
                    }

                    texCount = sr.Read<int>();
                    for (int i = 0; i < texCount; i++)
                    {
                        textLen = sr.Read<byte>();
                        texNames.Add(Encoding.UTF8.GetString(sr.ReadBytes(sr.Position, textLen)));
                        sr.Seek(textLen, System.IO.SeekOrigin.Current);
                    }

                    //TODO - Material color metadata
                    break;
                case 0x63:  //DeSR
                    int_18 = sr.Read<int>();
                    int_1C = sr.Read<int>();

                    textLen = sr.Read<byte>();
                    shaderNames.Add(Encoding.UTF8.GetString(sr.ReadBytes(sr.Position, textLen)));
                    sr.Seek(textLen, System.IO.SeekOrigin.Current);

                    texCount = sr.Read<int>();
                    for (int i = 0; i < texCount; i++)
                    {
                        meta0List.Add(sr.Read<CMTLMeta0>());
                    }
                    var texCount2 = sr.Read<int>();
                    for (int i = 0; i < texCount2; i++)
                    {
                        byte texLen = sr.Read<byte>();
                        Debug.WriteLine($"{texLen}");
                        texNames.Add(Encoding.UTF8.GetString(sr.ReadBytes(sr.Position, texLen)));
                        sr.Seek(texLen, System.IO.SeekOrigin.Current);
                    }

                    //TODO - Material color metadata
                    break;
                default:
                    throw new Exception($"Unexpected CMAT type {footerData.version}");
            }
        }
    }
}
