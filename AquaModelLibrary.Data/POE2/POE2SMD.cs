using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.Intermediary;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Helpers.Readers;
using System.Numerics;
using Half = AquaModelLibrary.Data.DataTypes.Half;

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
    //Mesh data is stored here. Skeletal data is stored with animations
    public class POE2SMD
    {
        public List<List<int>> meshIndices = new List<List<int>>();
        public List<POE2Vertex> vertices = new List<POE2Vertex>();

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

            //TMD models are the same, but with a one byte offset
            if (isSMD)
            {
                byte bt_0 = sr.ReadBE<byte>();
            }
            byte bt_1 = sr.ReadBE<byte>();
            byte bt_2 = sr.ReadBE<byte>();
            byte bt_3 = sr.ReadBE<byte>();

            int unktCount = sr.ReadBE<int>();
            int int_08 = sr.ReadBE<int>();
            int int_0C = sr.ReadBE<int>();
            int int_10 = sr.ReadBE<int>();
            int int_14 = sr.ReadBE<int>();
            int int_18 = sr.ReadBE<int>();
            int int_1C = sr.ReadBE<int>();

            int DOLm = sr.ReadBE<int>();
            byte bt_24 = sr.ReadBE<byte>();
            byte bt_25 = sr.ReadBE<byte>();
            byte bt_26 = sr.ReadBE<byte>();
            byte meshCount = sr.ReadBE<byte>();
            byte bt_28 = sr.ReadBE<byte>();
            VertexData vertexFormat = (VertexData)sr.ReadBE<int>();
            int int_2D = sr.ReadBE<int>();
            int vertexCount = sr.ReadBE<int>();

            List<int> meshStartIndex = new List<int>();
            List<int> meshIndexCount = new List<int>();
            bool[] meshUsesIntIndices = new bool[meshCount];
            bool intIndices = false;
            for(int i = 0; i < meshCount; i++)
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
            for(int i = 0; i < meshCount; i++)
            {
                List<int> mesh = new List<int>();
                sr.Seek(indicesStart + (intIndices ? 4 : 2) * meshStartIndex[i], SeekOrigin.Begin);
                for(int j = 0; j < meshIndexCount[i]; j++)
                {
                    if(meshUsesIntIndices[i])
                    {
                        mesh.Add(sr.ReadBE<int>());
                    } else
                    {
                        mesh.Add(sr.ReadBE<ushort>());
                    }
                }

                meshIndices.Add(mesh);
            }

            for(int i = 0; i < vertexCount; i++)
            {
                POE2Vertex vertex = new POE2Vertex();
                if((vertexFormat & VertexData.Position) > 0)
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

                vertices.Add(vertex);
            }
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

        public List<AquaObject> ConvertToAquaObject(AquaNode aqn = null)
        {
            List<AquaObject> aqoList = new List<AquaObject>();
            var aqo = new AquaObject();
            aqo.objc.type = 0xC33;

            if(aqn != null)
            {
                for(int i = 0; i < aqn.nodeList.Count; i++)
                {
                    aqo.bonePalette.Add((uint)i);
                }
            }

            List<Dictionary<int, int>> vertMappingList = new List<Dictionary<int, int>>();

            for (int m = 0; m < meshIndices.Count; m++)
            {
                VTXL vtxl = new VTXL();
                Dictionary<int, int> vertexMapping = new Dictionary<int, int>();
                List<Vector3> indices = new List<Vector3>();
                for (int f = 0; f < meshIndices[m].Count - 2; f += 3)
                {
                    var vert0 = meshIndices[m][f];
                    var vert1 = meshIndices[m][f + 1];
                    var vert2 = meshIndices[m][f + 2];

                    AddVertex(vtxl, vertexMapping, vert0, aqn != null);
                    AddVertex(vtxl, vertexMapping, vert1, aqn != null);
                    AddVertex(vtxl, vertexMapping, vert2, aqn != null);

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
            
            return aqoList;
        }

        private void AddVertex(VTXL vtxl, Dictionary<int, int> vertexMapping, int vert, bool hasBones)
        {
            if (!vertexMapping.ContainsKey(vert))
            {
                var vertId = vert;
                var newVert = vertices[vertId];
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
