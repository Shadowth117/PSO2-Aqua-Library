﻿using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows;
using static AquaModelLibrary.AquaObject;

namespace AquaModelLibrary
{
    public unsafe static class AquaObjectMethods
    {
        public static readonly List<string> DefaultShaderNames = new List<string>() { "0301p", "0301" };

        public static void VTXLFromFaceVerts(AquaObject model)
        {
            model.vtxlList = new List<VTXL>();

            for(int mesh = 0; mesh < model.tempTris.Count; mesh++)
            {
                //Set up a new VTXL based on an existing sample in order to figure optimize a bit for later.
                //For the sake of simplicity, we assume that vertex IDs for this start from 0 and end at the vertex count - 1. 
                VTXL vtxl = new VTXL(model.tempTris[mesh].vertCount, model.tempTris[mesh].faceVerts[0]);
                bool[] vtxlCheck = new bool[model.tempTris[mesh].vertCount];

                //Set up classic bone palette
                for(int b = 0; b < model.tempTris[mesh].bonePalette.Count; b++)
                {
                    vtxl.bonePalette.Add((ushort)model.tempTris[mesh].bonePalette[b]);
                }

                //Go through the faces, set vertices in at their index unless they're a duplicate index with different data. 
                for (int face = 0;  face < model.tempTris[mesh].triList.Count; face++)
                {
                    for(int faceVert = 0; faceVert < 3; faceVert++)
                    {
                        int vertIndex = model.tempTris[mesh].faceVerts[face].rawVertId[faceVert];
                        if (vtxlCheck[vertIndex] == true && !IsSameVertex(vtxl, vertIndex, model.tempTris[mesh].faceVerts[face], faceVert))
                        {
                            //If this really needs to be split to a new vertex, add it to the end of the new VTXL list
                            appendVertex(model.tempTris[mesh].faceVerts[face], vtxl, faceVert);

                            var tri = model.tempTris[mesh].triList[face];
                            switch (faceVert)
                            {
                                case 0:
                                    tri.X = vtxl.vertPositions.Count - 1;
                                    break;
                                case 1:
                                    tri.Y = vtxl.vertPositions.Count - 1;
                                    break;
                                case 2:
                                    tri.Z = vtxl.vertPositions.Count - 1;
                                    break;
                            }
                            model.tempTris[mesh].triList[face] = tri;
                        }
                        else if (vtxlCheck[vertIndex] == false)
                        {
                            copyVertex(model.tempTris[mesh].faceVerts[face], vtxl, faceVert, vertIndex);
                            vtxlCheck[vertIndex] = true;
                        }
                    }

                }

                //Loop through and check for missing vertices/isolated vertices. Proceed to dummy these out as a failsafe for later access.
                for(int i = 0; i < vtxl.vertPositions.Count; i++)
                {
                    if (vtxl.vertNormals.Count > 0 && vtxl.vertNormals[i] == null)
                    {
                        vtxl.vertNormals[i] = new Vector3();
                    }
                    if (vtxl.vertNormalsNGS.Count > 0 && vtxl.vertNormalsNGS[i] == null)
                    {
                        vtxl.vertNormalsNGS[i] = new short[4];
                    }
                    if (vtxl.vertColors.Count > 0 && vtxl.vertColors[i] == null)
                    {
                        vtxl.vertColors[i] = new byte[4];
                    }
                    if (vtxl.vertColor2s.Count > 0 && vtxl.vertColor2s[i] == null)
                    {
                        vtxl.vertColor2s[i] = new byte[4];
                    }
                    if (vtxl.uv1List.Count > 0 && vtxl.uv1List[i] == null)
                    {
                        vtxl.uv1List[i] = new Vector2();
                    }
                    if (vtxl.uv1ListNGS.Count > 0 && vtxl.uv1ListNGS[i] == null)
                    {
                        vtxl.uv1ListNGS[i] = new short[2];
                    }
                    if (vtxl.uv2ListShort.Count > 0 && vtxl.uv2ListShort[i] == null)
                    {
                        vtxl.uv2ListShort[i] = new short[2];
                    }
                    if (vtxl.uv2List.Count > 0 && vtxl.uv2List[i] == null)
                    {
                        vtxl.uv2List[i] = new Vector2();
                    }
                    if (vtxl.uv3List.Count > 0 && vtxl.uv3List[i] == null)
                    {
                        vtxl.uv3List[i] = new Vector2();
                    }
                    if (vtxl.uv4List.Count > 0 && vtxl.uv4List[i] == null)
                    {
                        vtxl.uv4List[i] = new Vector2();
                    }
                    if (vtxl.vert0x22.Count > 0 && vtxl.vert0x22[i] == null)
                    {
                        vtxl.vert0x22[i] = new short[2];
                    }
                    if (vtxl.vert0x23.Count > 0 && vtxl.vert0x23[i] == null)
                    {
                        vtxl.vert0x23[i] = new short[2];
                    }
                    if (vtxl.rawVertWeights.Count > 0 && vtxl.rawVertWeights[i] == null)
                    {
                        vtxl.rawVertWeights[i] = new List<float>();
                    }
                    if (vtxl.rawVertWeightIds.Count > 0 && vtxl.rawVertWeightIds[i] == null)
                    {
                        vtxl.rawVertWeightIds[i] = new List<int>();
                    }
                }

                model.vtxlList.Add(vtxl);
            }
        }

        public static bool IsEqualByteArray(byte[] bArr0, byte[] bArr1)
        {
            if(bArr0.Length != bArr1.Length)
            {
                return false;
            } else
            {
                for(int i = 0; i < bArr0.Length; i++)
                {
                    if(bArr0[i] != bArr1[i])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool IsEqualShortArray(short[] sArr0, short[] sArr1)
        {
            if (sArr0.Length != sArr1.Length)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < sArr0.Length; i++)
                {
                    if (sArr0[i] != sArr1[i])
                    {
                        return false;
                    }
                }
            }

            return true;
        }


        public static void copyVertex(VTXL sourceVTXL, VTXL destinationVTXL, int sourceIndex, int destinationIndex)
        {
            if (sourceVTXL.vertPositions.Count > sourceIndex)
            {
                destinationVTXL.vertPositions[destinationIndex] = sourceVTXL.vertPositions[sourceIndex];
            }
            if (sourceVTXL.vertWeightsNGS.Count > sourceIndex)
            {
                destinationVTXL.vertWeightsNGS[destinationIndex] = sourceVTXL.vertWeightsNGS[sourceIndex];
            }
            if (sourceVTXL.vertWeights.Count > sourceIndex)
            {
                destinationVTXL.vertWeights[destinationIndex] = sourceVTXL.vertWeights[sourceIndex];
            }
            if (sourceVTXL.vertNormalsNGS.Count > sourceIndex)
            {
                destinationVTXL.vertNormalsNGS[destinationIndex] = sourceVTXL.vertNormalsNGS[sourceIndex];
            }
            if (sourceVTXL.vertNormals.Count > sourceIndex)
            {
                destinationVTXL.vertNormals[destinationIndex] = sourceVTXL.vertNormals[sourceIndex];
            }
            if (sourceVTXL.vertColors.Count > sourceIndex)
            {
                destinationVTXL.vertColors[destinationIndex] = sourceVTXL.vertColors[sourceIndex];
            }
            if (sourceVTXL.vertColor2s.Count > sourceIndex)
            {
                destinationVTXL.vertColor2s[destinationIndex] = sourceVTXL.vertColor2s[sourceIndex];
            }
            if (sourceVTXL.vertWeightIndices.Count > sourceIndex)
            {
                destinationVTXL.vertWeightIndices[destinationIndex] = sourceVTXL.vertWeightIndices[sourceIndex];
            }
            if (sourceVTXL.uv1ListNGS.Count > sourceIndex)
            {
                destinationVTXL.uv1ListNGS[destinationIndex] = sourceVTXL.uv1ListNGS[sourceIndex];
            }
            if (sourceVTXL.uv1List.Count > sourceIndex)
            {
                destinationVTXL.uv1List[destinationIndex] = sourceVTXL.uv1List[sourceIndex];
            }
            if (sourceVTXL.uv2List.Count > sourceIndex)
            {
                destinationVTXL.uv2List[destinationIndex] = sourceVTXL.uv2List[sourceIndex];
            }
            if (sourceVTXL.uv3List.Count > sourceIndex)
            {
                destinationVTXL.uv3List[destinationIndex] = sourceVTXL.uv3List[sourceIndex];
            }
            if (sourceVTXL.uv4List.Count > sourceIndex)
            {
                destinationVTXL.uv4List[destinationIndex] = sourceVTXL.uv4List[sourceIndex];
            }
            if (sourceVTXL.vert0x22.Count > sourceIndex)
            {
                destinationVTXL.vert0x22[destinationIndex] = sourceVTXL.vert0x22[sourceIndex];
            }
            if (sourceVTXL.vert0x23.Count > sourceIndex)
            {
                destinationVTXL.vert0x23[destinationIndex] = sourceVTXL.vert0x23[sourceIndex];
            }
            if (sourceVTXL.vertTangentListNGS.Count > sourceIndex)
            {
                destinationVTXL.vertTangentListNGS[destinationIndex] = sourceVTXL.vertTangentListNGS[sourceIndex];
            }
            if (sourceVTXL.vertTangentList.Count > sourceIndex)
            {
                destinationVTXL.vertTangentList[destinationIndex] = sourceVTXL.vertTangentList[sourceIndex];
            }
            if (sourceVTXL.vertBinormalListNGS.Count > sourceIndex)
            {
                destinationVTXL.vertBinormalListNGS[destinationIndex] = sourceVTXL.vertBinormalListNGS[sourceIndex];
            }
            if (sourceVTXL.vertBinormalList.Count > sourceIndex)
            {
                destinationVTXL.vertBinormalList[destinationIndex] = sourceVTXL.vertBinormalList[sourceIndex];
            }
            if (sourceVTXL.rawVertWeights.Count > sourceIndex)
            {
                destinationVTXL.rawVertWeights[destinationIndex] = sourceVTXL.rawVertWeights[sourceIndex];
            }
            if (sourceVTXL.rawVertWeightIds.Count > sourceIndex)
            {
                destinationVTXL.rawVertWeightIds[destinationIndex] = sourceVTXL.rawVertWeightIds[sourceIndex];
            }
            if (sourceVTXL.trueVertWeights.Count > sourceIndex)
            {
                destinationVTXL.trueVertWeights[destinationIndex] = sourceVTXL.trueVertWeights[sourceIndex];
            }
            if (sourceVTXL.trueVertWeightIndices.Count > sourceIndex)
            {
                destinationVTXL.trueVertWeightIndices[destinationIndex] = sourceVTXL.trueVertWeightIndices[sourceIndex];
            }
        }

        public static void appendVertex(VTXL sourceVTXL, VTXL destinationVTXL, int sourceIndex)
        {
            if (sourceVTXL.vertPositions.Count > sourceIndex)
            {
                destinationVTXL.vertPositions.Add(sourceVTXL.vertPositions[sourceIndex]);
            }
            if (sourceVTXL.vertWeightsNGS.Count > sourceIndex)
            {
                destinationVTXL.vertWeightsNGS.Add(sourceVTXL.vertWeightsNGS[sourceIndex]);
            }
            if (sourceVTXL.vertWeights.Count > sourceIndex)
            {
                destinationVTXL.vertWeights.Add(sourceVTXL.vertWeights[sourceIndex]);
            }
            if (sourceVTXL.vertNormalsNGS.Count > sourceIndex)
            {
                destinationVTXL.vertNormalsNGS.Add(sourceVTXL.vertNormalsNGS[sourceIndex]);
            }
            if (sourceVTXL.vertNormals.Count > sourceIndex)
            {
                destinationVTXL.vertNormals.Add(sourceVTXL.vertNormals[sourceIndex]);
            }
            if (sourceVTXL.vertColors.Count > sourceIndex)
            {
                destinationVTXL.vertColors.Add(sourceVTXL.vertColors[sourceIndex]);
            }
            if (sourceVTXL.vertColor2s.Count > sourceIndex)
            {
                destinationVTXL.vertColor2s.Add(sourceVTXL.vertColor2s[sourceIndex]);
            }
            if (sourceVTXL.vertWeightIndices.Count > sourceIndex)
            {
                destinationVTXL.vertWeightIndices.Add(sourceVTXL.vertWeightIndices[sourceIndex]);
            }
            if (sourceVTXL.uv1ListNGS.Count > sourceIndex)
            {
                destinationVTXL.uv1ListNGS.Add(sourceVTXL.uv1ListNGS[sourceIndex]);
            }
            if (sourceVTXL.uv1List.Count > sourceIndex)
            {
                destinationVTXL.uv1List.Add(sourceVTXL.uv1List[sourceIndex]);
            }
            if (sourceVTXL.uv2List.Count > sourceIndex)
            {
                destinationVTXL.uv2List.Add(sourceVTXL.uv2List[sourceIndex]);
            }
            if (sourceVTXL.uv3List.Count > sourceIndex)
            {
                destinationVTXL.uv3List.Add(sourceVTXL.uv3List[sourceIndex]);
            }
            if (sourceVTXL.uv4List.Count > sourceIndex)
            {
                destinationVTXL.uv4List.Add(sourceVTXL.uv4List[sourceIndex]);
            }
            if (sourceVTXL.vert0x22.Count > sourceIndex)
            {
                destinationVTXL.vert0x22.Add(sourceVTXL.vert0x22[sourceIndex]);
            }
            if (sourceVTXL.vert0x23.Count > sourceIndex)
            {
                destinationVTXL.vert0x23.Add(sourceVTXL.vert0x23[sourceIndex]);
            }
            if (sourceVTXL.vertTangentListNGS.Count > sourceIndex)
            {
                destinationVTXL.vertTangentListNGS.Add(sourceVTXL.vertTangentListNGS[sourceIndex]);
            }
            if (sourceVTXL.vertTangentList.Count > sourceIndex)
            {
                destinationVTXL.vertTangentList.Add(sourceVTXL.vertTangentList[sourceIndex]);
            }
            if (sourceVTXL.vertBinormalListNGS.Count > sourceIndex)
            {
                destinationVTXL.vertBinormalListNGS.Add(sourceVTXL.vertBinormalListNGS[sourceIndex]);
            }
            if (sourceVTXL.vertBinormalList.Count > sourceIndex)
            {
                destinationVTXL.vertBinormalList.Add(sourceVTXL.vertBinormalList[sourceIndex]);
            }
            if (sourceVTXL.rawVertWeights.Count > sourceIndex)
            {
                destinationVTXL.rawVertWeights.Add(sourceVTXL.rawVertWeights[sourceIndex]);
            }
            if (sourceVTXL.rawVertWeightIds.Count > sourceIndex)
            {
                destinationVTXL.rawVertWeightIds.Add(sourceVTXL.rawVertWeightIds[sourceIndex]);
            }
            if (sourceVTXL.trueVertWeights.Count > sourceIndex)
            {
                destinationVTXL.trueVertWeights.Add(sourceVTXL.trueVertWeights[sourceIndex]);
            }
            if (sourceVTXL.trueVertWeightIndices.Count > sourceIndex)
            {
                destinationVTXL.trueVertWeightIndices.Add(sourceVTXL.trueVertWeightIndices[sourceIndex]);
            }
        }

        public static bool IsSameVertex(VTXL vtxl, int vertIndex, VTXL vtxl2, int faceVertIndex)
        {
            if (vtxl.vertNormals.Count > 0 && !vtxl.vertNormals[vertIndex].Equals(vtxl2.vertNormals[faceVertIndex]))
            {
                return false;
            }
            if (vtxl.vertNormalsNGS.Count > 0 && !vtxl.vertNormalsNGS[vertIndex].Equals(vtxl2.vertNormalsNGS[faceVertIndex]))
            {
                return false;
            }
            if (vtxl.vertColors.Count > 0 && !IsEqualByteArray(vtxl.vertColors[vertIndex], vtxl2.vertColors[faceVertIndex]))
            {
                return false;
            }
            if (vtxl.vertColor2s.Count > 0 && !IsEqualByteArray(vtxl.vertColor2s[vertIndex], vtxl2.vertColor2s[faceVertIndex]))
            {
                return false;
            }
            if (vtxl.uv1List.Count > 0 && !vtxl.uv1List[vertIndex].Equals(vtxl2.uv1List[faceVertIndex]))
            {
                return false;
            }
            if (vtxl.uv1ListNGS.Count > 0 && !IsEqualShortArray(vtxl.uv1ListNGS[vertIndex], vtxl2.uv1ListNGS[faceVertIndex]))
            {
                return false;
            }
            if (vtxl.uv2ListShort.Count > 0 && !IsEqualShortArray(vtxl.uv2ListShort[vertIndex], vtxl2.uv2ListShort[faceVertIndex]))
            {
                return false;
            }
            if (vtxl.uv2List.Count > 0 && !vtxl.uv2List[vertIndex].Equals(vtxl2.uv2List[faceVertIndex]))
            {
                return false;
            }
            if (vtxl.uv3List.Count > 0 && !vtxl.uv3List[vertIndex].Equals(vtxl2.uv3List[faceVertIndex]))
            {
                return false;
            }
            if (vtxl.uv4List.Count > 0 && !vtxl.uv4List[vertIndex].Equals(vtxl2.uv4List[faceVertIndex]))
            {
                return false;
            }
            if (vtxl.vert0x22.Count > 0 && !IsEqualShortArray(vtxl.vert0x22[vertIndex], vtxl2.vert0x22[faceVertIndex]))
            {
                return false;
            }
            if (vtxl.vert0x23.Count > 0 && !IsEqualShortArray(vtxl.vert0x23[vertIndex], vtxl2.vert0x23[faceVertIndex]))
            {
                return false;
            }

            return true;
        }

        public static VTXE ConstructClassicVTXE(VTXL vtxl, out int vertSize)
        {
            int curLength = 0;
            VTXE vtxe = new VTXE();

            if(vtxl.vertPositions.Count > 0)
            {
                vtxe.vertDataTypes.Add(vtxeElementGenerator(0x0, 0x3, curLength));
                curLength += 0xC;
            }
            if (vtxl.vertWeights.Count > 0)
            {
                vtxe.vertDataTypes.Add(vtxeElementGenerator(0x1, 0x4, curLength));
                curLength += 0x10;
            }
            if (vtxl.vertNormals.Count > 0)
            {
                vtxe.vertDataTypes.Add(vtxeElementGenerator(0x2, 0x3, curLength));
                curLength += 0xC;
            }
            if (vtxl.vertColors.Count > 0)
            {
                vtxe.vertDataTypes.Add(vtxeElementGenerator(0x3, 0x5, curLength));
                curLength += 0x4;
            }
            if (vtxl.vertColor2s.Count > 0)
            {
                vtxe.vertDataTypes.Add(vtxeElementGenerator(0x4, 0x5, curLength));
                curLength += 0x4;
            }
            if (vtxl.vertWeightIndices.Count > 0)
            {
                vtxe.vertDataTypes.Add(vtxeElementGenerator(0xb, 0x7, curLength));
                curLength += 0x4;
            }
            if (vtxl.uv1List.Count > 0)
            {
                vtxe.vertDataTypes.Add(vtxeElementGenerator(0x10, 0x2, curLength));
                curLength += 0x8;
            }
            if (vtxl.uv2List.Count > 0)
            {
                vtxe.vertDataTypes.Add(vtxeElementGenerator(0x11, 0x2, curLength));
                curLength += 0x8;
            }
            if (vtxl.uv3List.Count > 0)
            {
                vtxe.vertDataTypes.Add(vtxeElementGenerator(0x12, 0x2, curLength));
                curLength += 0x8;
            }
            if (vtxl.uv4List.Count > 0)
            {
                vtxe.vertDataTypes.Add(vtxeElementGenerator(0x13, 0x2, curLength));
                curLength += 0x8;
            }
            if (vtxl.vertTangentList.Count > 0)
            {
                vtxe.vertDataTypes.Add(vtxeElementGenerator(0x20, 0x3, curLength));
                curLength += 0xC;
            }
            if (vtxl.vertBinormalList.Count > 0)
            {
                vtxe.vertDataTypes.Add(vtxeElementGenerator(0x21, 0x3, curLength));
                curLength += 0xC;
            }

            vertSize = curLength;
            return vtxe;
        }

        public static VTXEElement vtxeElementGenerator(int dataType, int structType, int relativeAddress)
        {
            VTXEElement vtxeEle = new VTXEElement();
            vtxeEle.dataType = dataType;
            vtxeEle.structVariation = structType;
            vtxeEle.relativeAddress = relativeAddress;
            vtxeEle.reserve0 = 0;

            return vtxeEle;
        }

        public static void ReadVTXL(BufferedStreamReader streamReader, VTXE vtxeSet, VTXL vtxl, int vertCount, int vertTypeCount, int sizeToFill = -1)
        {
            for (int vtxlIndex = 0; vtxlIndex < vertCount; vtxlIndex++)
            {
                long startPosition = streamReader.Position();
                for (int vtxeIndex = 0; vtxeIndex < vertTypeCount; vtxeIndex++)
                {
                    switch (vtxeSet.vertDataTypes[vtxeIndex].dataType)
                    {
                        case (int)ClassicAquaObject.VertFlags.VertPosition:
                            vtxl.vertPositions.Add(streamReader.Read<Vector3>());
                            break;
                        case (int)ClassicAquaObject.VertFlags.VertWeight:
                            switch(vtxeSet.vertDataTypes[vtxeIndex].structVariation)
                            {
                                case 0x4:
                                    vtxl.vertWeights.Add(streamReader.Read<Vector4>());
                                    break;
                                case 0x11:
                                    vtxl.vertWeightsNGS.Add(Read4UShorts(streamReader));
                                    break;
                                default:
                                    throw new Exception($"Unexpected vert weight struct type {vtxeSet.vertDataTypes[vtxeIndex].structVariation}");
                            }
                            break;
                        case (int)ClassicAquaObject.VertFlags.VertNormal:
                            switch (vtxeSet.vertDataTypes[vtxeIndex].structVariation)
                            {
                                case 0x3:
                                    vtxl.vertNormals.Add(streamReader.Read<Vector3>());
                                    break;
                                case 0xF:
                                    vtxl.vertNormalsNGS.Add(Read4Shorts(streamReader));
                                    break;
                                default:
                                    throw new Exception($"Unexpected vert normal struct type {vtxeSet.vertDataTypes[vtxeIndex].structVariation}");
                            }
                            break;
                        case (int)ClassicAquaObject.VertFlags.VertColor:
                            vtxl.vertColors.Add(Read4Bytes(streamReader));
                            break;
                        case (int)ClassicAquaObject.VertFlags.VertColor2:
                            vtxl.vertColor2s.Add(Read4Bytes(streamReader));
                            break;
                        case (int)ClassicAquaObject.VertFlags.VertWeightIndex:
                            vtxl.vertWeightIndices.Add(Read4Bytes(streamReader));
                            break;
                        case (int)ClassicAquaObject.VertFlags.VertUV1:
                            switch (vtxeSet.vertDataTypes[vtxeIndex].structVariation)
                            {
                                case 0x2:
                                    vtxl.uv1List.Add(streamReader.Read<Vector2>());
                                    break;
                                case 0xE:
                                    vtxl.uv1ListNGS.Add(Read2Shorts(streamReader));
                                    break;
                                default:
                                    throw new Exception($"Unexpected vert uv1 struct type {vtxeSet.vertDataTypes[vtxeIndex].structVariation}");
                            }
                            break;
                        case (int)ClassicAquaObject.VertFlags.VertUV2:
                            switch (vtxeSet.vertDataTypes[vtxeIndex].structVariation)
                            {
                                case 0x2:
                                    vtxl.uv2List.Add(streamReader.Read<Vector2>());
                                    break;
                                case 0xE:
                                    vtxl.uv2ListShort.Add(Read2Shorts(streamReader));
                                    break;
                                default:
                                    throw new Exception($"Unexpected vert uv1 struct type {vtxeSet.vertDataTypes[vtxeIndex].structVariation}");
                            }
                            break;
                        case (int)ClassicAquaObject.VertFlags.VertUV3:
                            vtxl.uv3List.Add(streamReader.Read<Vector2>());
                            break;
                        case (int)NGSAquaObject.NGSVertFlags.VertUV4:
                            vtxl.uv3List.Add(streamReader.Read<Vector2>());
                            break;
                        case (int)ClassicAquaObject.VertFlags.VertTangent:
                            switch (vtxeSet.vertDataTypes[vtxeIndex].structVariation)
                            {
                                case 0x3:
                                    vtxl.vertTangentList.Add(streamReader.Read<Vector3>());
                                    break;
                                case 0xF:
                                    vtxl.vertTangentListNGS.Add(Read4Shorts(streamReader));
                                    break;
                                default:
                                    throw new Exception($"Unexpected vert tangent struct type {vtxeSet.vertDataTypes[vtxeIndex].structVariation}");
                            }
                            break;
                        case (int)ClassicAquaObject.VertFlags.VertBinormal:
                            switch (vtxeSet.vertDataTypes[vtxeIndex].structVariation)
                            {
                                case 0x3:
                                    vtxl.vertBinormalList.Add(streamReader.Read<Vector3>());
                                    break;
                                case 0xF:
                                    vtxl.vertBinormalListNGS.Add(Read4Shorts(streamReader));
                                    break;
                                default:
                                    throw new Exception($"Unexpected vert binormal struct type {vtxeSet.vertDataTypes[vtxeIndex].structVariation}");
                            }
                            break;
                        case (int)NGSAquaObject.NGSVertFlags.Vert0x22:
                            vtxl.vert0x22.Add(Read2Shorts(streamReader));
                            break;
                        case (int)NGSAquaObject.NGSVertFlags.Vert0x23:
                            vtxl.vert0x23.Add(Read2Shorts(streamReader));
                            break;
                        default:
                            MessageBox.Show($"Unknown Vert type {vtxeSet.vertDataTypes[vtxeIndex].dataType.ToString("X")}! Please report!");
                            break;
                    }
                }

                //Ensure for 0xC33 variants that we seek to the end of each entry
                if(sizeToFill > 0)
                {
                    streamReader.Seek(startPosition + sizeToFill, System.IO.SeekOrigin.Begin);
                }
            }

            //Process 0xC33 variants for later use
            if(sizeToFill > 0)
            {
                vtxl.convertToLegacyTypes();
            }
            vtxl.createTrueVertWeights();
        }

        public static BoundingVolume GenerateBounding(List<VTXL> vertData)
        {
            BoundingVolume bounds = new BoundingVolume();
            Vector3 maxPoint = new Vector3();
            Vector3 minPoint = new Vector3();
            Vector3 difference = new Vector3();
            Vector3 center = new Vector3();
            float radius = 0;

            for (int vset = 0; vset < vertData.Count; vset++)
            {
                for (int vert = 0; vert < vertData[vset].vertPositions.Count; vert++)
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
                    if (distance > radius)
                    {
                        radius = distance;
                    }
                }
            }

            bounds.modelCenter = center;
            bounds.modelCenter2 = center;
            bounds.halfExtents = difference;
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
            for (int mesh = 0; mesh < model.vsetList.Count; mesh++)
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
                        if (area < 0)
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
                        for (int i = 0; i < 3; i++)
                        {
                            if (vertBinormalArray[faceIndices[i]] == null)
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
                    for (int vert = 0; vert < model.vtxlList[vsetIndex].vertPositions.Count; vert++)
                    {
                        vertBinormalArray[vert] = Vector3.Normalize(vertBinormalArray[vert]);
                        vertTangentArray[vert] = Vector3.Normalize(vertTangentArray[vert]);
                        vertFaceNormalArray[vert] = Vector3.Normalize(vertFaceNormalArray[vert]);
                    }

                    //Set these back in the model
                    if (useFaceNormals)
                    {
                        model.vtxlList[vsetIndex].vertNormals = vertFaceNormalArray.ToList();
                    }
                    model.vtxlList[vsetIndex].vertBinormalList = vertBinormalArray.ToList();
                    model.vtxlList[vsetIndex].vertTangentList = vertTangentArray.ToList();
                }
            }
        }

        public static void splitMesh(AquaObject model, AquaObject outModel, int modelId, List<List<int>> facesToClone)
        {
            if (facesToClone.Count > 0)
            {
                for (int f = 0; f < facesToClone.Count; f++)
                {
                    Dictionary<float, int> faceVertDict = new Dictionary<float, int>();
                    List<int> faceVertIds = new List<int>();

                    List<int> tempFaceMatIds = new List<int>();
                    List<Vector3> tempFaces = new List<Vector3>();

                    int vertIndex = 0;
                    if (facesToClone[f].Count > 0)
                    {
                        //Get vert ids
                        for (int i = 0; i < facesToClone[f].Count; i++)
                        {
                            Vector3 face = model.tempTris[modelId].triList[facesToClone[f][i]];
                            if (!faceVertIds.Contains((int)face.X))
                            {
                                faceVertDict.Add(face.X, vertIndex++);
                                faceVertIds.Add((int)face.X);
                            }
                            if (!faceVertIds.Contains((int)face.Y))
                            {
                                faceVertDict.Add(face.Y, vertIndex++);
                                faceVertIds.Add((int)face.Y);
                            }
                            if (!faceVertIds.Contains((int)face.Z))
                            {
                                faceVertDict.Add(face.Z, vertIndex++);
                                faceVertIds.Add((int)face.Z);
                            }

                            tempFaceMatIds.Add(model.tempTris[modelId].matIdList[facesToClone[f][i]]);
                            tempFaces.Add(face);
                        }

                        //Remap Ids based based on the indices of the values in faceVertIds and add to outModel
                        for (int i = 0; i < tempFaces.Count; i++)
                        {
                            tempFaces[i] = new Vector3(faceVertDict[tempFaces[i].X], faceVertDict[tempFaces[i].Y], faceVertDict[tempFaces[i].Z]);
                        }

                        //Assign new tempTris
                        var newTris = new GenericTriangles(tempFaces, tempFaceMatIds);
                        newTris.baseMeshDummyId = model.tempTris[modelId].baseMeshDummyId;
                        newTris.baseMeshNodeId = model.tempTris[modelId].baseMeshNodeId;
                        outModel.tempTris.Add(newTris);

                        //Copy vertex data based on faceVertIds ordering
                        VTXL vtxl = new VTXL();
                        for (int i = 0; i < faceVertIds.Count; i++)
                        {
                            appendVertex(model.vtxlList[modelId], vtxl, faceVertIds[i]);
                        }

                        //Add things that aren't linked to the vertex ids
                        if (model.vtxlList[modelId].bonePalette != null)
                        {
                            vtxl.bonePalette = model.vtxlList[modelId].bonePalette;
                        }
                        if (model.vtxlList[modelId].edgeVerts != null)
                        {
                            vtxl.edgeVerts = model.vtxlList[modelId].edgeVerts;
                        }

                        outModel.vtxlList.Add(vtxl);
                    }
                }

            }


        }

        public static void SplitByBoneCount(AquaObject model, AquaObject outModel, int modelId, int boneLimit)
        {
            List<List<int>> faceLists = new List<List<int>>();
            bool[] usedFaceIds = new bool[model.tempTris[modelId].triList.Count];
            int startFace = 0;

            while (true)
            {
                List<int> objBones = new List<int>();
                List<int> faceArray = new List<int>();

                //Find start point for this iteration
                for (int i = 0; i < usedFaceIds.Length; i++)
                {
                    if (!usedFaceIds[i])
                    {
                        startFace = i;
                        break;
                    }
                }

                for (int f = startFace; f < usedFaceIds.Length; f++)
                {
                    //Used to watch how many bones are being used so it can be understood where to split the meshes.
                    //These also help to track the bones being used so the edge verts list can be generated. Not sure how important that is, but the game stores it.
                    List<int> faceBones = new List<int>();
                    int newBones = 0;

                    //Get vert ids of the face
                    float[] faceIds = VectorAsArray(model.tempTris[modelId].triList[f]);
                    List<int> edgeVertCandidates = new List<int>();

                    //Get bones in the face
                    foreach (float v in faceIds)
                    {
                        bool vertCheck = false;
                        for (int b = 0; b < model.vtxlList[modelId].vertWeightIndices[(int)v].Length; b++)
                        {
                            int id = model.vtxlList[modelId].vertWeightIndices[(int)v][b];
                            if (!faceBones.Contains(id))
                            {
                                faceBones.Add(id);
                            }

                            //If it's not in here, it's a candidate to be an edgeVert
                            if (!objBones.Contains(id))
                            {
                                vertCheck = true;
                            }
                        }

                        //Add edge vert candidates 
                        if (vertCheck)
                        {
                            edgeVertCandidates.Add((int)v);
                        }
                    }

                    //Check for bones in the face that weren't in the obj bone set yet
                    foreach (int fb in faceBones)
                    {
                        if (!objBones.Contains(fb))
                        {
                            newBones++;
                        }
                    }

                    //If enough space or no new, add to the object amount as well
                    if (newBones + objBones.Count <= boneLimit)
                    {
                        foreach (int fb in faceBones)
                        {
                            if (!objBones.Contains(fb))
                            {
                                objBones.Add(fb);
                            }
                        }
                        usedFaceIds[f] = true;
                        faceArray.Add(f);
                    } else
                    {
                        foreach (int vert in edgeVertCandidates)
                        {
                            model.vtxlList[modelId].edgeVerts.Add((ushort)vert);
                        }
                    }
                }
                faceLists.Add(faceArray);

                bool breakFromLoop = true;
                for (int i = 0; i < usedFaceIds.Length; i++)
                {
                    if (!usedFaceIds[i])
                    {
                        breakFromLoop = false;
                    }
                }
                if (breakFromLoop)
                {
                    break;
                }
            }

            //Split the meshes
            splitMesh(model, outModel, modelId, faceLists);
        }

        public static void BatchSplitByBoneCount(AquaObject model, AquaObject outModel, int boneLimit)
        {
            for (int i = 0; i < model.vtxlList.Count; i++)
            {
                RemoveUnusedBones(model.vtxlList[i]);
                //Pass to splitting function if beyond the limit, otherwise pass untouched
                if (model.vtxlList[i].rawVertWeightIds != null && model.vtxlList[i].bonePalette.Count > boneLimit)
                {
                    SplitByBoneCount(model, outModel, i, boneLimit);
                } else
                {
                    CloneUnprocessedMesh(model, outModel, i);
                }
            }
            outModel.tempMats = model.tempMats;
        }

        public static void SplitMeshByMaterial(AquaObject model, AquaObject outModel)
        {
            for (int i = 0; i < model.tempTris.Count; i++)
            {
                splitMesh(model, outModel, i, GetAllMaterialFaceGroups(model, i));
            }
            outModel.tempMats = model.tempMats;
        }

        public static List<List<int>> GetAllMaterialFaceGroups(AquaObject model, int meshId)
        {
            List<List<int>> matGroups = new List<List<int>>();
            for(int i = 0; i < model.tempMats.Count; i++)
            {
                matGroups.Add(new List<int>());
            }

            for(int i = 0; i < model.tempTris[meshId].matIdList.Count; i++)
            {
                //create list of face groups based on material data. Assume material ids are preprocessed to fit what will be used for the final AquaObject.
                matGroups[model.tempTris[meshId].matIdList[i]].Add(i);
            }

            return matGroups;
        }

        public static void splitOptimizedFaceGroups(AquaObject model, int faceLimit)
        {
            for(int i = 0; i < model.strips.Count; i++)
            {
                List<Vector3> tris = model.strips[i].getTriangles(true);

                if(tris.Count > faceLimit)
                {

                }
            }

        }

        public static void splitOptimizedVertGroups(AquaObject model, int vertLimit)
        {
            for (int i = 0; i < model.vtxlList.Count; i++)
            {
                if(model.vtxlList[i].vertPositions.Count > vertLimit)
                {
                    int vertSplitCount = model.vtxlList[i].vertPositions.Count / vertLimit;

                    //for(int j = 0; j < )
                }
            }

        }

        public static void CloneUnprocessedMesh(AquaObject model, AquaObject outModel, int meshId)
        {
            outModel.vtxlList.Add(model.vtxlList[meshId]);
            outModel.tempTris.Add(model.tempTris[meshId]);
        }

        //To be honest I don't really know what these actually do, but this seems to generate the structure roughly the way the game's exporter does.
        //Essentially, vertices between different meshes are linked together 
        public static void CalcUNRMs(AquaObject model, bool applyNormalAveraging, bool useUNRMs)
        {
            UNRM unrm = new UNRM();

            //Set up a boolean array for tracking what we've gone through for this
            bool[][] meshCheckArr = new bool[model.vtxlList.Count][];
            for (int m = 0; m < model.vtxlList.Count; m++)
            {
                meshCheckArr[m] = new bool[model.vtxlList[m].vertPositions.Count];
            }

            for (int m = 0; m < model.vtxlList.Count; m++)
            {
                for (int v = 0; v < model.vtxlList[m].vertPositions.Count; v++)
                {
                    Vector3 normals = new Vector3(); // Vector3s cannot be null
                    if(model.vtxlList[m].vertNormals.Count > 0)
                    {
                        normals = model.vtxlList[m].vertNormals[v];
                    }

                    List<int> meshNum = new List<int>() { m };
                    List<int> vertId = new List<int>() { v };
                    //Loop through the other vertices to match them up
                    for (int n = 0; n < model.vtxlList.Count; n++)
                    {
                        for (int w = 0; w < model.vtxlList[n].vertPositions.Count; w++)
                        {
                            bool sameVert = (m == n && v == w);
                            if (!sameVert && model.vtxlList[n].vertPositions[w].Equals(model.vtxlList[m].vertPositions[v]) && !meshCheckArr[n][w])
                            {
                                meshCheckArr[n][w] = true;
                                meshNum.Add(n);
                                vertId.Add(w);
                                if (model.vtxlList[n].vertNormals.Count > 0 && applyNormalAveraging)
                                {
                                    normals += model.vtxlList[n].vertNormals[w];
                                }
                            }
                        }
                    }
                    meshCheckArr[m][v] = true;

                    //UNRM groups are only valid if there's more than 1, ie more than one vertex linked by position.
                    if(meshNum.Count > 1)
                    {
                        unrm.vertGroupCountCount++;
                        unrm.vertCount += meshNum.Count;
                        unrm.unrmMeshIds.Add(meshNum);
                        unrm.unrmVertIds.Add(vertId);
                        if(applyNormalAveraging)
                        {
                            normals = Vector3.Normalize(normals);
                            for(int i = 0; i < meshNum.Count; i++)
                            {
                                model.vtxlList[meshNum[i]].vertNormals[vertId[i]] = normals;
                            }
                        }
                    }
                }
            }

            //Only actually apply them if we choose to. This function may just be used for averaging normals.
            if(useUNRMs)
            {
                model.unrms = unrm;
            }
        }

        //Removes bones in vtxls with unprocessed weights
        public static void RemoveUnusedBones(VTXL vtxl)
        {
            Dictionary<int, int> oldToNewId = new Dictionary<int, int>();
            List<int> bonesIndicesUsed = new List<int>();
            List<ushort> bonePaletteClone = new List<ushort>();
            bonePaletteClone.AddRange(vtxl.bonePalette);
            
            
            //Check for and collect bones which are referenced
            for(int i = 0; i < vtxl.rawVertWeightIds.Count; i++)
            {
                for(int j = 0; j < vtxl.rawVertWeightIds[i].Count; j++)
                {
                    if(!bonesIndicesUsed.Contains(vtxl.rawVertWeightIds[i][j]))
                    {
                        bonesIndicesUsed.Add(vtxl.rawVertWeightIds[i][j]);
                    }
                }
            }

            //Remove bones from bonePalette and generate oldToNewId dictionary based on differences
            int removeCounter = 0;
            for(int i = 0; i < vtxl.bonePalette.Count; i++)
            {
                if(bonesIndicesUsed.Contains(i))
                {
                    oldToNewId.Add(i, i + removeCounter);
                } else
                {
                    bonePaletteClone.RemoveAt(i + removeCounter);
                    removeCounter--;
                }
            }
            vtxl.bonePalette = bonePaletteClone;

            //Loop through again and replace ids with their new equivalents
            for (int i = 0; i < vtxl.rawVertWeights.Count; i++)
            {
                for (int j = 0; j < vtxl.rawVertWeights[i].Count; j++)
                {
                    vtxl.rawVertWeightIds[i][j] = oldToNewId[vtxl.rawVertWeightIds[i][j]];
                }
            }
        }

        public static void WriteClassicVTXL(VTXE vtxe, VTXL vtxl, List<byte> outBytes2)
        {
            for (int i = 0; i < vtxl.vertPositions.Count; i++)
            {
                for (int j = 0; j < vtxe.vertDataTypes.Count; j++)
                {
                    switch (vtxe.vertDataTypes[j].dataType)
                    {
                        case (int)ClassicAquaObject.VertFlags.VertPosition:
                            outBytes2.AddRange(ConvertStruct(vtxl.vertPositions[i]));
                            break;
                        case (int)ClassicAquaObject.VertFlags.VertWeight:
                            outBytes2.AddRange(ConvertStruct(vtxl.vertWeights[i]));
                            break;
                        case (int)ClassicAquaObject.VertFlags.VertNormal:
                            outBytes2.AddRange(ConvertStruct(vtxl.vertNormals[i]));
                            break;
                        case (int)ClassicAquaObject.VertFlags.VertColor:
                            for (int color = 0; color < 4; color++)
                            {
                                outBytes2.Add(vtxl.vertColors[i][color]);
                            }
                            break;
                        case (int)ClassicAquaObject.VertFlags.VertColor2:
                            for (int color = 0; color < 4; color++)
                            {
                                outBytes2.Add(vtxl.vertColor2s[i][color]);
                            }
                            break;
                        case (int)ClassicAquaObject.VertFlags.VertWeightIndex:
                            for (int weight = 0; weight < 4; weight++)
                            {
                                outBytes2.Add(vtxl.vertWeightIndices[i][weight]);
                            }
                            break;
                        case (int)ClassicAquaObject.VertFlags.VertUV1:
                            outBytes2.AddRange(ConvertStruct(vtxl.uv1List[i]));
                            break;
                        case (int)ClassicAquaObject.VertFlags.VertUV2:
                            outBytes2.AddRange(ConvertStruct(vtxl.uv2List[i]));
                            break;
                        case (int)ClassicAquaObject.VertFlags.VertUV3:
                            outBytes2.AddRange(ConvertStruct(vtxl.uv3List[i]));
                            break;
                        case (int)ClassicAquaObject.VertFlags.VertUV4:
                            outBytes2.AddRange(ConvertStruct(vtxl.uv4List[i]));
                            break;
                        case (int)ClassicAquaObject.VertFlags.VertTangent:
                            outBytes2.AddRange(ConvertStruct(vtxl.vertTangentList[i]));
                            break;
                        case (int)ClassicAquaObject.VertFlags.VertBinormal:
                            outBytes2.AddRange(ConvertStruct(vtxl.vertBinormalList[i]));
                            break;
                        default:
                            MessageBox.Show($"Unknown Vert type {vtxe.vertDataTypes[j].dataType}! Please report!");
                            throw new Exception("Not implemented!");
                    }


                }
            }
        }

        public static string GetPSO2String(byte* str)
        {
            string finalText;

            //Lazily determine string end
            int end = GetPSO2StringLength(str);

            byte[] text = new byte[end];
            Marshal.Copy(new IntPtr(str), text, 0, end);
            finalText = System.Text.Encoding.UTF8.GetString(text);

            return finalText;
        }

        public static int GetPSO2StringLength(byte* str)
        {
            int end = 0;
            for (int j = 0; j < 0x20; j++)
            {
                if (str[j] == 0)
                {
                    end = j;
                    break;
                }
            }

            return end;
        }

        //Used to generate materials like the various player materials in which the actual texture names are not used.
        public static void GenerateSpecialMaterialParameters(GenericMaterial mat)
        {
            List<string> texNames = new List<string>();
            switch(mat.specialType)
            {
                case "ey":
                case "eye":
                    if (mat.shaderNames == null)
                    {
                        mat.shaderNames = new List<string>() { "0106p", "0106" };
                    }
                    texNames.Add("pl_eye_diffuse.dds");
                    texNames.Add("pl_eye_multi.dds");
                    texNames.Add("pl_eye_env.dds");
                    texNames.Add("pl_leye_env.dds");
                    break;
                case "hr":
                case "hair":
                    //Fun fact, hair just seemingly doesn't note the other extra stuff it uses and the game figures it out at runtime. 
                    //If the texlist seems too short, that's why.
                    if (mat.shaderNames == null)
                    {
                        mat.shaderNames = new List<string>() { "0103p", "0103" };
                    }
                    texNames.Add("pl_hair_diffuse.dds");
                    texNames.Add("pl_hair_multi.dds");
                    texNames.Add("pl_hair_normal.dds");
                    break;
                case "fc":
                case "face":
                    if (mat.shaderNames == null)
                    {
                        mat.shaderNames = new List<string>() { "0101p", "0101" };
                    }
                    texNames.Add("pl_face_diffuse.dds");
                    texNames.Add("pl_face_multi.dds");
                    texNames.Add("pl_face_normal.dds");
                    break;
                case "pl":
                case "player":
                case "bd":
                case "ba":
                case "ou":
                    if(mat.shaderNames == null)
                    {
                        mat.shaderNames = new List<string>() { "0100p", "0100" };
                    }
                    texNames.Add("pl_body_diffuse.dds");
                    texNames.Add("pl_body_multi.dds");
                    texNames.Add("pl_body_decal.dds");
                    texNames.Add("pl_body_normal.dds");
                    break;
                case "rbd":
                case "reboot_bd":
                case "reboot_pl":
                case "reboot_player":
                case "reboot_ba":
                case "reboot_ou":
                    if (mat.shaderNames == null)
                    {
                        mat.shaderNames = new List<string>() { "1102p", "1102" };
                    }
                    texNames.Add("pl_body_base_diffuse.dds");
                    texNames.Add("pl_body_base_multi.dds");
                    texNames.Add("pl_body_base_normal.dds");
                    texNames.Add("pl_body_base_mask.dds");
                    texNames.Add("pl_body_base_subnormal_01.dds");
                    texNames.Add("pl_body_base_subnormal_02.dds");
                    texNames.Add("pl_body_base_subnormal_03.dds");
                    break;
                case "rbd_sk":
                case "rbd_skin":
                case "reboot_bd_skin":
                case "reboot_pl_skin":
                case "reboot_player_skin":
                case "reboot_ba_skin":
                case "reboot_ou_skin":
                    if (mat.shaderNames == null)
                    {
                        mat.shaderNames = new List<string>() { "1102p", "1102" };
                    }
                    texNames.Add("pl_body_skin_diffuse.dds");
                    texNames.Add("pl_body_skin_multi.dds");
                    texNames.Add("pl_body_skin_normal.dds");
                    texNames.Add("pl_body_skin_mask01.dds");
                    texNames.Add("pl_body_skin_subnormal_01.dds");
                    texNames.Add("pl_body_skin_subnormal_02.dds");
                    texNames.Add("pl_body_skin_subnormal_03.dds");
                    break;
                default:
                    break;
            }
            mat.texNames = texNames;
        }

        //Used to generate standard materials in which the texture names are used. 
        public static void GenerateMaterial(AquaObject model, GenericMaterial mat)
        {
            if(mat.specialType != null)
            {
                GenerateSpecialMaterialParameters(mat);
            }
            int texArrayStartIndex = model.tstaList.Count;
            TSET tset = new TSET();
            MATE mate = new MATE();
            SHAD shad = new SHAD();
            REND rend = new REND();

            //Set up textures
            for (int i = 0; i < mat.texNames.Count; i++)
            {
                TEXF texf = new TEXF();
                TSTA tsta = new TSTA();

                texf.texName.SetString(mat.texNames[i]);

                tsta.tag = 0x16; //Reexamine this one when possible, these actually vary a bit in 0xC33 variants.
                tsta.texUsageOrder = texArrayStartIndex + i;
                if (mat.texUVSets == null)
                {
                    tsta.modelUVSet = 1;
                } else
                {
                    tsta.modelUVSet = mat.texUVSets[i];
                }
                tsta.unkVector0 = new Vector3();
                tsta.unkInt0 = 0;
                tsta.unkInt1 = 0;
                tsta.unkInt2 = 0;
                tsta.unkInt3 = 1;
                tsta.unkInt4 = 1;
                tsta.unkInt5 = 1;
                tsta.unkFloat0 = 0f;
                tsta.unkFloat1 = 0f;
                tsta.texName = texf.texName;

                model.texfList.Add(texf);
                model.tstaList.Add(tsta);
            }

            //Set up texture set. If 0, we leave it all default
            if (mat.texNames.Count > 0)
            {
                tset.texCount = mat.texNames.Count;
                for(int tex = 0; tex < mat.texNames.Count; tex++ )
                {
                    tset.tstaTexIDs.Add(texArrayStartIndex + tex);
                }
            }

            //Set up material
            if (mat.shaderNames == null)
            {
                mat.shaderNames = DefaultShaderNames;
            }
            mate.diffuseRGBA = mat.diffuseRGBA;
            mate.unkRGBA0 = mat.unkRGBA0;
            mate._sRGBA = mat._sRGBA;
            mate.unkRGBA1 = mat.unkRGBA1;
            mate.reserve0 = mat.reserve0;
            mate.unkFloat0 = mat.unkFloat0;
            mate.unkFloat1 = mat.unkFloat1;
            mate.unkInt0 = mat.unkInt0;
            mate.unkInt1 = mat.unkInt1;
            mate.alphaType.SetString(mat.blendType);
            mate.matName.SetString(mat.matName);

            //Set up SHAD
            shad.pixelShader.SetString(mat.shaderNames[0]);
            shad.vertexShader.SetString(mat.shaderNames[1]);

            //Set up REND
            rend.tag = 0x1FF;
            rend.unk0 = 3;
            switch(mat.twoSided)
            {
                case true:
                    rend.twosided = 1;
                    break;
                case false:
                    rend.twosided = 0;
                    break;
            }
            switch(mat.blendType)
            {
                case "opaque":
                    rend.notOpaque = 0;
                    rend.unk10 = 0;
                    break;
                default:
                    rend.notOpaque = 1;
                    rend.unk10 = 0;
                    break;
            }

            rend.unk1 = 5;
            rend.unk2 = 6;
            rend.unk3 = 1;
            rend.unk4 = 0;

            rend.unk5 = 5;
            rend.unk6 = 6;
            rend.unk7 = 1;
            rend.unk8 = 1;

            rend.unk9 = 5;
            rend.unk11 = 1;
            rend.unk12 = 4;
            rend.unk13 = 1;

            model.tsetList.Add(tset);
            model.mateList.Add(mate);
            model.shadList.Add(shad);
            model.rendList.Add(rend);
        }

        public static List<string> GetTexListNames(AquaObject model, int tsetIndex)
        {
            List<string> textureList = new List<string>();
            
            TSET tset = model.tsetList[tsetIndex];

            for (int index = 0; index < tset.tstaTexIDs.Count; index++) 
            {
                int texIndex = tset.tstaTexIDs[index];
                if (texIndex != -1)
                {
                    TSTA tsta = model.tstaList[texIndex];

                    textureList.Add(tsta.texName.GetString());
                }
            }

            return textureList;
        }

        public static List<string> GetShaderNames(AquaObject model, int shadIndex)
        {
            List<string> shaderList = new List<string>();

            SHAD shad = model.shadList[shadIndex];

            shaderList.Add(shad.pixelShader.GetString());
            shaderList.Add(shad.vertexShader.GetString());

            return shaderList;
        }

        private static byte[] Read4Bytes(BufferedStreamReader streamReader)
        {
            byte[] bytes = new byte[4];
            for (int byteIndex = 0; byteIndex < 4; byteIndex++) { bytes[byteIndex] = streamReader.Read<byte>(); }

            return bytes;
        }

        private static ushort[] Read4UShorts(BufferedStreamReader streamReader)
        {
            ushort[] ushorts = new ushort[4];
            for (int ushortIndex = 0; ushortIndex < 4; ushortIndex++) { ushorts[ushortIndex] = streamReader.Read<ushort>(); }

            return ushorts;
        }

        private static short[] Read4Shorts(BufferedStreamReader streamReader)
        {
            short[] shorts = new short[4];
            for (int shortIndex = 0; shortIndex < 4; shortIndex++) { shorts[shortIndex] = streamReader.Read<short>(); }

            return shorts;
        }

        private static short[] Read2Shorts(BufferedStreamReader streamReader)
        {
            short[] shorts = new short[2];
            for (int shortIndex = 0; shortIndex < 2; shortIndex++) { shorts[shortIndex] = streamReader.Read<short>(); }

            return shorts;
        }

        public static float[] VectorAsArray(Vector3 vec3)
        {
            return new float[] { vec3.X, vec3.Y, vec3.Z }; 
        }

        //This shouldn't be necessary, but library binding issues in maxscript necessitated it over the Reloaded.Memory implementation. System.Runtime.CompilerServices.Unsafe causes errors otherwise.
        //Borrowed from: https://stackoverflow.com/questions/42154908/cannot-take-the-address-of-get-the-size-of-or-declare-a-pointer-to-a-managed-t
        public static byte[] ConvertStruct<T>(ref T str) where T : struct
        {
            int size = Marshal.SizeOf(str);
            IntPtr arrPtr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(str, arrPtr, true);
            var arr = new byte[size];
            Marshal.Copy(arrPtr, arr, 0, size);
            Marshal.FreeHGlobal(arrPtr);
            return arr;
        }

        public static byte[] ConvertStruct<T>(T str) where T : struct
        {
            return ConvertStruct(ref str);
        }
    }
}
