using AquaModelLibrary.Data.PSO2.Aqua.AquaCommonData;
using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers;
using AquaModelLibrary.Helpers.Extensions;
using System.Text;
using Zamboni;
using System.IO;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    public class PSO2Text : AquaCommon
    {
        public List<string> categoryNames = new List<string>();
        public List<List<List<TextPair>>> text = new List<List<List<TextPair>>>(); //Category, subCategory, id
        public override string[] GetEnvelopeTypes()
        {
            return new string[] {
            "text"
            };
        }
        public PSO2Text() { }
        public PSO2Text(byte[] file) : base(file) { }

        public PSO2Text(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }

        public PSO2Text(string filename)
        {
            var lines = File.ReadAllLines(filename);

            //We start on line 3 to avoid the metadata text
            if (lines.Length < 4)
            {
                return;
            }
            int mode = 0;
            for (int i = 3; i < lines.Length; i++)
            {
                switch (mode)
                {
                    case 0: //Category
                        if (lines[i] == "")
                        {
                            return;
                        }
                        categoryNames.Add(lines[i]);
                        text.Add(new List<List<TextPair>>());
                        mode = 1;
                        break;
                    case 1: //Group
                        if (lines[i] == "")
                        {
                            mode = 0;
                        }
                        else if (lines[i].Contains("Group"))
                        {
                            text[text.Count - 1].Add(new List<TextPair>());
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
                            PSO2Text.TextPair pair = new TextPair();
                            string[] line = lines[i].Split('-');

                            //Handle - being used as the name for whatever reason. Should be an uncommon issue
                            if (line[0] == "")
                            {
                                pair.name = "-";
                                lines[i] = StringHelpers.ReplaceFirst(lines[i], "- ", "");
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
                            pair.str = StringHelpers.ReplaceFirst(lines[i], line[0] + "-" + schrodingersSpace, ""); //Safely get the next part, in theory.

                            text[text.Count - 1][text[text.Count - 1].Count - 1].Add(pair);
                        }
                        break;
                }

            }
        }

        public override void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            var categoryPointer = sr.Read<int>();
            var categoryCount = sr.Read<int>();

            //Read through categories
            sr.Seek(categoryPointer + offset, SeekOrigin.Begin);
            for (int i = 0; i < categoryCount; i++)
            {
                var categoryNameOffset = sr.Read<int>();
                var categoryDataInfoOffset = sr.Read<int>();
                var subCategoryCount = sr.Read<int>();

                //Setup subcategory lists
                text.Add(new List<List<TextPair>>());
                for (int j = 0; j < subCategoryCount; j++)
                {
                    text[i].Add(new List<TextPair>());
                }

                //Get category name
                long bookmark = sr.Position;
                sr.Seek(categoryNameOffset + offset, SeekOrigin.Begin);

                categoryNames.Add(sr.ReadCString());

                //Get Category Info
                sr.Seek(categoryDataInfoOffset + offset, SeekOrigin.Begin);

                for (int sub = 0; sub < subCategoryCount; sub++)
                {
                    var categoryIndexOffset = sr.Read<int>();
                    var subCategoryId = sr.Read<int>();

                    //Thanks, SEA servers...
                    while (subCategoryId >= text[i].Count)
                    {
                        text[i].Add(new List<TextPair>());
                    }

                    var categoryIndexCount = sr.Read<int>();
                    var bookMarkSub = sr.Position;

                    sr.Seek(categoryIndexOffset + offset, SeekOrigin.Begin);
                    for (int j = 0; j < categoryIndexCount; j++)
                    {
                        var pair = new TextPair();
                        int nameLoc = sr.Read<int>();
                        int textLoc = sr.Read<int>();
                        long bookmarkLocal = sr.Position;

                        sr.Seek(nameLoc + offset, SeekOrigin.Begin);
                        pair.name = sr.ReadCString();

                        sr.Seek(textLoc + offset, SeekOrigin.Begin);
                        pair.str = sr.ReadUTF16String(true, (int)sr.BaseStream.Length);

                        text[i][subCategoryId].Add(pair);
                        sr.Seek(bookmarkLocal, SeekOrigin.Begin);
                    }
                    sr.Seek(bookMarkSub, SeekOrigin.Begin);
                }

                sr.Seek(bookmark, SeekOrigin.Begin);
            }
        }

        public struct TextPair
        {
            public string name;
            public string str;
        }

        //For helping with writing
        public struct TextPairLocation
        {
            public int address;
            public TextPair text;
            public int category;
        }

        public struct TextLocation
        {
            public int address;
            public string str;
        }

        public override byte[] GetBytesNIFL()
        {
            int rel0SizeOffset = 0;
            int categoryOffset = 0;

            List<byte> outBytes = new List<byte>();
            List<TextPairLocation> textPairs = new List<TextPairLocation>();
            List<TextLocation> texts = new List<TextLocation>();
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
            for (int cat = 0; cat < text.Count; cat++)
            {
                namePointers.Add(new Dictionary<string, int>());
                textAddresses.Add(new List<int>());
                for (int sub = 0; sub < text[cat].Count; sub++)
                {
                    //Add this for when we write it later
                    textAddresses[cat].Add(outBytes.Count);

                    for (int pair = 0; pair < text[cat][sub].Count; pair++)
                    {
                        DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
                        DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 4, 1);
                        var pairLoc = new PSO2Text.TextPairLocation();
                        pairLoc.address = outBytes.Count;
                        pairLoc.text = text[cat][sub][pair];
                        pairLoc.category = cat;
                        textPairs.Add(pairLoc);

                        //Add in placeholder values to fill later
                        outBytes.AddRange(new byte[8]);
                    }
                }
            }

            //Write the subcategory data
            for (int cat = 0; cat < text.Count; cat++)
            {
                subCategoryAddress.Add(outBytes.Count);
                for (int sub = 0; sub < text[cat].Count; sub++)
                {
                    DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
                    if (subCategoryNullAddresses.Count - 1 < sub)
                    {
                        subCategoryNullAddresses.Add(new List<int>());
                    }

                    if (text[cat][sub].Count > 0)
                    {
                        outBytes.AddRange(BitConverter.GetBytes(textAddresses[cat][sub]));
                    }
                    else
                    {
                        subCategoryNullAddresses[sub].Add(outBytes.Count);
                        outBytes.AddRange(BitConverter.GetBytes(0));
                    }
                    outBytes.AddRange(BitConverter.GetBytes(sub));
                    outBytes.AddRange(BitConverter.GetBytes(text[cat][sub].Count));
                }
            }

            //Write the category data
            categoryOffset = outBytes.Count;
            for (int cat = 0; cat < text.Count; cat++)
            {
                var categoryName = new PSO2Text.TextLocation();
                categoryName.address = outBytes.Count;
                categoryName.str = categoryNames[cat];
                texts.Add(categoryName);

                DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
                DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 4, 1);
                outBytes.AddRange(new byte[4]);
                outBytes.AddRange(BitConverter.GetBytes(subCategoryAddress[cat]));
                outBytes.AddRange(BitConverter.GetBytes(text[cat].Count));
            }

            //Write header data
            outBytes.SetByteListInt(rel0SizeOffset + 4, outBytes.Count);
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            if (text.Count > 0 || categoryNames.Count > 0)
            {
                outBytes.AddRange(BitConverter.GetBytes(categoryOffset));
                outBytes.AddRange(BitConverter.GetBytes(text.Count));
            }
            else //Handle empty files like retail
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
                    outBytes.SetByteListInt(textPairs[i].address, namePointers[textPairs[i].category][textPairs[i].text.name]);
                }
                else
                {
                    outBytes.SetByteListInt(textPairs[i].address, outBytes.Count);
                    namePointers[textPairs[i].category].Add(textPairs[i].text.name, outBytes.Count);
                    outBytes.AddRange(Encoding.UTF8.GetBytes(textPairs[i].text.name));
                    outBytes.Add(0);
                    outBytes.AlignWriter(0x4);
                }

                //Write the text
                outBytes.SetByteListInt(textPairs[i].address + 4, outBytes.Count);
                outBytes.AddRange(Encoding.Unicode.GetBytes(textPairs[i].text.str));
                outBytes.AddRange(BitConverter.GetBytes((ushort)0));
                outBytes.AlignWriter(0x4);
            }

            //Write category text
            for (int i = 0; i < texts.Count; i++)
            {
                //Write the internal name
                outBytes.SetByteListInt(texts[i].address, outBytes.Count);
                outBytes.AddRange(Encoding.UTF8.GetBytes(texts[i].str));
                outBytes.Add(0);
                outBytes.AlignWriter(0x4);
            }

            //Unknown data? Don't write if it's not needed. May just be debug related. Empty groups/subcategories point here, but it's possible you could direct them to a general 0.
            //NA .text files seem to write this slightly differently, writing only one of these while JP .text files write one for every category, seemingly.
            //For the sake of sanity and space, NA's style will be used for custom .text
            if (text.Count > 0)
            {
                for (int i = 0; i < text[0].Count; i++)
                {
                    if (text[0][0].Count > 0)
                    {
                        outBytes.AddRange(BitConverter.GetBytes(i));
                    }
                    else
                    {
                        for (int groupCounter = 0; groupCounter < subCategoryNullAddresses[i].Count; groupCounter++)
                        {
                            outBytes.SetByteListInt(subCategoryNullAddresses[i][groupCounter], outBytes.Count);
                        }
                        outBytes.AddRange(BitConverter.GetBytes(0));
                    }
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

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            for (int i = 0; i < text.Count; i++)
            {
                output.AppendLine(categoryNames[i]);

                for (int j = 0; j < text[i].Count; j++)
                {
                    output.AppendLine($"Group {j}");

                    for (int k = 0; k < text[i][j].Count; k++)
                    {
                        var pair = text[i][j][k];
                        output.AppendLine($"{pair.name} - {pair.str}");
                    }
                    output.AppendLine();
                }
                output.AppendLine();
            }

            return output.ToString();
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
                        partsText = new PSO2Text(file);
                    }
                }

                partsIce = null;
            }

            return partsText;
        }

        public static PSO2Text SyncNAToJPText(PSO2Text pso2TextNA, PSO2Text pso2TextJP)
        {

            for (int i = 0; i < pso2TextNA.text.Count; i++)
            {
                pso2TextNA.text[i].RemoveAt(0);
            }
            for (int i = 0; i < pso2TextJP.categoryNames.Count; i++)
            {
                if (!pso2TextNA.categoryNames.Contains(pso2TextJP.categoryNames[i]))
                {
                    pso2TextNA.categoryNames.Insert(i, pso2TextJP.categoryNames[i]);
                    pso2TextNA.text.Insert(i, pso2TextJP.text[i]); //Just plop the JP one in if the whole category is missing
                }
            }

            for (int i = 0; i < pso2TextJP.text.Count; i++)
            {
                //We only care about group 0, JP language, in this case
                if (pso2TextJP.text[i].Count > 0)
                {
                    List<string> idCheck = new List<string>();
                    if (pso2TextNA.text[i].Count > 0)
                    {
                        for (int id = 0; id < pso2TextNA.text[i][0].Count; id++)
                        {
                            idCheck.Add(pso2TextNA.text[i][0][id].name);
                        }
                    }
                    for (int j = 0; j < pso2TextJP.text[i][0].Count; j++)
                    {
                        //This really should be a dictionary check, but whoever designed this format allowed for funny business with the 'keys'
                        if (!idCheck.Contains(pso2TextJP.text[i][0][j].name))
                        {
                            pso2TextNA.text[i][0].Add(pso2TextJP.text[i][0][j]);
                        }
                        else if (pso2TextNA.text[i][0][j].str == "" || pso2TextNA.text[i][0][j].str.Contains("***") || pso2TextNA.text[i][0][j].str.Contains("＊＊＊")) //Use JP text if text is redacted
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
    }
}
