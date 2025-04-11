using AquaModelLibrary.Data.BluePoint.CANI;
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
    //Null
        x00000000 = 0x00000000,
    //Byte
        x08265914 = 0x14592608,
        x0ED3F07F = 0x7FF0D30E,
        x14EBCB95 = 0x95cbeb14,
        x192F2CC7 = 0xC72C2F19,
        x2565236A = 0x6A236525,
        x3D4706EB = 0xEB06473D,
        x4023FE7D = 0x7DFE2340,
        x4F13891F = 0x1F89134F,
        x56EFE0A2 = 0xA2E0EF56,
        x69A26EBB = 0xBB6EA269,
        x7DE40D99 = 0x990DE47D,
        x88BE09B0 = 0xB009BE88,
        xA62E6D6E = 0x6E6D2EA6,
        xA9395529 = 0x295539A9,
        xB7A02425 = 0x2524A0B7,
    //Short
        xF194196D = 0x6D1994F1,
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
        x448142A5 = 0xa5428144,
        x4AF33A10 = 0x103AF34A,
        x50022682 = 0x82260250,
        x524B7716 = 0x16774B52,
        x54490408 = 0x08044954,
        x772749BA = 0xBA492777,
        x7A4C040A = 0x0A044C7A,
        x7DE15005 = 0x0550E17D,
        x8338AB8D = 0x8DAB3883,
        x89D43D23 = 0x233dd489,
        x89579CCF = 0xCF9C5789,
        x9BA667E8 = 0xE867A69B,
        x9D2A49BC = 0xBC492A9D,
        xB8A23F0D = 0x0D3FA2B8,
        xBAAFC643 = 0x43C6AFBA,
        xBECBD3B3 = 0xB3D3CBBE,
        xC1A967EA = 0xEA67A9C1,
        xCAB92142 = 0x4221B9CA,
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
        x522EEA39 = 0x39EA2E52,
        x5FB66D5C = 0x5C6DB65F,
        x6FA33FA0 = 0xA03FA36F,
        x9658BB79 = 0x79BB5896,
        x9A4FFCE7 = 0xE7FC4F9A,
        xEA10EDBD = 0xBDED10EA,
        xEBC12345 = 0x4523C1EB,
        xF62C62A2 = 0xA2622CF6,
        xFB0B885A = 0x5A880BFB,
        //Float 3
        x118E5228 = 0x28528E11,
        x22AD50BC = 0xBC50AD22,
        xE50749C2 = 0xC24907E5,
    //Matrix4x3
        xBBAB82AD = 0xAD82ABBB,
    //String
        x2FBDFD9B = 0x9BFDBD2F,
        /// <summary>
        /// DeSR cmat filename
        /// </summary>
        x3B129F82 = 0x829F123B,
        x3D6E4A22 = 0x224A6E3D,
        x427AC0E6 = 0xE6C07A42,
        x4486A731 = 0x31A78644,
        /// <summary>
        /// SOTC cpid reference
        /// </summary>
        x56C23AA0 = 0xA03AC256,
        /// <summary>
        /// SOTC cmat filename
        /// </summary>
        x587568C3 = 0xC36E7558,
        /// <summary>
        /// SOTC cmsh reference
        /// </summary>
        x68955D41 = 0x415D9568,
        /// <summary>
        /// SOTC cclm reference HQ
        /// </summary>
        x733AC6C8 = 0xC8C63A73,
        x7FFE5BB3 = 0xB35BFE7F,
        /// <summary>
        /// DeSR Material reference
        /// </summary>
        x804FDBCB = 0xCBD54F80,
        /// <summary>
        /// SOTC Material reference
        /// </summary>
        xC317C885 = 0x85C817C3,
        /// <summary>
        /// DeSR cmsh reference
        /// </summary>
        xDC55E007 = 0x07E055DC,
        xEBE45CAB = 0xAB5CE4EB,
        /// <summary>
        /// SOTC cclm reference
        /// </summary>
        xF23E68B7 = 0xB7683EF2,
        xFAE88582 = 0x8285E8FA,
    //String Array
        x8D11D855 = 0x55D8118D,
    //SubObject Array 2
        x24CC5387 = 0x8753CC24,
        x82977AD1 = 0xD17A9782,
        xF7D9AE0F = 0x0FAED9F7,
    //SubObject Array
        x024007BD = 0xBD074002,
        x075602A6 = 0xA6025607,
        x1A24B17C = 0x7CB1241A,
        x1D2D64F7 = 0xF7642D1D,
        x256BD189 = 0x89d16b25,
        x38F25423 = 0x2354F238,
        x3BB01156 = 0x5611B03B,
        x3B8D2509 = 0x09258D3B,
        x47A4191F = 0x1F19A447,
        x509B796E = 0x6E799B50,
        x51F0A744 = 0x44A7F051,
        x57F7926D = 0x6D92F757,
        x5A77756C = 0x6C75775A,
        x5B3CA418 = 0x18A43C5B,
        x6C00F9ED = 0xEDF9006C,
        x76FA4A83 = 0x834AFA76,
        x790A3BBB = 0xBB3B0A79,
        x947E6E93 = 0x936E7E94,
        x97C0A9F4 = 0xF4A9C097,
        x9F74338B = 0x8B33749F,
        xA0511E50 = 0x501E51A0,
        xA18E5EA1 = 0xA15E8EA1,
        xB2315805 = 0x055831B2,
        xB65356AE = 0xAE5653B6,
        xC3A5BE4F = 0x4FBEA5C3,
        xD4E77FA8 = 0xA87FE7D4,
        xE7C8165C = 0x5C16C8E7,
    //MultiSubObject Array
        x0AF1EECC = 0xCCEEF10A,
        x1694E619 = 0x19E69416,
        x8259284A = 0x4A285982,
        x89DBCCD3 = 0xD3CCDB89,
        xB3AC42A3 = 0xA342ACB3,
    }

    public abstract class CGPRSubObject
    {
        public CGPRCommonHeader mainHeader;
        public BPEra era
        {
            get
            {
                return mainHeader.era;
            }
            set
            {
                mainHeader.era = value;
            }
        }
        public CGPRSubMagic magic
        {
            get
            {
                return (CGPRSubMagic)mainHeader.magic;
            }
            set
            {
                mainHeader.magic = (uint)value;
            }
        }

        public CGPRSubObject() { }

        public CGPRSubObject(BufferedStreamReaderBE<MemoryStream> sr)
        {
            throw new NotImplementedException();
        }

        public static CGPRSubObject ReadSubObject(BufferedStreamReaderBE<MemoryStream> sr, BPEra newEra)
        {
            var type = sr.Peek<CGPRSubMagic>();
            switch (type)
            {
                //Null
                case CGPRSubMagic.x00000000:
                    sr.Read<int>();
                    return null;
                //Byte
                case CGPRSubMagic.x08265914:
                case CGPRSubMagic.x0ED3F07F:
                case CGPRSubMagic.x14EBCB95:
                case CGPRSubMagic.x192F2CC7:
                case CGPRSubMagic.x2565236A:
                case CGPRSubMagic.x3D4706EB:
                case CGPRSubMagic.x4023FE7D:
                case CGPRSubMagic.x4F13891F:
                case CGPRSubMagic.x56EFE0A2:
                case CGPRSubMagic.x69A26EBB:
                case CGPRSubMagic.x7DE40D99:
                case CGPRSubMagic.x88BE09B0:
                case CGPRSubMagic.xA62E6D6E:
                case CGPRSubMagic.xA9395529:
                case CGPRSubMagic.xB7A02425:
                    return new CGPRByte_SubObject(sr, newEra);
                //Short
                case CGPRSubMagic.xF194196D:
                    return new CGPRShort_SubObject(sr, newEra);
                //4Short
                case CGPRSubMagic.xD5214B36:
                    return new CGPR4Short_SubObject(sr, newEra);
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
                case CGPRSubMagic.x448142A5:
                case CGPRSubMagic.x4AF33A10:
                case CGPRSubMagic.x50022682:
                case CGPRSubMagic.x524B7716:
                case CGPRSubMagic.x54490408:
                case CGPRSubMagic.x772749BA:
                case CGPRSubMagic.x7A4C040A:
                case CGPRSubMagic.x7DE15005:
                case CGPRSubMagic.x8338AB8D:
                case CGPRSubMagic.x89D43D23:
                case CGPRSubMagic.x89579CCF:
                case CGPRSubMagic.x9BA667E8:
                case CGPRSubMagic.x9D2A49BC:
                case CGPRSubMagic.xB8A23F0D:
                case CGPRSubMagic.xBAAFC643:
                case CGPRSubMagic.xBECBD3B3:
                case CGPRSubMagic.xC1A967EA:
                case CGPRSubMagic.xCAB92142:
                case CGPRSubMagic.xCEA86FAF:
                case CGPRSubMagic.xE42549B9:
                case CGPRSubMagic.xE7359E81:
                case CGPRSubMagic.xE74A0409:
                case CGPRSubMagic.xF0C420F0:
                case CGPRSubMagic.xFF5E723E:
                    return new CGPRInt_SubObject(sr, newEra);
                //2Int
                case CGPRSubMagic.x440197D8:
                case CGPRSubMagic.xD9A96316:
                    return new CGPR2Int_SubObject(sr, newEra);
                //Float
                case CGPRSubMagic.x522EEA39:
                case CGPRSubMagic.x5FB66D5C:
                case CGPRSubMagic.x6FA33FA0:
                case CGPRSubMagic.x9658BB79:
                case CGPRSubMagic.x9A4FFCE7:
                case CGPRSubMagic.xEA10EDBD:
                case CGPRSubMagic.xEBC12345:
                case CGPRSubMagic.xF62C62A2:
                case CGPRSubMagic.xFB0B885A:
                    return new CGPRFloat_SubObject(sr, newEra);
                //Float 3
                case CGPRSubMagic.x118E5228:
                case CGPRSubMagic.x22AD50BC:
                case CGPRSubMagic.xE50749C2:
                    return new CGPRVector3_SubObject(sr, newEra);
                //Matrix4x3
                case CGPRSubMagic.xBBAB82AD:
                    return new CGPRMatrix4x3_SubObject(sr, newEra);
                //String
                case CGPRSubMagic.x2FBDFD9B:
                case CGPRSubMagic.x3B129F82:
                case CGPRSubMagic.x3D6E4A22:
                case CGPRSubMagic.x427AC0E6:
                case CGPRSubMagic.x4486A731:
                case CGPRSubMagic.x56C23AA0:
                case CGPRSubMagic.x587568C3:
                case CGPRSubMagic.x68955D41:
                case CGPRSubMagic.x733AC6C8:
                case CGPRSubMagic.x7FFE5BB3:
                case CGPRSubMagic.x804FDBCB:
                case CGPRSubMagic.xC317C885:
                case CGPRSubMagic.xDC55E007:
                case CGPRSubMagic.xEBE45CAB:
                case CGPRSubMagic.xF23E68B7:
                case CGPRSubMagic.xFAE88582:
                    return new CGPRString_SubObject(sr, newEra);
                //String Array
                case CGPRSubMagic.x8D11D855:
                    return new CGPRStringArray_SubObject(sr, newEra);
                //SubObject Array 2
                case CGPRSubMagic.x24CC5387:
                case CGPRSubMagic.x82977AD1:
                case CGPRSubMagic.xF7D9AE0F:
                    return new CGPRSubObjectArray2_SubObject(sr, newEra);
                //SubObject Array
                case CGPRSubMagic.x024007BD:
                case CGPRSubMagic.x075602A6:
                case CGPRSubMagic.x1A24B17C:
                case CGPRSubMagic.x1D2D64F7:
                case CGPRSubMagic.x256BD189:
                case CGPRSubMagic.x38F25423:
                case CGPRSubMagic.x3BB01156:
                case CGPRSubMagic.x3B8D2509:
                case CGPRSubMagic.x47A4191F:
                case CGPRSubMagic.x509B796E:
                case CGPRSubMagic.x51F0A744:
                case CGPRSubMagic.x57F7926D:
                case CGPRSubMagic.x5A77756C:
                case CGPRSubMagic.x5B3CA418:
                case CGPRSubMagic.x6C00F9ED:
                case CGPRSubMagic.x76FA4A83:
                case CGPRSubMagic.x790A3BBB:
                case CGPRSubMagic.x947E6E93:
                case CGPRSubMagic.x97C0A9F4:
                case CGPRSubMagic.x9F74338B:
                case CGPRSubMagic.xA0511E50:
                case CGPRSubMagic.xA18E5EA1:
                case CGPRSubMagic.xB2315805:
                case CGPRSubMagic.xB65356AE:
                case CGPRSubMagic.xC3A5BE4F:
                case CGPRSubMagic.xD4E77FA8:
                case CGPRSubMagic.xE7C8165C:
                    return new CGPRSubObjectArray_SubObject(sr, newEra);
                case CGPRSubMagic.x0AF1EECC:
                case CGPRSubMagic.x1694E619:
                case CGPRSubMagic.x8259284A:
                case CGPRSubMagic.x89DBCCD3:
                case CGPRSubMagic.xB3AC42A3:
                    return new CGPRMultiSubObjectArray_SubObject(sr, newEra);
                default:
                    var pos = sr.Position;
                    var temphead = new CGPRCommonHeader(sr, newEra);
                    var len = temphead.GetTrueLength();
                    sr.Seek(pos, SeekOrigin.Begin);
                    switch (len)
                    {
                        case 0x4:
                            return new CGPRInt_SubObject(sr, newEra);
                        case 0x1:
                            return new CGPRByte_SubObject(sr, newEra);
                        default:
                            return new CGPRGeneric_SubObject(sr, newEra);
                    }
            }
        }
    }

    public class CGPRGeneric_SubObject : CGPRSubObject
    {
        public byte[] bytes = null;

        public CGPRGeneric_SubObject() { }

        public CGPRGeneric_SubObject(BufferedStreamReaderBE<MemoryStream> sr, BPEra newEra)
        {
            mainHeader = new CGPRCommonHeader(sr, newEra);
            bytes = sr.ReadBytesSeek(mainHeader.GetTrueLength());
        }
    }
    public class CGPRByte_SubObject : CGPRSubObject
    {
        public byte bt_00;

        public CGPRByte_SubObject() { }

        public CGPRByte_SubObject(BufferedStreamReaderBE<MemoryStream> sr, BPEra newEra)
        {
            mainHeader = new CGPRCommonHeader(sr, newEra);
            bt_00 = sr.Read<byte>();
        }
    }
    public class CGPRShort_SubObject : CGPRSubObject
    {
        public short sht_00;

        public CGPRShort_SubObject() { }

        public CGPRShort_SubObject(BufferedStreamReaderBE<MemoryStream> sr, BPEra newEra)
        {
            mainHeader = new CGPRCommonHeader(sr, era);
            sht_00 = sr.Read<short>();
        }
    }
    public class CGPR4Short_SubObject : CGPRSubObject
    {
        public short sht_00;
        public short sht_02;
        public short sht_04;
        public short sht_06;

        public CGPR4Short_SubObject() { }

        public CGPR4Short_SubObject(BufferedStreamReaderBE<MemoryStream> sr, BPEra newEra)
        {
            mainHeader = new CGPRCommonHeader(sr, newEra);
            sht_00 = sr.Read<short>();
            sht_02 = sr.Read<short>();
            sht_04 = sr.Read<short>();
            sht_06 = sr.Read<short>();
        }
    }
    /// <summary>
    /// No noted subobject count
    /// </summary>
    public class CGPRSubObjectArray2_SubObject : CGPRSubObject
    {
        public List<CGPRSubObject> subObjects = new List<CGPRSubObject>();

        public CGPRSubObjectArray2_SubObject() { }
        public CGPRSubObjectArray2_SubObject(BufferedStreamReaderBE<MemoryStream> sr, BPEra newEra)
        {
            var position = sr.Position;

            mainHeader = new CGPRCommonHeader(sr, newEra);
            while (sr.Position < position + mainHeader.GetLengthWithHeaderLength())
            {
                subObjects.Add(ReadSubObject(sr, era));
            }
            
        }
    }
    public class CGPRSubObjectArray_SubObject : CGPRSubObject
    {
        public List<CGPRSubObject> subObjects = new List<CGPRSubObject>();
        public ushort usht_00;
        public ushort usht_02;

        public CGPRSubObjectArray_SubObject() { }
        public CGPRSubObjectArray_SubObject(BufferedStreamReaderBE<MemoryStream> sr, BPEra newEra)
        {
            var position = sr.Position;

            mainHeader = new CGPRCommonHeader(sr, newEra);
            ushort subStructCount = sr.Read<ushort>();

            //Weird edge case where sometimes this is just an int or two shorts? Seen in ground_base_5e335f49.cmdl in DeSR
            if(mainHeader.GetTrueLength() == 0x4)
            {
                usht_00 = subStructCount;
                usht_02 = sr.ReadBE<ushort>();
            } else
            {
                for (int i = 0; i < subStructCount; i++)
                {
                    subObjects.Add(ReadSubObject(sr, era));
                }
            }

            var expectedEndPos = position + mainHeader.GetLengthWithHeaderLength();
            if (sr.Position != expectedEndPos)
            {
                throw new Exception("Unexpected SubObject Length!");
            }
        }
    }
    public class CGPRMultiSubObjectArray_SubObject : CGPRSubObject
    {
        public List<CGPRSubObjectSet> subObjectsList = new List<CGPRSubObjectSet>();

        public CGPRMultiSubObjectArray_SubObject() { }
        public CGPRMultiSubObjectArray_SubObject(BufferedStreamReaderBE<MemoryStream> sr, BPEra newEra)
        {
            var position = sr.Position;

            mainHeader = new CGPRCommonHeader(sr, newEra);
            ushort subStructCount = sr.Read<ushort>();
            if(subStructCount != 0)
            {
                ushort subObjectCount = sr.Read<ushort>();
                for (int s = 0; s < subStructCount; s++)
                {
                    var set = new CGPRSubObjectSet();
                    for (int i = 0; i < subObjectCount; i++)
                    {
                        set.subObjects.Add(ReadSubObject(sr, era));
                    }
                    if (s != subStructCount - 1)
                    {
                        set.shtEnd = sr.ReadBE<short>();
                    }
                    subObjectsList.Add(set);
                }
            }

            if (sr.Position != position + mainHeader.GetLengthWithHeaderLength())
            {
                throw new Exception("Unexpected SubObject Length!");
            }
        }
        public class CGPRSubObjectSet
        {
            public List<CGPRSubObject> subObjects = new List<CGPRSubObject>();
            public short shtEnd;
        }
    }
    public class CGPRInt_SubObject : CGPRSubObject
    {
        public int int_00;

        public CGPRInt_SubObject() { }

        public CGPRInt_SubObject(BufferedStreamReaderBE<MemoryStream> sr, BPEra newEra)
        {
            mainHeader = new CGPRCommonHeader(sr, newEra);
            int_00 = sr.Read<int>();
            if(mainHeader.GetTrueLength() != 4)
            {
                throw new Exception("Struct is NOT an int, please correct!");
            }
        }
    }
    public class CGPR2Int_SubObject : CGPRSubObject
    {
        public int int_00;
        public int int_04;

        public CGPR2Int_SubObject() { }
        public CGPR2Int_SubObject(BufferedStreamReaderBE<MemoryStream> sr, BPEra newEra)
        {
            mainHeader = new CGPRCommonHeader(sr, newEra);
            int_00 = sr.Read<int>();
            int_04 = sr.Read<int>();
            if (mainHeader.GetTrueLength() != 8)
            {
                throw new Exception("Struct is NOT a 2int, please correct!");
            }
        }
    }
    public class CGPRFloat_SubObject : CGPRSubObject
    {
        public float flt_00;
        public CGPRFloat_SubObject() { }
        public CGPRFloat_SubObject(BufferedStreamReaderBE<MemoryStream> sr, BPEra newEra)
        {
            mainHeader = new CGPRCommonHeader(sr, newEra);
            flt_00 = sr.Read<float>();

            if (mainHeader.GetTrueLength() != 4)
            {
                throw new Exception("Struct is NOT a float, please correct!");
            }
        }
    }
    public class CGPRVector3_SubObject : CGPRSubObject
    {
        public Vector3 vec3_00;
        public CGPRVector3_SubObject() { }
        public CGPRVector3_SubObject(BufferedStreamReaderBE<MemoryStream> sr, BPEra newEra)
        {
            mainHeader = new CGPRCommonHeader(sr, newEra);
            vec3_00 = sr.Read<Vector3>();

            if (mainHeader.GetTrueLength() != 0xC)
            {
                throw new Exception("Struct is NOT an vec3, please correct!");
            }
        }
    }
    public class CGPRMatrix4x3_SubObject : CGPRSubObject
    {
        public Matrix4x4 mat;

        public CGPRMatrix4x3_SubObject() { }

        public CGPRMatrix4x3_SubObject(BufferedStreamReaderBE<MemoryStream> sr, BPEra newEra)
        {
            var position = sr.Position;

            mainHeader = new CGPRCommonHeader(sr, newEra);
            mat = new Matrix4x4(sr.Read<float>(), sr.Read<float>(), sr.Read<float>(), 0,
                                sr.Read<float>(), sr.Read<float>(), sr.Read<float>(), 0,
                                sr.Read<float>(), sr.Read<float>(), sr.Read<float>(), 0,
                                sr.Read<float>(), sr.Read<float>(), sr.Read<float>(), 0);

            if (sr.Position != position + mainHeader.GetLengthWithHeaderLength())
            {
                throw new Exception("Unexpected SubObject Length!");
            }
        }
    }

    public class CGPRString_SubObject : CGPRSubObject
    {
        public BPString dataString;
        public CGPRString_SubObject() { }
        public CGPRString_SubObject(BufferedStreamReaderBE<MemoryStream> sr, BPEra newEra)
        {
            var magic = sr.Read<uint>();
            dataString = new BPString(sr, newEra);
            mainHeader = new CGPRCommonHeader(magic, dataString.lengthLength);
        }
    }
    public class CGPRStringArray_SubObject : CGPRSubObject
    {
        public List<string> strings = new List<string>();
        public CGPRStringArray_SubObject() { }
        public CGPRStringArray_SubObject(BufferedStreamReaderBE<MemoryStream> sr, BPEra newEra)
        {
            var position = sr.Position;

            mainHeader = new CGPRCommonHeader(sr, newEra);
            var stringCount = sr.Read<ushort>();
            for (int i = 0; i < stringCount; i++)
            {
                var strLen = sr.Read<byte>();
                strings.Add(sr.ReadCStringSeek(strLen));
            }

            if (sr.Position != position + mainHeader.GetLengthWithHeaderLength())
            {
                throw new Exception("Unexpected SubObject Length!");
            }
        }
    }
}
