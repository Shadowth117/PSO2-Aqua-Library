using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    public class VIBRegular : ARC
    {
        List<byte[]> vibValues = new List<byte[]>();
        public VIBRegular() { }
        public VIBRegular(byte[] file)
        {
            Read(file);
        }

        public VIBRegular(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        public override void Read(byte[] file)
        {
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        public override void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            sr._BEReadActive = true;
            base.Read(sr);
            sr.Seek(0x20, SeekOrigin.Begin);
            var unk0 = sr.ReadBE<int>();
            var unk1 = sr.ReadBE<int>();
            var count = sr.ReadBE<int>();
            var initialOffset = sr.ReadBE<int>();
            var reserve0 = sr.ReadBE<int>();
            List<long> offsets = new List<long>();

            for(int i = 0; i < count; i++)
            {
                sr.Seek(offsets[i] + 0x20, SeekOrigin.Begin);
                byte value = 0;
                List<byte> bytes = new List<byte>();
                while(value != 0xFF)
                {
                    value = sr.ReadBE<byte>();
                    bytes.Add(value);
                }
                vibValues.Add(bytes.ToArray());
            }
        }

        public byte[] GetBytes()
        {
            ByteListExtension.AddAsBigEndian = true; 
            List<byte> outBytes = new List<byte>();
            List<int> offsets = new List<int>();

            outBytes.AddValue((int)0);
            outBytes.AddValue((int)0);
            outBytes.AddValue((int)vibValues.Count);
            offsets.Add(outBytes.Count);
            outBytes.AddValue((int)0x14);
            outBytes.AddValue((int)0);

            for (int i = 0; i < vibValues.Count; i++)
            {
                offsets.Add(outBytes.Count);
                outBytes.ReserveLong($"vib{i}");
            }

            for (int i = 0; i < vibValues.Count; i++)
            {
                outBytes.FillLong($"vib{i}", outBytes.Count);
                offsets.Add(outBytes.Count);
                outBytes.AddValue((int)(outBytes.Count + 4));
                outBytes.AddRange(vibValues[i]);
                outBytes.AlignWriter(0x4);
            }

            //Write headerless POF0
            int pof0Offset = outBytes.Count;
            offsets.Sort();
            outBytes.AddRange(POF0.GenerateRawPOF0(offsets));
            int pof0End = outBytes.Count;
            int pof0Size = pof0End - pof0Offset;

            //ARC Header (insert at the end to make less messy)
            List<byte> arcHead = new List<byte>();
            arcHead.AddValue(outBytes.Count + 0x20);
            arcHead.AddValue(pof0Offset);
            arcHead.AddValue(pof0Size);
            arcHead.AddValue(0);

            arcHead.AddValue(0);
            arcHead.Add(0x30);
            arcHead.Add(0x31);
            arcHead.Add(0x30);
            arcHead.Add(0x30);
            arcHead.AddValue(0);
            arcHead.AddValue(0);
            outBytes.InsertRange(0, arcHead);
            ByteListExtension.Reset();

            return outBytes.ToArray();
        }

    }
}
