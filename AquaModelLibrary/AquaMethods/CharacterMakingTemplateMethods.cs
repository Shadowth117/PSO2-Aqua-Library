using AquaModelLibrary.AquaStructs;
using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AquaModelLibrary.AquaMethods
{
    public class CharacterMakingTemplateMethods
    {
        public static CharacterMakingTemplate ReadCMT(string fileName)
        {
            using (Stream stream = (Stream)new MemoryStream(File.ReadAllBytes(fileName)))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                return BeginReadCMT(streamReader);
            }
        }

        public static CharacterMakingTemplate ReadCMT(byte[] file)
        {
            using (Stream stream = (Stream)new MemoryStream(file))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                return BeginReadCMT(streamReader);
            }
        }
        public static CharacterMakingTemplate BeginReadCMT(BufferedStreamReader streamReader)
        {
            string type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
            int offset = 0x20; //Base offset due to NIFL header

            //Deal with deicer's extra header nonsense
            if (type.Equals("cmx\0"))
            {
                streamReader.Seek(0xC, SeekOrigin.Begin);
                //Basically always 0x60, but some deicer files from the Alpha have 0x50... 
                int headJunkSize = streamReader.Read<int>();

                streamReader.Seek(headJunkSize - 0x10, SeekOrigin.Current);
                type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
                offset += headJunkSize;
            }

            //Proceed based on file variant
            if (type.Equals("NIFL"))
            {
                //NIFL
                var cmt = ReadNIFLCMT(streamReader, offset);
                return cmt;
            }
            else if (type.Equals("VTBF"))
            {
                //VTBF - We don't do that here;
                return null;
            }
            else
            {
                return null;
            }
        }

        public static CharacterMakingTemplate ReadNIFLCMT(BufferedStreamReader streamReader, int offset)
        {
            var cmt = new CharacterMakingTemplate();
            cmt.nifl = streamReader.Read<AquaCommon.NIFL>();
            cmt.rel0 = streamReader.Read<AquaCommon.REL0>();

            streamReader.Seek(cmt.rel0.REL0DataStart + offset, SeekOrigin.Begin);
            cmt.cmtTable = ReadCMTTable(streamReader);

            for(int i = 0; i < cmt.cmtTable.addresses.Count; i++)
            {
                Dictionary<int, int> dict = new Dictionary<int, int>();
                streamReader.Seek(cmt.cmtTable.addresses[i] + offset, SeekOrigin.Begin);
                for (int j = 0; j < cmt.cmtTable.counts[i]; j++)
                {
                    dict.Add(streamReader.Read<int>(), streamReader.Read<int>());
                }

                cmt.cmtData.Add(dict);
            }

            return cmt;
        }

        public static CharacterMakingTemplate.CMTTable ReadCMTTable(BufferedStreamReader streamReader)
        {
            var cmtTable = new CharacterMakingTemplate.CMTTable();
            //Counts
            for (int i = 0; i < 20; i++)
            {
                cmtTable.counts.Add(streamReader.Read<int>());
            }
            //Addresses
            for (int i = 0; i < 20; i++)
            {
                cmtTable.addresses.Add(streamReader.Read<int>());
            }

            return cmtTable;
        }

        public static void SetNGSBenchmarkEnableFlag(CharacterMakingTemplate cmt)
        {
            for (int i = 0; i < cmt.cmtData.Count; i++)
            {
                var keys = cmt.cmtData[i].Keys.ToArray();
                for (int keyId = 0; keyId < keys.Length; keyId++)
                {
                    var key = keys[keyId];
                    int flags = cmt.cmtData[i][key];
                    var flagsBytes = BitConverter.GetBytes(flags);
                    flagsBytes[0] = (byte)(flagsBytes[0] | 0x40);
                    flags = BitConverter.ToInt32(flagsBytes, 0);
                    cmt.cmtData[i][key] = flags;
                }
            }
        }

        public static void ConvertToNGSBenchmark1(CharacterMakingTemplate cmt)
        {
            for(int i = 0; i < cmt.cmtData.Count; i++)
            {
                var keys = cmt.cmtData[i].Keys.ToArray();
                for (int keyId = 0; keyId < keys.Length; keyId++)
                {
                    var key = keys[keyId];
                    int flags = cmt.cmtData[i][key];
                    var flagsBytes = BitConverter.GetBytes(flags);
                    flagsBytes[1] = (byte)(flagsBytes[3] | 0x1);
                    flagsBytes[3] = 0;
                    flags = BitConverter.ToInt32(flagsBytes, 0);
                    cmt.cmtData[i][key] = flags;
                }
            }
        }

        public static void WriteCMT(string filePath, CharacterMakingTemplate cmt)
        {
            File.WriteAllBytes(filePath, CMTToBytes(cmt));
        }

        public static byte[] CMTToBytes(CharacterMakingTemplate cmt)
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
            for(int i = 0; i < cmt.cmtData.Count; i++)
            {
                cmt.cmtTable.addresses[i] = outBytes.Count;
                var dict = cmt.cmtData[i];
                foreach(var pair in dict)
                {
                    outBytes.AddRange(BitConverter.GetBytes(pair.Key));
                    outBytes.AddRange(BitConverter.GetBytes(pair.Value));
                }
            }

            //Write header data
            AquaGeneralMethods.SetByteListInt(outBytes, rel0SizeOffset + 4, outBytes.Count);

            for(int i = 0; i < cmt.cmtTable.counts.Count; i++)
            {
                outBytes.AddRange(BitConverter.GetBytes(cmt.cmtTable.counts[i]));
            }
            for (int i = 0; i < cmt.cmtTable.addresses.Count; i++)
            {
                AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
                outBytes.AddRange(BitConverter.GetBytes(cmt.cmtTable.addresses[i]));
            }
            AquaGeneralMethods.AlignWriter(outBytes, 0x10);

            //Write REL0 Size
            AquaGeneralMethods.SetByteListInt(outBytes, rel0SizeOffset, outBytes.Count - 0x8);

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
            NOF0FullSize += AquaGeneralMethods.AlignWriter(outBytes, 0x10);

            //NEND
            outBytes.AddRange(Encoding.UTF8.GetBytes("NEND"));
            outBytes.AddRange(BitConverter.GetBytes(0x8));
            outBytes.AddRange(BitConverter.GetBytes(0));
            outBytes.AddRange(BitConverter.GetBytes(0));

            //Generate NIFL
            AquaCommon.NIFL nifl = new AquaCommon.NIFL();
            nifl.magic = BitConverter.ToInt32(Encoding.UTF8.GetBytes("NIFL"), 0);
            nifl.NIFLLength = 0x18;
            nifl.unkInt0 = 1;
            nifl.offsetAddition = 0x20;

            nifl.NOF0Offset = NOF0Offset;
            nifl.NOF0OffsetFull = NOF0Offset + 0x20;
            nifl.NOF0BlockSize = NOF0FullSize;
            nifl.padding0 = 0;

            //Write NIFL
            outBytes.InsertRange(0, AquaGeneralMethods.ConvertStruct(nifl));

            return outBytes.ToArray();
        }
    }
}
