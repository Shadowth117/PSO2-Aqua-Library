using System.Numerics;

namespace AquaModelLibrary.Data.BluePoint.CMDL.CMDLBasic
{
    public struct CMDLBasicMesh
    {
        public Vector3 minBounding;
        public Vector3 maxBounding;
        public ushort usht0;
        public byte bt0;
        public int startingVertIndex;
        public int vertexCount;
        public int startingFaceIndex;
        public int faceIndexCount;

        /// <summary>
        /// This int is ONLY in master collection MGS cmdls!
        /// </summary>
        public int masterCollectionUnk0;

        public int unk0;
        /// <summary>
        /// Always seems to be 0x80808080 in MGS, 0xFFFFFF7F in SOTC/ICO
        /// </summary>
        public int unk80s0;
        public int unk80s1;

        //Only in MGS
        public int unk1;
        public int unk2;
        public int unk3;
    }
}
