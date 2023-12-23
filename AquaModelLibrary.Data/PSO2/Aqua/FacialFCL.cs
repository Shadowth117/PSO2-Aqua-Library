using AquaModelLibrary.Data.PSO2.Aqua.FCLData;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    public class FacialFCL : AquaCommon
    {
        public FCLHeader header;
        public List<int> unkIntList = new List<int>();
        public List<FCLFrameObject> frames = new List<FCLFrameObject>();
    }
}
