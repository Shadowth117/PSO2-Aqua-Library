using AquaModelLibrary.Helpers.Readers;
using System.Text;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    public class ItemNameCacheIndex : AquaCommon
    {
        StringBuilder output = null;
        public override string[] GetEnvelopeTypes()
        {
            return new string[] {
            "inca"
            };
        }

        public ItemNameCacheIndex() { }

        public ItemNameCacheIndex(byte[] file)
        {
            Read(file);
        }

        public ItemNameCacheIndex(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        public override void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            sr.Seek(sr.Read<int>() + offset, SeekOrigin.Begin);

            output = new StringBuilder();
            while (true)
            {
                int category = sr.Read<int>(); //Category?
                int id = sr.Read<int>(); //id
                if (category + id == 0)
                {
                    break;
                }
                int strPointer = sr.Read<int>();
                long bookmark = sr.Position;
                sr.Seek(strPointer + offset, SeekOrigin.Begin);
                output.AppendLine($"{category.ToString("X")} {id.ToString("X")} " + sr.ReadUTF16String(true, (int)sr.BaseStream.Length));

                sr.Seek(bookmark, SeekOrigin.Begin);
            }
        }
    }
}
