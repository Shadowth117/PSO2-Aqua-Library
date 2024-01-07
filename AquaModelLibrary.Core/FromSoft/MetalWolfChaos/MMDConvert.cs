using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaNodeData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.Intermediary;
using AquaModelLibrary.Helpers.MathHelpers;
using SoulsFormats;
using SoulsFormats.Formats.Other.MWC;
using System.Numerics;

namespace AquaModelLibrary.Core.FromSoft.MetalWolfChaos
{
    public class MMDConvert
    {
        public static AquaObject ConvertMMD(byte[] file, out AquaNode aqn)
        {
            return MMDToAqua(SoulsFile<MMD>.Read(file), out aqn);
        }

        public static AquaObject MMDToAqua(MMD mmd, out AquaNode aqn)
        {
            int matCounter = 0;

            AquaObject aqp = new AquaObject();
            aqn = new AquaNode();

            var model = mmd;
            for (int i = 0; i < model.bones.Count; i++)
            {
                aqp.bonePalette.Add((uint)i);
                var bone = model.bones[i];

                NODE node = new NODE();
                node.animatedFlag = 1;
                node.boneName.SetString($"bone_{i}");

                var mat = MathExtras.Compose(bone.translation / 2000, bone.rotation, bone.scale);
                Matrix4x4.Invert(mat, out var invMat);

                node.SetInverseBindPoseMatrix(invMat);
                node.pos = bone.translation / 2000;
                node.eulRot = MathExtras.QuaternionToEuler(bone.rotation);
                node.scale = bone.scale;
                node.parentId = i == 0 ? -1 : 0;
                node.nextSibling = -1;
                node.firstChild = -1;
                node.unkNode = -1;
                aqn.nodeList.Add(node);
            }
            aqn.ndtr.boneCount = aqn.nodeList.Count;
            aqp.objc.bonePaletteOffset = 1;

            VTXL vtxl = new VTXL();
            for (int i = 0; i < mmd.vertices.Count; i++)
            {
                var vertex = mmd.vertices[i];
                vtxl.vertPositions.Add(vertex.Position / 2000);
                //vtxl.vertNormals.Add(vertex.Normal);
                vtxl.uv1List.Add(vertex.UVs[0]);
                vtxl.uv2List.Add(vertex.UVs[1]);
                vtxl.uv3List.Add(vertex.UVs[2]);
                vtxl.uv4List.Add(vertex.UVs[3]);
                vtxl.vertColors.Add(new byte[] { vertex.Color.B, vertex.Color.G, vertex.Color.R, vertex.Color.A });
            }

            foreach (var faceSet in mmd.meshHeaders)
            {
                //Material
                var mat = new GenericMaterial();
                mat.matName = $"Material_{matCounter++}";
                mat.texNames = new List<string>() { mmd.texNames[faceSet.materialId] };
                aqp.tempMats.Add(mat);

                Dictionary<int, int> vertIdDict = new Dictionary<int, int>();
                VTXL matVtxl = new VTXL();
                GenericTriangles genMesh = new GenericTriangles();
                List<Vector3> triList = new List<Vector3>();
                for (int vertIndex = faceSet.faceIndexStart; vertIndex < faceSet.faceIndexStart + faceSet.faceIndexCount - 2; vertIndex++)
                {
                    Vector3 tri;
                    //When index is odd, flip
                    if ((vertIndex & 1) > 0)
                    {
                        tri = new Vector3(mmd.faceIndices[vertIndex + 2], mmd.faceIndices[vertIndex + 1], mmd.faceIndices[vertIndex]);
                    }
                    else
                    {
                        tri = new Vector3(mmd.faceIndices[vertIndex], mmd.faceIndices[vertIndex + 1], mmd.faceIndices[vertIndex + 2]);
                    }

                    VTXL.AddVertices(vtxl, vertIdDict, matVtxl, tri, out int x, out int y, out int z);

                    //Avoid degen tris
                    if (x == y || x == z || y == z && x != 0xFFFF && y != 0xFFFF && z != 0xFFFF)
                    {
                        continue;
                    }
                    triList.Add(new Vector3(x, y, z));
                }
                genMesh.triList = triList;
                //matVtxl.bonePalette.AddRange(eObjMesh.bonePalettes[bpCounter++]);

                //Extra
                genMesh.vertCount = matVtxl.vertPositions.Count;
                genMesh.matIdList = new List<int>(new int[genMesh.triList.Count]);
                for (int j = 0; j < genMesh.matIdList.Count; j++)
                {
                    genMesh.matIdList[j] = aqp.tempMats.Count - 1;
                }

                if (genMesh.vertCount > 0)
                {
                    aqp.tempTris.Add(genMesh);
                    aqp.vtxlList.Add(matVtxl);
                    aqp.vtxeList.Add(VTXE.ConstructFromVTXL(matVtxl, out int vc));
                }
            }
            return aqp;
        }
    }
}
