using AquaModelLibrary.Data.DataTypes.SetLengthStrings;
using AquaModelLibrary.Helpers.PSO2;

namespace AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData
{
    public class EYEBObject : BaseCMXObject
    {
        public EYEB eyeb;
        public string texString1 = null;
        public string texString2 = null;
        public string texString3 = null;

        public string texString4 = null;

        public EYEBObject() { }

        /// <summary>
        /// EYEB or EYEL
        /// </summary>
        public EYEBObject(List<Dictionary<int, object>> eyebRaw)
        {
            eyeb.id = (int)eyebRaw[0][0xFF];

            if (eyebRaw[0].ContainsKey(0x50))
            {
                texString1 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(eyebRaw[0], 0x50)).GetString();
            }
            else if (eyebRaw[0].ContainsKey(0x60))
            {
                texString1 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(eyebRaw[0], 0x60)).GetString();
            }
        }
    }

    //Also for EYEL
    public struct EYEB
    {
        public int id;
        public int texString1Ptr;
        public int texString2Ptr;
        public int texString3Ptr;

        public int texString4Ptr;
    }
}
