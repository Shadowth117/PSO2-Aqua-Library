using AquaModelLibrary.Data.Gamecube;
using System.Numerics;

namespace AquaModelLibrary.Data.CustomRoboBattleRevolution.Model.Common
{
    public class CRBRMeshData
    {
        public List<Vector3> vertPositions = new List<Vector3>();
        public List<Vector3> vertNormals = new List<Vector3>();
        public List<Vector3> vertUV1s = new List<Vector3>();
        public List<Vector3> vertUV2s = new List<Vector3>();
        
        public List<CRBRVertexDefinition> vertDefinitions = new List<CRBRVertexDefinition>();
        public List<GCPrimitive> gCPrimitives = new List<GCPrimitive>();

        public int int_00;
        public int int_04;
        public int vertexDefinitionsOffset;
        /// <summary>
        /// Always 0x80?
        /// </summary>
        public byte bt_0C;
        public byte bt_0D;
        /// <summary>
        /// Seems to correlate to the number of face primitive sets attached, but not always the actual count?
        /// Primitive sets should stop if 0 is encountered a primitive type.
        /// </summary>
        public ushort maxPrimitiveCount;
        public int primitiveSetsOffset;
        public int int_14;
        public int int_18;
        public int int_1C;

        public CRBRMeshData() { }
    }
}
