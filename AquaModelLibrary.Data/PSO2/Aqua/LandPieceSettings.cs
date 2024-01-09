using AquaModelLibrary.Helpers.Readers;
using System.Diagnostics;
using System.IO;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    public class LandPieceSettings : AquaCommon
    {
        public LPSHeader header;
        public Dictionary<string, float> fVarDict = new Dictionary<string, float>(); //Stored as pairs of string ptrs and floats
        public Dictionary<string, string> stringVarDict = new Dictionary<string, string>(); //Stored as pairs of string ptrs 
        public Dictionary<string, string> areaEntryExitDefaults = new Dictionary<string, string>(); //Stored as pairs of string ptrs; Somewhat unsure on this, but they specify a land formation file.
        public Dictionary<string, string> areaEntryExitDefaults2 = new Dictionary<string, string>(); //Stored as pairs of string ptrs; Somewhat unsure on this, but they specify a land formation file.
        public Dictionary<string, PieceSetObj> pieceSets = new Dictionary<string, PieceSetObj>();

        public static List<string> sharedFiles = new List<string>()
        {
            "common",
            "common_ex",
            "epgd",
            "ex",
            "pgd",
        };
        public override string[] GetEnvelopeTypes()
        {
            return new string[] {
            "lps\0"
            };
        }

        public LandPieceSettings() { }

        public LandPieceSettings(byte[] file) : base(file) { }

        public LandPieceSettings(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }

        public override void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            var startBookmark = sr.Position;
            var nof0Values = GetNOF0PointedValues(sr, offset);
            nof0Values.Sort();
            sr.Seek(startBookmark, SeekOrigin.Begin);
            header = sr.Read<LPSHeader>();

            sr.Seek(header.floatVariablesPtr + offset, SeekOrigin.Begin);
            for (int i = 0; i < header.floatVariablesCount; i++)
            {
                var strPtr = sr.Read<int>();
                float flt = sr.Read<float>();
                var bookmark = sr.Position;
                sr.Seek(strPtr + offset, SeekOrigin.Begin);
                var str = sr.ReadCString();
                if (str == null)
                {
                    str = $"value_{i}";
                }
                fVarDict.Add(str, flt);

                sr.Seek(bookmark, SeekOrigin.Begin);
            }
            sr.Seek(header.stringVariablesPtr + offset, SeekOrigin.Begin);
            for (int i = 0; i < header.stringVariablesCount; i++)
            {
                var strPtr = sr.Read<int>();
                var strPtr2 = sr.Read<int>();
                var bookmark = sr.Position;
                sr.Seek(strPtr + offset, SeekOrigin.Begin);
                var str = sr.ReadCString();
                sr.Seek(strPtr2 + offset, SeekOrigin.Begin);
                var str2 = sr.ReadCString();
                if (str == null)
                {
                    str = $"value_{i}";
                }
                stringVarDict.Add(str, str2);

                sr.Seek(bookmark, SeekOrigin.Begin);
            }
            sr.Seek(header.areaEntryExitDefaultsPtr + offset, SeekOrigin.Begin);
            for (int i = 0; i < header.areaEntryExitDefaultsCount; i++)
            {
                var strPtr = sr.Read<int>();
                var strPtr2 = sr.Read<int>();
                var bookmark = sr.Position;
                sr.Seek(strPtr + offset, SeekOrigin.Begin);
                var str = sr.ReadCString();
                sr.Seek(strPtr2 + offset, SeekOrigin.Begin);
                var str2 = sr.ReadCString();
                if (str == null)
                {
                    str = $"value_{i}";
                }
                areaEntryExitDefaults.Add(str, str2);

                sr.Seek(bookmark, SeekOrigin.Begin);
            }
            sr.Seek(header.areaEntryExitDefaults2Ptr + offset, SeekOrigin.Begin);
            for (int i = 0; i < header.areaEntryExitDefaults2Count; i++)
            {
                var strPtr = sr.Read<int>();
                var strPtr2 = sr.Read<int>();
                var bookmark = sr.Position;
                sr.Seek(strPtr + offset, SeekOrigin.Begin);
                var str = sr.ReadCString();
                sr.Seek(strPtr2 + offset, SeekOrigin.Begin);
                var str2 = sr.ReadCString();
                if (str == null)
                {
                    str = $"value_{i}";
                }
                areaEntryExitDefaults2.Add(str, str2);

                sr.Seek(bookmark, SeekOrigin.Begin);
            }
            sr.Seek(header.pieceSetsPtr + offset, SeekOrigin.Begin);
            for (int i = 0; i < header.pieceSetsCount; i++)
            {
                var pieceSetObj = new PieceSetObj();
                pieceSetObj.offset = sr.Position - offset;

                var pieceSet = sr.Read<PieceSet>();
                var bookmark = sr.Position;
                sr.Seek(pieceSet.namePtr + offset, SeekOrigin.Begin);
                pieceSetObj.name = sr.ReadCString();
                sr.Seek(pieceSet.fullNamePtr + offset, SeekOrigin.Begin);
                pieceSetObj.fullName = sr.ReadCString();

                //Read data 1 - Hack for now. Seems like it's when pieceSet.int_10 is 0xB, but hard to say
                var nofIdData1 = nof0Values.IndexOf((uint)pieceSet.data1Ptr);

                int data1Size;
                if (nofIdData1 + 1 != nof0Values.Count)
                {
                    data1Size = (int)(nof0Values[nofIdData1 + 1] - nof0Values[nofIdData1]);
                }
                else
                {
                    data1Size = pieceSet.data1Ptr == 0xB ? 0x8 : 0x4;
                }
                switch (data1Size)
                {
                    case 0x8:
                        pieceSetObj.data1 = sr.ReadBytes(pieceSet.data1Ptr + offset, 0x8);
                        break;
                    case 0x4:
                        pieceSetObj.data1 = sr.ReadBytes(pieceSet.data1Ptr + offset, 0x4);
                        break;
                    default:
                        Debug.WriteLine($"Piece Set at {pieceSetObj.offset:X} had a data 1 size of {data1Size}");
                        pieceSetObj.data1 = sr.ReadBytes(pieceSet.data1Ptr + offset, data1Size);
                        break;
                }

                //Read data 2
                var nofIdData2 = nof0Values.IndexOf((uint)pieceSet.data2Ptr);
                int data2Size;
                if (nofIdData2 + 1 != nof0Values.Count)
                {
                    data2Size = (int)(nof0Values[nofIdData2 + 1] - nof0Values[nofIdData2]);
                }
                else
                {
                    data2Size = 0x4;
                }
                switch (data2Size)
                {
                    case 0x8:
                        pieceSetObj.data2 = sr.ReadBytes(pieceSet.data2Ptr + offset, 0x8);
                        break;
                    case 0x4:
                        pieceSetObj.data2 = sr.ReadBytes(pieceSet.data2Ptr + offset, 0x4);
                        break;
                    default:
                        Debug.WriteLine($"Piece Set at {pieceSetObj.offset:X} had a data 2 size of {data2Size}");
                        pieceSetObj.data2 = sr.ReadBytes(pieceSet.data2Ptr + offset, data2Size);
                        break;
                }

                sr.Seek(bookmark, SeekOrigin.Begin);

                pieceSetObj.pieceSet = pieceSet;
                pieceSets.Add(pieceSetObj.name, pieceSetObj);
            }
        }

        //Has V3 and V4 variants. V3 encompasses the alpha version to nearly the end of classic PSO2. V4 seems to be for NGS forward.
        public struct LPSHeader
        {
            public float flt_00;
            public float flt_04;
            public float flt_08;
            public int floatVariablesCount;

            public int stringVariablesCount;
            public int areaEntryExitDefaultsCount;
            public int areaEntryExitDefaults2Count;
            public int floatVariablesPtr;

            public int stringVariablesPtr;
            public int areaEntryExitDefaultsPtr;
            public int areaEntryExitDefaults2Ptr;
            public int pieceSetsCount;

            public int pieceSetsPtr;
        }

        public class PieceSetObj
        {
            public PieceSet pieceSet;

            public long offset;
            public string name = null;
            public string fullName = null;

            public byte[] data1 = null;
            public byte[] data2 = null;
        }

        public struct PieceSet
        {
            public int namePtr;
            public ushort variantCount; //Count of files, ex ln_0310_i0_00, ln_0310_i0_01 etc. Only a single '80' file if 0?
            public ushort usht_06;
            public ushort usht_08;
            public ushort variant80Count;
            public int fullNamePtr;

            public int int_10;
            public int int_14;
            public int int_18;
            public int data1Ptr;

            public int data2Ptr;
        }
    }
}
