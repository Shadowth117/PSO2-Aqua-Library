using SoulsFormats;
using SoulsFormats.Other;
using System;
using System.Collections.Generic;
using System.Numerics;
using static Vector3Integer.Vector3Int;

namespace AquaModelLibrary.Extra.FromSoft.MetalWolfChaos
{
    public static class MDLConvert
    {
        public static NGSAquaObject ConvertMDL(byte[] file, out AquaNode aqn)
        {

            if (SoulsFile<MDL>.Is(file))
            {
                return MDLToAqua(SoulsFile<MDL>.Read(file), out aqn);
            }
            else if (SoulsFile<MDL0>.Is(file))
            {
                aqn = new AquaNode();
                return null;
            }

            aqn = null;
            return null;
        }

        public static NGSAquaObject MDLToAqua(MDL mdl, out AquaNode aqn)
        {
            NGSAquaObject aqp = new NGSAquaObject();
            aqn = new AquaNode();

            var model = mdl;
            Dictionary<int, int> usedMatIds = new Dictionary<int, int>();
            List<Matrix4x4> worldMats = new List<Matrix4x4>();
            for (int boneId = 0; boneId < mdl.Bones.Count; boneId++)
            {
                aqp.bonePalette.Add((uint)boneId);
                var bone = model.Bones[boneId];

                AquaNode.NODE node = new AquaNode.NODE();
                node.animatedFlag = 1;
                node.boneName.SetString($"bone_{boneId}");

                var parentMat = Matrix4x4.Identity;
                if (bone.ParentIndex != -1)
                {
                    parentMat = aqn.nodeList[bone.ParentIndex].GetInverseBindPoseMatrixInverted();
                }

                var mat = MathExtras.Compose(bone.Translation / 2000, bone.Rotation, bone.Scale) * parentMat;
                worldMats.Add(mat);
                Matrix4x4.Invert(mat, out var invMat);

                node.SetInverseBindPoseMatrix(invMat);
                node.pos = bone.Translation / 2000;
                node.eulRot = bone.Rotation;
                node.scale = bone.Scale;
                node.parentId = bone.ParentIndex;
                node.nextSibling = bone.NextSiblingIndex;
                node.firstChild = bone.ChildIndex;
                node.unkNode = -1;
                aqn.nodeList.Add(node);
            }
            aqn.ndtr.boneCount = aqn.nodeList.Count;
            aqp.objc.bonePaletteOffset = 1;

            for (int i = 0; i < model.Bones.Count; i++)
            {
                var bone = model.Bones[i];
                //Mesh A
                if (bone.FacesetsA.Count > 0)
                {
                    int vertSetAddition = 0;
                    AddMesh(aqp, model, usedMatIds, bone, vertSetAddition, model.VerticesA, bone.FacesetsA, worldMats, i, MDL.VertexFormat.A);
                }

                //Mesh B
                if (bone.FacesetsB.Count > 0)
                {
                    int vertSetAddition = model.VerticesA.Count;
                    AddMesh(aqp, model, usedMatIds, bone, vertSetAddition, model.VerticesB, bone.FacesetsB, worldMats, i, MDL.VertexFormat.B);
                }

                //Mesh C
                if (bone.FacesetsC.Count > 0)
                {
                    int vertSetAddition = model.VerticesA.Count + model.VerticesB.Count;
                    foreach (var faceSet in bone.FacesetsC)
                    {
                        AddMesh(aqp, model, usedMatIds, bone, vertSetAddition, model.VerticesC, faceSet.Facesets, worldMats, i, MDL.VertexFormat.C);
                    }
                }

                //Mesh D
                if (bone.FacesetsD.Count > 0)
                {
                    int vertSetAddition = model.VerticesA.Count + model.VerticesB.Count + model.VerticesC.Count;
                    throw new Exception("D mesh types not supported yet!");
                }
            }

            return aqp;
        }

        private static void AddMesh(NGSAquaObject aqp, MDL model, Dictionary<int, int> usedMatIds, MDL.Bone bone, int vertSetAddition, List<MDL.Vertex> vertices, List<MDL.Faceset> faceSets, List<Matrix4x4> worldMats, int boneId, MDL.VertexFormat fmt)
        {
            int fs = 0;
            if(boneId == 14)
            {
                var a = 0;
            }
            foreach (var faceSet in faceSets)
            {
                Dictionary<int, int> vertMap = new Dictionary<int, int>();

                AquaObject.GenericTriangles genMesh = new AquaObject.GenericTriangles();
                genMesh.triList = new List<Vector3>();
                genMesh.name = $"mesh_{boneId}_{fmt}_{fs}";
                aqp.meshNames.Add(genMesh.name);

                //Material
                if (usedMatIds.ContainsKey(faceSet.MaterialIndex))
                {
                    genMesh.matIdList.Add(usedMatIds[faceSet.MaterialIndex]);
                }
                else
                {
                    genMesh.matIdList.Add(aqp.tempMats.Count);
                    usedMatIds.Add(faceSet.MaterialIndex, aqp.tempMats.Count);

                    var mdlMat = model.Materials[faceSet.MaterialIndex];
                    var genMat = new AquaObject.GenericMaterial();
                    genMat.diffuseRGBA = mdlMat.diffuseColor;
                    genMat.matName = $"Material_{0}";
                    genMat.texNames = new List<string>() { model.Textures[mdlMat.TextureIndex] };
                    aqp.tempMats.Add(genMat);
                }

                //Mesh data
                int vertCounter = 0;
                bool flip = (faceSet.StartIndex & 1) > 0;
                for (int f = faceSet.StartIndex; f < faceSet.StartIndex + faceSet.IndexCount - 2; f++)
                {
                    AquaObject.VTXL faceVtxl = new AquaObject.VTXL();
                    Vec3Int triIndices;

                    //When index is odd, flip
                    MDL.Vertex vertA, vertB, vertC;

                    if (flip)
                    {
                        triIndices = new Vec3Int(model.Indices[f + 2], model.Indices[f + 1], model.Indices[f]);
                        vertA = vertices[model.Indices[f + 2]];
                        vertB = vertices[model.Indices[f + 1]];
                        vertC = vertices[model.Indices[f]];
                    }
                    else
                    {
                        triIndices = new Vec3Int(model.Indices[f], model.Indices[f + 1], model.Indices[f + 2]);
                        vertA = vertices[model.Indices[f]];
                        vertB = vertices[model.Indices[f + 1]];
                        vertC = vertices[model.Indices[f + 2]];
                    }
                    flip = !flip;

                    //Avoid degen tris
                    bool nullVert = triIndices.X != 0xFFFF && triIndices.Y != 0xFFFF && triIndices.Z != 0xFFFF;
                    if (triIndices.X == triIndices.Y || triIndices.X == triIndices.Z || triIndices.Y == triIndices.Z && nullVert)
                    {
                        if(nullVert)
                        {
                            flip = false;
                        }
                        continue;
                    }

                    if(!vertMap.ContainsKey(triIndices.X))
                    {
                        vertMap[triIndices.X] = vertCounter++;
                    }
                    if (!vertMap.ContainsKey(triIndices.Y))
                    {
                        vertMap[triIndices.Y] = vertCounter++;
                    }
                    if (!vertMap.ContainsKey(triIndices.Z))
                    {
                        vertMap[triIndices.Z] = vertCounter++;
                    }
                    faceVtxl.rawVertId.Add(vertMap[triIndices.X] );
                    faceVtxl.rawVertId.Add(vertMap[triIndices.Y] );
                    faceVtxl.rawVertId.Add(vertMap[triIndices.Z] );
                    AddVert(faceVtxl, vertA, bone, worldMats, boneId);
                    AddVert(faceVtxl, vertB, bone, worldMats, boneId);
                    AddVert(faceVtxl, vertC, bone, worldMats, boneId);
                    Vector3 tri = new Vector3(vertMap[triIndices.X], vertMap[triIndices.Y], vertMap[triIndices.Z]);

                    genMesh.triList.Add(tri);
                    genMesh.faceVerts.Add(faceVtxl);
                    genMesh.vertCount += 3;
                }

                aqp.tempTris.Add(genMesh);
                fs++;
            }
        }

        private static void AddVert(AquaObject.VTXL faceVtxl, MDL.Vertex vert, MDL.Bone bone, List<Matrix4x4> worldMats, int boneId)
        {
            switch (vert.StaticWeightFlag)
            {
                case 0x4:
                    boneId = bone.ParentIndex;
                    break;
            }
            faceVtxl.vertPositions.Add(Vector3.Transform(vert.Position / 2000, worldMats[boneId]));
            faceVtxl.vertColors.Add(new byte[] { vert.Color.B, vert.Color.G, vert.Color.R, vert.Color.A });
            faceVtxl.vertNormals.Add(Vector3.TransformNormal(vert.Normal, worldMats[boneId]));
            faceVtxl.uv1List.Add(vert.UVs[0]);
            faceVtxl.uv2List.Add(vert.UVs[1]);
            faceVtxl.uv3List.Add(vert.UVs[2]);
            faceVtxl.uv4List.Add(vert.UVs[3]);
            faceVtxl.vertWeightIndices.Add(new int[] { boneId, 0, 0, 0 });
            faceVtxl.vertWeights.Add(new Vector4(1, 0, 0, 0));
        }
    }
}
