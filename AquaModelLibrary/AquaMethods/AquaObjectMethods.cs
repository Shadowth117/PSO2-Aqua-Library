using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows;
using static AquaModelLibrary.AquaObject;

namespace AquaModelLibrary.AquaMethods
{
    public unsafe class AquaObjectMethods
    {
        public static void ReadVTXL(BufferedStreamReader streamReader, AquaObject.VTXE vtxeSet, AquaObject.VTXL vtxl, int vertCount, int vertTypeCount)
        {
            for (int vtxlIndex = 0; vtxlIndex < vertCount; vtxlIndex++)
            {
                for (int vtxeIndex = 0; vtxeIndex < vertTypeCount; vtxeIndex++)
                {
                    switch (vtxeSet.vertDataTypes[vtxeIndex].dataType)
                    {
                        case (int)AquaObject.VertFlags.VertPosition:
                            vtxl.vertPositions.Add(streamReader.Read<Vector3>());
                            break;
                        case (int)AquaObject.VertFlags.VertWeight:
                            vtxl.vertWeights.Add(streamReader.Read<Vector4>());
                            break;
                        case (int)AquaObject.VertFlags.VertNormal:
                            vtxl.vertNormals.Add(streamReader.Read<Vector3>());
                            break;
                        case (int)AquaObject.VertFlags.VertColor:
                            vtxl.vertColors.Add(Read4Bytes(streamReader));
                            break;
                        case (int)AquaObject.VertFlags.VertColor2:
                            vtxl.vertColor2s.Add(Read4Bytes(streamReader));
                            break;
                        case (int)AquaObject.VertFlags.VertWeightIndex:
                            vtxl.vertWeightIndices.Add(Read4Bytes(streamReader));
                            break;
                        case (int)AquaObject.VertFlags.VertUV1:
                            vtxl.uv1List.Add(streamReader.Read<Vector2>());
                            break;
                        case (int)AquaObject.VertFlags.VertUV2:
                            vtxl.uv2List.Add(streamReader.Read<Vector2>());
                            break;
                        case (int)AquaObject.VertFlags.VertUV3:
                            vtxl.uv3List.Add(streamReader.Read<Vector2>());
                            break;
                        case (int)AquaObject.VertFlags.VertTangent:
                            vtxl.vertTangentList.Add(streamReader.Read<Vector3>());
                            break;
                        case (int)AquaObject.VertFlags.VertBinormal:
                            vtxl.vertBinormalList.Add(streamReader.Read<Vector3>());
                            break;
                        default:
                            MessageBox.Show($"Unknown Vert type {vtxeSet.vertDataTypes[vtxeIndex].dataType}! Please report!");
                            break;
                    }
                }
            }
            vtxl.createTrueVertWeights();
        }

        public static BoundingVolume GenerateBounding (List<VTXL> vertData)
        {
            BoundingVolume bounds = new BoundingVolume();
            Vector3 maxPoint = new Vector3();
            Vector3 minPoint = new Vector3();
            Vector3 difference = new Vector3();
            Vector3 center = new Vector3();
            float radius = 0;

            for (int vset = 0; vset < vertData.Count; vset++)
            {
                for(int vert = 0; vert < vertData[vset].vertPositions.Count; vert++)
                {
                    //Compare to max
                    if (maxPoint.X < vertData[vset].vertPositions[vert].X) 
                    {
                        maxPoint.X = vertData[vset].vertPositions[vert].X;
                    }
                    if (maxPoint.Y < vertData[vset].vertPositions[vert].Y)
                    {
                        maxPoint.Y = vertData[vset].vertPositions[vert].Y;
                    }
                    if (maxPoint.Z < vertData[vset].vertPositions[vert].Z)
                    {
                        maxPoint.Z = vertData[vset].vertPositions[vert].Z;
                    }

                    //Compare to min
                    if (minPoint.X > vertData[vset].vertPositions[vert].X)
                    {
                        minPoint.X = vertData[vset].vertPositions[vert].X;
                    }
                    if (minPoint.Y > vertData[vset].vertPositions[vert].Y)
                    {
                        minPoint.Y = vertData[vset].vertPositions[vert].Y;
                    }
                    if (minPoint.Z > vertData[vset].vertPositions[vert].Z)
                    {
                        minPoint.Z = vertData[vset].vertPositions[vert].Z;
                    }
                }
            }

            difference.X = Math.Abs(maxPoint.X - minPoint.X / 2);
            difference.Y = Math.Abs(maxPoint.Y - minPoint.Y / 2);
            difference.Z = Math.Abs(maxPoint.Z - minPoint.Z / 2);
            center.X = maxPoint.X - difference.X;
            center.Y = maxPoint.Y - difference.Y;
            center.Z = maxPoint.Z - difference.Z;

            //Get max radius from center
            for (int vset = 0; vset < vertData.Count; vset++)
            {
                for (int vert = 0; vert < vertData[vset].vertPositions.Count; vert++)
                {
                    float distance = Distance(center, vertData[vset].vertPositions[vert]);
                    if(distance > radius)
                    {
                        radius = distance;
                    }
                }
            }

            bounds.modelCenter = center;
            bounds.modelCenter2 = center;
            bounds.maxMinXYZDifference = difference;
            bounds.boundingRadius = radius;

            return bounds;
        }

        public static float Distance(Vector3 point1, Vector3 point2)
        {
            return (float)Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2) + Math.Pow(point2.Z - point1.Z, 2));
        }

        //Adapted from this: https://forums.cgsociety.org/t/finding-bi-normals-tangents/975005/8 
        //Binormals and tangents for each face are calculated and each face's values for a particular vertex are summed and averaged for the result before being normalized
        //Though vertex position is used, due to the nature of the normalization applied during the process, resizing is unneeded.
        public static void computeTangentSpace(AquaObject model, bool useFaceNormals)
        {
            for(int mesh = 0; mesh < model.vsetList.Count; mesh++)
            {
                //Verify that there are vertices and with valid UVs
                int vsetIndex = model.meshList[mesh].vsetIndex;
                int psetIndex = model.meshList[mesh].psetIndex;
                if (model.vtxlList[vsetIndex].uv1List.Count > 0 && model.vtxlList[vsetIndex].vertPositions.Count > 0)
                {
                    Vector3[] vertBinormalArray = new Vector3[model.vtxlList[vsetIndex].vertPositions.Count];
                    Vector3[] vertTangentArray = new Vector3[model.vtxlList[vsetIndex].vertPositions.Count];
                    Vector3[] vertFaceNormalArray = new Vector3[model.vtxlList[vsetIndex].vertPositions.Count];
                    List<Vector3> faces = model.strips[psetIndex].getTriangles(true);

                    for (int f = 0; f < faces.Count; f++)
                    {
                        Vector3 face = faces[f];
                        List<int> faceIndices = new List<int>()
                        {
                            (int)face.X,
                            (int)face.Y,
                            (int)face.Z
                        };

                        List<Vector3> verts = new List<Vector3>() 
                        { 
                          model.vtxlList[vsetIndex].vertPositions[(int)face.X], 
                          model.vtxlList[vsetIndex].vertPositions[(int)face.Y],
                          model.vtxlList[vsetIndex].vertPositions[(int)face.Z]
                        };
                        List<Vector2> uvs = new List<Vector2>()
                        {
                          model.vtxlList[vsetIndex].uv1List[(int)face.X],
                          model.vtxlList[vsetIndex].uv1List[(int)face.Y],
                          model.vtxlList[vsetIndex].uv1List[(int)face.Z]
                        };

                        Vector3 dV1 = verts[0] - verts[1];
                        Vector3 dV2 = verts[0] - verts[2];
                        Vector2 dUV1 = uvs[0] - uvs[1];
                        Vector2 dUV2 = uvs[0] - uvs[2];

                        float area = dUV1.X * dUV2.Y - dUV1.Y * dUV2.X;
                        int sign;
                        if(area < 0)
                        {
                            sign = -1;
                        } else
                        {
                            sign = 1;
                        }

                        Vector3 tangent = new Vector3();
                        tangent.X = dV1.X * dUV2.Y - dUV1.Y * dV2.X;
                        tangent.Y = dV1.Y * dUV2.Y - dUV1.Y * dV2.Y;
                        tangent.Z = dV1.Z * dUV2.Y - dUV1.Y * dV2.Z;
                        tangent = Vector3.Normalize(tangent) * sign;

                        //Calculate face normal
                        Vector3 u = verts[1] - verts[0];
                        Vector3 v = verts[2] - verts[0];

                        Vector3 normal = Vector3.Normalize(Vector3.Cross(u, v));
                        Vector3 binormal = Vector3.Normalize(Vector3.Cross(normal, tangent)) * sign;

                        //Create or add vectors to the collections
                        for(int i = 0; i < 3; i++)
                        {
                            if(vertBinormalArray[faceIndices[i]] == null)
                            {
                                vertBinormalArray[faceIndices[i]] = binormal;
                                vertTangentArray[faceIndices[i]] = tangent;
                                vertFaceNormalArray[faceIndices[i]] = normal;
                            } else
                            {
                                vertBinormalArray[faceIndices[i]] += binormal;
                                vertTangentArray[faceIndices[i]] += tangent;
                                vertFaceNormalArray[faceIndices[i]] += normal;
                            }
                        }
                    }

                    //Average out the values for use ingame
                    for(int vert = 0; vert < model.vtxlList[vsetIndex].vertPositions.Count; vert++)
                    {
                        vertBinormalArray[vert] = Vector3.Normalize(vertBinormalArray[vert]);
                        vertTangentArray[vert] = Vector3.Normalize(vertTangentArray[vert]);
                        vertFaceNormalArray[vert] = Vector3.Normalize(vertFaceNormalArray[vert]);
                    }

                    //Set these back in the model
                    if(useFaceNormals)
                    {
                        model.vtxlList[vsetIndex].vertNormals = vertFaceNormalArray.ToList();
                    }
                    model.vtxlList[vsetIndex].vertBinormalList = vertBinormalArray.ToList();
                    model.vtxlList[vsetIndex].vertTangentList = vertTangentArray.ToList();
                }
            }
        }

        public static string GetPSO2String(byte* str)
        {
            string finalText;

            //Lazily determine string end
            int end = 0;
            for (int j = 0; j < 0x20; j++)
            {
                if (str[j] == 0)
                {
                    end = j;
                    break;
                }
            }

            byte[] text = new byte[end];
            Marshal.Copy(new IntPtr(str), text, 0, end);
            finalText = System.Text.Encoding.UTF8.GetString(text);

            return finalText;
        }

        public static string GetBoneName(AquaNode.NODE node) => GetPSO2String(node.boneName);

        public static string GetEffName(AquaNode.NODO eff) => GetPSO2String(eff.boneName);

        public static string GetMatName(MATE mate) => GetPSO2String(mate.matName);

        public static string GetMatOpacity(MATE mate) => GetPSO2String(mate.alphaType);

        public static List<string> GetTexListNames(AquaObject model, int tsetIndex)
        {
            List<string> textureList = new List<string>();

            TSET tset = model.tsetList[tsetIndex];

            for (int index = 0; index < 4; index++) 
            {
                int texIndex = tset.tstaTexIDs[index];
                if (texIndex != -1)
                {
                    TSTA tsta = model.tstaList[texIndex];

                    textureList.Add(GetPSO2String(tsta.texName));
                }
            }

            return textureList;
        }

        public static List<string> GetShaderNames(AquaObject model, int shadIndex)
        {
            List<string> shaderList = new List<string>();

            SHAD shad = model.shadList[shadIndex];

            shaderList.Add(GetPSO2String(shad.pixelShader));
            shaderList.Add(GetPSO2String(shad.vertexShader));

            return shaderList;
        }

        private static byte[] Read4Bytes(BufferedStreamReader streamReader)
        {
            byte[] bytes = new byte[4];
            for (int byteIndex = 0; byteIndex < 4; byteIndex++) { bytes[byteIndex] = streamReader.Read<byte>(); }

            return bytes;
        }
    }
}
