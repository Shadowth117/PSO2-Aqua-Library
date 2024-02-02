using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

namespace AquaModelLibrary.Data.BillyHatcher.BillyNinja
{
    public class BillyNJSObject
    {
        public int flags;
        public BillyNJSAttach mesh = null;
        public Vector3 pos;
        public Vector3 rot;
        public Vector3 scale;
        public BillyNJSObject childObject = null;
        public BillyNJSObject siblingObject = null;
        public int unkInt;

        public BillyNJSObject() { }

        public BillyNJSObject(byte[] file, bool be = true, int offset = 0)
        {
            Read(file, be);
        }

        public BillyNJSObject(BufferedStreamReaderBE<MemoryStream> sr, bool be = true, int offset = 0)
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
            flags = sr.ReadBE<int>();

            int attachOffset = sr.ReadBE<int>();
            sr.Seek(offset + attachOffset, SeekOrigin.Begin);
            mesh = attachOffset > 0 ? new BillyNJSAttach(sr, be, offset) : null;

            pos = sr.Read<Vector3>();
            rot = sr.Read<Vector3>();
            scale = sr.Read<Vector3>();

            int childOffset = sr.ReadBE<int>();
            sr.Seek(offset + childOffset, SeekOrigin.Begin);
            childObject = childOffset > 0 ? new BillyNJSObject(sr, be, offset) : null;

            int siblingOffset = sr.ReadBE<int>();
            sr.Seek(offset + siblingOffset, SeekOrigin.Begin);
            siblingObject = siblingOffset > 0 ? new BillyNJSObject(sr, be, offset) : null;

            unkInt = sr.ReadBE<int>();
        }
    }
}
