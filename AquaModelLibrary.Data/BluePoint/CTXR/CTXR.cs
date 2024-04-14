using AquaModelLibrary.Helpers;
using AquaModelLibrary.Helpers.Readers;
using static DirectXTex.DirectXTexUtility;

namespace AquaModelLibrary.Data.BluePoint.CTXR
{
    public class CTXR
    {
        public bool isPng = false;
        public int textureFormat = -1;
        public short alphaSetting;
        public int fileCount;
        public short externalMipCount;
        public short internalMipCount;
        
        /// <summary>
        /// CTXRs can contain arrays of textures. CTXRs image arrays all share the same mip count. 
        /// </summary>
        public short textureArrayCount;

        public byte desWidthByte;
        public byte desHeightByte;

        public List<CTXRExternalReference> mipPaths = new List<CTXRExternalReference>();
        public List<byte[]> mipMaps = new List<byte[]>();
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
                mipMaps.Add(file);
                return;
            }
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        private void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {

            sr.Seek(sr.BaseStream.Length - 0xC, SeekOrigin.Begin);
            footerData = sr.Read<CFooter>();

            sr.Seek(0, SeekOrigin.Begin);
            textureFormat = sr.Read<int>();
            switch(footerData.version)
            {
                case 0x25: //SOTC
                    for (int i = 0; i < externalMipCount; i++)
                    {
                        mipPaths.Add(new CTXRExternalReference(sr));
                    }
                    break;
                case 0x6E: //DeSR
                    var unkSht0 = sr.Read<short>();
                    alphaSetting = sr.Read<short>();
                    var unkInt0 = sr.Read<int>();
                    var unkInt1 = sr.Read<int>();

                    fileCount = sr.Read<int>(); //Always 1 + external mipCount. 1 is probably this file
                    externalMipCount = sr.Read<short>();
                    internalMipCount = sr.Read<short>();
                    var unkSht1 = sr.Read<short>();
                    var unkSht2 = sr.Read<short>();
                    var unkInt2 = sr.Read<int>();

                    for (int i = 0; i < externalMipCount; i++)
                    {
                        mipPaths.Add(new CTXRExternalReference(sr));
                    }
                    var unkInt3 = sr.ReadBE<int>();
                    var unkSht3 = sr.ReadBE<short>();
                    if(externalMipCount == 0)
                    {
                        var unkBt0 = sr.ReadBE<byte>();
                    }

                    //Texture info structure
                    var sht0 = sr.ReadBE<short>();
                    var bt0 = sr.ReadBE<byte>();
                    desWidthByte = sr.ReadBE<byte>();
                    var bt1 = sr.ReadBE<byte>();
                    desHeightByte = sr.ReadBE<byte>();
                    var sht1 = sr.ReadBE<short>();
                    var sht2 = sr.ReadBE<short>();
                    var sht3 = sr.ReadBE<short>();
                    var sht4 = sr.ReadBE<short>();
                    var int0 = sr.ReadBE<int>();
                    break;
                default:
                    throw new Exception("Unexpected CTXR type!");
            }
        }

        /// <summary>
        /// Assumes external references are first mips.
        /// </summary>
        public CTXRExternalReference[] GetSortedExternalRefList()
        {
            CTXRExternalReference[] refList = new CTXRExternalReference[externalMipCount];
            foreach(var reference in mipPaths)
            {
                refList[reference.mipLevel] = reference;
            }

            return refList;
        }

        public void WriteToDDS(string rootPath, string outPath)
        {
            if(textureFormat == -1)
            {
                return;
            }

            FileStream outStream = new FileStream(outPath, FileMode.Create);
            BinaryWriter outWriter = new BinaryWriter((Stream)outStream);

            //Assume external mips come first
            var refList = GetSortedExternalRefList();
            var pixelFormat = GetFormat();
            List<byte[]> mipsList = new List<byte[]>();
            foreach(var reference in refList)
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
                    return DXGIFormat.R16G16B16A16UNORM; //A guess for now; few examples
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
                    return DXGIFormat.BC6HUF16;
                case 0x11:
                    return DXGIFormat.BC7UNORM;
                default:
                    throw new Exception($"Unexpected pixel format: {textureFormat:X}");
            }
        }
    }
}
