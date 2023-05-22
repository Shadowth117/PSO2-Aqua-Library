using AquaModelLibrary.AquaMethods;
using AquaModelLibrary.BluePoint.CMSH;
using DirectXTex;
using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AquaModelLibrary.PSOXVMConvert;
using static DirectXTex.DirectXTexUtility;

namespace AquaModelLibrary.Extra.AM2
{
    public enum TXPType : int
    {
        BC1 = 0x6,
        BC5U = 0xB,
    }

    public class TXP
    {

        public static int TXP4Magic = 0x4505854;
        public static int TXP3Magic = 0x3505854;
        public static int TXP2Magic = 0x2505854;
        public TXP3 txp3 = new TXP3();

        
        public TXP()
        {

        }
        public TXP(BufferedStreamReader streamReader)
        {
            //TXPs start with TXP3, go to TXP4, then TXP2 (No typo, 3, 4, 2)
            //TXP3s reference TXP4s and are essentially texture containers. TXP4s reference mipmaps for a particular texture. TXP2s are the various mip containers.
            txp3 = new TXP3(streamReader);
        }
    }

    public class TXP4
    {
        public int magic;
        public int txp2Count;
        public int endOffset;
        public List<int> mipMapAddresses = new List<int>(); //Relative to start of this TXP4

        public List<TXP2> txp2List = new List<TXP2>();

        public TXP4()
        {

        }
        public TXP4(BufferedStreamReader streamReader)
        {
            var txp4Start = streamReader.Position();

            magic = streamReader.Read<int>();
            txp2Count = streamReader.Read<int>();
            endOffset = streamReader.Read<int>();
            streamReader.Seek(-2, System.IO.SeekOrigin.Current);
            var txp4endOffsetTest = streamReader.Read<ushort>();

            for (int i = 0; i < txp2Count; i++)
            {
                var offset = streamReader.Read<int>();
                mipMapAddresses.Add(offset);
            }
            for (int i = 0; i < txp2Count; i++)
            {
                streamReader.Seek(mipMapAddresses[i] + txp4Start, System.IO.SeekOrigin.Begin);
                txp2List.Add(new TXP2(streamReader));
            }


            //Some files don't list a proper end offset and we don't want to try to seek through these. Otherwise, we do to get the names
            if (txp4endOffsetTest == 0x0101)
            {
                streamReader.Seek(endOffset + txp4Start, System.IO.SeekOrigin.Begin);
            }
        }

        public byte[] GetDDS()
        {
            var outBytes = new List<byte>();
            outBytes.AddRange(GenerateDDSHeader());
            foreach(var mip in txp2List)
            {
                outBytes.AddRange(mip.buffer);
            }

            return outBytes.ToArray();
        }
        public byte[] GenerateDDSHeader()
        {
            var mip0 = txp2List[0];
            //Map to DXGIFormat based on XVRFormats list, recovered from a PSO1 executable. Redundancies prevent this from being an enum
            DXGIFormat fmt = DXGIFormat.BC1UNORM;
            bool PMAlpha = false;
            bool isDX10 = false;
            switch (mip0.txpType) //Technically mips could have different types, but HOPEFULLY Sega isn't insane
            {
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
                case 109:
                case 10:
                    fmt = DXGIFormat.BC3UNORM;
                    break;
                case 11:
                    fmt = DXGIFormat.BC5UNORM;
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
                case 60:
                    fmt = DXGIFormat.R8G8B8A8UNORMSRGB;
                    isDX10 = true;
                    break;
                case 130:
                    fmt = DXGIFormat.BC7UNORM;
                    isDX10 = true;
                    break;
            }
            var meta = GenerateMataData(mip0.width, mip0.height, txp2Count, fmt, false);
            if (PMAlpha)
            {
                meta.MiscFlags2 = TexMiscFlags2.TEXMISC2ALPHAMODEMASK;
            }
            DirectXTexUtility.GenerateDDSHeader(meta, DDSFlags.NONE, out var ddsHeader, out var dx10Header);

            List<byte> outbytes = new List<byte>(AquaGeneralMethods.ConvertStruct(ddsHeader));
            if(isDX10)
            {
                outbytes.AddRange(AquaGeneralMethods.ConvertStruct(dx10Header));
            }
            outbytes.InsertRange(0, new byte[] { 0x44, 0x44, 0x53, 0x20 });

            return outbytes.ToArray();
        }
    }

    public class TXP3
    {
        public int magic;
        public int txp4Count;
        public int endOffset; //Sometimes something else?
        public List<int> txpOffsets = new List<int>(); //Relative to start of this TXP3

        public List<TXP4> txp4List = new List<TXP4>();
        public List<long> texNameOffsets = new List<long>();
        public List<string> txp4Names = new List<string>(); //For sprite sets, since some are spliced out, these aren't used

        public TXP3()
        {

        }

        public TXP3(BufferedStreamReader streamReader)
        {
            var txp3Start = streamReader.Position();

            magic = streamReader.Read<int>();
            txp4Count = streamReader.Read<int>();
            endOffset = streamReader.Read<int>();
            streamReader.Seek(-2, System.IO.SeekOrigin.Current);
            var txp3endOffsetTest = streamReader.Read<ushort>();
            
            for(int i = 0; i < txp4Count; i++)
            {
                var offset = streamReader.Read<int>();
                txpOffsets.Add(offset);
            }
            for (int i = 0; i < txp4Count; i++)
            {
                streamReader.Seek(txpOffsets[i] + txp3Start, System.IO.SeekOrigin.Begin);
                txp4List.Add(new TXP4(streamReader));
            }

            //Some files don't list a proper end offset and we don't want to try to seek through these. Otherwise, we do to get the names
            //Probably left out for sprite sheets since the sprite images have their own references and the textures themselves don't need them
            if (txp3endOffsetTest != 0x0101)
            {
                streamReader.Seek(endOffset + txp3Start, System.IO.SeekOrigin.Begin);
                for(int i = 0; i < txp4Count; i++)
                {
                    var texNameOffset = streamReader.Read<long>();
                    texNameOffsets.Add(texNameOffset);
                }
                for (int i = 0; i < txp4Count; i++)
                {
                    streamReader.Seek(texNameOffsets[i] + txp3Start, System.IO.SeekOrigin.Begin);
                    txp4Names.Add(AquaGeneralMethods.ReadCString(streamReader)); ;
                }
            }
        }
    }

    public class TXP2
    {
        public int magic;
        public int width;
        public int height;
        public int txpType;

        public int int_10;
        public int bufferSize;

        public byte[] buffer;

        public TXP2()
        {

        }

        public TXP2(BufferedStreamReader streamReader)
        {
            magic = streamReader.Read<int>();
            width = streamReader.Read<int>();
            height = streamReader.Read<int>();
            txpType = streamReader.Read<int>();
            
            int_10 = streamReader.Read<int>();
            bufferSize = streamReader.Read<int>();
            buffer = streamReader.ReadBytes(streamReader.Position(), bufferSize);
        }
    }

}
