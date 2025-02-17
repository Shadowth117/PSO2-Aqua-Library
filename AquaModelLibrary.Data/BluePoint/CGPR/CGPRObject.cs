using AquaModelLibrary.Helpers.Readers;
using System.Numerics;
using System.Text;

namespace AquaModelLibrary.Data.BluePoint.CGPR
{
    public enum CGPRMagic : uint
    {
        x2FBDFD9B = 0x9BFDBD2F,
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
        private CLength length;

        public CGPRCommonHeader() { }
        public CGPRCommonHeader(uint magic, CLength len) 
        {
            var magicBytes = BitConverter.GetBytes(magic);
            bt_0 = magicBytes[0];
            bt_1 = magicBytes[1];
            bt_2 = magicBytes[2];
            bt_3 = magicBytes[3];
            length = len;
        }
        public CGPRCommonHeader(BufferedStreamReaderBE<MemoryStream> sr)
        {
            bt_0 = sr.Read<byte>();
            bt_1 = sr.Read<byte>();
            bt_2 = sr.Read<byte>();
            bt_3 = sr.Read<byte>();
            length = new CLength(sr);
        }

        public int GetTrueLength()
        {
            return length.GetTrueLength();
        }
    }

    public class _7FB9F5F0_Object : CGPRObject
    {
        public CGPRCommonHeader mainHeader;
        public CGPRCommonHeader subHeader0;
        public short subObjectCount;
        public List<CGPRSubObject> subObjects = new();
        public CLength endLength;
        public List<_7FB9F5F0_EndStruct1> endStruct1s = new();
        public _7FB9F5F0_Object(BufferedStreamReaderBE<MemoryStream> sr)
        {
            var start = sr.Position;
            magic = sr.Peek<uint>();
            mainHeader = new CGPRCommonHeader(sr);
            var end = start + mainHeader.GetTrueLength();

            subHeader0 = new CGPRCommonHeader(sr);
            subObjectCount = sr.Read<short>();
            for (int i = 0; i < subObjectCount; i++)
            {
                subObjects.Add(CGPRSubObject.ReadSubObject(sr, true));
            }

            endLength = new CLength(sr);
            int endCount0 = sr.Read<int>();
            if(endCount0 != 0)
            {
                throw new NotImplementedException();
            }
            int endCount1 = sr.Read<int>();
            for(int i = 0; i < endCount1; i++)
            {
                endStruct1s.Add(new _7FB9F5F0_EndStruct1() 
                {
                    int_00 = sr.Read<int>(),
                    int_04 = sr.Read<int>(),
                    int_08 = sr.Read<int>(),
                    int_0C = sr.Read<int>(),
                    
                    int_10 = sr.Read<int>(),
                    int_14 = sr.Read<int>(),
                    int_18 = sr.Read<int>(),
                    bt_1C = sr.Read<byte>(),

                    int_1D = sr.Read<int>(),
                    int_21 = sr.Read<int>(),
                    bt_25 = sr.Read<byte>(),
                });
            }
        }

        public struct _7FB9F5F0_EndStruct1
        {
            public int int_00;
            public int int_04;
            public int int_08;
            public int int_0C;

            public int int_10;
            public int int_14;
            public int int_18;
            public byte bt_1C;

            public int int_1D;
            public int int_21;
            public byte bt_25;
        }
    }

    //Used commonly for CMDLs
    public class _FAE88582_Object : CGPRObject
    {
        public long position;

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
            position = sr.Position;

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

        public CGPRCommonHeader vec3_0_0Header;
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
                    vec3_0_0Header = new CGPRCommonHeader(sr);
                    vec3_0_0 = sr.Read<Vector3>();
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
        public long position;
        public CGPRCommonHeader mainHeader;
        public CGPRCommonHeader subHeader0;
        public ushort subStructCount;

        public List<CGPRSubObject> subStructs = new List<CGPRSubObject>();

        public _427AC0E6_Object()
        {

        }

        public _427AC0E6_Object(BufferedStreamReaderBE<MemoryStream> sr)
        {
            position = sr.Position;
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

    public class _2FBDFD9B_Object : CGPRObject
    {
        public CGPRCommonHeader mainHeader;
        public CGPRCommonHeader subHeader0;
        public short subObjectCount;
        public List<CGPRSubObject> subStructs = new List<CGPRSubObject>();
        public CLength endLength;
        public int end00;
        public int end04;
        public _2FBDFD9B_Object() { }
        public _2FBDFD9B_Object(BufferedStreamReaderBE<MemoryStream> sr) 
        {
            mainHeader = new CGPRCommonHeader(sr);
            subHeader0 = new CGPRCommonHeader(sr);
            subObjectCount = sr.Read<short>();
            for(int i = 0; i < subObjectCount; i++)
            {
                subStructs.Add(CGPRSubObject.ReadSubObject(sr));
            }
            endLength = new CLength(sr);
            end00 = sr.Read<int>();
            end04 = sr.Read<int>();

            //These are probably a list section stub. Need to account for this if we find one that has a list
            if(end00 != 0 || end04 != 0)
            {
                throw new NotImplementedException();
            }
        }
    }

    public class _2C146841_Object : CGPRObject
    {
        public CGPRCommonHeader mainHeader;
        public CGPRCommonHeader subHeader0;
        public short subObjectCount;
        public List<CGPRSubObject> subStructs = new List<CGPRSubObject>();
        public CLength listSectionLength;
        public int unkInt;
        public int unkShort;
        public List<_2C146841_listChunk> listChunks = new List<_2C146841_listChunk>();
        public _2C146841_Object() { }

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
            listSectionLength = new CLength(sr);
            var listSectionCount = sr.Read<int>();
            unkInt = sr.Read<int>();

            for (int i = 0; i < listSectionCount; i++)
            {
                listChunks.Add(new _2C146841_listChunk(sr));
            }
        }

        public class _2C146841_listChunk
        {
            public int int_00;
            public int int_04;
            public int int_08;
            public short sht_0C;
            public short sht_0E;
            public short sht_10;
            public short sht_12;
            public short sht_14;
            public short sht_16;
            public short sht_18;
            public short sht_1A;
            public short sht_1C;
            public short sht_1E;
            public short sht_20;
            public short subStructCount;
            public List<CGPRSubObject> subStructs = new List<CGPRSubObject>();
            public int unkInt;
            public _2C146841_listChunk() { }
            public _2C146841_listChunk(BufferedStreamReaderBE<MemoryStream> sr)
            {
                int_00 = sr.Read<int>();
                int_04 = sr.Read<int>();
                int_08 = sr.Read<int>();
                sht_0C = sr.Read<short>();
                sht_0E = sr.Read<short>();
                sht_10 = sr.Read<short>();
                sht_12 = sr.Read<short>();
                sht_14 = sr.Read<short>();
                sht_16 = sr.Read<short>();
                sht_18 = sr.Read<short>();
                sht_1A = sr.Read<short>();
                sht_1C = sr.Read<short>();
                sht_1E = sr.Read<short>();
                sht_20 = sr.Read<short>();
                subStructCount = sr.Read<short>();
                for (int i = 0; i < subStructCount; i++)
                {
                    subStructs.Add(CGPRSubObject.ReadSubObject(sr));
                }
                unkInt = sr.Read<int>();
            }
        }
    }

    public class CGPRGeneric_Object : CGPRObject
    {
        public long position;
        public CGPRCommonHeader mainHeader;
        public byte[] bytes = null;

        public CGPRGeneric_Object() { }

        public CGPRGeneric_Object(BufferedStreamReaderBE<MemoryStream> sr)
        {
            position = sr.Position;
            magic = sr.Peek<uint>();
            mainHeader = new CGPRCommonHeader(sr);
            bytes = sr.ReadBytesSeek(mainHeader.GetTrueLength());
        }
    }
}
