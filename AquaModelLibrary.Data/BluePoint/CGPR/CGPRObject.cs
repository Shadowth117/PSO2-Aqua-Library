using AquaModelLibrary.Helpers.Readers;
using System.Numerics;
using System.Text;

namespace AquaModelLibrary.Data.BluePoint.CAWS
{
    public enum CGPRMagic : uint
    {
        xC1A69458 = 0x5894A6C1,
        xFAE88582 = 0x8285E8FA,
    }
    public abstract class CGPRObject
    {
        public uint magic;

        //Ending bytes
        public int int_SemiFinal;
        public int int_Final;
        public byte bt_Final;

        public CGPRObject()
        {

        }

        public CGPRObject(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Read<uint>();
        }
    }

    public struct cgprCommonHeader
    {
        public byte bt_0;
        public byte bt_1;
        public byte bt_2;
        public byte bt_3;
        public byte length;
    }
    /*
    public class _00010000_Object : CGPRObject
    {
        public byte extraByte;
        public byte stringLength;
        public string data = null;

        public _00010000_Object()
        {

        }

        public _00010000_Object(BufferedStreamReader sr)
        {
            magic = sr.Read<uint>();
            extraByte = sr.Read<byte>();
            int_04 = sr.Read<int>();
            int_08 = sr.Read<int>();
            int_0C = sr.Read<int>();

            int_10 = sr.Read<int>();
            stringLength = sr.Read<byte>();
            if(stringLength == 0)
            {
                sr.Seek(-1, System.IO.SeekOrigin.Current);
            } else
            {
                data = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position(), stringLength));
                sr.Seek(stringLength, System.IO.SeekOrigin.Current);
            }
            int_SemiFinal = sr.Read<int>();
            int_Final = sr.Read<int>();
        }
    }*/

    //Used commonly for CMDLs
    public class _FAE88582_Object : CGPRObject
    {
        public cgprCommonHeader mainHeader;
        public byte bt_mainTest;
        public cgprCommonHeader subHeader0;
        public byte bt_sub0Test0;
        public byte bt_sub0Test1;

        public byte bt_testPad; //Optional inclusion? appears in most of these anyways as 0
        public cgprCommonHeader subHeader1;
        public byte bt_sub1Test0;
        public byte bt_sub1_0;
        public byte bt_sub1_1;

        public cgprCommonHeader subHeader2; //Should be same first 4 bytes as mainHeader
        public byte bt_sub2Test0;

        public byte bt_18;
        public byte bt_19;
        public byte stringLengthPlus1;
        public byte stringLength;

        public int postStrInt_00;
        public int postStrInt_04;
        public int postStrInt_08;
        public int postStrInt_0C;

        public int postStrInt_10;
        public byte postStrBt_14;

        public int postStrInt_15;
        public int postStrInt_19;
        public int postStrInt_1D;
        public int postStrInt_21;

        public int postStrInt_25;

        public string cmdlPath = null;

        public _FAE88582_Object()
        {

        }

        public _FAE88582_Object(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Peek<uint>();
            mainHeader = sr.Read<cgprCommonHeader>();
            var byteTest = sr.Peek<byte>();
            if (byteTest == 0x1)
            {
                bt_mainTest = sr.Read<byte>();
            }
            subHeader0 = sr.Read<cgprCommonHeader>();
            byteTest = sr.Peek<byte>();
            if (byteTest == 0x1)
            {
                bt_sub0Test0 = sr.Read<byte>();
            }
            byteTest = sr.Peek<byte>();
            if (byteTest == 0x1)
            {
                bt_sub0Test1 = sr.Read<byte>();
            }
            bt_testPad = sr.Read<byte>();

            subHeader1 = sr.Read<cgprCommonHeader>();
            byteTest = sr.Peek<byte>();
            if (byteTest == 0x1)
            {
                bt_sub1Test0 = sr.Read<byte>();
            }
            bt_sub1_0 = sr.Read<byte>();
            bt_sub1_1 = sr.Read<byte>();

            subHeader2 = sr.Read<cgprCommonHeader>();
            byteTest = sr.Peek<byte>();
            if (byteTest == 0x1)
            {
                bt_sub2Test0 = sr.Read<byte>();
            }
            stringLengthPlus1 = sr.Read<byte>();
            stringLength = sr.Read<byte>();
            cmdlPath = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position, stringLength));
            sr.Seek(stringLength, System.IO.SeekOrigin.Current);

            postStrInt_00 = sr.Read<int>();
            postStrInt_04 = sr.Read<int>();
            postStrInt_08 = sr.Read<int>();
            postStrInt_0C = sr.Read<int>();

            postStrInt_10 = sr.Read<int>();
            postStrBt_14 = sr.Read<byte>();

            postStrInt_15 = sr.Read<int>();
            postStrInt_19 = sr.Read<int>();
            postStrInt_1D = sr.Read<int>();
            postStrInt_21 = sr.Read<int>();

            postStrInt_25 = sr.Read<int>();

            int_SemiFinal = sr.Read<int>();
            int_Final = sr.Read<int>();
        }
    }

    //Often at the start of CGPRs, sometimes spread throughout
    public class _C1A69458_Object : CGPRObject
    {
        public cgprCommonHeader mainHeader;
        public cgprCommonHeader subHeader0;
        public byte type0Flag; //Default of 0x1. 0x2 signifies a trailing string
        public byte type1Flag;

        public cgprCommonHeader subHeader1;
        public byte vecFlag0; //Default of 0x1. 0x2 signifies an extra vector3
        public byte vecFlag1;

        public cgprCommonHeader vec3_0Header;
        public Vector3 vec3_0_0;

        public cgprCommonHeader vec3_1SetHeader;
        public Vector3 vec3_0;
        public Vector3 vec3_1;
        public Vector3 vec3_2;
        public Vector3 vec3_3;

        public cgprCommonHeader stringHeader;
        public byte dataStringLength;
        public string dataString;

        public _C1A69458_Object()
        {

        }

        public _C1A69458_Object(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Peek<uint>();
            mainHeader = sr.Read<cgprCommonHeader>();
            subHeader0 = sr.Read<cgprCommonHeader>();
            type0Flag = sr.Read<byte>();
            type1Flag = sr.Read<byte>();

            subHeader1 = sr.Read<cgprCommonHeader>();
            vecFlag0 = sr.Read<byte>();
            vecFlag1 = sr.Read<byte>();

            switch (vecFlag0)
            {
                case 1:
                    break;
                case 2:
                    vec3_0Header = sr.Read<cgprCommonHeader>();
                    vec3_0 = sr.Read<Vector3>();
                    break;
                default:
                    throw new Exception($"Unexpected vecFlag0 value {vecFlag0}");
            }

            //Should be a 4x3 Matrix, 4 rows, 3 across
            vec3_1SetHeader = sr.Read<cgprCommonHeader>();
            vec3_0 = sr.Read<Vector3>();
            vec3_1 = sr.Read<Vector3>();
            vec3_2 = sr.Read<Vector3>();
            vec3_3 = sr.Read<Vector3>();

            switch (type0Flag)
            {
                case 1:
                    break;
                case 2:
                    stringHeader = sr.Read<cgprCommonHeader>();
                    dataString = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position, dataStringLength));
                    sr.Seek(dataStringLength, System.IO.SeekOrigin.Current);
                    break;
                default:
                    throw new Exception($"Unexpected typeFlag0 value {type0Flag}");
            }

            int_SemiFinal = sr.Read<int>();
            int_Final = sr.Read<int>();
            bt_Final = sr.Read<byte>();
        }
    }
}
