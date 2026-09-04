using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace AquaModelLibrary.Data.Ninja.Model.Chunk
{
    public class VertexChunk
    {
        public uint Header1 { get; set; }

        public ChunkType Type
        {
            get { return (ChunkType)(Header1 & 0xFF); }
            private set { Header1 = (Header1 & 0xFFFFFF00u) | (byte)value; }
        }

        public byte Flags
        {
            get { return (byte)((Header1 >> 8) & 0xFF); }
            set { Header1 = (Header1 & 0xFFFF00FFu) | (uint)(value << 8); }
        }

        public ChunkWeightStatus WeightStatus
        {
            get { return (ChunkWeightStatus)(Flags & 3); }
            set { Flags = (byte)((Flags & ~3) | (int)value); }
        }

        public ushort Size
        {
            get { return (ushort)(Header1 >> 16); }
            set { Header1 = (Header1 & 0xFFFFu) | (uint)(value << 16); }
        }

        public uint Header2 { get; set; }

        public ushort IndexOffset
        {
            get { return (ushort)(Header2 & 0xFFFF); }
            set { Header2 = (Header2 & 0xFFFF0000u) | value; }
        }

        private uint GetVertCount() => Header2 >> 16;

        private void SetVertCount(int count) => Header2 = (Header2 & 0xFFFFu) | (uint)(count << 16);

        public int VertexCount => Vertices.Count;

        public bool HasWeight { get { return Type == ChunkType.Vertex_VertexNinjaFlags | Type == ChunkType.Vertex_VertexNormalNinjaFlags; } }

        public List<Vector3> Vertices { get; set; }
        public List<Vector3> Normals { get; set; }
        public List<Color> Diffuse { get; set; }
        public List<Color> Specular { get; set; }
        public List<uint> UserFlags { get; set; }
        public List<uint> NinjaFlags { get; set; }

        public VertexChunk()
        {
            Type = ChunkType.Vertex_Vertex;
            Vertices = new List<Vector3>();
            Normals = new List<Vector3>();
            Diffuse = new List<Color>();
            Specular = new List<Color>();
            UserFlags = new List<uint>();
            NinjaFlags = new List<uint>();
        }

        public VertexChunk(ChunkType type)
            : this()
        {
            Type = type;
            switch (type)
            {
                case ChunkType.Vertex_VertexSH:
                case ChunkType.Vertex_Vertex:
                case ChunkType.Vertex_VertexDiffuse8:
                case ChunkType.Vertex_VertexUserFlags:
                case ChunkType.Vertex_VertexNinjaFlags:
                case ChunkType.Vertex_VertexDiffuseSpecular5:
                case ChunkType.Vertex_VertexDiffuseSpecular4:
                case ChunkType.Vertex_VertexNormalSH:
                case ChunkType.Vertex_VertexNormal:
                case ChunkType.Vertex_VertexNormalDiffuse8:
                case ChunkType.Vertex_VertexNormalUserFlags:
                case ChunkType.Vertex_VertexNormalNinjaFlags:
                case ChunkType.Vertex_VertexNormalDiffuseSpecular5:
                case ChunkType.Vertex_VertexNormalDiffuseSpecular4:
                case ChunkType.End:
                    break;
                default:
                    throw new NotSupportedException("Unsupported chunk type " + type + ".");
            }
        }

        public VertexChunk(BufferedStreamReaderBE<MemoryStream> sr, int offset = 0)
        {
            Read(sr, offset);
        }
        
        private void Read(BufferedStreamReaderBE<MemoryStream> sr, int offset = 0)
        {
            bool SADXColorReverse = sr.streamChecks.ContainsKey("SADXColorReverse") ? sr.streamChecks["SADXColorReverse"] : false;
            Header1 = sr.ReadBE<uint>();
            Header2 = sr.ReadBE<uint>();

            float padding = 0;
            for (int i = 0; i < GetVertCount(); i++)
            {
                switch (Type)
                {
                    case ChunkType.Vertex_VertexSH:
                        Vertices.Add(sr.ReadBEV3());
                        padding = sr.ReadBE<float>();
                        break;
                    case ChunkType.Vertex_VertexNormalSH:
                        Vertices.Add(sr.ReadBEV3());
                        padding = sr.ReadBE<float>();
                        Normals.Add(sr.ReadBEV3());
                        padding = sr.ReadBE<float>();
                        break;
                    case ChunkType.Vertex_Vertex:
                        Vertices.Add(sr.ReadBEV3());
                        break;
                    case ChunkType.Vertex_VertexDiffuse8:
                        Vertices.Add(sr.ReadBEV3());
                        Diffuse.Add(NinjaModelCommon.ReadColorARGB8888_32(sr._BEReadActive, SADXColorReverse, sr.Read4Bytes()));
                        break;
                    case ChunkType.Vertex_VertexUserFlags:
                        Vertices.Add(sr.ReadBEV3());
                        UserFlags.Add(sr.ReadBE<uint>());
                        break;
                    case ChunkType.Vertex_VertexNinjaFlags:
                        Vertices.Add(sr.ReadBEV3());
                        NinjaFlags.Add(sr.ReadBE<uint>());
                        break;
                    case ChunkType.Vertex_VertexDiffuseSpecular5:
                        Vertices.Add(sr.ReadBEV3());
                        //Color is stored as a 32 bit integer so read order is dependent on endianness
                        uint tmpcolor5 = sr.ReadBE<uint>();
                        Diffuse.Add(NinjaModelCommon.ReadColorRGB565(sr._BEReadActive, (ushort)(tmpcolor5 & 0xFFFF)));
                        Specular.Add(NinjaModelCommon.ReadColorRGB565(sr._BEReadActive, (ushort)(tmpcolor5 >> 16)));
                        break;
                    case ChunkType.Vertex_VertexDiffuseSpecular4:
                        Vertices.Add(sr.ReadBEV3());
                        //Color is stored as a 32 bit integer so read order is dependent on endianness
                        uint tmpcolor4 = sr.ReadBE<uint>();
                        Diffuse.Add(NinjaModelCommon.ReadColorARGB4444(sr._BEReadActive, (ushort)(tmpcolor4 & 0xFFFF)));
                        Specular.Add(NinjaModelCommon.ReadColorRGB565(sr._BEReadActive, (ushort)(tmpcolor4 >> 16)));
                        break;
                    case ChunkType.Vertex_VertexNormal:
                        Vertices.Add(sr.ReadBEV3());
                        Normals.Add(sr.ReadBEV3());
                        break;
                    case ChunkType.Vertex_VertexNormalDiffuse8:
                        Vertices.Add(sr.ReadBEV3());
                        Normals.Add(sr.ReadBEV3());
                        Diffuse.Add(NinjaModelCommon.ReadColorARGB8888_32(sr._BEReadActive, SADXColorReverse, sr.Read4Bytes()));
                        break;
                    case ChunkType.Vertex_VertexNormalUserFlags:
                        Vertices.Add(sr.ReadBEV3());
                        Normals.Add(sr.ReadBEV3());
                        UserFlags.Add(sr.ReadBE<uint>());
                        break;
                    case ChunkType.Vertex_VertexNormalNinjaFlags:
                        Vertices.Add(sr.ReadBEV3());
                        Normals.Add(sr.ReadBEV3());
                        NinjaFlags.Add(sr.ReadBE<uint>());
                        break;
                    case ChunkType.Vertex_VertexNormalDiffuseSpecular5:
                        Vertices.Add(sr.ReadBEV3());
                        Normals.Add(sr.ReadBEV3());
                        //Color is stored as a 32 bit integer so read order is dependent on endianness
                        uint tmpcolorn5 = sr.ReadBE<uint>();
                        Diffuse.Add(NinjaModelCommon.ReadColorRGB565(sr._BEReadActive, (ushort)(tmpcolorn5 & 0xFFFF)));
                        Specular.Add(NinjaModelCommon.ReadColorRGB565(sr._BEReadActive, (ushort)(tmpcolorn5 >> 16)));
                        break;
                    case ChunkType.Vertex_VertexNormalDiffuseSpecular4:
                        Vertices.Add(sr.ReadBEV3());
                        Normals.Add(sr.ReadBEV3());
                        //Color is stored as a 32 bit integer so read order is dependent on endianness
                        uint tmpcolorn4 = sr.ReadBE<uint>();
                        Diffuse.Add(NinjaModelCommon.ReadColorARGB4444(sr._BEReadActive, (ushort)(tmpcolorn4 & 0xFFFF)));
                        Specular.Add(NinjaModelCommon.ReadColorRGB565(sr._BEReadActive, (ushort)(tmpcolorn4 >> 16)));
                        break;
                    default:
                        throw new NotSupportedException("Unsupported chunk type " + Type + " at " + sr.Position.ToString("X8") + ".");
                }
            }
        }

        public byte[] GetBytes(bool bigEndian, bool SADXColorReverse)
        {
            VertexChunk next = null;
            int vertlimit;
            int vertcount = Vertices.Count;
            switch (Type)
            {
                case ChunkType.Vertex_VertexSH:
                    vertlimit = 65535 / 4;
                    if (Vertices.Count > vertlimit)
                    {
                        next = new VertexChunk(Type) { Vertices = Vertices.Skip(vertlimit).ToList() };
                        vertcount = vertlimit;
                    }
                    break;
                case ChunkType.Vertex_VertexNormalSH:
                    vertlimit = 65535 / 8;
                    if (Vertices.Count > vertlimit)
                    {
                        next = new VertexChunk(Type) { Vertices = Vertices.Skip(vertlimit).ToList(), Normals = Normals.Skip(vertlimit).ToList() };
                        vertcount = vertlimit;
                    }
                    break;
                case ChunkType.Vertex_Vertex:
                    vertlimit = 65535 / 3;
                    if (Vertices.Count > vertlimit)
                    {
                        next = new VertexChunk(Type) { Vertices = Vertices.Skip(vertlimit).ToList() };
                        vertcount = vertlimit;
                    }
                    break;
                case ChunkType.Vertex_VertexDiffuse8:
                    vertlimit = 65535 / 4;
                    if (Vertices.Count > vertlimit)
                    {
                        next = new VertexChunk(Type) { Vertices = Vertices.Skip(vertlimit).ToList(), Diffuse = Diffuse.Skip(vertlimit).ToList() };
                        vertcount = vertlimit;
                    }
                    break;
                case ChunkType.Vertex_VertexUserFlags:
                    vertlimit = 65535 / 4;
                    if (Vertices.Count > vertlimit)
                    {
                        next = new VertexChunk(Type) { Vertices = Vertices.Skip(vertlimit).ToList(), UserFlags = UserFlags.Skip(vertlimit).ToList() };
                        vertcount = vertlimit;
                    }
                    break;
                case ChunkType.Vertex_VertexNinjaFlags:
                    vertlimit = 65535 / 4;
                    if (Vertices.Count > vertlimit)
                    {
                        next = new VertexChunk(Type) { Vertices = Vertices.Skip(vertlimit).ToList(), NinjaFlags = NinjaFlags.Skip(vertlimit).ToList() };
                        vertcount = vertlimit;
                    }
                    break;
                case ChunkType.Vertex_VertexDiffuseSpecular5:
                case ChunkType.Vertex_VertexDiffuseSpecular4:
                    vertlimit = 65535 / 4;
                    if (Vertices.Count > vertlimit)
                    {
                        next = new VertexChunk(Type)
                        {
                            Vertices = Vertices.Skip(vertlimit).ToList(),
                            Diffuse = Diffuse.Skip(vertlimit).ToList(),
                            Specular = Specular.Skip(vertlimit).ToList()
                        };
                        vertcount = vertlimit;
                    }
                    break;
                case ChunkType.Vertex_VertexNormal:
                    vertlimit = 65535 / 6;
                    if (Vertices.Count > vertlimit)
                    {
                        next = new VertexChunk(Type) { Vertices = Vertices.Skip(vertlimit).ToList(), Normals = Normals.Skip(vertlimit).ToList() };
                        vertcount = vertlimit;
                    }
                    break;
                case ChunkType.Vertex_VertexNormalDiffuse8:
                    vertlimit = 65535 / 7;
                    if (Vertices.Count > vertlimit)
                    {
                        next = new VertexChunk(Type)
                        {
                            Vertices = Vertices.Skip(vertlimit).ToList(),
                            Normals = Normals.Skip(vertlimit).ToList(),
                            Diffuse = Diffuse.Skip(vertlimit).ToList()
                        };
                        vertcount = vertlimit;
                    }
                    break;
                case ChunkType.Vertex_VertexNormalUserFlags:
                    vertlimit = 65535 / 7;
                    if (Vertices.Count > vertlimit)
                    {
                        next = new VertexChunk(Type)
                        {
                            Vertices = Vertices.Skip(vertlimit).ToList(),
                            Normals = Normals.Skip(vertlimit).ToList(),
                            UserFlags = UserFlags.Skip(vertlimit).ToList()
                        };
                        vertcount = vertlimit;
                    }
                    break;
                case ChunkType.Vertex_VertexNormalNinjaFlags:
                    vertlimit = 65535 / 7;
                    if (Vertices.Count > vertlimit)
                    {
                        next = new VertexChunk(Type)
                        {
                            Vertices = Vertices.Skip(vertlimit).ToList(),
                            Normals = Normals.Skip(vertlimit).ToList(),
                            NinjaFlags = NinjaFlags.Skip(vertlimit).ToList()
                        };
                        vertcount = vertlimit;
                    }
                    break;
                case ChunkType.Vertex_VertexNormalDiffuseSpecular5:
                case ChunkType.Vertex_VertexNormalDiffuseSpecular4:
                    vertlimit = 65535 / 7;
                    if (Vertices.Count > vertlimit)
                    {
                        next = new VertexChunk(Type)
                        {
                            Vertices = Vertices.Skip(vertlimit).ToList(),
                            Normals = Normals.Skip(vertlimit).ToList(),
                            Diffuse = Diffuse.Skip(vertlimit).ToList(),
                            Specular = Specular.Skip(vertlimit).ToList()
                        };
                        vertcount = vertlimit;
                    }
                    break;
                case ChunkType.End:
                    break;
                default:
                    throw new NotSupportedException("Unsupported chunk type " + Type + ".");
            }
            SetVertCount(vertcount);
            switch (Type)
            {
                case ChunkType.Vertex_Vertex:
                    Size = (ushort)(vertcount * 3 + 1);
                    break;
                case ChunkType.Vertex_VertexSH:
                case ChunkType.Vertex_VertexDiffuse8:
                case ChunkType.Vertex_VertexUserFlags:
                case ChunkType.Vertex_VertexNinjaFlags:
                case ChunkType.Vertex_VertexDiffuseSpecular5:
                case ChunkType.Vertex_VertexDiffuseSpecular4:
                    Size = (ushort)(vertcount * 4 + 1);
                    break;
                case ChunkType.Vertex_VertexNormal:
                    Size = (ushort)(vertcount * 6 + 1);
                    break;
                case ChunkType.Vertex_VertexNormalDiffuse8:
                case ChunkType.Vertex_VertexNormalUserFlags:
                case ChunkType.Vertex_VertexNormalNinjaFlags:
                case ChunkType.Vertex_VertexNormalDiffuseSpecular5:
                case ChunkType.Vertex_VertexNormalDiffuseSpecular4:
                    Size = (ushort)(vertcount * 7 + 1);
                    break;
                case ChunkType.Vertex_VertexNormalSH:
                    Size = (ushort)(vertcount * 8 + 1);
                    break;
            }
            List<byte> result = new List<byte>((Size * 4) + 4);
            ByteListExtension.AddAsBigEndian = bigEndian;
            result.AddValue(Header1);
            result.AddValue(Header2);
            for (int i = 0; i < vertcount; i++)
            {
                switch (Type)
                {
                    case ChunkType.Vertex_VertexSH:
                        result.AddValue(Vertices[i]);
                        result.AddValue(1.0f);
                        break;
                    case ChunkType.Vertex_VertexNormalSH:
                        result.AddValue(Vertices[i]);
                        result.AddValue (1.0f);
                        result.AddValue(Normals[i]);
                        result.AddValue(1.0f);
                        break;
                    case ChunkType.Vertex_Vertex:
                        result.AddValue(Vertices[i]);
                        break;
                    case ChunkType.Vertex_VertexDiffuse8:
                        result.AddValue(Vertices[i]);
                        result.AddRange(NinjaModelCommon.GetBytesColorARGB8888_32(bigEndian, SADXColorReverse, Diffuse[i]));
                        break;
                    case ChunkType.Vertex_VertexUserFlags:
                        result.AddValue(Vertices[i]);
                        result.AddValue(UserFlags[i]);
                        break;
                    case ChunkType.Vertex_VertexNinjaFlags:
                        result.AddValue(Vertices[i]);
                        result.AddValue(NinjaFlags[i]);
                        break;
                    case ChunkType.Vertex_VertexDiffuseSpecular5:
                        result.AddValue(Vertices[i]);
                        result.AddValue(
                            NinjaModelCommon.GetUshortColorRGB565(Diffuse[i])
                            | (NinjaModelCommon.GetUshortColorRGB565(Specular[i]) << 16));
                        break;
                    case ChunkType.Vertex_VertexDiffuseSpecular4:
                        result.AddValue(Vertices[i]);
                        result.AddValue(
                            NinjaModelCommon.GetUshortColorARGB4444(Diffuse[i])
                            | (NinjaModelCommon.GetUshortColorRGB565(Specular[i]) << 16));
                        break;
                    case ChunkType.Vertex_VertexNormal:
                        result.AddValue(Vertices[i]);
                        result.AddValue(Normals[i]);
                        break;
                    case ChunkType.Vertex_VertexNormalDiffuse8:
                        result.AddValue(Vertices[i]);
                        result.AddValue(Normals[i]);
                        result.AddRange(NinjaModelCommon.GetBytesColorARGB8888_32(bigEndian, SADXColorReverse, Diffuse[i]));
                        break;
                    case ChunkType.Vertex_VertexNormalUserFlags:
                        result.AddValue(Vertices[i]);
                        result.AddValue(Normals[i]);
                        result.AddValue(UserFlags[i]);
                        break;
                    case ChunkType.Vertex_VertexNormalNinjaFlags:
                        result.AddValue(Vertices[i]);
                        result.AddValue(Normals[i]);
                        result.AddValue(NinjaFlags[i]);
                        break;
                    case ChunkType.Vertex_VertexNormalDiffuseSpecular5:
                        result.AddValue(Vertices[i]);
                        result.AddValue(Normals[i]);
                        result.AddValue(
                            NinjaModelCommon.GetUshortColorRGB565(Diffuse[i])
                            | (NinjaModelCommon.GetUshortColorRGB565(Specular[i]) << 16));
                        break;
                    case ChunkType.Vertex_VertexNormalDiffuseSpecular4:
                        result.AddValue(Vertices[i]);
                        result.AddValue(Normals[i]);
                        result.AddValue(
                            NinjaModelCommon.GetUshortColorARGB4444(Diffuse[i])
                            | (NinjaModelCommon.GetUshortColorRGB565(Specular[i]) << 16));
                        break;
                }
            }
            if (next != null)
                result.AddRange(next.GetBytes(bigEndian, SADXColorReverse));
            return result.ToArray();
        }
        
    }
}
