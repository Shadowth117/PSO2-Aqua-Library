using AquaModelLibrary.Data.BillyHatcher.Collision;
using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

namespace AquaModelLibrary.Data.BillyHatcher
{
    //Used for collision
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
    }
}
