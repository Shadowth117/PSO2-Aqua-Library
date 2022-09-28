using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SoulsFormats;
using static AquaModelLibrary.AquaNode;

namespace AquaModelLibrary.Extra
{
    public static class SoulsConvert
    {
        public enum SoulsGame
        {
            DemonsSouls,
            DarkSouls1,
            DarkSouls2,
            Bloodborne,
            DarkSouls3,
            Sekiro
        }

        public static AquaObject ReadFlver(string filePath, out AquaNode aqn)
        {
            SoulsFormats.IFlver flver = null;
            var raw = File.ReadAllBytes(filePath);
            if (SoulsFormats.SoulsFile<SoulsFormats.FLVER0>.Is(raw))
            {
                flver = SoulsFormats.SoulsFile<SoulsFormats.FLVER0>.Read(raw);
            }
            else if (SoulsFormats.SoulsFile<SoulsFormats.FLVER2>.Is(raw))
            {
                flver = SoulsFormats.SoulsFile<SoulsFormats.FLVER2>.Read(raw);
            }
            aqn = null;
            return FlverToAqua(flver, out aqn);
        }
        
        public static AquaObject FlverToAqua(IFlver flver, out AquaNode aqn)
        {
            AquaObject aqp = new NGSAquaObject();

            if(flver is FLVER2 flver2)
            {
                if(flver2.Header.Version > 20010)
                {
                    for(int i = 0; i < flver2.Bones.Count; i++)
                    {
                        aqp.bonePalette.Add((uint)i);
                    }
                }
            }
            //aqp.bonePalette = new List<uint>();
            aqn = new AquaNode();
            var mirrorMat = new Matrix4x4(-1, 0, 0, 0,
                                        0, 1, 0, 0,
                                        0, 0, 1, 0,
                                        0, 0, 0, 1);
            for (int i = 0; i < flver.Bones.Count; i++)
            {
                //aqp.bonePalette.Add((uint)i);
                var flverBone = flver.Bones[i];
                var parentId = flverBone.ParentIndex;

                FLVER.Bone.RotationOrder order = FLVER.Bone.RotationOrder.XZY;
                var tfmMat = Matrix4x4.Identity;
                if(flver is FLVER2)
                {
                } else
                {
                    //Translation adjustment
                    var boneTranslation = flverBone.Translation;
                    boneTranslation.X = -flverBone.Translation.X;
                    flverBone.Translation = boneTranslation;

                    //Rotation adjustment
                    var boneRotation = flverBone.Rotation;
                    boneRotation.Z = -boneRotation.Z;
                    flverBone.Rotation = boneRotation;
                }

                Matrix4x4 mat = flverBone.ComputeLocalTransform(order);

                mat *= tfmMat;

                Matrix4x4.Decompose(mat, out var scale, out var quatRot, out var translation);

                //If there's a parent, multiply by it
                if (parentId != -1)
                {
                    var pn = aqn.nodeList[parentId];
                    var parentInvTfm = new Matrix4x4(pn.m1.X, pn.m1.Y, pn.m1.Z, pn.m1.W,
                                                  pn.m2.X, pn.m2.Y, pn.m2.Z, pn.m2.W,
                                                  pn.m3.X, pn.m3.Y, pn.m3.Z, pn.m3.W,
                                                  pn.m4.X, pn.m4.Y, pn.m4.Z, pn.m4.W);
                    
                    Matrix4x4.Invert(parentInvTfm, out var invParentInvTfm);
                    mat = mat * invParentInvTfm;
                }
                if(parentId == -1 && i != 0)
                {
                    parentId = 0;
                }

                //Create AQN node
                NODE aqNode = new NODE();
                aqNode.boneShort1 = 0x1C0;
                aqNode.animatedFlag = 1;
                aqNode.parentId = parentId;
                aqNode.unkNode = -1;
                aqNode.pos = translation;
                aqNode.eulRot = MathExtras.QuaternionToEuler(quatRot);

                if (Math.Abs(aqNode.eulRot.Y) > 120)
                {
                    aqNode.scale = new Vector3(-1, -1, -1);
                }
                else
                {
                    aqNode.scale = new Vector3(1, 1, 1);
                }
                Matrix4x4.Invert(mat, out var invMat);
                aqNode.m1 = new Vector4(invMat.M11, invMat.M12, invMat.M13, invMat.M14);
                aqNode.m2 = new Vector4(invMat.M21, invMat.M22, invMat.M23, invMat.M24);
                aqNode.m3 = new Vector4(invMat.M31, invMat.M32, invMat.M33, invMat.M34);
                aqNode.m4 = new Vector4(invMat.M41, invMat.M42, invMat.M43, invMat.M44);
                aqNode.boneName.SetString(flverBone.Name);
                //Debug.WriteLine($"{i} " + aqNode.boneName.GetString());
                aqn.nodeList.Add(aqNode);
            }
            for (int i = 0; i < flver.Meshes.Count; i++)
            {
                var mesh = flver.Meshes[i];

                var nodeMatrix = Matrix4x4.Identity;

                //Vert data
                var vertCount = mesh.Vertices.Count;
                AquaObject.VTXL vtxl = new AquaObject.VTXL();

                if (mesh.Dynamic > 0)
                {
                    if(mesh is FLVER0.Mesh flv)
                    {

                        for (int b = 0; b < flv.BoneIndices.Length; b++)
                        {
                            if (flv.BoneIndices[b] == -1)
                            {
                                break;
                            }
                            vtxl.bonePalette.Add((ushort)flv.BoneIndices[b]);
                        }
                    } else if (mesh is FLVER2.Mesh flv2)
                    {
                        for (int b = 0; b < flv2.BoneIndices.Count; b++)
                        {
                            if (flv2.BoneIndices[b] == -1)
                            {
                                break;
                            }
                            vtxl.bonePalette.Add((ushort)flv2.BoneIndices[b]);
                        }
                    }
                }
                List<int> indices = new List<int>();
                if (flver is FLVER0)
                {
                    FLVER0.Mesh mesh0 = (FLVER0.Mesh)mesh;
                    vtxl.bonePalette = new List<ushort>();
                    for (int b = 0; b < mesh0.BoneIndices.Length; b++)
                    {
                        if (mesh0.BoneIndices[b] == -1)
                        {
                            break;
                        }
                        vtxl.bonePalette.Add((ushort)mesh0.BoneIndices[b]);
                    }
                    indices = mesh0.Triangulate(((FLVER0)flver).Header.Version);
                }
                else if (flver is FLVER2)
                {
                    FLVER2.Mesh mesh2 = (FLVER2.Mesh)mesh;

                    //Dark souls 3+ (Maybe bloodborne too) use direct bone id references instead of a bone palette
                    vtxl.bonePalette = new List<ushort>();
                    for (int b = 0; b < mesh2.BoneIndices.Count; b++)
                    {
                        if (mesh2.BoneIndices[b] == -1)
                        {
                            break;
                        }
                        vtxl.bonePalette.Add((ushort)mesh2.BoneIndices[b]);
                    }

                    FLVER2.FaceSet faceSet = mesh2.FaceSets[0];
                    indices = faceSet.Triangulate(mesh2.Vertices.Count < ushort.MaxValue);
                }
                else
                {
                    throw new Exception("Unexpected flver variant");
                }

                for (int v = 0; v < vertCount; v++)
                {
                    var vert = mesh.Vertices[v];
                    vtxl.vertPositions.Add(Vector3.Transform(vert.Position, mirrorMat));
                    if(flver is FLVER0)
                    {
                        vtxl.vertNormals.Add(Vector3.Transform(vert.Normal, mirrorMat));
                    } else
                    {
                        vtxl.vertNormals.Add(vert.Normal);
                    }

                    if (vert.UVs.Count > 0)
                    {
                        var uv1 = vert.UVs[0];
                        vtxl.uv1List.Add(new Vector2(uv1.X, uv1.Y));
                    }
                    if (vert.UVs.Count > 1)
                    {
                        var uv2 = vert.UVs[1];
                        vtxl.uv2List.Add(new Vector2(uv2.X, uv2.Y));
                    }
                    if (vert.UVs.Count > 2)
                    {
                        var uv3 = vert.UVs[2];
                        vtxl.uv3List.Add(new Vector2(uv3.X, uv3.Y));
                    }
                    if (vert.UVs.Count > 3)
                    {
                        var uv4 = vert.UVs[3];
                        vtxl.uv4List.Add(new Vector2(uv4.X, uv4.Y));
                    }

                    if (vert.Colors.Count > 0)
                    {
                        var color = vert.Colors[0];
                        vtxl.vertColors.Add(new byte[] { (byte)(color.B * 255), (byte)(color.G * 255), (byte)(color.R * 255), (byte)(color.A * 255) });
                    }
                    if (vert.Colors.Count > 1)
                    {
                        var color2 = vert.Colors[1];
                        vtxl.vertColor2s.Add(new byte[] { (byte)(color2.B * 255), (byte)(color2.G * 255), (byte)(color2.R * 255), (byte)(color2.A * 255) });
                    }

                    if(vert.BoneWeights.Length > 0)
                    {
                        vtxl.vertWeights.Add(new Vector4(vert.BoneWeights[0], vert.BoneWeights[1], vert.BoneWeights[2], vert.BoneWeights[3]));
                        vtxl.vertWeightIndices.Add(new int[] { vert.BoneIndices[0], vert.BoneIndices[1], vert.BoneIndices[2], vert.BoneIndices[3] });
                    } else if(vert.BoneIndices.Length > 0)
                    {
                        vtxl.vertWeights.Add(new Vector4(1, 0, 0, 0));
                        vtxl.vertWeightIndices.Add(new int[] { vert.BoneIndices[0], 0, 0, 0 });
                    } else if(vert.NormalW < 65535)
                    {
                        vtxl.vertWeights.Add(new Vector4(1, 0, 0, 0));
                        vtxl.vertWeightIndices.Add(new int[] { vert.NormalW, 0, 0, 0 });
                    }
                }

                vtxl.convertToLegacyTypes();
                aqp.vtxeList.Add(AquaObjectMethods.ConstructClassicVTXE(vtxl, out int vc));
                aqp.vtxlList.Add(vtxl);

                //Face data
                AquaObject.GenericTriangles genMesh = new AquaObject.GenericTriangles();

                List<Vector3> triList = new List<Vector3>();
                if (flver is FLVER0)
                {
                    for (int id = 0; id < indices.Count - 2; id += 3)
                    {
                        triList.Add(new Vector3(indices[id], indices[id + 1], indices[id + 2]));
                    }
                }
                else
                {
                    for (int id = 0; id < indices.Count - 2; id += 3)
                    {
                        triList.Add(new Vector3(indices[id], indices[id + 1], indices[id + 2]));
                    }
                }


                genMesh.triList = triList;

                //Extra
                genMesh.vertCount = vertCount;
                genMesh.matIdList = new List<int>(new int[genMesh.triList.Count]);
                for (int j = 0; j < genMesh.matIdList.Count; j++)
                {
                    genMesh.matIdList[j] = aqp.tempMats.Count;
                }
                aqp.tempTris.Add(genMesh);

                //Material
                var mat = new AquaObject.GenericMaterial();
                var flverMat = flver.Materials[mesh.MaterialIndex];
                mat.matName = flverMat.Name;
                mat.texNames = new List<string>();
                foreach(var tex in flverMat.Textures)
                {
                    mat.texNames.Add(Path.GetFileName(tex.Path));
                }
                aqp.tempMats.Add(mat);
            }

            return aqp;
        }

        public static IFlver ConvertModelToFlver(string initialFilePath, float scaleFactor, bool preAssignNodeIds, bool isNGS, SoulsGame game, IFlver matReferenceFlver = null)
        {
            return AquaToFlver(initialFilePath, ModelImporter.AssimpAquaConvertFull(initialFilePath, scaleFactor, preAssignNodeIds, isNGS, out AquaNode aqn), aqn, game, matReferenceFlver);
        }

        public static IFlver AquaToFlver(string initialFilePath, AquaObject aqp, AquaNode aqn, SoulsGame game, IFlver matReferenceFlver)
        {
            FLVER0 flver = new FLVER0();
            flver.Header = new FLVER0Header();
            flver.Header.BigEndian = true;
            flver.Header.Unicode = true;
            flver.Header.Version = 0x15;
            flver.Header.Unk4A = 0x1;
            flver.Header.Unk4B = 0;
            flver.Header.Unk4C = 0xFFFF;
            flver.Header.Unk5C = 0;

            List<string> usedMaterials = new List<string>();
            if (matReferenceFlver == null)
            {
            } else
            {
                Dictionary<string, FLVER0.Material> matDict = new Dictionary<string, FLVER0.Material>();
                for(int i = 0; i < matReferenceFlver.Materials.Count; i++)
                {
                    matDict.Add(matReferenceFlver.Materials[i].Name, (FLVER0.Material)matReferenceFlver.Materials[i]);
                }
                
                for (int i = 0; i < aqp.mateList.Count; i++)
                {
                    var matIndex = usedMaterials.IndexOf(aqp.mateList[i].matName.GetString());
                    if (matIndex != -1)
                    {
                        continue;
                    } else
                    {
                        var name = aqp.mateList[i].matName.GetString();
                        usedMaterials.Add(name);
                        if(matDict.TryGetValue(name, out var mat))
                        {
                            flver.Materials.Add(mat);
                        } else
                        {
                            var flvMat = new FLVER0.Material(true);
                            flvMat.Name = name;
                            flvMat.MTD = "u:\\nlimited\\based\\works\\p_metal[dsb].mtd";
                            flvMat.Layouts = new List<FLVER0.BufferLayout>();
                            //flvMat.Layouts.Add(new FLVER0.BufferLayout());
                            //flvMat.Textures
                        }
                    }
                }
            }

            return flver;
        }
    }
}
