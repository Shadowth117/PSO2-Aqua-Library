using ArchiveLib;
using SoulsFormats;
using VrSharp;

namespace AquaModelLibrary.Core.Billy
{
    /// <summary>
    /// Horrible black magic to ddsize a Puyo archive and stuff it into a FromSoft TPF for DSMS
    /// </summary>
    public class Puyo2TPF
    {
        public static TPF PuyoFile2TPF(PuyoFile puyoFile)
        {
            TPF tpf = new TPF();
            foreach(var tex in puyoFile.Entries)
            {
                tpf.Textures.Add(new TPF.Texture(tex.Name, 0, 0, TextureEncoding.EncodeDDS(tex)));
            }

            return tpf;
        }

        public static TPF LoosePuyoTexture2TPF(string fileName, VrTexture tex)
        {
            TPF tpf = new TPF();
            tpf.Textures.Add(new TPF.Texture(Path.GetFileNameWithoutExtension(fileName), 0, 0, TextureEncoding.EncodeDDS(tex.ToBitmap())));
            
            return tpf;
        }
    }
}
