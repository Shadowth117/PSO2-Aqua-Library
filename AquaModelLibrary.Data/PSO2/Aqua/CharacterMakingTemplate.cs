using AquaModelLibrary.Helpers.Readers;
using System.Diagnostics;
using System.Text;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers;
using AquaModelLibrary.Data.PSO2.Aqua.AquaCommonData;
using System.IO;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    public class CharacterMakingTemplate : AquaCommon
    {
        //Currently only planning to support the NGS versions of this. Could support specific versions of the old CMT files if desired, but they differ in part categories and use int16s vs int32s
        //Essentially, this whole file is a large table of character part ids for character creation with bitflags that enable and disable various things. 
        public CMTTable cmtTable = null;
        public List<Dictionary<int, int>> cmtData = new List<Dictionary<int, int>>();
        public override string[] GetEnvelopeTypes()
        {
            return new string[] {
            "cmt\0"
            };
        }

        public CharacterMakingTemplate() { }

        public CharacterMakingTemplate(byte[] bytes) : base(bytes) { }

        public CharacterMakingTemplate(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }

        public override void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            cmtTable = new CMTTable(sr);

            for (int i = 0; i < cmtTable.addresses.Count; i++)
            {
                Dictionary<int, int> dict = new Dictionary<int, int>();
                sr.Seek(cmtTable.addresses[i] + offset, SeekOrigin.Begin);
                for (int j = 0; j < cmtTable.counts[i]; j++)
                {
                    var id = sr.Read<int>();
                    var value = sr.Read<int>();
                    if (!dict.ContainsKey(id))
                    {
                        dict.Add(id, value);
                    }
                    else
                    {
                        Debug.WriteLine($"Duplicate entry for: {id}. Value was: {value:X}");
                    }
                }

                cmtData.Add(dict);
            }
        }

        public void SetNGSBenchmarkEnableFlag()
        {
            for (int i = 0; i < cmtData.Count; i++)
            {
                var keys = cmtData[i].Keys.ToArray();
                for (int keyId = 0; keyId < keys.Length; keyId++)
                {
                    var key = keys[keyId];
                    int flags = cmtData[i][key];
                    var flagsBytes = BitConverter.GetBytes(flags);
                    flagsBytes[0] = (byte)(flagsBytes[0] | 0x40);
                    flags = BitConverter.ToInt32(flagsBytes, 0);
                    cmtData[i][key] = flags;
                }
            }
        }

        public  void ConvertToNGSBenchmark1()
        {
            for (int i = 0; i < cmtData.Count; i++)
            {
                var keys = cmtData[i].Keys.ToArray();
                for (int keyId = 0; keyId < keys.Length; keyId++)
                {
                    var key = keys[keyId];
                    int flags = cmtData[i][key];
                    var flagsBytes = BitConverter.GetBytes(flags);
                    flagsBytes[1] = (byte)(flagsBytes[3] | 0x1);
                    flagsBytes[3] = 0;
                    flags = BitConverter.ToInt32(flagsBytes, 0);
                    cmtData[i][key] = flags;
                }
            }
        }

        public override byte[] GetBytesNIFL()
        {
            List<byte> outBytes = new List<byte>();
            List<int> nof0PointerLocations = new List<int>(); //Used for the NOF0 section

            int rel0SizeOffset = 0;

            //REL0
            outBytes.AddRange(Encoding.UTF8.GetBytes("REL0"));
            rel0SizeOffset = outBytes.Count; //We'll fill this later
            outBytes.AddRange(BitConverter.GetBytes(0));
            outBytes.AddRange(BitConverter.GetBytes(0));
            outBytes.AddRange(BitConverter.GetBytes(0));

            outBytes.AddRange(BitConverter.GetBytes(-1));

            //Write data
            for (int i = 0; i < cmtData.Count; i++)
            {
                cmtTable.addresses[i] = outBytes.Count;
                var dict = cmtData[i];
                foreach (var pair in dict)
                {
                    outBytes.AddRange(BitConverter.GetBytes(pair.Key));
                    outBytes.AddRange(BitConverter.GetBytes(pair.Value));
                }
            }

            //Write header data
            outBytes.SetByteListInt(rel0SizeOffset + 4, outBytes.Count);

            for (int i = 0; i < cmtTable.counts.Count; i++)
            {
                outBytes.AddRange(BitConverter.GetBytes(cmtTable.counts[i]));
            }
            for (int i = 0; i < cmtTable.addresses.Count; i++)
            {
                DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
                outBytes.AddRange(BitConverter.GetBytes(cmtTable.addresses[i]));
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
            outBytes.AddRange(BitConverter.GetBytes(0x10));//Write pointer offsets

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

        public class CMTTable
        {
            public List<int> counts = new List<int>();
            public List<int> addresses = new List<int>();

            public CMTTable() { }

            public CMTTable(BufferedStreamReaderBE<MemoryStream> sr)
            {
                //Counts
                for (int i = 0; i < 20; i++)
                {
                    counts.Add(sr.Read<int>());
                }
                //Addresses
                for (int i = 0; i < 20; i++)
                {
                    addresses.Add(sr.Read<int>());
                }
            }
        }
    }
}
