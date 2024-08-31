using AquaModelLibrary.Data.Gamecube;
using AquaModelLibrary.Helpers.Readers;
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
                int dataType = 1;
                do
                {
                    CRBRVertexDefinition vd = new CRBRVertexDefinition();
                    dataType = vd.dataType = sr.ReadBE<int>();
                    vd.size = sr.ReadBE<int>();
                    vd.int_08 = sr.ReadBE<int>();
                    vd.int_0C = sr.ReadBE<int>();
                    vd.strideInBytes = sr.ReadBE<int>();
                    vd.dataOffset = sr.ReadBE<int>();

                    switch(dataType)
                    {
                        case 0x9:
                            indexFlags |= GCIndexAttributeFlags.HasPosition;
                            if(vd.size == 3)
                            {
                                indexFlags |= GCIndexAttributeFlags.Position16BitIndex;
                            }
                            break;
                        case 0xA:
                            indexFlags |= GCIndexAttributeFlags.HasNormal;
                            if (vd.size == 3)
                            {
                                indexFlags |= GCIndexAttributeFlags.Normal16BitIndex;
                            }
                            break;
                        case 0xB:
                            indexFlags |= GCIndexAttributeFlags.HasColor;
                            if (vd.size == 3)
                            {
                                indexFlags |= GCIndexAttributeFlags.Color16BitIndex;
                            }
                            break;
                        case 0xD:
                            indexFlags |= GCIndexAttributeFlags.HasUV;
                            if (vd.size == 3)
                            {
                                indexFlags |= GCIndexAttributeFlags.UV16BitIndex;
                            }
                            break;
                        case 0xE:
                            indexFlags |= GCIndexAttributeFlags.HasUV2;
                            if (vd.size == 3)
                            {
                                indexFlags |= GCIndexAttributeFlags.UV2_16BitIndex;
                            }
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    vertDefinitions.Add(vd);
                } while (dataType > 0 && dataType < 0xFF);
            }

            if(primitiveSetsOffset != 0)
            {
                sr.Seek(primitiveSetsOffset + offset, SeekOrigin.Begin);
                for(int i = 0; i > maxPrimitiveCount; i++)
                {
                    var primPeek = sr.Peek<byte>();
                    if(primPeek == 0)
                    {
                        break;
                    }
                    gcPrimitives.Add(new GCPrimitive(sr, indexFlags));
                }
            }
        }
    }
}
