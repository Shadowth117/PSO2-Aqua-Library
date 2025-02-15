using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BluePoint.CGPR
{
    public enum CGPRSubMagic : uint
    {
        x192F2CC7 = 0xC72C2F19,
        x299F8DE6 = 0xE68D9F29,
        x2FBDFD9B = 0x9BFDBD2F,
        x3BB01156 = 0x5611B03B,
        x427AC0E6 = 0xE6C07A42,
        x4486A731 = 0x31A78644,
        x69A26EBB = 0xBB6EA269,
        x88BE09B0 = 0xB009BE88,
        x8D11D855 = 0x55D8118D,
        x9658BB79 = 0x79BB5896,
        xA62E6D6E = 0x6E6D2EA6,
        xA9395529 = 0x295539A9,
        xB7A02425 = 0x2524A0B7,
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

        public static CGPRSubObject ReadSubObject(BufferedStreamReaderBE<MemoryStream> sr, bool genericStringOverride = false)
        {
            var type = (CGPRSubMagic)sr.Peek<uint>();
            switch (type)
            {
                case CGPRSubMagic.x192F2CC7:
                    return new _192F2CC7_SubObject(sr);
                case CGPRSubMagic.x299F8DE6:
                    return new _299F8DE6_SubObject(sr);
                case CGPRSubMagic.x2FBDFD9B:
                    return new _2FBDFD98_SubObject(sr);
                case CGPRSubMagic.x3BB01156:
                    return new _3BB01156_SubObject(sr);
                case CGPRSubMagic.x427AC0E6:
                    return new _427AC0E6_SubObject(sr);
                case CGPRSubMagic.x4486A731:
                    return new _4486A731_SubObject(sr);
                case CGPRSubMagic.x69A26EBB:
                    return new _69A26EBB_SubObject(sr);
                case CGPRSubMagic.x88BE09B0:
                    return new _88BE09B0_SubObject(sr);
                case CGPRSubMagic.x8D11D855:
                    return new _8D11D855_SubObject(sr);
                case CGPRSubMagic.xA62E6D6E:
                    return new _A62E6D6E_SubObject(sr);
                case CGPRSubMagic.xA9395529:
                    return new _A9395529_SubObject(sr);
                case CGPRSubMagic.xB7A02425:
                    return new _B7A02425_SubObject(sr);
                case CGPRSubMagic.xE7359E81:
                    return new _E7359E81_SubObject(sr);
                case CGPRSubMagic.xF0C420F0:
                    return new _F0C420F0_SubObject(sr);
                case CGPRSubMagic.x9658BB79:
                    return new _9658BB79_SubObject(sr);
                default:
                    var pos = sr.Position;
                    if(genericStringOverride == false)
                    {
                        var temphead = new CGPRCommonHeader(sr);
                        var len = temphead.GetTrueLength();
                        sr.Seek(pos, SeekOrigin.Begin);
                        switch (len)
                        {
                            case 0x4:
                                return new CGPRInt_SubObject(sr);
                            case 0x1:
                                return new CGPRByte_SubObject(sr);
                            default:
                                return new CGPRGeneric_SubObject(sr);
                        }
                    } else
                    {
                        return new CGPRString_SubObject(sr);
                    }
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

    public class _2FBDFD98_SubObject : CGPRString_SubObject
    {
        public _2FBDFD98_SubObject() : base() { }
        public _2FBDFD98_SubObject(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }
    }

    public class _427AC0E6_SubObject : CGPRString_SubObject
    {
        public _427AC0E6_SubObject() : base() { }
        public _427AC0E6_SubObject(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }
    }

    public class _4486A731_SubObject : CGPRString_SubObject
    {
        public _4486A731_SubObject() : base() { }
        public _4486A731_SubObject(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }
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
    public class _299F8DE6_SubObject : CGPRInt_SubObject
    {
        public _299F8DE6_SubObject() : base() { }
        public _299F8DE6_SubObject(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }
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
    public class _192F2CC7_SubObject : CGPRByte_SubObject
    {
        public _192F2CC7_SubObject() : base() { }
        public _192F2CC7_SubObject(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }
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
    public class _B7A02425_SubObject : CGPRByte_SubObject
    {
        public _B7A02425_SubObject() : base() { }
        public _B7A02425_SubObject(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }
    }
    public class _9658BB79_SubObject : CGPRFloat_SubObject
    {
        public _9658BB79_SubObject() : base() { }
        public _9658BB79_SubObject(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }
    }

    public class CGPRGeneric_SubObject : CGPRSubObject
    {
        public CGPRCommonHeader mainHeader;
        public byte[] bytes = null;

        public CGPRGeneric_SubObject() { }

        public CGPRGeneric_SubObject(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Peek<uint>();
            mainHeader = new CGPRCommonHeader(sr);
            bytes = sr.ReadBytesSeek(mainHeader.GetTrueLength());
        }
    }
    public class CGPRByte_SubObject : CGPRSubObject
    {
        public CGPRCommonHeader mainHeader;
        public byte bt_00;

        public CGPRByte_SubObject() { }

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

        public CGPRInt_SubObject() { }

        public CGPRInt_SubObject(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Peek<uint>();
            mainHeader = new CGPRCommonHeader(sr);
            int_00 = sr.Read<int>();
        }
    }
    public class CGPRFloat_SubObject : CGPRSubObject
    {
        public CGPRCommonHeader mainHeader;
        public int flt_00;

        public CGPRFloat_SubObject() { }

        public CGPRFloat_SubObject(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Peek<uint>();
            mainHeader = new CGPRCommonHeader(sr);
            flt_00 = sr.Read<int>();
        }
    }

    public class CGPRString_SubObject : CGPRSubObject
    {
        public CGPRCommonHeader mainHeader;
        public BPString dataString;

        public CGPRString_SubObject() { }

        public CGPRString_SubObject(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Read<uint>();
            dataString = new BPString(sr);
            mainHeader = new CGPRCommonHeader(magic, dataString.lengthLength);
        }
    }
}
