using AquaModelLibrary.AquaStructs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Zamboni;
using static AquaExtras.FilenameConstants;
using static AquaModelLibrary.Extra.MusicFilenameConstants;
using static AquaModelLibrary.Extra.StageFilenameConstants;
using static AquaModelLibrary.AquaMethods.AquaGeneralMethods;
using static AquaModelLibrary.AquaMiscMethods;
using static AquaModelLibrary.CharacterMakingIndex;
using static AquaModelLibrary.CharacterMakingIndexMethods;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace AquaModelLibrary.Extra
{
    public unsafe class ReferenceGenerator
    {
        public static string partColumns = "Japanese Name,English Name,Id,Adjusted Id,Icon,Normal Quality,High Quality,Normal Quality RP,High Quality RP,Linked Inner,HQ Linked Inner,Sounds,Cast Sounds,Material Anim,Material Anim Ex,Hand Textures,HQ Hand Textures";
        public static string hairColumns = "Japanese Name,English Name,Id,Adjusted Id,Male Icon,Female Icon,Cast Icon,Normal Quality,High Quality,Normal Quality RP,High Quality RP,Linked Inner,HQ Linked Inner,Sounds,Cast Sounds,Material Anim,Material Anim Ex,Hand Textures,HQ Hand Textures";
        public static string acceColumns = "Japanese Name,English Name,Id,Icon,Normal Quality,High Quality,Bone 1,Bone 2,Bone 3,Bone 4,Bone 5,Bone 6,Bone 7,Bone 8,Bone 9,Bone 10,Bone 11,Bone 12,Bone 13,Bone 14,Bone 15,Bone 16,Effect Name";

        //Takes in pso2_bin directory and outputDirectory. From there, it will read to memory various files in order to determine namings. 
        //As win32_na is a patching folder, if it exists in the pso2_bin it will be prioritized for text related items.
        public unsafe static void OutputFileLists(string pso2_binDir, string outputDirectory)
        {
            string playerDirOut = Path.Combine(outputDirectory, playerOut);
            string playerClassicDirOut = Path.Combine(playerDirOut, classicOut);
            string playerCAnimDirOut = Path.Combine(playerClassicDirOut, animsEffectsOut);
            string playerRebootDirOut = Path.Combine(playerDirOut, ngsOut);
            string playerRAnimDirOut = Path.Combine(playerRebootDirOut, animsEffectsOut);
            string npcDirOut = Path.Combine(outputDirectory, npcOut);
            string enemyDirOut = Path.Combine(outputDirectory, enemiesOut);
            string musicDirOut = Path.Combine(outputDirectory, musicOut);
            string petsDirOut = Path.Combine(outputDirectory, petsOut);
            string stageDirOut = Path.Combine(outputDirectory, stageOut);
            string uiDirOut = Path.Combine(outputDirectory, uiOut);
            Directory.CreateDirectory(enemyDirOut);
            Directory.CreateDirectory(npcDirOut);
            Directory.CreateDirectory(playerClassicDirOut);
            Directory.CreateDirectory(playerCAnimDirOut);
            Directory.CreateDirectory(playerDirOut);
            Directory.CreateDirectory(playerRebootDirOut);
            Directory.CreateDirectory(playerRAnimDirOut);
            Directory.CreateDirectory(musicDirOut);
            Directory.CreateDirectory(petsDirOut);
            Directory.CreateDirectory(stageDirOut);
            Directory.CreateDirectory(uiDirOut);

            var aquaCMX = new CharacterMakingIndex();
            PSO2Text partsText = null;
            PSO2Text acceText = null;
            PSO2Text commonText = null;
            PSO2Text commonTextReboot = null;
            PSO2Text actorNameText = null;
            PSO2Text actorNameTextReboot_NPC = null;
            PSO2Text actorNameTextReboot = null;
            PSO2Text objectCommonText = null;
            PSO2Text uiMyRoomText = null;
            LobbyActionCommon lac = null;
            List<LobbyActionCommon> rebootLac = new List<LobbyActionCommon>();
            List<int> magIds = null;
            List<int> magIdsReboot = null;
            Dictionary<int, string> faceIds = new Dictionary<int, string>();

            aquaCMX = ExtractCMX(pso2_binDir, aquaCMX);
            ReadCMXText(pso2_binDir, out partsText, out acceText, out commonText, out commonTextReboot);
            ReadExtraText(pso2_binDir, out actorNameText, out actorNameTextReboot, out actorNameTextReboot_NPC, out uiMyRoomText, out objectCommonText);

            faceIds = GetFaceVariationLuaNameDict(pso2_binDir, faceIds);

            //Load lac
            string lacPath = Path.Combine(pso2_binDir, dataDir, GetFileHash(classicLobbyAction));
            string lacPathRe = Path.Combine(pso2_binDir, dataReboot, GetRebootHash(GetFileHash(rebootLobbyAction)));
            string lacTruePath = null;
            string lacTruePathReboot = null;
            if (File.Exists(lacPath))
            {
                lacTruePath = lacPath;
            }
            if (File.Exists(lacPathRe))
            {
                lacTruePathReboot = lacPathRe;
            }
            if (lacTruePath != null)
            {
                var strm = new MemoryStream(File.ReadAllBytes(lacTruePath));
                var fVarIce = IceFile.LoadIceFile(strm);
                strm.Dispose();

                List<byte[]> files = new List<byte[]>(fVarIce.groupOneFiles);
                files.AddRange(fVarIce.groupTwoFiles);

                //Loop through files to get what we need
                foreach (byte[] file in files)
                {
                    if (IceFile.getFileName(file).ToLower().Contains(lacName))
                    {
                        lac = ReadLAC(file);
                    }
                }

                fVarIce = null;
            }
            if (lacTruePathReboot != null)
            {
                var strm = new MemoryStream(File.ReadAllBytes(lacTruePathReboot));
                var fVarIce = IceFile.LoadIceFile(strm);
                strm.Dispose();

                List<byte[]> files = new List<byte[]>(fVarIce.groupOneFiles);
                files.AddRange(fVarIce.groupTwoFiles);

                //Loop through files to get what we need
                foreach (byte[] file in files)
                {
                    var name = IceFile.getFileName(file).ToLower();
                    if (name.Contains(".lac") && !name.Contains("_classic")) //In theory, classic ices are covered already
                    {
                        rebootLac.Add(ReadRebootLAC(file));
                    }
                }

                fVarIce = null;
            }

            //Load mag settings file
            string mgxPath = Path.Combine(pso2_binDir, dataDir, GetFileHash(magSettingIce));
            if (File.Exists(mgxPath))
            {
                var strm = new MemoryStream(File.ReadAllBytes(mgxPath));
                var fVarIce = IceFile.LoadIceFile(strm);
                strm.Dispose();

                List<byte[]> files = (new List<byte[]>(fVarIce.groupOneFiles));
                files.AddRange(fVarIce.groupTwoFiles);

                //Loop through files to get what we need
                foreach (byte[] file in files)
                {
                    if (IceFile.getFileName(file).ToLower().Contains(mgxName))
                    {
                        magIds = ReadMGX(file);
                    }
                }

                fVarIce = null;
            }

            string mgxRebootPath = Path.Combine(pso2_binDir, dataReboot, GetRebootHash(GetFileHash(magSettingIce)));
            if (File.Exists(mgxRebootPath))
            {
                var strm = new MemoryStream(File.ReadAllBytes(mgxRebootPath));
                var fVarIce = IceFile.LoadIceFile(strm);
                strm.Dispose();

                List<byte[]> files = (new List<byte[]>(fVarIce.groupOneFiles));
                files.AddRange(fVarIce.groupTwoFiles);

                //Loop through files to get what we need
                foreach (byte[] file in files)
                {
                    if (IceFile.getFileName(file).ToLower().Contains(mgxName))
                    {
                        magIdsReboot = ReadMGX(file);
                    }
                }

                fVarIce = null;
            }

            //Since we have an idea of what should be there and what we're interested in parsing out, throw these into a dictionary and go
            Dictionary<string, List<List<PSO2Text.textPair>>> textByCat = new Dictionary<string, List<List<PSO2Text.textPair>>>();
            Dictionary<string, List<List<PSO2Text.textPair>>> commByCat = new Dictionary<string, List<List<PSO2Text.textPair>>>();
            Dictionary<string, List<List<PSO2Text.textPair>>> commRebootByCat = new Dictionary<string, List<List<PSO2Text.textPair>>>();
            Dictionary<string, List<List<PSO2Text.textPair>>> actorNameByCat = new Dictionary<string, List<List<PSO2Text.textPair>>>();
            Dictionary<string, List<List<PSO2Text.textPair>>> actorNameRebootByCat = new Dictionary<string, List<List<PSO2Text.textPair>>>();
            Dictionary<string, List<List<PSO2Text.textPair>>> actorNameRebootNPCByCat = new Dictionary<string, List<List<PSO2Text.textPair>>>();
            Dictionary<string, List<List<PSO2Text.textPair>>> objectCommonByCat = new Dictionary<string, List<List<PSO2Text.textPair>>>();
            Dictionary<string, List<List<PSO2Text.textPair>>> uiMyRoomByCat = new Dictionary<string, List<List<PSO2Text.textPair>>>();

            if (partsText != null)
            {
                for (int i = 0; i < partsText.text.Count; i++)
                {
                    textByCat.Add(partsText.categoryNames[i], partsText.text[i]);
                }
            }
            if (acceText != null)
            {
                for (int i = 0; i < acceText.text.Count; i++)
                {
                    //Handle dummy decoy entry in old versions
                    if (textByCat.ContainsKey(acceText.categoryNames[i]) && textByCat[acceText.categoryNames[i]][0].Count == 0)
                    {
                        textByCat[acceText.categoryNames[i]] = acceText.text[i];
                    }
                    else
                    {
                        textByCat.Add(acceText.categoryNames[i], acceText.text[i]);
                    }
                }
            }
            if (commonText != null)
            {
                for (int i = 0; i < commonText.text.Count; i++)
                {
                    commByCat.Add(commonText.categoryNames[i], commonText.text[i]);
                }
            }
            if (commonTextReboot != null)
            {
                for (int i = 0; i < commonTextReboot.text.Count; i++)
                {
                    commRebootByCat.Add(commonTextReboot.categoryNames[i], commonTextReboot.text[i]);
                }
            }
            if (actorNameText != null)
            {
                for (int i = 0; i < actorNameText.text.Count; i++)
                {
                    actorNameByCat.Add(actorNameText.categoryNames[i], actorNameText.text[i]);
                }
            }
            if (actorNameTextReboot != null)
            {
                for (int i = 0; i < actorNameTextReboot.text.Count; i++)
                {
                    actorNameRebootByCat.Add(actorNameTextReboot.categoryNames[i], actorNameTextReboot.text[i]);
                }
            }
            if (actorNameTextReboot_NPC != null)
            {
                for (int i = 0; i < actorNameTextReboot_NPC.text.Count; i++)
                {
                    actorNameRebootNPCByCat.Add(actorNameTextReboot_NPC.categoryNames[i], actorNameTextReboot_NPC.text[i]);
                }
            }
            if (objectCommonText != null)
            {
                for (int i = 0; i < objectCommonText.text.Count; i++)
                {
                    objectCommonByCat.Add(objectCommonText.categoryNames[i], objectCommonText.text[i]);
                }
            }
            if (uiMyRoomText != null)
            {
                for (int i = 0; i < uiMyRoomText.text.Count; i++)
                {
                    uiMyRoomByCat.Add(uiMyRoomText.categoryNames[i], uiMyRoomText.text[i]);
                }
            }

            List<int> masterIdList;
            List<Dictionary<int, string>> nameDicts;
            List<string> masterNameList;
            List<Dictionary<string, string>> strNameDicts;
            List<string> genAnimList, genAnimListNGS;

            GenerateObjectLists(pso2_binDir, outputDirectory, objectCommonByCat, actorNameByCat, uiMyRoomByCat);
            GenerateRoomLists(pso2_binDir, outputDirectory, uiMyRoomByCat);
            GenerateAreaData(pso2_binDir, stageDirOut);
            GenerateUILists(pso2_binDir, outputDirectory);
            DumpPaletteData(outputDirectory, aquaCMX);
            GenerateMusicData(pso2_binDir, musicDirOut);
            GenerateCasinoData(pso2_binDir, outputDirectory);
            GenerateUnitLists(pso2_binDir, outputDirectory);
            GenerateCharacterPartLists(pso2_binDir, playerDirOut, playerClassicDirOut, playerRebootDirOut, aquaCMX, faceIds, textByCat, out masterIdList, out nameDicts, out masterNameList, out strNameDicts);
            GenerateVoiceLists(pso2_binDir, playerDirOut, npcDirOut, textByCat, masterIdList, nameDicts, masterNameList, strNameDicts, actorNameByCat, actorNameRebootByCat, actorNameRebootNPCByCat);
            GenerateWeaponLists(pso2_binDir, outputDirectory);
            GenerateLobbyActionLists(pso2_binDir, playerCAnimDirOut, playerRAnimDirOut, lac, rebootLac, lacTruePath, lacTruePathReboot, commByCat, commRebootByCat, masterNameList, strNameDicts);
            GenerateMotionChangeLists(pso2_binDir, playerRAnimDirOut, commonTextReboot, commRebootByCat);
            GenerateAnimation_EffectLists(pso2_binDir, playerCAnimDirOut, playerRAnimDirOut, out genAnimList, out genAnimListNGS);
            GenerateMagList(pso2_binDir, playerDirOut, magIds, magIdsReboot);
            GeneratePhotonBlastCreatureList(playerClassicDirOut);
            GenerateVehicle_SpecialWeaponList(playerDirOut, playerCAnimDirOut, playerRAnimDirOut, genAnimList, genAnimListNGS);
            GeneratePetList(petsDirOut);
            GenerateEnemyDataList(pso2_binDir, enemyDirOut, actorNameRebootByCat, out masterNameList, out strNameDicts);
        }

        private static void GenerateObjectLists(string pso2_binDir, string outputDirectory, Dictionary<string, List<List<PSO2Text.textPair>>> objectCommonByCat, Dictionary<string, List<List<PSO2Text.textPair>>> actorNameByCat, Dictionary<string, List<List<PSO2Text.textPair>>> uiRoomByCat)
        {
            string objectOutDir = Path.Combine(outputDirectory, "Objects");
            Directory.CreateDirectory(objectOutDir);

            //Dictionaries
            List<string> masterNameList = new List<string>();
            List<Dictionary<string, string>> roomDict = new List<Dictionary<string, string>>();
            GatherTextIdsStringRef(uiRoomByCat, masterNameList, roomDict, "Room", true);

            masterNameList.Clear();
            List<Dictionary<string, string>> actorDict = new List<Dictionary<string, string>>();
            GatherTextIdsStringRef(actorNameByCat, masterNameList, actorDict, "Object", true);

            masterNameList.Clear();
            List<Dictionary<string, string>> commonDict = new List<Dictionary<string, string>>();
            GatherTextIdsStringRef(objectCommonByCat, masterNameList, commonDict, "ObjectName", true);

            //Classic objects
            string scriptsPath = Path.Combine(pso2_binDir, dataDir, GetFileHash(classicScripts));
            var scripts = new PSO2CompressedScripts(scriptsPath);
            var classicObjectsScript = scripts.knownLua["object_behavior_pack.obj.lua"];
            var classicObjectsScript2 = scripts.knownLua["object_dofile_list_server.lua"];
            classicObjectsScript.Decompile();

            List<string> classicObjectsList = new List<string>();
            string obj = "";


            List<string> classicObjectsScriptList = new List<string>();
            foreach (var constant in classicObjectsScript.Function.Constants)
            {
                classicObjectsScriptList.Add(constant.ToString());
            }
            classicObjectsScriptList.Sort();

            foreach (var constant in classicObjectsScriptList)
            {
                var str = constant;
                if (str.Length > 3)
                {
                    str = str.Substring(1, str.Length - 2);
                }

                //Order should be object name then object ice
                if (str.StartsWith("ob_"))
                {
                    if(obj != "")
                    {
                        classicObjectsList.Add(obj + ",,\n");
                        obj = "";
                    }
                    if (PSO2ObjectNames.classicObjectNames.ContainsKey(str))
                    {
                        obj += PSO2ObjectNames.classicObjectNames[str] + ",";
                    } else
                    {
                        string jpName = "";
                        string enName = "";

                        //Check room dictionary
                        if (roomDict?.Count > 0)
                        {
                            if (roomDict?[0].ContainsKey(str) == true)
                            {
                                jpName = roomDict[0][str];
                            }
                            if (roomDict?.Count > 1)
                            {
                                if (roomDict?[1].ContainsKey(str) == true)
                                {
                                    enName = roomDict[1][str];
                                }
                            }
                        }

                        //Check actor dictionary
                        if (actorDict?.Count > 0)
                        {
                            if (actorDict?[0].ContainsKey(str) == true)
                            {
                                jpName = actorDict[0][str];
                            }
                            if (actorDict?.Count > 1)
                            {
                                if (actorDict?[1].ContainsKey(str) == true)
                                {
                                    enName = actorDict[1][str];
                                }
                            }
                        }
                        obj += $"{jpName},{enName},";
                    }
                    obj += $"{str},";

                    var modelFile = $"object/map_object/{str}.ice";
                    var modelExFile = $"object/map_object/{str}_ex.ice";
                    obj += $"{modelFile},{modelExFile},";

                    var hash = GetFileHash(modelFile);
                    var hashEx = GetFileHash(modelExFile);
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, hash)))
                    {
                        obj += hash;
                    }
                    obj += ",";

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, hashEx)))
                    {
                        obj += hashEx;
                    }
                    classicObjectsList.Add(obj);

                    obj = "";
                }
            }

            List<string> classicObjectsScript2List = new List<string>();
            foreach (var constant in classicObjectsScript2.Function.Constants)
            {
                classicObjectsScript2List.Add(constant.ToString());
            }
            classicObjectsScript2List.Sort();

            foreach (var constant in classicObjectsScript2List)
            {
                var str = constant;
                if (str.Length > 3)
                {
                    str = str.Substring(1, str.Length - 2);
                }

                //Order should be object name then object ice
                if (str.StartsWith("map_"))
                {
                    str = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(str));
                    if (obj != "")
                    {
                        classicObjectsList.Add(obj + ",,\n");
                        obj = "";
                    }
                    if (PSO2ObjectNames.classicObjectNames.ContainsKey(str))
                    {
                        obj += PSO2ObjectNames.classicObjectNames[str] + ",";
                    }
                    else
                    {
                        string jpName = "";
                        string enName = "";

                        //Check actor dictionary
                        if (actorDict?.Count > 0)
                        {
                            if (actorDict?[0].ContainsKey(str) == true)
                            {
                                jpName = actorDict[0][str];
                            }
                            if (actorDict?.Count > 1)
                            {
                                if (actorDict?[1].ContainsKey(str) == true)
                                {
                                    enName = actorDict[1][str];
                                }
                            }
                        }
                        obj += $"{jpName},{enName},";
                    }
                    obj += $"{str},";

                    var modelFile = $"object/map_object/{str}.ice";

                    var hash = GetFileHash(modelFile);
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, hash)))
                    {
                        modelFile = "";
                        hash = "";
                    }
                    obj += $"{modelFile},,{hash},";
                    classicObjectsList.Add(obj);

                    obj = "";
                }
            }
            classicObjectsList.Insert(0, "Object Name JP,Object Name EN,Object ID,Object Path,Object Path Tier 6 Textures,Object Hash,Object Hash Tier 6 Textures");


            WriteCSV(objectOutDir, $"ClassicObjects.csv", classicObjectsList);
            
            //Reboot Objects
            string rbScriptsPath = Path.Combine(pso2_binDir, dataReboot, GetRebootHash(GetFileHash(rebootScripts)));
            var rbScripts = new PSO2CompressedScripts(rbScriptsPath);
            var rbObjectsScript = rbScripts.knownLua["object_preset_list.lua"];
            var rbSeasonObjectsScript = rbScripts.knownLua["object_season.lua"];
            List<string> rbObjectsList = new List<string>();
            rbObjectsScript.Decompile();
            rbSeasonObjectsScript.Decompile();

            //Get seasonal data
            Dictionary<string, string> seasonalListing = new Dictionary<string, string>();
            byte[] bytes = null; 
            using (MemoryStream memStream = new MemoryStream())
            {
                using (var writer = new StreamWriter(memStream, new UTF8Encoding(false)))
                {
                    rbSeasonObjectsScript.Print(new UnluacNET.Output(writer));
                    writer.Flush();
                    bytes = memStream.ToArray();
                }
            }
            using (MemoryStream memStream = new MemoryStream(bytes))
            {
                using (var reader = new StreamReader(memStream))
                {
                    string currentObj = null;
                    string seasonStrings = "";
                    bool inSeasonTable = false;
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if(line.Contains("Object.ObjectSeasonTable"))
                        {
                            inSeasonTable = true;
                        } else if(inSeasonTable)
                        {
                            if (line.Contains("{"))
                            {
                                currentObj = line.Replace(" ", "").Replace("-", "").Replace("{", "").Replace("=","");
                                continue;
                            }
                            if (currentObj == null)
                            {
                                if(line.Contains("}"))
                                {
                                    break;
                                }
                            }
                            if (line.Contains("}"))
                            {
                                seasonalListing.Add(currentObj, seasonStrings.Substring(0, seasonStrings.Length - 1));
                                currentObj = null;
                                seasonStrings = "";
                                continue;
                            }
                            seasonStrings += line.Replace(" ", "").Replace(",", "").Replace("\"", "").Replace("=", " ") + "|";
                        }
                    }
                }
            }

            List<string> rbObjectList = new List<string>();
            using (MemoryStream memStream = new MemoryStream())
            {
                using (var writer = new StreamWriter(memStream, new UTF8Encoding(false)))
                {
                    rbObjectsScript.Print(new UnluacNET.Output(writer));
                    writer.Flush();
                    bytes = memStream.ToArray();
                }
            }
            using (MemoryStream memStream = new MemoryStream(bytes))
            {
                using (var reader = new StreamReader(memStream))
                {
                    bool inPresetTable = false;
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (line.Contains("Object.ObjectPresetList"))
                        {
                            inPresetTable = true;
                        } else if (inPresetTable)
                        {
                            if (line.Contains("}"))
                            {
                                break;
                            }
                            rbObjectList.Add(line.Replace(" ", "").Replace(",", "").Replace("\"", "").Replace("=", "|"));
                        }
                    }
                }
            }

            string objStart = "object/map_object/";
            rbObjectList.Sort();
            foreach (var rbObj in rbObjectList)
            {
                var split = rbObj.Split('|');
                var objName = split[0];
                var detail = split[1];

                var objLOD1 = objStart + objName + "_l1.ice";
                var objLOD2 = objStart + objName + "_l2.ice";
                var objLOD3 = objStart + objName + "_l3.ice";
                var objLOD4 = objStart + objName + "_l4.ice";
                var coll = objStart + objName + "_col.ice";
                var common = objStart + objName + "_com.ice";
                var effect = objStart + objName + "_eff.ice";

                var objLOD1Hash = GetRebootHash(GetFileHash(objLOD1));
                var objLOD2Hash = GetRebootHash(GetFileHash(objLOD2));
                var objLOD3Hash = GetRebootHash(GetFileHash(objLOD3));
                var objLOD4Hash = GetRebootHash(GetFileHash(objLOD4));
                var collHash = GetRebootHash(GetFileHash(coll));
                var commonHash = GetRebootHash(GetFileHash(common));
                var effHash = GetRebootHash(GetFileHash(effect));

                if(!File.Exists(Path.Combine(pso2_binDir, dataReboot, objLOD1Hash)))
                {
                    objLOD1 = "";
                    objLOD1Hash = "";
                }
                if (!File.Exists(Path.Combine(pso2_binDir, dataReboot, objLOD2Hash)))
                {
                    objLOD2 = "";
                    objLOD2Hash = "";
                }
                if (!File.Exists(Path.Combine(pso2_binDir, dataReboot, objLOD3Hash)))
                {
                    objLOD3 = "";
                    objLOD3Hash = "";
                }
                if (!File.Exists(Path.Combine(pso2_binDir, dataReboot, objLOD4Hash)))
                {
                    objLOD4 = "";
                    objLOD4Hash = "";
                }
                if (!File.Exists(Path.Combine(pso2_binDir, dataReboot, collHash)))
                {
                    coll = "";
                    collHash = "";
                }
                if (!File.Exists(Path.Combine(pso2_binDir, dataReboot, commonHash)))
                {
                    common = "";
                    commonHash = "";
                }
                if (!File.Exists(Path.Combine(pso2_binDir, dataReboot, effHash)))
                {
                    effect = "";
                    effHash = "";
                }

                string seasonListing = "";
                if (seasonalListing.ContainsKey(objName))
                {
                    seasonListing = seasonalListing[objName];
                }

                string jpName = "";
                string enName = "";

                //Check object_common dictionary
                if (commonDict?.Count > 0)
                {
                    if (commonDict?[0].ContainsKey(objName) == true)
                    {
                        jpName = commonDict[0][objName];
                    }
                    if (commonDict?.Count > 1)
                    {
                        if (commonDict?[1].ContainsKey(objName) == true)
                        {
                            enName = commonDict[1][objName];
                        }
                    }
                }

                rbObjectsList.Add($"{jpName},{enName},{objName},{detail},{objLOD1},{objLOD1Hash},{objLOD2},{objLOD2Hash},{objLOD3},{objLOD3Hash},{objLOD4},{objLOD4Hash},{coll},{collHash},{common},{commonHash},{effect},{effHash},{seasonListing}");
            }

            rbObjectsList.Insert(0, "Object Name JP,Object Name EN,Object ID,Object Detail,Object LOD1,Object LOD1 Hash,Object LOD2,Object LOD2 Hash,Object LOD3,Object LOD3 Hash,Object LOD4,Object LOD4 Hash,Object Collision,Object Collision Hash,Object Common,Object Common Hash,Object Effect,Object Effect Hash,Seasonal Object Swaps");
            WriteCSV(objectOutDir, $"NGSObjects.csv", rbObjectsList);
        }

        private static void GenerateUILists(string pso2_binDir, string outputDirectory)
        {
            string outputStampDirectory = Path.Combine(outputDirectory, "UI", "Stamps");
            Directory.CreateDirectory(outputStampDirectory);

            //---------------------------Generate Load Tunnel Lists
            List<string> loadTunnelsOut = new List<string>();
            loadTunnelsOut.Add($"PSO2 Classic Load Tunnel,{loadTunnelClassic},{GetFileHash(loadTunnelClassic)}");
            loadTunnelsOut.Add($"NGS Load Tunnel,{loadTunnelReboot},{GetRebootHash(GetFileHash(loadTunnelReboot))}");

            File.WriteAllLines(Path.Combine(outputDirectory, "UI", "LoadTunnels.csv"), loadTunnelsOut, Encoding.UTF8);

            //---------------------------Stamps
            List<string> stampsOut = new List<string>();
            List<string> stampsNAOut = new List<string>();
            for (int i = 0; i < 10000; i++)
            {
                string name = stampPath + $"{i:D4}.ice";
                string hash = GetFileHash(name);
                string path = Path.Combine(pso2_binDir, dataDir, hash);
                string pathNA = Path.Combine(pso2_binDir, dataNADir, hash);
                if (File.Exists(path))
                {
                    stampsOut.Add(name + "," + hash);
                    var image = GetFirstImageFromIce(path);
                    if(image != null)
                    {
                        var imagePath = Path.Combine(outputStampDirectory, Path.ChangeExtension(Path.GetFileName(name), ".png"));
                        image.Save(imagePath);
                    }
                }
                if (File.Exists(pathNA))
                {
                    stampsNAOut.Add(name + "," + hash);
                    var image = GetFirstImageFromIce(pathNA);
                    if (image != null)
                    {
                        var imagePath = Path.Combine(outputStampDirectory, Path.ChangeExtension(Path.GetFileName(name), "NA.png"));
                        image.Save(imagePath);
                    }
                }
            }

            File.WriteAllLines(Path.Combine(outputStampDirectory, "stamps.csv"), stampsOut, Encoding.UTF8);
            if(stampsNAOut.Count > 0)
            {
                stampsNAOut.Insert(0, "These stamp files are used specifically for the Global version when English language options are selected.");
                File.WriteAllLines(Path.Combine(outputStampDirectory, "stampsNA.csv"), stampsNAOut, Encoding.UTF8);
            }

        }
        public unsafe static Bitmap GetFirstImageFromIce(string fileName)
        {
            IceFile ice;
            using (var strm = new MemoryStream(File.ReadAllBytes(fileName)))
            {
                try
                {
                    ice = IceFile.LoadIceFile(strm);
                }
                catch
                {
                    return null;
                }
            }
            List<byte[]> files = (new List<byte[]>(ice.groupOneFiles));
            files.AddRange(ice.groupTwoFiles);

            foreach (byte[] file in files)
            {
                if (IceFile.getFileName(file).ToLower().Contains(".dds"))
                {
                    int int32 = BitConverter.ToInt32(file, 16);
                    string str = Encoding.ASCII.GetString(file, 64, int32).TrimEnd(new char[1]);

                    int iceHeaderSize = BitConverter.ToInt32(file, 0xC);
                    int newLength = file.Length - iceHeaderSize;
                    byte[] trueFile = new byte[newLength];
                    Array.ConstrainedCopy(file, iceHeaderSize, trueFile, 0, newLength);

                    return GetDDSBitMap(trueFile);

                }
            }

            return null;
        }

        private unsafe static Bitmap GetDDSBitMap(byte[] trueFile)
        {
            using (var image = Pfim.Pfimage.FromStream(new MemoryStream(trueFile)))
            {
                PixelFormat format;

                // Convert from Pfim's backend agnostic image format into GDI+'s image format
                switch (image.Format)
                {
                    case Pfim.ImageFormat.Rgba32:
                        format = PixelFormat.Format32bppArgb;
                        break;
                    default:
                        // see the sample for more details
                        throw new NotImplementedException();
                }

                // Pin pfim's data array so that it doesn't get reaped by GC, unnecessary
                // in this snippet but useful technique if the data was going to be used in
                // control like a picture box
                var handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
                try
                {
                    var data = Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0);
                    var bitmap = new Bitmap(image.Width, image.Height, image.Stride, format, data);
                    return bitmap;
                }
                finally
                {
                    handle.Free();
                }
            }
        }

        private static void GenerateMusicData(string pso2_binDir, string outputDirectory)
        {
            StringBuilder streamList = new StringBuilder();
            StringBuilder streamListRb = new StringBuilder();
            StringBuilder adaptiveList = new StringBuilder();
            StringBuilder adaptiveListRb = new StringBuilder();

            foreach (var str in classicStreamNames)
            {
                var hash = GetFileHash(streamPath + str.Key);
                if (File.Exists(Path.Combine(pso2_binDir, dataDir, hash)))
                {
                    streamList.AppendLine($"{str.Value},{str.Key}, {hash}");
                }
                if (File.Exists(Path.Combine(pso2_binDir, dataNADir, hash)))
                {
                    streamList.AppendLine($"{str.Value},{str.Key}, {hash},NA");
                }
            }
            foreach (var str in classicStreamHashes)
            {
                if(File.Exists(Path.Combine(pso2_binDir, dataDir, str.Key)))
                {
                    streamList.AppendLine($"{str.Value},,{str.Key}");
                }
                if (File.Exists(Path.Combine(pso2_binDir, dataNADir, str.Key)))
                {
                    streamList.AppendLine($"{str.Value},,{str.Key},NA");
                }
            }
            foreach (var str in classicAdaptiveNames)
            {
                var hash = GetFileHash(adaptivePath + str.Key);
                var strCpk = str.Key.Replace(".ice", ".cpk");
                var cpkHash = GetFileHash(adaptivePath + strCpk);
                if (File.Exists(Path.Combine(pso2_binDir, dataDir, hash)))
                {
                    adaptiveList.AppendLine($"{str.Value},{str.Key}, {hash}");
                }
                if (File.Exists(Path.Combine(pso2_binDir, dataNADir, hash)))
                {
                    adaptiveList.AppendLine($"{str.Value},{str.Key}, {hash},NA");
                }
                if (File.Exists(Path.Combine(pso2_binDir, dataDir, cpkHash)))
                {
                    adaptiveList.AppendLine($"{str.Value},{strCpk}, {cpkHash}");
                }
                if (File.Exists(Path.Combine(pso2_binDir, dataNADir, cpkHash)))
                {
                    adaptiveList.AppendLine($"{str.Value},{strCpk}, {cpkHash},NA");
                }
            }
            foreach (var str in ngsStreamNames)
            {
                var hash = GetRebootHash(GetFileHash(streamPath + str.Key));
                if (File.Exists(Path.Combine(pso2_binDir, dataReboot, hash)))
                {
                    streamListRb.AppendLine($"{str.Value},{str.Key}, {hash}");
                }
                if (File.Exists(Path.Combine(pso2_binDir, dataRebootNA, hash)))
                {
                    streamListRb.AppendLine($"{str.Value},{str.Key}, {hash},NA");
                }
            }
            foreach (var str in ngsAdaptiveNames)
            {
                var hash = GetRebootHash(GetFileHash(adaptivePath + str.Key));
                var strCpk = str.Key.Replace(".ice", ".cpk");
                var cpkHash = GetFileHash(adaptivePath + strCpk);
                if (File.Exists(Path.Combine(pso2_binDir, dataReboot, hash)))
                {
                    adaptiveListRb.AppendLine($"{str.Value},{str.Key}, {hash}");
                }
                if (File.Exists(Path.Combine(pso2_binDir, dataRebootNA, hash)))
                {
                    adaptiveListRb.AppendLine($"{str.Value},{str.Key}, {hash},NA");
                }
                if (File.Exists(Path.Combine(pso2_binDir, dataReboot, cpkHash)))
                {
                    adaptiveListRb.AppendLine($"{str.Value},{strCpk}, {cpkHash}");
                }
                if (File.Exists(Path.Combine(pso2_binDir, dataRebootNA, cpkHash)))
                {
                    adaptiveListRb.AppendLine($"{str.Value},{strCpk}, {cpkHash},NA");
                }
            }

            WriteCSV(outputDirectory, $"StreamList.csv", streamList);
            WriteCSV(outputDirectory, $"StreamListNGS.csv", streamListRb);
            WriteCSV(outputDirectory, $"AdaptiveList.csv", adaptiveList);
            WriteCSV(outputDirectory, $"AdaptiveListNGS.csv", adaptiveListRb);
        }

        private static void GenerateCasinoData(string pso2_binDir, string outputDirectory)
        {
            //--------------------------Casino Stuff
        }

        private static void GenerateAreaData(string pso2_binDir, string outputDirectory)
        {
            Dictionary<int, LandPieceSettings> lpsList = new Dictionary<int, LandPieceSettings>();
            Dictionary<int, LandPieceSettings> lpsRbList = new Dictionary<int, LandPieceSettings>();
            List<int> lpsKeys = new List<int>();
            List<int> lpsRbKeys = new List<int>();

            //---------------------------Load Area Template Commons
            string lpsPath = Path.Combine(pso2_binDir, dataDir, GetFileHash(lnAreaTemplateCommon));
            if (File.Exists(lpsPath))
            {
                var strm = new MemoryStream(File.ReadAllBytes(lpsPath));
                var lpsIce = IceFile.LoadIceFile(strm);
                strm.Dispose();

                List<byte[]> files = new List<byte[]>(lpsIce.groupOneFiles);
                files.AddRange(lpsIce.groupTwoFiles);

                //Loop through files to get what we need
                foreach (byte[] file in files)
                {
                    var fname = IceFile.getFileName(file).ToLower();
                    if (fname.Contains(".lps"))
                    {
                        lpsList.Add(Int32.Parse(fname.Substring(3, 4)), ReadLPS(file));
                    }
                }

                lpsIce = null;
            }
            string lpsRebootPath = Path.Combine(pso2_binDir, dataReboot, GetRebootHash(GetFileHash(lnAreaTemplateCommonReboot)));
            if (File.Exists(lpsRebootPath))
            {
                var strm = new MemoryStream(File.ReadAllBytes(lpsRebootPath));
                var lpsIce = IceFile.LoadIceFile(strm);
                strm.Dispose();

                List<byte[]> files = new List<byte[]>(lpsIce.groupOneFiles);
                files.AddRange(lpsIce.groupTwoFiles);

                //Loop through files to get what we need
                foreach (byte[] file in files)
                {
                    var fname = IceFile.getFileName(file).ToLower();
                    if (fname.Contains(".lps"))
                    {
                        lpsRbList.Add(Int32.Parse(fname.Substring(3, 4)), ReadLPS(file));
                    }
                }

                lpsIce = null;
            }
            lpsKeys = lpsList.Keys.ToList();
            lpsRbKeys = lpsRbList.Keys.ToList();
            lpsKeys.Sort();
            lpsRbKeys.Sort();

            Dictionary<string, StringBuilder> enlightenDict = new Dictionary<string, StringBuilder>();
            Dictionary<string, StringBuilder> lhiDict = new Dictionary<string, StringBuilder>();
            Dictionary<string, StringBuilder> navMeshDict = new Dictionary<string, StringBuilder>();
            Dictionary<string, StringBuilder> stageDataDict = new Dictionary<string, StringBuilder>();
            Dictionary<string, StringBuilder> stageDataRBDict = new Dictionary<string, StringBuilder>();
            StringBuilder templateList = new StringBuilder();
            StringBuilder templateListRb = new StringBuilder();

            //Classic
            foreach(var lpsNum in lpsKeys)
            {
                StringBuilder stageData = new StringBuilder();
                var lpsName = areaNames.ContainsKey(lpsNum) ? areaNames[lpsNum] : $"{lpsNum:D4}";
                lpsName = lpsName.Contains("?") ? "" : lpsName;

                //---------------------------Generate Template Lists
                string templateName = $"stage/ln_area_template_{lpsNum:D4}.ice";
                string templateHash = GetFileHash(templateName);
                if(File.Exists(Path.Combine(pso2_binDir, dataDir, templateHash)))
                {
                    var templateLpsName = lpsName + " template,";
                    templateList.AppendLine( templateLpsName + templateName + "," + templateHash);
                }

                //---------------------------Generate Classic Terrain Model + Common + (Texture) Pack List
                var lps = lpsList[lpsNum];
                var pieceKeys = lps.pieceSets.Keys.ToList();
                pieceKeys.Sort();
                foreach(var key in pieceKeys)
                {
                    var piece = lps.pieceSets[key];
                    var varCount = piece.pieceSet.variantCount;
                    var var80Count = piece.pieceSet.variant80Count;
                    for (int i = 0; i < varCount; i++)
                    {
                        var trIceName = $"stage/sn_{lpsNum:D4}/ln_{lpsNum:D4}_{key}_{i:D2}.ice";
                        var trIceHash = GetFileHash(trIceName);
                        var filePath = Path.Combine(pso2_binDir, dataDir, trIceHash);
                        if (File.Exists(filePath))
                        {
                            stageData.AppendLine(trIceName + "," + trIceHash);
                        }

                        //Check pgd
                        var pgdIceName = trIceName.Replace(".ice", "_pgd.ice");
                        var pgdIceHash = GetFileHash(pgdIceName);
                        var filePath2 = Path.Combine(pso2_binDir, dataDir, pgdIceHash);
                        if (File.Exists(filePath2))
                        {
                            stageData.AppendLine(pgdIceName + "," + pgdIceHash);
                        }
                    }
                    for (int i = 0; i < var80Count; i++)
                    {
                        var trIceName = $"stage/sn_{lpsNum:D4}/ln_{lpsNum:D4}_{key}_{i + 80:D2}.ice";
                        var trIceHash = GetFileHash(trIceName);
                        var filePath = Path.Combine(pso2_binDir, dataDir, trIceHash);
                        if (File.Exists(filePath))
                        {
                            stageData.AppendLine(trIceName + "," + trIceHash);
                        }

                        //Check pgd
                        var pgdIceName = trIceName.Replace(".ice", "_pgd.ice");
                        var pgdIceHash = GetFileHash(pgdIceName);
                        var filePath2 = Path.Combine(pso2_binDir, dataDir, pgdIceHash);
                        if (File.Exists(filePath2))
                        {
                            stageData.AppendLine(pgdIceName + "," + pgdIceHash);
                        }
                    }
                }

                //Check shared files
                foreach(var str in LandPieceSettings.sharedFiles)
                {
                    var sharedIceName = $"stage/sn_{lpsNum:D4}/ln_{lpsNum:D4}_{str}.ice";
                    var sharedIceHash = GetFileHash(sharedIceName);
                    var filePath = Path.Combine(pso2_binDir, dataDir, sharedIceHash);
                    if (File.Exists(filePath))
                    {
                        stageData.AppendLine(sharedIceName + "," + sharedIceHash);
                    }
                }

                if (lps.fVarDict.ContainsKey("tex_ice_num"))
                {
                    var packCount = lps.fVarDict["tex_ice_num"];
                    for (int p = 0; p < packCount; p++)
                    {
                        var packIceName = $"stage/sn_{lpsNum:D4}/pack/ln_{lpsNum:D4}_pack_{p:D2}.ice";
                        var packIceHash = GetFileHash(packIceName);
                        var filePath = Path.Combine(pso2_binDir, dataDir, packIceHash);
                        if (File.Exists(filePath))
                        {
                            stageData.AppendLine(packIceName + "," + packIceHash);
                        }
                    }
                }

                //Search ALL for block and flag texture packs. Seems like something Sega might have referenced through objects, and not directly, as the LPS files do NOT contain references to these.
                //Probably possible to only search these in ep5+, but can't hurt to check
                for(int p = 0; p < 5; p++)
                {
                    var packIceName = $"stage/sn_{lpsNum:D4}/pack/ln_{lpsNum:D4}_block_pack_{p:D2}.ice";
                    var packIceHash = GetFileHash(packIceName);
                    var filePath = Path.Combine(pso2_binDir, dataDir, packIceHash);
                    if (File.Exists(filePath))
                    {
                        stageData.AppendLine(packIceName + "," + packIceHash);
                    }
                    var pack2IceName = $"stage/sn_{lpsNum:D4}/pack/ln_{lpsNum:D4}_flag_pack_{p:D2}.ice";
                    var pack2IceHash = GetFileHash(pack2IceName);
                    var file2Path = Path.Combine(pso2_binDir, dataDir, pack2IceHash);
                    if (File.Exists(file2Path))
                    {
                        stageData.AppendLine(pack2IceName + "," + pack2IceHash);
                    }
                }

                //---------------------------Check for Effects
                var effectIceName = $"stage/effect/ef_sn_{lpsNum:D4}.ice";
                var effectIceHash = GetFileHash(effectIceName);
                var effectfilePath = Path.Combine(pso2_binDir, dataDir, effectIceHash);
                if (File.Exists(effectfilePath))
                {
                    stageData.AppendLine(effectIceName + "," + effectIceHash);
                }

                //---------------------------Get Radar models
                var radarIceName = $"stage/radar/ln_{lpsNum:D4}.ice";
                var radarIceHash = GetFileHash(radarIceName);
                var radarfilePath = Path.Combine(pso2_binDir, dataDir, radarIceHash);
                if (File.Exists(radarfilePath))
                {
                    stageData.AppendLine(radarIceName + "," + radarIceHash);
                }

                //---------------------------Get Skybox models
                var weatherIceName = $"stage/weather/ln_{lpsNum:D4}_wtr.ice";
                var weatherIceHash = GetFileHash(weatherIceName);
                var weatherfilePath = Path.Combine(pso2_binDir, dataDir, weatherIceHash);
                if (File.Exists(weatherfilePath))
                {
                    stageData.AppendLine(weatherIceName + "," + weatherIceHash);
                }
                var weatherExIceName = weatherIceName.Insert(weatherIceName.Length - 5, "_ex");
                var weatherExIceHash = GetFileHash(weatherExIceName);
                var weatherExfilePath = Path.Combine(pso2_binDir, dataDir, weatherExIceHash);
                if (File.Exists(weatherExfilePath))
                {
                    stageData.AppendLine(weatherExIceName + "," + weatherExIceHash);
                }

                if (stageData.Length > 0)
                {
                    stageDataDict.Add($"{lpsNum:D4}_{lpsName}", stageData);
                }
            }

            //NGS
            foreach (var lpsNum in lpsRbKeys)
            {
                StringBuilder stageDataRb = new StringBuilder();
                List<string> enlStrings = new List<string>();
                StringBuilder enlightenOut = new StringBuilder();
                StringBuilder lhiOut = new StringBuilder();
                StringBuilder navMeshOut = new StringBuilder();
                var lpsName = areaNames.ContainsKey(lpsNum) ? areaNames[lpsNum] : $"{lpsNum:D4}";
                lpsName = lpsName.Contains("?") ? "" : lpsName;
                var lps = lpsRbList[lpsNum];

                //---------------------------Generate Template Lists
                string templateName = $"stage/ln_area_template_{lpsNum:D4}.ice";
                string templateHash = GetRebootHash(GetFileHash(templateName));
                if (File.Exists(Path.Combine(pso2_binDir, dataReboot, templateHash)))
                {
                    var name = lpsName + " template,";
                    templateListRb.AppendLine(name + templateName + "," + templateHash);
                }

                //Get models
                var pieceKeys = lps.pieceSets.Keys.ToList();
                pieceKeys.Sort();
                if (lps.pieceSets.Count > 1)
                {
                    foreach (var key in pieceKeys)
                    {
                        var piece = lps.pieceSets[key];
                        var varCount = piece.pieceSet.variantCount;
                        var var80Count = piece.pieceSet.variant80Count;
                        for (int i = 0; i < varCount; i++)
                        {
                            var trIceName = $"stage/sn_{lpsNum:D4}/ln_{lpsNum:D4}_{key}_{i:D2}.ice";
                            var trIceHash = GetRebootHash(GetFileHash(trIceName));
                            var filePath = Path.Combine(pso2_binDir, dataReboot, trIceHash);
                            if (File.Exists(filePath))
                            {
                                stageDataRb.AppendLine(trIceName + "," + trIceHash);
                            }
                        }
                        for (int i = 0; i < var80Count; i++)
                        {
                            var trIceName = $"stage/sn_{lpsNum:D4}/ln_{lpsNum:D4}_{key}_{i + 80:D2}.ice";
                            var trIceHash = GetRebootHash(GetFileHash(trIceName));
                            var filePath = Path.Combine(pso2_binDir, dataReboot, trIceHash);
                            if (File.Exists(filePath))
                            {
                                stageDataRb.AppendLine(trIceName + "," + trIceHash);
                            }
                        }
                    }

                }
                else
                {
                    for(int i = 0; i < 100; i++)
                    {
                        var trIceName = $"stage/sn_{lpsNum:D4}/ln_{lpsNum:D4}_f0_{i:D2}.ice";
                        var trIceHash = GetRebootHash(GetFileHash(trIceName));
                        if (File.Exists(Path.Combine(pso2_binDir, dataReboot, trIceHash)))
                        {
                            stageDataRb.AppendLine(trIceName + "," + trIceHash);
                        }
                        var d2IceName = trIceName.Insert(trIceName.Length - 5, "_d2");
                        var d2IceHash = GetRebootHash(GetFileHash(d2IceName));
                        if (File.Exists(Path.Combine(pso2_binDir, dataReboot, d2IceHash)))
                        {
                            stageDataRb.AppendLine(d2IceName + "," + d2IceHash);
                        }
                        var d3IceName = trIceName.Insert(trIceName.Length - 5, "_d3");
                        var d3IceHash = GetRebootHash(GetFileHash(d3IceName));
                        if (File.Exists(Path.Combine(pso2_binDir, dataReboot, d3IceHash)))
                        {
                            stageDataRb.AppendLine(d3IceName + "," + d3IceHash);
                        }
                        var d4IceName = trIceName.Insert(trIceName.Length - 5, "_d4");
                        var d4IceHash = GetRebootHash(GetFileHash(d4IceName));
                        if (File.Exists(Path.Combine(pso2_binDir, dataReboot, d4IceHash)))
                        {
                            stageDataRb.AppendLine(d4IceName + "," + d4IceHash);
                        }
                        var colIceName = trIceName.Insert(trIceName.Length - 5, "_col");
                        var colIceHash = GetRebootHash(GetFileHash(colIceName));
                        if (File.Exists(Path.Combine(pso2_binDir, dataReboot, colIceHash)))
                        {
                            stageDataRb.AppendLine(colIceName + "," + colIceHash);
                        }
                        var txlIceName = trIceName.Insert(trIceName.Length - 5, "_txl");
                        var txlIceHash = GetRebootHash(GetFileHash(txlIceName));
                        if (File.Exists(Path.Combine(pso2_binDir, dataReboot, txlIceHash)))
                        {
                            stageDataRb.AppendLine(txlIceName + "," + txlIceHash);
                        }

                        //Enlighten
                        var enIceName = $"stage/sn_{lpsNum:D4}/enlighten/ln_{lpsNum:D4}_f0_{i:D2}_enl.ice";
                        var enIceHash = GetRebootHash(GetFileHash(enIceName));
                        var enPath = Path.Combine(pso2_binDir, dataReboot, enIceHash);
                        if (File.Exists(enPath))
                        {
                            enlightenOut.AppendLine(enIceName + "," + enIceHash);
                            var enl = IceFile.LoadIceFile(new MemoryStream(File.ReadAllBytes(enPath)));
                            var files = new List<byte[]>();
                            files.AddRange(enl.groupOneFiles);
                            files.AddRange(enl.groupTwoFiles);

                            foreach(var file in files)
                            {
                                var name = IceFile.getFileName(file);
                                if(name.Contains(".elpr"))
                                {
                                    var elpr = ReadELPR(file);
                                    foreach(var piece in elpr.elprList)
                                    {
                                        if (!enlStrings.Contains(piece.name0x18))
                                        {
                                            enlStrings.Add(piece.name0x18);
                                        }
                                    }
                                }
                            }
                        }

                    }

                    //---------------------------Generate Enlighten List
                    enlStrings.Sort();
                    foreach(var str in enlStrings)
                    {
                        var enIceName = $"stage/sn_{lpsNum:D4}/enlighten/{str}.ice";
                        var enIceHash = GetRebootHash(GetFileHash(enIceName));
                        var enPath = Path.Combine(pso2_binDir, dataReboot, enIceHash);
                        if(File.Exists(enPath))
                        {
                            enlightenOut.AppendLine(enIceName + "," + enIceHash);
                        }
                    }
                    if (enlStrings.Count > 0)
                    {
                        enlightenOut.AppendLine($"sn_{lpsNum:D4}_****_****_enl.ice files omitted due to unknown naming method and lengthy brute forcing.");
                        /*
                        for(int i = 0; i < 500; i++)
                        {
                            for(int j = 0; j < 10000; j++)
                            {
                                var enIceName = $"stage/sn_{lpsNum:D4}/enlighten/sn_{lpsNum:D4}_{i:D4}_{j:D4}.ice";
                                var enIceHash = GetRebootHash(GetFileHash(enIceName));
                                var enPath = Path.Combine(pso2_binDir, dataReboot, enIceHash);
                                if (File.Exists(enPath))
                                {
                                    enlightenOut.AppendLine(enIceName + "," + enIceHash);
                                }
                            }
                        }*/
                    }

                    if (enlightenOut.Length > 0)
                    {
                        enlightenDict.Add($"{lpsNum:D4}_{lpsName}", enlightenOut);
                    }

                    //---------------------------Generate Instancing (LHI data) List
                    for (int i = 0; i < 1000; i++)
                    {
                        var lhiIceName = $"stage/sn_{lpsNum:D4}/instancing/ln_{lpsNum:D4}_instancing_{i:D4}.ice";
                        var lhiIceHash = GetRebootHash(GetFileHash(lhiIceName));
                        var lhiFilePath = Path.Combine(pso2_binDir, dataReboot, lhiIceHash);
                        if (File.Exists(lhiFilePath))
                        {
                            lhiOut.AppendLine(lhiIceName + "," + lhiIceHash);
                        }
                    }

                    var lhiOtherIceName = $"stage/sn_{lpsNum:D4}/instancing/ln_{lpsNum:D4}_instancing_other.ice";
                    var lhiOtherIceHash = GetRebootHash(GetFileHash(lhiOtherIceName));
                    var lhiOtherFilePath = Path.Combine(pso2_binDir, dataReboot, lhiOtherIceHash);
                    if (File.Exists(lhiOtherFilePath))
                    {
                        lhiOut.AppendLine(lhiOtherIceName + "," + lhiOtherIceHash);
                    }

                    if(lhiOut.Length > 0)
                    {
                        lhiDict.Add($"{lpsNum:D4}_{lpsName}", lhiOut);
                    }

                    //---------------------------Generate Reboot Navmesh List
                    for (int i = 0; i < 1000; i++)
                    {
                        var navIceName = $"stage/sn_{lpsNum:D4}/navmesh/ln_{lpsNum:D4}_nav_{i:D4}.ice";
                        var navIceHash = GetRebootHash(GetFileHash(navIceName));
                        var navFilePath = Path.Combine(pso2_binDir, dataReboot, navIceHash);
                        if (File.Exists(navFilePath))
                        {
                            navMeshOut.AppendLine(navIceName + "," + navIceHash);
                        }
                    }

                    var navComIceName = $"stage/sn_{lpsNum:D4}/navmesh/ln_{lpsNum:D4}_nav_common.ice";
                    var navComIceHash = GetRebootHash(GetFileHash(navComIceName));
                    var navComFilePath = Path.Combine(pso2_binDir, dataReboot, navComIceHash);
                    if (File.Exists(navComFilePath))
                    {
                        navMeshOut.AppendLine(navComIceName + "," + navComIceHash);
                    }

                    if (navMeshOut.Length > 0)
                    {
                        navMeshDict.Add($"{lpsNum:D4}_{lpsName}", navMeshOut);
                    }

                }

                //Check shared files
                foreach (var str in LandPieceSettings.sharedFiles)
                {
                    var sharedIceName = $"stage/sn_{lpsNum:D4}/ln_{lpsNum:D4}_{str}.ice";
                    var sharedIceHash = GetRebootHash(GetFileHash(sharedIceName));
                    var filePath = Path.Combine(pso2_binDir, dataReboot, sharedIceHash);
                    if (File.Exists(filePath))
                    {
                        stageDataRb.AppendLine(sharedIceName + "," + sharedIceHash);
                    }
                }

                //---------------------------Get Radar models
                var radarIceName = $"stage/radar/ln_{lpsNum:D4}_rad.ice";
                var radarIceHash = GetRebootHash(GetFileHash(radarIceName));
                var radarfilePath = Path.Combine(pso2_binDir, dataReboot, radarIceHash);
                if (File.Exists(radarfilePath))
                {
                    stageDataRb.AppendLine(radarIceName + "," + radarIceHash);
                }

                //---------------------------Get Skybox Models
                var weatherIceName = $"stage/weather/ln_{lpsNum:D4}_wtr.ice";
                var weatherIceHash = GetRebootHash(GetFileHash(weatherIceName));
                var weatherfilePath = Path.Combine(pso2_binDir, dataReboot, weatherIceHash);
                if (File.Exists(weatherfilePath))
                {
                    stageDataRb.AppendLine(weatherIceName + "," + weatherIceHash);
                }

                //---------------------------Get Sitpoint
                var spIceName = $"stage/sn_{lpsNum:D4}/ln_{lpsNum:D4}_sitpoint.ice";
                var spIceHash = GetRebootHash(GetFileHash(spIceName));
                var spfilePath = Path.Combine(pso2_binDir, dataReboot, spIceHash);
                if (File.Exists(spfilePath))
                {
                    stageDataRb.AppendLine(spIceName + "," + spIceHash);
                }

                //---------------------------Get DesignSet
                var dsIceName = $"stage/sn_{lpsNum:D4}/designset/ln_{lpsNum:D4}_designset.ice";
                var dsIceHash = GetRebootHash(GetFileHash(dsIceName));
                var dsfilePath = Path.Combine(pso2_binDir, dataReboot, dsIceHash);
                if (File.Exists(dsfilePath))
                {
                    stageDataRb.AppendLine(dsIceName + "," + dsIceHash);
                }
                var dsdIceName = dsIceName.Insert(dsIceName.Length - 5, "_dynamic");
                var dsdIceHash = GetRebootHash(GetFileHash(dsdIceName));
                var dsdfilePath = Path.Combine(pso2_binDir, dataReboot, dsdIceHash);
                if (File.Exists(dsdfilePath))
                {
                    stageDataRb.AppendLine(dsdIceName + "," + dsdIceHash);
                }

                if (stageDataRb.Length > 0)
                {
                    stageDataRBDict.Add($"{lpsNum:D4}_{lpsName}", stageDataRb);
                }
            }
            WriteCSV(outputDirectory, $"StageTemplates.csv", templateList);
            WriteCSV(outputDirectory, $"StageTemplatesNGS.csv", templateListRb);

            var classicDirOut = Path.Combine(outputDirectory, "Classic");
            var ngsDirOut = Path.Combine(outputDirectory, "NGS");
            Directory.CreateDirectory(classicDirOut);
            Directory.CreateDirectory(ngsDirOut);
            foreach (var stgOut in stageDataDict)
            {
                WriteCSV(classicDirOut, $"{stgOut.Key}_StageData.csv", stgOut.Value);
            }
            foreach (var stgRbOut in stageDataRBDict)
            {
                WriteCSV(ngsDirOut, $"{stgRbOut.Key}_StageDataNGS.csv", stgRbOut.Value);
            }
            foreach (var enOut in enlightenDict)
            {
                WriteCSV(ngsDirOut, $"{enOut.Key}_EnlightenDataList.csv", enOut.Value);
            }
            foreach (var lhiOut in lhiDict)
            {
                WriteCSV(ngsDirOut, $"{lhiOut.Key}_DetailObjectList.csv", lhiOut.Value);
            }
            foreach (var navOut in navMeshDict)
            {
                WriteCSV(ngsDirOut, $"{navOut.Key}_NavMeshList.csv", navOut.Value);
            }

            string outCharCreatorRoom = $"NGS エステ,NGS Salon,{charCreatorRoom},{GetFileHash(charCreatorRoom)}";
            File.WriteAllText(Path.Combine(outputDirectory, "CharacterCreator_LoginRoom.csv"), outCharCreatorRoom);
        }

        private static void GenerateUnitLists(string pso2_binDir, string outputDirectory)
        {
            AddOnIndex aox = null;
            List<string> aoxOut = new List<string>();
            aoxOut.Add("Data is laid out as follows:\nUnit name (if known), 1st model attach bone, 2nd model attach bone, Extra model attach bone, object unhashed name, object hashed name\n\n");

            //Load unit settings file
            string aoxPath = Path.Combine(pso2_binDir, dataDir, GetFileHash(unitIndexIce));
            if (File.Exists(aoxPath))
            {
                var strm = new MemoryStream(File.ReadAllBytes(aoxPath));
                var fVarIce = IceFile.LoadIceFile(strm);
                strm.Dispose();

                List<byte[]> files = new List<byte[]>(fVarIce.groupOneFiles);
                files.AddRange(fVarIce.groupTwoFiles);

                //Loop through files to get what we need
                foreach (byte[] file in files)
                {
                    if (IceFile.getFileName(file).ToLower().Contains(unitIndexFilename))
                    {
                        aox = ReadAOX(file);
                    }
                }

                fVarIce = null;
            }

            foreach(var addo in aox.addonList)
            {
                string file = $"item/addon/it_ad_{addo.id:D5}.ice";
                string fileHashed = GetFileHash(file);
                if (File.Exists(Path.Combine(pso2_binDir, dataDir, fileHashed)))
                {
                    string unitName = unitNames.ContainsKey(addo.id) ? unitNames[addo.id] : "";
                    aoxOut.Add($"{unitName},{addo.leftBoneAttach},{addo.rightBoneAttach},{addo.extraAttach},{file},{fileHashed}");
                }
            }

            string unitOutDir = Path.Combine(outputDirectory, "Units");
            Directory.CreateDirectory(unitOutDir);
            File.WriteAllLines(Path.Combine(unitOutDir, "Units.csv"), aoxOut);
        }

        private static void GenerateRoomLists(string pso2_binDir, string outputDirectory, Dictionary<string, List<List<PSO2Text.textPair>>> uiRoomByCat)
        {
            List<string> masterNameList = new List<string>();
            List<Dictionary<string, string>> textDict = new List<Dictionary<string, string>>();
            GatherTextIdsStringRef(uiRoomByCat, masterNameList, textDict, "Room", true);

            List<string> roomsOut = new List<string>();
            List<string> roomGoodsOut = new List<string>();
            var filename = Path.Combine(pso2_binDir, dataDir, GetFileHash(myRoomParametersIce));
            var iceFile = IceFile.LoadIceFile(new MemoryStream(File.ReadAllBytes(filename)));
            List<byte[]> files = new List<byte[]>();
            files.AddRange(iceFile.groupOneFiles);
            files.AddRange(iceFile.groupTwoFiles);

            roomGoodsOut.Add("Data is laid out as follows:\nObject Name (if known), Object Function, Animation type, category 1, category 2, object unhashed name, object hashed name, object _ex textures hashed name\n\n");
            for (int i = 0; i < files.Count; i++)
            {
                var name = IceFile.getFileName(files[i]);
                if (name == myRoomGoodsFilename)
                {
                    var rg = ReadMyRoomParam(files[i], 0);
                    foreach (var good in rg.roomGoodsList)
                    {
                        string obj = $"object/map_object/ob_1000_{good.goods.id:D4}.ice";
                        string objHash = GetFileHash(obj);
                        string objEx = $"object/map_object/ob_1000_{good.goods.id:D4}_ex.ice";
                        string objExHash = GetFileHash(objEx);
                        string objFile = Path.Combine(pso2_binDir, dataDir, objHash);
                        string objFileEx = Path.Combine(pso2_binDir, dataDir, objExHash);
                        
                        if(File.Exists(objFile))
                        {
                            string goodsName = roomGoodsNames.ContainsKey(obj) ? roomGoodsNames[obj] : "";
                            string output = goodsName + $",{good.functionString},{good.motionType},{good.categoryString},{good.categoryString2}," + obj + "," + objHash;
                            if (File.Exists(objFileEx))
                            {
                                output += "," + objExHash;
                            }
                            roomGoodsOut.Add(output);
                        }
                    }
                }
                else if (name == myRoomChipFilename)
                {
                    var chips = ReadMyRoomParam(files[i], 1);
                    foreach (var chip in chips.chipsList)
                    {
                        for(int id = 0; id < 105; id++)
                        {
                            string chipFinalString = chip.objectString.Substring(0, 8) + $"{id:D4}";
                            string jpName = "";
                            string enName = "";

                            if(textDict?.Count > 0)
                            {
                                if (textDict?[0].ContainsKey(chipFinalString) == true)
                                {
                                    jpName = textDict[0][chipFinalString];
                                }
                                if (textDict?.Count > 1)
                                {
                                    if (textDict?[1].ContainsKey(chipFinalString) == true)
                                    {
                                        enName = textDict[1][chipFinalString];
                                    }
                                }
                            }
                            string chipBase = "object/map_object/" + chipFinalString + ".ice";
                            string chipBaseEx = "object/map_object/" + chipFinalString + "_ex.ice";
                            string chipObj = GetFileHash(chipBase);
                            string chipObjEx = GetFileHash(chipBaseEx);
                            string objFile = Path.Combine(pso2_binDir, dataDir, chipObj);
                            string objFileEx = Path.Combine(pso2_binDir, dataDir, chipObjEx);

                            if (File.Exists(objFile))
                            {
                                string roomName = $"{jpName},{enName}";
                                string output = roomName + "," + chipBase + "," + chipObj;
                                if (File.Exists(objFileEx))
                                {
                                    output += "," + chipBaseEx + "," + chipObjEx;
                                } else
                                {
                                    output += ",,";
                                }
                                roomsOut.Add(output);
                            }
                        }
                    }
                }
            }
            files.Clear();
            roomsOut.Insert(0, "Japanese Name,English Name,Unhashed Filename,Hashed Filename,Unhashed Filename Tier 6,Hashed Filename Tier 6");

            string roomOutDir = Path.Combine(outputDirectory, "Objects");
            Directory.CreateDirectory(roomOutDir);
            WriteCSV(roomOutDir, "Rooms.csv", roomsOut);
            WriteCSV(roomOutDir, "Room Goods.csv", roomGoodsOut);

            //MySpace 
            var rebootfilename = Path.Combine(pso2_binDir, dataReboot, GetRebootHash(GetFileHash(mySpaceObjectSettingsIce)));

            if(File.Exists(rebootfilename))
            {
                iceFile = IceFile.LoadIceFile(new MemoryStream(File.ReadAllBytes(rebootfilename)));
                files.AddRange(iceFile.groupOneFiles);
                files.AddRange(iceFile.groupTwoFiles);

                roomGoodsOut.Add("Data is laid out as follows:\nObject Name (if known), Object Function, Animation type, category 1, category 2, object unhashed name, object hashed name, object _ex textures hashed name\n\n");
                for (int i = 0; i < files.Count; i++)
                {
                    var name = IceFile.getFileName(files[i]);
                    if (name == mySpaceObjectSettingsMso)
                    {
                        var mso = AquaUtil.LoadMSO("mso\0", files[i]);
                        List<string> msoInfo = new List<string>();
                        msoInfo.Add("Name, Descriptor, Group Name, [Traits]");
                        foreach (var entry in mso.msoEntries)
                        {
                            var objFile1 = $"object/map_object/{entry.asciiName}_l1.ice";
                            var objFile2 = $"object/map_object/{entry.asciiName}_l2.ice";
                            var objFile3 = $"object/map_object/{entry.asciiName}_l3.ice";
                            var objFile4 = $"object/map_object/{entry.asciiName}_l4.ice";
                            var objFileCol = $"object/map_object/{entry.asciiName}_col.ice";
                            var objFileCom = $"object/map_object/{entry.asciiName}_com.ice";
                            var objFileEff = $"object/map_object/{entry.asciiName}_eff.ice";

                            var objFile1Hash = GetRebootHash(GetFileHash(objFile1));
                            var objFile2Hash = GetRebootHash(GetFileHash(objFile2));
                            var objFile3Hash = GetRebootHash(GetFileHash(objFile3));
                            var objFile4Hash = GetRebootHash(GetFileHash(objFile4));
                            var objFileColHash = GetRebootHash(GetFileHash(objFileCol));
                            var objFileComHash = GetRebootHash(GetFileHash(objFileCom));
                            var objFileEffHash = GetRebootHash(GetFileHash(objFileEff));

                            string entryString = $"{entry.asciiName},{entry.utf8Descriptor.Replace(',', '_')},";
                            bool found = false;
                            if (File.Exists(Path.Combine(pso2_binDir, dataReboot, objFile1Hash)))
                            {
                                entryString += $"{objFile1Hash},";
                                found = true;
                            }
                            if (File.Exists(Path.Combine(pso2_binDir, dataReboot, objFile2Hash)))
                            {
                                entryString += $"{objFile2Hash},";
                                found = true;
                            }
                            if (File.Exists(Path.Combine(pso2_binDir, dataReboot, objFile3Hash)))
                            {
                                entryString += $"{objFile3Hash},";
                                found = true;
                            }
                            if (File.Exists(Path.Combine(pso2_binDir, dataReboot, objFile4Hash)))
                            {
                                entryString += $"{objFile4Hash},";
                                found = true;
                            }
                            if (File.Exists(Path.Combine(pso2_binDir, dataReboot, objFileColHash)))
                            {
                                entryString += $"{objFileColHash},";
                                found = true;
                            }
                            if (File.Exists(Path.Combine(pso2_binDir, dataReboot, objFileComHash)))
                            {
                                entryString += $"{objFileComHash},";
                                found = true;
                            }
                            if (File.Exists(Path.Combine(pso2_binDir, dataReboot, objFileEffHash)))
                            {
                                entryString += $"{objFileEffHash},";
                                found = true;
                            }



                            entryString += $"{entry.groupName},[{entry.asciiTrait1.Replace(',', '_')}],[{entry.asciiTrait2.Replace(',', '_')}],[{entry.asciiTrait3.Replace(',', '_')}],[{entry.asciiTrait4.Replace(',', '_')}],[{entry.asciiTrait5.Replace(',', '_')}]";
                            if (found == false)
                            {
                                entryString += ",(Not Found)";
                            }
                            msoInfo.Add(entryString);
                        }
                        WriteCSV(roomOutDir, "MySpace Goods.csv", msoInfo);
                    }
                }
            }

        }

        private static void GenerateAnimation_EffectLists(string pso2_binDir, string playerCAnimDirOut, string playerRAnimDirOut, out List<string> genAnimList, out List<string> genAnimListNGS)
        {
            //---------------------------Generate General Animation Lists
            genAnimList = new List<string>();
            genAnimListNGS = new List<string>();
            genAnimListNGS.Add("All files listed will be in win32reboot");

            //Special character anims
            var loadDollAnims = characterStart + "apc_loaddoll_citizen.ice";
            var npcAnims = characterStart + "np_npc_object.ice";
            var supportPartnerAnims = characterStart + "np_support_partner.ice";
            var npcDelicious = characterStart + "npc_delicious.ice";
            var tpdAnims = characterStart + "pl_bodel.ice";
            var plLightLooks = characterStart + "pl_light_looks_basnet.ice";
            var laconiumAnims = characterStart + "pl_object_rgrs.ice";
            var playerRideRoidAnims = characterStart + "pl_object_rideroid.ice";
            var dashPanelAnims = characterStart + "pl_object_dashpanel.ice";
            var monHunAnim = characterStart + "pl_volcano.ice";
            var monHunCarve = characterStart + "pl_volcano_pickup.ice";
            genAnimList.Add("," + loadDollAnims + "," + GetFileHash(loadDollAnims));
            genAnimList.Add("NPC Anims," + npcAnims + "," + GetFileHash(npcAnims));
            genAnimList.Add("Support Partner Anims," + supportPartnerAnims + "," + GetFileHash(supportPartnerAnims));
            genAnimList.Add("," + npcDelicious + "," + GetFileHash(npcDelicious));
            genAnimList.Add("True Profound Darkness Anims," + tpdAnims + "," + GetFileHash(tpdAnims));
            genAnimList.Add("," + plLightLooks + "," + GetFileHash(plLightLooks));
            genAnimList.Add("Laconium Sword Anims," + laconiumAnims + "," + GetFileHash(laconiumAnims));
            genAnimList.Add("Rideroid Plalyer Anims," + playerRideRoidAnims + "," + GetFileHash(playerRideRoidAnims));
            genAnimList.Add("Dash Panel Anims," + dashPanelAnims + "," + GetFileHash(dashPanelAnims));
            genAnimList.Add("Monster Hunter Anim," + monHunAnim + "," + GetFileHash(monHunAnim));
            genAnimList.Add("Monster Hunter Curve Anim," + monHunCarve + "," + GetFileHash(monHunCarve));

            //Player Anims
            var plCommon = characterStart + "pl_common";
            //pl_common.ice is equivalent to the _base anims, but appears without it.
            genAnimList.Add("," + plCommon + ".ice" + "," + GetFileHash(plCommon + ".ice"));
            genAnimList.Add("," + plCommon + "_act.ice" + "," + GetFileHash(plCommon + "_act.ice"));
            genAnimList.Add("," + plCommon + "_caf_cf00.ice" + "," + GetFileHash(plCommon + "_caf_cf00.ice"));
            genAnimList.Add("," + plCommon + "_caf_cf50.ice" + "," + GetFileHash(plCommon + "_caf_cf50.ice"));
            genAnimList.Add("," + plCommon + "_cam_cm00.ice" + "," + GetFileHash(plCommon + "_cam_cm00.ice"));
            genAnimList.Add("," + plCommon + "_cam_cm50.ice" + "," + GetFileHash(plCommon + "_cam_cm50.ice"));
            genAnimList.Add("," + plCommon + "_std_cf00.ice" + "," + GetFileHash(plCommon + "_std_cf00.ice"));
            genAnimList.Add("," + plCommon + "_std_cm00.ice" + "," + GetFileHash(plCommon + "_std_cm00.ice"));

            var plBattle = characterStart + "pl_battle";
            genAnimListNGS.Add("," + characterStart + "np_common_human_reboot.ice" + "," + GetFileHash(characterStart + "np_common_human_reboot.ice"));
            genAnimListNGS.Add("," + plBattle + ".ice" + "," + GetFileHash(plBattle + ".ice"));
            genAnimListNGS.Add("," + plBattle + "_sdt.ice" + "," + GetFileHash(plBattle + "_std.ice"));
            genAnimListNGS.Add("," + plBattle + "_cam.ice" + "," + GetFileHash(plBattle + "_cam.ice"));
            genAnimListNGS.Add("," + plBattle + "_act.ice" + "," + GetFileHash(plBattle + "_act.ice"));
            genAnimListNGS.Add("," + plCommon + ".ice" + "," + GetFileHash(plCommon + ".ice"));
            genAnimListNGS.Add("," + plCommon + "_bti.ice" + "," + GetFileHash(plCommon + "_bti.ice"));
            genAnimListNGS.Add("," + plCommon + "_cam.ice" + "," + GetFileHash(plCommon + "_cam.ice"));
            genAnimListNGS.Add("," + plCommon + "_std.ice" + "," + GetFileHash(plCommon + "_std.ice"));

            var wepTypeList = new List<string>() { "compoundbow", "doublesaber", "dualblade", "gunslash", "jetboots", "katana", "knuckle", "launcher", "master_doublesaber",
            "master_dualblade", "master_wand", "partisan", "poka_compoundbow", "rifle", "rod", "slayer_gunslash", "sword", "takt", "talis", "twindagger", "twinsubmachinegun",
            "unarmed", "villain_katana", "villain_rifle", "villain_rod", "wand", "wiredlance", "wpnman_sword", "wpnman_talis", "wpnman_twinsubmachinegun"};
            foreach (var wep in wepTypeList)
            {
                string entry = "";
                if (wep == "poka_compoundbow")
                {
                    entry = "(Yes, there is a duplicate PVP weapon among regular character weapons)\n";
                }
                genAnimList.Add(entry + "," + characterStart + "pl_" + wep + "_act.ice" + "," + GetFileHash(characterStart + "pl_" + wep + "_act.ice"));
                genAnimList.Add("," + characterStart + "pl_" + wep + "_base.ice" + "," + GetFileHash(characterStart + "pl_" + wep + "_base.ice"));
                genAnimList.Add("," + characterStart + "pl_" + wep + "_caf.ice" + "," + GetFileHash(characterStart + "pl_" + wep + "_caf.ice"));
                genAnimList.Add("," + characterStart + "pl_" + wep + "_cam.ice" + "," + GetFileHash(characterStart + "pl_" + wep + "_cam.ice"));
                genAnimList.Add("," + characterStart + "pl_" + wep + "_std.ice" + "," + GetFileHash(characterStart + "pl_" + wep + "_std.ice"));
            }

            //PVP Anims
            var pvpWepList = new List<string>() { "compoundbow", "doublesaber", "dualblade", "gunslash", "jetboots", "katana", "knuckle", "launcher", "partisan", "rifle", "rod",
                "unarmed", "wand", "wiredlance"};
            foreach (var wep in wepTypeList)
            {
                genAnimList.Add("," + pvpStart + "pl_" + "poka_" + wep + "_act.ice" + "," + GetFileHash(pvpStart + "pl_" + "poka_" + wep + "_act.ice"));
                genAnimList.Add("," + pvpStart + "pl_" + "poka_" + wep + "_base.ice" + "," + GetFileHash(pvpStart + "pl_" + "poka_" + wep + "_base.ice"));
                genAnimList.Add("," + pvpStart + "pl_" + "poka_" + wep + "_caf.ice" + "," + GetFileHash(pvpStart + "pl_" + "poka_" + wep + "_caf.ice"));
                genAnimList.Add("," + pvpStart + "pl_" + "poka_" + wep + "_cam.ice" + "," + GetFileHash(pvpStart + "pl_" + "poka_" + wep + "_cam.ice"));
                genAnimList.Add("," + pvpStart + "pl_" + "poka_" + wep + "_std.ice" + "," + GetFileHash(pvpStart + "pl_" + "poka_" + wep + "_std.ice"));
            }

            var wepTypeListNGS = new List<string>() { "compoundbow", "doublesaber", "dualblade", "gunslash", "jetboots", "katana", "knuckle", "launcher",
                "partisan", "rifle", "rod", "sword", "takt", "talis", "twindagger", "twinsubmachinegun",
            "unarmed", "wand", "wiredlance"};
            foreach (var wep in wepTypeListNGS)
            {
                //We know most of the list above should be in and probably the same name, but we only want to list them if they exist at present
                if (File.Exists(Path.Combine(pso2_binDir, dataReboot, GetRebootHash(GetFileHash(characterStart + "pl_" + wep + "_std.ice")))))
                {
                    genAnimListNGS.Add("," + characterStart + "pl_" + wep + "_act.ice" + "," + GetFileHash(characterStart + "pl_" + wep + "_act.ice"));
                    genAnimListNGS.Add("," + characterStart + "pl_" + wep + "_base.ice" + "," + GetFileHash(characterStart + "pl_" + wep + "_base.ice"));
                    genAnimListNGS.Add("," + characterStart + "pl_" + wep + "_cam.ice" + "," + GetFileHash(characterStart + "pl_" + wep + "_cam.ice"));
                    genAnimListNGS.Add("," + characterStart + "pl_" + wep + "_std.ice" + "," + GetFileHash(characterStart + "pl_" + wep + "_std.ice"));
                }
            }

            //Write animations later in order to get other anim archives like Dark Blast stuff

            //---------------------------Generate General Player effect List
            var effOut = new List<string>();
            var effList = playerEffects;

            foreach (var eff in effList)
            {
                string entryStart = "";
                switch (eff)
                {
                    default:
                        break;
                }
                effOut.Add(entryStart + "," + eff + "," + GetFileHash(eff));
            }

            File.WriteAllLines(Path.Combine(playerCAnimDirOut, $"General Character Effects.csv"), effOut);

            //---------------------------Generate Reboot General Player effect List
            var effRebOut = new List<string>();
            var effRebList = playerNGSEffects;

            foreach (var eff in effRebList)
            {
                string entryStart = "";
                switch (eff)
                {
                    default:
                        break;
                }
                effRebOut.Add(entryStart + "," + eff + "," + GetRebootHash(GetFileHash(eff)));
            }

            File.WriteAllLines(Path.Combine(playerRAnimDirOut, $"General Reboot Character Effects.csv"), effRebOut);
        }

        private static void GenerateMotionChangeLists(string pso2_binDir, string playerRAnimDirOut, PSO2Text commonTextReboot, Dictionary<string, List<List<PSO2Text.textPair>>> commRebootByCat)
        {
            //---------------------------Get Substitute Motion files -- 
            if (commonTextReboot != null)
            {
                List<string> subCatList = new List<string>() { subSwim, subGlide, subJump, subLanding, subMove, subSprint, subIdle };
                List<StringBuilder> subMotions = new List<StringBuilder>();
                List<string> subMotionsDebug = new List<string>();

                for (int i = 0; i < subCatList.Count; i++)
                {
                    subMotions.Add(new StringBuilder());
                }
                Dictionary<string, Dictionary<int, List<string>>> subByCat = GatherSubCategories(commRebootByCat);

                //Substitute motions seem to not have an obvious "control" file clientside. However they only go to 999
                for (int i = 0; i < 1000; i++)
                {
                    for (int cat = 0; cat < subCatList.Count; cat++)
                    {
                        //Keep going if this doesn't exist
                        string humanHash = $"{substituteMotion}{subCatList[cat]}{ToThree(i)}{rebootLAHuman}.ice";

                        //These should all exist if humanHash does
                        string castHash = GetFileHash(humanHash.Replace($"{rebootLAHuman}.ice", rebootLACastMale + ".ice"));
                        string casealHash = GetFileHash(humanHash.Replace($"{rebootLAHuman}.ice", rebootLACastFemale + ".ice"));
                        string figHash = GetFileHash(humanHash.Replace($"{rebootLAHuman}.ice", rebootFig + ".ice"));

#if DEBUG
                        subMotionsDebug.Add(GetFileHash(humanHash) + " " + humanHash);
                        subMotionsDebug.Add(castHash + " " + humanHash.Replace($"{rebootLAHuman}.ice", rebootLACastMale + ".ice"));
                        subMotionsDebug.Add(casealHash + " " + humanHash.Replace($"{rebootLAHuman}.ice", rebootLACastFemale + ".ice"));
                        subMotionsDebug.Add(figHash + " " + humanHash.Replace($"{rebootLAHuman}.ice", rebootFig + ".ice"));
#endif

                        humanHash = GetFileHash(humanHash);
                        if (!File.Exists(Path.Combine(pso2_binDir, dataReboot, GetRebootHash(humanHash))))
                        {
                            continue;
                        }

                        Dictionary<int, List<string>> sub = subByCat[subCatList[cat]];
                        string output = "";
                        bool named = false;

                        if (sub.TryGetValue(i, out List<string> dict))
                        {
                            named = true;
                            foreach (string str in dict)
                            {
                                output += str + ",";
                            }
                        }
                        else
                        {
                            output += ",";
                        }

                        //Account for lack of a name
                        if (named == false)
                        {
                            output = $"[Unnamed {i}]" + output;
                        }

                        output += ToThree(i);
                        output += "," + GetRebootHash(humanHash);
                        output += "," + GetRebootHash(castHash);
                        output += "," + GetRebootHash(casealHash);
                        output += "," + GetRebootHash(figHash);

                        output += "\n";

                        subMotions[cat].Append(output);
                    }
                }

                //Write CSVs
                for (int cat = 0; cat < subCatList.Count; cat++)
                {
                    string sub;
                    switch (subCatList[cat])
                    {
                        case "swim_":
                            sub = "Swim";
                            break;
                        case "glide_":
                            sub = "Glide";
                            break;
                        case "jump_":
                            sub = "Jump";
                            break;
                        case "landing_":
                            sub = "Landing";
                            break;
                        case "mov_":
                            sub = "Run";
                            break;
                        case "sprint_":
                            sub = "PhotonDash";
                            break;
                        case "idle_":
                            sub = "Standby";
                            break;
                        default:
                            throw new Exception();
                    }


                    subMotions[cat].Insert(0, "Files are layed out as: NGSHumanfile NGSCastFile NGSCasealFile NGSFigFile\n" +
                        "Substitute Motions are in win32reboot, unlike most NGS player files\n" +
                        "The first two characters of each filename are the folder name\n\n");

                    WriteCSV(playerRAnimDirOut, $"SubstituteMotion{sub}.csv", subMotions[cat]);
                }
#if DEBUG
                // File.WriteAllLines(Path.Combine(outputDirectory, "motionSubs_md5.txt"), subMotionsDebug);
#endif

            }
        }

        private static void GenerateLobbyActionLists(string pso2_binDir, string playerCAnimDirOut, string playerRAnimDirOut, LobbyActionCommon lac, List<LobbyActionCommon> rebootLac, string lacTruePath, string lacTruePathReboot, Dictionary<string, List<List<PSO2Text.textPair>>> commByCat, Dictionary<string, List<List<PSO2Text.textPair>>> commRebootByCat, List<string> masterNameList, List<Dictionary<string, string>> strNameDicts)
        {
            //---------------------------Parse out Lobby Action files -- in lobby_action_setting.lac within defaa92bd5435c84af0da0302544b811 and common.text in a1d84c3c748cebdb6fc42f66b73d2e57
            if (lacTruePath != null)
            {
                StringBuilder lobbyActions = new StringBuilder();
                strNameDicts.Clear();
                masterNameList.Clear();
                List<string> iceTracker = new List<string>();
                GatherTextIdsStringRef(commByCat, masterNameList, strNameDicts, "LobbyAction", true);

                lobbyActions.AppendLine("Files are layed out as: PSO2File NGSfile NGSCastFile NGSCasealFile NGSFigFile");
                lobbyActions.AppendLine("There may also be a VFX ice and reboot VFX ice, which will be appended last when applicable");
                lobbyActions.AppendLine("NGS Lobby Actions are in win32reboot, unlike most NGS player files");
                lobbyActions.AppendLine("The first two characters of each filename are the folder name");

                for (int i = 0; i < lac.dataBlocks.Count; i++)
                {
                    //There are sometimes multiple references to the same ice, but we're not interested in these entries
                    if (iceTracker.Contains(lac.dataBlocks[i].iceName))
                    {
                        continue;
                    }
                    iceTracker.Add(lac.dataBlocks[i].iceName);
                    string output = "";
                    bool named = false;

                    output += lac.dataBlocks[i].chatCommand + ",";
                    foreach (var dict in strNameDicts)
                    {
                        if (dict.TryGetValue(lac.dataBlocks[i].commonReference1, out string str))
                        {
                            named = true;
                            output += str + ",";
                        }
                        else
                        {
                            output += ",";
                        }
                    }

                    //Account for lack of a name
                    if (named == false)
                    {
                        output = $"[Unnamed {lac.dataBlocks[i].commonReference1}]" + output;
                    }

                    string classic = $"{lobbyActionStart}{lac.dataBlocks[i].iceName}";
                    string reboot = $"{lobbyActionStartReboot}{lac.dataBlocks[i].iceName}";

                    var classicHash = GetFileHash(classic);
                    var rebootHumanHash = GetFileHash(reboot.Replace(".ice", rebootLAHuman + ".ice"));
                    var rebootCastMalehash = GetFileHash(reboot.Replace(".ice", rebootLACastMale + ".ice"));
                    var rebootCastFemaleHash = GetFileHash(reboot.Replace(".ice", rebootLACastFemale + ".ice"));
                    var rebootFigHash = GetFileHash(reboot.Replace(".ice", rebootFig + ".ice"));

                    output += classicHash;

                    //Some things apparently don't have reboot versions for some reason.
                    if (File.Exists(Path.Combine(pso2_binDir, dataReboot, GetRebootHash(rebootHumanHash))))
                    {
                        output += ", " + rebootHumanHash;
                        output += ", " + rebootCastMalehash;
                        output += ", " + rebootCastFemaleHash;
                        output += ", " + rebootFigHash;
                    }

                    //Handle vfx output
                    var vfxHash = GetFileHash(lobbyActionStart + lac.dataBlocks[i].vfxIce);
                    var rebootVfxHash = GetFileHash(lobbyActionStartReboot + lac.dataBlocks[i].vfxIce);

                    if (lac.dataBlocks[i].vfxIce != "" && lac.dataBlocks[i].vfxIce != null
                        && File.Exists(Path.Combine(pso2_binDir, dataDir, vfxHash)))
                    {
                        output += ", " + vfxHash;

                        if (File.Exists(Path.Combine(pso2_binDir, dataReboot, rebootVfxHash)))
                        {
                            output += ", " + rebootVfxHash;
                        }
                    }

                    if (lac.dataBlocks[i].iceName.Contains("_m.ice") || lac.dataBlocks[i].iceName.Contains("_m_"))
                    {
                        output += ", Male";
                    }
                    else if (lac.dataBlocks[i].iceName.Contains("_f.ice") || lac.dataBlocks[i].iceName.Contains("_f_"))
                    {
                        output += ", Female";
                    }

                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                    lobbyActions.Append(output);
                }

                WriteCSV(playerCAnimDirOut, "LobbyActions.csv", lobbyActions);
            }

            //---------------------------Reboot Lobby actions --f9/4e8bfb6ee674e39fa6bc1aa697bf82
            //Unlike classic, there's a bunch of .lac files here, separated by some sort of expansion versioning and whether they're for fingers or not
            if (lacTruePathReboot != null)
            {
                StringBuilder lobbyActionsReboot = new StringBuilder();
                strNameDicts.Clear();
                masterNameList.Clear();
                List<string> iceTracker = new List<string>();
                GatherTextIdsStringRef(commRebootByCat, masterNameList, strNameDicts, "LobbyAction", true);

                lobbyActionsReboot.AppendLine("Files are layed out as: NGSHumanfile NGSCastFile NGSCasealFile NGSFigFile");
                lobbyActionsReboot.AppendLine("There may also be a VFX ice and reboot VFX ice, which will be appended last when applicable");
                lobbyActionsReboot.AppendLine("NGS Lobby Actions are in win32reboot, unlike most NGS player files");
                lobbyActionsReboot.AppendLine("The first two characters of each filename are the folder name");

                foreach (var reLac in rebootLac)
                {
                    for (int i = 0; i < reLac.rebootDataBlocks.Count; i++)
                    {
                        string iceName;
                        if (reLac.rebootDataBlocks[i].iceName != null && reLac.rebootDataBlocks[i].iceName != "")
                        {
                            iceName = reLac.rebootDataBlocks[i].iceName;
                        }
                        else
                        {
                            iceName = "pl_" + reLac.rebootDataBlocks[i].internalName0.ToLower() + ".ice";
                        }

                        //There are sometimes multiple references to the same ice, but we're not interested in these entries
                        if (iceTracker.Contains(iceName))
                        {
                            continue;
                        }
                        iceTracker.Add(iceName);
                        string output = "";
                        bool named = false;
                        output += reLac.rebootDataBlocks[i].chatCommand + ",";
                        foreach (var dict in strNameDicts)
                        {
                            if (dict.TryGetValue(reLac.rebootDataBlocks[i].commonReference1, out string str))
                            {
                                named = true;
                                output += str + ",";
                            }
                            else
                            {
                                output += ",";
                            }
                        }

                        //Account for lack of a name
                        if (named == false)
                        {
                            output = $"[Unnamed {reLac.rebootDataBlocks[i].commonReference1}]" + output;
                        }
                        string reboot = $"{lobbyActionStartReboot}{iceName}";

                        var rebootHumanHash = GetFileHash(reboot.Replace(".ice", rebootLAHuman + ".ice"));
                        var rebootCastMalehash = GetFileHash(reboot.Replace(".ice", rebootLACastMale + ".ice"));
                        var rebootCastFemaleHash = GetFileHash(reboot.Replace(".ice", rebootLACastFemale + ".ice"));
                        var rebootFigHash = GetFileHash(reboot.Replace(".ice", rebootFig + ".ice"));

                        output += ", ";
                        //Some things apparently don't have reboot versions for some reason.
                        if (File.Exists(Path.Combine(pso2_binDir, dataReboot, GetRebootHash(rebootHumanHash))))
                        {
                            output += rebootHumanHash;
                        }
                        output += ", ";
                        if (File.Exists(Path.Combine(pso2_binDir, dataReboot, GetRebootHash(rebootCastMalehash))))
                        {
                            output += rebootCastMalehash;
                        }
                        output += ", ";
                        if (File.Exists(Path.Combine(pso2_binDir, dataReboot, GetRebootHash(rebootCastFemaleHash))))
                        {
                            output += rebootCastFemaleHash;
                        }
                        output += ", ";
                        if (File.Exists(Path.Combine(pso2_binDir, dataReboot, GetRebootHash(rebootFigHash))))
                        {
                            output += rebootFigHash;
                        }

                        //Handle vfx output
                        var vfxHash = GetFileHash(lobbyActionStartReboot + reLac.rebootDataBlocks[i].vfxIce);
                        var rebootVfxHash = GetRebootHash(GetFileHash(lobbyActionStartReboot + reLac.rebootDataBlocks[i].vfxIce));

                        if (reLac.rebootDataBlocks[i].vfxIce != "" && reLac.rebootDataBlocks[i].vfxIce != null
                            && File.Exists(Path.Combine(pso2_binDir, dataReboot, rebootVfxHash)))
                        {
                            output += ", " + vfxHash;
                        }

                        if (iceName.Contains("_m.ice") || iceName.Contains("_m_"))
                        {
                            output += ", Male";
                        }
                        else if (iceName.Contains("_f.ice") || iceName.Contains("_f_"))
                        {
                            output += ", Female";
                        }

                        if (!File.Exists(Path.Combine(pso2_binDir, dataReboot, GetRebootHash(rebootHumanHash))))
                        {
                            output += ", (Not found)";
                        }

                        output += "\n";

                        lobbyActionsReboot.Append(output);
                    }
                }

                WriteCSV(playerRAnimDirOut, "LobbyActionsNGS_HandPoses.csv", lobbyActionsReboot);
            }
        }

        private static void GenerateVoiceLists(string pso2_binDir, string playerDirOut, string npcDirOut, Dictionary<string, List<List<PSO2Text.textPair>>> textByCat, 
            List<int> masterIdList, List<Dictionary<int, string>> nameDicts, List<string> masterNameList, List<Dictionary<string, string>> strNameDicts, 
            Dictionary<string, List<List<PSO2Text.textPair>>> actorNameText, Dictionary<string, List<List<PSO2Text.textPair>>> actorNameTextReboot, Dictionary<string, List<List<PSO2Text.textPair>>> actorNameTextNPCReboot)
        {
            //---------------------------Parse out voices 
            StringBuilder outputMaleVoices = new StringBuilder();
            StringBuilder outputFemaleVoices = new StringBuilder();
            StringBuilder outputCastVoices = new StringBuilder();
            StringBuilder outputCasealVoices = new StringBuilder();

            masterIdList.Clear();
            nameDicts.Clear();
            strNameDicts.Clear();
            masterNameList.Clear();
            GatherTextIdsStringRef(textByCat, masterNameList, strNameDicts, "voice", true);

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (string str in masterNameList)
            {
                string output = "";
                int id = 0;
                foreach (var dict in strNameDicts)
                {
                    dict.TryGetValue(str, out string newStr);
                    output += newStr + ",";
                }

                int voiceNum = -1;
                string voiceNumStr = "";
                if (str.Contains(voiceCman))
                {
                    id = 0;
                    voiceNumStr = str.Replace(voiceCman, "");
                }
                else if (str.Contains(voiceCwoman))
                {
                    id = 1;
                    voiceNumStr = str.Replace(voiceCwoman, "");
                }
                else if (str.Contains(voiceMan))
                {
                    id = 2;
                    voiceNumStr = str.Replace(voiceMan, "");
                }
                else if (str.Contains(voiceWoman))
                {
                    id = 3;
                    voiceNumStr = str.Replace(voiceWoman, "");

                }
                voiceNum = Int32.Parse(voiceNumStr);

                string conversion = "11_sound_voice_";
                var semiFinalName = str;

                //For some reason the non default voices are done in an odd way
                //NGS defaults seem to be 900+ so far, unsure on its ac variants
                if (voiceNum > 31 && voiceNum < 900)
                {
                    conversion += "ac";
                    voiceNum -= 50; //Thanks to Selph!
                    string newVoiceNumStr = voiceNum.ToString();
                    if (voiceNum < 10)
                    {
                        newVoiceNumStr = "0" + newVoiceNumStr;
                    }
                    semiFinalName = semiFinalName.Replace(voiceNumStr, newVoiceNumStr);
                }

                var finalName = semiFinalName.Replace("11_voice_", conversion);

                string classic = $"{playerVoiceStart}{finalName}.ice";

                var classicHash = GetFileHash(classic);

                output += classicHash;
                if (!File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                {
                    output += ", (Not found)";
                }

                output += "\n";

                switch (id)
                {
                    case 2:
                        outputMaleVoices.Append(output);
                        break;
                    case 3:
                        outputFemaleVoices.Append(output);
                        break;
                    case 0:
                        outputCastVoices.Append(output);
                        break;
                    case 1:
                        outputCasealVoices.Append(output);
                        break;
                }
            }
            WriteCSV(playerDirOut, "MaleVoices.csv", outputMaleVoices);
            WriteCSV(playerDirOut, "FemaleVoices.csv", outputFemaleVoices);
            WriteCSV(playerDirOut, "CastVoices.csv", outputCastVoices);
            WriteCSV(playerDirOut, "CasealVoices.csv", outputCasealVoices);

            //--------------------------NPC Battle Voices Classic
            StringBuilder outputBattleVoices = new StringBuilder();
            StringBuilder outputBattleVoicesNA = new StringBuilder();

            masterIdList.Clear();
            nameDicts.Clear();
            strNameDicts.Clear();
            masterNameList.Clear();
            GatherTextIdsStringRef(actorNameText, masterNameList, strNameDicts, "Npc", true);
            foreach (var str in masterNameList)
            {
                if(str.Length > 7 || !Int32.TryParse(str.Substring(4), out int result))
                {
                    continue;
                }
                string languageVoices = "";
                string output = "";
                string outputNA = "";
                foreach (var dict in strNameDicts)
                {
                    dict.TryGetValue(str, out string newStr);
                    languageVoices += newStr + ",";
                }
                var voiceBase = npcBtVoiceStart + str.ToLower().Insert(4, "5") + ".ice"; //
                var voiceStr = GetFileHash(voiceBase);
                if (File.Exists(Path.Combine(pso2_binDir, dataDir, voiceStr)))
                {
                    output += languageVoices + voiceBase + "," + voiceStr + "\n";
                }
                if (File.Exists(Path.Combine(pso2_binDir, dataNADir, voiceStr)))
                {
                    outputNA += languageVoices + voiceBase + "," + voiceStr + "\n";
                }
                if (output != "")
                {
                    outputBattleVoices.Append(output);
                }
                if (outputNA != "")
                {
                    outputBattleVoicesNA.Append(outputNA);
                }
            }
            WriteCSV(npcDirOut, "NPC Battle Voices.csv", outputBattleVoices);
            WriteCSV(npcDirOut, "NPC Battle Voices NA.csv", outputBattleVoicesNA);

            //--------------------------NPC Battle Voices Reboot
            StringBuilder outputRBBattleVoices = new StringBuilder();
            StringBuilder outputRBBattleVoicesNA = new StringBuilder();

            masterIdList.Clear();
            nameDicts.Clear();
            strNameDicts.Clear();
            masterNameList.Clear();
            GatherTextIdsStringRef(actorNameTextReboot, masterNameList, strNameDicts, "Npc", true);
            foreach(var str in masterNameList)
            {
                string languageVoices = "";
                string output = "";
                string outputNA = "";
                foreach (var dict in strNameDicts)
                {
                    dict.TryGetValue(str, out string newStr);
                    languageVoices += newStr + ",";
                }
                var voiceBase = npcBtVoiceStart + str.ToLower();
                for(int i = 0; i < 200; i++)
                {
                    var voiceStrRaw = voiceBase + $"{i:D3}.ice";
                    var voiceStr = GetRebootHash(GetFileHash(voiceStrRaw));
                    if (File.Exists(Path.Combine(pso2_binDir, dataReboot, voiceStr)))
                    {
                        output += languageVoices + voiceStrRaw + "," + voiceStr + "\n"; 
                    }
                    if (File.Exists(Path.Combine(pso2_binDir, dataRebootNA, voiceStr)))
                    {
                        outputNA += languageVoices + voiceStrRaw + "," + voiceStr + "\n";
                    }
                }
                for (int i = 995; i < 999; i++)
                {
                    var voiceStrRaw = voiceBase + $"{i:D3}.ice";
                    var voiceStr = GetRebootHash(GetFileHash(voiceStrRaw));
                    if (File.Exists(Path.Combine(pso2_binDir, dataReboot, voiceStr)))
                    {
                        output += languageVoices + "," + voiceStrRaw + "," + voiceStr + "\n";
                    }
                    if (File.Exists(Path.Combine(pso2_binDir, dataRebootNA, voiceStr)))
                    {
                        outputNA += languageVoices + voiceStrRaw + "," + voiceStr + "\n";
                    }
                }
                if(output != "")
                {
                    outputRBBattleVoices.Append(output);
                }
                if (outputNA != "")
                {
                    outputRBBattleVoicesNA.Append(outputNA);
                }
            }
            WriteCSV(npcDirOut, "NGS NPC Battle Voices.csv", outputRBBattleVoices);
            WriteCSV(npcDirOut, "NGS NPC Battle Voices NA.csv", outputRBBattleVoicesNA);
        }

        private static void DumpPaletteData(string outputDirectory, CharacterMakingIndex aquaCMX)
        {
            //---------------------------Dump character palette data to .png
            if (aquaCMX.colDict.Count > 0 || aquaCMX.legacyColDict.Count > 0)
            {
                string paletteOut = Path.Combine(outputDirectory, colorPaletteOut);
                Directory.CreateDirectory(paletteOut);

                foreach (int id in aquaCMX.colDict.Keys)
                {
                    var col = aquaCMX.colDict[id];
                    fixed (byte* ptr = col.niflCol.colorData)
                    {
                        using (Bitmap image = new Bitmap(7, 6, 7 * 4, System.Drawing.Imaging.PixelFormat.Format32bppRgb, new IntPtr(ptr)))
                        {
                            image.Save(Path.Combine(paletteOut, $"{col.textString.Replace("\0", "")}_{id}.png"));
                        }
                    }
                }
                foreach (int id in aquaCMX.legacyColDict.Keys)
                {
                    var col = aquaCMX.legacyColDict[id];
                    fixed (byte* ptr = col.vtbfCol.colorData)
                    {
                        using (Bitmap image = new Bitmap(21, 6, 21 * 4, System.Drawing.Imaging.PixelFormat.Format32bppRgb, new IntPtr(ptr)))
                        {
                            image.Save(Path.Combine(paletteOut, $"{col.utf8Name.Replace("\0", "")}_{id}_{col.utf16Name.Replace("\0", "")}.png"));
                        }
                    }
                }
            }
        }

        private static void GenerateCharacterPartLists(string pso2_binDir, string playerDirOut, string playerClassicDirOut, string playerRebootDirOut, CharacterMakingIndex aquaCMX, Dictionary<int, string> faceIds, Dictionary<string, List<List<PSO2Text.textPair>>> textByCat, out List<int> masterIdList, out List<Dictionary<int, string>> nameDicts, out List<string> masterNameList, out List<Dictionary<string, string>> strNameDicts)
        {
            //---------------------------Parse out costume and body (includes outers and cast bodies)
            StringBuilder outputCostumeMale = new StringBuilder();
            StringBuilder outputCostumeFemale = new StringBuilder();
            StringBuilder outputCastBody = new StringBuilder();
            StringBuilder outputCasealBody = new StringBuilder();
            StringBuilder outputOuterMale = new StringBuilder();
            StringBuilder outputOuterFemale = new StringBuilder();
            StringBuilder outputNGSOuterMale = new StringBuilder();
            StringBuilder outputNGSOuterFemale = new StringBuilder();
            StringBuilder outputNGSCastBody = new StringBuilder();
            StringBuilder outputNGSCasealBody = new StringBuilder();
            //StringBuilder outputNGSCostumeMale = new StringBuilder();   //Replaced by Set type basewear maybe?
            //StringBuilder outputNGSCostumeFemale = new StringBuilder();
            StringBuilder outputUnknownWearables = new StringBuilder();

            outputCostumeMale.AppendLine(partColumns);
            outputCostumeFemale.AppendLine(partColumns);
            outputCastBody.AppendLine(partColumns);
            outputCasealBody.AppendLine(partColumns);
            outputOuterMale.AppendLine(partColumns);
            outputOuterFemale.AppendLine(partColumns);
            outputCostumeMale.AppendLine(partColumns);
            outputNGSOuterMale.AppendLine(partColumns);
            outputNGSOuterFemale.AppendLine(partColumns);
            outputNGSCastBody.AppendLine(partColumns);
            outputNGSCasealBody.AppendLine(partColumns);
            outputUnknownWearables.AppendLine(partColumns);

            //Build text Dict
            masterIdList = new List<int>();
            nameDicts = new List<Dictionary<int, string>>();
            GatherTextIds(textByCat, masterIdList, nameDicts, "costume", true);
            GatherTextIds(textByCat, masterIdList, nameDicts, "body", false);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.costumeDict.Keys);
            GatherDictKeys(masterIdList, aquaCMX.outerDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            //Check as well in pso2_bin directory if _rp version of outfit exists and note as well if there's a bm file for said bd file. (Hairs similar have hm files to complement hr files)
            //There may also be hn files for these while basewear would have ho files for hand textures
            foreach (int id in masterIdList)
            {
                PartData partData = new PartData();
                bool named = false;
                for (int i = 0; i < nameDicts.Count; i++)
                {
                    var dict = nameDicts[i];
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        named = true;
                        partData.namesByLanguage.Add(str);
                    }
                }
                partData.id = id;

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    if(partData.namesByLanguage.Count > 0)
                    {
                        partData.namesByLanguage[0] = $"[Unnamed {id}]";
                    } else
                    {
                        partData.namesByLanguage.Add($"[Unnamed {id}]");
                    }
                }

                //Double check these ids and use an adjustedId if needed
                int adjustedId = id;
                if (aquaCMX.costumeIdLink.ContainsKey(id))
                {
                    adjustedId = aquaCMX.costumeIdLink[id].bcln.fileId;
                }
                else if (aquaCMX.outerWearIdLink.ContainsKey(id))
                {
                    adjustedId = aquaCMX.outerWearIdLink[id].bcln.fileId;
                }
                partData.adjustedId = adjustedId;

                //Decide if bd or ow
                int soundId = -1;
                int linkedInnerId = id + 50000;
                string typeString = "bd_";
                bool classicOwCheck = id >= 20000 && id < 40000;
                bool rebootOwCheck = id >= 100000 && id < 300000;
                if (classicOwCheck == true || rebootOwCheck == true)
                {
                    typeString = "ow_";
                    if (aquaCMX.outerDict.ContainsKey(id))
                    {
                        soundId = aquaCMX.outerDict[id].body2.costumeSoundId;
                        linkedInnerId = aquaCMX.outerDict[id].body2.linkedInnerId;
                    }
                }
                else
                {
                    if (aquaCMX.costumeDict.ContainsKey(id))
                    {
                        soundId = aquaCMX.costumeDict[id].body2.costumeSoundId;
                        linkedInnerId = aquaCMX.costumeDict[id].body2.linkedInnerId;
                    }
                }

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{rebootStart}{typeString}{adjustedId}.ice";
                    string rebEx = $"{rebootExStart}{typeString}{adjustedId}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);
                    string rebLinkedInner = $"{rebootStart}b1_{linkedInnerId}.ice";
                    string rebLinkedInnerEx = $"{rebootExStart}b1_{linkedInnerId}_ex.ice";
                    string rebLinkedInnerHash = GetFileHash(rebLinkedInner);
                    string rebLinkedInnerExHash = GetFileHash(rebLinkedInnerEx);

                    //Set icon string
                    GetCostumeOuterIconString(partData, pso2_binDir, id.ToString());
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        partData.partHash = rebHash;
                        partData.partName = reb;
                    }

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebExHash)))
                    {
                        partData.partExHash = rebExHash;
                        partData.partExName = rebEx;
                    }
                    AddBodyExtraFiles(partData, reb, pso2_binDir, "_" + typeString, false);
                    AddHQBodyExtraFiles(partData, rebEx, pso2_binDir, "_" + typeString, false);
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebLinkedInnerHash)))
                    {
                        partData.linkedInnerHash = rebLinkedInnerHash;
                        partData.linkedInnerName = rebLinkedInner;
                    }
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebLinkedInnerExHash)))
                    {
                        partData.linkedInnerExHash = rebLinkedInnerExHash;
                        partData.linkedInnerName = rebLinkedInnerEx;
                    }
                    AddOutfitSound(partData, pso2_binDir, soundId);
                }
                else
                {
                    string finalId = $"{adjustedId:D5}";
                    string finalIdIcon = $"{id:D5}";
                    string classic = $"{classicStart}{typeString}{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        partData.partHash = classicHash;
                        partData.partName = classic;
                    }

                    //Set icon string
                    GetCostumeOuterIconString(partData, pso2_binDir, finalIdIcon);

                    AddBodyExtraFiles(partData, classic, pso2_binDir, "_" + typeString, true);
                    AddOutfitSound(partData, pso2_binDir, soundId);
                }

                string output = partData.GetLine();
                partData = null;

                //Decide which type this is
                if (id < 10000)
                {
                    outputCostumeMale.Append(output);
                }
                else if (id < 20000)
                {
                    outputCostumeFemale.Append(output);
                }
                else if (id < 30000)
                {
                    outputOuterMale.Append(output);
                }
                else if (id < 40000)
                {
                    outputOuterFemale.Append(output);
                }
                else if (id < 50000)
                {
                    outputCastBody.Append(output);
                }
                else if (id < 100000)
                {
                    outputCasealBody.Append(output);
                }
                else if (id < 200000)
                {
                    outputNGSOuterMale.Append(output);
                }
                else if (id < 300000)
                {
                    outputNGSOuterFemale.Append(output);
                }
                else if (id < 400000)
                {
                    outputNGSCastBody.Append(output);
                }
                else if (id < 500000)
                {
                    outputNGSCasealBody.Append(output);
                }
                else
                {
                    outputUnknownWearables.Append(output);
                }
            }
            WriteCSV(playerClassicDirOut, "MaleCostumes.csv", outputCostumeMale);
            WriteCSV(playerClassicDirOut, "FemaleCostumes.csv", outputCostumeFemale);
            WriteCSV(playerClassicDirOut, "MaleOuters.csv", outputOuterMale);
            WriteCSV(playerClassicDirOut, "FemaleOuters.csv", outputOuterFemale);
            WriteCSV(playerClassicDirOut, "CastBodies.csv", outputCastBody);
            WriteCSV(playerClassicDirOut, "CasealBodies.csv", outputCasealBody);
            WriteCSV(playerRebootDirOut, "MaleNGSOuters.csv", outputNGSOuterMale);
            WriteCSV(playerRebootDirOut, "FemaleNGSOuters.csv", outputNGSOuterFemale);
            WriteCSV(playerRebootDirOut, "CastNGSBodies.csv", outputNGSCastBody);
            WriteCSV(playerRebootDirOut, "CasealNGSBodies.csv", outputNGSCasealBody);
            //WriteCSV(playerRebootDirOut, "MaleNGSCostumes.csv", outputNGSCostumeMale);
            //WriteCSV(playerRebootDirOut, "FemaleNGSCostumes.csv", outputNGSCostumeFemale);
            WriteCSV(playerDirOut, "UnknownOutfits.csv", outputUnknownWearables);

            //---------------------------Parse out basewear
            StringBuilder outputBasewearMale = new StringBuilder();
            StringBuilder outputBasewearFemale = new StringBuilder();
            StringBuilder outputNGSBasewearMale = new StringBuilder();
            StringBuilder outputNGSBasewearFemale = new StringBuilder();
            StringBuilder outputNGSGenderlessBasewear = new StringBuilder();

            outputBasewearMale.AppendLine(partColumns);
            outputBasewearFemale.AppendLine(partColumns);
            outputNGSBasewearMale.AppendLine(partColumns);
            outputNGSBasewearFemale.AppendLine(partColumns);
            outputNGSGenderlessBasewear.AppendLine(partColumns);

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "basewear", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.baseWearDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            //Check as well in pso2_bin directory if _rp version of outfit exists and note as well if there's a bm file for said bd file. (Hairs similar have hm files to complement hr files)
            //There may also be hn files for these while basewear would have ho files for hand textures
            foreach (int id in masterIdList)
            {
                PartData partData = new PartData();
                foreach (var dict in nameDicts)
                {
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        partData.namesByLanguage.Add(str);
                    }
                }
                partData.id = id;

                //Get SoundID
                int soundId = -1;
                int linkedInnerId = -1;
                if (aquaCMX.baseWearDict.ContainsKey(id))
                {
                    soundId = aquaCMX.baseWearDict[id].body2.costumeSoundId;
                    linkedInnerId = aquaCMX.baseWearDict[id].body2.linkedInnerId;
                }

                //Double check these ids and use an adjustedId if needed
                int adjustedId = id;
                if (aquaCMX.baseWearIdLink.ContainsKey(id))
                {
                    adjustedId = aquaCMX.baseWearIdLink[id].bcln.fileId;
                }
                partData.adjustedId = adjustedId;

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{rebootStart}bw_{adjustedId}.ice";
                    string rebEx = $"{rebootExStart}bw_{adjustedId}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);
                    string rebLinkedInner = $"{rebootStart}b1_{linkedInnerId}.ice";
                    string rebLinkedInnerEx = $"{rebootExStart}b1_{linkedInnerId}_ex.ice";
                    string rebLinkedInnerHash = GetFileHash(rebLinkedInner);
                    string rebLinkedInnerExHash = GetFileHash(rebLinkedInnerEx);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        partData.partHash = rebHash;
                        partData.partName = reb;
                    }

                    //Set icon string
                    var iconStr = GetBasewearIconString(id.ToString());
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        partData.iconHash = iconStr;
                        partData.iconName = GetBasewearIconStringUnhashed(id.ToString());
                    }

                    AddBasewearExtraFiles(partData, reb, pso2_binDir, false);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebExHash)))
                    {
                        partData.partExHash = rebExHash;
                        partData.partExName = rebEx;
                    }

                    AddHQBasewearExtraFiles(partData, rebEx, pso2_binDir, false);
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebLinkedInnerHash)))
                    {
                        partData.linkedInnerHash = rebLinkedInnerHash;
                        partData.linkedInnerName = rebLinkedInner;
                    }
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebLinkedInnerExHash)))
                    {
                        partData.linkedInnerExHash = rebLinkedInnerExHash;
                        partData.linkedInnerName = rebLinkedInnerEx;
                    }
                    AddOutfitSound(partData, pso2_binDir, soundId);
                }
                else
                {
                    string finalId = $"{adjustedId:D5}";
                    string finalIdIcon = $"{id:D5}";
                    string classic = $"{classicStart}bw_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        partData.partHash = classicHash;
                        partData.partName = classic;
                    }

                    //Set icon string
                    var iconStr = GetBasewearIconString(finalIdIcon);
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        partData.iconHash = iconStr;
                        partData.iconName = GetBasewearIconStringUnhashed(id.ToString());
                    }

                    AddBasewearExtraFiles(partData, classic, pso2_binDir, true);
                    AddOutfitSound(partData, pso2_binDir, soundId);
                }

                string output = partData.GetLine();
                partData = null;

                //Decide which type this is
                if (id < 30000)
                {
                    outputBasewearMale.Append(output);
                }
                else if (id < 40000)
                {
                    outputBasewearFemale.Append(output);
                }
                else if (id < 200000)
                {
                    outputNGSBasewearMale.Append(output);
                }
                else if (id < 300000)
                {
                    outputNGSBasewearFemale.Append(output);
                }
                else if (id < 600000)
                {
                    outputNGSGenderlessBasewear.Append(output);
                }
                else
                {
                    Console.WriteLine("Unknown bw with id: " + id);
                }
            }
            WriteCSV(playerClassicDirOut, "MaleBasewear.csv", outputBasewearMale);
            WriteCSV(playerClassicDirOut, "FemaleBasewear.csv", outputBasewearFemale);
            WriteCSV(playerRebootDirOut, "MaleNGSBasewear.csv", outputNGSBasewearMale);
            WriteCSV(playerRebootDirOut, "FemaleNGSBasewear.csv", outputNGSBasewearFemale);
            WriteCSV(playerRebootDirOut, "GenderlessNGSBasewear.csv", outputNGSGenderlessBasewear);

            //---------------------------Parse out innerwear
            StringBuilder outputInnerwearMale = new StringBuilder();
            StringBuilder outputInnerwearFemale = new StringBuilder();
            StringBuilder outputNGSInnerwearMale = new StringBuilder();
            StringBuilder outputNGSInnerwearFemale = new StringBuilder();

            outputInnerwearMale.AppendLine(partColumns);
            outputInnerwearFemale.AppendLine(partColumns);
            outputNGSInnerwearMale.AppendLine(partColumns);
            outputNGSInnerwearFemale.AppendLine(partColumns);

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "innerwear", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.innerWearDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                PartData partData = new PartData();
                foreach (var dict in nameDicts)
                {
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        partData.namesByLanguage.Add(str);
                    }
                }
                partData.id = id;

                //Double check these ids and use an adjustedId if needed
                int adjustedId = id;
                if (aquaCMX.innerWearIdLink.ContainsKey(id))
                {
                    adjustedId = aquaCMX.innerWearIdLink[id].bcln.fileId;
                }
                partData.adjustedId = adjustedId;

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{rebootStart}iw_{adjustedId}.ice";
                    string rebEx = $"{rebootExStart}iw_{adjustedId}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        partData.partHash = rebHash;
                        partData.partName = reb;
                    }

                    string iconStr = GetInnerwearIconString(id.ToString());
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        partData.iconHash = iconStr;
                        partData.iconName = GetInnerwearIconStringUnhashed(id.ToString());
                    }

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebExHash)))
                    {
                        partData.partExHash = rebExHash;
                        partData.partExName = rebEx;
                    }

                }
                else
                {
                    string finalId = $"{adjustedId:D5}";
                    string finalIdIcon = $"{id:D5}";
                    string classic = $"{classicStart}iw_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        partData.partHash = classicHash;
                        partData.partName = classic;
                    }

                    //Set icon string
                    var iconStr = GetInnerwearIconString(finalIdIcon);
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        partData.iconHash = iconStr;
                        partData.iconName = GetInnerwearIconStringUnhashed(id.ToString());
                    }

                }
                string output = partData.GetLine();
                partData = null;

                //Decide which type this is
                if (id < 30000)
                {
                    outputInnerwearMale.Append(output);
                }
                else if (id < 40000)
                {
                    outputInnerwearFemale.Append(output);
                }
                else if (id < 200000)
                {
                    outputNGSInnerwearMale.Append(output);
                }
                else if (id < 300000)
                {
                    outputNGSInnerwearFemale.Append(output);
                }
                else
                {
                    Console.WriteLine("Unknown iw with id: " + id);
                }
            }
            WriteCSV(playerClassicDirOut, "MaleInnerwear.csv", outputInnerwearMale);
            WriteCSV(playerClassicDirOut, "FemaleInnerwear.csv", outputInnerwearFemale);
            WriteCSV(playerRebootDirOut, "MaleNGSInnerwear.csv", outputNGSInnerwearMale);
            WriteCSV(playerRebootDirOut, "FemaleNGSInnerwear.csv", outputNGSInnerwearFemale);

            //---------------------------Parse out cast arms
            StringBuilder outputCastArmMale = new StringBuilder();
            StringBuilder outputCastArmFemale = new StringBuilder();
            StringBuilder outputNGSCastArmMale = new StringBuilder();
            StringBuilder outputNGSCastArmFemale = new StringBuilder();

            outputCastArmMale.AppendLine(partColumns);
            outputCastArmFemale.AppendLine(partColumns);
            outputNGSCastArmMale.AppendLine(partColumns);
            outputNGSCastArmFemale.AppendLine(partColumns);

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "arm", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.carmDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                PartData partData = new PartData();
                foreach (var dict in nameDicts)
                {
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        partData.namesByLanguage.Add(str);
                    }
                }
                partData.id = id;

                //Double check these ids and use an adjustedId if needed
                int adjustedId = id;
                int linkedInnerId = -1;
                if (aquaCMX.carmDict.ContainsKey(id))
                {
                    linkedInnerId = aquaCMX.carmDict[id].body2.linkedInnerId;
                }
                if (aquaCMX.castArmIdLink.ContainsKey(id))
                {
                    adjustedId = aquaCMX.castArmIdLink[id].bcln.fileId;
                }
                partData.adjustedId = adjustedId;

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{rebootStart}am_{adjustedId}.ice";
                    string rebEx = $"{rebootExStart}am_{adjustedId}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);
                    string rebLinkedInner = $"{rebootStart}b1_{linkedInnerId}.ice";
                    string rebLinkedInnerEx = $"{rebootExStart}b1_{linkedInnerId}_ex.ice";
                    string rebLinkedInnerHash = GetFileHash(rebLinkedInner);
                    string rebLinkedInnerExHash = GetFileHash(rebLinkedInnerEx);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        partData.partHash = rebHash;
                        partData.partName = reb;
                    }
                    string iconStr = GetCastArmIconString(id.ToString());
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        partData.iconHash = iconStr;
                        partData.iconName = GetCastArmIconStringUnhashed(id.ToString());
                    }

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebExHash)))
                    {
                        partData.partExHash = rebExHash;
                        partData.partExName = rebEx;
                    }

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebLinkedInnerHash)))
                    {
                        partData.linkedInnerHash = rebLinkedInnerHash;
                        partData.linkedInnerName = rebLinkedInner;
                    }
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebLinkedInnerExHash)))
                    {
                        partData.linkedInnerExHash = rebLinkedInnerExHash;
                        partData.linkedInnerName = rebLinkedInnerEx;
                    }
                }
                else
                {
                    string finalId = $"{adjustedId:D5}";
                    string finalIdIcon = $"{id:D5}";
                    string classic = $"{classicStart}am_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        partData.partHash = classicHash;
                        partData.partName = classic;
                    }
                    //Set icon string
                    var iconStr = GetCastArmIconString(finalIdIcon);
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        partData.iconHash = iconStr;
                        partData.iconName = GetCastArmIconStringUnhashed(id.ToString());
                    }

                }
                string output = partData.GetLine();
                partData = null;

                //Decide which type this is
                if (id < 50000)
                {
                    outputCastArmMale.Append(output);
                }
                else if (id < 60000)
                {
                    outputCastArmFemale.Append(output);
                }
                else if (id < 400000)
                {
                    outputNGSCastArmMale.Append(output);
                }
                else if (id < 500000)
                {
                    outputNGSCastArmFemale.Append(output);
                }
                else
                {
                    Console.WriteLine("Unknown am with id: " + id);
                }
            }
            WriteCSV(playerClassicDirOut, "CastArms.csv", outputCastArmMale);
            WriteCSV(playerClassicDirOut, "CasealArms.csv", outputCastArmFemale);
            WriteCSV(playerRebootDirOut, "CastArmsNGS.csv", outputNGSCastArmMale);
            WriteCSV(playerRebootDirOut, "CasealArmsNGS.csv", outputNGSCastArmFemale);

            //---------------------------Parse out cast legs
            StringBuilder outputCastLegMale = new StringBuilder();
            StringBuilder outputCastLegFemale = new StringBuilder();
            StringBuilder outputNGSCastLegMale = new StringBuilder();
            StringBuilder outputNGSCastLegFemale = new StringBuilder();

            outputCastLegMale.AppendLine(partColumns);
            outputCastLegFemale.AppendLine(partColumns);
            outputNGSCastLegMale.AppendLine(partColumns);
            outputNGSCastLegFemale.AppendLine(partColumns);

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "Leg", true); //Yeah for some reason this string starts capitalized while none of the others do... don't ask me.

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.clegDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                PartData partData = new PartData();
                foreach (var dict in nameDicts)
                {
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        partData.namesByLanguage.Add(str);
                    }
                }
                partData.id = id;

                //Double check these ids and use an adjustedId if needed
                int adjustedId = id; 
                int linkedInnerId = -1;
                int soundId = -1;
                if (aquaCMX.clegDict.ContainsKey(id))
                {
                    linkedInnerId = aquaCMX.clegDict[id].body2.linkedInnerId;
                    soundId = aquaCMX.clegDict[id].body2.costumeSoundId;
                }
                if (aquaCMX.clegIdLink.ContainsKey(id))
                {
                    adjustedId = aquaCMX.clegIdLink[id].bcln.fileId;
                }
                partData.adjustedId = adjustedId;

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{rebootStart}lg_{adjustedId}.ice";
                    string rebEx = $"{rebootExStart}lg_{adjustedId}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);
                    string rebLinkedInner = $"{rebootStart}b1_{linkedInnerId}.ice";
                    string rebLinkedInnerEx = $"{rebootExStart}b1_{linkedInnerId}_ex.ice";
                    string rebLinkedInnerHash = GetFileHash(rebLinkedInner);
                    string rebLinkedInnerExHash = GetFileHash(rebLinkedInnerEx);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        partData.partHash = rebHash;
                        partData.partName = reb;
                    }
                    string iconStr = GetCastLegIconString(id.ToString());
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        partData.iconHash = iconStr;
                        partData.iconName = GetCastLegIconStringUnhashed(id.ToString());
                    }

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebExHash)))
                    {
                        partData.partExHash = rebExHash;
                        partData.partExName = rebEx;
                    }

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebLinkedInnerHash)))
                    {
                        partData.linkedInnerHash = rebLinkedInnerHash;
                        partData.linkedInnerName = rebLinkedInner;
                    }
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebLinkedInnerExHash)))
                    {
                        partData.linkedInnerExHash = rebLinkedInnerExHash;
                        partData.linkedInnerName = rebLinkedInnerEx;
                    }
                    AddOutfitSound(partData, pso2_binDir, soundId);
                }
                else
                {
                    string finalId = $"{adjustedId:D5}";
                    string finalIdIcon = $"{id:D5}";
                    string classic = $"{classicStart}lg_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        partData.partHash = classicHash;
                        partData.partName = classic;
                    }
                    //Set icon string
                    var iconStr = GetCastLegIconString(finalIdIcon);
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        partData.iconHash = iconStr;
                        partData.iconName = GetCastLegIconStringUnhashed(id.ToString());
                    }
                    AddOutfitSound(partData, pso2_binDir, soundId);

                }
                string output = partData.GetLine();
                partData = null;

                //Decide which type this is
                if (id < 50000)
                {
                    outputCastLegMale.Append(output);
                }
                else if (id < 60000)
                {
                    outputCastLegFemale.Append(output);
                }
                else if (id < 400000)
                {
                    outputNGSCastLegMale.Append(output);
                }
                else if (id < 500000)
                {
                    outputNGSCastLegFemale.Append(output);
                }
                else
                {
                    Console.WriteLine("Unknown lg with id: " + id);
                }
            }
            WriteCSV(playerClassicDirOut, "CastLegs.csv", outputCastLegMale);
            WriteCSV(playerClassicDirOut, "CasealLegs.csv", outputCastLegFemale);
            WriteCSV(playerRebootDirOut, "CastLegsNGS.csv", outputNGSCastLegMale);
            WriteCSV(playerRebootDirOut, "CasealLegsNGS.csv", outputNGSCastLegFemale);

            //---------------------------Parse out body paint
            StringBuilder outputMaleBodyPaint = new StringBuilder();
            StringBuilder outputFemaleBodyPaint = new StringBuilder();
            StringBuilder outputMaleLayeredBodyPaint = new StringBuilder();
            StringBuilder outputFemaleLayeredBodyPaint = new StringBuilder();
            StringBuilder outputNGSMaleBodyPaint = new StringBuilder();
            StringBuilder outputNGSFemaleBodyPaint = new StringBuilder();
            StringBuilder outputNGSCastMaleBodyPaint = new StringBuilder();
            StringBuilder outputNGSCastFemaleBodyPaint = new StringBuilder();
            StringBuilder outputNGSGenderlessBodyPaint = new StringBuilder();

            outputMaleBodyPaint.AppendLine(partColumns);
            outputFemaleBodyPaint.AppendLine(partColumns);
            outputMaleLayeredBodyPaint.AppendLine(partColumns);
            outputFemaleLayeredBodyPaint.AppendLine(partColumns);
            outputNGSMaleBodyPaint.AppendLine(partColumns);
            outputNGSFemaleBodyPaint.AppendLine(partColumns);
            outputNGSCastMaleBodyPaint.AppendLine(partColumns);
            outputNGSCastFemaleBodyPaint.AppendLine(partColumns);
            outputNGSGenderlessBodyPaint.AppendLine(partColumns);

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "bodypaint1", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.bodyPaintDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                PartData partData = new PartData();
                foreach (var dict in nameDicts)
                {
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        partData.namesByLanguage.Add(str);
                    }
                }
                partData.id = id;

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{rebootStart}b1_{id}.ice";
                    string rebEx = $"{rebootExStart}b1_{id}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        partData.partHash = rebHash;
                        partData.partName = reb;
                    }
                    //Set icon string
                    var iconStr = GetBodyPaintIconString(id.ToString());
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        partData.iconHash = iconStr;
                        partData.iconName = GetBodyPaintIconStringUnhashed(id.ToString());
                    }

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebExHash)))
                    {
                        partData.partExHash = rebExHash;
                        partData.partExName = rebEx;
                    }

                }
                else
                {
                    string finalId = $"{id:D5}";
                    string classic = $"{classicStart}b1_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        partData.partHash = classicHash;
                        partData.partName = classic;
                    }
                    //Set icon string
                    var iconStr = GetBodyPaintIconString(id.ToString());
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        partData.iconHash = iconStr;
                        partData.iconName = GetBodyPaintIconStringUnhashed(id.ToString());
                    }

                }
                string output = partData.GetLine();
                partData = null;

                //Decide which type this is
                if (id < 10000)
                {
                    outputMaleBodyPaint.Append(output);
                }
                else if (id < 20000)
                {
                    outputFemaleBodyPaint.Append(output);
                }
                else if (id < 30000)
                {
                    outputMaleLayeredBodyPaint.Append(output);
                }
                else if (id < 100000)
                {
                    outputFemaleLayeredBodyPaint.Append(output);
                }
                else if (id < 200000)
                {
                    outputNGSMaleBodyPaint.Append(output);
                }
                else if (id < 300000)
                {
                    outputNGSFemaleBodyPaint.Append(output);
                }
                else if (id < 400000)
                {
                    outputNGSCastMaleBodyPaint.Append(output);
                }
                else if (id < 500000)
                {
                    outputNGSCastFemaleBodyPaint.Append(output);
                }
                else if (id < 600000)
                {
                    outputNGSGenderlessBodyPaint.Append(output);
                }
                else
                {
                    Console.WriteLine("Unknown b1 with id: " + id);
                }
            }
            WriteCSV(playerClassicDirOut, "MaleBodyPaint.csv", outputMaleBodyPaint);
            WriteCSV(playerClassicDirOut, "FemaleBodyPaint.csv", outputFemaleBodyPaint);
            WriteCSV(playerClassicDirOut, "MaleLayeredBodyPaint.csv", outputMaleLayeredBodyPaint);
            WriteCSV(playerClassicDirOut, "FemaleLayeredBodyPaint.csv", outputFemaleLayeredBodyPaint);
            WriteCSV(playerRebootDirOut, "MaleNGSBodyPaint.csv", outputNGSMaleBodyPaint);
            WriteCSV(playerRebootDirOut, "FemaleNGSBodyPaint.csv", outputNGSFemaleBodyPaint);
            WriteCSV(playerRebootDirOut, "CastNGSBodyPaint.csv", outputNGSCastMaleBodyPaint);
            WriteCSV(playerRebootDirOut, "CasealNGSBodyPaint.csv", outputNGSCastFemaleBodyPaint);
            WriteCSV(playerRebootDirOut, "GenderlessNGSBodyPaint.csv", outputNGSGenderlessBodyPaint);

            //---------------------------Parse out stickers
            StringBuilder outputStickers = new StringBuilder();

            outputStickers.AppendLine(partColumns);

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "bodypaint2", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.stickerDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                PartData partData = new PartData();
                foreach (var dict in nameDicts)
                {
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        partData.namesByLanguage.Add(str);
                    }
                }
                partData.id = id;

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{rebootStart}b2_{id}.ice";
                    string rebEx = $"{rebootExStart}b2_{id}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        partData.partHash = rebHash;
                        partData.partName = reb;
                    }

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebExHash)))
                    {
                        partData.partExHash = rebExHash;
                        partData.partExName = rebEx;
                    }

                    //Set icon string
                    var iconStr = GetFileHash(icon + stickerIcon + id + ".ice");
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        partData.iconHash = iconStr;
                        partData.iconName = GetStickerIconStringUnhashed(id.ToString());
                    }

                }
                else
                {
                    string finalId = $"{id:D5}";
                    string classic = $"{classicStart}b2_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        partData.partHash = classicHash;
                        partData.partName = classic;
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + stickerIcon + finalId + ".ice");
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        partData.iconHash = iconStr;
                        partData.iconName = GetStickerIconStringUnhashed(id.ToString());
                    }

                }

                string output = partData.GetLine();
                partData = null;
                outputStickers.Append(output);
            }
            WriteCSV(playerDirOut, "Stickers.csv", outputStickers);

            //---------------------------Parse out hair
            StringBuilder outputMaleHair = new StringBuilder();
            StringBuilder outputFemaleHair = new StringBuilder();
            StringBuilder outputCasealHair = new StringBuilder();
            StringBuilder outputNGSHair = new StringBuilder();

            outputMaleHair.AppendLine(hairColumns);
            outputFemaleHair.AppendLine(hairColumns);
            outputCasealHair.AppendLine(hairColumns);
            outputNGSHair.AppendLine(hairColumns);

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "hair", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.hairDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                PartData partData = new PartData();
                foreach (var dict in nameDicts)
                {
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        partData.namesByLanguage.Add(str);
                    }
                }
                partData.id = id;

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{rebootStart}hr_{id}.ice";
                    string rebEx = $"{rebootExStart}hr_{id}_ex.ice";

                    //Cast heads
                    if (id >= 300000 && id < 500000)
                    {
                        reb = reb.Replace("hr", "fc");
                        rebEx = rebEx.Replace("hr", "fc");
                    }
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        partData.partHash = rebHash;
                        partData.partName = reb;
                    }

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebExHash)))
                    {
                        partData.partExHash = rebExHash;
                        partData.partExName = rebEx;
                    }

                    //Set icon string
                    string maleIconStr;
                    string femaleIconStr;
                    string castIconStr;

                    //Cast heads
                    if (id >= 300000 && id < 500000)
                    {
                        maleIconStr = "_";
                        femaleIconStr = "_";
                        castIconStr = icon + faceIcon + id + ".ice";
                    }
                    else
                    {
                        maleIconStr = icon + hairIcon + iconMale + id + ".ice";
                        femaleIconStr = icon + hairIcon + iconFemale + id + ".ice";
                        castIconStr = icon + hairIcon + iconCast + id + ".ice";
                    }
                    var maleIconStrHash = GetFileHash(maleIconStr);
                    var femaleIconStrHash = GetFileHash(femaleIconStr);
                    var castIconStrHash = GetFileHash(castIconStr);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, maleIconStrHash)))
                    {
                        partData.iconHash = maleIconStrHash;
                        partData.iconName = maleIconStr;
                    }
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, femaleIconStrHash)))
                    {
                        partData.iconOuterHash = femaleIconStrHash;
                        partData.iconOuterName = femaleIconStr;
                    }
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, castIconStrHash)))
                    {
                        partData.iconCastHash = castIconStrHash;
                        partData.iconCastName = castIconStr;
                    }
                }
                else
                {
                    string finalId = $"{id:D5}";
                    string classic = $"{classicStart}hr_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        partData.partHash = classicHash;
                        partData.partName = classic;
                    }
                    //Set icon string
                    var maleIconStr = icon + hairIcon + iconMale + id + ".ice";
                    var femaleIconStr = icon + hairIcon + iconFemale + id + ".ice";
                    var castIconStr = icon + hairIcon + iconCast + id + ".ice";
                    var maleIconStrHash = GetFileHash(maleIconStr);
                    var femaleIconStrHash = GetFileHash(femaleIconStr);
                    var castIconStrHash = GetFileHash(castIconStr);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, maleIconStrHash)))
                    {
                        partData.iconHash = maleIconStrHash;
                        partData.iconName = maleIconStr;
                    }
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, femaleIconStrHash)))
                    {
                        partData.iconOuterHash = femaleIconStrHash;
                        partData.iconOuterName = femaleIconStr;
                    }
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, castIconStrHash)))
                    {
                        partData.iconCastHash = castIconStrHash;
                        partData.iconCastName = castIconStr;

                    }
                }
                string output = partData.GetLineAllIcons();
                partData = null;

                //Decide which type this is
                if (id < 10000)
                {
                    outputMaleHair.Append(output);
                }
                else if (id < 20000)
                {
                    outputFemaleHair.Append(output);
                }
                else if (id < 60000)
                {
                    outputCasealHair.Append(output);
                }
                else
                {
                    outputNGSHair.Append(output);
                }
            }
            WriteCSV(playerClassicDirOut, "MaleHair.csv", outputMaleHair);
            WriteCSV(playerClassicDirOut, "FemaleHair.csv", outputFemaleHair);
            WriteCSV(playerClassicDirOut, "CasealHair.csv", outputCasealHair);
            WriteCSV(playerRebootDirOut, "AllHairNGS.csv", outputNGSHair);

            //---------------------------Parse out Eye
            StringBuilder outputEyes = new StringBuilder();
            StringBuilder outputNGSEyes = new StringBuilder();

            outputEyes.AppendLine(partColumns);
            outputNGSEyes.AppendLine(partColumns);

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "eye", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.eyeDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                PartData partData = new PartData();
                foreach (var dict in nameDicts)
                {
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        partData.namesByLanguage.Add(str);
                    }
                }
                partData.id = id;

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{rebootStart}ey_{id}.ice";
                    string rebEx = $"{rebootExStart}ey_{id}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        partData.partHash = rebHash;
                        partData.partName = reb;
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + eyeIcon + id + ".ice");
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        partData.iconHash = iconStr;
                        partData.iconName = GetEyeIconStringUnhashed(id.ToString());
                    }
                }
                else
                {
                    string finalId = $"{id:D5}";
                    string classic = $"{classicStart}ey_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        partData.partHash = classicHash;
                        partData.partName = classic;
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + eyeIcon + finalId + ".ice");
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        partData.iconHash = iconStr;
                        partData.iconName = GetEyeIconStringUnhashed(id.ToString());
                    }

                }
                string output = partData.GetLine();
                partData = null;

                if (id < 100000)
                {
                    outputEyes.Append(output);
                }
                else
                {
                    outputNGSEyes.Append(output);
                }
            }
            WriteCSV(playerClassicDirOut, "Eyes.csv", outputEyes);
            WriteCSV(playerRebootDirOut, "EyesNGS.csv", outputNGSEyes);

            //---------------------------Parse out EYEB
            StringBuilder outputEyebrows = new StringBuilder();
            StringBuilder outputNGSEyebrows = new StringBuilder();

            outputEyebrows.AppendLine(partColumns);
            outputNGSEyebrows.AppendLine(partColumns);

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "eyebrows", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.stickerDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                PartData partData = new PartData();
                foreach (var dict in nameDicts)
                {
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        partData.namesByLanguage.Add(str);
                    }
                }
                partData.id = id;

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{rebootStart}eb_{id}.ice";
                    string rebEx = $"{rebootExStart}eb_{id}_ex.ice";
                    string rebHash = GetFileHash(reb);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        partData.partHash = rebHash;
                        partData.partName = reb;
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + eyeBrowsIcon + id + ".ice");
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        partData.iconHash = iconStr;
                        partData.iconName = GetEyebrowsIconStringUnhashed(id.ToString());
                    }
                }
                else
                {
                    string finalId = $"{id:D5}";
                    string classic = $"{classicStart}eb_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        partData.partHash = classicHash;
                        partData.partName = classic;
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + eyeBrowsIcon + finalId + ".ice");
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        partData.iconHash = iconStr;
                        partData.iconName = GetEyebrowsIconStringUnhashed(id.ToString());
                    }

                }
                string output = partData.GetLine();
                partData = null;

                if (id <= 100000)
                {
                    outputEyebrows.Append(output);
                }
                else
                {
                    outputNGSEyebrows.Append(output);
                }
            }
            WriteCSV(playerClassicDirOut, "Eyebrows.csv", outputEyebrows);
            WriteCSV(playerRebootDirOut, "EyebrowsNGS.csv", outputNGSEyebrows);

            //---------------------------Parse out EYEL
            StringBuilder outputEyelashes = new StringBuilder();
            StringBuilder outputNGSEyelashes = new StringBuilder();

            outputEyelashes.AppendLine(partColumns);
            outputNGSEyelashes.AppendLine(partColumns);

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "eyelashes", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.stickerDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                PartData partData = new PartData();
                foreach (var dict in nameDicts)
                {
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        partData.namesByLanguage.Add(str);
                    }
                }
                partData.id = id;

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{rebootStart}el_{id}.ice";
                    string rebEx = $"{rebootExStart}el_{id}_ex.ice";
                    string rebHash = GetFileHash(reb);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        partData.partHash = rebHash;
                        partData.partName = reb;
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + eyelashesIcon + id + ".ice");
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        partData.iconHash = iconStr;
                        partData.iconName = GetEyelashesIconStringUnhashed(id.ToString());
                    }
                }
                else
                {
                    string finalId = $"{id:D5}";
                    string classic = $"{classicStart}el_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        partData.partHash = classicHash;
                        partData.partName = classic;
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + eyelashesIcon + finalId + ".ice");
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        partData.iconHash = iconStr;
                        partData.iconName = GetEyelashesIconStringUnhashed(id.ToString());
                    }

                }
                string output = partData.GetLine();
                partData = null;

                if (id <= 100000)
                {
                    outputEyelashes.Append(output);
                }
                else
                {
                    outputNGSEyelashes.Append(output);
                }
            }
            WriteCSV(playerClassicDirOut, "Eyelashes.csv", outputEyelashes);
            WriteCSV(playerRebootDirOut, "EyelashesNGS.csv", outputNGSEyelashes);

            //---------------------------Parse out ACCE //Stored under decoy in a99be286e3a7e1b45d88a3ea4d6c18c4
            StringBuilder outputAccessories = new StringBuilder();
            outputAccessories.AppendLine(acceColumns);

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "decoy", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.accessoryDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                string output = "";
                bool named = false;
                for (int i = 0; i < nameDicts.Count && i < 2; i++)
                {
                    var dict = nameDicts[i];
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        named = true;
                        output += str + ",";
                    }
                    else
                    {
                        output += ",";
                    }
                }
                output += $"{id},";

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    output = $"[Unnamed {id}]" + output;
                }

                //Double check these ids and use an adjustedId if needed
                int adjustedId = id;
                if (aquaCMX.accessoryIdLink.ContainsKey(id))
                {
                    adjustedId = aquaCMX.accessoryIdLink[id].bcln.fileId;
                }

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{rebootStart}ac_{adjustedId}.ice";
                    string rebEx = $"{rebootExStart}ac_{adjustedId}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    //Set icon string
                    var iconStr = GetFileHash(icon + accessoryIcon + id + ".ice");
                    output += iconStr + ",";

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        output += rebHash;
                    }
                    output += ",";
                }
                else
                {
                    string finalId = $"{id:D5}";
                    string classic = $"{classicStart}ac_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    //Set icon string
                    var iconStr = GetFileHash(icon + accessoryIcon + finalId + ".ice");
                    output += iconStr + ",";
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        output += $"{classicHash}";
                    }
                    output += ",";
                }

                //Add linked character nodes
                if (aquaCMX.accessoryDict.ContainsKey(id))
                {
                    var acce = aquaCMX.accessoryDict[id];

                    output += $",{acce.nodeAttach1},{acce.nodeAttach2},{acce.nodeAttach3},{acce.nodeAttach4},{acce.nodeAttach5}," +
                        $"{acce.nodeAttach6},{acce.nodeAttach7},{acce.nodeAttach8},{acce.nodeAttach9},{acce.nodeAttach10}," +
                        $"{acce.nodeAttach11},{acce.nodeAttach12},{acce.nodeAttach13},{acce.nodeAttach14},{acce.nodeAttach15},{acce.nodeAttach16}" +
                        $",{acce.effectName}";
                }

                output += "\n";

                outputAccessories.Append(output);
            }
            WriteCSV(playerDirOut, "Accessories.csv", outputAccessories);

            //---------------------------Parse out skin
            StringBuilder outputSkin = new StringBuilder();
            StringBuilder outputNGSSkin = new StringBuilder();
            outputSkin.AppendLine(partColumns);
            outputNGSSkin.AppendLine(partColumns);

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "skin", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.ngsSkinDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                PartData partData = new PartData();
                foreach (var dict in nameDicts)
                {
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        partData.namesByLanguage.Add(str);
                    }
                }
                partData.id = id;

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{rebootStart}sk_{id}.ice";
                    string rebEx = $"{rebootExStart}sk_{id}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        partData.partHash = rebHash;
                        partData.partName = reb;
                    }

                    //Set icon string
                    string iconStrTest = icon + skinIcon + GetIconGender(id) + id + ".ice";
                    var iconStr = GetFileHash(icon + skinIcon + GetIconGender(id) + id + ".ice");
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        partData.iconHash = iconStr;
                        partData.iconName = GetSkinIconStringUnhashed(id.ToString());
                    }

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebExHash)))
                    {
                        partData.partExHash = rebExHash;
                        partData.partExName = rebEx;
                    }

                }
                else
                {
                    string finalId = $"{id:D5}";
                    string classic = $"{classicStart}sk_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        partData.partHash = classicHash;
                        partData.partName = classic;
                    }

                    //Set icon string
                    var iconStr = GetFileHash(icon + skinIcon + GetIconGender(id) + id + ".ice");
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        partData.iconHash = iconStr;
                        partData.iconName = GetSkinIconStringUnhashed(id.ToString());
                    }

                }
                string output = partData.GetLine();
                partData = null;

                if (id < 100000)
                {
                    outputSkin.Append(output);
                }
                else
                {
                    outputNGSSkin.Append(output);
                }
            }
            WriteCSV(playerClassicDirOut, "Skins.csv", outputSkin);
            WriteCSV(playerRebootDirOut, "SkinsNGS.csv", outputNGSSkin);

            //---------------------------Parse out FCP1, Face Textures
            StringBuilder outputFCP1 = new StringBuilder();
            StringBuilder outputNGSFCP1 = new StringBuilder();
            outputFCP1.AppendLine(partColumns);
            outputNGSFCP1.AppendLine(partColumns);

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "facepaint1", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.fcpDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                PartData partData = new PartData();
                foreach (var dict in nameDicts)
                {
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        partData.namesByLanguage.Add(str);
                    }
                }
                partData.id = id;

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{rebootStart}f1_{id}.ice";
                    string rebEx = $"{rebootExStart}f1_{id}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        partData.partHash = rebHash;
                        partData.partName = reb;
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + faceIcon + id + ".ice");
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        partData.iconHash = iconStr;
                        partData.iconName = GetFacePaintIconStringUnhashed(id.ToString());
                    }

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebExHash)))
                    {
                        partData.partExHash = rebExHash;
                        partData.partExName = rebEx;
                    }

                }
                else
                {
                    string finalId = $"{id:D5}";
                    string classic = $"{classicStart}f1_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        partData.partHash = classicHash;
                        partData.partName = classic;
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + faceIcon + finalId + ".ice");
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        partData.iconHash = iconStr;
                        partData.iconName = GetFacePaintIconStringUnhashed(id.ToString());
                    }

                }
                string output = partData.GetLine();
                partData = null;

                if (id <= 100000)
                {
                    outputFCP1.Append(output);
                }
                else
                {
                    outputNGSFCP1.Append(output);
                }
            }
            WriteCSV(playerClassicDirOut, "FaceTextures.csv", outputFCP1);
            //Textures should just be with the model for these; no f1 ices in making_reboot
            /*
            if (outputNGSFCP1.Length > 0)
            {
                WriteCSV(outputDirectory, "FaceTexturesNGS.csv", outputNGSFCP1);
            }*/

            //---------------------------Parse out FCP2
            StringBuilder outputFCP2 = new StringBuilder();
            StringBuilder outputNGSFCP2 = new StringBuilder();
            outputFCP2.AppendLine(partColumns);
            outputNGSFCP2.AppendLine(partColumns);

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "facepaint2", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.fcpDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                PartData partData = new PartData();
                foreach (var dict in nameDicts)
                {
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        partData.namesByLanguage.Add(str);
                    }
                }
                partData.id = id;

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{rebootStart}f2_{id}.ice";
                    string rebEx = $"{rebootExStart}f2_{id}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        partData.partHash = rebHash;
                        partData.partName = reb;
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + facePaintIcon + id + ".ice");
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        partData.iconHash = iconStr;
                        partData.iconName = GetFacePaintIconStringUnhashed(id.ToString());
                    }

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebExHash)))
                    {
                        partData.partExHash = rebExHash;
                        partData.partExName = rebEx;
                    }

                }
                else
                {
                    string finalId = $"{id:D5}";
                    string classic = $"{classicStart}f2_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        partData.partHash = classicHash;
                        partData.partName = classic;
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + facePaintIcon + finalId + ".ice");
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        partData.iconHash = iconStr;
                        partData.iconName = GetFacePaintIconStringUnhashed(id.ToString());
                    }

                }
                string output = partData.GetLine();
                partData = null;

                if (id <= 100000)
                {
                    outputFCP2.Append(output);
                }
                else
                {
                    outputNGSFCP2.Append(output);
                }
            }
            WriteCSV(playerClassicDirOut, "FacePaint.csv", outputFCP2);
            if (outputNGSFCP2.Length > 0)
            {
                WriteCSV(playerRebootDirOut, "FacePaintNGS.csv", outputNGSFCP2);
            }

            //---------------------------Parse out FACE //face_variation.cmp.lua in 75b1632526cd6a1039625349df6ee8dd used to map file face ids to .text ids
            //This targets facevariations specifically. face seems to be redundant and not actually particularly useful at a glance.
            StringBuilder outputHumanMaleFace = new StringBuilder();
            StringBuilder outputHumanFemaleFace = new StringBuilder();
            StringBuilder outputNewmanMaleFace = new StringBuilder();
            StringBuilder outputNewmanFemaleFace = new StringBuilder();
            StringBuilder outputCastMaleFace = new StringBuilder();
            StringBuilder outputCastFemaleFace = new StringBuilder();
            StringBuilder outputDewmanMaleFace = new StringBuilder();
            StringBuilder outputDewmanFemaleFace = new StringBuilder();
            StringBuilder outputNGSFace = new StringBuilder();

            outputHumanMaleFace.AppendLine(partColumns);
            outputHumanFemaleFace.AppendLine(partColumns);
            outputNewmanMaleFace.AppendLine(partColumns);
            outputNewmanFemaleFace.AppendLine(partColumns);
            outputCastMaleFace.AppendLine(partColumns);
            outputCastFemaleFace.AppendLine(partColumns);
            outputDewmanMaleFace.AppendLine(partColumns);
            outputDewmanFemaleFace.AppendLine(partColumns);
            outputNGSFace.AppendLine(partColumns);

            masterIdList.Clear();
            nameDicts.Clear();

            masterNameList = new List<string>();
            strNameDicts = new List<Dictionary<string, string>>();
            GatherTextIdsStringRef(textByCat, masterNameList, strNameDicts, "facevariation", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.faceDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                PartData partData = new PartData();

                string realId = "";
                if (!faceIds.TryGetValue(id, out realId))
                {
                    realId = "No" + id;
                }


                foreach (var dict in strNameDicts)
                {
                    if (dict.TryGetValue(realId, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        partData.namesByLanguage.Add(str);
                    }
                }
                partData.id = id;

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{rebootStart}fc_{id}.ice";
                    string rebEx = $"{rebootExStart}fc_{id}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        partData.partHash = rebHash;
                        partData.partName = reb;
                    }

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebExHash)))
                    {
                        partData.partExHash = rebExHash;
                        partData.partExName = rebEx;
                    }

                }
                else
                {
                    string finalId = $"{id:D5}";
                    string classic = $"{classicStart}fc_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        partData.partHash = classicHash;
                        partData.partName = classic;
                    }

                }
                string output = partData.GetLine();
                partData = null;

                if (id < 10000)
                {
                    outputHumanMaleFace.Append(output);
                }
                else if (id < 20000)
                {
                    outputHumanFemaleFace.Append(output);
                }
                else if (id < 30000)
                {
                    outputNewmanMaleFace.Append(output);
                }
                else if (id < 40000)
                {
                    outputNewmanFemaleFace.Append(output);
                }
                else if (id < 50000)
                {
                    outputCastMaleFace.Append(output);
                }
                else if (id < 60000)
                {
                    outputCastFemaleFace.Append(output);
                }
                else if (id < 70000)
                {
                    outputDewmanMaleFace.Append(output);
                }
                else if (id < 100000)
                {
                    outputDewmanFemaleFace.Append(output);
                }
                else
                {
                    outputNGSFace.Append(output);
                }
            }
            WriteCSV(playerClassicDirOut, "MaleHumanFaces.csv", outputHumanMaleFace);
            WriteCSV(playerClassicDirOut, "FemaleHumanFaces.csv", outputHumanFemaleFace);
            WriteCSV(playerClassicDirOut, "MaleNewmanFaces.csv", outputNewmanMaleFace);
            WriteCSV(playerClassicDirOut, "FemaleNewmanFaces.csv", outputNewmanFemaleFace);
            WriteCSV(playerClassicDirOut, "CastFaces_Heads.csv", outputCastMaleFace);
            WriteCSV(playerClassicDirOut, "CasealFaces_Heads.csv", outputCastFemaleFace);
            WriteCSV(playerClassicDirOut, "MaleDeumanFaces.csv", outputDewmanMaleFace);
            WriteCSV(playerClassicDirOut, "FemaleDeumanFaces.csv", outputDewmanFemaleFace);
            WriteCSV(playerRebootDirOut, "AllFacesNGS.csv", outputNGSFace);

            //---------------------------Parse out Face motions
            StringBuilder outputFcmn = new StringBuilder();
            StringBuilder outputFcmnNGS = new StringBuilder();
            outputFcmn.AppendLine(partColumns);
            outputFcmnNGS.AppendLine(partColumns);

            masterIdList.Clear();
            nameDicts.Clear();

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.fcmnDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                PartData partData = new PartData();
                foreach (var dict in nameDicts)
                {
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        partData.namesByLanguage.Add(str);
                    }
                }
                partData.id = id;

                //Decide if it needs to be handled as a reboot file or not
                string typeString = "fm_";
                if (id >= 100000)
                {
                    var partName = $"{rebootStart}{typeString}{id}.ice";
                    var partExName = $"{rebootExStart}{typeString}{id}_ex.ice";
                    var partHash = GetFileHash(partName);
                    var partExHash = GetFileHash(partExName);
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, partHash)))
                    {
                        partData.partHash = partHash;
                        partData.partName = partName;
                    }
                }
                else
                {
                    string finalId = $"{id:D5}";
                    var partName = $"{classicStart}{typeString}{finalId}.ice";
                    var partHash = GetFileHash(partName);
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, partHash)))
                    {
                        partData.partHash = partHash;
                        partData.partName = partName;
                    }
                }
                string output = partData.GetLine();
                partData = null;

                if (id < 100000)
                {
                    outputFcmn.AppendLine(output);
                }
                else
                {
                    outputFcmnNGS.AppendLine(output);
                }
            }
            WriteCSV(playerClassicDirOut, "FaceMotions.csv", outputFcmn);
            WriteCSV(playerRebootDirOut, "FaceMotionsNGS.csv", outputFcmnNGS);

            //---------------------------Parse out NGS ears //The cmx has ear data, but no ids. Maybe it's done by order? Same for teeth and horns
            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "ears", true);

            if (aquaCMX.ngsEarDict.Count > 0 || masterIdList.Count > 0)
            {
                StringBuilder outputNGSEars = new StringBuilder();
                outputNGSEars.AppendLine(partColumns);

                //Add potential cmx ids that wouldn't be stored in
                GatherDictKeys(masterIdList, aquaCMX.ngsEarDict.Keys);

                masterIdList.Sort();

                //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
                foreach (int id in masterIdList)
                {
                    PartData partData = new PartData();
                    foreach (var dict in nameDicts)
                    {
                        if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                        {
                            partData.namesByLanguage.Add(str);
                        }
                    }
                    partData.id = id;

                    //Decide if it needs to be handled as a reboot file or not
                    if (id >= 100000)
                    {
                        string reb = $"{rebootStart}ea_{id}.ice";
                        string rebEx = $"{rebootExStart}ea_{id}_ex.ice";
                        string rebHash = GetFileHash(reb);

                        if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                        {
                            partData.partHash = rebHash;
                            partData.partName = reb;
                        }
                        //Set icon string
                        var iconStr = GetFileHash(icon + earIcon + id + ".ice");
                        if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                        {
                            partData.iconHash = iconStr;
                            partData.iconName = GetEarIconStringUnhashed(id.ToString());
                        }

                    }
                    else
                    {
                        string finalId = $"{id:D5}";
                        string classic = $"{classicStart}ea_{finalId}.ice";

                        var classicHash = GetFileHash(classic);

                        if (File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                        {
                            partData.partHash = classicHash;
                            partData.partName = classic;
                        }
                        //Set icon string
                        var iconStr = GetFileHash(icon + earIcon + finalId + ".ice");
                        if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                        {
                            partData.iconHash = iconStr;
                            partData.iconName = GetEarIconStringUnhashed(id.ToString());
                        }

                    }
                    string output = partData.GetLine();
                    partData = null;

                    outputNGSEars.Append(output);

                }
                WriteCSV(playerRebootDirOut, "EarsNGS.csv", outputNGSEars);
            }

            //---------------------------Parse out NGS teeth 
            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "dental", true);

            if (aquaCMX.ngsTeethDict.Count > 0 || masterIdList.Count > 0)
            {
                StringBuilder outputNGSTeeth = new StringBuilder();
                outputNGSTeeth.AppendLine(partColumns);

                //Add potential cmx ids that wouldn't be stored in
                GatherDictKeys(masterIdList, aquaCMX.ngsTeethDict.Keys);

                masterIdList.Sort();

                //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
                foreach (int id in masterIdList)
                {
                    PartData partData = new PartData();
                    foreach (var dict in nameDicts)
                    {
                        if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                        {
                            partData.namesByLanguage.Add(str);
                        }
                    }
                    partData.id = id;

                    //Decide if it needs to be handled as a reboot file or not
                    if (id >= 100000)
                    {
                        string reb = $"{rebootStart}de_{id}.ice";
                        string rebEx = $"{rebootExStart}de_{id}_ex.ice";
                        string rebHash = GetFileHash(reb);
                        string rebExHash = GetFileHash(rebEx);

                        if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                        {
                            partData.partHash = rebHash;
                            partData.partName = reb;
                        }
                        //Set icon string
                        var iconStr = GetFileHash(icon + teethIcon + id + ".ice");
                        if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                        {
                            partData.iconHash = iconStr;
                            partData.iconName = GetTeethIconStringUnhashed(id.ToString());
                        }
                    }
                    else
                    {
                        string finalId = $"{id:D5}";
                        string classic = $"{classicStart}de_{finalId}.ice";

                        var classicHash = GetFileHash(classic);

                        if (File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                        {
                            partData.partHash = classicHash;
                            partData.partName = classic;
                        }
                        //Set icon string
                        var iconStr = GetFileHash(icon + teethIcon + finalId + ".ice");
                        if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                        {
                            partData.iconHash = iconStr;
                            partData.iconName = GetTeethIconStringUnhashed(id.ToString());
                        }

                    }
                    string output = partData.GetLine();
                    partData = null;

                    outputNGSTeeth.Append(output);

                }
                WriteCSV(playerRebootDirOut, "TeethNGS.csv", outputNGSTeeth);
            }

            //---------------------------Parse out NGS horns 
            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "horn", true);

            if (aquaCMX.ngsHornDict.Count > 0 || masterIdList.Count > 0)
            {
                StringBuilder outputNGSHorns = new StringBuilder();
                outputNGSHorns.AppendLine(partColumns);

                //Add potential cmx ids that wouldn't be stored in
                GatherDictKeys(masterIdList, aquaCMX.ngsHornDict.Keys);

                masterIdList.Sort();

                //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
                foreach (int id in masterIdList)
                {
                    //Skip the なし horn entry. I'm not even sure why that's in there.
                    if (id == 0)
                    {
                        continue;
                    }
                    PartData partData = new PartData();
                    foreach (var dict in nameDicts)
                    {
                        if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                        {
                            partData.namesByLanguage.Add(str);
                        }
                    }
                    partData.id = id;

                    //Decide if it needs to be handled as a reboot file or not
                    if (id >= 100000)
                    {
                        string reb = $"{rebootStart}hn_{id}.ice";
                        string rebEx = $"{rebootExStart}hn_{id}_ex.ice";
                        string rebHash = GetFileHash(reb);
                        string rebExHash = GetFileHash(rebEx);

                        if (File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                        {
                            partData.partHash = rebHash;
                            partData.partName = reb;
                        }
                        //Set icon string
                        var iconStr = GetFileHash(icon + hornIcon + id + ".ice");
                        if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                        {
                            partData.iconHash = iconStr;
                            partData.iconName = GetHornIconStringUnhashed(id.ToString());
                        }
                    }
                    else
                    {
                        string finalId = $"{id:D5}";
                        string classic = $"{classicStart}hn_{finalId}.ice";
                        var classicHash = GetFileHash(classic);

                        if (File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                        {
                            partData.partHash = classicHash;
                            partData.partName = classic;
                        }
                        //Set icon string
                        var iconStr = GetFileHash(icon + hornIcon + finalId + ".ice");
                        if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                        {
                            partData.iconHash = iconStr;
                            partData.iconName = GetHornIconStringUnhashed(id.ToString());
                        }

                    }
                    string output = partData.GetLine();
                    partData = null;

                    outputNGSHorns.Append(output);

                }
                WriteCSV(playerRebootDirOut, "HornsNGS.csv", outputNGSHorns);
            }
            //---------------------------------------------------------------------------------------//End CMX related ids
        }

        public static void GenerateMagList(string pso2_binDir, string playerDirOut, List<int> magIds, List<int> magIdsReboot)
        {

            //---------------------------Generate Mag list

            if (magIds != null)
            {
                var magOut = new List<string>();

                foreach (var id in magIds)
                {
                    string names;
                    if (magNames.ContainsKey(id))
                    {
                        names = magNames[id] + ",";
                    }
                    else
                    {
                        names = $",Unknown {id},";
                    }

                    string mgFileName = magItem + $"{id:D5}" + ".ice";
                    string exists = "";

                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, GetFileHash(mgFileName))))
                    {
                        exists = ",(Not found)";
                    }

                    magOut.Add(names + mgFileName + "," + GetFileHash(mgFileName) + exists);
                }
                File.WriteAllLines(Path.Combine(playerDirOut, $"Mags.csv"), magOut, Encoding.UTF8);
            }

            //---------------------------Generate NGS Mag List

            if (magIdsReboot != null)
            {
                var magOut = new List<string>();

                foreach (var id in magIdsReboot)
                {
                    string names;
                    if (magNamesNGS.ContainsKey(id))
                    {
                        names = magNamesNGS[id] + ",";
                    }
                    else
                    {
                        names = $",Unknown {id},";
                    }

                    string mgFileName = magItem + $"{id:D5}" + ".ice";
                    string exists = "";

                    if (!File.Exists(Path.Combine(pso2_binDir, dataReboot, GetRebootHash(GetFileHash(mgFileName)))))
                    {
                        exists = ",(Not found)";
                    }

                    magOut.Add(names + mgFileName + "," + GetRebootHash(GetFileHash(mgFileName)) + exists);
                }
                File.WriteAllLines(Path.Combine(playerDirOut, $"MagsNGS.csv"), magOut, Encoding.UTF8);
            }
        }

        public static void GeneratePhotonBlastCreatureList(string playerClassicDirOut)
        {
            //---------------------------Generate Photon Blast Creature List
            var pbList = new List<string>();
            char letter = 'a';
            for (int i = 0; i < 5; i++)
            {
                string pbName = "";
                switch (i)
                {
                    case 0:
                        pbName = "ヘリクス,Helix,";
                        break;
                    case 1:
                        pbName = "アイアス,Ajax,";
                        break;
                    case 2:
                        pbName = "ケートス,Cetus,";
                        break;
                    case 3:
                        pbName = "ユリウス,Julius,";
                        break;
                    case 4:
                        pbName = "イリオス,Troy/Ilios,";
                        break;
                    default:
                        break;
                }
                pbList.Add(pbName + GetFileHash(pbCreatures + letter++ + ".ice"));
            }
            File.WriteAllLines(Path.Combine(playerClassicDirOut, "PhotonBlastCreatures.csv"), pbList, Encoding.UTF8);
        }

        public static void GenerateVehicle_SpecialWeaponList(string playerDirOut, string playerCAnimDirOut, string playerRAnimDirOut, List<string> genAnimList, List<string> genAnimListNGS)
        {
            //---------------------------Generate Dark Blast/Vehicle List
            var dbList = new List<string>();
            dbList.Add("A.I.S モデル,A.I.S Models," + GetFileHash(db_vehicle + "vc_robot_a.ice"));
            dbList.Add("A.I.S モーション,A.I.S Animations," + GetFileHash(db_vehicle + "vc_robot.ice"));
            dbList.Add("A.I.Sヴェガ モデル,A.I.S VEGAModels," + GetFileHash(db_vehicle + "vc_robot_armd_a.ice"));
            dbList.Add("A.I.Sヴェガ モーション,A.I.S Vega Animations," + GetFileHash(db_vehicle + "vc_robot_armd.ice"));
            dbList.Add("ライドロイド,Rideroid," + GetFileHash(db_vehicle + "vc_rideroid.ice"));
            dbList.Add("ダークブラスト　エフェクト,Dark Blast Effect," + GetFileHash(db_vehicle + "dh_effect_common.ice"));
            for (int i = 1; i < 5; i++)
            {
                string dbJP = "";
                string dbNA = "";
                string dbInternal = "";
                switch (i)
                {
                    case 1:
                        dbJP = "エルダー";
                        dbNA = "Elder";
                        dbInternal = "ak";
                        break;
                    case 2:
                        dbJP = "ルーサー";
                        dbNA = "Loser";
                        dbInternal = "te";
                        break;
                    case 3:
                        dbJP = "アプレンティス";
                        dbNA = "Apprentice";
                        dbInternal = "de";
                        break;
                    case 4:
                        dbJP = "ダブル";
                        dbNA = "Double/Gemini";
                        dbInternal = "ma";
                        break;
                    default:
                        break;
                }
                genAnimList.Add($"{dbJP} オーラモーション,{dbNA} Aura Animations," + GetFileHash(db_vehicle + $"pl_dh{i}{dbInternal}_ht.ice"));
                dbList.Add($"{dbJP} トランスフォームエフェクト,{dbNA} Transform Effects," + GetFileHash(db_vehicle + $"dh_se_transform_dh{i}{dbInternal}.ice"));
                dbList.Add($"{dbJP} モーション,{dbNA} Animations," + GetFileHash(db_vehicle + $"vc_dh{i}{dbInternal}.ice"));
                dbList.Add($"{dbJP} モデル,{dbNA} Model," + GetFileHash(db_vehicle + $"vc_dh{i}{dbInternal}_a.ice"));
                dbList.Add($"{dbJP} オーラモデル,{dbNA} Aura Model," + GetFileHash(db_vehicle + $"vc_dh{i}{dbInternal}_ht.ice"));
                dbList.Add($"{dbJP} LD モデル,{dbNA} Low Detail Model," + GetFileHash(db_vehicle + $"vc_dh{i}{dbInternal}_low.ice"));
                dbList.Add($"{dbJP} エフェクト,{dbNA} Effects," + GetFileHash(db_vehicle + $"vc_dh{i}{dbInternal}_eff.ice"));
                dbList.Add($"{dbJP} レベル,{dbNA} Levels," + GetFileHash(db_vehicle + $"vc_dh{i}{dbInternal}_level.ice"));
            }

            File.WriteAllLines(Path.Combine(playerRAnimDirOut, $"General Character Animations NGS.csv"), genAnimListNGS, Encoding.UTF8);
            File.WriteAllLines(Path.Combine(playerCAnimDirOut, $"General Character Animations.csv"), genAnimList, Encoding.UTF8);
            File.WriteAllLines(Path.Combine(playerDirOut, "DarkBlasts_DrivableVehicles.csv"), dbList, Encoding.UTF8);

            //---------------------------NGS Special Weapon/Vehicle List
            List<string> ngsVehicleOutput = new List<string>();
            ngsVehicleOutput.Add("Mobile Cannon,モバイルキャノン,f2/150838707a2fda44d80b91220a3b39");
            ngsVehicleOutput.Add("Snowboard,スノーボード,53/36651397f0cd04340eacf32a116e5b");
            File.WriteAllLines(Path.Combine(playerDirOut, "DarkBlasts_DrivableVehiclesNGS.csv"), ngsVehicleOutput, Encoding.UTF8);
        }

        public static void GeneratePetList(string petsDirOut)
        {
            //---------------------------Generate Pet List
            List<string> classicPetOutput = new List<string>();
            foreach (var str in EnemyData.classicPetNames)
            {
                classicPetOutput.Add(str + $",{ GetFileHash("enemy/" + str.Split(',')[2]) }");
            }
            File.WriteAllLines(Path.Combine(petsDirOut, "PetsClassic.csv"), classicPetOutput, Encoding.UTF8);

            //---------------------------Generate NGS Pet List

            //Placeholder until NGS pets are released
        }

        public static void GenerateEnemyDataList(string pso2_binDir, string enemyDirOut, Dictionary<string, List<List<PSO2Text.textPair>>> actorNameRebootByCat, out List<string> masterNameList, out List<Dictionary<string, string>> strNameDicts)
        {
            //---------------------------Generate Enemy Base Stats List
            List<string> classicEnemyStatOutput = new List<string>();
            foreach (var str in EnemyData.classicBaseStats)
            {
                classicEnemyStatOutput.Add(str + $",{ GetFileHash("enemy/" + str.Split(',')[2]) }");
            }
            File.WriteAllLines(Path.Combine(enemyDirOut, "EnemyBaseStats.csv"), classicEnemyStatOutput, Encoding.UTF8);

            //---------------------------Generate Enemy List
            List<string> classicEnemyOutput = new List<string>();
            foreach (var str in EnemyData.classicEnemyNames)
            {
                classicEnemyOutput.Add(str + $",{ GetFileHash("enemy/" + str.Split(',')[2]) }");
            }
            File.WriteAllLines(Path.Combine(enemyDirOut, "EnemiesClassic.csv"), classicEnemyOutput, Encoding.UTF8);

            //---------------------------Generate NGS Enemy List
            List<string> ngsEnemyOutput = new List<string>();
            Dictionary<string, string> processedMotions = new Dictionary<string, string>();
            Dictionary<string, string> processedEffects = new Dictionary<string, string>();
            List<string> usedMotions = new List<string>();
            List<string> usedEffects = new List<string>();

            masterNameList = new List<string>();
            strNameDicts = new List<Dictionary<string, string>>();
            GatherTextIdsStringRef(actorNameRebootByCat, masterNameList, strNameDicts, "Enemy", true);
            var rbRegions = new List<string>(EnemyData.rebootRegions);
            var rbFactions = new List<string>(EnemyData.rebootFaction);
            var rbNames = new List<string>(EnemyData.rebootEnNames);
            var rbEnds = new List<string>(EnemyData.rebootEnemyEnd);

            //Ensure that names not stored in the arrays are checked
            foreach (var dict in strNameDicts)
            {
                foreach (var fullName in dict.Keys)
                {
                    var nameArr = fullName.Split('_');
                    if (!rbRegions.Contains(nameArr[0]))
                    {
                        rbRegions.Add(nameArr[0]);
                        Debug.WriteLine($"Added NGS Enemy Region: {nameArr[0]}");
                    }
                    if (nameArr.Length > 1 && !rbFactions.Contains(nameArr[1]))
                    {
                        rbFactions.Add(nameArr[1]);
                        Debug.WriteLine($"Added NGS Enemy Faction: {nameArr[1]}");
                    }
                    if (nameArr.Length > 2 && !rbNames.Contains(nameArr[2]))
                    {
                        rbNames.Add(nameArr[2]);
                        Debug.WriteLine($"Added NGS Enemy Name: {nameArr[2]}");
                    }
                    if (nameArr.Length > 3 && !rbEnds.Contains(nameArr[3]))
                    {
                        rbEnds.Add(nameArr[3]);
                        Debug.WriteLine($"Added NGS Enemy End: {nameArr[3]}");
                    }
                }
            }

            foreach (var rg in rbRegions)
            {
                foreach (var fc in rbFactions)
                {
                    foreach (var nm in rbNames)
                    {
                        bool enemyFound = false;
                        List<string> validEndings = new List<string>();

                        foreach (var ed in rbEnds)
                        {
                            string _0 = "_";
                            string _1 = "_";
                            string _2 = "_";

                            //Handle name underscoring in the case of null segments
                            if (rg == "")
                            {
                                _0 = "";
                            }
                            if (fc == "")
                            {
                                _1 = "";
                            }
                            if (nm == "" || ed == "")
                            {
                                _2 = "";
                            }

                            string actorName = $"{rg}{_0}{fc}{_1}{nm}{_2}{ed}";
                            string file = $"{EnemyData.rebootEnemy}{actorName}.ice";
                            string fileHash = GetRebootHash(GetFileHash(file));
                            string vetFile = file.Replace(".ice", "_ag.ice");
                            string vetFileHash = GetRebootHash(GetFileHash(vetFile));
                            string nameString = "";

                            //Fix for cases where the internal name for actor_name.text uses the r01 designator, but not the file proper
                            if (rg == "")
                            {
                                actorName = "r01_" + actorName;
                            }

                            foreach (var dict in strNameDicts)
                            {
                                if (dict.TryGetValue(actorName, out string str) && str != null && str != "" && str.Length > 0)
                                {
                                    nameString += str + ",";
                                }
                                else
                                {
                                    nameString += ",";
                                }
                            }

                            if (File.Exists(Path.Combine(pso2_binDir, dataReboot, fileHash)))
                            {
                                ngsEnemyOutput.Add(nameString + file.Replace("enemy/", "") + "," + fileHash);
                                validEndings.Add(ed);
                                enemyFound = true;
                            }
                            if (File.Exists(Path.Combine(pso2_binDir, dataReboot, vetFileHash)))
                            {
                                ngsEnemyOutput.Add(nameString + vetFile.Replace("enemy/", "") + "," + vetFileHash + ",ベテラン,Veteran");
                                if (!validEndings.Contains(ed))
                                {
                                    validEndings.Add(ed);
                                }
                                enemyFound = true;
                            }
                        }

                        //Check animations and effects
                        string motion = $"{EnemyData.rebootEnemy}{fc}_{nm}_{EnemyData.rebootEndOther[0]}.ice";

                        //Duplicates are fine if it makes sense for an enemy, but otherwise avoid them
                        if (!processedMotions.ContainsKey(motion))
                        {
                            string motionHash = GetRebootHash(GetFileHash(motion));
                            if (File.Exists(Path.Combine(pso2_binDir, dataReboot, motionHash)))
                            {
                                string motString = motion.Replace("enemy/", "") + "," + motionHash;
                                processedMotions.Add(motion, motString);
                            }
                        }
                        string effect = $"{EnemyData.rebootEnemy}{fc}_{nm}_{EnemyData.rebootEndOther[1]}.ice";
                        string effect2 = $"{EnemyData.rebootEnemy}{fc}_{nm}_{EnemyData.rebootEndOther[0]}_{EnemyData.rebootEndOther[1]}.ice";
                        string effect3 = $"{EnemyData.rebootEnemy}{fc}_{nm}_{EnemyData.rebootEndOther[1]}_{EnemyData.rebootEndOther[0]}.ice";
                        if (!processedEffects.ContainsKey(effect))
                        {
                            string effectHash = GetRebootHash(GetFileHash(effect));
                            if (File.Exists(Path.Combine(pso2_binDir, dataReboot, effectHash)))
                            {
                                string effString = effect.Replace("enemy/", "") + "," + effectHash;
                                processedEffects.Add(effect, effString);
                            }
                        }
                        else if (!processedEffects.ContainsKey(effect2)) //common_eff
                        {
                            effect = effect2;
                            if (!processedEffects.ContainsKey(effect))
                            {
                                string effectHash = GetRebootHash(GetFileHash(effect));
                                if (File.Exists(Path.Combine(pso2_binDir, dataReboot, effectHash)))
                                {
                                    string effString = effect.Replace("enemy/", "") + "," + effectHash;
                                    processedEffects.Add(effect, effString);
                                }
                            }
                        }
                        else if (!processedEffects.ContainsKey(effect3)) //eff_common
                        {
                            effect = effect3;
                            if (!processedEffects.ContainsKey(effect))
                            {
                                string effectHash = GetRebootHash(GetFileHash(effect));
                                if (File.Exists(Path.Combine(pso2_binDir, dataReboot, effectHash)))
                                {
                                    string effString = effect.Replace("enemy/", "") + "," + effectHash;
                                    processedEffects.Add(effect, effString);
                                }
                            }
                        }

                        //Check for special animations and effects for variants


                        //Add to output if there's a linked enemy
                        if (enemyFound)
                        {
                            if (processedMotions.ContainsKey(motion))
                            {
                                ngsEnemyOutput.Add(processedMotions[motion]);
                                usedMotions.Add(motion);
                            }

                            if (processedEffects.ContainsKey(effect))
                            {
                                ngsEnemyOutput.Add(processedEffects[effect]);
                                usedEffects.Add(effect);
                            }
                        }
                    }
                }
            }

            //Add unused animations and effects
            foreach (var key in processedMotions.Keys)
            {
                if (usedMotions.Contains(key))
                {
                    continue;
                }
                else
                {
                    ngsEnemyOutput.Add(processedMotions[key]);
                }
            }
            foreach (var key in processedEffects.Keys)
            {
                if (usedEffects.Contains(key))
                {
                    continue;
                }
                else
                {
                    ngsEnemyOutput.Add(processedEffects[key]);
                }
            }

            File.WriteAllLines(Path.Combine(enemyDirOut, "EnemiesNGS.csv"), ngsEnemyOutput, Encoding.UTF8);

            //---------------------------Generate Miscellaneous NGS Enemy List
            List<string> ngsMiscOutput = new List<string>();

            foreach (var file in EnemyData.rebootEnemyMisc)
            {
                var hash = GetRebootHash(GetFileHash(file));
                if (File.Exists(Path.Combine(pso2_binDir, dataReboot, hash)))
                {
                    ngsMiscOutput.Add(file + "," + hash);
                }
            }

            File.WriteAllLines(Path.Combine(enemyDirOut, "EnemiesNGS Miscellaneous.csv"), ngsMiscOutput, Encoding.UTF8);
        }

        public static void GenerateWeaponLists(string pso2_binDir, string outputDirectory)
        {
            //---------------------------Generate Weapon Defaults
            List<string> wepDefOutput = new List<string>();
            List<string> wepDefNGSOutput = new List<string>();

            for (int i = 1; i < weaponTypes.Count + 1; i++)
            {
                var type = weaponTypes[i - 1];
                var file = weaponDir + baseWeaponString + $"{i:D2}" + "_" + type + ".ice";
                var hashedFile = GetFileHash(file);

                if (File.Exists(Path.Combine(pso2_binDir, dataDir, hashedFile)))
                {
                    wepDefOutput.Add($"{file},{hashedFile}");
                }

                if (Directory.Exists(Path.Combine(pso2_binDir, dataReboot)))
                {
                    var typeNGS = weaponTypesNGS[i - 1]; //Most of these are the same, but a few are slightly different
                    var fileNGS = weaponDir + baseWeaponString + $"{i:D2}" + "_" + typeNGS + ".ice";
                    var hashedFileNGS = GetFileHash(fileNGS);
                    var rebootHash = GetRebootHash(hashedFileNGS);
                    if (File.Exists(Path.Combine(pso2_binDir, dataReboot, rebootHash)))
                    {
                        wepDefNGSOutput.Add($"{file},{rebootHash}");
                    }
                }
            }

            Directory.CreateDirectory(outputDirectory + "\\Weapons\\PSO2\\");
            Directory.CreateDirectory(outputDirectory + "\\Weapons\\NGS\\");
            if (wepDefOutput.Count > 0)
            {
                File.WriteAllLines(outputDirectory + "\\Weapons\\PSO2\\" + "WeaponDefaults.csv", wepDefOutput, Encoding.UTF8);
            }
            if (wepDefNGSOutput.Count > 0)
            {
                File.WriteAllLines(outputDirectory + "\\Weapons\\NGS\\" + "WeaponDefaultsNGS.csv", wepDefNGSOutput, Encoding.UTF8);
            }

            //---------------------------Generate Weapon list 
            List<string> swordOutput = new List<string>();
            List<string> wiredLanceOutput = new List<string>();
            List<string> partizanOutput = new List<string>();
            List<string> twinDaggerOutput = new List<string>();
            List<string> doubleSaberOutput = new List<string>();
            List<string> knucklesOutput = new List<string>();
            List<string> gunslashOutput = new List<string>();
            List<string> rifleOutput = new List<string>();
            List<string> launcherOutput = new List<string>();
            List<string> tmgOutput = new List<string>();
            List<string> rodOutput = new List<string>();
            List<string> talisOutput = new List<string>();
            List<string> wandOutput = new List<string>();
            List<string> katanaOutput = new List<string>();
            List<string> bowOutput = new List<string>();
            List<string> jetbootsOutput = new List<string>();
            List<string> dualBladesOutput = new List<string>();
            List<string> tactOutput = new List<string>();

            List<string> fallbackOutput = new List<string>();

            List<List<string>> wepOutputList = new List<List<string>>()
            {
                swordOutput,
                wiredLanceOutput,
                partizanOutput,
                twinDaggerOutput,
                doubleSaberOutput,
                knucklesOutput,
                gunslashOutput,
                rifleOutput,
                launcherOutput,
                tmgOutput,
                rodOutput,
                talisOutput,
                wandOutput,
                katanaOutput,
                bowOutput,
                jetbootsOutput,
                dualBladesOutput,
                tactOutput,
                fallbackOutput
            };


            List<string> weaponListOutput;

            //Get the default weapons
            for (int i = 0; i < weaponTypesShort.Count; i++)
            {
                var type = weaponTypesShort[i];
                if (type != null)
                {
                    weaponListOutput = wepOutputList[i];

                    var file = weaponDir + defaultWeaponString + type + ".ice";
                    var hashedFile = GetFileHash(file);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, hashedFile)))
                    {
                        weaponListOutput.Add($"Default {weaponTypes[i]},,{file},{hashedFile}");
                    }
                }
            }

            //Weapon names
            for (int i = 1; i < 19; i++)
            {
                for (int id = 0; id < 1000; id++)
                {
                    var file = weaponDir + weaponString + $"{i:D2}" + "_" + $"{id:D3}" + ".ice";
                    var hashedFile = GetFileHash(file);

                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, hashedFile)))
                    {
                        weaponListOutput = wepOutputList[i - 1];
                        string name = null;
                        switch (i - 1)
                        {
                            case 0:
                                name = GetNameFromIdDict(id, swordNames);
                                break;
                            case 1:
                                name = GetNameFromIdDict(id, wiredLanceNames);
                                break;
                            case 2:
                                name = GetNameFromIdDict(id, partizanNames);
                                break;
                            case 3:
                                name = GetNameFromIdDict(id, twinDaggerNames);
                                break;
                            case 4:
                                name = GetNameFromIdDict(id, doubleSaberNames);
                                break;
                            case 5:
                                name = GetNameFromIdDict(id, knucklesNames);
                                break;
                            case 6:
                                name = GetNameFromIdDict(id, gunslashNames);
                                break;
                            case 7:
                                name = GetNameFromIdDict(id, rifleNames);
                                break;
                            case 8:
                                name = GetNameFromIdDict(id, launcherNames);
                                break;
                            case 9:
                                name = GetNameFromIdDict(id, tmgNames);
                                break;
                            case 10:
                                name = GetNameFromIdDict(id, rodNames);
                                break;
                            case 11:
                                name = GetNameFromIdDict(id, talysNames);
                                break;
                            case 12:
                                name = GetNameFromIdDict(id, wandNames);
                                break;
                            case 13:
                                name = GetNameFromIdDict(id, katanaNames);
                                break;
                            case 14:
                                name = GetNameFromIdDict(id, bowNames);
                                break;
                            case 15:
                                name = GetNameFromIdDict(id, jetBootsNames);
                                break;
                            case 16:
                                name = GetNameFromIdDict(id, dualBladesNames);
                                break;
                            case 17:
                                name = GetNameFromIdDict(id, tactNames);
                                break;
                            default:
                                weaponListOutput = new List<string>();
                                break;
                        }

                        if (name == null)
                        {
                            name = ",";
                        }

                        weaponListOutput.Add(name + "," + file + "," + hashedFile);
                    }
                }
            }

            WriteList(outputDirectory + "\\Weapons\\PSO2\\" + "SwordNames.csv", swordOutput);
            WriteList(outputDirectory + "\\Weapons\\PSO2\\" + "WiredLanceNames.csv", wiredLanceOutput);
            WriteList(outputDirectory + "\\Weapons\\PSO2\\" + "PartizanNames.csv", partizanOutput);
            WriteList(outputDirectory + "\\Weapons\\PSO2\\" + "TwinDaggerNames.csv", twinDaggerOutput);
            WriteList(outputDirectory + "\\Weapons\\PSO2\\" + "DoubleSaberNames.csv", doubleSaberOutput);
            WriteList(outputDirectory + "\\Weapons\\PSO2\\" + "KnucklesNames.csv", knucklesOutput);
            WriteList(outputDirectory + "\\Weapons\\PSO2\\" + "GunslashNames.csv", gunslashOutput);
            WriteList(outputDirectory + "\\Weapons\\PSO2\\" + "RifleNames.csv", rifleOutput);
            WriteList(outputDirectory + "\\Weapons\\PSO2\\" + "LauncherNames.csv", launcherOutput);
            WriteList(outputDirectory + "\\Weapons\\PSO2\\" + "TwinMachineGunNames.csv", tmgOutput);
            WriteList(outputDirectory + "\\Weapons\\PSO2\\" + "RodNames.csv", rodOutput);
            WriteList(outputDirectory + "\\Weapons\\PSO2\\" + "TalisNames.csv", talisOutput);
            WriteList(outputDirectory + "\\Weapons\\PSO2\\" + "WandNames.csv", wandOutput);
            WriteList(outputDirectory + "\\Weapons\\PSO2\\" + "KatanaNames.csv", katanaOutput);
            WriteList(outputDirectory + "\\Weapons\\PSO2\\" + "BowNames.csv", bowOutput);
            WriteList(outputDirectory + "\\Weapons\\PSO2\\" + "JetBootsNames.csv", jetbootsOutput);
            WriteList(outputDirectory + "\\Weapons\\PSO2\\" + "DualBladesNames.csv", dualBladesOutput);
            WriteList(outputDirectory + "\\Weapons\\PSO2\\" + "TactNames.csv", tactOutput);
            WriteList(outputDirectory + "\\Weapons\\PSO2\\" + "Undefined.csv", fallbackOutput);

            //---------------------------Generate NGS Weapon list
            List<string> swordNGSOutput = new List<string>();
            List<string> wiredLanceNGSOutput = new List<string>();
            List<string> partizanNGSOutput = new List<string>();
            List<string> twinDaggerNGSOutput = new List<string>();
            List<string> doubleSaberNGSOutput = new List<string>();
            List<string> knucklesNGSOutput = new List<string>();
            List<string> gunslashNGSOutput = new List<string>();
            List<string> rifleNGSOutput = new List<string>();
            List<string> launcherNGSOutput = new List<string>();
            List<string> tmgNGSOutput = new List<string>();
            List<string> rodNGSOutput = new List<string>();
            List<string> talisNGSOutput = new List<string>();
            List<string> wandNGSOutput = new List<string>();
            List<string> katanaNGSOutput = new List<string>();
            List<string> bowNGSOutput = new List<string>();
            List<string> jetbootsNGSOutput = new List<string>();
            List<string> dualBladesNGSOutput = new List<string>();
            List<string> tactNGSOutput = new List<string>();

            List<string> fallbackNGSOutput = new List<string>();

            List<List<string>> wepNGSOutputList = new List<List<string>>()
            {
                swordNGSOutput,
                wiredLanceNGSOutput,
                partizanNGSOutput,
                twinDaggerNGSOutput,
                doubleSaberNGSOutput,
                knucklesNGSOutput,
                gunslashNGSOutput,
                rifleNGSOutput,
                launcherNGSOutput,
                tmgNGSOutput,
                rodNGSOutput,
                talisNGSOutput,
                wandNGSOutput,
                katanaNGSOutput,
                bowNGSOutput,
                jetbootsNGSOutput,
                dualBladesNGSOutput,
                tactNGSOutput,
                fallbackNGSOutput
            };

            List<string> weaponListNGSOutput = new List<string>();

            //Weapon names
            for (int i = 0; i < 21; i++)
            {
                for (int id = 0; id < 1000; id++)
                {
                    var file = weaponDir + weaponString + $"{i:D2}" + "_" + $"{id:D3}" + ".ice";
                    var hashedFile = GetFileHash(file);
                    hashedFile = GetRebootHash(hashedFile);
                    var test = Path.Combine(pso2_binDir, dataReboot, hashedFile);
                    if (File.Exists(Path.Combine(pso2_binDir, dataReboot, hashedFile)))
                    {
                        weaponListNGSOutput = wepNGSOutputList[i - 1];
                        string name = null;
                        switch (i - 1)
                        {
                            case 0:
                                name = GetNameFromIdDict(id, swordNGSNames);
                                break;
                            case 1:
                                name = GetNameFromIdDict(id, wiredLanceNGSNames);
                                break;
                            case 2:
                                name = GetNameFromIdDict(id, partizanNGSNames);
                                break;
                            case 3:
                                name = GetNameFromIdDict(id, twinDaggerNGSNames);
                                break;
                            case 4:
                                name = GetNameFromIdDict(id, doubleSaberNGSNames);
                                break;
                            case 5:
                                name = GetNameFromIdDict(id, knucklesNGSNames);
                                break;
                            case 6:
                                name = GetNameFromIdDict(id, gunslashNGSNames);
                                break;
                            case 7:
                                name = GetNameFromIdDict(id, rifleNGSNames);
                                break;
                            case 8:
                                name = GetNameFromIdDict(id, launcherNGSNames);
                                break;
                            case 9:
                                name = GetNameFromIdDict(id, tmgNGSNames);
                                break;
                            case 10:
                                name = GetNameFromIdDict(id, rodNGSNames);
                                break;
                            case 11:
                                name = GetNameFromIdDict(id, talysNGSNames);
                                break;
                            case 12:
                                name = GetNameFromIdDict(id, wandNGSNames);
                                break;
                            case 13:
                                name = GetNameFromIdDict(id, katanaNGSNames);
                                break;
                            case 14:
                                name = GetNameFromIdDict(id, bowNGSNames);
                                break;
                            case 15:
                                name = GetNameFromIdDict(id, jetBootsNGSNames);
                                break;
                            case 16:
                                name = GetNameFromIdDict(id, dualBladesNGSNames);
                                break;
                            case 17:
                                name = GetNameFromIdDict(id, tactNGSNames);
                                break;
                            default:
                                weaponListNGSOutput = new List<string>();
                                break;
                        }

                        if (name == null)
                        {
                            name = ",";
                        }

                        weaponListNGSOutput.Add(name + "," + file + "," + hashedFile);
                    }
                }
            }

            WriteList(outputDirectory + "\\Weapons\\NGS\\" + "SwordNGSNames.csv", swordNGSOutput);
            WriteList(outputDirectory + "\\Weapons\\NGS\\" + "WiredLanceNGSNames.csv", wiredLanceNGSOutput);
            WriteList(outputDirectory + "\\Weapons\\NGS\\" + "PartizanNGSNames.csv", partizanNGSOutput);
            WriteList(outputDirectory + "\\Weapons\\NGS\\" + "TwinDaggerNGSNames.csv", twinDaggerNGSOutput);
            WriteList(outputDirectory + "\\Weapons\\NGS\\" + "DoubleSaberNGSNames.csv", doubleSaberNGSOutput);
            WriteList(outputDirectory + "\\Weapons\\NGS\\" + "KnucklesNGSNames.csv", knucklesNGSOutput);
            WriteList(outputDirectory + "\\Weapons\\NGS\\" + "GunslashNGSNames.csv", gunslashNGSOutput);
            WriteList(outputDirectory + "\\Weapons\\NGS\\" + "RifleNGSNames.csv", rifleNGSOutput);
            WriteList(outputDirectory + "\\Weapons\\NGS\\" + "LauncherNGSNames.csv", launcherNGSOutput);
            WriteList(outputDirectory + "\\Weapons\\NGS\\" + "TwinMachineGunNGSNames.csv", tmgNGSOutput);
            WriteList(outputDirectory + "\\Weapons\\NGS\\" + "RodNGSNames.csv", rodNGSOutput);
            WriteList(outputDirectory + "\\Weapons\\NGS\\" + "TalisNGSNames.csv", talisNGSOutput);
            WriteList(outputDirectory + "\\Weapons\\NGS\\" + "WandNGSNames.csv", wandNGSOutput);
            WriteList(outputDirectory + "\\Weapons\\NGS\\" + "KatanaNGSNames.csv", katanaNGSOutput);
            WriteList(outputDirectory + "\\Weapons\\NGS\\" + "BowNGSNames.csv", bowNGSOutput);
            WriteList(outputDirectory + "\\Weapons\\NGS\\" + "JetBootsNGSNames.csv", jetbootsNGSOutput);
            WriteList(outputDirectory + "\\Weapons\\NGS\\" + "DualBladesNGSNames.csv", dualBladesNGSOutput);
            WriteList(outputDirectory + "\\Weapons\\NGS\\" + "TactNGSNames.csv", tactNGSOutput);
            WriteList(outputDirectory + "\\Weapons\\NGS\\" + "UndefinedNGS.csv", fallbackNGSOutput);
        }

        private static string GetIconGender(int id)
        {
            //NGS ids
            if (id >= 100000)
            {
                switch (id /= 100000)
                {
                    case 1:
                        return iconMale;
                    case 2:
                        return iconFemale;
                    case 3:
                        return iconMale;
                    case 4:
                        return iconFemale;
                    case 5:
                        return iconMale;
                }

            }
            else //Classic ids
            {
                switch (id /= 10000)
                {
                    case 0:
                        return iconMale;
                    case 1:
                        return iconFemale;
                    case 2:
                        return iconMale;
                    case 3:
                        return iconFemale;
                    case 4:
                        return iconMale;
                    case 5:
                        return iconFemale;
                }
            }

            MessageBox.Show($"Unexpected id: {id}");
            throw new Exception();
        }

        private static void WriteCSV(string outputDirectory, string fileName, StringBuilder output, bool nullSB = true)
        {
            if (output.Length != 0)
            {
                File.WriteAllText(Path.Combine(outputDirectory, fileName), output.ToString(), Encoding.UTF8);
            }

            //If true, clear out the stringbuilder object
            if (nullSB == true)
            {
                output = null;
            }
        }

        private static void WriteCSV(string outputDirectory, string fileName, List<string> output)
        {
            if (output.Count != 0)
            {
                File.WriteAllLines(Path.Combine(outputDirectory, fileName), output, Encoding.UTF8);
            }
        }

        public static void AddBodyExtraFiles(PartData partData, string fname, string pso2_binDir, string typeString, bool isClassic)
        {
            string rpName, bmName, hnName;
            GetBodyExtraFileStrings(fname, typeString, out rpName, out bmName, out hnName);
            string rpHash = GetFileHash(rpName);
            string bmHash = GetFileHash(bmName);
            string hnHash = GetFileHash(hnName);

            //_rp alt model
            if (File.Exists(Path.Combine(pso2_binDir, dataDir, rpHash)))
            {
                partData.partRpHash = rpHash;
                partData.partRpName = rpName;
            }
            //Aqv archive
            if (File.Exists(Path.Combine(pso2_binDir, dataDir, bmHash)))
            {
                partData.matAnimHash = bmHash;
                partData.matAnimName = bmName;
            }

            //NGS doesn't have these sorts of files
            if (isClassic)
            {
                //Hand textures
                if (File.Exists(Path.Combine(pso2_binDir, dataDir, hnHash)))
                {
                    partData.handsHash = hnHash;
                    partData.handsName = hnName;
                }
            }

            return;
        }

        public static void AddHQBodyExtraFiles(PartData partData, string fname, string pso2_binDir, string typeString, bool isClassic)
        {
            string rpName, bmName, hnName;
            GetHQBodyExtraFileStrings(fname, typeString, out rpName, out bmName, out hnName);
            string rpHash = GetFileHash(rpName);
            string bmHash = GetFileHash(bmName);
            string hnHash = GetFileHash(hnName);

            //_rp alt model
            if (File.Exists(Path.Combine(pso2_binDir, dataDir, rpHash)))
            {
                partData.partRpHash = rpHash;
                partData.partRpName = rpName;
            }
            //Aqv archive
            if (File.Exists(Path.Combine(pso2_binDir, dataDir, bmHash)))
            {
                partData.matAnimHash = bmHash;
                partData.matAnimName = bmName;
            }

            //NGS doesn't have these sorts of files
            if (isClassic)
            {
                //Hand textures
                if (File.Exists(Path.Combine(pso2_binDir, dataDir, hnHash)))
                {
                    partData.handsHash = hnHash;
                    partData.handsName = hnName;
                }
            }

            return;
        }

        public static void GetHQBodyExtraFileStrings(string fname, string typeString, out string rpCheck, out string bmCheck, out string hnCheck)
        {
            rpCheck = fname.Replace("_ex.ice", "_rp_ex.ice");
            bmCheck = fname.Replace(typeString, "_bm_");
            hnCheck = fname.Replace(typeString, "_hn_");
            //If not basewear, hn. If basewear, ho
        }

        public static void GetBodyExtraFileStrings(string fname, string typeString, out string rpCheck, out string bmCheck, out string hnCheck)
        {
            rpCheck = fname.Replace(".ice", "_rp.ice");
            bmCheck = fname.Replace(typeString, "_bm_");
            hnCheck = fname.Replace(typeString, "_hn_");
            //If not basewear, hn. If basewear, ho
        }

        public static void AddBasewearExtraFiles(PartData partData, string fname, string pso2_binDir, bool isClassic)
        {
            string rpName, hnName, bmName;
            GetBasewearExtraFileStrings(fname, out rpName, out hnName, out bmName);
            string rpHash = GetFileHash(rpName);
            string bmHash = GetFileHash(bmName);
            string hnHash = GetFileHash(hnName);

            //_rp alt model
            if (File.Exists(Path.Combine(pso2_binDir, dataDir, rpHash)))
            {
                partData.partRpHash = rpHash;
                partData.partRpName = rpName;
            }
            var rebootBm = GetRebootHash(bmName);
            //Aqv archive
            if (File.Exists(Path.Combine(pso2_binDir, dataDir, bmHash)))
            {
                partData.matAnimHash = bmHash;
                partData.matAnimName = bmName;
            }

            //NGS doesn't have these sorts of files
            if (isClassic)
            {
                //Hand textures
                if (File.Exists(Path.Combine(pso2_binDir, dataDir, hnHash)))
                {
                    partData.handsHash = hnHash;
                    partData.handsName = hnName;
                }
            }

            return;
        }

        public static void AddHQBasewearExtraFiles(PartData partData, string fname, string pso2_binDir, bool isClassic)
        {
            string rpName, hnName, bmName;
            GetHQBasewearExtraFileStrings(fname, out rpName, out hnName, out bmName);
            string rpHash = GetFileHash(rpName);
            string bmHash = GetFileHash(bmName);
            string hnHash = GetFileHash(hnName);

            //_rp alt model
            if (File.Exists(Path.Combine(pso2_binDir, dataDir, rpHash)))
            {
                partData.partRpHash = rpHash;
                partData.partRpName = rpName;
            }
            var rebootBm = GetRebootHash(bmName);

            //Aqv archive
            if (File.Exists(Path.Combine(pso2_binDir, dataDir, bmHash)))
            {
                partData.matAnimHash = bmHash;
                partData.matAnimName = bmName;
            }

            //NGS doesn't have these sorts of files
            if (isClassic)
            {
                //Hand textures
                if (File.Exists(Path.Combine(pso2_binDir, dataDir, hnHash)))
                {
                    partData.handsHash = hnHash;
                    partData.handsName = hnName;
                }
            }

            return;
        }

        public static void GetBasewearExtraFileStrings(string fname, out string rpCheck, out string hnCheck, out string bmCheck)
        {
            rpCheck = fname.Replace(".ice", "_rp.ice");
            hnCheck = fname.Replace("bw", "ho");
            //If not basewear, hn. If basewear, ho
            bmCheck = fname.Replace("bw", "bm");
        }

        public static void GetHQBasewearExtraFileStrings(string fname, out string rpCheck, out string hnCheck, out string bmCheck)
        {
            rpCheck = fname.Replace("_ex.ice", "_rp_ex.ice");
            hnCheck = fname.Replace("bw", "ho");
            //If not basewear, hn. If basewear, ho
            bmCheck = fname.Replace("bw", "bm");
        }

        public static void AddOutfitSound(PartData partData, string pso2_binDir, int soundId)
        {
            if (soundId != -1)
            {
                string soundFileUnhash = "";
                string soundFileLegUnhash = "";
                if (soundId >= 100000)
                {
                    soundFileUnhash = $"{rebootStart}bs_{soundId}.ice";
                } else
                {
                    soundFileUnhash = $"{classicStart}bs_{soundId:D5}.ice";
                    soundFileLegUnhash = $"{classicStart}ls_{soundId:D5}.ice";
                }
                string soundFile = GetFileHash(soundFileUnhash);
                string castSoundFile = GetFileHash(soundFileLegUnhash);
                if (File.Exists(Path.Combine(pso2_binDir, dataDir, castSoundFile)))
                {
                    partData.castSoundHash = castSoundFile;
                    partData.castSoundName = soundFileLegUnhash;
                }
                if (File.Exists(Path.Combine(pso2_binDir, dataDir, soundFile)))
                {
                    partData.soundHash = soundFile;
                    partData.soundName = soundFileUnhash;
                }
            }

            return;
        }

        public static void GatherDictKeys<T>(List<int> masterIdList, Dictionary<int, T>.KeyCollection keys)
        {
            foreach (int key in keys)
            {
                if (!masterIdList.Contains(key))
                {
                    masterIdList.Add(key);
                }
            }
        }

        public static void GatherTextIds(Dictionary<string, List<List<PSO2Text.textPair>>> textByCat, List<int> masterIdList, List<Dictionary<int, string>> nameDicts, string category, bool firstDictSet, int subStart = 0, int subStop = -1)
        {
            if (textByCat.ContainsKey(category))
            {
                for (int sub = 0; sub < textByCat[category].Count; sub++)
                {
                    if (firstDictSet == true)
                    {
                        nameDicts.Add(new Dictionary<int, string>());
                    }
                    foreach (var pair in textByCat[category][sub])
                    {
                        //ids should be stored as an ascii string with "No" in front of them
                        int id;
                        if (pair.name.Substring(0, 2).ToUpper().Equals("NO"))
                        {
                            Int32.TryParse(pair.name.Substring(2), out id);
                        }
                        else
                        {
                            if (Int32.TryParse(pair.name, out id) == false)
                            {
                                Console.WriteLine($"Could not parse {pair.name} : {pair.str}");
                                continue;
                            }
                        }

                        //When combining areas, such as with body and costume, there will be duplicates so we have to account for that.
                        if (!nameDicts[sub].ContainsKey(id))
                        {
                            nameDicts[sub].Add(id, pair.str);
                        }
                        if (!masterIdList.Contains(id))
                        {
                            masterIdList.Add(id);
                        }
                    }
                }

            }
            else
            {
                Console.WriteLine($"No category '{category}' present.");
            }
        }

        public static void GatherTextIdsStringRef(Dictionary<string, List<List<PSO2Text.textPair>>> textByCat, List<string> masterNameList, List<Dictionary<string, string>> nameDicts, string category, bool firstDictSet)
        {
            for (int sub = 0; sub < textByCat[category].Count; sub++)
            {
                if (firstDictSet == true)
                {
                    nameDicts.Add(new Dictionary<string, string>());
                }
                foreach (var pair in textByCat[category][sub])
                {
                    if (!nameDicts[sub].ContainsKey(pair.name))
                    {
                        nameDicts[sub].Add(pair.name, pair.str);
                    }
                    if (!masterNameList.Contains(pair.name))
                    {
                        masterNameList.Add(pair.name);
                    }
                }
            }
        }

        public static Dictionary<string, Dictionary<int, List<string>>> GatherSubCategories(Dictionary<string, List<List<PSO2Text.textPair>>> textByCat)
        {
            Dictionary<string, Dictionary<int, List<string>>> subCats = new Dictionary<string, Dictionary<int, List<string>>>();
            subCats.Add(subGlide, new Dictionary<int, List<string>>());
            subCats.Add(subIdle, new Dictionary<int, List<string>>());
            subCats.Add(subJump, new Dictionary<int, List<string>>());
            subCats.Add(subLanding, new Dictionary<int, List<string>>());
            subCats.Add(subMove, new Dictionary<int, List<string>>());
            subCats.Add(subSprint, new Dictionary<int, List<string>>());
            subCats.Add(subSwim, new Dictionary<int, List<string>>());

            for (int sub = 0; sub < textByCat["Substitute"].Count; sub++)
            {
                foreach (var pair in textByCat["Substitute"][sub])
                {
                    var split = pair.name.Split('_');
                    var id = Int32.Parse(split[2]);
                    string cat;

                    //Check category of motion and add based on that
                    switch (split[1])
                    {
                        case "Idle":
                            cat = subIdle;
                            break;
                        case "Jump":
                            cat = subJump;
                            break;
                        case "Landing":
                            cat = subLanding;
                            break;
                        case "Move":
                            cat = subMove;
                            break;
                        case "Sprint":
                            cat = subSprint;
                            break;
                        case "Glide":
                            cat = subGlide;
                            break;
                        case "Swim":
                            cat = subSwim;
                            break;
                        default:
                            MessageBox.Show($"Unknown substitution type: {split[1]} ... halting generation");
                            throw new Exception();
                    }

                    if (subCats[cat].ContainsKey(id))
                    {
                        subCats[cat][id].Add(pair.str);
                    }
                    else
                    {
                        subCats[cat][id] = new List<string>() { pair.str };
                    }
                }
            }

            return subCats;
        }

        public static Dictionary<int, string> ReadFaceVariationLua(string face_variationFileName)
        {
            using (StreamReader streamReader = new StreamReader(face_variationFileName))
            {
                return ParseFaceVariationLua(streamReader);
            }
        }

        public static Dictionary<int, string> ReadFaceVariationLua(byte[] face_variationLua)
        {
            using (StreamReader streamReader = new StreamReader(new MemoryStream(face_variationLua)))
            {
                return ParseFaceVariationLua(streamReader);
            }
        }

        public static Dictionary<int, string> ParseFaceVariationLua(StreamReader streamReader)
        {
            Dictionary<int, string> faceStrings = new Dictionary<int, string>();
            string language = null;

            //Cheap hacky way to do this. We don't really need to keep these separate so we loop through and get the 'language' value, our key for the .text dictionary. Our key to get this is the number from crop_name
            while (streamReader.EndOfStream == false)
            {
                var line = streamReader.ReadLine();
                if (language == null)
                {
                    if (line.Contains("language"))
                    {
                        language = line.Split('\"')[1];
                    }
                }
                else if (line.Contains("crop_name"))
                {
                    line = line.Split('\"')[1];
                    if (line != "") //ONE face doesn't use a crop_name. As it also doesn't have a crop_name, we don't care. Otherwise, add it to the dict
                    {
                        faceStrings.Add(Int32.Parse(line.Substring(7)), language);
                    }
                    language = null;
                }
            }

            return faceStrings;
        }

        public static string ToThree(string numString)
        {
            while (numString.Length < 3)
            {
                numString = numString.Insert(0, "0");
            }

            return numString;
        }

        public static string ToThree(int num)
        {
            string numString = num.ToString();
            return ToThree(numString);
        }

        public static void DumpLACInfo(string fileName, LobbyActionCommon lac)
        {
            var lacInfo = new List<string>();

            for (int i = 0; i < lac.dataBlocks.Count; i++)
            {
                var block = lac.dataBlocks[i];

                lacInfo.Add("Block " + i);
                lacInfo.Add("UnkInt0 -" + block.unkInt0);
                lacInfo.Add("internalName0 -" + block.internalName0);
                lacInfo.Add("chatCommand -" + block.chatCommand);
                lacInfo.Add("internalName1 -" + block.internalName1);

                lacInfo.Add("LobbyActionID -" + block.lobbyActionId);
                lacInfo.Add("commonReference0 -" + block.commonReference0);
                lacInfo.Add("commonReference1 -" + block.commonReference1);
                lacInfo.Add("unkOffsetInt0 -" + block.unkOffsetInt0);

                lacInfo.Add("unkOffsetInt1 -" + block.unkOffsetInt1);
                lacInfo.Add("unkOffsetInt2 -" + block.unkOffsetInt2);
                lacInfo.Add("iceName -" + block.iceName);
                lacInfo.Add("humanAqm -" + block.humanAqm);

                lacInfo.Add("castAqm1 -" + block.castAqm1);
                lacInfo.Add("castAqm2 -" + block.castAqm2);
                lacInfo.Add("kmnAqm -" + block.kmnAqm);
                lacInfo.Add("vfxIce -" + block.vfxIce);

                lacInfo.Add("");
            }

            File.WriteAllLines(fileName, lacInfo);
        }

        public static void WriteList(string filepath, List<string> output)
        {
            if (output.Count > 0)
            {
                File.WriteAllLines(filepath, output, Encoding.UTF8);
            }
        }

        private static string GetNameFromIdDict(int id, Dictionary<int, string> names)
        {
            if (names.ContainsKey(id))
            {
                return names[id];
            }

            return null;
        }

        public static string GetCastLegIconString(string id)
        {
            return GetFileHash(icon + castPartIcon + castLegIcon + id + ".ice");
        }

        public static string GetCastArmIconString(string id)
        {
            return GetFileHash(icon + castPartIcon + castArmIcon + id + ".ice");
        }

        public static string GetInnerwearIconString(string id)
        {
            return GetFileHash(icon + innerwearIcon + GetIconGender(Int32.Parse(id)) + id + ".ice");
        }

        public static string GetBasewearIconString(string id)
        {
            return GetFileHash(icon + basewearIcon + GetIconGender(Int32.Parse(id)) + id + ".ice");
        }

        public static string GetSkinIconString(string id)
        {
            return GetFileHash(icon + skinIcon + GetIconGender(Int32.Parse(id)) + id + ".ice");
        }

        public static string GetBodyPaintIconString(string id)
        {
            return GetFileHash(icon + bodyPaintIcon + id + ".ice");
        }

        public static string GetStickerIconString(string id)
        {
            return GetFileHash(icon + stickerIcon + id + ".ice");
        }

        public static string GetAccessoryIconString(string id)
        {
            return GetFileHash(icon + accessoryIcon + id + ".ice");
        }

        public static string GetEyeIconString(string id)
        {
            return GetFileHash(icon + eyeIcon + id + ".ice");
        }

        public static string GetEyebrowsIconString(string id)
        {
            return GetFileHash(icon + eyeBrowsIcon + id + ".ice");
        }

        public static string GetEyelashesIconString(string id)
        {
            return GetFileHash(icon + eyelashesIcon + id + ".ice");
        }

        public static string GetFaceIconString(string id)
        {
            return GetFileHash(icon + faceIcon + id + ".ice");
        }

        public static string GetFacePaintIconString(string id)
        {
            return GetFileHash(icon + facePaintIcon + id + ".ice");
        }

        public static string GetHairCastIconString(string id)
        {
            return GetFileHash(icon + hairIcon + "cast_" + id + ".ice");
        }

        public static string GetHairManIconString(string id)
        {
            return GetFileHash(icon + hairIcon + "man_" + id + ".ice");
        }

        public static string GetHairWomanIconString(string id)
        {
            return GetFileHash(icon + hairIcon + "woman_" + id + ".ice");
        }

        public static string GetHornIconString(string id)
        {
            return GetFileHash(icon + hornIcon + id + ".ice");
        }

        public static string GetTeethIconString(string id)
        {
            return GetFileHash(icon + teethIcon + id + ".ice");
        }

        public static string GetEarIconString(string id)
        {
            return GetFileHash(icon + earIcon + id + ".ice");
        }

        public static string GetCastLegIconStringUnhashed(string id)
        {
            return icon + castPartIcon + castLegIcon + id + ".ice";
        }

        public static string GetCastArmIconStringUnhashed(string id)
        {
            return icon + castPartIcon + castArmIcon + id + ".ice";
        }

        public static string GetInnerwearIconStringUnhashed(string id)
        {
            return icon + innerwearIcon + GetIconGender(Int32.Parse(id)) + id + ".ice";
        }

        public static string GetBasewearIconStringUnhashed(string id)
        {
            return icon + basewearIcon + GetIconGender(Int32.Parse(id)) + id + ".ice";
        }

        public static string GetSkinIconStringUnhashed(string id)
        {
            return icon + skinIcon + GetIconGender(Int32.Parse(id)) + id + ".ice";
        }

        public static string GetBodyPaintIconStringUnhashed(string id)
        {
            return icon + bodyPaintIcon + id + ".ice";
        }

        public static string GetStickerIconStringUnhashed(string id)
        {
            return icon + stickerIcon + id + ".ice";
        }

        public static string GetAccessoryIconStringUnhashed(string id)
        {
            return icon + accessoryIcon + id + ".ice";
        }

        public static string GetEyeIconStringUnhashed(string id)
        {
            return icon + eyeIcon + id + ".ice";
        }

        public static string GetEyebrowsIconStringUnhashed(string id)
        {
            return icon + eyeBrowsIcon + id + ".ice";
        }

        public static string GetEyelashesIconStringUnhashed(string id)
        {
            return icon + eyelashesIcon + id + ".ice";
        }

        public static string GetFaceIconStringUnhashed(string id)
        {
            return icon + faceIcon + id + ".ice";
        }

        public static string GetFacePaintIconStringUnhashed(string id)
        {
            return icon + facePaintIcon + id + ".ice";
        }

        public static string GetHairCastIconStringUnhashed(string id)
        {
            return icon + hairIcon + "cast_" + id + ".ice";
        }

        public static string GetHairManIconStringUnhashed(string id)
        {
            return icon + hairIcon + "man_" + id + ".ice";
        }

        public static string GetHairWomanIconStringUnhashed(string id)
        {
            return icon + hairIcon + "woman_" + id + ".ice";
        }

        public static string GetHornIconStringUnhashed(string id)
        {
            return icon + hornIcon + id + ".ice";
        }

        public static string GetTeethIconStringUnhashed(string id)
        {
            return icon + teethIcon + id + ".ice";
        }

        public static string GetEarIconStringUnhashed(string id)
        {
            return icon + earIcon + id + ".ice";
        }

        public static void GetCostumeOuterIconString(PartData part, string pso2_binDir, string finalId)
        {
            var iconName = icon + costumeIcon + finalId + ".ice";
            var iconHash = GetFileHash(iconName);
            var iconCastName = icon + castPartIcon + finalId + ".ice";
            var iconCastHash = GetFileHash(iconCastName);
            var iconOuterName = icon + outerwearIcon + GetIconGender(Int32.Parse(finalId)) + finalId + ".ice";
            var iconOuterHash = GetFileHash(iconOuterName);
            if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconHash)))
            {
                part.iconHash = iconHash;
                part.iconName = iconName;
            }
            else if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconCastHash)))
            {
                part.iconCastHash = iconCastHash;
                part.iconCastName = iconCastName;
            } else if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconOuterHash)))
            {
                part.iconOuterHash = iconOuterHash;
                part.iconOuterName = iconOuterName;
            }

            return;
        }

        public static string GetCostumeOuterIconString(string pso2_binDir, string finalId)
        {
            var iconName = icon + costumeIcon + finalId + ".ice";
            var iconHash = GetFileHash(iconName);
            var iconCastName = icon + castPartIcon + finalId + ".ice";
            var iconCastHash = GetFileHash(iconCastName);
            var iconOuterName = icon + outerwearIcon + GetIconGender(Int32.Parse(finalId)) + finalId + ".ice";
            var iconOuterHash = GetFileHash(iconOuterName);
            if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconHash)))
            {
                return iconHash;
            }
            else if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconCastHash)))
            {
                return iconCastHash;
            }
            else if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconOuterHash)))
            {
                return iconOuterHash;
            }

            return "";
        }

        public static Dictionary<int, string> GetFaceVariationLuaNameDict(string pso2_binDir, Dictionary<int, string> faceIds)
        {
            //Load faceVariationLua
            string faceVarPath = Path.Combine(pso2_binDir, dataDir, GetFileHash(classicCharCreate));

            if (File.Exists(faceVarPath))
            {
                var strm = new MemoryStream(File.ReadAllBytes(faceVarPath));
                var fVarIce = IceFile.LoadIceFile(strm);
                strm.Dispose();

                List<byte[]> files = (new List<byte[]>(fVarIce.groupOneFiles));
                files.AddRange(fVarIce.groupTwoFiles);

                //Loop through files to get what we need
                foreach (byte[] file in files)
                {
                    if (IceFile.getFileName(file).ToLower().Contains(faceVarName))
                    {
                        faceIds = ReadFaceVariationLua(file);
                    }
                }

                fVarIce = null;
            }

            return faceIds;
        }

        public static void ReadCMXText(string pso2_binDir, out PSO2Text partsText, out PSO2Text acceText, out PSO2Text commonText, out PSO2Text commonTextReboot)
        {
            //Load partsText
            string partsTextPath = Path.Combine(pso2_binDir, dataDir, GetFileHash(classicPartText));
            string partsTextPathNA = Path.Combine(pso2_binDir, dataNADir, GetFileHash(classicPartText));

            partsText = GetTextConditional(partsTextPath, partsTextPathNA, partsTextName);

            //Load acceText
            string acceTextPath = Path.Combine(pso2_binDir, dataDir, GetFileHash(classicAcceText));
            string acceTextPathhNA = Path.Combine(pso2_binDir, dataNADir, GetFileHash(classicAcceText));

            acceText = GetTextConditional(acceTextPath, acceTextPathhNA, acceTextName);

            //Load commonText
            string commonTextPath = Path.Combine(pso2_binDir, dataDir, GetFileHash(classicCommon));
            string commonTextPathhNA = Path.Combine(pso2_binDir, dataNADir, GetFileHash(classicCommon));

            commonText = GetTextConditional(commonTextPath, commonTextPathhNA, commonTextName);

            //Load commonText from reboot
            if (Directory.Exists(Path.Combine(pso2_binDir, dataReboot)))
            {
                string commonTextRebootPath = Path.Combine(pso2_binDir, dataReboot, GetRebootHash(GetFileHash(classicCommon)));
                string commonTextRebootPathhNA = Path.Combine(pso2_binDir, dataRebootNA, GetRebootHash(GetFileHash(classicCommon)));

                commonTextReboot = GetTextConditional(commonTextRebootPath, commonTextRebootPathhNA, commonTextName);
            }
            else
            {
                commonTextReboot = null;
            }
        }

        public static void ReadExtraText(string pso2_binDir, out PSO2Text actorNameText, out PSO2Text actorNameTextReboot, out PSO2Text actorNameTextReboot_NPC, out PSO2Text uiMyRoomText, out PSO2Text objectCommonText)
        {            
            //Load actor name text
            string actorNameTextPath = Path.Combine(pso2_binDir, dataDir, GetFileHash(classicActorName));
            string actorNameTextPathhNA = Path.Combine(pso2_binDir, dataNADir, GetFileHash(classicActorName));

            actorNameText = GetTextConditional(actorNameTextPath, actorNameTextPathhNA, actorNameName);

            //Load ui my room text
            string uiRoomTextPath = Path.Combine(pso2_binDir, dataDir, GetFileHash(uiRoomIce));
            string uiRoomTextPathhNA = Path.Combine(pso2_binDir, dataNADir, GetFileHash(uiRoomIce));
            uiMyRoomText = GetTextConditional(uiRoomTextPath, uiRoomTextPathhNA, uiRoomFilename);

            //Load reboot text
            if (Directory.Exists(Path.Combine(pso2_binDir, dataReboot)))
            {
                //Load reboot actor name text
                string actorNameTextRebootPath = Path.Combine(pso2_binDir, dataReboot, GetRebootHash(GetFileHash(rebootActorName)));
                string actorNameTextRebootPathhNA = Path.Combine(pso2_binDir, dataRebootNA, GetRebootHash(GetFileHash(rebootActorName)));
                actorNameTextReboot = GetTextConditional(actorNameTextRebootPath, actorNameTextRebootPathhNA, actorNameName);

                string actorNameTextRebootNPCPath = Path.Combine(pso2_binDir, dataReboot, GetRebootHash(GetFileHash(rebootActorNameNPC)));
                string actorNameTextRebootNPCPathhNA = Path.Combine(pso2_binDir, dataRebootNA, GetRebootHash(GetFileHash(rebootActorNameNPC)));
                actorNameTextReboot_NPC = GetTextConditional(actorNameTextRebootNPCPath, actorNameTextRebootNPCPathhNA, actorNameNPCName);

                //Load object common text
                string objCommonTextPath = Path.Combine(pso2_binDir, dataReboot, GetRebootHash(GetFileHash(objCommonIce)));
                string objCommonTextPathhNA = Path.Combine(pso2_binDir, dataRebootNA, GetRebootHash(GetFileHash(objCommonIce)));
                objectCommonText = GetTextConditional(objCommonTextPath, objCommonTextPathhNA, objCommonText);
            }
            else
            {
                actorNameTextReboot = null;
                actorNameTextReboot_NPC = null;
                objectCommonText = null;
            }
        }

        public class PartData
        {
            public int id;
            public int adjustedId;
            public List<string> namesByLanguage = new List<string>();
            public string iconName = "";
            public string iconHash = "";
            public string iconCastName = "";
            public string iconCastHash = "";
            public string iconOuterName = "";
            public string iconOuterHash = "";
            public string partName = "";
            public string partHash = "";
            public string partRpName = "";
            public string partRpHash = "";
            public string partExName = "";
            public string partExHash = "";
            public string partRpExName = "";
            public string partRpExHash = "";
            public string handsName = "";
            public string handsHash = "";
            public string handsExName = "";
            public string handsExHash = "";
            public string soundName = "";
            public string soundHash = "";
            public string castSoundName = "";
            public string castSoundHash = "";
            public string matAnimName = "";
            public string matAnimHash = "";
            public string matAnimExName = "";
            public string matAnimExHash = "";
            public string linkedInnerName = "";
            public string linkedInnerHash = "";
            public string linkedInnerExName = "";
            public string linkedInnerExHash = "";

            public void CopyFiles(string pso2BinDir, string destinationPso2BinDir)
            {
                CopyFile(pso2BinDir, destinationPso2BinDir, iconHash);
                CopyFile(pso2BinDir, destinationPso2BinDir, iconCastHash);
                CopyFile(pso2BinDir, destinationPso2BinDir, iconOuterHash);
                CopyFile(pso2BinDir, destinationPso2BinDir, partHash);
                CopyFile(pso2BinDir, destinationPso2BinDir, partRpHash);
                CopyFile(pso2BinDir, destinationPso2BinDir, partExHash);
                CopyFile(pso2BinDir, destinationPso2BinDir, partRpExHash);
                CopyFile(pso2BinDir, destinationPso2BinDir, handsHash);
                CopyFile(pso2BinDir, destinationPso2BinDir, handsExHash);
                CopyFile(pso2BinDir, destinationPso2BinDir, soundHash);
                CopyFile(pso2BinDir, destinationPso2BinDir, castSoundHash);
                CopyFile(pso2BinDir, destinationPso2BinDir, matAnimHash);
                CopyFile(pso2BinDir, destinationPso2BinDir, matAnimExHash);
                CopyFile(pso2BinDir, destinationPso2BinDir, linkedInnerHash);
                CopyFile(pso2BinDir, destinationPso2BinDir, linkedInnerExHash);
            }

            private void CopyFile(string pso2BinDir, string destinationPso2BinDir, string name)
            {
                var filePath = Path.Combine(pso2BinDir, dataDir, name);
                var destPath = Path.Combine(destinationPso2BinDir, dataDir, name);
                if (!File.Exists(destPath))
                {
                    if(File.Exists(filePath))
                    {
                        File.Copy(filePath, destPath);
                    }
                }
            }

            public string GetLine(bool hash = true)
            {
                if(hash)
                {
                    return GetLineHash();
                }
                
                return GetLineUnhashed();

            }
            public string GetLineAllIcons(bool hash = true)
            {
                if (hash)
                {
                    return GetLineHashAllIcons();
                }

                return GetLineUnhashedAllIcons();

            }

            private string GetLineHash()
            {
                string jpName = namesByLanguage.Count > 0 ? namesByLanguage[0] : "";
                string enName = namesByLanguage.Count > 1 ? namesByLanguage[1] : "";
                return $"{jpName},{enName},{id},{adjustedId},{GetIconStringHashed()},{partHash},{partExHash},{partRpHash},{partRpExHash},{linkedInnerHash},{linkedInnerExHash},{soundHash},{castSoundHash},{matAnimHash},{matAnimExHash},{handsHash},{handsExHash}\n";
            }

            private string GetLineUnhashed()
            {
                string jpName = namesByLanguage.Count > 0 ? namesByLanguage[0] : "";
                string enName = namesByLanguage.Count > 1 ? namesByLanguage[1] : "";
                return $"{jpName},{enName},{id},{adjustedId},{GetIconStringUnhashed()},{partName},{partExName},{partRpName},{partRpExName},{linkedInnerName},{linkedInnerExName},{soundName},{castSoundName},{matAnimName},{matAnimExName},{handsName},{handsExName}\n";
            }
            private string GetLineHashAllIcons()
            {
                string jpName = namesByLanguage.Count > 0 ? namesByLanguage[0] : "";
                string enName = namesByLanguage.Count > 1 ? namesByLanguage[1] : "";
                return $"{jpName},{enName},{id},{adjustedId},{iconHash},{iconOuterHash},{iconOuterHash},{partHash},{partExHash},{partRpHash},{partRpExHash},{linkedInnerHash},{linkedInnerExHash},{soundHash},{castSoundHash},{matAnimHash},{matAnimExHash},{handsHash},{handsExHash}\n";
            }

            private string GetLineUnhashedAllIcons()
            {
                string jpName = namesByLanguage.Count > 0 ? namesByLanguage[0] : "";
                string enName = namesByLanguage.Count > 1 ? namesByLanguage[1] : "";
                return $"{jpName},{enName},{id},{adjustedId},{iconName},{iconOuterName},{iconOuterName},{partName},{partExName},{partRpName},{partRpExName},{linkedInnerName},{linkedInnerExName},{soundName},{castSoundName},{matAnimName},{matAnimExName},{handsName},{handsExName}\n";
            }

            private string GetIconStringHashed()
            {
                if(iconHash != "" && iconHash != null)
                {
                    return iconHash;
                }
                else if (iconOuterHash != "" && iconOuterHash != null)
                {
                    return iconOuterHash;
                }
                else if (iconCastHash != "" && iconCastHash != null)
                {
                    return iconCastHash;
                }

                return "";
            }

            private string GetIconStringUnhashed()
            {
                if (iconName != "" && iconName != null)
                {
                    return iconName;
                }
                else if (iconOuterName != "" && iconOuterName != null)
                {
                    return iconOuterName;
                }
                else if (iconCastName != "" && iconCastName != null)
                {
                    return iconCastName;
                }

                return "";
            }
        }

        public unsafe static Dictionary<string, Dictionary<string, List<PartData>>> OutputCharacterPartFileStringDict(string pso2_binDir, CharacterMakingIndex aquaCMX, out int totalCount)
        {
            Dictionary<string, List<List<PSO2Text.textPair>>> textByCat = new Dictionary<string, List<List<PSO2Text.textPair>>>();
            Dictionary<int, string> faceIds = new Dictionary<int, string>();
            ReadCMXText(pso2_binDir, out var partsText, out var acceText, out var commonText, out var commonTextReboot);
            faceIds = GetFaceVariationLuaNameDict(pso2_binDir, faceIds);

            if (partsText != null)
            {
                for (int i = 0; i < partsText.text.Count; i++)
                {
                    textByCat.Add(partsText.categoryNames[i], partsText.text[i]);
                }
            }
            if (acceText != null)
            {
                for (int i = 0; i < acceText.text.Count; i++)
                {
                    //Handle dummy decoy entry in old versions
                    if (textByCat.ContainsKey(acceText.categoryNames[i]) && textByCat[acceText.categoryNames[i]][0].Count == 0)
                    {
                        textByCat[acceText.categoryNames[i]] = acceText.text[i];
                    }
                    else
                    {
                        textByCat.Add(acceText.categoryNames[i], acceText.text[i]);
                    }
                }
            }
            
            return GenerateCharacterPartFileStrings(pso2_binDir, aquaCMX, faceIds, textByCat, out totalCount);
        }

        //Returns a dictionary of part filenames, the top key string being the category and the next level's key string being the gender, race, and game variant (PSO2 vs NGS)
        //Total count is a count of all partsData entities, not potential filestrings, since this doesn't check for legitimacy here
        public static Dictionary<string, Dictionary<string, List<PartData>>> GenerateCharacterPartFileStrings(string pso2_binDir, CharacterMakingIndex aquaCMX, Dictionary<int, string> faceIds, Dictionary<string, List<List<PSO2Text.textPair>>> textByCat, out int totalCount)
        {
            totalCount = 0;
            Dictionary<string, Dictionary<string, List<PartData>>> partListsDict = new Dictionary<string, Dictionary<string, List<PartData>>>();

            List<int> masterIdList;
            List<string> masterNameList;
            List<Dictionary<int, string>> nameDicts;
            List<Dictionary<string, string>> strNameDicts;

            //---------------------------Parse out costume and body (includes outers and cast bodies)
            string costumeBodyKey = "costume";
            Dictionary<string, List<PartData>> costumeBodyDict = new Dictionary<string, List<PartData>>();
            List<PartData> outputCostumeMale = new List<PartData>();
            List<PartData> outputCostumeFemale = new List<PartData>();
            List<PartData> outputCastBody = new List<PartData>();
            List<PartData> outputCasealBody = new List<PartData>();
            List<PartData> outputOuterMale = new List<PartData>();
            List<PartData> outputOuterFemale = new List<PartData>();
            List<PartData> outputNGSOuterMale = new List<PartData>();
            List<PartData> outputNGSOuterFemale = new List<PartData>();
            List<PartData> outputNGSCastBody = new List<PartData>();
            List<PartData> outputNGSCasealBody = new List<PartData>();
            //List<PartData> outputNGSCostumeMale = new List<PartData>();   //Replaced by Set type basewear maybe?
            //List<PartData> outputNGSCostumeFemale = new List<PartData>();
            List<PartData> outputUnknownWearables = new List<PartData>();

            //Build text Dict
            masterIdList = new List<int>();
            nameDicts = new List<Dictionary<int, string>>();
            GatherTextIds(textByCat, masterIdList, nameDicts, "costume", true);
            GatherTextIds(textByCat, masterIdList, nameDicts, "body", false);

            //Add potential cmx ids that wouldn't be stored in this
            GatherDictKeys(masterIdList, aquaCMX.costumeDict.Keys);
            GatherDictKeys(masterIdList, aquaCMX.outerDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            //Check as well in pso2_bin directory if _rp version of outfit exists and note as well if there's a bm file for said bd file. (Hairs similar have hm files to complement hr files)
            //There may also be hn files for these while basewear would have ho files for hand textures
            foreach (int id in masterIdList)
            {
                PartData data = new PartData();
                data.id = id;
                bool named = false;
                foreach (var dict in nameDicts)
                {
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        named = true;
                        data.namesByLanguage.Add(str);
                    }
                    else
                    {
                        data.namesByLanguage.Add("");
                    }
                }

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    data.namesByLanguage.Add($"[Unnamed {id}]");
                }

                //Double check these ids and use an adjustedId if needed
                int adjustedId = id;
                if (aquaCMX.costumeIdLink.ContainsKey(id))
                {
                    adjustedId = aquaCMX.costumeIdLink[id].bcln.fileId;
                }
                else if (aquaCMX.outerWearIdLink.ContainsKey(id))
                {
                    adjustedId = aquaCMX.outerWearIdLink[id].bcln.fileId;
                }

                //Decide if bd or ow
                int soundId = -1;
                int linkedInnerId = id + 50000;
                string typeString = "bd_";
                bool classicOwCheck = id >= 20000 && id < 40000;
                bool rebootOwCheck = id >= 100000 && id < 300000;
                if (classicOwCheck == true || rebootOwCheck == true)
                {
                    typeString = "ow_";
                    if (aquaCMX.outerDict.ContainsKey(id))
                    {
                        soundId = aquaCMX.outerDict[id].body2.costumeSoundId;
                        linkedInnerId = aquaCMX.outerDict[id].body2.linkedInnerId;
                    }
                }
                else
                {
                    if (aquaCMX.costumeDict.ContainsKey(id))
                    {
                        soundId = aquaCMX.costumeDict[id].body2.costumeSoundId;
                        linkedInnerId = aquaCMX.costumeDict[id].body2.linkedInnerId;
                    }
                }

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    data.partName = $"{rebootStart}{typeString}{adjustedId}.ice";
                    data.partExName = $"{rebootExStart}{typeString}{adjustedId}_ex.ice";
                    data.partHash = GetFileHash(data.partName);
                    data.partExHash = GetFileHash(data.partExName);
                    data.linkedInnerName = $"{rebootStart}b1_{linkedInnerId}.ice";
                    data.linkedInnerExName = $"{rebootExStart}b1_{linkedInnerId}_ex.ice";
                    data.linkedInnerHash = GetFileHash(data.linkedInnerName);
                    data.linkedInnerExHash = GetFileHash(data.linkedInnerExName);

                    //Set icon string
                    data.iconName = icon + costumeIcon + id + ".ice";
                    data.iconHash = GetFileHash(data.iconName);
                    data.iconCastName = icon + castPartIcon + id + ".ice";
                    data.iconCastHash = GetFileHash(data.iconCastName);
                    data.iconOuterName = icon + outerwearIcon + GetIconGender(id) + id + ".ice";
                    data.iconOuterHash = GetFileHash(data.iconOuterName);
                    data.partRpName = data.partName.Replace(".ice", "_rp.ice");
                    data.matAnimName = data.partName.Replace("_" + typeString, "_bm_");
                    data.handsName = data.partName.Replace("_" + typeString, "_hn_");
                    data.partRpHash = GetFileHash(data.partRpName);
                    data.matAnimHash = GetFileHash(data.matAnimName);
                    data.handsHash = GetFileHash(data.handsName);

                    data.partRpExName = data.partExName.Replace("_ex.ice", "_rp_ex.ice");
                    data.matAnimExName = data.partExName.Replace("_" + typeString, "_bm_");
                    data.handsExName = data.partExName.Replace("_" + typeString, "_hn_");
                    data.partRpExHash = GetFileHash(data.partRpExName);
                    data.matAnimExHash = GetFileHash(data.matAnimExName);
                    data.handsExHash = GetFileHash(data.handsExName);

                    if(soundId != -1)
                    {
                        string soundFileUnhash;
                        if (soundId >= 100000)
                        {
                            soundFileUnhash = $"{rebootStart}bs_{soundId}.ice";
                        }
                        else
                        {
                            soundFileUnhash = $"{classicStart}bs_{soundId:D5}.ice";
                        }
                        data.soundName = soundFileUnhash;
                        data.soundHash = GetFileHash(soundFileUnhash);
                    }
                }
                else
                {
                    string finalId = $"{adjustedId:D5}";
                    string finalIdIcon = $"{id:D5}";

                    data.partName = $"{classicStart}{typeString}{finalId}.ice";
                    data.partHash = GetFileHash(data.partName);
                    data.iconName = icon + costumeIcon + finalIdIcon + ".ice";
                    data.iconHash = GetFileHash(data.iconName);
                    data.iconCastName = icon + castPartIcon + finalIdIcon + ".ice";
                    data.iconCastHash = GetFileHash(data.iconCastName);
                    data.iconOuterName = icon + outerwearIcon + GetIconGender(id) + finalIdIcon + ".ice";
                    data.iconOuterHash = GetFileHash(data.iconOuterName);
                    data.partRpName = data.partName.Replace(".ice", "_rp.ice");
                    data.matAnimName = data.partName.Replace("_" + typeString, "_bm_");
                    data.handsName = data.partName.Replace("_" + typeString, "_hn_");
                    data.partRpHash = GetFileHash(data.partRpName);
                    data.matAnimHash = GetFileHash(data.matAnimName);
                    data.handsHash = GetFileHash(data.handsName);

                    if (soundId != -1)
                    {
                        string soundFileUnhash;
                        if (soundId >= 100000)
                        {
                            soundFileUnhash = $"{rebootStart}bs_{soundId}.ice";
                        }
                        else
                        {
                            soundFileUnhash = $"{classicStart}bs_{soundId:D5}.ice";
                        }
                        data.soundName = soundFileUnhash;
                        data.soundHash = GetFileHash(soundFileUnhash);
                    }
                }

                //Decide which type this is
                if (id < 10000)
                {
                    outputCostumeMale.Add(data);
                }
                else if (id < 20000)
                {
                    outputCostumeFemale.Add(data);
                }
                else if (id < 30000)
                {
                    outputOuterMale.Add(data);
                }
                else if (id < 40000)
                {
                    outputOuterFemale.Add(data);
                }
                else if (id < 50000)
                {
                    outputCastBody.Add(data);
                }
                else if (id < 100000)
                {
                    outputCasealBody.Add(data);
                }
                else if (id < 200000)
                {
                    outputNGSOuterMale.Add(data);
                }
                else if (id < 300000)
                {
                    outputNGSOuterFemale.Add(data);
                }
                else if (id < 400000)
                {
                    outputNGSCastBody.Add(data);
                }
                else if (id < 500000)
                {
                    outputNGSCasealBody.Add(data);
                }
                else
                {
                    outputUnknownWearables.Add(data);
                }
                totalCount++;
            }
            costumeBodyDict.Add("MaleCostumes", outputCostumeMale);
            costumeBodyDict.Add("FemaleCostumes", outputCostumeFemale);
            costumeBodyDict.Add("MaleOuters", outputOuterMale);
            costumeBodyDict.Add("FemaleOuters", outputOuterFemale);
            costumeBodyDict.Add("CastBodies", outputCastBody);
            costumeBodyDict.Add("CasealBodies", outputCasealBody);
            costumeBodyDict.Add("NGSMaleOuters", outputNGSOuterMale);
            costumeBodyDict.Add("NGSFemaleOuters", outputNGSOuterFemale);
            costumeBodyDict.Add("NGSCastBodies", outputNGSCastBody);
            costumeBodyDict.Add("NGSCasealBodies", outputNGSCasealBody);

            if (outputUnknownWearables.Count > 0)
            {
                costumeBodyDict.Add("UnknownOutfits", outputUnknownWearables);
            }
            partListsDict.Add(costumeBodyKey, costumeBodyDict);

            //---------------------------Parse out basewear
            string basewearKey = "basewear";
            Dictionary<string, List<PartData>> basewearDict = new Dictionary<string, List<PartData>>();
            List<PartData> outputBasewearMale = new List<PartData>();
            List<PartData> outputBasewearFemale = new List<PartData>();
            List<PartData> outputNGSBasewearMale = new List<PartData>();
            List<PartData> outputNGSBasewearFemale = new List<PartData>();
            List<PartData> outputNGSGenderlessBasewear = new List<PartData>();

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "basewear", true);

            //Add potential cmx ids that wouldn't be stored in this
            GatherDictKeys(masterIdList, aquaCMX.baseWearDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            //Check as well in pso2_bin directory if _rp version of outfit exists and note as well if there's a bm file for said bd file. (Hairs similar have hm files to complement hr files)
            //There may also be hn files for these while basewear would have ho files for hand textures
            foreach (int id in masterIdList)
            {
                PartData data = new PartData();
                data.id = id;
                bool named = false;
                foreach (var dict in nameDicts)
                {
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        named = true;
                        data.namesByLanguage.Add(str);
                    }
                    else
                    {
                        data.namesByLanguage.Add("");
                    }
                }

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    data.namesByLanguage.Add($"[Unnamed {id}]");
                }

                //Get SoundID
                int soundId = -1;
                int linkedInnerId = -1;
                if (aquaCMX.baseWearDict.ContainsKey(id))
                {
                    soundId = aquaCMX.baseWearDict[id].body2.costumeSoundId;
                    linkedInnerId = aquaCMX.baseWearDict[id].body2.linkedInnerId;
                }

                //Double check these ids and use an adjustedId if needed
                int adjustedId = id;
                if (aquaCMX.baseWearIdLink.ContainsKey(id))
                {
                    adjustedId = aquaCMX.baseWearIdLink[id].bcln.fileId;
                }
                //Decide if it needs to be handled as a reboot file or not
                var typeString = "bw_";
                if (id >= 100000)
                {
                    data.partName = $"{rebootStart}{typeString}{adjustedId}.ice";
                    data.partExName = $"{rebootExStart}{typeString}{adjustedId}_ex.ice";
                    data.partHash = GetFileHash(data.partName);
                    data.partExHash = GetFileHash(data.partExName);
                    data.linkedInnerName = $"{rebootStart}b1_{linkedInnerId}.ice";
                    data.linkedInnerExName = $"{rebootExStart}b1_{linkedInnerId}_ex.ice";
                    data.linkedInnerHash = GetFileHash(data.linkedInnerName);
                    data.linkedInnerExHash = GetFileHash(data.linkedInnerExName);

                    //Set icon string
                    data.iconName = icon + basewearIcon + GetIconGender(id) + id + ".ice";
                    data.iconHash = GetFileHash(data.iconName);
                    data.iconCastName = icon + castPartIcon + id + ".ice";
                    data.iconCastHash = GetFileHash(data.iconCastName);
                    data.iconOuterName = icon + outerwearIcon + GetIconGender(id) + id + ".ice";
                    data.iconOuterHash = GetFileHash(data.iconOuterName);
                    data.partRpName = data.partName.Replace(".ice", "_rp.ice");
                    data.handsName = data.partName.Replace("_" + typeString, "_ho_");
                    data.partRpHash = GetFileHash(data.partRpName);
                    data.matAnimHash = GetFileHash(data.matAnimName);
                    data.handsHash = GetFileHash(data.handsName);

                    data.partRpExName = data.partExName.Replace("_ex.ice", "_rp_ex.ice");
                    data.handsExName = data.partExName.Replace("_" + typeString, "_ho_");
                    data.partRpExHash = GetFileHash(data.partRpExName);
                    data.matAnimExHash = GetFileHash(data.matAnimExName);
                    data.handsExHash = GetFileHash(data.handsExName);

                    if(soundId != -1)
                    {
                        string soundFileUnhash;
                        if (soundId >= 100000)
                        {
                            soundFileUnhash = $"{rebootStart}bs_{soundId}.ice";
                        }
                        else
                        {
                            soundFileUnhash = $"{classicStart}bs_{soundId:D5}.ice";
                        }
                        data.soundName = soundFileUnhash;
                        data.soundHash = GetFileHash(soundFileUnhash);
                    }
                }
                else
                {
                    string finalId = $"{adjustedId:D5}";
                    string finalIdIcon = $"{id:D5}";

                    data.partName = $"{classicStart}{typeString}{finalId}.ice";
                    data.partHash = GetFileHash(data.partName);
                    data.iconName = icon + basewearIcon + GetIconGender(id) + finalIdIcon + ".ice";
                    data.iconHash = GetFileHash(data.iconName);
                    data.iconCastName = icon + castPartIcon + finalIdIcon + ".ice";
                    data.iconCastHash = GetFileHash(data.iconCastName);
                    data.iconOuterName = icon + outerwearIcon + GetIconGender(id) + finalIdIcon + ".ice";
                    data.iconOuterHash = GetFileHash(data.iconOuterName);
                    data.partRpName = data.partName.Replace(".ice", "_rp.ice");
                    data.handsName = data.partName.Replace("_" + typeString, "_hn_");
                    data.partRpHash = GetFileHash(data.partRpName);
                    data.handsHash = GetFileHash(data.handsName);

                    if (soundId != -1)
                    {
                        string soundFileUnhash;
                        if (soundId >= 100000)
                        {
                            soundFileUnhash = $"{rebootStart}bs_{soundId}.ice";
                        }
                        else
                        {
                            soundFileUnhash = $"{classicStart}bs_{soundId:D5}.ice";
                        }
                        data.soundName = soundFileUnhash;
                        data.soundHash = GetFileHash(soundFileUnhash);
                    }
                }

                //Decide which type this is
                if (id < 30000)
                {
                    outputBasewearMale.Add(data);
                }
                else if (id < 40000)
                {
                    outputBasewearFemale.Add(data);
                }
                else if (id < 200000)
                {
                    outputNGSBasewearMale.Add(data);
                }
                else if (id < 300000)
                {
                    outputNGSBasewearFemale.Add(data);
                }
                else if (id < 600000)
                {
                    outputNGSGenderlessBasewear.Add(data);
                }
                else
                {
                    Console.WriteLine("Unknown bw with id: " + id);
                }
                totalCount++;
            }
            basewearDict.Add("MaleBasewear", outputBasewearMale);
            basewearDict.Add("FemaleBasewear", outputBasewearFemale);
            basewearDict.Add("MaleNGSBasewear", outputNGSBasewearMale);
            basewearDict.Add("FemaleNGSBasewear", outputNGSBasewearFemale);
            basewearDict.Add("GenderlessNGSBasewear", outputNGSGenderlessBasewear);
            partListsDict.Add(basewearKey, basewearDict);

            //---------------------------Parse out innerwear
            string innerwearKey = "innerwear";
            Dictionary<string, List<PartData>> innerWearDict = new Dictionary<string, List<PartData>>();
            List<PartData> outputInnerwearMale = new List<PartData>();
            List<PartData> outputInnerwearFemale = new List<PartData>();
            List<PartData> outputNGSInnerwearMale = new List<PartData>();
            List<PartData> outputNGSInnerwearFemale = new List<PartData>();

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "innerwear", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.innerWearDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                PartData data = new PartData();
                data.id = id;
                bool named = false;
                foreach (var dict in nameDicts)
                {
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        named = true;
                        data.namesByLanguage.Add(str);
                    }
                    else
                    {
                        data.namesByLanguage.Add("");
                    }
                }

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    data.namesByLanguage.Add($"[Unnamed {id}]");
                }

                //Double check these ids and use an adjustedId if needed
                int adjustedId = id;
                if (aquaCMX.innerWearIdLink.ContainsKey(id))
                {
                    adjustedId = aquaCMX.innerWearIdLink[id].bcln.fileId;
                }
                //Decide if it needs to be handled as a reboot file or not
                string typeString = "iw_";
                if (id >= 100000)
                {
                    data.partName = $"{rebootStart}{typeString}{adjustedId}.ice";
                    data.partExName = $"{rebootExStart}{typeString}{adjustedId}_ex.ice";
                    data.partHash = GetFileHash(data.partName);
                    data.partExHash = GetFileHash(data.partExName);

                    //Set icon string
                    data.iconName = icon + innerwearIcon + GetIconGender(id) + id + ".ice";
                    data.iconHash = GetFileHash(data.iconName);
                }
                else
                {
                    string finalId = $"{adjustedId:D5}";
                    string finalIdIcon = $"{id:D5}";

                    data.partName = $"{classicStart}{typeString}{finalId}.ice";
                    data.partHash = GetFileHash(data.partName);

                    //Set icon string
                    data.iconName = icon + innerwearIcon + GetIconGender(id) + finalIdIcon + ".ice";
                    data.iconHash = GetFileHash(data.iconName);

                }

                //Decide which type this is
                if (id < 30000)
                {
                    outputInnerwearMale.Add(data);
                }
                else if (id < 40000)
                {
                    outputInnerwearFemale.Add(data);
                }
                else if (id < 200000)
                {
                    outputNGSInnerwearMale.Add(data);
                }
                else if (id < 300000)
                {
                    outputNGSInnerwearFemale.Add(data);
                }
                else
                {
                    Console.WriteLine("Unknown iw with id: " + id);
                }
                totalCount++;
            }
            innerWearDict.Add("MaleInnerwear", outputInnerwearMale);
            innerWearDict.Add("FemaleInnerwear", outputInnerwearFemale);
            innerWearDict.Add("MaleNGSInnerwear", outputNGSInnerwearMale);
            innerWearDict.Add("FemaleNGSInnerwear", outputNGSInnerwearFemale);
            partListsDict.Add(innerwearKey, innerWearDict);

            //---------------------------Parse out cast arms
            string castArmKey = "castArm";
            Dictionary<string, List<PartData>> castArmDict = new Dictionary<string, List<PartData>>();
            List<PartData> outputCastArmMale = new List<PartData>();
            List<PartData> outputCastArmFemale = new List<PartData>();
            List<PartData> outputNGSCastArmMale = new List<PartData>();
            List<PartData> outputNGSCastArmFemale = new List<PartData>();

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "arm", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.carmDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                PartData data = new PartData();
                data.id = id;
                bool named = false;
                foreach (var dict in nameDicts)
                {
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        named = true;
                        data.namesByLanguage.Add(str);
                    }
                    else
                    {
                        data.namesByLanguage.Add("");
                    }
                }

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    data.namesByLanguage.Add($"[Unnamed {id}]");
                }

                //Double check these ids and use an adjustedId if needed
                int adjustedId = id;
                if (aquaCMX.castArmIdLink.ContainsKey(id))
                {
                    adjustedId = aquaCMX.castArmIdLink[id].bcln.fileId;
                }
                int linkedInnerId = -1;
                if (aquaCMX.carmDict.ContainsKey(id))
                {
                    linkedInnerId = aquaCMX.carmDict[id].body2.linkedInnerId;
                }
                //Decide if it needs to be handled as a reboot file or not
                string typeString = "am_";
                if (id >= 100000)
                {
                    data.partName = $"{rebootStart}{typeString}{adjustedId}.ice";
                    data.partExName = $"{rebootExStart}{typeString}{adjustedId}_ex.ice";
                    data.partHash = GetFileHash(data.partName);
                    data.partExHash = GetFileHash(data.partExName);
                    data.linkedInnerName = $"{rebootStart}b1_{linkedInnerId}.ice";
                    data.linkedInnerExName = $"{rebootExStart}b1_{linkedInnerId}_ex.ice";
                    data.linkedInnerHash = GetFileHash(data.linkedInnerName);
                    data.linkedInnerExHash = GetFileHash(data.linkedInnerExName);

                    //Set icon string
                    data.iconName = icon + castPartIcon + castArmIcon + id + ".ice";
                    data.iconHash = GetFileHash(data.iconName);
                }
                else
                {
                    string finalId = $"{adjustedId:D5}";
                    string finalIdIcon = $"{id:D5}";
                    data.partName = $"{classicStart}{typeString}{finalId}.ice";
                    data.partHash = GetFileHash(data.partName);

                    //Set icon string
                    data.iconName = icon + castPartIcon + castArmIcon + finalIdIcon + ".ice";
                    data.iconHash = GetFileHash(data.iconName);
                }

                //Decide which type this is
                if (id < 50000)
                {
                    outputCastArmMale.Add(data);
                }
                else if (id < 60000)
                {
                    outputCastArmFemale.Add(data);
                }
                else if (id < 400000)
                {
                    outputNGSCastArmMale.Add(data);
                }
                else if (id < 500000)
                {
                    outputNGSCastArmFemale.Add(data);
                }
                else
                {
                    Console.WriteLine("Unknown am with id: " + id);
                }
                totalCount++;
            }
            castArmDict.Add("CastArms", outputCastArmMale);
            castArmDict.Add("CasealArms", outputCastArmFemale);
            castArmDict.Add("CastArmsNGS", outputNGSCastArmMale);
            castArmDict.Add("CasealArmsNGS", outputNGSCastArmFemale);
            partListsDict.Add(castArmKey, castArmDict);

            //---------------------------Parse out cast legs
            string castLegKey = "castLeg";
            Dictionary<string, List<PartData>> castLegDict = new Dictionary<string, List<PartData>>();
            List<PartData> outputCastLegMale = new List<PartData>();
            List<PartData> outputCastLegFemale = new List<PartData>();
            List<PartData> outputNGSCastLegMale = new List<PartData>();
            List<PartData> outputNGSCastLegFemale = new List<PartData>();

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "Leg", true); //Yeah for some reason this string starts capitalized while none of the others do... don't ask me.

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.clegDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                PartData data = new PartData();
                data.id = id;
                bool named = false;
                foreach (var dict in nameDicts)
                {
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        named = true;
                        data.namesByLanguage.Add(str);
                    }
                    else
                    {
                        data.namesByLanguage.Add("");
                    }
                }

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    data.namesByLanguage.Add($"[Unnamed {id}]");
                }

                //Double check these ids and use an adjustedId if needed
                int adjustedId = id;
                if (aquaCMX.clegIdLink.ContainsKey(id))
                {
                    adjustedId = aquaCMX.clegIdLink[id].bcln.fileId;
                }
                int soundId = -1;
                int linkedInnerId = -1;
                if (aquaCMX.clegDict.ContainsKey(id))
                {
                    soundId = aquaCMX.clegDict[id].body2.costumeSoundId;
                    linkedInnerId = aquaCMX.clegDict[id].body2.linkedInnerId;
                }
                //Decide if it needs to be handled as a reboot file or not
                string typeString = "lg_";
                if (id >= 100000)
                {
                    data.partName = $"{rebootStart}{typeString}{adjustedId}.ice";
                    data.partExName = $"{rebootExStart}{typeString}{adjustedId}_ex.ice";
                    data.partHash = GetFileHash(data.partName);
                    data.partExHash = GetFileHash(data.partExName);
                    data.linkedInnerName = $"{rebootStart}b1_{linkedInnerId}.ice";
                    data.linkedInnerExName = $"{rebootExStart}b1_{linkedInnerId}_ex.ice";
                    data.linkedInnerHash = GetFileHash(data.linkedInnerName);
                    data.linkedInnerExHash = GetFileHash(data.linkedInnerExName);

                    //Set icon string
                    data.iconName = icon + castPartIcon + castLegIcon + id + ".ice";
                    data.iconHash = GetFileHash(data.iconName);
                }
                else
                {
                    string finalId = $"{adjustedId:D5}";
                    string finalIdIcon = $"{id:D5}";
                    data.partName = $"{classicStart}{typeString}{finalId}.ice";
                    data.partHash = GetFileHash(data.partName);

                    //Set icon string
                    data.iconName = icon + castPartIcon + castLegIcon + finalIdIcon + ".ice";
                    data.iconHash = GetFileHash(data.iconName);
                }
                if (soundId != -1)
                {
                    string soundFileUnhash = "";
                    string castSoundFileUnhash = "";
                    if (soundId >= 100000)
                    {
                        soundFileUnhash = $"{rebootStart}bs_{soundId}.ice";
                    }
                    else
                    {
                        soundFileUnhash = $"{classicStart}bs_{soundId:D5}.ice";
                        castSoundFileUnhash = $"{classicStart}ls_{soundId:D5}.ice";
                    }
                    data.soundName = soundFileUnhash;
                    data.soundHash = GetFileHash(soundFileUnhash);
                    data.castSoundName = castSoundFileUnhash;
                    data.castSoundHash = GetFileHash(castSoundFileUnhash);
                }

                //Decide which type this is
                if (id < 50000)
                {
                    outputCastLegMale.Add(data);
                }
                else if (id < 60000)
                {
                    outputCastLegFemale.Add(data);
                }
                else if (id < 400000)
                {
                    outputNGSCastLegMale.Add(data);
                }
                else if (id < 500000)
                {
                    outputNGSCastLegFemale.Add(data);
                }
                else
                {
                    Console.WriteLine("Unknown lg with id: " + id);
                }
                totalCount++;
            }
            castLegDict.Add("CastLegs", outputCastLegMale);
            castLegDict.Add("CasealLegs", outputCastLegFemale);
            castLegDict.Add("CastLegsNGS", outputNGSCastLegMale);
            castLegDict.Add("CasealLegsNGS", outputNGSCastLegFemale);
            partListsDict.Add(castLegKey, castLegDict);

            //---------------------------Parse out body paint
            string bodyPaintKey = "bodyPaint";
            Dictionary<string, List<PartData>> bodyPaintDict = new Dictionary<string, List<PartData>>();
            List<PartData> outputMaleBodyPaint = new List<PartData>();
            List<PartData> outputFemaleBodyPaint = new List<PartData>();
            List<PartData> outputMaleLayeredBodyPaint = new List<PartData>();
            List<PartData> outputFemaleLayeredBodyPaint = new List<PartData>();
            List<PartData> outputNGSMaleBodyPaint = new List<PartData>();
            List<PartData> outputNGSFemaleBodyPaint = new List<PartData>();
            List<PartData> outputNGSCastMaleBodyPaint = new List<PartData>();
            List<PartData> outputNGSCastFemaleBodyPaint = new List<PartData>();
            List<PartData> outputNGSGenderlessBodyPaint = new List<PartData>();

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "bodypaint1", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.bodyPaintDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                PartData data = new PartData();
                data.id = id;
                bool named = false;
                foreach (var dict in nameDicts)
                {
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        named = true;
                        data.namesByLanguage.Add(str);
                    }
                    else
                    {
                        data.namesByLanguage.Add("");
                    }
                }

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    data.namesByLanguage.Add($"[Unnamed {id}]");
                }

                //Decide if it needs to be handled as a reboot file or not
                string typeString = "b1_";
                if (id >= 100000)
                {
                    data.partName = $"{rebootStart}{typeString}{id}.ice";
                    data.partExName = $"{rebootExStart}{typeString}{id}_ex.ice";
                    data.partHash = GetFileHash(data.partName);
                    data.partExHash = GetFileHash(data.partExName);

                    //Set icon string
                    data.iconName = icon + bodyPaintIcon + id + ".ice";
                    data.iconHash = GetFileHash(data.iconName);
                }
                else
                {
                    string finalId = $"{id:D5}";
                    string finalIdIcon = $"{id:D5}";
                    data.partName = $"{classicStart}{typeString}{finalId}.ice";
                    data.partHash = GetFileHash(data.partName);

                    //Set icon string
                    data.iconName = icon + bodyPaintIcon + finalIdIcon + ".ice";
                    data.iconHash = GetFileHash(data.iconName);
                }

                //Decide which type this is
                if (id < 10000)
                {
                    outputMaleBodyPaint.Add(data);
                }
                else if (id < 20000)
                {
                    outputFemaleBodyPaint.Add(data);
                }
                else if (id < 30000)
                {
                    outputMaleLayeredBodyPaint.Add(data);
                }
                else if (id < 100000)
                {
                    outputFemaleLayeredBodyPaint.Add(data);
                }
                else if (id < 200000)
                {
                    outputNGSMaleBodyPaint.Add(data);
                }
                else if (id < 300000)
                {
                    outputNGSFemaleBodyPaint.Add(data);
                }
                else if (id < 400000)
                {
                    outputNGSCastMaleBodyPaint.Add(data);
                }
                else if (id < 500000)
                {
                    outputNGSCastFemaleBodyPaint.Add(data);
                }
                else if (id < 600000)
                {
                    outputNGSGenderlessBodyPaint.Add(data);
                }
                else
                {
                    Console.WriteLine("Unknown b1 with id: " + id);
                }
                totalCount++;
            }
            bodyPaintDict.Add("MaleBodyPaint", outputMaleBodyPaint);
            bodyPaintDict.Add("FemaleBodyPaint", outputFemaleBodyPaint);
            bodyPaintDict.Add("MaleLayeredBodyPaint", outputMaleLayeredBodyPaint);
            bodyPaintDict.Add("FemaleLayeredBodyPaint", outputFemaleLayeredBodyPaint);
            bodyPaintDict.Add("MaleNGSBodyPaint", outputNGSMaleBodyPaint);
            bodyPaintDict.Add("FemaleNGSBodyPaint", outputNGSFemaleBodyPaint);
            bodyPaintDict.Add("CastNGSBodyPaint", outputNGSCastMaleBodyPaint);
            bodyPaintDict.Add("CasealNGSBodyPaint", outputNGSCastFemaleBodyPaint);
            bodyPaintDict.Add("GenderlessNGSBodyPaint", outputNGSGenderlessBodyPaint);
            partListsDict.Add(bodyPaintKey, bodyPaintDict);

            //---------------------------Parse out stickers
            string stickerKey = "stickers";
            Dictionary<string, List<PartData>> stickerDict = new Dictionary<string, List<PartData>>();
            List<PartData> outputStickers = new List<PartData>();

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "bodypaint2", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.stickerDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                PartData data = new PartData();
                data.id = id;
                bool named = false;
                foreach (var dict in nameDicts)
                {
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        named = true;
                        data.namesByLanguage.Add(str);
                    }
                    else
                    {
                        data.namesByLanguage.Add("");
                    }
                }

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    data.namesByLanguage.Add($"[Unnamed {id}]");
                }

                //Decide if it needs to be handled as a reboot file or not
                string typeString = "b2_";
                if (id >= 100000)
                {
                    data.partName = $"{rebootStart}{typeString}{id}.ice";
                    data.partExName = $"{rebootExStart}{typeString}{id}_ex.ice";
                    data.partHash = GetFileHash(data.partName);
                    data.partExHash = GetFileHash(data.partExName);

                    //Set icon string
                    data.iconName = icon + stickerIcon + id + ".ice";
                    data.iconHash = GetFileHash(data.iconName);
                }
                else
                {
                    string finalId = $"{id:D5}";
                    string finalIdIcon = $"{id:D5}";
                    data.partName = $"{classicStart}{typeString}{finalId}.ice";
                    data.partHash = GetFileHash(data.partName);

                    //Set icon string
                    data.iconName = icon + stickerIcon + finalIdIcon + ".ice";
                    data.iconHash = GetFileHash(data.iconName);
                }

                outputStickers.Add(data);
                totalCount++;
            }
            stickerDict.Add("Stickers", outputStickers);
            partListsDict.Add(stickerKey, stickerDict);

            //---------------------------Parse out hair
            string hairKey = "hair";
            Dictionary<string, List<PartData>> hairDict = new Dictionary<string, List<PartData>>();
            List<PartData> outputMaleHair = new List<PartData>();
            List<PartData> outputFemaleHair = new List<PartData>();
            List<PartData> outputCasealHair = new List<PartData>();
            List<PartData> outputNGSHair = new List<PartData>();

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "hair", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.hairDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                PartData data = new PartData();
                data.id = id;
                bool named = false;
                foreach (var dict in nameDicts)
                {
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        named = true;
                        data.namesByLanguage.Add(str);
                    }
                    else
                    {
                        data.namesByLanguage.Add("");
                    }
                }

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    data.namesByLanguage.Add($"[Unnamed {id}]");
                }

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    data.partName = $"{rebootStart}hr_{id}.ice";
                    data.partExName = $"{rebootExStart}hr_{id}_ex.ice";

                    //Cast heads
                    if (id >= 300000 && id < 500000)
                    {
                        data.partName = data.partName.Replace("hr", "fc");
                        data.partExName = data.partExName.Replace("hr", "fc");
                    }
                    data.partHash = GetFileHash(data.partName);
                    data.partExHash = GetFileHash(data.partExName);

                    //Cast heads
                    if (id >= 300000 && id < 500000)
                    {
                        data.iconName = "_";
                        data.iconOuterName = "_";
                        data.iconCastName = icon + faceIcon + id + ".ice";
                    }
                    else
                    {
                        data.iconName = icon + hairIcon + iconMale + id + ".ice";
                        data.iconOuterName = icon + hairIcon + iconFemale + id + ".ice";
                        data.iconCastName = icon + hairIcon + iconCast + id + ".ice";
                    }
                    data.iconHash = GetFileHash(data.iconName);
                    data.iconOuterHash = GetFileHash(data.iconOuterName);
                    data.iconCastHash = GetFileHash(data.iconCastName);
                }
                else
                {
                    string finalId = $"{id:D5}";
                    data.partName = $"{classicStart}hr_{finalId}.ice";

                    //Set icon string
                    data.iconName = icon + hairIcon + iconMale + finalId + ".ice";
                    data.iconOuterName = icon + hairIcon + iconFemale + finalId + ".ice";
                    data.iconCastName = icon + hairIcon + iconCast + finalId + ".ice";

                    data.partHash = GetFileHash(data.partName);
                    data.iconHash = GetFileHash(data.iconName);
                    data.iconOuterHash = GetFileHash(data.iconOuterName);
                    data.iconCastHash = GetFileHash(data.iconCastName);
                }

                //Decide which type this is
                if (id < 10000)
                {
                    outputMaleHair.Add(data);
                }
                else if (id < 20000)
                {
                    outputFemaleHair.Add(data);
                }
                else if (id < 60000)
                {
                    outputCasealHair.Add(data);
                }
                else
                {
                    outputNGSHair.Add(data);
                }
                totalCount++;
            }
            hairDict.Add("MaleHair", outputMaleHair);
            hairDict.Add("FemaleHair", outputFemaleHair);
            hairDict.Add("CasealHair", outputCasealHair);
            hairDict.Add("AllHairNGS", outputNGSHair);
            partListsDict.Add(hairKey, hairDict);

            //---------------------------Parse out Eye
            string eyeKey = "eye";
            Dictionary<string, List<PartData>> eyeDict = new Dictionary<string, List<PartData>>();
            List<PartData> outputEyes = new List<PartData>();
            List<PartData> outputNGSEyes = new List<PartData>();

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "eye", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.eyeDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                PartData data = new PartData();
                data.id = id;
                bool named = false;
                foreach (var dict in nameDicts)
                {
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        named = true;
                        data.namesByLanguage.Add(str);
                    }
                    else
                    {
                        data.namesByLanguage.Add("");
                    }
                }

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    data.namesByLanguage.Add($"[Unnamed {id}]");
                }

                //Decide if it needs to be handled as a reboot file or not
                string typeString = "ey_";
                if (id >= 100000)
                {
                    data.partName = $"{rebootStart}{typeString}{id}.ice";
                    data.partExName = $"{rebootExStart}{typeString}{id}_ex.ice";
                    data.partHash = GetFileHash(data.partName);
                    data.partExHash = GetFileHash(data.partExName);

                    //Set icon string
                    data.iconName = icon + eyeIcon + id + ".ice";
                    data.iconHash = GetFileHash(data.iconName);
                }
                else
                {
                    string finalId = $"{id:D5}";
                    string finalIdIcon = $"{id:D5}";
                    data.partName = $"{classicStart}{typeString}{finalId}.ice";
                    data.partHash = GetFileHash(data.partName);

                    //Set icon string
                    data.iconName = icon + eyeIcon + finalIdIcon + ".ice";
                    data.iconHash = GetFileHash(data.iconName);
                }

                if (id < 100000)
                {
                    outputEyes.Add(data);
                }
                else
                {
                    outputNGSEyes.Add(data);
                }
                totalCount++;
            }
            eyeDict.Add("Eyes", outputEyes);
            eyeDict.Add("EyesNGS", outputNGSEyes);
            partListsDict.Add(eyeKey, eyeDict);

            //---------------------------Parse out EYEB
            string eyeBKey = "eyebrows";
            Dictionary<string, List<PartData>> eyeBDict = new Dictionary<string, List<PartData>>();
            List<PartData> outputEyebrows = new List<PartData>();
            List<PartData> outputNGSEyebrows = new List<PartData>();

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "eyebrows", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.eyebrowDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                PartData data = new PartData();
                data.id = id;
                bool named = false;
                foreach (var dict in nameDicts)
                {
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        named = true;
                        data.namesByLanguage.Add(str);
                    }
                    else
                    {
                        data.namesByLanguage.Add("");
                    }
                }

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    data.namesByLanguage.Add($"[Unnamed {id}]");
                }

                //Decide if it needs to be handled as a reboot file or not
                string typeString = "eb_";
                if (id >= 100000)
                {
                    data.partName = $"{rebootStart}{typeString}{id}.ice";
                    data.partExName = $"{rebootExStart}{typeString}{id}_ex.ice";
                    data.partHash = GetFileHash(data.partName);
                    data.partExHash = GetFileHash(data.partExName);

                    //Set icon string
                    data.iconName = icon + eyeBrowsIcon + id + ".ice";
                    data.iconHash = GetFileHash(data.iconName);
                }
                else
                {
                    string finalId = $"{id:D5}";
                    string finalIdIcon = $"{id:D5}";
                    data.partName = $"{classicStart}{typeString}{finalId}.ice";
                    data.partHash = GetFileHash(data.partName);

                    //Set icon string
                    data.iconName = icon + eyeBrowsIcon + finalIdIcon + ".ice";
                    data.iconHash = GetFileHash(data.iconName);
                }

                if (id <= 100000)
                {
                    outputEyebrows.Add(data);
                }
                else
                {
                    outputNGSEyebrows.Add(data);
                }
                totalCount++;
            }
            eyeBDict.Add("Eyebrows", outputEyebrows);
            eyeBDict.Add("EyebrowsNGS", outputNGSEyebrows);
            partListsDict.Add(eyeBKey, eyeBDict);

            //---------------------------Parse out EYEL
            string eyeLKey = "eyelashes";
            Dictionary<string, List<PartData>> eyeLDict = new Dictionary<string, List<PartData>>();
            List<PartData> outputEyelashes = new List<PartData>();
            List<PartData> outputNGSEyelashes = new List<PartData>();

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "eyelashes", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.eyelashDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                PartData data = new PartData();
                data.id = id;
                bool named = false;
                foreach (var dict in nameDicts)
                {
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        named = true;
                        data.namesByLanguage.Add(str);
                    }
                    else
                    {
                        data.namesByLanguage.Add("");
                    }
                }

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    data.namesByLanguage.Add($"[Unnamed {id}]");
                }

                //Decide if it needs to be handled as a reboot file or not
                string typeString = "el_";
                if (id >= 100000)
                {
                    data.partName = $"{rebootStart}{typeString}{id}.ice";
                    data.partExName = $"{rebootExStart}{typeString}{id}_ex.ice";
                    data.partHash = GetFileHash(data.partName);
                    data.partExHash = GetFileHash(data.partExName);

                    //Set icon string
                    data.iconName = icon + eyelashesIcon + id + ".ice";
                    data.iconHash = GetFileHash(data.iconName);
                }
                else
                {
                    string finalId = $"{id:D5}";
                    string finalIdIcon = $"{id:D5}";
                    data.partName = $"{classicStart}{typeString}{finalId}.ice";
                    data.partHash = GetFileHash(data.partName);

                    //Set icon string
                    data.iconName = icon + eyelashesIcon + finalIdIcon + ".ice";
                    data.iconHash = GetFileHash(data.iconName);
                }

                if (id <= 100000)
                {
                    outputEyelashes.Add(data);
                }
                else
                {
                    outputNGSEyelashes.Add(data);
                }
                totalCount++;
            }
            eyeLDict.Add("Eyebrows", outputEyelashes);
            eyeLDict.Add("EyebrowsNGS", outputNGSEyelashes);
            partListsDict.Add(eyeLKey, eyeLDict);

            //---------------------------Parse out ACCE //Stored under decoy in a99be286e3a7e1b45d88a3ea4d6c18c4
            string acceKey = "accessory";
            Dictionary<string, List<PartData>> acceDict = new Dictionary<string, List<PartData>>();
            List<PartData> outputAccessories = new List<PartData>();

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "decoy", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.accessoryDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                PartData data = new PartData();
                data.id = id;
                bool named = false;
                foreach (var dict in nameDicts)
                {
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        named = true;
                        data.namesByLanguage.Add(str);
                    }
                    else
                    {
                        data.namesByLanguage.Add("");
                    }
                }

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    data.namesByLanguage.Add($"[Unnamed {id}]");
                }

                //Decide if it needs to be handled as a reboot file or not
                string typeString = "ac_";
                if (id >= 100000)
                {
                    data.partName = $"{rebootStart}{typeString}{id}.ice";
                    data.partExName = $"{rebootExStart}{typeString}{id}_ex.ice";
                    data.partHash = GetFileHash(data.partName);
                    data.partExHash = GetFileHash(data.partExName);

                    //Set icon string
                    data.iconName = icon + accessoryIcon + id + ".ice";
                    data.iconHash = GetFileHash(data.iconName);
                }
                else
                {
                    string finalId = $"{id:D5}";
                    string finalIdIcon = $"{id:D5}";
                    data.partName = $"{classicStart}{typeString}{finalId}.ice";
                    data.partHash = GetFileHash(data.partName);

                    //Set icon string
                    data.iconName = icon + accessoryIcon + finalIdIcon + ".ice";
                    data.iconHash = GetFileHash(data.iconName);
                }

                outputAccessories.Add(data);
                totalCount++;
            }
            acceDict.Add("Accessory", outputAccessories);
            partListsDict.Add(acceKey, acceDict);

            //---------------------------Parse out skin
            string skinKey = "skin";
            Dictionary<string, List<PartData>> skinDict = new Dictionary<string, List<PartData>>();
            List<PartData> outputSkin = new List<PartData>();
            List<PartData> outputNGSSkin = new List<PartData>();

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "skin", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.ngsSkinDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                PartData data = new PartData();
                data.id = id;
                bool named = false;
                foreach (var dict in nameDicts)
                {
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        named = true;
                        data.namesByLanguage.Add(str);
                    }
                    else
                    {
                        data.namesByLanguage.Add("");
                    }
                }

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    data.namesByLanguage.Add($"[Unnamed {id}]");
                }

                //Double check these ids and use an adjustedId if needed
                int adjustedId = id;
                if (aquaCMX.accessoryIdLink.ContainsKey(id))
                {
                    adjustedId = aquaCMX.accessoryIdLink[id].bcln.fileId;
                }

                //Decide if it needs to be handled as a reboot file or not
                string typeString = "sk_";
                if (id >= 100000)
                {
                    data.partName = $"{rebootStart}{typeString}{id}.ice";
                    data.partExName = $"{rebootExStart}{typeString}{id}_ex.ice";
                    data.partHash = GetFileHash(data.partName);
                    data.partExHash = GetFileHash(data.partExName);

                    //Set icon string
                    data.iconName = icon + skinIcon + GetIconGender(id) + id + ".ice";
                    data.iconHash = GetFileHash(data.iconName);
                }
                else
                {
                    string finalId = $"{id:D5}";
                    data.partName = $"{classicStart}{typeString}{finalId}.ice";
                    data.partHash = GetFileHash(data.partName);

                    //Set icon string
                    data.iconName = icon + skinIcon + GetIconGender(id) + id + ".ice";
                    data.iconHash = GetFileHash(data.iconName);
                }

                if (id < 100000)
                {
                    outputSkin.Add(data);
                }
                else
                {
                    outputNGSSkin.Add(data);
                }
                totalCount++;
            }
            skinDict.Add("Skin", outputSkin);
            skinDict.Add("SkinNGS", outputNGSSkin);
            partListsDict.Add(skinKey, skinDict);

            //---------------------------Parse out FCP1, Face Textures
            string faceTexKey = "faceTextures";
            Dictionary<string, List<PartData>> faceTexDict = new Dictionary<string, List<PartData>>();
            List<PartData> outputFCP1 = new List<PartData>();
            List<PartData> outputNGSFCP1 = new List<PartData>();

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "facepaint1", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.fcpDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                PartData data = new PartData();
                data.id = id;
                bool named = false;
                foreach (var dict in nameDicts)
                {
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        named = true;
                        data.namesByLanguage.Add(str);
                    }
                    else
                    {
                        data.namesByLanguage.Add("");
                    }
                }

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    data.namesByLanguage.Add($"[Unnamed {id}]");
                }

                //Decide if it needs to be handled as a reboot file or not
                string typeString = "f1_";
                if (id >= 100000)
                {
                    data.partName = $"{rebootStart}{typeString}{id}.ice";
                    data.partExName = $"{rebootExStart}{typeString}{id}_ex.ice";
                    data.partHash = GetFileHash(data.partName);
                    data.partExHash = GetFileHash(data.partExName);

                    //Set icon string
                    data.iconName = icon + faceIcon + id + ".ice";
                    data.iconHash = GetFileHash(data.iconName);
                }
                else
                {
                    string finalId = $"{id:D5}";
                    string finalIdIcon = $"{id:D5}";
                    data.partName = $"{classicStart}{typeString}{finalId}.ice";
                    data.partHash = GetFileHash(data.partName);

                    //Set icon string
                    data.iconName = icon + faceIcon + finalIdIcon + ".ice";
                    data.iconHash = GetFileHash(data.iconName);
                }

                if (id <= 100000)
                {
                    outputFCP1.Add(data);
                }
                else
                {
                    outputNGSFCP1.Add(data);
                }
                totalCount++;
            }
            faceTexDict.Add("FaceTextures", outputFCP1);
            faceTexDict.Add("FaceTexturesNGS", outputNGSFCP1);
            partListsDict.Add(faceTexKey, faceTexDict);

            //---------------------------Parse out FCP2
            string facePaintKey = "facePaint";
            Dictionary<string, List<PartData>> facePaintDict = new Dictionary<string, List<PartData>>();
            List<PartData> outputFCP2 = new List<PartData>();
            List<PartData> outputNGSFCP2 = new List<PartData>();

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "facepaint2", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.fcpDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                PartData data = new PartData();
                data.id = id;
                bool named = false;
                foreach (var dict in nameDicts)
                {
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        named = true;
                        data.namesByLanguage.Add(str);
                    }
                    else
                    {
                        data.namesByLanguage.Add("");
                    }
                }

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    data.namesByLanguage.Add($"[Unnamed {id}]");
                }

                //Decide if it needs to be handled as a reboot file or not
                string typeString = "f2_";
                if (id >= 100000)
                {
                    data.partName = $"{rebootStart}{typeString}{id}.ice";
                    data.partExName = $"{rebootExStart}{typeString}{id}_ex.ice";
                    data.partHash = GetFileHash(data.partName);
                    data.partExHash = GetFileHash(data.partExName);

                    //Set icon string
                    data.iconName = icon + facePaintIcon + id + ".ice";
                    data.iconHash = GetFileHash(data.iconName);
                }
                else
                {
                    string finalId = $"{id:D5}";
                    string finalIdIcon = $"{id:D5}";
                    data.partName = $"{classicStart}{typeString}{finalId}.ice";
                    data.partHash = GetFileHash(data.partName);

                    //Set icon string
                    data.iconName = icon + facePaintIcon + finalIdIcon + ".ice";
                    data.iconHash = GetFileHash(data.iconName);
                }

                if (id <= 100000)
                {
                    outputFCP2.Add(data);
                }
                else
                {
                    outputNGSFCP2.Add(data);
                }
                totalCount++;
            }
            facePaintDict.Add("FacePaint", outputFCP2);
            if (outputNGSFCP2.Count > 0)
            {
                facePaintDict.Add("FacePaintNGS", outputNGSFCP2);
            }
            partListsDict.Add(facePaintKey, facePaintDict);

            //---------------------------Parse out FACE //face_variation.cmp.lua in 75b1632526cd6a1039625349df6ee8dd used to map file face ids to .text ids
            //This targets facevariations specifically. face seems to be redundant and not actually particularly useful at a glance.
            string faceKey = "face";
            Dictionary<string, List<PartData>> faceDict = new Dictionary<string, List<PartData>>();
            List<PartData> outputHumanMaleFace = new List<PartData>();
            List<PartData> outputHumanFemaleFace = new List<PartData>();
            List<PartData> outputNewmanMaleFace = new List<PartData>();
            List<PartData> outputNewmanFemaleFace = new List<PartData>();
            List<PartData> outputCastMaleFace = new List<PartData>();
            List<PartData> outputCastFemaleFace = new List<PartData>();
            List<PartData> outputDewmanMaleFace = new List<PartData>();
            List<PartData> outputDewmanFemaleFace = new List<PartData>();
            List<PartData> outputNGSFace = new List<PartData>();

            masterIdList.Clear();
            nameDicts.Clear();

            masterNameList = new List<string>();
            strNameDicts = new List<Dictionary<string, string>>();
            GatherTextIdsStringRef(textByCat, masterNameList, strNameDicts, "facevariation", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.faceDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                PartData data = new PartData();
                data.id = id;
                bool named = false;

                string realId = "";
                if (!faceIds.TryGetValue(id, out realId))
                {
                    realId = "No" + id;
                }


                foreach (var dict in strNameDicts)
                {
                    if (dict.TryGetValue(realId, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        named = true;
                        data.namesByLanguage.Add(str);
                    }
                    else
                    {
                        data.namesByLanguage.Add("");
                    }
                }

                //Account for lack of a name for a face
                if (named == false)
                {
                    data.namesByLanguage.Add($"[Unnamed {id}]");
                }

                //Decide if it needs to be handled as a reboot file or not
                string typeString = "fc_";
                if (id >= 100000)
                {
                    data.partName = $"{rebootStart}{typeString}{id}.ice";
                    data.partExName = $"{rebootExStart}{typeString}{id}_ex.ice";
                    data.partHash = GetFileHash(data.partName);
                    data.partExHash = GetFileHash(data.partExName);

                    //Set icon string
                    data.iconName = icon + faceIcon + id + ".ice";
                    data.iconHash = GetFileHash(data.iconName);
                }
                else
                {
                    string finalId = $"{id:D5}";
                    string finalIdIcon = $"{id:D5}";
                    data.partName = $"{classicStart}{typeString}{finalId}.ice";
                    data.partHash = GetFileHash(data.partName);

                    //Set icon string
                    data.iconName = icon + faceIcon + finalIdIcon + ".ice";
                    data.iconHash = GetFileHash(data.iconName);
                }

                if (id < 10000)
                {
                    outputHumanMaleFace.Add(data);
                }
                else if (id < 20000)
                {
                    outputHumanFemaleFace.Add(data);
                }
                else if (id < 30000)
                {
                    outputNewmanMaleFace.Add(data);
                }
                else if (id < 40000)
                {
                    outputNewmanFemaleFace.Add(data);
                }
                else if (id < 50000)
                {
                    outputCastMaleFace.Add(data);
                }
                else if (id < 60000)
                {
                    outputCastFemaleFace.Add(data);
                }
                else if (id < 70000)
                {
                    outputDewmanMaleFace.Add(data);
                }
                else if (id < 100000)
                {
                    outputDewmanFemaleFace.Add(data);
                }
                else
                {
                    outputNGSFace.Add(data);
                }
                totalCount++;
            }
            faceDict.Add("HumanMaleFace", outputHumanMaleFace);
            faceDict.Add("HumanFemaleFace", outputHumanFemaleFace);
            faceDict.Add("NewmanMaleFace", outputNewmanMaleFace);
            faceDict.Add("NewmanFemaleFace", outputNewmanFemaleFace);
            faceDict.Add("CastFace", outputCastMaleFace);
            faceDict.Add("CasealFace", outputCastFemaleFace);
            faceDict.Add("DewmanMaleFace", outputDewmanMaleFace);
            faceDict.Add("DewmanFemaleFace", outputDewmanFemaleFace);
            faceDict.Add("NGSFace", outputNGSFace);
            partListsDict.Add(faceKey, faceDict);

            //---------------------------Parse out Face motions
            string fcmnKey = "faceMotion";
            Dictionary<string, List<PartData>> fcmnDict = new Dictionary<string, List<PartData>>();
            List<PartData> outputFcmn = new List<PartData>();
            List<PartData> outputFcmnNGS = new List<PartData>();

            masterIdList.Clear();
            nameDicts.Clear();

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.fcmnDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                PartData data = new PartData();
                data.id = id;
                bool named = false;
                foreach (var dict in nameDicts)
                {
                    if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                    {
                        named = true;
                        data.namesByLanguage.Add(str);
                    }
                    else
                    {
                        data.namesByLanguage.Add("");
                    }
                }

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    data.namesByLanguage.Add($"[Unnamed {id}]");
                }

                //Decide if it needs to be handled as a reboot file or not
                string typeString = "fm_";
                if (id >= 100000)
                {
                    data.partName = $"{rebootStart}{typeString}{id}.ice";
                    data.partExName = $"{rebootExStart}{typeString}{id}_ex.ice";
                    data.partHash = GetFileHash(data.partName);
                    data.partExHash = GetFileHash(data.partExName);
                }
                else
                {
                    string finalId = $"{id:D5}";
                    data.partName = $"{classicStart}{typeString}{finalId}.ice";
                    data.partHash = GetFileHash(data.partName);
                }

                if (id < 100000)
                {
                    outputFcmn.Add(data);
                }
                else
                {
                    outputFcmnNGS.Add(data);
                }
                totalCount++;
            }
            fcmnDict.Add("FaceMotions", outputFcmn);
            fcmnDict.Add("FaceMotionsNGS", outputFcmnNGS);
            partListsDict.Add(fcmnKey, fcmnDict);

            //---------------------------Parse out NGS ears //The cmx has ear data, but no ids. Maybe it's done by order? Same for teeth and horns
            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "ears", true);

            if (aquaCMX.ngsEarDict.Count > 0 || masterIdList.Count > 0)
            {
                string earKey = "ear";
                Dictionary<string, List<PartData>> earDict = new Dictionary<string, List<PartData>>();
                List<PartData> outputNGSEars = new List<PartData>();

                //Add potential cmx ids that wouldn't be stored in
                GatherDictKeys(masterIdList, aquaCMX.ngsEarDict.Keys);

                masterIdList.Sort();

                //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
                foreach (int id in masterIdList)
                {
                    PartData data = new PartData();
                    data.id = id;
                    bool named = false;
                    foreach (var dict in nameDicts)
                    {
                        if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                        {
                            named = true;
                            data.namesByLanguage.Add(str);
                        }
                        else
                        {
                            data.namesByLanguage.Add("");
                        }
                    }

                    //Account for lack of a name on an outfit
                    if (named == false)
                    {
                        data.namesByLanguage.Add($"[Unnamed {id}]");
                    }

                    //Decide if it needs to be handled as a reboot file or not
                    string typeString = "ea_";
                    if (id >= 100000)
                    {
                        data.partName = $"{rebootStart}{typeString}{id}.ice";
                        data.partExName = $"{rebootExStart}{typeString}{id}_ex.ice";
                        data.partHash = GetFileHash(data.partName);
                        data.partExHash = GetFileHash(data.partExName);

                        //Set icon string
                        data.iconName = icon + earIcon + id + ".ice";
                        data.iconHash = GetFileHash(data.iconName);
                    }
                    else
                    {
                        string finalId = $"{id:D5}";
                        string finalIdIcon = $"{id:D5}";
                        data.partName = $"{classicStart}{typeString}{finalId}.ice";
                        data.partHash = GetFileHash(data.partName);

                        //Set icon string
                        data.iconName = icon + earIcon + finalIdIcon + ".ice";
                        data.iconHash = GetFileHash(data.iconName);
                    }

                    outputNGSEars.Add(data);

                }
                earDict.Add("NGSEars", outputNGSEars);
                partListsDict.Add(earKey, earDict);
                totalCount++;
            }

            //---------------------------Parse out NGS teeth 
            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "dental", true);

            if (aquaCMX.ngsTeethDict.Count > 0 || masterIdList.Count > 0)
            {
                string teethKey = "teeth";
                Dictionary<string, List<PartData>> teethDict = new Dictionary<string, List<PartData>>();
                List<PartData> outputNGSTeeth = new List<PartData>();

                //Add potential cmx ids that wouldn't be stored in
                GatherDictKeys(masterIdList, aquaCMX.ngsTeethDict.Keys);

                masterIdList.Sort();

                //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
                foreach (int id in masterIdList)
                {
                    PartData data = new PartData();
                    data.id = id;
                    bool named = false;
                    foreach (var dict in nameDicts)
                    {
                        if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                        {
                            named = true;
                            data.namesByLanguage.Add(str);
                        }
                        else
                        {
                            data.namesByLanguage.Add("");
                        }
                    }

                    //Account for lack of a name on an outfit
                    if (named == false)
                    {
                        data.namesByLanguage.Add($"[Unnamed {id}]");
                    }

                    //Decide if it needs to be handled as a reboot file or not
                    string typeString = "de_";
                    if (id >= 100000)
                    {
                        data.partName = $"{rebootStart}{typeString}{id}.ice";
                        data.partExName = $"{rebootExStart}{typeString}{id}_ex.ice";
                        data.partHash = GetFileHash(data.partName);
                        data.partExHash = GetFileHash(data.partExName);

                        //Set icon string
                        data.iconName = icon + teethIcon + id + ".ice";
                        data.iconHash = GetFileHash(data.iconName);
                    }
                    else
                    {
                        string finalId = $"{id:D5}";
                        string finalIdIcon = $"{id:D5}";
                        data.partName = $"{classicStart}{typeString}{finalId}.ice";
                        data.partHash = GetFileHash(data.partName);

                        //Set icon string
                        data.iconName = icon + teethIcon + finalIdIcon + ".ice";
                        data.iconHash = GetFileHash(data.iconName);
                    }

                    outputNGSTeeth.Add(data);
                }

                teethDict.Add("NGSTeeth", outputNGSTeeth);
                partListsDict.Add(teethKey, teethDict);
                totalCount++;
            }

            //---------------------------Parse out NGS horns 
            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "horn", true);

            if (aquaCMX.ngsHornDict.Count > 0 || masterIdList.Count > 0)
            {
                string hornKey = "horn";
                Dictionary<string, List<PartData>> hornDict = new Dictionary<string, List<PartData>>();
                List<PartData> outputNGSHorns = new List<PartData>();

                //Add potential cmx ids that wouldn't be stored in
                GatherDictKeys(masterIdList, aquaCMX.ngsHornDict.Keys);

                masterIdList.Sort();

                //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
                foreach (int id in masterIdList)
                {
                    //Skip the なし horn entry. I'm not even sure why that's in there.
                    if (id == 0)
                    {
                        continue;
                    }
                    PartData data = new PartData();
                    data.id = id;
                    bool named = false;
                    foreach (var dict in nameDicts)
                    {
                        if (dict.TryGetValue(id, out string str) && str != null && str != "" && str.Length > 0)
                        {
                            named = true;
                            data.namesByLanguage.Add(str);
                        }
                        else
                        {
                            data.namesByLanguage.Add("");
                        }
                    }

                    //Account for lack of a name on an outfit
                    if (named == false)
                    {
                        data.namesByLanguage.Add($"[Unnamed {id}]");
                    }

                    //Decide if it needs to be handled as a reboot file or not
                    string typeString = "hn_";
                    if (id >= 100000)
                    {
                        data.partName = $"{rebootStart}{typeString}{id}.ice";
                        data.partExName = $"{rebootExStart}{typeString}{id}_ex.ice";
                        data.partHash = GetFileHash(data.partName);
                        data.partExHash = GetFileHash(data.partExName);

                        //Set icon string
                        data.iconName = icon + hornIcon + id + ".ice";
                        data.iconHash = GetFileHash(data.iconName);
                    }
                    else
                    {
                        string finalId = $"{id:D5}";
                        string finalIdIcon = $"{id:D5}";
                        data.partName = $"{classicStart}{typeString}{finalId}.ice";
                        data.partHash = GetFileHash(data.partName);

                        //Set icon string
                        data.iconName = icon + hornIcon + finalIdIcon + ".ice";
                        data.iconHash = GetFileHash(data.iconName);
                    }

                    outputNGSHorns.Add(data);

                }
                hornDict.Add("NGSHorns", outputNGSHorns);
                partListsDict.Add(hornKey, hornDict);
                totalCount++;
            }
            //---------------------------------------------------------------------------------------//End CMX related ids

            return partListsDict;
        }

    }
}
