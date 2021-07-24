using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AquaModelLibrary;
using zamboni;
using static AquaModelLibrary.CharacterMakingIndex;

namespace CMXPatcher
{
    public class CMXPatchHandler
    {
        public string settingsPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\";
        public string backupPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "BackupCMX\\";
        public string pso2_binDir;
        public IceFile cmxIce;
        public CharacterMakingIndex cmx;
        public byte[] cmxRaw; //Would be nice to reassemble these at some point, but it's it's a large format and it's easier to just edit a few things directly.

        public CMXPatchHandler()
        {
            InitializeCMX();

            //var baseTest = cmx.baseWearDict[500040];
            //baseTest.body.legLength = 2.0f;
            //var baseTestRaw = AquaObjectMethods.ConvertStruct(baseTest.body);

            //Array.Copy(baseTestRaw, 0, cmxRaw, baseTest.originalOffset, System.Runtime.InteropServices.Marshal.SizeOf(baseTestRaw));
        }

        public void InitializeCMX()
        {
            if (File.Exists(settingsPath + "settings.txt"))
            {
                var lines = File.ReadAllLines(settingsPath + "settings.txt");
                pso2_binDir = lines[0];


                if (File.Exists(backupPath + "pl_data_info.cmx"))
                {
                    cmx = CharacterMakingIndexMethods.ReadCMX(backupPath + "pl_data_info.cmx");
                    cmxRaw = File.ReadAllBytes(backupPath + "pl_data_info.cmx");
                }
                BackupCMX();
            }
            else
            {
                MessageBox.Show("Must set pso2_bin before attempting to patch .cmx!");
            }
        }

        public void BackupCMX()
        {
            bool isOldCmx = false;
            //Check original CMX to see if we need to do a new backup.
            string cmxPath = Path.Combine(pso2_binDir, dataDir, CharacterMakingIndexMethods.GetFileHash(classicCMX));
            if (File.Exists(cmxPath))
            {
                bool foundCmx = false;
                var strm = new MemoryStream(File.ReadAllBytes(cmxPath));
                var cmxIce = IceFile.LoadIceFile(strm);
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
                        foundCmx = true;

                        //We can break here since we're really only expecting an NGS cmx and there's only one of those.
                        break;
                    }
                }
                files.Clear();

                cmxIce = null;
            }
            else
            {
                throw new Exception("Cannot find CMX ice!");
            }

            //Backup if the file didn't exist or the CMX was old
            if (!File.Exists(backupPath + "pl_data_info.cmx") || isOldCmx)
            {
                Directory.CreateDirectory(backupPath);
                File.WriteAllBytes(backupPath + "pl_data_info.cmx", cmxRaw);
            }
        }
    }
}
