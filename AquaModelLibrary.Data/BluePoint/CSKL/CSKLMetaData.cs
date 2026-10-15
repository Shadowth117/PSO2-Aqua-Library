using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BluePoint.CSKL
{
    public class CSKLMetaData
    {
        public ushort usht_00;
        /// <summary>
        /// Always 0x1C?
        /// </summary>
        public ushort structSize = 0x1C; 
        public int boneHashOffset; //Same as above?
        public int morphTargetHashOffset;
        public int int_0C;

        public int int_10;
        public int familyIdListOffset;
        public int csklNamesOffset;

        public List<uint> skeletonHashes = new();
        public List<uint> morphTargetHashes = new();
        public List<CSKLFamilyIds> familyIds = new List<CSKLFamilyIds>();

        //Used as the prime basis for the name FNV hashes. For bones, the default is used
        public const uint MorphTargetFNVPrime = 0x2F3463F2;
        public const uint CameraFNVPrime = 0xE4BFEB65;


        public CSKLMetaData() { }

        public CSKLMetaData(BufferedStreamReaderBE<MemoryStream> sr, int boneCount, int morphTargetCount)
        {
            var start = sr.Position;

            usht_00 = sr.Read<ushort>();
            structSize = sr.Read<ushort>();
            boneHashOffset = sr.Read<int>();
            morphTargetHashOffset = sr.Read<int>();
            int_0C = sr.Read<int>();

            int_10 = sr.Read<int>();
            familyIdListOffset = sr.Read<int>();
            csklNamesOffset = sr.Read<int>();

            var absoluteBoneHashListOffset = boneHashOffset + start;
            var absoluteMorphHashListOffset = morphTargetHashOffset + start;
            var absoluteFamilyIdListOffset = familyIdListOffset + start;
            var absoluteCsklNamesOffset = csklNamesOffset + start;

            sr.Seek(absoluteBoneHashListOffset, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < boneCount; i++)
            {
                skeletonHashes.Add(sr.Read<uint>());
            }
            sr.Seek(absoluteMorphHashListOffset, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < morphTargetCount; i++)
            {
                morphTargetHashes.Add(sr.Read<uint>());
            }
            sr.Seek(absoluteFamilyIdListOffset, System.IO.SeekOrigin.Begin);
            for (int i = 0; i < boneCount; i++)
            {
                familyIds.Add(new CSKLFamilyIds(sr));
            }

            sr.Seek(absoluteCsklNamesOffset, System.IO.SeekOrigin.Begin);
        }
    }
}
