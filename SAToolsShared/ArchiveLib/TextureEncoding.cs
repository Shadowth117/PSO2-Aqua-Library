using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using VrSharp.Gvr;
using VrSharp.Pvr;
using VrSharp.Xvr;
using static ArchiveLib.GenericArchive;

namespace ArchiveLib
{
    public class TextureEncoding
    {
        #region Texture encoding
        public static MemoryStream EncodePVR(PvrTextureInfo tex)
        {
            if (tex.TextureData != null)
                return TextureFunctions.UpdateGBIX(tex.TextureData, tex.GlobalIndex);
            tex.PixelFormat = TextureFunctions.GetPvrPixelFormatFromBitmap(tex.Image);
            tex.DataFormat = TextureFunctions.GetPvrDataFormatFromBitmap(tex.Image, tex.Mipmap, true);
            PvrTextureEncoder encoder = new PvrTextureEncoder(tex.Image, tex.PixelFormat, tex.DataFormat);
            encoder.GlobalIndex = tex.GlobalIndex;
            MemoryStream pvr = new MemoryStream();
            encoder.Save(pvr);
            pvr.Seek(0, SeekOrigin.Begin);
            return pvr;
        }

        public static MemoryStream EncodeGVR(GvrTextureInfo tex)
        {
            if (tex.TextureData != null)
                return TextureFunctions.UpdateGBIX(tex.TextureData, tex.GlobalIndex, true);
            tex.DataFormat = TextureFunctions.GetGvrDataFormatFromBitmap(tex.Image, false, true);
            tex.PixelFormat = TextureFunctions.GetGvrPixelFormatFromBitmap(tex.Image);
            GvrTextureEncoder encoder = new GvrTextureEncoder(tex.Image, tex.PixelFormat, tex.DataFormat);
            encoder.GlobalIndex = tex.GlobalIndex;
            MemoryStream gvr = new MemoryStream();
            encoder.Save(gvr);
            gvr.Seek(0, SeekOrigin.Begin);
            return gvr;
        }

        public static MemoryStream EncodeXVR(XvrTextureInfo tex)
        {
            if (tex.TextureData != null)
            {
                return TextureFunctions.UpdateGBIX(tex.TextureData, tex.GlobalIndex, false, true);
            }
            XvrTextureEncoder encoder = new XvrTextureEncoder(tex.Image, tex.PixelFormat, tex.DataFormat);
            encoder.GlobalIndex = tex.GlobalIndex;
            encoder.HasAlpha = tex.useAlpha = TextureFunctions.GetAlphaLevelFromBitmap(tex.Image) != 0;
            encoder.HasMipmaps = tex.Mipmap;
            MemoryStream xvr = new MemoryStream();
            encoder.Save(xvr);
            xvr.Seek(0, SeekOrigin.Begin);
            return xvr;
        }

        public static byte[] EncodeDDS(GenericArchiveEntry tex)
        {
            var texImage = tex.GetBitmap();
            return EncodeDDS(texImage);
        }

        public static byte[] EncodeDDS(Bitmap texImage)
        {
            var rect = new Rectangle(0, 0, texImage.Width, texImage.Height);

            if (texImage.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb)
            {
                texImage = texImage.Clone(rect, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            }

            var data = texImage.LockBits(rect, ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            BcEncoder encoder = new BcEncoder();
            encoder.OutputOptions.GenerateMipMaps = true;
            encoder.OutputOptions.Quality = CompressionQuality.BestQuality;
            encoder.OutputOptions.Format = (TextureFunctions.GetAlphaLevelFromBitmap(texImage) != 0) ? CompressionFormat.Bc3 : CompressionFormat.Bc1;
            encoder.OutputOptions.FileFormat = OutputFileFormat.Dds;
            using var ddsData = new MemoryStream();

            unsafe
            {
                var span = new ReadOnlySpan<byte>((byte*)data.Scan0, data.Stride * data.Height);
                encoder.EncodeToStream(span, data.Width, data.Height, BCnEncoder.Encoder.PixelFormat.Bgra32, ddsData);
            }

            return ddsData.ToArray();
        }

        public static byte[] EncodePNG(GenericArchiveEntry tex)
        {
            MemoryStream bmp = new MemoryStream();
            var texImage = tex.GetBitmap();
            texImage.Save(bmp, ImageFormat.Png);
            return bmp.ToArray();
        }
        #endregion
    }
}
