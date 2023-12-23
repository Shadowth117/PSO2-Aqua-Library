namespace AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData
{
    public class FCMNObject : BaseCMXObject
    {
        public FCMN fcmn;
        public string proportionAnim = null;
        public string faceAnim1 = null;
        public string faceAnim2 = null;

        public string faceAnim3 = null;
        public string faceAnim4 = null;
        public string faceAnim5 = null;
        public string faceAnim6 = null;

        public string faceAnim7 = null;
        public string faceAnim8 = null;
        public string faceAnim9 = null;
        public string faceAnim10 = null;
    }

    public struct FCMN
    {
        public int id; //FF, 0x8
        public int proportionAnimPtr;
        public int faceAnim1Ptr;
        public int faceAnim2Ptr;

        public int faceAnim3Ptr;
        public int faceAnim4Ptr;
        public int faceAnim5Ptr;
        public int faceAnim6Ptr;

        public int faceAnim7Ptr;
        public int faceAnim8Ptr;
        public int faceAnim9Ptr;
        public int faceAnim10Ptr;
    }
}
