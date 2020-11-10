using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Documents;
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
                        outModel.tempTris.Add(new genericTriangles(tempFaces, tempFaceMatIds));

                        //Copy vertex data based on faceVertIds ordering
                        VTXL vtxl = new VTXL();
                        for (int i = 0; i < faceVertIds.Count; i++)
                        {
                            //Should probably always have vert position data, perhaps not other stuff
                            vtxl.vertPositions.Add(model.vtxlList[modelId].vertPositions[faceVertIds[i]]);
                            if (model.vtxlList[modelId].vertWeights != null)
                            {
                                vtxl.vertWeights.Add(model.vtxlList[modelId].vertWeights[faceVertIds[i]]);
                            }
                            if (model.vtxlList[modelId].vertNormals != null)
                            {
                                vtxl.vertNormals.Add(model.vtxlList[modelId].vertNormals[faceVertIds[i]]);
                            }
                            if (model.vtxlList[modelId].vertColors != null)
                            {
                                vtxl.vertColors.Add(model.vtxlList[modelId].vertColors[faceVertIds[i]]);
                            }
                            if (model.vtxlList[modelId].vertColor2s != null)
                            {
                                vtxl.vertColor2s.Add(model.vtxlList[modelId].vertColor2s[faceVertIds[i]]);
                            }
                            if (model.vtxlList[modelId].vertWeightIndices != null)
                            {
                                vtxl.vertWeightIndices.Add(model.vtxlList[modelId].vertWeightIndices[faceVertIds[i]]);
                            }
                            if (model.vtxlList[modelId].uv1List != null)
                            {
                                vtxl.uv1List.Add(model.vtxlList[modelId].uv1List[faceVertIds[i]]);
                            }
                            if (model.vtxlList[modelId].uv2List != null)
                            {
                                vtxl.uv2List.Add(model.vtxlList[modelId].uv2List[faceVertIds[i]]);
                            }
                            if (model.vtxlList[modelId].uv3List != null)
                            {
                                vtxl.uv3List.Add(model.vtxlList[modelId].uv3List[faceVertIds[i]]);
                            }

                            //Non pso2 sections
                            if (model.vtxlList[modelId].rawVertWeights != null)
                            {
                                vtxl.rawVertWeights.Add(model.vtxlList[modelId].rawVertWeights[faceVertIds[i]]);
                            }
                            if (model.vtxlList[modelId].rawVertIds != null)
                            {
                                vtxl.rawVertIds.Add(model.vtxlList[modelId].rawVertIds[faceVertIds[i]]);
                            }

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

        public void SplitByBoneCount(AquaObject model, AquaObject outModel, int modelId, int boneLimit)
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

        public void BatchSplitByBoneCount(AquaObject model, AquaObject outModel, int boneLimit)
        {
            for (int i = 0; i < model.vtxlList.Count; i++)
            {
                RemoveUnusedBones(model.vtxlList[i]);
                //Pass to splitting function if beyond the limit, otherwise pass untouched
                if (model.vtxlList[i].rawVertIds != null && model.vtxlList[i].bonePalette.Count > boneLimit)
                {
                    SplitByBoneCount(model, outModel, i, boneLimit);
                } else
                {
                    CloneUnprocessedMesh(model, outModel, i);
                }
            }

        }

        public void SplitMeshByMaterial(AquaObject model, AquaObject outModel)
        {
            for (int i = 0; i < model.tempTris.Count; i++)
            {
                splitMesh(model, outModel, i, GetAllMaterialFaceGroups(model, i));
            }
        }

        public List<List<int>> GetAllMaterialFaceGroups(AquaObject model, int meshId)
        {
            List<List<int>> matGroups = new List<List<int>>();
            for(int i = 0; i < model.mateList.Count; i++)
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

        public void CloneUnprocessedMesh(AquaObject model, AquaObject outModel, int meshId)
        {
            outModel.vtxlList.Add(model.vtxlList[meshId]);
            outModel.tempTris.Add(model.tempTris[meshId]);
        }

        //To be honest I don't really know what these actually do, but this seems to generate the structure the way the game does.
        public void CalcUNRMs(AquaObject model, bool applyNormalAveraging)
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
                            if(!sameVert && model.vtxlList[n].vertPositions[w].Equals(model.vtxlList[m].vertPositions[v]) && !meshCheckArr[n][w])
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

            model.unrms = unrm;
        }

        //Removes bones in vtxls with unprocessed weights
        public void RemoveUnusedBones(VTXL vtxl)
        {
            Dictionary<int, int> oldToNewId = new Dictionary<int, int>();
            List<int> bonesIndicesUsed = new List<int>();
            List<ushort> bonePaletteClone = new List<ushort>();
            bonePaletteClone.AddRange(vtxl.bonePalette);
            
            
            //Check for and collect bones which are referenced
            for(int i = 0; i < vtxl.rawVertIds.Count; i++)
            {
                for(int j = 0; j < vtxl.rawVertIds[i].Count; j++)
                {
                    if(!bonesIndicesUsed.Contains(vtxl.rawVertIds[i][j]))
                    {
                        bonesIndicesUsed.Add(vtxl.rawVertIds[i][j]);
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
                    vtxl.rawVertIds[i][j] = oldToNewId[vtxl.rawVertIds[i][j]];
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

        public float[] VectorAsArray(Vector3 vec3)
        {
            return new float[] { vec3.X, vec3.Y, vec3.Z }; 
        }
    }
}
