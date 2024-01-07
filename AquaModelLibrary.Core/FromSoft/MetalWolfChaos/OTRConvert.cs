using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.Intermediary;
using SoulsFormats;
using SoulsFormats.Formats.Other.MWC;
using System.Numerics;

namespace AquaModelLibrary.Core.FromSoft.MetalWolfChaos
{
    public class OTRConvert
    {
        public static AquaObject ConvertOTR(byte[] file, out AquaNode aqn)
        {
            return OTRToAqua(SoulsFile<OTR>.Read(file), out aqn);
        }

        public static AquaObject OTRToAqua(OTR otr, out AquaNode aqn)
        {
            AquaObject aqp = new AquaObject();
            aqn = AquaNode.GenerateBasicAQN();

            aqn.ndtr.boneCount = aqn.nodeList.Count;
            aqp.objc.bonePaletteOffset = 1;

            //Material
            var mat = new GenericMaterial();
            mat.matName = $"Material_{0}";
            mat.texNames = new List<string>() { "tex0.dds" };
            aqp.tempMats.Add(mat);

            GenericTriangles genMesh = new GenericTriangles();
            genMesh.triList = new List<Vector3>();
            genMesh.matIdList.Add(0);
            int f = 0;
            int v = 0;
            foreach (var face in otr.faces)
            {
                var faceNormal = face.normal;
                VTXL faceVtxl = new VTXL();
                genMesh.triList.Add(new Vector3(v, v + 1, v + 2));
                faceVtxl.rawFaceId.Add(f);
                faceVtxl.rawFaceId.Add(f);
                faceVtxl.rawFaceId.Add(f++);

                faceVtxl.rawVertId.Add(v++);
                faceVtxl.rawVertId.Add(v++);
                faceVtxl.rawVertId.Add(v++);

                var vert0 = otr.vertices[face.vertIndex0];
                AddVert(faceVtxl, faceNormal, vert0);

                var vert1 = otr.vertices[face.vertIndex1];
                AddVert(faceVtxl, faceNormal, vert1);

                var vert2 = otr.vertices[face.vertIndex2];
                AddVert(faceVtxl, faceNormal, vert2);

                genMesh.faceVerts.Add(faceVtxl);
                genMesh.vertCount += 3;
            }
            aqp.tempTris.Add(genMesh);

            return aqp;
        }

        private static void AddVert(VTXL faceVtxl, Vector3 faceNormal, OTR.Vertex vert0)
        {
            faceVtxl.vertPositions.Add(vert0.Position / 2000);
            faceVtxl.vertColors.Add(new byte[] { vert0.Color.B, vert0.Color.G, vert0.Color.R, vert0.Color.A });
            faceVtxl.vertNormals.Add(faceNormal);
        }
    }
}
