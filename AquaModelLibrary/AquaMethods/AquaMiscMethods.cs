﻿using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using zamboni;
using static AquaModelLibrary.AquaObjectMethods;

namespace AquaModelLibrary
{
    public class AquaMiscMethods
    {

        public static PSO2Text ReadPSO2Text(string fileName)
        {
            using (Stream stream = (Stream)new FileStream(fileName, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                return BeginReadPSO2Text(streamReader, fileName);
            }
        }

        public static PSO2Text ReadPSO2Text(byte[] file)
        {
            using (Stream stream = (Stream)new MemoryStream(file))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                return BeginReadPSO2Text(streamReader);
            }
        }

        public static PSO2Text BeginReadPSO2Text(BufferedStreamReader streamReader, string fileName = null)
        {
            string type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
            int offset = 0x20; //Base offset due to NIFL header

            //Deal with deicer's extra header nonsense
            if (type.Equals("text"))
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
                return ReadNIFLText(streamReader, offset, fileName);
            }
            else if (type.Equals("VTBF"))
            {
                //Text should really never be VTBF...
            }
            else
            {
                MessageBox.Show("Improper File Format!");
            }

            return null;
        }

        public static PSO2Text ReadNIFLText(BufferedStreamReader streamReader, int offset, string fileName)
        {
            var txt = new PSO2Text();
            var nifl = streamReader.Read<AquaCommon.NIFL>();
            var end = nifl.NOF0Offset + offset;
            var rel0 = streamReader.Read<AquaCommon.REL0>();
            streamReader.Seek(rel0.REL0DataStart + offset, SeekOrigin.Begin);

            var categoryPointer = streamReader.Read<int>();
            var categoryCount = streamReader.Read<int>();

            //Read through categories
            streamReader.Seek(categoryPointer + offset, SeekOrigin.Begin);
            for (int i = 0; i < categoryCount; i++)
            {
                var categoryNameOffset = streamReader.Read<int>();
                var categoryDataInfoOffset = streamReader.Read<int>();
                var subCategoryCount = streamReader.Read<int>();

                //Setup subcategory lists
                txt.text.Add(new List<List<PSO2Text.textPair>>());
                for (int j = 0; j < subCategoryCount; j++)
                {
                    txt.text[i].Add(new List<PSO2Text.textPair>());
                }

                //Get category name
                long bookmark = streamReader.Position();
                streamReader.Seek(categoryNameOffset + offset, SeekOrigin.Begin);

                txt.categoryNames.Add(ReadCString(streamReader));

                //Get Category Info
                streamReader.Seek(categoryDataInfoOffset + offset, SeekOrigin.Begin);

                for (int sub = 0; sub < subCategoryCount; sub++)
                {
                    var categoryIndexOffset = streamReader.Read<int>();
                    var subCategoryId = streamReader.Read<int>();
                    var categoryIndexCount = streamReader.Read<int>();
                    var bookMarkSub = streamReader.Position();

                    streamReader.Seek(categoryIndexOffset + offset, SeekOrigin.Begin);
                    for (int j = 0; j < categoryIndexCount; j++)
                    {
                        var pair = new PSO2Text.textPair();
                        int nameLoc = streamReader.Read<int>();
                        int textLoc = streamReader.Read<int>();
                        long bookmarkLocal = streamReader.Position();

                        streamReader.Seek(nameLoc + offset, SeekOrigin.Begin);
                        pair.name = ReadCString(streamReader);

                        streamReader.Seek(textLoc + offset, SeekOrigin.Begin);
                        pair.str = ReadUTF16String(streamReader, end);

                        txt.text[i][subCategoryId].Add(pair);
                        streamReader.Seek(bookmarkLocal, SeekOrigin.Begin);
                    }
                    streamReader.Seek(bookMarkSub, SeekOrigin.Begin);
                }

                streamReader.Seek(bookmark, SeekOrigin.Begin);
            }

#if DEBUG
            if (fileName != null)
            {
                StringBuilder output = new StringBuilder();

                for (int i = 0; i < txt.text.Count; i++)
                {
                    output.AppendLine(txt.categoryNames[i]);

                    for (int j = 0; j < txt.text[i].Count; j++)
                    {
                        output.AppendLine($"Group {j}");

                        for (int k = 0; k < txt.text[i][j].Count; k++)
                        {
                            var pair = txt.text[i][j][k];
                            output.AppendLine($"{pair.name} - {pair.str}");
                        }
                        output.AppendLine();
                    }
                    output.AppendLine();
                }

                File.WriteAllText(fileName + ".txt", output.ToString());
            }
#endif
            return txt;
        }

        public static void WritePSO2Text(string outname, string pso2TextTxtFile)
        {
            WritePSO2TextNIFL(outname, ReadPSO2TextFromTxt(pso2TextTxtFile));
        }
        public static void WritePSO2Text(string outname, PSO2Text pso2Text)
        {
            WritePSO2TextNIFL(outname, pso2Text);
        }

        //Expects somewhat strict formatting, but reads this from a .text file
        public static PSO2Text ReadPSO2TextFromTxt(string filename)
        {
            var lines = File.ReadAllLines(filename);
            PSO2Text text = new PSO2Text();

            //We start on line 3 to avoid the metadata text
            if(lines.Length < 4)
            {
                return null;
            }
            int mode = 0;
            for(int i = 3; i < lines.Length; i++)
            {
                switch (mode)
                {
                    case 0: //Category
                        if (lines[i] == "")
                        {
                            return text;
                        }
                        text.categoryNames.Add(lines[i]);
                        text.text.Add(new List<List<PSO2Text.textPair>>());
                        mode = 1;
                        break;
                    case 1: //Group
                        if (lines[i] == "")
                        {
                            mode = 0;
                        } else if (lines[i].Contains("Group"))
                        {
                            text.text[text.text.Count - 1].Add(new List<PSO2Text.textPair>());
                            mode = 2;
                        }
                        break;
                    case 2: //Text
                        if(lines[i] == "")
                        {
                            mode = 1;
                        } else if(lines[i].Contains('-'))
                        {
                            PSO2Text.textPair pair = new PSO2Text.textPair();
                            string[] line = lines[i].Split('-');

                            if(line[0][line[0].Length - 1] == ' ')
                            {
                                pair.name = line[0].Substring(0, line[0].Length - 1); //Get rid of the space
                            }

                            string schrodingersSpace = "";
                            if(line[1][0].ToString() == " ")
                            {
                                schrodingersSpace = " ";
                            }
                            pair.str = ReplaceFirst(lines[i], line[0] + "-" + schrodingersSpace, ""); //Safely get the next part, in theory.

                            text.text[text.text.Count - 1][text.text[text.text.Count - 1].Count - 1].Add(pair);
                        }
                        break;
                }

            }

            return text;
        }

        public static void WritePSO2TextNIFL(string outname, PSO2Text pso2Text)
        {
            if(pso2Text == null)
            {
                return;
            }
            List<byte> finalOutBytes = new List<byte>();

            int rel0SizeOffset = 0;
            int categoryOffset = 0;

            List<byte> outBytes = new List<byte>();
            List<PSO2Text.textPairLocation> textPairs = new List<PSO2Text.textPairLocation>();
            List<PSO2Text.textLocation> texts = new List<PSO2Text.textLocation>();
            List<Dictionary<string, int>> namePointers = new List<Dictionary<string, int>>();
            List<List<int>> textAddresses = new List<List<int>>();
            List<int> subCategoryAddress = new List<int>();
            List<List<int>> subCategoryNullAddresses = new List<List<int>>();
            List<int> nof0PointerLocations = new List<int>(); //Used for the NOF0 section
            
            //REL0
            outBytes.AddRange(Encoding.UTF8.GetBytes("REL0"));
            rel0SizeOffset = outBytes.Count; //We'll fill this later
            outBytes.AddRange(BitConverter.GetBytes(0));
            outBytes.AddRange(BitConverter.GetBytes(0));
            outBytes.AddRange(BitConverter.GetBytes(0));

            outBytes.AddRange(BitConverter.GetBytes(-1));

            //Write placeholders for the text pointers
            //Iterate through each category
            for(int cat = 0; cat < pso2Text.text.Count; cat++)
            {
                namePointers.Add(new Dictionary<string, int>());
                textAddresses.Add(new List<int>());
                for(int sub = 0; sub < pso2Text.text[cat].Count; sub++)
                {
                    //Add this for when we write it later
                    textAddresses[cat].Add(outBytes.Count);

                    for(int pair = 0; pair < pso2Text.text[cat][sub].Count; pair++)
                    {
                        NOF0Append(nof0PointerLocations, outBytes.Count, 1);
                        NOF0Append(nof0PointerLocations, outBytes.Count + 4, 1);
                        var pairLoc = new PSO2Text.textPairLocation();
                        pairLoc.address = outBytes.Count;
                        pairLoc.text = pso2Text.text[cat][sub][pair];
                        pairLoc.category = cat;
                        textPairs.Add(pairLoc);

                        //Add in placeholder values to fill later
                        outBytes.AddRange(new byte[8]);
                    }
                }
            }

            //Write the subcategory data
            for (int cat = 0; cat < pso2Text.text.Count; cat++)
            {
                subCategoryAddress.Add(outBytes.Count);
                for (int sub = 0; sub < pso2Text.text[cat].Count; sub++)
                {
                    NOF0Append(nof0PointerLocations, outBytes.Count, 1);
                    if (subCategoryNullAddresses.Count - 1 < sub)
                    {
                        subCategoryNullAddresses.Add(new List<int>());
                    }

                    if(pso2Text.text[cat][sub].Count > 0)
                    {
                        outBytes.AddRange(BitConverter.GetBytes(textAddresses[cat][sub]));
                    } else
                    {
                        subCategoryNullAddresses[sub].Add(outBytes.Count);
                        outBytes.AddRange(BitConverter.GetBytes(0));
                    }
                    outBytes.AddRange(BitConverter.GetBytes(sub));
                    outBytes.AddRange(BitConverter.GetBytes(pso2Text.text[cat][sub].Count));
                }
            }

            //Write the category data
            categoryOffset = outBytes.Count;
            for (int cat = 0; cat < pso2Text.text.Count; cat++)
            {
                var categoryName = new PSO2Text.textLocation();
                categoryName.address = outBytes.Count;
                categoryName.str = pso2Text.categoryNames[cat];
                texts.Add(categoryName);

                NOF0Append(nof0PointerLocations, outBytes.Count, 1);
                NOF0Append(nof0PointerLocations, outBytes.Count + 4, 1);
                outBytes.AddRange(new byte[4]);
                outBytes.AddRange(BitConverter.GetBytes(subCategoryAddress[cat]));
                outBytes.AddRange(BitConverter.GetBytes(pso2Text.text[cat].Count));
            }

            //Write header data
            SetByteListInt(outBytes, rel0SizeOffset + 4, outBytes.Count);
            NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(categoryOffset));
            outBytes.AddRange(BitConverter.GetBytes(pso2Text.text.Count));

            //Write main text as null terminated strings
            for(int i = 0; i < textPairs.Count; i++)
            {
                //Write the internal name
                if (namePointers[textPairs[i].category].ContainsKey(textPairs[i].text.name))
                {
                    SetByteListInt(outBytes, textPairs[i].address, namePointers[textPairs[i].category][textPairs[i].text.name]);
                } else
                {
                    SetByteListInt(outBytes, textPairs[i].address, outBytes.Count);
                    namePointers[textPairs[i].category].Add(textPairs[i].text.name, outBytes.Count);
                    outBytes.AddRange(Encoding.UTF8.GetBytes(textPairs[i].text.name));
                    outBytes.Add(0);
                    AlignWriter(outBytes, 0x4);
                }

                //Write the text
                SetByteListInt(outBytes, textPairs[i].address + 4, outBytes.Count);
                outBytes.AddRange(Encoding.Unicode.GetBytes(textPairs[i].text.str));
                outBytes.AddRange(BitConverter.GetBytes((ushort)0));
                AlignWriter(outBytes, 0x4);
            }

            //Write category text
            for (int i = 0; i < texts.Count; i++)
            {
                //Write the internal name
                SetByteListInt(outBytes, texts[i].address, outBytes.Count);
                outBytes.AddRange(Encoding.UTF8.GetBytes(texts[i].str));
                outBytes.Add(0);
                AlignWriter(outBytes, 0x4);
            }

            //Unknown data? Don't write if it's not needed. May just be debug related. Empty groups/subcategories point here, but it's possible you could direct them to a general 0.
            //NA .text files seem to write this slightly differently, writing only one of these while JP .text files write one for every category, seemingly.
            //For the sake of sanity and space, NA's style will be used for custom .text
            for(int i = 0; i < pso2Text.text[0].Count; i++)
            {
                if (pso2Text.text[0][0].Count > 0)
                {
                    outBytes.AddRange(BitConverter.GetBytes(i));
                } else
                {
                    for(int groupCounter = 0; groupCounter < subCategoryNullAddresses[i].Count; groupCounter++)
                    {
                        SetByteListInt(outBytes, subCategoryNullAddresses[i][groupCounter], outBytes.Count);
                    }
                    outBytes.AddRange(BitConverter.GetBytes(0));
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

            File.WriteAllBytes(outname, outBytes.ToArray());
        }

        public static PSO2Text GetTextConditional(string normalPath, string overridePath, string textFileName)
        {
            PSO2Text partsText = new PSO2Text();

            string partsPath = null;
            if (File.Exists(overridePath))
            {
                partsPath = overridePath;
            }
            else if (File.Exists(normalPath))
            {
                partsPath = normalPath;
            }
            else
            {
                return null;
            }

            if (partsPath != null)
            {
                var strm = new MemoryStream(File.ReadAllBytes(partsPath));
                var partsIce = IceFile.LoadIceFile(strm);
                strm.Dispose();

                List<byte[]> files = (new List<byte[]>(partsIce.groupOneFiles));
                files.AddRange(partsIce.groupTwoFiles);

                //Loop through files to get what we need
                foreach (byte[] file in files)
                {
                    if (IceFile.getFileName(file).ToLower().Contains(textFileName))
                    {
                        partsText = ReadPSO2Text(file);
                    }
                }

                partsIce = null;
            }

            return partsText;
        }

        public static LobbyActionCommon ReadLAC(string fileName)
        {
            using (Stream stream = (Stream)new FileStream(fileName, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                return BeginReadLAC(streamReader);
            }
        }
        public static LobbyActionCommon ReadLAC(byte[] file)
        {
            using (Stream stream = (Stream)new MemoryStream(file))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                return BeginReadLAC(streamReader);
            }
        }

        private static LobbyActionCommon BeginReadLAC(BufferedStreamReader streamReader)
        {
            string type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
            int offset = 0x20; //Base offset due to NIFL header

            //Deal with deicer's extra header nonsense
            if (type.Equals("lac\0"))
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
                return ReadNIFLLAC(streamReader, offset);
            }
            else if (type.Equals("VTBF"))
            {
                //Lacs should really never be VTBF...
            }
            else
            {
                MessageBox.Show("Improper File Format!");
            }

            return null;
        }

        public static LobbyActionCommon ReadNIFLLAC(BufferedStreamReader streamReader, int offset)
        {
            var lac = new LobbyActionCommon();
            var nifl = streamReader.Read<AquaCommon.NIFL>();
            var end = nifl.NOF0Offset + offset;
            var rel0 = streamReader.Read<AquaCommon.REL0>();

            streamReader.Seek(rel0.REL0DataStart + offset, SeekOrigin.Begin);
            lac.header = streamReader.Read<LobbyActionCommon.lacHeader>();

            streamReader.Seek(lac.header.dataInfoPointer + offset, SeekOrigin.Begin);
            lac.info = streamReader.Read<LobbyActionCommon.dataInfo>();

            streamReader.Seek(lac.info.blockOffset + offset, SeekOrigin.Begin);

            for (int i = 0; i < lac.info.blockCount; i++)
            {
                var block = streamReader.Read<LobbyActionCommon.dataBlock>();
                long bookmark = streamReader.Position();

                lac.dataBlocks.Add(LobbyActionCommon.ReadDataBlock(streamReader, offset, block));
                streamReader.Seek(bookmark, SeekOrigin.Begin);
            }

            return lac;
        }

        public static int NOF0Append(List<int> nof0, int currentOffset, int countToCheck = 1, int subtractedOffset = 0)
        {
            if (countToCheck < 1)
            {
                return -1;
            }
            int newAddress = currentOffset - subtractedOffset;
            nof0.Add(newAddress);

            return newAddress;
        }

        //https://stackoverflow.com/questions/8809354/replace-first-occurrence-of-pattern-in-a-string
        public static string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        //Mainly for handling pointer offsets
        public static int SetByteListInt(List<byte> outBytes, int offset, int value)
        {
            if (offset != -1)
            {
                var newBytes = BitConverter.GetBytes(value);
                for (int i = 0; i < 4; i++)
                {
                    outBytes[offset + i] = newBytes[i];
                }

                return value;
            }

            return -1;
        }

        public static void AlignReader(BufferedStreamReader streamReader, int align)
        {
            //Align to int align
            while (streamReader.Position() % align > 0)
            {
                streamReader.Read<byte>();
            }
        }

        public static int AlignWriter(List<byte> outBytes, int align)
        {
            //Align to int align
            int additions = 0;
            while (outBytes.Count % align > 0)
            {
                additions++;
                outBytes.Add(0);
            }

            return additions;
        }

        public static void AlignFileEndWrite(List<byte> outBytes, int align)
        {
            if (outBytes.Count % align == 0)
            {
                for (int i = 0; i < 0x10; i++)
                {
                    outBytes.Add(0);
                }
            }
            else
            {
                //Align to 0x10
                while (outBytes.Count % align > 0)
                {
                    outBytes.Add(0);
                }
            }
        }
    }
}
