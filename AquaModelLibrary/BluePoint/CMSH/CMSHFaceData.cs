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

        public CMSHFaceData(BufferedStreamReader sr, int vertCount, int variantFlag, int variantFlag2)
        {
            flags = sr.Read<int>();
            indexCount = sr.Read<int>();
            bool useInts = vertCount > ushort.MaxValue;

            if (variantFlag == 0x1 && variantFlag2 == 0xA)
            {
                bool flip = true;
                for (int i = 0; i < indexCount / 3; i++)
                {
                    Vector3Int.Vec3Int vec3Int;
                    if (useInts)
                    {
                        vec3Int = Vector3Int.Vec3Int.CreateVec3Int(sr.Read<int>(), sr.Read<int>(), sr.Read<int>());
                    }
                    else
                    {
                        vec3Int = Vector3Int.Vec3Int.CreateVec3Int(sr.Read<ushort>(), sr.Read<ushort>(), sr.Read<ushort>());
                    }

                    
                    if(vec3Int.X == vec3Int.Y || vec3Int.X == vec3Int.Z || vec3Int.Y == vec3Int.Z)
                    {
                        //flip = !flip;
                        //continue;
                    }

                    if (flip)
                    {
                        var temp = vec3Int.Z;
                        vec3Int.Z = vec3Int.X;
                        vec3Int.X = temp;
                    } 

                    faceList.Add(vec3Int);
                }
            } else
            {
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
}
