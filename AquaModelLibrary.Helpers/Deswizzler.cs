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
            GetsourceBytesPerPixelSetAndPixelSize(pixelFormat, out var sourceBytesPerPixelSet, out var pixelBlockSize, out int formatbpp);
            int maxU = (int)(Math.Log(width, 2));
            int maxV = (int)(Math.Log(height, 2));

            byte[] unswizzledData = new byte[(formatbpp * width * height) / 8];

            for (int j = 0; (j < width * height) && (j * sourceBytesPerPixelSet < swizzledData.Length); j++)
            {
                int u = 0, v = 0;
                int origCoord = j;
                for (int k = 0; k < maxU || k < maxV; k++)
                {
                    if (k < maxV)   //Transpose!
                    {
                        v |= (origCoord & 1) << k;
                        origCoord >>= 1;
                    }
                    if (k < maxU)   //Transpose!
                    {
                        u |= (origCoord & 1) << k;
                        origCoord >>= 1;
                    }
                }
                if (u < width && v < height)
                {
                    Array.Copy(swizzledData, j * sourceBytesPerPixelSet, unswizzledData, (v * width + u) * sourceBytesPerPixelSet, sourceBytesPerPixelSet);
                }

            }
            return unswizzledData;
        }


        public static byte[] Xbox360DeSwizzle(byte[] swizzledData, int width, int height, DXGIFormat pixelFormat)
        {
            GetsourceBytesPerPixelSetAndPixelSize(pixelFormat, out var sourceBytesPerPixelSet, out var pixelBlockSize, out int formatbpp);



            return swizzledData;
        }

        /// <summary>
        /// RawTex Implementation
        /// </summary>
        public static byte[] PS3DeSwizzle(byte[] swizzledData, int width, int height, DXGIFormat pixelFormat)
        {
            GetsourceBytesPerPixelSetAndPixelSize(pixelFormat, out var sourceBytesPerPixelSet, out var pixelBlockSize, out int formatbpp);

            byte[] outBuffer = new byte[(formatbpp * width * height) / 8];
            byte[] tempBuffer = new byte[sourceBytesPerPixelSet];
            int sy = height / pixelBlockSize;
            int sx = width / pixelBlockSize;
            for (int t = 0; t < sx * sy; ++t)
            {
                int num5 = Morton(t, sx, sy);
                Array.Copy(swizzledData, t * sourceBytesPerPixelSet, tempBuffer, 0, sourceBytesPerPixelSet);
                int destinationIndex = sourceBytesPerPixelSet * num5;
                Array.Copy(tempBuffer, 0, outBuffer, destinationIndex, sourceBytesPerPixelSet);
            }
            return outBuffer;
        }

        /// <summary>
        /// RawTex Implementation
        /// </summary>
        public static byte[] PS4DeSwizzle(byte[] swizzledData, int width, int height, DXGIFormat pixelFormat)
        {
            GetsourceBytesPerPixelSetAndPixelSize(pixelFormat, out var sourceBytesPerPixelSet, out var pixelBlockSize, out int formatbpp);

            byte[] outBuffer = new byte[(formatbpp * width * height) / 8];
            byte[] tempBuffer = new byte[sourceBytesPerPixelSet];
            int sy = height / pixelBlockSize;
            int sx = width / pixelBlockSize;

            int streamPos = 0;
            for (int index1 = 0; index1 < (sy + 7) / 8; ++index1)
            {
                for (int index2 = 0; index2 < (sx + 7) / 8; ++index2)
                {
                    for (int t = 0; t < 64; ++t)
                    {
                        int num7 = Morton(t, 8, 8);
                        int num8 = num7 / 8;
                        int num9 = num7 % 8;

                        var byteLimit = (swizzledData.Length - sourceBytesPerPixelSet);
                        if (streamPos > byteLimit)
                        {
                            return outBuffer;
                        }
                        Array.Copy(swizzledData, streamPos, tempBuffer, 0, sourceBytesPerPixelSet);
                        streamPos += sourceBytesPerPixelSet;
                        if (index2 * 8 + num9 < sx && index1 * 8 + num8 < sy)
                        {
                            int destinationIndex = sourceBytesPerPixelSet * ((index1 * 8 + num8) * sx + index2 * 8 + num9);
                            Array.Copy((Array)tempBuffer, 0, (Array)outBuffer, destinationIndex, sourceBytesPerPixelSet);
                        }
                    }
                }
            }
            return outBuffer;
        }

        /// <summary>
        /// RawTex Implementation
        /// </summary>
        public static byte[] PS5DeSwizzle(byte[] swizzledData, int width, int height, DXGIFormat pixelFormat)
        {
            GetsourceBytesPerPixelSetAndPixelSize(pixelFormat, out var sourceBytesPerPixelSet, out var pixelBlockSize, out int formatbpp);

            byte[] outBuffer = new byte[(formatbpp * width * height) / 8];
            byte[] tempBuffer = new byte[sourceBytesPerPixelSet];
            int verticalPixelBlockCount = height / pixelBlockSize;
            int horizontalPixelBlockCount = width / pixelBlockSize;
            int num7 = 1;
            if (sourceBytesPerPixelSet == 16)
                num7 = 1;
            if (sourceBytesPerPixelSet == 8)
                num7 = 2;
            if (sourceBytesPerPixelSet == 4)
                num7 = 4;

            int streamPos = 0;
            if (pixelBlockSize == 1)
            {
                for (int index1 = 0; index1 < (verticalPixelBlockCount + (int)sbyte.MaxValue) / 128; ++index1)
                {
                    for (int index2 = 0; index2 < (horizontalPixelBlockCount + (int)sbyte.MaxValue) / 128; ++index2)
                    {
                        for (int t = 0; t < 512; ++t)
                        {
                            int num8 = Morton(t, 32, 16);
                            int num9 = num8 % 32;
                            int num10 = num8 / 32;
                            for (int index3 = 0; index3 < 32 && streamPos + 0x10 < swizzledData.Length; ++index3)
                            {
                                Array.Copy(swizzledData, streamPos, tempBuffer, 0, sourceBytesPerPixelSet);
                                streamPos += sourceBytesPerPixelSet;
                                int currentHorizontalPixelBlock = index2 * 128 + num9 * 4 + index3 % 4;
                                int currentVerticalPixelBlock = index1 * 128 + (num10 * 8 + index3 / 4);
                                if (currentHorizontalPixelBlock < horizontalPixelBlockCount && currentVerticalPixelBlock < verticalPixelBlockCount)
                                {
                                    int destinationIndex = sourceBytesPerPixelSet * (currentVerticalPixelBlock * horizontalPixelBlockCount + currentHorizontalPixelBlock);
                                    Array.Copy(tempBuffer, 0, outBuffer, destinationIndex, sourceBytesPerPixelSet);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                for (int index1 = 0; index1 < (verticalPixelBlockCount + 63) / 64; ++index1)
                {
                    for (int index2 = 0; index2 < (horizontalPixelBlockCount + 63) / 64; ++index2)
                    {
                        for (int t = 0; t < 256 / num7; ++t)
                        {
                            int num8 = Morton(t, 16, 16 / num7);
                            int num9 = num8 / 16;
                            int num10 = num8 % 16;
                            for (int index3 = 0; index3 < 16; ++index3)
                            {
                                for (int index4 = 0; index4 < num7 && streamPos + 0x10 < swizzledData.Length; ++index4)
                                {
                                    Array.Copy(swizzledData, streamPos, tempBuffer, 0, sourceBytesPerPixelSet);
                                    streamPos += sourceBytesPerPixelSet;
                                    int currentHorizontalPixelBlock = index2 * 64 + (num9 * 4 + index3 / 4) * num7 + index4;
                                    int currentVerticalPixelBlock = index1 * 64 + num10 * 4 + index3 % 4;
                                    if (currentHorizontalPixelBlock < horizontalPixelBlockCount && currentVerticalPixelBlock < verticalPixelBlockCount)
                                    {
                                        int destinationIndex = sourceBytesPerPixelSet * (currentVerticalPixelBlock * horizontalPixelBlockCount + currentHorizontalPixelBlock);
                                        Array.Copy(tempBuffer, 0, outBuffer, destinationIndex, sourceBytesPerPixelSet);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return outBuffer;
        }


        /// <summary>
        /// RawTex Implementation
        /// </summary>
        public static byte[] SwitchDeSwizzle(byte[] swizzledData, int width, int height, DXGIFormat pixelFormat)
        {
            GetsourceBytesPerPixelSetAndPixelSize(pixelFormat, out var sourceBytesPerPixelSet, out var pixelBlockSize, out int formatbpp);

            byte[] outBuffer = new byte[(formatbpp * width * height) / 8];
            byte[] tempBuffer = new byte[sourceBytesPerPixelSet];
            int sy = height / pixelBlockSize;
            int sx = width / pixelBlockSize;
            int[,] numArray = new int[sx * 2, sy * 2];
            int num7 = sy / 8;
            if (num7 > 16)
                num7 = 16;
            int num8 = 0;
            int num9 = 1;
            if (sourceBytesPerPixelSet == 16)
                num9 = 1;
            if (sourceBytesPerPixelSet == 8)
                num9 = 2;
            if (sourceBytesPerPixelSet == 4)
                num9 = 4;

            int streamPos = 0;
            for (int index1 = 0; index1 < sy / 8 / num7; ++index1)
            {
                for (int index2 = 0; index2 < sx / 4 / num9; ++index2)
                {
                    for (int index3 = 0; index3 < num7; ++index3)
                    {
                        for (int index4 = 0; index4 < 32; ++index4)
                        {
                            for (int index5 = 0; index5 < num9; ++index5)
                            {
                                int num10 = swi[index4];
                                int num11 = num10 / 4;
                                int num12 = num10 % 4;

                                Array.Copy(swizzledData, streamPos, tempBuffer, 0, sourceBytesPerPixelSet);
                                streamPos += sourceBytesPerPixelSet;
                                int index6 = (index1 * num7 + index3) * 8 + num11;
                                int index7 = (index2 * 4 + num12) * num9 + index5;
                                int destinationIndex = sourceBytesPerPixelSet * (index6 * sx + index7);
                                Array.Copy(tempBuffer, 0, outBuffer, destinationIndex, sourceBytesPerPixelSet);
                                numArray[index7, index6] = num8;
                                ++num8;
                            }
                        }
                    }
                }
            }

            return tempBuffer;
        }

        /// <summary>
        /// RawTex Implementation
        /// </summary>
        private static int Morton(int t, int sx, int sy)
        {
            int num1;
            int num2 = num1 = 1;
            int num3 = t;
            int num4 = sx;
            int num5 = sy;
            int num6 = 0;
            int num7 = 0;
            while (num4 > 1 || num5 > 1)
            {
                if (num4 > 1)
                {
                    num6 += num2 * (num3 & 1);
                    num3 >>= 1;
                    num2 *= 2;
                    num4 >>= 1;
                }
                if (num5 > 1)
                {
                    num7 += num1 * (num3 & 1);
                    num3 >>= 1;
                    num1 *= 2;
                    num5 >>= 1;
                }
            }
            return num7 * sx + num6;
        }

        /// <summary>
        /// Grabs a tile from from an array of pixels. Expects a tile divisible by two
        /// </summary>
        public static byte[] ExtractTile(byte[] texBuffer, DXGIFormat pixelFormat, int texBufferTotalWdith, int tileLeftmostPixel, int tileTopmostPixel, int tileWidth, int tileHeight)
        {
            GetsourceBytesPerPixelSetAndPixelSize(pixelFormat, out var pixelSetSize, out var pixelBlockSize, out int formatbpp);
            byte[] tileBuffer = new byte[(formatbpp * tileWidth * tileHeight) / 8];

            if (pixelBlockSize == 4)
            {
                tileHeight /= 4;
                tileTopmostPixel /= 4;
                tileLeftmostPixel /= 4;
                texBufferTotalWdith /= 4;
                tileWidth /= 4;
            }

            for(int i = tileTopmostPixel; i < tileHeight; i++)
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
