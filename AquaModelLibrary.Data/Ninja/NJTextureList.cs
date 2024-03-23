using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers.Extensions;
using System.Text;

namespace AquaModelLibrary.Data.Ninja
{
    public class NJTextureList
    {
        public List<string> texNames = new List<string>();
        public NJTextureList() { }

        public NJTextureList(byte[] bytes, int offset = 0)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr, offset);
            }
        }

        public NJTextureList(BufferedStreamReaderBE<MemoryStream> sr, int offset = 0)
        {
            Read(sr, offset);
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr, int offset = 0)
        {
            var startOffset = sr.ReadBE<int>();
            var count = sr.ReadBE<int>();

            sr.Seek(startOffset + offset, SeekOrigin.Begin);
            List<int> stringOffsets = new List<int>();
            for(int i = 0; i < count; i++)
            {
                stringOffsets.Add(sr.ReadBE<int>());
                sr.ReadBE<int>(); //Count
                sr.ReadBE<int>();
            }
            foreach(var strOffset in stringOffsets)
            {
                sr.Seek(strOffset + offset, SeekOrigin.Begin);
                texNames.Add(sr.ReadCString());
            }
        }

        public void Write(List<byte> outBytes, List<int> offsets, int offset = 0)
        {
            offsets.Add(outBytes.Count + offset);
            outBytes.ReserveInt("TexListReferencesOffset");
            outBytes.AddValue(texNames.Count);
            outBytes.FillInt("TexListReferencesOffset", outBytes.Count);
            for (int i = 0; i < texNames.Count; i++)
            {
                offsets.Add(outBytes.Count + offset);
                outBytes.ReserveInt($"TexRef{i}");
                outBytes.AddValue(i);
                outBytes.AddValue(0);
            }
            for (int i = 0; i < texNames.Count; i++)
            {
                outBytes.FillInt($"TexRef{i}", outBytes.Count);
                outBytes.AddRange(Encoding.UTF8.GetBytes(texNames[i]));
                outBytes.Add(0);
            }
        }
    }
}
