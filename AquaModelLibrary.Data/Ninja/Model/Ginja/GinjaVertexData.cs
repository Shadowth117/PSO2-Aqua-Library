using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

//Adapted from SA Tools
namespace AquaModelLibrary.Data.Ninja.Model.Ginja
{
    public class GinjaVertexData
    {
        public List<Vector3> posList = new List<Vector3>();
        public List<Vector3> nrmList = new List<Vector3>();
        /// <summary>
        /// BGRA order
        /// </summary>
        public List<byte[]>[] colorsArray = new List<byte[]>[2];
        public List<Vector2>[] uvsArray = new List<Vector2>[8];

        public List<GinjaVertexDataElement> elements = new List<GinjaVertexDataElement>();
        public GinjaVertexData(BufferedStreamReaderBE<MemoryStream> sr, bool be = true, int offset = 0)
        {
            var element = new GinjaVertexDataElement(sr, be, offset);
            while (element.dataCategoryAttribute != GCVertexAttribute.Null)
            {
                var bookmark = sr.Position;
                ReadElementData(element, sr, be, offset);
                elements.Add(element);
                sr.Seek(bookmark, SeekOrigin.Begin);
                element = new GinjaVertexDataElement(sr, be, offset);
            }
        }

        public void Write(List<byte> outBytes, List<int> POF0Offsets)
        {
            for(int i = 0; i < elements.Count; i++)
            {
                int dataSize = 0;
                var element = elements[i];
                outBytes.Add((byte)element.dataCategoryAttribute);
                outBytes.Add((byte)element.structSize);
                switch(element.dataCategoryAttribute)
                {
                    case GCVertexAttribute.PositionMatrixIdx:
                    case GCVertexAttribute.Position:
                        outBytes.AddValue((ushort)posList.Count);
                        dataSize = (int)(posList.Count * element.structSize);
                        break;
                    case GCVertexAttribute.Normal:
                        outBytes.AddValue((ushort)nrmList.Count);
                        dataSize = (int)(nrmList.Count * element.structSize);
                        break;
                    case GCVertexAttribute.Color0:
                    case GCVertexAttribute.Color1:
                        outBytes.AddValue((ushort)colorsArray[element.dataCategoryAttribute - GCVertexAttribute.Color0].Count);
                        dataSize = (int)(colorsArray[element.dataCategoryAttribute - GCVertexAttribute.Color0].Count * element.structSize);
                        break;
                    case GCVertexAttribute.Tex0:
                    case GCVertexAttribute.Tex1:
                    case GCVertexAttribute.Tex2:
                    case GCVertexAttribute.Tex3:
                    case GCVertexAttribute.Tex4:
                    case GCVertexAttribute.Tex5:
                    case GCVertexAttribute.Tex6:
                    case GCVertexAttribute.Tex7:
                        outBytes.AddValue((ushort)uvsArray[element.dataCategoryAttribute - GCVertexAttribute.Tex0].Count);
                        dataSize = (int)(uvsArray[element.dataCategoryAttribute - GCVertexAttribute.Tex0].Count * element.structSize);
                        break;
                    case GCVertexAttribute.Null:
                        outBytes.AddValue((ushort)0);
                        break;
                }
                outBytes.AddValue(element.structure);
                if(element.dataCategoryAttribute != GCVertexAttribute.Null)
                {
                    POF0Offsets.Add(outBytes.Count);
                }
                outBytes.ReserveInt($"element{i}_data");
                outBytes.AddValue(dataSize);
            }

            //Add Null vert data
            outBytes.Add(0xFF);
            outBytes.Add(0);
            outBytes.AddValue((short)0);
            outBytes.AddValue((int)0);
            outBytes.AddValue((int)0);
            outBytes.AddValue((int)0);

            for(int i = 0; i < elements.Count; i++)
            {
                var element = elements[i];
                if(element.dataCategoryAttribute == GCVertexAttribute.Null)
                {
                    break;
                }
                outBytes.FillInt($"element{i}_data", outBytes.Count);
                switch (element.dataCategoryAttribute)
                {
                    case GCVertexAttribute.PositionMatrixIdx:
                    case GCVertexAttribute.Position:
                        foreach(var pos in posList)
                        {
                            outBytes.AddValue(pos.X);
                            outBytes.AddValue(pos.Y);
                            outBytes.AddValue(pos.Z);
                        }
                        break;
                    case GCVertexAttribute.Normal:
                        foreach (var nrm in nrmList)
                        {
                            outBytes.AddValue(nrm.X);
                            outBytes.AddValue(nrm.Y);
                            outBytes.AddValue(nrm.Z);
                        }
                        break;
                    case GCVertexAttribute.Color0:
                    case GCVertexAttribute.Color1:
                        foreach(var color in colorsArray[element.dataCategoryAttribute - GCVertexAttribute.Color0])
                        {
                            switch(element.dataType)
                            {
                                case GCDataType.RGB8:
                                    outBytes.Add(color[2]);
                                    outBytes.Add(color[1]);
                                    outBytes.Add(color[0]);
                                    outBytes.Add(0xFF);
                                    break;
                                case GCDataType.RGBA8:
                                    outBytes.Add(color[2]);
                                    outBytes.Add(color[1]);
                                    outBytes.Add(color[0]);
                                    outBytes.Add(color[3]);
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }
                        }
                        break;
                    case GCVertexAttribute.Tex0:
                    case GCVertexAttribute.Tex1:
                    case GCVertexAttribute.Tex2:
                    case GCVertexAttribute.Tex3:
                    case GCVertexAttribute.Tex4:
                    case GCVertexAttribute.Tex5:
                    case GCVertexAttribute.Tex6:
                    case GCVertexAttribute.Tex7:
                        foreach (var uv in uvsArray[element.dataCategoryAttribute - GCVertexAttribute.Tex0])
                        {
                            outBytes.AddValue((short)(uv.X * 255.0));
                            outBytes.AddValue((short)(uv.Y * 255.0));
                        }
                        break;
                }
                outBytes.AlignWriter(0x4);
            }
        }

        public void ReadElementData(GinjaVertexDataElement element, BufferedStreamReaderBE<MemoryStream> sr, bool be = true, int offset = 0)
        {
            sr._BEReadActive = be;
            sr.Seek(element.dataAddress + offset, SeekOrigin.Begin);
            //Assumes all elements are 3d variations
            switch (element.dataCategoryAttribute)
            {
                case GCVertexAttribute.Position:
                    for (int i = 0; i < element.dataCount; i++)
                    {
                        switch (element.dataType)
                        {
                            case GCDataType.Unsigned8:
                            case GCDataType.Signed8:
                            case GCDataType.Unsigned16:
                                throw new NotImplementedException();
                            case GCDataType.Signed16:
                                posList.Add(new Vector3((float)(sr.ReadBE<short>() / 255.0), (float)(sr.ReadBE<short>() / 255.0), (float)(sr.ReadBE<short>() / 255.0)));
                                break;
                            case GCDataType.Float32:
                                posList.Add(new Vector3(sr.ReadBE<float>(), sr.ReadBE<float>(), sr.ReadBE<float>()));
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                    break;
                case GCVertexAttribute.Normal:
                    for (int i = 0; i < element.dataCount; i++)
                    {
                        switch (element.dataType)
                        {
                            case GCDataType.Unsigned8:
                            case GCDataType.Signed8:
                            case GCDataType.Unsigned16:
                                throw new NotImplementedException();
                            case GCDataType.Signed16:
                                nrmList.Add(new Vector3((float)(sr.ReadBE<short>() / 255.0), (float)(sr.ReadBE<short>() / 255.0), (float)(sr.ReadBE<short>() / 255.0)));
                                break;
                            case GCDataType.Float32:
                                nrmList.Add(new Vector3(sr.ReadBE<float>(), sr.ReadBE<float>(), sr.ReadBE<float>()));
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                    break;
                case GCVertexAttribute.Color0:
                case GCVertexAttribute.Color1:
                    List<byte[]> colorList = new List<byte[]>();
                    for (int i = 0; i < element.dataCount; i++)
                    {
                        byte[] color = new byte[4];
                        switch (element.dataType)
                        {
                            case GCDataType.RGB565:
                                short colorShort = sr.ReadBE<short>();
                                color[2] = (byte)((colorShort & 0xF800) >> 8); //red
                                color[1] = (byte)((colorShort & 0x07E0) >> 3); //green
                                color[0] = (byte)((colorShort & 0x001F) << 3); //blue
                                break;
                            case GCDataType.RGBA4:
                                ushort colorShortA = sr.ReadBE<ushort>();
                                // multiplying all by 0x11, so that e.g. 0xF becomes 0xFF
                                color[2] = (byte)(((colorShortA & 0xF000) >> 12) * 0x11); //red
                                color[1] = (byte)(((colorShortA & 0x0F00) >> 8) * 0x11); //green
                                color[0] = (byte)(((colorShortA & 0x00F0) >> 4) * 0x11); //blue
                                color[3] = (byte)((colorShortA & 0x000F) * 0x11); //alpha
                                break;
                            case GCDataType.RGBA6:
                                uint colorInt = sr.ReadBE<uint>();
                                // shifting all 2 less to the left, so that they are more accurate to the color that they should represent
                                color[2] = (byte)((colorInt & 0xFC0000) >> 16); //red
                                color[1] = (byte)((colorInt & 0x03F000) >> 10); //green
                                color[0] = (byte)((colorInt & 0x000FC0) >> 4); //blue
                                color[3] = (byte)((colorInt & 0x00003F) << 2); //alpha
                                sr.Seek(-1, SeekOrigin.Current); //Seek back since it's only 3 bytes and not 4
                                break;
                            case GCDataType.RGB8:
                            case GCDataType.RGBX8:
                            case GCDataType.RGBA8:
                                var RGBA = sr.ReadBE<uint>();
                                color[2] = (byte)(RGBA & 0xFF);
                                color[1] = (byte)((RGBA >> 8) & 0xFF);
                                color[0] = (byte)((RGBA >> 16) & 0xFF);
                                color[3] = (byte)(RGBA >> 24);
                                if (element.dataType != GCDataType.RGBA8)
                                    color[3] = 255;
                                break;
                            default:
                                throw new ArgumentException($"{element.dataType} is not a valid color type");
                        }
                        colorList.Add(color);
                    }
                    colorsArray[element.dataCategoryAttribute - GCVertexAttribute.Color0] = colorList;
                    break;
                case GCVertexAttribute.Tex0:
                case GCVertexAttribute.Tex1:
                case GCVertexAttribute.Tex2:
                case GCVertexAttribute.Tex3:
                case GCVertexAttribute.Tex4:
                case GCVertexAttribute.Tex5:
                case GCVertexAttribute.Tex6:
                case GCVertexAttribute.Tex7:
                    List<Vector2> uvList = new List<Vector2>();
                    for (int i = 0; i < element.dataCount; i++)
                    {
                        switch (element.dataType)
                        {
                            case GCDataType.Unsigned8:
                            case GCDataType.Signed8:
                            case GCDataType.Unsigned16:
                                throw new NotImplementedException();
                            case GCDataType.Signed16:
                                uvList.Add(new Vector2((float)(sr.ReadBE<short>() / 255.0), (float)(sr.ReadBE<short>() / 255.0)));
                                break;
                            case GCDataType.Float32:
                                uvList.Add(new Vector2(sr.ReadBE<float>(), sr.ReadBE<float>()));
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                    uvsArray[element.dataCategoryAttribute - GCVertexAttribute.Tex0] = uvList;
                    break;
                default:
                    throw new ArgumentException($"Unexpected attribute type: {element.dataCategoryAttribute}");
            }
        }
    }
}
