using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.Extra.AM2
{
    //All common skeletons
    public class MOT_BONE
    {
        public boneHeader header;
        public List<int> skeletonOffsets = new List<int>();
        public List<int> skeletonNameOffsets = new List<int>();
        public List<string> skeletonNames = new List<string>();

        public struct boneHeader
        {
            public int int_00;
            public int skeletonCount;
            public int skeletonOffsetsOffset;
            public int int_0C;

            public int skeletonNameOffsetsOffset;
        }

    }
}
