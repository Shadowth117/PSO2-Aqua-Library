using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.CustomRoboBattleRevolution
{
    /// <summary>
    /// Archive used for models and effects. Archives will only contain one data type or the other, seemingly
    /// </summary>
    public class SFD
    {
        public List<byte[]> files = new List<byte[]>();

        public SFD() { }
        
        public SFD(byte[] data)
        {
            Read(new BufferedStreamReaderBE<MemoryStream>(new MemoryStream(data)));
        }

        public SFD(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            sr._BEReadActive = true;
            var magic = sr.ReadBE<int>();
            var count = sr.ReadBE<int>();

            List<int> offsets = new List<int>();
            List<int> sizes = new List<int>();
            for(int i = 0; i < count; i++)
            {
                offsets.Add(sr.ReadBE<int>());
                sizes.Add(sr.ReadBE<int>());
            }

            for(int i = 0; i < count; i++)
            {
                files.Add(sr.ReadBytes(offsets[i], sizes[i]));
            }
        }
    }
}
