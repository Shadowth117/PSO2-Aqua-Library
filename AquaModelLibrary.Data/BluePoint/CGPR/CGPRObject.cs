using AquaModelLibrary.Data.BluePoint.CANI;
using AquaModelLibrary.Helpers.Readers;
using System.Diagnostics.Metrics;
using System.Numerics;
using System.Text;

namespace AquaModelLibrary.Data.BluePoint.CGPR
{
    public enum CGPRMagic : uint
    {
        xC1A69458 = 0x5894A6C1,
        xFAE88582 = 0x8285E8FA,
        x427AC0E6 = 0xE6C07A42,
        x2C146841 = 0x4168142C,
        x7FB9F5F0 = 0xF0F5B97F,
    }
    public abstract class CGPRObject
    {
        public uint magic;

        //Ending bytes
        public byte bt_EndSize;
        public byte[] endBytes = null;

        public CGPRObject()
        {

        }

        public CGPRObject(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Peek<uint>();
        }
    }

    public class CGPRCommonHeader
    {
        public byte bt_0;
        public byte bt_1;
        public byte bt_2;
        public byte bt_3;
        private byte length;
        public byte lengthAddition;

        public CGPRCommonHeader() { }
        public CGPRCommonHeader(BufferedStreamReaderBE<MemoryStream> sr)
        {
            bt_0 = sr.Read<byte>();
            bt_1 = sr.Read<byte>();
            bt_2 = sr.Read<byte>();
            bt_3 = sr.Read<byte>();
            length = sr.Read<byte>();
            if (length >= 0x80)
            {
                lengthAddition = sr.Read<byte>();
            }
        }

        public int GetTrueLength()
        {
            return length + (lengthAddition - 1) * 0x80;
        }
    }

    public class CGPRLength
    {
        private byte length;
        public byte lengthAddition;

        public CGPRLength() { }
        public CGPRLength(BufferedStreamReaderBE<MemoryStream> sr)
        {
            length = sr.Read<byte>();
            if (length >= 0x80)
            {
                lengthAddition = sr.Read<byte>();
            }
        }

        public int GetTrueLength()
        {
            return length + (lengthAddition - 1) * 0x80;
        }
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

    public class _7FB9F5F0_Object : CGPRObject
    {
        public CGPRCommonHeader mainHeader;
        public byte bt_mainTest;
        public CGPRCommonHeader subHeader0;
        public byte bt_sub0Test0;
        public short sht_00;
        public int int_00;
        public BPString str_00;
        public List<CGPRSubObject> subObjects = new List<CGPRSubObject>();

        public _7FB9F5F0_Object(BufferedStreamReaderBE<MemoryStream> sr)
        {
            var start = sr.Position;
            magic = sr.Peek<uint>();
            mainHeader = new CGPRCommonHeader(sr);
            var end = start + mainHeader.GetTrueLength();

            subHeader0 = new CGPRCommonHeader(sr);
            sht_00 = sr.Read<short>();
            int_00 = sr.Read<int>();
            str_00 = new BPString(sr);

            var subMagic = sr.Peek<int>();
            while(sr.Position < end)
            {
                subObjects.Add(CGPRSubObject.ReadSubObject(sr));
                if (subMagic == 0x8)
                {
                    break;
                }
                if (sr.Position < sr.BaseStream.Length - 0x4)
                {
                    subMagic = sr.Peek<int>();
                }
            }
        }
    }

    //Used commonly for CMDLs
    public class _FAE88582_Object : CGPRObject
    {
        public CGPRCommonHeader mainHeader;
        public byte bt_mainTest;
        public CGPRCommonHeader subHeader0;
        public byte bt_sub0Test0;
        public byte bt_sub0Test1;

        public byte bt_testPad; //Optional inclusion? appears in most of these anyways as 0
        public CGPRCommonHeader subHeader1;
        public byte bt_sub1Test0;
        public byte bt_sub1_0;
        public byte bt_sub1_1;

        public CGPRCommonHeader subHeader2; //Should be same first 4 bytes as mainHeader
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
            mainHeader = new CGPRCommonHeader(sr);
            subHeader0 = new CGPRCommonHeader(sr);

            //Secondary thing seemingly unrelated to previous struct's length
            var byteTest = sr.Peek<byte>();
            if (byteTest == 0x1)
            {
                bt_sub0Test1 = sr.Read<byte>();
            }
            bt_testPad = sr.Read<byte>();

            subHeader1 = new CGPRCommonHeader(sr);
            bt_sub1_0 = sr.Read<byte>();
            bt_sub1_1 = sr.Read<byte>();

            subHeader2 = new CGPRCommonHeader(sr);
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

            endBytes = sr.ReadBytesSeek(8);
        }
    }

    //Often at the start of CGPRs, sometimes spread throughout
    public class _C1A69458_Object : CGPRObject
    {
        public CGPRCommonHeader mainHeader;
        public CGPRCommonHeader subHeader0;
        public byte type0Flag; //Default of 0x1. 0x2 signifies a trailing string
        public byte type1Flag;

        public CGPRCommonHeader subHeader1;
        public byte vecFlag0; //Default of 0x1. 0x2 signifies an extra vector3
        public byte vecFlag1;

        public CGPRCommonHeader vec3_0Header;
        public Vector3 vec3_0_0;

        public CGPRCommonHeader vec3_1SetHeader;
        public Vector3 vec3_0;
        public Vector3 vec3_1;
        public Vector3 vec3_2;
        public Vector3 vec3_3;

        public CGPRCommonHeader stringHeader;
        public byte dataStringLength;
        public string dataString;

        public _C1A69458_Object()
        {

        }

        public _C1A69458_Object(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Peek<uint>();
            mainHeader = new CGPRCommonHeader(sr);
            subHeader0 = new CGPRCommonHeader(sr);
            type0Flag = sr.Read<byte>();
            type1Flag = sr.Read<byte>();

            subHeader1 = new CGPRCommonHeader(sr);
            vecFlag0 = sr.Read<byte>();
            vecFlag1 = sr.Read<byte>();

            switch (vecFlag0)
            {
                case 1:
                    break;
                case 2:
                    vec3_0Header = new CGPRCommonHeader(sr);
                    vec3_0 = sr.Read<Vector3>();
                    break;
                default:
                    throw new Exception($"Unexpected vecFlag0 value {vecFlag0}");
            }

            //Should be a 4x3 Matrix, 4 rows, 3 across
            vec3_1SetHeader = new CGPRCommonHeader(sr);
            vec3_0 = sr.Read<Vector3>();
            vec3_1 = sr.Read<Vector3>();
            vec3_2 = sr.Read<Vector3>();
            vec3_3 = sr.Read<Vector3>();

            switch (type0Flag)
            {
                case 1:
                    break;
                case 2:
                    stringHeader = new CGPRCommonHeader(sr);
                    dataString = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position, dataStringLength));
                    sr.Seek(dataStringLength, System.IO.SeekOrigin.Current);
                    break;
                default:
                    throw new Exception($"Unexpected typeFlag0 value {type0Flag}");
            }

            bt_EndSize = sr.Read<byte>();
            endBytes = sr.ReadBytesSeek(bt_EndSize);
        }
    }

    public class _427AC0E6_Object : CGPRObject
    {
        public CGPRCommonHeader mainHeader;
        public CGPRCommonHeader subHeader0;
        public ushort subStructCount;

        public List<CGPRSubObject> subStructs = new List<CGPRSubObject>();

        public _427AC0E6_Object()
        {

        }

        public _427AC0E6_Object(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Peek<uint>();
            mainHeader = new CGPRCommonHeader(sr);
            subHeader0 = new CGPRCommonHeader(sr);
            subStructCount = sr.Read<ushort>();
            for(int i = 0; i < subStructCount; i++)
            {
                subStructs.Add(CGPRSubObject.ReadSubObject(sr));
            }

            bt_EndSize = sr.Read<byte>();
            endBytes = sr.ReadBytesSeek(bt_EndSize);
        }
    }

    public class _2C146841_Object : CGPRObject
    {
        public CGPRCommonHeader mainHeader;
        public CGPRCommonHeader subHeader0;
        public short subObjectCount;
        public List<CGPRSubObject> subStructs = new List<CGPRSubObject>();
        public CGPRLength listSectionLength;

        public _2C146841_Object()
        {

        }

        public _2C146841_Object(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Peek<uint>();
            mainHeader = new CGPRCommonHeader(sr);
            subHeader0 = new CGPRCommonHeader(sr);
            subObjectCount = sr.Read<short>();
            for(int i = 0; i < subObjectCount; i++)
            {
                subStructs.Add(CGPRSubObject.ReadSubObject(sr));
            }
            listSectionLength = new CGPRLength(sr);
            var listSectionCount = sr.Read<int>();
        }
    }
}
