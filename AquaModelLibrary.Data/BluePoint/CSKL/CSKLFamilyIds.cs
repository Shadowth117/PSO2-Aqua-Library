using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers.Writers;

namespace AquaModelLibrary.Data.BluePoint.CSKL
{
    public class CSKLFamilyIds
    {
        /// <summary>
        /// Partition index * 0x100
        /// </summary>
        public ushort partitionIndex;
        /// <summary>
        /// 0x8000 means this isn't a normal animated bone, so usually a jiggle bone or similar
        /// </summary>
        public ushort boneFlags;
        public int parentId = -1;
        public int firstChildId = -1;
        public int nextSiblingId = -1;

        public CSKLFamilyIds()
        {

        }

        public CSKLFamilyIds(BufferedStreamReaderBE<MemoryStream> sr)
        {
            partitionIndex = sr.Read<ushort>();
            boneFlags = sr.Read<ushort>();
            parentId = sr.Read<int>();
            firstChildId = sr.Read<int>();
            nextSiblingId = sr.Read<int>();
        }

        public byte[] GetBytes()
        {
            ByteListWriter outBytes = new();
            outBytes.AddValue(partitionIndex);
            outBytes.AddValue(boneFlags);
            outBytes.AddValue(parentId);
            outBytes.AddValue(firstChildId);
            outBytes.AddValue(nextSiblingId);

            return outBytes.ToArray();
        }
    }
}
