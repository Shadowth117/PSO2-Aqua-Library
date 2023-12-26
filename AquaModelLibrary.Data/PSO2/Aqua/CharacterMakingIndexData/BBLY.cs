using AquaModelLibrary.Data.DataTypes.SetLengthStrings;
using AquaModelLibrary.Helpers.PSO2;
using System.Text;

namespace AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData
{
    //BBLY, BDP1
    public class BBLYObject : BaseCMXObject
    {
        public BBLY bbly;
        public string texString1 = null;
        public string texString2 = null;
        public string texString3 = null;

        public string texString4 = null;
        public string texString5 = null;

        public BBLYObject() { }

        //NIFL cmx lumps BDP1 with BBLY
        public BBLYObject(List<Dictionary<int, object>> bblyRaw)
        {
            bbly.id = (int)bblyRaw[0][0xFF];

            texString1 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(bblyRaw[0], 0x10)).GetString();
            texString2 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(bblyRaw[0], 0x11)).GetString();
            texString3 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(bblyRaw[0], 0x12)).GetString();
            texString4 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(bblyRaw[0], 0x13)).GetString();
        }

        public byte[] GetBytes()
        {
            List<byte> outBytes = new List<byte>();

            VTBFMethods.AddBytes(outBytes, 0xFF, 0x8, BitConverter.GetBytes(bbly.id));

            string texStr1 = texString1;
            VTBFMethods.AddBytes(outBytes, 0x01, 0x2, (byte)texStr1.Length, Encoding.UTF8.GetBytes(texStr1));
            string texStr2 = texString2;
            VTBFMethods.AddBytes(outBytes, 0x02, 0x2, (byte)texStr2.Length, Encoding.UTF8.GetBytes(texStr2));
            string texStr3 = texString3;
            VTBFMethods.AddBytes(outBytes, 0x03, 0x2, (byte)texStr3.Length, Encoding.UTF8.GetBytes(texStr3));
            string texStr4 = texString4;
            VTBFMethods.AddBytes(outBytes, 0x04, 0x2, (byte)texStr4.Length, Encoding.UTF8.GetBytes(texStr4));
            string texStr5 = texString5;
            VTBFMethods.AddBytes(outBytes, 0x05, 0x2, (byte)texStr5.Length, Encoding.UTF8.GetBytes(texStr5));

            VTBFMethods.WriteTagHeader(outBytes, "BBLY", 0x0, 0xE);

            return outBytes.ToArray();
        }
    }

    public struct BBLY
    {
        public int id; //0xFF, 0x8
        public int texString1Ptr;
        public int texString2Ptr;
        public int texString3Ptr;

        public int texString4Ptr;
        public int texString5Ptr;
        public int unkInt0;       //This value forward may just be junk or new functionality in this version. Old BBLY only had the id and texture strings. Seems to correlate to BODY stuff though.
        public int unkInt1;

        public int unkInt2;
        public int unkInt3;
        public float unkFloat0;
        public float unkFloat1;

        public int unkInt4;
        public int unkInt5;
        public float unkFloat2;
        public float unkFloat3;

        public float unkFloat4;
        public float unkFloat5;
    }
}
