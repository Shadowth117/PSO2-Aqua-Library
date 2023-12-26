using AquaModelLibrary.Data.DataTypes.SetLengthStrings;
using AquaModelLibrary.Helpers.PSO2;
using System.Text;

namespace AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData
{
    public class BODYObject : BaseCMXObject
    {
        public BODY body;
        public BODYMaskColorMapping bodyMaskColorMapping;
        public BODY2 body2;
        public BODY40Cap body40cap;
        public BODY2023_1 body2023_1;
        public BODYVer2 bodyVer2;
        public string dataString = null;
        public string texString1 = null;
        public string texString2 = null;
        public string texString3 = null;
        public string texString4 = null;
        public string texString5 = null;
        public string texString6 = null;
        public string nodeString0 = null;
        public string nodeString1 = null;
        public string nodeString2 = null;

        public BODYObject() { }

        public BODYObject(List<Dictionary<int, object>> bodyRaw)
        {
            body.id = (int)bodyRaw[0][0xFF];

            dataString = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(bodyRaw[0], 0)).GetString();
            texString1 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(bodyRaw[0], 1)).GetString();
            texString2 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(bodyRaw[0], 2)).GetString();
            texString3 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(bodyRaw[0], 3)).GetString();
            texString4 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(bodyRaw[0], 4)).GetString();
            texString5 = PSO2String.GeneratePSO2String(VTBFMethods.GetObject<byte[]>(bodyRaw[0], 5)).GetString();
            //TexString6 seemingly isn't stored in vtbf? Might correct later if that's not really the case.

            body2.int_24_0x9_0x9 = VTBFMethods.GetObject<int>(bodyRaw[0], 0x9);
            body2.costumeSoundId = VTBFMethods.GetObject<int>(bodyRaw[0], 0xA);
            body2.headId = VTBFMethods.GetObject<int>(bodyRaw[0], 0xD);
            body2.legLength = VTBFMethods.GetObject<float>(bodyRaw[0], 0x8);
            body2.float_4C_0xB = VTBFMethods.GetObject<float>(bodyRaw[0], 0xB);
        }

        public static BODYObject parseCARM(List<Dictionary<int, object>> carmRaw)
        {
            return new BODYObject(carmRaw);
        }

        public static BODYObject parseCLEG(List<Dictionary<int, object>> clegRaw)
        {
            return new BODYObject(clegRaw);
        }

        public static byte[] toBODY(BODYObject body)
        {
            List<byte> outBytes = new List<byte>();

            VTBFMethods.AddBytes(outBytes, 0xFF, 0x8, BitConverter.GetBytes(body.body.id));

            string dataStr = body.dataString;
            VTBFMethods.AddBytes(outBytes, 0x00, 0x2, (byte)dataStr.Length, Encoding.UTF8.GetBytes(dataStr));
            string texStr1 = body.texString1;
            VTBFMethods.AddBytes(outBytes, 0x01, 0x2, (byte)texStr1.Length, Encoding.UTF8.GetBytes(texStr1));
            string texStr2 = body.texString2;
            VTBFMethods.AddBytes(outBytes, 0x02, 0x2, (byte)texStr2.Length, Encoding.UTF8.GetBytes(texStr2));
            string texStr3 = body.texString3;
            VTBFMethods.AddBytes(outBytes, 0x03, 0x2, (byte)texStr3.Length, Encoding.UTF8.GetBytes(texStr3));
            string texStr4 = body.texString4;
            VTBFMethods.AddBytes(outBytes, 0x04, 0x2, (byte)texStr4.Length, Encoding.UTF8.GetBytes(texStr4));
            string texStr5 = body.texString5;
            VTBFMethods.AddBytes(outBytes, 0x05, 0x2, (byte)texStr5.Length, Encoding.UTF8.GetBytes(texStr5));

            VTBFMethods.AddBytes(outBytes, 0xA, 0x8, BitConverter.GetBytes(body.body2.costumeSoundId));
            VTBFMethods.AddBytes(outBytes, 0xB, 0xA, BitConverter.GetBytes(body.body2.float_4C_0xB));
            VTBFMethods.AddBytes(outBytes, 0xC, 0x9, BitConverter.GetBytes((int)0));
            VTBFMethods.AddBytes(outBytes, 0x6, 0x6, BitConverter.GetBytes((ushort)0x2));
            VTBFMethods.AddBytes(outBytes, 0x7, 0x6, BitConverter.GetBytes((ushort)0x0));
            VTBFMethods.AddBytes(outBytes, 0x8, 0xA, BitConverter.GetBytes(body.body2.legLength));
            VTBFMethods.AddBytes(outBytes, 0x9, 0x9, BitConverter.GetBytes(body.body2.int_24_0x9_0x9));

            VTBFMethods.WriteTagHeader(outBytes, "BODY", 0x0, 0xE);

            return outBytes.ToArray();
        }
    }

    public struct BODY
    {
        public int id; //0xFF, 0x8
        public int dataStringPtr; //Name of the aqp, aqn, fltd, etc.
        public int texString1Ptr;
        public int texString2Ptr;

        public int texString3Ptr;
        public int texString4Ptr;
        public int texString5Ptr;
        public int texString6Ptr;

        public int int_20;
    }

    public struct BODYMaskColorMapping //Body struct section addition added with Ritem
    {
        public CharColorMapping redIndex;
        public CharColorMapping greenIndex;
        public CharColorMapping blueIndex;
        public CharColorMapping alphaIndex;
    }

    public struct BODY2
    {
        public int int_24_0x9_0x9;   //0x9, 0x9
        public int int_28;
        public int int_2C;           //One byte of this is a set of bitflags for parts to hide if it's outer wear. They follow the order of basewear ids.

        public int costumeSoundId;   //0xA, 0x8
        public int headId;  //0xD, 0x8 //Contains the id for a linked head piece, such as madoka's hair or bask repca's helmet. If they exist, this will be both the head part, 
        public int int_38;         //Usually -1
        public int int_3C;         //Usually -1

        public int linkedInnerId;         //Usually -1
        public int int_44;         //Usually -1
        public float legLength;   //0x8, 0xA
        public float float_4C_0xB;   //0xB, 0xA

        public float float_50;
        public float float_54;
        public float float_58;
        public float float_5C;

        public float float_60;
        public int int_64;
    }

    public struct BODY40Cap //Added 2/9 update
    {
        public float float_78;
        public float float_7C;
    }

    public struct BODY2023_1
    {
        public int nodeStrPtr_0;
        public int nodeStrPtr_1;
    }

    public struct BODYVer2
    {
        public int nodeStrPtr_2;
        public float flt_8C;
        public float flt_90;
        public float flt_94;
        public float flt_98;
    }
}
