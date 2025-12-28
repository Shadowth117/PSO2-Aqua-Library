using AquaModelLibrary.Data.BillyHatcher;
using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaNodeData;
using AquaModelLibrary.Helpers.MathHelpers;
using System.Numerics;

namespace AquaModelLibrary.Core.Billy
{
    public class PathConvert
    {
        //These are path ids that shouuld never be calced in the base game. It's hardcoded there so we'll hard code it here, unless forced otherwise
        //Known builds of the game only have 1 race to a path, but just in case we'll do these as lists
        public static List<int> blueRacePaths = new List<int>() { 8 };
        public static List<int> redRacePaths = new List<int>() { 1 };
        public static List<int> purpleRacePaths = new List<int>() { 3 };
        public static List<int> yellowRacePaths = new List<int>() { 7 };

        public static bool IsRacePath(string fileName, int pathSplineId)
        {
            switch(fileName.ToLower())
            {
                case "path_blue.pth":
                    return blueRacePaths.Contains(pathSplineId);
                case "path_red.pth":
                    return redRacePaths.Contains(pathSplineId);
                case "path_purple.pth":
                    return purpleRacePaths.Contains(pathSplineId);
                case "path_yellow.pth":
                    return yellowRacePaths.Contains(pathSplineId);
            }

            return false;
        }

        public static void ImportSplineToPath(PATH path, AquaNode aqn, string pathFileName, bool addNormals, bool isLiquidCurrent, bool isObjectPath, 
            bool forceNoBSPCalc = false, int pathSplineId = -1)
        {
            PATH.PathInfo pathInfo = new PATH.PathInfo();
            pathInfo.doesNotUseNormals = (byte)(addNormals ? 0 : 1);
            pathInfo.isLiquidCurrent = isLiquidCurrent ? 1 : 0;
            pathInfo.isObjectPath = (byte)(isObjectPath ? 1 : 0);
            pathInfo.vertDef = new PATH.VertDefinition();
            double length = 0;
            Vector3 prevTranslation = new Vector3();
            for(int i = 0; i < aqn.nodeList.Count; i++)
            {
                if(aqn.nodeList[i].boneName.GetString().ToLower().Contains("(n)"))
                {
                    continue;
                }
                var nodeMat = aqn.nodeList[i].GetInverseBindPoseMatrixInverted();
                pathInfo.vertDef.vertPositions.Add(nodeMat.Translation);
                if(i == 0)
                {
                    prevTranslation = nodeMat.Translation;
                } else
                {
                    length += MathExtras.Distance(prevTranslation, nodeMat.Translation);
                }
                pathInfo.lengthsList.Add((float)length);

                if (addNormals)
                {
                    Quaternion nodeRot = Quaternion.CreateFromRotationMatrix(nodeMat);
                    var normalVector = Vector3.Transform(new Vector3(0, 1, 0), nodeRot);
                    //Grab the first (N) child if it exists, grab the next non (N) node as the next point
                    var children = aqn.GetNODEChildren(i);
                    foreach (var child in children)
                    {
                        if (aqn.nodeList[child].boneName.GetString().ToLower().Contains("(n)"))
                        {
                            var childMat = aqn.nodeList[child].GetInverseBindPoseMatrixInverted();
                            normalVector = childMat.Translation - nodeMat.Translation;
                            break;
                        }
                    }

                    pathInfo.vertDef.vertNormals.Add(normalVector);
                }
            }
            pathInfo.totalLength = pathInfo.lengthsList[^1];

            //Decide if this is an additional path or replacing an existing one
            if (pathSplineId == -1 || pathSplineId > path.pathInfoList.Count - 1)
            {
                //If this is a path with normals, to follow the game's precedent we should insert it before the paths without normals
                //For now we will not though
                pathInfo.id = path.pathInfoList.Count;
                path.pathInfoList.Add(pathInfo);
            } else
            {
                pathInfo.id = pathSplineId;
                for(int i = 0; i < path.pathInfoList.Count; i++)
                {
                    if (path.pathInfoList[i].id == pathSplineId)
                    {
                        path.pathInfoList[i] = pathInfo;
                    }
                }
            }

            //Decide if we recalculate the BSP quadtree for this path
            if (!forceNoBSPCalc && !isObjectPath && !IsRacePath(pathFileName, pathSplineId))
            {
                float xMin = float.MaxValue;
                float xMax = float.MinValue;
                float zMin = float.MaxValue;
                float zMax = float.MinValue;
                for (int i = 0; i < path.pathInfoList.Count; i++)
                {
                    if(isObjectPath || IsRacePath(pathFileName, i))
                    {
                        continue;
                    }
                    foreach(var pos in path.pathInfoList[i].vertDef.vertPositions)
                    {
                        xMin = Math.Min(pos.X, xMin);
                        xMax = Math.Max(pos.X, xMax);
                        zMin = Math.Min(pos.Z, zMin);
                        zMax = Math.Max(pos.Z, zMax);
                    }
                }
                    
                path.rootSector = path.SubdivideSector(new Vector2(xMin, xMax), new Vector2(zMin, zMax), new List<PATH.PathSegment>(), 0);
            }
        }

        public static List<AquaNode> ExportPathToSplines(PATH path)
        {
            List<AquaNode> aquaNodes = new List<AquaNode>();

            for(int i = 0; i < path.pathInfoList.Count; i++)
            {
                aquaNodes.Add(ExportPathSpline(path, i));
            }

            return aquaNodes;
        }

        public static AquaNode ExportPathSpline(PATH path, int splineIndex)
        {
            AquaNode aqn = new AquaNode();
            for (int j = 0; j < path.pathInfoList[splineIndex].vertDef.vertPositions.Count; j++)
            {
                NODE node = new NODE();
                var pos = path.pathInfoList[splineIndex].vertDef.vertPositions[j];
                node.SetInverseBindPoseMatrixFromUninvertedMatrix(Matrix4x4.CreateTranslation(pos));
                node.boneName.SetString($"Vert {j}");
                if (j == 0)
                {
                    node.parentId = -1;
                }
                else
                {
                    node.parentId = aqn.nodeList.Count;
                    if (path.pathInfoList[splineIndex].vertDef.vertNormals.Count > 0)
                    {
                        node.parentId -= 1;
                    }
                }
                node.firstChild = -1;
                node.nextSibling = -1;

                aqn.nodeList.Add(node);
                //Create child node to contain normal direction 
                if (path.pathInfoList[splineIndex].vertDef.vertNormals.Count > 0)
                {
                    node.firstChild = aqn.nodeList.Count;
                    NODE nrmNode = new NODE();
                    nrmNode.parentId = aqn.nodeList.Count - 1;
                    nrmNode.firstChild = -1;
                    var nrm = path.pathInfoList[splineIndex].vertDef.vertNormals[j];
                    nrmNode.SetInverseBindPoseMatrixFromUninvertedMatrix(Matrix4x4.CreateTranslation(nrm + pos));
                    nrmNode.boneName.SetString($"(N) Normal {j}");
                    aqn.nodeList[aqn.nodeList.Count - 1] = node;
                    aqn.nodeList.Add(nrmNode);
                }

            }

            return aqn;
        }
    }
}
