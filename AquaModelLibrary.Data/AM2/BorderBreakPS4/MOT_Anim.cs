using AquaModelLibrary.Helpers.Readers;
using Half = AquaModelLibrary.Data.DataTypes.Half;

namespace AquaModelLibrary.Data.AM2.BorderBreakPS4
{
    public class MOT_Anim
    {
        public motionHeader header;
        private List<int> motionNameOffsets = new List<int>();
        public List<string> motionNames = new List<string>();

        private List<int> motionOffsetList = new List<int>();
        private List<motionDataHeader> motionDataHeaderList = new List<motionDataHeader>();
        public List<Motion> motionList = new List<Motion>();

        public struct motionHeader
        {
            public int int_00;
            public int motionCount;
            public int motionOffsets;
            public int int_0C;

            public int motionNameOffsets;
        }

        public struct motionDataHeader
        {
            public int int_00;
            public int int_04;
            public int offset0ByteCount;
            public int reserve0;

            public int offset0;
            public int reserve1;
            public int offset1HalfFloatCount;
            public int reserve2;

            public int offset1;
            public int reserve3;
            public int reserve4;
            public int reserve5;
        }

        public class Motion
        {
            public motionDataHeader header;
            public List<byte> btList = new List<byte>();
            public List<Half> halfList = new List<Half>();
        }

        public MOT_Anim()
        {

        }

        public MOT_Anim(BufferedStreamReaderBE<MemoryStream> streamReader)
        {
            header = streamReader.Read<motionHeader>();

            //Read motion names
            streamReader.Seek(header.motionNameOffsets, SeekOrigin.Begin);
            for (int i = 0; i < header.motionCount; i++)
            {
                motionNameOffsets.Add(streamReader.Read<int>());
                streamReader.Seek(4, SeekOrigin.Current);
            }
            foreach (var nameOffset in motionNameOffsets)
            {
                streamReader.Seek(nameOffset, SeekOrigin.Begin);
                motionNames.Add(streamReader.ReadCString());
            }

            //Read motions
            streamReader.Seek(header.motionOffsets, SeekOrigin.Begin);
            for (int i = 0; i < header.motionCount; i++)
            {
                motionOffsetList.Add(streamReader.Read<int>());
                streamReader.Seek(4, SeekOrigin.Current);
            }
            foreach (var offset in motionOffsetList)
            {
                streamReader.Seek(offset, SeekOrigin.Begin);
                motionDataHeaderList.Add(streamReader.Read<motionDataHeader>());
            }

            foreach (var motHead in motionDataHeaderList)
            {
                Motion mot = new Motion();
                mot.header = motHead;

                //Bytes, correlates slightly to boneCount, give or take some. Probably has extra 'nodes' for controlling various things.
                mot.btList = streamReader.ReadBytes(motHead.offset0, motHead.offset0ByteCount).ToList();

                //Halves, seemingly frame data? Some data may be a different data type. Unclear at a glance how to use
                streamReader.Seek(motHead.offset1, SeekOrigin.Begin);
                List<Half> halves = new List<Half>();
                for (int h = 0; h < motHead.offset1HalfFloatCount; h++)
                {
                    halves.Add(streamReader.Read<Half>());
                }
                mot.halfList = halves;

                motionList.Add(mot);
            }
        }
    }
}
