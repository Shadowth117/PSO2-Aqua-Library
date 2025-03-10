namespace AquaModelLibrary.Data.Capcom.MonsterHunter
{
    public enum MHMainSection : uint
    {
        Model = 0x2,
        Mesh = 0x4,
        Material = 0x9,
        UnkSectionA = 0xA,
        Skeleton = 0xC0000000,
    }

    public enum AMOSection : ushort
    {
        Tristrips = 0x4,
        Position = 0x7,
        Normal = 0x8,
        UV = 0xA,
        /// <summary>
        /// RGBA Colors, colors are stored in range 0-255.0F
        /// </summary>
        Color = 0xB,
        /// <summary>
        /// Values have an int count and then an array of int index + float pairs for the count per vertex
        /// Weights are totaled to 100.0f
        /// </summary>
        Weights = 0xC,
    }
    /// <summary>
    /// .amo and .ahi files are the same setup with different sections.
    /// They all start with a MainSection type, a subsection count, and a size of the section including this 0xC size header.
    /// Subsections are the same except they seem to have a 0ed int16 and then be defined by a secondary int16 for the first 4 bytes.
    /// </summary>
    public class AMO
    {
        public static uint AMOMagic = 0x1;
    }
}
