//THANK YOU KNUX https://github.com/Big-Endian-32/Marathon/blob/03a2812cc903241ab65fd21d2270c0680044bc09/Marathon/Formats/Mesh/Ninja/NinjaKeyframe.cs
using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

namespace Marathon.Formats.Mesh.Ninja
{
    /// <summary>
    /// Different structures for different keyframe types.
    /// </summary>
    public class NinjaKeyframe
    {
        /// <summary>
        /// Keyframe that consists of a single floating point value.
        /// </summary>
        public class NNS_MOTION_KEY_FLOAT
        {
            public float Frame { get; set; }

            public float Value { get; set; }

            public void Read(BufferedStreamReaderBE<MemoryStream> reader, bool be)
            {
                Frame = reader.ReadBE<float>(be);
                Value = reader.ReadBE<float>(be);
            }
        }

        /// <summary>
        /// Keyframe that consists of a single short, usually using the Binary Angle Measurement System.
        /// </summary>
        public class NNS_MOTION_KEY_SINT16
        {
            public short Frame { get; set; }

            public short Value { get; set; }

            public void Read(BufferedStreamReaderBE<MemoryStream> reader, bool be)
            {
                Frame = reader.ReadBE<short>(be);
                Value = reader.ReadBE<short>(be);
            }
        }

        /// <summary>
        /// Keyframe that consists of a Vector3.
        /// </summary>
        public class NNS_MOTION_KEY_VECTOR
        {
            public float Frame { get; set; }

            public Vector3 Value { get; set; }

            public void Read(BufferedStreamReaderBE<MemoryStream> reader, bool be)
            {
                Frame = reader.ReadBE<float>(be);
                Value = reader.ReadBEV3(be);
            }
        }

        /// <summary>
        /// Keyframe that consists of three shorts, usually using the Binary Angle Measurement System.
        /// </summary>
        public class NNS_MOTION_KEY_ROTATE_A16
        {
            public short Frame { get; set; }

            public ushort Value1 { get; set; }

            public ushort Value2 { get; set; }

            public ushort Value3 { get; set; }

            public void Read(BufferedStreamReaderBE<MemoryStream> reader, bool be)
            {
                Frame = reader.ReadBE<short>(be);
                Value1 = reader.ReadBE<ushort>(be);
                Value2 = reader.ReadBE<ushort>(be);
                Value3 = reader.ReadBE<ushort>(be);
            }

            public Vector3 GetVec3()
            {
                return new Vector3(Value1 * 360 / 65536, Value2 * 360 / 65536, Value3 * 360 / 65536);
            }

            public override string ToString()
            {
                var vec3 = GetVec3();
                return $"Frame {Frame} Vec3 {vec3.X} {vec3.Y} {vec3.Z}";
            }
        }
    }
}