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
            aqms = NinjaMotionConvert.NJMToAqm(motions);
        }

        public static NJSObject ConvertToNinja(AquaObject aqo, AquaNode aqn, NinjaVariant variant, out NJTextureList njTL)
        {
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
            bool isStaticWeighted = CheckIfStaticWeighted(aqo);

            switch (variant)
            {
                case NinjaVariant.Basic:
                    throw new NotImplementedException();
                case NinjaVariant.Chunk:

                    throw new NotImplementedException();
                case NinjaVariant.Ginja:
                    NvStripifier stripifier = new NvStripifier() { StitchStrips = false, UseRestart = false };
                    //If this model is static weighted, we assign full mesh data per NJSObject it's attached to and do not use any weighted data. Static weighted models are also allowed to have
                    //vertex colors or normals
                    //For this case we won't be rebuilding with vertex morphs/blend targets/shape motion in mind.
                    //Faces with more than one bone referenced in their vertices will be ignored since there's no way to support that
                    if (isStaticWeighted)
                    {

                    } else //If this model is not fully static weighted, we assign color, uv, and general mesh data to the root and then patch proportional vertex weighted data onto appropriate NJSObjects
                           //The root NJSObject is also a valid area to put weighted data.Verts share a single list.
                    {
                        var posAtr = new VtxAttrFmtParameter(GCVertexAttribute.Position, true);
                        var nrmAtr = new VtxAttrFmtParameter(GCVertexAttribute.Normal, true);
                        var uvAtr = new VtxAttrFmtParameter(GCVertexAttribute.Tex0, true);

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
                        for(int b = 0; b < aqn.nodeList.Count; b++)
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
                                    if(staticVerts == null)
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
                                for(int w = 0; w < combinedVTXL.vertWeightIndices[i].Length; w++)
                                {
                                    int wi = combinedVTXL.vertWeightIndices[i][w];
                                    if(b == wi)
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
                                            } else
                                            {
                                                ele = partialMidVerts;
                                            }
                                        } else
                                        {
                                            vertMapping.Add(i, currentVertCounter);
                                            if (partialStartVerts == null)
                                            {
                                                partialStartVerts = ele = new GinjaSkinVertexDataElement(GCSkinAttribute.PartialWeightStart);
                                                ele.startingIndex = (ushort)currentVertCounter;
                                            } else
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
                            if(skinVertData.elements.Count > 0)
                            {
                                skinVerts.Add(skinVertData);
                            } else
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
                        foreach(var key in vertMapKeys)
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
                            foreach (NvTriStripDotNet.PrimitiveGroup grp in primitiveGroups)
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
                            

                            if(mateAlphaType == "blendalpha")
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
                            } else
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
                        for(int i = 0; i < njsObjects.Count; i++)
                        {
                            var njsObject = njsObjects[i];
                            if (i == 0)
                            {
                                njsObject.mesh = new GinjaAttach() { opaqueFaceData = opaqueMeshes, transparentFaceData = transparentMeshes, vertData = faceVertData };
                               
                            }
                            if (skinVerts[i] != null)
                            {
                                if(njsObject.mesh == null)
                                {
                                    njsObject.mesh = new GinjaAttach();
                                }
                                ((GinjaAttach)njsObject.mesh).skinVertData = skinVerts[i];
                            }
                        }
                    }
                    break;
                case NinjaVariant.XJ:
                    throw new NotImplementedException();
            }

            return njsObjects[0];
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

                    if (vertIndices0[0] == vertIndices0[1] || vertIndices0[1] == vertIndices0[2] || vertIndices0[0] == vertIndices0[2])
                    {
                        isStaticWeighted = false;
                        break;
                    }
                }
            }

            return isStaticWeighted;
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
