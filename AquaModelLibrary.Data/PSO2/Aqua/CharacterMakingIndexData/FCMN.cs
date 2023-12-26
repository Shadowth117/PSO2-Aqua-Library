using AquaModelLibrary.Data.DataTypes.SetLengthStrings;
using AquaModelLibrary.Helpers.PSO2;

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

        public FCMNObject() { }

        public FCMNObject(List<Dictionary<int, object>> fcmnRaw)
        {
            fcmn.id = (int)fcmnRaw[0][0xFF];
            proportionAnim = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(fcmnRaw[0], 0xC0)).GetString();
            faceAnim1 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(fcmnRaw[0], 0xC1)).GetString();

            PSO2String[] faceAnims = new PSO2String[8];
            if (fcmnRaw.Count > 1)
            {
                for (int i = 1; i < fcmnRaw.Count; i++)
                {
                    faceAnims[i - 1] = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(fcmnRaw[0], 0xC1));
                }
            }
            faceAnim2 = faceAnims[0].GetString();
            faceAnim3 = faceAnims[1].GetString();
            faceAnim4 = faceAnims[2].GetString();
            faceAnim5 = faceAnims[3].GetString();
            faceAnim6 = faceAnims[4].GetString();
            faceAnim7 = faceAnims[5].GetString();
            faceAnim8 = faceAnims[6].GetString();
            faceAnim9 = faceAnims[7].GetString();
        }
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
