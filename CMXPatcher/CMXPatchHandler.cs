using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AquaModelLibrary;
using AquaModelLibrary.AquaMethods;
using zamboni;
using static AquaModelLibrary.CharacterMakingIndex;

namespace CMXPatcher
{
    public class CMXPatchHandler
    {
        public bool readyToMod = false;
        public string settingsPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\";
        public string backupPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\BackupCMX\\";
        public string moddedCMXPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\ModdedCMX\\";
        public string downgradeCMXPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\BenchmarkCMX\\";
        public string modPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Mods\\";
        public string pso2_binDir;
        public IceFile cmxIce;
        public CharacterMakingIndex cmx;
        public byte[] cmxRaw; //Would be nice to reassemble these at some point, but it's it's a large format and it's easier to just edit a few things directly.

        public CMXPatchHandler()
        {
            InitializeCMX();
        }

        public void InitializeCMX()
        {
            Directory.CreateDirectory(modPath);
            if (File.Exists(settingsPath + "settings.txt"))
            {
                var lines = File.ReadAllLines(settingsPath + "settings.txt");
                pso2_binDir = lines[0];


                BackupCMX();
                if (File.Exists(backupPath + "\\pl_data_info.cmx"))
                {
                    cmx = CharacterMakingIndexMethods.ReadCMX(backupPath + "\\pl_data_info.cmx");
                    cmxRaw = File.ReadAllBytes(backupPath + "\\pl_data_info.cmx");
                }
                //InjectCMXMods();
            }
            else
            {
                MessageBox.Show("Must set pso2_bin before attempting to patch .cmx!");
            }
        }

        public void InjectCMXMods(bool restoreMode = false)
        {
            if(readyToMod == false)
            {
                MessageBox.Show("Please set all paths properly and attempt this again.");
            }

            //Reload backup cmx
            if (File.Exists(backupPath + "\\pl_data_info.cmx"))
            {
                cmx = CharacterMakingIndexMethods.ReadCMX(backupPath + "\\pl_data_info.cmx");
                cmxRaw = File.ReadAllBytes(backupPath + "\\pl_data_info.cmx");
            }

            //Write cmx to ice
            string cmxPath = Path.Combine(pso2_binDir, dataDir, CharacterMakingIndexMethods.GetFileHash(classicCMX));
            if (File.Exists(cmxPath))
            {
                var strm = new MemoryStream(File.ReadAllBytes(cmxPath));
                cmxIce = IceFile.LoadIceFile(strm);
                strm.Dispose();

                InjectCmxToIceGroup(cmxIce.groupOneFiles, cmxRaw);
                InjectCmxToIceGroup(cmxIce.groupTwoFiles, cmxRaw);
            }

            //Gather and write mods to cmx
            if(restoreMode == false)
            {
                GatherCMXModText();
            }
#if DEBUG
            File.WriteAllBytes(settingsPath + "test.cmx", cmxRaw);
#endif
            //Write ice - We recreate the file in case for some strange reason it was something other than a v4 ice
            byte[] rawData = new IceV4File((new IceHeaderStructures.IceArchiveHeader()).GetBytes(), cmxIce.groupOneFiles, cmxIce.groupTwoFiles).getRawData(false, false);
            
            try
            {
                File.WriteAllBytes(cmxPath, rawData);
            }
            catch
            {
                MessageBox.Show("Unable to write cmx to game directory. Check file permissions. \nIf using the MS Store version, this may not be fixable.\n" +
                    $"Attempting to write to {moddedCMXPath}");
                try
                {
                    Directory.CreateDirectory(moddedCMXPath);
                    File.WriteAllBytes(moddedCMXPath + CharacterMakingIndexMethods.GetFileHash(classicCMX), rawData);
                }
                catch
                {
                    MessageBox.Show("Unable to write cmx to patcher directory. Check file permissions.");
                }
            }
        }

        private void InjectCmxToIceGroup(byte[][] group, byte[] cmxRaw)
        {
            //Loop through files to get what we need
            for (int i = 0; i < group.Length; i++)
            {
                if (IceFile.getFileName(group[i]).ToLower().Contains(".cmx"))
                {
                    group[i] = cmxRaw;
                    //We can break here since we're really only expecting an NGS cmx and there's only one of those.
                    break;
                }
            }
        }

        public void DowngradeCmx()
        {
            if (readyToMod == false)
            {
                MessageBox.Show("Please set all paths properly and attempt this again.");
            }

            //Reload backup cmx
            if (File.Exists(backupPath + "\\pl_data_info.cmx"))
            {
                cmx = CharacterMakingIndexMethods.ReadCMX(backupPath + "\\pl_data_info.cmx");
                cmxRaw = File.ReadAllBytes(backupPath + "\\pl_data_info.cmx");
            }
            string cmxPath = Path.Combine(pso2_binDir, dataDir, CharacterMakingIndexMethods.GetFileHash(classicCMX));
            if (File.Exists(cmxPath))
            {
                var strm = new MemoryStream(File.ReadAllBytes(cmxPath));
                cmxIce = IceFile.LoadIceFile(strm);
                strm.Dispose();

                InjectCmxToIceGroup(cmxIce.groupOneFiles, cmxRaw);
                InjectCmxToIceGroup(cmxIce.groupTwoFiles, cmxRaw);
            }

            //ACCE replacement
            PatchACCE(cmx, cmxRaw);

            //Write ice - We recreate the file in case for some strange reason it was something other than a v4 ice
            byte[] rawData = new IceV4File((new IceHeaderStructures.IceArchiveHeader()).GetBytes(), cmxIce.groupOneFiles, cmxIce.groupTwoFiles).getRawData(false, false);

            try
            {
                Directory.CreateDirectory(downgradeCMXPath);
                File.WriteAllBytes(downgradeCMXPath + CharacterMakingIndexMethods.GetFileHash(classicCMX), rawData);
            }
            catch
            {
                MessageBox.Show("Unable to write cmx to patcher directory. Check file permissions.");
            }
        }

        public byte[] PatchACCE(CharacterMakingIndex cmx, byte[] cmxRaw)
        {
            List<byte> acceBytes = new List<byte>();
            foreach(var acceKey in cmx.accessoryDict.Keys)
            {
                acceBytes.AddRange(AquaGeneralMethods.ConvertStruct(cmx.accessoryDict[acceKey].acce));
                acceBytes.AddRange(AquaGeneralMethods.ConvertStruct(cmx.accessoryDict[acceKey].acce2));
            }
            Array.Copy(acceBytes.ToArray(), 0, cmxRaw, cmx.cmxTable.accessoryAddress, acceBytes.Count);

            return cmxRaw;
        }

        public void BackupCMX()
        {
            bool isOldCmx = false;
            //Check original CMX to see if we need to do a new backup.
            string cmxPath = Path.Combine(pso2_binDir, dataDir, CharacterMakingIndexMethods.GetFileHash(classicCMX));
            if (File.Exists(cmxPath))
            {
                //bool foundCmx = false;
                var strm = new MemoryStream(File.ReadAllBytes(cmxPath));
                cmxIce = IceFile.LoadIceFile(strm);
                strm.Dispose();

                List<byte[]> files = (new List<byte[]>(cmxIce.groupOneFiles));
                files.AddRange(cmxIce.groupTwoFiles);

                //Loop through files to get what we need
                foreach (byte[] file in files)
                {
                    if (IceFile.getFileName(file).ToLower().Contains(".cmx"))
                    {
                        //If the filesize is actually different, since it should be the same between modded ones and the originals, we want to back it up
                        if(cmxRaw == null || file.Length != cmxRaw.Length)
                        {
                            isOldCmx = true;
                            cmxRaw = file;
                            cmx = CharacterMakingIndexMethods.ReadCMX(file);
                        }
                        //foundCmx = true;

                        //We can break here since we're really only expecting an NGS cmx and there's only one of those.
                        break;
                    }
                }
                files.Clear();
            }
            else
            {
                MessageBox.Show("Cannot find CMX ice!");
                return;
            }

            //Backup if the file didn't exist or the CMX was old
            if (!File.Exists(backupPath + "\\pl_data_info.cmx") || isOldCmx)
            {
                Directory.CreateDirectory(backupPath);
                File.WriteAllBytes(backupPath + "\\pl_data_info.cmx", cmxRaw);
            }
            readyToMod = true;
        }

        public void GatherCMXModText()
        {
            var files = Directory.EnumerateFiles(modPath, "*_cmxConfig.txt", SearchOption.AllDirectories).ToArray();
            foreach(var file in files)
            {
                var cmxText = File.ReadAllLines(file);
                var cmxEntry = new List<string>();
                string cmxType = null;
                int cmxId = -1;
                foreach(var line in cmxText)
                {
                    //Ignore empty lines
                    if(line == "")
                    {
                        continue;
                    }

                    //Check if this line defines a new entry
                    var value = line.Split(':');
                    if(value.Length == 2)
                    {
                        if(cmxType != null)
                        {
                            ProcessEntry(ref cmxType, ref cmxId, cmxEntry);
                        }
                        cmxType = value[0];
                        cmxId = Int32.Parse(value[1]);
                        continue;
                    }

                    //Add data to current entry
                    cmxEntry.Add(line);

                }

                //Flush the last potential entry if needed
                if (cmxType != null)
                {
                    ProcessEntry(ref cmxType, ref cmxId, cmxEntry);
                }

            }
        }

        public void ProcessEntry(ref string cmxType, ref int cmxId, List<string> cmxEntry)
        {
            byte[] entryRaw;
            switch (cmxType.ToLower())
            {
                case "base ":
                case "basewear ":
                    var baseEntry = cmx.baseWearDict[cmxId];
                    BODYStructHandler.PatchBody(cmx.baseWearDict[cmxId], cmxEntry);
                    entryRaw = BODYStructHandler.GetBODYAsBytes(baseEntry);
                    Array.Copy(entryRaw, 0, cmxRaw, baseEntry.originalOffset, entryRaw.Length);
                    break;
                case "outer ":
                case "outerwear ":
                    var ouEntry = cmx.outerDict[cmxId];
                    BODYStructHandler.PatchBody(cmx.outerDict[cmxId], cmxEntry);
                    entryRaw = BODYStructHandler.GetBODYAsBytes(ouEntry);
                    Array.Copy(entryRaw, 0, cmxRaw, ouEntry.originalOffset, entryRaw.Length);
                    break;
                case "carm ":
                case "castarm ":
                    var carmEntry = cmx.carmDict[cmxId];
                    BODYStructHandler.PatchBody(cmx.carmDict[cmxId], cmxEntry);
                    entryRaw = BODYStructHandler.GetBODYAsBytes(carmEntry);
                    Array.Copy(entryRaw, 0, cmxRaw, carmEntry.originalOffset, entryRaw.Length);
                    break;
                case "cleg ":
                case "castleg ":
                    var clegEntry = cmx.clegDict[cmxId];
                    BODYStructHandler.PatchBody(cmx.clegDict[cmxId], cmxEntry);
                    entryRaw = BODYStructHandler.GetBODYAsBytes(clegEntry);
                    Array.Copy(entryRaw, 0, cmxRaw, clegEntry.originalOffset, entryRaw.Length);
                    break;
                case "body ":
                case "costume ":
                    var bodyEntry = cmx.costumeDict[cmxId];
                    BODYStructHandler.PatchBody(cmx.costumeDict[cmxId], cmxEntry);
                    entryRaw = BODYStructHandler.GetBODYAsBytes(bodyEntry);
                    Array.Copy(entryRaw, 0, cmxRaw, bodyEntry.originalOffset, entryRaw.Length);
                    break;
                case "hair ":
                    var hairEntry = cmx.hairDict[cmxId];
                    HAIRStructHandler.PatchBody(cmx.hairDict[cmxId], cmxEntry);
                    entryRaw = HAIRStructHandler.GetHAIRAsBytes(hairEntry);
                    Array.Copy(entryRaw, 0, cmxRaw, hairEntry.originalOffset, entryRaw.Length);
                    break;
                default:
                    break;
            }
            cmxType = null;
            cmxId = -1;
        }
    }
}
