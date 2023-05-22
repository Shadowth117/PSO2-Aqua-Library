using AquaModelLibrary.Nova.Structures;
using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.Nova
{
    //Same format as .axs, but uses exclusively the texture data
    public static class AIFMethods
    {
        public static XgmiStruct ReadXgmi(this BufferedStreamReader streamReader)
        {
            XgmiStruct xgmiStr = new XgmiStruct();

            xgmiStr.magic = streamReader.Read<int>();
            xgmiStr.len = streamReader.Read<int>();
            xgmiStr.int_08 = streamReader.Read<int>();
            xgmiStr.paddingLen = streamReader.Read<int>();

            xgmiStr.mipIdByte = streamReader.Read<byte>();
            xgmiStr.idByte1 = streamReader.Read<byte>();
            xgmiStr.idByte2 = streamReader.Read<byte>();
            xgmiStr.idByte3 = streamReader.Read<byte>();
            xgmiStr.texCatId = streamReader.Read<int>();
            xgmiStr.int_18 = streamReader.Read<int>();
            xgmiStr.int_1C = streamReader.Read<int>();

            xgmiStr.dxtType = streamReader.Read<byte>();
            xgmiStr.bt_21 = streamReader.Read<byte>();
            xgmiStr.bt_22 = streamReader.Read<byte>();
            xgmiStr.bt_23 = streamReader.Read<byte>();
            xgmiStr.int_24 = streamReader.Read<int>();
            xgmiStr.width = streamReader.Read<ushort>();
            xgmiStr.height = streamReader.Read<ushort>();
            xgmiStr.bt_2C = streamReader.Read<byte>();
            xgmiStr.bt_2D = streamReader.Read<byte>();
            xgmiStr.alphaTesting = streamReader.Read<byte>();
            xgmiStr.bt_2F = streamReader.Read<byte>();

            xgmiStr.int_30 = streamReader.Read<int>();
            xgmiStr.int_34 = streamReader.Read<int>();
            xgmiStr.int_38 = streamReader.Read<int>();
            xgmiStr.int_3C = streamReader.Read<int>();

            xgmiStr.md5_1 = streamReader.Read<long>();
            xgmiStr.md5_2 = streamReader.Read<long>();

            xgmiStr.int_50 = streamReader.Read<int>();
            xgmiStr.int_54 = streamReader.Read<int>();
            xgmiStr.int_58 = streamReader.Read<int>();
            xgmiStr.int_5C = streamReader.Read<int>();

            xgmiStr.int_60 = streamReader.Read<int>();
            xgmiStr.int_64 = streamReader.Read<int>();
            xgmiStr.int_68 = streamReader.Read<int>();
            xgmiStr.int_6C = streamReader.Read<int>();

            //Processed data
            int mipByteIdBit = 0;
            if((xgmiStr.mipIdByte & 0x80) > 0)
            {
                mipByteIdBit = 0x80;
            }
            xgmiStr.stamCombinedId = mipByteIdBit.ToString("X2") + xgmiStr.idByte1.ToString("X2") + xgmiStr.idByte2.ToString("X2") + xgmiStr.idByte3.ToString("X2") + xgmiStr.texCatId.ToString("X");
            xgmiStr.stamUniqueId = xgmiStr.mipIdByte.ToString("X2") + xgmiStr.idByte1.ToString("X2") + xgmiStr.idByte2.ToString("X2") + xgmiStr.idByte3.ToString("X2") + xgmiStr.texCatId.ToString("X");

            return xgmiStr;
        }

        public static byte[] GetImage(XgmiStruct xgmiStr, byte[] buffer)
        {
            int sourceBytesPerPixel;
            switch(xgmiStr.dxtType)
            {
                case 0x10:
                    sourceBytesPerPixel = 0x8;
                    break;
                case 0x12:
                    sourceBytesPerPixel = 0x10;
                    break;
                case 0x14:
                    sourceBytesPerPixel = 0x10;
                    break;
                default:
                    sourceBytesPerPixel = 0x8;
                    break;
            }
            var processedBuffer = Unswizzle(buffer, xgmiStr.width >> 2, xgmiStr.height >> 2, sourceBytesPerPixel);
            var dds = AssembleDDS(processedBuffer, xgmiStr.height, xgmiStr.width, xgmiStr.alphaTesting, xgmiStr.dxtType);
            return dds;
            /*using (var image = Pfim.Pfim.FromStream(new MemoryStream(dds)))
            {
                // Pin pfim's data array so that it doesn't get reaped by GC, unnecessary
                // in this snippet but useful technique if the data was going to be used in
                // control like a picture box
                var bytes = (new MemoryStream(image.Data)).ToArray();
                Bitmap bmp = new Bitmap(xgmiStr.width, xgmiStr.height, PixelFormat.Format32bppArgb);
                BitmapData currMip = bmp.LockBits(new Rectangle(0, 0, xgmiStr.width, xgmiStr.height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

                Marshal.Copy(bytes, 0, currMip.Scan0, bytes.Length);
                bytes = null;
                bmp.UnlockBits(currMip);

                return bmp;
            }*/
        }

        //Massive credit to Agrajag for the deswizzling here
        public static byte[] Unswizzle(byte[] swizzledData, int width, int height, int sourceBytesPerPixel)
        {
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

        //Massive credit to Agrajag for the deswizzling here
        public static byte[] blockLevelUnswizzle(byte[] swizzledData, int width, int height, int bytesPerPixel)
        {
            int maxV = (int)(Math.Log(width, 2)) - 2; //-2 exponent = /4
            int maxU = (int)(Math.Log(height, 2)) - 2; //-2 exponent = /4

            byte[] unswizzledData = new byte[width * height * bytesPerPixel];

            for (int j = 0; (j * 16 < width * height) && (j * bytesPerPixel * 16 < swizzledData.Length); j++)
            {
                int u = 0, v = 0;
                int originalBlockId = j;
                for (int k = 0; k < maxU || k < maxV; k++)
                {
                    if (k < maxV)   //Transpose!
                    {
                        v |= (originalBlockId & 1) << k;
                        originalBlockId >>= 1;
                    }
                    if (k < maxU)   //Transpose!
                    {
                        u |= (originalBlockId & 1) << k;
                        originalBlockId >>= 1;
                    }
                }
                if (u < (height / 4) && v < (width / 4))
                {
                    copyBlock(bytesPerPixel, width, swizzledData, u, v, unswizzledData, j);
                }
            }
            return unswizzledData;
        }

        //Again, massive credit to Agrajag for the deswizzling here
        public static void copyBlock(int bytesPerPixel, int stride, byte[] swizzledData, int swizzleBlockX, int swizzleBlockY, byte[] unswizzledData, int unswizzledOffset)
        {
            int blocksPerRow = stride / 4;
            int row = unswizzledOffset / blocksPerRow;
            int indexInRow = unswizzledOffset % blocksPerRow;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    //Copy each row over.
                    Array.Copy(swizzledData, (row * stride * 4 + indexInRow * 4 + i * stride + j) * bytesPerPixel, unswizzledData, ((swizzleBlockY * 4 + i) * stride + swizzleBlockX * 4 + j) * bytesPerPixel, bytesPerPixel);
                }
            }
        }

        public static byte[] AssembleDDS(byte[] buffer, int width, int height, byte alphaTesting, byte pixelFormat, byte mipCount = 1)
        {
            List<byte> outBytes = new List<byte>();
            outBytes.AddRange(GenerateDDSHeader((ushort)width, (ushort)height, buffer.Length, alphaTesting, pixelFormat, mipCount));
            outBytes.AddRange(buffer);

            return outBytes.ToArray();
        }

        public static byte[] GenerateDDSHeader(ushort height, ushort width, int size, byte alphaTesting, byte pixelFormat, byte mipCount = 1)
        {
            var outBytes = new List<byte>();

            outBytes.AddRange(new byte[] { 0x44, 0x44, 0x53, 0x20, 0x7C, 0x00, 0x00, 0x00, 0x07, 0x10, 0x08, 0x00 });
            outBytes.AddRange(BitConverter.GetBytes(height));
            outBytes.AddRange(new byte[] { 0x00, 0x00 });
            outBytes.AddRange(BitConverter.GetBytes(width));
            outBytes.AddRange(new byte[] { 0x00, 0x00 });
            outBytes.AddRange(BitConverter.GetBytes(size));
            outBytes.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00, });
            outBytes.AddRange(new byte[] { mipCount, 0x00, 0x00, 0x00, });
            outBytes.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  0x20, 0x00, 0x00, 0x00,
                0x04, 0x00, 0x00, 0x00,  });
            outBytes.AddRange(new byte[] { 0x44, 0x58, 0x54}); 
            if(alphaTesting == 0x8)
            {
                if(pixelFormat == 0x12)
                {
                    outBytes.Add(0x33); //DXT3
                } else
                {
                    outBytes.Add(0x35); //DXT5
                }
            } else
            {
                outBytes.Add(0x31); //DXT1
            }
            outBytes.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  0x00, 0x10, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00 });

            return outBytes.ToArray();
        }
    }
}
