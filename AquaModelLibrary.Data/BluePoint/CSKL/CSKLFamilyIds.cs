using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BluePoint.CSKL
{
    public class CSKLFamilyIds
    {
        public ushort boneFlag0; //Sometiems 0x100 for unknown reasons
        public ushort boneFlag1; //Sometimes 0x8000 for unknown reasons
        public int parentId = -1;
        public int firstChildId = -1;
        public int nextSiblingId = -1;

        public CSKLFamilyIds()
        {

        }

        public CSKLFamilyIds(BufferedStreamReaderBE<MemoryStream> sr)
        {
            boneFlag0 = sr.Read<ushort>();
            boneFlag1 = sr.Read<ushort>();
            parentId = sr.Read<int>();
            firstChildId = sr.Read<int>();
            nextSiblingId = sr.Read<int>();
        }
    }
}
