using AquaModelLibrary.Data.PSO2.Aqua.AquaMotionData;
using AquaModelLibrary.Data.Utility;
using AquaModelLibrary.Helpers;
using AquaModelLibrary.Helpers.Readers;
using static DirectXTex.DirectXTexUtility;

namespace AquaModelLibrary.Data.BluePoint.CTXR
{
    public class CTXR
    {
        public bool isPng = false;
        public int textureFormat = -1;
        public short unkShort0;
        public short alphaSetting;
        public CTextureType textureType;
        public int fileCount;
        public short externalMipCount;
        public short internalMipCount;
        public int sliceCount;

        /// <summary>
        /// Related somehow to the size of the texture, possibly pixel block sizes for swizzling
        /// </summary>
        public short pixelBlockSizeThing;

        public byte desWidthBaseByte;
        public byte desWidthByte;
        public byte desHeightBaseByte;
        public byte desHeightByte;

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
            var pngCheck = BitConverter.ToInt32(file, 0);
            if (isPng = pngCheck == 0x474E5089)
            {
                mipMapsList.Add(new List<byte[]> { file });
                return;
            }
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        public CTXR(byte[] file, bool readTexBuffers)
        {
            file = CompressionHandler.CheckCompression(file);

            //Apparently these can just be actual .png files? Well, gotta check for that.
            var pngCheck = BitConverter.ToInt32(file, 0);
            if (isPng = pngCheck == 0x474E5089)
            {
                mipMapsList.Add(new List<byte[]> { file });
                return;
            }
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr, readTexBuffers);
            }
        }

        private void Read(BufferedStreamReaderBE<MemoryStream> sr, bool readTexBuffers = true)
        {

            sr.Seek(sr.BaseStream.Length - 0xC, SeekOrigin.Begin);
            footerData = sr.Read<CFooter>();

            sr.Seek(0, SeekOrigin.Begin);
            textureFormat = sr.Read<int>();
            long headerLength = 0;
            switch (footerData.version)
            {
                case 0x25: //SOTC
                    for (int i = 0; i < externalMipCount; i++)
                    {
                        mipPaths.Add(new CTXRExternalReference(sr));
                    }
                    headerLength = sr.Position;
                    break;
                case 0x6E: //DeSR
                    unkShort0 = sr.Read<short>();
                    alphaSetting = sr.Read<short>();
                    textureType = sr.Read<CTextureType>();
                    var unkInt1 = sr.Read<int>();

                    fileCount = sr.Read<int>(); //Always 1 + external mipCount. 1 is probably this file
                    externalMipCount = sr.Read<short>();
                    internalMipCount = sr.Read<short>();
                    var unkSht1 = sr.Read<short>();
                    pixelBlockSizeThing = sr.Read<short>();
                    var unkInt2 = sr.Read<int>();
                    var unkByte = sr.Read<byte>();

                    for (int i = 0; i < externalMipCount; i++)
                    {
                        mipPaths.Add(new CTXRExternalReference(sr));
                    }
                    var unkInt3 = sr.ReadBE<int>();
                    var unkSht3 = sr.ReadBE<short>();

                    //Texture info structure
                    var sht0 = sr.ReadBE<short>();
                    desWidthBaseByte = sr.ReadBE<byte>();
                    desWidthByte = sr.ReadBE<byte>();
                    desHeightBaseByte = sr.ReadBE<byte>();
                    desHeightByte = sr.ReadBE<byte>();
                    var sht1 = sr.ReadBE<short>();
                    var sht2 = sr.ReadBE<short>();

                    sliceCount = sr.ReadBE<short>() + 1; //Usually has a value except for the very large textures
                    var sht4 = sr.ReadBE<short>();
                    var int0 = sr.ReadBE<int>();
                    var int1 = sr.ReadBE<int>();
                    var int2 = sr.ReadBE<int>();
                    headerLength = sr.Position;

                    var pixelFormat = GetFormat();

                    var totalBufferLength = sr.BaseStream.Length - headerLength - 0xC; //Subtract file header and footer from file total length
                    var sliceBufferLength = totalBufferLength / sliceCount;

                    //Get top internal mip resolution
                    var texWidth = GetDesResolutionComponent(desWidthBaseByte, desWidthByte);
                    var texHeight = GetDesResolutionComponent(desHeightBaseByte, desHeightByte);
                    GetLargestInternalMipResolution(texWidth, texHeight,
                        externalMipCount, out var finalWidth, out var finalHeight);
                    DeSwizzler.GetsourceBytesPerPixelSetAndPixelSize(pixelFormat, out var sourceBytesPerPixelSet, out var pixelBlockSize, out var formatBpp);

                    //The texture buffers for internal mipmaps seemingly subdivide by 2 each time we go down a mip, UNTIL we reach 0x400. When the buffer should be 0x400, we instead skip to 0x200.
                    //All mipmap buffers after this will be 0x100 regardless of true size.
                    //While the buffers are larger than the actual texture size, the swizzling happens at the BUFFER level and thus reading the full buffer for deswizzling is paramount
                    if (readTexBuffers)
                    {
                        for (int s = 0; s < sliceCount; s++)
                        {
                            int mipWidth = finalWidth;
                            int mipHeight = finalHeight;
                            long bufferLength = sliceBufferLength / 2;

                            //In some cases, we want the full buffer size because of overrun and the need for it in deswizzling,
                            //but sometimes we want the calculated version since larger buffers don't have padding,
                            //which means the smaller mips combined won't equal half the slice's buffer length
                            long calculatedBufferLength = formatBpp * finalWidth * finalHeight / 8;
                            if(calculatedBufferLength > bufferLength)
                            {
                                bufferLength = calculatedBufferLength;
                            }

                            mipMapsList.Add(new List<byte[]>());

                            long bufferUsed = 0;
                            for (int i = 0; i < internalMipCount; i++)
                            {
                                if (internalMipCount > 1)
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
                                var mipOffset = ((sliceBufferLength * sliceCount) - (sliceBufferLength * s)) - bufferUsed + headerLength;
                                var mipFull = sr.ReadBytes(mipOffset, (int)bufferLength);

                                //Make sure that we have enough bytes to actually deswizzle
                                var deSwizzChunkSize = GetDeSwizzleSize(mipFull.Length, pixelFormat, mipWidth, mipHeight, out int deSwizzWidth, out int deSwizzHeight);
                                int swizzleBlockWidth = deSwizzWidth < 8 ? 8 : deSwizzWidth;
                                int swizzleBlockHeight = deSwizzHeight < 8 ? 8 : deSwizzHeight;

                                if((formatBpp * mipWidth * mipHeight / 8) <= sourceBytesPerPixelSet)
                                {
                                    var newMipFull = new byte[sourceBytesPerPixelSet];
                                    Array.Copy(mipFull, 0, newMipFull, 0, sourceBytesPerPixelSet);
                                    mipFull = newMipFull;
                                } else
                                {
                                    //If it's too small, we don't need to deswizzle
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
                    break;
                default:
                    throw new Exception("Unexpected CTXR type!");
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

        public static int GetDesResolutionComponent(byte DesBaseByte, byte DesResByte)
        {
            var resByte = DesResByte % 0x10;

            switch (DesBaseByte)
            {
                case 0x1B:
                    if (resByte == 2)
                    {
                        return 2160;
                    }
                    else
                    {
                        return ((DesBaseByte + 1) * 4) * (resByte + 1);
                    }
                case 0xBF:
                    if (resByte == 3)
                    {
                        return 3840;
                    }
                    else
                    {
                        return ((DesBaseByte + 1) * 4) * (resByte + 1);
                    }
                default:
                    return ((DesBaseByte + 1) * 4) * (resByte + 1);
            }
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
            var texWidth = GetDesResolutionComponent(desWidthBaseByte, desWidthByte);
            var texHeight = GetDesResolutionComponent(desHeightBaseByte, desHeightByte);
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
                    ReadAndDeSwizzle(pixelFormat, chunkWidth, chunkHeight, externalMipsData, chunkPath);
                }
                else if (File.Exists(ctxrPathPs5))
                {
                    chunkPath = ctxrPathPs5;
                    ReadAndDeSwizzle(pixelFormat, chunkWidth, chunkHeight, externalMipsData, chunkPath);
                }
                else if (File.Exists(ctxrPathPs4))
                {
                    chunkPath = ctxrPathPs4;
                    ReadAndDeSwizzle(pixelFormat, chunkWidth, chunkHeight, externalMipsData, chunkPath);
                }

                chunkWidth /= 2;
                chunkHeight /= 2;
            }

            //Handle separately based on if this is a cubemap/volume texture vs a standard texture
            switch(textureType)
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

                        File.WriteAllBytes(texPath, outbytes.ToArray());
                    }
                    break;
                case CTextureType.CubeMap:
                    List<byte> cubeOut = GenerateDDSHeader(pixelFormat, texWidth, texHeight, internalMipCount);
                    for(int i = 0; i < mipMapsList.Count; i++)
                    {
                        foreach (var mip in mipMapsList[i])
                        {
                            cubeOut.AddRange(mip);
                        }
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
                    File.WriteAllBytes(outPath, volumeOut.ToArray());
                    break;
            }


        }

        private void ReadAndDeSwizzle(DXGIFormat pixelFormat, int chunkWidth, int chunkHeight, List<byte> externalMipsData, string chunkPath)
        {
            var chunk = File.ReadAllBytes(chunkPath);
            chunk = CompressionHandler.CheckCompression(chunk);

            var deSwizzChunkSize = GetDeSwizzleSize(chunk.Length - 0xC, pixelFormat, chunkWidth, chunkHeight, out int deSwizzWidth, out int deSwizzHeight);
            byte[] data = null;
            switch (footerData.version)
            {
                case 0x25: //SOTC
                           //mipsList.Add(Deswizzler.PS4DeSwizzle(chunk, ,, pixelFormat));
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
            DeSwizzler.GetsourceBytesPerPixelSetAndPixelSize(pixelFormat, out var a, out var b, out var formatBpp);
            if(((width * height * formatBpp) / 8) < dataLength)
            {
                if(width > height)
                {
                    deSwizzWidth = width;
                    deSwizzHeight = width;
                    return (deSwizzWidth * deSwizzHeight * formatBpp) / 8;
                } else
                {
                    deSwizzWidth = height;
                    deSwizzHeight = height;
                    return (deSwizzWidth * deSwizzHeight * formatBpp) / 8;
                }
            } else
            {
                deSwizzWidth = width;
                deSwizzHeight = height;
                return dataLength;
            }
        }

        private List<byte> GenerateDDSHeader(DXGIFormat pixelFormat, int texWidth, int texHeight, int mipCount, int depth = 1)
        {
            var meta = GenerateMataData(texWidth, texHeight, mipCount, pixelFormat, textureType == CTextureType.CubeMap, depth);
            if (alphaSetting > 0)
            {
                meta.MiscFlags2 = TexMiscFlags2.TEXMISC2ALPHAMODEMASK;
            }
            DirectXTex.DirectXTexUtility.GenerateDDSHeader(meta, DDSFlags.NONE, out var ddsHeader, out var dx10Header, textureType == CTextureType.CubeMap);

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
                default:
                    throw new Exception($"Unexpected pixel format: {textureFormat:X}");
            }
        }

        public bool isDx10()
        {
            switch (textureFormat)
            {
                case 0x0:
                case 0x1:
                    return true;
                case 0x3:
                case 0xB:
                case 0xC:
                case 0xD:
                    return false;
                case 0xE:
                case 0xF:
                case 0x10:
                case 0x11:
                    return true;
                default:
                    throw new Exception($"Unexpected pixel format: {textureFormat:X}");
            }
        }
    }
}
