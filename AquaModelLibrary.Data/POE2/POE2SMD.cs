using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.Intermediary;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Helpers.Readers;
using System.Numerics;
using Half = AquaModelLibrary.Data.DataTypes.Half;
using static AquaModelLibrary.Data.POE2.POE2SMD;

namespace AquaModelLibrary.Data.POE2
{
    [Flags]
    public enum VertexData : int
    {
        Color = 0x1,
        UV2 = 0x2,       //Always 0 in the file found. Might not be a uv at all
        Weights = 0x4,
        UV = 0x8,        //UV, Normal, and Position might be mixed up as they seem to all be used every time
        Normal = 0x10,
        Position = 0x20,
    }

    public class POE2Model
    {
        public List<POE2ModelLOD> modelLodList = new List<POE2ModelLOD>();
    }


    public class POE2ModelLOD
    {
        public List<List<int>> meshIndices = new List<List<int>>();
        public List<POE2Vertex> vertices = new List<POE2Vertex>();
    }

    //Mesh data is stored here. Skeletal data is stored with animations
    //SMD is SkinnedMeshData while TMD is TerrainMeshData
    public class POE2SMD
    {
        public List<POE2Model> models = new List<POE2Model>();
        public POE2SMD() { }

        public POE2SMD(byte[] file)
        {
            Read(file);
        }

        public POE2SMD(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }
        public void Read(byte[] file)
        {
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }
        public void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            sr.Seek(0x20, SeekOrigin.Begin);
            var magic2 = sr.ReadBE<int>();
            bool isSMD = false;
            if (magic2 == 0x6D4C4F44) //DOLm
            {
                isSMD = true;
            }
            else if (magic2 == 0x026D4C4F)
            {
                isSMD = false;
            }
            sr.Seek(0x0, SeekOrigin.Begin);

            //TMD models are similar, but begin slightly different. They are ultimately offset 1 byte from how an SMD would be.
            int startValue0 = 0;
            int smdStartValue1 = 0;
            if (isSMD)
            {
                startValue0 = sr.ReadBE<int>();
                smdStartValue1 = sr.ReadBE<int>();
            } else
            {
                startValue0 = sr.ReadBE<byte>();
            }

            Vector3 minBoundingCorner = sr.ReadBEV3();
            Vector3 maxBoundingCorner = sr.ReadBEV3();

            //TMD had some specific values to it
            ushort tmdBoundCount = 0;
            ushort extraDOLms = 0;
            ushort tmdValue2 = 0;
            int shouldReadSecondModel = 0;
            if(!isSMD)
            {
                //Should be one bound per 'mesh'
                tmdBoundCount = sr.ReadBE<ushort>();
                extraDOLms = sr.ReadBE<ushort>();
                tmdValue2 = sr.ReadBE<ushort>();
                shouldReadSecondModel = extraDOLms > 0 ? 1 : 0;
            }

            for(var mdl = 0; mdl < 1 + shouldReadSecondModel; mdl++)
            {
                POE2Model model = new POE2Model();
                if (mdl != 0)
                {
                    //This is the index for the bounds
                    for(int bd = 0; bd < tmdBoundCount; bd++)
                    {
                        sr.ReadBE<ushort>();
                        minBoundingCorner = sr.ReadBEV3();
                        maxBoundingCorner = sr.ReadBEV3();
                    }
                }
                byte[] DOLm = sr.Read4Bytes();
                if (DOLm[2] != 0x4C || DOLm[3] != 0x6D)
                {
                    break;
                }
                byte bt_24 = sr.ReadBE<byte>();

                //For some reason it seems like you can have an empty model and then another magic that starts a proper model after?
                if (sr.Peek<int>() == 0)
                {
                    sr.ReadBE<int>();
                    sr.ReadBE<int>();
                    sr.ReadBE<int>();
                    sr.ReadBE<byte>();
                }
                byte bt_25 = sr.ReadBE<byte>();
                byte lodCount = sr.ReadBE<byte>();
                byte meshCount = sr.ReadBE<byte>();
                byte bt_28 = sr.ReadBE<byte>();
                VertexData vertexFormat = (VertexData)sr.ReadBE<int>();
                List<LODInfo> lodInfoList = new List<LODInfo>();

                for (int i = 0; i < lodCount; i++)
                {
                    lodInfoList.Add(new LODInfo() { int_00 = sr.ReadBE<int>(), vertexCount = sr.ReadBE<int>() });
                }

                for (int l = 0; l < lodCount; l++)
                {
                    LODInfo info = lodInfoList[l];
                    POE2ModelLOD lod = new POE2ModelLOD();
                    List<int> meshStartIndex = new List<int>();
                    List<int> meshIndexCount = new List<int>();
                    bool[] meshUsesIntIndices = new bool[meshCount];
                    bool intIndices = false;
                    for (int i = 0; i < meshCount; i++)
                    {
                        meshStartIndex.Add(sr.ReadBE<int>());
                        meshIndexCount.Add(sr.ReadBE<int>());
                        if (meshStartIndex[i] > ushort.MaxValue || meshIndexCount[i] > ushort.MaxValue)
                        {
                            meshUsesIntIndices[i] = true;
                            intIndices = true;
                        }
                    }

                    long indicesStart = sr.Position;
                    for (int i = 0; i < meshCount; i++)
                    {
                        List<int> mesh = new List<int>();
                        sr.Seek(indicesStart + (intIndices ? 4 : 2) * meshStartIndex[i], SeekOrigin.Begin);
                        for (int j = 0; j < meshIndexCount[i]; j++)
                        {
                            if (meshUsesIntIndices[i])
                            {
                                mesh.Add(sr.ReadBE<int>());
                            }
                            else
                            {
                                mesh.Add(sr.ReadBE<ushort>());
                            }
                        }

                        lod.meshIndices.Add(mesh);
                    }

                    for (int i = 0; i < info.vertexCount; i++)
                    {
                        POE2Vertex vertex = new POE2Vertex();
                        if ((vertexFormat & VertexData.Position) > 0)
                        {
                            vertex.position = sr.ReadBE<Vector3>();
                        }
                        if ((vertexFormat & VertexData.Normal) > 0)
                        {
                            vertex.normal = new Quaternion(sr.ReadBE<Half>(), sr.ReadBE<Half>(), sr.ReadBE<Half>(), sr.ReadBE<Half>());
                        }
                        if ((vertexFormat & VertexData.UV) > 0)
                        {
                            vertex.uv1 = new Vector2(sr.ReadBE<Half>(), sr.ReadBE<Half>());
                        }
                        if ((vertexFormat & VertexData.UV2) > 0)
                        {
                            vertex.uv2 = new Vector2(sr.ReadBE<Half>(), sr.ReadBE<Half>());
                        }
                        if ((vertexFormat & VertexData.Color) > 0)
                        {
                            vertex.color = sr.Read4Bytes();
                        }
                        if ((vertexFormat & VertexData.Weights) > 0)
                        {
                            vertex.weightIndices = sr.Read4Bytes();
                            vertex.weights = new Vector4(sr.ReadBE<byte>() / (float)byte.MaxValue, sr.ReadBE<byte>() / (float)byte.MaxValue, sr.ReadBE<byte>() / (float)byte.MaxValue, sr.ReadBE<byte>() / (float)byte.MaxValue);
                        }

                        lod.vertices.Add(vertex);
                    }

                    model.modelLodList.Add(lod);
                }
                models.Add(model);
            }

        }

        public struct LODInfo
        {
            public int int_00;
            public int vertexCount;
        }

        public class POE2Vertex
        {
            public Vector3 position;
            public Quaternion normal;
            public Vector3 tangent;
            public Vector2 uv1;
            public Vector2 uv2;
            /// <summary>
            /// RGBA, probably
            /// </summary>
            public byte[] color = null;
            public byte[] weightIndices = null;
            public Vector4 weights;
        }

        public List<List<AquaObject>> ConvertToAquaObject(AquaNode aqn = null)
        {
            List<List<AquaObject>> aqoListList = new List<List<AquaObject>>();

            for(int mdl = 0; mdl < models.Count; mdl++)
            {
                List<AquaObject> aqoList = new List<AquaObject>();

                for(var lod = 0; lod < models[mdl].modelLodList.Count; lod++)
                {
                    var lodMdl = models[mdl].modelLodList[lod];
                    var aqo = new AquaObject();
                    aqo.objc.type = 0xC33;
                    if (aqn != null)
                    {
                        for (int i = 0; i < aqn.nodeList.Count; i++)
                        {
                            aqo.bonePalette.Add((uint)i);
                        }
                    }

                    List<Dictionary<int, int>> vertMappingList = new List<Dictionary<int, int>>();

                    for (int m = 0; m < lodMdl.meshIndices.Count; m++)
                    {
                        VTXL vtxl = new VTXL();
                        Dictionary<int, int> vertexMapping = new Dictionary<int, int>();
                        List<Vector3> indices = new List<Vector3>();
                        for (int f = 0; f < lodMdl.meshIndices[m].Count - 2; f += 3)
                        {
                            var vert0 = lodMdl.meshIndices[m][f];
                            var vert1 = lodMdl.meshIndices[m][f + 1];
                            var vert2 = lodMdl.meshIndices[m][f + 2];

                            AddVertex(lodMdl, vtxl, vertexMapping, vert0, aqn != null);
                            AddVertex(lodMdl, vtxl, vertexMapping, vert1, aqn != null);
                            AddVertex(lodMdl, vtxl, vertexMapping, vert2, aqn != null);

                            indices.Add(new Vector3(vertexMapping[vert0], vertexMapping[vert1], vertexMapping[vert2]));
                        }
                        vertMappingList.Add(vertexMapping);
                        var tris = new GenericTriangles(indices);

                        tris.matIdList = new List<int>(new int[tris.triList.Count]);
                        aqo.vtxlList.Add(vtxl);
                        aqo.tempTris.Add(tris);
                        aqo.tempMats.Add(new GenericMaterial() { matName = "ColMat" });
                    }

                    aqo.ConvertToPSO2Model(true, true, false, true, false, false, false, true);
                    aqoList.Add(aqo);
                }
                aqoListList.Add(aqoList);
            }
            
            return aqoListList;
        }

        private void AddVertex(POE2ModelLOD lodMdl, VTXL vtxl, Dictionary<int, int> vertexMapping, int vert, bool hasBones)
        {
            if (!vertexMapping.ContainsKey(vert))
            {
                var vertId = vert;
                var newVert = lodMdl.vertices[vertId];
                vertexMapping[vertId] = vtxl.vertPositions.Count;
                vtxl.vertPositions.Add(newVert.position);
                vtxl.uv1List.Add(newVert.uv1);
                if(hasBones)
                {
                    vtxl.vertWeightIndices.Add(new int[] { newVert.weightIndices[0], newVert.weightIndices[1], newVert.weightIndices[2], newVert.weightIndices[3] });
                    vtxl.vertWeights.Add(newVert.weights);
                }
            }
        }
    }
}
