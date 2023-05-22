using AquaModelLibrary.BluePoint.CMSH;
using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }

    public class TXP3
    {
        public int magic;
        public int txp4Count;
        public int endOffset; //Sometimes something else?
        public List<int> txpOffsets = new List<int>(); //Relative to start of this TXP3

        public List<TXP4> txp4List = new List<TXP4>();

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
            if (txp3endOffsetTest == 0x0101)
            {
                streamReader.Seek(endOffset + txp3Start, System.IO.SeekOrigin.Begin);
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
