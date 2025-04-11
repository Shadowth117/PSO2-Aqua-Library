using AquaModelLibrary.Data.Gamecube;
using AquaModelLibrary.Helpers.Readers;

//Sourced from SA Tools
namespace AquaModelLibrary.Data.Ninja.Model.Ginja
{
    public class GinjaVertexDataElement
    {
        public GCVertexAttribute dataCategoryAttribute;
        public ushort dataCount;
        public uint structure;
        public uint dataAddress;

        /// <summary>
        /// The size of a single object in the list in bytes
        /// </summary>
        public uint structSize
        {
            get
            {
                uint num_components = 1;

                switch (structType)
                {
                    case GCStructType.Position_XY:
                    case GCStructType.TexCoord_ST:
                        num_components = 2;
                        break;
                    case GCStructType.Position_XYZ:
                    case GCStructType.Normal_XYZ:
                        num_components = 3;
                        break;
                }

                switch (dataType)
                {
                    case GCDataType.Unsigned8:
                    case GCDataType.Signed8:
                        return num_components;
                    case GCDataType.Unsigned16:
                    case GCDataType.Signed16:
                    case GCDataType.RGB565:
                    case GCDataType.RGBA4:
                        return num_components * 2;
                    case GCDataType.Float32:
                    case GCDataType.RGBA8:
                    case GCDataType.RGB8:
                    case GCDataType.RGBX8:
                    default:
                        return num_components * 4;
                }
            }
        }

        public GCStructType structType
        {
            get
            {
                return (GCStructType)(structure & 0x0F);
            }

            set
            {
                structure = (uint)(((byte)dataType << 4) | (byte)value);
            }
        }

        public GCDataType dataType
        {
            get
            {
                return (GCDataType)((structure >> 4) & 0x0F);
            }

            set
            {
                structure = (uint)(((byte)value << 4) | (byte)structType);
            }
        }

        public GinjaVertexDataElement(GCVertexAttribute attributeType)
        {
            dataCategoryAttribute = attributeType;

            switch (dataCategoryAttribute)
            {
                case GCVertexAttribute.Position:
                    dataType = GCDataType.Float32;
                    structType = GCStructType.Position_XYZ;
                    break;
                case GCVertexAttribute.Normal:
                    dataType = GCDataType.Float32;
                    structType = GCStructType.Normal_XYZ;
                    break;
                case GCVertexAttribute.Color0:
                    dataType = GCDataType.RGBA8;
                    structType = GCStructType.Color_RGBA;
                    break;
                case GCVertexAttribute.Tex0:
                    dataType = GCDataType.Signed16;
                    structType = GCStructType.TexCoord_ST;
                    break;
                default:
                    throw new ArgumentException($"Datatype {dataCategoryAttribute} is not a valid vertex type for SA2");
            }
        }

        public GinjaVertexDataElement(BufferedStreamReaderBE<MemoryStream> sr, bool be = true, int offset = 0)
        {
            Read(sr, be, offset);
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr, bool be = true, int offset = 0)
        {
            sr._BEReadActive = be;
            dataCategoryAttribute = sr.ReadBE<GCVertexAttribute>();
            byte structSizeLocal = sr.ReadBE<byte>();
            dataCount = sr.ReadBE<ushort>();
            structure = sr.ReadBE<uint>();
            dataAddress = sr.ReadBE<uint>();
            uint dataBufferSizeLocal = sr.ReadBE<uint>();
        }
    }
}
