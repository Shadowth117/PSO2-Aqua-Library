using AquaModelLibrary.Data.CustomRoboBattleRevolution.Model;
using AquaModelLibrary.Data.CustomRoboBattleRevolution.Model.Common;
using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaNodeData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.Intermediary;
using System.Numerics;

namespace AquaModelLibrary.Data.CustomRoboBattleRevolution
{
    public class CRBRModelConvert
    {
        public static List<AquaObject> ModelConvert(CRBRPartModel model, out List<AquaNode> aqnList)
        {
            List<AquaObject> aqoList = new List<AquaObject>();
            aqnList = new List<AquaNode>();
            foreach (var mdl in model.modelDataList)
            {
                int nodeCounter = 0;
                AquaObject aqo = new AquaObject();
                AquaNode aqn = new AquaNode();
                GatherModelData(mdl, ref nodeCounter, aqo, aqn, Matrix4x4.Identity, -1);
                aqoList.Add(aqo);
                aqnList.Add(aqn);
            }

            return aqoList;
        }

        public static void GatherModelData(CRBRModelDataSet modelDataSet, ref int nodeId, AquaObject aqo, AquaNode aqn, Matrix4x4 parentMatrix, int parentId)
        {
            GatherModelNodeDataRecursive(modelDataSet.rootNode, ref nodeId, aqo, aqn, parentMatrix, parentId);
        }

        public static void GatherModelNodeDataRecursive(CRBRNode node, ref int nodeId, AquaObject aqo, AquaNode aqn, Matrix4x4 parentMatrix, int parentId)
        {
            aqo.bonePalette.Add((uint)nodeId);
            int currentNodeId = nodeId;
            Matrix4x4 mat = Matrix4x4.Identity;
            mat *= Matrix4x4.CreateScale(node.scale);
            var rotation = Matrix4x4.CreateRotationX(node.eulerRotation.X) *
                Matrix4x4.CreateRotationY(node.eulerRotation.Y) *
                Matrix4x4.CreateRotationZ(node.eulerRotation.Z);
            mat *= rotation;
            mat *= Matrix4x4.CreateTranslation(node.translation);
            mat = mat * parentMatrix;

            //Create AQN node
            NODE aqNode = new NODE();
            aqNode.boneShort1 = 0x1C0;
            aqNode.animatedFlag = 1;
            aqNode.parentId = parentId;
            aqNode.nextSibling = -1;
            aqNode.firstChild = -1;
            aqNode.unkNode = -1;
            aqNode.pos = node.translation;
            aqNode.eulRot = new Vector3((float)(node.eulerRotation.X * 180 / Math.PI),
                (float)(node.eulerRotation.Y * 180 / Math.PI), (float)(node.eulerRotation.Z * 180 / Math.PI));
            aqNode.scale = node.scale;
            Matrix4x4.Invert(mat, out var invMat);
            aqNode.m1 = new Vector4(invMat.M11, invMat.M12, invMat.M13, invMat.M14);
            aqNode.m2 = new Vector4(invMat.M21, invMat.M22, invMat.M23, invMat.M24);
            aqNode.m3 = new Vector4(invMat.M31, invMat.M32, invMat.M33, invMat.M34);
            aqNode.m4 = new Vector4(invMat.M41, invMat.M42, invMat.M43, invMat.M44);
            aqNode.boneName.SetString(aqn.nodeList.Count.ToString());
            aqn.nodeList.Add(aqNode);

            if (node.crbrMesh != null)
            {
                int meshCounter = 0;
                GatherModelMeshDataRecursive(node.crbrMesh, ref nodeId, aqo, aqn, mat, currentNodeId, ref meshCounter);
            }

            if (node.childNode != null)
            {
                aqNode.firstChild = ++nodeId;
                GatherModelNodeDataRecursive(node.childNode, ref nodeId, aqo, aqn, mat, currentNodeId);
            }

            if (node.siblingNode != null)
            {
                aqNode.firstChild = ++nodeId;
                GatherModelNodeDataRecursive(node.siblingNode, ref nodeId, aqo, aqn, parentMatrix, parentId);
            }
        }

        public static void GatherModelMeshDataRecursive(CRBRMesh mesh, ref int nodeId, AquaObject aqo, AquaNode aqn, Matrix4x4 parentMatrix, int parentId, ref int meshCounter)
        {
            int materialId = 0;
            if (mesh.matData != null)
            {
                materialId = GatherMaterialData(mesh.matData, aqo);
            }

            if (mesh.meshData != null)
            {
                GatherMeshData(mesh.meshData, aqo, parentMatrix, mesh.matData, nodeId, meshCounter, materialId);
            }

            if (mesh.nextCRBRNode != null)
            {
                GatherModelNodeDataRecursive(mesh.nextCRBRNode, ref nodeId, aqo, aqn, parentMatrix, parentId);
            }

            if (mesh.nextCRBRMesh != null)
            {
                meshCounter++;
                GatherModelMeshDataRecursive(mesh.nextCRBRMesh, ref nodeId, aqo, aqn, parentMatrix, parentId, ref meshCounter);
            }
        }

        public static void GatherMeshData(CRBRMeshData md, AquaObject aqo, Matrix4x4 parentMatrix, CRBRMaterialData mat, int nodeId, int meshCounter, int materialId)
        {
            var vtxl = new VTXL();
            md.GatherVertexData(nodeId, vtxl, parentMatrix);
            md.GetFaceData(nodeId, vtxl, aqo, meshCounter, materialId);
        }

        public static int GatherMaterialData(CRBRMaterialData crMat, AquaObject aqo)
        {
            GenericMaterial mat = new GenericMaterial();
            mat.matName = $"Mat_{crMat.matAddress.ToString("X")}";

            mat.diffuseRGBA = new Vector4((float)(crMat.matColor.diffuseColor[2] / 255.0),
                (float)(crMat.matColor.diffuseColor[1] / 255.0), 
                (float)(crMat.matColor.diffuseColor[0] / 255.0), 
                (float)(crMat.matColor.diffuseColor[3] / 255.0));
            mat.unkRGBA0 = new Vector4((float)(crMat.matColor.ambientColor[2] / 255.0),
                (float)(crMat.matColor.ambientColor[1] / 255.0),
                (float)(crMat.matColor.ambientColor[0] / 255.0),
                (float)(crMat.matColor.ambientColor[3] / 255.0));
            mat.unkRGBA1 = new Vector4((float)(crMat.matColor.specularColor[2] / 255.0),
                (float)(crMat.matColor.specularColor[1] / 255.0),
                (float)(crMat.matColor.specularColor[0] / 255.0),
                (float)(crMat.matColor.specularColor[3] / 255.0));

            mat.texNames = new List<string>();
            foreach (var tex in crMat.textureList)
            {
                mat.texNames.Add(tex.texture.textureBufferOffset.ToString("X") + ".png");
            }

            //Check for existing equivalent materials
            int matIndex = -1;
            for (int i = 0; i < aqo.tempMats.Count; i++)
            {
                var genMat = aqo.tempMats[i];
                if (genMat.matName == mat.matName && genMat.texNames[0] == mat.texNames[0])
                {
                    matIndex = i;
                    break;
                }
            }
            if (matIndex == -1)
            {
                matIndex = aqo.tempMats.Count;
                aqo.tempMats.Add(mat);
            }

            return matIndex;
        }
    }
}
