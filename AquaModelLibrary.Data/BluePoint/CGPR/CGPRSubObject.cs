using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

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
        x4F13891F = 0x1F89134F,
        x69A26EBB = 0xBB6EA269,
        x7DE40D99 = 0x990DE47D,
        x88BE09B0 = 0xB009BE88,
        xA62E6D6E = 0x6E6D2EA6,
        xA9395529 = 0x295539A9,
        xB7A02425 = 0x2524A0B7,
        x56EFE0A2 = 0xA2E0EF56,
        //Short
        x38F25423 = 0x2354F238,
        x3B8D2509 = 0x09258D3B,
        x89DBCCD3 = 0xD3CCDB89,
        //4Short
        xD5214B36 = 0x364B21D5,
        //Int
        x061C53D9 = 0xD9531C06,
        x08A567E7 = 0xE767A508,
        x0A2949BB = 0xBB49290A,
        x0D4E040B = 0x0B044E0D,
        x14B5C289 = 0x89C2B514,
        x299F8DE6 = 0xE68D9F29,
        x2EA867E9 = 0xE967A82E,
        x37F7B290 = 0x90B2F737,
        x41A33D77 = 0x773DA341,
        x4AF33A10 = 0x103AF34A,
        x524B7716 = 0x16774B52,
        x54490408 = 0x08044954,
        x772749BA = 0xBA492777,
        x7A4C040A = 0x0A044C7A,
        x9BA667E8 = 0xE867A69B,
        x9D2A49BC = 0xBC492A9D,
        xC1A967EA = 0xEA67A9C1,
        xCEA86FAF = 0xAF6FA8CE,
        xE42549B9 = 0xB94925E4,
        xE7359E81 = 0x819E35E7,
        xE74A0409 = 0x09044AE7,
        xF0C420F0 = 0xF020C4F0,
        xFF5E723E = 0x3E725EFF,
        //2Int
        x440197D8 = 0xD8970144,
        xD9A96316 = 0x1663A9D9,
        //Float
        x9658BB79 = 0x79BB5896,
        //Float 3
        x118E5228 = 0x28528E11,
        //Matrix4x3
        xBBAB82AD = 0xAD82ABBB,
        //String
        x2FBDFD9B = 0x9BFDBD2F,
        x427AC0E6 = 0xE6C07A42,
        x4486A731 = 0x31A78644,
        xFAE88582 = 0x8285E8FA,
        //String Array
        x8D11D855 = 0x55D8118D,
        //SubObject Array
        x024007BD = 0xBD074002,
        x075602A6 = 0xA6025607,
        x1D2D64F7 = 0xF7642D1D,
        x3BB01156 = 0x5611B03B,
        x47A4191F = 0x1F19A447,
        x57F7926D = 0x6D92F757,
        x5A77756C = 0x6C75775A,
        x5B3CA418 = 0x18A43C5B,
        x947E6E93 = 0x936E7E94,
        x97C0A9F4 = 0xF4A9C097,
        xB2315805 = 0x055831B2,
        xD4E77FA8 = 0xA87FE7D4,
        xDC55E007 = 0x07E055DC,
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
                case CGPRSubMagic.x4F13891F:
                case CGPRSubMagic.x56EFE0A2:
                case CGPRSubMagic.x69A26EBB:
                case CGPRSubMagic.x7DE40D99:
                case CGPRSubMagic.x88BE09B0:
                case CGPRSubMagic.xA62E6D6E:
                case CGPRSubMagic.xA9395529:
                case CGPRSubMagic.xB7A02425:
                    return new CGPRByte_SubObject(sr);
                //Short
                case CGPRSubMagic.x38F25423:
                case CGPRSubMagic.x3B8D2509:
                case CGPRSubMagic.x89DBCCD3:
                    return new CGPRShort_SubObject(sr);
                //4Short
                case CGPRSubMagic.xD5214B36:
                    return new CGPR4Short_SubObject(sr);
                //Int
                case CGPRSubMagic.x061C53D9:
                case CGPRSubMagic.x08A567E7:
                case CGPRSubMagic.x0A2949BB:
                case CGPRSubMagic.x0D4E040B:
                case CGPRSubMagic.x14B5C289:
                case CGPRSubMagic.x299F8DE6:
                case CGPRSubMagic.x2EA867E9:
                case CGPRSubMagic.x37F7B290:
                case CGPRSubMagic.x41A33D77:
                case CGPRSubMagic.x4AF33A10:
                case CGPRSubMagic.x524B7716:
                case CGPRSubMagic.x54490408:
                case CGPRSubMagic.x772749BA:
                case CGPRSubMagic.x7A4C040A:
                case CGPRSubMagic.x9BA667E8:
                case CGPRSubMagic.x9D2A49BC:
                case CGPRSubMagic.xC1A967EA:
                case CGPRSubMagic.xCEA86FAF:
                case CGPRSubMagic.xE42549B9:
                case CGPRSubMagic.xE7359E81:
                case CGPRSubMagic.xE74A0409:
                case CGPRSubMagic.xF0C420F0:
                case CGPRSubMagic.xFF5E723E:
                    return new CGPRInt_SubObject(sr);
                //2Int
                case CGPRSubMagic.x440197D8:
                case CGPRSubMagic.xD9A96316:
                    return new CGPR2Int_SubObject(sr);
                //Float
                case CGPRSubMagic.x9658BB79:
                    return new CGPRFloat_SubObject(sr);
                //Float 3
                case CGPRSubMagic.x118E5228:
                    return new CGPRVector3_SubObject(sr);
                //Matrix4x3
                case CGPRSubMagic.xBBAB82AD:
                    return new CGPRMatrix4x3_SubObject(sr);
                //String
                case CGPRSubMagic.x2FBDFD9B:
                case CGPRSubMagic.x427AC0E6:
                case CGPRSubMagic.x4486A731:
                case CGPRSubMagic.xFAE88582:
                    return new CGPRString_SubObject(sr);
                //String Array
                case CGPRSubMagic.x8D11D855:
                    return new CGPRStringArray_SubObject(sr);
                //SubObject Array
                case CGPRSubMagic.x024007BD:
                case CGPRSubMagic.x075602A6:
                case CGPRSubMagic.x1D2D64F7:
                case CGPRSubMagic.x3BB01156:
                case CGPRSubMagic.x47A4191F:
                case CGPRSubMagic.x57F7926D:
                case CGPRSubMagic.x5A77756C:
                case CGPRSubMagic.x5B3CA418:
                case CGPRSubMagic.x947E6E93:
                case CGPRSubMagic.x97C0A9F4:
                case CGPRSubMagic.xB2315805:
                case CGPRSubMagic.xD4E77FA8:
                case CGPRSubMagic.xDC55E007:
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
    public class CGPRShort_SubObject : CGPRSubObject
    {
        public CGPRCommonHeader mainHeader;
        public short sht_00;

        public CGPRShort_SubObject() { }

        public CGPRShort_SubObject(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Peek<uint>();
            mainHeader = new CGPRCommonHeader(sr);
            sht_00 = sr.Read<short>();
        }
    }
    public class CGPR4Short_SubObject : CGPRSubObject
    {
        public CGPRCommonHeader mainHeader;
        public short sht_00;
        public short sht_02;
        public short sht_04;
        public short sht_06;

        public CGPR4Short_SubObject() { }

        public CGPR4Short_SubObject(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Peek<uint>();
            mainHeader = new CGPRCommonHeader(sr);
            sht_00 = sr.Read<short>();
            sht_02 = sr.Read<short>();
            sht_04 = sr.Read<short>();
            sht_06 = sr.Read<short>();
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
        public float flt_00;

        public CGPRFloat_SubObject() { }

        public CGPRFloat_SubObject(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Peek<uint>();
            mainHeader = new CGPRCommonHeader(sr);
            flt_00 = sr.Read<float>();
        }
    }
    public class CGPRVector3_SubObject : CGPRSubObject
    {
        public CGPRCommonHeader mainHeader;
        public Vector3 vec3_00;

        public CGPRVector3_SubObject() { }

        public CGPRVector3_SubObject(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Peek<uint>();
            mainHeader = new CGPRCommonHeader(sr);
            vec3_00 = sr.Read<Vector3>();
        }
    }
    public class CGPRMatrix4x3_SubObject : CGPRSubObject
    {
        public CGPRCommonHeader mainHeader;
        public Matrix4x4 mat;

        public CGPRMatrix4x3_SubObject() { }

        public CGPRMatrix4x3_SubObject(BufferedStreamReaderBE<MemoryStream> sr)
        {
            magic = sr.Peek<uint>();
            mainHeader = new CGPRCommonHeader(sr);
            mat = new Matrix4x4(sr.Read<float>(), sr.Read<float>(), sr.Read<float>(), 0,
                                sr.Read<float>(), sr.Read<float>(), sr.Read<float>(), 0,
                                sr.Read<float>(), sr.Read<float>(), sr.Read<float>(), 0,
                                sr.Read<float>(), sr.Read<float>(), sr.Read<float>(), 0);
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
