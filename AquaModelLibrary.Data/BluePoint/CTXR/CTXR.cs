using AquaModelLibrary.Data.DirectX;
using AquaModelLibrary.Data.Utility;
using AquaModelLibrary.Helpers;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers.Writers;
using static AquaModelLibrary.Helpers.DDS.DirectXTexUtility;

namespace AquaModelLibrary.Data.BluePoint.CTXR
{
    public class CTXR
    {
        public bool isPng = false;
        public int textureFormat = -1;
        public short texFlags;
        public short alphaSetting;
        public CTextureType textureType;
        public short externalMipCount;
        public short internalMipCount;
        public int sliceCount;
        /// <summary>
        /// Combined size of all external texture buffers. Not in Demon's Souls
        /// </summary>
        public int externalTexturesSize;
        public byte WidthBaseByte;
        public byte WidthMultiplierByte;
        public byte HeightBaseByte;
        public byte HeightMultiplierByte;

        //Mainly used in older types
        public int explicitWidth;
        public int explicitHeight;

        public List<CTXRExternalReference> mipPaths = new List<CTXRExternalReference>();
        public List<List<byte[]>> mipMapsList = new List<List<byte[]>>();
        public CFooter footerData;

        public CTXR()
        {

        }

        public CTXR(byte[] file)
        {
            file = CompressionHandler.CheckCompression(file);

            //Apparently these can just be actual .png files? Well, gotta check for that.
            var magicCheck = BitConverter.ToUInt32(file, 0);
            if (isPng = magicCheck == 0x474E5089)
            {
                mipMapsList.Add(new List<byte[]> { file });
                return;
            }
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr, magicCheck);
            }
        }

        public CTXR(byte[] file, bool readTexBuffers)
        {
            file = CompressionHandler.CheckCompression(file);

            //Apparently these can just be actual .png files? Well, gotta check for that.
            var magicCheck = BitConverter.ToUInt32(file, 0);
            if (isPng = magicCheck == 0x474E5089)
            {
                mipMapsList.Add(new List<byte[]> { file });
                return;
            }
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr, magicCheck, readTexBuffers);
            }
        }

        public static void ConvertDDSToCTXR_CTXC(string ddsPath, string ctxrPath)
        {
            ConvertDDSToCTXR_CTXC(File.ReadAllBytes(ddsPath), ctxrPath);
        }

        public static void ConvertDDSToCTXR_CTXC(byte[] ddsBytes, string ctxrPath)
        {
            CTXR ctxr = new();
            var dds = new DDS(ddsBytes);
            var dxgi = dds.GetDXGIFormat();
            ctxr.textureFormat = GetCTXRFormatFromDXGIFormat(dxgi);
            ctxr.alphaSetting = 1;
            ctxr.footerData = new() { version = 0x6E };
            ctxr.SetDesWidthComponent(dds.dwWidth);
            ctxr.SetDesHeightComponent(dds.dwHeight);

            //If this is meant to be a 'Standard' type CTXR, we probably want to externalize textures with dimensions beyond 256
            if ((dds.dwCaps2 & DDS.DDSCAPS2.CUBEMAP) == 0 && (dds.dwCaps2 & DDS.DDSCAPS2.VOLUME) == 0 && dds.mipData[0].subImages.Count > 1)
            {
                var largeSide = Math.Max(dds.dwWidth, dds.dwHeight);
                while(largeSide > 256)
                {
                    largeSide /= 2;
                    ctxr.externalMipCount++;
                }

                var width = GetDesResolutionComponent(ctxr.WidthBaseByte, ctxr.WidthMultiplierByte, 0xC0);
                var height = GetDesResolutionComponent(ctxr.HeightBaseByte, ctxr.HeightMultiplierByte, 0x80);
                string rootPath = PSUtility.GetPSRootPath(ctxrPath);
                ctxr.mipMapsList = new();
                ctxr.mipMapsList.Add(new List<byte[]>());

                for (int i = 0; i < dds.mipData[0].subImages.Count; i++)
                {
                    if(i < ctxr.externalMipCount)
                    {
                        string ctxcPath = Path.ChangeExtension(ctxrPath, $".chunk{i}.ctxc");
                        File.WriteAllBytes(ctxcPath, ctxr.WriteAndSwizzleCTexChunk(dxgi, width, height, dds.mipData[0].subImages[i], out int finalBufferSize));

                        //External mip paths go in reverse order
                        ctxr.mipPaths.Insert(0, new CTXRExternalReference()
                        {
                            mipLevel = (short)i,
                            unkSht0 = 1,
                            bufferSize = finalBufferSize,
                            externalMipReference = ctxcPath.Replace(rootPath, "$").Replace(@"\", "/").Replace("_ps5", "****")
                        });
                    } else
                    {
                        ctxr.mipMapsList[0].Add(dds.mipData[0].subImages[i]);
                    }
                    width /= 2;
                    height /= 2;
                }
            } else
            {
                ctxr.mipMapsList = new();
                for (int i = 0; i < dds.mipData.Count; i++)
                {
                    ctxr.mipMapsList.Add(new List<byte[]>());
                    for(int j = 0; j < dds.mipData[i].subImages.Count; j++)
                    {
                        ctxr.mipMapsList[i].Add(dds.mipData[i].subImages[j]);
                    }
                }
            }

            File.WriteAllBytes(ctxrPath, ctxr.GetBytes());
        }

        public byte[] GetBytes()
        {
            if(isPng)
            {
                return mipMapsList[0][0];
            }
            switch(footerData.version)
            {
                case 0x1:
                    throw new NotImplementedException();
                case 0x25:
                    throw new NotImplementedException();
                case 0x6E:
                    return GetDeSRBytes();
                default:
                    throw new NotImplementedException();
            }
        }

        public byte[] GetDeSRBytes()
        {
            ByteListWriter outBytes = new();
            outBytes.AddValue(textureFormat);
            outBytes.AddValue((ushort)(textureFormat == 3 ? 0xA : 0xFFFF));
            outBytes.AddValue(alphaSetting);
            outBytes.AddValue((int)textureType);
            outBytes.AddValue(0); //UnkInt1
            outBytes.AddValue((mipPaths.Count + 1));
            outBytes.AddValue((ushort)(mipPaths.Count));
            outBytes.AddValue((ushort)mipMapsList[0].Count);

            var ctxrInternalTexBufferSize = mipMapsList[0].Count > 1 ? mipMapsList[0][0].Length * 2 : mipMapsList[0][0].Length;
            CalcLowerMipBufferSizes(ctxrInternalTexBufferSize);
            outBytes.AddValue(ctxrInternalTexBufferSize);
            outBytes.AddValue(0); //UnkInt2
            outBytes.Add(0); //UnkByte
            foreach (var ext in mipPaths)
            {
                outBytes.AddRange(ext.GetBytes());
            }
            outBytes.AddValue(0); //UnkInt3
            outBytes.AddValue((short)0); //UnkShort3

            outBytes.AddValue(GetFormatInfo0());
            outBytes.AddValue(WidthBaseByte);
            outBytes.AddValue(WidthMultiplierByte);
            outBytes.AddValue(HeightBaseByte);
            outBytes.AddValue(HeightMultiplierByte);
            outBytes.AddValue(GetFormatInfo1());

            var minMipLevel = GetMinMipLevel();
            outBytes.AddValue(GetTypeMipFlags(minMipLevel));
            outBytes.AddValue((short)(mipMapsList.Count - 1));
            outBytes.AddValue((short)0);
            outBytes.AddValue(0x10 * minMipLevel);
            outBytes.AddValue(0);
            outBytes.AddValue(0);

            //Write internal mip buffers                    
            var texWidth = GetDesResolutionComponent(WidthBaseByte, WidthMultiplierByte, 0xC0);
            var texHeight = GetDesResolutionComponent(HeightBaseByte, HeightMultiplierByte, 0x80);
            GetLargestInternalMipResolution(texWidth, texHeight,
                externalMipCount, out var finalWidth, out var finalHeight);

            var outBuffers = PackDeSRTextureBuffers(out int gapBufferLength);
            outBytes.AddRange(new byte[gapBufferLength]);
            for(int i = 0; i < outBuffers.Count; i++)
            {
                for(int j = outBuffers[i].Count - 1; j >= 0; j--)
                {
                    outBytes.AddRange(outBuffers[i][j]);
                }
            }

            //Write footer
            var fileSize = outBytes.Count;
            outBytes.AddRange([0x52, 0x54, 0x58, 0x54, 0x6E, 0, 0, 1]);
            outBytes.AddValue(fileSize);

            return outBytes.ToArray();
        }

        public void CalcLowerMipBufferSizes(int ctxrInternalTexBufferSize)
        {
            foreach (var reference in mipPaths)
            {
                int remaining = ctxrInternalTexBufferSize;
                foreach (var other in mipPaths)
                {
                    //We check all of these in case they're out of order
                    if (other.mipLevel > reference.mipLevel)
                    {
                        remaining += other.bufferSize;
                    }
                }
                reference.lowerMipBufferSize = remaining;
            }
        }

        public ushort GetTypeMipFlags(int minMipLevel)
        {
            int flags = 0;
            switch(textureType)
            {
                case CTextureType.Standard:
                    flags = 0x9000;
                    break;
                case CTextureType.Volume:
                    flags = 0xA000;
                    break;
                case CTextureType.CubeMap:
                    flags = 0xB000;
                    break;
                case CTextureType.LargeUI:
                    flags = 0xD000;
                    break;
            }

            switch(textureFormat)
            {
                case 3:
                    break;
                default:
                    flags |= 0x90;
                    break;
            }

            return (ushort)(flags | minMipLevel);
        }

        public int GetMinMipLevel()
        {
            int width = GetDesResolutionComponent(WidthBaseByte, WidthMultiplierByte, 0xC0);
            int height = GetDesResolutionComponent(HeightBaseByte, HeightMultiplierByte, 0x80);
            int maxDimensionPow2 = Math.Max(width, height);
            int log2 = 0;
            while (maxDimensionPow2 > 1)
            {
                maxDimensionPow2 /= 2;
                log2++;
            }
            int totalMips = externalMipCount + mipMapsList[0].Count;
            return Math.Min(log2, totalMips - 1);
        }

        private ushort GetFormatInfo0()
        {
            ushort dSht0;
            switch (textureFormat) 
            {
                case 0x0:
                case 0x1:
                    dSht0 = 0xC380;
                    break;
                case 0x3:
                    dSht0 = 0xC160;
                    break;
                case 0xB:
                    dSht0 = 0xCA90;
                    break;
                case 0xC:
                    dSht0 = 0xCAB0; 
                    break;
                case 0xD:
                    dSht0 = 0xCAD0;
                    break;
                case 0xE:
                    dSht0 = 0xCAF0;
                    break;
                case 0xF:
                    dSht0 = 0xCB10;
                    break;
                case 0x10:
                    dSht0 = 0xCB30;
                    break;
                case 0x11:
                    dSht0 = 0xCB50;
                    break;
                default:
                    throw new NotImplementedException();
            }
            if (textureFormat >= 0xB && textureFormat <= 0x11 && alphaSetting > 0)
            {
                dSht0 |= 0x10;
            }

            return dSht0;
        }

        private ushort GetFormatInfo1()
        {
            ushort dSht1;
            switch (textureFormat) 
            {
                case 0x3:
                case 0xE:
                    dSht1 = 0x204;
                    break;
                case 0xF:
                    dSht1 = 0x22C;
                    break;
                case 0x10:
                    dSht1 = 0x3AC;
                    break;
                default:
                    dSht1 = 0xFAC;
                    break;
            }

            return dSht1;
        }

        private void Read(BufferedStreamReaderBE<MemoryStream> sr, uint magicCheck, bool readTexBuffers = true)
        {
            switch(magicCheck)
            {
                //Double check if this is a PS3 CTXR
                case 0x01010002:
                case 0xFF000002:
                    sr.ReadBE<int>();
                    textureFormat = 0;
                    if (sr.ReadBE<int>(true) == sr.BaseStream.Length - 0x80)
                    {
                        ReadTopHeaderPS3(sr, readTexBuffers);
                    }
                    return;
            }

            sr.Seek(sr.BaseStream.Length - 0xC, SeekOrigin.Begin);
            footerData = sr.Read<CFooter>();

            sr.Seek(0, SeekOrigin.Begin);
            textureFormat = sr.Read<int>();
            var pixelFormat = GetFormat();
            long headerLength = 0;
            long totalBufferLength;
            long sliceBufferLength;
            int texWidth;
            int texHeight;
            int finalWidth;
            int finalHeight;
            int sourceBytesPerPixelSet, pixelBlockSize, formatBpp;
            int ctxrInternalTexBufferSize;

            switch (footerData.version)
            {
                case 0x25: //SOTC
                    alphaSetting = sr.ReadBE<short>();
                    textureType = (CTextureType)sr.ReadBE<ushort>();
                    var sUnkSht1 = sr.ReadBE<short>();
                    var sUnkSht2 = sr.ReadBE<short>();
                    var sUnkSht3 = sr.ReadBE<short>();
                    var fileCount = sr.ReadBE<short>(); //Always 1 + external mipCount. 1 is probably this file
                    var sUnkSht4 = sr.ReadBE<short>();
                    externalMipCount = sr.ReadBE<short>();
                    internalMipCount = sr.ReadBE<short>();
                    ctxrInternalTexBufferSize = sr.ReadBE<int>();
                    externalTexturesSize = sr.ReadBE<int>();
                    var sUnkByte = sr.ReadBE<byte>();

                    switch(textureType)
                    {
                        case CTextureType.CubeMap:
                            sliceCount = 6;
                            internalMipCount /= 6;
                            break;
                        default:
                            sliceCount = 1;
                            break;
                    }

                    for (int i = 0; i < externalMipCount; i++)
                    {
                        mipPaths.Add(new CTXRExternalReference(sr));
                    }

                    var sUnkInt3 = sr.ReadBE<int>();
                    var sUnkSht5 = sr.ReadBE<short>();

                    //Texture info structure
                    var sht0 = sr.ReadBE<short>();
                    WidthBaseByte = sr.ReadBE<byte>();
                    WidthMultiplierByte = sr.ReadBE<byte>();
                    HeightBaseByte = sr.ReadBE<byte>();
                    HeightMultiplierByte = sr.ReadBE<byte>();
                    var sht1 = sr.ReadBE<short>();
                    var sht2 = sr.ReadBE<short>();

                    var sUnkSize = sr.ReadBE<int>();
                    var sUnkInt0 = sr.ReadBE<int>();
                    var sUnkInt1 = sr.ReadBE<int>();
                    var sUnkInt2 = sr.ReadBE<int>();
                    headerLength = sr.Position;

                    totalBufferLength = sr.BaseStream.Length - headerLength - 0xC; //Subtract file header and footer from file total length
                    sliceBufferLength = totalBufferLength / sliceCount;

                    //Get top internal mip resolution
                    texWidth = GetSOTCWidthComponent(WidthBaseByte, WidthMultiplierByte);
                    texHeight = GetSOTCHeightComponent(HeightBaseByte, HeightMultiplierByte);
                    GetLargestInternalMipResolution(texWidth, texHeight,
                        externalMipCount, out finalWidth, out finalHeight);
                    DeSwizzler.GetsourceBytesPerPixelSetAndPixelSize(pixelFormat, out sourceBytesPerPixelSet, out pixelBlockSize, out formatBpp);

                    if (readTexBuffers)
                    {
                        ReadSOTCTexBuffers(sr, pixelFormat, headerLength, sliceBufferLength, finalWidth, finalHeight, sourceBytesPerPixelSet, formatBpp);
                    }
                    break;
                case 0x6E: //DeSR
                    texFlags = sr.ReadBE<short>();
                    alphaSetting = sr.ReadBE<short>();
                    textureType = sr.ReadBE<CTextureType>();
                    var unkInt1 = sr.ReadBE<int>();

                    var filesCount = sr.ReadBE<int>(); //Always 1 + external mipCount. 1 is probably this file
                    externalMipCount = sr.ReadBE<short>();
                    internalMipCount = sr.ReadBE<short>();
                    ctxrInternalTexBufferSize = sr.ReadBE<int>();
                    var unkInt2 = sr.ReadBE<int>();
                    var unkByte = sr.ReadBE<byte>();

                    for (int i = 0; i < externalMipCount; i++)
                    {
                        mipPaths.Add(new CTXRExternalReference(sr));
                    }
                    var unkInt3 = sr.ReadBE<int>();
                    var unkSht3 = sr.ReadBE<short>();

                    //Texture info structure
                    var formatInfo0 = sr.ReadBE<short>();
                    WidthBaseByte = sr.ReadBE<byte>();
                    WidthMultiplierByte = sr.ReadBE<byte>();
                    HeightBaseByte = sr.ReadBE<byte>();
                    HeightMultiplierByte = sr.ReadBE<byte>();
                    var formatInfo1 = sr.ReadBE<short>();
                    var typeMipFlags = sr.ReadBE<short>();

                    sliceCount = sr.ReadBE<short>() + 1; //Usually has a value except for the very large textures
                    var sht4 = sr.ReadBE<short>();
                    var mipMapTableSize = sr.ReadBE<int>();
                    var int1 = sr.ReadBE<int>();
                    var int2 = sr.ReadBE<int>();
                    headerLength = sr.Position;

                    totalBufferLength = sr.BaseStream.Length - headerLength - 0xC; //Subtract file header and footer from file total length
                    sliceBufferLength = totalBufferLength / sliceCount;

                    //Get top internal mip resolution
                    texWidth = GetDesResolutionComponent(WidthBaseByte, WidthMultiplierByte, 0xC0);
                    texHeight = GetDesResolutionComponent(HeightBaseByte, HeightMultiplierByte, 0x80);
                    GetLargestInternalMipResolution(texWidth, texHeight,
                        externalMipCount, out finalWidth, out finalHeight);
                    DeSwizzler.GetsourceBytesPerPixelSetAndPixelSize(pixelFormat, out sourceBytesPerPixelSet, out pixelBlockSize, out formatBpp);

                    if (readTexBuffers)
                    {
                        ReadDeSRTexBuffers(sr, pixelFormat, headerLength, sliceBufferLength, finalWidth, finalHeight, sourceBytesPerPixelSet, formatBpp);
                    }
                    break;
                default:
                    throw new Exception("Unexpected CTXR type!");
            }
        }

        /// <summary>
        /// For reading the variation of the format found in PS3 Shadow of the Colossus + Ico and Metal Gear Solid HD Collection
        /// </summary>
        private void ReadTopHeaderPS3(BufferedStreamReaderBE<MemoryStream> sr, bool readTexBuffers = true)
        {
            sr._BEReadActive = true;
            footerData = new CFooter();
            footerData.version = (byte)sr.ReadBE<int>();
            var int_0C = sr.ReadBE<int>();

            int headerSize = sr.ReadBE<int>();
            int bufferDataSize = sr.ReadBE<int>(); //Size of the buffer with used data
            int int_18 = sr.ReadBE<int>();
            int int_1C = sr.ReadBE<int>();

            int int_20 = sr.ReadBE<int>();
            byte bt_24 = sr.ReadBE<byte>();
            internalMipCount = sr.ReadBE<byte>();
            byte bt_26 = sr.ReadBE<byte>();
            byte bt_27 = sr.ReadBE<byte>();
            int int_28 = sr.ReadBE<int>();
            explicitWidth = sr.ReadBE<ushort>();
            explicitHeight = sr.ReadBE<ushort>();
            sliceCount = sr.ReadBE<ushort>();

            var pixelFormat = GetFormat();
            DeSwizzler.GetsourceBytesPerPixelSetAndPixelSize(pixelFormat, out int sourceBytesPerPixelSet, out int pixelBlockSize, out int formatBpp);
            sr.Seek(0x80, SeekOrigin.Begin);
            int finalWidth = explicitWidth;
            int finalHeight = explicitHeight;
            for (int s = 0; s < sliceCount; s++)
            {
                int mipWidth = finalWidth;
                int mipHeight = finalHeight;

                mipMapsList.Add(new List<byte[]>());

                long bufferUsed = 0;
                for (int i = 0; i < internalMipCount; i++)
                {
                    mipWidth = Math.Max(mipWidth, pixelBlockSize);
                    mipHeight = Math.Max(mipHeight, pixelBlockSize);
                    long bufferLength = (formatBpp * mipWidth * mipHeight) / 8;
                    var mipOffset = bufferUsed + 0x80;
                    bufferUsed += bufferLength;
                    var mipFull = sr.ReadBytes(mipOffset, (int)bufferLength);
                    ARGBToRGBA(mipFull);
                    //If it's too small, we don't need to deswizzle
                    if ((formatBpp * mipWidth * mipHeight / 8) <= sourceBytesPerPixelSet)
                    {
                        var newMipFull = new byte[sourceBytesPerPixelSet];
                        Array.Copy(mipFull, 0, newMipFull, 0, sourceBytesPerPixelSet);
                        mipFull = newMipFull;
                    }
                    else
                    {
                        mipFull = DeSwizzler.PS3DeSwizzle(mipFull, mipWidth, mipHeight, pixelFormat);
                    }

                    mipMapsList[s].Add(mipFull);
                    mipWidth /= 2;
                    mipHeight /= 2;
                    if (mipWidth == 0)
                    {
                        mipWidth = 1;
                    }
                    if (mipHeight == 0)
                    {
                        mipHeight = 1;
                    }
                }
            }
        }

        public static void ARGBToRGBA(byte[] raw)
        {
            for (int i = 0; i < raw.Length; i += 4)
            {
                byte low = raw[i];
                byte low1 = raw[i + 1];
                byte high = raw[i + 1];
                byte high2 = raw[i + 2];
                raw[i] = low1;
                raw[i + 1] = high;
                raw[i + 2] = high2;
                raw[i + 3] = low;
            }
        }

        private void ReadSOTCTexBuffers(BufferedStreamReaderBE<MemoryStream> sr, DXGIFormat pixelFormat, long headerLength, long sliceBufferLength, int finalWidth, int finalHeight, int sourceBytesPerPixelSet, int formatBpp)
        {
            long bufferUsed = 0;
            int mipWidth = finalWidth;
            int mipHeight = finalHeight;

            //Swizzling can go outside the bounds of the texture so we want to check the full buffer in these cases. Hopefully it's only for single mip instances
            long bufferLength = internalMipCount == 1 ? sliceBufferLength : formatBpp * finalWidth * finalHeight / 8;

            //Prepare mip set lists
            for(int i = 0; i < sliceCount; i++)
            {
                mipMapsList.Add(new List<byte[]>());
            }

            int sliceBufferMin;
            if (sliceCount > 1)
            {
                sliceBufferMin = 0x400;
            }
            else
            {
                sliceBufferMin = 0x200;
            }

            //SOTC textures seem to lay out slices at the same level sequentially rather than having slices go through each mip in their set before proceeding to the next slice
            for (int i = 0; i < internalMipCount; i++)
            {
                if (internalMipCount > 1 || sliceCount > 1)
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

                for (int s = 0; s < sliceCount; s++)
                {
                    var mipOffset = bufferUsed + headerLength;
                    var mipFull = sr.ReadBytes(mipOffset, (int)bufferLength);
                    bufferUsed += bufferLength;

                    //Make sure that we have enough bytes to actually deswizzle
                    var deSwizzChunkSize = GetDeSwizzleSize(mipFull.Length, pixelFormat, mipWidth, mipHeight, out int deSwizzWidth, out int deSwizzHeight);
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
                        mipFull = DeSwizzler.PS4DeSwizzle(mipFull, swizzleBlockWidth, swizzleBlockHeight, pixelFormat);

                        //Extract as a tile from the pixels if we haven't done that at the deswizzle step
                        if (swizzleBlockWidth != mipWidth || swizzleBlockHeight != mipHeight)
                        {
                            mipFull = DeSwizzler.ExtractTile(mipFull, pixelFormat, swizzleBlockWidth, 0, 0, mipWidth, mipHeight);
                        }
                    }

                    mipMapsList[s].Add(mipFull);
                }
                mipWidth /= 2;
                mipHeight /= 2;

                //Cubemaps seem to pad to the size of 8 textures
                if(sliceCount > 1)
                {
                    //If volume textures exist in SOTC, those need to be figured out
                    if(sliceCount > 8)
                    {
                        throw new Exception();
                    }

                    bufferUsed += 8 * bufferLength - sliceCount * bufferLength;
                }
            }
        }

        /// <summary>
        /// Takes a parsed CTXR's deswizzled output buffers and processes them back into what the game expects, ready to slap into the CTXR output.
        /// </summary>
        private List<List<byte[]>> PackDeSRTextureBuffers(out int gapBufferLength)
        {
            List<List<byte[]>> outBuffers = new List<List<byte[]>>();
            var pixelFormat = GetFormat();
            var texWidth = GetDesResolutionComponent(WidthBaseByte, WidthMultiplierByte, 0xC0);
            var texHeight = GetDesResolutionComponent(HeightBaseByte, HeightMultiplierByte, 0x80);
            GetLargestInternalMipResolution(texWidth, texHeight,
                externalMipCount, out var finalWidth, out var finalHeight);
            DeSwizzler.GetsourceBytesPerPixelSetAndPixelSize(pixelFormat, out var sourceBytesPerPixelSet, out var pixelBlockSize, out var formatBpp);

            int bufferUsed = 0;
            int internalMipCount = mipMapsList[0].Count;
            for (int s = 0; s < mipMapsList.Count; s++)
            {
                int mipWidth = finalWidth;
                int mipHeight = finalHeight;
                long bufferLength = mipMapsList[s][0].Length;
                outBuffers.Add(new List<byte[]>());

                for (int i = 0; i < internalMipCount; i++)
                {
                    if (internalMipCount > 1 && i != 0)
                    {
                        if (bufferLength != 0x100)
                        {
                            bufferLength = bufferLength / 2;

                            if (bufferLength == 0x400 || (bufferLength >= 0x10000))
                            {
                                bufferLength = bufferLength / 2;
                            }
                        }
                    }

                    //Calc dimension data for swizzling
                    var deSwizzChunkSize = GetDeSwizzleSize(bufferLength, pixelFormat, mipWidth, mipHeight, out int deSwizzWidth, out int deSwizzHeight);
                    int swizzleBlockWidth = deSwizzWidth < 8 ? 8 : deSwizzWidth;
                    int swizzleBlockHeight = deSwizzHeight < 8 ? 8 : deSwizzHeight;
                    byte[] mipCurrent = mipMapsList[s][i];
                    byte[] mipFull;

                    //If it's too small, we don't need to swizzle, but we do need to put it in a larger buffer
                    if ((formatBpp * mipWidth * mipHeight / 8) <= sourceBytesPerPixelSet)
                    {
                        var newMipFull = new byte[0x100];
                        Array.Copy(mipCurrent, 0, newMipFull, 0, sourceBytesPerPixelSet);
                        mipFull = newMipFull;
                    }
                    else
                    {
                        mipFull = new byte[bufferLength];
                        mipCurrent = DrSwizzler.Swizzler.PS5Swizzle(mipCurrent, swizzleBlockWidth, swizzleBlockHeight, (DrSwizzler.DDS.DXEnums.DXGIFormat)pixelFormat);
                        Array.Copy(mipCurrent, mipFull, mipCurrent.Length);
                    }

                    bufferUsed += mipFull.Length;
                    outBuffers[s].Add(mipFull);
                    mipWidth /= 2;
                    mipHeight /= 2;
                }
            }

            switch(textureType)
            {
                case CTextureType.LargeUI:
                    gapBufferLength = 0;
                    break;
                default:
                    gapBufferLength = (outBuffers[0][0].Length * 2) - bufferUsed;
                    break;
            }

            return outBuffers;
        }

        /// <summary> 
        /// The texture buffers for internal mipmaps seemingly subdivide by 2 each time we go down a mip, UNTIL we reach 0x400. When the buffer should be 0x400, we instead skip to 0x200.
        /// All mipmap buffers after this will be 0x100 regardless of true size.
        /// While the buffers are larger than the actual texture size, the swizzling happens at the BUFFER level and thus reading the full buffer for deswizzling is paramount
        /// </summary>
        private void ReadDeSRTexBuffers(BufferedStreamReaderBE<MemoryStream> sr, DXGIFormat pixelFormat, long headerLength, long sliceBufferLength, int finalWidth, int finalHeight, int sourceBytesPerPixelSet, int formatBpp)
        {
            for (int s = 0; s < sliceCount; s++)
            {
                int mipWidth = finalWidth;
                int mipHeight = finalHeight;
                long bufferLength = sliceBufferLength;
                if (internalMipCount > 1)
                {
                    bufferLength /= 2;
                }

                //In some cases, we want the full buffer size because of overrun and the need for it in deswizzling,
                //but sometimes we want the calculated version since larger buffers don't have padding,
                //which means the smaller mips combined won't equal half the slice's buffer length
                long calculatedBufferLength = formatBpp * finalWidth * finalHeight / 8;
                if (calculatedBufferLength > bufferLength)
                {
                    bufferLength = calculatedBufferLength;
                }

                mipMapsList.Add(new List<byte[]>());

                long bufferUsed = 0;
                for (int i = 0; i < internalMipCount; i++)
                {
                    if (internalMipCount > 1 && i != 0)
                    {
                        if (bufferLength != 0x100)
                        {
                            bufferLength = bufferLength / 2;

                            if (bufferLength == 0x400 || (bufferLength >= 0x10000))
                            {
                                bufferLength = bufferLength / 2;
                            }
                        }
                    }
                    bufferUsed += bufferLength;
                    var mipOffset = ((sliceBufferLength * sliceCount) - (sliceBufferLength * s)) - bufferUsed + headerLength;
                    var mipFull = sr.ReadBytes(mipOffset, (int)bufferLength);

                    //Make sure that we have enough bytes to actually deswizzle
                    var deSwizzChunkSize = GetDeSwizzleSize(mipFull.Length, pixelFormat, mipWidth, mipHeight, out int deSwizzWidth, out int deSwizzHeight);
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
                        mipFull = DeSwizzler.PS5DeSwizzle(mipFull, swizzleBlockWidth, swizzleBlockHeight, pixelFormat);

                        //Extract as a tile from the pixels if we haven't done that at the deswizzle step
                        if (swizzleBlockWidth != mipWidth || swizzleBlockHeight != mipHeight)
                        {
                            mipFull = DeSwizzler.ExtractTile(mipFull, pixelFormat, swizzleBlockWidth, 0, 0, mipWidth, mipHeight);
                        }
                    }

                    mipMapsList[s].Add(mipFull);
                    mipWidth /= 2;
                    mipHeight /= 2;
                }
            }
        }

        public static void GetLargestInternalMipResolution(int width, int height, int externalMipCount, out int finalWidth, out int finalHeight)
        {
            finalWidth = width;
            finalHeight = height;
            for (int i = 0; i < externalMipCount; i++)
            {
                finalWidth /= 2;
                finalHeight /= 2;
            }
        }

        /// <summary>
        /// Outside of special cases, the first byte + 1, NOT multiplied by 4, is the base resolution which gets multiplied by the 2nd nybble in the 2nd byte.
        /// </summary>
        public static int GetSOTCWidthComponent(byte SOTCBaseByte, byte SOTCResByte)
        {
            var resByte = SOTCResByte - 0xC0;

            switch (SOTCBaseByte)
            {
                case 0x3F:
                    switch (resByte)
                    {
                        case 6:
                            return 1600;
                    }
                    break;
                case 0x57:
                    switch(resByte)
                    {
                        case 6:
                            return 1624;
                        case 2:
                            return 600;
                    }
                    break;
                case 0x7F:
                    switch(resByte)
                    {
                        case 1:
                            return 380;
                        case 2:
                            return 636;
                        case 7:
                            return 1920;
                        case 0xC:
                            return 3200;
                    }
                    break;
                case 0xA3:
                    switch (resByte)
                    {
                        case 1:
                            return 420;
                    }
                    break;
                case 0xBF:
                    switch (resByte)
                    {
                        case 0:
                            return 190;
                    }
                    break;
                case 0xC3:
                    switch (resByte)
                    {
                        case 6:
                            return 1730;
                    }
                    break;

            }
            return (SOTCBaseByte + 1) * (resByte + 1);
        }

        /// <summary>
        /// Outside of special cases, the first byte + 1, then multiplied by 4, is the base resolution which gets multiplied by the 2nd nybble in the 2nd byte.
        /// </summary>
        public static int GetSOTCHeightComponent(byte SOTCBaseByte, byte SOTCResByte)
        {
            var resByte = SOTCResByte - 0x70;

            switch (SOTCBaseByte)
            {
                case 0x5:
                    switch (resByte)
                    {
                        case 1:
                            return 1048;
                    }
                    break;
                case 0xD:
                    switch (resByte)
                    {
                        case 1:
                            return 1080;
                    }
                    break;
                case 0x1B:
                    if (resByte == 2)
                    {
                        return 2160;
                    }
                    break;
                case 0x37:
                    switch (resByte)
                    {
                        case 1:
                            return 1248;
                    }
                    break;
                case 0x9B:
                    switch (resByte)
                    {
                        case 0:
                            return 624;
                    }
                    break;
            }
            return ((SOTCBaseByte + 1) * 4) * (resByte + 1);
        }

        /// <summary>
        /// Outside of special cases, the first byte + 1, then multiplied by 4, is the base resolution which gets multiplied by the 2nd nybble in the 2nd byte.
        /// 0xC0 should be the width mask while 0x80 should be the height mask
        /// </summary>
        public static int GetDesResolutionComponent(byte DesBaseByte, byte DesResByte, byte mask)
        {
            var resByte = DesResByte - mask;

            switch (DesBaseByte)
            {
                case 0x1B:
                    if (resByte == 2)
                    {
                        return 2160;
                    }
                    break;
                case 0xBF:
                    if (resByte == 3)
                    {
                        return 3840;
                    }
                    break;
            }
            return ((DesBaseByte + 1) * 4) * (resByte + 1);
        }

        public void SetDesWidthComponent(int res)
        {
            SetDesWidthComponent(res, out byte desBaseByte, out byte desResByte);
            WidthBaseByte = desBaseByte;
            WidthMultiplierByte = desResByte;
        }

        public void SetDesHeightComponent(int res)
        {
            SetDesHeightComponent(res, out byte desBaseByte, out byte desResByte);
            HeightBaseByte = desBaseByte;
            HeightMultiplierByte = desResByte;
        }

        public static void SetDesWidthComponent(int res, out byte DesBaseByte, out byte DesResByte)
        {
            SetDesResolutionComponent(res, 0xC0, out DesBaseByte, out DesResByte);
        }

        public static void SetDesHeightComponent(int res, out byte DesBaseByte, out byte DesResByte)
        {
            SetDesResolutionComponent(res, 0x80, out DesBaseByte, out DesResByte);
        }

        private static void SetDesResolutionComponent(int res, byte mask, out byte DesBaseByte, out byte DesResByte)
        {
            if (res % 4 != 0)
            {
                throw new Exception($"Resolution components must be a multiple of 4. Received: {res}");
            }
            switch (res)
            {
                case 2160:
                    DesBaseByte = 0x1B;
                    DesResByte = (byte)(mask + 2);
                    break;
                case 3840:
                    DesBaseByte = 0xBF;
                    DesResByte = (byte)(mask + 3);
                    break;
            }

            int quartered = res / 4;
            int baseValue = Math.Min(quartered, 256);
            while (quartered % baseValue != 0)
            {
                baseValue--;
            }

            DesBaseByte = (byte)(baseValue - 1);
            DesResByte = (byte)(mask + (quartered / baseValue) - 1);
        }

        /// <summary>
        /// Assumes external references are first mips.
        /// </summary>
        public CTXRExternalReference[] GetSortedExternalRefList()
        {
            CTXRExternalReference[] refList = new CTXRExternalReference[externalMipCount];
            foreach (var reference in mipPaths)
            {
                refList[reference.mipLevel] = reference;
            }

            return refList;
        }

        public void WriteToDDS(string ctxrPath, string outPath)
        {
            var rootPath = PSUtility.GetPSRootPath(ctxrPath);

            //If this is a png, just write it out directly
            if (textureFormat == -1)
            {
                File.WriteAllBytes(outPath.Replace(".dds", ".png"), mipMapsList[0][0]);
                return;
            }

            //Assume external mips come first
            var refList = GetSortedExternalRefList();
            var pixelFormat = GetFormat();
            int texWidth;
            int texHeight;

            switch (footerData.version)
            {
                case 0x1:
                    texWidth = explicitWidth;
                    texHeight = explicitHeight;
                    break;
                case 0x25: //SOTC
                    texWidth = GetSOTCWidthComponent(WidthBaseByte, WidthMultiplierByte);
                    texHeight = GetSOTCHeightComponent(HeightBaseByte, HeightMultiplierByte);
                    break;
                case 0x6E: //DeSR
                    texWidth = GetDesResolutionComponent(WidthBaseByte, WidthMultiplierByte, 0xC0);
                    texHeight = GetDesResolutionComponent(HeightBaseByte, HeightMultiplierByte, 0x80);
                    break;
                default:
                    throw new Exception("Unexpected CTXR type!");
            }

            var chunkWidth = texWidth;
            var chunkHeight = texHeight;

            List<byte> externalMipsData = new List<byte>();
            foreach (var reference in refList)
            {
                var chunkPath = Path.Combine(rootPath, reference.externalMipReference.Substring(2).Replace("/", "\\"));
                var ctxrPathCmn = chunkPath.Replace("****", "_cmn");
                var ctxrPathPs5 = chunkPath.Replace("****", "_ps5");
                var ctxrPathPs4 = chunkPath.Replace("****", "_ps4");
                if (File.Exists(ctxrPathCmn))
                {
                    chunkPath = ctxrPathCmn;
                    ReadAndDeSwizzleCTexChunk(pixelFormat, chunkWidth, chunkHeight, externalMipsData, chunkPath);
                }
                else if (File.Exists(ctxrPathPs5))
                {
                    chunkPath = ctxrPathPs5;
                    ReadAndDeSwizzleCTexChunk(pixelFormat, chunkWidth, chunkHeight, externalMipsData, chunkPath);
                }
                else if (File.Exists(ctxrPathPs4))
                {
                    chunkPath = ctxrPathPs4;
                    ReadAndDeSwizzleCTexChunk(pixelFormat, chunkWidth, chunkHeight, externalMipsData, chunkPath);
                }

                chunkWidth /= 2;
                chunkHeight /= 2;
            }

            //Handle separately based on if this is a cubemap/volume texture vs a standard texture
            switch (textureType)
            {
                case CTextureType.Standard:
                case CTextureType.LargeUI:
                    bool first = true;
                    for (int i = mipMapsList.Count - 1; i >= 0; i--)
                    {
                        int mipCount = mipMapsList[i].Count;

                        //Should only be possible to have external mips with one set of texture data. In theory, there won't be other texture slices in a texture with externals, but let's be safe
                        if (first)
                        {
                            mipCount += refList.Length;
                        }

                        List<byte> outbytes = GenerateDDSHeader(pixelFormat, texWidth, texHeight, mipCount);

                        if (first)
                        {
                            first = false;
                            outbytes.AddRange(externalMipsData);
                        }
                        foreach (var mip in mipMapsList[i])
                        {
                            outbytes.AddRange(mip);
                        }

                        string texPath = outPath;
                        if (mipMapsList.Count > 1)
                        {
                            texPath = texPath.Replace(".dds", $"_{mipMapsList.Count - 1 - i}.dds");
                        }

                        //Add for safety since some PS5 writes aren't perfect
                        if(footerData.version == 0x6E)
                        {
                            outbytes.AddRange(new byte[0x100]);
                        }
                        File.WriteAllBytes(texPath, outbytes.ToArray());
                    }
                    break;
                case CTextureType.CubeMap:
                    List<byte> cubeOut = GenerateDDSHeader(pixelFormat, texWidth, texHeight, internalMipCount);
                    for (int i = 0; i < mipMapsList.Count; i++)
                    {
                        foreach (var mip in mipMapsList[i])
                        {
                            cubeOut.AddRange(mip);
                        }
                    }

                    //Add for safety since some PS5 writes aren't perfect
                    if (footerData.version == 0x6E)
                    {
                        cubeOut.AddRange(new byte[0x100]);
                    }
                    File.WriteAllBytes(outPath, cubeOut.ToArray());
                    break;
                case CTextureType.Volume:
                    //Volume maps in Demon's Souls do not use mipmaps, but the pattern is to divide the count of mipmaps alongside their resolution. See: https://learn.microsoft.com/en-us/windows/win32/direct3ddds/dds-file-layout-for-volume-textures
                    List<byte> volumeOut = GenerateDDSHeader(pixelFormat, texWidth, texHeight, internalMipCount, sliceCount);
                    for (int i = 0; i < mipMapsList.Count; i++)
                    {
                        volumeOut.AddRange(mipMapsList[i][0]);
                    }


                    //Add for safety since some PS5 writes aren't perfect
                    if (footerData.version == 0x6E)
                    {
                        volumeOut.AddRange(new byte[0x100]);
                    }
                    File.WriteAllBytes(outPath, volumeOut.ToArray());
                    break;
            }


        }

        private byte[] WriteAndSwizzleCTexChunk(DXGIFormat pixelFormat, int chunkWidth, int chunkHeight, byte[] externalMipData, out int finalBufferSize)
        {
            var swizzChunkSize = GetDeSwizzleSize(externalMipData.Length, pixelFormat, chunkWidth, chunkHeight, out int swizzWidth, out int swizzHeight);
            List<byte> swizzledDataBuffer = new();
            switch (footerData.version)
            {
                case 0x25:
                    swizzledDataBuffer.AddRange(DrSwizzler.Swizzler.PS4Swizzle(externalMipData, swizzWidth, swizzHeight, (DrSwizzler.DDS.DXEnums.DXGIFormat)pixelFormat));
                    throw new NotImplementedException();
                case 0x6E:
                    swizzledDataBuffer.AddRange(DrSwizzler.Swizzler.PS5Swizzle(externalMipData, swizzWidth, swizzHeight, (DrSwizzler.DDS.DXEnums.DXGIFormat)pixelFormat));
                    int fileSize = finalBufferSize = swizzledDataBuffer.Count;
                    swizzledDataBuffer.AddRange([0x43, 0x54, 0x58, 0x54,  0x64, 0x0, 0x0, 0x1]);
                    swizzledDataBuffer.AddValue(fileSize);
                    break;
                default:
                    throw new Exception("Unexpected CTXR type!");
            }

            return swizzledDataBuffer.ToArray();
        }

        private void ReadAndDeSwizzleCTexChunk(DXGIFormat pixelFormat, int chunkWidth, int chunkHeight, List<byte> externalMipsData, string chunkPath)
        {
            var chunk = File.ReadAllBytes(chunkPath);
            chunk = CompressionHandler.CheckCompression(chunk);

            var deSwizzChunkSize = GetDeSwizzleSize(chunk.Length - 0xC, pixelFormat, chunkWidth, chunkHeight, out int deSwizzWidth, out int deSwizzHeight);
            byte[] data = null;
            switch (footerData.version)
            {
                case 0x25: //SOTC
                    data = DeSwizzler.PS4DeSwizzle(chunk, deSwizzWidth, deSwizzHeight, pixelFormat);
                    break;
                case 0x6E: //DeSR
                    data = DeSwizzler.PS5DeSwizzle(chunk, deSwizzWidth, deSwizzHeight, pixelFormat);
                    break;
                default:
                    throw new Exception("Unexpected CTXR type!");
            }
            data = DeSwizzler.ExtractTile(data, pixelFormat, deSwizzWidth, 0, 0, chunkWidth, chunkHeight);
            externalMipsData.AddRange(data);
        }

        public long GetDeSwizzleSize(long dataLength, DXGIFormat pixelFormat, int width, int height, out int deSwizzWidth, out int deSwizzHeight)
        {
            DeSwizzler.GetsourceBytesPerPixelSetAndPixelSize(pixelFormat, out var sourceBytePerPixelSet, out var pixelBlockSize, out var formatBpp);
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

        private List<byte> GenerateDDSHeader(DXGIFormat pixelFormat, int texWidth, int texHeight, int mipCount, int depth = 1)
        {
            var meta = GenerateMetaData(texWidth, texHeight, mipCount, pixelFormat, textureType == CTextureType.CubeMap, depth);
            if (alphaSetting > 0)
            {
                meta.MiscFlags2 = TexMiscFlags2.TEXMISC2ALPHAMODEMASK;
            }
            AquaModelLibrary.Helpers.DDS.DirectXTexUtility.GenerateDDSHeader(meta, DDSFlags.NONE, out var ddsHeader, out var dx10Header, textureType == CTextureType.CubeMap);

            List<byte> outbytes = new List<byte>(DataHelpers.ConvertStruct(ddsHeader));
            if (isDx10())
            {
                outbytes.AddRange(DataHelpers.ConvertStruct(dx10Header));
            }
            outbytes.InsertRange(0, new byte[] { 0x44, 0x44, 0x53, 0x20 });
            return outbytes;
        }

        public DXGIFormat GetFormat()
        {
            return GetCTXR_DXGIFormat(textureFormat);
        }

        public static DXGIFormat GetCTXR_DXGIFormat(int textureFormat)
        {
            switch (textureFormat)
            {
                case 0x0:
                    return DXGIFormat.R8G8B8A8UNORM;
                case 0x1:
                    return DXGIFormat.R16G16B16A16UNORM; //Incorrect type. Used in Demon's Souls only for HDR comparison textures. Unsure what this should be.
                case 0x3:
                    return DXGIFormat.BC1UNORM; //Incorrect type. Used in Demon's Souls for shadow maps. Unsure what this should be.
                case 0xB:
                    return DXGIFormat.BC1UNORM;
                case 0xC:
                    return DXGIFormat.BC2UNORM;
                case 0xD:
                    return DXGIFormat.BC3UNORM;
                case 0xE:
                    return DXGIFormat.BC4UNORM;
                case 0xF:
                    return DXGIFormat.BC5UNORM;
                case 0x10:
                    return DXGIFormat.BC6HUF16; //Image Based Lighting maps may appear VERY dark just naturally. This is simply how they are and tools like RenderDoc can 'fix' the value grading
                case 0x11:
                    return DXGIFormat.BC7UNORM;
                case -0x101:
                    return DXGIFormat.R8G8B8A8SNORM;
                default:
                    throw new Exception($"Unexpected pixel format: {textureFormat:X}");
            }
        }

        public static int GetCTXRFormatFromDXGIFormat(DXGIFormat dxgi)
        {
            switch(dxgi)
            {
                case DXGIFormat.R8G8B8A8UNORM:
                    return 0x0;
                case DXGIFormat.R16G16B16A16UNORM:
                    return 0x1;
                case DXGIFormat.BC1UNORM:
                    return 0xB;
                case DXGIFormat.BC2UNORM:
                    return 0xC;
                case DXGIFormat.BC3UNORM:
                    return 0xD;
                case DXGIFormat.BC4UNORM:
                    return 0xE;
                case DXGIFormat.BC5UNORM:
                    return 0xF;
                case DXGIFormat.BC6HUF16:
                    return 0x10;
                case DXGIFormat.BC7UNORM:
                    return 0x11;
                case DXGIFormat.R8G8B8A8SNORM: //Probably not valid for modern models
                    return -0x101;
                default:
                    throw new Exception($"Unsupported pixel format: {dxgi.ToString():X}");
            }
        }

        public bool isDx10()
        {
            return isCTXRDx10(textureFormat);
        }

        public static bool isCTXRDx10(int textureFormat)
        {
            switch (textureFormat)
            {
                case -0x101:
                case 0x0:
                    return false;
                case 0x1:
                    return true;
                case 0x3:
                case 0xB:
                case 0xC:
                case 0xD:
                case 0xE:
                case 0xF:
                    return false;
                case 0x10:
                case 0x11:
                    return true;
                default:
                    throw new Exception($"Unexpected pixel format: {textureFormat:X}");
            }
        }
    }
}
