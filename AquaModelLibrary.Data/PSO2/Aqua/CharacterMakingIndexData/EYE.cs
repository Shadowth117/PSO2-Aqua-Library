using AquaModelLibrary.Data.DataTypes.SetLengthStrings;
using AquaModelLibrary.Helpers.PSO2;

namespace AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData
{
    public class EYEObject : BaseCMXObject
    {
        public EYE eye;
        public string texString1 = null;
        public string texString2 = null;
        public string texString3 = null;

        public string texString4 = null;
        public string texString5 = null;

        public EYEObject() { }

        public EYEObject(List<Dictionary<int, object>> eyeRaw)
        {
            eye.id = (int)eyeRaw[0][0xFF];

            texString1 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(eyeRaw[0], 0x40)).GetString();
            texString2 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(eyeRaw[0], 0x42)).GetString();
            texString3 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(eyeRaw[0], 0x43)).GetString();
            texString4 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(eyeRaw[0], 0x44)).GetString();
        }
    }

    public struct EYE
    {
        public int id;
        public int texString1Ptr;
        public int texString2Ptr;
        public int texString3Ptr;

        public int texString4Ptr;
        public int texString5Ptr;
        public float unkFloat0;
        public float unkFloat1;
    }
}
