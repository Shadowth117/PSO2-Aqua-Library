using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BluePoint.CGPR
{
    /// <summary>
    /// Likely hashes that represent substruct names.
    /// Variable name is the little endian reading as you'd see in hex while the values are the actual hex values.
    /// </summary>
    public enum CGPRSubMagic : uint
    {
        //Byte 
        x192F2CC7 = 0xC72C2F19,
        x4023FE7D = 0x7DFE2340,
        x69A26EBB = 0xBB6EA269,
        x88BE09B0 = 0xB009BE88,
        xA62E6D6E = 0x6E6D2EA6,
        xA9395529 = 0x295539A9,
        xB7A02425 = 0x2524A0B7,
        x56EFE0A2 = 0xA2E0EF56,
        //Int
        x14B5C289 = 0x89C2B514,
        x299F8DE6 = 0xE68D9F29,
        x37F7B290 = 0x90B2F737,
        x4AF33A10 = 0x103AF34A,
        xE7359E81 = 0x819E35E7,
        xF0C420F0 = 0xF020C4F0,
        //2Int
        x440197D8 = 0xD8970144,
        xD9A96316 = 0x1663A9D9,
        //Float
        x9658BB79 = 0x79BB5896,
        //String
        x2FBDFD9B = 0x9BFDBD2F,
        x427AC0E6 = 0xE6C07A42,
        x4486A731 = 0x31A78644,
        //String Array
        x8D11D855 = 0x55D8118D,
        //SubObject Array
        x024007BD = 0xBD074002,
        x075602A6 = 0xA6025607,
        x3BB01156 = 0x5611B03B,
        x47A4191F = 0x1F19A447,
        x5A77756C = 0x6C75775A,
        x97C0A9F4 = 0xF4A9C097,
    }

    public abstract class CGPRSubObject
    {
        public uint magic;

        public CGPRSubObject() { }

        public CGPRSubObject(BufferedStreamReaderBE<MemoryStream> sr)
        {
            throw new NotImplementedException();
        }

        public static CGPRSubObject ReadSubObject(BufferedStreamReaderBE<MemoryStream> sr, bool genericStringOverride = false)
        {
            var type = (CGPRSubMagic)sr.Peek<uint>();
            switch (type)
            {
                //Byte
                case CGPRSubMagic.x192F2CC7:
                case CGPRSubMagic.x4023FE7D:
                case CGPRSubMagic.x56EFE0A2:
                case CGPRSubMagic.x69A26EBB:
                case CGPRSubMagic.x88BE09B0:
                case CGPRSubMagic.xA62E6D6E:
                case CGPRSubMagic.xA9395529:
                case CGPRSubMagic.xB7A02425:
                    return new CGPRByte_SubObject(sr);
                //Int
                case CGPRSubMagic.x14B5C289:
                case CGPRSubMagic.x299F8DE6:
                case CGPRSubMagic.x37F7B290:
                case CGPRSubMagic.x4AF33A10:
                case CGPRSubMagic.xE7359E81:
                case CGPRSubMagic.xF0C420F0:
                    return new CGPRInt_SubObject(sr);
                //2Int
                case CGPRSubMagic.x440197D8:
                case CGPRSubMagic.xD9A96316:
                    return new CGPR2Int_SubObject(sr);
                //Float
                case CGPRSubMagic.x9658BB79:
                    return new CGPRFloat_SubObject(sr);
                //String
                case CGPRSubMagic.x2FBDFD9B:
                case CGPRSubMagic.x427AC0E6:
                case CGPRSubMagic.x4486A731:
                    return new CGPRString_SubObject(sr);
                //String Array
                case CGPRSubMagic.x8D11D855:
                    return new CGPRStringArray_SubObject(sr);
                //SubObject Array
                case CGPRSubMagic.x024007BD:
                case CGPRSubMagic.x075602A6:
                case CGPRSubMagic.x3BB01156:
                case CGPRSubMagic.x47A4191F:
                case CGPRSubMagic.x5A77756C:
                case CGPRSubMagic.x97C0A9F4:
                    return new CGPRSubObjectArray_SubObject(sr);
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
    public class CGPRSubObjectArray_SubObject : CGPRSubObject
    {
        public CGPRCommonHeader mainHeader;
        public List<CGPRSubObject> subObjects = new List<CGPRSubObject>();

        public CGPRSubObjectArray_SubObject() { }
        public CGPRSubObjectArray_SubObject(BufferedStreamReaderBE<MemoryStream> sr)
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
    public class CGPR2Int_SubObject : CGPRSubObject
    {
        public CGPRCommonHeader mainHeader;
        public int int_00;
        public int int_04;

        public CGPR2Int_SubObject() { }

        public CGPR2Int_SubObject(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Peek<uint>();
            mainHeader = new CGPRCommonHeader(sr);
            int_00 = sr.Read<int>();
            int_04 = sr.Read<int>();
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
    public class CGPRStringArray_SubObject : CGPRSubObject
    {
        public CGPRCommonHeader mainHeader;
        public List<string> strings = new List<string>();

        public CGPRStringArray_SubObject() { }
        public CGPRStringArray_SubObject(BufferedStreamReaderBE<MemoryStream> sr)
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
}
