﻿using AquaModelLibrary.Helpers.Readers;
using System.Diagnostics;

namespace AquaModelLibrary.Data.Gamecube
{
    /// <summary>
    /// A single corner of a polygon, called loop
    /// </summary>
    [Serializable]
    public class Loop : IEquatable<Loop>
    {
        /// <summary>
        /// The index of the position value
        /// </summary>
        public ushort PositionIndex;

        /// <summary>
        /// The index of the normal value
        /// </summary>
        public ushort NormalIndex;

        /// <summary>
        /// The index of the color value
        /// </summary>
        public ushort Color0Index;

        /// <summary>
        /// The index of the texture coordinate value
        /// </summary>
        public ushort UV0Index;

        /// <summary>
        /// The index of the second texture coordinate value
        /// </summary>
        public ushort UV1Index;

        public bool Equals(Loop other)
        {
            return PositionIndex == other.PositionIndex && NormalIndex == other.NormalIndex && Color0Index == other.Color0Index && UV0Index == other.UV0Index && UV1Index == other.UV1Index;
        }
    }

    /// <summary>
    /// A collection of polygons
    /// </summary>
    [Serializable]
    public class GCPrimitive
    {
        /// <summary>
        /// The way in which triangles are being stored
        /// </summary>
        public GCPrimitiveType primitiveType;

        /// <summary>
        /// The stored polygons
        /// </summary>
        public List<Loop> loops { get; set; }

        /// <summary>
        /// Create a new empty Primitive
        /// </summary>
        /// <param name="type">The type of primitive</param>
        public GCPrimitive(GCPrimitiveType type)
        {
            primitiveType = type;
            loops = new List<Loop>();
        }

        /// <summary>
        /// Read a primitive object from a file
        /// </summary>
        /// <param name="file">The files contents as a byte array</param>
        /// <param name="address">The starting address of the primitive</param>
        /// <param name="indexFlags">How the indices of the loops are structured</param>
        public GCPrimitive(BufferedStreamReaderBE<MemoryStream> sr, GCIndexAttributeFlags indexFlags)
        {
            primitiveType = sr.ReadBE<GCPrimitiveType>();

            bool wasBigEndian = sr._BEReadActive;
            sr._BEReadActive = true;

            ushort vtxCount = sr.ReadBE<ushort>();

            // checking the flags
            bool hasFlag(GCIndexAttributeFlags flag)
            {
                return indexFlags.HasFlag(flag);
            }

            // position always exists
            bool has_color = hasFlag(GCIndexAttributeFlags.HasColor);
            bool has_normal = hasFlag(GCIndexAttributeFlags.HasNormal);
            bool has_uv = hasFlag(GCIndexAttributeFlags.HasUV);
            bool has_uv2 = hasFlag(GCIndexAttributeFlags.HasUV2);

            //whether any of the indices use 16 bits instead of 8
            bool pos16bit = hasFlag(GCIndexAttributeFlags.Position16BitIndex);
            bool col16bit = hasFlag(GCIndexAttributeFlags.Color16BitIndex);
            bool nrm16bit = hasFlag(GCIndexAttributeFlags.Normal16BitIndex);
            bool uv16bit = hasFlag(GCIndexAttributeFlags.UV16BitIndex);
            bool uv216bit = hasFlag(GCIndexAttributeFlags.UV2_16BitIndex);

            loops = new List<Loop>();

            for (ushort i = 0; i < vtxCount; i++)
            {
                Loop l = new Loop();

                // reading position, which should always exist
                if (pos16bit)
                {
                    l.PositionIndex = sr.ReadBE<ushort>();
                }
                else
                {
                    l.PositionIndex = sr.ReadBE<byte>();
                }

                // reading normals
                if (has_normal)
                {
                    if (nrm16bit)
                    {
                        l.NormalIndex = sr.ReadBE<ushort>();
                    }
                    else
                    {
                        l.NormalIndex = sr.ReadBE<byte>();
                    }
                }

                // reading colors
                if (has_color)
                {
                    if (col16bit)
                    {
                        l.Color0Index = sr.ReadBE<ushort>();
                    }
                    else
                    {
                        l.Color0Index = sr.ReadBE<byte>();
                    }
                }

                // reading uvs
                if (has_uv)
                {
                    if (uv16bit)
                    {
                        l.UV0Index = sr.ReadBE<ushort>();
                    }
                    else
                    {
                        l.UV0Index = sr.ReadBE<byte>();
                    }
                }

                // reading uv2s
                if (has_uv2)
                {
                    if (uv16bit)
                    {
                        l.UV1Index = sr.ReadBE<ushort>();
                    }
                    else
                    {
                        l.UV1Index = sr.ReadBE<byte>();
                    }
                }

                loops.Add(l);
            }

            sr._BEReadActive = wasBigEndian;
        }

        /// <summary>
        /// Write the contents
        /// </summary>
        /// <param name="writer">The output stream</param>
        /// <param name="indexFlags">How the indices of the loops are structured</param>
        public byte[] GetBytes(GCIndexAttributeFlags indexFlags)
        {
            List<byte> result = new List<byte>
            {
                (byte)primitiveType
            };

            byte[] big_endian_count = BitConverter.GetBytes((ushort)loops.Count);
            // writing count as big endian
            result.Add(big_endian_count[1]);
            result.Add(big_endian_count[0]);

            // checking the flags
            bool hasFlag(GCIndexAttributeFlags flag)
            {
                return indexFlags.HasFlag(flag);
            }

            // position always exists
            bool has_color = hasFlag(GCIndexAttributeFlags.HasColor);
            bool has_normal = hasFlag(GCIndexAttributeFlags.HasNormal);
            bool has_uv = hasFlag(GCIndexAttributeFlags.HasUV);
            bool has_uv2 = hasFlag(GCIndexAttributeFlags.HasUV2);

            bool is_position_16bit = hasFlag(GCIndexAttributeFlags.Position16BitIndex);
            bool is_color_16bit = hasFlag(GCIndexAttributeFlags.Color16BitIndex);
            bool is_normal_16bit = hasFlag(GCIndexAttributeFlags.Normal16BitIndex);
            bool is_uv_16bit = hasFlag(GCIndexAttributeFlags.UV16BitIndex);
            bool is_uv2_16bit = hasFlag(GCIndexAttributeFlags.UV2_16BitIndex);

            foreach (Loop v in loops)
            {
                // Position should always exist
                if (is_position_16bit)
                {
                    byte[] big_endian_pos = BitConverter.GetBytes(v.PositionIndex);
                    // writing count as big endian
                    result.Add(big_endian_pos[1]);
                    result.Add(big_endian_pos[0]);
                }
                else
                {
                    result.Add((byte)v.PositionIndex);
                }

                if (has_normal)
                {
                    if (is_normal_16bit)
                    {
                        byte[] big_endian_nrm = BitConverter.GetBytes(v.NormalIndex);
                        // writing count as big endian
                        result.Add(big_endian_nrm[1]);
                        result.Add(big_endian_nrm[0]);
                    }
                    else
                    {
                        result.Add((byte)v.NormalIndex);
                    }
                }

                if (has_color)
                {
                    if (is_color_16bit)
                    {
                        byte[] big_endian_col = BitConverter.GetBytes(v.Color0Index);
                        // writing count as big endian
                        result.Add(big_endian_col[1]);
                        result.Add(big_endian_col[0]);
                    }
                    else
                    {
                        result.Add((byte)v.Color0Index);
                    }
                }

                if (has_uv)
                {
                    if (is_uv_16bit)
                    {
                        byte[] big_endian_uv = BitConverter.GetBytes(v.UV0Index);
                        // writing count as big endian
                        result.Add(big_endian_uv[1]);
                        result.Add(big_endian_uv[0]);
                    }
                    else
                    {
                        result.Add((byte)v.UV0Index);
                    }
                }

                if (has_uv2)
                {
                    if (is_uv2_16bit)
                    {
                        byte[] big_endian_uv = BitConverter.GetBytes(v.UV1Index);
                        // writing count as big endian
                        result.Add(big_endian_uv[1]);
                        result.Add(big_endian_uv[0]);
                    }
                    else
                    {
                        result.Add((byte)v.UV1Index);
                    }
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// Convert the primitive into a triangle list
        /// </summary>
        /// <returns></returns>
        public List<Loop> ToTriangles()
        {
            List<Loop> sorted_vertices = new List<Loop>();
            int degTriangles = 0;

            switch (primitiveType)
            {
                case GCPrimitiveType.Quads:
                    for (int v = 0; v < loops.Count - 4; v += 4)
                    {
                        // Triangle is always, v, v+1, v+2
                        Loop[] newTri0 = new Loop[]
                        {
                            loops[v],
                            loops[v + 1],
                            loops[v + 2],
                        };
                        Loop[] newTri1 = new Loop[]
                        {
                            loops[v + 3],
                            loops[v + 2],
                            loops[v + 1],
                        };

                        // Check against degenerate triangles (a triangle which shares indexes) for both new triangles
                        if (newTri0[0] != newTri0[1] && newTri0[1] != newTri0[2] && newTri0[2] != newTri0[0])
                            sorted_vertices.AddRange(newTri0);
                        else degTriangles++;

                        if (newTri1[0] != newTri1[1] && newTri1[1] != newTri1[2] && newTri1[2] != newTri1[0])
                            sorted_vertices.AddRange(newTri1);
                        else degTriangles++;
                    }
                    break;
                case GCPrimitiveType.Triangles:
                    return loops;
                case GCPrimitiveType.TriangleStrip:
                    bool isEven = false;
                    for (int v = 2; v < loops.Count; v++)
                    {
                        Loop[] newTri = new Loop[]
                        {
                            loops[v - 2],
                            isEven ? loops[v] : loops[v - 1],
                            isEven ? loops[v - 1] : loops[v]
                        };
                        isEven = !isEven;

                        // Check against degenerate triangles (a triangle which shares indexes)
                        if (newTri[0] != newTri[1] && newTri[1] != newTri[2] && newTri[2] != newTri[0])
                            sorted_vertices.AddRange(newTri);
                        else degTriangles++;
                    }
                    break;
                case GCPrimitiveType.TriangleFan:
                    for (int v = 1; v < loops.Count - 1; v++)
                    {
                        // Triangle is always, v, v+1, and index[0]?
                        Loop[] newTri = new Loop[]
                        {
                            loops[v],
                            loops[v + 1],
                            loops[0],
                        };

                        // Check against degenerate triangles (a triangle which shares indexes)
                        if (newTri[0] != newTri[1] && newTri[1] != newTri[2] && newTri[2] != newTri[0])
                            sorted_vertices.AddRange(newTri);
                        else degTriangles++;
                    }
                    break;
                default:
                    Debug.WriteLine($"Attempted to triangulate primitive type {primitiveType}");
                    break;
            }

            if (degTriangles > 0)
            {
                Debug.WriteLine("Degenerate triangles skipped: " + degTriangles);
            }

            return sorted_vertices;
        }

        public override string ToString()
        {
            return $"{primitiveType}: {loops.Count}";
        }
    }
}
