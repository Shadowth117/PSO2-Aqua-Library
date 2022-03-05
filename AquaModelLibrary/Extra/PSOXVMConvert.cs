using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary
{
    public class PSOXVMConvert
    {
        public const int MAGIC_XVRT = 0x54525658;

        public static void ExtractXVM(string xvmName)
        {
            ExtractXVM(xvmName, new List<string>(), xvmName.Replace(".xvm", "_xvmOut"));
        }

        public static void ExtractXVM(string xvmName, List<string> texNames, string outFolder)
        {
            if (xvmName != null && File.Exists(xvmName))
            {
                var xvm = File.ReadAllBytes(xvmName);
                var xvmList = new List<byte>(xvm);
                var fileCount = BitConverter.ToUInt32(xvm, 0x8);
                int offset = 0x40; //Start past XVM header

                for (int i = 0; i < fileCount; i++)
                {
                    //Seek through potential padding
                    int magic = 0;
                    while (magic != MAGIC_XVRT && offset < xvm.Length)
                    {
                        magic = BitConverter.ToInt32(xvm, offset);
                        offset += 4;
                    }
                    if (offset > xvm.Length)
                    {
                        break;
                    }
                    offset -= 4;
                    int fullSize = BitConverter.ToInt32(xvm, offset + 0x4) + 8;
                    int type1 = BitConverter.ToInt32(xvm, offset + 0x8);
                    int type2 = BitConverter.ToInt32(xvm, offset + 0xC);
                    ushort sizeX = BitConverter.ToUInt16(xvm, offset + 0x14);
                    ushort sizeY = BitConverter.ToUInt16(xvm, offset + 0x16);
                    int dataSize = BitConverter.ToInt32(xvm, offset + 0x18);

                    var xvr = new List<byte>();
                    xvr.AddRange(GenerateDDSHeader(sizeX, sizeY, dataSize, type1, type2));
                    xvr.AddRange(xvmList.GetRange(offset + 0x40, dataSize));

                    if(outFolder == null || outFolder == "")
                    {
                        outFolder = Path.GetDirectoryName(xvmName);
                    }
                    if (texNames != null && i < texNames.Count)
                    {
                        File.WriteAllBytes(outFolder + texNames[i] + ".dds", xvr.ToArray());
                    }
                    else
                    {
                        File.WriteAllBytes(outFolder + $"Texture{i}.dds", xvr.ToArray());
                    }

                    offset += fullSize + (fullSize % 0x10);
                }
                xvm = null;
            }
        }

        public static void ConvertLooseXVR(string xvrName)
        {
            int offset = 0;
            var xvr = File.ReadAllBytes(xvrName);
            var xvrList = new List<byte>(xvr);
            int magic = BitConverter.ToInt32(xvr, offset);
            int fullSize = BitConverter.ToInt32(xvr, offset + 0x4) + 8;
            int type1 = BitConverter.ToInt32(xvr, offset + 0x8);
            int type2 = BitConverter.ToInt32(xvr, offset + 0xC);
            ushort sizeX = BitConverter.ToUInt16(xvr, offset + 0x14);
            ushort sizeY = BitConverter.ToUInt16(xvr, offset + 0x16);
            int dataSize = BitConverter.ToInt32(xvr, offset + 0x18);

            var xvrOut = new List<byte>();
            xvrOut.AddRange(GenerateDDSHeader(sizeX, sizeY, dataSize, type1, type2));
            xvrOut.AddRange(xvrList.GetRange(offset + 0x40, dataSize));


            var outFileName = Path.ChangeExtension(xvrName, ".dds");
            
            File.WriteAllBytes(outFileName, xvrOut.ToArray());

            xvr = null;
            xvrList = null;
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
                0x04, 0x00, 0x00, 0x00,  0x44, 0x58, 0x54});
            if(type2 == 0x7)
            {
                switch (type1)
                {
                    default:
                        outBytes.Add(0x33); //3 in DXT3
                        break;
                }
            } else
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
