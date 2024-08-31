using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.CustomRoboBattleRevolution.Model.Common
{
    public class CRBRModelDataSet
    {
        public CRBRNode rootNode = null;
        public CRBRMotionNode rootMotionNode = null;

        public int nodetreeOffset;
        public int motionNodeTreeOffset;
        public int int_08;
        public int int_0C;

        public CRBRModelDataSet() { }
        public CRBRModelDataSet(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            nodetreeOffset = sr.ReadBE<int>();
            motionNodeTreeOffset = sr.ReadBE<int>();
            int_08 = sr.ReadBE<int>();
            int_0C = sr.ReadBE<int>();

            if(nodetreeOffset != 0)
            {
                sr.Seek(nodetreeOffset + offset, SeekOrigin.Begin);
                rootNode = new CRBRNode(sr, offset);
            }

            if (nodetreeOffset != 0)
            {
                sr.Seek(motionNodeTreeOffset + offset, SeekOrigin.Begin);
                rootMotionNode = new CRBRMotionNode(sr, offset);
            }

#if DEBUG
            if(int_08 != 0)
            {
                throw new NotImplementedException();
            }

            if (int_0C != 0)
            {
                throw new NotImplementedException();
            }
#endif
        }
    }
}
