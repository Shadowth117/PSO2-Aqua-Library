using AquaModelLibrary.Data.Gamecube;
using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.Intermediary;
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
                    vd.indexSize = sr.ReadBE<int>();
                    vd.int_08 = sr.ReadBE<int>();
                    vd.dataFormat = sr.ReadBE<GCDataType>();
                    vd.sht_10 = sr.ReadBE<short>();
                    vd.strideInBytes = sr.ReadBE<short>();
                    vd.dataOffset = sr.ReadBE<int>();

                    switch (dataType)
                    {
                        case CRBRVertexType.Position:
                            indexFlags |= GCIndexAttributeFlags.HasPosition;
                            if (vd.indexSize == 3)
                            {
                                indexFlags |= GCIndexAttributeFlags.Position16BitIndex;
                            }
                            break;
                        case CRBRVertexType.Normal:
                            indexFlags |= GCIndexAttributeFlags.HasNormal;
                            if (vd.indexSize == 3)
                            {
                                indexFlags |= GCIndexAttributeFlags.Normal16BitIndex;
                            }
                            break;
                        case CRBRVertexType.Color:
                            indexFlags |= GCIndexAttributeFlags.HasColor;
                            if (vd.indexSize == 3)
                            {
                                indexFlags |= GCIndexAttributeFlags.Color16BitIndex;
                            }
                            break;
                        case CRBRVertexType.UV1:
                            indexFlags |= GCIndexAttributeFlags.HasUV;
                            if (vd.indexSize == 3)
                            {
                                indexFlags |= GCIndexAttributeFlags.UV16BitIndex;
                            }
                            break;
                        case CRBRVertexType.UV2:
                            indexFlags |= GCIndexAttributeFlags.HasUV2;
                            if (vd.indexSize == 3)
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

                for (int i = 0; i < maxPrimitiveCount; i++)
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
                                vertPositions.Add(ReadPositionV3(sr, vd.dataFormat));
                            }
                            break;
                        case CRBRVertexType.Normal:
                            for (int i = 0; i < nrmCount; i++)
                            {
                                vertNormals.Add(ReadNormalV3(sr, vd.dataFormat));
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

        public Vector3 ReadPositionV3(BufferedStreamReaderBE<MemoryStream> sr, GCDataType type)
        {
            switch (type)
            {
                case GCDataType.Signed16:
                    return new Vector3(sr.ReadBE<short>() / (float)0x2000, sr.ReadBE<short>() / (float)0x2000, sr.ReadBE<short>() / (float)0x2000);
                case GCDataType.Float32:
                    return sr.ReadBEV3();
                default:
                    throw new Exception();
            }
        }
        public Vector3 ReadNormalV3(BufferedStreamReaderBE<MemoryStream> sr, GCDataType type)
        {
            switch(type)
            {
                case GCDataType.Signed16:
                    return new Vector3(sr.ReadBE<short>() / (float)0x4000, sr.ReadBE<short>() / (float)0x4000, sr.ReadBE<short>() / (float)0x4000);
                case GCDataType.Float32:
                    return sr.ReadBEV3();
                default:
                    throw new Exception();
            }
        }
        public Vector2 ReadUVV2(BufferedStreamReaderBE<MemoryStream> sr, GCDataType type)
        {
            switch (type)
            {
                case GCDataType.Signed16:
                    return new Vector2(sr.ReadBE<short>() / (float)0x4000, sr.ReadBE<short>() / (float)0x4000);
                case GCDataType.Float32:
                    return sr.ReadBEV2();
                default:
                    throw new Exception();
            }
        }

        public void GatherVertexData(int nodeId, VTXL vtxl, Matrix4x4 transform)
        {
            if (vertPositions.Count > 0)
            {
                for(int i = 0; i < vertPositions.Count; i++)
                {
                    vtxl.vertPositions.Add(Vector3.Transform(vertPositions[i], transform));
                    vtxl.vertWeightIndices.Add(new int[] { nodeId, 0, 0, 0 });
                    vtxl.vertWeights.Add(new Vector4(1, 0, 0, 0));
                }
            }
            if (vertNormals.Count > 0)
            {
                for (int i = 0; i < vertNormals.Count; i++)
                {
                    vtxl.vertNormals.Add(Vector3.TransformNormal(vertNormals[i], transform));
                }
            }
            if (vertColors.Count > 0)
            {
                vtxl.vertColors.AddRange(vertColors);
            }
            if (vertUV1s.Count > 0)
            {
                vtxl.uv1List.AddRange(vertUV1s);
            }
            if (vertUV2s.Count > 0)
            {
                vtxl.uv2List.AddRange(vertUV2s);
            }

        }

        /// <summary>
        /// Takes in the mesh's parent node id, a vtxl containing vertex data, an AquaObject to store it in, an index for this mesh within this node's meshes, and a material index
        /// Most Gamecube models optimize vertex lists per data type and so faces must contain a reference to each of these optimized lists.
        /// e.g. A model with vert position data, normal data, color data, and uv1 data would need 4 indices per triangle vertex, with 12 total indices for that triangle.
        /// 
        /// Since most modern vertex data expects these to by synced up, we recreate these vertices here, taking care not to store the inevitable duplicate vertex data.
        /// </summary>
        public void GetFaceData(int nodeId, VTXL tempVtxl, AquaObject aqo, int meshCounter, int materialId)
        {
            //Set up mesh
            GenericTriangles mesh = new GenericTriangles();
            Dictionary<string, int> vertTracker = new Dictionary<string, int>();
            mesh.triList = new List<Vector3>();
            mesh.name = $"Mesh_{nodeId}_{meshCounter}";
            aqo.meshNames.Add(mesh.name);
            int f = 0;
            foreach (var triData in gcPrimitives)
            {
                var tris = triData.ToTriangles();
                for (int index = 0; index < tris.Count - 2; index += 3)
                {
                    VTXL faceVtxl = new VTXL();
                    faceVtxl.rawFaceId.Add(f);
                    faceVtxl.rawFaceId.Add(f);
                    faceVtxl.rawFaceId.Add(f++);

                    var x = AddGCVert(tempVtxl, tris[index + 2], faceVtxl, mesh, vertTracker);
                    var y = AddGCVert(tempVtxl, tris[index + 1], faceVtxl, mesh, vertTracker);
                    var z = AddGCVert(tempVtxl, tris[index + 0], faceVtxl, mesh, vertTracker);
                    mesh.triList.Add(new Vector3(x, y, z));
                    faceVtxl.rawVertId.Add(x);
                    faceVtxl.rawVertId.Add(y);
                    faceVtxl.rawVertId.Add(z);
                    mesh.matIdList.Add(materialId);

                    mesh.faceVerts.Add(faceVtxl);
                }
            }

            aqo.tempTris.Add(mesh);
        }

        private int AddGCVert(VTXL sourceVtxl, Loop loop, VTXL faceVtxl, GenericTriangles mesh, Dictionary<string, int> vertTracker)
        {
            string vertId = "";
            if (sourceVtxl.vertPositions.Count > 0)
            {
                faceVtxl.vertPositions.Add(sourceVtxl.vertPositions[loop.PositionIndex]);
                vertId += ((int)GCIndexAttributeFlags.HasPosition).ToString() + loop.PositionIndex;

                faceVtxl.vertWeightIndices.Add((int[])sourceVtxl.vertWeightIndices[loop.PositionIndex].Clone());
                faceVtxl.vertWeights.Add(sourceVtxl.vertWeights[loop.PositionIndex]);
            }
            if (sourceVtxl.vertNormals.Count > 0)
            {
                faceVtxl.vertNormals.Add(sourceVtxl.vertNormals[loop.NormalIndex]);
                vertId += ((int)GCIndexAttributeFlags.HasNormal).ToString() + loop.NormalIndex;
            }
            if (sourceVtxl.vertColors.Count > 0)
            {
                faceVtxl.vertColors.Add((byte[])sourceVtxl.vertColors[loop.Color0Index].Clone());
                vertId += ((int)GCIndexAttributeFlags.HasColor).ToString() + loop.Color0Index;
            }
            if (sourceVtxl.uv1List.Count > 0)
            {
                faceVtxl.uv1List.Add(sourceVtxl.uv1List[loop.UV0Index]);
                vertId += ((int)GCIndexAttributeFlags.HasUV).ToString() + loop.UV0Index;
            }
            if (sourceVtxl.uv2List.Count > 0)
            {
                faceVtxl.uv2List.Add(sourceVtxl.uv2List[loop.UV1Index]);
                vertId += ((int)GCIndexAttributeFlags.HasUV).ToString() + loop.UV1Index;
            }


            if (vertTracker.ContainsKey(vertId))
            {
                return vertTracker[vertId];
            }
            else
            {
                vertTracker.Add(vertId, mesh.vertCount);
                return mesh.vertCount++;
            }
        }

    }
}
