using AquaModelLibrary.Data.FromSoft;
using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Helpers.MathHelpers;
using AquaModelLibrary.Core.General;
using SoulsFormats;
using System.Numerics;
using static AquaModelLibrary.Core.FromSoft.SoulsConvert;
using AquaModelLibrary.Data.Utility;

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
            var addFbxRootNodeReal = SoulsConvert.addFBXRootNode;
            SoulsConvert.addFBXRootNode = false;
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
                string[] texBhds = new string[0];

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
                                GatherModel(useMetaData, aqpList, aqnList, modelNames, instanceTransformList, objectTransformsDict, texNames, unrefAqpList, unrefAqnList, unrefModelNames, unrefTexNames, Path.GetExtension(modelFile), Path.GetFileNameWithoutExtension(modelFile).ToLower(), true, File.ReadAllBytes(modelFile), new Dictionary<string, MATBIN>(), false, null);
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
                                GatherModel(useMetaData, aqpList, aqnList, modelNames, instanceTransformList, objectTransformsDict, texNames, unrefAqpList, unrefAqnList, unrefModelNames, unrefTexNames, Path.GetExtension(modelFile), Path.GetFileNameWithoutExtension(modelFile).ToLower(), foundKey, File.ReadAllBytes(modelFile), new Dictionary<string, MATBIN>(), false, null);
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
                            GatherModelsFromBinder(useMetaData, aqpList, aqnList, modelNames, instanceTransformList, objectTransformsDict, bxf, texNames, unrefAqpList, unrefAqnList, unrefModelNames, unrefTexNames, new Dictionary<string, MATBIN>(), false, null);
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
                                GatherModel(useMetaData, aqpList, aqnList, modelNames, instanceTransformList, objectTransformsDict, texNames, unrefAqpList, unrefAqnList, unrefModelNames, unrefTexNames, Path.GetExtension(modelFile), Path.GetFileNameWithoutExtension(modelFile).ToLower(), foundKey, File.ReadAllBytes(modelFile), new Dictionary<string, MATBIN>(), false, null);
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
                    case SoulsGame.ArmoredCore6:
                        rootPath = Path.GetDirectoryName(Path.GetDirectoryName(filePath));
                        if (Path.GetFileName(rootPath).ToLower() != "map")
                        {
                            rootPath = Path.GetDirectoryName(rootPath);
                        }
                        var gameRootPath = Path.GetDirectoryName(rootPath);
                        Dictionary<string, MATBIN> matBnds = new();
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
                                string allMatPath = Path.Combine(gameRootPath, "material", "allmaterial.matbinbnd.dcx");
                                string allMatDLC01Path = Path.Combine(gameRootPath, "material", "allmaterial_dlc01.matbinbnd.dcx");
                                string allMatDLC02Path = Path.Combine(gameRootPath, "material", "allmaterial_dlc02.matbinbnd.dcx");
                                string allMatSpeedTreePath = Path.Combine(gameRootPath, "material", "speedtree.matbinbnd.dcx");
                                if(File.Exists(allMatPath))
                                {
                                    var matBnd = new BND4Reader(File.ReadAllBytes(allMatPath));
                                    foreach(var matFile in matBnd.Files)
                                    {
                                        var matBinBinary = matBnd.ReadFile(matFile);
                                        var matBin = MATBIN.Read(matBinBinary);
                                        matBnds[Path.GetFileNameWithoutExtension(matFile.Name)] = matBin;
                                    }
                                }
                                if (File.Exists(allMatDLC01Path))
                                {
                                    var matBnd = new BND4Reader(File.ReadAllBytes(allMatDLC01Path));
                                    foreach (var matFile in matBnd.Files)
                                    {
                                        var matBinBinary = matBnd.ReadFile(matFile);
                                        var matBin = MATBIN.Read(matBinBinary);
                                        matBnds[Path.GetFileNameWithoutExtension(matFile.Name)] = matBin;
                                    }
                                }
                                if (File.Exists(allMatDLC02Path))
                                {
                                    var matBnd = new BND4Reader(File.ReadAllBytes(allMatDLC02Path));
                                    foreach (var matFile in matBnd.Files)
                                    {
                                        var matBinBinary = matBnd.ReadFile(matFile);
                                        var matBin = MATBIN.Read(matBinBinary);
                                        matBnds[Path.GetFileNameWithoutExtension(matFile.Name)] = matBin;
                                    }
                                }
                                if (File.Exists(allMatSpeedTreePath))
                                {
                                    var matBnd = new BND4Reader(File.ReadAllBytes(allMatSpeedTreePath));
                                    foreach (var matFile in matBnd.Files)
                                    {
                                        var matBinBinary = matBnd.ReadFile(matFile);
                                        var matBin = MATBIN.Read(matBinBinary);
                                        matBnds[Path.GetFileNameWithoutExtension(matFile.Name)] = matBin;
                                    }
                                }
                                break;
                            case SoulsGame.ArmoredCore6:
                                msb = SoulsFormats.SoulsFile<SoulsFormats.MSBVI>.Read(bndFile.Bytes);
                                string allMtPath = Path.Combine(gameRootPath, "material", "allmaterial.matbinbnd.dcx");
                                if (File.Exists(allMtPath))
                                {
                                    var matBnd = new BND4Reader(File.ReadAllBytes(allMtPath));
                                    foreach (var matFile in matBnd.Files)
                                    {
                                        var matBinBinary = matBnd.ReadFile(matFile);
                                        var matBin = MATBIN.Read(matBinBinary);
                                        matBnds[Path.GetFileNameWithoutExtension(matFile.Name)] = matBin;
                                    }
                                }
                                break;
                        }

                        List<string> aegModelPaths = new List<string>();
                        foreach (var p in msb.Parts.GetEntries())
                        {
                            if (p is SoulsFormats.MSB3.Part.MapPiece || p is SoulsFormats.MSBS.Part.MapPiece || p is SoulsFormats.MSBE.Part.MapPiece || p is SoulsFormats.MSBE.Part.Asset || p is SoulsFormats.MSBVI.Part.MapPiece || p is SoulsFormats.MSBVI.Part.Asset)
                            {
                                if(p is SoulsFormats.MSBE.Part.Asset)
                                {
                                    string aegFolder = p.ModelName.Substring(0, 6);
                                    var aeg = $@"asset\aeg\{aegFolder}\{p.ModelName}.geombnd.dcx";
                                    if(!aegModelPaths.Contains(aeg))
                                    {
                                        aegModelPaths.Add(aeg);
                                    }
                                } else if(p is SoulsFormats.MSBVI.Part.Asset)
                                {
                                    var aeg = $@"asset\environment\geometry\{p.ModelName}.geombnd.dcx";
                                    if (!aegModelPaths.Contains(aeg))
                                    {
                                        aegModelPaths.Add(aeg);
                                    }
                                }
                                AddToTFMDict(objectTransformsDict, p, msbMapId);
                            }
                        }

                        switch (game)
                        {
                            case SoulsGame.DarkSouls3:
                            case SoulsGame.Sekiro:
                                modelPath = Path.Combine(rootPath, $"{msbMapId}\\");
                                texPath = Path.Combine(rootPath, $"{worldString}\\");
                                break;
                            case SoulsGame.EldenRing:
                            case SoulsGame.ArmoredCore6:
                                modelPath = Path.Combine(rootPath, $"{worldString}\\{msbMapId}\\");
                                texPath = modelPath;
                                break;
                        }

                        List<string> modelFilesList = new List<string>();
                        if(Directory.Exists(modelPath))
                        {
                            modelFilesList = Directory.GetFiles(modelPath, "*.mapbnd.dcx").ToList();
                        }
                        List<string> fullAegPathList = new List<string>();
                        List<string> fullAetPathList = new List<string>();
                        foreach (var aegPath in aegModelPaths)
                        {
                            var fullPath = Path.Combine(gameRootPath, aegPath);
                            if(File.Exists(fullPath) && !fullAegPathList.Contains(fullPath))
                            {
                                fullAegPathList.Add(fullPath);
                            }
                        }

                        foreach (var modelFile in modelFilesList)
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
                                    GatherModelsFromBinder(useMetaData, aqpList, aqnList, modelNames, instanceTransformList, objectTransformsDict, mapBnd, texNames, unrefAqpList, unrefAqnList, unrefModelNames, unrefTexNames, matBnds, false, null);
                                }
                            }
                        }
                        foreach (var modelFile in fullAegPathList)
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
                                    GatherModelsFromBinder(useMetaData, aqpList, aqnList, modelNames, instanceTransformList, objectTransformsDict, mapBnd, texNames, unrefAqpList, unrefAqnList, unrefModelNames, unrefTexNames, matBnds, true, fullAetPathList);
                                }
                            }
                        }
                        foreach (var path in fullAetPathList)
                        {
                            string fullPath = null;
                            switch(game)
                            {
                                case SoulsGame.EldenRing:
                                    fullPath = Path.Combine(gameRootPath, $@"asset\aet\{path}.tpf.dcx");
                                    break;
                                case SoulsGame.ArmoredCore6:
                                    fullPath = Path.Combine(gameRootPath, $@"asset\environment\texture\{path}.tpf.dcx");
                                    break;
                            }
                            if(File.Exists(fullPath))
                            {
                                GatherTexturesFromTPF(texNames, outPathDirectory, Path.GetExtension(fullPath), Path.GetFileNameWithoutExtension(fullPath), File.ReadAllBytes(fullPath));
                            }
                        }
                        if (game == SoulsGame.EldenRing || game == SoulsGame.ArmoredCore6)
                        {
                            if(Directory.Exists(texPath))
                            {
                                texBhds = Directory.GetFiles(texPath, "*.tpfbnd*");
                            }
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
                            FbxExporterNative.ExportToFile(aqpList[m], aqnList[m], new List<AquaMotion>(), Path.Combine(outPathDirectory, $"{modelNames[m]}.fbx"), new List<string>(), instanceTransformList[m], useMetaData, (int)coordSystem);
                        }
                    }
                    else
                    {
                        FbxExporterNative.ExportToFileSets(aqpList, aqnList, modelNames, outPath, instanceTransformList, useMetaData, (int)coordSystem);
                    }

                    if (extractUnreferencedMapData)
                    {
                        for (int m = 0; m < unrefAqpList.Count; m++)
                        {
                            FbxExporterNative.ExportToFile(unrefAqpList[m], unrefAqnList[m], new List<AquaMotion>(), Path.Combine(outPathDirectory, "Unreferenced", $"{unrefModelNames[m]}.fbx"), new List<string>(), new List<Matrix4x4>(), useMetaData, (int)coordSystem);
                        }
                    }

                }
            }

            SoulsConvert.addFBXRootNode = addFbxRootNodeReal;
        }

        private static void ProcessModelsForExport(List<AquaObject> aqpList)
        {
            for(int i = 0; i < aqpList.Count; i++)
            {
                if(aqpList[i] == null)
                {
                    aqpList[i] = AquaObject.GetEmptyAquaObjectUnprocessed();
                }
                aqpList[i].ConvertToPSO2Model(true, false, false, true, false, false, false, true, false);
                aqpList[i].ConvertToLegacyTypes();
                aqpList[i].CreateTrueVertWeights();
            }
        }

        private static void GatherModelsFromBinder(bool useMetaData, List<AquaObject> aqpList, List<AquaNode> aqnList, List<string> modelNames, List<List<Matrix4x4>> instanceTransformList, Dictionary<string, List<Matrix4x4>> objectTransformsDict, BinderReader bxf, List<string> texFilenameList, List<AquaObject> unrefAqpList, List<AquaNode> unrefAqnList, List<string> unrefModelNames, List<string> unrefTexFilenameList, Dictionary<string, MATBIN> matBnds, bool isAEG, List<string> aetReferences)
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

                GatherModel(useMetaData, aqpList, aqnList, modelNames, instanceTransformList, objectTransformsDict, texFilenameList, unrefAqpList, unrefAqnList, unrefModelNames, unrefTexFilenameList, ext, bFNameSansExt, foundKey, fileBytes, matBnds, isAEG, aetReferences);
            }
        }

        private static void GatherModel(bool useMetaData, List<AquaObject> aqpList, List<AquaNode> aqnList, List<string> modelNames, List<List<Matrix4x4>> instanceTransformList, Dictionary<string, List<Matrix4x4>> objectTransformsDict, List<string> texFilenameList, List<AquaObject> unrefAqpList, List<AquaNode> unrefAqnList, List<string> unrefModelNames, List<string> unrefTexFilenameList, string ext, string bFNameSansExt, bool foundKey, byte[] fileBytes, Dictionary<string, MATBIN> matBnds, bool isAEG, List<string> aetReferences)
        {
            string speedtreeAddition = "";

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
                speedtreeAddition += flver.IsSpeedtree() ? "_Speedtree" : "";

                //Gather textures that are actually referenced by the model
                if (foundKey || extractUnreferencedMapData)
                {
                    for(int m = 0; m < flver.Meshes.Count; m++)
                    {
                        var mesh = flver.Meshes[m];
                        var mat = flver.Materials[mesh.MaterialIndex];
                        var matBinName = Path.GetFileNameWithoutExtension(mat.MTD);
                        if (matBnds.ContainsKey(matBinName))
                        {
                            //We'll never have FLVER0 here
                            FLVER2 flv = (FLVER2)flver;
                            var matbin = matBnds[matBinName];
                            for(int i = 0; i < mat.Textures.Count; i++)
                            {
                                if (mat.Textures[i].Path == "" && matbin.Samplers.Count > i)
                                {
                                    flv.Materials[mesh.MaterialIndex].Textures[i].Path = matbin.Samplers[i].Path;
                                }
                            }
                        }

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
                            if(isAEG && tex.Path != "")
                            {
                                var aetName = Path.GetFileNameWithoutExtension(texName);
                                if (aetName.Length > 10)
                                {
                                    aetName = aetName.Substring(0, 10);
                                }
                                switch (game)
                                {
                                    case SoulsGame.EldenRing:
                                        var aetDir = Path.GetFileName(Path.GetDirectoryName(tex.Path.ToLower()));
                                        var aetPath = $@"{aetDir}\{aetName}";
                                        if (!aetReferences.Contains(aetPath))
                                        {
                                            aetReferences.Add(aetPath);
                                        }
                                        break;
                                    case SoulsGame.ArmoredCore6:
                                        if (!aetReferences.Contains(aetName))
                                        {
                                            aetReferences.Add(aetName);
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }

                var aqp = FlverToAqua(flver, out var aqn, useMetaData);

                if (foundKey)
                {
                    switch (mirrorType)
                    {
                        case MirrorType.Z:
                            HandednessUtility.FlipHandednessAqpAndAqnZ(aqp, aqn);
                            break;
                        case MirrorType.Y:
                            HandednessUtility.FlipHandednessAqpAndAqnY(aqp, aqn);
                            break;
                        case MirrorType.X:
                            HandednessUtility.FlipHandednessAqpAndAqnX(aqp, aqn);
                            break;
                        case MirrorType.None:
                            break;
                    }

                    foreach (var vtxl in aqp.vtxlList)
                    {
                        vtxl.vertWeightIndices.Clear();
                        vtxl.vertWeights.Clear();
                    }
                    aqpList.Add(aqp);
                    aqnList.Add(aqn);
                    modelNames.Add(bFNameSansExt + speedtreeAddition);
                    instanceTransformList.Add(objectTransformsDict[bFNameSansExt]);
                }
                else
                {
                    unrefAqpList.Add(aqp);
                    unrefAqnList.Add(aqn);
                    unrefModelNames.Add(bFNameSansExt + speedtreeAddition);
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
            var position = p.Position;
            var rotation = p.Rotation;
            var scale = p.Scale;

            switch (mirrorType)
            {
                case MirrorType.Z:
                    position.Z *= -1;
                    rotation.X *= -1;
                    rotation.Y *= -1;
                    break;
                case MirrorType.Y:
                    position.Y *= -1;
                    rotation.X *= -1;
                    rotation.Z *= -1;
                    break;
                case MirrorType.X:
                    position.X *= -1;
                    rotation.Y *= -1;
                    rotation.Z *= -1;
                    break;
                case MirrorType.None:
                    break;
            }
            var tfm = MathExtras.ComposeFromDegreeRotation(position, rotation, scale);

            switch (game)
            {
                case SoulsGame.DarkSouls1:
                case SoulsGame.DarkSouls1Remastered:
                    mdlName = $@"{mdlName}A{mapId.Substring(1, 2)}";
                    break;
                case SoulsGame.DemonsSouls:
                case SoulsGame.DarkSouls2:
                    break;
                case SoulsGame.EldenRing:
                case SoulsGame.ArmoredCore6:
                    if (!(p is MSBE.Part.Asset || p is MSBVI.Part.Asset))
                    {
                        mdlName = $@"{mapId}_{mdlName.Substring(1)}";
                    }
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
