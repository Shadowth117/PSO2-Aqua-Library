using AquaModelLibrary.Helpers.Extensions;
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

        public void Write(List<byte> outBytes, List<int> POF0Offsets)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                var element = elements[i];
                outBytes.AddValue((ushort)element.elementType);
                switch (element.elementType)
                {
                    case GCSkinAttribute.StaticWeight:
                        outBytes.AddValue((ushort)(element.posNrms.Count * 3));
                        break;
                    case GCSkinAttribute.PartialWeightStart:
                    case GCSkinAttribute.PartialWeight:
                        outBytes.AddValue((ushort)(element.posNrms.Count * 4));
                        break;
                    case GCSkinAttribute.WeightEnd:
                        outBytes.AddValue((ushort)0);
                        break;
                }
                outBytes.AddValue((ushort)element.startingIndex);
                outBytes.AddValue((ushort)element.posNrms.Count);
                if(element.posNrms.Count > 0)
                {
                    POF0Offsets.Add(outBytes.Count);
                }
                outBytes.ReserveInt($"posNrms{i}Offset");
                if (element.weightData.Count > 0)
                {
                    POF0Offsets.Add(outBytes.Count);
                }
                outBytes.ReserveInt($"weights{i}Offset");
            }
            outBytes.AddValue((ushort)3);
            outBytes.AddValue((ushort)0);
            outBytes.AddValue((int)0);
            outBytes.AddValue((int)0);
            outBytes.AddValue((int)0);
            outBytes.AlignWriter(0x20);

            for (int i = 0; i < elements.Count; i++)
            {
                var element = elements[i];
                if (element.posNrms.Count > 0)
                {
                    outBytes.AlignWriter(0x20);

                    outBytes.FillInt($"posNrms{i}Offset", outBytes.Count);
                    foreach (var posNrm in element.posNrms)
                    {
                        outBytes.AddValue(posNrm.posX);
                        outBytes.AddValue(posNrm.posY);
                        outBytes.AddValue(posNrm.posZ);
                        outBytes.AddValue(posNrm.nrmX);
                        outBytes.AddValue(posNrm.nrmY);
                        outBytes.AddValue(posNrm.nrmZ);
                    }
                }

                if (element.weightData.Count > 0)
                {
                    outBytes.FillInt($"weights{i}Offset", outBytes.Count);
                    foreach (var wt in element.weightData)
                    {
                        outBytes.AddValue(wt.vertIndex);
                        outBytes.AddValue(wt.weight);
                    }
                }
            }
        }
    }
}
