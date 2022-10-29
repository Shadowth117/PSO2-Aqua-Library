using AquaModelLibrary.AquaMethods;
using DirectXTex;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using static DirectXTex.DirectXTexUtility;

namespace AquaModelLibrary
{
    public class PSOXVMConvert
    {
        public enum XVRTexFlag : int
        {
            Mips = 1,
            Alpha = 2,
        }

        public static List<string> XVRFormats = new List<string>()
        {
            "D3DFMT_UNKNOWN",
            "D3DFMT_A8R8G8B8",
            "D3DFMT_R5G6B5",
            "D3DFMT_A1R5G5B5",
            "D3DFMT_A4R4G4B4",
            "D3DFMT_P8",
            "D3DFMT_DXT1",
            "D3DFMT_DXT2",
            "D3DFMT_DXT3",
            "D3DFMT_DXT4",
            "D3DFMT_DXT5",
            "D3DFMT_A8R8G8B8",
            "D3DFMT_R5G6B5",
            "D3DFMT_A1R5G5B5",
            "D3DFMT_A4R4G4B4",
            "D3DFMT_YUY2",
            "D3DFMT_V8U8",
            "D3DFMT_A8",
            "D3DFMT_X1R5G5B5",
            "D3DFMT_X8R8G8B8",
            "D3DFMT_UNKNOWN",
            "D3DFMT_UNKNOWN",
            "D3DFMT_UNKNOWN",
            "D3DFMT_UNKNOWN",
        };

        public const int MAGIC_XVRT = 0x54525658;

        public static void ExtractXVM(string xvmName)
        {
            ExtractXVM(xvmName, new List<string>(), xvmName.Replace(".xvm", "_xvmOut"));
        }

        public static void ExtractXVM(string xvmName, List<string> texNames, string outFolder, bool dumpRawXvr = false)
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
                    while (magic != MAGIC_XVRT && offset < xvm.Length - 3)
                    {
                        magic = BitConverter.ToInt32(xvm, offset);
                        offset += 4;
                    }
                    if (offset > xvm.Length - 3)
                    {
                        break;
                    }
                    offset -= 4;
                    int fullSize = BitConverter.ToInt32(xvm, offset + 0x4) + 8;
                    int flags = BitConverter.ToInt32(xvm, offset + 0x8);
                    int type = BitConverter.ToInt32(xvm, offset + 0xC);
                    ushort sizeY = BitConverter.ToUInt16(xvm, offset + 0x14);
                    ushort sizeX = BitConverter.ToUInt16(xvm, offset + 0x16);
                    int dataSize = BitConverter.ToInt32(xvm, offset + 0x18);
                    var xvr = new List<byte>();

                    if (dumpRawXvr)
                    {
                        var rawXvr = new byte[fullSize];
                        //var rawXvrData = new byte[fullSize - 0x40];
                        Array.Copy(xvm, offset, rawXvr, 0, fullSize);
                        //Array.Copy(xvm, offset + 0x40, rawXvrData, 0, fullSize - 0x40);
                        /*
                        Bitmap img = new Bitmap(sizeX, sizeY, PixelFormat.Format16bppArgb1555);
                        BitmapData bitmapData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.WriteOnly, PixelFormat.Format16bppArgb1555);
                        Marshal.Copy(rawXvrData, 0, bitmapData.Scan0, rawXvrData.Length);
                        img.UnlockBits(bitmapData);
                        img.Save($"C:\\test{i}.png", ImageFormat.Png);
                        */
                        if (texNames != null && i < texNames.Count)
                        {
                            Debug.WriteLine(texNames[i] + ".dds" + " " + flags.ToString("X") + " " + type.ToString("X"));
                            File.WriteAllBytes(outFolder + ModelImporter.NixIllegalCharacters(texNames[i]) + ".xvr", rawXvr);
                        }
                        else
                        {
                            Debug.WriteLine($"Texture{i}.dds" + " " + flags.ToString("X") + " " + type.ToString("X"));
                            File.WriteAllBytes(outFolder + $"Texture{i}.xvr", rawXvr);
                        }
                    } else
                    {
                        xvr.AddRange(GenerateDDSHeaderNew(sizeX, sizeY, dataSize, flags, type));
                        xvr.AddRange(xvmList.GetRange(offset + 0x40, dataSize));

                        if (outFolder == null || outFolder == "")
                        {
                            outFolder = Path.GetDirectoryName(xvmName);
                        }
                        if (texNames != null && i < texNames.Count)
                        {
                            //Debug.WriteLine(texNames[i] + ".dds" + " " + flags.ToString("X") + " " + type.ToString("X"));
                            File.WriteAllBytes(outFolder + ModelImporter.NixIllegalCharacters(texNames[i]) + ".dds", xvr.ToArray());
                        }
                        else
                        {
                            Debug.WriteLine($"Texture{i}.dds" + " " + flags.ToString("X") + " " + type.ToString("X"));
                            File.WriteAllBytes(outFolder + $"Texture{i}.dds", xvr.ToArray());
                        }
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
            int flags = BitConverter.ToInt32(xvr, offset + 0x8);
            int type = BitConverter.ToInt32(xvr, offset + 0xC);
            ushort sizeX = BitConverter.ToUInt16(xvr, offset + 0x14);
            ushort sizeY = BitConverter.ToUInt16(xvr, offset + 0x16);
            int dataSize = BitConverter.ToInt32(xvr, offset + 0x18);

            var xvrOut = new List<byte>();
            xvrOut.AddRange(GenerateDDSHeaderNew(sizeY, sizeX, dataSize, flags, type));
            xvrOut.AddRange(xvrList.GetRange(offset + 0x40, dataSize));


            var outFileName = Path.ChangeExtension(xvrName, ".dds");

            File.WriteAllBytes(outFileName, xvrOut.ToArray());

            xvr = null;
            xvrList = null;
        }

        public static byte[] GenerateDDSHeaderNew(ushort height, ushort width, int size, int flags, int type)
        {
            int mipCount = 0;
            if((flags & (int)XVRTexFlag.Mips) > 0)
            {
                int heightMip = height;
                int widthMip = width;
                while(heightMip > 1 && widthMip > 1)
                {
                    heightMip /= 2;
                    widthMip /= 2;
                    mipCount++;
                }
                mipCount--;
            }
            //Map to DXGIFormat based on XVRFormats list, recovered from a PSO1 executable. Redundancies prevent this from being an enum
            DXGIFormat fmt = DXGIFormat.BC1UNORM;
            bool PMAlpha = false;
            switch (type)
            {
                case 11:
                case 1:
                    fmt = DXGIFormat.B8G8R8A8UNORM;
                    break;
                case 12:
                case 2:
                    fmt = DXGIFormat.B5G6R5UNORM;
                    break;
                case 13:
                case 3:
                    fmt = DXGIFormat.B5G5R5A1UNORM;
                    break;
                case 14:
                case 4:
                    fmt = DXGIFormat.B4G4R4A4UNORM;
                    break;
                case 5:
                    fmt = DXGIFormat.P8;
                    break;
                case 6:
                    fmt = DXGIFormat.BC1UNORM;
                    break;
                case 7:
                    fmt = DXGIFormat.BC2UNORM;
                    PMAlpha = true;
                    break;
                case 8:
                    fmt = DXGIFormat.BC2UNORM;
                    break;
                case 9:
                    fmt = DXGIFormat.BC3UNORM;
                    PMAlpha = true;
                    break;
                case 10:
                    fmt = DXGIFormat.BC3UNORM;
                    break;
                case 15:
                    fmt = DXGIFormat.YUY2;
                    break;
                case 16:
                    fmt = DXGIFormat.R8G8SNORM;
                    break;
                case 17:
                    fmt = DXGIFormat.A8UNORM;
                    break;
            }
            var meta = GenerateMataData(width, height, mipCount, fmt, false);
            if(PMAlpha)
            {
                meta.MiscFlags2 = TexMiscFlags2.TEXMISC2ALPHAMODEMASK;
            }
            DirectXTexUtility.GenerateDDSHeader(meta, DDSFlags.NONE, out var ddsHeader, out var dx10Header);

            List<byte> outbytes = new List<byte>(AquaGeneralMethods.ConvertStruct(ddsHeader));
            outbytes.InsertRange(0, new byte[] { 0x44, 0x44, 0x53, 0x20 });

            return outbytes.ToArray();
        }

        public static byte[] GenerateDDSHeader(ushort height, ushort width, int size, int flags, int type)
        {
            var outBytes = new List<byte>();

            outBytes.AddRange(new byte[] { 0x44, 0x44, 0x53, 0x20, 0x7C, 0x00, 0x00, 0x00, 0x07, 0x10, 0x08, 0x00 });
            outBytes.AddRange(BitConverter.GetBytes(height));
            outBytes.AddRange(new byte[] { 0x00, 0x00 });
            outBytes.AddRange(BitConverter.GetBytes(width));
            outBytes.AddRange(new byte[] { 0x00, 0x00 });
            outBytes.AddRange(BitConverter.GetBytes(size));
            outBytes.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  0x00, 0x00, 0x00, 0x00,  0x20, 0x00, 0x00, 0x00,
                0x04, 0x00, 0x00, 0x00,  0x44, 0x58, 0x54});
            if (type == 0x7)
            {
                switch (flags)
                {
                    default:
                        outBytes.Add(0x33); //3 in DXT3
                        break;
                }
            }
            else
            {
                switch (flags)
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
