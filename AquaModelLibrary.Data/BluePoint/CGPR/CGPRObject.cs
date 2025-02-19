using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BluePoint.CGPR
{
    public enum CGPRMagic : uint
    {
        x2FBDFD9B = 0x9BFDBD2F,
        xC1A69458 = 0x5894A6C1,
        xFAE88582 = 0x8285E8FA,
        x427AC0E6 = 0xE6C07A42,
        x2C146841 = 0x4168142C,
        x58D3EEDC = 0xDCEED358,
        x7FB9F5F0 = 0xF0F5B97F,
    }
    public abstract class CGPRObject
    {
        public uint magic;

        //Ending bytes
        public CLength endSize;
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

        public int GetLengthWithHeaderLength()
        {
            return GetTrueLength() + 4 + length.GetCLengthStructSize();
        }
    }

    public class _7FB9F5F0_Object : CGPRObject
    {
        public CGPRCommonHeader mainHeader;
        public CGPRSubObject subObject;
        public CLength endLength;
        public List<CGPR_EndSubstruct0> endStruct1s = new();
        public _7FB9F5F0_Object(BufferedStreamReaderBE<MemoryStream> sr)
        {
            var start = sr.Position;
            magic = sr.Peek<uint>();
            mainHeader = new CGPRCommonHeader(sr);
            var end = start + mainHeader.GetTrueLength();
            subObject = CGPRSubObject.ReadSubObject(sr);

            endLength = new CLength(sr);
            int endCount0 = sr.Read<int>();
            if(endCount0 != 0)
            {
                throw new NotImplementedException();
            }
            int endCount1 = sr.Read<int>();
            for(int i = 0; i < endCount1; i++)
            {
                endStruct1s.Add(new CGPR_EndSubstruct0() 
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
    }

    //Used commonly for CMDLs
    public class _FAE88582_Object : CGPRObject
    {
        public long position;

        public CGPRCommonHeader mainHeader;
        public CGPRSubObject subObject;
        public CLength endLength;
        public int end00;
        public int end04;

        public _FAE88582_Object() { }

        public _FAE88582_Object(BufferedStreamReaderBE<MemoryStream> sr)
        {
            position = sr.Position;

            magic = sr.Peek<uint>();
            mainHeader = new CGPRCommonHeader(sr);
            subObject = CGPRSubObject.ReadSubObject(sr);

            endLength = new CLength(sr);
            end00 = sr.Read<int>();
            end04 = sr.Read<int>();

            //These are probably a list section stub. Need to account for this if we find one that has a list
            if (end00 != 0 || end04 != 0)
            {
                throw new NotImplementedException();
            }
        }
    }

    //Often at the start of CGPRs, sometimes spread throughout
    public class _C1A69458_Object : CGPRObject
    {
        public long position;

        public CGPRCommonHeader mainHeader;
        public CGPRSubObject subObject;

        public int end00Count;
        public int end04Count;

        public List<CGPR_EndSubstruct0> endStruct0s = new List<CGPR_EndSubstruct0>();
        public _C1A69458_Object()
        {

        }

        public _C1A69458_Object(BufferedStreamReaderBE<MemoryStream> sr)
        {
            position = sr.Position;

            magic = sr.Peek<uint>();
            mainHeader = new CGPRCommonHeader(sr);
            subObject = CGPRSubObject.ReadSubObject(sr);

            var endSizePosition = sr.Position;
            endSize = new CLength(sr);
            end00Count = sr.Read<int>();

            for (int i = 0; i < end00Count; i++)
            {
                endStruct0s.Add(new CGPR_EndSubstruct0()
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

            end04Count = sr.Read<int>();

            if (end04Count != 0)
            {
                //throw new NotImplementedException();
                sr.Seek(position + mainHeader.GetLengthWithHeaderLength(), SeekOrigin.Begin);
            }
        }
    }

    public class _427AC0E6_Object : CGPRObject
    {
        public long position;
        public CGPRCommonHeader mainHeader;
        public CGPRSubObject subObject;

        public _427AC0E6_Object()
        {

        }

        public _427AC0E6_Object(BufferedStreamReaderBE<MemoryStream> sr)
        {
            position = sr.Position;
            magic = sr.Peek<uint>();
            mainHeader = new CGPRCommonHeader(sr);
            subObject = CGPRSubObject.ReadSubObject(sr);

            endSize = new CLength(sr);
            endBytes = sr.ReadBytesSeek(endSize.GetTrueLength());
        }
    }

    public class _2FBDFD9B_Object : CGPRObject
    {
        public CGPRCommonHeader mainHeader;
        public CGPRSubObject subObject;
        public int end00;
        public int end04;
        public _2FBDFD9B_Object() { }
        public _2FBDFD9B_Object(BufferedStreamReaderBE<MemoryStream> sr) 
        {
            mainHeader = new CGPRCommonHeader(sr);
            subObject = CGPRSubObject.ReadSubObject(sr);
            endSize = new CLength(sr);
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
        public CGPRSubObject subObject;
        public CLength listSectionLength;
        public int unkInt;
        public List<_2C146841_listChunk> listChunks = new List<_2C146841_listChunk>();
        public _2C146841_Object() { }

        public _2C146841_Object(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Peek<uint>();
            mainHeader = new CGPRCommonHeader(sr);
            subObject = CGPRSubObject.ReadSubObject(sr);
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

    public class _58D3EEDC_Object : CGPRObject
    {
        public CGPRCommonHeader mainHeader;
        public CGPRSubObject subObject;
        public int end00Count;
        public int end04Count;

        public List<CGPR_EndSubstruct0> endStruct1s = new List<CGPR_EndSubstruct0>();
        public _58D3EEDC_Object() { }
        public _58D3EEDC_Object(BufferedStreamReaderBE<MemoryStream> sr)
        {
            mainHeader = new CGPRCommonHeader(sr);
            subObject = CGPRSubObject.ReadSubObject(sr);
            endSize = new CLength(sr);
            end00Count = sr.Read<int>();
            end04Count = sr.Read<int>();

            for (int i = 0; i < end04Count; i++)
            {
                endStruct1s.Add(new CGPR_EndSubstruct0()
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

            //These are probably a list section stub. Need to account for this if we find one that has a list
            if (end00Count != 0)
            {
                throw new NotImplementedException();
            }
        }
    }

    public struct CGPR_EndSubstruct0
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
