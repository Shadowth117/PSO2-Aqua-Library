using AquaModelLibrary.Data.Gamecube;
using AquaModelLibrary.Data.Ninja.Model.Ginja;
using AquaModelLibrary.Data.Ninja.Motion;
using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaNodeData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.Intermediary;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.MathHelpers;
using AquaModelLibrary.Helpers.Readers;
using NvTriStripDotNet;
using System.Numerics;
using System.Reflection.Metadata;
using System.Transactions;

namespace AquaModelLibrary.Data.Ninja.Model
{
    public class NinjaModelConvert
    {
        public static AquaObject GinjaConvert(string fileName, out AquaNode aqn, List<string> texNames = null)
        {
            return ModelConvert(File.ReadAllBytes(fileName), NinjaVariant.Ginja, out aqn);
        }

        public static AquaObject XjConvert(string fileName, out AquaNode aqn, List<string> texNames = null)
        {
            return ModelConvert(File.ReadAllBytes(fileName), NinjaVariant.XJ, out aqn);
        }

        public static AquaObject ModelConvert(byte[] ninjaModel, NinjaVariant variant, out AquaNode aqn, int offset = 0, List<string> texNames = null)
        {
            using (var ms = new MemoryStream(ninjaModel))
            using (var sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                return ModelConvert(sr, variant, out aqn, offset);
            }
        }
        public static void ModelAnimConvert(byte[] ninjaModel, NinjaVariant variant, List<NJSMotion> motions, out AquaObject aqo, out List<AquaMotion> aqms, out AquaNode aqn, int offset = 0, List<string> texNames = null)
        {
            aqo = ModelConvert(ninjaModel, variant, out aqn, offset, texNames);
            aqms = NinjaMotionConvert.NJMToAqmList(motions);
        }

        public static NJSObject ConvertToNinja(AquaObject aqo, AquaNode aqn, NinjaVariant variant, bool forceSingleWeightPerMesh, out NJTextureList njTL)
        {
            switch (variant)
            {
                case NinjaVariant.Basic:
                case NinjaVariant.XJ:
                    //Since standard Ninja Basic and XJ do not support weights, forceSingleWeightPerMesh will act as true
                    forceSingleWeightPerMesh = true;
                    break;
                case NinjaVariant.Chunk:
                case NinjaVariant.Ginja:
                    break;
            }

            bool isSingleWeightPerMesh = false;
            if(forceSingleWeightPerMesh)
            {
                foreach(var vtxl in aqo.vtxlList)
                {
                    if(vtxl.vertWeightIndices.Count > 0 && vtxl.vertWeights.Count > 0)
                    {
                        int highestWeightIndex = GetHighestWeightIndex(vtxl);
                        for (int i = 0; i < vtxl.vertWeightIndices.Count; i++)
                        {
                            vtxl.vertWeightIndices[i] = new int[] { highestWeightIndex, 0, 0, 0 };
                            vtxl.vertWeights[i] = new Vector4(1, 0, 0, 0);
                        }
                    }
                }
                isSingleWeightPerMesh = true;
            }

            njTL = new NJTextureList();
            if(aqo.texFUnicodeNames.Count > 0)
            {
                njTL.texNames = aqo.texFUnicodeNames;
            } else
            {
                njTL.texNames = aqo.texfList.Select(x => x.texName.GetString()).ToList();
            }
            List<NJSObject> njsObjects = new List<NJSObject>();

            //Do first loop to set up bulk of bone info
            for(int i = 0; i < aqn.nodeList.Count; i++)
            {
                var bn = aqn.nodeList[i];
                var bnTfm = bn.GetInverseBindPoseMatrixInverted();
                if(bn.parentId != -1)
                {
                    var pn = aqn.nodeList[bn.parentId];
                    var pnInvTfm = pn.GetInverseBindPoseMatrix();
                    bnTfm = bnTfm * pnInvTfm;
                }
                Matrix4x4.Decompose(bnTfm, out var scl, out var rot, out var pos);
                var eulRot = MathExtras.QuaternionToEulerRadians(rot);

                NJSObject njBone = new();
                njBone.pos = pos;
                njBone.rot = eulRot;
                njBone.scale = scl;
                njBone.unkInt = -33686019; //For some reason gc models just use this.
                njBone.variant = variant;
                njsObjects.Add(njBone);
            }

            //Link neighboring nodes
            for (int i = 0; i < aqn.nodeList.Count; i++)
            {
                var bn = aqn.nodeList[i];
                var njBone = njsObjects[i];
                njBone.childObject = njsObjects[bn.firstChild];
                njBone.siblingObject = njsObjects[bn.nextSibling];
            }

            //Generate mesh data

            //Check if we can treat this as an static weighted model
            //Static weighted models not only allow more mesh data, but are structured fairly differently
            foreach(var vtxl in aqo.vtxlList)
            {
                vtxl.AssureSumOfOneOnWeights();
                vtxl.SortBoneIndexWeightOrderByWeight();
            }
            CheckIfSingleWeightPerMesh(aqo, out bool isSingleWeightPerMeshCheck, out bool usesVertColors);
            isSingleWeightPerMesh = isSingleWeightPerMesh ? true : isSingleWeightPerMeshCheck; //Check here unless we already forced it set
            switch (variant)
            {
                case NinjaVariant.Basic:
                    throw new NotImplementedException();
                case NinjaVariant.Chunk:

                    throw new NotImplementedException();
                case NinjaVariant.Ginja:
                    ToGinjaModel(aqo, aqn, njsObjects, isSingleWeightPerMesh, usesVertColors);
                    break;
                case NinjaVariant.XJ:
                    throw new NotImplementedException();
            }

            return njsObjects[0];
        }

        private static int GetHighestWeightIndex(VTXL vtxl)
        {
            int highestWeightIndex = 0;
            float highestWeight = 0;
            for (int i = 0; i < vtxl.vertWeightIndices[0].Length; i++)
            {
                switch (i)
                {
                    case 0:
                        highestWeight = vtxl.vertWeights[0].X;
                        break;
                    case 1:
                        if (highestWeight > vtxl.vertWeights[0].Y)
                        {
                            highestWeight = vtxl.vertWeights[0].Y;
                            highestWeightIndex = i;
                        }
                        break;
                    case 2:
                        if (highestWeight > vtxl.vertWeights[0].Z)
                        {
                            highestWeight = vtxl.vertWeights[0].Z;
                            highestWeightIndex = i;
                        }
                        break;
                    case 3:
                        if (highestWeight > vtxl.vertWeights[0].W)
                        {
                            highestWeightIndex = i;
                        }
                        break;
                    default:
                        throw new Exception();
                }
            }

            return highestWeightIndex;
        }

        /// <summary>
        /// Ginja models are written differently based on if any portion of them uses weights or not.
        /// 
        /// If there are no weighted vertices, or we want to emulate this with a mesh only ever weighted to one bone
        /// then there will be one mesh for each material and opacity type for those particular bones.
        /// 
        /// If there are weighted vertices, the translations and normals of vertices are both written in portions based on
        /// their weight to each bone they are affected by. UV vert data will be written to the root NJS_Object's mesh, as well
        /// as any face data.
        /// </summary>
        private static void ToGinjaModel(AquaObject aqo, AquaNode aqn, List<NJSObject> njsObjects, bool isSingleWeightPerMesh, bool usesVertColors)
        {
            List<int> weightedMeshIndices = new List<int>();
            //If each mesh isn't just weighted to a single bone, resort faces and vertices
            //We want to avoid this if possible in order to allow potential for vertex morphs/blend targets/shape motion
            if (!isSingleWeightPerMesh)
            {
                for (int i = 0; i < aqo.meshList.Count; i++)
                {
                    var mesh = aqo.meshList[i];
                    var tris = aqo.strips[mesh.psetIndex].GetTriangles();
                    var vtxl = aqo.vtxlList[mesh.vsetIndex];
                    List<int> newVertIndices = new List<int>();
                    for (int j = 0; j > tris.Count; j++)
                    {
                        var vertIndices0 = vtxl.vertWeightIndices[(int)tris[j].X];
                        var vertIndices1 = vtxl.vertWeightIndices[(int)tris[j].Y];
                        var vertIndices2 = vtxl.vertWeightIndices[(int)tris[j].Z];
                        List<int> indices = new List<int>();
                        indices.AddRange(vertIndices0);
                        indices.AddRange(vertIndices1);
                        indices.AddRange(vertIndices2);
                        var distinctIndicesindices = indices.Distinct().ToArray();
                        foreach (var index in distinctIndicesindices)
                        {
                            if (!newVertIndices.Contains(index))
                            {
                                newVertIndices.Add(index);
                            }
                        }
                    }
                    if (newVertIndices.Count > 2)
                    {
                        weightedMeshIndices.Add(i);
                    }
                    else if (newVertIndices[0] != 0 && newVertIndices.Count > 1 && newVertIndices[1] != 0)
                    {
                        weightedMeshIndices.Add(i);
                    }
                }
            }

            NvStripifier stripifier = new NvStripifier() { StitchStrips = false, UseRestart = false };

            if (isSingleWeightPerMesh)
            {
                var posAtr = new VtxAttrFmtParameter(GCVertexAttribute.Position, true);
                var uvAtr = new VtxAttrFmtParameter(GCVertexAttribute.Tex0, true);
                VtxAttrFmtParameter nrmAtr = null;
                VtxAttrFmtParameter clrAtr = null;

                //Ninja will only use vert colors OR normals
                if (usesVertColors)
                {
                    clrAtr = new VtxAttrFmtParameter(GCVertexAttribute.Color0, true);
                } else
                {
                    nrmAtr = new VtxAttrFmtParameter(GCVertexAttribute.Normal, true);
                }

                for(int i = 0; i < aqo.meshList.Count; i++)
                {
                    var newIndexParam = new IndexAttributeParameter();
                    var vtxl = aqo.vtxlList[aqo.meshList[i].vsetIndex];
                    int boneIndex = GetHighestWeightIndex(vtxl); //This will be the node the mesh is attached to
                    var njObj = njsObjects[boneIndex];

                    int maxIndex = Math.Max(vtxl.vertPositions.Count, vtxl.uv1List.Count);
                    bool meshUsesVertColors = false;
                    if(usesVertColors && vtxl.vertColors.Count > 0)
                    {
                        meshUsesVertColors = true;
                        maxIndex = Math.Max(maxIndex, vtxl.vertColors.Count);
                        newIndexParam.IndexAttributes = GCIndexAttributeFlags.HasPosition | GCIndexAttributeFlags.HasUV | GCIndexAttributeFlags.HasColor;
                        if (maxIndex > 255)
                        {
                            newIndexParam.IndexAttributes |= GCIndexAttributeFlags.Position16BitIndex | GCIndexAttributeFlags.Color16BitIndex | GCIndexAttributeFlags.UV16BitIndex;
                        }
                    } else
                    {
                        maxIndex = Math.Max(maxIndex, vtxl.vertNormals.Count);
                        newIndexParam.IndexAttributes = GCIndexAttributeFlags.HasPosition | GCIndexAttributeFlags.HasUV | GCIndexAttributeFlags.HasNormal;
                        if (maxIndex > 255)
                        {
                            newIndexParam.IndexAttributes |= GCIndexAttributeFlags.Position16BitIndex | GCIndexAttributeFlags.Normal16BitIndex | GCIndexAttributeFlags.UV16BitIndex;
                        }
                    }
                    var texList = aqo.GetTexListNames(aqo.meshList[i].tsetIndex);
                    var texId = aqo.texFUnicodeNames.IndexOf(texList[0]);

                    GinjaAttach gjAttach;
                    if(njObj.mesh == null)
                    {
                        njObj.mesh = gjAttach = new GinjaAttach();
                    } else
                    {
                        gjAttach = (GinjaAttach)njObj.mesh;
                    }

                    //Vertex data
                    GinjaVertexData vertData;
                    int vertPosStart = 0;
                    int vertNrmStart = 0;
                    int vertUvStart = 0;
                    int vertColorStart = 0;
                    if(gjAttach.vertData == null)
                    {
                        gjAttach.vertData = vertData = new GinjaVertexData();
                    } else
                    {
                        vertData = gjAttach.vertData;
                        vertPosStart = vertData.posList.Count;
                        vertNrmStart = vertData.nrmList.Count;
                        vertUvStart = vertData.uvsArray.Length > 0 ? vertData.uvsArray[0].Count : 0;
                        vertColorStart = vertData.colorsArray.Length > 0 ? vertData.colorsArray[0].Count : 0;
                    }
                    for(int v = 0; v < vtxl.vertPositions.Count; v++)
                    {
                        vertData.posList.Add(vtxl.vertPositions[v]);
                    }

                    if (vertData.uvsArray[0] == null)
                    {
                        vertData.uvsArray[0] = new List<Vector2>();
                    }
                    for (int v = 0; v < vtxl.uv1List.Count; v++)
                    {
                        vertData.uvsArray[0].Add(vtxl.uv1List[v]);
                    }
                    if(meshUsesVertColors)
                    {
                        if (vertData.colorsArray[0] == null)
                        {
                            vertData.colorsArray[0] = new List<byte[]>();
                        }
                        for (int v = 0; v < vtxl.vertColors.Count; v++)
                        {
                            vertData.colorsArray[0].Add(vtxl.vertColors[v]);
                        }
                    } else
                    {
                        for (int v = 0; v < vtxl.vertNormals.Count; v++)
                        {
                            vertData.nrmList.Add(vtxl.vertNormals[v]);
                        }
                    }

                    //Face data
                    var tris = aqo.strips[aqo.meshList[i].psetIndex];
                    stripifier.GenerateStrips(tris.triStrips.ToArray(), out var primitiveGroups);
;
                    List<GCPrimitive> primitives = new List<GCPrimitive>();
                    foreach (PrimitiveGroup grp in primitiveGroups)
                    {
                        GCPrimitive prim = new GCPrimitive(GCPrimitiveType.TriangleStrip);
                        for (var j = 0; j < grp.Indices.Length; j++)
                        {
                            Loop vert = new Loop();
                            vert.PositionIndex = (ushort)(vertPosStart + grp.Indices[j]);
                            vert.UV0Index = (ushort)(vertUvStart + grp.Indices[j]);
                            if(meshUsesVertColors)
                            {
                                vert.NormalIndex = (ushort)(vertColorStart + grp.Indices[j]);
                            } else
                            {
                                vert.NormalIndex = (ushort)(vertNrmStart + grp.Indices[j]);
                            }
                            prim.loops.Add(vert);
                        }
                        primitives.Add(prim);
                    }

                    IndexAttributeParameter indexParam = new IndexAttributeParameter();
                    var mateAlphaType = aqo.mateList[aqo.meshList[i].mateIndex].alphaType.curString.ToLower();
                    if (mateAlphaType == "blendalpha")
                    {
                        var transparentMeshes = gjAttach.transparentFaceData = gjAttach.transparentFaceData ?? new List<GinjaMesh>(); //Make sure there's now a mesh list if there wasn't
                        TextureParameter finalTexParam = GetTexParam(texId, transparentMeshes);
                        List<GCParameter> parameters = new List<GCParameter>() { };
                        if (transparentMeshes.Count == 0)
                        {
                            parameters.Add(posAtr);
                            parameters.Add(meshUsesVertColors ? clrAtr : nrmAtr);
                        }
                        if (indexParam.IndexAttributes != newIndexParam.IndexAttributes)
                        {
                            parameters.Add(newIndexParam);
                        }
                        if (transparentMeshes.Count == 0)
                        {
                            parameters.Add(uvAtr);
                            parameters.Add(new BlendAlphaParameter() { DestAlpha = GCBlendModeControl.InverseSrcAlpha, NJDestAlpha = AlphaInstruction.InverseSourceAlpha, NJSourceAlpha = AlphaInstruction.SourceAlpha, SourceAlpha = GCBlendModeControl.SrcAlpha });
                        }
                        parameters.Add(new LightingParameter(0xC611, 1));
                        if (transparentMeshes.Count == 0)
                        {
                            parameters.Add(new AmbientColorParameter());
                        }
                        if (finalTexParam != null)
                        {
                            parameters.Add(finalTexParam);
                        }
                        if (transparentMeshes.Count == 0)
                        {
                            parameters.Add(new Unknown9Parameter());
                        }
                        parameters.Add(new TexCoordGenParameter(GCTexCoordID.TexCoord0, GCTexGenType.Matrix3x4, GCTexGenSrc.Tex0, GCTexGenMatrix.Matrix4));

                        transparentMeshes.Add(new GinjaMesh(parameters, primitives));
                    }
                    else
                    {
                        var opaqueMeshes = gjAttach.opaqueFaceData = gjAttach.opaqueFaceData ?? new List<GinjaMesh>(); //Make sure there's now a mesh list if there wasn't
                        TextureParameter finalTexParam = GetTexParam(texId, opaqueMeshes);
                        List<GCParameter> parameters = new List<GCParameter>() { };
                        if (opaqueMeshes.Count == 0)
                        {
                            parameters.Add(posAtr);
                            parameters.Add(meshUsesVertColors ? clrAtr : nrmAtr);
                        }
                        if (indexParam.IndexAttributes != newIndexParam.IndexAttributes)
                        {
                            parameters.Add(newIndexParam);
                        }
                        if (opaqueMeshes.Count == 0)
                        {
                            parameters.Add(uvAtr);
                            parameters.Add(new BlendAlphaParameter() { DestAlpha = GCBlendModeControl.InverseSrcAlpha, NJDestAlpha = AlphaInstruction.InverseSourceAlpha, NJSourceAlpha = AlphaInstruction.SourceAlpha, SourceAlpha = GCBlendModeControl.SrcAlpha });
                        }
                        parameters.Add(new LightingParameter(0xC611, 1));
                        if (opaqueMeshes.Count == 0)
                        {
                            parameters.Add(new AmbientColorParameter());
                        }
                        if (finalTexParam != null)
                        {
                            parameters.Add(finalTexParam);
                        }
                        if (opaqueMeshes.Count == 0)
                        {
                            parameters.Add(new Unknown9Parameter());
                        }
                        parameters.Add(new TexCoordGenParameter(GCTexCoordID.TexCoord0, GCTexGenType.Matrix3x4, GCTexGenSrc.Tex0, GCTexGenMatrix.Matrix4));

                        opaqueMeshes.Add(new GinjaMesh(parameters, primitives));
                    }
                }
            }
            else //If this mesh is not fully static weighted, we assign color, uv, and general mesh data to the root and then patch proportional vertex weighted data onto appropriate NJSObjects
                 //The root NJSObject is also a valid area to put weighted data.Verts share a single list.
            {
                var posAtr = new VtxAttrFmtParameter(GCVertexAttribute.Position, true);
                var uvAtr = new VtxAttrFmtParameter(GCVertexAttribute.Tex0, true);
                var nrmAtr = new VtxAttrFmtParameter(GCVertexAttribute.Normal, true);
                //Ninja does not allow vert colors in weighted meshes, probably because they use normals which conflict with vert colors in this engine

                //Gather aqo vertices into a singular, optimized vertex list
                //We're going to assume all vertex lists in the model get used
                VTXL combinedVTXL = new();
                List<int> vtxlIndexAdditions = new List<int>();
                int totalVtxlIndices = 0;
                for (int i = 0; i < aqo.vtxlList.Count; i++)
                {
                    vtxlIndexAdditions.Add(totalVtxlIndices);
                    VTXL.AppendAllVertices(aqo.vtxlList[i], combinedVTXL);
                    totalVtxlIndices += aqo.vtxlList[i].vertPositions.Count;
                }

                Dictionary<int, int> vertMapping = new Dictionary<int, int>();
                //Loop through combined vertex list, reorder vertices per node and track the mapping for the master ids
                //After, the tri indices will need to have their ids remapped based on this
                List<GinjaSkinVertexData> skinVerts = new List<GinjaSkinVertexData>();

                int currentVertCounter = 0;
                for (int b = 0; b < aqn.nodeList.Count; b++)
                {
                    var transform = aqn.nodeList[b].GetInverseBindPoseMatrixInverted();

                    GinjaSkinVertexData skinVertData = new();
                    GinjaSkinVertexDataElement staticVerts = null;
                    GinjaSkinVertexDataElement partialStartVerts = null;
                    GinjaSkinVertexDataElement partialMidVerts = null;

                    //Check first for static weights. Static weights and initial Partial weights MUST be in sequence
                    for (int i = 0; i < combinedVTXL.vertPositions.Count; i++)
                    {
                        //Check early if this is a static weighted vertex
                        if (combinedVTXL.vertWeightIndices[i][0] == b && combinedVTXL.vertWeights[i].X == 1f)
                        {
                            if (staticVerts == null)
                            {
                                staticVerts = new GinjaSkinVertexDataElement(GCSkinAttribute.StaticWeight);
                                staticVerts.startingIndex = (ushort)currentVertCounter;
                            }
                            var pos = Vector3.Transform(combinedVTXL.vertPositions[i], transform);
                            var nrm = Vector3.TransformNormal(combinedVTXL.vertNormals[i], transform);
                            staticVerts.posNrms.Add(new GinjaSkinVertexSetPosNrm()
                            {
                                posX = (short)(pos.X * 255.0),
                                posY = (short)(pos.Y * 255.0),
                                posZ = (short)(pos.Z * 255.0),
                                nrmX = (short)(nrm.X * 255.0),
                                nrmY = (short)(nrm.Y * 255.0),
                                nrmZ = (short)(nrm.Z * 255.0),
                            });
                            vertMapping.Add(i, currentVertCounter);
                            currentVertCounter++;
                        }
                    }
                    //Handle partial weights
                    for (int i = 0; i < combinedVTXL.vertPositions.Count; i++)
                    {
                        for (int w = 0; w < combinedVTXL.vertWeightIndices[i].Length; w++)
                        {
                            int wi = combinedVTXL.vertWeightIndices[i][w];
                            if (b == wi)
                            {
                                float weight = combinedVTXL.vertWeights[i].Get(w);

                                GinjaSkinVertexDataElement ele;

                                //Vert id should remain 0 if this is a starting weight
                                int vertId = 0;

                                //Decide if we handle this as a start or not
                                if (vertMapping.ContainsKey(i))
                                {
                                    vertId = currentVertCounter;
                                    if (partialMidVerts == null)
                                    {
                                        partialMidVerts = ele = new GinjaSkinVertexDataElement(GCSkinAttribute.PartialWeight);
                                    }
                                    else
                                    {
                                        ele = partialMidVerts;
                                    }
                                }
                                else
                                {
                                    vertMapping.Add(i, currentVertCounter);
                                    if (partialStartVerts == null)
                                    {
                                        partialStartVerts = ele = new GinjaSkinVertexDataElement(GCSkinAttribute.PartialWeightStart);
                                        ele.startingIndex = (ushort)currentVertCounter;
                                    }
                                    else
                                    {
                                        ele = partialStartVerts;
                                    }
                                }
                                var pos = Vector3.Transform(combinedVTXL.vertPositions[i], transform) * weight;
                                var nrm = Vector3.TransformNormal(combinedVTXL.vertNormals[i], transform) * weight;
                                ele.posNrms.Add(new GinjaSkinVertexSetPosNrm()
                                {
                                    posX = (short)(pos.X * 255.0),
                                    posY = (short)(pos.Y * 255.0),
                                    posZ = (short)(pos.Z * 255.0),
                                    nrmX = (short)(nrm.X * 255.0),
                                    nrmY = (short)(nrm.Y * 255.0),
                                    nrmZ = (short)(nrm.Z * 255.0),
                                });
                                ele.weightData.Add(new GinjaSkinVertexSetWeight()
                                {
                                    vertIndex = (short)vertId,
                                    weight = (short)(weight * 255.0)
                                });

                                if (ele.elementType == GCSkinAttribute.PartialWeightStart)
                                {
                                    currentVertCounter++;
                                }
                            }
                        }
                    }

                    //Only add this set if there's data to add
                    if (staticVerts != null)
                    {
                        skinVertData.elements.Add(staticVerts);
                    }
                    if (partialStartVerts != null)
                    {
                        skinVertData.elements.Add(partialStartVerts);
                    }
                    if (partialMidVerts != null)
                    {
                        skinVertData.elements.Add(partialMidVerts);
                    }
                    if (skinVertData.elements.Count > 0)
                    {
                        skinVerts.Add(skinVertData);
                    }
                    else
                    {
                        skinVerts.Add(null);
                    }
                }

                //All we can have on a skinned model is uv data here
                GinjaVertexData faceVertData = new GinjaVertexData();
                GinjaVertexDataElement uv = new GinjaVertexDataElement(GCVertexAttribute.Tex0);
                faceVertData.elements.Add(uv);

                var vertMapKeys = vertMapping.Keys.ToList();
                vertMapKeys.Sort();
                Vector2[] uvs = new Vector2[vertMapKeys.Count];
                foreach (var key in vertMapKeys)
                {
                    uvs[vertMapKeys[key]] = combinedVTXL.uv1List[key];
                }
                faceVertData.uvsArray = new List<Vector2>[] { uvs.ToList() };

                //Convert mesh data
                TextureParameter texAttr = null;
                IndexAttributeParameter indexParam = new IndexAttributeParameter();
                List<GinjaMesh> opaqueMeshes = new List<GinjaMesh>();
                List<GinjaMesh> transparentMeshes = new List<GinjaMesh>();
                for (int i = 0; i < aqo.meshList.Count; i++)
                {
                    var texList = aqo.GetTexListNames(aqo.meshList[i].tsetIndex);
                    var texId = aqo.texFUnicodeNames.IndexOf(texList[0]);
                    var newTexAttr = new TextureParameter((ushort)texId, GCTileMode.TileX | GCTileMode.TileY);
                    if (texAttr != newTexAttr)
                    {
                        texAttr = newTexAttr;
                    }
                    else
                    {
                        newTexAttr = null;
                    }
                    var mateAlphaType = aqo.mateList[aqo.meshList[i].mateIndex].alphaType.curString.ToLower();
                    int vertStartIndex = vtxlIndexAdditions[aqo.meshList[i].vsetIndex];
                    var tris = aqo.strips[aqo.meshList[i].psetIndex];
                    stripifier.GenerateStrips(tris.triStrips.ToArray(), out var primitiveGroups);

                    int maxIndex = 0;
                    List<GCPrimitive> primitives = new List<GCPrimitive>();
                    foreach (PrimitiveGroup grp in primitiveGroups)
                    {
                        GCPrimitive prim = new GCPrimitive(GCPrimitiveType.TriangleStrip);
                        for (var j = 0; j < grp.Indices.Length; j++)
                        {
                            Loop vert = new Loop();
                            int newIndex = vertMapping[vertStartIndex + grp.Indices[j]];
                            maxIndex = Math.Max(maxIndex, newIndex);
                            vert.PositionIndex = (ushort)newIndex;
                            vert.UV0Index = (ushort)newIndex;
                            vert.NormalIndex = (ushort)newIndex;
                            prim.loops.Add(vert);
                        }
                        primitives.Add(prim);
                    }

                    var newIndexParam = new IndexAttributeParameter();

                    newIndexParam.IndexAttributes = GCIndexAttributeFlags.HasPosition | GCIndexAttributeFlags.HasNormal | GCIndexAttributeFlags.HasUV;
                    if (maxIndex > 255)
                    {
                        newIndexParam.IndexAttributes |= GCIndexAttributeFlags.Position16BitIndex | GCIndexAttributeFlags.Normal16BitIndex | GCIndexAttributeFlags.UV16BitIndex;
                    }

                    if (mateAlphaType == "blendalpha")
                    {
                        List<GCParameter> parameters = new List<GCParameter>() { };
                        if (transparentMeshes.Count == 0)
                        {
                            parameters.Add(posAtr);
                            parameters.Add(nrmAtr);
                        }
                        if (indexParam.IndexAttributes != newIndexParam.IndexAttributes)
                        {
                            parameters.Add(newIndexParam);
                        }
                        if (transparentMeshes.Count == 0)
                        {
                            parameters.Add(uvAtr);
                            parameters.Add(new BlendAlphaParameter() { DestAlpha = GCBlendModeControl.InverseSrcAlpha, NJDestAlpha = AlphaInstruction.InverseSourceAlpha, NJSourceAlpha = AlphaInstruction.SourceAlpha, SourceAlpha = GCBlendModeControl.SrcAlpha });
                        }
                        parameters.Add(new LightingParameter(0xC611, 1));
                        if (transparentMeshes.Count == 0)
                        {
                            parameters.Add(new AmbientColorParameter());
                        }
                        if (newTexAttr != null)
                        {
                            parameters.Add(newTexAttr);
                        }
                        if (transparentMeshes.Count == 0)
                        {
                            parameters.Add(new Unknown9Parameter());
                        }
                        parameters.Add(new TexCoordGenParameter(GCTexCoordID.TexCoord0, GCTexGenType.Matrix3x4, GCTexGenSrc.Tex0, GCTexGenMatrix.Matrix4));

                        transparentMeshes.Add(new GinjaMesh(parameters, primitives));
                    }
                    else
                    {
                        List<GCParameter> parameters = new List<GCParameter>() { };
                        if (opaqueMeshes.Count == 0)
                        {
                            parameters.Add(posAtr);
                            parameters.Add(nrmAtr);
                        }
                        if (indexParam.IndexAttributes != newIndexParam.IndexAttributes)
                        {
                            parameters.Add(newIndexParam);
                        }
                        if (opaqueMeshes.Count == 0)
                        {
                            parameters.Add(uvAtr);
                            parameters.Add(new BlendAlphaParameter() { DestAlpha = GCBlendModeControl.InverseSrcAlpha, NJDestAlpha = AlphaInstruction.InverseSourceAlpha, NJSourceAlpha = AlphaInstruction.SourceAlpha, SourceAlpha = GCBlendModeControl.SrcAlpha });
                        }
                        parameters.Add(new LightingParameter(0xC611, 1));
                        if (opaqueMeshes.Count == 0)
                        {
                            parameters.Add(new AmbientColorParameter());
                        }
                        if (newTexAttr != null)
                        {
                            parameters.Add(newTexAttr);
                        }
                        if (opaqueMeshes.Count == 0)
                        {
                            parameters.Add(new Unknown9Parameter());
                        }
                        parameters.Add(new TexCoordGenParameter(GCTexCoordID.TexCoord0, GCTexGenType.Matrix3x4, GCTexGenSrc.Tex0, GCTexGenMatrix.Matrix4));

                        opaqueMeshes.Add(new GinjaMesh(parameters, primitives));
                    }
                }

                //Assign vertex and mesh data
                for (int i = 0; i < njsObjects.Count; i++)
                {
                    var njsObject = njsObjects[i];
                    if (i == 0)
                    {
                        njsObject.mesh = new GinjaAttach() { opaqueFaceData = opaqueMeshes, transparentFaceData = transparentMeshes, vertData = faceVertData };

                    }
                    if (skinVerts[i] != null)
                    {
                        if (njsObject.mesh == null)
                        {
                            njsObject.mesh = new GinjaAttach();
                        }
                        ((GinjaAttach)njsObject.mesh).skinVertData = skinVerts[i];
                    }
                }
            }
        }

        private static TextureParameter GetTexParam(int texId, List<GinjaMesh> transparentMeshes)
        {
            TextureParameter finalTexParam;
            TextureParameter oldTexParam = null;
            if (transparentMeshes.Count > 0)
            {
                for (int p = transparentMeshes.Count; p >= 0; p--)
                {
                    var bMesh = transparentMeshes[p];
                    foreach (var param in bMesh.parameters)
                    {
                        if (param is TextureParameter meshTexParam)
                        {
                            oldTexParam = meshTexParam;
                            break;
                        }
                    }
                    if (oldTexParam != null)
                    {
                        break;
                    }
                }


            }
            var newTexParam = new TextureParameter((ushort)texId, GCTileMode.TileX | GCTileMode.TileY);
            //If there isn't an existing param before this one or if the previous param is in any way different, we want to make a new one.
            //otherwise, we'll return null to signal that it should use the one from previous mesh(es)
            if (oldTexParam == null || oldTexParam.TextureID != newTexParam.TextureID || oldTexParam.Tile != newTexParam.Tile)
            {
                finalTexParam = newTexParam;
            }
            else
            {
                finalTexParam = null;
            }

            return finalTexParam;
        }

        private static bool CheckIfStaticWeighted(AquaObject aqo)
        {
            bool isStaticWeighted = true;
            for (int i = 0; i < aqo.vtxlList.Count; i++)
            {
                //Check through vert weights, if this vertex list has them
                //If we have more than one weight with a value other than 0, we assume this isn't static weighted
                //We check this way to avoid issues with floating point nonsense
                for (int j = 0; j < aqo.vtxlList[i].vertWeights.Count; j++)
                {
                    var weights = aqo.vtxlList[i].vertWeights[j];
                    int weightCheck = weights.X != 0 ? 1 : 0;
                    weightCheck = weights.Y != 0 ? weightCheck + 1 : weightCheck;
                    weightCheck = weights.Z != 0 ? weightCheck + 1 : weightCheck;
                    weightCheck = weights.W != 0 ? weightCheck + 1 : weightCheck;

                    if (weightCheck > 1)
                    {
                        isStaticWeighted = false;
                        break;
                    }
                }
                if (isStaticWeighted == false)
                {
                    break;
                }
            }

            //Break early if we know
            if(isStaticWeighted == false)
            {
                return false;
            }

            //By this point, we know there are no bones with partial weights
            //Check if we have faces with more than one bone referenced and used
            for(int i = 0; i < aqo.meshList.Count; i++)
            {
                var mesh = aqo.meshList[i];
                var tris = aqo.strips[mesh.psetIndex].GetTriangles();
                var vtxl = aqo.vtxlList[mesh.vsetIndex];
                for(int j = 0; j > tris.Count; j++)
                {
                    var vertIndices0 = vtxl.vertWeightIndices[(int)tris[j].X];
                    var vertIndices1 = vtxl.vertWeightIndices[(int)tris[j].Y];
                    var vertIndices2 = vtxl.vertWeightIndices[(int)tris[j].Z];
                    List<int> indices = new List<int>();
                    indices.AddRange(vertIndices0);
                    indices.AddRange(vertIndices1);
                    indices.AddRange(vertIndices2);
                    var distinctIndicesindices = indices.Distinct().ToArray();
                    if(distinctIndicesindices.Length > 2)
                    {
                        isStaticWeighted = false;
                        break;
                    } else if (distinctIndicesindices[0] != 0 && distinctIndicesindices.Length > 1 && distinctIndicesindices[1] != 0)
                    {
                        isStaticWeighted = false;
                        break;
                    }
                }
            }

            return isStaticWeighted;
        }

        private static void CheckIfSingleWeightPerMesh(AquaObject aqo, out bool isSingleWeightPerMesh, out bool usesVertColors)
        {
            isSingleWeightPerMesh = true;
            usesVertColors = false;

            for (int i = 0; i < aqo.meshList.Count; i++)
            {
                var mesh = aqo.meshList[i];
                var tris = aqo.strips[mesh.psetIndex].GetTriangles();
                var vtxl = aqo.vtxlList[mesh.vsetIndex];
                List<int> newVertIndices = new List<int>();
                for (int j = 0; j > tris.Count; j++)
                {
                    var vertIndices0 = vtxl.vertWeightIndices[(int)tris[j].X];
                    var vertIndices1 = vtxl.vertWeightIndices[(int)tris[j].Y];
                    var vertIndices2 = vtxl.vertWeightIndices[(int)tris[j].Z];
                    List<int> indices = new List<int>();
                    indices.AddRange(vertIndices0);
                    indices.AddRange(vertIndices1);
                    indices.AddRange(vertIndices2);
                    var distinctIndicesindices = indices.Distinct().ToArray();
                    foreach(var index in distinctIndicesindices)
                    {
                        if(!newVertIndices.Contains(index))
                        {
                            newVertIndices.Add(index);
                        }
                    }
                }
                if (newVertIndices.Count > 2)
                {
                    isSingleWeightPerMesh = false;
                    break;
                }
                else if (newVertIndices[0] != 0 && newVertIndices.Count > 1 && newVertIndices[1] != 0)
                {
                    isSingleWeightPerMesh = false;
                    break;
                }
            }
        }

        public static AquaObject ModelConvert(BufferedStreamReaderBE<MemoryStream> sr, NinjaVariant variant, out AquaNode aqn, int offset = 0, List<string> texNames = null)
        {
            var magic = sr.Peek<NJMagic>();
            switch(magic)
            {
                case NJMagic.NJBM:
                    variant = NinjaVariant.Basic;
                    offset += 8;
                    sr.Seek(8, SeekOrigin.Current);
                    break;
                case NJMagic.NJCM:
                    //Don't assign one here because for some unknown reason XJ and NJ Chunk have the same magic (thanks, sega)
                    offset += 8;
                    sr.Seek(8, SeekOrigin.Current);
                    break;
                case NJMagic.GJCM:
                    variant = NinjaVariant.Ginja;
                    offset += 8;
                    sr.Seek(8, SeekOrigin.Current);
                    break;
                case NJMagic.NJTL:
                    var njtlStart = sr.Position;
                    sr.Seek(0x4, SeekOrigin.Current);
                    var size = sr.Read<int>();
                    var njtl = new NJTextureList(sr, offset + 8);
                    texNames = njtl.texNames;
                    sr.Seek(njtlStart + size + 0x8, SeekOrigin.Begin);
                    offset += size + 0x8;

                    //Add POF0 size
                    sr.Seek(0x4, SeekOrigin.Current);
                    var pofSize = sr.Read<int>();
                    sr.Seek(pofSize, SeekOrigin.Current);
                    offset += 0x8 + pofSize;
                    return ModelConvert(sr, variant, out aqn, offset, texNames);
                default:
                    //Assume there's no 8 byte ninja header
                    break;
            }
            var leAddress = sr.Peek<int>();
            var beAddress = sr.PeekBigEndianInt32();

            var root = new NJSObject(sr, variant, leAddress > beAddress, offset);
            var model = NinjaToAqua(root, out aqn, texNames);
            return model;
        }

        private static bool CheckFaces(NJSObject nj, bool checkFaces = true)
        {
            if (nj == null)
            {
                return false;
            }
            if(checkFaces && (nj.mesh != null && (((GinjaAttach)nj.mesh).opaqueFaceData.Count > 0 || ((GinjaAttach)nj.mesh).transparentFaceData.Count > 0)))
            {
                return true;
            }

            return CheckFaces(nj.childObject) || CheckFaces(nj.siblingObject);
        }

        public static AquaObject NinjaToAqua(NJSObject NinjaModelRoot, out AquaNode aqn, List<string> texNames = null)
        {
            VTXL fullVertList = null;
            AquaObject aqo = new AquaObject();
            aqn = new AquaNode();
            int nodeCounter = 0;

            if (NinjaModelRoot.HasWeights())
            {
                fullVertList = new VTXL();
                GatherFullVertexListRecursive(NinjaModelRoot, fullVertList, ref nodeCounter, Matrix4x4.Identity, -1);
                fullVertList.ProcessToPSO2Weights();
            }

            nodeCounter = 0;
            GatherModelDataRecursive(NinjaModelRoot, fullVertList, ref nodeCounter, aqo, aqn, Matrix4x4.Identity, -1);
            aqn.ndtr.boneCount = aqn.nodeList.Count;
            aqo.objc.bonePaletteOffset = 1;

            //Assign texture names, Ninja models don't contain these
            foreach(var tempMats in aqo.tempMats)
            {
                for(int i = 0; i < tempMats.texNames.Count; i++)
                {
                    var texId = Int32.Parse(tempMats.texNames[i]);
                    if (texNames?.Count > texId)
                    {
                        tempMats.texNames[i] = texNames[texId];
                    }
                }
            }
            if(aqo.tempTris.Count == 0)
            {
                GenericTriangles genTri = new GenericTriangles();
                genTri.triList.Add(new Vector3(0, 1, 2));
                genTri.matIdList.Add(0);
                aqo.tempTris.Add(genTri);
                aqo.vtxlList.Add(new VTXL() { vertPositions = new List<Vector3>() { new Vector3(), new Vector3(), new Vector3() } });
                aqo.tempMats.Add(new GenericMaterial() { matName = "GenericMat" });
            }

            return aqo;
        }

        /// <summary>
        /// For weighted models, at some point we have to gather all of the vertices before we can apply them. 
        /// With this, we can do a preprocessing loop for later usage.
        /// </summary>
        public static void GatherFullVertexListRecursive(NJSObject njObj, VTXL fullVertList, ref int nodeId, Matrix4x4 parentMatrix, int parentId)
        {
            Matrix4x4 mat = Matrix4x4.Identity;
            mat *= Matrix4x4.CreateScale(njObj.scale);
            var rotation = Matrix4x4.CreateRotationX(njObj.rot.X) *
                Matrix4x4.CreateRotationY(njObj.rot.Y) *
                Matrix4x4.CreateRotationZ(njObj.rot.Z);
            mat *= rotation;
            mat *= Matrix4x4.CreateTranslation(njObj.pos);
            mat = mat * parentMatrix;

            njObj.GetVertexData(nodeId, fullVertList, mat);

            if(njObj.childObject != null)
            {
                nodeId++;
                GatherFullVertexListRecursive(njObj.childObject, fullVertList, ref nodeId, mat, nodeId);
            }
            if (njObj.siblingObject != null)
            {
                nodeId++;
                GatherFullVertexListRecursive(njObj.siblingObject, fullVertList, ref nodeId, parentMatrix, parentId);
            }
        }

        public static void GatherModelDataRecursive(NJSObject njObj, VTXL fullVertList, ref int nodeId, AquaObject aqo, AquaNode aqn, Matrix4x4 parentMatrix, int parentId)
        {
            aqo.bonePalette.Add((uint)nodeId);
            int currentNodeId = nodeId;
            Matrix4x4 mat = Matrix4x4.Identity;
            mat *= Matrix4x4.CreateScale(njObj.scale);
            var rotation = Matrix4x4.CreateRotationX(njObj.rot.X) *
                Matrix4x4.CreateRotationY(njObj.rot.Y) *
                Matrix4x4.CreateRotationZ(njObj.rot.Z);
            mat *= rotation;
            mat *= Matrix4x4.CreateTranslation(njObj.pos);
            mat = mat * parentMatrix;

            //Create AQN node
            NODE aqNode = new NODE();
            aqNode.boneShort1 = 0x1C0;
            aqNode.animatedFlag = 1;
            aqNode.parentId = parentId;
            aqNode.nextSibling = -1;
            aqNode.firstChild = -1;
            aqNode.unkNode = -1;
            aqNode.pos = njObj.pos;
            aqNode.eulRot = new Vector3((float)(njObj.rot.X * 180 / Math.PI), 
                (float)(njObj.rot.Y * 180 / Math.PI), (float)(njObj.rot.Z * 180 / Math.PI));
            aqNode.scale = njObj.scale;
            Matrix4x4.Invert(mat, out var invMat);
            aqNode.m1 = new Vector4(invMat.M11, invMat.M12, invMat.M13, invMat.M14);
            aqNode.m2 = new Vector4(invMat.M21, invMat.M22, invMat.M23, invMat.M24);
            aqNode.m3 = new Vector4(invMat.M31, invMat.M32, invMat.M33, invMat.M34);
            aqNode.m4 = new Vector4(invMat.M41, invMat.M42, invMat.M43, invMat.M44);
            aqNode.boneName.SetString(aqn.nodeList.Count.ToString());
            aqn.nodeList.Add(aqNode);

            VTXL tempVTXL;
            if (fullVertList == null)
            {
                tempVTXL = new VTXL();
                njObj.GetVertexData(nodeId, tempVTXL, mat);
                tempVTXL.ProcessToPSO2Weights();
            } else
            {
                tempVTXL = fullVertList;
            }

            njObj.GetFaceData(nodeId, tempVTXL, aqo);

            if (njObj.childObject != null)
            {
                aqNode.firstChild = ++nodeId;
                GatherModelDataRecursive(njObj.childObject, fullVertList, ref nodeId, aqo, aqn, mat, currentNodeId);
            }
            if (njObj.siblingObject != null)
            {
                aqNode.nextSibling = ++nodeId;
                GatherModelDataRecursive(njObj.siblingObject, fullVertList, ref nodeId, aqo, aqn, parentMatrix, parentId);
            }
            aqn.nodeList[currentNodeId] = aqNode;
        }

        public static byte[] GetGjBytes(NJSObject njsObject)
        {
            ByteListExtension.AddAsBigEndian = true;
            List<byte> outBytes = new List<byte>();
            List<int> pofSets = new List<int>();
            njsObject.Write(outBytes, pofSets, true);

            List<byte> headerMagic = new List<byte>
            {
                0x47,
                0x4A,
                0x43,
                0x4D
            };
            //This should almost always be little endian, but can be be in rare cases such as skies of arcadia
            headerMagic.AddRange(BitConverter.GetBytes(outBytes.Count));

            outBytes.InsertRange(0, headerMagic);
            outBytes.AddRange(POF0.GeneratePOF0(pofSets));

            ByteListExtension.Reset();
            return outBytes.ToArray();
        }
        public static byte[] GetNjmBytes(NJSMotion njsMotion, NJSMotion.MotionWriteMode mode)
        {
            ByteListExtension.AddAsBigEndian = true;
            List<byte> outBytes = new List<byte>();
            List<int> pofSets = new List<int>();
            njsMotion.Write(outBytes, pofSets, mode);

            List<byte> headerMagic = new List<byte>
            {
                0x4E,
                0x4D,
                0x44,
                0x4D
            };
            //This should almost always be little endian, but can be be in rare cases such as skies of arcadia
            headerMagic.AddRange(BitConverter.GetBytes(outBytes.Count));

            outBytes.InsertRange(0, headerMagic);
            outBytes.AddRange(POF0.GeneratePOF0(pofSets));

            ByteListExtension.Reset();
            return outBytes.ToArray();
        }
    }
}
