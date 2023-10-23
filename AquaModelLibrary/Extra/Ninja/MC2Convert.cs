using Reloaded.Memory.Streams;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;

namespace AquaModelLibrary.Extra.Ninja
{
    public class MC2Convert
    {
        public static NGSAquaObject ConvertMC2(byte[] file, out AquaNode aqn)
        {
            using (Stream stream = (Stream)new MemoryStream(file))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                return MC2ToAqua(new MC2(streamReader), out aqn);
            }
        }

        public static NGSAquaObject MC2ToAqua(MC2 mc2, out AquaNode aqn)
        {
            NGSAquaObject aqp = new NGSAquaObject();
            aqn = AquaNode.GenerateBasicAQN();

            aqn.ndtr.boneCount = aqn.nodeList.Count;
            aqp.objc.bonePaletteOffset = 1;

            //Material
            var mat = new AquaObject.GenericMaterial();
            mat.matName = $"Material_{0}";
            mat.texNames = new List<string>() { "tex0.dds" };
            aqp.tempMats.Add(mat);

            AquaObject.GenericTriangles genMesh = new AquaObject.GenericTriangles();
            genMesh.triList = new List<Vector3>();
            genMesh.matIdList.Add(0);
            genMesh.vertCount = mc2.vertPositions.Count;
            int f = 0;
            int v = 0;
            for (int i = 0; i < mc2.faceData.Count; i++)
            {
                var mc2Face = mc2.faceData[i];
                Vector3 face = new Vector3(mc2Face.vert0, mc2Face.vert1, mc2Face.vert2);
                AquaObject.VTXL faceVtxl = new AquaObject.VTXL();
                faceVtxl.rawFaceId.Add(f);
                faceVtxl.rawFaceId.Add(f);
                faceVtxl.rawFaceId.Add(f++);

                faceVtxl.rawVertId.Add(mc2Face.vert0);
                faceVtxl.rawVertId.Add(mc2Face.vert1);
                faceVtxl.rawVertId.Add(mc2Face.vert2);

                faceVtxl.vertPositions.Add(mc2.vertPositions[mc2Face.vert0] / 10);
                faceVtxl.vertPositions.Add(mc2.vertPositions[mc2Face.vert1] / 10);
                faceVtxl.vertPositions.Add(mc2.vertPositions[mc2Face.vert2] / 10);

                genMesh.faceVerts.Add(faceVtxl);
                genMesh.triList.Add(face);
            }
            aqp.tempTris.Add(genMesh);

            return aqp;
        }
    }
}
