using Reloaded.Memory.Streams;
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
                    var unkValue = streamReader.Read<int>();
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

                        txt.text[i][sub].Add(pair);
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
    }
}
