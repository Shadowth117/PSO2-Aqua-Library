using AquaModelLibrary.Data.Gamecube;
using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

namespace AquaModelLibrary.Data.CustomRoboBattleRevolution.Model.Common
{
    public class CRBRMeshData
    {
        public List<Vector3> vertPositions = new List<Vector3>();
        public List<byte[]> vertColors = new List<byte[]>();
        public List<Vector3> vertNormals = new List<Vector3>();
        public List<Vector2> vertUV1s = new List<Vector2>();
        public List<Vector2> vertUV2s = new List<Vector2>();

        public List<CRBRVertexDefinition> vertDefinitions = new List<CRBRVertexDefinition>();
        public List<GCPrimitive> gcPrimitives = new List<GCPrimitive>();

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

        public CRBRMeshData(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            int_00 = sr.ReadBE<int>();
            int_04 = sr.ReadBE<int>();
            vertexDefinitionsOffset = sr.ReadBE<int>();
            bt_0C = sr.ReadBE<byte>();
            bt_0D = sr.ReadBE<byte>();
            maxPrimitiveCount = sr.ReadBE<ushort>();
            primitiveSetsOffset = sr.ReadBE<int>();
            int_14 = sr.ReadBE<int>();
            int_18 = sr.ReadBE<int>();
            int_1C = sr.ReadBE<int>();

            GCIndexAttributeFlags indexFlags = 0;
            if (vertexDefinitionsOffset != 0)
            {
                sr.Seek(vertexDefinitionsOffset + offset, SeekOrigin.Begin);
                CRBRVertexType dataType = (CRBRVertexType)1;
                do
                {
                    CRBRVertexDefinition vd = new CRBRVertexDefinition();
                    dataType = vd.dataType = sr.ReadBE<CRBRVertexType>();
                    vd.size = sr.ReadBE<int>();
                    vd.int_08 = sr.ReadBE<int>();
                    vd.int_0C = sr.ReadBE<int>();
                    vd.strideInBytes = sr.ReadBE<int>();
                    vd.dataOffset = sr.ReadBE<int>();

                    switch (dataType)
                    {
                        case CRBRVertexType.Position:
                            indexFlags |= GCIndexAttributeFlags.HasPosition;
                            if (vd.size == 3)
                            {
                                indexFlags |= GCIndexAttributeFlags.Position16BitIndex;
                            }
                            break;
                        case CRBRVertexType.Normal:
                            indexFlags |= GCIndexAttributeFlags.HasNormal;
                            if (vd.size == 3)
                            {
                                indexFlags |= GCIndexAttributeFlags.Normal16BitIndex;
                            }
                            break;
                        case CRBRVertexType.Color:
                            indexFlags |= GCIndexAttributeFlags.HasColor;
                            if (vd.size == 3)
                            {
                                indexFlags |= GCIndexAttributeFlags.Color16BitIndex;
                            }
                            break;
                        case CRBRVertexType.UV1:
                            indexFlags |= GCIndexAttributeFlags.HasUV;
                            if (vd.size == 3)
                            {
                                indexFlags |= GCIndexAttributeFlags.UV16BitIndex;
                            }
                            break;
                        case CRBRVertexType.UV2:
                            indexFlags |= GCIndexAttributeFlags.HasUV2;
                            if (vd.size == 3)
                            {
                                indexFlags |= GCIndexAttributeFlags.UV2_16BitIndex;
                            }
                            break;
                        case CRBRVertexType.None:
                        case CRBRVertexType.End:
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    vertDefinitions.Add(vd);
                } while ((int)dataType < 0xFF);
            }

            if (primitiveSetsOffset != 0)
            {
                sr.Seek(primitiveSetsOffset + offset, SeekOrigin.Begin);
                int posCount = 0;
                int nrmCount = 0;
                int colCount = 0;
                int uv0Count = 0;
                int uv1Count = 0;

                for (int i = 0; i > maxPrimitiveCount; i++)
                {
                    var primPeek = sr.Peek<byte>();
                    if (primPeek == 0)
                    {
                        break;
                    }
                    var prim = new GCPrimitive(sr, indexFlags);
                    gcPrimitives.Add(prim);

                    //Get max indices so we can read what we need from the vertex buffers
                    foreach(var lp in prim.loops)
                    {
                        posCount = Math.Max(lp.PositionIndex + 1, posCount);
                        nrmCount = Math.Max(lp.NormalIndex + 1, nrmCount);
                        colCount = Math.Max(lp.Color0Index + 1, colCount);
                        uv0Count = Math.Max(lp.UV0Index + 1, uv0Count);
                        uv1Count = Math.Max(lp.UV1Index + 1, uv1Count);
                    }
                }

                //Read vertex buffers
                foreach(var vd in vertDefinitions)
                {
                    sr.Seek(vd.dataOffset + offset, SeekOrigin.Begin);
                    switch(vd.dataType)
                    {
                        case CRBRVertexType.Position:
                            for(int i = 0; i < posCount; i++)
                            {
                                vertPositions.Add(sr.ReadBEV3());
                            }
                            break;
                        case CRBRVertexType.Normal:
                            for (int i = 0; i < nrmCount; i++)
                            {
                                vertNormals.Add(sr.ReadBEV3());
                            }
                            break;
                        case CRBRVertexType.Color:
                            for (int i = 0; i < colCount; i++)
                            {
                                vertColors.Add(sr.Read4Bytes());
                            }
                            break;
                        case CRBRVertexType.UV1:
                            for (int i = 0; i < uv0Count; i++)
                            {
                                vertUV1s.Add(sr.ReadBEV2());
                            }
                            break;
                        case CRBRVertexType.UV2:
                            for (int i = 0; i < uv1Count; i++)
                            {
                                vertUV2s.Add(sr.ReadBEV2());
                            }
                            break;
                    }
                }
            }
        }
    }
}
