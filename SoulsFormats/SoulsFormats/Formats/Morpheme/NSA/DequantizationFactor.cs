using System.Numerics;

namespace SoulsFormats.Formats.Morpheme.NSA
{
    public class DequantizationFactor
    {
        public Vector3 min = new Vector3();
        public Vector3 scaledExtent = new Vector3(1, 1, 1);

        public DequantizationFactor() { }
        public DequantizationFactor(Vector3 newMin, Vector3 newScaledExtent)
        {
            min = newMin;
            scaledExtent = newScaledExtent;
        }

        public DequantizationFactor(BinaryReaderEx br)
        {
            min = br.ReadVector3();
            scaledExtent = br.ReadVector3();
        }
    }
}
