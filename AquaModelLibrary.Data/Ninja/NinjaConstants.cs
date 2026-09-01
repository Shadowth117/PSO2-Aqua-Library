namespace AquaModelLibrary.Data.Ninja
{
    public class NinjaConstants
    {
        public const double FromBAMSvalueToRadians = ((2 * Math.PI) / 65536.0);
        public const double ToBAMSValueFromRadians = (65536.0 / (2 * Math.PI));
        public const double FromBAMSValueToDegrees = (360.0 / 65536.0);
        public const double ToBAMSValueFromDegrees = (65536.0 / 360.0);
        public const double FromDegreesToRadians = (Math.PI / 180.0);
        public const double FromRadiansToDegrees = (180.0 / Math.PI);
    }

    public enum AlphaInstruction : int
    {
        Zero = 0,
        One = 1,
        OtherColor = 2,
        InverseOtherColor = 3,
        SourceAlpha = 4,
        InverseSourceAlpha = 5,
        DestinationAlpha = 6,
        InverseDestinationAlpha = 7,
    }

    [Flags]
    public enum AnimFlags : ushort
    {
        Position = 0x1,
        Rotation = 0x2,
        Scale = 0x4,
        Vector = 0x8,
        Vertex = 0x10,
        Normal = 0x20, //In billy this is seemingly used for int16 rotations
        Target = 0x40,
        Roll = 0x80,
        Angle = 0x100,
        Color = 0x200,
        Intensity = 0x400,
        Spot = 0x800,
        Point = 0x1000,
        Quaternion = 0x2000
    }

    public enum InterpolationMode
    {
        Linear,
        Spline,
        User
    }

    public enum ChunkWeightStatus
    {
        Start,
        Middle,
        End
    }

    public enum NJD_MTYPE_FN
    {
        NJD_MTYPE_LINER = 0x0000, /* use liner                */
        NJD_MTYPE_SPLINE = 0x0040, /* use spline               */
        NJD_MTYPE_USER = 0x0080, /* use user function        */
        NJD_MTYPE_MASK = 0x00c0  /* Sampling mask*/
    }

    [Flags]
    public enum ObjectFlags : int
    {
        NoPosition = 0x0001,
        NoRotate = 0x0002,
        NoScale = 0x0004,
        NoDisplay = 0x0008,
        NoChildren = 0x0010,
        RotateZYX = 0x0020,
        NoAnimate = 0x0040,
        NoMorph = 0x0080,
        Clip = 0x0100,
        Modifier = 0x0200,
        Quaternion = 0x0400,
        RotateBase = 0x0800,
        RotateSet = 0x1000,
        Envelope = 0x2000
    }

    public enum ChunkType : byte
    {
        Null = 0,
        Bits = 1,
        Bits_BlendAlpha = Bits + 0,
        Bits_MipmapDAdjust = Bits + 1,
        Bits_SpecularExponent = Bits + 2,
        Bits_CachePolygonList = Bits + 3,
        Bits_DrawPolygonList = Bits + 4,
        Tiny = 8,
        Tiny_TextureID = Tiny + 0,
        Tiny_TextureID2 = Tiny + 1,
        Material = 16,
        Material_Diffuse = Material + 1,
        Material_Ambient = Material + 2,
        Material_DiffuseAmbient = Material + 3,
        Material_Specular = Material + 4,
        Material_DiffuseSpecular = Material + 5,
        Material_AmbientSpecular = Material + 6,
        Material_DiffuseAmbientSpecular = Material + 7,
        Material_Bump = Material + 8,
        Material_Diffuse2 = Material + 9,
        Material_Ambient2 = Material + 10,
        Material_DiffuseAmbient2 = Material + 11,
        Material_Specular2 = Material + 12,
        Material_DiffuseSpecular2 = Material + 13,
        Material_AmbientSpecular2 = Material + 14,
        Material_DiffuseAmbientSpecular2 = Material + 15,
        Vertex = 32,
        Vertex_VertexSH = Vertex + 0,
        Vertex_VertexNormalSH = Vertex + 1,
        Vertex_Vertex = Vertex + 2,
        Vertex_VertexDiffuse8 = Vertex + 3,
        Vertex_VertexUserFlags = Vertex + 4,
        Vertex_VertexNinjaFlags = Vertex + 5,
        Vertex_VertexDiffuseSpecular5 = Vertex + 6,
        Vertex_VertexDiffuseSpecular4 = Vertex + 7,
        Vertex_VertexDiffuseSpecular16 = Vertex + 8,
        Vertex_VertexNormal = Vertex + 9,
        Vertex_VertexNormalDiffuse8 = Vertex + 10,
        Vertex_VertexNormalUserFlags = Vertex + 11,
        Vertex_VertexNormalNinjaFlags = Vertex + 12,
        Vertex_VertexNormalDiffuseSpecular5 = Vertex + 13,
        Vertex_VertexNormalDiffuseSpecular4 = Vertex + 14,
        Vertex_VertexNormalDiffuseSpecular16 = Vertex + 15,
        Vertex_VertexNormalX = Vertex + 16,
        Vertex_VertexNormalXDiffuse8 = Vertex + 17,
        Vertex_VertexNormalXUserFlags = Vertex + 18,
        Volume = 56,
        Volume_Polygon3 = Volume + 0,
        Volume_Polygon4 = Volume + 1,
        Volume_Strip = Volume + 2,
        Strip = 64,
        Strip_Strip = Strip + 0,
        Strip_StripUVN = Strip + 1,
        Strip_StripUVH = Strip + 2,
        Strip_StripNormal = Strip + 3,
        Strip_StripUVNNormal = Strip + 4,
        Strip_StripUVHNormal = Strip + 5,
        Strip_StripColor = Strip + 6,
        Strip_StripUVNColor = Strip + 7,
        Strip_StripUVHColor = Strip + 8,
        Strip_Strip2 = Strip + 9,
        Strip_StripUVN2 = Strip + 10,
        Strip_StripUVH2 = Strip + 11,
        End = 255
    }
}
