using AquaModelLibrary.AquaStructs;
using AquaModelLibrary.OtherStructs;
using Reloaded.Memory.Streams;
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
using static AquaModelLibrary.AquaStructs.LandPieceSettings;
using static AquaModelLibrary.VTBFMethods;

namespace AquaModelLibrary
{
    public class AquaMiscMethods
    {

        public static AquaBTI_MotionConfig LoadBTI(string inFilename)
        {
            AquaBTI_MotionConfig bti = new AquaBTI_MotionConfig();

            AquaPackage.AFPMain afp = new AquaPackage.AFPMain();
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
                variant = ReadAquaHeader(streamReader, ext, variant, out offset, afp);

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

                    //Thanks, SEA servers...
                    while (subCategoryId >= txt.text[i].Count)
                    {
                        txt.text[i].Add(new List<PSO2Text.textPair>());
                    }

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
        public static byte[] GetEngToJPTextAsBytes(PSO2Text pso2TextNA, PSO2Text pso2TextJP)
        {
            return PSO2TextToNIFLBytes(SyncNAToJPText(pso2TextNA, pso2TextJP)).ToArray();
        }

        //Expects somewhat strict formatting, but reads this from a .text file
        public static PSO2Text ReadPSO2TextFromTxt(string filename)
        {
            var lines = File.ReadAllLines(filename);
            PSO2Text text = new PSO2Text();

            //We start on line 3 to avoid the metadata text
            if (lines.Length < 4)
            {
                return new PSO2Text();
            }
            int mode = 0;
            for (int i = 3; i < lines.Length; i++)
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
                        }
                        else if (lines[i].Contains("Group"))
                        {
                            text.text[text.text.Count - 1].Add(new List<PSO2Text.textPair>());
                            mode = 2;
                        }
                        break;
                    case 2: //Text
                        if (lines[i] == "")
                        {
                            mode = 1;
                        }
                        else if (lines[i].Contains('-'))
                        {
                            PSO2Text.textPair pair = new PSO2Text.textPair();
                            string[] line = lines[i].Split('-');

                            //Handle - being used as the name for whatever reason. Should be an uncommon issue
                            if (line[0] == "")
                            {
                                pair.name = "-";
                                lines[i] = ReplaceFirst(lines[i], "- ", "");
                            }
                            else if (line[0][line[0].Length - 1] == ' ')
                            {
                                pair.name = line[0].Substring(0, line[0].Length - 1); //Get rid of the space
                            }

                            string schrodingersSpace = "";
                            if (line[1][0].ToString() == " ")
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

        public static PSO2Text SyncNAToJPText(PSO2Text pso2TextNA, PSO2Text pso2TextJP)
        {
            
            for(int i = 0; i < pso2TextNA.text.Count; i++)
            {
                pso2TextNA.text[i].RemoveAt(0);
            }
            for(int i = 0; i < pso2TextJP.categoryNames.Count; i++)
            {
                if(!pso2TextNA.categoryNames.Contains(pso2TextJP.categoryNames[i]))
                {
                    pso2TextNA.categoryNames.Insert(i, pso2TextJP.categoryNames[i]);
                    pso2TextNA.text.Insert(i, pso2TextJP.text[i]); //Just plop the JP one in if the whole category is missing
                }
            }

            for (int i = 0; i < pso2TextJP.text.Count; i++)
            {
                //We only care about group 0, JP language, in this case
                if(pso2TextJP.text[i].Count > 0)
                {
                    List<string> idCheck = new List<string>();
                    if(pso2TextNA.text[i].Count > 0)
                    {
                        for(int id = 0; id < pso2TextNA.text[i][0].Count; id++)
                        {
                            idCheck.Add(pso2TextNA.text[i][0][id].name);
                        }
                    }
                    for(int j = 0; j < pso2TextJP.text[i][0].Count; j++)
                    {
                        //This really should be a dictionary check, but whoever designed this format allowed for funny business with the 'keys'
                        if(!idCheck.Contains(pso2TextJP.text[i][0][j].name))
                        {
                            pso2TextNA.text[i][0].Add(pso2TextJP.text[i][0][j]);
                        } else if(pso2TextNA.text[i][0][j].str == "" || pso2TextNA.text[i][0][j].str.Contains("***") || pso2TextNA.text[i][0][j].str.Contains("＊＊＊")) //Use JP text if text is redacted
                        {
                            var text = pso2TextNA.text[i][0][j];
                            text.str = pso2TextNA.text[i][0][j].str;
                            pso2TextNA.text[i][0][j] = text;
                        }
                    }
                }
            }

            return pso2TextNA;
        }

        public static void WritePSO2TextNIFL(string outname, PSO2Text pso2Text)
        {
            if (pso2Text == null)
            {
                return;
            }
            List<byte> outBytes = PSO2TextToNIFLBytes(pso2Text);

            File.WriteAllBytes(outname, outBytes.ToArray());
        }

        public static void AddOntoDict(Dictionary<string, List<int>> dict, List<string> strList, string str, int address)
        {
            str = str ?? "";
            if(dict.ContainsKey(str))
            {
                dict[str].Add(address);
            } else
            {
                strList.Add(str);
                dict.Add(str, new List<int>() { address });
            }
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

        public static List<byte> PSO2TextToNIFLBytes(PSO2Text pso2Text)
        {
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
            for (int cat = 0; cat < pso2Text.text.Count; cat++)
            {
                namePointers.Add(new Dictionary<string, int>());
                textAddresses.Add(new List<int>());
                for (int sub = 0; sub < pso2Text.text[cat].Count; sub++)
                {
                    //Add this for when we write it later
                    textAddresses[cat].Add(outBytes.Count);

                    for (int pair = 0; pair < pso2Text.text[cat][sub].Count; pair++)
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

                    if (pso2Text.text[cat][sub].Count > 0)
                    {
                        outBytes.AddRange(BitConverter.GetBytes(textAddresses[cat][sub]));
                    }
                    else
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
            if(pso2Text.text.Count > 0 || pso2Text.categoryNames.Count > 0)
            {
                outBytes.AddRange(BitConverter.GetBytes(categoryOffset));
                outBytes.AddRange(BitConverter.GetBytes(pso2Text.text.Count));
            } else //Handle empty files like retail
            {
                outBytes.AddRange(BitConverter.GetBytes(0x24));
                outBytes.AddRange(BitConverter.GetBytes(0));
                outBytes.AddRange(BitConverter.GetBytes(0));
                outBytes.AddRange(BitConverter.GetBytes(1));
                outBytes.AddRange(BitConverter.GetBytes(0));
            }

            //Write main text as null terminated strings
            for (int i = 0; i < textPairs.Count; i++)
            {
                //Write the internal name
                if (namePointers[textPairs[i].category].ContainsKey(textPairs[i].text.name))
                {
                    SetByteListInt(outBytes, textPairs[i].address, namePointers[textPairs[i].category][textPairs[i].text.name]);
                }
                else
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
            if(pso2Text.text.Count > 0)
            {
                for (int i = 0; i < pso2Text.text[0].Count; i++)
                {
                    if (pso2Text.text[0][0].Count > 0)
                    {
                        outBytes.AddRange(BitConverter.GetBytes(i));
                    }
                    else
                    {
                        for (int groupCounter = 0; groupCounter < subCategoryNullAddresses[i].Count; groupCounter++)
                        {
                            SetByteListInt(outBytes, subCategoryNullAddresses[i][groupCounter], outBytes.Count);
                        }
                        outBytes.AddRange(BitConverter.GetBytes(0));
                    }
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
        public static LobbyActionCommon ReadRebootLAC(string fileName)
        {
            using (Stream stream = (Stream)new FileStream(fileName, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                return BeginReadRebootLAC(streamReader);
            }
        }
        public static LobbyActionCommon ReadRebootLAC(byte[] file)
        {
            using (Stream stream = (Stream)new MemoryStream(file))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                return BeginReadRebootLAC(streamReader);
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

        private static LobbyActionCommon BeginReadRebootLAC(BufferedStreamReader streamReader)
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
                return ReadNIFLRebootLAC(streamReader, offset);
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

        public static LobbyActionCommon ReadNIFLRebootLAC(BufferedStreamReader streamReader, int offset)
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
                var block = streamReader.Read<LobbyActionCommon.dataBlockReboot>();
                long bookmark = streamReader.Position();

                lac.rebootDataBlocks.Add(LobbyActionCommon.ReadDataBlockReboot(streamReader, offset, block));
                streamReader.Seek(bookmark, SeekOrigin.Begin);
            }

            return lac;
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
                MessageBox.Show("Error, NIFL .mgx found");
                return null;
            }
            else if (type.Equals("VTBF"))
            {
                return ReadVTBFMGX(streamReader);
            }
            else
            {
                MessageBox.Show("Improper File Format!");
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

        public static AddOnIndex ReadAOX(string inFilename)
        {
            using (Stream stream = (Stream)new FileStream(inFilename, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                return BeginReadAOX(streamReader);
            }
        }
        public static AddOnIndex ReadAOX(byte[] file)
        {
            using (Stream stream = (Stream)new MemoryStream(file))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                return BeginReadAOX(streamReader);
            }
        }

        private static AddOnIndex BeginReadAOX(BufferedStreamReader streamReader)
        {
            string type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
            int offset = 0x20; //Base offset due to NIFL header

            //Deal with deicer's extra header nonsense
            if (type.Equals("aox\0"))
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
                MessageBox.Show("Error, NIFL .aox found");
                return null;
            }
            else if (type.Equals("VTBF"))
            {
                return ReadVTBFAOX(streamReader);
            }
            else
            {
                MessageBox.Show("Improper File Format!");
                return null;
            }
        }

        public static AddOnIndex ReadVTBFAOX(BufferedStreamReader streamReader)
        {
            AddOnIndex aoi = new AddOnIndex();

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
                    case "ADDO":
                        aoi.addonList.Add(parseADDO(data));
                        break;
                    default:
                        //Data being null signfies that the last thing read wasn't a proper tag. This should mean the end of the VTBF stream if nothing else.
                        if (data == null)
                        {
                            return aoi;
                        }
                        throw new System.Exception($"Unexpected tag at {streamReader.Position().ToString("X")}! {tagType} Please report!");
                }
            }

            return aoi;
        }

        public static MyRoomParameters ReadMyRoomParam(string fileName)
        {
            using (Stream stream = (Stream)new FileStream(fileName, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                int type = 0;
                if(fileName.Contains("roomgoods"))
                {
                    type = 0;
                } else if(fileName.Contains("chip"))
                {
                    type = 1;
                }
                return BeginReadMyRoomParam(streamReader, type);
            }
        }

        public static MyRoomParameters ReadMyRoomParam(byte[] file, int type)
        {
            using (Stream stream = (Stream)new MemoryStream(file))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                return BeginReadMyRoomParam(streamReader, type);
            }
        }

        public static MyRoomParameters BeginReadMyRoomParam(BufferedStreamReader streamReader, int mrpType)
        {
            string type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
            int offset = 0x20; //Base offset due to NIFL header

            //Deal with deicer's extra header nonsense
            if (type.Equals("mrp\0"))
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
                return ReadMyRoomParam(streamReader, offset, mrpType);
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

        public static MyRoomParameters ReadMyRoomParam(BufferedStreamReader streamReader, int offset, int mrpType)
        {
            var mrp = new MyRoomParameters();
            var nifl = streamReader.Read<AquaCommon.NIFL>();
            var end = nifl.NOF0Offset + offset;
            var rel0 = streamReader.Read<AquaCommon.REL0>();
            streamReader.Seek(rel0.REL0DataStart + offset, SeekOrigin.Begin);

            var count = streamReader.Read<int>();
            var mainOffset = streamReader.Read<int>();
            long bookmark;

            streamReader.Seek(mainOffset + offset, SeekOrigin.Begin);
            for(int i = 0; i < count; i++)
            {
                switch(mrpType)
                {
                    case 0:
                        MyRoomParameters.RoomGoodsObject rgobj = new MyRoomParameters.RoomGoodsObject();
                        rgobj.goods = streamReader.Read<MyRoomParameters.RoomGoods>();
                        bookmark = streamReader.Position();

                        streamReader.Seek(rgobj.goods.categoryPtr + offset, SeekOrigin.Begin);
                        rgobj.categoryString = ReadCString(streamReader);
                        streamReader.Seek(rgobj.goods.allPtr + offset, SeekOrigin.Begin);
                        rgobj.allString = ReadCString(streamReader);
                        streamReader.Seek(rgobj.goods.categoryPtr2 + offset, SeekOrigin.Begin);
                        rgobj.categoryString2 = ReadCString(streamReader);
                        streamReader.Seek(rgobj.goods.functionStrPtr + offset, SeekOrigin.Begin);
                        rgobj.functionString = ReadCString(streamReader);
                        streamReader.Seek(rgobj.goods.motionPtr + offset, SeekOrigin.Begin);
                        rgobj.motionType = ReadCString(streamReader);
                        streamReader.Seek(rgobj.goods.unknownPtr + offset, SeekOrigin.Begin);
                        rgobj.unknownString = ReadCString(streamReader);

                        streamReader.Seek(bookmark, SeekOrigin.Begin);
                        mrp.roomGoodsList.Add(rgobj);
                        break;
                    case 1:
                        MyRoomParameters.ChipObject chipobj = new MyRoomParameters.ChipObject();
                        chipobj.chip = streamReader.Read<MyRoomParameters.Chip>();
                        bookmark = streamReader.Position();

                        streamReader.Seek(chipobj.chip.objectPtr + offset, SeekOrigin.Begin);
                        chipobj.objectString = ReadCString(streamReader);
                        streamReader.Seek(chipobj.chip.collisionTypePtr + offset, SeekOrigin.Begin);
                        chipobj.collisionTypeString = ReadCString(streamReader);
                        streamReader.Seek(chipobj.chip.unkStringPtr + offset, SeekOrigin.Begin);
                        chipobj.unknownString = ReadCString(streamReader);

                        streamReader.Seek(bookmark, SeekOrigin.Begin);
                        mrp.chipsList.Add(chipobj);
                        break;
                }
            }

            return mrp;
        }

        public static LandPieceSettings ReadLPS(string inFilename)
        {
            using (Stream stream = (Stream)new FileStream(inFilename, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                return BeginReadLPS(streamReader);
            }
        }
        public static LandPieceSettings ReadLPS(byte[] file)
        {
            using (Stream stream = (Stream)new MemoryStream(file))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                return BeginReadLPS(streamReader);
            }
        }

        private static LandPieceSettings BeginReadLPS(BufferedStreamReader streamReader)
        {
            string type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
            int offset = 0x20; //Base offset due to NIFL header

            //Deal with deicer's extra header nonsense
            if (type.Equals("lps\0"))
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
                return ReadNIFLLPS(streamReader, offset);
            }
            else if (type.Equals("VTBF"))
            {
                MessageBox.Show("Error, VTBF .lps found");
                return null;
            }
            else
            {
                MessageBox.Show("Improper File Format!");
                return null;
            }
        }

        public static LandPieceSettings ReadNIFLLPS(BufferedStreamReader streamReader, int offset)
        {
            var lps = new LandPieceSettings();
            var nifl = streamReader.Read<AquaCommon.NIFL>();
            var rel0 = streamReader.Read<AquaCommon.REL0>();
            streamReader.Seek(nifl.NOF0OffsetFull + offset - 0x20, SeekOrigin.Begin);
            var nof0 = AquaCommon.readNOF0(streamReader);
            var nof0Values = AquaCommon.GetNOF0PointedValues(nof0, streamReader, offset);
            nof0Values.Sort();
            streamReader.Seek(rel0.REL0DataStart + offset, SeekOrigin.Begin);

            lps.header = streamReader.Read<LandPieceSettings.LPSHeader>();

            streamReader.Seek(lps.header.floatVariablesPtr + offset, SeekOrigin.Begin);
            for (int i = 0; i < lps.header.floatVariablesCount; i++)
            {
                var strPtr = streamReader.Read<int>();
                float flt = streamReader.Read<float>();
                var bookmark = streamReader.Position();
                streamReader.Seek(strPtr + offset, SeekOrigin.Begin);
                var str = ReadCString(streamReader);
                if(str == null)
                {
                    str = $"value_{i}";
                }
                lps.fVarDict.Add(str, flt);

                streamReader.Seek(bookmark, SeekOrigin.Begin);
            }
            streamReader.Seek(lps.header.stringVariablesPtr + offset, SeekOrigin.Begin);
            for (int i = 0; i < lps.header.stringVariablesCount; i++)
            {
                var strPtr = streamReader.Read<int>();
                var strPtr2 = streamReader.Read<int>();
                var bookmark = streamReader.Position();
                streamReader.Seek(strPtr + offset, SeekOrigin.Begin);
                var str = ReadCString(streamReader);
                streamReader.Seek(strPtr2 + offset, SeekOrigin.Begin);
                var str2 = ReadCString(streamReader);
                if (str == null)
                {
                    str = $"value_{i}";
                }
                lps.stringVarDict.Add(str, str2);

                streamReader.Seek(bookmark, SeekOrigin.Begin);
            }
            streamReader.Seek(lps.header.areaEntryExitDefaultsPtr + offset, SeekOrigin.Begin);
            for (int i = 0; i < lps.header.areaEntryExitDefaultsCount; i++)
            {
                var strPtr = streamReader.Read<int>();
                var strPtr2 = streamReader.Read<int>();
                var bookmark = streamReader.Position();
                streamReader.Seek(strPtr + offset, SeekOrigin.Begin);
                var str = ReadCString(streamReader);
                streamReader.Seek(strPtr2 + offset, SeekOrigin.Begin);
                var str2 = ReadCString(streamReader);
                if (str == null)
                {
                    str = $"value_{i}";
                }
                lps.areaEntryExitDefaults.Add(str, str2);

                streamReader.Seek(bookmark, SeekOrigin.Begin);
            }
            streamReader.Seek(lps.header.areaEntryExitDefaults2Ptr + offset, SeekOrigin.Begin);
            for (int i = 0; i < lps.header.areaEntryExitDefaults2Count; i++)
            {
                var strPtr = streamReader.Read<int>();
                var strPtr2 = streamReader.Read<int>();
                var bookmark = streamReader.Position();
                streamReader.Seek(strPtr + offset, SeekOrigin.Begin);
                var str = ReadCString(streamReader);
                streamReader.Seek(strPtr2 + offset, SeekOrigin.Begin);
                var str2 = ReadCString(streamReader);
                if (str == null)
                {
                    str = $"value_{i}";
                }
                lps.areaEntryExitDefaults2.Add(str, str2);

                streamReader.Seek(bookmark, SeekOrigin.Begin);
            }
            streamReader.Seek(lps.header.pieceSetsPtr + offset, SeekOrigin.Begin);
            for (int i = 0; i < lps.header.pieceSetsCount; i++)
            {
                var pieceSetObj = new PieceSetObj();
                pieceSetObj.offset = streamReader.Position() - offset;

                var pieceSet = streamReader.Read<PieceSet>();
                var bookmark = streamReader.Position();
                streamReader.Seek(pieceSet.namePtr + offset, SeekOrigin.Begin);
                pieceSetObj.name = ReadCString(streamReader);
                streamReader.Seek(pieceSet.fullNamePtr + offset, SeekOrigin.Begin);
                pieceSetObj.fullName = ReadCString(streamReader);

                //Read data 1 - Hack for now. Seems like it's when pieceSet.int_10 is 0xB, but hard to say
                var nofIdData1 = nof0Values.IndexOf((uint)pieceSet.data1Ptr);

                int data1Size;
                if (nofIdData1 + 1 != nof0Values.Count)
                {
                    data1Size = (int)(nof0Values[nofIdData1 + 1] - nof0Values[nofIdData1]);
                }
                else
                {
                    data1Size = pieceSet.data1Ptr == 0xB ? 0x8 : 0x4;
                }
                switch (data1Size)
                {
                    case 0x8:
                        pieceSetObj.data1 = streamReader.ReadBytes(pieceSet.data1Ptr + offset, 0x8);
                        break;
                    case 0x4:
                        pieceSetObj.data1 = streamReader.ReadBytes(pieceSet.data1Ptr + offset, 0x4);
                        break;
                    default:
                        Debug.WriteLine($"Piece Set at {pieceSetObj.offset:X} had a data 1 size of {data1Size}");
                        pieceSetObj.data1 = streamReader.ReadBytes(pieceSet.data1Ptr + offset, data1Size);
                        break;
                }

                //Read data 2
                var nofIdData2 = nof0Values.IndexOf((uint)pieceSet.data2Ptr);
                int data2Size;
                if (nofIdData2 + 1 != nof0Values.Count)
                {
                    data2Size = (int)(nof0Values[nofIdData2 + 1] - nof0Values[nofIdData2]);
                } else
                {
                    data2Size = 0x4;
                }
                switch (data2Size)
                {
                    case 0x8:
                        pieceSetObj.data2 = streamReader.ReadBytes(pieceSet.data2Ptr + offset, 0x8);
                        break;
                    case 0x4:
                        pieceSetObj.data2 = streamReader.ReadBytes(pieceSet.data2Ptr + offset, 0x4);
                        break;
                    default:
                        Debug.WriteLine($"Piece Set at {pieceSetObj.offset:X} had a data 2 size of {data2Size}");
                        pieceSetObj.data2 = streamReader.ReadBytes(pieceSet.data2Ptr + offset, data2Size);
                        break;
                }

                streamReader.Seek(bookmark, SeekOrigin.Begin);

                pieceSetObj.pieceSet = pieceSet;
                lps.pieceSets.Add(pieceSetObj.name, pieceSetObj);
            }

            return lps;
        }

        public static ELPR_EnlightenParam ReadELPR(string inFilename)
        {
            using (Stream stream = (Stream)new FileStream(inFilename, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                return BeginReadELPR(streamReader);
            }
        }
        public static ELPR_EnlightenParam ReadELPR(byte[] file)
        {
            using (Stream stream = (Stream)new MemoryStream(file))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                return BeginReadELPR(streamReader);
            }
        }

        private static ELPR_EnlightenParam BeginReadELPR(BufferedStreamReader streamReader)
        {
            string type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
            int offset = 0x0; //Base offset due to NIFL header

            //Deal with deicer's extra header nonsense
            if (type.Equals("elpr"))
            {
                streamReader.Seek(0xC, SeekOrigin.Begin);
                //Basically always 0x60, but some deicer files from the Alpha have 0x50... 
                int headJunkSize = streamReader.Read<int>();

                streamReader.Seek(headJunkSize - 0x10, SeekOrigin.Current);
                type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
                offset += headJunkSize;
            }

            return ReadELPR(streamReader, offset);
        }

        private static ELPR_EnlightenParam ReadELPR(BufferedStreamReader streamReader, int offset)
        {
            ELPR_EnlightenParam elpr = new ELPR_EnlightenParam();
            var type = Encoding.UTF8.GetString(streamReader.ReadBytes(streamReader.Position(), 4));
            streamReader.Seek(0x6, SeekOrigin.Current);
            var count = streamReader.Read<ushort>();
            streamReader.Seek(0x8, SeekOrigin.Current);

            for (int i = 0; i < count; i++)
            {
                ELPR_EnlightenParam.ElprPiece piece = new ELPR_EnlightenParam.ElprPiece();
                piece.name0x18 = ReadCString(streamReader);
                streamReader.Seek(0x18, SeekOrigin.Current);
                piece.usht_18 = streamReader.Read<ushort>();
                piece.usht_1A = streamReader.Read<ushort>();
                piece.int_1C = streamReader.Read<int>();
                piece.vec3_20 = streamReader.Read<Vector3>();
                piece.int_2C = streamReader.Read<int>();
                piece.vec3_30 = streamReader.Read<Vector3>();
                piece.int_3C = streamReader.Read<int>();

                elpr.elprList.Add(piece);
            }

            return elpr;
        }
    }
}
