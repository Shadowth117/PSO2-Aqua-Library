using AquaModelLibrary.Helpers.PSO2;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaEffectData
{
    //Seems to have varying pointer data, but no actual data
    public struct ANIM { }

    public class CURVObject
    {
        public CURV curv;
        public List<KEYS> keys = new List<KEYS>();
        public List<float> times = new List<float>();

        public CURVObject() { }

        public CURVObject(List<Dictionary<int, object>> curvRaw)
        {
            curv = new CURV();

            curv.type = VTBFMethods.GetObject<byte>(curvRaw[0], 0x71);
            curv.startFrame = VTBFMethods.GetObject<float>(curvRaw[0], 0x74);
            curv.int_0C = VTBFMethods.GetObject<short>(curvRaw[0], 0x73);
            curv.float_10 = VTBFMethods.GetObject<float>(curvRaw[0], 0x77);

            curv.int_14 = VTBFMethods.GetObject<int>(curvRaw[0], 0x76);
            curv.endFrame = VTBFMethods.GetObject<float>(curvRaw[0], 0x75);
        }
    }
    public struct CURV
    {
        public int type; //0x70, Type 0x4 //0x71, Type 0x4 //71 possibly combined with this? Never observed filled so unsure.
        public float startFrame; //0x74, Type 0xA //Probably start frame. It's this or float_10;
        public int int_0C; //0x73, Type 0x6
        public float float_10; //0x77, Type 0xA

        public int int_14; //0x76, Type 0x9 
        public float endFrame; //0x75, Type 0xA
        public int keysOffset;
        public int keysCount;

        public int timeOffset;
        public int timeCount;
    }
}
