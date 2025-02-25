using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using System.Text;

namespace AquaModelLibrary.Data.BillyHatcher
{
    public class BGMRegular
    {
        public List<string> bgmFiles = new List<string>();

        public BGMRegular() { }
        public BGMRegular(byte[] file)
        {
            Read(file);
        }
        public BGMRegular(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        public void Read(byte[] file)
        {
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            sr._BEReadActive = true;
            sr.Seek(0x10, SeekOrigin.Begin);
            var count = sr.ReadBE<int>();
            var startOffset = sr.ReadBE<int>();

            List<int> offsets = new List<int>();
            for(int i = 0; i < count; i++)
            {
                sr.ReadBE<int>();
                offsets.Add(sr.ReadBE<int>());
            }
            foreach(var offset in offsets)
            {
                sr.Seek(0x8 + offset, SeekOrigin.Begin);
                var stringOffset = sr.ReadBE<int>();
                if(stringOffset == 0x0)
                {
                    bgmFiles.Add(null);
                } else
                {
                    sr.Seek(0x8 + stringOffset, SeekOrigin.Begin);
                    bgmFiles.Add(sr.ReadCString());
                }
            }
        }

        public byte[] GetBytes()
        {
            ByteListExtension.AddAsBigEndian = true;
            List<int> offsets = new List<int>();
            List<byte> outBytes = new List<byte>();
            outBytes.AddValue((int)0);
            outBytes.AddValue((int)0);
            outBytes.AddValue(bgmFiles.Count);
            offsets.Add(outBytes.Count);
            outBytes.AddValue((int)0x10);

            for(int i = 0; i < bgmFiles.Count; i++)
            {
                outBytes.AddValue((int)0);
                offsets.Add(outBytes.Count);
                outBytes.ReserveInt($"BGMNamePointerPointer{i}");
            }
            for (int i = 0; i < bgmFiles.Count; i++)
            {
                outBytes.FillInt($"BGMNamePointerPointer{i}", outBytes.Count);
                offsets.Add(outBytes.Count);
                outBytes.ReserveInt($"BGMNamePointer{i}");
            }
            for (int i = 0; i < bgmFiles.Count; i++)
            {
                outBytes.FillInt($"BGMNamePointer{i}", outBytes.Count);
                outBytes.AddValue(Encoding.ASCII.GetBytes(bgmFiles[i]));
            }
            outBytes.AlignWriter(0x10);

            List<byte> header = new List<byte>();
            var size = outBytes.Count;
            header.AddRange(new byte[] { 0x4D, 0x53, 0x4D, 0x50});
            header.AddValue((int)size);
            outBytes.InsertRange(0, header);
            ByteListExtension.Reset();

            outBytes.AddRange(POF0.GeneratePOF0(offsets));

            return outBytes.ToArray();
        }
    }
}
