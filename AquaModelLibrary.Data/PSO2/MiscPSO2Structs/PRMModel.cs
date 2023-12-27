using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.Intermediary;
using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Ice;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace AquaModelLibrary
{        
    //Version 1 is the most basic and was seen in alpha pso2. Version 3 was used in PSO2 Classic for most of its life. Version 4 was used some in PSO2 and in NGS.
    //Version 1 requires some extra work to convert to since it doesn't have a face array and so is not supported for converting back at this time.
    public unsafe class PRMModel
    {
        //Seems to somehow link the PRM entries together when the count isn't 0
        public PRMHeader header = new PRMHeader();
        public List<Vector3> faces = new List<Vector3>(); //These are stored as ushorts, but we'll read them in to Vector3s;
        public List<PRMVert> vertices = new List<PRMVert>();

        public PRMModel() { }

        public PRMModel(BufferedStreamReaderBE<MemoryStream> streamReader, out PRMModel prmModel)
        {
            int offset = 0x0; //No NIFL header
            prmModel = new PRMModel();
            int type = streamReader.Peek<int>();
            streamReader.Seek(0xC, SeekOrigin.Begin);
            int iceEnvelopeSize = streamReader.Peek<int>();
            string prmText = "prm\0";

            //Deal with deicer's extra header nonsense
            if (iceEnvelopeSize > 0x10)
            {
                IceMethods.SkipIceEnvelope(streamReader, Encoding.UTF8.GetString(BitConverter.GetBytes(type)), ref prmText, ref offset);
            }
            streamReader.Seek(0x0 + offset, SeekOrigin.Begin);

            prmModel.header = streamReader.Read<PRMModel.PRMHeader>();

            int faceCount;
            switch (prmModel.header.entryVersion)
            {
                case 1:
                    for (int i = 0; i < prmModel.header.entryCount; i++)
                    {
                        prmModel.vertices.Add(new PRMModel.PRMVert(streamReader.Read<PRMModel.PRMType01Vert>()));
                    }

                    if (prmModel.header.groupIndexCount > 0)
                    {
                        faceCount = prmModel.header.groupIndexCount / 3;

                        for (int i = 0; i < faceCount; i++)
                        {

                            prmModel.faces.Add(new Vector3(streamReader.Read<ushort>(), streamReader.Read<ushort>(), streamReader.Read<ushort>()));
                        }
                    }
                    else
                    {
                        faceCount = PRMGenerateFacesFromVerts();
                    }
                    break;
                case 2:
                    Debug.WriteLine("Unimplemented PRM version! Please report if found!");
                    return;
                case 3:
                    for (int i = 0; i < prmModel.header.entryCount; i++)
                    {
                        prmModel.vertices.Add(new PRMModel.PRMVert(streamReader.Read<PRMModel.PRMType03Vert>()));
                    }

                    if (prmModel.header.groupIndexCount > 0)
                    {
                        faceCount = prmModel.header.groupIndexCount / 3;
                        for (int i = 0; i < faceCount; i++)
                        {
                            prmModel.faces.Add(new Vector3(streamReader.Read<ushort>(), streamReader.Read<ushort>(), streamReader.Read<ushort>()));
                        }
                    }
                    else
                    {
                        faceCount = PRMGenerateFacesFromVerts();
                    }
                    break;
                case 4:
                    for (int i = 0; i < prmModel.header.entryCount; i++)
                    {
                        prmModel.vertices.Add(new PRMModel.PRMVert(streamReader.Read<PRMModel.PRMType04Vert>()));
                    }

                    if (prmModel.header.groupIndexCount > 0)
                    {
                        faceCount = prmModel.header.groupIndexCount / 3;
                        for (int i = 0; i < faceCount; i++)
                        {
                            prmModel.faces.Add(new Vector3(streamReader.Read<ushort>(), streamReader.Read<ushort>(), streamReader.Read<ushort>()));
                        }
                    }
                    else
                    {
                        faceCount = PRMGenerateFacesFromVerts();
                    }
                    break;
                default:
                    Debug.WriteLine("Unknown PRM version! Please report!");
                    break;
            }
        }

        public byte[] GetBytes(int version)
        {
            List<byte> finalOutBytes = new List<byte>();
            finalOutBytes.AddRange(Encoding.UTF8.GetBytes("prm\0"));
            finalOutBytes.AddRange(BitConverter.GetBytes(vertices.Count));
            switch (version)
            {
                case 1:
                    Debug.WriteLine("Version 1 unsupported at this time!");
                    throw new NotImplementedException();
                case 2:
                    Debug.WriteLine("Version 2 unsupported at this time!");
                    throw new NotImplementedException();
                case 3:
                case 4:
                    finalOutBytes.AddRange(BitConverter.GetBytes(faces.Count * 3));
                    break;
                default:
                    Debug.WriteLine($"Version {version} unsupported at this time!");
                    throw new NotImplementedException();
            }
            finalOutBytes.AddRange(BitConverter.GetBytes(version));

            for (int i = 0; i < vertices.Count; i++)
            {
                switch (version)
                {
                    case 3:
                        var vert3 = vertices[i].GetType03Vert();
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert3.pos.X));
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert3.pos.Y));
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert3.pos.Z));
                        finalOutBytes.Add(vert3.color[0]);
                        finalOutBytes.Add(vert3.color[1]);
                        finalOutBytes.Add(vert3.color[2]);
                        finalOutBytes.Add(vert3.color[3]);
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert3.uv1.X));
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert3.uv1.Y));
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert3.uv2.X));
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert3.uv2.Y));
                        break;
                    case 4:
                        var vert4 = vertices[i].GetType04Vert();
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert4.pos.X));
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert4.pos.Y));
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert4.pos.Z));
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert4.normal.X));
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert4.normal.Y));
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert4.normal.Z));
                        finalOutBytes.Add(vert4.color[0]);
                        finalOutBytes.Add(vert4.color[1]);
                        finalOutBytes.Add(vert4.color[2]);
                        finalOutBytes.Add(vert4.color[3]);
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert4.uv1.X));
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert4.uv1.Y));
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert4.uv2.X));
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert4.uv2.Y));
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            for (int i = 0; i < faces.Count; i++)
            {
                finalOutBytes.AddRange(BitConverter.GetBytes((ushort)faces[i].X));
                finalOutBytes.AddRange(BitConverter.GetBytes((ushort)faces[i].Y));
                finalOutBytes.AddRange(BitConverter.GetBytes((ushort)faces[i].Z));
            }

            finalOutBytes.AlignWriter(0x10); //Should be padded at the end

            return finalOutBytes.ToArray();
        }

        public int PRMGenerateFacesFromVerts()
        {
            int faceCount = header.entryCount;
            for (int i = 0; i < faceCount; i += 3)
            {
                faces.Add(new Vector3(i, i + 1, i + 2));
            }

            return faceCount;
        }

        public struct PRMHeader
        {
            public int magic;
            public int entryCount;
            public int groupIndexCount;
            public int entryVersion;
        }

        public struct PRMType01Vert
        {
            public Vector3 pos;
            public float oftenOne;
            public fixed byte color[4];
            public Vector2 uv1;
            public Vector2 uv2;
            public Vector3 unkVector; //Padding? Never observed non zero.
        }

        public struct PRMType02Vert
        {

        }

        public struct PRMType03Vert
        {
            public Vector3 pos;
            public fixed byte color[4];
            public Vector2 uv1;
            public Vector2 uv2;
        }

        public struct PRMType04Vert
        {
            public Vector3 pos;
            public Vector3 normal;
            public fixed byte color[4];
            public Vector2 uv1;
            public Vector2 uv2;
        }

        public class PRMVert
        {
            public Vector3 pos;
            public Vector3 normal;
            public byte[] color;
            public Vector2 uv1;
            public Vector2 uv2;

            public PRMVert()
            {
            }

            public PRMVert(PRMType01Vert vert)
            {
                pos = vert.pos;
                color = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    color[i] = vert.color[i];
                }
                uv1 = vert.uv1;
                uv2 = vert.uv2;
            }

            public PRMVert(PRMType03Vert vert)
            {
                pos = vert.pos;
                color = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    color[i] = vert.color[i];
                }
                uv1 = vert.uv1;
                uv2 = vert.uv2;
            }

            public PRMVert(PRMType04Vert vert)
            {
                pos = vert.pos;
                normal = vert.normal;
                color = new byte[4];
                for (int i = 0; i < 4; i++)
                {
                    color[i] = vert.color[i];
                }
                uv1 = vert.uv1;
                uv2 = vert.uv2;
            }

            public PRMType01Vert GetType01Vert()
            {
                PRMType01Vert vert = new PRMType01Vert();
                vert.pos = pos;
                if (color != null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        vert.color[i] = color[i];
                    }
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        vert.color[i] = 0xFF;
                    }
                }
                vert.oftenOne = 0x1;
                vert.uv1 = uv1;
                vert.uv2 = uv2;

                return vert;
            }

            public PRMType03Vert GetType03Vert()
            {
                PRMType03Vert vert = new PRMType03Vert();
                vert.pos = pos;
                if (color != null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        vert.color[i] = color[i];
                    }
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        vert.color[i] = 0xFF;
                    }
                }
                vert.uv1 = uv1;
                vert.uv2 = uv2;

                return vert;
            }

            public PRMType04Vert GetType04Vert()
            {
                PRMType04Vert vert = new PRMType04Vert();
                vert.pos = pos;
                vert.normal = normal;
                if (color != null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        vert.color[i] = color[i];
                    }
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        vert.color[i] = 0xFF;
                    }
                }
                vert.uv1 = uv1;
                vert.uv2 = uv2;

                return vert;
            }
        }
        public void ConvertToAquaObject(out AquaObject aqo, out AquaNode aqn)
        {
            aqo = new AquaObject();
            aqo.objc.type = 0xC33;

            VTXL vtxl = new VTXL();
            for (int v = 0; v < vertices.Count; v++)
            {
                var vertex = vertices[v];
                vtxl.vertPositions.Add(vertex.pos);
                vtxl.vertNormals.Add(vertex.normal);
                vtxl.vertColors.Add(vertex.color);
                vtxl.uv1List.Add(vertex.uv1);
                vtxl.uv2List.Add(vertex.uv2);
            }
            aqo.vtxlList.Add(vtxl);

            var tris = new GenericTriangles(faces);
            tris.matIdList = new List<int>(new int[tris.triList.Count]);
            aqo.tempTris.Add(tris);
            aqo.tempMats.Add(new GenericMaterial() { matName = "PRMMat" });

            aqn = AquaNode.GenerateBasicAQN();

            aqo.ConvertToNGSPSO2Mesh(false, true, false, true, false, false, false);
        }
    }
}
