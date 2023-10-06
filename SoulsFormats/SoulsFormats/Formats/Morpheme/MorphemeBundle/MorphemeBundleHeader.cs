namespace SoulsFormats.Formats.Morpheme.MorphemeBundle
{
    /// <summary>
    /// The MorphemeBundle header. Many times this is simply 0ed out.
    /// TODO: Detail this if it's ever figured out.
    /// </summary>
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public struct MorphemeBundleHeader
    {
        public int int_00;
        public int int_04;
        public int int_08;
        public int int_0C;
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
