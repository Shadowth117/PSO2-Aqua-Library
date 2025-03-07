using AquaModelLibrary.Helpers;
using AquaModelLibrary.Helpers.Readers;
using static DirectXTex.DirectXTexUtility;

namespace AquaModelLibrary.Data.Capcom.MonsterHunter
{
    /// <summary>
    /// Based off https://github.com/Silvris/MH-Tools-and-Scripts/blob/18a51a92cb028f3c0d16134fbfb4a37276f61f5c/Noesis/plugins/python/tex_MH1_apx.py
    /// </summary>
    public class APX
    {
        public ushort bitDepth;
        public ushort width;
        public ushort height;
        public ushort paletteIndex;
        public ushort paletteBits;
        public ushort unkValue;

        public byte[] imageData = null;
        public byte[] paletteData = null;
        public APX() { }

        public APX(byte[] file)
        {
            Read(file);
        }
        public APX(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        private void Read(byte[] file)
        {
            using (MemoryStream stream = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                Read(sr);
            }
        }

        private void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            var fileSize = sr.ReadBE<int>();
            var imageDataLength = sr.ReadBE<int>();
            var paletteLength = sr.ReadBE<int>();
            bitDepth = sr.ReadBE<ushort>();
            width = sr.ReadBE<ushort>();

            height = sr.ReadBE<ushort>();
            paletteIndex = sr.ReadBE<ushort>();
            paletteBits = sr.ReadBE<ushort>();
            unkValue = sr.ReadBE<ushort>();
            var reserve0 = sr.ReadBE<int>();
            var reserve1 = sr.ReadBE<int>();

            imageData = sr.ReadBytesSeek(imageDataLength);
            paletteData = sr.ReadBytesSeek(paletteLength);
        }

        public byte[] GetPalettizedBytes()
        {
            if (paletteBits == 0)
            {
                return imageData;
            }
            var imageDataList = new List<byte>();
            if(bitDepth == 4)
            {
                for (int i = 0; i < imageData.Length; i++)
                {
                    var bt = imageData[i];
                    imageDataList.Add((byte)(bt & 0xF));
                    imageDataList.Add((byte)(bt >> 4));
                }
            } else
            {
                imageDataList.AddRange(imageData);
            }

            var paletteDataList = new List<byte>();
            if (paletteBits == 0x10)
            {
                
                for (int i = 0; i < paletteData.Length; i+=2)
                {
                    var bt = paletteData[i];
                    var bt1 = paletteData[i + 1];
                    paletteDataList.Add((byte)Math.Min((bt & 0x1F) * 8, 0xFF));
                    paletteDataList.Add((byte)Math.Min(((bt >> 5) | (bt1 & 3) << 3) * 8, 0xFF));
                    paletteDataList.Add((byte)Math.Min(((bt1 >> 2) & 0x1F) * 8, 0xFF));
                    paletteDataList.Add((byte)(((bt1 >> 7) & 0x1) * 0xFF));
                }
            }
            else
            {
                paletteDataList.AddRange(paletteData);
            }

            List<byte> palettizedBytes = new List<byte>();

            for (int i = 0; i < imageDataList.Count; i++)
            {
                var paletteIndexBase = imageDataList[i] * 4;
                palettizedBytes.Add(paletteDataList[paletteIndexBase]);
                palettizedBytes.Add(paletteDataList[paletteIndexBase + 1]);
                palettizedBytes.Add(paletteDataList[paletteIndexBase + 2]);
                palettizedBytes.Add(paletteDataList[paletteIndexBase + 3]);
            }

            return palettizedBytes.ToArray();
        }

        public byte[] ToDDS()
        {
            List<byte> outBytes = new List<byte>();
            var header = GenerateDDSHeader(DXGIFormat.R8G8B8A8UNORM, width, height, 1);
            outBytes.AddRange(header);
            outBytes.AddRange(GetPalettizedBytes());

            return outBytes.ToArray();
        }

        private static List<byte> GenerateDDSHeader(DXGIFormat pixelFormat, int texWidth, int texHeight, int mipCount, int depth = 1)
        {
            var meta = GenerateMataData(texWidth, texHeight, mipCount, pixelFormat, false, depth);
            /*
            if (alphaSetting > 0)
            {
                meta.MiscFlags2 = TexMiscFlags2.TEXMISC2ALPHAMODEMASK;
            }
            */
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
