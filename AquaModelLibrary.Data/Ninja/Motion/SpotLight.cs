using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.Ninja.Motion
{
    public class Spotlight : IEquatable<Spotlight>
    {
        public float Near { get; set; }
        public float Far { get; set; }
        public int InsideAngle { get; set; }
        public int OutsideAngle { get; set; }

        public Spotlight() { }

        public Spotlight(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Near = sr.ReadBE<float>();
            Far = sr.ReadBE<float>();
            InsideAngle = sr.ReadBE<int>();
            OutsideAngle = sr.ReadBE<int>();
        }

        public byte[] GetBytes(bool bigEndian)
        {
            List<byte> result = new List<byte>(16);
            ByteListExtension.AddAsBigEndian = bigEndian;
            result.AddValue(Near);
            result.AddValue(Far);
            result.AddValue(InsideAngle);
            result.AddValue(OutsideAngle);
            return result.ToArray();
        }

        public bool Equals(Spotlight other)
        {
            return Near == other.Near && Far == other.Far && InsideAngle == other.InsideAngle && OutsideAngle == other.OutsideAngle;
        }
    }
}
