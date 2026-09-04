using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers.Writers;
using System.Numerics;

namespace AquaModelLibrary.Data.Ninja.Model.Chunk
{
    public class ChunkAttach : Attach
    {
        public NinjaBoundingVolume bounding = new NinjaBoundingVolume();

        public ChunkAttach() { }

        public ChunkAttach(byte[] file, bool be = false, int offset = 0, bool DX = false)
            : this()
        {
            Read(file, be, offset, DX);
        }

        public ChunkAttach(BufferedStreamReaderBE<MemoryStream> sr, bool be = false, int offset = 0, bool DX = false)
            : this()
        {
            Read(sr, be, offset, DX);
        }
        public void Read(byte[] file, bool be = false, int offset = 0, bool DX = false)
        {
            using (var ms = new MemoryStream(file))
            using (var sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr, be, offset, DX);
            }
        }
        public void Read(BufferedStreamReaderBE<MemoryStream> sr, bool be = false, int offset = 0, bool DX = false)
        {
            sr._BEReadActive = be;
            var vertChunkPointer = sr.ReadBE<int>();
            var polyChunkPointer = sr.ReadBE<int>();
            bounding = new NinjaBoundingVolume()
            {
                center = sr.ReadBEV3(),
                radius = sr.ReadBE<float>()
            };
        }


        public void GetFaceData(int nodeId, VTXL vtxl, AquaObject aqo)
        {
            throw new NotImplementedException();
        }

        public void GetVertexData(int nodeId, VTXL vtxl, Matrix4x4 transform)
        {
            throw new NotImplementedException();
        }

        public bool HasWeights()
        {
            throw new NotImplementedException();
        }

        public void Write(ByteListWriter outBytes, List<int> POF0Offsets)
        {
            throw new NotImplementedException();
        }
    }
}
