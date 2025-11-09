using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Helpers.MathHelpers;
using System.Numerics;

namespace AquaModelLibrary.Data.Utility
{
    /// <summary>
    /// Based on Assimp implementation in https://github.com/assimp/assimp/blob/83d7216726726a07e9e40f86cc2322b22fec11fa/code/PostProcessing/ConvertToLHProcess.cpp. 
    /// Flips the handedness of the model units.
    /// </summary>
    public class HandednessUtility
    {
        public static void FlipHandednessAqpAndAqnX(AquaObject aqp, AquaNode aqn)
        {
            FlipHandednessAqpX(aqp);
            FlipHandednessAqnX(aqn);
        }
        public static void FlipHandednessAqpAndAqnY(AquaObject aqp, AquaNode aqn)
        {
            FlipHandednessAqpY(aqp);
            FlipHandednessAqnY(aqn);
        }

        public static void FlipHandednessAqpAndAqnZ(AquaObject aqp, AquaNode aqn)
        {
            FlipHandednessAqpZ(aqp);
            FlipHandednessAqnZ(aqn);
        }

        public static void NegativeYZSwap(AquaObject aqp, AquaNode aqn)
        {
            NegativeYZSwapAqp(aqp);
            NegativeYZSwapAqn(aqn);
        }

        private static Matrix4x4 NYZSwapMat = new Matrix4x4(1, 0, 0, 0,
                                                     0, 0, 1, 0,
                                                     0, 1, 0, 0,
                                                     0, 0, 0, 1);

        public static void NegativeYZSwapAqn(AquaNode aqn)
        {
            List<Matrix4x4> originalMatricesInverted = aqn.nodeList.Select(x => x.GetInverseBindPoseMatrix()).ToList();
            List<Matrix4x4> originalMatrices = aqn.nodeList.Select(x => x.GetInverseBindPoseMatrixInverted()).ToList();
            for (int i = 0; i < aqn.nodeList.Count; i++)
            {
                var bone = aqn.nodeList[i];
                var tfm = bone.GetInverseBindPoseMatrixInverted();
                tfm *= NYZSwapMat;
                tfm = NYZSwapMat * tfm;

                bone.SetInverseBindPoseMatrixFromUninvertedMatrix(tfm);

                aqn.nodeList[i] = bone;
            }

            for (int i = 0; i < aqn.nodoList.Count; i++)
            {
                var bn = aqn.nodoList[i];
                // NODOs are a bit more primitive. We need to generate the matrix for these ones.
                var tfm = Matrix4x4.Identity;
                var rotation = Matrix4x4.CreateRotationX(bn.eulRot.X) *
                               Matrix4x4.CreateRotationY(bn.eulRot.Y) *
                               Matrix4x4.CreateRotationZ(bn.eulRot.Z);

                tfm *= rotation;
                tfm *= originalMatrices[i];
                tfm = FlipMatrixX(tfm);
                tfm *= originalMatricesInverted[i];

                Matrix4x4.Decompose(tfm, out var scale, out var rot, out var pos);
                bn.pos = new Vector3(-bn.pos.X, bn.pos.Y, bn.pos.Z);
                bn.eulRot = MathExtras.QuaternionToEuler(rot);

                aqn.nodoList[i] = bn;
            }
        }
        public static void NegativeYZSwapAqp(AquaObject aqp)
        {
            // Process mesh data
            foreach (var vtxl in aqp.vtxlList)
            {
                for (int v = 0; v < vtxl.vertPositions.Count; v++)
                {
                    vtxl.vertPositions[v] = Vector3.Transform(vtxl.vertPositions[v], NYZSwapMat);
                    if (vtxl.vertNormals.Count > v)
                    {
                        vtxl.vertNormals[v] = Vector3.TransformNormal(vtxl.vertNormals[v], NYZSwapMat);
                    }
                    if (vtxl.vertTangentList.Count > v)
                    {
                        vtxl.vertTangentList[v] = Vector3.TransformNormal(vtxl.vertTangentList[v], NYZSwapMat);
                    }
                    if (vtxl.vertBinormalList.Count > v)
                    {
                        vtxl.vertBinormalList[v] = Vector3.TransformNormal(vtxl.vertBinormalList[v], NYZSwapMat);
                    }
                }
            }
        }

        public static void FlipHandednessMotionAqmX(AquaMotion aqm)
        {
            for (int i = 0; i < aqm.motionKeys.Count; i++)
            {
                var posMKey = aqm.motionKeys[i].GetMKEYofType(1);
                var rotMKey = aqm.motionKeys[i].GetMKEYofType(2);
                if (posMKey != null)
                {
                    for (int k = 0; k < posMKey.vector4Keys.Count; k++)
                    {
                        var key = posMKey.vector4Keys[k];
                        key.X *= -1;
                        posMKey.vector4Keys[k] = key;
                    }
                }
                if (rotMKey != null)
                {
                    for (int k = 0; k < rotMKey.vector4Keys.Count; k++)
                    {
                        var key = rotMKey.vector4Keys[k];
                        key.Y *= -1;
                        key.Z *= -1;
                        rotMKey.vector4Keys[k] = key;
                    }
                }
            }
        }

        public static void FlipHandednessMotionAqmY(AquaMotion aqm)
        {
            for (int i = 0; i < aqm.motionKeys.Count; i++)
            {
                var posMKey = aqm.motionKeys[i].GetMKEYofType(1);
                var rotMKey = aqm.motionKeys[i].GetMKEYofType(2);
                if (posMKey != null)
                {
                    for (int k = 0; k < posMKey.vector4Keys.Count; k++)
                    {
                        var key = posMKey.vector4Keys[k];
                        key.Y *= -1;
                        posMKey.vector4Keys[k] = key;
                    }
                }
                if (rotMKey != null)
                {
                    for (int k = 0; k < rotMKey.vector4Keys.Count; k++)
                    {
                        var key = rotMKey.vector4Keys[k];
                        key.X *= -1;
                        key.Z *= -1;
                        rotMKey.vector4Keys[k] = key;
                    }
                }
            }
        }

        public static void FlipHandednessMotionAqmZ(AquaMotion aqm)
        {
            for (int i = 0; i < aqm.motionKeys.Count; i++)
            {
                var posMKey = aqm.motionKeys[i].GetMKEYofType(1);
                var rotMKey = aqm.motionKeys[i].GetMKEYofType(2);
                if (posMKey != null)
                {
                    for (int k = 0; k < posMKey.vector4Keys.Count; k++)
                    {
                        var key = posMKey.vector4Keys[k];
                        key.Z *= -1;
                        posMKey.vector4Keys[k] = key;
                    }
                }
                if (rotMKey != null)
                {
                    for (int k = 0; k < rotMKey.vector4Keys.Count; k++)
                    {
                        var key = rotMKey.vector4Keys[k];
                        key.X *= -1;
                        key.Y *= -1;
                        rotMKey.vector4Keys[k] = key;
                    }
                }
            }
        }

        public static void FlipHandednessAqpX(AquaObject aqp)
        {
            // Process mesh data
            foreach (var vtxl in aqp.vtxlList)
            {
                for (int v = 0; v < vtxl.vertPositions.Count; v++)
                {
                    var vertPos = vtxl.vertPositions[v];
                    vertPos.X *= -1;
                    vtxl.vertPositions[v] = vertPos;
                    if (vtxl.vertNormals.Count > v)
                    {
                        var vertNrm = vtxl.vertNormals[v];
                        vertNrm.X *= -1;
                        vtxl.vertNormals[v] = vertNrm;
                    }
                    if (vtxl.vertTangentList.Count > v)
                    {
                        var vertTng = vtxl.vertTangentList[v];
                        vertTng.X *= -1;
                        vtxl.vertTangentList[v] = vertTng;
                    }
                    if (vtxl.vertBinormalList.Count > v)
                    {
                        var vertBi = vtxl.vertBinormalList[v];
                        vertBi.X *= -1;
                        vtxl.vertBinormalList[v] = vertBi;
                    }
                }
            }
        }

        public static void FlipHandednessAqnX(AquaNode aqn)
        {
            List<Matrix4x4> originalMatricesInverted = aqn.nodeList.Select(x => x.GetInverseBindPoseMatrix()).ToList();
            List<Matrix4x4> originalMatrices = aqn.nodeList.Select(x => x.GetInverseBindPoseMatrixInverted()).ToList();
            for (int i = 0; i < aqn.nodeList.Count; i++)
            {
                var bone = aqn.nodeList[i];
                var tfm = bone.GetInverseBindPoseMatrixInverted();
                tfm = FlipMatrixX(tfm);

                bone.SetInverseBindPoseMatrixFromUninvertedMatrix(tfm);

                aqn.nodeList[i] = bone;
            }

            for (int i = 0; i < aqn.nodoList.Count; i++)
            {
                var bn = aqn.nodoList[i];
                // NODOs are a bit more primitive. We need to generate the matrix for these ones.
                var tfm = Matrix4x4.Identity;
                var rotation = Matrix4x4.CreateRotationX(bn.eulRot.X) *
                               Matrix4x4.CreateRotationY(bn.eulRot.Y) *
                               Matrix4x4.CreateRotationZ(bn.eulRot.Z);

                tfm *= rotation;
                tfm *= originalMatrices[i];
                tfm = FlipMatrixX(tfm);
                tfm *= originalMatricesInverted[i];

                Matrix4x4.Decompose(tfm, out var scale, out var rot, out var pos);
                bn.pos = new Vector3(-bn.pos.X, bn.pos.Y, bn.pos.Z);
                bn.eulRot = MathExtras.QuaternionToEuler(rot);

                aqn.nodoList[i] = bn;
            }
        }

        private static Matrix4x4 FlipMatrixX(Matrix4x4 tfm)
        {
            tfm.M11 = -tfm.M11;
            tfm.M12 = -tfm.M12;
            tfm.M13 = -tfm.M13;
            tfm.M14 = -tfm.M14;

            tfm.M11 = -tfm.M11;
            tfm.M21 = -tfm.M21;
            tfm.M31 = -tfm.M31;
            tfm.M41 = -tfm.M41;
            return tfm;
        }

        public static void FlipHandednessAqpY(AquaObject aqp)
        {
            // Process mesh data
            foreach (var vtxl in aqp.vtxlList)
            {
                for (int v = 0; v < vtxl.vertPositions.Count; v++)
                {
                    var vertPos = vtxl.vertPositions[v];
                    vertPos.Y *= -1;
                    vtxl.vertPositions[v] = vertPos;
                    if (vtxl.vertNormals.Count > v)
                    {
                        var vertNrm = vtxl.vertNormals[v];
                        vertNrm.Y *= -1;
                        vtxl.vertNormals[v] = vertNrm;
                    }
                    if (vtxl.vertTangentList.Count > v)
                    {
                        var vertTng = vtxl.vertTangentList[v];
                        vertTng.Y *= -1;
                        vtxl.vertTangentList[v] = vertTng;
                    }
                    if (vtxl.vertBinormalList.Count > v)
                    {
                        var vertBi = vtxl.vertBinormalList[v];
                        vertBi.Y *= -1;
                        vtxl.vertBinormalList[v] = vertBi;
                    }
                }
            }
        }

        public static void FlipHandednessAqnY(AquaNode aqn)
        {
            List<Matrix4x4> originalMatricesInverted = aqn.nodeList.Select(x => x.GetInverseBindPoseMatrix()).ToList();
            List<Matrix4x4> originalMatrices = aqn.nodeList.Select(x => x.GetInverseBindPoseMatrixInverted()).ToList();
            for (int i = 0; i < aqn.nodeList.Count; i++)
            {
                var bone = aqn.nodeList[i];
                var tfm = bone.GetInverseBindPoseMatrixInverted();
                tfm = FlipMatrixY(tfm);

                bone.SetInverseBindPoseMatrixFromUninvertedMatrix(tfm);

                aqn.nodeList[i] = bone;
            }

            for (int i = 0; i < aqn.nodoList.Count; i++)
            {
                var bn = aqn.nodoList[i];
                // NODOs are a bit more primitive. We need to generate the matrix for these ones.
                var tfm = Matrix4x4.Identity;
                var rotation = Matrix4x4.CreateRotationX(bn.eulRot.X) *
                               Matrix4x4.CreateRotationY(bn.eulRot.Y) *
                               Matrix4x4.CreateRotationZ(bn.eulRot.Z);

                tfm *= rotation;
                tfm *= originalMatrices[i];
                tfm = FlipMatrixY(tfm);
                tfm *= originalMatricesInverted[i];

                Matrix4x4.Decompose(tfm, out var scale, out var rot, out var pos);
                bn.pos = new Vector3(bn.pos.X, -bn.pos.Y, bn.pos.Z);
                bn.eulRot = MathExtras.QuaternionToEuler(rot);

                aqn.nodoList[i] = bn;
            }
        }

        private static Matrix4x4 FlipMatrixY(Matrix4x4 tfm)
        {
            tfm.M21 = -tfm.M21;
            tfm.M22 = -tfm.M22;
            tfm.M23 = -tfm.M23;
            tfm.M24 = -tfm.M24;

            tfm.M12 = -tfm.M12;
            tfm.M22 = -tfm.M22;
            tfm.M32 = -tfm.M32;
            tfm.M42 = -tfm.M42;
            return tfm;
        }

        public static void FlipHandednessAqpZ(AquaObject aqp)
        {
            //Process mesh data
            foreach (var vtxl in aqp.vtxlList)
            {
                for (int v = 0; v < vtxl.vertPositions.Count; v++)
                {
                    var vertPos = vtxl.vertPositions[v];
                    vertPos.Z *= -1;
                    vtxl.vertPositions[v] = vertPos;
                    if (vtxl.vertNormals.Count > v)
                    {
                        var vertNrm = vtxl.vertNormals[v];
                        vertNrm.Z *= -1;
                        vtxl.vertNormals[v] = vertNrm;
                    }
                    if (vtxl.vertTangentList.Count > v)
                    {
                        var vertTng = vtxl.vertTangentList[v];
                        vertTng.Z *= -1;
                        vtxl.vertTangentList[v] = vertTng;
                    }
                    if (vtxl.vertBinormalList.Count > v)
                    {
                        var vertBi = vtxl.vertBinormalList[v];
                        vertBi.Z *= -1;
                        vtxl.vertBinormalList[v] = vertBi;
                    }
                }
            }
        }

        public static void FlipHandednessAqnZ(AquaNode aqn)
        {
            List<Matrix4x4> originalMatricesInverted = aqn.nodeList.Select(x => x.GetInverseBindPoseMatrix()).ToList();
            List<Matrix4x4> originalMatrices = aqn.nodeList.Select(x => x.GetInverseBindPoseMatrixInverted()).ToList();
            for (int i = 0; i < aqn.nodeList.Count; i++)
            {
                var bone = aqn.nodeList[i];
                var tfm = bone.GetInverseBindPoseMatrixInverted();
                tfm = FlipMatrixZ(tfm);

                bone.SetInverseBindPoseMatrixFromUninvertedMatrix(tfm);

                aqn.nodeList[i] = bone;
            }

            for (int i = 0; i < aqn.nodoList.Count; i++)
            {
                var bn = aqn.nodoList[i];
                //NODOs are a bit more primitive. We need to generate the matrix for these ones.
                var tfm = Matrix4x4.Identity;
                var rotation = Matrix4x4.CreateRotationX(bn.eulRot.X) *
                   Matrix4x4.CreateRotationY(bn.eulRot.Y) *
                   Matrix4x4.CreateRotationZ(bn.eulRot.Z);

                tfm *= rotation;
                tfm *= originalMatrices[bn.parentId];
                tfm = FlipMatrixZ(tfm);
                tfm *= originalMatricesInverted[bn.parentId];

                Matrix4x4.Decompose(tfm, out var scale, out var rot, out var pos);
                bn.pos = new Vector3(bn.pos.X, bn.pos.Y, -bn.pos.Z);
                bn.eulRot = MathExtras.QuaternionToEuler(rot);

                aqn.nodoList[i] = bn;
            }
        }

        private static Matrix4x4 FlipMatrixZ(Matrix4x4 tfm)
        {
            tfm.M31 = -tfm.M31;
            tfm.M32 = -tfm.M32;
            tfm.M33 = -tfm.M33;
            tfm.M34 = -tfm.M34;

            tfm.M13 = -tfm.M13;
            tfm.M23 = -tfm.M23;
            tfm.M33 = -tfm.M33;
            tfm.M43 = -tfm.M43;
            return tfm;
        }
    }
}
