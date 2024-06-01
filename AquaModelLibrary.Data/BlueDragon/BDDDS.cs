using AquaModelLibrary.Data.BluePoint.CTXR;
using AquaModelLibrary.Helpers;
using AquaModelLibrary.Helpers.Readers;
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
        /// This times 128 seems to be the texture width
        /// </summary>
        public byte resolutionValueX;

        /// <summary>
        /// 0x29
        /// Changes the base 128 to 160.
        /// </summary>
        public byte resolutionModifierX;

        /// <summary>
        /// 0x28
        /// This plus 1, then multiplied times 8 seems to be the texture height
        /// </summary>
        public byte resolutionValueY;

        public byte[] buffer = null;

        public BDDDS()
        {

        }

        public BDDDS(byte[] file)
        {
            Read(file);
        }

        public void Read(byte[] file)
        {
            using (var ms = new MemoryStream(file))
            using (var sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                sr._BEReadActive = true;
                fullBufferSize = sr.ReadBE<int>();

                sr.Seek(0x20, SeekOrigin.Begin);
                resolutionValueX = sr.ReadBE<byte>();
                resolutionModifierX = sr.ReadBE<byte>();

                sr.Seek(0x24, SeekOrigin.Begin);
                pixelFormat = sr.ReadBE<int>();
                sr.Seek(0x1, SeekOrigin.Current);
                resolutionValueY = sr.ReadBE<byte>();

                buffer = sr.ReadBytes(0x800, fullBufferSize);
            }
        }

        public void GetResolution(out int width, out int height)
        {
            int baseXValue = 128;
            if(resolutionModifierX == 0xC0)
            {
                baseXValue = 160;
            }
            width = (resolutionValueX - 0x80) * baseXValue;
            height = (resolutionValueY + 1) * 8;
        }

        public DXGIFormat GetPixelFormat()
        {
            switch(pixelFormat)
            {
                case 0x52:
                    return DXGIFormat.BC1UNORM;
                case 0x53:
                    return DXGIFormat.BC2UNORM;
                case 0x54:
                    return DXGIFormat.BC3UNORM;
                case 0x86:
                    return DXGIFormat.R8G8B8A8UNORM;
                default:
#if DEBUG
                    throw new Exception($"Unknown pixel format: {pixelFormat:X}");
#endif
                    Debug.WriteLine($"Unknown pixel format: {pixelFormat:X}");
                    return DXGIFormat.BC1UNORM;
            }
        }

        public List<byte> GenerateDDSHeader(DXGIFormat pixelFormat, int texWidth, int texHeight, int mipCount, int depth = 1)
        {
            var meta = GenerateMataData(texWidth, texHeight, mipCount, pixelFormat, false, depth);

            //if (alphaSetting > 0)
            //{
                meta.MiscFlags2 = TexMiscFlags2.TEXMISC2ALPHAMODEMASK;
            //}
            DirectXTex.DirectXTexUtility.GenerateDDSHeader(meta, DDSFlags.NONE, out var ddsHeader, out var dx10Header, false);

            List<byte> outbytes = new List<byte>(DataHelpers.ConvertStruct(ddsHeader));
            /*
            if (isDx10())
            {
                outbytes.AddRange(DataHelpers.ConvertStruct(dx10Header));
            }
            */
            outbytes.InsertRange(0, new byte[] { 0x44, 0x44, 0x53, 0x20 });
            return outbytes;
        }
    }
}
