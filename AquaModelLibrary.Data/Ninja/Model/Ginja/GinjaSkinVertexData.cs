using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.Ninja.Model.Ginja
{
    public class GinjaSkinVertexData
    {
        public List<GinjaSkinVertexDataElement> elements = new List<GinjaSkinVertexDataElement>();
        public GinjaSkinVertexData(BufferedStreamReaderBE<MemoryStream> sr, bool be = true, int offset = 0)
        {
            sr._BEReadActive = be;
            var element = new GinjaSkinVertexDataElement(sr, be, offset);
            while (element.elementType < GCSkinAttribute.WeightEnd)
            {
                elements.Add(element);
                element = new GinjaSkinVertexDataElement(sr, be, offset);
            }
        }
    }
}
