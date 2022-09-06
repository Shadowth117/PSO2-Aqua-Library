using AquaModelLibrary.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using static AquaModelLibrary.AquaObjectMethods;
using static AquaModelLibrary.Extra.MathExtras;

namespace AquaModelLibrary
{
    public class ModelImporter
    {
        public static Dictionary<char, char> illegalChars = new Dictionary<char, char>() {
            { '<', '[' },
            { '>', ']'},
            {':', '-'},
            {'"', '\''},
            {'/', '_'},
            {'\\', '_'},
            {'|', '_'},
            {'?', '_'},
            {'*', '_'},
        };

        public static string GetLastPipeString(string str)
        {
            var split = str.Split('|');
            return split[split.Length - 1];
        }

        public static string NixIllegalCharacters(string str)
        {
            foreach (var ch in illegalChars.Keys)
            {
                if (str.Contains(ch))
                {
                    str = str.Replace(ch, illegalChars[ch]);
                }
            }

            return str;
        }

        public static void AssimpAQMConvert(string initialFilePath, bool forceNoPlayerExport, bool useScaleFrames, float scaleFactor)
        {
            float baseScale = scaleFactor;
            Assimp.AssimpContext context = new Assimp.AssimpContext();
            context.SetConfig(new Assimp.Configs.FBXPreservePivotsConfig(false));
            Assimp.Scene aiScene = context.ImportFile(initialFilePath, Assimp.PostProcessSteps.Triangulate | Assimp.PostProcessSteps.JoinIdenticalVertices | Assimp.PostProcessSteps.FlipUVs);

            bool playerExport = aiScene.RootNode.Children[0].Name.Contains("pl_");
            if(playerExport && forceNoPlayerExport)
            {
                playerExport = false;
            }
            string inputFilename = Path.GetFileNameWithoutExtension(initialFilePath);
            List<string> aqmNames = new List<string>(); //Leave off extensions in case we want this to be .trm later
            List<AquaMotion> aqmList = new List<AquaMotion>();
            Dictionary<int, Assimp.Node> aiNodes = GetAnimatedNodes(aiScene);
            var nodeKeys = aiNodes.Keys.ToList();
            nodeKeys.Sort();
            int animatedNodeCount = nodeKeys.Last() + 1;
            AquaUtil aqua = new AquaUtil();

            for (int i = 0; i < aiScene.Animations.Count; i++)
            {
                if (aiScene.Animations[i] == null)
                {
                    continue;
                }
                AquaMotion aqm = new AquaMotion();
                var anim = aiScene.Animations[i];
                int animEndFrame = 0; //We'll fill this later. Assumes frame 0 to be the start

                string name;
                if (anim.Name != null && anim.Name != "")
                {
                    //Make sure we're not overwriting anims that somehow have duplicate names
                    if (aqmNames.Contains(anim.Name))
                    {
                        name = $"Anim_{i}_" + anim.Name;
                    }
                    else
                    {
                        name = anim.Name;
                    }
                }
                else
                {
                    name = $"Anim_{i}_" + inputFilename;
                }

                //Double check it has the pso2 extension
                var nameCleaned = NixIllegalCharacters(GetLastPipeString(name));
                var ext = Path.GetExtension(nameCleaned);
                if (aqmNames.Contains(nameCleaned) || aqmNames.Contains(nameCleaned + ".aqm"))
                {
                    nameCleaned = $"Anim_{i}_" + nameCleaned;
                }
                if ( ext != ".aqm" && ext != ".trm")
                {
                    nameCleaned += ".aqm";
                }
                aqmNames.Add(nameCleaned);

                aqm.moHeader = new AquaMotion.MOHeader();

                //Get anim fps
                if (anim.TicksPerSecond == 0)
                {
                    //Default to 30
                    aqm.moHeader.frameSpeed = 30;
                }
                else
                {
                    aqm.moHeader.frameSpeed = (float)anim.TicksPerSecond;
                }
                aqm.moHeader.endFrame = (int)anim.DurationInTicks;
                aqm.moHeader.unkInt0 = 2; //Always, always 2 for NIFL
                aqm.moHeader.variant = 0x2; //These are flags for the animation to tell the game what type it is. Since this is a skeletal animation, we always put 2 here.
                //If it's a player one specifically, the game generally adds 0x10 to this.
                aqm.moHeader.nodeCount = animatedNodeCount;
                if (playerExport)
                {
                    aqm.moHeader.variant += 0x10010;
                    aqm.moHeader.nodeCount++; //Add an extra for the nodeTreeFlag 'node'
                }
                aqm.moHeader.testString.SetString("test");

                //Set this ahead of time in case these are out of order
                aqm.motionKeys = new List<AquaMotion.KeyData>(new AquaMotion.KeyData[aqm.moHeader.nodeCount]);

                //Nodes
                foreach (var animNode in anim.NodeAnimationChannels)
                {
                    if (animNode == null)
                    {
                        continue;
                    }

                    int id;
                    ParseNodeId(animNode.NodeName, out string finalName, out id);
                    if(id < 0)
                    {
                        continue;
                    }
                    var node = aqm.motionKeys[id] = new AquaMotion.KeyData();

                    node.mseg.nodeName.SetString(finalName);
                    node.mseg.nodeId = id;
                    node.mseg.nodeType = 2;
                    node.mseg.nodeDataCount = useScaleFrames ? 3 : 2;

                    if (animNode.HasPositionKeys)
                    {
                        AquaMotion.MKEY posKeys = new AquaMotion.MKEY();
                        posKeys.keyType = 1;
                        posKeys.dataType = 1;
                        if(aqm.moHeader.endFrame > AquaMotion.ushortThreshold)
                        {
                            posKeys.dataType += 0x80;
                        }
                        var first = true;
                        foreach (var pos in animNode.PositionKeys)
                        {
                            posKeys.vector4Keys.Add(new Vector4(pos.Value.X * baseScale, pos.Value.Y * baseScale, pos.Value.Z * baseScale, 0));
                            if(animNode.PositionKeys.Count > 1)
                            {
                                //Account for first frame difference
                                if (first)
                                {
                                    posKeys.frameTimings.Add(1);
                                    first = false;
                                }
                                else
                                {
                                    posKeys.frameTimings.Add((uint)(pos.Time * posKeys.GetTimeMultiplier()));
                                }
                            }
                            posKeys.keyCount++;
                        }
                        posKeys.frameTimings[posKeys.keyCount - 1] += 2; //Account for final frame bitflags

                        animEndFrame = (int)Math.Max(animEndFrame, posKeys.frameTimings[posKeys.keyCount - 1] / 0x10);
                        node.keyData.Add(posKeys);
                    }

                    if (animNode.HasRotationKeys)
                    {
                        AquaMotion.MKEY rotKeys = new AquaMotion.MKEY();
                        rotKeys.keyType = 2;
                        rotKeys.dataType = 3;
                        if (aqm.moHeader.endFrame > AquaMotion.ushortThreshold)
                        {
                            rotKeys.dataType += 0x80;
                        }
                        var first = true;
                        foreach (var rot in animNode.RotationKeys)
                        {
                            rotKeys.vector4Keys.Add(new Vector4(rot.Value.X, rot.Value.Y, rot.Value.Z, rot.Value.W));
                            if (animNode.RotationKeys.Count > 1)
                            {
                                //Account for first frame difference
                                if (first)
                                {
                                    rotKeys.frameTimings.Add(1);
                                    first = false;
                                }
                                else
                                {
                                    rotKeys.frameTimings.Add((uint)(rot.Time * rotKeys.GetTimeMultiplier()));
                                }
                            }
                            rotKeys.keyCount++;
                        }
                        rotKeys.frameTimings[rotKeys.keyCount - 1] += 2; //Account for final frame bitflags

                        animEndFrame = (int)Math.Max(animEndFrame, rotKeys.frameTimings[rotKeys.keyCount - 1] / 0x10);
                        node.keyData.Add(rotKeys);
                    }

                    if (animNode.HasScalingKeys)
                    {
                        AquaMotion.MKEY sclKeys = new AquaMotion.MKEY();
                        sclKeys.keyType = 3;
                        sclKeys.dataType = 1; 
                        if (aqm.moHeader.endFrame > AquaMotion.ushortThreshold)
                        {
                            sclKeys.dataType += 0x80;
                        }
                        var first = true;
                        foreach (var scl in animNode.ScalingKeys)
                        {
                            var sclKey = new Vector4(scl.Value.X, scl.Value.Y, scl.Value.Z, 0);
                            if (float.IsNaN(sclKey.X))
                            {
                                sclKey.X = 1.0f;
                            }
                            if (float.IsNaN(sclKey.Y))
                            {
                                sclKey.Y = 1.0f;
                            }
                            if (float.IsNaN(sclKey.Z))
                            {
                                sclKey.Z = 1.0f;
                            }
                            if (float.IsNaN(sclKey.W))
                            {
                                sclKey.W = 0f;
                            }
                            sclKeys.vector4Keys.Add(sclKey);
                            if (animNode.ScalingKeys.Count > 1)
                            {
                                //Account for first frame difference
                                if (first)
                                {
                                    sclKeys.frameTimings.Add(1);
                                    first = false;
                                }
                                else
                                {
                                    sclKeys.frameTimings.Add((uint)(scl.Time * sclKeys.GetTimeMultiplier()));
                                }
                            }
                            sclKeys.keyCount++;
                        }
                        sclKeys.frameTimings[sclKeys.keyCount - 1] += 2; //Account for final frame bitflags

                        animEndFrame = (int)Math.Max(animEndFrame, sclKeys.frameTimings[sclKeys.keyCount - 1] / 0x10);
                        node.keyData.Add(sclKeys);
                    }
                }

                //NodeTreeFlag
                if (playerExport)
                {
                    var node = aqm.motionKeys[aqm.motionKeys.Count - 1] = new AquaMotion.KeyData();
                    node.mseg.nodeName.SetString("__NodeTreeFlag__");
                    node.mseg.nodeId = 0;
                    node.mseg.nodeType = 0x10;
                    node.mseg.nodeDataCount = useScaleFrames ? 3 : 2;

                    //Position
                    AquaMotion.MKEY posKeys = new AquaMotion.MKEY();
                    posKeys.keyType = 0x10;
                    posKeys.dataType = 5; 
                    if (aqm.moHeader.endFrame > AquaMotion.ushortThreshold)
                    {
                        posKeys.dataType += 0x80;
                    }
                    posKeys.keyCount = animEndFrame + 1;
                    for (int frame = 0; frame < posKeys.keyCount; frame++)
                    {
                        if (frame == 0)
                        {
                            posKeys.frameTimings.Add(0x9);
                        }
                        else if (frame == posKeys.keyCount - 1)
                        {
                            posKeys.frameTimings.Add((uint)((frame * posKeys.GetTimeMultiplier()) + 0xA));
                        }
                        else
                        {
                            posKeys.frameTimings.Add((uint)((frame * posKeys.GetTimeMultiplier()) + 0x8));
                        }
                        posKeys.intKeys.Add(0x31);
                    }
                    node.keyData.Add(posKeys);

                    //Rotation
                    AquaMotion.MKEY rotKeys = new AquaMotion.MKEY();
                    rotKeys.keyType = 0x11;
                    rotKeys.dataType = 5;
                    if (aqm.moHeader.endFrame > AquaMotion.ushortThreshold)
                    {
                        rotKeys.dataType += 0x80;
                    }
                    rotKeys.keyCount = animEndFrame + 1;
                    for (int frame = 0; frame < rotKeys.keyCount; frame++)
                    {
                        if (frame == 0)
                        {
                            rotKeys.frameTimings.Add(0x9);
                        }
                        else if (frame == rotKeys.keyCount - 1)
                        {
                            rotKeys.frameTimings.Add((uint)((frame * rotKeys.GetTimeMultiplier()) + 0xA));
                        }
                        else
                        {
                            rotKeys.frameTimings.Add((uint)((frame * rotKeys.GetTimeMultiplier()) + 0x8));
                        }
                        rotKeys.intKeys.Add(0x31);
                    }
                    node.keyData.Add(rotKeys);

                    //Scale
                    if (useScaleFrames)
                    {
                        AquaMotion.MKEY sclKeys = new AquaMotion.MKEY();
                        sclKeys.keyType = 0x12;
                        sclKeys.dataType = 5;
                        if (aqm.moHeader.endFrame > AquaMotion.ushortThreshold)
                        {
                            sclKeys.dataType += 0x80;
                        }
                        sclKeys.keyCount = animEndFrame + 1;
                        for (int frame = 0; frame < sclKeys.keyCount; frame++)
                        {
                            if (frame == 0)
                            {
                                sclKeys.frameTimings.Add(0x9);
                            }
                            else if (frame == sclKeys.keyCount - 1)
                            {
                                sclKeys.frameTimings.Add((uint)((frame * sclKeys.GetTimeMultiplier()) + 0xA));
                            }
                            else
                            {
                                sclKeys.frameTimings.Add((uint)((frame * sclKeys.GetTimeMultiplier()) + 0x8));
                            }
                            sclKeys.intKeys.Add(0x31);
                        }
                        node.keyData.Add(sclKeys);
                    }
                }

                //Sanity check
                bool add0x80 = aqm.moHeader.endFrame > AquaMotion.ushortThreshold;
                Dictionary<int, int> ids = new Dictionary<int, int>();
                foreach (var aiPair in aiNodes)
                {
                    var node = aqm.motionKeys[aiPair.Key];
                    var aiNode = aiPair.Value;
                    int id, parId;
                    ParseNodeId(aiNode.Name, out string finalName, out id);
                    if (aiNode.Parent != null)
                    {
                        ParseNodeId(aiNode.Parent.Name, out string parName, out parId);
                        ids.Add(id, parId);
                    }

                    if (node == null)
                    {
                        node = aqm.motionKeys[aiPair.Key] = new AquaMotion.KeyData();
                        node.mseg.nodeName.SetString(finalName);
                        node.mseg.nodeId = id != -1 ? id : aiPair.Key;
                        node.mseg.nodeType = 2;
                        node.mseg.nodeDataCount = useScaleFrames ? 3 : 2;

                        //Position
                        AddOnePosFrame(node, aiNode, baseScale, add0x80);

                        //Rotation
                        AddOneRotFrame(node, aiNode, add0x80);

                        //Scale
                        AddOneScaleFrame(useScaleFrames, node, add0x80);
                    }
                    else
                    {
                        if (node.keyData[0].vector4Keys.Count < 1)
                        {
                            AddOnePosFrame(node, aiNode, baseScale, add0x80);
                        }

                        if (node.keyData[1].vector4Keys.Count < 1)
                        {
                            AddOneRotFrame(node, aiNode, add0x80);
                        }

                        if (useScaleFrames && node.keyData[2].vector4Keys.Count < 1)
                        {
                            AddOneScaleFrame(useScaleFrames, node, add0x80, aiNode);
                        }
                    }
                }
                
                if(useScaleFrames)
                {
                    for (int k = 0; k < aqm.motionKeys.Count; k++)
                    {
                        if(ids.ContainsKey(k) && ids[k] >= 0)
                        {
                            var scaleKeys = aqm.motionKeys[k].GetMKEYofType(3);
                            var parScaleKeys = aqm.motionKeys[ids[k]].GetMKEYofType(3);

                            if(scaleKeys == null || parScaleKeys == null)
                            {
                                continue;
                            }

                            for(int t = 0; t < scaleKeys.vector4Keys.Count; t++)
                            {
                                var time = scaleKeys.frameTimings.Count > 0 ? scaleKeys.frameTimings[t] : 1;
                                scaleKeys.vector4Keys[t] = scaleKeys.vector4Keys[t] * parScaleKeys.GetLinearInterpolatedVec4Key(time);
                            }
                        }
                    }
                }

                aqmList.Add(aqm);
            }

            for (int i = 0; i < aqmList.Count; i++)
            {
                var aqm = aqmList[i];
                AquaUtilData.AnimSet set = new AquaUtilData.AnimSet();
                set.anims.Add(aqm);
                aqua.aquaMotions.Add(set);
                aqua.WriteNIFLMotion(initialFilePath + "_" + aqmNames[i]);

                aqua.aquaMotions.Clear();
            }
        }

        private static void AddOneScaleFrame(bool useScaleFrames, AquaMotion.KeyData node, bool add0x80 = false, Assimp.Node aiNode = null)
        {
            if (useScaleFrames)
            {
                AquaMotion.MKEY sclKeys = new AquaMotion.MKEY();
                sclKeys.keyType = 3;
                sclKeys.dataType = 1;
                sclKeys.keyCount = 1;
                if (aiNode == null)
                {
                    sclKeys.vector4Keys = new List<Vector4>() { new Vector4(1.0f, 1.0f, 1.0f, 0) };
                }
                else
                {
                    Matrix4x4.Invert(GetMat4FromAssimpMat4(aiNode.Transform), out Matrix4x4 mat4);
                    if(float.IsNaN(mat4.M11))
                    {
                        mat4.M11 = 1.0f;
                    }
                    if (float.IsNaN(mat4.M22))
                    {
                        mat4.M22 = 1.0f;
                    }
                    if (float.IsNaN(mat4.M33))
                    {
                        mat4.M33 = 1.0f;
                    }
                    sclKeys.vector4Keys = new List<Vector4>() { new Vector4(mat4.M11, mat4.M22, mat4.M33, 0) };
                }
                node.keyData.Add(sclKeys);
            }
        }

        private static void AddOneRotFrame(AquaMotion.KeyData node, Assimp.Node aiNode, bool add0x80 = false)
        {
            AquaMotion.MKEY rotKeys = new AquaMotion.MKEY();
            rotKeys.keyType = 2;
            rotKeys.dataType = 3;
            rotKeys.keyCount = 1;
            Matrix4x4.Invert(GetMat4FromAssimpMat4(aiNode.Transform), out Matrix4x4 mat4);
            var quat = Quaternion.CreateFromRotationMatrix(mat4);
            rotKeys.vector4Keys = new List<Vector4>() { new Vector4(quat.X, quat.Y, quat.Z, quat.W) };
            node.keyData.Add(rotKeys);
        }

        private static void AddOnePosFrame(AquaMotion.KeyData node, Assimp.Node aiNode, float baseScale, bool add0x80 = false)
        {
            AquaMotion.MKEY posKeys = new AquaMotion.MKEY();
            posKeys.keyType = 1;
            posKeys.dataType = 1;
            posKeys.keyCount = 1;
            var mat4 = GetMat4FromAssimpMat4(aiNode.Transform);
            posKeys.vector4Keys = new List<Vector4>() { new Vector4(mat4.M14 * baseScale, mat4.M24 * baseScale, mat4.M34 * baseScale, 0) };
            node.keyData.Add(posKeys);
        }

        public static string FilterAnimatedNodeName(string name)
        {
            return name.Substring(name.IndexOf(')') + 1);
        }

        public static int GetNodeNumber(string name)
        {
            var nameArr = name.Split('(');
            if (nameArr.Length == 1)
            {
                return -1;
            }
            nameArr = nameArr[1].Split(')');
            if (nameArr.Length == 1)
            {
                return -1;
            }
            string num = nameArr[0];

            if (Int32.TryParse(num, out int result))
            {
                return result;
            }
            else
            {
                return -1;
            }
        }

        public static Dictionary<int, Assimp.Node> GetAnimatedNodes(Assimp.Scene aiScene)
        {
            Dictionary<int, Assimp.Node> nodes = new Dictionary<int, Assimp.Node>();

            foreach (var node in aiScene.RootNode.Children)
            {
                CollectAnimated(node, nodes);
            }

            return nodes;
        }

        public static void CollectAnimated(Assimp.Node node, Dictionary<int, Assimp.Node> nodes)
        {
            //Be extra sure that this isn't an effect node
            if (IsAnimatedNode(node, out string name, out int num))
            {
                //For now, assume animated node is numbered and we can add it directly
                nodes.Add(num, node);
            }

            foreach (var childNode in node.Children)
            {
                CollectAnimated(childNode, nodes);
            }
        }

        //Check if a node is an animated by node by checking if it has the numbering formatting and is NOT marked as an effect node. Output name 
        public static bool IsAnimatedNode(Assimp.Node node, out string name, out int num)
        {
            bool isNumbered = IsNumberedAnimated(node, out name, out num);
            return isNumbered && !node.Name.Contains("#Eff");
        }

        //Check if node is numbered as an animated node. Output name
        private static bool IsNumberedAnimated(Assimp.Node node, out string name, out int num)
        {
            name = node.Name;
            num = -1;
            if (node.Name[0] == '(' && node.Name.Contains(')'))
            {
                if (!int.TryParse(name.Split('(')[1].Split(')')[0], out num))
                {
                    //Make sure this is actually -1 if not parssed properly
                    num = -1;
                };

                return true;
            }
            return false;
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

            foreach (var childNode in node.Children)
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
                vert.pos = (Vector3.Transform(newPos, nodeMat) * 2.54f);

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

        //Takes in an Assimp model and generates a full PSO2 model and skeleton from it.
        public static AquaObject AssimpAquaConvertFull(string initialFilePath, float scaleFactor, bool preAssignNodeIds, bool isNGS, out AquaNode aqn)
        {
            AquaUtil aquaUtil = new AquaUtil();
            Assimp.AssimpContext context = new Assimp.AssimpContext();
            context.SetConfig(new Assimp.Configs.FBXPreservePivotsConfig(false));
            Assimp.Scene aiScene = context.ImportFile(initialFilePath, Assimp.PostProcessSteps.Triangulate | Assimp.PostProcessSteps.JoinIdenticalVertices | Assimp.PostProcessSteps.FlipUVs);

            double scale = 100;
            float orgScale = 100;
            if (aiScene.Metadata.ContainsKey("UnitScaleFactor"))
            {
                scale = (double)aiScene.Metadata["UnitScaleFactor"].Data;
            }
            if (aiScene.Metadata.ContainsKey("OriginalUnitScaleFactor"))
            {
                orgScale = (float)aiScene.Metadata["OriginalUnitScaleFactor"].Data;
            }
            float baseScale = (float)(scale / 100.0);
            AquaObject aqp;
            aqn = new AquaNode();
            if (isNGS)
            {
                aqp = new NGSAquaObject();
            }
            else
            {
                aqp = new ClassicAquaObject();
            }

            //Construct Materials
            Dictionary<string, int> matNameTracker = new Dictionary<string, int>();
            foreach (var aiMat in aiScene.Materials)
            {
                string name;
                if (matNameTracker.ContainsKey(aiMat.Name))
                {
                    name = $"{aiMat.Name} ({matNameTracker[aiMat.Name]})";
                    matNameTracker[aiMat.Name] += 1;
                }
                else
                {
                    name = aiMat.Name;
                    matNameTracker.Add(aiMat.Name, 1);
                }

                AquaObject.GenericMaterial genMat = new AquaObject.GenericMaterial();
                List<string> shaderList = new List<string>();
                AquaObjectMethods.GetMaterialNameData(ref name, ref shaderList, out string alphaType, out string playerFlag, out int twoSided);
                genMat.matName = name;
                genMat.shaderNames = shaderList;
                genMat.blendType = alphaType;
                genMat.specialType = playerFlag;
                genMat.twoSided = twoSided;
                genMat.texNames = new List<string>();
                genMat.texUVSets = new List<int>();

                //Texture assignments. Since we can't rely on these to export properly, we dummy them or just put diffuse if a playerFlag isn't defined.
                //We'll have the user set these later if needed.
                if (genMat.specialType != null)
                {
                    AquaObjectMethods.GenerateSpecialMaterialParameters(genMat);
                }
                else if (aiMat.TextureDiffuse.FilePath != null)
                {
                    genMat.texNames.Add(Path.GetFileName(aiMat.TextureDiffuse.FilePath));
                }
                else
                {
                    genMat.texNames.Add("tex0_d.dds");
                }
                genMat.texUVSets.Add(0);

                AquaObjectMethods.GenerateMaterial(aqp, genMat, true);
            }

            //Default to this so ids can be assigned by order if needed
            Dictionary<string, int> boneDict = new Dictionary<string, int>();
            var aqnRoot = GetRootNode(aiScene.RootNode);
            if (aqnRoot == null || preAssignNodeIds == true)
            {
                int nodeCounter = 0;
                BuildAiNodeDictionary(aiScene.RootNode, ref nodeCounter, boneDict);
            }
            IterateAiNodesAQP(aqp, aqn, aiScene, aiScene.RootNode, Matrix4x4.Transpose(GetMat4FromAssimpMat4(aiScene.RootNode.Transform)), baseScale);

            //Generate bonepalette. No real reason not to just put in every bone at the moment.
            aqp.bonePalette = new List<uint>();
            for (uint i = 0; i < aqn.nodeList.Count; i++)
            {
                aqp.bonePalette.Add(i);
            }

            //Assimp data is gathered, proceed to processing model data for PSO2
            AquaUtilData.ModelSet set = new AquaUtilData.ModelSet();
            set.models.Add(aqp);
            aquaUtil.aquaModels.Add(set);
            aquaUtil.ConvertToNGSPSO2Mesh(false, false, false, true, false, false, true);

            //AQPs created this way will require more processing to finish.
            //-Texture lists in particular, MUST be generated as what exists is not valid without serious errors
            return aquaUtil.aquaModels[0].models[0];
        }

        private static Assimp.Node GetRootNode(Assimp.Node aiNode)
        {
            var nodeCountName = GetNodeNumber(aiNode.Name);
            if (nodeCountName != -1)
            {
                if (nodeCountName == 0)
                {
                    return aiNode;
                }
            }
            foreach (var childNode in aiNode.Children)
            {
                var node = GetRootNode(childNode);
                if (node != null)
                {
                    return node;
                }
            }
            return null;
        }

        private static void BuildAiNodeDictionary(Assimp.Node aiNode, ref int nodeCounter, Dictionary<string, int> boneDict, bool useNameNodeNum = false)
        {
            var nodeCountName = GetNodeNumber(aiNode.Name);
            if (nodeCountName != -1)
            {
                useNameNodeNum = true;
                boneDict.Add(aiNode.Name, nodeCountName);
            }
            else if (useNameNodeNum == false)
            {
                boneDict.Add(aiNode.Name, nodeCounter);
            }

            //We can in theory ignore bones that don't meet either condition since they'll be effect nodes and listed outside the normal count
            nodeCounter++;

            foreach (var childNode in aiNode.Children)
            {
                BuildAiNodeDictionary(childNode, ref nodeCounter, boneDict, useNameNodeNum);
            }
        }

        private static void IterateAiNodesAQP(AquaObject aqp, AquaNode aqn, Assimp.Scene aiScene, Assimp.Node aiNode, Matrix4x4 parentTfm, float baseScale)
        {
            //Decide if this is an effect node or not
            string nodeName = aiNode.Name;
            var nodeParent = aiNode.Parent;
            if (ParseNodeId(nodeName, out string finalName, out int nodeId))
            {
                AquaNode.NODE node = new AquaNode.NODE();
                node.boneName.SetString(finalName);
                node.animatedFlag = 1;
                node.unkNode = -1;
                node.firstChild = -1;
                node.nextSibling = -1;
                node.const0_2 = 0;
                node.ngsSibling = 0; //Unsure how this is truly set. Seems to correlate to an id or bone count subtracted from 0xFFFFFFFF. However 0 seems to work so we just leave it as that.

                //Ignore parent logic for node 0 since it may be attached to a root node
                if (nodeId != 0)
                {
                    //If there's a parent, do things reliant on that
                    if (nodeParent != null && ParseNodeId(nodeParent.Name, out string nodeParentName, out int parNodeId))
                    {
                        node.parentId = parNodeId;

                        //Fix up parent node associations
                        if (aqn.nodeList[parNodeId].firstChild == -1 || (aqn.nodeList[parNodeId].firstChild > nodeId))
                        {
                            var parNode = aqn.nodeList[parNodeId];
                            parNode.firstChild = nodeId;
                            aqn.nodeList[parNodeId] = parNode;
                        }

                        //Set next sibling. We loop through the parent node's children and set the smallest id that's larger than the present node's id. If nothing is found, keep it -1.
                        foreach (var childNode in nodeParent.Children)
                        {
                            if (childNode.Name != aiNode.Name)
                            {
                                ParseNodeId(childNode.Name, out string childName, out int sibCandidate);
                                if (sibCandidate > nodeId && (node.nextSibling == -1 || sibCandidate < node.nextSibling))
                                {
                                    node.nextSibling = sibCandidate;
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Error: Parent node not processed before its child");
                    }
                }
                else
                {
                    node.parentId = -1;
                }
                ParseShorts(nodeName, out node.boneShort1, out node.boneShort2);

                //Assign transform data
                Matrix4x4 worldMat;
                var localMat = SwapRow4Column4Mat4(GetMat4FromAssimpMat4(aiNode.Transform));
                worldMat = localMat;
                if (node.parentId != -1)
                {
                    //Matrix4x4.Invert(aqn.nodeList[node.parentId].GetInverseBindPoseMatrix(), out var parMatrix);
                    //worldMat *= parMatrix;
                    worldMat = GetWorldTransform(aiNode);
                }
                worldMat = SetMatrixScale1(worldMat);
                Matrix4x4.Invert(worldMat, out var worldMatInv);
                node.m1 = new Vector4(worldMatInv.M11, worldMatInv.M12, worldMatInv.M13, worldMatInv.M14);
                node.m2 = new Vector4(worldMatInv.M21, worldMatInv.M22, worldMatInv.M23, worldMatInv.M24);
                node.m3 = new Vector4(worldMatInv.M31, worldMatInv.M32, worldMatInv.M33, worldMatInv.M34);
                node.m4 = new Vector4(worldMatInv.M41 * baseScale, worldMatInv.M42 * baseScale, worldMatInv.M43 * baseScale, worldMatInv.M44);
                node.pos = localMat.Translation * baseScale;
                node.eulRot = QuaternionToEuler(Quaternion.CreateFromRotationMatrix(localMat));
                node.scale = new Vector3(1, 1, 1); //This is a bit of a weird thing

                //Put in  list at appropriate id 
                if (aqn.nodeList.Count < nodeId + 1)
                {
                    while (aqn.nodeList.Count < nodeId + 1)
                    {
                        aqn.nodeList.Add(new AquaNode.NODE());
                    }
                }
                aqn.nodeList[nodeId] = node;
            }
            else
            {
                //Most nodos should have parents, but sometimes sega is crazy
                int parNodeId = -1;
                if (aiNode.Parent != null)
                {
                    ParseNodeId(aiNode.Parent.Name, out string parentName, out parNodeId);
                }
                //Nodo nodes can't have nodo parents. Therefore, we skip anything below another nodo or nodes that aren't in the proper hierarchy.
                if (!aiNode.HasChildren)
                {
                    AquaNode.NODO nodo = new AquaNode.NODO();
                    nodo.boneName.SetString(finalName);
                    nodo.animatedFlag = 1;
                    nodo.boneName.SetString(nodeName);
                    nodo.parentId = parNodeId;
                    ParseShorts(nodeName, out nodo.boneShort1, out nodo.boneShort2);

                    //Assign transform data
                    var localMat = SwapRow4Column4Mat4(GetMat4FromAssimpMat4(aiNode.Transform));
                    nodo.pos = localMat.Translation;

                    nodo.eulRot = QuaternionToEuler(Quaternion.CreateFromRotationMatrix(localMat));

                    aqn.nodoList.Add(nodo);
                } else
                {
                    Debug.WriteLine("Error: Effect nodes CANNOT have children. Add an id to the name to treat it as a standard node instead.");
                }
            }

            Matrix4x4 nodeMat = Matrix4x4.Transpose(GetMat4FromAssimpMat4(aiNode.Transform));
            nodeMat = Matrix4x4.Multiply(nodeMat, parentTfm);

            foreach (int meshId in aiNode.MeshIndices)
            {
                var mesh = aiScene.Meshes[meshId];
                AddAiMeshToAQP(aqp, mesh, nodeMat, baseScale);
            }

            foreach (var childNode in aiNode.Children)
            {
                IterateAiNodesAQP(aqp, aqn, aiScene, childNode, nodeMat, baseScale);
            }
        }

        //There's probably a better way to do this, but I am no math king
        private static Matrix4x4 SetMatrixScale1(Matrix4x4 worldMat)
        {
            Matrix4x4 mat;
            Matrix4x4.Decompose(worldMat, out var scaleMat, out var rotMat, out var posMat);
            mat = Matrix4x4.Identity;
            mat *= Matrix4x4.CreateScale(new Vector3(1, 1, 1));
            mat *= Matrix4x4.CreateFromQuaternion(rotMat);
            mat *= Matrix4x4.CreateTranslation(posMat);
            return mat;
        }

        public static void AddAiMeshToAQP(AquaObject aqp, Assimp.Mesh mesh, Matrix4x4 nodeMat, float baseScale)
        {
            AquaObject.GenericTriangles genTris = new AquaObject.GenericTriangles();
            genTris.name = mesh.Name;
            genTris.baseMeshNodeId = 0;
            genTris.baseMeshDummyId = -1;
            var ids = GetMeshIds(mesh.Name);
            if (ids.Count > 0)
            {
                genTris.baseMeshNodeId = ids[0];
                if (ids.Count > 1)
                {
                    genTris.baseMeshDummyId = ids[1];
                }
            }

            //Iterate through faces to get face and vertex data
            for (int faceId = 0; faceId < mesh.FaceCount; faceId++)
            {
                var face = mesh.Faces[faceId];
                var faceVerts = face.Indices;
                genTris.triList.Add(new Vector3(faceVerts[0], faceVerts[1], faceVerts[2]));
                genTris.matIdList.Add(mesh.MaterialIndex);
                genTris.vertCount = mesh.Vertices.Count;
                AquaObject.VTXL faceVtxl = new AquaObject.VTXL();

                foreach (var v in faceVerts) //Expects triangles, not quads or polygons
                {
                    faceVtxl.rawFaceId.Add(faceId);
                    faceVtxl.rawVertId.Add(v);
                    var vertPos = new Vector3(mesh.Vertices[v].X, mesh.Vertices[v].Y, mesh.Vertices[v].Z);
                    vertPos = Vector3.Transform(vertPos, nodeMat);
                    vertPos = new Vector3(vertPos.X * baseScale, vertPos.Y * baseScale, vertPos.Z * baseScale);
                    faceVtxl.vertPositions.Add(vertPos);
                    if (mesh.HasNormals)
                    {
                        faceVtxl.vertNormals.Add(new Vector3(mesh.Normals[v].X * baseScale, mesh.Normals[v].Y * baseScale, mesh.Normals[v].Z * baseScale));
                    }
                    if (mesh.HasVertexColors(0))
                    {
                        var color = mesh.VertexColorChannels[0][v];
                        faceVtxl.vertColors.Add(new byte[] { floatToColor(color.B), floatToColor(color.G), floatToColor(color.R), floatToColor(color.A) });
                    }
                    if (mesh.HasTextureCoords(0))
                    {
                        var uv = mesh.TextureCoordinateChannels[0][v];
                        faceVtxl.uv1List.Add(new Vector2(uv.X, uv.Y));
                    }
                    if (mesh.HasTextureCoords(1))
                    {
                        var uv = mesh.TextureCoordinateChannels[1][v];
                        faceVtxl.uv2List.Add(new Vector2(uv.X, uv.Y));
                    }
                    if (mesh.HasTextureCoords(2))
                    {
                        var uv = mesh.TextureCoordinateChannels[2][v];
                        faceVtxl.uv3List.Add(new Vector2(uv.X, uv.Y));
                    }
                    if (mesh.HasTextureCoords(3))
                    {
                        var uv = mesh.TextureCoordinateChannels[3][v];
                        faceVtxl.uv4List.Add(new Vector2(uv.X, uv.Y));
                    }
                    if (mesh.HasTextureCoords(4))
                    {
                        var uv = mesh.TextureCoordinateChannels[4][v];
                        faceVtxl.vert0x22.Add(new short[] { floatToShort(uv.X), floatToShort(uv.Y) });
                    }
                    if (mesh.HasTextureCoords(5))
                    {
                        var uv = mesh.TextureCoordinateChannels[5][v];
                        faceVtxl.vert0x23.Add(new short[] { floatToShort(uv.X), floatToShort(uv.Y) });
                    }
                    if (mesh.HasTextureCoords(6))
                    {
                        var uv = mesh.TextureCoordinateChannels[6][v];
                        faceVtxl.vert0x24.Add(new short[] { floatToShort(uv.X), floatToShort(uv.Y) });
                    }
                    if (mesh.HasTextureCoords(7))
                    {
                        var uv = mesh.TextureCoordinateChannels[7][v];
                        faceVtxl.vert0x25.Add(new short[] { floatToShort(uv.X), floatToShort(uv.Y) });
                    }
                    if (mesh.HasTextureCoords(8))
                    {
                        if(mesh.HasTextureCoords(9))
                        {
                            var uv = mesh.TextureCoordinateChannels[8][v];
                            var uv2 = mesh.TextureCoordinateChannels[9][v];
                            faceVtxl.vertColor2s.Add(new byte[] { floatToColor(uv.X), floatToColor(uv.Y), floatToColor(uv2.X), floatToColor(uv2.Y) });
                        }
                    }

                    //Bone weights and indices
                    if (mesh.HasBones)
                    {
                        List<int> vertWeightIds = new List<int>();
                        List<float> vertWeights = new List<float>();
                        foreach (var bone in mesh.Bones)
                        {
                            var boneId = GetNodeNumber(bone.Name);
                            foreach (var weight in bone.VertexWeights)
                            {
                                if (weight.VertexID == v)
                                {
                                    vertWeightIds.Add(boneId);
                                    vertWeights.Add(weight.Weight);
                                    break;
                                }
                            }
                        }
                        faceVtxl.rawVertWeightIds.Add(vertWeightIds);
                        faceVtxl.rawVertWeights.Add(vertWeights);
                    }
                }
                genTris.faceVerts.Add(faceVtxl);
            }

            aqp.tempTris.Add(genTris);
        }

        public static List<int> GetMeshIds(string name)
        {
            if (name.Substring(name.Length - 5, 5) == "_mesh")
            {
                name = name.Substring(0, name.Length - 5);
            }
            List<int> ids = new List<int>();
            var split = name.Split('#');
            if (split.Length > 1)
            {
                ids.Add(Int32.Parse(split[1]));
                if (split.Length > 2)
                {
                    ids.Add(Int32.Parse(split[2]));
                }
            }

            return ids;
        }

        public static Assimp.Matrix4x4 GetWorldMatrix(Assimp.Node node)
        {
            var mat = node.Transform;
            var currNode = node;
            while (currNode.Parent != null)
            {
                mat *= currNode.Parent.Transform;
                currNode = currNode.Parent;
            }

            return mat;
        }

        public static void ParseShorts(string nodeName, out ushort boneShort1, out ushort boneShort2)
        {
            boneShort1 = 0x1C0;
            boneShort2 = 0;
            var numParse = nodeName.Split('#');
            if (numParse.Length > 1)
            {
                numParse[1] = numParse[1].Replace("0x", "");
                try
                {
                    boneShort1 = ushort.Parse(numParse[1], System.Globalization.NumberStyles.HexNumber);
                    if (numParse.Length > 2)
                    {
                        numParse[2] = numParse[1].Replace("0x", "");

                        try
                        {
                            boneShort2 = ushort.Parse(numParse[2], System.Globalization.NumberStyles.HexNumber);
                        }
                        catch
                        {
                        }
                    }
                }
                catch
                {
                }
            }
        }

        public static bool ParseNodeId(string nodeName, out string nodeNameSeparated, out int id)
        {
            nodeNameSeparated = nodeName;
            var numParse = nodeName.Split('(', ')');
            if (numParse.Length == 1)
            {
                id = -1;
                return false;
            }
            nodeNameSeparated = numParse[2];
            nodeNameSeparated = nodeNameSeparated.Split('#')[0];
            if (Int32.TryParse(numParse[1], out int result))
            {
                id = result;
                return true;
            }
            else
            {
                id = -1;
                return false;
            }
        }
        private static Matrix4x4 GetWorldTransform(Assimp.Node aiNode)
        {
            var transform = aiNode.Transform;

            while ((aiNode = aiNode.Parent) != null)
                transform *= aiNode.Transform;

            return ToNumericsTransposed(transform);
        }

        public static Matrix4x4 ToNumericsTransposed(Assimp.Matrix4x4 value)
        {
            return new Matrix4x4(
                value.A1, value.B1, value.C1, value.D1,
                value.A2, value.B2, value.C2, value.D2,
                value.A3, value.B3, value.C3, value.D3,
                value.A4, value.B4, value.C4, value.D4);
        }


        public static Matrix4x4 GetMat4FromAssimpMat4(Assimp.Matrix4x4 mat4)
        {
            return new Matrix4x4(mat4.A1, mat4.A2, mat4.A3, mat4.A4,
                                 mat4.B1, mat4.B2, mat4.B3, mat4.B4,
                                 mat4.C1, mat4.C2, mat4.C3, mat4.C4,
                                 mat4.D1, mat4.D2, mat4.D3, mat4.D4);
        }
        public static Matrix4x4 SwapRow4Column4Mat4(Matrix4x4 mat4)
        {
            return new Matrix4x4(mat4.M11, mat4.M12, mat4.M13, mat4.M41,
                                mat4.M21, mat4.M22, mat4.M23, mat4.M42,
                                mat4.M31, mat4.M32, mat4.M33, mat4.M43,
                                mat4.M14, mat4.M24, mat4.M34, mat4.M44);
        }

        public static byte floatToColor(float flt)
        {
            double dbl = flt * 255;
            byte fltByte;
            if (dbl > 255)
            {
                fltByte = 255;
            }
            else
            {
                fltByte = (byte)dbl;
            }
            return fltByte;
        }

        public static short floatToShort(float flt)
        {
            double dbl = flt;
            dbl *= 32767;

            return (short)dbl;
        }
    }
}
