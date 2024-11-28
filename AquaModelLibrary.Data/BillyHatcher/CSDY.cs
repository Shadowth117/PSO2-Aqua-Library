using AquaModelLibrary.Data.BillyHatcher.Collision;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.Intermediary;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

namespace AquaModelLibrary.Data.BillyHatcher
{
    /// <summary>
    /// Collision Shape DummY?
    /// Basic model with just positions and billy type collision faces
    /// All CSDY files are vestigial and are only included on PC.
    /// </summary>
    public class CSDY
    {
        public List<List<Vector3>> vertexListList = new List<List<Vector3>>();
        public List<List<CSDYFace>> faceListList = new List<List<CSDYFace>>();
        public CSDY() { }
        public CSDY(byte[] file, bool hasNinjaHeader, int offset = 0)
        {
            Read(file, hasNinjaHeader, offset);
        }
        public CSDY(BufferedStreamReaderBE<MemoryStream> sr, bool hasNinjaHeader, int offset = 0)
        {
            Read(sr, hasNinjaHeader, offset);
        }

        public void Read(byte[] file, bool hasNinjaHeader, int offset = 0)
        {
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr, hasNinjaHeader, offset);
            }
        }


        public void Read(BufferedStreamReaderBE<MemoryStream> sr, bool hasNinjaHeader, int offset = 0)
        {
            sr._BEReadActive = true;
            if (hasNinjaHeader)
            {
                sr.Seek(0x8, SeekOrigin.Begin);
            }

            var firstOffset = sr.ReadBE<int>();

            //No real tell of when these multi containers or just one collision piece. But if they are multiple, the offset will always be 8 before the offset table.
            //If it's greater, we're already in the offset table and there's only one model to worry about, at least for stock models
            if (firstOffset > sr.Position + 4)
            {
                var count = sr.ReadBE<int>();
                for (int i = 0; i < count; i++)
                {
                    var bookmark = sr.Position;
                    ReadModel(sr, offset);
                    sr.Seek(bookmark, SeekOrigin.Begin);
                }
            }
            else
            {
                sr.Seek(-4, SeekOrigin.Current);
                ReadModel(sr, offset);
            }
        }

        public void ReadModel(BufferedStreamReaderBE<MemoryStream> sr, int offset = 0)
        {
            var vertexOffset = sr.ReadBE<int>();
            var vertexCount = sr.ReadBE<int>();
            var faceOffset = sr.ReadBE<int>();
            var faceCount = sr.ReadBE<int>();
            sr.Seek(vertexOffset + offset, SeekOrigin.Begin);
            List<Vector3> vertices = new List<Vector3>();
            for (int i = 0; i < vertexCount; i++)
            {
                vertices.Add(sr.ReadBEV3());
            }
            vertexListList.Add(vertices);
            sr.Seek(faceOffset + offset, SeekOrigin.Begin);
            List<CSDYFace> faces = new List<CSDYFace>();
            for (int i = 0; i < faceCount; i++)
            {
                CSDYFace face = new CSDYFace();
                face.index0 = sr.ReadBE<ushort>();
                face.index1 = sr.ReadBE<ushort>();
                face.index2 = sr.ReadBE<ushort>();
                face.index3 = sr.ReadBE<ushort>();
                face.int_08 = sr.ReadBE<int>();
                face.faceNormal = sr.ReadBEV3();
                face.faceBounds = new CollisionBounds() { MinX = sr.ReadBE<float>(), MaxX = sr.ReadBE<float>(), MinZ = sr.ReadBE<float>(), MaxZ = sr.ReadBE<float>() };
                faces.Add(face);
            }
            faceListList.Add(faces);
        }

        public List<AquaObject> ConvertToAquaObject()
        {
            List<AquaObject> aqoList = new List<AquaObject>();
            for(int i = 0; i < vertexListList.Count; i++)
            {
                var aqo = new AquaObject();
                aqo.objc.type = 0xC33;

                VTXL vtxl = new VTXL();
                for (int v = 0; v < vertexListList[i].Count; v++)
                {
                    var vertex = vertexListList[i][v];
                    vtxl.vertPositions.Add(vertex);
                }
                aqo.vtxlList.Add(vtxl);

                List<ushort> indices = new List<ushort>();
                for (int f = 0; f < faceListList[i].Count; f++)
                {
                    indices.Add(faceListList[i][f].index0);
                    indices.Add(faceListList[i][f].index1);
                    indices.Add(faceListList[i][f].index2);
                }
                var tris = new GenericTriangles(indices);
                tris.matIdList = new List<int>(new int[tris.triList.Count]);
                aqo.tempTris.Add(tris);
                aqo.tempMats.Add(new GenericMaterial() { matName = "ColMat" });

                aqo.ConvertToPSO2Model(false, true, false, true, false, false, false);
                aqoList.Add(aqo);
            }

            return aqoList;
        }
    }
}
