using AquaModelLibrary.Helpers;
using AquaModelLibrary.Helpers.Readers;
using System.Text;

namespace AquaModelLibrary.Data.BlueDragon
{
    public class IPK
    {
        public int magic;
        /// <summary>
        /// Amount to align fileinfo block and each file data block to.
        /// </summary>
        public int blockAlignment;
        public int fileCount;
        public int fileSize;

        public List<IPKFileInfo> fileInfoList = new List<IPKFileInfo>();
        public Dictionary<string, byte[]> fileDict = new Dictionary<string, byte[]>();

        public IPK() { }

        public IPK(byte[] fileBytes)
        {
            Read(fileBytes);
        }

        public void Read(byte[] fileBytes)
        {
            using (var ms = new MemoryStream(fileBytes))
            using (var sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                //Note these files are little endian despite being on an Xbox 360.
                magic = sr.ReadBE<int>();
                blockAlignment = sr.ReadBE<int>();
                fileCount = sr.ReadBE<int>();
                fileSize = sr.ReadBE<int>();

                for(int i = 0; i < fileCount; i++)
                {
                    IPKFileInfo ipkFileInfo = new IPKFileInfo();
                    ipkFileInfo.filePath = Encoding.UTF8.GetString(sr.ReadBytesSeek(0x40));
                    ipkFileInfo.filePath = ipkFileInfo.filePath.Remove(ipkFileInfo.filePath.IndexOf(char.MinValue));
                    ipkFileInfo.usesCompression = sr.ReadBE<int>();
                    ipkFileInfo.compressedSize = sr.ReadBE<int>();
                    ipkFileInfo.absoluteFileOffset = sr.ReadBE<int>();
                    ipkFileInfo.uncompressedSize = sr.ReadBE<int>();
                    ipkFileInfo.unkInt = sr.ReadBE<int>();
                    ipkFileInfo.pad0 = sr.ReadBE<int>();
                    ipkFileInfo.pad1 = sr.ReadBE<int>();
                    ipkFileInfo.pad2 = sr.ReadBE<int>();

                    var innerFileBytes = sr.ReadBytes(ipkFileInfo.absoluteFileOffset, ipkFileInfo.compressedSize);
                    if(ipkFileInfo.usesCompression != 0)
                    {
                        innerFileBytes = CompressionHelper.ZlibDefaultDecompress(innerFileBytes, ipkFileInfo.uncompressedSize);
                    }
                    fileDict[ipkFileInfo.filePath] = innerFileBytes;
                }
            }
        }
    }
}
