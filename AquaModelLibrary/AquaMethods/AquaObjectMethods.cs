using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using SystemHalf;
using AquaModelLibrary.AquaStructs.NGSShaderPresets;
using AquaModelLibrary.AquaStructs.AquaObjectExtras;
using static AquaModelLibrary.AquaObject;
using static AquaModelLibrary.AquaMethods.AquaGeneralMethods;

namespace AquaModelLibrary
{
    public unsafe static class AquaObjectMethods
    {
        public static readonly List<string> DefaultShaderNames = new List<string>() { "0398p", "0398" };

        public static void VTXLFromFaceVerts(AquaObject model)
        {
            model.vtxlList = new List<VTXL>();

            for(int mesh = 0; mesh < model.tempTris.Count; mesh++)
            {
                //Set up a new VTXL based on an existing sample in order to figure optimize a bit for later.
                //For the sake of simplicity, we assume that vertex IDs for this start from 0 and end at the vertex count - 1. 
                VTXL vtxl = new VTXL(model.tempTris[mesh].vertCount, model.tempTris[mesh].faceVerts[0]);
                List<bool> vtxlCheck = new List<bool>(new bool[model.tempTris[mesh].vertCount]);

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

                        //Handle if for whatever reason we have more vertices than expected
                        if(vertIndex > (vtxlCheck.Count - 1))
                        {
                            vtxlCheck.AddRange(new bool[vertIndex - (vtxlCheck.Count - 1)]);
                        } 

                        if (vertIndex > (vtxl.vertPositions.Count - 1))
                        {
                            vtxl.AddRange(vertIndex - (vtxl.vertPositions.Count - 1), vtxl);
                        }

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
                    if (vtxl.uv2ListNGS.Count > 0 && vtxl.uv2ListNGS[i] == null)
                    {
                        vtxl.uv2ListNGS[i] = new short[2];
                    }
                    if (vtxl.uv3ListNGS.Count > 0 && vtxl.uv3ListNGS[i] == null)
                    {
                        vtxl.uv3ListNGS[i] = new short[2];
                    }
                    if (vtxl.uv4ListNGS.Count > 0 && vtxl.uv4ListNGS[i] == null)
                    {
                        vtxl.uv4ListNGS[i] = new short[2];
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
                    if (vtxl.vert0x24.Count > 0 && vtxl.vert0x24[i] == null)
                    {
                        vtxl.vert0x24[i] = new short[2];
                    }
                    if (vtxl.vert0x25.Count > 0 && vtxl.vert0x25[i] == null)
                    {
                        vtxl.vert0x25[i] = new short[2];
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
                destinationVTXL.vertWeightsNGS[destinationIndex] = (ushort[])sourceVTXL.vertWeightsNGS[sourceIndex].Clone();
            }
            if (sourceVTXL.vertWeights.Count > sourceIndex)
            {
                destinationVTXL.vertWeights[destinationIndex] = sourceVTXL.vertWeights[sourceIndex];
            }
            if (sourceVTXL.vertNormalsNGS.Count > sourceIndex)
            {
                destinationVTXL.vertNormalsNGS[destinationIndex] = (short[])sourceVTXL.vertNormalsNGS[sourceIndex].Clone();
            }
            if (sourceVTXL.vertNormals.Count > sourceIndex)
            {
                destinationVTXL.vertNormals[destinationIndex] = sourceVTXL.vertNormals[sourceIndex];
            }
            if (sourceVTXL.vertColors.Count > sourceIndex)
            {
                destinationVTXL.vertColors[destinationIndex] = (byte[])sourceVTXL.vertColors[sourceIndex].Clone();
            }
            if (sourceVTXL.vertColor2s.Count > sourceIndex)
            {
                destinationVTXL.vertColor2s[destinationIndex] = (byte[])sourceVTXL.vertColor2s[sourceIndex].Clone();
            }
            if (sourceVTXL.vertWeightIndices.Count > sourceIndex)
            {
                destinationVTXL.vertWeightIndices[destinationIndex] = (int[])sourceVTXL.vertWeightIndices[sourceIndex].Clone();
            }
            if (sourceVTXL.uv1ListNGS.Count > sourceIndex)
            {
                destinationVTXL.uv1ListNGS[destinationIndex] = (short[])sourceVTXL.uv1ListNGS[sourceIndex].Clone();
            }
            if (sourceVTXL.uv2ListNGS.Count > sourceIndex)
            {
                destinationVTXL.uv2ListNGS[destinationIndex] = (short[])sourceVTXL.uv2ListNGS[sourceIndex].Clone();
            }
            if (sourceVTXL.uv3ListNGS.Count > sourceIndex)
            {
                destinationVTXL.uv3ListNGS[destinationIndex] = (short[])sourceVTXL.uv3ListNGS[sourceIndex].Clone();
            }
            if (sourceVTXL.uv4ListNGS.Count > sourceIndex)
            {
                destinationVTXL.uv4ListNGS[destinationIndex] = (short[])sourceVTXL.uv4ListNGS[sourceIndex].Clone();
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
                destinationVTXL.vert0x22[destinationIndex] = (short[])sourceVTXL.vert0x22[sourceIndex].Clone();
            }
            if (sourceVTXL.vert0x23.Count > sourceIndex)
            {
                destinationVTXL.vert0x23[destinationIndex] = (short[])sourceVTXL.vert0x23[sourceIndex].Clone();
            }
            if (sourceVTXL.vert0x24.Count > sourceIndex)
            {
                destinationVTXL.vert0x24[destinationIndex] = (short[])sourceVTXL.vert0x24[sourceIndex].Clone();
            }
            if (sourceVTXL.vert0x25.Count > sourceIndex)
            {
                destinationVTXL.vert0x25[destinationIndex] = (short[])sourceVTXL.vert0x25[sourceIndex].Clone();
            }
            if (sourceVTXL.vertTangentListNGS.Count > sourceIndex)
            {
                destinationVTXL.vertTangentListNGS[destinationIndex] = (short[])sourceVTXL.vertTangentListNGS[sourceIndex].Clone();
            }
            if (sourceVTXL.vertTangentList.Count > sourceIndex)
            {
                destinationVTXL.vertTangentList[destinationIndex] = sourceVTXL.vertTangentList[sourceIndex];
            }
            if (sourceVTXL.vertBinormalListNGS.Count > sourceIndex)
            {
                destinationVTXL.vertBinormalListNGS[destinationIndex] = (short[])sourceVTXL.vertBinormalListNGS[sourceIndex].Clone();
            }
            if (sourceVTXL.vertBinormalList.Count > sourceIndex)
            {
                destinationVTXL.vertBinormalList[destinationIndex] = sourceVTXL.vertBinormalList[sourceIndex];
            }
            if (sourceVTXL.rawVertWeights.Count > sourceIndex)
            {
                destinationVTXL.rawVertWeights[destinationIndex] = new List<float>(sourceVTXL.rawVertWeights[sourceIndex]);
            }
            if (sourceVTXL.rawVertWeightIds.Count > sourceIndex)
            {
                destinationVTXL.rawVertWeightIds[destinationIndex] = new List<int>(sourceVTXL.rawVertWeightIds[sourceIndex]);
            }
            if (sourceVTXL.trueVertWeights.Count > sourceIndex)
            {
                destinationVTXL.trueVertWeights[destinationIndex] = sourceVTXL.trueVertWeights[sourceIndex];
            }
            if (sourceVTXL.trueVertWeightIndices.Count > sourceIndex)
            {
                destinationVTXL.trueVertWeightIndices[destinationIndex] = (int[])sourceVTXL.trueVertWeightIndices[sourceIndex].Clone();
            }
            if (sourceVTXL.edgeVerts.Contains((ushort)sourceIndex))
            {
                destinationVTXL.edgeVerts.Add((ushort)destinationIndex);
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
                destinationVTXL.vertWeightsNGS.Add((ushort[])sourceVTXL.vertWeightsNGS[sourceIndex].Clone());
            }
            if (sourceVTXL.vertWeights.Count > sourceIndex)
            {
                destinationVTXL.vertWeights.Add(sourceVTXL.vertWeights[sourceIndex]);
            }
            if (sourceVTXL.vertNormalsNGS.Count > sourceIndex)
            {
                destinationVTXL.vertNormalsNGS.Add((short[])sourceVTXL.vertNormalsNGS[sourceIndex].Clone());
            }
            if (sourceVTXL.vertNormals.Count > sourceIndex)
            {
                destinationVTXL.vertNormals.Add(sourceVTXL.vertNormals[sourceIndex]);
            }
            if (sourceVTXL.vertColors.Count > sourceIndex)
            {
                destinationVTXL.vertColors.Add((byte[])sourceVTXL.vertColors[sourceIndex].Clone());
            }
            if (sourceVTXL.vertColor2s.Count > sourceIndex)
            {
                destinationVTXL.vertColor2s.Add((byte[])sourceVTXL.vertColor2s[sourceIndex].Clone());
            }
            if (sourceVTXL.vertWeightIndices.Count > sourceIndex)
            {
                destinationVTXL.vertWeightIndices.Add((int[])sourceVTXL.vertWeightIndices[sourceIndex].Clone());
            }
            if (sourceVTXL.uv1ListNGS.Count > sourceIndex)
            {
                destinationVTXL.uv1ListNGS.Add((short[])sourceVTXL.uv1ListNGS[sourceIndex].Clone());
            }
            if (sourceVTXL.uv2ListNGS.Count > sourceIndex)
            {
                destinationVTXL.uv2ListNGS.Add((short[])sourceVTXL.uv2ListNGS[sourceIndex].Clone());
            }
            if (sourceVTXL.uv3ListNGS.Count > sourceIndex)
            {
                destinationVTXL.uv3ListNGS.Add((short[])sourceVTXL.uv3ListNGS[sourceIndex].Clone());
            }
            if (sourceVTXL.uv4ListNGS.Count > sourceIndex)
            {
                destinationVTXL.uv4ListNGS.Add((short[])sourceVTXL.uv4ListNGS[sourceIndex].Clone());
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
                destinationVTXL.vert0x22.Add((short[])sourceVTXL.vert0x22[sourceIndex].Clone());
            }
            if (sourceVTXL.vert0x23.Count > sourceIndex)
            {
                destinationVTXL.vert0x23.Add((short[])sourceVTXL.vert0x23[sourceIndex].Clone());
            }
            if (sourceVTXL.vert0x24.Count > sourceIndex)
            {
                destinationVTXL.vert0x24.Add((short[])sourceVTXL.vert0x24[sourceIndex].Clone());
            }
            if (sourceVTXL.vert0x25.Count > sourceIndex)
            {
                destinationVTXL.vert0x25.Add((short[])sourceVTXL.vert0x25[sourceIndex].Clone());
            }
            if (sourceVTXL.vertTangentListNGS.Count > sourceIndex)
            {
                destinationVTXL.vertTangentListNGS.Add((short[])sourceVTXL.vertTangentListNGS[sourceIndex].Clone());
            }
            if (sourceVTXL.vertTangentList.Count > sourceIndex)
            {
                destinationVTXL.vertTangentList.Add(sourceVTXL.vertTangentList[sourceIndex]);
            }
            if (sourceVTXL.vertBinormalListNGS.Count > sourceIndex)
            {
                destinationVTXL.vertBinormalListNGS.Add((short[])sourceVTXL.vertBinormalListNGS[sourceIndex].Clone());
            }
            if (sourceVTXL.vertBinormalList.Count > sourceIndex)
            {
                destinationVTXL.vertBinormalList.Add(sourceVTXL.vertBinormalList[sourceIndex]);
            }
            if (sourceVTXL.rawVertWeights.Count > sourceIndex)
            {
                destinationVTXL.rawVertWeights.Add(new List<float>(sourceVTXL.rawVertWeights[sourceIndex]));
            }
            if (sourceVTXL.rawVertWeightIds.Count > sourceIndex)
            {
                destinationVTXL.rawVertWeightIds.Add(new List<int>(sourceVTXL.rawVertWeightIds[sourceIndex]));
            }
            if (sourceVTXL.trueVertWeights.Count > sourceIndex)
            {
                destinationVTXL.trueVertWeights.Add(sourceVTXL.trueVertWeights[sourceIndex]);
            }
            if (sourceVTXL.trueVertWeightIndices.Count > sourceIndex)
            {
                destinationVTXL.trueVertWeightIndices.Add((int[])sourceVTXL.trueVertWeightIndices[sourceIndex].Clone());
            }
            if(sourceVTXL.edgeVerts.Contains((ushort)sourceIndex))
            {
                destinationVTXL.edgeVerts.Add((ushort)(destinationVTXL.vertPositions.Count - 1));
            }
        }

        public static bool IsSameVertex(VTXL vtxl, int vertIndex, VTXL vtxl2, int vertIndex2)
        {
            if (vtxl.vertPositions.Count > 0 && !vtxl.vertPositions[vertIndex].Equals(vtxl2.vertPositions[vertIndex2]))
            {
                return false;
            }
            if (vtxl.vertNormals.Count > 0 && !vtxl.vertNormals[vertIndex].Equals(vtxl2.vertNormals[vertIndex2]))
            {
                return false;
            }
            if (vtxl.vertNormalsNGS.Count > 0 && !vtxl.vertNormalsNGS[vertIndex].Equals(vtxl2.vertNormalsNGS[vertIndex2]))
            {
                return false;
            }
            if (vtxl.vertColors.Count > 0 && !IsEqualByteArray(vtxl.vertColors[vertIndex], vtxl2.vertColors[vertIndex2]))
            {
                return false;
            }
            if (vtxl.vertColor2s.Count > 0 && !IsEqualByteArray(vtxl.vertColor2s[vertIndex], vtxl2.vertColor2s[vertIndex2]))
            {
                return false;
            }
            if (vtxl.uv1List.Count > 0 && !vtxl.uv1List[vertIndex].Equals(vtxl2.uv1List[vertIndex2]))
            {
                return false;
            }
            if (vtxl.uv1ListNGS.Count > 0 && !IsEqualShortArray(vtxl.uv1ListNGS[vertIndex], vtxl2.uv1ListNGS[vertIndex2]))
            {
                return false;
            }
            if (vtxl.uv2ListNGS.Count > 0 && !IsEqualShortArray(vtxl.uv2ListNGS[vertIndex], vtxl2.uv2ListNGS[vertIndex2]))
            {
                return false;
            }
            if (vtxl.uv3ListNGS.Count > 0 && !IsEqualShortArray(vtxl.uv3ListNGS[vertIndex], vtxl2.uv3ListNGS[vertIndex2]))
            {
                return false;
            }
            if (vtxl.uv4ListNGS.Count > 0 && !IsEqualShortArray(vtxl.uv4ListNGS[vertIndex], vtxl2.uv4ListNGS[vertIndex2]))
            {
                return false;
            }
            if (vtxl.uv2List.Count > 0 && !vtxl.uv2List[vertIndex].Equals(vtxl2.uv2List[vertIndex2]))
            {
                return false;
            }
            if (vtxl.uv3List.Count > 0 && !vtxl.uv3List[vertIndex].Equals(vtxl2.uv3List[vertIndex2]))
            {
                return false;
            }
            if (vtxl.uv4List.Count > 0 && !vtxl.uv4List[vertIndex].Equals(vtxl2.uv4List[vertIndex2]))
            {
                return false;
            }
            if (vtxl.vert0x22.Count > 0 && !IsEqualShortArray(vtxl.vert0x22[vertIndex], vtxl2.vert0x22[vertIndex2]))
            {
                return false;
            }
            if (vtxl.vert0x23.Count > 0 && !IsEqualShortArray(vtxl.vert0x23[vertIndex], vtxl2.vert0x23[vertIndex2]))
            {
                return false;
            }
            if (vtxl.vert0x24.Count > 0 && !IsEqualShortArray(vtxl.vert0x24[vertIndex], vtxl2.vert0x24[vertIndex2]))
            {
                return false;
            }
            if (vtxl.vert0x25.Count > 0 && !IsEqualShortArray(vtxl.vert0x25[vertIndex], vtxl2.vert0x25[vertIndex2]))
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
            if(vtxl.vertWeightsNGS.Count > 0)
            {
                vtxe.vertDataTypes.Add(vtxeElementGenerator(0x1, 0x11, curLength));
                curLength += 0x8;
            } else if (vtxl.vertWeights.Count > 0)
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
            if (vtxl.vertBinormalList.Count > 0)
            {
                vtxe.vertDataTypes.Add(vtxeElementGenerator(0x21, 0x3, curLength));
                curLength += 0xC;
            }
            if (vtxl.vertTangentList.Count > 0)
            {
                vtxe.vertDataTypes.Add(vtxeElementGenerator(0x20, 0x3, curLength));
                curLength += 0xC;
            }
            if (vtxl.vert0x22.Count > 0)
            {
                vtxe.vertDataTypes.Add(vtxeElementGenerator(0x22, 0xC, curLength));
                curLength += 0x4;
            }
            if (vtxl.vert0x23.Count > 0)
            {
                vtxe.vertDataTypes.Add(vtxeElementGenerator(0x23, 0xC, curLength));
                curLength += 0x4;
            }
            if (vtxl.vert0x24.Count > 0)
            {
                vtxe.vertDataTypes.Add(vtxeElementGenerator(0x24, 0xC, curLength));
                curLength += 0x4;
            }
            if (vtxl.vert0x25.Count > 0)
            {
                vtxe.vertDataTypes.Add(vtxeElementGenerator(0x25, 0xC, curLength));
                curLength += 0x4;
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
                            switch (vtxeSet.vertDataTypes[vtxeIndex].structVariation)
                            {
                                case 0x3:
                                    vtxl.vertPositions.Add(streamReader.Read<Vector3>());
                                    break;
                                case 0x13:
                                case 0x99: //Nova
                                    vtxl.vertPositions.Add(new Vector3(streamReader.Read<Half>(), streamReader.Read<Half>(), streamReader.Read<Half>()));
                                    var fillerHalf = streamReader.Read<ushort>(); //Technically another half float. Should always be 1.
                                    break;
                                default:
                                    break;
                            }
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
                                case 0x13:
                                    vtxl.vertNormals.Add(new Vector3(streamReader.Read<Half>(), streamReader.Read<Half>(), streamReader.Read<Half>()));
                                    var fillerHalf = streamReader.Read<ushort>(); //Technically another half float. Should always be 1.
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
                            vtxl.vertWeightIndices.Add(Read4BytesToIntArray(streamReader));
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
                                case 0x12:
                                    vtxl.uv1List.Add(new Vector2(streamReader.Read<Half>(), streamReader.Read<Half>()));
                                    break;
                                case 0x99: //For nova
                                    vtxl.uv1List.Add(UshortsToVector2(Read2Ushorts(streamReader)));
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
                                    vtxl.uv2ListNGS.Add(Read2Shorts(streamReader));
                                    break;
                                case 0x12:
                                    vtxl.uv2List.Add(new Vector2(streamReader.Read<Half>(), streamReader.Read<Half>()));
                                    break;
                                case 0x99: //For nova
                                    vtxl.uv2List.Add(UshortsToVector2(Read2Ushorts(streamReader)));
                                    break;
                                default:
                                    throw new Exception($"Unexpected vert uv2 struct type {vtxeSet.vertDataTypes[vtxeIndex].structVariation}");
                            }
                            break;
                        case (int)ClassicAquaObject.VertFlags.VertUV3:
                            switch (vtxeSet.vertDataTypes[vtxeIndex].structVariation)
                            {
                                case 0x2:
                                    vtxl.uv3List.Add(streamReader.Read<Vector2>());
                                    break;
                                case 0xE:
                                    vtxl.uv3ListNGS.Add(Read2Shorts(streamReader));
                                    break;
                                case 0x12:
                                    vtxl.uv3List.Add(new Vector2(streamReader.Read<Half>(), streamReader.Read<Half>()));
                                    break;
                                case 0x99: //For nova
                                    vtxl.uv3List.Add(UshortsToVector2(Read2Ushorts(streamReader)));
                                    break;
                                default:
                                    throw new Exception($"Unexpected vert uv3 struct type {vtxeSet.vertDataTypes[vtxeIndex].structVariation}");
                            }
                            break;
                        case (int)NGSAquaObject.NGSVertFlags.VertUV4:
                            switch (vtxeSet.vertDataTypes[vtxeIndex].structVariation)
                            {
                                case 0x2:
                                    vtxl.uv4List.Add(streamReader.Read<Vector2>());
                                    break;
                                case 0xE:
                                    vtxl.uv4ListNGS.Add(Read2Shorts(streamReader));
                                    break;
                                case 0x12:
                                    vtxl.uv4List.Add(new Vector2(streamReader.Read<Half>(), streamReader.Read<Half>()));
                                    break;
                                case 0x99: //For nova
                                    vtxl.uv4List.Add(UshortsToVector2(Read2Ushorts(streamReader)));
                                    break;
                                default:
                                    throw new Exception($"Unexpected vert uv4 struct type {vtxeSet.vertDataTypes[vtxeIndex].structVariation}");
                            }
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
                                case 0x13:
                                    vtxl.vertTangentList.Add(new Vector3(streamReader.Read<Half>(), streamReader.Read<Half>(), streamReader.Read<Half>()));
                                    var fillerHalf = streamReader.Read<ushort>(); //Technically another half float. Should always be 1.
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
                                case 0x13:
                                    vtxl.vertBinormalList.Add(new Vector3(streamReader.Read<Half>(), streamReader.Read<Half>(), streamReader.Read<Half>()));
                                    var fillerHalf = streamReader.Read<ushort>(); //Technically another half float. Should always be 1.
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
                        case (int)NGSAquaObject.NGSVertFlags.Vert0x24:
                            vtxl.vert0x24.Add(Read2Shorts(streamReader));
                            break;
                        case (int)NGSAquaObject.NGSVertFlags.Vert0x25:
                            vtxl.vert0x25.Add(Read2Shorts(streamReader));
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

        public static Vector3 GetMaximumBounding(Vector3 maxPoint, Vector3 newPoint)
        {
            if (maxPoint.X < newPoint.X)
            {
                maxPoint.X = newPoint.X;
            }
            if (maxPoint.Y < newPoint.Y)
            {
                maxPoint.Y = newPoint.Y;
            }
            if (maxPoint.Z < newPoint.Z)
            {
                maxPoint.Z = newPoint.Z;
            }

            return maxPoint;
        }

        public static Vector3 GetMinimumBounding(Vector3 minPoint, Vector3 newPoint)
        {
            if (minPoint.X > newPoint.X)
            {
                minPoint.X = newPoint.X;
            }
            if (minPoint.Y > newPoint.Y)
            {
                minPoint.Y = newPoint.Y;
            }
            if (minPoint.Z > newPoint.Z)
            {
                minPoint.Z = newPoint.Z;
            }

            return minPoint;
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
                    maxPoint = GetMaximumBounding(maxPoint, vertData[vset].vertPositions[vert]);

                    //Compare to min
                    minPoint = GetMinimumBounding(minPoint, vertData[vset].vertPositions[vert]);
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
                    float distance = Extra.MathExtras.Distance(center, vertData[vset].vertPositions[vert]);
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
        
        //Adapted from this: https://forums.cgsociety.org/t/finding-bi-normals-tangents/975005/8 
        //Binormals and tangents for each face are calculated and each face's values for a particular vertex are summed and averaged for the result before being normalized
        //Though vertex position is used, due to the nature of the normalization applied during the process, resizing is unneeded.
        //This function expects that binormals and tangents have both either not been populated or both have been populated in particular vertex sets. The game always does both or neither.
        //Normals will also be generated if non existent because it needs those too
        public static void ComputeTangentSpace(AquaObject model, bool useFaceNormals, bool flipUv = false)
        {
            List<List<Vector3>> faces = new List<List<Vector3>>();
            Vector3[][] vertBinormalArrays = new Vector3[model.vtxlList.Count][];
            Vector3[][] vertTangentArrays = new Vector3[model.vtxlList.Count][];
            Vector3[][] vertFaceNormalArrays = new Vector3[model.vtxlList.Count][];
            int uvSign = 1;
            if(flipUv == true)
            {
                uvSign = -1;
            }

            //Get faces depending on model state
            if (model.strips.Count > 0)
            {
                foreach(var tris in model.strips)
                {
                    faces.Add(tris.GetTriangles(true));
                }
            } else
            {
                foreach (var tris in model.tempTris)
                {
                    faces.Add(tris.triList);
                }
            }

            //Clear before calcing
            for(int i = 0; i <  model.vtxlList.Count; i++)
            {
                model.vtxlList[i].vertTangentList.Clear();
                model.vtxlList[i].vertTangentListNGS.Clear();
                model.vtxlList[i].vertBinormalList.Clear();
                model.vtxlList[i].vertBinormalListNGS.Clear();
            }

            //Loop through faces and sum the calculated data for each vertice's faces
            for(int meshIndex = 0; meshIndex < faces.Count; meshIndex++)
            {
                int vsetIndex;
                int psetIndex;
                //Unlike older aqo variants, NGS models can have a different vsetIndex than their
                if((model.objc.type >= 0xc31) && model.meshList.Count > 0)
                {
                    vsetIndex = model.meshList[meshIndex].vsetIndex;
                    psetIndex = model.meshList[meshIndex].psetIndex;
                } else
                {
                    vsetIndex = meshIndex;
                    psetIndex = meshIndex;
                }

                //Check if it's null or not since we don't want to overwrite what's there. NGS meshes can share vertex sets
                if(vertBinormalArrays[vsetIndex] == null)
                {
                    vertBinormalArrays[vsetIndex] = new Vector3[model.vtxlList[vsetIndex].vertPositions.Count];
                    vertTangentArrays[vsetIndex] = new Vector3[model.vtxlList[vsetIndex].vertPositions.Count];
                    vertFaceNormalArrays[vsetIndex] = new Vector3[model.vtxlList[vsetIndex].vertPositions.Count];
                }

                foreach (var face in faces[psetIndex])
                {
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
                    List<Vector2> uvs;
                    if (model.vtxlList[vsetIndex].uv1List.Count > 0)
                    {
                        uvs = new List<Vector2>()
                        {
                          new Vector2(model.vtxlList[vsetIndex].uv1List[(int)face.X].X, uvSign * model.vtxlList[vsetIndex].uv1List[(int)face.X].Y),
                          new Vector2(model.vtxlList[vsetIndex].uv1List[(int)face.Y].X, uvSign * model.vtxlList[vsetIndex].uv1List[(int)face.Y].Y),
                          new Vector2(model.vtxlList[vsetIndex].uv1List[(int)face.Z].X, uvSign * model.vtxlList[vsetIndex].uv1List[(int)face.Z].Y)
                        };
                    }
                    else
                    {
                        uvs = new List<Vector2>()
                        {
                            new Vector2(0, 0),
                            new Vector2(0, 0),
                            new Vector2(0, 0)
                        };
                    }

                    Vector3 dV1 = verts[0] - verts[1];
                    Vector3 dV2 = verts[0] - verts[2];

                    Vector2 dUV1 = uvs[0] - uvs[1];
                    Vector2 dUV2 = uvs[0] - uvs[2];

                    float area = dUV1.X * dUV2.Y - dUV1.Y * dUV2.X;
                    int sign = 1;
                    if (area < 0)
                    {
                        sign = -1;
                    }

                    Vector3 tangent = new Vector3(0, 0, 1);
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
                        if (vertBinormalArrays[vsetIndex][faceIndices[i]] == null)
                        {
                            vertBinormalArrays[vsetIndex][faceIndices[i]] = binormal;
                            vertTangentArrays[vsetIndex][faceIndices[i]] = tangent;
                            vertFaceNormalArrays[vsetIndex][faceIndices[i]] = normal;
                        }
                        else
                        {
                            vertBinormalArrays[vsetIndex][faceIndices[i]] += binormal;
                            vertTangentArrays[vsetIndex][faceIndices[i]] += tangent;
                            vertFaceNormalArrays[vsetIndex][faceIndices[i]] += normal;
                        }
                    }
                }
            }

            //Loop through vsets and verts and assign these first so that verts that aren't linked get assigned.
            //Then, we can use UNRMs calculations to fix verts that were split in exporting
            bool[] vertNormalsCheck = new bool[model.vtxlList.Count];
            for (int i = 0; i < vertNormalsCheck.Length; i++)
            {
                vertNormalsCheck[i] = true;
            }

            for(int vsetIndex = 0; vsetIndex < vertBinormalArrays.Length; vsetIndex++)
            {
                int vertCount = model.vtxlList[vsetIndex].vertPositions.Count;

                if (model.vtxlList[vsetIndex].vertBinormalList == null || model.vtxlList[vsetIndex].vertBinormalList.Count != vertCount)
                {
                    model.vtxlList[vsetIndex].vertBinormalList = new List<Vector3>(new Vector3[vertCount]);
                    model.vtxlList[vsetIndex].vertTangentList = new List<Vector3>(new Vector3[vertCount]);
                }
                //Add normals if they aren't there or if we want to regenerate them
                if (model.vtxlList[vsetIndex].vertNormals == null || model.vtxlList[vsetIndex].vertNormals.Count == 0)
                {
                    vertNormalsCheck[vsetIndex] = false;
                    model.vtxlList[vsetIndex].vertNormals = new List<Vector3>(new Vector3[vertCount]);
                }
                for(int vert = 0; vert < vertBinormalArrays[vsetIndex].Length; vert++)
                {
                    model.vtxlList[vsetIndex].vertBinormalList[vert] = Vector3.Normalize(vertBinormalArrays[vsetIndex][vert]);
                    model.vtxlList[vsetIndex].vertTangentList[vert] = Vector3.Normalize(vertTangentArrays[vsetIndex][vert]);

                    if(vertNormalsCheck[vsetIndex] == false || useFaceNormals == true)
                    {
                        model.vtxlList[vsetIndex].vertNormals[vert] = Vector3.Normalize(vertFaceNormalArrays[vsetIndex][vert]);
                    }
                }
            }

            //Hack since for now we don't convert back from legacy types properly
            int biggest = 0;
            for (int i = 0; i < model.vsetList.Count; i++)
            {
                int vertSize;
                var vset = model.vsetList[i];
                if (model.objc.type >= 0xC32)
                {
                    model.vtxeList[model.vsetList[i].vtxeCount] = ConstructClassicVTXE(model.vtxlList[i], out vertSize);
                } else
                {
                    model.vtxeList[i] = ConstructClassicVTXE(model.vtxlList[i], out vertSize);
                    vset.vtxeCount = model.vtxeList[i].vertDataTypes.Count;
                }
                vset.vertDataSize = vertSize;
                model.vsetList[i] = vset;
                if (vertSize > biggest)
                {
                    biggest = vertSize;
                }
            }
            model.objc.largetsVtxl = biggest;
        }

        //Calculates tangent space using tempMesh data
        public static void ComputeTangentSpaceTempMesh(AquaObject model, bool useFaceNormals)
        {
            for (int mesh = 0; mesh < model.tempTris.Count; mesh++)
            {
                var vertBinormalArray = new Vector3[model.vtxlList[mesh].vertPositions.Count];
                var vertTangentArray = new Vector3[model.vtxlList[mesh].vertPositions.Count];
                var vertFaceNormalArray = new Vector3[model.vtxlList[mesh].vertPositions.Count];
                var faces = model.tempTris[mesh].triList;

                foreach (var face in faces)
                {
                    List<int> faceIndices = new List<int>()
                        {
                            (int)face.X,
                            (int)face.Y,
                            (int)face.Z
                        };

                    List<Vector3> verts = new List<Vector3>()
                        {
                          model.vtxlList[mesh].vertPositions[(int)face.X],
                          model.vtxlList[mesh].vertPositions[(int)face.Y],
                          model.vtxlList[mesh].vertPositions[(int)face.Z]
                        };
                    //We expect model UVs to be flipped when brought into .NET. But we have to flip them back for these calculations...
                    List<Vector2> uvs = new List<Vector2>()
                        {
                          new Vector2(model.vtxlList[mesh].uv1List[(int)face.X].X, -model.vtxlList[mesh].uv1List[(int)face.X].Y),
                          new Vector2(model.vtxlList[mesh].uv1List[(int)face.Y].X, -model.vtxlList[mesh].uv1List[(int)face.Y].Y),
                          new Vector2(model.vtxlList[mesh].uv1List[(int)face.Z].X, -model.vtxlList[mesh].uv1List[(int)face.Z].Y)
                        };

                    Vector3 dV1 = verts[0] - verts[1];
                    Vector3 dV2 = verts[0] - verts[2];

                    Vector2 dUV1 = uvs[0] - uvs[1];
                    Vector2 dUV2 = uvs[0] - uvs[2];

                    float area = dUV1.X * dUV2.Y - dUV1.Y * dUV2.X;
                    int sign = 1;
                    if (area < 0)
                    {
                        sign = -1;
                    }

                    Vector3 tangent = new Vector3(0, 0, 1);
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
                        }
                        else
                        {
                            vertBinormalArray[faceIndices[i]] += binormal;
                            vertTangentArray[faceIndices[i]] += tangent;
                            vertFaceNormalArray[faceIndices[i]] += normal;
                        }
                    }
                }

                //Normalize and assign values
                for(int i = 0; i < vertBinormalArray.Length; i++)
                {
                    vertBinormalArray[i] = Vector3.Normalize(vertBinormalArray[i]);
                    vertTangentArray[i] = Vector3.Normalize(vertTangentArray[i]);

                    model.vtxlList[mesh].vertBinormalList.Add(vertBinormalArray[i]);
                    model.vtxlList[mesh].vertTangentList.Add(vertTangentArray[i]);

                    if(useFaceNormals)
                    {
                        vertFaceNormalArray[i] = Vector3.Normalize(vertFaceNormalArray[i]);

                        model.vtxlList[mesh].vertNormals.Add(vertFaceNormalArray[i]);
                    }
                }
            }
        }

        //Requires a fully assigned model
        public static void ComputeTangentSpaceOld(AquaObject model, bool useFaceNormals)
        {
            for (int mesh = 0; mesh < model.vsetList.Count; mesh++)
            {
                //Verify that there are vertices and with valid UVs
                int vsetIndex = model.meshList[mesh].vsetIndex;
                int psetIndex = model.meshList[mesh].psetIndex;

                model.vtxlList[vsetIndex].vertTangentList.Clear();
                model.vtxlList[vsetIndex].vertTangentListNGS.Clear();
                model.vtxlList[vsetIndex].vertBinormalList.Clear();
                model.vtxlList[vsetIndex].vertBinormalListNGS.Clear();
                if (model.vtxlList[vsetIndex].uv1List.Count > 0 && model.vtxlList[vsetIndex].vertPositions.Count > 0)
                {
                    Vector3[] vertBinormalArray = new Vector3[model.vtxlList[vsetIndex].vertPositions.Count];
                    Vector3[] vertTangentArray = new Vector3[model.vtxlList[vsetIndex].vertPositions.Count];
                    Vector3[] vertFaceNormalArray = new Vector3[model.vtxlList[vsetIndex].vertPositions.Count];
                    List<Vector3> faces = model.strips[psetIndex].GetTriangles(true);

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
                          new Vector2(model.vtxlList[vsetIndex].uv1List[(int)face.X].X, -model.vtxlList[vsetIndex].uv1List[(int)face.X].Y),
                          new Vector2(model.vtxlList[vsetIndex].uv1List[(int)face.Y].X, -model.vtxlList[vsetIndex].uv1List[(int)face.Y].Y),
                          new Vector2(model.vtxlList[vsetIndex].uv1List[(int)face.Z].X, -model.vtxlList[vsetIndex].uv1List[(int)face.Z].Y)
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

        //Uses mesh id unlike the temp data variation so that it can work with retail models
        public static void SplitMesh(AquaObject model, int meshId, List<List<int>> facesToClone, bool forceVtxlSplit = false)
        {
            if (facesToClone.Count > 0)
            {
                var mesh = model.meshList[meshId];
                var meshFaces = model.strips[mesh.psetIndex].GetTriangles();
                var referenceVTXL = model.vtxlList[mesh.vsetIndex];
                List<VTXL> vtxlList = new List<VTXL>();
                List<string> meshNames = new List<string>();
                List<stripData> faceList = new List<stripData>();
                List<PSET> psetList = new List<PSET>();
                for (int f = 0; f < facesToClone.Count; f++)
                {
                    Dictionary<float, int> faceVertDict = new Dictionary<float, int>();
                    List<int> faceVertIds = new List<int>();
                    List<int> boneIds = new List<int>();
                    List<ushort> tempFaces = new List<ushort>();

                    int vertIndex = 0;
                    if (facesToClone[f].Count > 0)
                    {
                        //Get vert ids
                        for (int i = 0; i < facesToClone[f].Count; i++)
                        {
                            Vector3 face = meshFaces[facesToClone[f][i]];
                            if (!faceVertIds.Contains((int)face.X))
                            {
                                faceVertDict.Add(face.X, vertIndex++);
                                faceVertIds.Add((int)face.X);

                                if (referenceVTXL.vertWeightIndices.Count > 0)
                                {
                                    for (int bi = 0; bi < referenceVTXL.vertWeightIndices[(int)face.X].Length; bi++)
                                    {
                                        if (!boneIds.Contains(referenceVTXL.vertWeightIndices[(int)face.X][bi]))
                                        {
                                            boneIds.Add(referenceVTXL.vertWeightIndices[(int)face.X][bi]);
                                        }
                                    }
                                }
                            }
                            if (!faceVertIds.Contains((int)face.Y))
                            {
                                faceVertDict.Add(face.Y, vertIndex++);
                                faceVertIds.Add((int)face.Y);
                                if (referenceVTXL.vertWeightIndices.Count > 0)
                                {
                                    for (int bi = 0; bi < referenceVTXL.vertWeightIndices[(int)face.Y].Length; bi++)
                                    {
                                        if (!boneIds.Contains(referenceVTXL.vertWeightIndices[(int)face.Y][bi]))
                                        {
                                            boneIds.Add(referenceVTXL.vertWeightIndices[(int)face.Y][bi]);
                                        }
                                    }
                                }
                            }
                            if (!faceVertIds.Contains((int)face.Z))
                            {
                                faceVertDict.Add(face.Z, vertIndex++);
                                faceVertIds.Add((int)face.Z);
                                if (referenceVTXL.vertWeightIndices.Count > 0)
                                {
                                    for (int bi = 0; bi < referenceVTXL.vertWeightIndices[(int)face.Z].Length; bi++)
                                    {
                                        if (!boneIds.Contains(referenceVTXL.vertWeightIndices[(int)face.Z][bi]))
                                        {
                                            boneIds.Add(referenceVTXL.vertWeightIndices[(int)face.Z][bi]);
                                        }
                                    }
                                }
                            }

                            tempFaces.Add((ushort)face.X);
                            tempFaces.Add((ushort)face.Y);
                            tempFaces.Add((ushort)face.Z);
                        }

                        //Remap Ids based based on the indices of the values in faceVertIds and add to outModel
                        for (int i = 0; i < tempFaces.Count; i++)
                        {
                            tempFaces[i] = (ushort)faceVertDict[tempFaces[i]];
                        }

                        //Assign new stripdata
                        stripData newTris;
                        if(model.objc.type >= 0xC31)
                        {
                            newTris = new stripData();
                            newTris.triStrips = tempFaces;
                            newTris.format0xC33 = true;
                            newTris.triIdCount = tempFaces.Count;
                            newTris.faceGroups.Add(tempFaces.Count);
                        } else
                        {
                            newTris = new stripData(tempFaces.ToArray());
                            newTris.format0xC33 = false;
                        }
                        faceList.Add(newTris);

                        //PSET
                        var pset = new PSET();
                        if (model.objc.type >= 0xC31)
                        {
                            pset.tag = 0x1000;
                        }
                        else
                        {
                            pset.tag = 0x2100;
                        }
                        pset.faceGroupCount = 0x1;
                        pset.psetFaceCount = tempFaces.Count;
                        psetList.Add(pset);

                        //Copy vertex data based on faceVertIds ordering
                        VTXL vtxl = new VTXL();
                        for (int i = 0; i < faceVertIds.Count; i++)
                        {
                            appendVertex(referenceVTXL, vtxl, faceVertIds[i]);
                        }

                        //Add things that aren't linked to the vertex ids
                        if (referenceVTXL.bonePalette != null)
                        {
                            vtxl.bonePalette.AddRange(referenceVTXL.bonePalette);
                        }
                        if (meshNames.Count > meshId)
                        {
                            meshNames.Add(model.meshNames[meshId] + $"_{f}");
                        }
                        RemoveUnusedBones(vtxl);
                        vtxlList.Add(vtxl);
                    }
                }

                //Assign first split back to original slot, assign subsequent splits to end of the list
                for(int i = 0; i < faceList.Count; i++)
                {
                    if(i == 0)
                    {
                        model.strips[mesh.psetIndex] = faceList[i];
                        continue;
                    }
                    var newMesh = mesh;
                    newMesh.psetIndex = model.strips.Count;
                    if (model.objc.type < 0xC32 || forceVtxlSplit)
                    {
                        newMesh.vsetIndex = model.vsetList.Count + i - 1;
                    }
                    model.strips.Add(faceList[i]);
                    model.psetList.Add(psetList[i]);
                    model.meshList.Add(newMesh);
                }

                //If we're doing an NGS model, we can leave the vertices alone since we can recycle vertices for strips
                if (model.objc.type < 0xC32 || forceVtxlSplit)
                {
                    var vset0 = model.vsetList[mesh.vsetIndex];
                    vset0.vtxlCount = vtxlList[0].vertPositions.Count;
                    model.vsetList[mesh.vsetIndex] = vset0;
                    model.vtxlList[mesh.vsetIndex] = vtxlList[0];

                    for(int i = 0; i < vtxlList.Count; i++)
                    {
                        if(i == 0)
                        {
                            continue;
                        }
                        model.vtxlList.Add(vtxlList[i]);
                        model.vtxeList.Add(model.vtxeList[mesh.vsetIndex]);
                        model.vsetList.Add(vset0);
                    }
                }

                //Update stripStartCounts for psets
                var totalStripsShorts = 0;
                for(int i = 0; i < model.psetList.Count; i++)
                {
                    var pset = model.psetList[i];
                    pset.stripStartCount = totalStripsShorts;
                    model.psetList[i] = pset;
                    totalStripsShorts += model.psetList[i].psetFaceCount;
                }
            }


        }

        public static void SplitMeshTempData(AquaObject model, AquaObject outModel, int modelId, List<List<int>> facesToClone)
        {
            List<List<int>> boneIdList = new List<List<int>>();
            if (facesToClone.Count > 0)
            {
                for (int f = 0; f < facesToClone.Count; f++)
                {
                    var referenceVTXL = model.vtxlList[modelId];
                    Dictionary<float, int> faceVertDict = new Dictionary<float, int>();
                    List<int> faceVertIds = new List<int>();
                    List<int> tempFaceMatIds = new List<int>();
                    List<int> boneIds = new List<int>();
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

                                if(referenceVTXL.vertWeightIndices.Count > 0)
                                {
                                    for (int bi = 0; bi < referenceVTXL.vertWeightIndices[(int)face.X].Length; bi++)
                                    {
                                        if (!boneIds.Contains(referenceVTXL.vertWeightIndices[(int)face.X][bi]))
                                        {
                                            boneIds.Add(referenceVTXL.vertWeightIndices[(int)face.X][bi]);
                                        }
                                    }
                                }
                            }
                            if (!faceVertIds.Contains((int)face.Y))
                            {
                                faceVertDict.Add(face.Y, vertIndex++);
                                faceVertIds.Add((int)face.Y);
                                if (referenceVTXL.vertWeightIndices.Count > 0)
                                {
                                    for (int bi = 0; bi < referenceVTXL.vertWeightIndices[(int)face.Y].Length; bi++)
                                    {
                                        if (!boneIds.Contains(referenceVTXL.vertWeightIndices[(int)face.Y][bi]))
                                        {
                                            boneIds.Add(referenceVTXL.vertWeightIndices[(int)face.Y][bi]);
                                        }
                                    }
                                }
                            }
                            if (!faceVertIds.Contains((int)face.Z))
                            {
                                faceVertDict.Add(face.Z, vertIndex++);
                                faceVertIds.Add((int)face.Z);
                                if (referenceVTXL.vertWeightIndices.Count > 0)
                                {
                                    for (int bi = 0; bi < referenceVTXL.vertWeightIndices[(int)face.Z].Length; bi++)
                                    {
                                        if (!boneIds.Contains(referenceVTXL.vertWeightIndices[(int)face.Z][bi]))
                                        {
                                            boneIds.Add(referenceVTXL.vertWeightIndices[(int)face.Z][bi]);
                                        }
                                    }
                                }
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
                        newTris.name = model.tempTris[modelId].name;
                        outModel.tempTris.Add(newTris);

                        //Copy vertex data based on faceVertIds ordering
                        VTXL vtxl = new VTXL();
                        for (int i = 0; i < faceVertIds.Count; i++)
                        {
                            appendVertex(referenceVTXL, vtxl, faceVertIds[i]);
                        }

                        //Add things that aren't linked to the vertex ids
                        if (referenceVTXL.bonePalette != null)
                        {
                            vtxl.bonePalette.AddRange(referenceVTXL.bonePalette);
                        }
                        boneIdList.Add(boneIds);
                        if(model.meshNames.Count > modelId)
                        {
                            outModel.meshNames.Add(model.meshNames[modelId]);
                        }
                        outModel.vtxlList.Add(vtxl);
                    }
                }

            }
        }

        public static void SplitByBoneCount(AquaObject model, int meshId, int boneLimit, bool forceVtxlSplit = false)
        {
            List<List<int>> faceLists = new List<List<int>>();

            var mesh = model.meshList[meshId];
            var tris = model.strips[mesh.psetIndex].GetTriangles();
            bool[] usedFaceIds = new bool[tris.Count];
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

                List<int> edgeVertCandidates = new List<int>();
                List<int> usedVerts = new List<int>();
                for (int f = startFace; f < usedFaceIds.Length; f++)
                {
                    if (usedFaceIds[f] != true)
                    {
                        //Used to watch how many bones are being used so it can be understood where to split the meshes.
                        //These also help to track the bones being used so the edge verts list can be generated. Not sure how important that is, but the game stores it.
                        List<int> faceBones = new List<int>();
                        int newBones = 0;

                        //Get vert ids of the face
                        float[] faceIds = VectorAsArray(tris[f]);

                        //Get bones in the face
                        foreach (float v in faceIds)
                        {
                            bool vertCheck = false;
                            for (int b = 0; b < model.vtxlList[mesh.vsetIndex].vertWeightIndices[(int)v].Length; b++)
                            {
                                int id = model.vtxlList[mesh.vsetIndex].vertWeightIndices[(int)v][b];
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
                            if (vertCheck && !edgeVertCandidates.Contains((int)v))
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
                            //Add new bones if there's room
                            foreach (int fb in faceBones)
                            {
                                if (!objBones.Contains(fb))
                                {
                                    objBones.Add(fb);
                                }
                            }
                            //Track used verts so we can figure out edge verts later
                            foreach (float id in faceIds)
                            {
                                if (!usedVerts.Contains((int)id))
                                {
                                    usedVerts.Add((int)id);
                                }
                            }
                            usedFaceIds[f] = true;
                            faceArray.Add(f);
                        }
                    }
                }
                //Determine what should be added to the final edge vert list
                foreach (int vert in edgeVertCandidates)
                {
                    if (!model.vtxlList[mesh.vsetIndex].edgeVerts.Contains((ushort)vert) && usedVerts.Contains(vert))
                    {
                        model.vtxlList[mesh.vsetIndex].edgeVerts.Add((ushort)vert);
                    }
                }
                faceLists.Add(faceArray);


                bool breakFromLoop = true;
                for (int i = 0; i < usedFaceIds.Length; i++)
                {
                    if (usedFaceIds[i] == false)
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
            SplitMesh(model, meshId, faceLists, forceVtxlSplit);
        }

        public static void SplitByBoneCountTempData(AquaObject model, AquaObject outModel, int modelId, int boneLimit)
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

                List<int> edgeVertCandidates = new List<int>();
                List<int> usedVerts = new List<int>();
                for (int f = startFace; f < usedFaceIds.Length; f++)
                {
                    if(usedFaceIds[f] != true)
                    {
                        //Used to watch how many bones are being used so it can be understood where to split the meshes.
                        //These also help to track the bones being used so the edge verts list can be generated. Not sure how important that is, but the game stores it.
                        List<int> faceBones = new List<int>();
                        int newBones = 0;

                        //Get vert ids of the face
                        float[] faceIds = VectorAsArray(model.tempTris[modelId].triList[f]);

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
                            if (vertCheck && !edgeVertCandidates.Contains((int)v))
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
                            //Add new bones if there's room
                            foreach (int fb in faceBones)
                            {
                                if (!objBones.Contains(fb))
                                {
                                    objBones.Add(fb);
                                }
                            }
                            //Track used verts so we can figure out edge verts later
                            foreach (float id in faceIds)
                            {
                                if (!usedVerts.Contains((int)id))
                                {
                                    usedVerts.Add((int)id);
                                }
                            }
                            usedFaceIds[f] = true;
                            faceArray.Add(f);
                        }
                    }
                }
                //Determine what should be added to the final edge vert list
                foreach (int vert in edgeVertCandidates)
                {
                    if (!model.vtxlList[modelId].edgeVerts.Contains((ushort)vert) && usedVerts.Contains(vert))
                    {
                        model.vtxlList[modelId].edgeVerts.Add((ushort)vert);
                    }
                }
                faceLists.Add(faceArray);


                bool breakFromLoop = true;
                for (int i = 0; i < usedFaceIds.Length; i++)
                {
                    if (usedFaceIds[i] == false)
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
            SplitMeshTempData(model, outModel, modelId, faceLists);
        }

        public static void RemoveAllUnusedBones(AquaObject model)
        {
            for(int i = 0; i < model.vtxlList.Count; i++)
            {
                RemoveUnusedBones(model.vtxlList[i]);
            }
        }

        public static void BatchSplitByBoneCount(AquaObject model, int boneLimit, bool forceVtxlSplit = false)
        {
            //Set up a temporary bone palette to be used in case a mesh needs it, such as for NGS meshes
            List<ushort> bonePalette = new List<ushort>();
            for (int b = 0; b < model.bonePalette.Count; b++)
            {
                bonePalette.Add((ushort)model.bonePalette[b]);
            }

            int startingMeshCount = model.meshList.Count;
            for (int i = 0; i < startingMeshCount; i++)
            {
                var mesh = model.meshList[i];
                //Make sure there's a bonepalette to pull from
                if (model.vtxlList[mesh.vsetIndex].bonePalette?.Count == 0)
                {
                    model.vtxlList[mesh.vsetIndex].bonePalette = bonePalette;
                }
                RemoveUnusedBones(model.vtxlList[mesh.vsetIndex]);
                //Pass to splitting function if beyond the limit, otherwise pass untouched
                if (model.vtxlList[mesh.vsetIndex].bonePalette.Count > boneLimit)
                {
                    SplitByBoneCount(model, i, boneLimit, forceVtxlSplit);
                }
            }

            //Get rid of this since we don't understand it and it's probably going to confuse the game if it's not processed along with these.
            model.strips2.Clear();
            model.strips3.Clear();
            model.pset2List.Clear();
            model.mesh2List.Clear();
            model.strips3Lengths.Clear();
        }

        public static void BatchSplitByBoneCountTempData(AquaObject model, AquaObject outModel, int boneLimit)
        {
            for (int i = 0; i < model.vtxlList.Count; i++)
            {
                RemoveUnusedBones(model.vtxlList[i]);
                //Pass to splitting function if beyond the limit, otherwise pass untouched
                if (model.vtxlList[i].bonePalette.Count > boneLimit)
                {
                    SplitByBoneCountTempData(model, outModel, i, boneLimit);
                } else
                {
                    CloneUnprocessedMesh(model, outModel, i);
                }
            }
            outModel.matUnicodeNames = model.matUnicodeNames;
            outModel.tempMats = model.tempMats;
        }

        //Seemingly not necessary on pso2 data since this split is already a requirement
        //Therefore, we only operate on temp data
        public static void SplitMeshByMaterialTempData(AquaObject model, AquaObject outModel)
        {
            for (int i = 0; i < model.tempTris.Count; i++)
            {
                SplitMeshTempData(model, outModel, i, GetAllMaterialFaceGroups(model, i));
            }
            outModel.matUnicodeNames = model.matUnicodeNames;
            outModel.tempMats = model.tempMats;
        }

        public static List<List<int>> GetAllMaterialFaceGroups(AquaObject model, int meshId)
        {
            List<List<int>> matGroups = new List<List<int>>();
            int matCount = 0;
            if(model.tempMats.Count > 0)
            {
                matCount = model.tempMats.Count;
            } else
            {
                matCount = model.mateList.Count;
            }
            for(int i = 0; i < matCount; i++)
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
                List<Vector3> tris = model.strips[i].GetTriangles(true);

                if(tris.Count > faceLimit)
                {

                }
            }

        }
        /*
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

        }*/

        public static void OptimizeGlobalBonePalette(AquaObject model)
        {
            List<uint> newBonePalette = new List<uint>();
            //Construct the bone palette
            for (int i = 0; i < model.vtxlList.Count; i++)
            {
                if (model.vtxlList[i].vertWeightIndices.Count > 0)
                {
                    for (int j = 0; j < model.vtxlList[i].vertWeightIndices.Count; j++)
                    {
                        for(int k = 0; k < model.vtxlList[i].vertWeightIndices[j].Length; k++)
                        {
                            if (!newBonePalette.Contains((uint)model.vtxlList[i].vertWeightIndices[j][k]))
                            {
                                newBonePalette.Add((uint)model.vtxlList[i].vertWeightIndices[j][k]);
                            }
                        }
                    }
                }
            }

            newBonePalette.Sort();
            if(newBonePalette.Count == 0)
            {
#if DEBUG
                Console.WriteLine("No bones to set up");
#endif
                return;
            }
            //Reassign indices
            for (int i = 0; i < model.vtxlList.Count; i++)
            {
                for(int j = 0; j < model.vtxlList[i].vertWeightIndices.Count; j++)
                {
                    List<int> usedIds = new List<int>();
                    for (int k = 0; k < model.vtxlList[i].vertWeightIndices[j].Length; k++)
                    {
                        if(usedIds.Contains(model.vtxlList[i].vertWeightIndices[j][k]))
                        {
                            model.vtxlList[i].vertWeightIndices[j][k] = 0;
                        } else
                        {
                            usedIds.Add(model.vtxlList[i].vertWeightIndices[j][k]);
                            model.vtxlList[i].vertWeightIndices[j][k] = newBonePalette.IndexOf(model.bonePalette[model.vtxlList[i].vertWeightIndices[j][k]]);
                        }
                    }
                }
            }
            model.bonePalette = newBonePalette;
        }

        public static void GenerateGlobalBonePalette(AquaObject model)
        {
            List<uint> newBonePalette = new List<uint>();
            //Construct the bone palette
            for (int i = 0; i < model.vtxlList.Count; i++)
            {
                if (model.vtxlList[i].vertWeightIndices.Count > 0)
                {
                    for (int j = 0; j < model.vtxlList[i].vertWeightIndices.Count; j++)
                    {
                        for (int k = 0; k < model.vtxlList[i].vertWeightIndices[j].Length; k++)
                        {
                            if (!newBonePalette.Contains((uint)model.vtxlList[i].vertWeightIndices[j][k]))
                            {
                                newBonePalette.Add((uint)model.vtxlList[i].vertWeightIndices[j][k]);
                            }
                        }
                    }
                }
            }

            //Construct from vtxl bonepalettes. More efficient, but doesn't optimize the list like we need for large bonecounts
            for (int i = 0; i < model.vtxlList.Count; i++)
            {
                if (model.vtxlList[i].bonePalette.Count > 0)
                {
                    for (int j = 0; j < model.vtxlList[i].bonePalette.Count; j++)
                    {
                        if (!newBonePalette.Contains(model.vtxlList[i].bonePalette[j]))
                        {
                            newBonePalette.Add(model.vtxlList[i].bonePalette[j]);
                        }
                    }
                }
            }
            newBonePalette.Sort();
            if (newBonePalette.Count == 0)
            {
#if DEBUG
                Console.WriteLine("No bones to set up");
#endif
                return;
            }
            //Reassign indices
            for (int i = 0; i < model.vtxlList.Count; i++)
            {
                for (int j = 0; j < model.vtxlList[i].vertWeightIndices.Count; j++)
                {
                    List<int> usedIds = new List<int>();
                    for (int k = 0; k < model.vtxlList[i].vertWeightIndices[j].Length; k++)
                    {
                        if (usedIds.Contains(model.vtxlList[i].vertWeightIndices[j][k]))
                        {
                            model.vtxlList[i].vertWeightIndices[j][k] = 0;
                        }
                        else
                        {
                            usedIds.Add(model.vtxlList[i].vertWeightIndices[j][k]);
                            model.vtxlList[i].vertWeightIndices[j][k] = newBonePalette.IndexOf(model.vtxlList[i].bonePalette[model.vtxlList[i].vertWeightIndices[j][k]]);
                        }
                    }
                }
            }
            model.bonePalette = newBonePalette;
        }

        public static void CloneUnprocessedMesh(AquaObject model, AquaObject outModel, int meshId)
        {
            if(model.meshNames.Count > meshId)
            {
                outModel.meshNames.Add(model.meshNames[meshId]);
            }
            if(model.vtxlList.Count > meshId)
            {
                outModel.vtxlList.Add(model.vtxlList[meshId]);
            }
            if(model.tempTris.Count > meshId)
            {
                outModel.tempTris.Add(model.tempTris[meshId]);
            }
        }

        //To be honest I don't really know what these actually do, but this seems to generate the structure roughly the way the game's exporter does.
        //Essentially, vertices between different meshes are linked together 
        public static void CalcUNRMs(AquaObject model, bool applyNormalAveraging, bool useUNRMs)
        {
            UNRM unrm = new UNRM();
            if (useUNRMs == false && applyNormalAveraging == false)
            {
                return;
            }

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
                    Vector3 normals = new Vector3();
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
                        unrm.unrmVertGroups.Add(meshNum.Count);
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
            if (useUNRMs)
            {
                model.unrms = unrm;
            }
        }

        //Removes bones in vtxls with unprocessed weights
        public static void RemoveUnusedBones(VTXL vtxl)
        {
            List<int> newBonePaletteCheck = new List<int>();
            List<ushort> newBonePalette = new List<ushort>();

            //Loop through weight indices and gather them as they're stored
            for(int v = 0; v < vtxl.vertWeightIndices.Count; v++)
            {
                List<int> usedIds = new List<int>();
                for(int vi = 0; vi < vtxl.vertWeightIndices[v].Length; vi++)
                {
                    //Repeat ids shouldn't exist and should be 0ed. Usually a duplicate implies that the original was 0 anyways.
                    if (usedIds.Contains(vtxl.vertWeightIndices[v][vi]))
                    {
                        vtxl.vertWeightIndices[v][vi] = 0;
                    } else
                    {
                        usedIds.Add(vtxl.vertWeightIndices[v][vi]);
                        if (!newBonePaletteCheck.Contains(vtxl.vertWeightIndices[v][vi]))
                        {
                            newBonePaletteCheck.Add(vtxl.vertWeightIndices[v][vi]);
                            newBonePalette.Add(vtxl.bonePalette[vtxl.vertWeightIndices[v][vi]]);
                            vtxl.vertWeightIndices[v][vi] = (newBonePaletteCheck.Count - 1);
                        }
                        else
                        {
                            vtxl.vertWeightIndices[v][vi] = newBonePaletteCheck.IndexOf(vtxl.vertWeightIndices[v][vi]);
                        }
                    }
                }
            }

            vtxl.bonePalette = newBonePalette;
        }

        public static void WriteVTXL(VTXE vtxe, VTXL vtxl, List<byte> outBytes2, int sizeToFill = -1)
        {
            int padding = 0;
            if(sizeToFill != -1)
            {
                padding = sizeToFill - GetVTXESize(vtxe);
            } else
            {
                padding = 0;
            }

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
                            switch(vtxe.vertDataTypes[j].structVariation)
                            {
                                case 0x4:
                                    outBytes2.AddRange(ConvertStruct(vtxl.vertWeights[i]));
                                    break;
                                case 0x11:
                                    for (int id = 0; id < 4; id++)
                                    {
                                        outBytes2.AddRange(BitConverter.GetBytes(vtxl.vertWeightsNGS[i][id]));
                                    }
                                    break;
                            }
                            break;
                        case (int)ClassicAquaObject.VertFlags.VertNormal:
                            switch (vtxe.vertDataTypes[j].structVariation)
                            {
                                case 0x3:
                                    outBytes2.AddRange(ConvertStruct(vtxl.vertNormals[i]));
                                    break;
                                case 0xF:
                                    for (int id = 0; id < 4; id++)
                                    {
                                        outBytes2.AddRange(BitConverter.GetBytes(vtxl.vertNormalsNGS[i][id]));
                                    }
                                    break;
                            }
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
                                outBytes2.Add((byte)vtxl.vertWeightIndices[i][weight]);
                            }
                            break;
                        case (int)ClassicAquaObject.VertFlags.VertUV1:
                            switch (vtxe.vertDataTypes[j].structVariation)
                            {
                                case 0x2:
                                    outBytes2.AddRange(ConvertStruct(vtxl.uv1List[i]));
                                    break;
                                case 0xE:
                                    for (int id = 0; id < 2; id++)
                                    {
                                        outBytes2.AddRange(BitConverter.GetBytes(vtxl.uv1ListNGS[i][id]));
                                    }
                                    break;
                            }
                            break;
                        case (int)ClassicAquaObject.VertFlags.VertUV2:
                            switch (vtxe.vertDataTypes[j].structVariation)
                            {
                                case 0x2:
                                    outBytes2.AddRange(ConvertStruct(vtxl.uv2List[i]));
                                    break;
                                case 0xE:
                                    for (int id = 0; id < 2; id++)
                                    {
                                        outBytes2.AddRange(BitConverter.GetBytes(vtxl.uv2ListNGS[i][id]));
                                    }
                                    break;
                            }
                            break;
                        case (int)ClassicAquaObject.VertFlags.VertUV3:
                            switch (vtxe.vertDataTypes[j].structVariation)
                            {
                                case 0x2:
                                    outBytes2.AddRange(ConvertStruct(vtxl.uv3List[i]));
                                    break;
                                case 0xE:
                                    for (int id = 0; id < 2; id++)
                                    {
                                        outBytes2.AddRange(BitConverter.GetBytes(vtxl.uv3ListNGS[i][id]));
                                    }
                                    break;
                            }
                            break;
                        case (int)ClassicAquaObject.VertFlags.VertUV4:
                            switch (vtxe.vertDataTypes[j].structVariation)
                            {
                                case 0x2:
                                    outBytes2.AddRange(ConvertStruct(vtxl.uv4List[i]));
                                    break;
                                case 0xE:
                                    for (int id = 0; id < 2; id++)
                                    {
                                        outBytes2.AddRange(BitConverter.GetBytes(vtxl.uv4ListNGS[i][id]));
                                    }
                                    break;
                            }
                            break;
                        case (int)ClassicAquaObject.VertFlags.VertTangent:
                            switch (vtxe.vertDataTypes[j].structVariation)
                            {
                                case 0x3:
                                    outBytes2.AddRange(ConvertStruct(vtxl.vertTangentList[i]));
                                    break;
                                case 0xF:
                                    for (int id = 0; id < 4; id++)
                                    {
                                        outBytes2.AddRange(BitConverter.GetBytes(vtxl.vertTangentListNGS[i][id]));
                                    }
                                    break;
                            }
                            break;
                        case (int)ClassicAquaObject.VertFlags.VertBinormal:
                            switch (vtxe.vertDataTypes[j].structVariation)
                            {
                                case 0x3:
                                    outBytes2.AddRange(ConvertStruct(vtxl.vertBinormalList[i]));
                                    break;
                                case 0xF:
                                    for (int id = 0; id < 4; id++)
                                    {
                                        outBytes2.AddRange(BitConverter.GetBytes(vtxl.vertBinormalListNGS[i][id]));
                                    }
                                    break;
                            }
                            break;
                        case (int)NGSAquaObject.NGSVertFlags.Vert0x22:
                            for (int id = 0; id < 2; id++)
                            {
                                outBytes2.AddRange(BitConverter.GetBytes(vtxl.vert0x22[i][id]));
                            }
                            break;
                        case (int)NGSAquaObject.NGSVertFlags.Vert0x23:
                            for (int id = 0; id < 2; id++)
                            {
                                outBytes2.AddRange(BitConverter.GetBytes(vtxl.vert0x23[i][id]));
                            }
                            break;
                        case (int)NGSAquaObject.NGSVertFlags.Vert0x24:
                            for (int id = 0; id < 2; id++)
                            {
                                outBytes2.AddRange(BitConverter.GetBytes(vtxl.vert0x24[i][id]));
                            }
                            break;
                        case (int)NGSAquaObject.NGSVertFlags.Vert0x25:
                            for (int id = 0; id < 2; id++)
                            {
                                outBytes2.AddRange(BitConverter.GetBytes(vtxl.vert0x25[i][id]));
                            }
                            break;
                        default:
                            MessageBox.Show($"Unknown Vert type {vtxe.vertDataTypes[j].dataType}! Please report!");
                            throw new Exception("Not implemented!");
                    }
                }
                outBytes2.AddRange(new byte[padding]);
            }
        }

        public static int GetVTXESize(VTXE vtxe)
        {
            int size = 0;
            for (int j = 0; j < vtxe.vertDataTypes.Count; j++)
            {
                switch (vtxe.vertDataTypes[j].dataType)
                {
                    case (int)ClassicAquaObject.VertFlags.VertPosition:
                        size += 0xC;
                        break;
                    case (int)ClassicAquaObject.VertFlags.VertWeight:
                        switch (vtxe.vertDataTypes[j].structVariation)
                        {
                            case 0x4:
                                size += 0x10;
                                break;
                            case 0x11:
                                size += 0x8;
                                break;
                        }
                        break;
                    case (int)ClassicAquaObject.VertFlags.VertNormal:
                        switch (vtxe.vertDataTypes[j].structVariation)
                        {
                            case 0x3:
                                size += 0xC;
                                break;
                            case 0xF:
                                size += 0x8;
                                break;
                        }
                        break;
                    case (int)ClassicAquaObject.VertFlags.VertColor:
                        size += 0x4;
                        break;
                    case (int)ClassicAquaObject.VertFlags.VertColor2:
                        size += 0x4;
                        break;
                    case (int)ClassicAquaObject.VertFlags.VertWeightIndex:
                        size += 0x4;
                        break;
                    case (int)ClassicAquaObject.VertFlags.VertUV1:
                        switch (vtxe.vertDataTypes[j].structVariation)
                        {
                            case 0x2:
                                size += 0x8;
                                break;
                            case 0xE:
                                size += 0x4;
                                break;
                        }
                        break;
                    case (int)ClassicAquaObject.VertFlags.VertUV2:
                        switch (vtxe.vertDataTypes[j].structVariation)
                        {
                            case 0x2:
                                size += 0x8;
                                break;
                            case 0xE:
                                size += 0x4;
                                break;
                        }
                        break;
                    case (int)ClassicAquaObject.VertFlags.VertUV3:
                        switch (vtxe.vertDataTypes[j].structVariation)
                        {
                            case 0x2:
                                size += 0x8;
                                break;
                            case 0xE:
                                size += 0x4;
                                break;
                        }
                        break;
                    case (int)ClassicAquaObject.VertFlags.VertUV4:
                        switch (vtxe.vertDataTypes[j].structVariation)
                        {
                            case 0x2:
                                size += 0x8;
                                break;
                            case 0xE:
                                size += 0x4;
                                break;
                        }
                        break;
                    case (int)ClassicAquaObject.VertFlags.VertTangent:
                        switch (vtxe.vertDataTypes[j].structVariation)
                        {
                            case 0x3:
                                size += 0xC;
                                break;
                            case 0xF:
                                size += 0x8;
                                break;
                        }
                        break;
                    case (int)ClassicAquaObject.VertFlags.VertBinormal:
                        switch (vtxe.vertDataTypes[j].structVariation)
                        {
                            case 0x3:
                                size += 0xC;
                                break;
                            case 0xF:
                                size += 0x8;
                                break;
                        }
                        break;
                    case (int)NGSAquaObject.NGSVertFlags.Vert0x22:
                        size += 0x4;
                        break;
                    case (int)NGSAquaObject.NGSVertFlags.Vert0x23:
                        size += 0x4;
                        break;
                    case (int)NGSAquaObject.NGSVertFlags.Vert0x24:
                        size += 0x4;
                        break;
                    case (int)NGSAquaObject.NGSVertFlags.Vert0x25:
                        size += 0x4;
                        break;
                    default:
                        MessageBox.Show($"Unknown Vert type {vtxe.vertDataTypes[j].dataType}! Please report!");
                        throw new Exception("Not implemented!");
                }


            }
            return size;
        }

        public static string GetPSO2String(byte* str, int end)
        {
            string finalText;

            byte[] text = new byte[end];
            Marshal.Copy(new IntPtr(str), text, 0, end);
            finalText = System.Text.Encoding.UTF8.GetString(text);

            return finalText;
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
                case "rbd_d":
                case "reboot_bd_d":
                case "reboot_pl_d":
                case "reboot_player_d":
                case "reboot_ba_decal":
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
                    texNames.Add("pl_body_decal.dds");
                    break;
                case "rhr":
                case "reboot_hair":
                    if (mat.shaderNames == null)
                    {
                        mat.shaderNames = new List<string>() { "1103p", "1103" };
                    }
                    texNames.Add("pl_hair_diffuse.dds");
                    texNames.Add("pl_hair_multi.dds");
                    texNames.Add("pl_hair_normal.dds");
                    texNames.Add("pl_hair_alpha.dds");
                    texNames.Add("pl_hair_noise.dds");
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
                case "rbd_ou":
                case "reboot_ou":
                case "reboot_outer":
                    texNames.Add("pl_body_outer_diffuse.dds");
                    texNames.Add("pl_body_outer_multi.dds");
                    texNames.Add("pl_body_outer_normal.dds");
                    texNames.Add("pl_body_outer_mask.dds");
                    texNames.Add("pl_body_outer_subnormal_01.dds");
                    texNames.Add("pl_body_outer_subnormal_02.dds");
                    texNames.Add("pl_body_outer_subnormal_03.dds");
                    break;
                case "rbd_ou_d":
                case "rbd_ou_decal":
                case "reboot_outer_decal":
                    texNames.Add("pl_body_outer_diffuse.dds");
                    texNames.Add("pl_body_outer_multi.dds");
                    texNames.Add("pl_body_outer_normal.dds");
                    texNames.Add("pl_body_outer_mask.dds");
                    texNames.Add("pl_body_outer_subnormal_01.dds");
                    texNames.Add("pl_body_outer_subnormal_02.dds");
                    texNames.Add("pl_body_outer_subnormal_03.dds");
                    texNames.Add("pl_body_decal.dds");
                    break;
                default:
                    break;
            }
            mat.texNames = texNames;
        }

        public static string GetSpecialMatType(List<string> names)
        {
            if (names.Count > 0)
            {
                var name = names[0];
                switch (name)
                {
                    case "pl_eye_diffuse.dds":
                        return "ey";
                    case "pl_hair_diffuse.dds":
                        switch (names[names.Count - 1])
                        {
                            case "pl_hair_noise.dds":
                                return "rhr";
                            default:
                                return "hr";
                        }
                    case "pl_face_diffuse.dds":
                        return "fc";
                    case "pl_body_diffuse.dds":
                        return "pl";
                    case "pl_body_base_diffuse.dds":
                        switch (names[names.Count - 1])
                        {
                            case "pl_body_decal.dds":
                                return "rbd_d";
                            default:
                                return "rbd";
                        }
                    case "pl_body_skin_diffuse.dds":
                        return "rbd_sk";
                    case "pl_body_outer_diffuse.dds":
                        switch(names[names.Count - 1])
                        {
                            case "pl_body_decal.dds":
                                return "rbd_ou_d";
                            default:
                                return "rbd_ou";
                        }
                    default:
                        return "";
                }

            }
            else
            {
                return "";
            }
        }

        //Used to generate standard materials in which the texture names are used. 
        public static void GenerateMaterial(AquaObject model, GenericMaterial mat, bool ngsMat = false)
        {
            if(mat.specialType != null)
            {
                GenerateSpecialMaterialParameters(mat);
            }
            if (mat.shaderNames == null || mat.shaderNames.Count < 2)
            {
                mat.shaderNames = DefaultShaderNames;
            }
            int texArrayStartIndex = model.tstaList.Count;
            List<int> tempTexIds = new List<int>();
            TSET tset = new TSET();
            MATE mate = new MATE();
            SHAD shad = new SHAD();
            REND rend = new REND();
            shad.isNGS = ngsMat;

            //Set up textures
            var shaderKey = $"{mat.shaderNames[0]} {mat.shaderNames[1]}";
            if(PSO2ShaderTexSetPresets.shaderTexSet.ContainsKey(shaderKey))
            {
                Dictionary<string, int> setTracker = new Dictionary<string, int>();
                var set = PSO2ShaderTexSetPresets.shaderTexSet[shaderKey];
                var info = PSO2ShaderTexInfoPresets.tstaTexSet[shaderKey];
                string firstTexName; //Base tex string in the case we need to generate the others
                if(mat.texNames?.Count > 0)
                {
                    firstTexName = mat.texNames[0];
                    if(firstTexName.Contains("_"))
                    {
                        int _index = -1;
                        for(int i = firstTexName.Length - 1; i > 0; i--)
                        {
                            if(firstTexName[i] == '_')
                            {
                                _index = i;
                                break;
                            }
                        }
                        if(_index == 0)
                        {
                            firstTexName = "";
                        } else if(_index != -1)
                        {
                            firstTexName = firstTexName.Substring(0, _index);
                        }
                    }
                } else
                {
                    firstTexName = "tex";
                }
                for(int i = 0; i < set.Count; i++)
                {
                    var tex = set[i];
                    string curTexStr = "";
                    if(setTracker.ContainsKey(tex))
                    {
                        int curNum = setTracker[tex] = setTracker[tex] + 1;
                        curTexStr = tex + curNum.ToString();
                    } else
                    {
                        curTexStr = tex + "0";
                        setTracker.Add(tex, 0);
                    }

                    TEXF texf = new TEXF();
                    TSTA tsta = new TSTA();
                    if(info.ContainsKey(curTexStr))
                    {
                        tsta = info[curTexStr];
                    } else
                    {
                        if(TSTATypePresets.tstaTypeDict.ContainsKey(curTexStr))
                        {
                            tsta = TSTATypePresets.tstaTypeDict[curTexStr];
                        } else
                        {
                            tsta = TSTATypePresets.defaultPreset;
                        }
                    }
                    if (mat.texNames?.Count > i)
                    {
                        tsta.texName.SetString(mat.texNames[i]);
                        texf.texName.SetString(mat.texNames[i]);
                    } else
                    {
                        string texName = firstTexName + "_" + tex + ".dds";
                        tsta.texName.SetString(texName);
                        texf.texName.SetString(texName);
                    }

                    //Make sure the texf list only has unique entries
                    int texId = -1;
                    bool isUniqueTexname = true;
                    for(int t = 0; t < model.texfList.Count; t++)
                    {
                        var tempTex = model.texfList[t];
                        if(tempTex.texName == texf.texName)
                        {
                            isUniqueTexname = false;
                        }
                    }
                    if(isUniqueTexname)
                    {
                        model.texfList.Add(texf);
                    }

                    //Cull full duplicates of tsta
                    bool isUniqueTsta = true;
                    for (int t = 0; t < model.tstaList.Count; t++)
                    {
                        var tempTex = model.tstaList[t];
                        if (tempTex == tsta)
                        {
                            texId = t;
                            isUniqueTsta = false;
                        }
                    }
                    if (isUniqueTsta)
                    {
                        model.tstaList.Add(tsta);
                    }
                    tempTexIds.Add(texId == -1 ? model.texfList.Count - 1 : texId);
                }
            } else
            {
                if (mat.texNames != null)
                {
                    for (int i = 0; i < mat.texNames.Count; i++)
                    {
                        bool foundCopy = false;
                        for (int texIndex = 0; texIndex < model.texfList.Count; texIndex++)
                        {
                            if (mat.texNames[i].Equals(model.texfList[texIndex].texName.GetString()))
                            {
                                tempTexIds.Add(texIndex);
                                foundCopy = true;
                                break;
                            }
                        }

                        if (foundCopy == true)
                        {
                            continue;
                        }

                        TEXF texf = new TEXF();
                        TSTA tsta = new TSTA();

                        texf.texName.SetString(mat.texNames[i]);

                        tsta.tag = 0x16; //Reexamine this one when possible, these actually vary a bit in 0xC33 variants.
                                         //NGS does some funny things with the tex usage order int
                        if (mat.texNames[i].Contains("subnormal_"))
                        {
                            tsta.texUsageOrder = 0xA;
                        }
                        else
                        {
                            tsta.texUsageOrder = texArrayStartIndex + i;
                        }
                        if (mat.texUVSets == null)
                        {
                            tsta.modelUVSet = 0;
                        }
                        else
                        {
                            tsta.modelUVSet = mat.texUVSets[i];
                        }
                        var unkVector0 = new Vector3();
                        if (ngsMat == true)
                        {
                            unkVector0.Z = 1;
                        }

                        tsta.unkVector0 = unkVector0;
                        tsta.unkFloat2 = 0;
                        tsta.unkFloat3 = 0;
                        tsta.unkFloat4 = 0;
                        tsta.unkInt3 = 1;
                        tsta.unkInt4 = 1;
                        tsta.unkInt5 = 1;
                        tsta.unkFloat0 = 0f;
                        tsta.unkFloat1 = 0f;
                        tsta.texName = texf.texName;

                        model.texfList.Add(texf);
                        model.tstaList.Add(tsta);
                        tempTexIds.Add(model.texfList.Count - 1);
                    }
                }
            }

            //Set up texture set.
            tset.tstaTexIDs = tempTexIds;
            tset.texCount = tempTexIds.Count;

            //Set up material
            mate.diffuseRGBA = mat.diffuseRGBA;
            mate.unkRGBA0 = mat.unkRGBA0;
            mate._sRGBA = mat._sRGBA;
            mate.unkRGBA1 = mat.unkRGBA1;
            mate.reserve0 = mat.reserve0;
            mate.unkFloat0 = mat.unkFloat0;
            mate.unkFloat1 = mat.unkFloat1;
            mate.unkInt0 = mat.unkInt0;
            mate.unkInt1 = mat.unkInt1;
            if(mat.blendType == null || mat.blendType == "")
            {
                mat.blendType = "blendalpha";
            }
            mate.alphaType.SetString(mat.blendType);
            mate.matName.SetString(mat.matName);

            //Set up SHAD
            string key = mat.shaderNames[0] + " " + mat.shaderNames[1];
            shad.pixelShader.SetString(mat.shaderNames[0]);
            shad.vertexShader.SetString(mat.shaderNames[1]);

            //Only in NGS shaders, but in theory could come up in others. Otherwise 0. No idea what this is.
            if(NGSShaderUnk0ValuesPresets.ShaderUnk0Values.TryGetValue(key, out var unk0Val))
            {
                shad.unk0 = unk0Val;
            }

            if(ngsMat)
            {
                var ngsShad = shad;
                if(NGSShaderDetailPresets.NGSShaderDetail.TryGetValue(key, out var detailVal))
                {
                    ngsShad.shadDetail = detailVal;
                    ngsShad.shadDetailOffset = 1; 
                }
                if (NGSShaderExtraPresets.NGSShaderExtra.TryGetValue(key, out var extraVal))
                {
                    ngsShad.shadExtra = extraVal;
                    ngsShad.shadExtraOffset = 1;
                }
            }

            //Set up REND
            rend.tag = 0x1FF;
            rend.unk0 = 3;
            rend.twosided = mat.twoSided;

            rend.sourceAlpha = 5;
            rend.destinationAlpha = 6;
            rend.unk3 = 1;
            rend.unk4 = 0;

            rend.unk5 = 5;
            rend.unk6 = 6;
            rend.unk7 = 1;
            //rend.unk8 = 1;

            rend.unk9 = 5;
            rend.alphaCutoff = mat.alphaCutoff;
            rend.unk11 = 1;
            rend.unk12 = 4;
            rend.unk13 = 1;

            switch (mat.blendType)
            {
                case "add":
                    rend.unk0 = 1;
                    rend.int_0C = 1;
                    rend.unk8 = 1;
                    rend.destinationAlpha = 2;
                    break;
                case "opaque":
                    rend.int_0C = 0;
                    rend.unk8 = 0;
                    break;
                case "hollow":
                    rend.int_0C = 0;
                    rend.unk8 = 1;
                    rend.twosided = 2;
                    break;
                case "blendalpha":
                default:
                    rend.int_0C = 1;
                    rend.unk8 = 1;
                    break;
            }

            model.tsetList.Add(tset);
            model.mateList.Add(mate);
            model.shadList.Add(shad);
            model.rendList.Add(rend);
            model.matUnicodeNames.Add(mat.matName);
        }

        public static List<TSTA> GetTexListTSTAs(AquaObject model, int tsetIndex)
        {
            List<TSTA> textureList = new List<TSTA>();

            //Don't try to read what's not there
            if (model.tstaList.Count == 0 || model.tstaList == null)
            {
                return textureList;
            }
            TSET tset = model.tsetList[tsetIndex];

            for (int index = 0; index < tset.tstaTexIDs.Count; index++)
            {
                int texIndex = tset.tstaTexIDs[index];
                if (texIndex != -1)
                {
                    TSTA tsta = model.tstaList[texIndex];

                    textureList.Add(tsta);
                }
            }

            return textureList;
        }

        public static List<string> GetTexListNames(AquaObject model, int tsetIndex)
        {
            List<string> textureList = new List<string>();
            
            //Don't try to read what's not there
            if(model.tstaList.Count == 0 || model.tstaList == null)
            {
                return textureList;
            }
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

        public static List<int> GetTexListUVChannels(AquaObject model, int tsetIndex)
        {
            List<int> uvList = new List<int>();

            //Don't try to read what's not there
            if (model.tstaList.Count == 0 || model.tstaList == null)
            {
                return uvList;
            }
            TSET tset = model.tsetList[tsetIndex];

            for (int index = 0; index < tset.tstaTexIDs.Count; index++)
            {
                int texIndex = tset.tstaTexIDs[index];
                if (texIndex != -1)
                {
                    TSTA tsta = model.tstaList[texIndex];

                    uvList.Add(tsta.modelUVSet);
                }
            }

            return uvList;
        }

        public static string RemoveBlendersShitDuplicateDenoterAndAssimpDuplicate(string name)
        {
            if(name.Length < 5)
            {
                return name;
            }
            if(name[name.Length - 4] == '.')
            {
                return name.Substring(0, name.Length - 4);
            }
            if(name[name.Length - 1] == ')')
            {
                if(name.Contains('('))
                {
                    int index = name.Length - 1;
                    for(int i = name.Length - 1; i > 0; i--)
                    {
                        if(name[i] == '(')
                        {
                            index = i;
                            break;
                        }
                    }
                    return name.Substring(0, index);
                }
            }
            return name;
        }

        public static void GetMaterialNameData(ref string name, ref List<string> shaderNames, out string alphaType, out string playerFlag, out int twoSided, out int alphaCutoff)
        {
            name = RemoveBlendersShitDuplicateDenoterAndAssimpDuplicate(name);
            shaderNames = new List<string>();
            alphaType = null;
            alphaCutoff = 0;
            playerFlag = null;
            twoSided = 0;
            if (name.Contains('|'))
            {
                return;
            }

            //Get shader names
            string[] nameArr = name.Split(')');
            if(nameArr.Length > 1)
            {
                string shaderText = nameArr[0].Split('(')[1];
                var shaderSet = shaderText.Split(',');
                if(shaderSet.Length >= 2)
                {
                    shaderNames.Add(shaderSet[0]);
                    shaderNames.Add(shaderSet[1]);
                    name = nameArr[1];
                }
            }
            
            //Get alpha type
            nameArr = name.Split('}');
            if(nameArr.Length > 1)
            {
                alphaType = nameArr[0].Split('{')[1];
                name = nameArr[1];
            }

            //Get player flags
            nameArr = name.Split(']');
            if (nameArr.Length > 1)
            {
                playerFlag = nameArr[0].Split('[')[1];
                name = nameArr[1];
            }

            //Get two-sided and alphaCutoff
            nameArr = name.Split('@');
            name = nameArr[0];
            if(nameArr.Length > 1)
            {
                for(int i = 1; i < nameArr.Length; i++)
                {
                    switch(i)
                    {
                        case 1:
                            twoSided = Int32.Parse(nameArr[i]);
                            break;
                        case 2:
                            alphaCutoff = Int32.Parse(nameArr[i]);
                            break;
                    }
                }
            }
        }

        public static List<string> GetShaderNames(AquaObject model, int shadIndex)
        {
            List<string> shaderList = new List<string>();

            SHAD shad = model.shadList[shadIndex];

            shaderList.Add(shad.pixelShader.GetString());
            shaderList.Add(shad.vertexShader.GetString());

            return shaderList;
        }


    }
}
