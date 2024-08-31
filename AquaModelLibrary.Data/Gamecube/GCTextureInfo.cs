namespace AquaModelLibrary.Data.Gamecube
{
    /// <summary>
    /// Sourced from https://wiki.tockdom.com/wiki/Image_Formats
    /// </summary>
    public class GCTextureInfo
    {
        public enum TPLFormat
        {
            I4 = 0x0,
            I8 = 0x1,
            IA4 = 0x2,
            IA8 = 0x3,
            RGB565 = 0x4,
            RGB5A3 = 0x5,
            RGBA32_RGBA8 = 0x6,
            C4_CI4 = 0x8,
            C8_CI8 = 0x9,
            C14X2_CI14x2 = 0xA,
            CMPR = 0xE,
        }

        public Dictionary<TPLFormat, int> GCBPP = new Dictionary<TPLFormat, int>()
        {
            { TPLFormat.I4, 4 },
            { TPLFormat.I8, 8 },
            { TPLFormat.IA4, 8 },
            { TPLFormat.IA8, 16 },
            { TPLFormat.RGB565, 16 },
            { TPLFormat.RGB5A3, 16 },
            { TPLFormat.RGBA32_RGBA8, 32 },
            { TPLFormat.C4_CI4, 4 },
            { TPLFormat.C8_CI8, 8 },
            { TPLFormat.C14X2_CI14x2, 16 },
            { TPLFormat.CMPR, 4 },
        };

        public Dictionary<TPLFormat, int> GCBlockWidth = new Dictionary<TPLFormat, int>()
        {
            { TPLFormat.I4, 8 },
            { TPLFormat.I8, 8 },
            { TPLFormat.IA4, 8 },
            { TPLFormat.IA8, 4 },
            { TPLFormat.RGB565, 4 },
            { TPLFormat.RGB5A3, 4 },
            { TPLFormat.RGBA32_RGBA8, 4 },
            { TPLFormat.C4_CI4, 8 },
            { TPLFormat.C8_CI8, 8 },
            { TPLFormat.C14X2_CI14x2, 4 },
            { TPLFormat.CMPR, 8 },
        };

        public Dictionary<TPLFormat, int> GCBlockHeight = new Dictionary<TPLFormat, int>()
        {
            { TPLFormat.I4, 8 },
            { TPLFormat.I8, 4 },
            { TPLFormat.IA4, 4 },
            { TPLFormat.IA8, 4 },
            { TPLFormat.RGB565, 4 },
            { TPLFormat.RGB5A3, 4 },
            { TPLFormat.RGBA32_RGBA8, 4 },
            { TPLFormat.C4_CI4, 8 },
            { TPLFormat.C8_CI8, 4 },
            { TPLFormat.C14X2_CI14x2, 4 },
            { TPLFormat.CMPR, 8 },
        };

        public Dictionary<TPLFormat, int> GCBlockSize = new Dictionary<TPLFormat, int>()
        {
            { TPLFormat.I4, 32 },
            { TPLFormat.I8, 32 },
            { TPLFormat.IA4, 32 },
            { TPLFormat.IA8, 32 },
            { TPLFormat.RGB565, 32 },
            { TPLFormat.RGB5A3, 32 },
            { TPLFormat.RGBA32_RGBA8, 64 },
            { TPLFormat.C4_CI4, 32 },
            { TPLFormat.C8_CI8, 32 },
            { TPLFormat.C14X2_CI14x2, 32 },
            { TPLFormat.CMPR, 32 },
        };
    }
}
