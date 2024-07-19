using DrSwizzler;
using static DirectXTex.DirectXTexUtility;

namespace AquaModelLibrary.Helpers
{
    public class DeSwizzler
    {
        /// <summary>
        /// RawTex Implementation
        /// </summary>
        private static int[] bitsPerPixel = new int[116]
        {
              0,
              128,
              128,
              128,
              128,
              96,
              96,
              96,
              96,
              64,
              64,
              64,
              64,
              64,
              64,
              64,
              64,
              64,
              64,
              64,
              64,
              64,
              64,
              32,
              32,
              32,
              32,
              32,
              32,
              32,
              32,
              32,
              32,
              32,
              32,
              32,
              32,
              32,
              32,
              32,
              32,
              32,
              32,
              32,
              32,
              32,
              32,
              32,
              16,
              16,
              16,
              16,
              16,
              16,
              16,
              16,
              16,
              16,
              16,
              16,
              8,
              8,
              8,
              8,
              8,
              8,
              1,
              32,
              32,
              32,
              4,
              4,
              4,
              8,
              8,
              8,
              8,
              8,
              8,
              4,
              4,
              4,
              8,
              8,
              8,
              16,
              16,
              32,
              32,
              32,
              32,
              32,
              32,
              32,
              8,
              8,
              8,
              8,
              8,
              8,
              0,
              0,
              0,
              0,
              0,
              0,
              0,
              0,
              0,
              0,
              0,
              0,
              0,
              0,
              0,
              16
        };

        /// <summary>
        /// RawTex Switch Texture Info
        /// </summary>
        private static int[] swi = new int[32]
        {
              0,
              4,
              1,
              5,
              8,
              12,
              9,
              13,
              16,
              20,
              17,
              21,
              24,
              28,
              25,
              29,
              2,
              6,
              3,
              7,
              10,
              14,
              11,
              15,
              18,
              22,
              19,
              23,
              26,
              30,
              27,
              31
        };

        /// <summary>
        /// Based on RawTex handling
        /// </summary>
        public static void GetsourceBytesPerPixelSetAndPixelSize(DXGIFormat pixelFormat, out int sourceBytesPerPixelSet, out int pixelBlockSize, out int formatBpp)
        {
            int pixelFormatInt = (int)pixelFormat;
            if ((pixelFormatInt >= 70 && pixelFormatInt <= 84) || (pixelFormatInt >= 94 && pixelFormatInt <= 99))
            {
                pixelBlockSize = 4;
            }
            else
            {
                pixelBlockSize = 1;
            }

            formatBpp = bitsPerPixel[pixelFormatInt];
            if (pixelBlockSize == 1)
            {
                sourceBytesPerPixelSet = formatBpp / 8;
            }
            else
            {
                sourceBytesPerPixelSet = formatBpp * 2;
            }
        }

        /// <summary>
        /// Massive credit to Agrajag for the deswizzling here
        /// </summary>
        public static byte[] VitaDeSwizzle(byte[] swizzledData, int width, int height, DXGIFormat pixelFormat)
        {
            return DrSwizzler.Deswizzler.PS4Deswizzle(swizzledData, width, height, (DrSwizzler.DDS.DXEnums.DXGIFormat)pixelFormat);
        }

        public static byte[] VitaDeSwizzle(byte[] swizzledData, int width, int height, int sourceBytesPerPixelSet, int formatbpp)
        {
            return DrSwizzler.Deswizzler.VitaDeswizzle(swizzledData, width, height, sourceBytesPerPixelSet, formatbpp);
        }

        public static byte[] Xbox360DeSwizzle(byte[] swizzledData, int width, int height, DXGIFormat pixelFormat)
        {
            return DrSwizzler.Deswizzler.Xbox360Deswizzle(swizzledData, width, height, (DrSwizzler.DDS.DXEnums.DXGIFormat)pixelFormat);
        }

        public static byte[] Xbox360DeSwizzle(byte[] swizzledData, int width, int height, DXGIFormat pixelFormat, int sourceBytesPerPixelSet, int pixelBlockSize, int formatbpp)
        {
            return DrSwizzler.Deswizzler.Xbox360Deswizzle(swizzledData, width, height, (DrSwizzler.DDS.DXEnums.DXGIFormat)pixelFormat, sourceBytesPerPixelSet, pixelBlockSize, formatbpp);
        }

        /// <summary>
        /// RawTex Implementation
        /// </summary>
        public static byte[] PS3DeSwizzle(byte[] swizzledData, int width, int height, DXGIFormat pixelFormat)
        {
            return DrSwizzler.Deswizzler.PS3Deswizzle(swizzledData, width, height, (DrSwizzler.DDS.DXEnums.DXGIFormat)pixelFormat);
        }

        public static byte[] PS3DeSwizzle(byte[] swizzledData, int width, int height, int sourceBytesPerPixelSet, int pixelBlockSize, int formatbpp)
        {
            return DrSwizzler.Deswizzler.PS3Deswizzle(swizzledData, width, height, sourceBytesPerPixelSet, pixelBlockSize, formatbpp);
        }

        /// <summary>
        /// RawTex Implementation
        /// </summary>
        public static byte[] PS4DeSwizzle(byte[] swizzledData, int width, int height, DXGIFormat pixelFormat)
        {
            return DrSwizzler.Deswizzler.PS4Deswizzle(swizzledData, width, height, (DrSwizzler.DDS.DXEnums.DXGIFormat)pixelFormat);
        }

        public static byte[] PS4DeSwizzle(byte[] swizzledData, int width, int height, int sourceBytesPerPixelSet, int pixelBlockSize, int formatbpp)
        {
            return DrSwizzler.Deswizzler.PS4Deswizzle(swizzledData, width, height, sourceBytesPerPixelSet, pixelBlockSize, formatbpp);
        }

        /// <summary>
        /// RawTex Implementation
        /// </summary>
        public static byte[] PS5DeSwizzle(byte[] swizzledData, int width, int height, DXGIFormat pixelFormat)
        {
            return DrSwizzler.Deswizzler.PS5Deswizzle(swizzledData, width, height, (DrSwizzler.DDS.DXEnums.DXGIFormat)pixelFormat);
        }

        public static byte[] PS5DeSwizzle(byte[] swizzledData, int width, int height, int sourceBytesPerPixelSet, int pixelBlockSize, int formatbpp)
        {
            return DrSwizzler.Deswizzler.PS5Deswizzle(swizzledData, width, height, sourceBytesPerPixelSet, pixelBlockSize, formatbpp);
        }


        /// <summary>
        /// RawTex Implementation
        /// </summary>
        public static byte[] SwitchDeSwizzle(byte[] swizzledData, int width, int height, DXGIFormat pixelFormat)
        {
            return DrSwizzler.Deswizzler.SwitchDeswizzle(swizzledData, width, height, (DrSwizzler.DDS.DXEnums.DXGIFormat)pixelFormat);
        }

        public static byte[] SwitchDeSwizzle(byte[] swizzledData, int width, int height, int sourceBytesPerPixelSet, int pixelBlockSize, int formatbpp)
        {
            return DrSwizzler.Deswizzler.SwitchDeswizzle(swizzledData, width, height, sourceBytesPerPixelSet, pixelBlockSize, formatbpp);
        }

        /// <summary>
        /// Grabs a tile from from an array of pixels. Expects a tile divisible by two
        /// </summary>
        public static byte[] ExtractTile(byte[] texBuffer, DXGIFormat pixelFormat, int texBufferTotalWdith, int tileLeftmostPixel, int tileTopmostPixel, int tileWidth, int tileHeight)
        {
            GetsourceBytesPerPixelSetAndPixelSize(pixelFormat, out var pixelSetSize, out var pixelBlockSize, out int formatbpp);
            return ExtractTile(texBuffer, ref texBufferTotalWdith, ref tileLeftmostPixel, ref tileTopmostPixel, ref tileWidth, ref tileHeight, pixelSetSize, pixelBlockSize, formatbpp);
        }

        public static byte[] ExtractTile(byte[] texBuffer, ref int texBufferTotalWdith, ref int tileLeftmostPixel, ref int tileTopmostPixel, ref int tileWidth, ref int tileHeight, int pixelSetSize, int pixelBlockSize, int formatbpp)
        {
            byte[] tileBuffer = new byte[(formatbpp * tileWidth * tileHeight) / 8];

            if (pixelBlockSize == 4)
            {
                tileHeight /= 4;
                tileTopmostPixel /= 4;
                tileLeftmostPixel /= 4;
                texBufferTotalWdith /= 4;
                tileWidth /= 4;
            }

            for (int i = tileTopmostPixel; i < tileHeight; i++)
            {
                var rowStart = (tileLeftmostPixel * pixelSetSize) + (i * texBufferTotalWdith * pixelSetSize);
                Array.Copy(texBuffer, rowStart, tileBuffer, i * (pixelSetSize * tileWidth), pixelSetSize * tileWidth);
            }

            return tileBuffer;
        }

        /// <summary>
        /// Takes in a pixel buffer size, pixel size, the width, or top / left,of an aspect ratio and the height, or bottom / right, of an aspect ratio and outputs a width and height.
        /// Intended for integer output for things like pixel dimensions.
        /// </summary>
        public static void GetDimensionsFromPixelBufferCount_PixelSizeAndAspectRatio(int bufferSize, int pixelSize, int aspectWidth, int aspectHeight, out int width, out int height)
        {
            GetDimensionsFromAreaAndAspectRatio(bufferSize / pixelSize, aspectWidth, aspectHeight, out width, out height);
        }

        /// <summary>
        /// Takes in an area, the width, or top / left,of an aspect ratio and the height, or bottom / right, of an aspect ratio and outputs a width and height.
        /// Intended for integer output for things like pixel dimensions.
        /// </summary>
        public static void GetDimensionsFromAreaAndAspectRatio(int area, int aspectWidth, int aspectHeight, out int width, out int height)
        {
            int multFactorWidth = area * aspectWidth / aspectWidth;
            int multFactorHeight = area * aspectHeight / aspectWidth;

            width = (int)Math.Sqrt(multFactorWidth);
            height = (int)Math.Sqrt(multFactorHeight);
        }
    }
}
