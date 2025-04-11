using AquaModelLibrary.Data.BillyHatcher;
using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaNodeData;
using System.Numerics;

namespace AquaModelLibrary.Core.Billy
{
    public class PathConvert
    {
        public static void ImportSplineToPath(PATH path, AquaNode aqn)
        {
            PATH.PathInfo pathInfo = new PATH.PathInfo();
            for(int i = 0; i < aqn.nodeList.Count; i++)
            {
                //Check if there's an (N) child node containing the normal direction
                if (i + 1 < aqn.nodeList.Count)
                {

                }
            }

            //If this is a path with normals, to follow the game's precedent we should insert it before the paths without normals
        }

        public static List<AquaNode> ExportPathToSplines(PATH path)
        {
            List<AquaNode> aquaNodes = new List<AquaNode>();

            for(int i = 0; i < path.pathInfoList.Count; i++)
            {
                AquaNode aqn = new AquaNode();
                for(int j = 0; j < path.pathInfoList[i].vertDef.vertPositions.Count; j++)
                {
                    NODE node = new NODE();
                    var pos = path.pathInfoList[i].vertDef.vertPositions[j];
                    node.SetInverseBindPoseMatrixFromUninvertedMatrix(Matrix4x4.CreateTranslation(pos));
                    node.boneName.SetString($"Vert {j}");
                    if(j == 0)
                    {
                        node.parentId = -1;
                    } else
                    {
                        node.parentId = aqn.nodeList.Count;
                        if (path.pathInfoList[i].vertDef.vertNormals.Count > 0)
                        {
                            node.parentId -= 1;
                        }
                    }
                    node.firstChild = -1;
                    node.nextSibling = -1;

                    aqn.nodeList.Add(node);
                    //Create child node to contain normal direction 
                    if (path.pathInfoList[i].vertDef.vertNormals.Count > 0)
                    {
                        node.firstChild = aqn.nodeList.Count;
                        NODE nrmNode = new NODE();
                        nrmNode.parentId = aqn.nodeList.Count - 1;
                        nrmNode.firstChild = -1;
                        var nrm = path.pathInfoList[i].vertDef.vertNormals[j];
                        nrmNode.SetInverseBindPoseMatrixFromUninvertedMatrix(Matrix4x4.CreateTranslation(nrm + pos));
                        nrmNode.boneName.SetString($"(N) Normal {j}");
                        aqn.nodeList[aqn.nodeList.Count - 1] = node;
                        aqn.nodeList.Add(nrmNode);
                    } 

                }
                aquaNodes.Add(aqn);
            }    

            return aquaNodes;
        }
    }
}
