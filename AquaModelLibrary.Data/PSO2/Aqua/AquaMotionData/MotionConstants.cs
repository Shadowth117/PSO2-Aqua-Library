namespace AquaModelLibrary.Data.PSO2.Aqua.AquaMotionData
{
    public class MotionConstants
    {
        public const int CAMO = 0x4F4D4143; //Camera animation
        public const int SPMO = 0x4F4D5053; //UV animation
        public const int NDMO = 0x4F4D444E; //3d Motion animation
        public const int stdAnim = 0x10002;
        public const int stdPlayerAnim = 0x10012;
        public const int cameraAnim = 0x10004;
        public const int materialAnim = 0x20;
        public const int UshortThreshold = 4095; //Threshold to change over to uints for timing. Times are multiplied by 0x10 and so 65535 / 0x10 leaves 4095 as the integer sans remainder

        public static readonly Dictionary<int, string> keyTypeNames = new Dictionary<int, string>()
        {
            {0x1, "0x1 Position" },  //Standard, Camera
            {0x2, "0x2 Rotation" },  //Standard
            {0x3, "0x3 Scale" },     //Standard
            {0x4, "0x4 Unk Floats" }, //Camera
            {0x8, "0x8 Unk Vector4s" }, //Alternate UV
            {0x9, "0x9 Unk Vector4s" }, //Alternate UV
            {0xA, "0xA Unk Vector4s" }, //Alternate UV
            {0xB, "0xB Unk Vector4s" }, //Alternate UV
            {0xC, "0xC Unk Vector4s" }, //Alternate UV
            {0xD, "0xD Unk Vector4s" }, //Alternate UV
            {0xE, "0xE Unk Vector4s" }, //Alternate UV
            //0x5-0x7 and 0xD-0x13 seemingly not used in this format. Effects seem to use values in this range for equivalent area there. 
            //In addition, the way those are constructed is akin to an alternative or early version of the overall motion format, based on namings and other bits.  
            {0x10, "0x10 NodeTreeFlag Ints 1" }, //Standard (Player)
            {0x11, "0x11 NodeTreeFlag Ints 2" }, //Standard (Player)
            {0x12, "0x12 NodeTreeFlag Ints 3" }, //Standard (Player)
            {0x14, "0x14 Unk Floats" }, //Camera
            {0x15, "0x15 Unk Floats" }, //Camera
            {0x16, "0x16 Unk Floats" }, //UV
            {0x17, "0x17 Unk Floats" }, //UV
            {0x18, "0x18 Unk Floats" }, //UV
            {0x19, "0x19 Unk Floats" }, //UV
            {0x1A, "0x1A Unk Floats" }, //UV
            {0x1B, "0x1B Unk Floats" }, //UV
            {0x1C, "0x1C Unk Floats" }, //UV
            {0x1D, "0x1D Unk Floats" }, //UV
            {0x1E, "0x1E Unk Floats" }, //Alternate UV
            {0x1F, "0x1F Unk Floats" }, //Alternate UV
            {0x20, "0x20 Unk Floats" }, //Alternate UV
            {0x21, "0x21 Unk Floats" } //Alternate UV
        };
        public static readonly List<int> standardTypes = new List<int>()
        {
            0x1,
            0x2,
            0x3,
            0x10,
            0x11,
            0x12
        };
        public static readonly List<int> cameraTypes = new List<int>()
        {
            0x1,
            0x4,
            0x14,
            0x15
        };
        public static readonly List<int> materialTypes = new List<int>()
        {
            0x8,
            0x9,
            0xA,
            0xB,
            0xC,
            0xD,
            0xE,
            0x16,
            0x17,
            0x18,
            0x19,
            0x1A,
            0x1B,
            0x1C,
            0x1D,
            0x1E,
            0x1F,
            0x20,
            0x21
        };

        public static int GetKeyDataType(int keyType)
        {
            switch (keyType)
            {
                case 0x1:
                    return 0x1;
                case 0x2:
                    return 0x3;
                case 0x3:
                case 0x4:
                    return 0x1;
                case 0x8:
                case 0x9:
                    return 0x2;
                case 0x10:
                case 0x11:
                case 0x12:
                    return 0x5;
                case 0xA:
                    return 0x1;
                case 0xB:
                case 0xC:
                case 0xD:
                case 0xE:
                    return 0x2;
                case 0x14:
                case 0x15:
                    return 0x6;
                case 0x16:
                case 0x17:
                case 0x18:
                case 0x19:
                case 0x1A:
                case 0x1B:
                case 0x1C:
                case 0x1D:
                    return 0x4;
                default:
                    System.Console.WriteLine($"Unknown key type: {keyType}. Returning 1");
                    return 0x1;
            }
        }
    }
}
