using AquaModelLibrary.Data.DataTypes.SetLengthStrings;
using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.Vicious
{
    public class PAK
    {
        public PAK() { }

        public PAK(byte[] fileBytes, string outDir)
        {
            Read(fileBytes, outDir);
        }

        public void Read(byte[] fileBytes, string outDir)
        {
            using (var ms = new MemoryStream(fileBytes))
            using (var sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                var test = sr.Peek<int>();
                var test2 = sr.ReadBE<int>(true);
                sr._BEReadActive = test > test2;
                sr.Seek(0, SeekOrigin.Begin);

                var fileCount = sr.ReadBE<int>();
                var unkInt = sr.ReadBE<int>();
                sr.Seek(0x800, SeekOrigin.Begin);

                List<PakFileInfo> fileList = new List<PakFileInfo>();
                for (int i = 0; i < fileCount; i++)
                {
                    PakFileInfo pakFileInfo = new PakFileInfo();
                    //Normal 0x20 text block that endianness should have no bearing on
                    pakFileInfo.filePath = sr.Read<PSO2String>();
                    pakFileInfo.unkFlags = sr.ReadBE<int>();
                    pakFileInfo.unkInt0 = sr.ReadBE<int>();
                    pakFileInfo.unkInt1 = sr.ReadBE<int>();
                    pakFileInfo.fileSize = sr.ReadBE<int>();
                    fileList.Add(pakFileInfo);
                }

                sr.AlignReader(0x800);
                Directory.CreateDirectory(outDir);
                for (int i = 0; i < fileList.Count; i++)
                {
                    var fileInfo = fileList[i];
                    var outPath = Path.Combine(outDir, fileInfo.filePath.GetString().Replace("/", "\\"));
                    Directory.CreateDirectory(Path.GetDirectoryName(outPath));
                    File.WriteAllBytes(outPath, sr.ReadBytesSeek(fileInfo.fileSize));
                    sr.AlignReader(0x800);
                }
            }
        }

        public struct PakFileInfo
        {
            public PSO2String filePath;
            public int unkFlags;
            public int unkInt0;
            public int unkInt1;
            public int fileSize;
        }
    }
}
