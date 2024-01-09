using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers;
using System.Numerics;
using System.Text;
using AquaModelLibrary.Data.PSO2.Aqua.AquaCommonData;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    public class BTI_MotionConfig : AquaCommon
    {
        public BTIHeader header = new BTIHeader();
        public List<BTIEntryObject> btiEntries = new List<BTIEntryObject>();
        public static int btiEntrySize = 0x74;

        public override string[] GetEnvelopeTypes()
        {
            return new string[] {
            "bti\0"
            };
        }
        public BTI_MotionConfig() { }

        public BTI_MotionConfig(byte[] file) : base(file) { }

        public BTI_MotionConfig(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }

        public override void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            header = sr.Read<BTIHeader>();

            for (int i = 0; i < header.entryCount; i++)
            {
                sr.Seek(offset + header.entryPtr + btiEntrySize * i, SeekOrigin.Begin);

                BTI_MotionConfig.BTIEntryObject btiEntry = new BTIEntryObject();
                btiEntry.entry = sr.Read<BTIEntry>();

                //Get strings
                sr.Seek(offset + btiEntry.entry.additionPtr, SeekOrigin.Begin);
                btiEntry.addition = sr.ReadCString();

                sr.Seek(offset + btiEntry.entry.nodePtr, SeekOrigin.Begin);
                btiEntry.node = sr.ReadCString();

                btiEntries.Add(btiEntry);
            }
        }

        public override byte[] GetBytesNIFL()
        {
            List<byte> outBytes = new List<byte>();
            List<int> nof0PointerLocations = new List<int>(); //Used for the NOF0 section
            Dictionary<string, List<int>> textAddressDict = new Dictionary<string, List<int>>();
            List<string> textList = new List<string>();
            int rel0SizeOffset = 0;

            //REL0
            outBytes.AddRange(Encoding.UTF8.GetBytes("REL0"));
            rel0SizeOffset = outBytes.Count; //We'll fill this later
            outBytes.AddRange(BitConverter.GetBytes(0));
            outBytes.AddRange(BitConverter.GetBytes(0));
            outBytes.AddRange(BitConverter.GetBytes(1));

            outBytes.AddRange(BitConverter.GetBytes(-1));

            //Entries
            for (int i = 0; i < btiEntries.Count; i++)
            {
                var entry = btiEntries[i];
                DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
                DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 4, 1);
                DataHelpers.AddOntoDict(textAddressDict, textList, entry.addition, outBytes.Count);
                DataHelpers.AddOntoDict(textAddressDict, textList, entry.node, outBytes.Count + 4);
                outBytes.AddRange(DataHelpers.ConvertStruct(entry.entry));
            }

            //Write header data
            outBytes.SetByteListInt(rel0SizeOffset + 4, outBytes.Count);
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(0x14));
            outBytes.AddRange(BitConverter.GetBytes(btiEntries.Count));
            outBytes.AddRange(BitConverter.GetBytes(header.animLength));

            //Write text
            for (int i = 0; i < textList.Count; i++)
            {
                var offsetList = textAddressDict[textList[i]];
                for (int j = 0; j < offsetList.Count; j++)
                {
                    outBytes.SetByteListInt(offsetList[j], outBytes.Count);
                }
                outBytes.AddRange(Encoding.UTF8.GetBytes(textList[i]));
                var count = outBytes.Count;
                outBytes.AlignWriter(0x4);
                if (count == outBytes.Count)
                {
                    outBytes.AddRange(new byte[4]);
                }
            }

            outBytes.AlignWriter(0x10);

            //Write REL0 Size
            outBytes.SetByteListInt(rel0SizeOffset, outBytes.Count - 0x8);

            //Write NOF0
            int NOF0Offset = outBytes.Count;
            int NOF0Size = (nof0PointerLocations.Count + 2) * 4;
            int NOF0FullSize = NOF0Size + 0x8;
            outBytes.AddRange(Encoding.UTF8.GetBytes("NOF0"));
            outBytes.AddRange(BitConverter.GetBytes(NOF0Size));
            outBytes.AddRange(BitConverter.GetBytes(nof0PointerLocations.Count));
            outBytes.AddRange(BitConverter.GetBytes(0x10));

            //Write pointer offsets
            for (int i = 0; i < nof0PointerLocations.Count; i++)
            {
                outBytes.AddRange(BitConverter.GetBytes(nof0PointerLocations[i]));
            }
            NOF0FullSize += outBytes.AlignWriter(0x10);

            //NEND
            outBytes.AddRange(Encoding.UTF8.GetBytes("NEND"));
            outBytes.AddRange(BitConverter.GetBytes(0x8));
            outBytes.AddRange(BitConverter.GetBytes(0));
            outBytes.AddRange(BitConverter.GetBytes(0));

            //Generate NIFL
            NIFL nifl = new NIFL();
            nifl.magic = BitConverter.ToInt32(Encoding.UTF8.GetBytes("NIFL"), 0);
            nifl.NIFLLength = 0x18;
            nifl.unkInt0 = 1;
            nifl.offsetAddition = 0x20;

            nifl.NOF0Offset = NOF0Offset;
            nifl.NOF0OffsetFull = NOF0Offset + 0x20;
            nifl.NOF0BlockSize = NOF0FullSize;
            nifl.padding0 = 0;

            //Write NIFL
            outBytes.InsertRange(0, DataHelpers.ConvertStruct(nifl));

            return outBytes.ToArray();
        }

        public struct BTIHeader
        {
            public uint entryPtr;
            public int entryCount;
            public float animLength;
        }

        public struct BTIEntry
        {
            public int additionPtr;
            public int nodePtr;
            public short sht_08;
            public short sht_0A;
            public float startFrame;

            public float float_10;

            public Vector3 pos;
            public float float_20;

            public Vector3 eulerRot;
            public float float_30;

            public Vector3 scale;
            public float endFrame;

            public float float_44;
            public float float_48;
            public float float_4C;
            public float float_50;

            public int field_54;
            public int field_58;
            public int field_5C;
            public int field_60;

            public int field_64;
            public Vector3 vec3_68;
        }

        public class BTIEntryObject
        {
            public BTIEntry entry = new BTIEntry();
            public string addition = ""; //Usually an effect
            public string node = "";
        }

    }
}
