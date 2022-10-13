using AquaModelLibrary.BluePoint.CMSH;
using AquaModelLibrary.BluePoint.CSKL;
using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static AquaModelLibrary.AquaNode;

namespace AquaModelLibrary.Extra
{
    public class BluePointConvert
    {
        public static AquaObject ReadCMDL(string filePath, out AquaNode aqn)
        {
            string cmshPath = Path.ChangeExtension(filePath, ".cmsh");
            string csklPath = Path.ChangeExtension(filePath, ".cskl");
            CSKL cskl = null;
            if(File.Exists(csklPath))
            {
                using (Stream stream = new MemoryStream(File.ReadAllBytes(csklPath)))
                using (var streamReader = new BufferedStreamReader(stream, 8192))
                {
                    cskl = new CSKL(streamReader);
                }
            }
            using (Stream stream = new MemoryStream(File.ReadAllBytes(cmshPath)))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                return CMDLToAqua(new List<CMSH>() { new CMSH(streamReader) }, cskl, out aqn);
            }
        }

        public static AquaObject CMDLToAqua(List<CMSH> mdl, CSKL cskl, out AquaNode aqn)
        {
            var mirrorMat = Matrix4x4.Identity;
            /*var mirrorMat = new Matrix4x4(-1, 0, 0, 0,
                                        0, 1, 0, 0,
                                        0, 0, 1, 0,
                                        0, 0, 0, 1);
            */

            aqn = AquaNode.GenerateBasicAQN();
            AquaObject aqp = new NGSAquaObject();
            if(cskl == null || cskl.header.boneCount == 0)
            {
                aqp.bonePalette.Add((uint)0);
            } else
            {
                for (int i = 0; i < cskl.header.boneCount; i++)
                {
                    aqp.bonePalette.Add((uint)i);
                }
                aqn = new AquaNode();

                for (int i = 0; i < cskl.header.boneCount; i++)
                {
                    var metadata = cskl.metadata.familyIds[i];
                    var parentId = metadata.parentId;

                    var tfmMat = Matrix4x4.Identity;

                    Matrix4x4 mat = cskl.transforms[i].ComputeLocalTransform();
                    Matrix4x4 invMatReal = cskl.invTransforms[i];
                    //invMatReal = Matrix4x4.Transpose(invMatReal);
                    Matrix4x4.Invert(invMatReal, out var invInvMat);
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
                    if (parentId == -1 && i != 0)
                    {
                        parentId = 0;
                    }

                    //Create AQN node
                    NODE aqNode = new NODE();
                    aqNode.boneShort1 = 0x1C0;
                    aqNode.animatedFlag = 1;
                    aqNode.parentId = parentId;
                    aqNode.unkNode = -1;

                    aqNode.scale = new Vector3(1, 1, 1);

                    Matrix4x4.Invert(mat, out var invMat);
                    aqNode.m1 = new Vector4(invMat.M11, invMat.M12, invMat.M13, invMat.M14);
                    aqNode.m2 = new Vector4(invMat.M21, invMat.M22, invMat.M23, invMat.M24);
                    aqNode.m3 = new Vector4(invMat.M31, invMat.M32, invMat.M33, invMat.M34);
                    aqNode.m4 = new Vector4(invMat.M41, invMat.M42, invMat.M43, invMat.M44);
                    aqNode.boneName.SetString(cskl.names.primaryNames.names[i].Split('|').Last());
                    //Debug.WriteLine($"{i} " + aqNode.boneName.GetString());
                    aqn.nodeList.Add(aqNode);
                }

                //I 100% believe there's a better way to do this when constructing the matrix, but for now we do this.
                for (int i = 0; i < aqn.nodeList.Count; i++)
                {
                    var bone = aqn.nodeList[i];
                    Matrix4x4.Invert(bone.GetInverseBindPoseMatrix(), out var mat);
                    mat *= mirrorMat;
                    Matrix4x4.Decompose(mat, out var scale, out var rot, out var translation);
                    bone.pos = translation;
                    bone.eulRot = MathExtras.QuaternionToEuler(rot);

                    Matrix4x4.Invert(mat, out var invMat);
                    bone.SetInverseBindPoseMatrix(invMat);
                    aqn.nodeList[i] = bone;
                }
            }
            

            for (int i = 0; i < mdl.Count; i++)
            {
                var mesh = mdl[i];

                var nodeMatrix = Matrix4x4.Identity;

                //Vert data
                var vertCount = mesh.vertData.positionList.Count;
                AquaObject.VTXL vtxl = new AquaObject.VTXL();

                for (int v = 0; v < vertCount; v++)
                {
                    vtxl.vertPositions.Add(Vector3.Transform(mesh.vertData.positionList[v], mirrorMat));
                    //vtxl.vertNormals.Add(Vector3.Transform(mesh.vertData.normals[v], mirrorMat));

                    if (mesh.vertData.uvDict.ContainsKey(VertexMagic.TEX0))
                    {
                        var uv1 = mesh.vertData.uvDict[VertexMagic.TEX0][v];
                        vtxl.uv1List.Add(new Vector2(uv1.X, uv1.Y));
                    }
                    if (mesh.vertData.uvDict.ContainsKey(VertexMagic.TEX1))
                    {
                        var uv2 = mesh.vertData.uvDict[VertexMagic.TEX1][v];
                        vtxl.uv2List.Add(new Vector2(uv2.X, uv2.Y));
                    }
                    if (mesh.vertData.uvDict.ContainsKey(VertexMagic.TEX2))
                    {
                        var uv3 = mesh.vertData.uvDict[VertexMagic.TEX2][v];
                        vtxl.uv3List.Add(new Vector2(uv3.X, uv3.Y));
                    }
                    if (mesh.vertData.uvDict.ContainsKey(VertexMagic.TEX3))
                    {
                        var uv4 = mesh.vertData.uvDict[VertexMagic.TEX3][v];
                        vtxl.uv4List.Add(new Vector2(uv4.X, uv4.Y));
                    }

                    /*
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
                    */

                    if (mesh.vertData.vertWeights.Count > 0)
                    {
                        vtxl.vertWeights.Add(mesh.vertData.vertWeights[v]);
                        vtxl.vertWeightIndices.Add(mesh.vertData.vertWeightIndices[v]);
                    }
                    else if (mesh.vertData.vertWeightIndices.Count > 0)
                    {
                        vtxl.vertWeights.Add(new Vector4(1, 0, 0, 0));
                        vtxl.vertWeightIndices.Add(new int[] { mesh.vertData.vertWeightIndices[v][0], 0, 0, 0 });
                    }
                }

                vtxl.convertToLegacyTypes();
                aqp.vtxeList.Add(AquaObjectMethods.ConstructClassicVTXE(vtxl, out int vc));
                aqp.vtxlList.Add(vtxl);

                //Face data
                AquaObject.GenericTriangles genMesh = new AquaObject.GenericTriangles();

                List<Vector3> triList = new List<Vector3>();
                for(int f = 0; f < mesh.faceData.faceList.Count; f++)
                {
                    var tri = mesh.faceData.faceList[f];
                    triList.Add(new Vector3(tri.X, tri.Y, tri.Z));
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
                mat.matName = $"Mat{i}";
                mat.texNames = new List<string>();
                mat.texNames.Add("test_d.dds");
                /*var flverMat = flver.Materials[mesh.MaterialIndex];
                mat.matName = flverMat.Name;
                mat.texNames = new List<string>();
                foreach (var tex in flverMat.Textures)
                {
                    mat.texNames.Add(Path.GetFileName(tex.Path));
                }*/
                aqp.tempMats.Add(mat);
            }

            return aqp;
        }

    }
}
