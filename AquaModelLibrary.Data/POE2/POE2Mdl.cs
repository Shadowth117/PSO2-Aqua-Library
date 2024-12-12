using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.Intermediary;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

namespace AquaModelLibrary.Data.POE2
{
    public class POE2Mdl
    {
        public List<List<int>> meshIndices = new List<List<int>>();
        public List<POE2Vertex> vertices = new List<POE2Vertex>();

        public POE2Mdl() { }

        public POE2Mdl(byte[] file)
        {
            Read(file);
        }

        public POE2Mdl(BufferedStreamReaderBE<MemoryStream> sr)
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
            byte bt_0 = sr.ReadBE<byte>();
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
            int int_29 = sr.ReadBE<int>();
            int int_2D = sr.ReadBE<int>();
            int vertexCount = sr.ReadBE<int>();

            List<int> meshStartIndex = new List<int>();
            List<int> meshIndexCount = new List<int>();
            bool intIndices = false;
            for(int i = 0; i < meshCount; i++)
            {
                meshStartIndex.Add(sr.ReadBE<int>());
                meshIndexCount.Add(sr.ReadBE<int>());
                if (meshStartIndex[i] > ushort.MaxValue || meshIndexCount[i] > ushort.MaxValue)
                {
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
                    if(intIndices)
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
                vertex.position = sr.ReadBE<Vector3>();
                vertex.ints.Add(sr.ReadBE<int>());
                vertex.ints.Add(sr.ReadBE<int>());
                vertex.ints.Add(sr.ReadBE<int>());
                vertex.ints.Add(sr.ReadBE<int>());
                vertex.ints.Add(sr.ReadBE<int>());
                vertices.Add(vertex);
            }
        }

        public class POE2Vertex
        {
            public Vector3 position;
            public List<int> ints = new List<int>();
        }

        public List<AquaObject> ConvertToAquaObject()
        {
            List<AquaObject> aqoList = new List<AquaObject>();
            var aqo = new AquaObject();
            aqo.objc.type = 0xC33;

            VTXL vtxl = new VTXL();
            for (int v = 0; v < vertices.Count; v++)
            {
                var vertex = vertices[v];
                vtxl.vertPositions.Add(vertex.position);
            }
            aqo.vtxlList.Add(vtxl);

            List<ushort> indices = new List<ushort>();
            for (int i = 0; i < meshIndices.Count; i++)
            {
                for (int f = 0; f < meshIndices[i].Count; f++)
                {
                    indices.Add((ushort)meshIndices[i][f]);
                }
            }
            var tris = new GenericTriangles(indices);
            
            tris.matIdList = new List<int>(new int[tris.triList.Count]);
            aqo.tempTris.Add(tris);
            aqo.tempMats.Add(new GenericMaterial() { matName = "ColMat" });

            aqo.ConvertToPSO2Model(true, true, false, true, false, false, false);
            aqoList.Add(aqo);
            

            return aqoList;
        }
    }
}
