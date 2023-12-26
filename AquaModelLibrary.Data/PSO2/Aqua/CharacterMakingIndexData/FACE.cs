using AquaModelLibrary.Data.DataTypes.SetLengthStrings;
using AquaModelLibrary.Helpers.PSO2;

namespace AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData
{
    public class FACEObject : BaseCMXObject
    {
        public FACE face;
        public FACERitem faceRitem;
        public FACE2 face2;
        public float unkFloatRitem;
        public int unkVer2Int;

        public string dataString = null;
        public string texString1 = null;
        public string texString2 = null;

        public string texString3 = null;
        public string texString4 = null;
        public string texString5 = null;
        public string texString6 = null;

        public FACEObject() { }

        public FACEObject(List<Dictionary<int, object>> faceRaw)
        {
            face.id = (int)faceRaw[0][0xFF];

            dataString = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(faceRaw[0], 0x70)).GetString();
            face2.unkInt3 = VTBFMethods.GetObject<int>(faceRaw[0], 0x71);
            texString1 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(faceRaw[0], 0x72)).GetString();
            texString2 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(faceRaw[0], 0x73)).GetString();
            texString3 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(faceRaw[0], 0x74)).GetString();
            texString4 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(faceRaw[0], 0x75)).GetString();
            texString5 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(faceRaw[0], 0x76)).GetString();

            //0x77 seemingly isn't parsed
            //0x78 seemingly isn't parsed
            face2.unkInt5 = VTBFMethods.GetObject<int>(faceRaw[0], 0x79);
        }
    }

    public struct FACE
    {
        public int id; //0xFF, 0x8
        public int dataStringPtr; //Name of the aqp, aqn, fltd, etc.
        public int texString1Ptr;
        public int texString2Ptr;

        public int texString3Ptr;
        public int texString4Ptr;
        public int texString5Ptr;
        public int texString6Ptr;
    }

    public struct FACERitem
    {
        public int unkIntRT0;
        public int unkIntRT1;
    }

    public struct FACE2
    {
        public int unkInt0;
        public int unkInt1;
        public int unkInt2;
        public int unkInt3;

        public int unkInt4;
        public float unkFloat0;
        public float unkFloat1;
        public float unkFloat2;

        public float unkFloat3;
        public int unkInt5;
        public int unkInt6;
        public int unkInt7;

        public int unkInt8;
        public int unkInt9;
        public int unkInt10;
        public int unkInt11;
    }
}
