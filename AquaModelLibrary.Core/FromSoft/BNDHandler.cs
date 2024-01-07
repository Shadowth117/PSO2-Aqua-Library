using SoulsFormats;

namespace AquaModelLibrary.Core.FromSoft
{
    public class BNDHandler
    {
        public static void BNDExtract(string filePath)
        {
            var folderPath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath));
            Directory.CreateDirectory(folderPath);

            var bndRaw = File.ReadAllBytes(filePath);
            if (SoulsFile<BND>.Is(bndRaw))
            {
                var bnd = SoulsFile<BND>.Read(bndRaw);
                foreach (var file in bnd.Files)
                {
                    File.WriteAllBytes(Path.Combine(folderPath, Path.GetFileName(file.Name)), file.Bytes);
                }
            }
        }

        public static void BNDPack(string folderPath)
        {
            var filePaths = Directory.GetFiles(folderPath);

            BND bnd = new BND();
            bnd.internalVersion = 0xCA;
            bnd.format0 = 0x10;
            bnd.format1 = 0x1;
            bnd.Files = new List<BinderFile>();
            foreach (var filePath in filePaths)
            {
                BinderFile bndFile = new BinderFile();
                bndFile.Name = filePath.Replace(folderPath, "");
                if (folderPath == Path.GetDirectoryName(filePath))
                {
                    bndFile.Name = bndFile.Name.Substring(1, bndFile.Name.Length - 1);
                }
                bndFile.Bytes = File.ReadAllBytes(filePath);
                bnd.Files.Add(bndFile);
            }

            File.WriteAllBytes(folderPath + ".bnd", bnd.Write());
        }
    }
}
