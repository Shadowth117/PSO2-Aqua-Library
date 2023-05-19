using System.Collections.Generic;

namespace AquaModelLibrary.AquaStructs
{
    public class FacialFCL : AquaCommon
    {
        public FCLHeader header;
        public List<int> unkIntList = new List<int>();
        public List<FCLFrameObject> frames = new List<FCLFrameObject>();


        public class FCLFrameObject
        {
            public FCLFrame fclFrameStruct;
            public float frameValue;
        }

        public struct FCLFrame
        {
            public float frameNumber;
            public int frameValueOffset;
        }

        public struct FCLHeader
        {
            public int frameCount;
            public float endFrame;
            public int unkIntListCount;
            public int unkIntListOffset;

            public int frameListOffset;
        }
    }
}
