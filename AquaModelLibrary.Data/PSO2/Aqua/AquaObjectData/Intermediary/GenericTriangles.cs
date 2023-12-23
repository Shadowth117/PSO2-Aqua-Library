using System.Numerics;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.Intermediary
{
    //Used for processing meshes for setting up compatibility with PSO2
    public class GenericTriangles
    {
        public List<VTXL> faceVerts = new List<VTXL>();
        public List<Vector3> triList = new List<Vector3>();
        /// <summary>
        /// Should have at least 1 value set for the mesh material. Otherwise, one per face. 
        /// </summary>
        public List<int> matIdList = new List<int>();
        /// <summary>
        /// For helping convert mat ids if they need to be reindexed. This is helpful if you'd rather offload remapping of material ids for whatever reason.
        /// </summary>
        public Dictionary<int, int> matIdDict = new Dictionary<int, int>();
        public List<uint> bonePalette = new List<uint>();
        public int baseMeshNodeId;
        public int baseMeshDummyId;
        public int vertCount;
        public string name = null;
        public GenericTriangles()
        {
        }

        public GenericTriangles(ushort[] tris, int[] matIds = null)
        {
            setUpVector3List(tris);
            matIdList = matIds.ToList();
        }

        public GenericTriangles(List<ushort> tris, List<int> matIds = null)
        {
            setUpVector3List(tris.ToArray());
            matIdList = matIds;
        }

        public GenericTriangles(List<Vector3> vec3s, List<int> matIds = null)
        {
            triList = vec3s;
            matIdList = matIds;
        }

        public List<ushort> toUshortList()
        {
            List<ushort> shorts = new List<ushort>();

            for (int i = 0; i < triList.Count; i++)
            {
                shorts.Add((ushort)triList[i].X);
                shorts.Add((ushort)triList[i].Y);
                shorts.Add((ushort)triList[i].Z);
            }

            return shorts;
        }

        public ushort[] toUshortArray()
        {
            List<ushort> shorts = new List<ushort>();

            for (int i = 0; i < triList.Count; i++)
            {
                shorts.Add((ushort)triList[i].X);
                shorts.Add((ushort)triList[i].Y);
                shorts.Add((ushort)triList[i].Z);
            }

            return shorts.ToArray();
        }

        public void setUpVector3List(ushort[] tris)
        {
            for (int i = 0; i < tris.Length; i += 3)
            {
                triList.Add(new Vector3(tris[i], tris[i + 1], tris[i + 2]));
            }
        }

        public bool needsSplitting()
        {
            if (matIdList != null && matIdList.Count > 1)
            {
                int firstId = matIdList[0];
                for (int i = 1; i < matIdList.Count; i++)
                {
                    if (firstId != matIdList[i])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public GenericTriangles Clone()
        {
            GenericTriangles genTri = new GenericTriangles();
            genTri.faceVerts = faceVerts.ConvertAll(vtxl => vtxl.Clone()).ToList();
            genTri.triList = new List<Vector3>(triList);
            genTri.matIdList = new List<int>(matIdList);
            genTri.matIdDict = matIdDict.Keys.ToDictionary(x => x, x => matIdDict[x]);
            genTri.bonePalette = new List<uint>(bonePalette);
            genTri.baseMeshNodeId = baseMeshNodeId;
            genTri.baseMeshDummyId = baseMeshDummyId;
            genTri.vertCount = vertCount;
            genTri.name = name;

            return genTri;
        }
    }

}
