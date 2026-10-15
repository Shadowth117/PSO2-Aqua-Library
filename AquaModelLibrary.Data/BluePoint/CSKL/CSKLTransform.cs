using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers.Writers;
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

        public byte[] GetBytes(int csklVersion)
        {
            ByteListWriter outBytes = new();
            switch (csklVersion)
            {
                case 0x9:
                    outBytes.AddValue(rotation.X); outBytes.AddValue(rotation.Y); outBytes.AddValue(rotation.Z); outBytes.AddValue(rotation.W);
                    outBytes.AddValue(position.X); outBytes.AddValue(position.Y); outBytes.AddValue(position.Z); outBytes.AddValue(0f);
                    outBytes.AddValue(scale.X); outBytes.AddValue(scale.Y); outBytes.AddValue(scale.Z); outBytes.AddValue(0f);
                    break;
                case 0x19:
                    outBytes.AddValue(scale.X); outBytes.AddValue(scale.Y); outBytes.AddValue(scale.Z); outBytes.AddValue(0f);
                    outBytes.AddValue(rotation.X); outBytes.AddValue(rotation.Y); outBytes.AddValue(rotation.Z); outBytes.AddValue(rotation.W);
                    outBytes.AddValue(position.X); outBytes.AddValue(position.Y); outBytes.AddValue(position.Z); outBytes.AddValue(0f);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return outBytes.ToArray();
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
