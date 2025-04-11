namespace AquaModelLibrary.Data.BluePoint
{
    public enum BPEra : int
    {
        None = -1,
        /// <summary>
        /// Object sizes are seemingly VLQs. https://en.wikipedia.org/wiki/Variable-length_quantity
        /// TLDR, you read byte by byte and if the quantity goes above 0x80, you move up one more byte
        /// </summary>
        DemonsSouls = 0,
        /// <summary>
        /// Object sizes are int16s or uint16s
        /// </summary>
        SOTC = 1,
    }
}
