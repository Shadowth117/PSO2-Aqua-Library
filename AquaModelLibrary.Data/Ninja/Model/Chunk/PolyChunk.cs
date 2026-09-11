using AquaModelLibrary.Data.Ninja.Model.Basic;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers.Writers;
using System.Drawing;
using System.Numerics;

namespace AquaModelLibrary.Data.Ninja.Model.Chunk
{
    public abstract class PolyChunk
    {
        public ushort Header { get; set; }

        public ChunkType Type
        {
            get { return (ChunkType)(Header & 0xFF); }
            protected set { Header = (ushort)((Header & 0xFF00) | (byte)value); }
        }
        public ChunkType CMDEType
        {
            get { return Type; }
            set
            {
                ChunkType oldtype = Type;
                Header = (ushort)((Header & 0xFF00) | (byte)value);
            }
        }

        public byte Flags
        {
            get { return (byte)(Header >> 8); }
            set { Header = (ushort)((Header & 0xFF) | (ushort)(value << 8)); }
        }

        public abstract int ByteSize { get; }

        public static PolyChunk Load(BufferedStreamReaderBE<MemoryStream> sr)
        {
            ChunkType type = (ChunkType)(sr.ReadBE<ushort>() & 0xFF);
            switch (type)
            {
                case ChunkType.Null:
                    return new PolyChunkNull(sr);
                case ChunkType.Bits_BlendAlpha:
                    return new PolyChunkBitsBlendAlpha(sr);
                case ChunkType.Bits_MipmapDAdjust:
                    return new PolyChunkBitsMipmapDAdjust(sr);
                case ChunkType.Bits_SpecularExponent:
                    return new PolyChunkBitsSpecularExponent(sr);
                case ChunkType.Bits_CachePolygonList:
                    return new PolyChunkBitsCachePolygonList(sr);
                case ChunkType.Bits_DrawPolygonList:
                    return new PolyChunkBitsDrawPolygonList(sr);
                case ChunkType.Tiny_TextureID:
                case ChunkType.Tiny_TextureID2:
                    return new PolyChunkTinyTextureID(sr);
                case ChunkType.Material_Diffuse:
                case ChunkType.Material_Ambient:
                case ChunkType.Material_DiffuseAmbient:
                case ChunkType.Material_Specular:
                case ChunkType.Material_DiffuseSpecular:
                case ChunkType.Material_AmbientSpecular:
                case ChunkType.Material_DiffuseAmbientSpecular:
                case ChunkType.Material_Diffuse2:
                case ChunkType.Material_Ambient2:
                case ChunkType.Material_DiffuseAmbient2:
                case ChunkType.Material_Specular2:
                case ChunkType.Material_DiffuseSpecular2:
                case ChunkType.Material_AmbientSpecular2:
                case ChunkType.Material_DiffuseAmbientSpecular2:
                    return new PolyChunkMaterial(sr);
                case ChunkType.Material_Bump:
                    return new PolyChunkMaterialBump(sr);
                case ChunkType.Volume_Polygon3:
                case ChunkType.Volume_Polygon4:
                case ChunkType.Volume_Strip:
                    return new PolyChunkVolume(sr);
                case ChunkType.Strip_Strip:
                case ChunkType.Strip_StripUVN:
                case ChunkType.Strip_StripUVH:
                case ChunkType.Strip_StripColor:
                case ChunkType.Strip_StripUVNColor:
                case ChunkType.Strip_StripUVHColor:
                case ChunkType.Strip_Strip2:
                case ChunkType.Strip_StripUVN2:
                case ChunkType.Strip_StripUVH2:
                    return new PolyChunkStrip(sr);
                case ChunkType.End:
                    return new PolyChunkEnd(sr);
                default:
                    throw new NotSupportedException("Unsupported chunk type " + type + " at " + sr.Position.ToString("X8") + ".");
            }
        }

        public abstract void Write(ByteListWriter outBytes);
    }

    public abstract class PolyChunkBits : PolyChunk
    {
        public override int ByteSize
        {
            get { return 2; }
        }

        public override void Write(ByteListWriter outBytes)
        {
            outBytes.AddValue(Header);
        }
    }

    public class PolyChunkNull : PolyChunkBits
    {
        public PolyChunkNull()
        {
            Type = ChunkType.Null;
        }

        public PolyChunkNull(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Header = sr.ReadBE<ushort>();
        }
    }

    public class PolyChunkEnd : PolyChunkBits
    {
        public PolyChunkEnd()
        {
            Type = ChunkType.End;
        }

        public PolyChunkEnd(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Header = sr.ReadBE<ushort>();
        }
    }

    public class PolyChunkBitsBlendAlpha : PolyChunkBits
    {
        public AlphaInstruction SourceAlpha
        {
            get { return (AlphaInstruction)((Flags >> 3) & 7); }
            set { Flags = (byte)((Flags & ~0x38) | ((byte)value << 3)); }
        }

        public AlphaInstruction DestinationAlpha
        {
            get { return (AlphaInstruction)(Flags & 7); }
            set { Flags = (byte)((Flags & ~7) | (byte)value); }
        }
        public bool SourceBufferSelect
        {
            get { return (Flags & 0x40) == 0x40; }
            set { Flags = (byte)((Flags & ~0x40) | (value ? 0x40 : 0)); }
        }

        public bool DestinationBufferSelect
        {
            get { return (Flags & 0x80) == 0x80; }
            set { Flags = (byte)((Flags & ~0x80) | (value ? 0x80 : 0)); }
        }
        public PolyChunkBitsBlendAlpha()
        {
            Type = ChunkType.Bits_BlendAlpha;
        }

        public PolyChunkBitsBlendAlpha(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Header = sr.ReadBE<ushort>();
        }
    }

    public class PolyChunkBitsMipmapDAdjust : PolyChunkBits
    {
        public float MipmapDAdjust
        {
            get { return (Flags & 0xF) * 0.25f; }
            set
            {
                Flags = (byte)((Flags & 0xF0) | (byte)Math.Max(0, Math.Min(0xF, Math.Round(value / 0.25, MidpointRounding.AwayFromZero))));
            }
        }

        public PolyChunkBitsMipmapDAdjust()
        {
            Type = ChunkType.Bits_MipmapDAdjust;
        }

        public PolyChunkBitsMipmapDAdjust(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Header = sr.ReadBE<ushort>();
        }
    }

    public class PolyChunkBitsSpecularExponent : PolyChunkBits
    {
        public byte SpecularExponent
        {
            get { return (byte)(Flags & 0x1F); }
            set { Flags = (byte)((Flags & ~0x1F) | Math.Min(value, (byte)16)); }
        }

        public PolyChunkBitsSpecularExponent()
        {
            Type = ChunkType.Bits_SpecularExponent;
        }

        public PolyChunkBitsSpecularExponent(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Header = sr.ReadBE<ushort>();
        }
    }

    public class PolyChunkBitsCachePolygonList : PolyChunkBits
    {
        public byte List
        {
            get { return Flags; }
            set { Flags = value; }
        }

        public PolyChunkBitsCachePolygonList()
        {
            Type = ChunkType.Bits_CachePolygonList;
        }

        public PolyChunkBitsCachePolygonList(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Header = sr.ReadBE<ushort>();
        }
    }

    public class PolyChunkBitsDrawPolygonList : PolyChunkBits
    {
        public byte List
        {
            get { return Flags; }
            set { Flags = value; }
        }

        public PolyChunkBitsDrawPolygonList()
        {
            Type = ChunkType.Bits_DrawPolygonList;
        }

        public PolyChunkBitsDrawPolygonList(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Header = sr.ReadBE<ushort>();
        }
    }

    public class PolyChunkTinyTextureID : PolyChunk
    {
        public bool Second { get; set; }

        public float MipmapDAdjust
        {
            get { return (Flags & 0xF) * 0.25f; }
            set
            {
                Flags = (byte)((Flags & 0xF0) | (byte)Math.Max(0, Math.Min(0xF, Math.Round(value / 0.25, MidpointRounding.AwayFromZero))));
            }
        }

        public bool ClampV
        {
            get { return (Flags & 0x10) == 0x10; }
            set { Flags = (byte)((Flags & ~0x10) | (value ? 0x10 : 0)); }
        }

        public bool ClampU
        {
            get { return (Flags & 0x20) == 0x20; }
            set { Flags = (byte)((Flags & ~0x20) | (value ? 0x20 : 0)); }
        }

        public bool FlipV
        {
            get { return (Flags & 0x40) == 0x40; }
            set { Flags = (byte)((Flags & ~0x40) | (value ? 0x40 : 0)); }
        }

        public bool FlipU
        {
            get { return (Flags & 0x80) == 0x80; }
            set { Flags = (byte)((Flags & ~0x80) | (value ? 0x80 : 0)); }
        }

        public ushort Data { get; set; }

        public ushort TextureID
        {
            get { return (ushort)(Data & 0x1FFF); }
            set { Data = (ushort)((Data & ~0x1FFF) | Math.Min(value, (ushort)0x1FFF)); }
        }

        public bool SuperSample
        {
            get { return (Data & 0x2000) == 0x2000; }
            set { Data = (ushort)((Data & ~0x2000) | (value ? 0x2000 : 0)); }
        }

        public FilterMode FilterMode
        {
            get { return (FilterMode)(Data >> 14); }
            set { Data = (ushort)((Data & ~0xC000) | ((ushort)value << 14)); }
        }

        public override int ByteSize
        {
            get { return 4; }
        }

        public PolyChunkTinyTextureID()
        {
            Type = ChunkType.Tiny_TextureID;
        }

        public PolyChunkTinyTextureID(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Header = sr.ReadBE<ushort>();
            Second = Type == ChunkType.Tiny_TextureID2;
            Data = sr.ReadBE<ushort>();
        }

        public PolyChunkTinyTextureID(NJSMaterial mat)
            : this()
        {
            MipmapDAdjust = mat.MipmapDAdjust;
            ClampV = mat.ClampV;
            ClampU = mat.ClampU;
            FlipV = mat.FlipV;
            FlipU = mat.FlipU;
            TextureID = (ushort)mat.TextureID;
            SuperSample = mat.SuperSample;
            FilterMode = mat.FilterMode;
        }

        public override void Write(ByteListWriter outBytes)
        {
            Type = Second ? ChunkType.Tiny_TextureID2 : ChunkType.Tiny_TextureID;
            outBytes.AddValue(Header);
            outBytes.AddValue(Data);
        }
    }

    public abstract class PolyChunkSize : PolyChunk
    {
        public ushort Size { get; protected set; }

        public override int ByteSize
        {
            get { return (Size * 2) + 4; }
        }

        public override void Write(ByteListWriter outBytes)
        {
            outBytes.AddValue(Header);
            outBytes.AddValue(Size);
        }
    }

    public class PolyChunkMaterial : PolyChunkSize
    {
        public AlphaInstruction SourceAlpha
        {
            get { return (AlphaInstruction)((Flags >> 3) & 7); }
            set { Flags = (byte)((Flags & ~0x38) | ((byte)value << 3)); }
        }

        public AlphaInstruction DestinationAlpha
        {
            get { return (AlphaInstruction)(Flags & 7); }
            set { Flags = (byte)((Flags & ~7) | (byte)value); }
        }

        public bool SourceBufferSelect
        {
            get { return (Flags & 0x40) == 0x40; }
            set { Flags = (byte)((Flags & ~0x40) | (value ? 0x40 : 0)); }
        }

        public bool DestinationBufferSelect
        {
            get { return (Flags & 0x80) == 0x80; }
            set { Flags = (byte)((Flags & ~0x80) | (value ? 0x80 : 0)); }
        }

        public Color? Diffuse { get; set; }
        public Color? Ambient { get; set; }
        public Color? Specular { get; set; }
        public byte SpecularExponent { get; set; }
        public bool Second { get; set; }

        public PolyChunkMaterial()
        {
            Type = ChunkType.Material_Diffuse;
            Diffuse = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
        }

        public PolyChunkMaterial(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Header = sr.ReadBE<ushort>();
            Size = sr.ReadBE<ushort>();
            switch (Type)
            {
                case ChunkType.Material_Diffuse:
                case ChunkType.Material_DiffuseAmbient:
                case ChunkType.Material_DiffuseSpecular:
                case ChunkType.Material_DiffuseAmbientSpecular:
                case ChunkType.Material_Diffuse2:
                case ChunkType.Material_DiffuseAmbient2:
                case ChunkType.Material_DiffuseSpecular2:
                case ChunkType.Material_DiffuseAmbientSpecular2:
                    Diffuse = NinjaModelCommon.ReadColorARGB8888_16(sr._BEReadActive, sr.ReadBE<ushort>(), sr.ReadBE<ushort>());
                    break;
            }
            switch (Type)
            {
                case ChunkType.Material_Ambient:
                case ChunkType.Material_DiffuseAmbient:
                case ChunkType.Material_AmbientSpecular:
                case ChunkType.Material_DiffuseAmbientSpecular:
                case ChunkType.Material_Ambient2:
                case ChunkType.Material_DiffuseAmbient2:
                case ChunkType.Material_AmbientSpecular2:
                case ChunkType.Material_DiffuseAmbientSpecular2:
                    Ambient = NinjaModelCommon.ReadColorXRGB8888_16(sr._BEReadActive, sr.ReadBE<ushort>(), sr.ReadBE<ushort>());
                    break;
            }
            switch (Type)
            {
                case ChunkType.Material_Specular:
                case ChunkType.Material_DiffuseSpecular:
                case ChunkType.Material_AmbientSpecular:
                case ChunkType.Material_DiffuseAmbientSpecular:
                case ChunkType.Material_Specular2:
                case ChunkType.Material_DiffuseSpecular2:
                case ChunkType.Material_AmbientSpecular2:
                case ChunkType.Material_DiffuseAmbientSpecular2:
                    var data0 = sr.ReadBE<ushort>();
                    var data1 = sr.ReadBE<ushort>();
                    Specular = NinjaModelCommon.ReadColorXRGB8888_16(sr._BEReadActive, data0, data1);
                    SpecularExponent = (byte)(data1 >> 8);
                    break;
            }
            switch (Type)
            {
                case ChunkType.Material_Diffuse2:
                case ChunkType.Material_Ambient2:
                case ChunkType.Material_DiffuseAmbient2:
                case ChunkType.Material_Specular2:
                case ChunkType.Material_DiffuseSpecular2:
                case ChunkType.Material_AmbientSpecular2:
                case ChunkType.Material_DiffuseAmbientSpecular2:
                    Second = true;
                    break;
            }
        }
    }

    public class PolyChunkMaterialBump : PolyChunkSize
    {
        public short DX { get; set; }
        public short DY { get; set; }
        public short DZ { get; set; }
        public short UX { get; set; }
        public short UY { get; set; }
        public short UZ { get; set; }

        public PolyChunkMaterialBump()
        {
            Type = ChunkType.Material_Bump;
            Size = 6;
        }

        public PolyChunkMaterialBump(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Header = sr.ReadBE<ushort>();
            Size = sr.ReadBE<ushort>();
            DX = sr.ReadBE<short>();
            DY = sr.ReadBE<short>();
            DZ = sr.ReadBE<short>();
            UX = sr.ReadBE<short>();
            UY = sr.ReadBE<short>();
            UZ = sr.ReadBE<short>();
        }

        public override void Write(ByteListWriter outBytes)
        {
            base.Write(outBytes);
            outBytes.AddValue(DX);
            outBytes.AddValue(DY);
            outBytes.AddValue(DZ);
            outBytes.AddValue(UX);
            outBytes.AddValue(UY);
            outBytes.AddValue(UZ);
        }
    }

    public class PolyChunkVolume : PolyChunkSize
    {
        [Serializable]
        public sealed class Triangle : Poly
        {
            public List<ushort> UserFlags = new();

            public Triangle()
            {
                Indices = new ushort[3];
            }

            public Triangle(BufferedStreamReaderBE<MemoryStream> sr, byte userFlags)
                : this()
            {
                Indices[0] = sr.ReadBE<ushort>();
                Indices[1] = sr.ReadBE<ushort>();
                Indices[2] = sr.ReadBE<ushort>();
                for(int i = 0; i < userFlags; i++)
                {
                    if (i > 2)
                    {
                        break;
                    }
                    UserFlags.Add(sr.ReadBE<ushort>());
                }
            }

            public override byte[] GetBytes(bool bigEndian)
            {
                var result = new ByteListWriter() { AddAsBigEndian = bigEndian};
                foreach (ushort item in Indices)
                    result.AddValue(item);
                foreach (ushort item in UserFlags)
                    result.AddValue(item);
                return result.ToArray();
            }

            public override int Size
            {
                get
                {
                    int size = 6 + (UserFlags.Count * 2);
                    return size;
                }
            }
        }

        [Serializable]
        public sealed class Quad : Poly
        {
            public List<ushort> UserFlags = new();

            public Quad()
            {
                Indices = new ushort[4];
            }

            public Quad(BufferedStreamReaderBE<MemoryStream> sr, byte userFlags)
                : this()
            {
                Indices[0] = sr.ReadBE<ushort>();
                Indices[1] = sr.ReadBE<ushort>();
                Indices[2] = sr.ReadBE<ushort>();
                Indices[3] = sr.ReadBE<ushort>();
                for (int i = 0; i < userFlags; i++)
                {
                    if (i > 2)
                    {
                        break;
                    }
                    UserFlags.Add(sr.ReadBE<ushort>());
                }
            }

            public override byte[] GetBytes(bool bigEndian)
            {
                var result = new ByteListWriter() { AddAsBigEndian = bigEndian };
                foreach (ushort item in Indices)
                    result.AddValue(item);
                foreach (ushort item in UserFlags)
                    result.AddValue(item);
                return result.ToArray();
            }

            public override int Size
            {
                get
                {
                    int size = 8 + (UserFlags.Count * 2);
                    return size;
                }
            }
        }

        [Serializable]
        public sealed class Strip : Poly
        {
            public bool Reversed { get; private set; }
            public List<List<ushort>> UserFlags = new();

            public Strip(int NumVerts, bool Reverse)
            {
                Indices = new ushort[NumVerts];
                Reversed = Reverse;
            }

            public Strip(ushort[] VertIndices, bool Reverse)
            {
                Indices = VertIndices;
                Reversed = Reverse;
            }

            public Strip(BufferedStreamReaderBE<MemoryStream> sr, byte userFlags)
            {
                var headerFlags = sr.ReadBE<ushort>();
                Indices = new ushort[headerFlags & 0x7FFF];
                Reversed = (headerFlags & 0x8000) == 0x8000;

                for (int i = 0; i < Indices.Length; i++)
                {
                    Indices[i] = sr.ReadBE<ushort>();
                    if (i > 1)
                    {
                        List<ushort> userFlagSet = new();
                        for (int j = 0; j < userFlags; j++)
                        {
                            if (j > 2)
                            {
                                break;
                            }
                            userFlagSet.Add(sr.ReadBE<ushort>());
                        }
                        UserFlags.Add(userFlagSet);
                    }
                }
            }

            public override int Size
            {
                get
                {
                    int size = 2;
                    size += Indices.Length * 2 + UserFlags[0].Count * UserFlags.Count * 2;
                    return size;
                }
            }

            public override byte[] GetBytes(bool bigEndian)
            {
                ByteListWriter result = new ByteListWriter() { AddAsBigEndian = bigEndian };
                int ind = Indices.Length;
                if (Reversed)
                    ind = -ind;
                result.AddValue((short)(ind));
                for (int i = 0; i < Indices.Length; i++)
                {
                    result.AddValue(Indices[i]);
                    if (i > 1)
                    {
                        foreach(var flag in UserFlags[i - 2])
                        {
                            result.AddValue(flag);
                        }
                    }
                }
                return result.ToArray();
            }
        }

        public abstract class Poly
        {
            public ushort[] Indices { get; protected set; }

            internal Poly()
            {
            }

            public static Poly CreatePoly(ChunkType type)
            {
                switch (type)
                {
                    case ChunkType.Volume_Polygon3:
                        return new Triangle();
                    case ChunkType.Volume_Polygon4:
                        return new Quad();
                    case ChunkType.Volume_Strip:
                        throw new ArgumentException(
                            "Cannot create strip-type poly without additional information.\nUse Strip.Strip(int NumVerts, bool Reverse) instead.",
                            "type");
                }
                throw new ArgumentException("Unknown poly type!", "type");
            }

            public static Poly CreatePoly(ChunkType type, BufferedStreamReaderBE<MemoryStream> sr, byte userFlags)
            {
                switch (type)
                {
                    case ChunkType.Volume_Polygon3:
                        return new Triangle(sr, userFlags);
                    case ChunkType.Volume_Polygon4:
                        return new Quad(sr, userFlags);
                    case ChunkType.Volume_Strip:
                        return new Strip(sr, userFlags);
                }
                throw new ArgumentException("Unknown poly type!", "type");
            }

            public abstract int Size { get; }

            public abstract byte[] GetBytes(bool bigEndian);
        }

        public ushort Header2 { get; private set; }

        public byte UserFlags
        {
            get { return (byte)(Header2 >> 14); }
            private set { Header2 = (ushort)((Header2 & 0x3FFF) | ((value & 3) << 14)); }
        }

        public ushort PolyCount
        {
            get { return (ushort)Polys.Count; }
            private set { Header2 = (ushort)((Header2 & 0xC000) | (value & 0x3FFF)); }
        }

        public List<Poly> Polys { get; private set; }

        public PolyChunkVolume(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Header = sr.ReadBE<ushort>();
            Size = sr.ReadBE<ushort>();
            Header2 = sr.ReadBE<ushort>();
            int polyCount = Header2 & 0x3FFF;
            Polys = new List<Poly>(polyCount);
            for (int i = 0; i < polyCount; i++)
            {
                Poly str = Poly.CreatePoly(Type, sr, UserFlags);
                Polys.Add(str);
            }
        }

        public override void Write(ByteListWriter outBytes)
        {
            PolyCount = (ushort)Polys.Count;
            Size = 1;
            foreach (Poly str in Polys)
                Size += (ushort)(str.Size / 2);
            base.Write(outBytes);
            outBytes.AddValue(Header2);
            foreach (Poly str in Polys)
                outBytes.AddRange(str.GetBytes(outBytes.AddAsBigEndian));
        }
    }

    public class PolyChunkStrip : PolyChunkSize
    {
        [Serializable]
        public class Strip
        {
            public bool Reversed { get; private set; }
            public bool CMDEReversed
            {
                get { return Reversed; }
                set
                {
                    bool oldsettype = Reversed;
                    Reversed = value;
                }
            }
            public ushort[] Indices { get; private set; }
            public Vector2[] UVs { get; private set; }
            public Vector2[] CMDEUVs
            {
                get { return UVs; }
                set
                {
                    Vector2[] olduvs = UVs;
                    UVs = value;
                }
            }
            public Vector2[] UVs2 { get; private set; }
            public Color[] VColors { get; private set; }
            public List<List<ushort>> UserFlags = new();

            public Strip(bool reversed, ushort[] indexes, Vector2[] uvs, Color[] vcolors)
            {
                Reversed = reversed;
                Indices = indexes;
                UVs = uvs;
                VColors = vcolors;
            }

            public Strip(bool reversed, ushort[] indexes, Vector2[] uvs, Vector2[] uvs2, Color[] vcolors)
            {
                Reversed = reversed;
                Indices = indexes;
                UVs = uvs;
                UVs2 = uvs2;
                VColors = vcolors;
            }

            public Strip(BufferedStreamReaderBE<MemoryStream> sr, ChunkType type, byte userFlags)
            {
                bool SADXColorReverse = sr.streamChecks.ContainsKey("SADXColorReverse") ? sr.streamChecks["SADXColorReverse"] : false;
                var head = sr.ReadBE<ushort>();
                Indices = new ushort[head & 0x7FFF];
                Reversed = (head & 0x8000) == 0x8000;
                switch (type)
                {
                    case ChunkType.Strip_StripUVN:
                    case ChunkType.Strip_StripUVH:
                        UVs = new Vector2[Indices.Length];
                        break;
                    case ChunkType.Strip_StripUVN2:
                    case ChunkType.Strip_StripUVH2:
                        UVs = new Vector2[Indices.Length];
                        UVs2 = new Vector2[Indices.Length];
                        break;
                    case ChunkType.Strip_StripColor:
                        VColors = new Color[Indices.Length];
                        break;
                    case ChunkType.Strip_StripUVNColor:
                    case ChunkType.Strip_StripUVHColor:
                        UVs = new Vector2[Indices.Length];
                        VColors = new Color[Indices.Length];
                        break;
                }
                for (int i = 0; i < Indices.Length; i++)
                {
                    Indices[i] = sr.ReadBE<ushort>();
                    switch (type)
                    {
                        case ChunkType.Strip_StripUVN:
                        case ChunkType.Strip_StripUVNColor:
                        case ChunkType.Strip_StripUVN2:
                            UVs[i] = NinjaModelCommon.ReadUV(sr, false, true, SADXColorReverse);
                            break;
                        case ChunkType.Strip_StripUVH:
                        case ChunkType.Strip_StripUVHColor:
                        case ChunkType.Strip_StripUVH2:
                            UVs[i] = NinjaModelCommon.ReadUV(sr, true, true, SADXColorReverse);
                            break;
                    }
                    switch (type)
                    {
                        case ChunkType.Strip_StripColor:
                        case ChunkType.Strip_StripUVNColor:
                        case ChunkType.Strip_StripUVHColor:
                            VColors[i] = NinjaModelCommon.ReadColorARGB8888_16(sr._BEReadActive, sr.ReadBE<ushort>(), sr.ReadBE<ushort>());
                            break;
                    }
                    switch (type)
                    {
                        case ChunkType.Strip_StripUVN2:
                            UVs2[i] = NinjaModelCommon.ReadUV(sr, false, true, SADXColorReverse);
                            break;
                        case ChunkType.Strip_StripUVH2:
                            UVs2[i] = NinjaModelCommon.ReadUV(sr, true, true, SADXColorReverse);
                            break;
                    }

                    if (i > 1)
                    {
                        List<ushort> userFlagSet = new();
                        for (int j = 0; j < userFlags; j++)
                        {
                            if (j > 2)
                            {
                                break;
                            }
                            userFlagSet.Add(sr.ReadBE<ushort>());
                        }
                        UserFlags.Add(userFlagSet);
                    }
                }
            }

            public byte[] GetBytes(ChunkType type, bool bigEndian, bool SADXColorReverse)
            {
                ByteListWriter result = new ByteListWriter() { AddAsBigEndian = bigEndian };
                int ind = Indices.Length;
                if (Reversed)
                    ind = -ind;
                result.AddValue((short)(ind));
                for (int i = 0; i < Indices.Length; i++)
                {
                    result.AddValue(Indices[i]);
                    switch (type)
                    {
                        case ChunkType.Strip_StripUVN:
                        case ChunkType.Strip_StripUVNColor:
                        case ChunkType.Strip_StripUVN2:
                            NinjaModelCommon.GetUVBytes(result, UVs[i], false, true, SADXColorReverse);
                            break;
                        case ChunkType.Strip_StripUVH:
                        case ChunkType.Strip_StripUVHColor:
                        case ChunkType.Strip_StripUVH2:
                            NinjaModelCommon.GetUVBytes(result, UVs[i], true, true, SADXColorReverse);
                            break;
                    }
                    switch (type)
                    {
                        case ChunkType.Strip_StripColor:
                        case ChunkType.Strip_StripUVNColor:
                        case ChunkType.Strip_StripUVHColor:
                            result.AddRange(NinjaModelCommon.GetBytesColorARGB8888_16(VColors[i]));
                            break;
                    }
                    switch (type)
                    {
                        case ChunkType.Strip_StripUVN2:
                            NinjaModelCommon.GetUVBytes(result, UVs2[i], false, true, SADXColorReverse);
                            break;
                        case ChunkType.Strip_StripUVH2:
                            NinjaModelCommon.GetUVBytes(result, UVs2[i], true, true, SADXColorReverse);
                            break;
                    }
                    if (i > 1)
                    {
                        foreach (var flag in UserFlags[i - 2])
                        {
                            result.AddValue(flag);
                        }
                    }
                }
                return result.ToArray();
            }

            public int Size
            {
                get
                {
                    int size = 2;
                    size += Indices.Length * 2;
                    if (UVs != null)
                        size += UVs.Length * 4;
                    if (VColors != null)
                        size += VColors.Length * 4;
                    if (UVs2 != null)
                        size += UVs2.Length * 4;
                    if (UserFlags.Count > 0)
                    {
                        size += UserFlags.Count * UserFlags[0].Count * 2;
                    }
                    return size;
                }
            }
        }

        public bool IgnoreLight
        {
            get { return (Flags & 1) == 1; }
            set { Flags = (byte)((Flags & ~1) | (value ? 1 : 0)); }
        }

        public bool IgnoreSpecular
        {
            get { return (Flags & 2) == 2; }
            set { Flags = (byte)((Flags & ~2) | (value ? 2 : 0)); }
        }

        public bool IgnoreAmbient
        {
            get { return (Flags & 4) == 4; }
            set { Flags = (byte)((Flags & ~4) | (value ? 4 : 0)); }
        }

        public bool UseAlpha
        {
            get { return (Flags & 8) == 8; }
            set { Flags = (byte)((Flags & ~8) | (value ? 8 : 0)); }
        }

        public bool DoubleSide
        {
            get { return (Flags & 0x10) == 0x10; }
            set { Flags = (byte)((Flags & ~0x10) | (value ? 0x10 : 0)); }
        }

        public bool FlatShading
        {
            get { return (Flags & 0x20) == 0x20; }
            set { Flags = (byte)((Flags & ~0x20) | (value ? 0x20 : 0)); }
        }

        public bool EnvironmentMapping
        {
            get { return (Flags & 0x40) == 0x40; }
            set { Flags = (byte)((Flags & ~0x40) | (value ? 0x40 : 0)); }
        }

        public bool NoPunchthrough
        {
            get { return (Flags & 0x80) == 0x80; }
            set { Flags = (byte)((Flags & ~0x80) | (value ? 0x80 : 0)); }
        }

        public ushort Header2 { get; private set; }

        public byte UserFlags
        {
            get { return (byte)(Header2 >> 14); }
            private set { Header2 = (ushort)((Header2 & 0x3FFF) | ((value & 3) << 14)); }
        }

        public ushort StripCount
        {
            get { return (ushort)Strips.Count; }
            private set { Header2 = (ushort)((Header2 & 0xC000) | (value & 0x3FFF)); }
        }

        public List<Strip> Strips { get; private set; }
        public List<Strip> CMDEStrips
        {
            get { return Strips; }
            set
            {
                List<Strip> oldstrips = Strips;
                Strips = value;
            }
        }

        public PolyChunkStrip(ChunkType type)
        {
            Type = type;
            Strips = new List<Strip>();
        }

        public PolyChunkStrip(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Header = sr.ReadBE<ushort>();
            Size = sr.ReadBE<ushort>();
            Header2 = sr.ReadBE<ushort>();
            int stripCount = Header2 & 0x3FFF;
            Strips = new List<Strip>(stripCount);
            for (int i = 0; i < stripCount; i++)
            {
                Strip str = new Strip(sr, Type, UserFlags);
                Strips.Add(str);
            }
        }

        public override void Write(ByteListWriter outBytes)
        {
            bool SADXColorReverse = outBytes.writeChecks.ContainsKey("SADXColorReverse") ? outBytes.writeChecks["SADXColorReverse"] : false;
            StripCount = (ushort)Strips.Count;
            Size = 1;
            foreach (Strip str in Strips)
                Size += (ushort)(str.Size / 2);

            int alignmentPadding = (Size % 2);
            Size += (ushort)alignmentPadding;

            base.Write(outBytes);
            outBytes.AddValue(Header2);
            foreach (Strip str in Strips)
                outBytes.AddRange(str.GetBytes(Type, outBytes.AddAsBigEndian, SADXColorReverse));

            if (alignmentPadding > 0)
            {
                outBytes.AddRange(new byte[2]);
            }
        }

        public void UpdateFlags(NJSMaterial mat)
        {
            IgnoreLight = mat.IgnoreLighting;
            IgnoreSpecular = mat.IgnoreSpecular;
            UseAlpha = mat.UseAlpha;
            DoubleSide = mat.DoubleSided;
            FlatShading = mat.FlatShading;
            EnvironmentMapping = mat.EnvironmentMap;
        }
    }
}
