using AquaModelLibrary.Helpers.Readers;
using System.IO;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    //TXL files for pso2 aren't used the same as NN or Ninja due to models always having stringe references to textures
    public class TextureList : AquaCommon
    {
        public TxlHeader header;
        public List<string> texList = new List<string>();
        public List<string> iceList = new List<string>();
        public List<List<byte[]>> dataList = new List<List<byte[]>>(); //RGBA Colors, and some other data depending on texture type
        public override string[] GetEnvelopeTypes()
        {
            return new string[] {
            "txl\0"
            };
        }
        public TextureList() { }
        public TextureList(byte[] file) : base(file) { }
        public TextureList(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }

        public override void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            header = sr.Read<TxlHeader>();

            if (header.texOffsetListOffset != 0x10 && header.texOffsetListOffset != 0)
            {
                sr.Seek(offset + header.texOffsetListOffset, SeekOrigin.Begin);
                for (int i = 0; i < header.texCount; i++)
                {
                    var strOffset = sr.Read<int>();
                    if (strOffset != 0x10 && strOffset != 0)
                    {
                        var bookmark = sr.Position;

                        sr.Seek(offset + strOffset, SeekOrigin.Begin);
                        texList.Add(sr.ReadCString());

                        sr.Seek(bookmark, SeekOrigin.Begin);
                    }
                }
            }

            if (header.iceOffsetListOffset != 0x10 && header.iceOffsetListOffset != 0)
            {
                sr.Seek(offset + header.iceOffsetListOffset, SeekOrigin.Begin);
                for (int i = 0; i < header.iceCount; i++)
                {
                    var strOffset = sr.Read<int>();
                    if (strOffset != 0x10 && strOffset != 0)
                    {
                        var bookmark = sr.Position;

                        sr.Seek(offset + strOffset, SeekOrigin.Begin);
                        iceList.Add(sr.ReadCString());

                        sr.Seek(bookmark, SeekOrigin.Begin);
                    }
                }
            }

            if (header.dataOffsetListOffset != 0x10 && header.dataOffsetListOffset != 0)
            {
                sr.Seek(offset + header.dataOffsetListOffset, SeekOrigin.Begin);
                for (int i = 0; i < header.texCount; i++)
                {
                    var colorOffset = sr.Read<int>();
                    if (colorOffset != 0x10 && colorOffset != 0)
                    {
                        var bookmark = sr.Position;

                        List<byte[]> colorList = new List<byte[]>();
                        for (int j = 0; j < 0x15; j++)
                        {
                            colorList.Add(sr.Read4Bytes());
                        }
                        dataList.Add(colorList);

                        sr.Seek(bookmark, SeekOrigin.Begin);
                    }
                }
            }
        }

        public struct TxlHeader
        {
            public int texCount;
            public int iceCount;
            public int int_08;
            public int int_0C;

            public int texOffsetListOffset;
            public int dataOffsetListOffset;
            public int iceNamesOffset; //But why?
            public int iceOffsetListOffset;
        }
    }
}
