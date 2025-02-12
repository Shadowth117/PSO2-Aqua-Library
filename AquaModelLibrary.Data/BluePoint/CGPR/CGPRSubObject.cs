using AquaModelLibrary.Helpers.Readers;
using System.Text;

namespace AquaModelLibrary.Data.BluePoint.CGPR
{
    public enum CGPRSubMagic : uint
    {
        x3BB01156 = 0x5611B03B,
        x427AC0E6 = 0xE6C07A42,
        x69A26EBB = 0xBB6EA269,
        x88BE09B0 = 0xB009BE88,
        x8D11D855 = 0x55D8118D,
        xA62E6D6E = 0x6E6D2EA6,
        xA9395529 = 0x295539A9,
        xE7359E81 = 0x819E35E7,
        xF0C420F0 = 0xF020C4F0,
    }

    public abstract class CGPRSubObject
    {
        public uint magic;

        public CGPRSubObject()
        {

        }

        public CGPRSubObject(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Peek<uint>();
        }

        public static CGPRSubObject ReadSubObject(BufferedStreamReaderBE<MemoryStream> sr)
        {
            var type = sr.Peek<uint>();
            switch (type)
            {
                case (uint)CGPRSubMagic.x3BB01156:
                    return new _3BB01156_SubObject(sr);
                case (uint)CGPRSubMagic.x427AC0E6:
                    return new _427AC0E6_SubObject(sr);
                case (uint)CGPRSubMagic.x69A26EBB:
                    return new _69A26EBB_SubObject(sr);
                case (uint)CGPRSubMagic.x88BE09B0:
                    return new _88BE09B0_SubObject(sr);
                case (uint)CGPRSubMagic.x8D11D855:
                    return new _8D11D855_SubObject(sr);
                case (uint)CGPRSubMagic.xA62E6D6E:
                    return new _A62E6D6E_SubObject(sr);
                case (uint)CGPRSubMagic.xA9395529:
                    return new _427AC0E6_SubObject(sr);
                case (uint)CGPRSubMagic.xE7359E81:
                    return new _E7359E81_SubObject(sr);
                case (uint)CGPRSubMagic.xF0C420F0:
                    return new _F0C420F0_SubObject(sr);
                default:
                    return new CGPRInt_SubObject(sr);
            }
        }
    }

    public class _8D11D855_SubObject : CGPRSubObject
    {
        public CGPRCommonHeader mainHeader;
        public List<string> strings = new List<string>();

        public _8D11D855_SubObject() { }
        public _8D11D855_SubObject(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Peek<uint>();
            mainHeader = new CGPRCommonHeader(sr);
            var stringCount = sr.Read<ushort>();
            for (int i = 0; i < stringCount; i++)
            {
                var strLen = sr.Read<byte>();
                strings.Add(sr.ReadCStringSeek(strLen));
            }
        }

    }

    public class _427AC0E6_SubObject : CGPRSubObject
    {
        public CGPRCommonHeader mainHeader;
        public byte dataStringLength;
        public string dataString;

        public _427AC0E6_SubObject()
        {

        }

        public _427AC0E6_SubObject(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Peek<uint>();
            mainHeader = new CGPRCommonHeader(sr);
            dataStringLength = sr.Read<byte>();
            dataString = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position, dataStringLength));
            sr.Seek(dataStringLength, System.IO.SeekOrigin.Current);
        }
    }

    public class _3BB01156_SubObject : CGPRSubObject
    {
        public CGPRCommonHeader mainHeader;
        public List<CGPRSubObject> subObjects = new List<CGPRSubObject>();

        public _3BB01156_SubObject()
        {

        }

        public _3BB01156_SubObject(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Peek<uint>();
            mainHeader = new CGPRCommonHeader(sr);
            ushort subStructCount = sr.Read<ushort>();
            for (int i = 0; i < subStructCount; i++)
            {
                subObjects.Add(ReadSubObject(sr));
            }
        }
    }
    public class _E7359E81_SubObject : CGPRInt_SubObject
    {
        public _E7359E81_SubObject() : base() { }
        public _E7359E81_SubObject(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }
    }
    public class _F0C420F0_SubObject : CGPRInt_SubObject
    {
        public _F0C420F0_SubObject() : base() { }
        public _F0C420F0_SubObject(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }
    }
    public class _88BE09B0_SubObject : CGPRByte_SubObject
    {
        public _88BE09B0_SubObject() : base() { }
        public _88BE09B0_SubObject(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }
    }
    public class _69A26EBB_SubObject : CGPRByte_SubObject
    {
        public _69A26EBB_SubObject() : base() { }
        public _69A26EBB_SubObject(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }
    }
    public class _A9395529_SubObject : CGPRByte_SubObject
    {
        public _A9395529_SubObject() : base() { }
        public _A9395529_SubObject(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }
    }
    public class _A62E6D6E_SubObject : CGPRByte_SubObject
    {
        public _A62E6D6E_SubObject() : base() { }
        public _A62E6D6E_SubObject(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }
    }

    public class CGPRByte_SubObject : CGPRSubObject
    {
        public CGPRCommonHeader mainHeader;
        public byte bt_00;

        public CGPRByte_SubObject()
        {

        }

        public CGPRByte_SubObject(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Peek<uint>();
            mainHeader = new CGPRCommonHeader(sr);
            bt_00 = sr.Read<byte>();
        }
    }
    public class CGPRInt_SubObject : CGPRSubObject
    {
        public CGPRCommonHeader mainHeader;
        public int int_00;

        public CGPRInt_SubObject()
        {

        }

        public CGPRInt_SubObject(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Peek<uint>();
            mainHeader = new CGPRCommonHeader(sr);
            int_00 = sr.Read<int>();
        }
    }
}
