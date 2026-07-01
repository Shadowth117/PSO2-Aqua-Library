using NvTriStripDotNet;
using System.Numerics;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData
{
    public class StripData
    {
        public bool format0xC31 = false;
        public int triIdCount; //0xB7, type 0x9 
        public int reserve0;
        public int reserve1;
        public int reserve2;

        //The strip data is in a separate place in 0xC33, but will be placed here for convenience
        //Triangles should be interpreted as 0, 1, 2 followed by 0, 2, 1. While this results in degenerate faces, wireframe views ingame show they are rendered with these.
        public List<ushort> triStrips = new List<ushort>(); //0xB8, type 0x86 

        public List<Vector3> largeTriSet = new List<Vector3>(); //Shouldn't normally be used, mainly for special cases.

        public List<int> faceGroups = new List<int>(); //Count should match parent PSET's faceGroupCount. Unknown use. Seemingly groups sets of faces by order.

        public StripData()
        {
        }

        public StripData(Dictionary<int, object> psetRaw)
        {
            triIdCount = (int)psetRaw[0xB7];
            triStrips = ((ushort[])psetRaw[0xB8]).ToList();
        }

        public StripData(ushort[] indices)
        {
            toStrips(indices);
        }

        public void toStrips(ushort[] indices)
        {
            indices = RemoveDegenerateFaces(indices).ToArray();

            NvStripifier stripifier = new NvStripifier();

            var nvStrips = stripifier.GenerateStripsReturner(indices, true);
            triIdCount = nvStrips[0].Indices.Length; //Should in theory be twice the actual length as it's counting bytes.
            triStrips.Clear();
            triStrips = nvStrips[0].Indices.ToList();
        }

        public static List<ushort> RemoveDegenerateFaces(ushort[] indices)
        {
            List<ushort> newIndices = new List<ushort>();
            for (int i = 0; i < indices.Length; i += 3)
            {
                if (indices[i] != indices[i + 1] && indices[i] != indices[i + 2] && indices[i + 1] != indices[i + 2])
                {
                    newIndices.Add(indices[i]);
                    newIndices.Add(indices[i + 1]);
                    newIndices.Add(indices[i + 2]);
                }
            }

            return newIndices;
        }

        public List<Vector3> GetTriangles(bool removeDegenFaces = true)
        {
            if (largeTriSet.Count > 0)
            {
                return largeTriSet;
            }
            List<Vector3> tris = new List<Vector3>();
            if (format0xC31 == false)
            {
                for (int vertIndex = 0; vertIndex < triStrips.Count - 2; vertIndex++)
                {
                    //A degenerate triangle is a triangle with two references to the same vertex index. 
                    if (removeDegenFaces)
                    {
                        if (triStrips[vertIndex] == triStrips[vertIndex + 1] || triStrips[vertIndex] == triStrips[vertIndex + 2]
                            || triStrips[vertIndex + 1] == triStrips[vertIndex + 2])
                        {
                            continue;
                        }
                    }

                    //When index is odd, flip
                    if ((vertIndex & 1) > 0)
                    {
                        tris.Add(new Vector3(triStrips[vertIndex], triStrips[vertIndex + 2], triStrips[vertIndex + 1]));
                    }
                    else
                    {
                        tris.Add(new Vector3(triStrips[vertIndex], triStrips[vertIndex + 1], triStrips[vertIndex + 2]));
                    }
                }

            }
            else
            {
                //0xC33 really just uses normal triangles. Yup.
                for (int vertIndex = 0; vertIndex < triStrips.Count - 2; vertIndex += 3)
                {
                    tris.Add(new Vector3(triStrips[vertIndex], triStrips[vertIndex + 1], triStrips[vertIndex + 2]));
                }
            }

            return tris;
        }

        public class FaceGroupData
        {
            public List<Vector3> triangles = new List<Vector3>();
            public List<int> vertexMappingList = null;

            public FaceGroupData() { }

            public FaceGroupData(List<Vector3> triangles_, List<int> vertexMappingList_)
            {
                triangles = triangles_;
                vertexMappingList = vertexMappingList_;
            }

            public int GetVertexMapping(int vertId)
            {
                return vertexMappingList == null ? vertId : vertexMappingList[vertId];
            }
        }

        public FaceGroupData GetTrianglesFaceGroup(int faceGroupId, bool removeDegenFaces = true)
        {
            bool multipleUsedFaceGroups = false;
            var prevfgCount = 0;
            foreach(var fgCount in faceGroups)
            {
                if(prevfgCount > 0)
                {
                    multipleUsedFaceGroups = true;
                    break;
                }
                prevfgCount = fgCount;
            }

            //Get the sum of previous face groups to skip to the current one
            int priorGroupFaceSum = 0;
            for(int i = 0; i < faceGroupId; i++)
            {
                priorGroupFaceSum += faceGroups[i];
            }
            int faceGroupCount = faceGroups[faceGroupId];
            
            List<Vector3> tris = new List<Vector3>();
            if (largeTriSet.Count > 0)
            {
                for (int triIndex = priorGroupFaceSum / 3; triIndex < ((priorGroupFaceSum / 3) + (faceGroupCount / 3)); triIndex++)
                {
                    tris.Add(largeTriSet[triIndex]);
                }
            } else if (format0xC31 == false)
            {
                for (int vertIndex = priorGroupFaceSum; vertIndex < (priorGroupFaceSum + faceGroupCount - 2); vertIndex++)
                {
                    //A degenerate triangle is a triangle with two references to the same vertex index. 
                    if (removeDegenFaces)
                    {
                        if (triStrips[vertIndex] == triStrips[vertIndex + 1] || triStrips[vertIndex] == triStrips[vertIndex + 2]
                            || triStrips[vertIndex + 1] == triStrips[vertIndex + 2])
                        {
                            continue;
                        }
                    }

                    //When index is odd, flip
                    if ((vertIndex & 1) > 0)
                    {
                        tris.Add(new Vector3(triStrips[vertIndex], triStrips[vertIndex + 2], triStrips[vertIndex + 1]));
                    }
                    else
                    {
                        tris.Add(new Vector3(triStrips[vertIndex], triStrips[vertIndex + 1], triStrips[vertIndex + 2]));
                    }
                }
            }
            else
            {
                //0xC33 really just uses normal triangles. Yup.
                for (int vertIndex = priorGroupFaceSum; vertIndex < (priorGroupFaceSum + faceGroupCount - 2); vertIndex += 3)
                {
                    tris.Add(new Vector3(triStrips[vertIndex], triStrips[vertIndex + 1], triStrips[vertIndex + 2]));
                }
            }

            //Generate vertex mappings if we're splitting a facegroup so we can treat it as a separate mesh on export
            //Adjust faces as well to match new mapping
            List<int> vertexMappingList = null;
            if (multipleUsedFaceGroups)
            {
                vertexMappingList = new List<int>();
                Dictionary<int, int> vertexMapping = new Dictionary<int, int>();
                int vertCounter = 0;
                for(int i = 0; i < tris.Count; i++)
                {
                    Vector3 newTri = new Vector3();
                    if (!vertexMapping.ContainsKey((int)tris[i].X))
                    {
                        vertexMapping.Add((int)tris[i].X, vertCounter++);
                        vertexMappingList.Add((int)tris[i].X);
                    }
                    newTri.X = vertexMapping[(int)tris[i].X];
                    if (!vertexMapping.ContainsKey((int)tris[i].Y))
                    {
                        vertexMapping.Add((int)tris[i].Y, vertCounter++);
                        vertexMappingList.Add((int)tris[i].Y);
                    }
                    newTri.Y = vertexMapping[(int)tris[i].Y];
                    if (!vertexMapping.ContainsKey((int)tris[i].Z))
                    {
                        vertexMapping.Add((int)tris[i].Z, vertCounter++);
                        vertexMappingList.Add((int)tris[i].Z);
                    }
                    newTri.Z = vertexMapping[(int)tris[i].Z];

                    tris[i] = newTri;
                }
            }

            return new FaceGroupData(tris, vertexMappingList);
        }

        public StripData Clone()
        {
            StripData newStrip = new StripData();
            newStrip.format0xC31 = format0xC31;
            newStrip.triIdCount = triIdCount;
            newStrip.reserve0 = reserve0;
            newStrip.reserve1 = reserve1;
            newStrip.reserve2 = reserve2;
            newStrip.triStrips = new List<ushort>(triStrips);
            newStrip.faceGroups = new List<int>(faceGroups);
            newStrip.largeTriSet = new List<Vector3>(largeTriSet);
            newStrip.faceGroups = new List<int>(faceGroups);

            return newStrip;
        }
    }
}
