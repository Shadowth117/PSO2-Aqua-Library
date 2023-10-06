using System.Collections.Generic;
using System.Diagnostics;

namespace SoulsFormats.Formats.Morpheme.MorphemeBundle
{
    public class SkeletonMap : MorphemeBundle_Base
    {

        public List<int> parentIdList = new List<int>();
        public List<int> idList = new List<int>();
        public List<string> boneNames = new List<string>();

        public SkeletonMap() { }

        public SkeletonMap(BinaryReaderEx br)
        {

        }

        public override long CalculateBundleSize()
        {
            throw new System.NotImplementedException();
        }
    }
}
