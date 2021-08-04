using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Numerics;
using SharpGLTF.Schema2;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using System.Linq;

namespace AquaModelLibrary
{
    public static class ModelExporter
    {
        public static void getGLTF(string filePath)
        {
            var model = ModelRoot.Load(filePath);

            var a = model;
        }

        public static void ToGLTF(string filePath, AquaObject model)
        {
            var gltfModel = ModelRoot.CreateModel();
            var scene = gltfModel.UseScene(Path.GetFileName(filePath));

            //gltfModel.

            //Handle NGS models
            //We can leave this alone, but if we do, we get isolated vertices.
            if (model.objc.type > 0xC2A)
            {
                model.splitVSETPerMesh();
            }

            var mesh = gltfModel.CreateMeshes(ConvertToGLTFMeshes(model)).First();

            gltfModel.SaveGLB(filePath);
        }

        public static MeshBuilder<VertexPositionNormal, VertexColor2Texture2, VertexJoints4>[] ConvertToGLTFMeshes(AquaObject model)
        {
            var gltfMeshes = new MeshBuilder<VertexPositionNormal, VertexColor2Texture2, VertexJoints4>[model.meshList.Count];
            for (int i = 0; i < model.meshList.Count; i++)
            {
                var gltfMesh = new MeshBuilder<VertexPositionNormal, VertexColor2Texture2, VertexJoints4>();

                var mesh = model.meshList[i];
                var mate = model.mateList[mesh.mateIndex];
                var rend = model.rendList[mesh.rendIndex];
                var shad = model.shadList[mesh.shadIndex];
                var tris = model.strips[mesh.psetIndex].GetTriangles(true);
                var vset = model.vsetList[mesh.vsetIndex];
                var vtxl = model.vtxlList[mesh.vsetIndex];

                string meshName = "mesh" + "[" + i + "]" + "_" + model.meshList[i].mateIndex + "_" + model.meshList[i].rendIndex
                    + "_" + model.meshList[i].shadIndex + "_" + model.meshList[i].tsetIndex + "@" + mesh.baseMeshNodeId;
                gltfMesh.Name = meshName;

                var matName = $"({shad.pixelShader.GetString()},{shad.vertexShader.GetString()})"
                    + "{" + mate.alphaType.GetString() + "}" + $"{mate.matName.GetString()}";
                var material = new MaterialBuilder(matName).WithDoubleSide(rend.twosided > 0)
                    .WithAlpha(SharpGLTF.Materials.AlphaMode.BLEND, rend.alphaCutoff).WithChannelParam("BaseColor", mate.diffuseRGBA);


                var meshData = gltfMesh.UsePrimitive(material);

                //Gather vertexData
                var verts = new List<VertexBuilder<VertexPositionNormal, VertexColor2Texture2, VertexJoints4>>();
                for (int v = 0; v < vtxl.vertPositions.Count; v++)
                {
                    IVertexGeometry posNrm;
                    VertexColor2Texture2 colorUv;
                    VertexJoints4 weights;

                    //Conditionally combine these attributes

                    //Check for normals
                    if (vtxl.vertNormals.Count > 0)
                    {
                        posNrm = new VertexPositionNormal(vtxl.vertPositions[v], vtxl.vertNormals[v]);
                    }
                    else
                    {
                        posNrm = new VertexPosition(vtxl.vertPositions[v]);
                    }

                    //Check vert colors and UVs. Unfortunately, we're limited to 2 of each type here and NGS models may suffer
                    Vector2 uv1 = new Vector2();
                    Vector2 uv2 = new Vector2();
                    Vector4 color = new Vector4();
                    Vector4 color2 = new Vector4();
                    if (vtxl.uv1List.Count > 0)
                    {
                        uv1 = vtxl.uv1List[v];
                    }
                    if (vtxl.uv2List.Count > 0)
                    {
                        uv2 = vtxl.uv2List[v];
                    }
                    if (vtxl.vertColors.Count > 0)
                    {
                        //PSO2 colors are BGRA bytes for vertices
                        var vc = vtxl.vertColors[v];
                        color.X = ((float)vc[2]) / 255;
                        color.Y = ((float)vc[1]) / 255;
                        color.Z = ((float)vc[0]) / 255;
                        color.W = ((float)vc[3]) / 255;
                    }
                    if (vtxl.vertColor2s.Count > 0)
                    {
                        //PSO2 colors are BGRA bytes for vertices
                        var vc = vtxl.vertColor2s[v];
                        color2.X = ((float)vc[2]) / 255;
                        color2.Y = ((float)vc[1]) / 255;
                        color2.Z = ((float)vc[0]) / 255;
                        color2.W = ((float)vc[3]) / 255;
                    }
                    colorUv = new VertexColor2Texture2(color, color2, uv1, uv2);

                    //Check for weights
                    var tupleSkin = new List<(int, float)>();
                    if (vtxl.trueVertWeights.Count > 0)
                    {
                        for (int w = 0; w < vtxl.trueVertWeightIndices[v].Length && w < 4; w++)
                        {
                            float weight = 0;
                            switch (w)
                            {
                                case 0:
                                    weight = vtxl.trueVertWeights[v].X;
                                    break;
                                case 1:
                                    weight = vtxl.trueVertWeights[v].Y;
                                    break;
                                case 2:
                                    weight = vtxl.trueVertWeights[v].Z;
                                    break;
                                case 3:
                                    weight = vtxl.trueVertWeights[v].W;
                                    break;
                            }
                            tupleSkin.Add((vtxl.trueVertWeightIndices[v][w], weight));
                        }
                    }
                    weights = new VertexJoints4(tupleSkin.ToArray());

                    var vert = new VertexBuilder<VertexPositionNormal, VertexColor2Texture2, VertexJoints4>(
                        new VertexPositionNormal(vtxl.vertPositions[v], vtxl.vertNormals[v]), colorUv, weights);
                    verts.Add(vert);
                }

                //Set triangles
                for (var t = 0; t < tris.Count; t++)
                {
                    meshData.AddTriangle(verts[(int)tris[t].X], verts[(int)tris[t].Y], verts[(int)tris[t].Z]);
                }
                gltfMeshes[i] = gltfMesh;
            }

            return gltfMeshes;
        }

        public static Assimp.Scene AssimpExport(string filePath, AquaObject aqp, AquaNode aqn)
        {
            if(aqp is NGSAquaObject)
            {
                //NGS aqps will give lots of isolated vertices if we don't handle them
                //Since we're not actually altering the data so much as rearranging references, we can just do this
                aqp = aqp.getShallowCopy();
                aqp.splitVSETPerMesh();
            }
            Assimp.Scene aiScene = new Assimp.Scene();

            //Create an array to hold references to these since Assimp lacks a way to grab these by order or id
            //We don't need the nodo count in this since they can't be parents
            Assimp.Node[] boneArray = new Assimp.Node[aqn.nodeList.Count]; 

            //Set up root node
            var root = aqn.nodeList[0];
            var aiRootNode = new Assimp.Node("({0})" + root.boneName.GetString(), null);
            aiRootNode.Transform = new Assimp.Matrix4x4(root.m1.X, root.m1.Y, root.m1.Z, root.m1.W,
                                                         root.m2.X, root.m2.Y, root.m2.Z, root.m2.W,
                                                         root.m3.X, root.m3.Y, root.m3.Z, root.m3.W,
                                                         root.m4.X, root.m4.Y, root.m4.Z, root.m4.W);

            aiScene.RootNode = aiRootNode;
            boneArray[0] = aiRootNode;

            //Assign bones
            for (int i = 0; i < aqn.nodeList.Count; i++)
            {
                var bn = aqn.nodeList[i];
                if(bn.parentId == -1)
                {
                    continue;
                }
                var parentNode = boneArray[bn.parentId];
                var aiNode = new Assimp.Node($"({i})" + bn.boneName.GetString(), parentNode);
                aiNode.Transform = new Assimp.Matrix4x4(bn.m1.X, bn.m1.Y, bn.m1.Z, bn.m1.W,
                                                         bn.m2.X, bn.m2.Y, bn.m2.Z, bn.m2.W,
                                                         bn.m3.X, bn.m3.Y, bn.m3.Z, bn.m3.W,
                                                         bn.m4.X, bn.m4.Y, bn.m4.Z, bn.m4.W);
                parentNode.Children.Add(aiNode);
                boneArray[i] = aiNode;
            }

            foreach (AquaNode.NODO bn in aqn.nodoList)
            {
                var parentNodo = boneArray[bn.parentId];
                var aiNode = new Assimp.Node(bn.boneName.GetString(), parentNodo);

                //NODOs are a bit more primitive. We need to generate the matrix for these ones.
                var matrix = Assimp.Matrix4x4.Identity;
                var rotation = Assimp.Matrix4x4.FromRotationX(bn.eulRot.X) *
                   Assimp.Matrix4x4.FromRotationY(bn.eulRot.Y) *
                   Assimp.Matrix4x4.FromRotationZ(bn.eulRot.Z);

                matrix *= rotation;
                matrix *= Assimp.Matrix4x4.FromTranslation(new Assimp.Vector3D(bn.pos.X, bn.pos.Y, bn.pos.Z));
                aiNode.Transform = matrix;

                parentNodo.Children.Add(aiNode);
            }

            //Assign meshes and materials
            foreach (AquaObject.MESH msh in aqp.meshList)
            {
                var vtxl = aqp.vtxlList[msh.vsetIndex];

                //Mesh
                var aiMeshName = string.Format("mesh_{0}_{1}_{2}_{3}", msh.mateIndex, msh.rendIndex, msh.shadIndex, msh.tsetIndex);
                bool hasVertexWeights = aqp.vtxlList[msh.vsetIndex].vertWeightIndices.Count > 0;

                var aiMesh = new Assimp.Mesh(aiMeshName, Assimp.PrimitiveType.Triangle);

                //Faces
                foreach(var face in aqp.strips[msh.psetIndex].GetTriangles(true))
                {
                    aiMesh.Faces.Add(new Assimp.Face(new int[] { (int)face.X, (int)face.Y, (int)face.Z }));
                }

                //Vertex face data - PSO2 Actually doesn't do this, it just has per vertex data so we can just map a vertice's data to each face using it
                //It may actually be possible to add this to the previous loop, but my reference didn't so I'm doing it in a separate loop for safety
                //Reference: https://github.com/TGEnigma/Amicitia/blob/master/Source/AmicitiaLibrary/Graphics/RenderWare/RWClumpNode.cs
                foreach (var face in aqp.strips[msh.psetIndex].GetTriangles(true))
                {
                    var faceVerts = new int[] { (int)face.X, (int)face.Y, (int)face.Z};
                    foreach(var vertId in faceVerts)
                    {
                        if(vtxl.vertPositions.Count > 0)
                        {
                            var pos = vtxl.vertPositions[vertId];
                            aiMesh.Vertices.Add(new Assimp.Vector3D(pos.X, pos.Y, pos.Z));
                        }

                        if(vtxl.vertNormals.Count > 0)
                        {
                            var nrm = vtxl.vertNormals[vertId];
                            aiMesh.Vertices.Add(new Assimp.Vector3D(nrm.X, nrm.Y, nrm.Z));
                        }

                        if (vtxl.vertColors.Count > 0)
                        {
                            //Vert colors are bgra
                            var rawClr = vtxl.vertColors[vertId];
                            var clr = new Assimp.Color4D(clrToFloat(rawClr[2]), clrToFloat(rawClr[1]), clrToFloat(rawClr[0]), clrToFloat(rawClr[3]));
                            aiMesh.VertexColorChannels[0].Add(clr);
                        }

                        if (vtxl.vertColor2s.Count > 0)
                        {
                            //Vert colors are bgra
                            var rawClr = vtxl.vertColor2s[vertId];
                            var clr = new Assimp.Color4D(clrToFloat(rawClr[2]), clrToFloat(rawClr[1]), clrToFloat(rawClr[0]), clrToFloat(rawClr[3]));
                            aiMesh.VertexColorChannels[1].Add(clr);
                        }

                        if(vtxl.uv1List.Count > 0)
                        {
                            var textureCoordinate = vtxl.uv1List[vertId];
                            var aiTextureCoordinate = new Assimp.Vector3D(textureCoordinate.X, textureCoordinate.Y, 0f);
                            aiMesh.TextureCoordinateChannels[0].Add(aiTextureCoordinate);
                        }

                        if (vtxl.uv2List.Count > 0)
                        {
                            var textureCoordinate = vtxl.uv2List[vertId];
                            var aiTextureCoordinate = new Assimp.Vector3D(textureCoordinate.X, textureCoordinate.Y, 0f);
                            aiMesh.TextureCoordinateChannels[1].Add(aiTextureCoordinate);
                        }

                        if (vtxl.uv3List.Count > 0)
                        {
                            var textureCoordinate = vtxl.uv3List[vertId];
                            var aiTextureCoordinate = new Assimp.Vector3D(textureCoordinate.X, textureCoordinate.Y, 0f);
                            aiMesh.TextureCoordinateChannels[2].Add(aiTextureCoordinate);
                        }

                        if (vtxl.uv4List.Count > 0)
                        {
                            var textureCoordinate = vtxl.uv4List[vertId];
                            var aiTextureCoordinate = new Assimp.Vector3D(textureCoordinate.X, textureCoordinate.Y, 0f);
                            aiMesh.TextureCoordinateChannels[3].Add(aiTextureCoordinate);
                        }

                        if (vtxl.vert0x22.Count > 0)
                        {
                            var textureCoordinate = vtxl.vert0x22[vertId];
                            var aiTextureCoordinate = new Assimp.Vector3D(shortToFloat(textureCoordinate[0]), shortToFloat(textureCoordinate[1]), 0f);
                            aiMesh.TextureCoordinateChannels[4].Add(aiTextureCoordinate);
                        }

                        if (vtxl.vert0x23.Count > 0)
                        {
                            var textureCoordinate = vtxl.vert0x23[vertId];
                            var aiTextureCoordinate = new Assimp.Vector3D(shortToFloat(textureCoordinate[0]), shortToFloat(textureCoordinate[1]), 0f);
                            aiMesh.TextureCoordinateChannels[5].Add(aiTextureCoordinate);
                        }
                    }
                }

                //Assimp Bones - Assimp likes to store vertex weights in bones and bones references in meshes
                if(hasVertexWeights)
                {
                    //Get bone palette
                    List<uint> bonePalette;
                    if (aqp.objc.bonePaletteOffset > 0)
                    {
                        bonePalette = aqp.bonePalette;
                    } else
                    {
                        bonePalette = new List<uint>();
                        for(int bn = 0; bn < vtxl.bonePalette.Count; bn++)
                        {
                            bonePalette.Add(vtxl.bonePalette[bn]);
                        }
                    }
                    var aiBoneMap = new Dictionary<int, Assimp.Bone>();

                    //Iterate through vertices
                    for(int vertId = 0; vertId < vtxl.vertWeightIndices.Count; vertId++)
                    {
                        var boneIndices = vtxl.vertWeightIndices[vertId];
                        var boneWeights = Vector4ToFloatArray(vtxl.vertWeights[vertId]);

                        //Iterate through weights
                        for (int wt = 0; wt < 4; wt++)
                        {
                            var boneIndex = boneIndices[wt];
                            var boneWeight = boneWeights[wt];

                            if (boneWeight == 0.0f)
                                continue;

                            if (!aiBoneMap.Keys.Contains(boneIndex))
                            {
                                var aiBone = new Assimp.Bone();
                                var aqnBone = boneArray[bonePalette[boneIndex]];

                                aiBone.Name = aqnBone.Name;
                                aiBone.VertexWeights.Add(new Assimp.VertexWeight(vertId, boneWeight));

                                var transform = new Assimp.Matrix4x4(aqnBone.Transform.A1, aqnBone.Transform.A2, aqnBone.Transform.A3, aqnBone.Transform.A4,
                                    aqnBone.Transform.B1, aqnBone.Transform.B2, aqnBone.Transform.B3, aqnBone.Transform.B4,
                                    aqnBone.Transform.C1, aqnBone.Transform.C2, aqnBone.Transform.C3, aqnBone.Transform.C4,
                                    aqnBone.Transform.D1, aqnBone.Transform.D2, aqnBone.Transform.D3, aqnBone.Transform.D4);
                                transform.Inverse();
                                aiBone.OffsetMatrix = transform;

                                aiBoneMap[boneIndex] = aiBone;
                            }

                            if (!aiBoneMap[boneIndex].VertexWeights.Any(x => x.VertexID == vertId))
                                aiBoneMap[boneIndex].VertexWeights.Add(new Assimp.VertexWeight(vertId, boneWeight));
                        }
                    }

                    //Add the bones to the mesh
                    aiMesh.Bones.AddRange(aiBoneMap.Values);
                } else //Handle rigid meshes
                {
                    var aiBone = new Assimp.Bone();
                    var aqnBone = boneArray[msh.baseMeshNodeId];

                    // Name
                    aiBone.Name = aqnBone.Name;

                    // VertexWeights
                    for (int i = 0; i < aiMesh.Vertices.Count; i++)
                    {
                        var aiVertexWeight = new Assimp.VertexWeight(i, 1f);
                        aiBone.VertexWeights.Add(aiVertexWeight);
                    }

                    aiBone.OffsetMatrix = Assimp.Matrix4x4.Identity;

                    aiMesh.Bones.Add(aiBone);
                }

                //Material
                var mat = aqp.mateList[msh.mateIndex];
                var shaderSet = AquaObjectMethods.GetShaderNames(aqp, msh.shadIndex);
                var textureSet = AquaObjectMethods.GetTexListNames(aqp, msh.tsetIndex);
                Assimp.Material mate = new Assimp.Material();

                mate.ColorDiffuse = new Assimp.Color4D(mat.diffuseRGBA.X, mat.diffuseRGBA.Y, mat.diffuseRGBA.Z, mat.diffuseRGBA.W);
                if (mat.alphaType.GetString().Equals("add"))
                {
                    mate.BlendMode = Assimp.BlendMode.Additive;
                }
                mate.Name = "(" + shaderSet[0] + "," + shaderSet[1] + ")" + "{" + mat.alphaType.GetString() + "}" + mat.matName.GetString();
                
                //Set textures - PSO2 Texture slots are NOT consistent and depend entirely on the selected shader. As such, slots will be somewhat arbitrary after albedo/diffuse
                for(int i = 0; i < textureSet.Count; i++)
                {
                    switch (i)
                    {
                        case 0:
                            mate.TextureDiffuse = new Assimp.TextureSlot(
                                textureSet[i], Assimp.TextureType.Diffuse, i, Assimp.TextureMapping.FromUV, aqp.tstaList[aqp.tsetList[msh.tsetIndex].tstaTexIDs[i]].modelUVSet, 0, 
                                Assimp.TextureOperation.Add, Assimp.TextureWrapMode.Wrap, Assimp.TextureWrapMode.Wrap, 0);
                            break;
                        case 1:
                            mate.TextureSpecular = new Assimp.TextureSlot(
                                textureSet[i], Assimp.TextureType.Specular, i, Assimp.TextureMapping.FromUV, aqp.tstaList[aqp.tsetList[msh.tsetIndex].tstaTexIDs[i]].modelUVSet, 0,
                                Assimp.TextureOperation.Add, Assimp.TextureWrapMode.Wrap, Assimp.TextureWrapMode.Wrap, 0);
                            break;
                        case 2:
                            mate.TextureNormal = new Assimp.TextureSlot(
                                textureSet[i], Assimp.TextureType.Normals, i, Assimp.TextureMapping.FromUV, aqp.tstaList[aqp.tsetList[msh.tsetIndex].tstaTexIDs[i]].modelUVSet, 0,
                                Assimp.TextureOperation.Add, Assimp.TextureWrapMode.Wrap, Assimp.TextureWrapMode.Wrap, 0);
                            break;
                        case 3:
                            mate.TextureLightMap = new Assimp.TextureSlot(
                                textureSet[i], Assimp.TextureType.Lightmap, i, Assimp.TextureMapping.FromUV, aqp.tstaList[aqp.tsetList[msh.tsetIndex].tstaTexIDs[i]].modelUVSet, 0,
                                Assimp.TextureOperation.Add, Assimp.TextureWrapMode.Wrap, Assimp.TextureWrapMode.Wrap, 0);
                            break;
                        case 4:
                            mate.TextureDisplacement = new Assimp.TextureSlot(
                                textureSet[i], Assimp.TextureType.Displacement, i, Assimp.TextureMapping.FromUV, aqp.tstaList[aqp.tsetList[msh.tsetIndex].tstaTexIDs[i]].modelUVSet, 0,
                                Assimp.TextureOperation.Add, Assimp.TextureWrapMode.Wrap, Assimp.TextureWrapMode.Wrap, 0);
                            break;
                        case 5:
                            mate.TextureOpacity = new Assimp.TextureSlot(
                                textureSet[i], Assimp.TextureType.Opacity, i, Assimp.TextureMapping.FromUV, aqp.tstaList[aqp.tsetList[msh.tsetIndex].tstaTexIDs[i]].modelUVSet, 0,
                                Assimp.TextureOperation.Add, Assimp.TextureWrapMode.Wrap, Assimp.TextureWrapMode.Wrap, 0);
                            break;
                        case 6:
                            mate.TextureHeight = new Assimp.TextureSlot(
                                textureSet[i], Assimp.TextureType.Height, i, Assimp.TextureMapping.FromUV, aqp.tstaList[aqp.tsetList[msh.tsetIndex].tstaTexIDs[i]].modelUVSet, 0,
                                Assimp.TextureOperation.Add, Assimp.TextureWrapMode.Wrap, Assimp.TextureWrapMode.Wrap, 0);
                            break;
                        case 7:
                            mate.TextureEmissive = new Assimp.TextureSlot(
                                textureSet[i], Assimp.TextureType.Emissive, i, Assimp.TextureMapping.FromUV, aqp.tstaList[aqp.tsetList[msh.tsetIndex].tstaTexIDs[i]].modelUVSet, 0,
                                Assimp.TextureOperation.Add, Assimp.TextureWrapMode.Wrap, Assimp.TextureWrapMode.Wrap, 0);
                            break;
                        case 8:
                            mate.TextureAmbient = new Assimp.TextureSlot(
                                textureSet[i], Assimp.TextureType.Ambient, i, Assimp.TextureMapping.FromUV, aqp.tstaList[aqp.tsetList[msh.tsetIndex].tstaTexIDs[i]].modelUVSet, 0,
                                Assimp.TextureOperation.Add, Assimp.TextureWrapMode.Wrap, Assimp.TextureWrapMode.Wrap, 0);
                            break;
                        case 9:
                            mate.TextureReflection = new Assimp.TextureSlot(
                                textureSet[i], Assimp.TextureType.Reflection, i, Assimp.TextureMapping.FromUV, aqp.tstaList[aqp.tsetList[msh.tsetIndex].tstaTexIDs[i]].modelUVSet, 0,
                                Assimp.TextureOperation.Add, Assimp.TextureWrapMode.Wrap, Assimp.TextureWrapMode.Wrap, 0);
                            break;
                        default:
                            break;
                    }
                }

                mate.ShadingMode = Assimp.ShadingMode.Phong;

                // Add mesh to meshes
                aiScene.Meshes.Add(aiMesh);

                // Add material to materials
                aiScene.Materials.Add(mate);

                // MaterialIndex
                aiMesh.MaterialIndex = aiScene.Materials.Count - 1;

                // Add mesh index to node
                aiRootNode.MeshIndices.Add(aiScene.Meshes.Count - 1);
            }

            return aiScene;
        }

        public static float[] Vector4ToFloatArray(Vector4 vector4)
        {
            return new float[] { vector4.X, vector4.Y, vector4.Z, vector4.W };
        }

        public static float clrToFloat(byte bt)
        {
            float flt = bt;
            return flt / 255f;
        }

        public static float shortToFloat(short sht)
        {
            float flt = sht;
            return flt / 255f;
        }
    }
}
