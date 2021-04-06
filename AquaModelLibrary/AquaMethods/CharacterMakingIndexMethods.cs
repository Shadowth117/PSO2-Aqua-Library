using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AquaModelLibrary
{
    public class CharacterMakingIndexMethods
    {
        public static CharacterMakingIndex ReadCMX(string fileName)
        {
            using (Stream stream = (Stream)new FileStream(fileName, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                string type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
                int offset = 0x20; //Base offset due to NIFL header

                //Deal with deicer's extra header nonsense
                if (type.Equals("cmx\0"))
                {
                    streamReader.Seek(0xC, SeekOrigin.Begin);
                    //Basically always 0x60, but some deicer files from the Alpha have 0x50... 
                    int headJunkSize = streamReader.Read<int>();

                    streamReader.Seek(headJunkSize - 0x10, SeekOrigin.Current);
                    type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
                    offset += headJunkSize;
                }

                //Proceed based on file variant
                if (type.Equals("NIFL"))
                {
                    //NIFL
                    return ReadNIFLCMX(streamReader, offset);
                }
                else if (type.Equals("VTBF"))
                {
                    //VTBF
                    return null;
                }
                else
                {
                    MessageBox.Show("Improper File Format!");
                    return null;
                }
            }
        }

        public static CharacterMakingIndex ReadNIFLCMX(BufferedStreamReader streamReader, int offset)
        {
            var cmx = new CharacterMakingIndex();
            cmx.nifl = streamReader.Read<AquaCommon.NIFL>();
            cmx.rel0 = streamReader.Read<AquaCommon.REL0>();

            streamReader.Seek(cmx.rel0.REL0DataStart + offset, SeekOrigin.Begin);
            cmx.cmxTable = streamReader.Read<CharacterMakingIndex.CMXTable>();

            ReadBODY(streamReader, offset, cmx.cmxTable.bodyAddress, cmx.cmxTable.bodyCount, cmx.costumeDict);
            ReadBODY(streamReader, offset, cmx.cmxTable.carmAddress, cmx.cmxTable.carmCount, cmx.carmDict);
            ReadBODY(streamReader, offset, cmx.cmxTable.clegAddress, cmx.cmxTable.clegCount, cmx.clegDict);
            ReadBODY(streamReader, offset, cmx.cmxTable.bodyOuterAddress, cmx.cmxTable.bodyOuterCount, cmx.outerDict);

            ReadBODY(streamReader, offset, cmx.cmxTable.baseWearAddress, cmx.cmxTable.baseWearCount, cmx.baseWearDict);
            ReadBBLY(streamReader, offset, cmx.cmxTable.innerWearAddress, cmx.cmxTable.innerWearCount, cmx.innerWearDict);
            ReadBBLY(streamReader, offset, cmx.cmxTable.bodyPaintAddress, cmx.cmxTable.bodyPaintCount, cmx.bodyPaintDict);
            ReadSticker(streamReader, offset, cmx.cmxTable.stickerAddress, cmx.cmxTable.stickerCount, cmx.stickerDict);

            ReadFACE(streamReader, offset, cmx.cmxTable.faceAddress, cmx.cmxTable.faceCount, cmx.faceDict);
            ReadFCMN(streamReader, offset, cmx.cmxTable.faceMotionAddress, cmx.cmxTable.faceMotionCount, cmx.fcmnDict);
            ReadNGSFACE(streamReader, offset, cmx.cmxTable.rebootFaceAddress, cmx.cmxTable.rebootFaceCount, cmx.ngsFaceDict);
            ReadFCP(streamReader, offset, cmx.cmxTable.faceTexturesAddress, cmx.cmxTable.faceTexturesCount, cmx.fcpDict);

            ReadACCE(streamReader, offset, cmx.cmxTable.accessoryAddress, cmx.cmxTable.accessoryCount, cmx.accessoryDict);
            ReadEYE(streamReader, offset, cmx.cmxTable.eyeTextureAddress, cmx.cmxTable.eyeTextureCount, cmx.eyeDict);
            //NGS ears
            streamReader.Seek(cmx.cmxTable.earAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < cmx.cmxTable.earCount; i++)
            {
                cmx.ngsEarList.Add(streamReader.Read<CharacterMakingIndex.NGS_Ear>());
            }

            //NGS Teeth
            streamReader.Seek(cmx.cmxTable.teethAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < cmx.cmxTable.teethCount; i++)
            {
                cmx.ngsTeethList.Add(streamReader.Read<CharacterMakingIndex.NGS_Teeth>());
            }

            //NGS Horns
            streamReader.Seek(cmx.cmxTable.hornAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < cmx.cmxTable.hornCount; i++)
            {
                cmx.ngsHornList.Add(streamReader.Read<CharacterMakingIndex.NGS_Horn>());
            }
            ReadNGSSKIN(streamReader, offset, cmx.cmxTable.skinAddress, cmx.cmxTable.skinCount, cmx.ngsSkinDict);
            ReadEYEB(streamReader, offset, cmx.cmxTable.eyebrowAddress, cmx.cmxTable.eyebrowCount, cmx.eyebrowDict);
            ReadEYEB(streamReader, offset, cmx.cmxTable.eyelashAddress, cmx.cmxTable.eyelashCount, cmx.eyelashDict);

            ReadHAIR(streamReader, offset, cmx.cmxTable.hairAddress, cmx.cmxTable.hairCount, cmx.hairDict);
            ReadNIFLCOL(streamReader, offset, cmx.cmxTable.colAddress, cmx.cmxTable.colCount, cmx.colDict);

            //Unk
            streamReader.Seek(cmx.cmxTable.unkAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < cmx.cmxTable.unkCount; i++)
            {
                cmx.unkList.Add(streamReader.Read<CharacterMakingIndex.Unk_IntField>());
            }
            ReadIndexLinks(streamReader, offset, cmx.cmxTable.costumeIdLinkAddress, cmx.cmxTable.costumeIdLinkCount, cmx.costumeIdLink);

            ReadIndexLinks(streamReader, offset, cmx.cmxTable.castArmIdLinkAddress, cmx.cmxTable.castArmIdLinkCount, cmx.castArmIdLink);
            ReadIndexLinks(streamReader, offset, cmx.cmxTable.castLegIdLinkAddress, cmx.cmxTable.castLegIdLinkCount, cmx.clegIdLink);
            ReadIndexLinks(streamReader, offset, cmx.cmxTable.outerIdLinkAddress, cmx.cmxTable.outerIdLinkCount, cmx.outerWearIdLink);
            ReadIndexLinks(streamReader, offset, cmx.cmxTable.baseWearIdLinkAddress, cmx.cmxTable.baseWearIdLinkCount, cmx.baseWearIdLink);

            ReadIndexLinks(streamReader, offset, cmx.cmxTable.innerWearIdLinkAddress, cmx.cmxTable.innerWearIdLinkCount, cmx.innerWearIdLink);

            return cmx;
        }

        private static void ReadBODY(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, CharacterMakingIndex.BODY> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                var body = streamReader.Read<CharacterMakingIndex.BODY>();
                dict.Add(body.id, body); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadBBLY(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, CharacterMakingIndex.BBLY> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                var bbly = streamReader.Read<CharacterMakingIndex.BBLY>();
                dict.Add(bbly.id, bbly); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadSticker(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, CharacterMakingIndex.Sticker> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                var sticker = streamReader.Read<CharacterMakingIndex.Sticker>();
                dict.Add(sticker.id, sticker); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadFACE(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, CharacterMakingIndex.FACE> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                var face = streamReader.Read<CharacterMakingIndex.FACE>();
                dict.Add(face.id, face); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadFCMN(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, CharacterMakingIndex.FCMN> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                var fcmn = streamReader.Read<CharacterMakingIndex.FCMN>();
                dict.Add(fcmn.id, fcmn); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadFCP(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, CharacterMakingIndex.FCP> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                var fcp = streamReader.Read<CharacterMakingIndex.FCP>();
                dict.Add(fcp.id, fcp); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadNGSFACE(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, CharacterMakingIndex.NGS_FACE> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                var ngsFace = streamReader.Read<CharacterMakingIndex.NGS_FACE>();
                dict.Add(ngsFace.id, ngsFace); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadACCE(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, CharacterMakingIndex.ACCE> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                var acce = streamReader.Read<CharacterMakingIndex.ACCE>();
                dict.Add(acce.id, acce); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadEYE(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, CharacterMakingIndex.EYE> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                var eye = streamReader.Read<CharacterMakingIndex.EYE>();
                dict.Add(eye.id, eye); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadNGSSKIN(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, CharacterMakingIndex.NGS_Skin> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                var ngsSkin = streamReader.Read<CharacterMakingIndex.NGS_Skin>();
                dict.Add(ngsSkin.id, ngsSkin); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadEYEB(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, CharacterMakingIndex.EYEB> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                var eyeb = streamReader.Read<CharacterMakingIndex.EYEB>();
                dict.Add(eyeb.id, eyeb); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadHAIR(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, CharacterMakingIndex.HAIR> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                var hair = streamReader.Read<CharacterMakingIndex.HAIR>();
                dict.Add(hair.id, hair); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadNIFLCOL(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, CharacterMakingIndex.NIFL_COL> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                var COL = streamReader.Read<CharacterMakingIndex.NIFL_COL>();
                dict.Add(COL.id, COL); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadIndexLinks(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, CharacterMakingIndex.BCLN> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                var bcln = streamReader.Read<CharacterMakingIndex.BCLN>();
                if (!dict.ContainsKey(bcln.id))
                {
                    dict.Add(bcln.id, bcln); //Set like this so we can access it by id later if we want. 
                }
                else
                {
                    Console.WriteLine($"Duplicate key at {streamReader.Position().ToString("X")}");
                }

            }
        }

        public static void OutputCharacterFileList(CharacterMakingIndex aquaCMX, PSO2Text partsText, PSO2Text acceText, string pso2_binDir, string outputDirectory)
        {
            //Since we have an idea of what should be there and what we're interested in parsing out, throw these into a dictionary and go
            Dictionary<string, List<List<PSO2Text.textPair>>> textByCat = new Dictionary<string, List<List<PSO2Text.textPair>>>();
            for (int i = 0; i < partsText.text.Count; i++)
            {
                textByCat.Add(partsText.categoryNames[i], partsText.text[i]);
            }
            for (int i = 0; i < acceText.text.Count; i++)
            {
                textByCat.Add(acceText.categoryNames[i], acceText.text[i]);
            }

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
            StringBuilder outputUnknown = new StringBuilder();

            //Build text Dict
            List<int> masterIdList = new List<int>();
            List<Dictionary<int, string>> nameDicts = new List<Dictionary<int, string>>();
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
                string output = "";
                bool named = false;
                foreach (var dict in nameDicts)
                {
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

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    output = $"[Unnamed {id}]" + output;
                }

                //Double check these ids and use an adjustedId if needed
                int adjustedId = id;
                if (aquaCMX.costumeIdLink.ContainsKey(id))
                {
                    adjustedId = aquaCMX.costumeIdLink[id].fileId;
                }
                else if (aquaCMX.castArmIdLink.ContainsKey(id))
                {
                    adjustedId = aquaCMX.outerWearIdLink[id].fileId;
                }

                //Decide if bd or ow
                string typeString = "bd_";
                bool classicOwCheck = id >= 20000 && id < 40000;
                bool rebootOwCheck = id >= 100000 && id < 300000;
                if (classicOwCheck == true || rebootOwCheck == true)
                {
                    typeString = "ow_";
                }

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{CharacterMakingIndex.rebootStart}{typeString}{adjustedId}.ice";
                    string rebEx = $"{CharacterMakingIndex.rebootExStart}{typeString}{adjustedId}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }
                    output += "\n";
                    output = AddBodyExtraFiles(output, reb, pso2_binDir, "_" + typeString);


                    output += ",[HQ Texture Ice]," + rebExHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, rebExHash)))
                    {
                        output += ", (Not found)";
                    }
                    output += "\n";
                    output = AddBodyExtraFiles(output, rebEx, pso2_binDir, "_" + typeString);

                }
                else
                {
                    string finalId = ToFive(adjustedId);
                    string classic = $"{CharacterMakingIndex.classicStart}bd_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }
                    output += "\n";
                    output = AddBodyExtraFiles(output, classic, pso2_binDir, "_" + typeString);
                }

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
                    outputUnknown.Append(output);
                }
            }
            File.WriteAllText(Path.Combine(outputDirectory, "MaleCostumes.csv"), outputCostumeMale.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "FemaleCostumes.csv"), outputCostumeFemale.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "MaleOuters.csv"), outputOuterMale.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "FemaleOuters.csv"), outputOuterFemale.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "CastBodies.csv"), outputCastBody.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "CasealBodies.csv"), outputCasealBody.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "MaleNGSOuters.csv"), outputNGSOuterMale.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "FemaleNGSOuters.csv"), outputNGSOuterFemale.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "CastNGSBodies.csv"), outputNGSCastBody.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "CasealNGSBodies.csv"), outputNGSCasealBody.ToString());
            //File.WriteAllText(Path.Combine(outputDirectory, "MaleNGSCostumes.csv"), outputNGSCostumeMale.ToString());
            //File.WriteAllText(Path.Combine(outputDirectory, "FemaleNGSCostumes.csv"), outputNGSCostumeFemale.ToString());
            if(outputUnknown.Length > 0)
            {
                File.WriteAllText(Path.Combine(outputDirectory, "UnknownOutfits.csv"), outputUnknown.ToString());
            }

            //---------------------------Parse out basewear
            StringBuilder outputBasewearMale = new StringBuilder();
            StringBuilder outputBasewearFemale = new StringBuilder();
            StringBuilder outputNGSBasewearMale = new StringBuilder();
            StringBuilder outputNGSBasewearFemale = new StringBuilder();
            StringBuilder outputNGSGenderlessBasewear = new StringBuilder();

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
                string output = "";
                bool named = false;
                foreach (var dict in nameDicts)
                {
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

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    output = $"[Unnamed {id}]" + output;
                }

                //Double check these ids and use an adjustedId if needed
                int adjustedId = id;
                if (aquaCMX.baseWearIdLink.ContainsKey(id))
                {
                    adjustedId = aquaCMX.baseWearIdLink[id].fileId;
                }
                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{CharacterMakingIndex.rebootStart}bw_{adjustedId}.ice";
                    string rebEx = $"{CharacterMakingIndex.rebootExStart}bw_{adjustedId}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";
                    output = AddBasewearExtraFiles(output, reb, pso2_binDir);

                    output += ",[HQ Texture Ice]," + rebExHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, rebExHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";
                    output = AddBasewearExtraFiles(output, rebEx, pso2_binDir);

                }
                else
                {
                    string finalId = ToFive(adjustedId);
                    string classic = $"{CharacterMakingIndex.classicStart}bw_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";
                    output = AddBasewearExtraFiles(output, classic, pso2_binDir);

                }

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
                } else if (id < 600000)
                {
                    outputNGSGenderlessBasewear.Append(output);
                } else
                {
                    Console.WriteLine("Unknown bw with id: " + id);
                }
            }
            File.WriteAllText(Path.Combine(outputDirectory, "MaleBasewear.csv"), outputBasewearMale.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "FemaleBasewear.csv"), outputBasewearFemale.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "MaleNGSBasewear.csv"), outputNGSBasewearMale.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "FemaleNGSBasewear.csv"), outputNGSBasewearFemale.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "GenderlessNGSBasewear.csv"), outputNGSGenderlessBasewear.ToString());

            //---------------------------Parse out innerwear
            StringBuilder outputInnerwearMale = new StringBuilder();
            StringBuilder outputInnerwearFemale = new StringBuilder();
            StringBuilder outputNGSInnerwearMale = new StringBuilder();
            StringBuilder outputNGSInnerwearFemale = new StringBuilder();

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "innerwear", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.innerWearDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                string output = "";
                bool named = false;
                foreach (var dict in nameDicts)
                {
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

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    output = $"[Unnamed {id}]" + output;
                }

                //Double check these ids and use an adjustedId if needed
                int adjustedId = id;
                if (aquaCMX.innerWearIdLink.ContainsKey(id))
                {
                    adjustedId = aquaCMX.innerWearIdLink[id].fileId;
                }
                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{CharacterMakingIndex.rebootStart}iw_{adjustedId}.ice";
                    string rebEx = $"{CharacterMakingIndex.rebootExStart}iw_{adjustedId}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                    output += ",[HQ Texture Ice]," + rebExHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, rebExHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }
                else
                {
                    string finalId = ToFive(adjustedId);
                    string classic = $"{CharacterMakingIndex.classicStart}iw_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }

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
            File.WriteAllText(Path.Combine(outputDirectory, "MaleInnerwear.csv"), outputInnerwearMale.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "FemaleInnerwear.csv"), outputInnerwearFemale.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "MaleNGSInnerwear.csv"), outputNGSInnerwearMale.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "FemaleNGSInnerwear.csv"), outputNGSInnerwearFemale.ToString());

            //---------------------------Parse out cast arms
            StringBuilder outputCastArmMale = new StringBuilder();
            StringBuilder outputCastArmFemale = new StringBuilder();
            StringBuilder outputNGSCastArmMale = new StringBuilder();
            StringBuilder outputNGSCastArmFemale = new StringBuilder();

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "arm", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.carmDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                string output = "";
                bool named = false;
                foreach (var dict in nameDicts)
                {
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

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    output = $"[Unnamed {id}]" + output;
                }

                //Double check these ids and use an adjustedId if needed
                int adjustedId = id;
                if (aquaCMX.castArmIdLink.ContainsKey(id))
                {
                    adjustedId = aquaCMX.castArmIdLink[id].fileId;
                }
                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{CharacterMakingIndex.rebootStart}am_{adjustedId}.ice";
                    string rebEx = $"{CharacterMakingIndex.rebootExStart}am_{adjustedId}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                    output += ",[HQ Texture Ice]," + rebExHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, rebExHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }
                else
                {
                    string finalId = ToFive(adjustedId);
                    string classic = $"{CharacterMakingIndex.classicStart}am_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }

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
            File.WriteAllText(Path.Combine(outputDirectory, "CastArms.csv"), outputCastArmMale.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "CasealArms.csv"), outputCastArmFemale.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "NGSCastArms.csv"), outputNGSCastArmMale.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "NGSCasealArms.csv"), outputNGSCastArmFemale.ToString());

            //---------------------------Parse out cast legs
            StringBuilder outputCastLegMale = new StringBuilder();
            StringBuilder outputCastLegFemale = new StringBuilder();
            StringBuilder outputNGSCastLegMale = new StringBuilder();
            StringBuilder outputNGSCastLegFemale = new StringBuilder();

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "Leg", true); //Yeah for some reason this string starts capitalized while none of the others do... don't ask me.

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.clegDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                string output = "";
                bool named = false;
                foreach (var dict in nameDicts)
                {
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

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    output = $"[Unnamed {id}]" + output;
                }

                //Double check these ids and use an adjustedId if needed
                int adjustedId = id;
                if (aquaCMX.clegIdLink.ContainsKey(id))
                {
                    adjustedId = aquaCMX.clegIdLink[id].fileId;
                }
                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{CharacterMakingIndex.rebootStart}lg_{adjustedId}.ice";
                    string rebEx = $"{CharacterMakingIndex.rebootExStart}lg_{adjustedId}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                    output += ",[HQ Texture Ice]," + rebExHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, rebExHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }
                else
                {
                    string finalId = ToFive(adjustedId);
                    string classic = $"{CharacterMakingIndex.classicStart}lg_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }

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
            File.WriteAllText(Path.Combine(outputDirectory, "CastLegs.csv"), outputCastLegMale.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "CasealLegs.csv"), outputCastLegFemale.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "NGSCastLegs.csv"), outputNGSCastLegMale.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "NGSCasealLegs.csv"), outputNGSCastLegFemale.ToString());

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

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "bodypaint1", true); 

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.bodyPaintDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                string output = "";
                bool named = false;
                foreach (var dict in nameDicts)
                {
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

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    output = $"[Unnamed {id}]" + output;
                }

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{CharacterMakingIndex.rebootStart}b1_{id}.ice";
                    string rebEx = $"{CharacterMakingIndex.rebootExStart}b1_{id}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                    output += ",[HQ Texture Ice]," + rebExHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, rebExHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }
                else
                {
                    string finalId = ToFive(id);
                    string classic = $"{CharacterMakingIndex.classicStart}b1_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }

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
                } else if(id < 600000)
                {
                    outputNGSGenderlessBodyPaint.Append(output);
                }
                else
                {
                    Console.WriteLine("Unknown b1 with id: " + id);
                }
            }
            File.WriteAllText(Path.Combine(outputDirectory, "MaleBodyPaint.csv"), outputMaleBodyPaint.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "FemaleBodyPaint.csv"), outputFemaleBodyPaint.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "MaleLayeredBodyPaint.csv"), outputMaleLayeredBodyPaint.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "FemaleLayeredBodyPaint.csv"), outputFemaleLayeredBodyPaint.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "NGSMaleBodyPaint.csv"), outputNGSMaleBodyPaint.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "NGSFemaleBodyPaint.csv"), outputNGSFemaleBodyPaint.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "NGSCastBodyPaint.csv"), outputNGSCastMaleBodyPaint.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "NGSCasealBodyPaint.csv"), outputNGSCastFemaleBodyPaint.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "NGSGenderlessBodyPaint.csv"), outputNGSGenderlessBodyPaint.ToString());

            //---------------------------Parse out stickers
            StringBuilder outputStickers = new StringBuilder();

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "bodypaint2", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.stickerDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                string output = "";
                bool named = false;
                foreach (var dict in nameDicts)
                {
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

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    output = $"[Unnamed {id}]" + output;
                }

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{CharacterMakingIndex.rebootStart}b2_{id}.ice";
                    string rebEx = $"{CharacterMakingIndex.rebootExStart}b2_{id}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                    output += ",[HQ Texture Ice]," + rebExHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, rebExHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }
                else
                {
                    string finalId = ToFive(id);
                    string classic = $"{CharacterMakingIndex.classicStart}b2_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }

                outputStickers.Append(output);
            }
            File.WriteAllText(Path.Combine(outputDirectory, "Stickers.csv"), outputStickers.ToString());

            //---------------------------Parse out hair
            StringBuilder outputMaleHair = new StringBuilder();
            StringBuilder outputFemaleHair = new StringBuilder();
            StringBuilder outputCasealHair = new StringBuilder();
            StringBuilder outputNGSHair = new StringBuilder();

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "hair", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.hairDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                string output = "";
                bool named = false;
                foreach (var dict in nameDicts)
                {
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

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    output = $"[Unnamed {id}]" + output;
                }

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{CharacterMakingIndex.rebootStart}hr_{id}.ice";
                    string rebEx = $"{CharacterMakingIndex.rebootExStart}hr_{id}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                    output += ",[HQ Texture Ice]," + rebExHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, rebExHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }
                else
                {
                    string finalId = ToFive(id);
                    string classic = $"{CharacterMakingIndex.classicStart}hr_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }

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
            File.WriteAllText(Path.Combine(outputDirectory, "MaleHair.csv"), outputMaleHair.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "FemaleHair.csv"), outputFemaleHair.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "CasealHair.csv"), outputCasealHair.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "NGSHair.csv"), outputNGSHair.ToString());

            //---------------------------Parse out Eye
            StringBuilder outputEyes = new StringBuilder();
            StringBuilder outputNGSEyes = new StringBuilder();

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "eye", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.stickerDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                string output = "";
                bool named = false;
                foreach (var dict in nameDicts)
                {
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

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    output = $"[Unnamed {id}]" + output;
                }

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{CharacterMakingIndex.rebootStart}ey_{id}.ice";
                    string rebEx = $"{CharacterMakingIndex.rebootExStart}ey_{id}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                    output += ",[HQ Texture Ice]," + rebExHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, rebExHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }
                else
                {
                    string finalId = ToFive(id);
                    string classic = $"{CharacterMakingIndex.classicStart}ey_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }

                if (id >= 100000)
                {
                    outputEyes.Append(output);
                } else
                {
                    outputNGSEyes.Append(output);
                }
            }
            File.WriteAllText(Path.Combine(outputDirectory, "Eyes.csv"), outputEyes.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "NGSEyes.csv"), outputNGSEyes.ToString());

            //---------------------------Parse out EYEB
            StringBuilder outputEyebrows = new StringBuilder();
            StringBuilder outputNGSEyebrows = new StringBuilder();

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "eyebrows", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.stickerDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                string output = "";
                bool named = false;
                foreach (var dict in nameDicts)
                {
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

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    output = $"[Unnamed {id}]" + output;
                }

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{CharacterMakingIndex.rebootStart}eb_{id}.ice";
                    string rebEx = $"{CharacterMakingIndex.rebootExStart}eb_{id}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                    output += ",[HQ Texture Ice]," + rebExHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, rebExHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }
                else
                {
                    string finalId = ToFive(id);
                    string classic = $"{CharacterMakingIndex.classicStart}eb_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }

                if (id >= 100000)
                {
                    outputEyebrows.Append(output);
                }
                else
                {
                    outputNGSEyebrows.Append(output);
                }
            }
            File.WriteAllText(Path.Combine(outputDirectory, "Eyebrows.csv"), outputEyebrows.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "NGSEyebrows.csv"), outputNGSEyebrows.ToString());

            //---------------------------Parse out EYEL
            StringBuilder outputEyelashes = new StringBuilder();
            StringBuilder outputNGSEyelashes = new StringBuilder();

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "eyelashes", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.stickerDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                string output = "";
                bool named = false;
                foreach (var dict in nameDicts)
                {
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

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    output = $"[Unnamed {id}]" + output;
                }

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{CharacterMakingIndex.rebootStart}el_{id}.ice";
                    string rebEx = $"{CharacterMakingIndex.rebootExStart}el_{id}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                    output += ",[HQ Texture Ice]," + rebExHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, rebExHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }
                else
                {
                    string finalId = ToFive(id);
                    string classic = $"{CharacterMakingIndex.classicStart}el_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }

                if (id >= 100000)
                {
                    outputEyelashes.Append(output);
                }
                else
                {
                    outputNGSEyelashes.Append(output);
                }
            }
            File.WriteAllText(Path.Combine(outputDirectory, "Eyelashes.csv"), outputEyelashes.ToString());
            File.WriteAllText(Path.Combine(outputDirectory, "NGSEyelashes.csv"), outputNGSEyelashes.ToString());

            //---------------------------Parse out ACCE //Stored under decoy in a99be286e3a7e1b45d88a3ea4d6c18c4
            StringBuilder outputAccessories = new StringBuilder();

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
                foreach (var dict in nameDicts)
                {
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

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    output = $"[Unnamed {id}]" + output;
                }

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{CharacterMakingIndex.rebootStart}ac_{id}.ice";
                    string rebEx = $"{CharacterMakingIndex.rebootExStart}ac_{id}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                    output += ",[HQ Texture Ice]," + rebExHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, rebExHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }
                else
                {
                    string finalId = ToFive(id);
                    string classic = $"{CharacterMakingIndex.classicStart}ac_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }

                outputAccessories.Append(output);
            }
            File.WriteAllText(Path.Combine(outputDirectory, "Accessories.csv"), outputAccessories.ToString());

            //---------------------------Parse out FCP1

            //---------------------------Parse out FCP2

            //---------------------------Parse out FACE //face_variation.cmp.lua in 75b1632526cd6a1039625349df6ee8dd used to map file face ids to .text ids

            //----------------------------//End CMX related ids

            //---------------------------Parse out voices 

            //---------------------------Parse out NGS ears //The cmx has ear data, but no ids. Maybe it's done by order? Same for teeth and horns

            //---------------------------Parse out NGS teeth 

            //---------------------------Parse out NGS horns 

        }

        private static string AddBodyExtraFiles(string output, string classic, string pso2_binDir, string typeString)
        {
            string rpCheck = GetFileHash(classic.Replace(".ice", "_rp.ice"));
            string bmCheck = GetFileHash(classic.Replace(typeString, "_bm_"));
            string hnCheck = GetFileHash(classic.Replace(typeString, "_hn_")); //If not basewear, hn. If basewear, ho

            //_rp alt model
            if (File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, rpCheck)))
            {
                output += ",[Alt Model]," + rpCheck + "\n";
            }
            //Aqv archive
            if (File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, bmCheck)))
            {
                output += ",[Aqv]," + bmCheck + "\n";
            }
            //Hand textures
            if (File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, hnCheck)))
            {
                output += ",[Hand Textures]," + hnCheck + "\n";
            }

            return output;
        }

        private static string AddBasewearExtraFiles(string output, string classic, string pso2_binDir)
        {
            string rpCheck = GetFileHash(classic.Replace(".ice", "_rp.ice"));
            string hnCheck = GetFileHash(classic.Replace("bw", "ho")); //If not basewear, hn. If basewear, ho

            //_rp alt model
            if (File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, rpCheck)))
            {
                output += ",[Alt Model]," + rpCheck + "\n";
            }
            //Hand textures
            if (File.Exists(Path.Combine(pso2_binDir, CharacterMakingIndex.dataDir, hnCheck)))
            {
                output += ",[Hand Textures]," + hnCheck + "\n";
            }

            return output;
        }

        private static void GatherDictKeys<T>(List<int> masterIdList, Dictionary<int, T>.KeyCollection keys)
        {
            foreach (int key in keys)
            {
                if (!masterIdList.Contains(key))
                {
                    masterIdList.Add(key);
                }
            }
        }

        private static void GatherTextIds(Dictionary<string, List<List<PSO2Text.textPair>>> textByCat, List<int> masterIdList, List<Dictionary<int, string>> nameDicts, string category, bool firstDictSet)
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

        public static Dictionary<int, string> ReadFaceVariationLua(string face_variationFileName)
        {
            Dictionary<int, string> faceStrings = new Dictionary<int, string>();

            using (StreamReader streamReader = new StreamReader(face_variationFileName))
            {
                string language = null;
                
                //Cheap hacky way to do this. We don't really need to keep these separate so we loop through and get the 'language' value, our key for the .text dictionary. Our key to get this is the number from crop_name
                while(streamReader.EndOfStream == false)
                {
                    var line = streamReader.ReadLine();
                    if(language == null)
                    {
                        if(line.Contains("language"))
                        {
                            language = line.Split('\"')[1];
                        }
                    } else if(line.Contains("crop_name"))
                    {
                        language = null;
                        line = line.Split('\"')[1];
                        if(line != "") //ONE face doesn't use a crop_name. As it also doesn't have a crop_name, we don't care. Otherwise, add it to the dict
                        {
                            faceStrings.Add(Int32.Parse(line.Substring(7)), language);
                        }
                    }
                }
            }

            return faceStrings;
        }

        public static string ToFive(int num)
        {
            string numString = num.ToString();
            while (numString.Length < 5)
            {
                numString = numString.Insert(0, "0");
            }

            return numString;
        }

        public static string GetFileHash(string str)
        {
            if (str == null)
            {
                return "";
            }
            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(new UTF8Encoding().GetBytes(str));
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
        }
    }
}
