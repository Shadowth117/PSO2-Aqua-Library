using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.OtherStructs;
using Reloaded.Memory.Streams;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows;
using Zamboni;
using static AquaModelLibrary.AquaMethods.AquaGeneralMethods;
using static AquaModelLibrary.Data.PSO2.Aqua.LandAreaTemplate;
using static AquaModelLibrary.Data.PSO2.Aqua.LandPieceSettings;
using static AquaModelLibrary.VTBFMethods;

namespace AquaModelLibrary
{
    public class AquaMiscMethods
    {
        public static LandAreaTemplate LoadLAT(string inFilename)
        {
            LandAreaTemplate lat = new LandAreaTemplate();

            string ext = Path.GetExtension(inFilename);
            string variant = "";
            int offset;
            if (ext.Length > 4)
            {
                ext = ext.Substring(0, 4);
            }

            using (Stream stream = (Stream)new FileStream(inFilename, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                variant = ReadAquaHeader(streamReader, ext, out offset);

                if (variant == "NIFL")
                {
                    var nifl = streamReader.Read<AquaCommon.NIFL>();
                    var rel = streamReader.Read<AquaCommon.REL0>();
                    streamReader.Seek(offset + rel.REL0DataStart, SeekOrigin.Begin);
                    lat.header = streamReader.Read<LandAreaTemplate.LATHeader>();

                    streamReader.Seek(offset + lat.header.unkGridIndicesOffset, SeekOrigin.Begin);
                    for (int i = 0; i < lat.header.gridHeight; i++)
                    {
                        var latRow = new List<LATGridSmallData>();
                        for(int j = 0; j < lat.header.gridWidth; j++)
                        {
                            latRow.Add(streamReader.Read<LATGridSmallData>());
                        }
                        lat.latGridSmallData.Add(latRow);
                    }

                    streamReader.Seek(offset + lat.header.latGridDataOffset, SeekOrigin.Begin);
                    for (int i = 0; i < lat.header.gridHeight; i++)
                    {
                        var latRow = new List<LandAreaTemplate.LATGridData>();
                        for (int j = 0; j < lat.header.gridWidth; j++)
                        {
                            latRow.Add(streamReader.Read<LandAreaTemplate.LATGridData>());
                        }
                        lat.latGridData.Add(latRow);
                    }
                }
            }

            return lat;
        }

        public static AquaBTI_MotionConfig LoadBTI(string inFilename)
        {
            AquaBTI_MotionConfig bti = new AquaBTI_MotionConfig();

            string ext = Path.GetExtension(inFilename);
            string variant = "";
            int offset;
            if (ext.Length > 4)
            {
                ext = ext.Substring(0, 4);
            }

            using (Stream stream = (Stream)new FileStream(inFilename, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                variant = ReadAquaHeader(streamReader, ext, out offset);

                if (variant == "NIFL")
                {
                    var nifl = streamReader.Read<AquaCommon.NIFL>();
                    var rel = streamReader.Read<AquaCommon.REL0>();
                    streamReader.Seek(offset + rel.REL0DataStart, SeekOrigin.Begin);
                    bti.header = streamReader.Read<AquaBTI_MotionConfig.BTIHeader>();

                    for (int i = 0; i < bti.header.entryCount; i++)
                    {
                        streamReader.Seek(offset + bti.header.entryPtr + AquaBTI_MotionConfig.btiEntrySize * i, SeekOrigin.Begin);

                        AquaBTI_MotionConfig.BTIEntryObject btiEntry = new AquaBTI_MotionConfig.BTIEntryObject();
                        btiEntry.entry = streamReader.Read<AquaBTI_MotionConfig.BTIEntry>();

                        //Get strings
                        streamReader.Seek(offset + btiEntry.entry.additionPtr, SeekOrigin.Begin);
                        btiEntry.addition = ReadCString(streamReader);

                        streamReader.Seek(offset + btiEntry.entry.nodePtr, SeekOrigin.Begin);
                        btiEntry.node = ReadCString(streamReader);

                        bti.btiEntries.Add(btiEntry);
                    }
                }
            }

            return bti;
        }
        
        public static void WriteBTI(AquaBTI_MotionConfig bti, string outFileName)
        {
            var outBytes = BTIToBytes(bti);
            File.WriteAllBytes(outFileName, outBytes.ToArray());
        }

        public static List<byte> BTIToBytes(AquaBTI_MotionConfig bti)
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
            for(int i = 0; i < bti.btiEntries.Count; i++)
            {
                var entry = bti.btiEntries[i];
                NOF0Append(nof0PointerLocations, outBytes.Count, 1);
                NOF0Append(nof0PointerLocations, outBytes.Count + 4, 1);
                AddOntoDict(textAddressDict, textList, entry.addition, outBytes.Count);
                AddOntoDict(textAddressDict, textList, entry.node, outBytes.Count + 4);
                outBytes.AddRange(ConvertStruct(entry.entry));
            }

            //Write header data
            SetByteListInt(outBytes, rel0SizeOffset + 4, outBytes.Count);
            NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(0x14));
            outBytes.AddRange(BitConverter.GetBytes(bti.btiEntries.Count));
            outBytes.AddRange(BitConverter.GetBytes(bti.header.animLength));

            //Write text
            for(int i = 0; i < textList.Count; i++)
            {
                var offsetList = textAddressDict[textList[i]];
                for(int j = 0; j < offsetList.Count; j++)
                {
                    SetByteListInt(outBytes, offsetList[j], outBytes.Count);
                }
                outBytes.AddRange(Encoding.UTF8.GetBytes(textList[i]));
                var count = outBytes.Count;
                AlignWriter(outBytes, 0x4);
                if (count == outBytes.Count)
                {
                    outBytes.AddRange(new byte[4]);
                }
            }

            AlignWriter(outBytes, 0x10);

            //Write REL0 Size
            SetByteListInt(outBytes, rel0SizeOffset, outBytes.Count - 0x8);

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
            NOF0FullSize += AlignWriter(outBytes, 0x10);

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
            outBytes.InsertRange(0, ConvertStruct(nifl));

            return outBytes;
        }




        public static List<int> ReadMGX(string inFilename)
        {
            using (Stream stream = (Stream)new FileStream(inFilename, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                return BeginReadMGX(streamReader);
            }
        }
        public static List<int> ReadMGX(byte[] file)
        {
            using (Stream stream = (Stream)new MemoryStream(file))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                return BeginReadMGX(streamReader);
            }
        }

        private static List<int> BeginReadMGX(BufferedStreamReader streamReader)
        {
            string type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
            int offset = 0x20; //Base offset due to NIFL header

            //Deal with deicer's extra header nonsense
            if (type.Equals("mgx\0"))
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
                //There shouldn't be a nifl variant of this for now.
                Debug.WriteLine("Error, NIFL .mgx found");
                return null;
            }
            else if (type.Equals("VTBF"))
            {
                return ReadVTBFMGX(streamReader);
            }
            else
            {
                Debug.WriteLine("Improper File Format!");
                return null;
            }
        }

        public static List<int> ReadVTBFMGX(BufferedStreamReader streamReader)
        {
            List<int> mgxIds = new List<int>(); //For now, just get these. The data inside is pretty accessible, but we have nothing to do with it

            int dataEnd = (int)streamReader.BaseStream().Length;

            //Seek past vtbf tag
            streamReader.Seek(0x10, SeekOrigin.Current);          //VTBF tags

            while (streamReader.Position() < dataEnd)
            {
                var data = ReadVTBFTag(streamReader, out string tagType, out int ptrCount, out int entryCount);
                switch (tagType)
                {
                    case "DOC ":
                        break;
                    case "MAGR":
                        mgxIds.Add((int)data[0][0xFF]);
                        break;
                    default:
                        //Data being null signfies that the last thing read wasn't a proper tag. This should mean the end of the VTBF stream if nothing else.
                        if (data == null)
                        {
                            return mgxIds;
                        }
                        throw new System.Exception($"Unexpected tag at {streamReader.Position().ToString("X")}! {tagType} Please report!");
                }
            }

            return mgxIds;
        }
    }
}
