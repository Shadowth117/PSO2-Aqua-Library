using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace AquaModelLibrary
{
    public class ModelImporter
    {

        public static void AssimpPRMConvert(string initialFilePath, string finalFilePath)
        {
            Assimp.AssimpContext context = new Assimp.AssimpContext();
            context.SetConfig(new Assimp.Configs.FBXPreservePivotsConfig(false));
            Assimp.Scene aiScene = context.ImportFile(initialFilePath, Assimp.PostProcessSteps.Triangulate | Assimp.PostProcessSteps.JoinIdenticalVertices | Assimp.PostProcessSteps.FlipUVs);

            PRMModel prm = new PRMModel();

            int totalVerts = 0;

            //Iterate through and combine meshes. PRMs can only have a single mesh
            IterateAiNodesPRM(prm, ref totalVerts, aiScene, aiScene.RootNode, Matrix4x4.Transpose(GetMat4FromAssimpMat4(aiScene.RootNode.Transform)));
            AquaUtil.WritePRMToFile(prm, finalFilePath, 4);
        }

        private static void IterateAiNodesPRM(PRMModel prm, ref int totalVerts, Assimp.Scene aiScene, Assimp.Node node, Matrix4x4 parentTfm)
        {
            Matrix4x4 nodeMat = Matrix4x4.Transpose(GetMat4FromAssimpMat4(node.Transform));
            nodeMat = Matrix4x4.Multiply(nodeMat, parentTfm);

            foreach (int meshId in node.MeshIndices)
            {
                var mesh = aiScene.Meshes[meshId];
                AddAiMeshToPRM(prm, ref totalVerts, mesh, nodeMat);
            }

            foreach(var childNode in node.Children)
            {
                IterateAiNodesPRM(prm, ref totalVerts, aiScene, childNode, nodeMat);
            }
        }

        private static void AddAiMeshToPRM(PRMModel prm, ref int totalVerts, Assimp.Mesh aiMesh, Matrix4x4 nodeMat)
        {
            //Convert vertices
            for (int vertId = 0; vertId < aiMesh.VertexCount; vertId++)
            {
                PRMModel.PRMVert vert = new PRMModel.PRMVert();
                var aiPos = aiMesh.Vertices[vertId];
                var newPos = (new Vector3(aiPos.X, aiPos.Y, aiPos.Z));
                vert.pos = Vector3.Transform(newPos, nodeMat) / 100;

                if (aiMesh.HasVertexColors(0))
                {
                    var aiColor = aiMesh.VertexColorChannels[0][vertId];
                    vert.color = new byte[] { (byte)(aiColor.B * 255), (byte)(aiColor.G * 255), (byte)(aiColor.R * 255), (byte)(aiColor.A * 255) };
                }
                else
                {
                    vert.color = new byte[4];
                }

                if (aiMesh.HasNormals)
                {
                    var aiNorm = aiMesh.Normals[vertId];
                    var normal = new Vector3(aiNorm.X, aiNorm.Y, aiNorm.Z);
                    vert.normal = Vector3.TransformNormal(normal, nodeMat);
                }
                else
                {
                    vert.normal = new Vector3();
                }

                if (aiMesh.HasTextureCoords(0))
                {
                    var aiUV1 = aiMesh.TextureCoordinateChannels[0][vertId];
                    vert.uv1 = new Vector2(aiUV1.X, aiUV1.Y);
                }
                else
                {
                    vert.uv1 = new Vector2();
                }

                if (aiMesh.HasTextureCoords(1))
                {
                    var aiUV2 = aiMesh.TextureCoordinateChannels[1][vertId];
                    vert.uv2 = new Vector2(aiUV2.X, aiUV2.Y);
                }
                else
                {
                    vert.uv2 = new Vector2();
                }

                prm.vertices.Add(vert);
            }

            //Convert Faces
            foreach (var aiFace in aiMesh.Faces)
            {
                prm.faces.Add(new Vector3(aiFace.Indices[0] + totalVerts, aiFace.Indices[1] + totalVerts, aiFace.Indices[2] + totalVerts));
            }

            //Keep count up to date for next potential loop
            totalVerts = prm.vertices.Count;
        }

        public static Matrix4x4 GetMat4FromAssimpMat4(Assimp.Matrix4x4 mat4)
        {
            return new Matrix4x4(mat4.A1, mat4.A2, mat4.A3, mat4.A4,
                                 mat4.B1, mat4.B2, mat4.B3, mat4.B4,
                                 mat4.C1, mat4.C2, mat4.C3, mat4.C4,
                                 mat4.D1, mat4.D2, mat4.D3, mat4.D4);
        }
    }
}
