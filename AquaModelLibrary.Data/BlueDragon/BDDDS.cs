using System.Diagnostics;
using static DirectXTex.DirectXTexUtility;

namespace AquaModelLibrary.Data.BlueDragon
{
    /// <summary>
    /// Blue Dragon Dds file. Uses the .dds extension, but it's a distinct header format. Texture buffers use Xbox 360 swizzling. Big Endian.
    /// Texture buffer always starts at 0x800
    /// </summary>
    public class BDDDS
    {
        public int fullBufferSize;

        /// <summary>
        /// 0x24
        /// </summary>
        public int pixelFormat;

        /// <summary>
        /// 0x28
        /// This times 8 seems to be the texture width and height (Only square textures?)
        /// </summary>
        public ushort resolutionHint;
    
        public static DXGIFormat GetPixelFormat(int pixelFormat)
        {
            switch(pixelFormat)
            {
                case 0x52:
                    return DXGIFormat.BC1UNORM;
                case 0x53:
                    return DXGIFormat.BC2UNORM;
                case 0x54:
                    return DXGIFormat.BC3UNORM;
                default:
                    Debug.WriteLine($"Unknown pixel format: {pixelFormat:X}");
                    return DXGIFormat.BC1UNORM;
            }
        }
    }
}
