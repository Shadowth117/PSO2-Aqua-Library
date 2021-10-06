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
        public static void AssimpAQMConvert(string initialFilePath, bool playerExport, bool useScale)
        {
            Assimp.AssimpContext context = new Assimp.AssimpContext();
            context.SetConfig(new Assimp.Configs.FBXPreservePivotsConfig(false));
            Assimp.Scene aiScene = context.ImportFile(initialFilePath, Assimp.PostProcessSteps.Triangulate | Assimp.PostProcessSteps.JoinIdenticalVertices | Assimp.PostProcessSteps.FlipUVs);

            string inputFilename = Path.GetFileNameWithoutExtension(initialFilePath);
            List<string> nodeNames = new List<string>();
            List<string> aqmNames = new List<string>(); //Leave off extensions in case we want this to be .trm later
            List<AquaMotion> aqmList = new List<AquaMotion>();
            int animatedNodeCount = GetAnimatedNodeCount(aiScene, nodeNames);
            
            for(int i = 0; i < aiScene.Animations.Count; i++)
            {
                AquaMotion aqm = new AquaMotion();
                var anim = aiScene.Animations[i];

                if(anim.Name != null && anim.Name != "")
                {
                    //Make sure we're not overwriting anims that somehow have duplicate names
                    if(aqmNames.Contains(anim.Name))
                    {
                        aqmNames.Add($"Anim_{i}_" + anim.Name);
                    } else
                    {
                        aqmNames.Add(anim.Name);
                    }
                } else
                {
                    aqmNames.Add($"Anim_{i}_"  + inputFilename);
                }

                aqm.moHeader = new AquaMotion.MOHeader();

                //Get anim fps
                if (anim.TicksPerSecond == 0)
                {
                    //Default to 30
                    aqm.moHeader.frameSpeed = 30;
                } else
                {
                    aqm.moHeader.frameSpeed = (float)anim.TicksPerSecond;
                }

                aqm.moHeader.unkInt0 = 2; //Always, always 2 for NIFL
                aqm.moHeader.variant = 0x2; //These are flags for the animation to tell the game what type it is. Since this is a skeletal animation, we always put 2 here.
                //If it's a player one specifically, the game generally adds 0x10 to this.
                aqm.moHeader.nodeCount = animatedNodeCount;
                if (playerExport)
                {
                    aqm.moHeader.variant += 0x10;
                    aqm.moHeader.nodeCount++; //Add an extra for the nodeTreeFlag 'node'
                }
                aqm.moHeader.testString.SetString("test");

                //Set this ahead of time in case these are out of order
                aqm.motionKeys = new List<AquaMotion.KeyData>(new AquaMotion.KeyData[aqm.moHeader.nodeCount]);
                
                //Nodes
                foreach(var node in anim.NodeAnimationChannels)
                {

                }

                //NodeTreeFlag

                //Sanity check
                for (int nodeId = 0; nodeId < animatedNodeCount; nodeId++)
                {
                    var node = aqm.motionKeys[nodeId];

                    if(node == null)
                    {
                        node = aqm.motionKeys[nodeId] = new AquaMotion.KeyData();

                        node.mseg.nodeName.SetString($"FillerNode_{nodeId}"); //The actual name doesn't matter normally for these as long as they have data. Should be fine for fallback
                        node.mseg.nodeId = nodeId;
                        node.mseg.nodeType = 2;
                        node.mseg.nodeDataCount = useScale ? 3 : 2;

                        //Position
                        AquaMotion.MKEY posKeys = new AquaMotion.MKEY();
          //  ****            //TODO - GET bind position from bone
                        node.keyData.Add(posKeys);

                        //Rotation
                        AquaMotion.MKEY rotKeys = new AquaMotion.MKEY();
       //     ****            //TODO - GET bind rotation from bone
                        node.keyData.Add(rotKeys);

                        //Scale
                        if (useScale)
                        {
                            AquaMotion.MKEY sclKeys = new AquaMotion.MKEY();
                            sclKeys.keyType = 3;
                            sclKeys.dataType = 1;
                            sclKeys.keyCount = 1;
                            sclKeys.vector4Keys = new List<Vector4>() { new Vector4(1.0f, 1.0f, 1.0f, 0)};
                            node.keyData.Add(sclKeys);
                        }
                    }
                }

            }
        }

        public static string FilterAnimatedNodeName(string name)
        {
            return name.Substring(name.IndexOf(')') + 1);
        }

        public static int GetNodeNumber(string name)
        {
            string num = ((name.Split('('))[1].Split(')'))[0];
            
            if(Int32.TryParse(num, out int result))
            {
                return result;
            } else
            {
                return -1;
            }
        }

        public static int GetAnimatedNodeCount(Assimp.Scene aiScene, List<string> nodeNames)
        {
            nodeNames = new List<string>();
            int nodeCount = 0;
            foreach(var node in aiScene.RootNode.Children)
            {
                CollectAnimatedCount(node, ref nodeCount, nodeNames);
            }
            nodeNames.Sort();

            return nodeCount;
        }

        public static void CollectAnimatedCount(Assimp.Node node, ref int nodeCount, List<string> nodeNames)
        {
            //Be extra sure that this isn't an effect node
            if (IsAnimatedNode(node, out string name))
            {
                nodeCount++;

                //For now, assume animated node is numbered and we can add it directly
                nodeNames.Add(name);
            }

            foreach (var childNode in node.Children)
            {
                CollectAnimatedCount(childNode, ref nodeCount, nodeNames);
            }
        }

        //Check if a node is an animated by node by checking if it has the numbering formatting and is NOT marked as an effect node. Output name 
        public static bool IsAnimatedNode(Assimp.Node node, out string name)
        {
            bool isNumbered = IsNumberedAnimated(node, out name);
            return isNumbered && !node.Name.Contains("#Eff");
        }

        //Check if node is numbered as an animated node. Output name
        private static bool IsNumberedAnimated(Assimp.Node node, out string name)
        {
            name = node.Name;
            return node.Name[0] == '(' && node.Name.Contains(')');
        }

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
