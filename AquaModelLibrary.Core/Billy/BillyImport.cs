using AquaModelLibrary.Core.General;
using AquaModelLibrary.Data.BillyHatcher;
using AquaModelLibrary.Data.BillyHatcher.LNDH;
using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Helpers.MathHelpers;
using System.Numerics;
using static AquaModelLibrary.Data.BillyHatcher.LNDConvert;

namespace AquaModelLibrary.Core.Billy
{
    public class BillyImport
    {
        public static LND ConvertToLND(string folderPath)
        {
            Dictionary<string, ModelData> tempModelData = new Dictionary<string, ModelData>();
            List<ModelData> modelData = new List<ModelData>();
            var files = Directory.GetFiles(folderPath);
            byte[] gvmBytes = null;

            foreach (var file in files)
            {
                if (Path.GetExtension(file) == ".gvm")
                {
                    gvmBytes = File.ReadAllBytes(file);
                }
                else if (Path.GetExtension(file) == ".fbx")
                {
                    var aqp = AssimpModelImporter.AssimpAquaConvertFull(file, 1, true, true, out var aqn, false, true);
                    AquaObject nightAqp = null;
                    AquaMotion aqm = null;
                    AquaNode placementAqn = null;

                    var name = Path.GetFileNameWithoutExtension(file);
                    var splitName = name.Split('+');

                    //Check our 'parameter' for special use
                    if (splitName.Length > 1)
                    {
                        switch (splitName[1].ToLower())
                        {
                            case "animation":
                                //aqm = ModelImporter.AssimpAQMConvertNoNameSingle(file, false, true, 1);
                                break;
                            case "night":
                                nightAqp = (AquaObject)aqp;
                                aqp = null;
                                break;
                            case "transform":
                                placementAqn = aqn;
                                break;
                        }
                    }

                    //Retrieve or create ModelData
                    ModelData mdl;
                    if (!tempModelData.ContainsKey(splitName[0].ToLower()))
                    {
                        mdl = new ModelData();
                        mdl.name = splitName[0];
                        tempModelData.Add(splitName[0].ToLower(), mdl);
                    }
                    else
                    {
                        mdl = tempModelData[splitName[0].ToLower()];
                    }

                    //Assign data
                    if (aqp != null)
                    {
                        mdl.aqp = (AquaObject)aqp;
                    }
                    if (placementAqn != null)
                    {
                        mdl.placementAqn = placementAqn;
                    }
                    if (aqm != null)
                    {
                        mdl.aqm = aqm;
                    }
                    if (nightAqp != null)
                    {
                        mdl.nightAqp = nightAqp;
                    }
                }
            }

            if (!tempModelData.ContainsKey("block"))
            {
                throw new Exception("Error, main terrain model, block, is missing!");
            }
            modelData.Add(tempModelData["block"]);
            foreach (var pair in tempModelData)
            {
                if (pair.Key != "block" && pair.Value.aqp != null)
                {
                    modelData.Add(pair.Value);
                }
            }
            tempModelData.Clear();

            return ConvertToLND(modelData, gvmBytes);
        }

        public static LND ConvertToLND(List<ModelData> modelData, byte[] gvmBytes)
        {
            LND lnd = new LND();
            lnd.gvm = new ArchiveLib.PuyoFile(gvmBytes);
            lnd.isArcLND = true;
            List<ModelData> animModels = new List<ModelData>();

            //Get texture lists;
            lnd.texnames.texNames = lnd.gvm.Entries.Select(e => Path.GetFileNameWithoutExtension(e.Name)).ToList();

            //Get model data
            foreach (var mdl in modelData)
            {
                if (mdl.aqm == null)
                {
                    var model = AquaToLND(lnd, mdl, false, lnd.texnames.texNames);
                    if (mdl.name.ToLower() != "block")
                    {
                        model.arcAltVertColorList.Clear();
                    }
                    lnd.arcLndModels.Add(new LND.ARCLNDStaticMeshData() { name = mdl.name, model = model });
                    lnd.fileNames.Add(mdl.name);
                }
                else
                {
                    /*
                    var animModel = new LND.ARCLNDAnimatedMeshData() { model = AquaToLND(lnd, mdl, true, lnd.texnames) };
                    //animModel.motion = GetAnimatedNightColors(mdl.nightAqp);
                    //animModel.mplMotion = GetMPLMotion(mdl.aqm);
                    lnd.arcLndAnimatedMeshDataList.Add(animModel);
                    */
                }
            }
            lnd.fileNames.Add("land");
            if (lnd.arcMPL != null)
            {
                lnd.fileNames.Add("mpl");
            }
            lnd.arcLand = new LND.ARCLNDLand();

            return lnd;
        }

        public static ARCLNDModel AquaToLND(LND lnd, ModelData mdl, bool isAnimatedModel, List<string> texNames)
        {
            //Unfortunately normals disable vert colors, but they can be auto calculated by the game.
            assignNormalsOnExport = false;
            ARCLNDModel lndMdl = new ARCLNDModel();

            //While the original format has an array of arrays of these, used to separate rendering mesh types. However this doesn't seem to matter for performance? 
            List<ARCLNDMeshData> meshData = new List<ARCLNDMeshData>();
            var vertSet = new ARCLNDVertDataSet();

            lndMdl.arcVertDataSetList.Add(vertSet);
            ushort boundCount = 1;
            List<int> badMeshIndices = new List<int>();
            for (int msh = 0; msh < mdl.aqp.meshList.Count; msh++)
            {
                var mesh = mdl.aqp.meshList[msh];
                //For reassigning vertex ids in faces after they've been combined.
                //Billy stores separate indices per vertex data type so we may as well optimize them where possible.
                Dictionary<int, int> vertIndexRemapper = new Dictionary<int, int>();
                Dictionary<int, int> normalIndexRemapper = new Dictionary<int, int>();
                Dictionary<int, int> colorIndexRemapper = new Dictionary<int, int>();
                Dictionary<int, int> uvIndexRemapper = new Dictionary<int, int>();

                var mate = mdl.aqp.mateList[mesh.mateIndex];
                var matName = mdl.aqp.matUnicodeNames[mesh.mateIndex];
                var tset = mdl.aqp.tsetList[mesh.tsetIndex];
                var strips = mdl.aqp.strips[mesh.psetIndex];
                if (StripData.RemoveDegenerateFaces(strips.triStrips.ToArray()).Count == 0)
                {
                    badMeshIndices.Add(msh);
                    continue;
                }
                strips.toStrips(strips.triStrips.ToArray());
                var vtxl = mdl.aqp.vtxlList[mesh.vsetIndex];

                //Vertices
                for (int v = 0; v < vtxl.vertPositions.Count; v++)
                {
                    var vertData = vtxl.vertPositions[v];
                    Vector3 normalData;
                    if (vtxl.vertNormals.Count > v)
                    {
                        normalData = vtxl.vertNormals[v];
                    }
                    else
                    {
                        normalData = new Vector3(0, 1, 0);
                    }
                    byte[] colorData;
                    if (vtxl.vertColors.Count > v)
                    {
                        colorData = new byte[] { vtxl.vertColors[v][2], vtxl.vertColors[v][1], vtxl.vertColors[v][0], vtxl.vertColors[v][3] };
                    }
                    else
                    {
                        colorData = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
                    }
                    short[] uvData = new short[2];
                    if (vtxl.uv1List.Count > v)
                    {
                        uvData = new short[] { (short)(vtxl.uv1List[v].X * 255), (short)(vtxl.uv1List[v].Y * 255) };
                    }

                    //Combine and remap repeated vertices as needed.
                    //Assimp splits things by material by necessity and so we need to recombine them.
                    bool foundDuplicateVert = false;
                    bool foundDuplicateNormal = false;
                    bool foundDuplicateColor = false;
                    bool foundDuplicateUv = false;
                    for (int vt = 0; vt < vertSet.PositionData.Count; vt++)
                    {
                        if (foundDuplicateVert == false && vertSet.PositionData.Count > vt && vertData == vertSet.PositionData[vt])
                        {
                            foundDuplicateVert = true;
                            vertIndexRemapper.Add(v, vt);
                            if (!optimizeVertColors)
                            {
                                foundDuplicateColor = true;
                                colorIndexRemapper.Add(v, vt);
                            }
                        }
                        if (assignNormalsOnExport && foundDuplicateNormal == false && vertSet.NormalData.Count > vt && normalData == vertSet.NormalData[vt])
                        {
                            foundDuplicateNormal = true;
                            normalIndexRemapper.Add(v, vt);
                        }
                        if (optimizeVertColors && foundDuplicateColor == false && vertSet.VertColorData.Count > vt && colorData[0] == vertSet.VertColorData[vt][0] && colorData[1] == vertSet.VertColorData[vt][1] && colorData[2] == vertSet.VertColorData[vt][2] && colorData[3] == vertSet.VertColorData[vt][3])
                        {
                            foundDuplicateColor = true;
                            colorIndexRemapper.Add(v, vt);
                        }
                        if (foundDuplicateUv == false && vertSet.UV1Data.Count > vt && uvData[0] == vertSet.UV1Data[vt][0] && uvData[1] == vertSet.UV1Data[vt][1])
                        {
                            foundDuplicateUv = true;
                            uvIndexRemapper.Add(v, vt);
                        }
                    }
                    if (!foundDuplicateVert)
                    {
                        vertIndexRemapper.Add(v, vertSet.PositionData.Count);
                        vertSet.PositionData.Add(vertData);
                    }
                    if (assignNormalsOnExport && !foundDuplicateNormal)
                    {
                        normalIndexRemapper.Add(v, vertSet.NormalData.Count);
                        vertSet.NormalData.Add(normalData);
                    }
                    if (!foundDuplicateColor)
                    {
                        colorIndexRemapper.Add(v, vertSet.VertColorData.Count);
                        vertSet.VertColorData.Add(colorData);
                    }
                    if (!foundDuplicateUv)
                    {
                        uvIndexRemapper.Add(v, vertSet.UV1Data.Count);
                        vertSet.UV1Data.Add(uvData);
                    }
                }

                //Strips
                var faceData = new ARCLNDFaceDataHead();
                faceData.flags = ArcLndVertType.Position | ArcLndVertType.VertColor | ArcLndVertType.UV1;
                if (assignNormalsOnExport)
                {
                    faceData.flags |= ArcLndVertType.Normal;
                }
                bool startNewStrip = true;
                var currentList = faceData.triIndicesList0;
                var currentStartList = faceData.triIndicesListStarts0;
                for (int f = 0; f < strips.triStrips.Count - 2; f++)
                {
                    if (strips.triStrips[f] == strips.triStrips[f + 1] || strips.triStrips[f] == strips.triStrips[f + 2]
                        || strips.triStrips[f + 1] == strips.triStrips[f + 2])
                    {
                        //Check the strip has data before adding the count.
                        if (startNewStrip == true || currentList[currentList.Count - 1].Count == 0)
                        {
                            continue;
                        }

                        //Since the strip has ended, add the count and set the flag
                        startNewStrip = true;
                        currentStartList[currentStartList.Count - 1][0].Add(currentList[currentList.Count - 1].Count);
                        continue;
                    }

                    //Start the strip, add the count on exit
                    if (startNewStrip)
                    {
                        //Swap lists if needed to preserve face normals
                        if ((f & 1) > 0)
                        {
                            currentList = faceData.triIndicesList0;
                            currentStartList = faceData.triIndicesListStarts0;
                        }
                        else
                        {
                            currentList = faceData.triIndicesList1;
                            currentStartList = faceData.triIndicesListStarts1;
                        }
                        startNewStrip = false;
                        currentStartList.Add(new List<List<int>>() { new List<int>() { 0x98 } });
                        currentList.Add(new List<List<int>>());
                        AssignFaceVertIds(faceData.flags, vertIndexRemapper, normalIndexRemapper, colorIndexRemapper, uvIndexRemapper, strips, currentList, f);
                        AssignFaceVertIds(faceData.flags, vertIndexRemapper, normalIndexRemapper, colorIndexRemapper, uvIndexRemapper, strips, currentList, f + 1);
                        AssignFaceVertIds(faceData.flags, vertIndexRemapper, normalIndexRemapper, colorIndexRemapper, uvIndexRemapper, strips, currentList, f + 2);
                    }
                    else
                    {
                        AssignFaceVertIds(faceData.flags, vertIndexRemapper, normalIndexRemapper, colorIndexRemapper, uvIndexRemapper, strips, currentList, f + 2);
                    }
                }
                //Set the final count of the facedata
                if (currentStartList[currentStartList.Count - 1][0].Count == 1)
                {
                    currentStartList[currentStartList.Count - 1][0].Add(currentList[currentList.Count - 1].Count);
                }
                lndMdl.arcFaceDataList.Add(faceData);

                //Bounding
                ARCLNDNodeBounding bnd = new ARCLNDNodeBounding();
                if (isAnimatedModel)
                {
                    var node = mdl.aqn.nodeList[mdl.placementAqn.nodeList.Count - 1];
                    Matrix4x4.Decompose(node.GetInverseBindPoseMatrixInverted(), out var scale, out var rot, out var pos);
                    bnd.Position = pos;
                    bnd.Scale = scale;
                    bnd.SetRotation(MathExtras.QuaternionToEuler(rot));
                    MathExtras.CalculateBoundingSphere(vtxl.vertPositions, out var center, out var rad);
                    bnd.center = center;
                    bnd.radius = rad;
                }
                else
                {
                    MathExtras.CalculateBoundingSphere(vtxl.vertPositions, out var center, out var rad);
                    bnd.center = center;
                    bnd.radius = rad;
                }
                if (boundCount != mdl.aqp.meshList.Count)
                {
                    bnd.index = boundCount;
                }
                boundCount++;
                lndMdl.arcBoundingList.Add(bnd);

                //Material
                ARCLNDMaterialEntry matEntry = new ARCLNDMaterialEntry();
                var texName = Path.GetFileNameWithoutExtension(mdl.aqp.texfList[tset.tstaTexIDs[0]].texName.GetString());
                matEntry.TextureId = texNames.IndexOf(texName);
                if (matEntry.TextureId == -1)
                {
                    matEntry.TextureId = 0;
                }
                ARCLNDMaterialEntryRef matRef = new ARCLNDMaterialEntryRef();
                matRef.extraDataEnabled = 1;

                var flagsSplit = matName.Split('#');
                matEntry.RenderFlags = 0;
                matEntry.textureFlags = 0;
                if (flagsSplit.Length > 1)
                {
                    for (int f = 1; f < flagsSplit.Length; f++)
                    {
                        switch (flagsSplit[f].ToLower())
                        {
                            case "s_zero":
                                matEntry.sourceAlpha = AlphaInstruction.Zero;
                                break;
                            case "s_one":
                                matEntry.sourceAlpha = AlphaInstruction.One;
                                break;
                            case "s_othercolor":
                                matEntry.sourceAlpha = AlphaInstruction.OtherColor;
                                break;
                            case "s_inverseothercolor":
                                matEntry.sourceAlpha = AlphaInstruction.InverseOtherColor;
                                break;
                            case "s_sourcealpha":
                                matEntry.sourceAlpha = AlphaInstruction.SourceAlpha;
                                break;
                            case "s_inversesourcealpha":
                                matEntry.sourceAlpha = AlphaInstruction.InverseSourceAlpha;
                                break;
                            case "s_destinationalpha":
                                matEntry.sourceAlpha = AlphaInstruction.DestinationAlpha;
                                break;
                            case "s_inversedestinationalpha":
                                matEntry.sourceAlpha = AlphaInstruction.InverseDestinationAlpha;
                                break;
                            case "d_zero":
                                matEntry.destinationAlpha = AlphaInstruction.Zero;
                                break;
                            case "d_one":
                                matEntry.destinationAlpha = AlphaInstruction.One;
                                break;
                            case "d_othercolor":
                                matEntry.destinationAlpha = AlphaInstruction.OtherColor;
                                break;
                            case "d_inverseothercolor":
                                matEntry.destinationAlpha = AlphaInstruction.InverseOtherColor;
                                break;
                            case "d_sourcealpha":
                                matEntry.destinationAlpha = AlphaInstruction.SourceAlpha;
                                break;
                            case "d_inversesourcealpha":
                                matEntry.destinationAlpha = AlphaInstruction.InverseSourceAlpha;
                                break;
                            case "d_destinationalpha":
                                matEntry.destinationAlpha = AlphaInstruction.DestinationAlpha;
                                break;
                            case "d_inversedestinationalpha":
                                matEntry.destinationAlpha = AlphaInstruction.InverseDestinationAlpha;
                                break;
                            case "enablelighting":
                                matEntry.RenderFlags |= ARCLNDRenderFlags.EnableLighting;
                                break;
                            case "rfunknown0x2":
                                matEntry.RenderFlags |= ARCLNDRenderFlags.RFUnknown0x2;
                                break;
                            case "twosided":
                                matEntry.RenderFlags |= ARCLNDRenderFlags.TwoSided;
                                break;
                            case "rfunknown0x8":
                                matEntry.RenderFlags |= ARCLNDRenderFlags.RFUnknown0x8;
                                break;
                            case "renderorderthing":
                                matEntry.RenderFlags |= ARCLNDRenderFlags.renderOrderThing;
                                break;
                            case "renderorderthing2":
                                matEntry.RenderFlags |= ARCLNDRenderFlags.renderOrderThing2;
                                break;
                            case "rfunknown0x40":
                                matEntry.RenderFlags |= ARCLNDRenderFlags.RFUnknown0x40;
                                break;
                            case "rfunknown0x80":
                                matEntry.RenderFlags |= ARCLNDRenderFlags.RFUnknown0x80;
                                break;
                            case "tfunknownx0x1":
                                matEntry.textureFlags |= ARCLNDTextureFlags.TFUnknownX0x1;
                                break;
                            case "tilex":
                                matEntry.textureFlags |= ARCLNDTextureFlags.TileX;
                                break;
                            case "mirroredtilex":
                                matEntry.textureFlags |= ARCLNDTextureFlags.MirroredTileX;
                                break;
                            case "tfunknowny0x8":
                                matEntry.textureFlags |= ARCLNDTextureFlags.TFUnknownY0x8;
                                break;
                            case "tiley":
                                matEntry.textureFlags |= ARCLNDTextureFlags.TileY;
                                break;
                            case "mirroredtiley":
                                matEntry.textureFlags |= ARCLNDTextureFlags.MirroredTileY;
                                break;
                            case "tfunknown0x40":
                                matEntry.textureFlags |= ARCLNDTextureFlags.TFUnknown0x40;
                                break;
                            case "tfunknown0x80":
                                matEntry.textureFlags |= ARCLNDTextureFlags.TFUnknown0x80;
                                break;
                        }
                    }
                }
                //Default if nothing
                if (matEntry.RenderFlags == 0)
                {
                    matEntry.RenderFlags = (ARCLNDRenderFlags)0x3;
                }
                if (matEntry.textureFlags == 0)
                {
                    matEntry.textureFlags = ARCLNDTextureFlags.TileX | ARCLNDTextureFlags.TileY;
                }

                //Optimize out same materials
                int matId = -1;
                for (int i = 0; i < lndMdl.arcMatEntryList.Count; i++)
                {
                    var entry = lndMdl.arcMatEntryList[i].entry;
                    if (entry.unkBool == matEntry.unkBool && entry.unkFlags1 == matEntry.unkFlags1 && entry.textureFlags == matEntry.textureFlags && entry.RenderFlags == matEntry.RenderFlags && entry.destinationAlpha == matEntry.destinationAlpha
                        && entry.diffuseColor == matEntry.diffuseColor && entry.sourceAlpha == matEntry.sourceAlpha && entry.specularColor == matEntry.specularColor && entry.TextureId == matEntry.TextureId && entry.unkInt6 == matEntry.unkInt6
                        && entry.ushort0 == matEntry.ushort0)
                    {
                        matId = i;
                        break;
                    }
                }
                if (matId == -1)
                {
                    matRef.entry = matEntry;
                    matId = lndMdl.arcMatEntryList.Count;
                    lndMdl.arcMatEntryList.Add(matRef);
                }

                //Mesh
                ARCLNDMeshData arcMesh = new ARCLNDMeshData();
                arcMesh.matEntryId = matId;
                arcMesh.BoundingDataId = lndMdl.arcBoundingList.Count - 1;
                arcMesh.faceDataId = lndMdl.arcFaceDataList.Count - 1;
                meshData.Add(arcMesh);
            }
            lndMdl.arcMeshDataList.Add(meshData);
            lndMdl.arcMeshDataRefList.Add(new ARCLNDMeshDataRef() { count = meshData.Count, unkEnum = 1 });

            //If we're optimizing vert colors, we need to dummy out the alt vert color set since we can no longer make a normal night vert color set 
            lndMdl.arcAltVertColorList = new List<ARCLNDVertDataSet>() { new ARCLNDVertDataSet() };
            if (optimizeVertColors || mdl.nightAqp == null)
            {
                lndMdl.arcAltVertColorList[0].VertColor = lndMdl.arcVertDataSetList[0].VertColor;
            }
            else
            {
                //Recycle here, it's already been assigned and we don't really want to add more to the old one
                vertSet = new ARCLNDVertDataSet();
                for (int msh = 0; msh < mdl.nightAqp.meshList.Count; msh++)
                {
                    if (badMeshIndices.Contains(msh))
                    {
                        continue;
                    }
                    var mesh = mdl.aqp.meshList[msh];
                    //For reassigning vertex ids in faces after they've been combined.
                    //Billy stores separate indices per vertex data type so we may as well optimize them where possible.
                    Dictionary<int, int> vertIndexRemapper = new Dictionary<int, int>();
                    Dictionary<int, int> colorIndexRemapper = new Dictionary<int, int>();
                    var vtxl = mdl.nightAqp.vtxlList[mesh.vsetIndex];

                    //Vertices
                    List<Vector3> posList = new List<Vector3>();
                    for (int v = 0; v < vtxl.vertPositions.Count; v++)
                    {
                        var vertData = vtxl.vertPositions[v];
                        byte[] colorData;

                        if (vtxl.vertColors.Count > v)
                        {
                            colorData = new byte[] { vtxl.vertColors[v][2], vtxl.vertColors[v][1], vtxl.vertColors[v][0], vtxl.vertColors[v][3] };
                        }
                        else
                        {
                            colorData = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF };
                        }

                        //Combine and remap repeated vertices as needed.
                        //Assimp splits things by material by necessity and so we need to recombine them.
                        bool foundDuplicateVert = false;
                        bool foundDuplicateColor = false;
                        for (int vt = 0; vt < vertSet.PositionData.Count; vt++)
                        {
                            if (foundDuplicateVert == false && vertSet.PositionData.Count > vt && vertData == vertSet.PositionData[vt])
                            {
                                foundDuplicateVert = true;
                                vertIndexRemapper.Add(v, vt);
                                if (!optimizeVertColors)
                                {
                                    foundDuplicateColor = true;
                                    colorIndexRemapper.Add(v, vt);
                                }
                            }
                            if (foundDuplicateColor == false && vertSet.VertColorData.Count > vt && colorData[0] == vertSet.VertColorData[vt][0] && colorData[1] == vertSet.VertColorData[vt][1] && colorData[2] == vertSet.VertColorData[vt][2] && colorData[3] == vertSet.VertColorData[vt][3])
                            {
                                foundDuplicateColor = true;
                                colorIndexRemapper.Add(v, vt);
                            }
                        }
                        if (!foundDuplicateVert)
                        {
                            vertIndexRemapper.Add(v, vertSet.PositionData.Count);
                            vertSet.PositionData.Add(vertData);
                            posList.Add(vertData);
                        }
                        if (!foundDuplicateColor)
                        {
                            colorIndexRemapper.Add(v, vertSet.VertColorData.Count);
                            lndMdl.arcAltVertColorList[0].VertColorData.Add(colorData);
                        }
                    }

                }
            }

            return lndMdl;
        }

        private static void AssignFaceVertIds(ArcLndVertType flags, Dictionary<int, int> vertIndexRemapper, Dictionary<int, int> normalIndexRemapper, Dictionary<int, int> colorIndexRemapper, Dictionary<int, int> uvIndexRemapper, StripData strips, List<List<List<int>>> currentList, int f)
        {
            var vert = new List<int>();
            if ((flags & ArcLndVertType.Position) > 0)
            {
                vert.Add(vertIndexRemapper[strips.triStrips[f]]);
            }
            if ((flags & ArcLndVertType.Normal) > 0)
            {
                vert.Add(normalIndexRemapper[strips.triStrips[f]]);
            }
            if ((flags & ArcLndVertType.VertColor) > 0)
            {
                vert.Add(colorIndexRemapper[strips.triStrips[f]]);
            }
            if ((flags & ArcLndVertType.UV1) > 0)
            {
                vert.Add(uvIndexRemapper[strips.triStrips[f]]);
            }

            currentList[currentList.Count - 1].Add(vert);
        }


        /// <summary>
        /// Facecount limited by shorts or ushorts (needs testing).
        /// </summary>
        public static MC2 ConvertToMC2(string initialFilePath)
        {
            MC2 mc2 = new MC2();
            var scene = AssimpModelImporter.GetAssimpScene(initialFilePath, AssimpModelImporter.GetPostProcessSteps(preTransformVertices: true));
            var baseScale = AssimpModelImporter.SetAssimpScale(scene);

            Vector3 rootBoxMinExtents = new Vector3((float)(scene.Meshes[0].Vertices[0].X * baseScale), (float)(scene.Meshes[0].Vertices[0].Y * baseScale), (float)(scene.Meshes[0].Vertices[0].Z * baseScale));
            Vector3 rootBoxMaxExtents = rootBoxMinExtents;
            for (int i = 0; i < scene.MeshCount; i++)
            {
                var mesh = scene.Meshes[i];
                Dictionary<int, int> vertIndexRemapper = new Dictionary<int, int>(); //For reassigning vertex ids in faces after they've been combined.

                for (int v = 0; v < mesh.VertexCount; v++)
                {
                    var vert = mesh.Vertices[v];
                    vert.X = (float)(vert.X * baseScale);
                    vert.Y = (float)(vert.Y * baseScale);
                    vert.Z = (float)(vert.Z * baseScale);

                    //Min extents
                    if (rootBoxMinExtents.X > vert.X)
                    {
                        rootBoxMinExtents.X = vert.X;
                    }
                    if (rootBoxMinExtents.Y > vert.Y)
                    {
                        rootBoxMinExtents.Y = vert.Y;
                    }
                    if (rootBoxMinExtents.Z > vert.Z)
                    {
                        rootBoxMinExtents.Z = vert.Z;
                    }

                    //Max extents
                    if (rootBoxMaxExtents.X < vert.X)
                    {
                        rootBoxMaxExtents.X = vert.X;
                    }
                    if (rootBoxMaxExtents.Y < vert.Y)
                    {
                        rootBoxMaxExtents.Y = vert.Y;
                    }
                    if (rootBoxMaxExtents.Z < vert.Z)
                    {
                        rootBoxMaxExtents.Z = vert.Z;
                    }

                    //Combine and remap repeated vertices as needed.
                    //Assimp splits things by material by necessity and so we need to recombine them.
                    var vertData = new Vector3(vert.X, vert.Y, vert.Z);
                    bool foundDuplicateVert = false;
                    for (int vt = 0; vt < mc2.vertPositions.Count; vt++)
                    {
                        if (vertData == mc2.vertPositions[vt])
                        {
                            foundDuplicateVert = true;
                            vertIndexRemapper.Add(v, vt);
                            break;
                        }
                    }
                    if (!foundDuplicateVert)
                    {
                        vertIndexRemapper.Add(v, mc2.vertPositions.Count);
                        mc2.vertPositions.Add(vertData);
                    }
                }

                var flagsSplit = scene.Materials[mesh.MaterialIndex].Name.Split('#');
                MC2.FlagSet0 flags0 = new MC2.FlagSet0();
                MC2.FlagSet1 flags1 = new MC2.FlagSet1();
                MC2.FlagSet2 flags2 = new MC2.FlagSet2();

                if (flagsSplit.Length > 1)
                {
                    for (int f = 1; f < flagsSplit.Length; f++)
                    {
                        switch (flagsSplit[f].ToLower())
                        {
                            case "defaultground":
                                flags1 |= MC2.FlagSet1.DefaultGround;
                                break;
                            case "unk1_0x2":
                                flags1 |= MC2.FlagSet1.Unk1_0x2;
                                break;
                            case "drown":
                                flags1 |= MC2.FlagSet1.Drown;
                                break;
                            case "quicksand":
                                flags1 |= MC2.FlagSet1.Quicksand;
                                break;
                            case "unk1_0x10":
                                flags1 |= MC2.FlagSet1.Unk1_0x10;
                                break;
                            case "unk1_0x20":
                                flags1 |= MC2.FlagSet1.Unk1_0x20;
                                break;
                            case "unk1_0x40":
                                flags1 |= MC2.FlagSet1.Unk1_0x40;
                                break;
                            case "snow":
                                flags1 |= MC2.FlagSet1.Snow;
                                break;
                            case "lava":
                                flags0 |= MC2.FlagSet0.Lava;
                                break;
                            case "unk0_0x2":
                                flags0 |= MC2.FlagSet0.Unk0_0x2;
                                break;
                            case "slide":
                                flags0 |= MC2.FlagSet0.Slide;
                                break;
                            case "unk0_0x8":
                                flags0 |= MC2.FlagSet0.Unk0_0x8;
                                break;
                            case "unk0_0x10":
                                flags0 |= MC2.FlagSet0.Unk0_0x10;
                                break;
                            case "nobillyandeggcollision":
                                flags0 |= MC2.FlagSet0.NoBillyAndEggCollision;
                                break;
                            case "unk0_0x40":
                                flags0 |= MC2.FlagSet0.Unk0_0x40;
                                break;
                            case "death":
                                flags0 |= MC2.FlagSet0.Death;
                                break;
                            case "unk2_0x1":
                                flags2 |= MC2.FlagSet2.Unk2_0x1;
                                break;
                            case "unk2_0x2":
                                flags2 |= MC2.FlagSet2.Unk2_0x2;
                                break;
                            case "unk2_0x4":
                                flags2 |= MC2.FlagSet2.Unk2_0x4;
                                break;
                            case "unk2_0x8":
                                flags2 |= MC2.FlagSet2.Unk2_0x8;
                                break;
                            case "unk2_0x10":
                                flags2 |= MC2.FlagSet2.Unk2_0x10;
                                break;
                            case "unk2_0x20":
                                flags2 |= MC2.FlagSet2.Unk2_0x20;
                                break;
                            case "unk2_0x40":
                                flags2 |= MC2.FlagSet2.Unk2_0x40;
                                break;
                            case "unk2_0x80":
                                flags2 |= MC2.FlagSet2.Unk2_0x80;
                                break;
                        }
                    }
                }

                //Default ground if no flags
                if (flags0 == 0 && flags1 == 0 && flags2 == 0)
                {
                    flags1 = MC2.FlagSet1.DefaultGround;
                }

                for (int f = 0; f < mesh.FaceCount; f++)
                {
                    var face = mesh.Faces[f];
                    var tri = new MC2.MC2FaceData();
                    tri.flagSet0 = flags0;
                    tri.flagSet1 = flags1;
                    tri.flagSet2 = flags2;

                    //Ensure we remap vert indices to their new, combined ids as needed
                    tri.vert0 = (ushort)(vertIndexRemapper.ContainsKey(face.Indices[0]) ? vertIndexRemapper[face.Indices[0]] : face.Indices[0]);
                    tri.vert1 = (ushort)(vertIndexRemapper.ContainsKey(face.Indices[1]) ? vertIndexRemapper[face.Indices[1]] : face.Indices[1]);
                    tri.vert2 = (ushort)(vertIndexRemapper.ContainsKey(face.Indices[2]) ? vertIndexRemapper[face.Indices[2]] : face.Indices[2]);
                    tri.vert0Value = mc2.vertPositions[tri.vert0];
                    tri.vert1Value = mc2.vertPositions[tri.vert1];
                    tri.vert2Value = mc2.vertPositions[tri.vert2];

                    //Calculate face normal
                    Vector3 u = mc2.vertPositions[tri.vert1] - mc2.vertPositions[tri.vert0];
                    Vector3 v = mc2.vertPositions[tri.vert2] - mc2.vertPositions[tri.vert0];
                    tri.faceNormal = Vector3.Normalize(Vector3.Cross(u, v));

                    mc2.faceData.Add(tri);
                }
            }

            List<ushort> faceIndices = new List<ushort>();
            for (int i = 0; i < mc2.faceData.Count; i++)
            {
                faceIndices.Add((ushort)i);
            }

            mc2.PopulateFaceBounds();
            mc2.rootSector = mc2.SubdivideSector(new Vector2(rootBoxMinExtents.X, rootBoxMaxExtents.X), new Vector2(rootBoxMinExtents.Z, rootBoxMaxExtents.Z), faceIndices, 0);
            mc2.header.maxDepth = (ushort)MC2.maxDepth;
            mc2.header.ushort3 = 0x14;

            return mc2;
        }
    }
}
