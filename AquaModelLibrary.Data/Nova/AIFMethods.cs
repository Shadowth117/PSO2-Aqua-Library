using AquaModelLibrary.Data.Nova.Structures;
using AquaModelLibrary.Helpers;
using DirectXTex;
using Reloaded.Memory.Streams;
using static DirectXTex.DirectXTexUtility;

namespace AquaModelLibrary.Data.Nova
{
    //Same format as .axs, but uses exclusively the texture data
    public static class AIFMethods
    {
        public static XgmiStruct ReadXgmi(this BufferedStreamReader<MemoryStream> streamReader)
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
            if ((xgmiStr.mipIdByte & 0x80) > 0)
            {
                mipByteIdBit = 0x80;
            }
            xgmiStr.stamCombinedId = mipByteIdBit.ToString("X2") + xgmiStr.idByte1.ToString("X2") + xgmiStr.idByte2.ToString("X2") + xgmiStr.idByte3.ToString("X2") + xgmiStr.texCatId.ToString("X");
            xgmiStr.stamUniqueId = xgmiStr.mipIdByte.ToString("X2") + xgmiStr.idByte1.ToString("X2") + xgmiStr.idByte2.ToString("X2") + xgmiStr.idByte3.ToString("X2") + xgmiStr.texCatId.ToString("X");

            return xgmiStr;
        }

        public static byte[] GetMipImage(XgmiStruct xgmiStr, byte[] buffer)
        {
            DXGIFormat pixelFormat;
            int deswizzleWidth = xgmiStr.width;
            int deswizzleHeight = xgmiStr.height;
            pixelFormat = GetDDSType(xgmiStr, ref deswizzleWidth, ref deswizzleHeight);

            return DeSwizzler.VitaDeSwizzle(buffer, deswizzleWidth, deswizzleHeight, pixelFormat);
        }

        private static DXGIFormat GetDDSType(XgmiStruct xgmiStr, ref int deswizzleWidth, ref int deswizzleHeight)
        {
            DXGIFormat pixelFormat;
            switch (xgmiStr.dxtType)
            {
                case 0x1:
                    //Not sure actual pixel format for this, needs a fix
                    if (xgmiStr.alphaTesting == 0x8)
                    {
                        pixelFormat = DXGIFormat.BC3UNORM;
                    }
                    else
                    {
                        pixelFormat = DXGIFormat.BC1UNORM;
                    }
                    deswizzleWidth >>= 2;
                    deswizzleHeight >>= 2;
                    break;
                case 0x8:
                    pixelFormat = DXGIFormat.R8G8B8A8UNORM;
                    break;
                case 0xD:
                    //Not sure actual pixel format for this, needs a fix
                    if (xgmiStr.alphaTesting == 0x8)
                    {
                        pixelFormat = DXGIFormat.BC3UNORM;
                    }
                    else
                    {
                        pixelFormat = DXGIFormat.BC1UNORM;
                    }
                    deswizzleWidth >>= 2;
                    deswizzleHeight >>= 2;
                    break;
                case 0x10:
                    if (xgmiStr.alphaTesting == 0x8)
                    {
                        pixelFormat = DXGIFormat.BC3UNORM;
                    }
                    else
                    {
                        pixelFormat = DXGIFormat.BC1UNORM;
                    }
                    deswizzleWidth >>= 2;
                    deswizzleHeight >>= 2;
                    break;
                case 0x12:
                    if (xgmiStr.alphaTesting == 0x8)
                    {
                        pixelFormat = DXGIFormat.BC2UNORM;
                    }
                    else
                    {
                        pixelFormat = DXGIFormat.BC1UNORM;
                    }
                    deswizzleWidth >>= 2;
                    deswizzleHeight >>= 2;
                    break;
                case 0x14:
                    if (xgmiStr.alphaTesting == 0x8)
                    {
                        pixelFormat = DXGIFormat.BC3UNORM;
                    }
                    else
                    {
                        pixelFormat = DXGIFormat.BC1UNORM;
                    }
                    deswizzleWidth >>= 2;
                    deswizzleHeight >>= 2;
                    break;
                case 0x2A:
                    //Not sure actual pixel format for this, needs a fix
                    if (xgmiStr.alphaTesting == 0x8)
                    {
                        pixelFormat = DXGIFormat.BC3UNORM;
                    }
                    else
                    {
                        pixelFormat = DXGIFormat.BC1UNORM;
                    }
                    deswizzleWidth >>= 2;
                    deswizzleHeight >>= 2;
                    break;
                default:
                    throw new Exception($"Unexpected format {xgmiStr.dxtType}");
            }

            return pixelFormat;
        }

        public static byte[] GetDDSHeaderBytes(int width, int height, XgmiStruct headXgmiStruct, byte mipCount, bool isCubeMap)
        {
            int a = 0;
            int b = 0;
            var meta = GenerateMataData(width, height, mipCount, GetDDSType(headXgmiStruct, ref a, ref b), isCubeMap);
            GenerateDDSHeader(meta, DDSFlags.NONE, out var ddsHeader, out var dx10Header, isCubeMap);

            List<byte> outBytes = new List<byte>(DataHelpers.ConvertStruct(ddsHeader));
            outBytes.InsertRange(0, new byte[] { 0x44, 0x44, 0x53, 0x20 });

            return outBytes.ToArray();
        }
    }
}
