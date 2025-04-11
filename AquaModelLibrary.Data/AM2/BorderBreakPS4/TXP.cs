﻿using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers;
using static DirectXTex.DirectXTexUtility;
using DirectXTex;

namespace AquaModelLibrary.Data.AM2.BorderBreakPS4
{
    public enum TXPType : int
    {
        BC1 = 0x6,
        BC5U = 0xB,
    }

    public class TXP
    {
        //Border Break PS4
        public const int TXP4Magic = 0x4505854;
        public const int TXP3Magic = 0x3505854;
        public const int TXP2Magic = 0x2505854;

        //Afterburner Climax
        public const int TXP13Magic = 0x13505854;
        public const int TXP1Magic = 0x31505854;
        public const int TEX1Magic = 0x31584554;

        public TXP3 txp3 = null;

        public TXP1 txp1 = null;

        public TXP()
        {

        }
        public TXP(BufferedStreamReaderBE<MemoryStream> streamReader)
        {
            var magic = streamReader.Peek<int>();

            switch(magic)
            {
                case TXP1Magic:
                    txp1 = new TXP1(streamReader);
                    break;
                case TXP13Magic:
                case TXP3Magic:
                    //Border Break TXPs start with TXP3, go to TXP4, then TXP2 (No typo, 3, 4, 2)
                    //TXP3s reference TXP4s and are essentially texture containers. TXP4s reference mipmaps for a particular texture. TXP2s are the various mip containers.
                    //In AfterBurner Climax, this same pattern is followed, but the magic is instead TXP13 when not on sprites
                    txp3 = new TXP3(streamReader);
                    break;

            }
        }
    }

    public class TXP1
    {
        public long position;
        public int magic;
        /// <summary>
        /// Names are NOT unique!!!
        /// </summary>
        public List<string> texStrings = new List<string>();
        public List<TEX1> textures = new List<TEX1>();
        public TXP1() { }
        public TXP1(BufferedStreamReaderBE<MemoryStream> streamReader)
        {
            position = streamReader.Position;
            magic = streamReader.Read<int>();
            int tex1Count = streamReader.Read<int>();
            List<int> texOffsets = new List<int>();
            List<int> texSizes = new List<int>();

            for(int i = 0; i < tex1Count; i++)
            {
                texOffsets.Add(streamReader.Read<int>());
                texSizes.Add(streamReader.Read<int>());
                texStrings.Add(streamReader.ReadCStringSeek(8));
            }
            for(int i = 0; i < texOffsets.Count; i++)
            {
                streamReader.Seek(position + texOffsets[i], SeekOrigin.Begin);
                textures.Add(new TEX1(streamReader));
            }
        }
    }

    public class TEX1
    {
        public long position;
        public int magic;
        public int pixelFormat;
        public int width;
        public int height;

        public int mipCount;
        public List<int> dataOffsets = new List<int>();
        public List<int> bufferSizes = new List<int>();
        public int reserve0;

        public List<byte[]> mipBuffers = new List<byte[]>();

        public TEX1() { }
        public TEX1(BufferedStreamReaderBE<MemoryStream> streamReader)
        {
            position = streamReader.Position;
            magic = streamReader.Read<int>();
            pixelFormat = streamReader.Read<int>();
            width = streamReader.Read<int>();
            height = streamReader.Read<int>();
            
            mipCount = streamReader.Read<int>();
            for(int i = 0; i < mipCount; i++)
            {
                dataOffsets.Add(streamReader.Read<int>());
                bufferSizes.Add(streamReader.Read<int>());
            }
            reserve0 = streamReader.Read<int>();
            for(int i = 0; i < dataOffsets.Count; i++)
            {
                mipBuffers.Add(streamReader.ReadBytes(position + dataOffsets[i], bufferSizes[i]));
            }
        }

        public byte[] GetDDS()
        {
            var outBytes = new List<byte>();
            outBytes.AddRange(GenerateDDSHeader());
            foreach(var buffer in mipBuffers)
            {
                outBytes.AddRange(buffer);
            }

            return outBytes.ToArray();
        }
        private List<byte> GenerateDDSHeader()
        {
            var fmt = DXGIFormat.R8G8B8A8UNORM;
            bool PMAlpha = false;
            bool isDX10 = false;
            switch (pixelFormat) 
            {
                case 1:
                    fmt = DXGIFormat.B8G8R8A8UNORM;
                    break;
                case 12:
                case 2:
                    fmt = DXGIFormat.R8G8B8A8UNORM;
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
                    fmt = DXGIFormat.BC3UNORM;
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
            var meta = GenerateMetaData(width, height, mipBuffers.Count, fmt, false);
            DirectXTexUtility.GenerateDDSHeader(meta, DirectXTexUtility.DDSFlags.NONE, out var ddsHeader, out var dx10Header, false);

            List<byte> outbytes = new List<byte>(DataHelpers.ConvertStruct(ddsHeader));
            outbytes.InsertRange(0, new byte[] { 0x44, 0x44, 0x53, 0x20 });
            return outbytes;
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
        public TXP4(BufferedStreamReaderBE<MemoryStream> streamReader)
        {
            var txp4Start = streamReader.Position;

            magic = streamReader.Read<int>();
            txp2Count = streamReader.Read<int>();
            endOffset = streamReader.Read<int>();
            streamReader.Seek(-2, SeekOrigin.Current);
            var txp4endOffsetTest = streamReader.Read<ushort>();

            for (int i = 0; i < txp2Count; i++)
            {
                var offset = streamReader.Read<int>();
                mipMapAddresses.Add(offset);
            }
            for (int i = 0; i < txp2Count; i++)
            {
                streamReader.Seek(mipMapAddresses[i] + txp4Start, SeekOrigin.Begin);
                txp2List.Add(new TXP2(streamReader));
            }


            //Some files don't list a proper end offset and we don't want to try to seek through these. Otherwise, we do to get the names
            if (txp4endOffsetTest == 0x0101)
            {
                streamReader.Seek(endOffset + txp4Start, SeekOrigin.Begin);
            }
        }

        public byte[] GetDDS()
        {
            var outBytes = new List<byte>();
            outBytes.AddRange(GenerateDDSHeader());
            foreach (var mip in txp2List)
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
            var meta = GenerateMetaData(mip0.width, mip0.height, txp2Count, fmt, false);
            if (PMAlpha)
            {
                meta.MiscFlags2 = TexMiscFlags2.TEXMISC2ALPHAMODEMASK;
            }
            DirectXTexUtility.GenerateDDSHeader(meta, DDSFlags.NONE, out var ddsHeader, out var dx10Header, false);

            List<byte> outbytes = new List<byte>(DataHelpers.ConvertStruct(ddsHeader));
            if (isDX10)
            {
                outbytes.AddRange(DataHelpers.ConvertStruct(dx10Header));
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

        public TXP3(BufferedStreamReaderBE<MemoryStream> streamReader)
        {
            var txp3Start = streamReader.Position;

            magic = streamReader.Read<int>();
            txp4Count = streamReader.Read<int>();
            endOffset = streamReader.Read<int>();
            streamReader.Seek(-2, SeekOrigin.Current);
            var txp3endOffsetTest = streamReader.Read<ushort>();

            for (int i = 0; i < txp4Count; i++)
            {
                var offset = streamReader.Read<int>();
                txpOffsets.Add(offset);
            }
            for (int i = 0; i < txp4Count; i++)
            {
                streamReader.Seek(txpOffsets[i] + txp3Start, SeekOrigin.Begin);
                txp4List.Add(new TXP4(streamReader));
            }

            //Some files don't list a proper end offset and we don't want to try to seek through these. Otherwise, we do to get the names
            //Probably left out for sprite sheets since the sprite images have their own references and the textures themselves don't need them
            if (magic != TXP.TXP13Magic &&  txp3endOffsetTest != 0x0101)
            {
                streamReader.Seek(endOffset + txp3Start, SeekOrigin.Begin);
                for (int i = 0; i < txp4Count; i++)
                {
                    var texNameOffset = streamReader.Read<long>();
                    texNameOffsets.Add(texNameOffset);
                }
                for (int i = 0; i < txp4Count; i++)
                {
                    streamReader.Seek(texNameOffsets[i] + txp3Start, SeekOrigin.Begin);
                    txp4Names.Add(streamReader.ReadCString()); ;
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

        public byte[]? buffer;

        public TXP2()
        {

        }

        public TXP2(BufferedStreamReaderBE<MemoryStream> streamReader)
        {
            magic = streamReader.Read<int>();
            width = streamReader.Read<int>();
            height = streamReader.Read<int>();
            txpType = streamReader.Read<int>();

            int_10 = streamReader.Read<int>();
            bufferSize = streamReader.Read<int>();
            buffer = streamReader.ReadBytes(streamReader.Position, bufferSize);
        }
    }

}
