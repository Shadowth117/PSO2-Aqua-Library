using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers.Writers;
using System.Diagnostics;
using System.Text;
using static AquaModelLibrary.Helpers.DDS.DirectXTexUtility;

namespace AquaModelLibrary.Data.DirectX
{
    //Rework of the SoulsFormats DDS handling to be used specifically for Aqua
    public class DDS
    {
        [Flags]
        public enum DDSD : uint
        {
            CAPS = 0x1,
            HEIGHT = 0x2,
            WIDTH = 0x4,
            PITCH = 0x8,
            PIXELFORMAT = 0x1000,
            MIPMAPCOUNT = 0x20000,
            LINEARSIZE = 0x80000,
            DEPTH = 0x800000,
        }

        [Flags]
        public enum DDSCAPS : uint
        {
            COMPLEX = 0x8,
            TEXTURE = 0x1000,
            MIPMAP = 0x400000,
        }

        [Flags]
        public enum DDSCAPS2 : uint
        {
            CUBEMAP = 0x200,
            CUBEMAP_POSITIVEX = 0x400,
            CUBEMAP_NEGATIVEX = 0x800,
            CUBEMAP_POSITIVEY = 0x1000,
            CUBEMAP_NEGATIVEY = 0x2000,
            CUBEMAP_POSITIVEZ = 0x4000,
            CUBEMAP_NEGATIVEZ = 0x8000,
            VOLUME = 0x200000,
        }

        [Flags]
        public enum DDPF : uint
        {
            ALPHAPIXELS = 0x1,
            ALPHA = 0x2,
            FOURCC = 0x4,
            RGB = 0x40,
            YUV = 0x200,
            LUMINANCE = 0x20000,
        }

        public class PixelFormat
        {
            public DDPF dwFlags;
            public string dwFourCC;
            public int dwRGBBitCount;
            public uint dwRBitMask;
            public uint dwGBitMask;
            public uint dwBBitMask;
            public uint dwABitMask;

            /// <summary>
            /// Create a new PIXELFORMAT with default values.
            /// </summary>
            public PixelFormat()
            {
                dwFourCC = "\0\0\0\0";
            }

            internal PixelFormat(BufferedStreamReaderBE<MemoryStream> sr)
            {
                var dwSize = sr.Read<uint>(); // dwSize
                dwFlags = (DDPF)sr.Read<uint>();
                dwFourCC = Encoding.UTF8.GetString(sr.Read4Bytes());
                dwRGBBitCount = sr.Read<int>();
                dwRBitMask = sr.Read<uint>();
                dwGBitMask = sr.Read<uint>();
                dwBBitMask = sr.Read<uint>();
                dwABitMask = sr.Read<uint>();
            }

            internal byte[] GetBytes()
            {
                ByteListWriter outBytes = new();
                outBytes.AddValue(32);
                outBytes.AddValue((uint)dwFlags);

                // Make sure it's 4 characters
                var ASCIIbytesTemp = Encoding.UTF8.GetBytes(dwFourCC);
                byte[] ASCIIBytes = new byte[4];
                Array.Copy(ASCIIbytesTemp, ASCIIBytes, 4);

                outBytes.AddRange(ASCIIBytes);
                outBytes.AddValue(dwRGBBitCount);
                outBytes.AddValue(dwRBitMask);
                outBytes.AddValue(dwGBitMask);
                outBytes.AddValue(dwBBitMask);
                outBytes.AddValue(dwABitMask);

                return outBytes.ToArray();
            }
        }

        public enum DIMENSION : uint
        {
            TEXTURE1D = 2,
            TEXTURE2D = 3,
            TEXTURE3D = 4,
        }

        [Flags]
        public enum RESOURCE_MISC : uint
        {
            TEXTURECUBE = 0x4,
        }

        public enum ALPHA_MODE : uint
        {
            UNKNOWN = 0,
            STRAIGHT = 1,
            PREMULTIPLIED = 2,
            OPAQUE = 3,
            CUSTOM = 4,
        }

        public class HeaderDX10
        {
            public DXGIFormat dxgiFormat;
            public DIMENSION resourceDimension;
            public RESOURCE_MISC miscFlag;
            public uint arraySize;
            public ALPHA_MODE miscFlags2;

            /// <summary>
            /// Creates a new DX10 header with default values.
            /// </summary>
            public HeaderDX10()
            {
                dxgiFormat = DXGIFormat.UNKNOWN;
                resourceDimension = DIMENSION.TEXTURE2D;
                arraySize = 1;
                miscFlags2 = ALPHA_MODE.UNKNOWN;
            }

            internal HeaderDX10(BufferedStreamReaderBE<MemoryStream> sr)
            {
                dxgiFormat = (DXGIFormat)sr.Read<uint>();
                resourceDimension = (DIMENSION)sr.Read<uint>();
                miscFlag = (RESOURCE_MISC)sr.Read<uint>();
                arraySize = sr.Read<uint>();
                miscFlags2 = (ALPHA_MODE)sr.Read<uint>();
            }

            internal byte[] GetBytes()
            {
                ByteListWriter outBytes = new();
                outBytes.AddValue((uint)dxgiFormat);
                outBytes.AddValue((uint)resourceDimension);
                outBytes.AddValue((uint)miscFlag);
                outBytes.AddValue(arraySize);
                outBytes.AddValue((uint)miscFlags2);

                return outBytes.ToArray();
            }
        }

        public DXGIFormat GetDXGIFormat()
        {
            if (header10 != null)
            {
                return header10.dxgiFormat;
            }
            else
            {
                //https://learn.microsoft.com/en-us/windows/uwp/gaming/complete-code-for-ddstextureloader
                switch (ddspf.dwFourCC)
                {
                    case "DXT1":
                    case "DXT2":
                        return DXGIFormat.BC1UNORM;
                    case "DXT3":
                        return DXGIFormat.BC2UNORM;
                    case "DXT4":
                    case "DXT5":
                        return DXGIFormat.BC3UNORM;
                    case "ATI1":
                        return DXGIFormat.BC4UNORM;
                    case "BC4S":
                        return DXGIFormat.BC4SNORM;
                    case "ATI2":
                    case "BC5U":
                        return DXGIFormat.BC5UNORM;
                    case "BC5S":
                        return DXGIFormat.BC5SNORM;
                    case "RGBG":
                        return DXGIFormat.R8G8B8G8UNORM;
                    case "GRGB":
                        return DXGIFormat.G8R8G8B8UNORM;
                    case "$\0\0\0": //36
                        return DXGIFormat.R16G16B16A16UNORM;
                    case "n\0\0\0": //110
                        return DXGIFormat.R16G16B16A16SNORM;
                    case "o\0\0\0": //111
                        return DXGIFormat.R16FLOAT;
                    case "p\0\0\0": //112
                        return DXGIFormat.R16G16FLOAT;
                    case "q\0\0\0": //113
                        return DXGIFormat.R16G16B16A16FLOAT;
                    case "r\0\0\0": //114
                        return DXGIFormat.R32FLOAT;
                    case "s\0\0\0": //115
                        return DXGIFormat.R32G32FLOAT;
                    case "t\0\0\0": //116
                        return DXGIFormat.R32G32B32A32FLOAT;
                    default:
                        Debug.WriteLine("Unrecognized FourCC, defaulting to R8G8B8A8");
                        return DXGIFormat.R8G8B8A8UNORM;
                }
            }
        }

        public List<Image> mipData = new();

        public DDSD dwFlags;
        public int dwHeight;
        public int dwWidth;
        public int dwPitchOrLinearSize;
        public int dwDepth;
        public int dwMipMapCount;
        public List<int> dwReserved1 = new();
        public PixelFormat ddspf;
        public DDSCAPS dwCaps;
        public DDSCAPS2 dwCaps2;
        public int dwCaps3;
        public int dwCaps4;
        public int dwReserved2;
        public HeaderDX10 header10;
        public int DataOffset => ddspf.dwFourCC == "DX10" ? 0x94 : 0x80;


        /// <summary>
        /// Read a DDS header from an array of bytes.
        /// </summary>
        public DDS(byte[] bytes)
        {
            var sr = new BufferedStreamReaderBE<MemoryStream>(new MemoryStream(bytes));

            var magic = sr.Read<int>(); // dwMagic
            var size = sr.Read<int>(); // dwSize
            dwFlags = (DDSD)sr.Read<uint>();
            dwHeight = sr.Read<int>();
            dwWidth = sr.Read<int>();
            dwPitchOrLinearSize = sr.Read<int>();
            dwDepth = sr.Read<int>();
            dwMipMapCount = sr.Read<int>();
            for(int i = 0; i < 11; i++)
            {
                dwReserved1.Add(sr.Read<int>());
            }
            ddspf = new PixelFormat(sr);
            dwCaps = (DDSCAPS)sr.Read<uint>();
            dwCaps2 = (DDSCAPS2)sr.Read<uint>();
            dwCaps3 = sr.Read<int>();
            dwCaps4 = sr.Read<int>();
            dwReserved2 = sr.Read<int>();

            if (ddspf.dwFourCC == "DX10")
                header10 = new HeaderDX10(sr);
            else
                header10 = null;

            ReadDDSTextureBuffers(sr);
        }

        /// <summary>
        /// Write a DDS file from this header object and given pixel data.
        /// </summary>
        public byte[] Write()
        {
            var outBytes = new ByteListWriter();

            outBytes.AddRange([0x44, 0x44, 0x53, 0x20]);
            outBytes.AddValue(0x7C);
            outBytes.AddValue((uint)dwFlags);
            outBytes.AddValue(dwHeight);
            outBytes.AddValue(dwWidth);
            outBytes.AddValue(dwPitchOrLinearSize);
            outBytes.AddValue(dwDepth);
            outBytes.AddValue(dwMipMapCount);
            foreach(var reserve in dwReserved1)
            {
                outBytes.AddValue(reserve);
            }
            outBytes.AddRange(ddspf.GetBytes());
            outBytes.AddValue((uint)dwCaps);
            outBytes.AddValue((uint)dwCaps2);
            outBytes.AddValue(dwCaps3);
            outBytes.AddValue(dwCaps4);
            outBytes.AddValue(dwReserved2);

            if (ddspf.dwFourCC == "DX10")
                outBytes.AddRange(header10.GetBytes());

            outBytes.AddRange(Image.Write(mipData));
            return outBytes.ToArray();
        }

        /// <summary>
        /// Grab a dds's texture buffer data. 
        /// </summary>
        public void ReadDDSTextureBuffers(BufferedStreamReaderBE<MemoryStream> sr)
        {
            mipData = new List<Image>();
            sr.Seek(DataOffset, SeekOrigin.Begin);
            DrSwizzler.Util.GetsourceBytesPerPixelSetAndPixelSize((DrSwizzler.DDS.DXEnums.DXGIFormat)GetDXGIFormat(), out int sourceBytesPerPixelSet, out int pixelBlockSize, out int formatBpp);
            long fullImageSize = formatBpp * dwWidth * dwHeight / 8;

            if (dwCaps2.HasFlag(DDS.DDSCAPS2.VOLUME))
            {
                int depth = dwDepth;
                for(int d = 0; d < depth; d++)
                {
                    mipData.Add(new Image());
                }
                for (int m = 0; m < dwMipMapCount; m++)
                {
                    var imageSize = fullImageSize;
                    for (int i = 0; i < depth; i++)
                    {
                        if (sr.Position + imageSize <= sr.BaseStream.Length)
                        {
                            mipData[i].subImages.Add(sr.ReadBytesSeek((int)imageSize));
                            if (imageSize < sourceBytesPerPixelSet)
                            {
                                imageSize = sourceBytesPerPixelSet;
                            }
                        }
                        else
                        {
                            //Fix mipmapcount if we have to
                            dwMipMapCount = m;
                        }
                    }

                    //After we hit 1 depth, we should continue with 1 map for each subsequent mip
                    if (depth != 1)
                    {
                        depth /= 2;
                        imageSize /= 4;
                    }
                }
            }
            else //We can read CubeMaps and standard textures together
            {
                int depth = 1;
                //If someone tries to put in a screwy cubemap with less than 6 textures, user error.
                if (dwCaps2.HasFlag(DDS.DDSCAPS2.CUBEMAP))
                {
                    depth = 6;
                }
                else if (dwDepth > 1)
                {
                    depth = dwDepth;
                }

                for (int i = 0; i < depth; i++)
                {
                    Image img = new Image();
                    var imageSize = fullImageSize;
                    for (int m = 0; m < dwMipMapCount; m++)
                    {
                        if (sr.Position + imageSize <= sr.BaseStream.Length)
                        {
                            img.subImages.Add(sr.ReadBytesSeek((int)imageSize));
                            imageSize /= 4;
                            if (imageSize < sourceBytesPerPixelSet)
                            {
                                imageSize = sourceBytesPerPixelSet;
                            }
                        }
                        else
                        {
                            //Fix mipmapcount if we have to
                            dwMipMapCount = m;
                        }
                    }
                    mipData.Add(img);
                }
            }
        }

        public class Image
        {
            //Used for a particular image's mipmap, or for all images of a particular miplevel for a volume texture
            public List<byte[]> subImages;

            public Image()
            {
                subImages = new List<byte[]>();
            }

            public static byte[] Write(List<Image> images)
            {
                var outBytes = new List<byte>();
                foreach (Image image in images)
                    foreach (byte[] mip in image.subImages)
                        outBytes.AddRange(mip);
                return outBytes.ToArray();
            }
        }
    }
}
