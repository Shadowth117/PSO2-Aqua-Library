﻿using AquaModelLibrary.Data.Gamecube;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;

//Sourced from SA Tools
namespace AquaModelLibrary.Data.Ninja.Model.Ginja
{
    /// <summary>
    /// The types of parameter that exist
    /// </summary>
    public enum ParameterType : uint
    {
        VtxAttrFmt = 0,
        IndexAttributeFlags = 1,
        Lighting = 2,
        BlendAlpha = 4,
        AmbientColor = 5,
        Texture = 8,
        Unknown_9 = 9,
        TexCoordGen = 10,
    }

    /// <summary>
    /// Base class for all GC parameter types. <br/>
    /// Used to store geometry information (like materials).
    /// </summary>
    [Serializable]
    public abstract class GCParameter
    {
        /// <summary>
        /// The type of parameter
        /// </summary>
        public readonly ParameterType type;

        /// <summary>
        /// All parameter data is stored in these 4 bytes
        /// </summary>
        protected uint data;

        /// <summary>
        /// Base constructor for an empty parameter. <br/>
        /// Used only in child classes.
        /// </summary>
        /// <param name="type">The type of parameter to create</param>
        protected GCParameter(ParameterType type)
        {
            this.type = type;
            data = 0;
        }

        /// <summary>
        /// Create a parameter object from a file and address
        /// </summary>
        /// <param name="file">The file contents</param>
        /// <param name="address">The address at which the parameter is located</param>
        /// <returns>Any of the parameter types</returns>
        public static GCParameter Read(BufferedStreamReaderBE<MemoryStream> sr, bool be = true)
        {
            sr._BEReadActive = be;
            GCParameter result = null;
            ParameterType paramType = (ParameterType)sr.ReadBE<byte>();
            sr.Seek(3, SeekOrigin.Current);

            switch (paramType)
            {
                case ParameterType.VtxAttrFmt:
                    result = new VtxAttrFmtParameter(GCVertexAttribute.Null);
                    break;
                case ParameterType.IndexAttributeFlags:
                    result = new IndexAttributeParameter();
                    break;
                case ParameterType.Lighting:
                    result = new LightingParameter();
                    break;
                case ParameterType.BlendAlpha:
                    result = new BlendAlphaParameter();
                    break;
                case ParameterType.AmbientColor:
                    result = new AmbientColorParameter();
                    break;
                case ParameterType.Texture:
                    result = new TextureParameter();
                    break;
                case ParameterType.Unknown_9:
                    result = new Unknown9Parameter();
                    break;
                case ParameterType.TexCoordGen:
                    result = new TexCoordGenParameter();
                    break;
            }

            result.data = sr.ReadBE<uint>();

            return result;
        }

        /// <summary>
        /// Writes the parameter contents to a stream
        /// </summary>
        /// <param name="writer">The stream writer</param>
        public void Write(BinaryWriter writer)
        {
            writer.Write((byte)type);
            writer.Write(data);
        }

        public void SetData(uint _data)
        {
            data = _data;
        }

        public virtual byte[] GetBytes()
        {
            ByteListExtension.AddAsBigEndian = true;
            List<byte> result = new List<byte>();
            result.Add((byte)type);
            result.AddRange(new byte[3]);
            result.AddValue(data);
            return result.ToArray();
        }
    }

    /// <summary>
    /// Parameter that is relevent for Vertex data. <br/>
    /// A geometry object needs to have one for each 
    /// </summary>
    [Serializable]
    public class VtxAttrFmtParameter : GCParameter
    {
        /// <summary>
        /// The attribute type that this parameter applies for
        /// </summary>
        public GCVertexAttribute VertexAttribute
        {
            get
            {
                return (GCVertexAttribute)(data >> 16);
            }
            set
            {
                data &= 0xFFFF;
                data |= ((uint)value) << 16;
            }
        }

        /// <summary>
        /// Seems to be some type of address of buffer length. <br/>
        /// Sa2 only uses a specific value for each attribute type either way
        /// </summary>
        public ushort Unknown
        {
            get
            {
                return (ushort)(data & 0xFFFF);
            }
            set
            {
                data &= 0xFFFF0000;
                data |= value;
            }
        }

        /// <summary>
        /// Creates a new parameter with the default value according to each attribute type <br/> (which are the only ones that work ingame)
        /// Models with non static weights must use a different value.
        /// </summary>
        /// <param name="vertexAttrib">The vertex attribute type that the parameter is for</param>
        public VtxAttrFmtParameter(GCVertexAttribute vertexAttrib, bool weightedModel = false) : base(ParameterType.VtxAttrFmt)
        {
            VertexAttribute = vertexAttrib;

            // Setting the default values
            switch (vertexAttrib)
            {
                case GCVertexAttribute.Position:
                    Unknown = (ushort)(weightedModel ? 0x1308 : 0x1400);
                    break;
                case GCVertexAttribute.Normal:
                    Unknown = (ushort)(weightedModel ? 0x2308 : 0x2400);
                    break;
                case GCVertexAttribute.Color0:
                    Unknown = (ushort)(0x6A00);
                    break;
                case GCVertexAttribute.Tex0:
                    Unknown = (ushort)0x8308;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Allows to manually create a Vertex attribute parameter
        /// </summary>
        /// <param name="Unknown"></param>
        /// <param name="vertexAttrib">The vertex attribute type that the parameter is for</param>
        public VtxAttrFmtParameter(ushort Unknown, GCVertexAttribute vertexAttrib) : base(ParameterType.VtxAttrFmt)
        {
            this.Unknown = Unknown;
            VertexAttribute = vertexAttrib;
        }
    }

    /// <summary>
    /// Holds information about the vertex data thats stored in the geometry
    /// </summary>
    [Serializable]
    public class IndexAttributeParameter : GCParameter
    {
        /// <summary>
        /// Holds information about the vertex data thats stored in the geometry 
        /// </summary>
        public GCIndexAttributeFlags IndexAttributes
        {
            get
            {
                return (GCIndexAttributeFlags)data;
            }
            set
            {
                data = (uint)value;
            }
        }

        /// <summary>
        /// Creates an empty index attribute parameter
        /// </summary>
        public IndexAttributeParameter() : base(ParameterType.IndexAttributeFlags)
        {
            //this always exists
            IndexAttributes &= GCIndexAttributeFlags.HasPosition;
        }

        /// <summary>
        /// Creates an index attribute parameter based on existing flags
        /// </summary>
        /// <param name="flags"></param>
        public IndexAttributeParameter(GCIndexAttributeFlags flags) : base(ParameterType.IndexAttributeFlags)
        {
            IndexAttributes = flags;
        }

    }

    /// <summary>
    /// Holds lighting information
    /// </summary>
    [Serializable]
    public class LightingParameter : GCParameter
    {
        /// <summary>
        /// Lighting flags. Pretty much unknown how they work
        /// </summary>
        public ushort LightingFlags
        {
            get
            {
                return (ushort)(data & 0xFFFF);
            }
            set
            {
                data &= 0xFFFF0000;
                data |= value;
            }
        }

        /// <summary>
        /// Which shadow stencil the geometry should use. <br/>
        /// Ranges from 0 - 15
        /// </summary>
        public byte ShadowStencil
        {
            get
            {
                return (byte)((data >> 16) & 0xF);
            }
            set
            {
                data &= 0xFFF0FFFF;
                data |= (uint)((value & 0xF) << 16);
            }
        }

        public byte Unknown1
        {
            get
            {
                return (byte)((data >> 20) & 0xF);
            }
            set
            {
                data &= 0xFFF0FFFF;
                data |= (uint)((value & 0xF) << 20);
            }
        }

        public byte Unknown2
        {
            get
            {
                return (byte)((data >> 24) & 0xFF);
            }
            set
            {
                data &= 0xFFF0FFFF;
                data |= (uint)(value << 24);
            }
        }

        /// <summary>
        /// Creates a lighting parameter with the default data
        /// </summary>
        public LightingParameter() : base(ParameterType.Lighting)
        {
            //default value
            LightingFlags = 0xB11;
            ShadowStencil = 1;
        }

        public LightingParameter(ushort lightingFlags, byte shadowStencil) : base(ParameterType.Lighting)
        {
            LightingFlags = lightingFlags;
            ShadowStencil = shadowStencil;
        }
    }

    /// <summary>
    /// The blending information for the surface of the geometry
    /// </summary>
    [Serializable]
    public class BlendAlphaParameter : GCParameter
    {
        /// <summary>
        /// NJ Blendmode for the source alpha
        /// </summary>
        public AlphaInstruction NJSourceAlpha
        {
            get
            {
                return GCEnumConverter.GXToNJAlphaInstruction((GCBlendModeControl)((data >> 11) & 7));
            }
            set
            {
                uint inst = (uint)GCEnumConverter.NJtoGXBlendModeControl(value);
                data &= 0xFFFFC7FF; // ~(7 << 11)
                data |= (inst & 7) << 11;
            }
        }

        /// <summary>
        /// NJ Blendmode for the destination alpha
        /// </summary>
        public AlphaInstruction NJDestAlpha
        {
            get
            {
                return GCEnumConverter.GXToNJAlphaInstruction((GCBlendModeControl)((data >> 8) & 7));
            }
            set
            {
                uint inst = (uint)GCEnumConverter.NJtoGXBlendModeControl(value);
                data &= 0xFFFFF8FF; // ~(7 << 8)
                data |= (inst & 7) << 8;
            }
        }

        /// <summary>
        /// Blendmode for the source alpha
        /// </summary>
        public GCBlendModeControl SourceAlpha
        {
            get
            {
                return (GCBlendModeControl)((data >> 11) & 7);
            }
            set
            {
                uint inst = (uint)value;
                data &= 0xFFFFC7FF; // ~(7 << 11)
                data |= (inst & 7) << 11;
            }
        }

        /// <summary>
        /// Blendmode for the destination alpha
        /// </summary>
        public GCBlendModeControl DestAlpha
        {
            get
            {
                return (GCBlendModeControl)((data >> 8) & 7);
            }
            set
            {
                uint inst = (uint)value;
                data &= 0xFFFFF8FF; // ~(7 << 8)
                data |= (inst & 7) << 8;
            }
        }

        public BlendAlphaParameter() : base(ParameterType.BlendAlpha)
        {

        }
    }

    /// <summary>
    /// Ambient color of the geometry
    /// </summary>
    [Serializable]
    public class AmbientColorParameter : GCParameter
    {
        /// <summary>
        /// The Color of the gemoetry
        /// </summary>
        public uint AmbientColor
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
            }
        }

        public AmbientColorParameter() : base(ParameterType.AmbientColor)
        {
            data = uint.MaxValue;
        }
    }

    /// <summary>
    /// Texture information for the geometry
    /// </summary>
    [Serializable]
    public class TextureParameter : GCParameter
    {
        /// <summary>
        /// The id of the texture
        /// </summary>
        public ushort TextureID
        {
            get
            {
                return (ushort)(data & 0xFFFF);
            }
            set
            {
                data &= 0xFFFF0000;
                data |= value;
            }
        }

        /// <summary>
        /// Texture Tiling properties
        /// </summary>
        public GCTileMode Tile
        {
            get
            {
                return (GCTileMode)(data >> 16);
            }
            set
            {
                data &= 0xFFFF;
                data |= ((uint)value) << 16;
            }
        }

        public TextureParameter() : base(ParameterType.Texture)
        {
            TextureID = 0;
            Tile = GCTileMode.TileY | GCTileMode.TileX;
        }

        public TextureParameter(ushort TexID, GCTileMode tileMode) : base(ParameterType.Texture)
        {
            TextureID = TexID;
            Tile = tileMode;
        }
    }

    /// <summary>
    /// No idea what this is for, but its needed
    /// </summary>
    [Serializable]
    public class Unknown9Parameter : GCParameter
    {
        /// <summary>
        /// No idea what this does. Default is 4
        /// </summary>
        public ushort Unknown1
        {
            get
            {
                return (ushort)(data & 0xFFFF);
            }
            set
            {
                data &= 0xFFFF0000;
                data |= (uint)value;
            }
        }

        /// <summary>
        /// No idea what this does. Default is 0
        /// </summary>
        public ushort Unknown2
        {
            get
            {
                return (ushort)(data >> 16);
            }
            set
            {
                data &= 0xFFFF;
                data |= (uint)value << 16;
            }
        }

        public Unknown9Parameter() : base(ParameterType.Unknown_9)
        {
            // default values
            Unknown1 = 4;
            Unknown2 = 0;
        }
    }

    /// <summary>
    /// Determines where or how the geometry gets the texture coordinates
    /// </summary>
    [Serializable]
    public class TexCoordGenParameter : GCParameter
    {
        /// <summary>
        /// The output location of the generated texture coordinates
        /// </summary>
        public GCTexCoordID TexCoordID
        {
            get
            {
                return (GCTexCoordID)((data >> 16) & 0xFF);
            }
            set
            {
                data &= 0xFF00FFFF;
                data |= (uint)value << 16;
            }
        }

        /// <summary>
        /// The function to use for generating the texture coordinates
        /// </summary>
        public GCTexGenType TexGenType
        {
            get
            {
                return (GCTexGenType)((data >> 12) & 0xF);
            }
            set
            {
                data &= 0xFFFF0FFF;
                data |= (uint)value << 12;
            }
        }

        /// <summary>
        /// The source which should be used to generate the texture coordinates
        /// </summary>
        public GCTexGenSrc TexGenSrc
        {
            get
            {
                return (GCTexGenSrc)((data >> 4) & 0xFF);
            }
            set
            {
                data &= 0xFFFFF00F;
                data |= (uint)value << 4;
            }
        }

        /// <summary>
        /// The id of the matrix to use for generating the texture coordinates
        /// </summary>
        public GCTexGenMatrix MatrixID
        {
            get
            {
                return (GCTexGenMatrix)(data & 0xF);
            }
            set
            {
                data &= 0xFFFFFFF0;
                data |= (uint)value;
            }
        }

        public TexCoordGenParameter() : base(ParameterType.TexCoordGen)
        {

        }

        /// <summary>
        /// Create a custom Texture coordinate generation parameter
        /// </summary>
        /// <param name="texCoordID">The output location of the generated texture coordinates</param>
        /// <param name="texGenType">The function to use for generating the texture coordinates</param>
        /// <param name="texGenSrc">The source which should be used to generate the texture coordinates</param>
        /// <param name="matrixID">The id of the matrix to use for generating the texture coordinates</param>
        public TexCoordGenParameter(GCTexCoordID texCoordID, GCTexGenType texGenType, GCTexGenSrc texGenSrc, GCTexGenMatrix matrixID) : base(ParameterType.TexCoordGen)
        {
            TexCoordID = texCoordID;
            TexGenType = texGenType;
            TexGenSrc = texGenSrc;
            MatrixID = matrixID;
        }
    }
}
