using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

namespace AquaModelLibrary.Data.BluePoint.CSKL
{
    public class CSKLTransform
    {
        public Vector3 scale;
        public Quaternion rotation;
        public Vector3 position;

        public CSKLTransform()
        {

        }

        public CSKLTransform(BufferedStreamReaderBE<MemoryStream> sr, int csklVersion)
        {
            switch(csklVersion)
            {
                case 0x9:
                    rotation = sr.Read<Quaternion>();
                    position = sr.Read<Vector3>();
                    sr.Seek(4, System.IO.SeekOrigin.Current);
                    scale = sr.Read<Vector3>();
                    sr.Seek(4, System.IO.SeekOrigin.Current);
                    break;
                case 0x19:
                    scale = sr.Read<Vector3>();
                    sr.Seek(4, System.IO.SeekOrigin.Current);
                    rotation = sr.Read<Quaternion>();
                    position = sr.Read<Vector3>();
                    sr.Seek(4, System.IO.SeekOrigin.Current);
                    break;
            }
        }

        /// <summary>
        /// Creates a transformation matrix from the scale, rotation, and translation of the bone.
        /// </summary>
        public Matrix4x4 ComputeLocalTransform()
        {
            var mat = Matrix4x4.CreateScale(scale);

            mat *= Matrix4x4.CreateFromQuaternion(rotation);

            mat *= Matrix4x4.CreateTranslation(position);

            return mat;
        }
    }
}
