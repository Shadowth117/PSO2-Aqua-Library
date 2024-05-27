namespace SoulsFormats.Formats.Morpheme.MorphemeBundle
{
    /// <summary>
    /// The MorphemeBundle header. Many times this is simply 0ed out.
    /// TODO: Detail this if it's ever figured out.
    /// </summary>
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public struct MorphemeBundleGUID
    {
        public int GUID_00;
        public int GUID_04;
        public int GUID_08;
        public int GUID_0C;
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
