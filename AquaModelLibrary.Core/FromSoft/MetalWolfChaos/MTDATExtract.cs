using SoulsFormats;

namespace AquaModelLibrary.Core.FromSoft.MetalWolfChaos
{
    public class MTDATExtract
    {
        public static void ExtractDAT(string fileName)
        {
            var fileBase = fileName.Split('_')[0];
            var outPathBase = Path.Combine(Path.GetDirectoryName(fileName), fileBase);
            if (fileName.EndsWith("_m.dat"))
            {
                var mdat = SoulsFile<SoulsFormats.MWC.MDAT>.Read(fileName);
                Directory.CreateDirectory(outPathBase);

                var mdlPath = Path.Combine(outPathBase, Path.GetFileName(fileBase + ".mdl"));
                if(mdat.mdlData?.Length > 0)
                {
                    File.WriteAllBytes(mdlPath, mdat.mdlData);
                }

                var data2Path = Path.Combine(outPathBase, Path.GetFileName(fileBase + "_2"));
                if (mdat.Data2?.Length > 0)
                {
                    File.WriteAllBytes(data2Path, mdat.Data2);
                }

                var data3Path = Path.Combine(outPathBase, Path.GetFileName(fileBase + "_3"));
                if (mdat.Data3?.Length > 0)
                {
                    File.WriteAllBytes(data3Path, mdat.Data3);
                }

                var data5Path = Path.Combine(outPathBase, Path.GetFileName(fileBase + "_5"));
                if (mdat.Data5?.Length > 0)
                {
                    File.WriteAllBytes(data5Path, mdat.Data5);
                }

                var data6Path = Path.Combine(outPathBase, Path.GetFileName(fileBase + "_6"));
                if (mdat.Data6?.Length > 0)
                {
                    File.WriteAllBytes(data6Path, mdat.Data6);
                }

            }
            else if (fileName.EndsWith("_t.dat"))
            {
                var tdat = SoulsFile<SoulsFormats.MWC.TDAT>.Read(fileName);
                Directory.CreateDirectory(outPathBase);
                foreach (var texture in tdat.Textures)
                {
                    File.WriteAllBytes(Path.Combine(outPathBase, Path.GetFileName(texture.Name)), texture.Data);
                }
            }
        }
    }
}
