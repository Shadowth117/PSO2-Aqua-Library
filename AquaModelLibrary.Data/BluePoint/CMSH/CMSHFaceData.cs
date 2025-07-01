using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Data.DataTypes;
using AquaModelLibrary.Helpers.Extensions;

namespace AquaModelLibrary.Data.BluePoint.CMSH
{
    public class CMSHFaceData
    {
        public int flags;
        public int indexCount;

        //If vertCount exceeds 0xFFFF, these are ints
        public List<Vector3Int.Vec3Int> faceList = new List<Vector3Int.Vec3Int>();

        public CMSHFaceData() {}

        public CMSHFaceData(BufferedStreamReaderBE<MemoryStream> sr, CMSHHeader header, int vertCount)
        {
            flags = sr.Read<int>();
            indexCount = sr.Read<int>();
            bool useInts = vertCount > ushort.MaxValue;

            for (int i = 0; i < indexCount / 3; i++)
            {
                switch (useInts)
                {
                    case true:
                        faceList.Add(Vector3Int.Vec3Int.CreateVec3Int(sr.Read<int>(), sr.Read<int>(), sr.Read<int>()));
                        break;
                    case false:
                        faceList.Add(Vector3Int.Vec3Int.CreateVec3Int(sr.Read<ushort>(), sr.Read<ushort>(), sr.Read<ushort>()));
                        break;
                }
            }
        }

        public byte[] GetBytes(int vertCount)
        {
            List<byte> outBytes = new List<byte>();
            outBytes.AddValue(flags);
            outBytes.AddValue(faceList.Count * 3);
            bool useInts = vertCount > ushort.MaxValue;

            foreach (var face in faceList)
            {
                if(useInts)
                {
                    outBytes.AddValue(face.X);
                    outBytes.AddValue(face.Y);
                    outBytes.AddValue(face.Z);
                } else
                {
                    outBytes.AddValue((ushort)face.X);
                    outBytes.AddValue((ushort)face.Y);
                    outBytes.AddValue((ushort)face.Z);
                }
            }
            return outBytes.ToArray();
        }
    }
}
