using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BillyHatcher.BillyNinja
{
    public class BillyNJSAttach
    {
        public BillyNJSVertexData vertData = null;
        //public List<BillyNJSFaceData> opaqueFaceData = new List<BillyNJSFaceData>();
        //public List<BillyNJSFaceData> transparentFaceData = new List<BillyNJSFaceData>();
        public BillyNJSBoundingVolume bounding;

        public BillyNJSAttach() { }

        public BillyNJSAttach(byte[] file, bool be = true, int offset = 0)
        {
            Read(file, be);
        }

        public BillyNJSAttach(BufferedStreamReaderBE<MemoryStream> sr, bool be = true, int offset = 0)
        {
            Read(sr, be);
        }

        public void Read(byte[] file, bool be = true, int offset = 0)
        {
            using (var ms = new MemoryStream(file))
            using (var sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr, be);
            }
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr, bool be = true, int offset = 0)
        {
            sr._BEReadActive = be;
            /*
            int vertexOffset = sr.ReadBE<int>();
            sr.Seek(offset + vertexOffset, SeekOrigin.Begin);
            vertData = vertexOffset > 0 ? new BillyNJSVertexData(sr, be, offset) : null;

            int skinMeshOffset = sr.ReadBE<int>();
            int opaqueOffset = sr.ReadBE<int>();
            int transparentOffset = sr.ReadBE<int>();

            ushort opaqueCount = sr.ReadBE<ushort>();
            ushort transparentCount = sr.ReadBE<ushort>();

            bounding = new BillyNJSBoundingVolume();
            bounding.center = sr.ReadBEV3();
            bounding.radius = sr.ReadBE<float>();

            sr.Seek(offset + opaqueOffset, SeekOrigin.Begin);
            for (int i = 0; i < opaqueCount; i++)
            {
                opaqueFaceData.Add(opaqueOffset > 0 ? new BillyNJSFaceData(sr, be, offset) : null);
            }

            sr.Seek(offset + transparentOffset, SeekOrigin.Begin);
            for (int i = 0; i < transparentCount; i++)
            {
                transparentFaceData.Add(opaqueOffset > 0 ? new BillyNJSFaceData(sr, be, offset) : null);
            }
            */
        }
    }
}
