using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaNodeData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using System.Numerics;

namespace AquaModelLibrary.Core.General
{
    public static class AssimpModelExporter
    {
        public static Assimp.Scene AssimpExport(string filePath, AquaObject aqp, AquaNode aqn)
        {
            if (aqp.IsNGS)
            {
                //NGS aqps will give lots of isolated vertices if we don't handle them
                //Since we're not actually altering the data so much as rearranging references, we can just do this
                aqp = aqp.Clone();
                aqp.splitVSETPerMesh();
            }
            Assimp.Scene aiScene = new Assimp.Scene();

            //Create an array to hold references to these since Assimp lacks a way to grab these by order or id
            //We don't need the nodo count in this since they can't be parents
            Assimp.Node[] boneArray = new Assimp.Node[aqn.nodeList.Count];

            //Set up root node
            var root = aqn.nodeList[0];
            var aiRootNode = new Assimp.Node("RootNode", null);
            aiRootNode.Transform = Assimp.Matrix4x4.Identity;

            aiScene.RootNode = aiRootNode;

            //Assign bones
            for (int i = 0; i < aqn.nodeList.Count; i++)
            {
                var bn = aqn.nodeList[i];
                Assimp.Node parentNode;
                var parentTfm = Matrix4x4.Identity;
                if (bn.parentId == -1)
                {
                    parentNode = aiRootNode;
                }
                else
                {
                    parentNode = boneArray[bn.parentId];
                    var pn = aqn.nodeList[bn.parentId];
                    parentTfm = new Matrix4x4(pn.m1.X, pn.m1.Y, pn.m1.Z, pn.m1.W,
                                                pn.m2.X, pn.m2.Y, pn.m2.Z, pn.m2.W,
                                                pn.m3.X, pn.m3.Y, pn.m3.Z, pn.m3.W,
                                                pn.m4.X, pn.m4.Y, pn.m4.Z, pn.m4.W);
                }
                var aiNode = new Assimp.Node($"({i})" + bn.boneName.GetString(), parentNode);

                //Use inverse bind matrix as base
                var bnMat = new Matrix4x4(bn.m1.X, bn.m1.Y, bn.m1.Z, bn.m1.W,
                                            bn.m2.X, bn.m2.Y, bn.m2.Z, bn.m2.W,
                                            bn.m3.X, bn.m3.Y, bn.m3.Z, bn.m3.W,
                                            bn.m4.X, bn.m4.Y, bn.m4.Z, bn.m4.W);
                Matrix4x4.Invert(bnMat, out bnMat);

                //Get local transform
                aiNode.Transform = GetAssimpMat4(bnMat * parentTfm);

                parentNode.Children.Add(aiNode);
                boneArray[i] = aiNode;
            }

            foreach (NODO bn in aqn.nodoList)
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
            foreach (MESH msh in aqp.meshList)
            {
                List<uint> bonePalette = new List<uint>();
                var vtxl = aqp.vtxlList[msh.vsetIndex];

                //Mesh
                var aiMeshName = string.Format("mesh[{4}]_{0}_{1}_{2}_{3}_mesh", msh.mateIndex, msh.rendIndex, msh.shadIndex, msh.tsetIndex, aiScene.Meshes.Count);
                bool hasVertexWeights = aqp.vtxlList[msh.vsetIndex].vertWeightIndices.Count > 0;

                var aiMesh = new Assimp.Mesh(aiMeshName, Assimp.PrimitiveType.Triangle);

                if (hasVertexWeights)
                {
                    //Get bone palette
                    if (aqp.objc.bonePaletteOffset > 0)
                    {
                        bonePalette = aqp.bonePalette;
                    }
                    else
                    {
                        bonePalette = new List<uint>();
                        for (int bn = 0; bn < vtxl.bonePalette.Count; bn++)
                        {
                            bonePalette.Add(vtxl.bonePalette[bn]);
                        }
                    }
                }
                //Vertex face data - PSO2 Actually doesn't do this, it just has per vertex data so we can just map a vertice's data to each face using it
                //It may actually be possible to add this to the previous loop, but my reference didn't so I'm doing it in a separate loop for safety
                //Reference: https://github.com/TGEnigma/Amicitia/blob/master/Source/AmicitiaLibrary/Graphics/RenderWare/RWClumpNode.cs
                //UVs will have dummied data to ensure that if the game arbitrarily writes them, they will still be exported back in the same order
                for (int vertId = 0; vertId < vtxl.vertPositions.Count; vertId++)
                {
                    if (vtxl.vertPositions.Count > 0)
                    {
                        var pos = vtxl.vertPositions[vertId];
                        aiMesh.Vertices.Add(new Assimp.Vector3D(pos.X, pos.Y, pos.Z));
                    }

                    if (vtxl.vertNormals.Count > 0)
                    {
                        var nrm = vtxl.vertNormals[vertId];
                        aiMesh.Normals.Add(new Assimp.Vector3D(nrm.X, nrm.Y, nrm.Z));
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

                    if (vtxl.uv1List.Count > 0)
                    {
                        var textureCoordinate = vtxl.uv1List[vertId];
                        var aiTextureCoordinate = new Assimp.Vector3D(textureCoordinate.X, textureCoordinate.Y, 0f);
                        aiMesh.TextureCoordinateChannels[0].Add(aiTextureCoordinate);
                    }
                    else
                    {
                        var aiTextureCoordinate = new Assimp.Vector3D(1, 1, 1);
                        aiMesh.TextureCoordinateChannels[0].Add(aiTextureCoordinate);
                    }

                    if (vtxl.uv2List.Count > 0)
                    {
                        var textureCoordinate = vtxl.uv2List[vertId];
                        var aiTextureCoordinate = new Assimp.Vector3D(textureCoordinate.X, textureCoordinate.Y, 0f);
                        aiMesh.TextureCoordinateChannels[1].Add(aiTextureCoordinate);
                    }
                    else
                    {
                        var aiTextureCoordinate = new Assimp.Vector3D(1, 1, 1);
                        aiMesh.TextureCoordinateChannels[1].Add(aiTextureCoordinate);
                    }

                    if (vtxl.uv3List.Count > 0)
                    {
                        var textureCoordinate = vtxl.uv3List[vertId];
                        var aiTextureCoordinate = new Assimp.Vector3D(textureCoordinate.X, textureCoordinate.Y, 0f);
                        aiMesh.TextureCoordinateChannels[2].Add(aiTextureCoordinate);
                    }
                    else
                    {
                        var aiTextureCoordinate = new Assimp.Vector3D(1, 1, 1);
                        aiMesh.TextureCoordinateChannels[2].Add(aiTextureCoordinate);
                    }

                    if (vtxl.uv4List.Count > 0)
                    {
                        var textureCoordinate = vtxl.uv4List[vertId];
                        var aiTextureCoordinate = new Assimp.Vector3D(textureCoordinate.X, textureCoordinate.Y, 0f);
                        aiMesh.TextureCoordinateChannels[3].Add(aiTextureCoordinate);
                    }
                    else
                    {
                        var aiTextureCoordinate = new Assimp.Vector3D(1, 1, 1);
                        aiMesh.TextureCoordinateChannels[3].Add(aiTextureCoordinate);
                    }

                    if (vtxl.vert0x22.Count > 0)
                    {
                        var textureCoordinate = vtxl.vert0x22[vertId];
                        var aiTextureCoordinate = new Assimp.Vector3D(uvShortToFloat(textureCoordinate[0]), uvShortToFloat(textureCoordinate[1]), 0f);
                        aiMesh.TextureCoordinateChannels[4].Add(aiTextureCoordinate);
                    }
                    else
                    {
                        var aiTextureCoordinate = new Assimp.Vector3D(1, 1, 1);
                        aiMesh.TextureCoordinateChannels[4].Add(aiTextureCoordinate);
                    }

                    if (vtxl.vert0x23.Count > 0)
                    {
                        var textureCoordinate = vtxl.vert0x23[vertId];
                        var aiTextureCoordinate = new Assimp.Vector3D(uvShortToFloat(textureCoordinate[0]), uvShortToFloat(textureCoordinate[1]), 0f);
                        aiMesh.TextureCoordinateChannels[5].Add(aiTextureCoordinate);
                    }
                    else
                    {
                        var aiTextureCoordinate = new Assimp.Vector3D(1, 1, 1);
                        aiMesh.TextureCoordinateChannels[5].Add(aiTextureCoordinate);
                    }

                    if (vtxl.vert0x24.Count > 0)
                    {
                        var textureCoordinate = vtxl.vert0x24[vertId];
                        var aiTextureCoordinate = new Assimp.Vector3D(uvShortToFloat(textureCoordinate[0]), uvShortToFloat(textureCoordinate[1]), 0f);
                        aiMesh.TextureCoordinateChannels[6].Add(aiTextureCoordinate);
                    }
                    else
                    {
                        var aiTextureCoordinate = new Assimp.Vector3D(1, 1, 1);
                        aiMesh.TextureCoordinateChannels[6].Add(aiTextureCoordinate);
                    }

                    if (vtxl.vert0x25.Count > 0)
                    {
                        var textureCoordinate = vtxl.vert0x25[vertId];
                        var aiTextureCoordinate = new Assimp.Vector3D(uvShortToFloat(textureCoordinate[0]), uvShortToFloat(textureCoordinate[1]), 0f);
                        aiMesh.TextureCoordinateChannels[7].Add(aiTextureCoordinate);
                    }
                    else
                    {
                        var aiTextureCoordinate = new Assimp.Vector3D(1, 1, 1);
                        aiMesh.TextureCoordinateChannels[7].Add(aiTextureCoordinate);
                    }
                }

                for (int uv = 0; uv < aiMesh.TextureCoordinateChannelCount; uv++)
                {
                    aiMesh.UVComponentCount[uv] = 2;
                }

                //Assimp Bones - Assimp likes to store vertex weights in bones and bones references in meshes
                if (hasVertexWeights)
                {
                    var aiBoneMap = new Dictionary<int, Assimp.Bone>();

                    //Iterate through vertices
                    for (int vertId = 0; vertId < vtxl.vertWeightIndices.Count; vertId++)
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
                                var rawBone = aqn.nodeList[(int)bonePalette[boneIndex]];

                                aiBone.Name = $"({bonePalette[boneIndex]})" + rawBone.boneName.GetString();
                                aiBone.VertexWeights.Add(new Assimp.VertexWeight(vertId, boneWeight));

                                var invTransform = new Assimp.Matrix4x4(rawBone.m1.X, rawBone.m2.X, rawBone.m3.X, rawBone.m4.X,
                                                                     rawBone.m1.Y, rawBone.m2.Y, rawBone.m3.Y, rawBone.m4.Y,
                                                                     rawBone.m1.Z, rawBone.m2.Z, rawBone.m3.Z, rawBone.m4.Z,
                                                                     rawBone.m1.W, rawBone.m2.W, rawBone.m3.W, rawBone.m4.W);

                                aiBone.OffsetMatrix = invTransform;

                                aiBoneMap[boneIndex] = aiBone;
                            }

                            if (!aiBoneMap[boneIndex].VertexWeights.Any(x => x.VertexID == vertId))
                                aiBoneMap[boneIndex].VertexWeights.Add(new Assimp.VertexWeight(vertId, boneWeight));
                        }
                    }

                    //Add the bones to the mesh
                    aiMesh.Bones.AddRange(aiBoneMap.Values);
                }
                else //Handle rigid meshes
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

                //Faces
                foreach (var face in aqp.strips[msh.vsetIndex].GetTriangles(true))
                {
                    aiMesh.Faces.Add(new Assimp.Face(new int[] { (int)face.X, (int)face.Y, (int)face.Z }));
                }

                //Material
                var mat = aqp.mateList[msh.mateIndex];
                var shaderSet = aqp.GetShaderNames(msh.shadIndex);
                var textureSet = aqp.GetTexListNamesUnicode(msh.tsetIndex);
                Assimp.Material mate = new Assimp.Material();

                mate.ColorDiffuse = new Assimp.Color4D(mat.diffuseRGBA.X, mat.diffuseRGBA.Y, mat.diffuseRGBA.Z, mat.diffuseRGBA.W);
                if (mat.alphaType.GetString().Equals("add"))
                {
                    mate.BlendMode = Assimp.BlendMode.Additive;
                }
                mate.Name = "(" + shaderSet[0] + "," + shaderSet[1] + ")" + "{" + mat.alphaType.GetString() + "}" + mat.matName.GetString();

                //Set textures - PSO2 Texture slots are NOT consistent and depend entirely on the selected shader. As such, slots will be somewhat arbitrary after albedo/diffuse
                for (int i = 0; i < textureSet.Count; i++)
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


                var meshNodeName = string.Format("mesh[{4}]_{0}_{1}_{2}_{3}#{4}#{5}", msh.mateIndex, msh.rendIndex, msh.shadIndex, msh.tsetIndex, aiScene.Meshes.Count, msh.baseMeshNodeId, msh.baseMeshDummyId);

                // Add mesh to meshes
                aiScene.Meshes.Add(aiMesh);

                // Add material to materials
                aiScene.Materials.Add(mate);

                // MaterialIndex
                aiMesh.MaterialIndex = aiScene.Materials.Count - 1;

                // Set up mesh node and add this mesh's index to it (This tells assimp to export it as a mesh for various formats)
                var meshNode = new Assimp.Node(meshNodeName, aiScene.RootNode);
                meshNode.Transform = Assimp.Matrix4x4.Identity;

                aiScene.RootNode.Children.Add(meshNode);

                meshNode.MeshIndices.Add(aiScene.Meshes.Count - 1);
            }

            return aiScene;
        }

        public static Assimp.Scene AssimpPRMExport(string filePath, PRM prm)
        {
            Assimp.Scene aiScene = new Assimp.Scene();

            //Create an array to hold references to these since Assimp lacks a way to grab these by order or id
            //We don't need the nodo count in this since they can't be parents
            Assimp.Node[] boneArray = new Assimp.Node[2];

            //Set up root node
            var aiRootNode = new Assimp.Node("RootNode", null);
            aiRootNode.Transform = Assimp.Matrix4x4.Identity;

            boneArray[0] = aiRootNode;
            aiScene.RootNode = aiRootNode;

            //Set up single child node
            var aiNode = new Assimp.Node(Path.GetFileNameWithoutExtension(filePath) + "_node", aiRootNode);

            //Use inverse bind matrix as base

            //Get local transform
            aiNode.Transform = aiRootNode.Transform;

            aiRootNode.Children.Add(aiNode);
            boneArray[1] = aiNode;

            //Mesh
            string aiMeshName = Path.GetFileNameWithoutExtension(filePath);

            var aiMesh = new Assimp.Mesh(aiMeshName, Assimp.PrimitiveType.Triangle);

            //Vertex face data - PSO2 Actually doesn't do this, it just has per vertex data so we can just map a vertice's data to each face using it
            //It may actually be possible to add this to the previous loop, but my reference didn't so I'm doing it in a separate loop for safety
            //Reference: https://github.com/TGEnigma/Amicitia/blob/master/Source/AmicitiaLibrary/Graphics/RenderWare/RWClumpNode.cs
            for (int vertId = 0; vertId < prm.vertices.Count; vertId++)
            {
                var prmVert = prm.vertices[vertId];

                var pos = prmVert.pos;
                aiMesh.Vertices.Add(new Assimp.Vector3D(pos.X, pos.Y, pos.Z));

                var nrm = prmVert.normal;
                aiMesh.Normals.Add(new Assimp.Vector3D(nrm.X, nrm.Y, nrm.Z));

                //Vert colors are bgra
                var rawClr = prmVert.color;
                var clr = new Assimp.Color4D(clrToFloat(rawClr[2]), clrToFloat(rawClr[1]), clrToFloat(rawClr[0]), clrToFloat(rawClr[3]));
                aiMesh.VertexColorChannels[0].Add(clr);

                var uv1 = prmVert.uv1;
                var aiUV1 = new Assimp.Vector3D(uv1.X, uv1.Y, 0f);
                aiMesh.TextureCoordinateChannels[0].Add(aiUV1);


                var uv2 = prmVert.uv2;
                var aiUV2 = new Assimp.Vector3D(uv2.X, uv2.Y, 0f);
                aiMesh.TextureCoordinateChannels[1].Add(aiUV2);

            }

            //Handle rigid meshes
            {
                var aiBone = new Assimp.Bone();
                var aqnBone = boneArray[0];

                // Name
                aiBone.Name = aiNode.Name;

                // VertexWeights
                for (int i = 0; i < aiMesh.Vertices.Count; i++)
                {
                    var aiVertexWeight = new Assimp.VertexWeight(i, 1f);
                    aiBone.VertexWeights.Add(aiVertexWeight);
                }

                aiBone.OffsetMatrix = Assimp.Matrix4x4.Identity;

                aiMesh.Bones.Add(aiBone);
            }

            //Faces
            foreach (var face in prm.faces)
            {
                aiMesh.Faces.Add(new Assimp.Face(new int[] { (int)face.X, (int)face.Y, (int)face.Z }));
            }

            //Material
            Assimp.Material mate = new Assimp.Material();

            mate.ColorDiffuse = new Assimp.Color4D(1, 1, 1, 1);
            mate.Name = aiMeshName + "_material";

            mate.ShadingMode = Assimp.ShadingMode.Phong;

            var meshNodeName = Path.GetFileNameWithoutExtension(filePath);

            // Add mesh to meshes
            aiScene.Meshes.Add(aiMesh);

            // Add material to materials
            aiScene.Materials.Add(mate);

            // MaterialIndex
            aiMesh.MaterialIndex = aiScene.Materials.Count - 1;

            // Set up mesh node and add this mesh's index to it (This tells assimp to export it as a mesh for various formats)
            var meshNode = new Assimp.Node(meshNodeName, aiScene.RootNode);
            meshNode.Transform = Assimp.Matrix4x4.Identity;

            aiScene.RootNode.Children.Add(meshNode);

            meshNode.MeshIndices.Add(aiScene.Meshes.Count - 1);


            return aiScene;
        }

        public static Assimp.Matrix4x4 GetAssimpMat4(Matrix4x4 mat4)
        {
            return new Assimp.Matrix4x4(mat4.M11, mat4.M21, mat4.M31, mat4.M41,
                                        mat4.M12, mat4.M22, mat4.M32, mat4.M42,
                                        mat4.M13, mat4.M23, mat4.M33, mat4.M43,
                                        mat4.M14, mat4.M24, mat4.M34, mat4.M44);
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

        public static float uvShortToFloat(short sht)
        {
            float flt = sht;
            return flt / 32767;
        }
    }
}
