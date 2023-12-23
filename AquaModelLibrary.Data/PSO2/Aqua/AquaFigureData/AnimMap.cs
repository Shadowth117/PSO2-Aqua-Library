namespace AquaModelLibrary.Data.PSO2.Aqua.AquaFigureData
{
    public class AnimMapObject
    {
        public AnimMapStruct animStruct;
        public string name = null;
        public string followUp = null;
        public string type = null;
        public string anim = null;
        public List<AnimFrameInfo> frameInfoList = new List<AnimFrameInfo>();
    }

    public struct AnimMapStruct
    {
        public int namePtr;
        public int followUpPtr;
        public int typePtr;
        public int animPtr;

        public float flt_10;
        public float flt_14;
        public float flt_18;
        public float flt_1C;

        public int int_20;
        public int frameInfoPtr;
        public int frameInfoPtrCount;
    }

    //Seemingly, the ones with default frames are set specially somehow. -1, -1 may play on transition.
    public struct AnimFrameInfo
    {
        public float startFrame; //-1 if default
        public float endFrame;   //9999 if default
        public int effectId;
    }
}
