using System.Security.Cryptography.X509Certificates;

namespace AquaModelLibrary.Data.POE2
{
    public class POE2ArchiveUtility
    {
        public struct ArchiveFile
        {
            public string name;
            public byte[] file;
        }

        public static List<ArchiveFile> DecompressArchive(byte[] archive)
        {
            int decompSize = BitConverter.ToInt32(archive, 0x0);
            int bufferOffset = BitConverter.ToInt32(archive, 0x8) + 0xC;
            byte[] buffer = new byte[archive.Length - bufferOffset];
            Array.Copy(archive, bufferOffset, buffer, 0, archive.Length - bufferOffset);

            ArchiveFile file = new ArchiveFile();
            file.name = "file_0";
            file.file = Zamboni.Oodle.OodleDecompress(buffer, decompSize);
            List<ArchiveFile> files = new List<ArchiveFile>();
            files.Add(file);

            return files;
        }
    }
}
