using AquaModelLibrary.Helpers;
using AquaModelLibrary.Helpers.Readers;
using System.Diagnostics;
using static DirectXTex.DirectXTexUtility;
using static VrSharp.Pvr.PvrDataCodec;

namespace AquaModelLibrary.Data.BluePoint.CTXR
{
    public class CTXR
    {
        public bool isPng = false;
        public int textureFormat = -1;
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

        private void Read(BufferedStreamReaderBE<MemoryStream> sr, bool readData = true)
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
                    var unkSht0 = sr.Read<short>();
                    alphaSetting = sr.Read<short>();
                    textureType = sr.Read<CTextureType>();
                    var unkInt1 = sr.Read<int>();

                    fileCount = sr.Read<int>(); //Always 1 + external mipCount. 1 is probably this file
                    externalMipCount = sr.Read<short>();
                    internalMipCount = sr.Read<short>();
                    var unkSht1 = sr.Read<short>();
                    pixelBlockSizeThing = sr.Read<short>();
                    var unkInt2 = sr.Read<int>();

                    for (int i = 0; i < externalMipCount; i++)
                    {
                        mipPaths.Add(new CTXRExternalReference(sr));
                    }
                    var unkInt3 = sr.ReadBE<int>();
                    var unkSht3 = sr.ReadBE<short>();
                    if (externalMipCount == 0)
                    {
                        var unkBt0 = sr.ReadBE<byte>();
                    }

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
                    DeSwizzler.GetSourceBytesPerPixelAndPixelSize(pixelFormat, out var sourceBytesPerPixel, out var pixelBlockSize);

                    var totalBufferLength = sr.BaseStream.Length - headerLength - 0xC; //Subtract file header and footer from file total length
                    var sliceBufferLength = totalBufferLength / sliceCount;

                    //Get top internal mip resolution
                    var texWidth = GetDesResolutionComponent(desWidthBaseByte, desWidthByte);
                    var texHeight = GetDesResolutionComponent(desHeightBaseByte, desHeightByte);

                    GetLargestInternalMipResolution(texWidth, texHeight,
                        externalMipCount, out var finalWidth, out var finalHeight);

                    //The texture buffers for internal mipmaps seemingly subdivide by 2 each time we go down a mip, UNTIL we reach 0x400. When the buffer should be 0x400, we instead skip to 0x200.
                    //All mipmap buffers after this will be 0x100 regardless of true size.
                    //While the buffers are larger than the actual texture size, the swizzling happens at the BUFFER level and thus reading the full buffer for deswizzling is paramount
                    if (readData)
                    {
                        for (int s = 0; s < sliceCount; s++)
                        {
                            int mipWidth = finalWidth;
                            int mipHeight = finalHeight;
                            long bufferLength = sliceBufferLength;

                            //Some buffer lengths are irregular and we want to adjust them for processing
                            if(bufferLength == 0x60000 && internalMipCount > 1)
                            {
                                bufferLength = 0x80000;
                            }
                            mipMapsList.Add(new List<byte[]>());

                            long bufferUsed = 0;
                            for (int i = 0; i < internalMipCount; i++)
                            {
                                if (internalMipCount > 1)
                                {
                                    if (bufferLength != 0x100)
                                    {
                                        bufferLength = bufferLength / 2;

                                        if(bufferLength == 0x400)
                                        {
                                            bufferLength = bufferLength / 2;
                                        }
                                    }
                                }
                                bufferUsed += bufferLength;
                                var mipFull = sr.ReadBytes( ((sliceBufferLength * sliceCount) - (sliceBufferLength * s)) - bufferUsed + headerLength, (int)bufferLength);

                                //Make sure that we have enough bytes to actually deswizzle
                                int swizzleBlockWidth = mipWidth < 4 ? 4 : mipWidth;
                                int swizzleBlockHeight = mipHeight < 4 ? 4 : mipHeight;
                                
                                //If it's too small, we don't need to deswizzle
                                if (mipWidth <= 4 && mipHeight <= 4)
                                {
                                    mipFull = DeSwizzler.PS5DeSwizzle(mipFull, swizzleBlockWidth, swizzleBlockHeight, pixelFormat);
                                }

                                //Extract as a tile from the pixels if we haven't done that at the deswizzle step
                                if(swizzleBlockWidth != mipWidth || swizzleBlockHeight != mipHeight)
                                {
                                    mipFull = DeSwizzler.ExtractTile(mipFull, pixelFormat, swizzleBlockWidth, 0, 0, mipWidth, mipHeight);
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
            for(int i = 0; i < externalMipCount; i++)
            {
                finalWidth /= 2;
                finalHeight /= 2;
            }
        }

        public static int GetDesResolutionComponent(byte DesBaseByte, byte DesResByte)
        {
            var resByte = DesResByte % 0x10;
            
            switch(DesBaseByte)
            {
                case 0x1B:
                    if (resByte == 2)
                    {
                        return 2160;
                    }
                    else
                    {
                        throw new Exception($"Unexpected Resolution Value For {DesBaseByte:X}!");
                    }
                case 0xBF:
                    if(resByte == 3)
                    {
                        return 3840;
                    } else
                    {
                        throw new Exception($"Unexpected Resolution Value For {DesBaseByte:X}!");
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

        public void WriteToDDS(string rootPath, string outPath)
        {
            //If this is a png, just write it out directly
            if (textureFormat == -1)
            {
                File.WriteAllBytes(outPath, mipMapsList[0][0]);
                return;
            }

            FileStream outStream = new FileStream(outPath, FileMode.Create);
            BinaryWriter outWriter = new BinaryWriter((Stream)outStream);

            //Assume external mips come first
            var refList = GetSortedExternalRefList();
            var pixelFormat = GetFormat();
            List<byte[]> mipsList = new List<byte[]>();
            foreach (var reference in refList)
            {
                var chunk = File.ReadAllBytes(Path.Combine(rootPath, reference.externalMipReference.Substring(2)));
                switch (footerData.version)
                {
                    case 0x25: //SOTC
                        //mipsList.Add(Deswizzler.PS4DeSwizzle(chunk, ,, pixelFormat));
                        break;
                    case 0x6E: //DeSR
                        //mipsList.Add(Deswizzler.PS5DeSwizzle(chunk, ,, pixelFormat));
                        break;
                    default:
                        throw new Exception("Unexpected CTXR type!");
                }
            }

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
    }
}
