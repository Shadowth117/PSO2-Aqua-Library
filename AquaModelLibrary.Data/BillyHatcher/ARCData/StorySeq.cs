using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using System.Diagnostics;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    public class StorySeq : ARC
    {
        public List<StorySeqEntry> entries = new List<StorySeqEntry>();
        public StorySeq() { }
        public StorySeq(byte[] file)
        {
            Read(file);
        }

        public StorySeq(BufferedStreamReaderBE<MemoryStream> sr)
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
            var offsetToPtrs = sr.ReadBE<int>();

            for(int i = 0; i < count; i++)
            {
                var entry = new StorySeqEntry()
                {
                    ptr = sr.ReadBE<int>(),
                    int_04 = sr.ReadBE<int>(),
                    int_08 = sr.ReadBE<int>()
                };
                entries.Add(entry);
                var bookmark = sr.Position;
                sr.Seek(entry.ptr + 0x20, SeekOrigin.Begin);

                bool shouldContinue = true;
                int dataCounter = 0;
                while(shouldContinue)
                {
                    shouldContinue = false;
                    StorySeqData data = new StorySeqData();
                    data.start = sr.ReadBE<short>();
                    data.value = sr.ReadBE<int>();
                    data.shouldContinue = sr.ReadBE<short>();
                    shouldContinue = (data.shouldContinue & 0x100) > 0 || ((data.shouldContinue == 0x19));
                    if(shouldContinue)
                    {
                        data.endValue0 = sr.ReadBE<short>();
                        data.endValue1 = ((data.endValue0 & 0x9) > 0) ? sr.ReadBE<short>() : (short)0;
                    }
                    entry.dataList.Add(data);

                    Debug.WriteLine($"{i} {dataCounter} - {data.value} {data.value:X}");
                    dataCounter++;
                }

                sr.Seek(bookmark, SeekOrigin.Begin);
            }
        }

        public byte[] GetBytes()
        {
            ByteListExtension.AddAsBigEndian = true;
            List<byte> outBytes = new List<byte>();
            List<int> offsets = new List<int>();

            outBytes.AddValue((int)0x1000000);
            outBytes.AddValue((int)0);
            outBytes.AddValue((int)entries.Count);
            offsets.Add(outBytes.Count);
            outBytes.AddValue((int)0x10);

            for (int i = 0; i < entries.Count; i++)
            {
                offsets.Add(outBytes.Count);
                outBytes.ReserveInt($"entry{i}");
                outBytes.AddValue((int)0);
                outBytes.AddValue((int)0);
            }

            for (int i = 0; i < entries.Count; i++)
            {
                outBytes.FillInt($"entry{i}", outBytes.Count);
                foreach(var entryData in entries[i].dataList)
                {
                    outBytes.AddRange(entryData.GetBytes());
                }
            }
            outBytes.AlignWriter(0x4);

            //Write headerless POF0
            int pof0Offset = outBytes.Count;
            offsets.Sort();
            outBytes.AddRange(POF0.GenerateRawPOF0(offsets));
            int pof0End = outBytes.Count;
            int pof0Size = pof0End - pof0Offset;

            //End text
            outBytes.AddValue((int)0);
            outBytes.AddValue((int)0);
            outBytes.Add(0x73);
            outBytes.Add(0x63);
            outBytes.Add(0x72);
            outBytes.Add(0x69);
            outBytes.Add(0x70);
            outBytes.Add(0x74);
            outBytes.Add(0x0);

            //ARC Header (insert at the end to make less messy)
            List<byte> arcHead = new List<byte>();
            arcHead.AddValue(outBytes.Count + 0x20);
            arcHead.AddValue(pof0Offset);
            arcHead.AddValue(pof0Size);
            arcHead.AddValue(1);

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

        public class StorySeqEntry
        {
            public int ptr;
            public int int_04;
            public int int_08;

            public List<StorySeqData> dataList = new List<StorySeqData>();
        }

        public class StorySeqData
        {
            public short start;
            public int value;
            public short shouldContinue;
            public short endValue0;
            /// <summary>
            /// Used if previous 'endvalue' is 9;
            /// </summary>
            public short endValue1;

            public byte[] GetBytes()
            {
                List<byte> outBytes = new List<byte>();
                outBytes.AddValue(start);
                outBytes.AddValue(value);
                outBytes.AddValue(shouldContinue);
                if((shouldContinue & 0x100) > 0 || (shouldContinue == 0x19))
                {
                    outBytes.AddValue(endValue0);
                    if((endValue0 & 0x9) > 0)
                    {
                        outBytes.AddValue(endValue1);
                    }
                }

                return outBytes.ToArray();
            }
        }
    }
}
