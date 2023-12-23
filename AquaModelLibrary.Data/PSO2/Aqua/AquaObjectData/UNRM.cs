namespace AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData
{
    //UNRM Struct - Seemingly links vertices split for various reasons(vertex colors per face, UVs, etc.).
    public class UNRM
    {
        public int vertGroupCountCount;  //0xDA, type 0x9 //Amount of vertex group counts (The amount of verts for each group of vertices in the mesh ids and vert ids).
        public int vertGroupCountOffset; //Offset for listing of vertex group counts. 
        public int vertCount; //0xDC, type 0x9 //Total vertices in the mesh id and vertId data
        public int meshIdOffset;
        public int vertIDOffset;
        public double padding0;
        public int padding1;
        public List<int> unrmVertGroups = new List<int>(); //0xDB, type 0x89
                                                           //Align to 0x10
        public List<List<int>> unrmMeshIds = new List<List<int>>(); //0xDD, type 0x89
                                                                    //Align to 0x10
        public List<List<int>> unrmVertIds = new List<List<int>>(); //0xDE, type 0x89
                                                                    //Align to 0x10

        public UNRM Clone()
        {
            UNRM newUnrm = new UNRM();
            newUnrm.vertGroupCountCount = vertGroupCountCount;
            newUnrm.vertGroupCountOffset = vertGroupCountOffset;
            newUnrm.vertCount = vertCount;
            newUnrm.meshIdOffset = meshIdOffset;
            newUnrm.vertIDOffset = vertIDOffset;
            newUnrm.padding0 = padding0;
            newUnrm.padding1 = padding1;
            newUnrm.unrmVertGroups = new List<int>(unrmVertGroups);
            newUnrm.unrmMeshIds = unrmMeshIds.ConvertAll(id => new List<int>((int[])id.ToArray().Clone())).ToList();
            newUnrm.unrmVertIds = unrmVertIds.ConvertAll(id => new List<int>((int[])id.ToArray().Clone())).ToList();

            return newUnrm;
        }
    }
}
