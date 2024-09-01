using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.CustomRoboBattleRevolution.Model.Common
{
    public class CRBRMesh
    {
        public CRBRMeshData meshData = null;
        public CRBRMaterialData matData = null;

        public int int_00;
        public int int_04;
        public int materialDataOffset;
        public int meshDataOffset;

        public CRBRMesh() { }
        
        public CRBRMesh(BufferedStreamReaderBE<MemoryStream> sr, int offset, CRBRModel model)
        {
            int_00 = sr.ReadBE<int>();
            int_04 = sr.ReadBE<int>();
            materialDataOffset = sr.ReadBE<int>();
            meshDataOffset = sr.ReadBE<int>();

#if DEBUG
            if(int_00 != 0)
            {
                throw new NotImplementedException();
            }
            if (int_04 != 0)
            {
                throw new NotImplementedException();
            }
#endif

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
