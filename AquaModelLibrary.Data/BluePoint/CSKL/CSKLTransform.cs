using Reloaded.Memory.Streams;
using System.Numerics;

namespace AquaModelLibrary.BluePoint.CSKL
{
    public class CSKLTransform
    {
        public Vector3 scale;
        public Quaternion rotation;
        public Vector3 position;

        public CSKLTransform()
        {

        }

        public CSKLTransform(BufferedStreamReader sr)
        {
            scale = sr.Read<Vector3>();
            sr.Seek(4, System.IO.SeekOrigin.Current);
            rotation = sr.Read<Quaternion>();
            position = sr.Read<Vector3>();
            sr.Seek(4, System.IO.SeekOrigin.Current);
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
