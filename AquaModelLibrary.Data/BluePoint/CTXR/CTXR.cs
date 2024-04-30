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
        public int singleTextureSize;
        /// <summary>
        /// Combined size of all external texture buffers. Not in Demon's Souls
        /// </summary>
        public int externalTexturesSize;
        public byte WidthBaseByte;
        public byte WidthMultiplierByte;
        public byte HeightBaseByte;
        public byte HeightMultiplierByte;

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
            var pixelFormat = GetFormat();
            long headerLength = 0;
            long totalBufferLength;
            long sliceBufferLength;
            int texWidth;
            int texHeight;
            int finalWidth;
            int finalHeight;
            int sourceBytesPerPixelSet, pixelBlockSize, formatBpp;

            switch (footerData.version)
            {
                case 0x25: //SOTC
                    alphaSetting = sr.ReadBE<short>();
                    textureType = (CTextureType)sr.ReadBE<ushort>();
                    var sUnkSht1 = sr.ReadBE<short>();
                    var sUnkSht2 = sr.ReadBE<short>();
                    var sUnkSht3 = sr.ReadBE<short>();
                    fileCount = sr.ReadBE<short>(); //Always 1 + external mipCount. 1 is probably this file
                    var sUnkSht4 = sr.ReadBE<short>();
                    externalMipCount = sr.ReadBE<short>();
                    internalMipCount = sr.ReadBE<short>();
                    singleTextureSize = sr.ReadBE<int>();
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
                    unkShort0 = sr.ReadBE<short>();
                    alphaSetting = sr.ReadBE<short>();
                    textureType = sr.ReadBE<CTextureType>();
                    var unkInt1 = sr.ReadBE<int>();

                    fileCount = sr.ReadBE<int>(); //Always 1 + external mipCount. 1 is probably this file
                    externalMipCount = sr.ReadBE<short>();
                    internalMipCount = sr.ReadBE<short>();
                    singleTextureSize = sr.ReadBE<int>();
                    var unkInt2 = sr.ReadBE<int>();
                    var unkByte = sr.ReadBE<byte>();

                    for (int i = 0; i < externalMipCount; i++)
                    {
                        mipPaths.Add(new CTXRExternalReference(sr));
                    }
                    var unkInt3 = sr.ReadBE<int>();
                    var unkSht3 = sr.ReadBE<short>();

                    //Texture info structure
                    var dSht0 = sr.ReadBE<short>();
                    WidthBaseByte = sr.ReadBE<byte>();
                    WidthMultiplierByte = sr.ReadBE<byte>();
                    HeightBaseByte = sr.ReadBE<byte>();
                    HeightMultiplierByte = sr.ReadBE<byte>();
                    var dSht1 = sr.ReadBE<short>();
                    var dSht2 = sr.ReadBE<short>();

                    sliceCount = sr.ReadBE<short>() + 1; //Usually has a value except for the very large textures
                    var sht4 = sr.ReadBE<short>();
                    var int0 = sr.ReadBE<int>();
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
                long bufferLength = sliceBufferLength / 2;

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
        /// 0xC0 should be the width base while 0x80 should be the height base
        /// </summary>
        public static int GetDesResolutionComponent(byte DesBaseByte, byte DesResByte, byte multBase)
        {
            var resByte = DesResByte - multBase;

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
            DeSwizzler.GetsourceBytesPerPixelSetAndPixelSize(pixelFormat, out var a, out var b, out var formatBpp);
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
