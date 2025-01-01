using AquaModelLibrary.Helpers;
using AquaModelLibrary.Helpers.Readers;
using System.Diagnostics;
using System.Text;

namespace AquaModelLibrary.Data.POE2
{
    public class POE2Index
    {
        public List<int> fileSizes = new List<int>();
        public List<string> fileNames = new List<string>();
        public List<FileHeader> fileHeaderList = new List<FileHeader>();
        public Dictionary<ulong, FileHeader> fileHeaderDict = new Dictionary<ulong, FileHeader>();
        public List<TextBlockInfo> textBlockInfoList = new List<TextBlockInfo>();
        public byte[] textBlob = null;
        public POE2Index() { }
        public POE2Index(byte[] file)
        {
            using (MemoryStream stream = new MemoryStream(file))
            using (var streamReader = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                Read(streamReader);
            }
        }

        public POE2Index(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            var fileCount = sr.ReadBE<int>();
            for(int i = 0; i < fileCount; i++)
            {
                var len = sr.ReadBE<int>();
                fileNames.Add(sr.ReadCStringSeek(len));
                fileSizes.Add(sr.ReadBE<int>());
            }
            var fileHeaderCount = sr.ReadBE<int>();
            for(int i = 0; i < fileHeaderCount; i++)
            {
                var fileHeader = new FileHeader(sr);
                fileHeaderList.Add(fileHeader);
                fileHeaderDict.Add(fileHeader.hash, fileHeader);
            }
            var textBlockCount = sr.ReadBE<int>();
            for (int i = 0; i < textBlockCount; i++)
            {
                textBlockInfoList.Add(new TextBlockInfo(sr));
            }
            var textBlobStart = sr.Position;

            //Yes, to be clear this is a compressed blob within the already compressed index. Maybe at some point the index wasn't oodled?
            textBlob = POE2ArchiveUtility.DecompressArchive(sr.ReadBytesSeek((int)(sr.BaseStream.Length - textBlobStart)))[0].file;

            // Based on implementation in https://github.com/aianlinb/VisualGGPK2
            using (MemoryStream stream = new MemoryStream(textBlob))
            using (var streamReader = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                for (int i = 0; i < textBlockInfoList.Count; i++)
                {
                    var textBlockInfo = textBlockInfoList[i];
                    List<string> currentTextList = new List<string>();
                    bool shouldReset = false;

                    streamReader.Seek(textBlockInfo.textBlockOffset, SeekOrigin.Begin);
                    while (streamReader.Position <= textBlockInfo.textBlockOffset + textBlockInfo.innerOffset - 0x4)
                    {
                        int index = streamReader.ReadBE<int>();
                        if(index == 0)
                        {
                            shouldReset = !shouldReset;
                            if(shouldReset)
                            {
                                currentTextList.Clear();
                            }
                        } else
                        {
                            index -= 1;

                            var posFirst = streamReader.Position;
                            var filePath = streamReader.ReadCStringSeek();
                            var posAfter = streamReader.Position;

                            if (index < currentTextList.Count)
                            {
                                filePath = currentTextList[index] + filePath;
                            }
                            if(shouldReset)
                            {
                                currentTextList.Add(filePath);
                            } else
                            {
                                var fileHash = GetNameHash(filePath);
                                if(fileHeaderDict.ContainsKey(fileHash))
                                {
                                    fileHeaderDict[fileHash].virtualFilePath = filePath;
                                } else
                                {
                                    //Debug.WriteLine($"Unable to find hash {fileHash.ToString("X")}, path: {filePath}");
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get the hash of a file path
        /// Implementation from https://github.com/aianlinb/VisualGGPK2
        /// </summary>
        public unsafe ulong GetNameHash(string name)
        {
            return textBlockInfoList[0].hash switch
            {
                0xF42A94E69CFF42FE => HashHelpers.MurmurHash64A(Encoding.UTF8.GetBytes(name.ToLowerInvariant())),
                0x07E47507B4A92E53 => HashHelpers.FNV1a64Hash(name),
                _ => throw new($"Unexpected Index hash variant {textBlockInfoList[0].hash} !")
            };
        }

        public class FileHeader
        {
            /// <summary>
            /// Hash of the virtual file path for this data.
            /// </summary>
            public ulong hash;
            public int archiveIndexId;
            public int archiveOffset;
            public int fileSize;

            /// <summary>
            /// Added later in reading. This is what the hash is based on.
            /// The game essentially ids files based on a virtual path agnostic to the actual physical file path 
            /// and this is the aforementioned virtual path.
            /// </summary>
            public string virtualFilePath;
            public FileHeader() { }
            public FileHeader(byte[] file)
            {
                using (MemoryStream stream = new MemoryStream(file))
                using (var streamReader = new BufferedStreamReaderBE<MemoryStream>(stream))
                {
                    Read(streamReader);
                }
            }

            public FileHeader(BufferedStreamReaderBE<MemoryStream> sr)
            {
                Read(sr);
            }

            public void Read(BufferedStreamReaderBE<MemoryStream> sr)
            {
                hash = sr.ReadBE<ulong>();
                archiveIndexId = sr.ReadBE<int>();
                archiveOffset = sr.ReadBE<int>();
                fileSize = sr.ReadBE<int>();
            }

        }

        public class TextBlockInfo
        {
            public ulong hash;
            public int textBlockOffset;
            public int innerOffset;
            public int sectionSize;
            public TextBlockInfo() { }
            public TextBlockInfo(byte[] file)
            {
                using (MemoryStream stream = new MemoryStream(file))
                using (var streamReader = new BufferedStreamReaderBE<MemoryStream>(stream))
                {
                    Read(streamReader);
                }
            }

            public TextBlockInfo(BufferedStreamReaderBE<MemoryStream> sr)
            {
                Read(sr);
            }

            public void Read(BufferedStreamReaderBE<MemoryStream> sr)
            {
                hash = sr.ReadBE<ulong>();
                textBlockOffset = sr.ReadBE<int>();
                innerOffset = sr.ReadBE<int>();
                sectionSize = sr.ReadBE<int>();
            }

        }
    }
}
