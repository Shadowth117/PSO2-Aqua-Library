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
