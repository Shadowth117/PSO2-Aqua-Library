using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.MathHelpers;
using AquaModelLibrary.Helpers.Readers;
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
        TEX4 = 0x54455834,
        TEX5 = 0x54455835,
        TEX6 = 0x54455836,
        TEX7 = 0x54455837,
        TEX8 = 0x54455838,
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
        public List<Vector3> normals = new List<Vector3>();
        public List<Quaternion> normalQs = new List<Quaternion>();
        public List<byte[]> normalsTesting = new List<byte[]>();
        public List<string> normalsTesting2 = new List<string>();
        public List<byte[]> normalTemp = new List<byte[]>();
        public List<int[]> vertWeightIndices = new List<int[]>();
        public List<Vector4> vertWeights = new List<Vector4>();
        /// <summary>
        /// Increments varying amounts from some amount to 1.0f
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

            var vertCount = vertDefs[0].dataSize / 0xC; //First should alwasy be position
            for (int i = 0; i < vertDefinitionsCount; i++)
            {
                sr.Seek(vertexDataStart + vertDefs[i].dataStart, System.IO.SeekOrigin.Begin);
                ReadVertDefData(sr, vertCount, vertDefs[i].dataMagic, vertDefs[i].dataSize);
            }
            sr.Seek(vertexDataStart + vertDefs[^1].dataStart + vertDefs[^1].dataSize, System.IO.SeekOrigin.Begin);
        }

        public void ReadVertDefData(BufferedStreamReaderBE<MemoryStream> sr, int vertCount, VertexMagic dataMagic, long dataSize)
        {
            switch (dataMagic)
            {
                case VertexMagic.POS0:
                    for (int v = 0; v < vertCount; v++)
                    {
                        positionList.Add(sr.Read<Vector3>());
                    }
                    break;
                case VertexMagic.NRM0:
                case VertexMagic.TAN0:
                case VertexMagic.QUT0:
                    for (int v = 0; v < vertCount; v++)
                    {
                        var byteArr = sr.Read4Bytes();
                        normalTemp.Add(byteArr);
#if DEBUG
                        sr.Seek(-0x4, SeekOrigin.Current);
                        var uarr = sr.Peek<uint>();
                        var iarr = sr.Peek<int>();

                        int x = (int)(uarr & 0x3FF);          
                        int y = (int)((uarr >> 10) & 0x3FF);
                        int z = (int)((uarr >> 20) & 0x3FF); 
                        x = (x >= 512) ? x - 1024 : x;
                        y = (y >= 512) ? y - 1024 : y;
                        z = (z >= 512) ? z - 1024 : z;

                        // Normalize to [-1,1] range
                        Vector3 normal10s = new Vector3(x, y, z) / 511.0f;
                        Vector3 normal10s2 = Vector3.Normalize(normal10s);


                        //Debug.WriteLine($"Byte represntation {byteArr[0]:X2} {byteArr[1]:X2} {byteArr[2]:X2} {byteArr[3]:X2} - {((float)byteArr[0]) / 255} {((float)byteArr[1]) / 255} {((float)byteArr[2]) / 255} {((float)byteArr[3]) / 255} \nSByte representation {sbyteArr[0]:X2} {sbyteArr[1]:X2} {sbyteArr[2]:X2} {sbyteArr[3]:X2} - {((float)sbyteArr[0]) / 127} {((float)sbyteArr[1]) / 127} {((float)sbyteArr[2]) / 127} {((float)sbyteArr[3]) / 127} ");
                        Quaternion quat = new Quaternion( (float)(((double)sr.Read<sbyte>()) / 127), (float)(((double)sr.Read<sbyte>()) / 127), (float)(((double)sr.Read<sbyte>()) / 127), (float)(((double)sr.Read<sbyte>()) / 127));
                        Vector4 quat2 = new Vector4((float)(((double)byteArr[0]) / 255), (float)(((double)byteArr[1]) / 255), (float)(((double)byteArr[2]) / 255), (float)(((double)byteArr[3]) / 255));
                        var quat2Mult = (quat2 * 2);
                        var quat2Minus = quat2Mult + new Vector4(-1,-1,-1,-1);
                        var quat2Quat = quat2Minus.ToQuat();
                        var quat2QuatNormalized = Quaternion.Normalize(quat2Quat);
                        var originalQuat = quat;
                        var testQuat = new Quaternion(quat.Z, quat.Y, quat.X, quat.W);
                        var testQuat2 = new Quaternion(quat.Z, quat.Y, quat.X, quat.W);
                        testQuat2 = Quaternion.Normalize(testQuat2);
                        quat = quat2QuatNormalized;

                        //Method 1
                        var vq4Quat = quat.ToVec4() * 2 + new Vector4(-1,-1,-1,-1);
                        var t = new Vector3(1, 0, 0) + new Vector3(-2, 2, 2) * vq4Quat.Y * new Vector3(vq4Quat.Y, vq4Quat.X, vq4Quat.W) + new Vector3(-2, -2, 2) * vq4Quat.Z * new Vector3(vq4Quat.Z, vq4Quat.W, vq4Quat.X);
                        var b = new Vector3(0, 1, 0) + new Vector3(2, -2, 2) * vq4Quat.Z * new Vector3(vq4Quat.W, vq4Quat.Z, vq4Quat.Y) + new Vector3(2, -2, -2) * vq4Quat.X * new Vector3(vq4Quat.Y, vq4Quat.X, vq4Quat.W);
                        var n = new Vector3(0, 0, 1) + new Vector3(2, 2, -2) * vq4Quat.X * new Vector3(vq4Quat.Z, vq4Quat.W, vq4Quat.X) + new Vector3(-2, 2, -2) * vq4Quat.Y * new Vector3(vq4Quat.W, vq4Quat.Z, vq4Quat.Y);

                        //Method 2
                        var vq4Quat2 = quat2 * (float)(2 * Math.PI) - new Vector4((float)Math.PI, (float)Math.PI, (float)Math.PI, (float)Math.PI);
                        Vector4 sc0, sc1;
                        sc0.X = (float)Math.Sin(vq4Quat2.X);
                        sc0.Y = (float)Math.Cos(vq4Quat2.X);
                        sc0.Z = (float)Math.Sin(vq4Quat2.Y);
                        sc0.W = (float)Math.Cos(vq4Quat2.Y);
                        sc1.X = (float)Math.Sin(vq4Quat2.Z);
                        sc1.Y = (float)Math.Cos(vq4Quat2.Z);
                        sc1.Z = (float)Math.Sin(vq4Quat2.W);
                        sc1.W = (float)Math.Cos(vq4Quat2.W);
                        var tan = new Vector3(sc0.Y * Math.Abs(sc0.Z), sc0.X * Math.Abs(sc0.Z), sc0.W);
                        var bitan = new Vector3(sc1.Y * Math.Abs(sc1.Z), sc1.X * Math.Abs(sc1.Z), sc1.W);
                        var normal = Vector3.Cross(tan, bitan);
                        normal = vq4Quat2.W > 0 ? normal : -normal;
                        var testnrm = Vector3.Normalize(normal);

                        normals.Add(SphereDecode(uarr));
#endif
                    }
                    break;
                case VertexMagic.COL0:
                case VertexMagic.COL1:
                case VertexMagic.COL2:
                    List<byte[]> colors = new List<byte[]>();
                    for (int v = 0; v < vertCount; v++)
                    {
                        colors.Add(sr.ReadBytes(sr.Position, 4));
                        sr.Seek(4, System.IO.SeekOrigin.Current);
                    }
                    colorDict.Add(dataMagic, colors);
                    break;
                case VertexMagic.TEX0:
                case VertexMagic.TEX1:
                case VertexMagic.TEX2:
                case VertexMagic.TEX3:
                case VertexMagic.TEX4:
                case VertexMagic.TEX5:
                case VertexMagic.TEX6:
                case VertexMagic.TEX7:
                case VertexMagic.TEX8:
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
            List<byte> outBytes = new();
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

            for (int i = 0; i < vertDefs.Count; i++)
            {
                var vertDef = vertDefs[i];
                outBytes.AddValue((int)vertDef.dataMagic);
                switch (vertDef.dataMagic)
                {
                    case VertexMagic.POS0:
                        outBytes.AddValue(0x2);
                        break;
                    case VertexMagic.NRM0:
                    case VertexMagic.TAN0:
                    case VertexMagic.QUT0:
                        outBytes.AddValue(0x10);
                        break;
                    case VertexMagic.TEX0:
                    case VertexMagic.TEX1:
                    case VertexMagic.TEX2:
                    case VertexMagic.TEX3:
                    case VertexMagic.TEX4:
                    case VertexMagic.TEX5:
                    case VertexMagic.TEX6:
                    case VertexMagic.TEX7:
                    case VertexMagic.TEX8:
                        outBytes.AddValue(0x6);
                        break;
                    case VertexMagic.BONI:
                        outBytes.AddValue(largeBoneCount ? 0xB : 0x5);
                        break;
                    case VertexMagic.BONW:
                    case VertexMagic.COL0:
                    case VertexMagic.COL1:
                    case VertexMagic.COL2:
                        outBytes.AddValue(0x4);
                        break;
                    case VertexMagic.SAT_:
                        outBytes.AddValue(0x0);
                        break;
                    default:
                        throw new Exception("Unexpected vertex magic!");
                }
                outBytes.AddValue(0x2);
                outBytes.ReserveLong($"VertDefDataStart{i}");
                outBytes.ReserveLong($"VertDefDataSize{i}");
            }

            for (int i = 0; i < vertDefs.Count; i++)
            {
                var dataStart = outBytes.FillLong($"VertDefDataStart{i}", outBytes.Count - vertexDataStart);
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
                    case VertexMagic.QUT0:
                        foreach (var nrm in normalTemp)
                        {
                            outBytes.AddValue(nrm);
                        }
                        break;
                    case VertexMagic.TEX0:
                    case VertexMagic.TEX1:
                    case VertexMagic.TEX2:
                    case VertexMagic.TEX3:
                    case VertexMagic.TEX4:
                    case VertexMagic.TEX5:
                    case VertexMagic.TEX6:
                    case VertexMagic.TEX7:
                    case VertexMagic.TEX8:
                        var uvs = uvDict[vertDefs[i].dataMagic];
                        foreach(var uv in uvs)
                        {
                            outBytes.AddValue(uv);
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
                            outBytes.AddValue(color);
                        }
                        break;
                    case VertexMagic.SAT_:
                        foreach(var sat in satValues)
                        {
                            outBytes.AddValue(sat);
                        }
                        break;
                }
                outBytes.FillLong($"VertDefDataSize{i}", outBytes.Count - vertexDataStart - dataStart);
            }
            outBytes.FillInt("VertBufferSize", outBytes.Count - vertexDataStart);

            return outBytes.ToArray();
        }

        public Vector3 SphereDecode(uint value)
        {
            float x = ((value & 0xFFFF) - 32767.0f) / 32767.0f;
            float y = (((value >> 16) & 0xFFFF) - 32767.0f) / 32767.0f;
            
            float z = (float)Math.Pow(2, (2.0f * Math.Sqrt(x * x + y * y))) - 1.0f;
            var vec3 =  new Vector3(x, y, z);
            var vec3n = Vector3.Normalize(vec3);

            return vec3n;
        }
    }
}
