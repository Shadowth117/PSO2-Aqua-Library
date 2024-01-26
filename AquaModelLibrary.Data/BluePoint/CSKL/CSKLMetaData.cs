using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BluePoint.CSKL
{
    public class CSKLMetaData
    {
        public ushort usht_00;
        public ushort usht_02;
        public int unkInt; //Same as above?
        public int unkCount;
        public int int_0C;

        public int int_10;
        public int familyIdListOffset;
        public int csklNamesOffset;

        public List<CSKLFamilyIds> familyIds = new List<CSKLFamilyIds>();

        public CSKLMetaData()
        {

        }

        public CSKLMetaData(BufferedStreamReaderBE<MemoryStream> sr, int boneCount)
        {
            var start = sr.Position;

            usht_00 = sr.Read<ushort>();
            usht_02 = sr.Read<ushort>();
            unkInt = sr.Read<int>();
            unkCount = sr.Read<int>();
            int_0C = sr.Read<int>();

            int_10 = sr.Read<int>();
            familyIdListOffset = sr.Read<int>();
            csklNamesOffset = sr.Read<int>();

            var absoluteFamilyIdListOffset = familyIdListOffset + start;
            var absoluteCsklNamesOffset = csklNamesOffset + start;

            sr.Seek(absoluteFamilyIdListOffset, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < boneCount; i++)
            {
                familyIds.Add(new CSKLFamilyIds(sr));
            }

            sr.Seek(absoluteCsklNamesOffset, System.IO.SeekOrigin.Begin);
        }
    }
}
