using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UnluacNET;
using Vector3Integer;

namespace AquaModelLibrary.BluePoint.CMSH
{
    public class CMSHFaceData
    {
        public int flags;
        public int indexCount;

        //If vertCount exceeds 0xFFFF, these are ints
        public List<int> rawFaces = new List<int>();
        public List<Vector3Int.Vec3Int> faceList = new List<Vector3Int.Vec3Int>();

        public CMSHFaceData()
        {

        }

        public CMSHFaceData(BufferedStreamReader sr, CMSHHeader header, int vertCount)
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
    }
}
