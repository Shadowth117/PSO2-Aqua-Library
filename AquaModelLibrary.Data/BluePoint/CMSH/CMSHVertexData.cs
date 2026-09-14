using AquaModelLibrary.Data.DataTypes;
using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers.Writers;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using Half = AquaModelLibrary.Data.DataTypes.Half;

namespace AquaModelLibrary.Data.BluePoint.CMSH
{
    public enum VertexMagic : int
    {
        POS0 = 0x504F5330,
        NRM0 = 0x4E524D30,
        QUT0 = 0x51555430,
        COL0 = 0x434F4C30,
        COL1 = 0x434F4C31,
        COL2 = 0x434F4C32,
        TAN0 = 0x54414E30,
        TEX0 = 0x54455830,
        TEX1 = 0x54455831,
        TEX2 = 0x54455832,
        TEX3 = 0x54455833,
        BONI = 0x424F4E49,
        BONW = 0x424F4E57,
        SAT_ = 0x5341545F,
    }

    public class CMSHVertexData
    {
        public int unkData0Count;
        public int int_08;
        public int vertexBufferSize; //Size of VertexData section after this point

        public int vertDefinitionsCount;
        public int int_14;

        public List<CMSHVertexDataDefinition> vertDefs = new List<CMSHVertexDataDefinition>();

        //Data
        public List<Vector3> positionList = new List<Vector3>();
        public List<byte[]> normalTemp = new List<byte[]>();
        public List<uint> qut0List = new List<uint>();
        public List<int[]> vertWeightIndices = new List<int[]>();
        public List<Vector4> vertWeights = new List<Vector4>();
        /// <summary>
        /// Per face surface area table? Ratio of the surface area covered by the face at the index and those before until 1.0 on the last
        /// </summary>
        public List<float> satValues = new List<float>();
        public Dictionary<VertexMagic, List<byte[]>> colorDict = new Dictionary<VertexMagic, List<byte[]>>(); 
        public Dictionary<VertexMagic, List<Vector2>> uvDict = new Dictionary<VertexMagic, List<Vector2>>(); //Access by magic, ex 0XET or 3XET (TEX0 and TEX3) as ints. UVs seem stored as half floats
        public Dictionary<VertexMagic, byte[]> unkDict = new Dictionary<VertexMagic, byte[]>();

        //SOTC Extra
        public List<CMSHSOTCUnkData0> sotcUnk0List = new List<CMSHSOTCUnkData0>();
        public List<CMSHSOTCUnkData1> sotcUnk1List = new List<CMSHSOTCUnkData1>();

        public CMSHVertexData()
        {

        }

        public CMSHVertexData(BufferedStreamReaderBE<MemoryStream> sr, CMSHHeader header)
        {
            unkData0Count = sr.Read<int>();

            //Read special SOTC stuff, if it's there
            if(unkData0Count != 0)
            {
                for(int i = 0; i < unkData0Count; i++)
                {
                    sotcUnk0List.Add(sr.Read<CMSHSOTCUnkData0>());
                }
                var unk1Count = sr.Read<int>();
                for (int i = 0; i < unk1Count; i++)
                {
                    sotcUnk1List.Add(sr.Read<CMSHSOTCUnkData1>());
                }
            }

            int_08 = sr.Read<int>();
            vertexBufferSize = sr.Read<int>();
            var vertexDataStart = sr.Position;

            vertDefinitionsCount = sr.Read<int>();
            int_14 = sr.Read<int>();

            for (int i = 0; i < vertDefinitionsCount; i++)
            {
                var vertDef = new CMSHVertexDataDefinition();
                vertDef.dataMagic = sr.Read<VertexMagic>();
                vertDef.dataFormat = sr.Read<ushort>();
                vertDef.usht_06 = sr.Read<ushort>();
                vertDef.dataStart = sr.Read<int>();
                vertDef.int_0C = sr.Read<int>();
                vertDef.dataSize = sr.Read<int>();
                vertDef.int_14 = sr.Read<int>();
                vertDefs.Add(vertDef);
            }

            var vertCount = vertDefs[0].dataSize / 0xC; //First should always be position
            for (int i = 0; i < vertDefinitionsCount; i++)
            {
                sr.Seek(vertexDataStart + vertDefs[i].dataStart, System.IO.SeekOrigin.Begin);
                ReadVertDefData(sr, vertCount, vertDefs[i].dataMagic, vertDefs[i].dataSize, vertDefs[i].dataFormat);
            }
            sr.Seek(vertexDataStart + vertDefs[^1].dataStart + vertDefs[^1].dataSize, System.IO.SeekOrigin.Begin);
        }

        public void ReadVertDefData(BufferedStreamReaderBE<MemoryStream> sr, int vertCount, VertexMagic dataMagic, long dataSize, ushort dataFormat)
        {
            switch (dataMagic)
            {
                case VertexMagic.POS0:
                    for (int v = 0; v < vertCount; v++)
                    {
                        positionList.Add(sr.Read<Vector3>());
                    }
                    break;
                case VertexMagic.NRM0: //These are a similar idea, but NOT the same as QUT0
                case VertexMagic.TAN0:
                    for (int v = 0; v < vertCount; v++)
                    {
                        var byteArr = sr.Read4Bytes();
                        normalTemp.Add(byteArr);
                    }
                    break;
                case VertexMagic.QUT0:
                    for (int v = 0; v < vertCount; v++)
                    {
                        qut0List.Add(sr.Read<uint>());
                    }
                    break;
                case VertexMagic.COL0:
                case VertexMagic.COL1:
                case VertexMagic.COL2:
                    List<byte[]> colors = new List<byte[]>();
                    for (int v = 0; v < vertCount; v++)
                    {
                        switch (dataFormat)
                        {
                            case 4:
                                colors.Add(sr.ReadBytes(sr.Position, 4));
                                sr.Seek(4, System.IO.SeekOrigin.Current);
                                break;
                            case 7: //Hack until 16 bit vert colors are actually handled
                                colors.Add([(byte)(sr.Read<ushort>() / 0x100), (byte)(sr.Read<ushort>() / 0x100), (byte)(sr.Read<ushort>() / 0x100), (byte)(sr.Read<ushort>() / 0x100)]); 
                                sr.Seek(8, System.IO.SeekOrigin.Current);
                                break;
                        }

                    }
                    colorDict.Add(dataMagic, colors);
                    break;
                case VertexMagic.TEX0:
                case VertexMagic.TEX1:
                case VertexMagic.TEX2:
                case VertexMagic.TEX3:
                    var uvList = new List<Vector2>();
                    for (int v = 0; v < vertCount; v++)
                    {
                        uvList.Add(new Vector2(sr.Read<Half>(), sr.Read<Half>()));
                    }
                    uvDict.Add(dataMagic, uvList);
                    break;
                case VertexMagic.BONI:
                    var smolCount = dataSize / 0x4;
                    if (vertCount != smolCount)
                    {
                        for (int v = 0; v < vertCount; v++)
                        {
                            vertWeightIndices.Add(new int[] { sr.Read<ushort>(), sr.Read<ushort>(), sr.Read<ushort>(), sr.Read<ushort>() });
                        }
                    }
                    else
                    {
                        for (int v = 0; v < vertCount; v++)
                        {
                            var indices = sr.ReadBytes(sr.Position, 4);
                            vertWeightIndices.Add(new int[] { indices[0], indices[1], indices[2], indices[3] });
                            sr.Seek(4, System.IO.SeekOrigin.Current);
                        }
                    }
                    break;
                case VertexMagic.BONW:
                    for (int v = 0; v < vertCount; v++)
                    {
                        var weights = sr.ReadBytes(sr.Position, 4);
                        vertWeights.Add(new Vector4((float)((double)weights[0] / 0xFF), (float)((double)weights[1] / 0xFF), (float)((double)weights[2] / 0xFF), (float)((double)weights[3] / 0xFF)));
                        sr.Seek(4, System.IO.SeekOrigin.Current);
                    }
                    break;
                case VertexMagic.SAT_:
                    for(int f = 0; f < dataSize / 4; f++)
                    {
                        satValues.Add(sr.Read<float>());
                    }
                    break;
                default:
                    Debug.WriteLine($"Unknown data type {dataMagic.ToString("X")} {dataMagic} {UTF8Encoding.UTF8.GetString(BitConverter.GetBytes((int)dataMagic))}");
                    unkDict.Add(dataMagic, sr.ReadBytes(sr.Position, (int)dataSize));
                    break;
            }
        }

        public byte[] GetBytes(int boneCount)
        {
            bool largeBoneCount = boneCount > 256;
            var outBytes = new ByteListWriter();
            outBytes.AddValue(sotcUnk0List.Count);
            if (sotcUnk0List.Count > 0)
            {
                foreach (var unk0 in sotcUnk0List)
                {
                    outBytes.AddValue(unk0.minBounding);
                    outBytes.AddValue(unk0.maxBounding);
                    outBytes.AddValue(unk0.unkInt0);
                    outBytes.AddValue(unk0.unkInt1);
                }
                outBytes.AddValue(sotcUnk1List.Count);
                foreach (var unk1 in sotcUnk1List)
                {
                    outBytes.AddValue(unk1.id);
                    outBytes.AddValue(unk1.startIndex);
                    outBytes.AddValue(unk1.length);
                }
            }
            outBytes.AddValue(int_08);
            outBytes.ReserveInt("VertBufferSize");
            var vertexDataStart = outBytes.Count;
            outBytes.AddValue(vertDefs.Count);
            outBytes.AddValue(int_14);

            for (int i = 0; i < vertDefs.Count; i++)
            {
                var vertDef = vertDefs[i];
                outBytes.AddValue((int)vertDef.dataMagic);
                switch (vertDef.dataMagic)
                {
                    case VertexMagic.POS0:
                        outBytes.AddValue((ushort)0x2);
                        break;
                    case VertexMagic.NRM0:
                    case VertexMagic.TAN0:
                    case VertexMagic.QUT0:
                        outBytes.AddValue((ushort)0x10);
                        break;
                    case VertexMagic.TEX0:
                    case VertexMagic.TEX1:
                    case VertexMagic.TEX2:
                    case VertexMagic.TEX3:
                        outBytes.AddValue((ushort)0x6);
                        break;
                    case VertexMagic.BONI:
                        outBytes.AddValue((ushort)(largeBoneCount ? 0xB : 0x5));
                        break;
                    case VertexMagic.BONW:
                    case VertexMagic.COL0:
                    case VertexMagic.COL1:
                    case VertexMagic.COL2:
                        outBytes.AddValue(vertDef.dataFormat == 7 ? (ushort)7 : (ushort)0x4);
                        break;
                    case VertexMagic.SAT_:
                        outBytes.AddValue((ushort)0x0);
                        break;
                    default:
                        throw new Exception("Unexpected vertex magic!");
                }
                outBytes.AddValue((ushort)0x2);
                outBytes.ReserveLong($"VertDefDataStart{i}");
                outBytes.ReserveLong($"VertDefDataSize{i}");
            }

            for (int i = 0; i < vertDefs.Count; i++)
            {
                if(i != 0)
                {
                    //This isn't exact, but it's not clear what the pattern with this 'padding' is, as it doesn't align anything seemingly. Works regardless though
                    outBytes.AddRange([0xFF, 0xFF, 0xFF, 0xFF,  0xFF, 0xFF, 0xFF, 0xFF]); 
                }
                var dataStart = outBytes.FillLong($"VertDefDataStart{i}", outBytes.Count - vertexDataStart);
                var dataStartAbsolute = outBytes.Count;
                switch(vertDefs[i].dataMagic)
                {
                    case VertexMagic.POS0:
                        foreach(var pos in positionList)
                        {
                            outBytes.AddValue(pos);
                        }
                        break;
                    case VertexMagic.NRM0:
                    case VertexMagic.TAN0:
                        foreach (var nrm in normalTemp)
                        {
                            outBytes.AddValue(nrm);
                        }
                        break;
                    case VertexMagic.QUT0:
                        foreach (var qut0 in qut0List)
                        {
                            outBytes.AddValue(qut0);
                        }
                        break;
                    case VertexMagic.TEX0:
                    case VertexMagic.TEX1:
                    case VertexMagic.TEX2:
                    case VertexMagic.TEX3:
                        var uvs = uvDict[vertDefs[i].dataMagic];
                        foreach(var uv in uvs)
                        {
                            outBytes.AddValue(Half.GetBits((Half)uv.X));
                            outBytes.AddValue(Half.GetBits((Half)uv.Y));
                        }
                        break;
                    case VertexMagic.BONI:
                        foreach(var boni in vertWeightIndices)
                        {
                            if(largeBoneCount)
                            {
                                outBytes.AddValue((ushort)boni[0]);
                                outBytes.AddValue((ushort)boni[1]);
                                outBytes.AddValue((ushort)boni[2]);
                                outBytes.AddValue((ushort)boni[3]);
                            } else
                            {
                                outBytes.AddValue((byte)boni[0]);
                                outBytes.AddValue((byte)boni[1]);
                                outBytes.AddValue((byte)boni[2]);
                                outBytes.AddValue((byte)boni[3]);
                            }
                        }
                        break;
                    case VertexMagic.BONW:
                        foreach(var bonw in vertWeights)
                        {
                            outBytes.Add((byte)Math.Max(bonw.X * 255.0, 255));
                            outBytes.Add((byte)Math.Max(bonw.Y * 255.0, 255));
                            outBytes.Add((byte)Math.Max(bonw.Z * 255.0, 255));
                            outBytes.Add((byte)Math.Max(bonw.W * 255.0, 255));
                        }
                        break;
                    case VertexMagic.COL0:
                    case VertexMagic.COL1:
                    case VertexMagic.COL2:
                        var colors = colorDict[vertDefs[i].dataMagic];
                        foreach (var color in colors)
                        {
                            switch(vertDefs[i].dataFormat)
                            {
                                case 7: //Hack until 16 bit vert colors are actually handled
                                    outBytes.AddValue((ushort)(color[0] * 0x100));
                                    outBytes.AddValue((ushort)(color[1] * 0x100));
                                    outBytes.AddValue((ushort)(color[2] * 0x100));
                                    outBytes.AddValue((ushort)(color[3] * 0x100));
                                    break;
                                case 4:
                                default:
                                    outBytes.AddValue(color);
                                    break;
                            }
                        }
                        break;
                    case VertexMagic.SAT_:
                        foreach(var sat in satValues)
                        {
                            outBytes.AddValue(sat);
                        }
                        break;
                }
                outBytes.FillLong($"VertDefDataSize{i}", outBytes.Count - dataStartAbsolute);
            }
            outBytes.FillInt("VertBufferSize", outBytes.Count - vertexDataStart);

            return outBytes.ToArray();
        }

        public void CreateQUT0List(List<Vector3> normals, List<Vector3> tangents)
        {
            qut0List.Clear();
            for(int i = 0; i < normals.Count; i++)
            {
                qut0List.Add(PackQUT0(normals[i], tangents[i]));
            }
        }

        public void GetQut0Data(out List<Vector3> normals, out List<Vector3> tangents, out List<Vector3> bitangents)
        {
            normals = new();
            tangents = new();
            bitangents = new();

            foreach(var qut0 in qut0List)
            {
                UnpackQUT0(qut0, out var nrm, out var tan, out var bit);
                normals.Add(nrm);
                tangents.Add(tan);
                bitangents.Add(bit);
            }
        }


        private const double qut0Scale = 511.5;
        private const double qut0Scale2 = 512;
        private const double qut0Clamp = 1023;
        private static uint GetQut0Component(double value)
        {
            int component = (int)(qut0Scale2 + qut0Scale * value);
            return (uint)Math.Clamp(component, 0, qut0Clamp);
        }

        /// <summary>
        /// QUT0 is a packed quaternion of a TBN (tangent, bitangent, normal) matrix stored as a 10 10 10 2 int with the last 2 bits always 1.
        /// </summary>
        public static void UnpackQUT0(uint qut0, out Vector3 normal, out Vector3 tangent, out Vector3 bitangent)
        {
            //Get the xyz coords of the quaternion
            var quatPieces = new Vector3(
            (float)(((qut0 & 0x3FF) - qut0Scale) / qut0Scale),
            (float)((((qut0 >> 10) & 0x3FF) - qut0Scale) / qut0Scale),
            (float)((((qut0 >> 20) & 0x3FF) - qut0Scale) / qut0Scale));

            //Normalize if needed
            double length = quatPieces.Length();
            if(length > 1.0)
            {
                var divisor = Math.Sqrt(length);
                quatPieces = new Vector3((float)(quatPieces.X / divisor), (float)(quatPieces.Y / divisor), (float)(quatPieces.Z / divisor));
            }

            //Calc all quaternion values
            double finalScaleFactor = Math.Sqrt(2.0 - length);
            Quaternion quat = new Quaternion(
                (float)(quatPieces.X * finalScaleFactor),
                (float)(quatPieces.Y * finalScaleFactor),
                (float)(quatPieces.Z * finalScaleFactor),
                (float)(1 - length));

            //Create final rotation matrix and extract TBN values
            var rotationMatrix = Matrix4x4.CreateFromQuaternion(quat);
            tangent = new Vector3(rotationMatrix.M11, rotationMatrix.M12, rotationMatrix.M13);
            bitangent = new Vector3(rotationMatrix.M21, rotationMatrix.M22, rotationMatrix.M23);
            normal = new Vector3(rotationMatrix.M31, rotationMatrix.M32, rotationMatrix.M33);
        }

        /// <summary>
        /// QUT0 is a packed quaternion of a TBN (tangent, bitangent, normal) matrix stored as a 10 10 10 2 int with the last 2 bits always 1.
        /// It does need a tangent to calculate, but a bitangent can be calculated from the normal and tangent alone, so only those are necessary inputs
        /// </summary>
        public static uint PackQUT0(Vector3 normal, Vector3 tangent)
        {
            //Derive bitangent
            var bitangent = Vector3.Cross(normal, tangent);

            //Rotation matrix with TBN layout
            float m00 = tangent.X, m10 = tangent.Y, m20 = tangent.Z;
            float m01 = bitangent.X, m11 = bitangent.Y, m21 = bitangent.Z;
            float m02 = normal.X, m12 = normal.Y, m22 = normal.Z;

            //Convert to quaternion values with Shepperd's Method
            double trace = m00 + m11 + m22;
            double x;
            double y;
            double z;
            double w;

            if (trace > 0)
            {
                double S = Math.Sqrt(trace + 1f) * 2f;
                w = 0.25f * S;
                x = (m21 - m12) / S;
                y = (m02 - m20) / S;
                z = (m10 - m01) / S;
            }
            else if (m00 > m11 && m00 > m22)
            {
                double S = Math.Sqrt(1f + m00 - m11 - m22) * 2f;
                w = (m21 - m12) / S;
                x = 0.25f * S;
                y = (m01 + m10) / S;
                z = (m02 + m20) / S;
            }
            else if (m11 > m22)
            {
                double S = Math.Sqrt(1f + m11 - m00 - m22) * 2f;
                w = (m02 - m20) / S;
                x = (m01 + m10) / S;
                y = 0.25f * S;
                z = (m12 + m21) / S;
            }
            else
            {
                double S = Math.Sqrt(1f + m22 - m00 - m11) * 2f;
                w = (m10 - m01) / S;
                x = (m02 + m20) / S;
                y = (m12 + m21) / S;
                z = 0.25f * S;
            }

            //Adjust representation to avoid negative w and handle w of 0 specially to match game handling
            if (w < 0f)
            {
                x = -x; y = -y; z = -z; w = -w;
            }
            else if (w == 0f)
            {
                double absX = Math.Abs(x);
                double absY = Math.Abs(y); 
                double absZ = Math.Abs(z);
                double largest = absX >= absY ? (absX >= absZ ? x : z) : (absY >= absZ ? y : z);
                if (largest < 0f)
                {
                    x = -x; y = -y; z = -z;
                }
            }

            //Normalize and quantize
            double length = Math.Sqrt(x * x + y * y + z * z + w * w);
            x /= length; y /= length; z /= length; w /= length;

            double denom = Math.Sqrt(Math.Max(1f + w, 0f));

            uint qut0 = 3u << 30;
            qut0 |= GetQut0Component(x / denom);
            qut0 |= GetQut0Component(y / denom) << 10;
            qut0 |= GetQut0Component(z / denom) << 20;
            return qut0;
        }

        public float GetSizeFloat(List<Vector3Int.Vec3Int> faceList, out List<float> satValues)
        {
            double sizeFloat = 0;
            List<double> tempTotals = new(); 
            satValues = new List<float>();
            foreach(var face in faceList)
            {
                sizeFloat += Vector3.Cross(positionList[face.Y] - positionList[face.X], positionList[face.Z] - positionList[face.X]).Length() / 2;
                tempTotals.Add(sizeFloat);
            }

            //Generate Surface Area Table values
            foreach(var temp in tempTotals)
            {
                satValues.Add((float)(temp / sizeFloat));
            }

            return (float)sizeFloat;
        }

        /// <summary>
        /// QUT0 values require tangents to be generated. While normals will likely already exist for the model, they're a biproduct of generating tangents
        /// </summary>
        public static void GenerateNormalsAndTangents(List<Vector3> positions, List<Vector3Int.Vec3Int> faces, List<Vector2>? uvs, out List<Vector3> normals, out List<Vector3> tangents)
        {
            int vertCount = positions.Count;
            normals = new List<Vector3>(vertCount);
            tangents = new List<Vector3>(vertCount);
            var normalAcc = new Vector3[vertCount];
            var tangentAcc = new Vector3[vertCount];

            foreach (var face in faces)
            {
                var p0 = positions[face.X];
                var p1 = positions[face.Y];
                var p2 = positions[face.Z];
                var faceNormal = Vector3.Cross(p1 - p0, p2 - p0);
                normalAcc[face.X] += faceNormal;
                normalAcc[face.Y] += faceNormal;
                normalAcc[face.Z] += faceNormal;

                if (uvs != null)
                {
                    var e1 = p1 - p0;
                    var e2 = p2 - p0;
                    var uv0 = uvs[face.X];
                    float du1 = uvs[face.Y].X - uv0.X;
                    float dv1 = uvs[face.Y].Y - uv0.Y;
                    float du2 = uvs[face.Z].X - uv0.X;
                    float dv2 = uvs[face.Z].Y - uv0.Y;
                    float det = du1 * dv2 - du2 * dv1;
                    if (Math.Abs(det) > 1e-12f)
                    {
                        var t = (e1 * dv2 - e2 * dv1) / det;
                        float length = t.Length();
                        if (length > 1e-12f)
                        {
                            t /= length;
                            tangentAcc[face.X] += t;
                            tangentAcc[face.Y] += t;
                            tangentAcc[face.Z] += t;
                        }
                    }
                }
            }

            for (int i = 0; i < vertCount; i++)
            {
                var n = normalAcc[i];
                n = n.LengthSquared() < 1e-20f ? Vector3.UnitY : Vector3.Normalize(n);

                var t = tangentAcc[i] - n * Vector3.Dot(n, tangentAcc[i]);
                if (t.LengthSquared() < 1e-12f)
                {
                    var helper = MathF.Abs(n.X) < 0.9f ? Vector3.UnitX : Vector3.UnitY;
                    t = Vector3.Cross(helper, n);
                }
                normals.Add(n);
                tangents.Add(Vector3.Normalize(t));
            }
        }
    }
}
