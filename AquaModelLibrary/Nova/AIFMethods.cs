using AquaModelLibrary.Nova.Structures;
using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.Nova
{
    //Same format as .axs, but uses exclusively the texture data
    public static class AIFMethods
    {
        public static XgmiStruct ReadXgmi(this BufferedStreamReader streamReader)
        {
            XgmiStruct xgmiStr = new XgmiStruct();

            xgmiStr.magic = streamReader.Read<int>();
            xgmiStr.len = streamReader.Read<int>();
            xgmiStr.int_08 = streamReader.Read<int>();
            xgmiStr.paddingLen = streamReader.Read<int>();

            xgmiStr.int_10 = streamReader.Read<int>();
            xgmiStr.int_14 = streamReader.Read<int>();
            xgmiStr.int_18 = streamReader.Read<int>();
            xgmiStr.int_1C = streamReader.Read<int>();

            xgmiStr.int_20 = streamReader.Read<int>();
            xgmiStr.int_24 = streamReader.Read<int>();
            xgmiStr.width = streamReader.Read<ushort>();
            xgmiStr.height = streamReader.Read<ushort>();
            xgmiStr.int_2C = streamReader.Read<int>();

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

            return xgmiStr;
        }

        public static void AssembleDDS()
        {

        }

        public static byte[] GenerateDDSHeader(ushort width, ushort height, int size, int type1, int type2)
        {
            var outBytes = new List<byte>();

            outBytes.AddRange(new byte[] { 0x44, 0x44, 0x53, 0x20, 0x7C, 0x00, 0x00, 0x00, 0x07, 0x10, 0x08, 0x00 });
            outBytes.AddRange(BitConverter.GetBytes(width));
            outBytes.AddRange(new byte[] { 0x00, 0x00 });
            outBytes.AddRange(BitConverter.GetBytes(height));
            outBytes.AddRange(new byte[] { 0x00, 0x00 });
            outBytes.AddRange(BitConverter.GetBytes(size));
            outBytes.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  0x20, 0x00, 0x00, 0x00,
                0x04, 0x00, 0x00, 0x00,  });
            outBytes.AddRange(new byte[] { 0x44, 0x58, 0x54 }); //DXT
            if (type2 == 0x7)
            {
                switch (type1)
                {
                    default:
                        outBytes.Add(0x33); //3 in DXT3
                        break;
                }
            }
            else
            {
                switch (type1)
                {
                    //DXT1
                    case 3:
                    case 6:
                    default:
                        outBytes.Add(0x31); //1 in DXT1
                        break;
                }
            }
            outBytes.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  0x00, 0x10, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00 });

            return outBytes.ToArray();
        }
    }
}
