namespace AquaModelLibrary.Data.POE2
{
    public class POE2ArchiveUtility
    {
        public struct ArchiveFile
        {
            public string name;
            public byte[] file;
        }

        public static List<ArchiveFile> DecompressArchive(byte[] archive, POE2Index index = null, string path = null)
        {
            int decompSize = BitConverter.ToInt32(archive, 0x0);
            //int compSize = BitConverter.ToInt32(archive, 0x4);
            int bufferOffset = BitConverter.ToInt32(archive, 0x8) + 0xC;
            //int unkInt0 = BitConverter.ToInt32(archive, 0xC); //Always 0xC or 0xD?

            //int compressedBufferCount = BitConverter.ToInt32(archive, 0x10); //Always 0x1? If this is ever higher we probably need to tweak this
            //int decompBufferSize = BitConverter.ToInt32(archive, 0x14);
            //int compBufferStartOffset = BitConverter.ToInt32(archive, 0x18); //?
            //int compBufferSize = BitConverter.ToInt32(archive, 0x1C);

            //int bufferBlocksOffset = BitConverter.ToInt32(archive, 0x20); //?
            //int bufferBlocksCount = BitConverter.ToInt32(archive, 0x24);
            //int maxBufferSize = BitConverter.ToInt32(archive, 0x28); //Game does not like if this is not big enough for its blocks
            //int int_2C = BitConverter.ToInt32(archive, 0x2C);

            //int int_30 = BitConverter.ToInt32(archive, 0x30);
            //int int_34 = BitConverter.ToInt32(archive, 0x34);

            //List<int> bufferStartList = new List<int>();
            //List<int> bufferSizeList = new List<int>();
            //for(int i = 0; i < bufferBlocksCount; i++)
            //{
            //  bufferStartList.Add(BitConverter.ToInt32(archive, 0x38 + i * 8));
            //  bufferSizeList.Add(BitConverter.ToInt32(archive, 0x38 + i * 8 + 4));
            //}

            byte[] buffer = new byte[archive.Length - bufferOffset];
            Array.Copy(archive, bufferOffset, buffer, 0, archive.Length - bufferOffset);

            var fileBlob = Zamboni.Oodle.OodleDecompress(buffer, decompSize);
            List<ArchiveFile> files = new List<ArchiveFile>();
            
            if(index != null)
            {
                var id = index.fileNames.IndexOf(path);
                if(id == -1)
                {
                    SetFileBlob(fileBlob, files);
                } else
                {
                    int counter = 0;

                    //In theory we don't need this dictionary, but just in case...
                    Dictionary<string, int> filePathCounter = new Dictionary<string, int>();
                    foreach(var header in index.fileHeaderList)
                    {
                        if(header.archiveIndexId == id)
                        {
                            var fileBytes = new byte[header.fileSize];
                            Array.Copy(fileBlob, header.archiveOffset, fileBytes, 0, header.fileSize);
                            ArchiveFile file = new ArchiveFile();
                            
                            if(header.virtualFilePath == null)
                            {
                                string extension = "";
                                var magic = BitConverter.ToInt32(fileBytes);
                                switch (magic)
                                {
                                    case 0x20534444: //DDS
                                        extension = ".dds";
                                        break;
                                    case 0x5367674F: //OggS
                                        extension = ".ogg";
                                        break;
                                    case 0x46464952: //RIFF
                                        extension = ".wav";
                                        break;
                                    default:
                                        if (fileBytes.Length > 0x24)
                                        {
                                            var magic2 = BitConverter.ToInt32(fileBytes, 0x20);
                                            var magic3 = BitConverter.ToInt32(fileBytes, 0x1F);
                                            if (magic2 == 0x6D4C4F44) //DOLm
                                            {
                                                extension = ".smd";
                                            }
                                            else if (magic3 == 0x6D4C4F44)
                                            {
                                                extension = ".tmd";
                                            }

                                            if (extension == "")
                                            {
                                                var magic4 = BitConverter.ToInt16(fileBytes, 0x8);
                                                //Really not a magic, but most armatures will have bone 1 as the child and should always have no bone, or -1/FF, for the sibling.
                                                //Exceptions might be single bone armatures which probably don't have anything of interest to see anyways.
                                                if (magic4 == 0x1FF)
                                                {
                                                    extension = ".ast"; //Formerly .action when I didn't know what this was
                                                }
                                            }
                                        }
                                        break;
                                }
                                file.name = $"file_{counter}" + extension;
                            } else
                            {
                                var fileName = header.virtualFilePath;
                                if (!filePathCounter.ContainsKey(fileName))
                                {
                                    filePathCounter[fileName] = 1;
                                } else
                                {
                                    filePathCounter[fileName] += 1;
                                }

                                //In the offchance it's possible for a duplicate since we're not retaining the original folders
                                if(filePathCounter[fileName] > 1)
                                {
                                    var ext = Path.GetExtension(fileName);
                                    fileName = fileName.Replace(ext, $"({filePathCounter[fileName]}).{ext}");
                                }
                                file.name = Path.GetFileName(fileName);
                            }

                            file.file = fileBytes;
                            files.Add(file);
                            counter++;
                        }
                    }
                }

            } else
            {
                SetFileBlob(fileBlob, files);
            }

            return files;
        }

        private static void SetFileBlob(byte[] fileBlob, List<ArchiveFile> files)
        {
            ArchiveFile file = new ArchiveFile();
            file.name = "file";
            file.file = fileBlob;
            files.Add(file);
        }
    }
}
