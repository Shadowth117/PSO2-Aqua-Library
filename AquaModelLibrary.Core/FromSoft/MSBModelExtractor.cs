using AquaModelLibrary.Data.FromSoft;
using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Helpers.MathHelpers;
using AquaModelLibrary.Core.General;
using SoulsFormats;
using System.Numerics;
using static AquaModelLibrary.Core.FromSoft.SoulsConvert;

namespace AquaModelLibrary.Core.FromSoft
{
    public class MSBModelExtractor
    {
        public static void ExtractMSBMapModels(string filePath, bool useMetaData = false)
        {
            ExtractMSBMapModels(Path.GetDirectoryName(filePath), filePath);
        }

        public static void ExtractMSBMapModels(string outDir, string filePath, bool useMetaData = false)
        {
            ExtractMSBMapModels(outDir, filePath, File.ReadAllBytes(filePath), false);
        }
        public static void ExtractMSBMapModels(string outDir, string filePath, byte[] file, bool useMetaData = false)
        {
            mirrorMat = mirrorMatZ;
            byte[] newFile = file;
            string msbMapId = Path.GetFileNameWithoutExtension(filePath);

            if (SoulsFormats.DCX.Is(file))
            {
                newFile = DCX.Decompress(file);
                msbMapId = Path.GetFileNameWithoutExtension(msbMapId);
            }

            IBinder files = null;
            if (SoulsFile<SoulsFormats.BND3>.Is(newFile))
            {
                files = SoulsFile<SoulsFormats.BND3>.Read(newFile);
            }
            else if (SoulsFile<SoulsFormats.BND4>.Is(newFile))
            {
                files = SoulsFile<SoulsFormats.BND4>.Read(newFile);
            }

            if (files == null)
            {
                files = new BND3();
                files.Files.Add(new BinderFile() { Bytes = newFile, Name = null });
            }
            else
            {
                outDir = Path.Combine(outDir, Path.GetFileName(filePath) + "_");
            }

            foreach (var bndFile in files.Files)
            {
                var name = bndFile.Name ?? filePath;
                var fileName = Path.GetFileNameWithoutExtension(name);
                string extension = bndFile.Name != null ? Path.GetExtension(bndFile.Name) : Path.GetExtension(filePath);
                string outPathDirectory = Path.Combine(outDir, fileName);
                string outPath = Path.Combine(outPathDirectory, $"{fileName}_map.fbx");
                Directory.CreateDirectory(outPathDirectory);
                if (extractUnreferencedMapData)
                {
                    Directory.CreateDirectory(Path.Combine(outPathDirectory, "Unreferenced"));
                }

                List<AquaObject> aqpList = new List<AquaObject>();
                List<AquaNode> aqnList = new List<AquaNode>();
                List<string> modelNames = new List<string>();
                List<AquaObject> unrefAqpList = new List<AquaObject>();
                List<AquaNode> unrefAqnList = new List<AquaNode>();
                List<string> unrefModelNames = new List<string>();
                List<string> texNames = new List<string>();
                List<string> unrefTexNames = new List<string>();
                List<List<Matrix4x4>> instanceTransformList = new List<List<Matrix4x4>>();
                IMsb msb = null;
                Dictionary<string, List<Matrix4x4>> objectTransformsDict = new Dictionary<string, List<Matrix4x4>>();
                string rootPath = "";
                string worldString = msbMapId.Split('_')[0];
                string modelPath = "";
                string texPath = "";
                string[] modelFiles;
                string[] tpfFiles;
                string[] texBhds;

                switch (game)
                {
                    case SoulsGame.DemonsSouls:
                        msb = SoulsFormats.SoulsFile<SoulsFormats.MSBD>.Read(bndFile.Bytes);
                        foreach (var p in msb.Parts.GetEntries())
                        {
                            if (p is SoulsFormats.MSBD.Part.MapPiece)
                            {
                                AddToTFMDict(objectTransformsDict, p, msbMapId);
                            }
                        }

                        rootPath = Path.GetDirectoryName(Path.GetDirectoryName(filePath));
                        modelPath = Path.Combine(rootPath, $"{fileName}\\");
                        texPath = Path.Combine(rootPath, $"{worldString}\\");

                        modelFiles = Directory.GetFiles(modelPath, "*.flver.dcx");
                        tpfFiles = Directory.GetFiles(texPath, "*.tpf.dcx");

                        foreach (var modelFile in modelFiles)
                        {
                            var id = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(modelFile)).ToLower();
                            if (objectTransformsDict.ContainsKey(id) || extractUnreferencedMapData)
                            {
                                GatherModel(useMetaData, aqpList, aqnList, modelNames, instanceTransformList, objectTransformsDict, texNames, unrefAqpList, unrefAqnList, unrefModelNames, unrefTexNames, Path.GetExtension(modelFile), Path.GetFileNameWithoutExtension(modelFile).ToLower(), true, File.ReadAllBytes(modelFile));
                            }
                        }
                        foreach (var texFile in tpfFiles)
                        {
                            GatherTexturesFromTPF(texNames, outPathDirectory, Path.GetExtension(texFile), Path.GetFileNameWithoutExtension(texFile), File.ReadAllBytes(texFile));
                        }
                        break;
                    case SoulsGame.DarkSouls1:
                    case SoulsGame.DarkSouls1Remastered:
                        msb = SoulsFormats.SoulsFile<SoulsFormats.MSB1>.Read(bndFile.Bytes);
                        foreach (var p in msb.Parts.GetEntries())
                        {
                            if (p is SoulsFormats.MSB1.Part.MapPiece)
                            {
                                AddToTFMDict(objectTransformsDict, p, msbMapId);
                            }
                        }

                        rootPath = Path.GetDirectoryName(Path.GetDirectoryName(filePath));
                        modelPath = Path.Combine(rootPath, $"{fileName}\\");
                        texPath = Path.Combine(rootPath, $"{worldString}\\");
                        var txPath = Path.Combine(rootPath, $"tx\\");

                        modelFiles = Directory.GetFiles(modelPath, "*.flver*");
                        var texFilePaths = Directory.GetFiles(texPath, "*.tpf*").ToList();
                        if (game == SoulsGame.DarkSouls1)
                        {
                            texFilePaths.AddRange(Directory.GetFiles(txPath, "*.tpf*"));
                        }

                        foreach (var modelFile in modelFiles)
                        {
                            var id = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(modelFile)).ToLower();
                            var foundKey = objectTransformsDict.ContainsKey(id);
                            if (foundKey || extractUnreferencedMapData)
                            {
                                GatherModel(useMetaData, aqpList, aqnList, modelNames, instanceTransformList, objectTransformsDict, texNames, unrefAqpList, unrefAqnList, unrefModelNames, unrefTexNames, Path.GetExtension(modelFile), Path.GetFileNameWithoutExtension(modelFile).ToLower(), foundKey, File.ReadAllBytes(modelFile));
                            }
                        }
                        if (game == SoulsGame.DarkSouls1Remastered)
                        {
                            foreach (var texFile in texFilePaths)
                            {
                                if (texFile.Contains("bhd"))
                                {
                                    using (var bxf = new BXF3Reader(texFile, texFile.Replace("bhd", "bdt")))
                                    {
                                        GatherTexturesFromBinder(bxf, texNames, outPathDirectory);
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var texFile in texFilePaths)
                            {
                                var id = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(texFile)).ToLower();
                                //Purposely no extractUnreferencedMapData check. We do NOT want every map texture in Dark Souls 1 dumped here... 
                                if (texNames.Contains(id))
                                {
                                    GatherTexturesFromTPF(texNames, outPathDirectory, Path.GetExtension(texFile), Path.GetFileNameWithoutExtension(texFile), File.ReadAllBytes(texFile));
                                }
                                else if (unrefTexNames.Contains(id))
                                {
                                    GatherTexturesFromTPF(texNames, outPathDirectory, Path.GetExtension(texFile), Path.GetFileNameWithoutExtension(texFile), File.ReadAllBytes(texFile));
                                }
                            }
                        }
                        break;
                    case SoulsGame.DarkSouls2:
                        msb = SoulsFormats.SoulsFile<SoulsFormats.MSB2>.Read(bndFile.Bytes);
                        foreach (var p in msb.Parts.GetEntries())
                        {
                            if (p is SoulsFormats.MSB2.Part.MapPiece)
                            {
                                AddToTFMDict(objectTransformsDict, p, msbMapId);
                            }
                        }

                        rootPath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(filePath)));
                        var modelBhdPath = Path.Combine(rootPath, $"model/map/{fileName}.mapbhd");
                        var modelBdtPath = Path.Combine(rootPath, $"model/map/{fileName}.mapbdt");

                        var fileNameTex = fileName.Replace('m', 't');
                        var texBhdPath = Path.Combine(rootPath, $"model/map/{fileNameTex}.tpfbhd");
                        var texBdtPath = Path.Combine(rootPath, $"model/map/{fileNameTex}.tpfbdt");
                        using (var bxf = new BXF4Reader(modelBhdPath, modelBdtPath))
                        {
                            GatherModelsFromBinder(useMetaData, aqpList, aqnList, modelNames, instanceTransformList, objectTransformsDict, bxf, texNames, unrefAqpList, unrefAqnList, unrefModelNames, unrefTexNames);
                        }
                        using (var bxf = new BXF4Reader(texBhdPath, texBdtPath))
                        {
                            GatherTexturesFromBinder(bxf, texNames, outPathDirectory);
                        }
                        break;
                    case SoulsGame.Bloodborne:
                        msb = SoulsFormats.SoulsFile<SoulsFormats.MSBB>.Read(bndFile.Bytes);
                        foreach (var p in msb.Parts.GetEntries())
                        {
                            if (p is SoulsFormats.MSBB.Part.MapPiece)
                            {
                                AddToTFMDict(objectTransformsDict, p, msbMapId);
                            }
                        }

                        rootPath = Path.GetDirectoryName(Path.GetDirectoryName(filePath));
                        if (Path.GetFileName(rootPath).ToLower() != "map")
                        {
                            rootPath = Path.GetDirectoryName(rootPath);
                        }
                        modelPath = Path.Combine(rootPath, $"{Path.GetFileNameWithoutExtension(fileName)}\\");
                        texPath = Path.Combine(rootPath, $"{worldString}\\");

                        modelFiles = Directory.GetFiles(modelPath, "*.flver.dcx");
                        texBhds = Directory.GetFiles(texPath, "*.tpfbhd*");

                        foreach (var modelFile in modelFiles)
                        {
                            var id = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(modelFile)).ToLower();
                            var foundKey = objectTransformsDict.ContainsKey(id);
                            if (foundKey || extractUnreferencedMapData)
                            {
                                GatherModel(useMetaData, aqpList, aqnList, modelNames, instanceTransformList, objectTransformsDict, texNames, unrefAqpList, unrefAqnList, unrefModelNames, unrefTexNames, Path.GetExtension(modelFile), Path.GetFileNameWithoutExtension(modelFile).ToLower(), foundKey, File.ReadAllBytes(modelFile));
                            }
                        }
                        foreach (var bhd in texBhds)
                        {
                            using (var bxf = new BXF4Reader(bhd, bhd.Replace("bhd", "bdt")))
                            {
                                GatherTexturesFromBinder(bxf, texNames, outPathDirectory);
                            }
                        }
                        break;
                    case SoulsGame.DarkSouls3:
                    case SoulsGame.Sekiro:
                    case SoulsGame.EldenRing:
                        switch (game)
                        {
                            case SoulsGame.DarkSouls3:
                                msb = SoulsFormats.SoulsFile<SoulsFormats.MSB3>.Read(bndFile.Bytes);
                                break;
                            case SoulsGame.Sekiro:
                                msb = SoulsFormats.SoulsFile<SoulsFormats.MSBS>.Read(bndFile.Bytes);
                                break;
                            case SoulsGame.EldenRing:
                                msb = SoulsFormats.SoulsFile<SoulsFormats.MSBE>.Read(bndFile.Bytes);
                                break;
                        }
                        foreach (var p in msb.Parts.GetEntries())
                        {
                            if (p is SoulsFormats.MSB3.Part.MapPiece || p is SoulsFormats.MSBS.Part.MapPiece || p is SoulsFormats.MSBE.Part.MapPiece)
                            {
                                AddToTFMDict(objectTransformsDict, p, msbMapId);
                            }
                        }

                        rootPath = Path.GetDirectoryName(Path.GetDirectoryName(filePath));
                        if (Path.GetFileName(rootPath).ToLower() != "map")
                        {
                            rootPath = Path.GetDirectoryName(rootPath);
                        }
                        switch (game)
                        {
                            case SoulsGame.DarkSouls3:
                            case SoulsGame.Sekiro:
                                modelPath = Path.Combine(rootPath, $"{msbMapId}\\");
                                texPath = Path.Combine(rootPath, $"{worldString}\\");
                                break;
                            case SoulsGame.EldenRing:
                                modelPath = Path.Combine(rootPath, $"{worldString}\\{msbMapId}\\");
                                texPath = modelPath;
                                break;
                        }

                        modelFiles = Directory.GetFiles(modelPath, "*.mapbnd.dcx");

                        foreach (var modelFile in modelFiles)
                        {
                            var id = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(modelFile)).ToLower();
                            if (objectTransformsDict.ContainsKey(id) || extractUnreferencedMapData)
                            {
                                var fileBytes = File.ReadAllBytes(modelFile);
                                if (SoulsFormats.DCX.Is(fileBytes))
                                {
                                    fileBytes = DCX.Decompress(fileBytes);
                                }
                                using (var mapBnd = new BND4Reader(fileBytes))
                                {
                                    GatherModelsFromBinder(useMetaData, aqpList, aqnList, modelNames, instanceTransformList, objectTransformsDict, mapBnd, texNames, unrefAqpList, unrefAqnList, unrefModelNames, unrefTexNames);
                                }
                            }
                        }
                        if (game == SoulsGame.EldenRing)
                        {
                            texBhds = Directory.GetFiles(texPath, "*.tpfbnd*");
                            foreach (var bnd in texBhds)
                            {
                                //Avoid LOD textures
                                if (bnd.Contains("middle") || bnd.Contains("low"))
                                {
                                    continue;
                                }
                                var fileBytes = File.ReadAllBytes(bnd);
                                if (SoulsFormats.DCX.Is(fileBytes))
                                {
                                    fileBytes = DCX.Decompress(fileBytes);
                                }
                                using (var bxf = new BND4Reader(fileBytes))
                                {
                                    GatherTexturesFromBinder(bxf, texNames, outPathDirectory);
                                }
                            }
                        }
                        else
                        {
                            texBhds = Directory.GetFiles(texPath, "*.tpfbhd*");
                            foreach (var bhd in texBhds)
                            {
                                using (var bxf = new BXF4Reader(bhd, bhd.Replace("bhd", "bdt")))
                                {
                                    GatherTexturesFromBinder(bxf, texNames, outPathDirectory);
                                }
                            }
                        }
                        break;
                }
                if (msb != null)
                {
                    ProcessModelsForExport(aqpList);
                    ProcessModelsForExport(unrefAqpList);

                    //Grab model data if it exists, send to FBX
                    if (separateMSBDumpByModel)
                    {
                        for (int m = 0; m < aqpList.Count; m++)
                        {
                            FbxExporterNative.ExportToFile(aqpList[m], aqnList[m], new List<AquaMotion>(), Path.Combine(outPathDirectory, $"{modelNames[m]}.fbx"), new List<string>(), instanceTransformList[m], useMetaData);
                        }
                    }
                    else
                    {
                        FbxExporterNative.ExportToFileSets(aqpList, aqnList, modelNames, outPath, instanceTransformList, useMetaData);
                    }

                    if (extractUnreferencedMapData)
                    {
                        for (int m = 0; m < unrefAqpList.Count; m++)
                        {
                            FbxExporterNative.ExportToFile(unrefAqpList[m], unrefAqnList[m], new List<AquaMotion>(), Path.Combine(outPathDirectory, "Unreferenced", $"{unrefModelNames[m]}.fbx"), new List<string>(), new List<Matrix4x4>(), useMetaData);
                        }
                    }

                }
            }

        }

        private static void ProcessModelsForExport(List<AquaObject> aqpList)
        {
            foreach(var aqp in aqpList)
            {
                aqp.ConvertToPSO2Model(true, false, false, true, false, false, false, true, false);
                aqp.ConvertToLegacyTypes();
                aqp.CreateTrueVertWeights();
            }
        }

        private static void GatherModelsFromBinder(bool useMetaData, List<AquaObject> aqpList, List<AquaNode> aqnList, List<string> modelNames, List<List<Matrix4x4>> instanceTransformList, Dictionary<string, List<Matrix4x4>> objectTransformsDict, BinderReader bxf, List<string> texFilenameList, List<AquaObject> unrefAqpList, List<AquaNode> unrefAqnList, List<string> unrefModelNames, List<string> unrefTexFilenameList)
        {
            foreach (var bxfFile in bxf.Files)
            {
                var bFName = Path.GetFileNameWithoutExtension(bxfFile.Name);
                var ext = Path.GetExtension(bFName);
                if (ext == "")
                {
                    ext = Path.GetExtension(bxfFile.Name);
                }
                var bFNameSansExt = Path.GetFileNameWithoutExtension(bFName).ToLower();
                bool foundKey = objectTransformsDict.ContainsKey(bFNameSansExt);
                var fileBytes = bxf.ReadFile(bxfFile);

                GatherModel(useMetaData, aqpList, aqnList, modelNames, instanceTransformList, objectTransformsDict, texFilenameList, unrefAqpList, unrefAqnList, unrefModelNames, unrefTexFilenameList, ext, bFNameSansExt, foundKey, fileBytes);
            }
        }

        private static void GatherModel(bool useMetaData, List<AquaObject> aqpList, List<AquaNode> aqnList, List<string> modelNames, List<List<Matrix4x4>> instanceTransformList, Dictionary<string, List<Matrix4x4>> objectTransformsDict, List<string> texFilenameList, List<AquaObject> unrefAqpList, List<AquaNode> unrefAqnList, List<string> unrefModelNames, List<string> unrefTexFilenameList, string ext, string bFNameSansExt, bool foundKey, byte[] fileBytes)
        {
            if (ext == ".dcx")
            {
                if (SoulsFormats.DCX.Is(fileBytes))
                {
                    fileBytes = DCX.Decompress(fileBytes);
                    ext = Path.GetExtension(bFNameSansExt);
                    bFNameSansExt = Path.GetFileNameWithoutExtension(bFNameSansExt).ToLower();
                }
            }

            if ((ext == ".flv" || ext == ".flver") && (foundKey || extractUnreferencedMapData))
            {
                IFlver flver;
                if (SoulsFile<FLVER2>.Is(fileBytes))
                {
                    flver = SoulsFile<FLVER2>.Read(fileBytes);
                }
                else
                {
                    flver = SoulsFile<FLVER0>.Read(fileBytes);
                }

                //Gather textures that are actually referenced by the model
                if (foundKey || extractUnreferencedMapData)
                {
                    foreach (var mesh in flver.Meshes)
                    {
                        var mat = flver.Materials[mesh.MaterialIndex];
                        foreach (var tex in mat.Textures)
                        {
                            var texName = Path.GetFileNameWithoutExtension(tex.Path).ToLower();
                            if (foundKey)
                            {
                                if (!texFilenameList.Contains(texName))
                                {
                                    texFilenameList.Add(texName);
                                }
                            }
                            else
                            {
                                if (!unrefTexFilenameList.Contains(texName))
                                {
                                    unrefTexFilenameList.Add(texName);
                                }
                            }
                        }
                    }
                }

                var aqp = FlverToAqua(flver, out var aqn, useMetaData);

                if (foundKey)
                {
                    foreach (var vtxl in aqp.vtxlList)
                    {
                        vtxl.vertWeightIndices.Clear();
                        vtxl.vertWeights.Clear();
                    }
                    aqpList.Add(aqp);
                    aqnList.Add(aqn);
                    modelNames.Add(bFNameSansExt);
                    instanceTransformList.Add(objectTransformsDict[bFNameSansExt]);
                }
                else
                {
                    unrefAqpList.Add(aqp);
                    unrefAqnList.Add(aqn);
                    unrefModelNames.Add(bFNameSansExt);
                }
            }
        }

        private static void GatherTexturesFromBinder(BinderReader bxf, List<string> texFilenameList, string outPath)
        {
            foreach (var bxfFile in bxf.Files)
            {
                var bFName = Path.GetFileNameWithoutExtension(bxfFile.Name);
                var ext = Path.GetExtension(bFName);
                var bFNameSansExt = Path.GetFileNameWithoutExtension(bFName).ToLower();
                var bytes = bxf.ReadFile(bxfFile);

                GatherTexturesFromTPF(texFilenameList, outPath, ext, bFNameSansExt, bytes);
            }
        }

        private static void GatherTexturesFromTPF(List<string> texFilenameList, string outPath, string ext, string bFNameSansExt, byte[] bytes)
        {
            if (ext == ".dcx")
            {
                if (SoulsFormats.DCX.Is(bytes))
                {
                    bytes = DCX.Decompress(bytes);
                    ext = Path.GetExtension(bFNameSansExt);
                }
            }

            switch (ext)
            {
                case ".tpf":
                    Directory.CreateDirectory(outPath);
                    var tpf = TPF.Read(bytes);
                    foreach (var tex in tpf.Textures)
                    {
                        if (texFilenameList.Contains(tex.Name.ToLower()))
                        {
                            File.WriteAllBytes(Path.Combine(outPath, Path.GetFileName(tex.Name) + ".dds"), tex.Headerize());
                        }
                        else if (extractUnreferencedMapData)
                        {
                            File.WriteAllBytes(Path.Combine(outPath, "Unreferenced", Path.GetFileName(tex.Name) + ".dds"), tex.Headerize());
                        }
                    }
                    break;
            }
        }

        private static void AddToTFMDict(Dictionary<string, List<Matrix4x4>> objectTransformsDict, IMsbPart p, string mapId = "")
        {
            var mdlName = p.ModelName;
            var tfm = MathExtras.ComposeFromDegreeRotation(p.Position, p.Rotation, p.Scale);

            switch (game)
            {
                case SoulsGame.DarkSouls1:
                case SoulsGame.DarkSouls1Remastered:
                    mdlName = $@"{mdlName}A{mapId.Substring(1, 2)}";
                    break;
                case SoulsGame.DemonsSouls:
                case SoulsGame.DarkSouls2:
                    break;
                default:
                    mdlName = $@"{mapId}_{mdlName.Substring(1)}";
                    break;
            }
            mdlName = mdlName.ToLower();

            if (objectTransformsDict.ContainsKey(mdlName))
            {
                objectTransformsDict[mdlName].Add(tfm);
            }
            else
            {
                objectTransformsDict[mdlName] = new List<Matrix4x4>() { tfm };
            }
        }

    }
}
