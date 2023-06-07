using Microsoft.Win32;
using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace AquaModelLibrary.Extra.AM2
{
    public class BorderBreakPS4Convert
    {
        public static List<AquaNode> motBonesToAQN(MOT_BONE bones)
        {
            List<AquaNode> aquaNodes = new List<AquaNode>();

            for(int i = 0; i < bones.skeletonList.Count; i++)
            {
                var skeleton = bones.skeletonList[i];
                var aqn = new AquaNode();
                var nodeArr = new AquaNode.NODE[skeleton.Count]; 

                for(int b = skeleton.Count - 1; b >= 0; b--)
                {
                    var bone = skeleton[b];
                    //Debug.WriteLine($"{bone.boneStruct.usht_08} {bone.boneStruct.usht_0A} {bone.name}");
                    var node = new AquaNode.NODE();
                    node.boneName.SetString(bone.name);
                    node.animatedFlag = 1;
                    node.boneShort1 = 0x1C0;
                    node.pos = bone.boneStruct.position;
                    node.eulRot = bone.boneStruct.eulerRotation * (float)(360 / Math.PI);
                    node.scale = new Vector3(1, 1, 1);
                    node.parentId = -1;
                    node.nextSibling = -1;
                    if(bone.childrenIds.Count > 0)
                    {
                        node.firstChild = bone.childrenIds[0];
                    } else
                    {
                        node.firstChild = -1;
                    }
                    
                    //Set parent and nextSibling for children. Only child data is stored
                    for(int c = 0; c < bone.childrenIds.Count; c++)
                    {
                        var childId = bone.childrenIds[c];
                        var cbone = nodeArr[childId];
                        cbone.parentId = b;

                        if (c != bone.childrenIds.Count - 1)
                        {
                            cbone.nextSibling = bone.childrenIds[c + 1];
                        }
                        nodeArr[childId] = cbone;
                    }
                    nodeArr[b] = node;
                }
                aqn.nodeList.AddRange(nodeArr);

                //Set inverse world matrices
                for(int b = 0; b < aqn.nodeList.Count; b++)
                {
                    var node = aqn.nodeList[b];

                    Matrix4x4 mat = Matrix4x4.Identity;
                    mat *= Matrix4x4.CreateScale(node.scale);
                    Matrix4x4 rotation = Matrix4x4.Identity;

                    if (node.boneName.GetString() == "cl_momo_l" || node.boneName.GetString() == "cl_momo_r")
                    {

                    }
                    else
                    {
                    }
                        rotation = Matrix4x4.CreateRotationX(node.eulRot.X) *
                            Matrix4x4.CreateRotationY(node.eulRot.Y) *
                            Matrix4x4.CreateRotationZ(node.eulRot.Z);
                        mat *= rotation;
                        mat *= Matrix4x4.CreateTranslation(node.pos);
                    //}
                    
                    

                    //If there's a parent, multiply by it
                    if (node.parentId > -1)
                    {
                        var pn = aqn.nodeList[node.parentId];
                        var parentInvTfm = new Matrix4x4(pn.m1.X, pn.m1.Y, pn.m1.Z, pn.m1.W,
                                                      pn.m2.X, pn.m2.Y, pn.m2.Z, pn.m2.W,
                                                      pn.m3.X, pn.m3.Y, pn.m3.Z, pn.m3.W,
                                                      pn.m4.X, pn.m4.Y, pn.m4.Z, pn.m4.W);
                        Matrix4x4.Invert(parentInvTfm, out var invParentInvTfm);
                        mat = mat * invParentInvTfm;
                    }

                    Matrix4x4.Invert(mat, out var invMat);
                    node.SetInverseBindPoseMatrix(invMat);

                    aqn.nodeList[b] = node;
                }

                aquaNodes.Add(aqn);
            }

            return aquaNodes;
        }

        public static List<NGSAquaObject> EOBJToAqua(E_OBJ mdl)
        {
            int matCounter = 0;
            List<NGSAquaObject> aqpList = new List<NGSAquaObject>();

            for(int m = 0; m < mdl.models.Count; m++)
            {
                NGSAquaObject aqp = new NGSAquaObject();
                var model = mdl.models[m];
                for(int i = 0; i <= model.highestBone; i++)
                {
                    aqp.bonePalette.Add((uint)i);
                }
                aqp.objc.bonePaletteOffset = 1;

                for(int vertSet = 0; vertSet < model.meshes.Count; vertSet++)
                {
                    var eObjMesh = model.meshes[vertSet];

                    AquaObject.VTXL vtxl = new AquaObject.VTXL();

                    vtxl.vertPositions.AddRange(eObjMesh.vertPositions);
                    vtxl.vertNormals.AddRange(eObjMesh.vertNormals);
                    vtxl.uv1List.AddRange(eObjMesh.vertUvs);
                    vtxl.uv2List.AddRange(eObjMesh.vertUv2s);
                    
                    if(eObjMesh.vertRigidWeightIndices.Count > 0)
                    {
                        foreach(var index in eObjMesh.vertRigidWeightIndices)
                        {
                            vtxl.vertWeightIndices.Add(new int[] { (int)index, 0, 0, 0 });
                            vtxl.vertWeights.Add(new Vector4(1, 0, 0, 0));
                        }
                    } else
                    {
                        vtxl.vertWeightIndices.AddRange(eObjMesh.vertWeightIndices);
                        vtxl.vertWeights.AddRange(eObjMesh.vertWeights);
                    }

                    int bpCounter = 0;
                    foreach(var faceSet in eObjMesh.faceLists)
                    {
                        //Material
                        var mat = new AquaObject.GenericMaterial();
                        mat.matName = $"Material_{matCounter++}";
                        mat.texNames = new List<string>() { "test_d.dds" };
                        aqp.tempMats.Add(mat);

                        Dictionary<int, int> vertIdDict = new Dictionary<int, int>();
                        AquaObject.VTXL matVtxl = new AquaObject.VTXL();
                        AquaObject.GenericTriangles genMesh = new AquaObject.GenericTriangles();
                        List<Vector3> triList = new List<Vector3>();
                        for (int vertIndex = 0; vertIndex < faceSet.Count - 2; vertIndex++)
                        {
                            Vector3 tri;
                            //When index is odd, flip
                            if ((vertIndex & 1) > 0)
                            {
                                tri = new Vector3(faceSet[vertIndex + 2], faceSet[vertIndex + 1], faceSet[vertIndex]);
                            }
                            else
                            {
                                tri =new Vector3(faceSet[vertIndex], faceSet[vertIndex + 1], faceSet[vertIndex + 2]);
                            }

                            int x;
                            int y;
                            int z;
                            if (vertIdDict.TryGetValue((int)tri.X, out var value))
                            {
                                x = value;
                            }
                            else
                            {
                                vertIdDict.Add((int)tri.X, matVtxl.vertPositions.Count);
                                x = matVtxl.vertPositions.Count;
                                AquaObjectMethods.appendVertex(vtxl, matVtxl, (int)tri.X);
                            }
                            if (vertIdDict.TryGetValue((int)tri.Y, out var value2))
                            {
                                y = value2;
                            }
                            else
                            {
                                vertIdDict.Add((int)tri.Y, matVtxl.vertPositions.Count);
                                y = matVtxl.vertPositions.Count;
                                AquaObjectMethods.appendVertex(vtxl, matVtxl, (int)tri.Y);
                            }
                            if (vertIdDict.TryGetValue((int)tri.Z, out var value3))
                            {
                                z = value3;
                            }
                            else
                            {
                                vertIdDict.Add((int)tri.Z, matVtxl.vertPositions.Count);
                                z = matVtxl.vertPositions.Count;
                                AquaObjectMethods.appendVertex(vtxl, matVtxl, (int)tri.Z);
                            }

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
                }

                aqpList.Add(aqp);
            }

            return aqpList;
        }
    }
}
