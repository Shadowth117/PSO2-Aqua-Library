using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.CustomRoboBattleRevolution.Model.Common
{
    public class CRBRMotionNode
    {
        public CRBRMotionNode childNode = null;
        public CRBRMotionNode siblingNode = null;
        public CRBRMotionKeyframeRoot keyframeRoot = null;

        public int childNodeOffset;
        public int siblingNodeOffset;
        public int keyframeRootOffset;
        public int int_0C;

        public int int_10;

        public CRBRMotionNode() { }

        public CRBRMotionNode(BufferedStreamReaderBE<MemoryStream> sr, int offset) 
        {
            childNodeOffset = sr.ReadBE<int>();
            siblingNodeOffset = sr.ReadBE<int>();
            keyframeRootOffset = sr.ReadBE<int>();
            int_0C = sr.ReadBE<int>();

            int_10 = sr.ReadBE<int>();


#if DEBUG
            if (int_0C != 0)
            {
                throw new NotImplementedException();
            }

            if (int_10 != 0)
            {
                throw new NotImplementedException();
            }
#endif

            if (childNodeOffset > 0)
            {
                sr.Seek(childNodeOffset + offset, SeekOrigin.Begin);
                childNode = new CRBRMotionNode(sr, offset);
            }

            if (siblingNodeOffset > 0)
            {
                sr.Seek(siblingNodeOffset + offset, SeekOrigin.Begin);
                siblingNode = new CRBRMotionNode(sr, offset);
            }

            if (keyframeRootOffset > 0)
            {
                sr.Seek(keyframeRootOffset + offset, SeekOrigin.Begin);
                keyframeRoot = new CRBRMotionKeyframeRoot(sr, offset);
            }
        }
    }
}
