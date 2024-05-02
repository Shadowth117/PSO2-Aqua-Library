using System.Runtime.CompilerServices;

namespace AquaModelLibrary.Data.BlueDragon
{
    public class IPKFileInfo
    {
        /// <summary>
        /// Stored in a fixed byte[0x40]
        /// </summary>
        public string filePath = null;

        public int usesCompression;
        public int compressedSize;
        public int absoluteFileOffset;
        public int uncompressedSize;

        /// <summary>
        /// Looks like a hash at a glance, but it's probably not. They appear too similar...
        /// </summary>
        public int unkInt;
        public int pad0;
        public int pad1;
        public int pad2;
    }
}
