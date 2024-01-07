using SoulsFormats;
using SoulsFormats.Formats.Other.MWC;

namespace AquaModelLibrary.Core.FromSoft.MetalWolfChaos
{
    public class DEVDIVUtil
    {
        public static void DEVDIVExtract(string filePath)
        {
            var dev = SoulsFile<DEV>.Read(filePath);
            dev.ReadData(filePath);
            var outPathBase = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath));

            for (int i = 0; i < dev.fileSets.Count; i++)
            {
                var outFolder = Path.Combine(outPathBase, $"{i:D3}");
                Directory.CreateDirectory(outFolder);
                for (int j = 0; j < dev.fileSets[i].Count; j++)
                {
                    File.WriteAllBytes(Path.Combine(outFolder, Path.GetFileName(dev.fileHeaderSets[i][j].fileName)), dev.fileSets[i][j]);
                }
            }
        }
    }
}
