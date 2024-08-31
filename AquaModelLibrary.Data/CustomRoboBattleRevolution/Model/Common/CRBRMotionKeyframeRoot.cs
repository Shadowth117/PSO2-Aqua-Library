using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.CustomRoboBattleRevolution.Model.Common
{
    public class CRBRMotionKeyframeRoot
    {
        public List<CRBRKeyframe> keyFrames = new List<CRBRKeyframe>();

        public int int_00;
        public float ft_04;
        public int keyFrameOffset;
        public int int_0C;

        public CRBRMotionKeyframeRoot() { }

        public CRBRMotionKeyframeRoot(BufferedStreamReaderBE<MemoryStream> sr, int offset) 
        {
            int_00 = sr.ReadBE<int>();
            ft_04 = sr.ReadBE<float>();
            keyFrameOffset = sr.ReadBE<int>();
            int_0C = sr.ReadBE<int>();

            var tempKfOffset = keyFrameOffset;
            do
            {
                if(tempKfOffset != 0)
                {
                    sr.Seek(tempKfOffset + offset, SeekOrigin.Begin);
                    var kf = new CRBRKeyframe(sr, offset);
                    keyFrames.Add(kf);
                    tempKfOffset = kf.nextFrameOffset;
                }
            } while (tempKfOffset != 0);
        }
    }
}
