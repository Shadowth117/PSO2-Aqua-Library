using System;
using System.IO;
using static DirectXTex.DirectXTexUtility;

namespace AquaModelLibrary.Helpers
{
    public class Deswizzler
    {
        /// <summary>
        /// RawTex Implementation
        /// </summary>
        private static int[] bpp = new int[116]
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
        public static void GetSourceBytesPerPixelAndPixelSize(DXGIFormat pixelFormat, out int sourceBytesPerPixel, out int pixelBlockSize)
        {
            int pixelFormatInt = (int)pixelFormat;
            if ((pixelFormatInt >= 70 && pixelFormatInt <= 84) || (pixelFormatInt >= 94 && pixelFormatInt <= 99))
            {
                pixelBlockSize = 4;
            } else
            {
                pixelBlockSize = 1;
            }

            int formatBpp = bpp[pixelFormatInt];
            if (pixelBlockSize == 1)
            {
                sourceBytesPerPixel = formatBpp / 8;
            } else
            {
                sourceBytesPerPixel = formatBpp * 2;
            }
        }

        /// <summary>
        /// Massive credit to Agrajag for the deswizzling here
        /// </summary>
        public static byte[] VitaDeswizzle(byte[] swizzledData, int width, int height, DXGIFormat pixelFormat)
        {
            GetSourceBytesPerPixelAndPixelSize(pixelFormat, out var sourceBytesPerPixel, out var pixelBlockSize);
            int maxU = (int)(Math.Log(width, 2));
            int maxV = (int)(Math.Log(height, 2));

            byte[] unswizzledData = new byte[swizzledData.Length];

            for (int j = 0; (j < width * height) && (j * sourceBytesPerPixel < swizzledData.Length); j++)
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
                    Array.Copy(swizzledData, j * sourceBytesPerPixel, unswizzledData, (v * width + u) * sourceBytesPerPixel, sourceBytesPerPixel);
                }

            }
            return unswizzledData;
        }

        /// <summary>
        /// RawTex Implementation
        /// </summary>
        public static byte[] PS3DeSwizzle(byte[] swizzledData, int width, int height, DXGIFormat pixelFormat)
        {
            GetSourceBytesPerPixelAndPixelSize(pixelFormat, out var sourceBytesPerPixel, out var pixelBlockSize);

            byte[] outBuffer = new byte[swizzledData.Length];
            byte[] tempBuffer = new byte[sourceBytesPerPixel];
            int sy = height / pixelBlockSize;
            int sx = width / pixelBlockSize;
            for (int t = 0; t < sx * sy; ++t)
            {
                int num5 = Morton(t, sx, sy);
                Array.Copy(swizzledData, t * sourceBytesPerPixel, tempBuffer, 0, sourceBytesPerPixel);
                int destinationIndex = sourceBytesPerPixel * num5;
                Array.Copy(tempBuffer, 0, outBuffer, destinationIndex, sourceBytesPerPixel);
            }
            return outBuffer;
        }


        /// <summary>
        /// RawTex Implementation
        /// </summary>
        public static byte[] PS4DeSwizzle(byte[] swizzledData, int width, int height, DXGIFormat pixelFormat)
        {
            GetSourceBytesPerPixelAndPixelSize(pixelFormat, out var sourceBytesPerPixel, out var pixelBlockSize);

            byte[] outBuffer = new byte[swizzledData.Length];
            byte[] tempBuffer = new byte[sourceBytesPerPixel];
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
                        Array.Copy(swizzledData, streamPos * sourceBytesPerPixel, tempBuffer, 0, sourceBytesPerPixel);
                        streamPos += sourceBytesPerPixel;
                        if (index2 * 8 + num9 < sx && index1 * 8 + num8 < sy)
                        {
                            int destinationIndex = sourceBytesPerPixel * ((index1 * 8 + num8) * sx + index2 * 8 + num9);
                            Array.Copy((Array)tempBuffer, 0, (Array)outBuffer, destinationIndex, sourceBytesPerPixel);
                        }
                    }
                }
            }
            return outBuffer;
        }

        /// <summary>
        /// RawTex Implementation
        /// </summary>
        public static byte[] PS5Deswizzle(byte[] swizzledData, int width, int height, DXGIFormat pixelFormat)
        {
            GetSourceBytesPerPixelAndPixelSize(pixelFormat, out var sourceBytesPerPixel, out var pixelBlockSize);

            byte[] outBuffer = new byte[swizzledData.Length];
            byte[] tempBuffer = new byte[sourceBytesPerPixel];
            int verticalPixelBlockCount = height / pixelBlockSize;
            int horizontalPixelBlockCount = width / pixelBlockSize;
            int num7 = 1;
            if (sourceBytesPerPixel == 16)
                num7 = 1;
            if (sourceBytesPerPixel == 8)
                num7 = 2;
            if (sourceBytesPerPixel == 4)
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
                            for (int index3 = 0; index3 < 32; ++index3)
                            {
                                Array.Copy(swizzledData, streamPos * sourceBytesPerPixel, tempBuffer, 0, sourceBytesPerPixel);
                                streamPos += sourceBytesPerPixel;
                                int num11 = index2 * 128 + num9 * 4 + index3 % 4;
                                int num12 = index1 * 128 + (num10 * 8 + index3 / 4);
                                if (num11 < horizontalPixelBlockCount && num12 < verticalPixelBlockCount)
                                {
                                    int destinationIndex = sourceBytesPerPixel * (num12 * horizontalPixelBlockCount + num11);
                                    Array.Copy(tempBuffer, 0, outBuffer, destinationIndex, sourceBytesPerPixel);
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
                                for (int index4 = 0; index4 < num7; ++index4)
                                {
                                    Array.Copy(swizzledData, streamPos * sourceBytesPerPixel, tempBuffer, 0, sourceBytesPerPixel);
                                    streamPos += sourceBytesPerPixel;
                                    int num11 = index2 * 64 + (num9 * 4 + index3 / 4) * num7 + index4;
                                    int num12 = index1 * 64 + num10 * 4 + index3 % 4;
                                    if (num11 < horizontalPixelBlockCount && num12 < verticalPixelBlockCount)
                                    {
                                        int destinationIndex = sourceBytesPerPixel * (num12 * horizontalPixelBlockCount + num11);
                                        Array.Copy(tempBuffer, 0, outBuffer, destinationIndex, sourceBytesPerPixel);
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
        public static byte[] SwitchDeswizzle(byte[] swizzledData, int width, int height, DXGIFormat pixelFormat)
        {
            GetSourceBytesPerPixelAndPixelSize(pixelFormat, out var sourceBytesPerPixel, out var pixelBlockSize);

            byte[] outBuffer = new byte[swizzledData.Length];
            byte[] tempBuffer = new byte[sourceBytesPerPixel];
            int sy = height / pixelBlockSize;
            int sx = width / pixelBlockSize;
            int[,] numArray = new int[sx * 2, sy * 2];
            int num7 = sy / 8;
            if (num7 > 16)
                num7 = 16;
            int num8 = 0;
            int num9 = 1;
            if (sourceBytesPerPixel == 16)
                num9 = 1;
            if (sourceBytesPerPixel == 8)
                num9 = 2;
            if (sourceBytesPerPixel == 4)
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

                                Array.Copy(swizzledData, streamPos * sourceBytesPerPixel, tempBuffer, 0, sourceBytesPerPixel);
                                streamPos += sourceBytesPerPixel;
                                int index6 = (index1 * num7 + index3) * 8 + num11;
                                int index7 = (index2 * 4 + num12) * num9 + index5;
                                int destinationIndex = sourceBytesPerPixel * (index6 * sx + index7);
                                Array.Copy(tempBuffer, 0, outBuffer, destinationIndex, sourceBytesPerPixel);
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
    }
}
