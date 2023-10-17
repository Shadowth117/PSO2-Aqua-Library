using System.Numerics;

namespace SoulsFormats.Formats.Other.MWC
{
    public class SMD : SoulsFile<SMD>
    {
        public struct SMDHeader
        {
            public int fileSize;
            public int SMDHeaderEntryCount;
            public int unkCount0;
            public int unkCount1;

            public int headerEntryOffset;
            public int indicesOffset; //Triangle indices? Might not be tristrips
            public int dataOffset;
            public int unkCount2;

            public int unkCount3;
            public int unkCount4;
        }
        public struct SMDHeaderEntry
        {
            public int int00;
            public int int04;
            public int int08;
            public int int0C;

            public int int10;
        }

        public struct SMDDataEntry
        {
            public Vector3 vec3Data;
            public short usht0C;
            public short usht0A;
        }
    }
}
