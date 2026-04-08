using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using System.Text.Json.Serialization;

namespace AquaModelLibrary.Data.BillyHatcher
{
    public class SEMapRegular
    {
        public class SEMap1
        {
            public ushort id0 { get; set; }
            public ushort id1 { get; set; }
        }
        public class SEMap2
        {
            public byte id0 { get; set; }
            public byte id1 { get; set; }
        }

        //For easier JSON editing
        public List<string> seMap1ListJ { get; set; } = new List<string>();
        public List<string> seMap2ListJ { get; set; } = new List<string>();

        [JsonIgnore]
        public List<SEMap1> seMap1List { get; set; } = new List<SEMap1>();
        [JsonIgnore]
        public List<SEMap2> seMap2List { get; set; } = new List<SEMap2>();
        public List<byte> seMap3List { get; set; } = new List<byte>();


        public void JSONPrepLists()
        {
            seMap1ListJ.Clear();
            for(int i = 0; i < seMap1List.Count; i++)
            {
                seMap1ListJ.Add($"{seMap1List[i].id0}_{seMap1List[i].id1}");
            }
            seMap2ListJ.Clear();
            for (int i = 0; i < seMap2List.Count; i++)
            {
                seMap2ListJ.Add($"{seMap2List[i].id0}_{seMap2List[i].id1}");
            }
        }

        public void GamePrepFromJSONLists()
        {
            seMap1List.Clear();
            for (int i = 0; i < seMap1ListJ.Count; i++)
            {
                var splitNums = seMap1ListJ[i].Split('_');
                seMap1List.Add(new SEMap1() { id0 = ushort.Parse(splitNums[0]), id1 = ushort.Parse(splitNums[1]) });
            }
            seMap2List.Clear();
            for (int i = 0; i < seMap2ListJ.Count; i++)
            {
                var splitNums = seMap2ListJ[i].Split('_');
                seMap2List.Add(new SEMap2() { id0 = byte.Parse(splitNums[0]), id1 = byte.Parse(splitNums[1]) });
            }
        }

        public SEMapRegular() { }
        public SEMapRegular(byte[] file)
        {
            Read(file);
        }
        public SEMapRegular(BufferedStreamReaderBE<MemoryStream> sr)
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
            sr.Seek(0x4, SeekOrigin.Begin);
            var size = sr.Read<int>(); //Always little endian

            sr.Seek(0x8, SeekOrigin.Current);
            var count = sr.ReadBE<int>();
            var start1Offset = sr.ReadBE<int>();
            var start2Offset = sr.ReadBE<int>();
            var start3Offset = sr.ReadBE<int>();

            sr.Seek(0x8 + start1Offset, SeekOrigin.Begin);

            for(int i = 0; i < count; i++)
            {
                seMap1List.Add(new SEMap1() { id0 = sr.ReadBE<ushort>(), id1 = sr.ReadBE<ushort>()});
            }

            sr.Seek(0x8 + start2Offset, SeekOrigin.Begin);
            for (int i = 0; i < ((start3Offset - start2Offset) / 2); i++) //0x230 count in retail
            {
                seMap2List.Add(new SEMap2() { id0 = sr.ReadBE<byte>(), id1 = sr.ReadBE<byte>() });
            }

            sr.Seek(0x8 + start3Offset, SeekOrigin.Begin);
            for (int i = 0; i < (size - start3Offset); i++) //0x24 count in retail
            {
                seMap3List.Add(sr.Read<byte>());
            }
        }

        public byte[] GetBytes()
        {
            ByteListExtension.AddAsBigEndian = true;
            List<int> offsets = new List<int>() { 0xC, 0x10, 0x14};
            List<byte> outBytes = new List<byte>();
            outBytes.AddValue((int)0);
            outBytes.AddValue((int)0);
            outBytes.AddValue(seMap1List.Count);
            outBytes.ReserveInt($"SEMap1List");
            outBytes.ReserveInt($"SEMap2List");
            outBytes.ReserveInt($"SEMap3List");

            outBytes.FillInt($"SEMap1List", outBytes.Count);
            for(int i = 0; i < seMap1List.Count; i++)
            {
                outBytes.AddValue(seMap1List[i].id0);
                outBytes.AddValue(seMap1List[i].id1);
            }
            outBytes.FillInt($"SEMap2List", outBytes.Count);
            for (int i = 0; i < seMap2List.Count; i++)
            {
                outBytes.Add(seMap2List[i].id0);
                outBytes.Add(seMap2List[i].id1);
            }
            outBytes.FillInt($"SEMap3List", outBytes.Count);
            for (int i = 0; i < seMap3List.Count; i++)
            {
                outBytes.Add(seMap3List[i]);
            }

            List<byte> header = new List<byte>();
            var size = outBytes.Count;
            header.AddRange(new byte[] { 0x53, 0x45, 0x4D, 0x50});
            ByteListExtension.AddAsBigEndian = false;
            header.AddValue((int)size);
            outBytes.InsertRange(0, header);
            ByteListExtension.Reset();

            outBytes.AddRange(POF0.GeneratePOF0(offsets));

            return outBytes.ToArray();
        }
    }
}
