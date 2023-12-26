using AquaModelLibrary.Data.DataTypes.SetLengthStrings;
using AquaModelLibrary.Helpers.PSO2;

namespace AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData
{
    public class FCPObject : BaseCMXObject
    {
        public FCP fcp;
        public string texString1 = null;
        public string texString2 = null;
        public string texString3 = null;

        public string texString4 = null;

        public FCPObject() { }

        public FCPObject(List<Dictionary<int, object>> fcp1Raw)
        {
            fcp.id = (int)fcp1Raw[0][0xFF];

            if (fcp1Raw[0].ContainsKey(0x80))
            {
                texString1 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(fcp1Raw[0], 0x80)).GetString();
                texString2 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(fcp1Raw[0], 0x81)).GetString();
                texString3 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(fcp1Raw[0], 0x82)).GetString();
                texString4 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(fcp1Raw[0], 0x83)).GetString();
            }
            else if(fcp1Raw[0].ContainsKey(0x90))
            {
                texString1 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(fcp1Raw[0], 0x90)).GetString();
                texString2 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(fcp1Raw[0], 0x92)).GetString();
                texString3 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(fcp1Raw[0], 0x93)).GetString();
            }
        }
    }

    public struct FCP
    {
        public int id;
        public int texString1Ptr;
        public int texString2Ptr;
        public int texString3Ptr;

        public int texString4Ptr;
        public int unkInt0;
        public int unkInt1;
        public int unkInt2;

        public int unkInt3;
        public int unkInt4;
        public int unkInt5;
    }
}
