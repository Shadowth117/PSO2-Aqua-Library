using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.Ninja.Model.Ginja
{
    public class GinjaSkinVertexDataElement
    {
        public List<GinjaSkinVertexSetPosNrm> posNrms = new List<GinjaSkinVertexSetPosNrm>();
        public List<GinjaSkinVertexSetWeight> weightData = new List<GinjaSkinVertexSetWeight>();

        //Both the data and the mesh struct here are 0x20 aligned
        public GCSkinAttribute elementType;
        /// <summary>
        /// 3 * indexCount if elementType 0, 4 * indexCount if 1 or 2.
        /// Data assumedly vert positions, normals, and uvs. Weights would optionally be the 4th.
        /// </summary>
        public ushort totalVertIndices;
        public ushort startingIndex;
        public ushort indexCount;
        public int positionNormalsOffset;
        public int weightsOffset;

        public GinjaSkinVertexDataElement() { }
        public GinjaSkinVertexDataElement(BufferedStreamReaderBE<MemoryStream> sr, bool be = true, int offset = 0)
        {
            sr._BEReadActive = be;
            elementType = sr.ReadBE<GCSkinAttribute>();
            totalVertIndices = sr.ReadBE<ushort>();
            startingIndex = sr.ReadBE<ushort>();
            indexCount = sr.ReadBE<ushort>();
            positionNormalsOffset = sr.ReadBE<int>();
            weightsOffset = sr.ReadBE<int>();

            var bookmark = sr.Position;

            switch (elementType)
            {
                case GCSkinAttribute.StaticWeight:
                    sr.Seek(offset + positionNormalsOffset, SeekOrigin.Begin);
                    for (int i = 0; i < indexCount; i++)
                    {
                        posNrms.Add(new GinjaSkinVertexSetPosNrm()
                        {
                            posX = sr.ReadBE<short>(),
                            posY = sr.ReadBE<short>(),
                            posZ = sr.ReadBE<short>(),
                            nrmX = sr.ReadBE<short>(),
                            nrmY = sr.ReadBE<short>(),
                            nrmZ = sr.ReadBE<short>(),
                        });
                    }
                    break;
                case GCSkinAttribute.PartialWeightStart:
                    sr.Seek(offset + positionNormalsOffset, SeekOrigin.Begin);
                    for (int i = 0; i < indexCount; i++)
                    {
                        posNrms.Add(new GinjaSkinVertexSetPosNrm()
                        {
                            posX = sr.ReadBE<short>(),
                            posY = sr.ReadBE<short>(),
                            posZ = sr.ReadBE<short>(),
                            nrmX = sr.ReadBE<short>(),
                            nrmY = sr.ReadBE<short>(),
                            nrmZ = sr.ReadBE<short>(),
                        });
                    }
                    sr.Seek(offset + weightsOffset, SeekOrigin.Begin);
                    for (int i = 0; i < indexCount; i++)
                    {
                        weightData.Add(new GinjaSkinVertexSetWeight()
                        {
                            vertIndex = sr.ReadBE<short>(),
                            weight = sr.ReadBE<short>(),
                        });
                    }
                    break;
                case GCSkinAttribute.PartialWeight:
                    sr.Seek(offset + positionNormalsOffset, SeekOrigin.Begin);
                    for (int i = 0; i < indexCount; i++)
                    {
                        posNrms.Add(new GinjaSkinVertexSetPosNrm()
                        {
                            posX = sr.ReadBE<short>(),
                            posY = sr.ReadBE<short>(),
                            posZ = sr.ReadBE<short>(),
                            nrmX = sr.ReadBE<short>(),
                            nrmY = sr.ReadBE<short>(),
                            nrmZ = sr.ReadBE<short>(),
                        });
                    }
                    sr.Seek(offset + weightsOffset, SeekOrigin.Begin);
                    for (int i = 0; i < indexCount; i++)
                    {
                        weightData.Add(new GinjaSkinVertexSetWeight()
                        {
                            vertIndex = sr.ReadBE<short>(),
                            weight = sr.ReadBE<short>(),
                        });
                    }
                    break;
                case GCSkinAttribute.WeightEnd:
                    break;
                default:
                    throw new System.Exception($"Bad GCSkinVertexSetElement type {elementType:X}");
            }

            sr.Seek(bookmark, SeekOrigin.Begin);
        }
    }
}
