using AquaModelLibrary.Data.Ninja.Model.Ginja;
using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Helpers.MathHelpers;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace AquaModelLibrary.Core.General
{
    public class SmdExporter
    {
        public static void ExportToFile(AquaObject aqo, AquaNode aqn, string exportPath, AquaMotion aqm = null)
        {
            NumberFormatInfo numberFormatInfo = new NumberFormatInfo();
            numberFormatInfo.NumberDecimalSeparator = ".";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("version 1");
            sb.AppendLine("nodes");
            for(int i = 0; i < aqn.nodeList.Count; i++)
            {
                sb.AppendLine($"{i} \"{aqn.nodeList[i].boneName.GetString()}\" {aqn.nodeList[i].parentId}");
            }
            for (int i = 0; i < aqn.nodoList.Count; i++)
            {
                sb.AppendLine($"{i + aqn.nodeList.Count} \"{aqn.nodoList[i].boneName.GetString()}\" {aqn.nodoList[i].parentId}");
            }
            sb.AppendLine("end");
            sb.AppendLine("skeleton");
            sb.AppendLine("");

            if(aqm == null)
            {
                sb.AppendLine("time 0");
                for (int i = 0; i < aqn.nodeList.Count; i++)
                {
                    var bone = aqn.nodeList[i];
                    var tfm = bone.GetInverseBindPoseMatrixInverted();
                    if(bone.parentId != -1)
                    {
                        var parBone = aqn.nodeList[bone.parentId];
                        var parTfm = parBone.GetInverseBindPoseMatrix();
                        tfm *= parTfm;
                    }

                    Matrix4x4.Decompose(tfm, out var scale, out var rot, out var pos);
                    var eulRot = MathExtras.QuaternionToEulerRadians(rot);

                    sb.AppendLine($"{i} {pos.X.ToString("0.######", numberFormatInfo)} {pos.Y.ToString("0.######", numberFormatInfo)} {pos.Z.ToString("0.######", numberFormatInfo)}" +
                        $" {eulRot.X.ToString("0.######", numberFormatInfo)} {eulRot.Y.ToString("0.######", numberFormatInfo)} {eulRot.Z.ToString("0.######", numberFormatInfo)}");
                }
                for (int i = 0; i < aqn.nodoList.Count; i++)
                {
                    var bone = aqn.nodoList[i];
                    var pos = bone.pos;
                    var eulRot = bone.eulRot;
                    sb.AppendLine($"{i + aqn.nodeList.Count} {pos.X.ToString("0.######", numberFormatInfo)} {pos.Y.ToString("0.######", numberFormatInfo)} {pos.Z.ToString("0.######", numberFormatInfo)}" +
                        $" {eulRot.X.ToString("0.######", numberFormatInfo)} {eulRot.Y.ToString("0.######", numberFormatInfo)} {eulRot.Z.ToString("0.######", numberFormatInfo)}");
                }
            } else
            {
                for(int i = 0; i < aqm.moHeader.endFrame; i++)
                {
                    sb.AppendLine($"time {i}");
                    for (int j = 0; j < aqm.motionKeys.Count; j++)
                    {
                        var bone = aqn.nodeList[i];
                        var parBone = aqn.nodeList[bone.parentId];
                        var tfm = bone.GetInverseBindPoseMatrixInverted();
                        var parTfm = parBone.GetInverseBindPoseMatrix();
                        tfm *= parTfm;
                        Matrix4x4.Decompose(tfm, out var scale, out var rot, out var pos);
                        var eulRot = MathExtras.QuaternionToEulerRadians(rot);

                        var posMkey = aqm.motionKeys[i].GetMKEYofType(0x1);
                        var rotMkey = aqm.motionKeys[i].GetMKEYofType(0x2);
                        if(posMkey != null)
                        {
                            var tempPos = posMkey.GetLinearInterpolatedVec4Key(i);
                            pos = new Vector3(tempPos.X, tempPos.Y, tempPos.Z);
                        }
                        if (posMkey != null)
                        {
                            var tempRot = rotMkey.GetLinearInterpolatedVec4Key(i);
                            eulRot = MathExtras.QuaternionToEulerRadians(tempRot.ToQuat());
                        }

                        sb.AppendLine($"{j + aqn.nodeList.Count} {pos.X.ToString("0.######", numberFormatInfo)} {pos.Y.ToString("0.######", numberFormatInfo)} {pos.Z.ToString("0.######", numberFormatInfo)}" +
                            $" {eulRot.X.ToString("0.######", numberFormatInfo)} {eulRot.Y.ToString("0.######", numberFormatInfo)} {eulRot.Z.ToString("0.######", numberFormatInfo)}");
                    }
                    for (int j = 0; j < aqn.nodoList.Count; j++)
                    {
                        var bone = aqn.nodoList[j];
                        var pos = bone.pos;
                        var eulRot = bone.eulRot;
                        sb.AppendLine($"{j + aqn.nodeList.Count} {pos.X.ToString("0.######", numberFormatInfo)} {pos.Y.ToString("0.######", numberFormatInfo)} {pos.Z.ToString("0.######", numberFormatInfo)}" +
                            $" {eulRot.X.ToString("0.######", numberFormatInfo)} {eulRot.Y.ToString("0.######", numberFormatInfo)} {eulRot.Z.ToString("0.######", numberFormatInfo)}");
                    }
                }
            }
            sb.AppendLine("end");
            for(int i = 0; i < aqo.meshList.Count; i++)
            {
                var mesh = aqo.meshList[i];
                var vtxl = aqo.vtxlList[i];
                List<uint> bonePalette = aqo.bonePalette.Count > 0 ? aqo.bonePalette : vtxl.bonePalette.Select(x => (uint)x).ToList();
                var tris = aqo.strips[mesh.psetIndex].GetTriangles();
                var matName = aqo.matUnicodeNames[mesh.mateIndex];
                var texNames = aqo.GetTexListNamesUnicode(mesh.tsetIndex);
                if(texNames.Count == 0)
                {
                    texNames.Add("");
                }

                sb.AppendLine("triangles");
                for(int j = 0; j < tris.Count; j++)
                {
                    var vertXWeightCount = vtxl.trueVertWeightIndices[(int)tris[j].X].Length;
                    var vertYWeightCount = vtxl.trueVertWeightIndices[(int)tris[j].Y].Length;
                    var vertZWeightCount = vtxl.trueVertWeightIndices[(int)tris[j].Z].Length;

                    var weightDataX = GetWeightDataString(vtxl, (int)tris[j].X, vertXWeightCount, bonePalette);
                    var weightDataY = GetWeightDataString(vtxl, (int)tris[j].Y, vertYWeightCount, bonePalette);
                    var weightDataZ = GetWeightDataString(vtxl, (int)tris[j].Z, vertZWeightCount, bonePalette);

                    sb.AppendLine($"{matName}|{texNames[0]}");
                    sb.AppendLine($"0 {vtxl.vertPositions[(int)tris[j].X].X} {vtxl.vertPositions[(int)tris[j].X].Y} {vtxl.vertPositions[(int)tris[j].X].Z} " +
                                    $"{vtxl.vertNormals[(int)tris[j].X].X} {vtxl.vertNormals[(int)tris[j].X].Y} {vtxl.vertNormals[(int)tris[j].X].Z} " +
                                    $"{vtxl.uv1List[(int)tris[j].X].X} {vtxl.uv1List[(int)tris[j].X].Y} " +
                                    $"{vertXWeightCount} {weightDataX}");
                    sb.AppendLine($"0 {vtxl.vertPositions[(int)tris[j].Y].X} {vtxl.vertPositions[(int)tris[j].Y].Y} {vtxl.vertPositions[(int)tris[j].Y].Z} " +
                                    $"{vtxl.vertNormals[(int)tris[j].Y].X} {vtxl.vertNormals[(int)tris[j].Y].Y} {vtxl.vertNormals[(int)tris[j].Y].Z} " +
                                    $"{vtxl.uv1List[(int)tris[j].Y].X} {vtxl.uv1List[(int)tris[j].Y].Y} " +
                                    $"{vertYWeightCount} {weightDataY}");
                    sb.AppendLine($"0 {vtxl.vertPositions[(int)tris[j].Z].X} {vtxl.vertPositions[(int)tris[j].Z].Y} {vtxl.vertPositions[(int)tris[j].Z].Z} " +
                                    $"{vtxl.vertNormals[(int)tris[j].Z].X} {vtxl.vertNormals[(int)tris[j].Z].Y} {vtxl.vertNormals[(int)tris[j].Z].Z} " +
                                    $"{vtxl.uv1List[(int)tris[j].Z].X} {vtxl.uv1List[(int)tris[j].Z].Y} " +
                                    $"{vertZWeightCount} {weightDataZ}");
                }
                sb.AppendLine("end");
            }

            File.WriteAllText(exportPath, sb.ToString());
        }

        private static string GetWeightDataString(Data.PSO2.Aqua.AquaObjectData.VTXL vtxl, int vertId, int weightCount, List<uint> bonePalette)
        {
            string weightData = "";
            for (int wt = 0; wt < weightCount; wt++)
            {
                weightData += $"{bonePalette[vtxl.trueVertWeightIndices[vertId][wt]]} ";
                switch (wt)
                {
                    case 0:
                        weightData += $"{vtxl.trueVertWeights[vertId].X} ";
                        break;
                    case 1:
                        weightData += $"{vtxl.trueVertWeights[vertId].Y} ";
                        break;
                    case 2:
                        weightData += $"{vtxl.trueVertWeights[vertId].Z} ";
                        break;
                    case 3:
                        weightData += $"{vtxl.trueVertWeights[vertId].W} ";
                        break;
                }
            }

            return weightData;
        }
    }
}
