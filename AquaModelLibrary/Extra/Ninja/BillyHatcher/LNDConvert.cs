using AquaModelLibrary.Extra.Ninja.BillyHatcher.LNDH;
using Reloaded.Memory.Streams;
using SoulsFormats.Formats.Other.MWC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.Extra.Ninja.BillyHatcher
{
    public class LNDConvert
    {
        public static NGSAquaObject ConvertLND(byte[] file, out AquaNode aqn)
        {
            using (Stream stream = (Stream)new MemoryStream(file))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                return LNDToAqua(new LND(streamReader), out aqn);
            }
        }

        public static NGSAquaObject LNDToAqua(LND lnd, out AquaNode aqn)
        {
            NGSAquaObject aqp = new NGSAquaObject();
            aqn = AquaNode.GenerateBasicAQN();
            //Material
            var mat = new AquaObject.GenericMaterial();
            mat.matName = $"Material_{0}";
            mat.texNames = new List<string>() { "tex0.dds" };
            aqp.tempMats.Add(mat);

            foreach(var mesh in lnd.meshInfo)
            {
                var mesh2 = mesh.lndMeshInfo2;

                AquaObject.GenericTriangles genMesh = new AquaObject.GenericTriangles();
                genMesh.triList = new List<Vector3>();
                genMesh.matIdList.Add(0);
                int f = 0;
                int v = 0;
                if(mesh2?.polyInfo0?.triIndicesList?.Count > 0)
                {
                    AddFromPolyData(mesh2, genMesh, mesh2.polyInfo0, ref f, ref v);
                }
                if (mesh2?.polyInfo1?.triIndicesList?.Count > 0)
                {
                    AddFromPolyData(mesh2, genMesh, mesh2.polyInfo1, ref f, ref v);
                }
                aqp.tempTris.Add(genMesh);
            }


            return aqp;
        }

        private static void AddFromPolyData(LNDMeshInfo2 mesh2, AquaObject.GenericTriangles genMesh, PolyInfo polyInfo, ref int f, ref int v)
        {
            var triIndicesList = polyInfo.triIndicesList;
            for (int s = 0; s < triIndicesList.Count; s++)
            {
                var strip = triIndicesList[s];
                var stripStart = polyInfo.triIndicesListStarts[s];
                if (stripStart[0][0] == 0x98)
                {
                    for (int i = 0; i < strip.Count - 2; i++)
                    {
                        AquaObject.VTXL faceVtxl = new AquaObject.VTXL();
                        genMesh.triList.Add(new Vector3(v, v + 1, v + 2));
                        faceVtxl.rawFaceId.Add(f);
                        faceVtxl.rawFaceId.Add(f);
                        faceVtxl.rawFaceId.Add(f++);

                        faceVtxl.rawVertId.Add(v++);
                        faceVtxl.rawVertId.Add(v++);
                        faceVtxl.rawVertId.Add(v++);

                        if ((i & 1) > 0)
                        {
                            AddVert(mesh2, faceVtxl, strip[i], polyInfo.vertIndexMapping);
                            AddVert(mesh2, faceVtxl, strip[i + 1], polyInfo.vertIndexMapping);
                            AddVert(mesh2, faceVtxl, strip[i + 2], polyInfo.vertIndexMapping);
                        }
                        else
                        {
                            AddVert(mesh2, faceVtxl, strip[i + 2], polyInfo.vertIndexMapping);
                            AddVert(mesh2, faceVtxl, strip[i + 1], polyInfo.vertIndexMapping);
                            AddVert(mesh2, faceVtxl, strip[i], polyInfo.vertIndexMapping);
                        }

                        genMesh.faceVerts.Add(faceVtxl);
                        genMesh.vertCount += 3;
                    }
                } else if (stripStart[0][0] == 0x90)
                {
                    for (int i = 0; i < strip.Count - 2; i +=3)
                    {
                        AquaObject.VTXL faceVtxl = new AquaObject.VTXL();
                        genMesh.triList.Add(new Vector3(v, v + 1, v + 2));
                        faceVtxl.rawFaceId.Add(f);
                        faceVtxl.rawFaceId.Add(f);
                        faceVtxl.rawFaceId.Add(f++);

                        faceVtxl.rawVertId.Add(v++);
                        faceVtxl.rawVertId.Add(v++);
                        faceVtxl.rawVertId.Add(v++);

                        AddVert(mesh2, faceVtxl, strip[i + 2], polyInfo.vertIndexMapping);
                        AddVert(mesh2, faceVtxl, strip[i + 1], polyInfo.vertIndexMapping);
                        AddVert(mesh2, faceVtxl, strip[i], polyInfo.vertIndexMapping);

                        genMesh.faceVerts.Add(faceVtxl);
                        genMesh.vertCount += 3;
                    }
                }
            }
        }

        public static void AddVert(LNDMeshInfo2 mesh, AquaObject.VTXL vtxl, List<int> faceIds, Dictionary<int, int> vertIndexMapping)
        {
            for(int i = 0; i < mesh.layouts.Count; i++)
            {
                var lyt = mesh.layouts[i];
                switch(lyt.vertType)
                {
                    case 0x1:
                        vtxl.vertPositions.Add(mesh.vertData.vertPositions[faceIds[vertIndexMapping[i]]] / 10);
                        break;
                    case 0x2:
                        //mesh.vertData.vert2Data.Add(new byte[] { sr.Read<byte>(), sr.Read<byte>(), sr.Read<byte>() });
                        break;
                    case 0x3:
                        //mesh.vertData.vertColorData.Add(sr.ReadBE<short>());
                        break;
                    case 0x5:
                        //mesh.vertData.vertUVData.Add(new short[] { sr.ReadBE<short>(), sr.ReadBE<short>() });
                        break;
                    default:
                        throw new System.Exception($"Unk Vert type: {lyt.vertType:X} Data type: {lyt.dataType:X}");
                }
            }
        }
    }
}
