using Reloaded.Memory.Streams;
using SoulsFormats;
using SoulsFormats.Formats.Other.MWC;
using System.Collections.Generic;
using System.Numerics;

namespace AquaModelLibrary.Extra.FromSoft
{
    public class MMDConvert
    {
        public static NGSAquaObject ConvertMMD(byte[] file, out AquaNode aqn)
        {
            return MMDToAqua(SoulsFile<MMD>.Read(file), out aqn);
        }

        public static NGSAquaObject MMDToAqua(MMD mmd, out AquaNode aqn)
        {
            int matCounter = 0;

            NGSAquaObject aqp = new NGSAquaObject();
            aqn = new AquaNode();

            var model = mmd;
            for (int i = 0; i < model.bones.Count; i++)
            {
                aqp.bonePalette.Add((uint)i);
                var bone = model.bones[i];

                AquaNode.NODE node = new AquaNode.NODE();
                node.animatedFlag = 1;
                node.boneName.SetString($"bone_{i}");

                var mat = MathExtras.Compose(bone.translation, bone.rotation, bone.scale);
                Matrix4x4.Invert(mat, out var invMat);

                node.SetInverseBindPoseMatrix(invMat);
                node.pos = bone.translation;
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

            AquaObject.VTXL vtxl = new AquaObject.VTXL();
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
                var mat = new AquaObject.GenericMaterial();
                mat.matName = $"Material_{matCounter++}";
                mat.texNames = new List<string>() { mmd.texNames[faceSet.materialId] };
                aqp.tempMats.Add(mat);

                Dictionary<int, int> vertIdDict = new Dictionary<int, int>();
                AquaObject.VTXL matVtxl = new AquaObject.VTXL();
                AquaObject.GenericTriangles genMesh = new AquaObject.GenericTriangles();
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

                    AquaObjectMethods.AddVertices(vtxl, vertIdDict, matVtxl, tri, out int x, out int y, out int z);

                    //Avoid degen tris
                    if (x == y || x == z || y == z)
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
                    aqp.vtxeList.Add(AquaObjectMethods.ConstructClassicVTXE(matVtxl, out int vc));
                }
            }

            var a = AquaObjectMethods.GenerateBounding(aqp.vtxlList);
            return aqp;
        }
    }
}
