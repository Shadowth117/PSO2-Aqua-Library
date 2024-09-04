using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.CustomRoboBattleRevolution.Model.Common
{
    public class CRBRMesh
    {
        public CRBRNode nextCRBRNode = null;
        public CRBRMesh nextCRBRMesh = null;

        public CRBRMeshData meshData = null;
        public CRBRMaterialData matData = null;

        public int nextCRBRNodeOffset;
        public int nextCRBRMeshOffset;
        public int materialDataOffset;
        public int meshDataOffset;

        public CRBRMesh() { }
        
        public CRBRMesh(BufferedStreamReaderBE<MemoryStream> sr, int offset, CRBRModel model)
        {
            nextCRBRNodeOffset = sr.ReadBE<int>();
            nextCRBRMeshOffset = sr.ReadBE<int>();
            materialDataOffset = sr.ReadBE<int>();
            meshDataOffset = sr.ReadBE<int>();

            if(nextCRBRNodeOffset != 0)
            {
                sr.Seek(nextCRBRNodeOffset + offset, SeekOrigin.Begin);
                nextCRBRNode = new CRBRNode(sr, offset, model);
            }

            if (nextCRBRMeshOffset != 0)
            {
                sr.Seek(nextCRBRMeshOffset + offset, SeekOrigin.Begin);
                nextCRBRMesh = new CRBRMesh(sr, offset, model);
            }

            if (materialDataOffset != 0)
            {
                sr.Seek(materialDataOffset + offset, SeekOrigin.Begin);
                matData = new CRBRMaterialData(sr, offset, model);
            }
            if (meshDataOffset != 0)
            {
                sr.Seek(meshDataOffset + offset, SeekOrigin.Begin);
                meshData = new CRBRMeshData(sr, offset);
            }
        }
    }
}
