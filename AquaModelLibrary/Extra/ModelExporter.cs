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

        //For now, we'll just assume we don't care about LOD models and export the first model stored
        /*
        public void ExportToDae(string filePath)
        {
            if(aquaModels.Count == 0 || aquaBones.Count == 0)
            {
                MessageBox.Show("You can't export a model without a skeleton!");
                return;
            }

            Assimp.Scene scene = new Assimp.Scene();

            if(aquaBones.Count != 0)
            {
                //Assign bones
                foreach (AquaNode.NODE bn in aquaBones[0].nodeList)
                {

                }

                foreach (AquaNode.NODO bn in aquaBones[0].nodoList)
                {
                    //NODOs are a bit more primitive. We need to generate the matrix for these ones.
                }
            }

            //Assign meshes and materials
            foreach (AquaObject.MESH msh in aquaModels[0].models[0].meshList)
            {
                //Mesh


                //Material
                var mat = aquaModels[0].models[0].mateList[msh.mateIndex];
                Assimp.Material mate = new Assimp.Material();

                mate.ColorDiffuse = new Assimp.Color4D(mat.diffuseRGBA.X, mat.diffuseRGBA.Y, mat.diffuseRGBA.Z, mat.diffuseRGBA.W);
                if (mat.alphaType.GetString().Equals("add"))
                {
                    mate.BlendMode = Assimp.BlendMode.Additive;
                }
                mate.Name = mat.matName.GetString();
                mate.TextureDiffuse = 
            }

        }*/
    }
}
