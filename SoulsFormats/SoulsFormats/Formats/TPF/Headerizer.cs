using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Linq.Expressions;
using static SoulsFormats.DDS;
using static SoulsFormats.TPF;

namespace SoulsFormats
{
    /* BCn block sizes
    BC1 (DXT1) - 8
    BC2 (DXT3) - 16
    BC3 (DXT5) - 16
    BC4 (ATI1) - 8
    BC5 (ATI2) - 16
    BC6 - 16
    BC7 - 16
    */
    internal static class Headerizer
    {
        /* Known TPF texture formats
          0 - DXT1
          1 - DXT1
          3 - DXT3
          5 - DXT5
          6 - B5G5R5A1_UNORM
          9 - B8G8R8A8
         10 - R8G8B8 on PC, A8G8B8R8 on PS3
         16 - A8
         22 - A16B16G16R16f
         23 - DXT5
         24 - BC4
         25 - DXT1
         33 - DXT5
        100 - BC6H_UF16
        102 - BC7_UNORM
        103 - ATI1
        104 - ATI2
        105 - A8B8G8R8
        106 - BC7_UNORM
        107 - BC7_UNORM
        108 - DXT1
        109 - DXT1
        110 - DXT5
        112 - BC7_UNORM_SRGB
        113 - BC6H_UF16
        */
        /// <summary>
        /// Map to DXGI format
        /// </summary>
        private static Dictionary<int, DXGI_FORMAT> textureFormatMap = new Dictionary<int, DXGI_FORMAT>()
        {
            [0] = DXGI_FORMAT.BC1_UNORM,
            [1] = DXGI_FORMAT.BC1_UNORM,
            [3] = DXGI_FORMAT.BC2_UNORM,
            [5] = DXGI_FORMAT.BC3_UNORM,
            [6] = DXGI_FORMAT.B5G5R5A1_UNORM,
            [8] = DXGI_FORMAT.R8G8B8A8_UNORM,
            [9] = DXGI_FORMAT.B8G8R8A8_UNORM,
            [10] = DXGI_FORMAT.R8G8B8A8_UNORM,
            [16] = DXGI_FORMAT.A8_UNORM,
            [22] = DXGI_FORMAT.R16G16B16A16_UNORM,
            [23] = DXGI_FORMAT.BC3_UNORM,
            [24] = DXGI_FORMAT.BC4_UNORM,
            [25] = DXGI_FORMAT.BC1_UNORM,
            [29] = DXGI_FORMAT.BC1_UNORM,
            [33] = DXGI_FORMAT.BC3_UNORM,
            [100] = DXGI_FORMAT.BC6H_UF16,
            [102] = DXGI_FORMAT.BC7_UNORM,
            [103] = DXGI_FORMAT.BC4_UNORM,
            [104] = DXGI_FORMAT.BC5_UNORM,
            [105] = DXGI_FORMAT.R8G8B8A8_UNORM,
            [106] = DXGI_FORMAT.BC7_UNORM,
            [107] = DXGI_FORMAT.BC7_UNORM,
            [108] = DXGI_FORMAT.BC1_UNORM,
            [109] = DXGI_FORMAT.BC1_UNORM,
            [110] = DXGI_FORMAT.BC3_UNORM,
            [112] = DXGI_FORMAT.BC7_UNORM_SRGB,
            [113] = DXGI_FORMAT.BC6H_UF16,
        };

        /// <summary>
        /// Compressed Bits Per Block
        /// </summary>
        private static Dictionary<byte, int> CompressedBPB = new Dictionary<byte, int>
        {
            [0] = 8,
            [1] = 8,
            [3] = 16,
            [5] = 16,
            [23] = 16,
            [24] = 8,
            [25] = 8,
            [29] = 8,
            [33] = 16,
            [100] = 16,
            [102] = 16,
            [103] = 8,
            [104] = 16,
            [106] = 16,
            [107] = 16,
            [108] = 8,
            [109] = 8,
            [110] = 16,
            [112] = 16,
            [113] = 16,
        };

        /// <summary>
        /// Uncompressed Bytes Per Pixel
        /// </summary>
        private static Dictionary<byte, int> UncompressedBPP = new Dictionary<byte, int>
        {
            [6] = 2,
            [8] = 4,
            [9] = 4,
            [10] = 4,
            [16] = 1,
            [22] = 8,
            [105] = 4,
        };

        /// <summary>
        /// DDS FourCC bytes
        /// </summary>
        private static Dictionary<byte, string> FourCC = new Dictionary<byte, string>
        {
            [0] = "DXT1",
            [1] = "DXT1",
            [3] = "DXT3",
            [5] = "DXT5",
            [22] = "q\0\0\0", // 0x71
            [23] = "DXT5",
            [24] = "ATI1",
            [25] = "DXT1",
            [29] = "DXT1",
            [33] = "DXT5",
            [103] = "ATI1",
            [104] = "ATI2",
            [108] = "DXT1",
            [109] = "DXT1",
            [110] = "DXT5",
        };

        /// <summary>
        /// DX10+ dds pixel formats
        /// </summary>
        private static byte[] DX10Formats = { 6, 100, 102, 106, 107, 112, 113 };

        /// <summary>
        /// By default, we'll assume no swizzling, PC type. Bear in mind Demon's Souls and Dark Souls 1 do NOT use PS3 swizzling and should be assigned 'PC'!
        /// </summary>
        public static byte[] Headerize(TPF.Texture texture)
        {
            if (SFEncoding.ASCII.GetString(texture.Bytes, 0, 4) == "DDS ")
                return texture.Bytes;

            var dds = new DDS();
            byte format = texture.Format;
            short width = texture.Header.Width;
            short height = texture.Header.Height;
            int depth = texture.Header.TextureCount;
            int mipCount = texture.Mipmaps;
            TPF.TexType type = texture.Type;

            dds.dwFlags = DDSD.CAPS | DDSD.HEIGHT | DDSD.WIDTH | DDSD.PIXELFORMAT | DDSD.MIPMAPCOUNT;
            if (CompressedBPB.ContainsKey(format))
                dds.dwFlags |= DDSD.LINEARSIZE;
            else if (UncompressedBPP.ContainsKey(format))
                dds.dwFlags |= DDSD.PITCH;

            dds.dwHeight = height;
            dds.dwWidth = width;

            if (CompressedBPB.ContainsKey(format))
                dds.dwPitchOrLinearSize = Math.Max(1, (width + 3) / 4) * CompressedBPB[format];
            else if (UncompressedBPP.ContainsKey(format))
                dds.dwPitchOrLinearSize = (width * UncompressedBPP[format] + 7) / 8;

            dds.dwDepth = type == TPF.TexType.Volume ? depth : 0;

            if (mipCount == 0)
                mipCount = DetermineMipCount(width, height);
            dds.dwMipMapCount = mipCount;

            dds.dwCaps = DDSCAPS.TEXTURE;
            if (type == TPF.TexType.Cubemap)
                dds.dwCaps |= DDSCAPS.COMPLEX;
            if (mipCount > 1)
                dds.dwCaps |= DDSCAPS.COMPLEX | DDSCAPS.MIPMAP;

            if (type == TPF.TexType.Cubemap)
                dds.dwCaps2 = CUBEMAP_ALLFACES;
            else if (type == TPF.TexType.Volume)
                dds.dwCaps2 = DDSCAPS2.VOLUME;

            PIXELFORMAT ddspf = dds.ddspf;

            if (FourCC.ContainsKey(format) || DX10Formats.Contains(format))
                ddspf.dwFlags = DDPF.FOURCC;
            if (format == 6)
                ddspf.dwFlags |= DDPF.ALPHAPIXELS | DDPF.RGB;
            else if (format == 8)
                ddspf.dwFlags |= DDPF.ALPHAPIXELS | DDPF.RGB;
            else if (format == 9)
                ddspf.dwFlags |= DDPF.ALPHAPIXELS | DDPF.RGB;
            else if (format == 10)
                ddspf.dwFlags |= DDPF.RGB;
            else if (format == 16)
                ddspf.dwFlags |= DDPF.ALPHA;
            else if (format == 105)
                ddspf.dwFlags |= DDPF.ALPHAPIXELS | DDPF.RGB;

            if (FourCC.ContainsKey(format))
                ddspf.dwFourCC = FourCC[format];
            else if (DX10Formats.Contains(format))
                ddspf.dwFourCC = "DX10";

            if (format == 6)
            {
                ddspf.dwRGBBitCount = 16;
                ddspf.dwRBitMask = 0b01111100_00000000;
                ddspf.dwGBitMask = 0b00000011_11100000;
                ddspf.dwBBitMask = 0b00000000_00011111;
                ddspf.dwABitMask = 0b10000000_00000000;
            }
            else if (format == 8)
            {
                ddspf.dwRGBBitCount = 32;
                ddspf.dwRBitMask = 0x00FF0000;
                ddspf.dwGBitMask = 0x0000FF00;
                ddspf.dwBBitMask = 0x000000FF;
                ddspf.dwABitMask = 0xFF000000;
            }
            else if (format == 9)
            {
                ddspf.dwRGBBitCount = 32;
                ddspf.dwRBitMask = 0x00FF0000;
                ddspf.dwGBitMask = 0x0000FF00;
                ddspf.dwBBitMask = 0x000000FF;
                ddspf.dwABitMask = 0xFF000000;
            }
            else if (format == 10)
            {
                ddspf.dwRGBBitCount = 24;
                ddspf.dwRBitMask = 0x00FF0000;
                ddspf.dwGBitMask = 0x0000FF00;
                ddspf.dwBBitMask = 0x000000FF;
            }
            else if (format == 16)
            {
                ddspf.dwRGBBitCount = 8;
                ddspf.dwABitMask = 0x000000FF;
            }
            else if (format == 105)
            {
                ddspf.dwRGBBitCount = 32;
                ddspf.dwRBitMask = 0x000000FF;
                ddspf.dwGBitMask = 0x0000FF00;
                ddspf.dwBBitMask = 0x00FF0000;
                ddspf.dwABitMask = 0xFF000000;
            }

            if (DX10Formats.Contains(format))
            {
                dds.header10 = new HEADER_DXT10();
                dds.header10.dxgiFormat = (DXGI_FORMAT)texture.Header.DXGIFormat;
                if (type == TPF.TexType.Cubemap)
                    dds.header10.miscFlag = RESOURCE_MISC.TEXTURECUBE;
            }

            var images = RebuildPixelData(texture.Bytes, format, width, height, depth, mipCount, type, texture.Platform);
            
            //Failsafe for if whatever reason we don't read all of the mipmaps
            if(images.Count > 0)
            {
                dds.dwMipMapCount = images[0].MipLevels.Count;
            }
            return dds.Write(Image.Write(images));
        }

        private static int DetermineMipCount(int width, int height)
        {
            return (int)Math.Ceiling(Math.Log(Math.Max(width, height), 2)) + 1;
        }

        private static List<Image> RebuildPixelData(byte[] bytes, byte format, short width, short height, int depth, int mipCount, TPF.TexType type, TPFPlatform platform)
        {
            List<Image> images = ReadImages(platform, bytes, width, height, depth, mipCount, format, type);

            return images;
        }

        private static int PadTo(int value, int pad)
        {
            return (int)Math.Ceiling(value / (float)pad) * pad;
        }

        private static List<Image> ReadImages(TPFPlatform platform, byte[] bytes, int width, int height, int depth, int mipCount, int format, TPF.TexType type)
        {
            switch (platform)
            {
                case TPFPlatform.Xbox360:
                    return Read360Images(new BinaryReaderEx(false, bytes), width, height, depth, mipCount, format);
                case TPFPlatform.PS3:
                    return ReadPS3Images(new BinaryReaderEx(false, bytes), width, height, depth, mipCount, format);
                case TPFPlatform.PS4:
                    return ReadPS4Images(new BinaryReaderEx(false, bytes), width, height, depth, mipCount, format, type);
                case TPFPlatform.PS5:
                    return ReadPS5Images(new BinaryReaderEx(false, bytes), width, height, depth, mipCount, format);
                case TPFPlatform.PC:
                default:
                    //Original behavior, probably not necessary.
                    return ReadPS3Images(new BinaryReaderEx(false, bytes), width, height, depth, mipCount, format);
            }

            return null;
        }

        private static List<Image> Read360Images(BinaryReaderEx br, int finalWidth, int finalHeight, int depth, int mipCount, int format)
        {
            var pixelFormat = (DrSwizzler.DDS.DXEnums.DXGIFormat)textureFormatMap[format];

            DrSwizzler.Util.GetsourceBytesPerPixelSetAndPixelSize(pixelFormat, out int sourceBytesPerPixelSet, out int pixelBlockSize, out int formatBpp);
            var images = new List<Image>(depth);

            List<byte[]> bufferArray = new List<byte[]>();
            for (int i = 0; i < depth; i++)
            {
                var image = new Image();
                for (int j = 0; j < mipCount; j++)
                {
                    int scale = (int)Math.Pow(2, j);
                    int w = PadTo(finalWidth / scale, 1);
                    int h = PadTo(finalHeight / scale, 1);
                    long calculatedBufferLength = formatBpp * w * h / 8;

                    if (calculatedBufferLength < sourceBytesPerPixelSet)
                    {
                        calculatedBufferLength = sourceBytesPerPixelSet;
                    }

                    //Xbox 360 textures have minimum buffer caps. To read all the mips properly, you'd need to extract them as tiles from these.
                    //It gets a bit crazy when it gets low enough for Dark Souls and frankly, someone else can handle it better later if they so desire.
                    long ogCalcBuffLength = calculatedBufferLength;
                    int ogW = w;
                    int ogH = h;

                    byte[] mip = DrSwizzler.Deswizzler.Xbox360Deswizzle(br.ReadBytes((int)calculatedBufferLength), w, h, pixelFormat);
                    mip = DrSwizzler.Util.ExtractTile(mip, pixelFormat, w, 0, 0, ogW, ogH);
                    image.MipLevels.Add(mip);

                    //Skip all but the first mip unless someone wants to finish it offer more properly.
                    break;
                }
                images.Add(image);
            }
            return images;
        }

        private static List<Image> ReadPS3Images(BinaryReaderEx br, int finalWidth, int finalHeight, int depth, int mipCount, int format)
        {
            var pixelFormat = (DrSwizzler.DDS.DXEnums.DXGIFormat)textureFormatMap[format];
            DrSwizzler.Util.GetsourceBytesPerPixelSetAndPixelSize(pixelFormat, out int sourceBytesPerPixelSet, out int pixelBlockSize, out int formatBpp);
            var images = new List<Image>(depth);

            for (int i = 0; i < depth; i++)
            {
                var image = new Image();
                br.Pad(0x80);
                for (int j = 0; j < mipCount; j++)
                {
                    int scale = (int)Math.Pow(2, j);
                    int w = PadTo(finalWidth / scale, 1);
                    int h = PadTo(finalHeight / scale, 1);
                    long calculatedBufferLength = formatBpp * w * h / 8;

                    if (calculatedBufferLength < sourceBytesPerPixelSet)
                    {
                        calculatedBufferLength = sourceBytesPerPixelSet;
                    }

                    byte[] mip = br.ReadBytes((int)calculatedBufferLength);
                    if (format == 10)
                    {
                        mip = DrSwizzler.Deswizzler.PS3Deswizzle(mip, w, h, pixelFormat);
                    }
                    image.MipLevels.Add(mip);
                }
                images.Add(image);
            }
            return images;
        }

        private static List<Image> ReadPS4Images(BinaryReaderEx br, int finalWidth, int finalHeight, int depth, int mipCount, int format, TPF.TexType type)
        {
            var pixelFormat = (DrSwizzler.DDS.DXEnums.DXGIFormat)textureFormatMap[format];
            DrSwizzler.Util.GetsourceBytesPerPixelSetAndPixelSize(pixelFormat, out int sourceBytesPerPixelSet, out int pixelBlockSize, out int formatBpp);

            long sliceBufferLength = br.Length / depth;
            List<Image> imageList = new List<Image>();
            long bufferUsed = 0;
            int mipWidth = finalWidth;
            int mipHeight = finalHeight;

            //Swizzling can go outside the bounds of the texture so we want to check the full buffer in these cases. Hopefully it's only for single mip instances
            long bufferLength = mipCount == 1 ? sliceBufferLength : formatBpp * finalWidth * finalHeight / 8;

            //Prepare mip set lists
            for (int i = 0; i < depth; i++)
            {
                imageList.Add(new Image());
            }

            int sliceBufferMin;
            if (depth > 1)
            {
                sliceBufferMin = 0x400;
            }
            else
            {
                sliceBufferMin = 0x200;
            }

            //PS4 textures seem to lay out slices at the same level sequentially rather than having slices go through each mip in their set before proceeding to the next slice
            for (int i = 0; i < mipCount; i++)
            {
                if (mipCount > 1 || depth > 1)
                {
                    if (bufferLength != sliceBufferMin && i != 0)
                    {
                        bufferLength = bufferLength / 4;
                        if (bufferLength < sliceBufferMin)
                        {
                            bufferLength = sliceBufferMin;
                        }
                    }
                }

                for (int s = 0; s < depth; s++)
                {
                    var mipOffset = bufferUsed;
                    br.Position = mipOffset;
                    var mipFull = br.ReadBytes((int)bufferLength);
                    bufferUsed += bufferLength;

                    //Make sure that we have enough bytes to actually deswizzle
                    var deSwizzChunkSize = GetDeswizzleSize(mipFull.Length, formatBpp, mipWidth, mipHeight, out int deSwizzWidth, out int deSwizzHeight);
                    int swizzleBlockWidth = deSwizzWidth < 8 ? 8 : deSwizzWidth;
                    int swizzleBlockHeight = deSwizzHeight < 8 ? 8 : deSwizzHeight;

                    //If it's too small, we don't need to deswizzle
                    if ((formatBpp * mipWidth * mipHeight / 8) <= sourceBytesPerPixelSet)
                    {
                        var newMipFull = new byte[sourceBytesPerPixelSet];

                        for(int m = 0; m < Math.Min(mipFull.Length, newMipFull.Length); m++)
                        {
                            newMipFull[m] = mipFull[m];
                        }
                        mipFull = newMipFull;
                    }
                    else
                    {
                        mipFull = DrSwizzler.Deswizzler.PS4Deswizzle(mipFull, swizzleBlockWidth, swizzleBlockHeight, pixelFormat);

                        //Extract as a tile from the pixels if we haven't done that at the deswizzle step
                        if (swizzleBlockWidth != mipWidth || swizzleBlockHeight != mipHeight)
                        {
                            mipFull = DrSwizzler.Util.ExtractTile(mipFull, pixelFormat, swizzleBlockWidth, 0, 0, mipWidth, mipHeight);
                        }
                    }

                    imageList[s].MipLevels.Add(mipFull);
                }
                mipWidth /= 2;
                mipHeight /= 2;

                //Cubemaps seem to pad to the size of 8 textures
                if (type == TexType.Cubemap)
                {
                    bufferUsed += 8 * bufferLength - depth * bufferLength;
                }
            }

            return imageList;
        }

        /// <summary> 
        /// The texture buffers for internal mipmaps seemingly subdivide by 2 each time we go down a mip, UNTIL we reach 0x400. When the buffer should be 0x400, we instead skip to 0x200.
        /// All mipmap buffers after this will be 0x100 regardless of true size.
        /// While the buffers are larger than the actual texture size, the swizzling happens at the BUFFER level and thus reading the full buffer for deswizzling is paramount
        /// </summary>
        private static List<Image> ReadPS5Images(BinaryReaderEx br, int finalWidth, int finalHeight, int depth, int mipCount, int format)
        {
            var pixelFormat = (DrSwizzler.DDS.DXEnums.DXGIFormat)textureFormatMap[format];
            DrSwizzler.Util.GetsourceBytesPerPixelSetAndPixelSize(pixelFormat, out int sourceBytesPerPixelSet, out int pixelBlockSize, out int formatBpp);
            List<Image> imageList = new List<Image>();

            //Prepare mip set lists
            for (int i = 0; i < depth; i++)
            {
                imageList.Add(new Image());
            }

            long sliceBufferLength = br.Length / depth;
            for (int s = 0; s < depth; s++)
            {
                int mipWidth = finalWidth;
                int mipHeight = finalHeight;
                long bufferLength = sliceBufferLength / 2;

                //In some cases, we want the full buffer size because of overrun and the need for it in deswizzling,
                //but sometimes we want the calculated version since larger buffers don't have padding,
                //which means the smaller mips combined won't equal half the slice's buffer length
                long calculatedBufferLength = formatBpp * finalWidth * finalHeight / 8;
                if (calculatedBufferLength > bufferLength)
                {
                    bufferLength = calculatedBufferLength;
                }

                long bufferUsed = 0;
                for (int i = 0; i < mipCount; i++)
                {
                    if (mipCount > 1)
                    {
                        if (bufferLength != 0x100 && i != 0)
                        {
                            bufferLength = bufferLength / 2;

                            if (bufferLength == 0x400 || (bufferLength >= 0x10000))
                            {
                                bufferLength = bufferLength / 2;
                            }
                        }
                    }
                    bufferUsed += bufferLength;
                    var mipOffset = ((sliceBufferLength * depth) - (sliceBufferLength * s)) - bufferUsed;
                    br.Position = mipOffset;
                    var mipFull = br.ReadBytes((int)bufferLength);

                    //Make sure that we have enough bytes to actually deswizzle
                    var deSwizzChunkSize = GetDeswizzleSize(mipFull.Length, formatBpp, mipWidth, mipHeight, out int deSwizzWidth, out int deSwizzHeight);
                    int swizzleBlockWidth = deSwizzWidth < 8 ? 8 : deSwizzWidth;
                    int swizzleBlockHeight = deSwizzHeight < 8 ? 8 : deSwizzHeight;

                    //If it's too small, we don't need to deswizzle
                    if ((formatBpp * mipWidth * mipHeight / 8) <= sourceBytesPerPixelSet)
                    {
                        var newMipFull = new byte[sourceBytesPerPixelSet];
                        Array.Copy(mipFull, 0, newMipFull, 0, sourceBytesPerPixelSet);
                        mipFull = newMipFull;
                    }
                    else
                    {
                        mipFull = DrSwizzler.Deswizzler.PS5Deswizzle(mipFull, swizzleBlockWidth, swizzleBlockHeight, pixelFormat);

                        //Extract as a tile from the pixels if we haven't done that at the deswizzle step
                        if (swizzleBlockWidth != mipWidth || swizzleBlockHeight != mipHeight)
                        {
                            mipFull = DrSwizzler.Util.ExtractTile(mipFull, pixelFormat, swizzleBlockWidth, 0, 0, mipWidth, mipHeight);
                        }
                    }

                    imageList[s].MipLevels.Add(mipFull);
                    mipWidth /= 2;
                    mipHeight /= 2;
                }
            }

            return imageList;
        }

        private static long GetDeswizzleSize(long dataLength, int formatBpp, int width, int height, out int deSwizzWidth, out int deSwizzHeight)
        {
            if (((width * height * formatBpp) / 8) < dataLength)
            {
                if (width > height)
                {
                    deSwizzWidth = width;
                    deSwizzHeight = width;
                    return (deSwizzWidth * deSwizzHeight * formatBpp) / 8;
                }
                else
                {
                    deSwizzWidth = height;
                    deSwizzHeight = height;
                    return (deSwizzWidth * deSwizzHeight * formatBpp) / 8;
                }
            }
            else
            {
                deSwizzWidth = width;
                deSwizzHeight = height;
                return dataLength;
            }
        }

        private class Image
        {
            public List<byte[]> MipLevels;

            public Image()
            {
                MipLevels = new List<byte[]>();
            }

            public static byte[] Write(List<Image> images)
            {
                var bw = new BinaryWriterEx(false);
                foreach (Image image in images)
                    foreach (byte[] mip in image.MipLevels)
                        bw.WriteBytes(mip);
                return bw.FinishBytes();
            }
        }
    }
}
