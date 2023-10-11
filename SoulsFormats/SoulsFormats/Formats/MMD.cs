using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats
{
    public class MMD : SoulsFile<MMD>
    {
        public List<ushort> faceIndices = new List<ushort>(); //Maybe? 

        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 8)
                return false;

            br.ReadInt32();
            string magic = br.GetASCII(4);
            return magic == "MMD ";
        }

        protected override void Read(BinaryReaderEx br)
        {
            br.ReadInt32();
            br.AssertASCII("MMD ");
        }

        public struct MMDHeader
        {
            public int fileSize;
            public int magic;
            public ushort usht_08;
            public ushort usht_0A;
            public int int_0C;

            public int unkCount0;
            public int faceIndexCount;
            public int unkCount1;
            public int unkCount2;

            public int meshCount;
            public int meshHeaderOffset;
            public int faceIndicesOffset;
            public int unkOffset0;

            public int unkOffset1;
            public int unkOffset2;
            public int unkCount3;
            public int unkCount4;
        }

        public struct meshHeader
        {
            public int materialId;
            public int int_04;
            public int count_08;
            public int count_0C;
        }
    }
}
