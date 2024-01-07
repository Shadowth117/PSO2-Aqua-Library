using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaNodeData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.Intermediary;
using AquaModelLibrary.Helpers.MathHelpers;
using SoulsFormats;
using SoulsFormats.Other;
using System.Numerics;
using static AquaModelLibrary.Data.DataTypes.Vector3Int;

namespace AquaModelLibrary.Core.FromSoft.MetalWolfChaos
{
    public static class MDLConvert
    {
        public static AquaObject ConvertMDL(byte[] file, out AquaNode aqn)
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

        public static AquaObject MDLToAqua(MDL mdl, out AquaNode aqn)
        {
            AquaObject aqp = new AquaObject();
            aqn = new AquaNode();

            var model = mdl;
            Dictionary<int, int> usedMatIds = new Dictionary<int, int>();
            List<Matrix4x4> worldMats = new List<Matrix4x4>();
            for (int boneId = 0; boneId < mdl.Bones.Count; boneId++)
            {
                aqp.bonePalette.Add((uint)boneId);
                var bone = model.Bones[boneId];

                NODE node = new NODE();
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

        private static void AddMesh(AquaObject aqp, MDL model, Dictionary<int, int> usedMatIds, MDL.Bone bone, int vertSetAddition, List<MDL.Vertex> vertices, List<MDL.Faceset> faceSets, List<Matrix4x4> worldMats, int boneId, MDL.VertexFormat fmt)
        {
            int fs = 0;
            foreach (var faceSet in faceSets)
            {
                Dictionary<int, int> vertMap = new Dictionary<int, int>();

                GenericTriangles genMesh = new GenericTriangles();
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
                    var genMat = new GenericMaterial();
                    genMat.diffuseRGBA = mdlMat.diffuseColor;
                    genMat.matName = $"Material_{0}";
                    genMat.texNames = new List<string>() { model.Textures[mdlMat.TextureIndex] };
                    aqp.tempMats.Add(genMat);
                }


                //DEBUG REMOVE AFTER
                /*
                List<int> indexList = new List<int>();
                List<int> indexListValues = new List<int>();
                for (int f = faceSet.StartIndex; f < faceSet.StartIndex + faceSet.IndexCount - 2; f++)
                {
                    indexList.Add(f);
                    indexListValues.Add(model.Indices[f]);
                }
                List<bool> faceFlipList = new List<bool>();
                List<Vec3Int> faceIndicesListList = new List<Vec3Int>();
                */
                //DEBUG REMOVE AFTER


                //Mesh data
                int vertCounter = 0;
                bool flip = ((faceSet.StartIndex & 1) > 0);
                for (int f = faceSet.StartIndex; f < faceSet.StartIndex + faceSet.IndexCount - 2; f++)
                {
                    VTXL faceVtxl = new VTXL();
                    Vec3Int triIndices;

                    //When index is odd, flip
                    MDL.Vertex vertA, vertB, vertC;
                    bool currentFlip = flip;
                    if (flip)
                    {
                        triIndices = new Vec3Int(model.Indices[f + 2], model.Indices[f + 1], model.Indices[f]);
                    }
                    else
                    {
                        triIndices = new Vec3Int(model.Indices[f], model.Indices[f + 1], model.Indices[f + 2]);
                    }
                    vertA = vertices[triIndices.X];
                    vertB = vertices[triIndices.Y];
                    vertC = vertices[triIndices.Z];

                    //Avoid degen tris
                    //Apparently MWC just has some faces made from vertices that are share the same position. We obviously don't want lines.
                    bool nullVert = triIndices.X != 0xFFFF && triIndices.Y != 0xFFFF && triIndices.Z != 0xFFFF;
                    if (triIndices.X == triIndices.Y || triIndices.X == triIndices.Z || triIndices.Y == triIndices.Z && nullVert ||
                        vertA.Position.EpsEqual(vertB.Position, MathExtras.basicEpsilon) || vertA.Position.EpsEqual(vertC.Position, MathExtras.basicEpsilon) ||
                        vertB.Position.EpsEqual(vertC.Position, MathExtras.basicEpsilon))
                    {
                        if (nullVert)
                        {
                            flip = false;
                        }
                        continue;
                    }

                    var vertNormSum = vertA.Normal + vertB.Normal + vertC.Normal;
                    var vertNormSum2 = GetVertNormal(vertA, bone, worldMats, boneId) + GetVertNormal(vertB, bone, worldMats, boneId) + GetVertNormal(vertC, bone, worldMats, boneId);
                    var test0FNormal = MathExtras.GetFaceNormal(vertA.Position, vertB.Position, vertC.Position);
                    var test0 = MathExtras.Angle(test0FNormal, vertNormSum2);
                    var test1FNormal = MathExtras.GetFaceNormal(vertC.Position, vertB.Position, vertA.Position);
                    var test1 = MathExtras.Angle(test1FNormal, vertNormSum2);
                    if (test0 > test1)
                    {
                        var temp = triIndices.X;
                        triIndices.X = triIndices.Z;
                        triIndices.Z = temp;

                        var tempVert = vertA;
                        vertA = vertC;
                        vertC = tempVert;
                    }
                    else
                    {
                        flip = !flip;
                    }

                    //faceIndicesListList.Add(triIndices);
                    //faceFlipList.Add(currentFlip);

                    if (!vertMap.ContainsKey(triIndices.X))
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
                    faceVtxl.rawVertId.Add(vertMap[triIndices.X]);
                    faceVtxl.rawVertId.Add(vertMap[triIndices.Y]);
                    faceVtxl.rawVertId.Add(vertMap[triIndices.Z]);
                    AddVert(faceVtxl, vertA, bone, worldMats, boneId);
                    AddVert(faceVtxl, vertB, bone, worldMats, boneId);
                    AddVert(faceVtxl, vertC, bone, worldMats, boneId);
                    Vector3 tri = new Vector3(vertMap[triIndices.X], vertMap[triIndices.Y], vertMap[triIndices.Z]);

                    genMesh.triList.Add(tri);
                    genMesh.faceVerts.Add(faceVtxl);
                }
                genMesh.vertCount = vertCounter;
                aqp.tempTris.Add(genMesh);
                fs++;
            }
        }

        private static Vector3 GetVertNormal(MDL.Vertex vert, MDL.Bone bone, List<Matrix4x4> worldMats, int boneId)
        {
            switch (vert.StaticWeightFlag)
            {
                case 0x4:
                    boneId = bone.ParentIndex;
                    break;
            }

            return Vector3.TransformNormal(vert.Normal, worldMats[boneId]);
        }

        private static void AddVert(VTXL faceVtxl, MDL.Vertex vert, MDL.Bone bone, List<Matrix4x4> worldMats, int boneId)
        {
            switch (vert.StaticWeightFlag)
            {
                case 0x4:
                    boneId = bone.ParentIndex;
                    break;
            }
            faceVtxl.vertPositions.Add(Vector3.Transform(vert.Position / 2000, worldMats[boneId]));
            faceVtxl.vertNormals.Add(Vector3.TransformNormal(vert.Normal, worldMats[boneId]));
            faceVtxl.vertColors.Add(new byte[] { vert.Color.B, vert.Color.G, vert.Color.R, vert.Color.A });
            faceVtxl.uv1List.Add(vert.UVs[0]);
            faceVtxl.uv2List.Add(vert.UVs[1]);
            faceVtxl.uv3List.Add(vert.UVs[2]);
            faceVtxl.uv4List.Add(vert.UVs[3]);

            if (vert.PrimaryVertexWeight != 0 || vert.SecondaryVertexWeight != 0)
            {
                //var finalPos = Vector3.Transform(vert.Position / 2000, worldMats[boneId]) * vert.PrimaryVertexWeight + Vector3.Transform(vert.Position / 2000, worldMats[bone.ParentIndex]) * vert.SecondaryVertexWeight;
                //faceVtxl.vertPositions.Add(finalPos);
                //var finalNrm = Vector3.TransformNormal(vert.Normal / 2000, worldMats[boneId]) * vert.PrimaryVertexWeight + Vector3.TransformNormal(vert.Normal / 2000, worldMats[bone.ParentIndex]) * vert.SecondaryVertexWeight;
                //faceVtxl.vertNormals.Add(finalNrm);
                faceVtxl.vertWeightIndices.Add(new int[] { boneId, bone.ParentIndex, 0, 0 });
                faceVtxl.vertWeights.Add(new Vector4(vert.PrimaryVertexWeight, vert.SecondaryVertexWeight, 0, 0));
            }
            else
            {
                //faceVtxl.vertPositions.Add(Vector3.Transform(vert.Position / 2000, worldMats[boneId]));
                //faceVtxl.vertNormals.Add(Vector3.TransformNormal(vert.Normal, worldMats[boneId]));
                faceVtxl.vertWeightIndices.Add(new int[] { boneId, 0, 0, 0 });
                faceVtxl.vertWeights.Add(new Vector4(1, 0, 0, 0));
            }
        }
    }
}
