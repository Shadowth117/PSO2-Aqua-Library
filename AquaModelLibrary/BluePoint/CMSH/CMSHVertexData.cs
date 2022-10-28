using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using SystemHalf;

namespace AquaModelLibrary.BluePoint.CMSH
{
    public enum VertexMagic : int
    {
        POS0 = 0x504F5330,
        QUT0 = 0x51555430,
        COL0 = 0x434F4C30,
        COL1 = 0x434F4C31,
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
        public int flags;
        public int int_04;
        public int int_08;
        public int vertexBufferSize; //Size of VertexData section after this point

        public int vertDefinitionsCount;
        public int int_14;

        public List<CMSHVertexDataDefinition> vertDefs = new List<CMSHVertexDataDefinition>();

        //Data
        public List<Vector3> positionList = new List<Vector3>();
        public List<Quaternion> normals = new List<Quaternion>();
        public List<byte[]> normalTemp = new List<byte[]>();
        public List<byte[]> colors = new List<byte[]>();
        public List<byte[]> color2s = new List<byte[]>();
        public List<int[]> vertWeightIndices = new List<int[]>();
        public List<Vector4> vertWeights = new List<Vector4>();
        public Dictionary<VertexMagic, List<Vector2>> uvDict = new Dictionary<VertexMagic, List<Vector2>>(); //Access by magic, ex 0XET or 3XET (TEX0 and TEX3) as ints. UVs seem stored as half floats
        public Dictionary<VertexMagic, byte[]> unkDict = new Dictionary<VertexMagic, byte[]>();

        public CMSHVertexData()
        {

        }

        public CMSHVertexData(BufferedStreamReader sr)
        {
            flags = sr.Read<int>();
            int_04 = sr.Read<int>();
            int_08 = sr.Read<int>();
            vertexBufferSize = sr.Read<int>();
            var vertexDataStart = sr.Position();

            vertDefinitionsCount = sr.Read<int>();
            int_14 = sr.Read<int>();

            for(int i = 0; i < vertDefinitionsCount; i++)
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
                switch(vertDefs[i].dataMagic)
                {
                    case VertexMagic.POS0:
                        for(int v = 0; v < vertCount; v++)
                        {
                            positionList.Add(sr.Read<Vector3>());
                        }
                        break;
                    case VertexMagic.QUT0:
                        for (int v = 0; v < vertCount; v++)
                        {
                            var byteArr = sr.ReadBytes(sr.Position(), 4);
                            sr.Seek(4, System.IO.SeekOrigin.Current);
                            Quaternion quat = new Quaternion(ConvertBPSbyte(byteArr[0]), ConvertBPSbyte(byteArr[1]), ConvertBPSbyte(byteArr[2]), ConvertBPSbyte(byteArr[3]));
                            normalTemp.Add(byteArr);
                            //Debug.WriteLine($"Byte represntation {byteArr[0]:X2} {byteArr[1]:X2} {byteArr[2]:X2} {byteArr[3]:X2} - {((float)byteArr[0]) / 255} {((float)byteArr[1]) / 255} {((float)byteArr[2]) / 255} {((float)byteArr[3]) / 255} \nSByte representation {sbyteArr[0]:X2} {sbyteArr[1]:X2} {sbyteArr[2]:X2} {sbyteArr[3]:X2} - {((float)sbyteArr[0]) / 127} {((float)sbyteArr[1]) / 127} {((float)sbyteArr[2]) / 127} {((float)sbyteArr[3]) / 127} ");
                            //Quaternion quat = new Quaternion( (float)(((double)sr.Read<sbyte>()) / 127), (float)(((double)sr.Read<sbyte>()) / 127), (float)(((double)sr.Read<sbyte>()) / 127), (float)(((double)sr.Read<sbyte>()) / 127));
                            normals.Add(quat);
                        }
                        break;
                    case VertexMagic.COL0:
                        for (int v = 0; v < vertCount; v++)
                        {
                            colors.Add(sr.ReadBytes(sr.Position(), 4));
                            sr.Seek(4, System.IO.SeekOrigin.Current);
                        }
                        break;
                    case VertexMagic.COL1:
                        for (int v = 0; v < vertCount; v++)
                        {
                            color2s.Add(sr.ReadBytes(sr.Position(), 4));
                            sr.Seek(4, System.IO.SeekOrigin.Current);
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
                        var uvList = new List<Vector2>();
                        for (int v = 0; v < vertCount; v++)
                        {
                            uvList.Add(new Vector2(sr.Read<Half>(), sr.Read<Half>()));
                        }
                        uvDict.Add(vertDefs[i].dataMagic, uvList);
                        break;
                    case VertexMagic.BONI:
                        var smolCount = vertDefs[i].dataSize / 0x4;
                        if (vertCount != smolCount)
                        {
                            for (int v = 0; v < vertCount; v++)
                            {
                                vertWeightIndices.Add(new int[] { sr.Read<ushort>(), sr.Read<ushort>(), sr.Read<ushort>(), sr.Read<ushort>() });
                            }
                        } else
                        {
                            for (int v = 0; v < vertCount; v++)
                            {
                                var indices = sr.ReadBytes(sr.Position(), 4);
                                vertWeightIndices.Add(new int[] { indices[0], indices[1], indices[2], indices[3] });
                                sr.Seek(4, System.IO.SeekOrigin.Current);
                            }
                        }
                        break;
                    case VertexMagic.BONW:
                        for (int v = 0; v < vertCount; v++)
                        {
                            var weights = sr.ReadBytes(sr.Position(), 4);
                            vertWeights.Add(new Vector4((float)((double)weights[0] / 0xFF), (float)((double)weights[1] / 0xFF), (float)((double)weights[2] / 0xFF), (float)((double)weights[3] / 0xFF)));
                            sr.Seek(4, System.IO.SeekOrigin.Current);
                        }
                        break;
                    case VertexMagic.SAT_:
                        unkDict.Add(vertDefs[i].dataMagic, sr.ReadBytes(sr.Position(), vertDefs[i].dataSize));
                        sr.Seek(vertexDataStart + vertDefs[i].dataStart + vertDefs[i].dataSize, System.IO.SeekOrigin.Begin);
                        break;
                    default:
                        Debug.WriteLine($"Unknown data type {vertDefs[i].dataMagic.ToString("X")} {vertDefs[i].dataMagic} {UTF8Encoding.UTF8.GetString(BitConverter.GetBytes((int)vertDefs[i].dataMagic))}");
                        unkDict.Add(vertDefs[i].dataMagic, sr.ReadBytes(sr.Position(), vertDefs[i].dataSize));
                        sr.Seek(vertexDataStart + vertDefs[i].dataStart + vertDefs[i].dataSize, System.IO.SeekOrigin.Begin);
                        break;
                }
            }
        }

        public float ConvertBPSbyte(byte b)
        {
            if(b < 128)
            {
                return (float)((double)b / 127.0);
            } else
            {
                b -= 127;
                return (float)((double)b / 127.0);
            }
        }
    }
}
