using AquaModelLibrary.AquaMethods;
using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Zamboni;
using static AquaModelLibrary.AquaMethods.AquaGeneralMethods;
using static AquaModelLibrary.AquaMiscMethods;
using static AquaModelLibrary.CharacterMakingIndex;
using static AquaModelLibrary.VTBFMethods;

namespace AquaModelLibrary
{
    public unsafe static class CharacterMakingIndexMethods
    {
        public static CharacterMakingIndex ReadCMX(string fileName, CharacterMakingIndex cmx = null)
        {
            using (Stream stream = (Stream)new MemoryStream(File.ReadAllBytes(fileName)))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                return BeginReadCMX(streamReader, cmx);
            }
        }

        public static CharacterMakingIndex ReadCMX(byte[] file, CharacterMakingIndex cmx = null)
        {
            using (Stream stream = (Stream)new MemoryStream(file))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                return BeginReadCMX(streamReader, cmx);
            }
        }

        public static CharacterMakingIndex BeginReadCMX(BufferedStreamReader streamReader, CharacterMakingIndex cmx = null)
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
                cmx = ReadNIFLCMX(streamReader, offset);
                return cmx;
            }
            else if (type.Equals("VTBF"))
            {
                //VTBF
                return ReadVTBFCMX(streamReader, cmx);
            }
            else
            {
                MessageBox.Show("Improper File Format!");
                return null;
            }
        }

        public static CharacterMakingIndex ReadVTBFCMX(BufferedStreamReader streamReader, CharacterMakingIndex cmx = null)
        {
            //Create one if it's not passed. The idea is multiple files can be combined as needed.
            if (cmx == null)
            {
                cmx = new CharacterMakingIndex();
            }

            //Due to how structs are laid out and their repeating, we have to assume these are in order and that specific sections correlate to specific types
            bool pastBody0 = false;
            bool pastCarm0 = false;
            bool pastBcln0 = false;

            //Seek to and read the DOC tag's 
            streamReader.Seek(0x1C, SeekOrigin.Current);
            int cmxCount = streamReader.Read<ushort>();
            streamReader.Seek(0x2, SeekOrigin.Current);
            //Read tags, get id from subtag 0xFF, and assign to dictionary with that. 
            for (int i = 0; i < cmxCount; i++)
            {
                List<Dictionary<int, object>> data = ReadVTBFTag(streamReader, out string tagType, out int ptrCount, out int entryCount);
                switch (tagType)
                {
                    case "ACCE":
                        cmx.accessoryDict.Add((int)data[0][0xFF], parseACCE(data));
                        break;
                    case "BODY":
                        if (pastBody0 == false)
                        {
                            cmx.costumeDict.Add((int)data[0][0xFF], parseBODY(data));
                        }
                        else
                        {
                            cmx.baseWearDict.Add((int)data[0][0xFF], parseBODY(data));
                        }
                        break;
                    case "CARM":
                        pastBody0 = true;
                        if (pastCarm0 == false)
                        {
                            cmx.carmDict.Add((int)data[0][0xFF], parseCARM(data));
                        }
                        else
                        {
                            cmx.outerDict.Add((int)data[0][0xFF], parseCARM(data));
                        }
                        break;
                    case "CLEG":
                        pastCarm0 = true;
                        cmx.clegDict.Add((int)data[0][0xFF], parseCLEG(data));
                        break;
                    case "BDP1":
                        cmx.bodyPaintDict.Add((int)data[0][0xFF], parseBDP1(data));
                        break;
                    case "BDP2":
                        cmx.stickerDict.Add((int)data[0][0xFF], parseBDP2(data));
                        break;
                    case "FACE":
                        cmx.faceDict.Add((int)data[0][0xFF], parseFACE(data));
                        break;
                    case "FCMN":
                        cmx.fcmnDict.Add((int)data[0][0xFF], parseFCMN(data));
                        break;
                    case "FCP1":
                        //These were kinda redundant with the FACE structs so SEGA yeeted them for NIFL. Not much reason to read them.
                        //cmx.fcpDict.Add((int)data[0][0xFF], parseFCP1(data));
                        break;
                    case "FCP2":
                        cmx.fcpDict.Add((int)data[0][0xFF], parseFCP2(data));
                        break;
                    case "EYE ":
                        cmx.eyeDict.Add((int)data[0][0xFF], parseEYE(data));
                        break;
                    case "EYEB":
                        cmx.eyebrowDict.Add((int)data[0][0xFF], parseEYEB(data));
                        break;
                    case "EYEL":
                        cmx.eyelashDict.Add((int)data[0][0xFF], parseEYEB(data));
                        break;
                    case "HAIR":
                        cmx.hairDict.Add((int)data[0][0xFF], parseHAIR(data));
                        break;
                    case "COL ":
                        cmx.legacyColDict.Add((int)data[0][0xFF], parseCOL(data));
                        break;
                    case "BBLY":
                        cmx.innerWearDict.Add((int)data[0][0xFF], parseBBLY(data));
                        break;
                    case "BCLN":
                        if (pastBcln0 == false)
                        {
                            cmx.costumeIdLink.Add((int)data[0][0xFF], parseLN(data));
                        }
                        else
                        {
                            cmx.baseWearIdLink.Add((int)data[0][0xFF], parseLN(data));
                        }
                        break;
                    case "LCLN":
                        pastBcln0 = true;
                        cmx.clegIdLink.Add((int)data[0][0xFF], parseLN(data));
                        break;
                    case "ACLN":
                        cmx.castArmIdLink.Add((int)data[0][0xFF], parseLN(data));
                        break;
                    case "ICLN":
                        cmx.innerWearIdLink.Add((int)data[0][0xFF], parseLN(data));
                        break;
                    default:
                        throw new Exception($"Unexpected tag type {tagType}");
                }
            }

            return cmx;
        }

        public static CharacterMakingIndex ReadNIFLCMX(BufferedStreamReader streamReader, int offset)
        {
            var cmx = new CharacterMakingIndex();
            cmx.nifl = streamReader.Read<AquaCommon.NIFL>();
            cmx.rel0 = streamReader.Read<AquaCommon.REL0>();

            streamReader.Seek(cmx.rel0.REL0Size + offset, SeekOrigin.Begin);
            AlignReader(streamReader, 0x10);
            cmx.nof0 = AquaCommon.readNOF0(streamReader);

            streamReader.Seek(cmx.rel0.REL0DataStart + offset, SeekOrigin.Begin);
            cmx.cmxTable = ReadCMXTable(streamReader, cmx.rel0.REL0DataStart);

            ReadBODY(streamReader, offset, cmx.cmxTable.bodyAddress, cmx.cmxTable.bodyCount, cmx.costumeDict, cmx.rel0.REL0DataStart);
            ReadBODY(streamReader, offset, cmx.cmxTable.carmAddress, cmx.cmxTable.carmCount, cmx.carmDict, cmx.rel0.REL0DataStart);
            ReadBODY(streamReader, offset, cmx.cmxTable.clegAddress, cmx.cmxTable.clegCount, cmx.clegDict, cmx.rel0.REL0DataStart);
            ReadBODY(streamReader, offset, cmx.cmxTable.bodyOuterAddress, cmx.cmxTable.bodyOuterCount, cmx.outerDict, cmx.rel0.REL0DataStart);

            ReadBODY(streamReader, offset, cmx.cmxTable.baseWearAddress, cmx.cmxTable.baseWearCount, cmx.baseWearDict, cmx.rel0.REL0DataStart);
            ReadBBLY(streamReader, offset, cmx.cmxTable.innerWearAddress, cmx.cmxTable.innerWearCount, cmx.innerWearDict);
            ReadBBLY(streamReader, offset, cmx.cmxTable.bodyPaintAddress, cmx.cmxTable.bodyPaintCount, cmx.bodyPaintDict);
            ReadSticker(streamReader, offset, cmx.cmxTable.stickerAddress, cmx.cmxTable.stickerCount, cmx.stickerDict);

            ReadFACE(streamReader, offset, cmx.cmxTable.faceAddress, cmx.cmxTable.faceCount, cmx.faceDict, cmx.rel0.REL0DataStart);
            ReadFCMN(streamReader, offset, cmx.cmxTable.faceMotionAddress, cmx.cmxTable.faceMotionCount, cmx.fcmnDict);
            ReadFaceTextures(streamReader, offset, cmx.cmxTable.faceTextureAddress, cmx.cmxTable.faceTextureCount, cmx.faceTextureDict);
            ReadFCP(streamReader, offset, cmx.cmxTable.faceTexturesAddress, cmx.cmxTable.faceTexturesCount, cmx.fcpDict);

            ReadACCE(streamReader, offset, cmx.cmxTable.accessoryAddress, cmx.cmxTable.accessoryCount, cmx.accessoryDict, cmx.rel0.REL0DataStart);
            ReadEYE(streamReader, offset, cmx.cmxTable.eyeTextureAddress, cmx.cmxTable.eyeTextureCount, cmx.eyeDict);

            ReadNGSEAR(streamReader, offset, cmx);
            ReadNGSTEETH(streamReader, offset, cmx);
            ReadNGSHorn(streamReader, offset, cmx);

            ReadNGSSKIN(streamReader, offset, cmx.cmxTable.skinAddress, cmx.cmxTable.skinCount, cmx.ngsSkinDict);
            ReadEYEB(streamReader, offset, cmx.cmxTable.eyebrowAddress, cmx.cmxTable.eyebrowCount, cmx.eyebrowDict);
            ReadEYEB(streamReader, offset, cmx.cmxTable.eyelashAddress, cmx.cmxTable.eyelashCount, cmx.eyelashDict);

            ReadHAIR(streamReader, offset, cmx.cmxTable.hairAddress, cmx.cmxTable.hairCount, cmx.hairDict);
            ReadNIFLCOL(streamReader, offset, cmx.cmxTable.colAddress, cmx.cmxTable.colCount, cmx.colDict);

            //Unk
            streamReader.Seek(cmx.cmxTable.unkAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < cmx.cmxTable.unkCount; i++)
            {
                cmx.unkList.Add(streamReader.Read<Unk_IntField>());
            }

            ReadIndexLinks(streamReader, offset, cmx.cmxTable.costumeIdLinkAddress, cmx.cmxTable.costumeIdLinkCount, cmx.costumeIdLink, cmx.rel0.REL0DataStart);
            ReadIndexLinks(streamReader, offset, cmx.cmxTable.castArmIdLinkAddress, cmx.cmxTable.castArmIdLinkCount, cmx.castArmIdLink, cmx.rel0.REL0DataStart);
            ReadIndexLinks(streamReader, offset, cmx.cmxTable.castLegIdLinkAddress, cmx.cmxTable.castLegIdLinkCount, cmx.clegIdLink, cmx.rel0.REL0DataStart);

            //If after a oct 21, the order is changed and we need to read things differently as the addresses get shifted down
            if (cmx.cmxTable.oct21UnkAddress != 0)
            {
                ReadIndexLinks(streamReader, offset, cmx.cmxTable.outerIdLinkAddress, cmx.cmxTable.outerIdLinkCount, cmx.castHeadIdLink, cmx.rel0.REL0DataStart);
                ReadIndexLinks(streamReader, offset, cmx.cmxTable.baseWearIdLinkAddress, cmx.cmxTable.baseWearIdLinkCount, cmx.outerWearIdLink, cmx.rel0.REL0DataStart);
                ReadIndexLinks(streamReader, offset, cmx.cmxTable.innerWearIdLinkAddress, cmx.cmxTable.innerWearIdLinkCount, cmx.baseWearIdLink, cmx.rel0.REL0DataStart);
                ReadIndexLinks(streamReader, offset, cmx.cmxTable.oct21UnkAddress, cmx.cmxTable.oct21UnkCount, cmx.innerWearIdLink, cmx.rel0.REL0DataStart);
                if (cmx.cmxTable.jun7_22Address != 0)
                {
                    ReadPart6_7_22(streamReader, offset, cmx.cmxTable.jun7_22Address, cmx.cmxTable.jun7_22Count, cmx.part6_7_22Dict, cmx.rel0.REL0DataStart);
                }
            }
            else
            {
                ReadIndexLinks(streamReader, offset, cmx.cmxTable.outerIdLinkAddress, cmx.cmxTable.outerIdLinkCount, cmx.outerWearIdLink, cmx.rel0.REL0DataStart);
                ReadIndexLinks(streamReader, offset, cmx.cmxTable.baseWearIdLinkAddress, cmx.cmxTable.baseWearIdLinkCount, cmx.baseWearIdLink, cmx.rel0.REL0DataStart);
                ReadIndexLinks(streamReader, offset, cmx.cmxTable.innerWearIdLinkAddress, cmx.cmxTable.innerWearIdLinkCount, cmx.innerWearIdLink, cmx.rel0.REL0DataStart);
            }
            if (cmx.cmxTable.feb8_22UnkAddress != 0)
            {
                ReadIndexLinks(streamReader, offset, cmx.cmxTable.feb8_22UnkAddress, cmx.cmxTable.feb8_22UnkCount, cmx.accessoryIdLink, cmx.rel0.REL0DataStart);
            }

#if DEBUG
            //CalcCMXPointers(cmx.cmxTable, cmx.nof0, cmx.rel0.REL0DataStart, cmx);
#endif 

            return cmx;
        }

        public static void WriteCMX(string filename, CharacterMakingIndex cmx, int mode = 0)
        {
            File.WriteAllBytes(filename, CMXToBytes(cmx, mode));
        }

        //Mode 1 will be latest. Mode 0 will be benchmark
        public static byte[] CMXToBytes(CharacterMakingIndex cmx, int mode = 0)
        {
            List<byte> outBytes = new List<byte>();
            List<int> nof0PointerLocations = new List<int>(); //Used for the NOF0 section
            Dictionary<string, List<int>> textAddressDict = new Dictionary<string, List<int>>();
            List<string> textList = new List<string>();
            int rel0SizeOffset = 0;

            //REL0
            outBytes.AddRange(Encoding.UTF8.GetBytes("REL0"));
            rel0SizeOffset = outBytes.Count; //We'll fill this later
            outBytes.AddRange(BitConverter.GetBytes(0));
            outBytes.AddRange(BitConverter.GetBytes(0));
            outBytes.AddRange(BitConverter.GetBytes(0));

            outBytes.AddRange(BitConverter.GetBytes(-1));

            //BODY
            //Costumes/Cast Bodies
            int bodyAddress = outBytes.Count;
            var costList = cmx.costumeDict.Keys.ToList();
            costList.Sort();
            foreach (var key in costList)
            {
                AddBodyBytes(cmx.costumeDict[key], mode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //Cast Arms
            int carmAddress = outBytes.Count;
            var carmList = cmx.carmDict.Keys.ToList();
            carmList.Sort();
            foreach (var key in carmList)
            {
                AddBodyBytes(cmx.carmDict[key], mode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //Cast Legs
            int clegAddress = outBytes.Count;
            var clegList = cmx.clegDict.Keys.ToList();
            clegList.Sort();
            foreach (var key in clegList)
            {
                AddBodyBytes(cmx.clegDict[key], mode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //BDP1
            //Body Paint
            int bodyPaintAddress = outBytes.Count;
            var bdpList = cmx.bodyPaintDict.Keys.ToList();
            bdpList.Sort();
            foreach (var key in bdpList)
            {
                AddBDPBytes(cmx.bodyPaintDict[key], mode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //Stickers
            int stickerAddress = outBytes.Count;
            var stickerList = cmx.stickerDict.Keys.ToList();
            stickerList.Sort();
            foreach (var key in stickerList)
            {
                AddStickerBytes(cmx.stickerDict[key], mode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //FACE
            int faceAddress = outBytes.Count;
            var faceList = cmx.faceDict.Keys.ToList();
            faceList.Sort();
            foreach (var key in faceList)
            {
                AddFACEBytes(cmx.faceDict[key], mode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //FCMN
            int fcmnAddress = outBytes.Count;
            var fcmnList = cmx.fcmnDict.Keys.ToList();
            fcmnList.Sort();
            foreach (var key in fcmnList)
            {
                AddFCMNBytes(cmx.fcmnDict[key], mode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //FaceTextures
            int ftexAddress = outBytes.Count;
            var ftexList = cmx.faceTextureDict.Keys.ToList();
            ftexList.Sort();
            foreach (var key in ftexList)
            {
                AddFaceTexturesBytes(cmx.faceTextureDict[key], mode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //FCP
            int fcpAddress = outBytes.Count;
            var fcpList = cmx.fcpDict.Keys.ToList();
            fcpList.Sort();
            foreach (var key in fcpList)
            {
                AddFCPBytes(cmx.fcpDict[key], mode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //ACCE
            int acceAddress = outBytes.Count;
            var acceList = cmx.accessoryDict.Keys.ToList();
            acceList.Sort();
            foreach (var key in acceList)
            {
                AddACCEBytes(cmx.accessoryDict[key], mode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //EYE
            int eyeAddress = outBytes.Count;
            var eyeList = cmx.eyeDict.Keys.ToList();
            eyeList.Sort();
            foreach (var key in eyeList)
            {
                AddEYEBytes(cmx.eyeDict[key], mode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //EAR
            int earAddress = outBytes.Count;
            var earList = cmx.ngsEarDict.Keys.ToList();
            earList.Sort();
            foreach (var key in earList)
            {
                AddEARBytes(cmx.ngsEarDict[key], mode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //Teeth
            int teethAddress = outBytes.Count;
            var teethList = cmx.ngsTeethDict.Keys.ToList();
            teethList.Sort();
            foreach (var key in teethList)
            {
                AddTeethBytes(cmx.ngsTeethDict[key], mode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //Horn
            int hornAddress = outBytes.Count;
            var hornList = cmx.ngsHornDict.Keys.ToList();
            hornList.Sort();
            foreach (var key in hornList)
            {
                AddHornBytes(cmx.ngsHornDict[key], mode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //Skin
            int skinAddress = outBytes.Count;
            var skinList = cmx.ngsSkinDict.Keys.ToList();
            skinList.Sort();
            foreach (var key in skinList)
            {
                AddSkinBytes(cmx.ngsSkinDict[key], mode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //EYEB
            //Eyebrow
            int eyeBAddress = outBytes.Count;
            var eyeBList = cmx.eyebrowDict.Keys.ToList();
            eyeBList.Sort();
            foreach (var key in eyeBList)
            {
                AddEYEBBytes(cmx.eyebrowDict[key], mode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //Eyelash
            int eyeLAddress = outBytes.Count;
            var eyeLList = cmx.eyelashDict.Keys.ToList();
            eyeLList.Sort();
            foreach (var key in eyeLList)
            {
                AddEYEBBytes(cmx.eyelashDict[key], mode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //Hair
            int hairAddress = outBytes.Count;
            var hairList = cmx.hairDict.Keys.ToList();
            hairList.Sort();
            foreach (var key in hairList)
            {
                AddHAIRBytes(cmx.hairDict[key], mode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //NIFL COL
            int colAddress = outBytes.Count;
            var colList = cmx.colDict.Keys.ToList();
            colList.Sort();
            foreach (var key in colList)
            {
                AddNIFLCOLBytes(cmx.colDict[key], mode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //Unk
            int unkAddress = outBytes.Count;
            var unkList = cmx.unkList;
            foreach (var unkArr in unkList)
            {
                AddUnkIntFieldBytes(unkArr, mode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }
            //Outer
            int bodyOuterAddress = outBytes.Count;
            var outerList = cmx.outerDict.Keys.ToList();
            outerList.Sort();
            foreach (var key in outerList)
            {
                AddBodyBytes(cmx.outerDict[key], mode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //Basewear
            int baseWearAddress = outBytes.Count;
            var baseList = cmx.baseWearDict.Keys.ToList();
            baseList.Sort();
            foreach (var key in baseList)
            {
                AddBodyBytes(cmx.baseWearDict[key], mode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //BBLY
            //Inner wear 
            int innerWearAddress = outBytes.Count;
            var innerList = cmx.innerWearDict.Keys.ToList();
            innerList.Sort();
            foreach (var key in innerList)
            {
                AddBDPBytes(cmx.innerWearDict[key], mode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //Index Links
            //Costume/Body
            int bodyLinkAddress = outBytes.Count;
            var bodyLinkList = cmx.costumeIdLink.Keys.ToList();
            bodyLinkList.Sort();
            foreach (var key in bodyLinkList)
            {
                AddBCLNBytes(cmx.costumeIdLink[key], mode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }
            //Cast Leg
            int clegLinkAddress = outBytes.Count;
            var clegLinkList = cmx.clegIdLink.Keys.ToList();
            clegLinkList.Sort();
            foreach (var key in clegLinkList)
            {
                AddBCLNBytes(cmx.clegIdLink[key], mode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }
            //Cast Arm
            int carmLinkAddress = outBytes.Count;
            var carmLinkList = cmx.castArmIdLink.Keys.ToList();
            carmLinkList.Sort();
            foreach (var key in carmLinkList)
            {
                AddBCLNBytes(cmx.castArmIdLink[key], mode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            int castHeadLinkAddress = -1;
            if (mode >= 1)
            {
                //Cast Head
                castHeadLinkAddress = outBytes.Count;
                var cHeadLinkList = cmx.castHeadIdLink.Keys.ToList();
                cHeadLinkList.Sort();
                foreach (var key in cHeadLinkList)
                {
                    AddBCLNBytes(cmx.castHeadIdLink[key], mode, outBytes, nof0PointerLocations, textAddressDict, textList);
                }
            }
            //Outer
            int outerLinkAddress = outBytes.Count;
            var outerLinkList = cmx.outerWearIdLink.Keys.ToList();
            outerLinkList.Sort();
            foreach (var key in outerLinkList)
            {
                AddBCLNBytes(cmx.outerWearIdLink[key], mode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }
            //Base
            int baseLinkAddress = outBytes.Count;
            var baseLinkList = cmx.baseWearIdLink.Keys.ToList();
            baseLinkList.Sort();
            foreach (var key in baseLinkList)
            {
                AddBCLNBytes(cmx.baseWearIdLink[key], mode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }
            //Inner
            int innerLinkAddress = outBytes.Count;
            var innerLinkList = cmx.innerWearIdLink.Keys.ToList();
            innerLinkList.Sort();
            foreach (var key in innerLinkList)
            {
                AddBCLNBytes(cmx.innerWearIdLink[key], mode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            int accessoryIdLinkAddress = -1;
            if (mode >= 1)
            {
                //Accessory
                accessoryIdLinkAddress = outBytes.Count;
                var acceLinkList = cmx.accessoryIdLink.Keys.ToList();
                acceLinkList.Sort();
                foreach (var key in acceLinkList)
                {
                    AddBCLNBytes(cmx.accessoryIdLink[key], mode, outBytes, nof0PointerLocations, textAddressDict, textList);
                }
            }
            //Write header data
            AquaGeneralMethods.SetByteListInt(outBytes, rel0SizeOffset + 4, outBytes.Count);
            //CMX Table
            //Addresses
            AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(bodyAddress));
            AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(carmAddress));
            AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(clegAddress));
            AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(bodyOuterAddress));
            AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(baseWearAddress));
            AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(innerWearAddress));
            AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(bodyPaintAddress));
            AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(stickerAddress));
            AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(faceAddress));
            AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(fcmnAddress));
            AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(ftexAddress));
            AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(fcpAddress));
            AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(acceAddress));
            AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(eyeAddress));
            AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(earAddress));
            AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(teethAddress));
            AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(hornAddress));
            AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(skinAddress));
            AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(eyeBAddress));
            AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(eyeLAddress));
            AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(hairAddress));
            AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(colAddress));
            AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(unkAddress));

            //ID Links
            AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(bodyLinkAddress));
            AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(carmLinkAddress));
            AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(clegLinkAddress));

            if (mode >= 1)
            {
                AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
                outBytes.AddRange(BitConverter.GetBytes(castHeadLinkAddress));
            }
            AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(outerLinkAddress));
            AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(baseLinkAddress));
            AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(innerLinkAddress));

            if (mode >= 1)
            {
                AquaGeneralMethods.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
                outBytes.AddRange(BitConverter.GetBytes(accessoryIdLinkAddress));
            }

            //Counts
            outBytes.AddRange(BitConverter.GetBytes(cmx.costumeDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(cmx.carmDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(cmx.clegDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(cmx.outerDict.Count));

            outBytes.AddRange(BitConverter.GetBytes(cmx.baseWearDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(cmx.innerWearDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(cmx.bodyPaintDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(cmx.stickerDict.Count));

            outBytes.AddRange(BitConverter.GetBytes(cmx.faceDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(cmx.fcmnDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(cmx.faceTextureDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(cmx.fcpDict.Count));

            outBytes.AddRange(BitConverter.GetBytes(cmx.accessoryDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(cmx.eyeDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(cmx.ngsEarDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(cmx.ngsTeethDict.Count));

            outBytes.AddRange(BitConverter.GetBytes(cmx.ngsHornDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(cmx.ngsSkinDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(cmx.eyebrowDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(cmx.eyelashDict.Count));

            outBytes.AddRange(BitConverter.GetBytes(cmx.hairDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(cmx.colDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(cmx.unkList.Count));

            outBytes.AddRange(BitConverter.GetBytes(cmx.costumeIdLink.Count));
            outBytes.AddRange(BitConverter.GetBytes(cmx.castArmIdLink.Count));
            outBytes.AddRange(BitConverter.GetBytes(cmx.clegIdLink.Count));

            if (mode >= 1)
            {
                outBytes.AddRange(BitConverter.GetBytes(cmx.castHeadIdLink.Count));
                outBytes.AddRange(BitConverter.GetBytes(cmx.outerWearIdLink.Count));
                outBytes.AddRange(BitConverter.GetBytes(cmx.baseWearIdLink.Count));
                outBytes.AddRange(BitConverter.GetBytes(cmx.innerWearIdLink.Count));
                outBytes.AddRange(BitConverter.GetBytes(cmx.accessoryIdLink.Count));
            }
            else
            {
                outBytes.AddRange(BitConverter.GetBytes(cmx.outerWearIdLink.Count));
                outBytes.AddRange(BitConverter.GetBytes(cmx.baseWearIdLink.Count));
                outBytes.AddRange(BitConverter.GetBytes(cmx.innerWearIdLink.Count));
            }

            //Write text
            for (int i = 0; i < textList.Count; i++)
            {
                var offsetList = textAddressDict[textList[i]];
                for (int j = 0; j < offsetList.Count; j++)
                {
                    AquaGeneralMethods.SetByteListInt(outBytes, offsetList[j], outBytes.Count);
                }
                outBytes.AddRange(Encoding.UTF8.GetBytes(textList[i]));
                var count = outBytes.Count;
                AquaGeneralMethods.AlignWriter(outBytes, 0x4);
                if (count == outBytes.Count)
                {
                    outBytes.AddRange(new byte[4]);
                }
            }

            AquaGeneralMethods.AlignWriter(outBytes, 0x10);

            //Write REL0 Size
            AquaGeneralMethods.SetByteListInt(outBytes, rel0SizeOffset, outBytes.Count - 0x8);

            //Write NOF0
            int NOF0Offset = outBytes.Count;
            int NOF0Size = (nof0PointerLocations.Count + 2) * 4;
            int NOF0FullSize = NOF0Size + 0x8;
            outBytes.AddRange(Encoding.UTF8.GetBytes("NOF0"));
            outBytes.AddRange(BitConverter.GetBytes(NOF0Size));
            outBytes.AddRange(BitConverter.GetBytes(nof0PointerLocations.Count));
            outBytes.AddRange(BitConverter.GetBytes(0x10));

            //Write pointer offsets
            for (int i = 0; i < nof0PointerLocations.Count; i++)
            {
                outBytes.AddRange(BitConverter.GetBytes(nof0PointerLocations[i]));
            }
            NOF0FullSize += AquaGeneralMethods.AlignWriter(outBytes, 0x10);

            //NEND
            outBytes.AddRange(Encoding.UTF8.GetBytes("NEND"));
            outBytes.AddRange(BitConverter.GetBytes(0x8));
            outBytes.AddRange(BitConverter.GetBytes(0));
            outBytes.AddRange(BitConverter.GetBytes(0));

            //Generate NIFL
            AquaCommon.NIFL nifl = new AquaCommon.NIFL();
            nifl.magic = BitConverter.ToInt32(Encoding.UTF8.GetBytes("NIFL"), 0);
            nifl.NIFLLength = 0x18;
            nifl.unkInt0 = 1;
            nifl.offsetAddition = 0x20;

            nifl.NOF0Offset = NOF0Offset;
            nifl.NOF0OffsetFull = NOF0Offset + 0x20;
            nifl.NOF0BlockSize = NOF0FullSize;
            nifl.padding0 = 0;

            //Write NIFL
            outBytes.InsertRange(0, AquaGeneralMethods.ConvertStruct(nifl));

            return outBytes.ToArray();
        }

        private static void AddNIFLText(int address, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList, string str)
        {
            AquaGeneralMethods.NOF0Append(nof0PointerLocations, address, 1);
            AddOntoDict(textAddressDict, textList, str, address);
        }

        private static void AddBCLNBytes(BCLNObject bcln, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            outBytes.AddRange(AquaGeneralMethods.ConvertStruct(bcln.bcln));
            if (mode >= 1)
            {
                outBytes.AddRange(AquaGeneralMethods.ConvertStruct(bcln.bclnRitem));
                outBytes.AddRange(AquaGeneralMethods.ConvertStruct(bcln.bclnRitem2));
            }
        }

        private static void AddUnkIntFieldBytes(Unk_IntField unkIntField, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            outBytes.AddRange(AquaGeneralMethods.ConvertStruct(unkIntField));
        }

        private static void AddNIFLCOLBytes(NIFL_COLObject col, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, col.textString);

            outBytes.AddRange(AquaGeneralMethods.ConvertStruct(col.niflCol));
        }

        private static void AddHAIRBytes(HAIRObject hair, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, hair.dataString);
            AddNIFLText(outBytes.Count + 0x8, nof0PointerLocations, textAddressDict, textList, hair.texString1);
            AddNIFLText(outBytes.Count + 0xC, nof0PointerLocations, textAddressDict, textList, hair.texString2);
            AddNIFLText(outBytes.Count + 0x10, nof0PointerLocations, textAddressDict, textList, hair.texString3);
            AddNIFLText(outBytes.Count + 0x14, nof0PointerLocations, textAddressDict, textList, hair.texString4);
            AddNIFLText(outBytes.Count + 0x18, nof0PointerLocations, textAddressDict, textList, hair.texString5);
            AddNIFLText(outBytes.Count + 0x1C, nof0PointerLocations, textAddressDict, textList, hair.texString6);
            AddNIFLText(outBytes.Count + 0x20, nof0PointerLocations, textAddressDict, textList, hair.texString7);

            outBytes.AddRange(AquaGeneralMethods.ConvertStruct(hair.hair));
        }

        private static void AddEYEBBytes(EYEBObject eyeB, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, eyeB.texString1);
            AddNIFLText(outBytes.Count + 0x8, nof0PointerLocations, textAddressDict, textList, eyeB.texString2);
            AddNIFLText(outBytes.Count + 0xC, nof0PointerLocations, textAddressDict, textList, eyeB.texString3);
            AddNIFLText(outBytes.Count + 0x10, nof0PointerLocations, textAddressDict, textList, eyeB.texString4);

            outBytes.AddRange(AquaGeneralMethods.ConvertStruct(eyeB.eyeb));
        }

        private static void AddSkinBytes(NGS_SKINObject skin, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, skin.texString1);
            AddNIFLText(outBytes.Count + 0x8, nof0PointerLocations, textAddressDict, textList, skin.texString2);
            AddNIFLText(outBytes.Count + 0xC, nof0PointerLocations, textAddressDict, textList, skin.texString3);
            AddNIFLText(outBytes.Count + 0x10, nof0PointerLocations, textAddressDict, textList, skin.texString4);
            AddNIFLText(outBytes.Count + 0x14, nof0PointerLocations, textAddressDict, textList, skin.texString5);
            AddNIFLText(outBytes.Count + 0x18, nof0PointerLocations, textAddressDict, textList, skin.texString6);
            AddNIFLText(outBytes.Count + 0x1C, nof0PointerLocations, textAddressDict, textList, skin.texString7);
            AddNIFLText(outBytes.Count + 0x20, nof0PointerLocations, textAddressDict, textList, skin.texString8);
            AddNIFLText(outBytes.Count + 0x24, nof0PointerLocations, textAddressDict, textList, skin.texString9);
            AddNIFLText(outBytes.Count + 0x28, nof0PointerLocations, textAddressDict, textList, skin.texString10);

            outBytes.AddRange(AquaGeneralMethods.ConvertStruct(skin.ngsSkin));
        }

        private static void AddHornBytes(NGS_HornObject horn, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, horn.dataString);

            outBytes.AddRange(AquaGeneralMethods.ConvertStruct(horn.ngsHorn));
        }

        private static void AddTeethBytes(NGS_TeethObject teeth, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, teeth.dataString);
            AddNIFLText(outBytes.Count + 0x8, nof0PointerLocations, textAddressDict, textList, teeth.texString1);
            AddNIFLText(outBytes.Count + 0xC, nof0PointerLocations, textAddressDict, textList, teeth.texString2);
            AddNIFLText(outBytes.Count + 0x10, nof0PointerLocations, textAddressDict, textList, teeth.texString3);
            AddNIFLText(outBytes.Count + 0x14, nof0PointerLocations, textAddressDict, textList, teeth.texString4);

            outBytes.AddRange(AquaGeneralMethods.ConvertStruct(teeth.ngsTeeth));
        }

        private static void AddEARBytes(NGS_EarObject ear, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, ear.dataString);
            AddNIFLText(outBytes.Count + 0x8, nof0PointerLocations, textAddressDict, textList, ear.texString1);
            AddNIFLText(outBytes.Count + 0xC, nof0PointerLocations, textAddressDict, textList, ear.texString2);
            AddNIFLText(outBytes.Count + 0x10, nof0PointerLocations, textAddressDict, textList, ear.texString3);
            AddNIFLText(outBytes.Count + 0x14, nof0PointerLocations, textAddressDict, textList, ear.texString4);
            AddNIFLText(outBytes.Count + 0x18, nof0PointerLocations, textAddressDict, textList, ear.texString5);

            outBytes.AddRange(AquaGeneralMethods.ConvertStruct(ear.ngsEar));
        }

        private static void AddEYEBytes(EYEObject eye, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, eye.texString1);
            AddNIFLText(outBytes.Count + 0x8, nof0PointerLocations, textAddressDict, textList, eye.texString2);
            AddNIFLText(outBytes.Count + 0xC, nof0PointerLocations, textAddressDict, textList, eye.texString3);
            AddNIFLText(outBytes.Count + 0x10, nof0PointerLocations, textAddressDict, textList, eye.texString4);
            AddNIFLText(outBytes.Count + 0x14, nof0PointerLocations, textAddressDict, textList, eye.texString5);

            outBytes.AddRange(AquaGeneralMethods.ConvertStruct(eye.eye));
        }

        private static void AddACCEBytes(ACCEObject acce, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, acce.dataString);
            AddNIFLText(outBytes.Count + 0x8, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach1);
            AddNIFLText(outBytes.Count + 0xC, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach2);
            AddNIFLText(outBytes.Count + 0x10, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach3);
            AddNIFLText(outBytes.Count + 0x14, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach4);
            if(mode >= 1)
            {
                AddNIFLText(outBytes.Count + 0x18, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach5);
                AddNIFLText(outBytes.Count + 0x1C, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach6);
                AddNIFLText(outBytes.Count + 0x20, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach7);
                AddNIFLText(outBytes.Count + 0x24, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach8);
            } else 
            {
                //Reordering happened at some point
                AddNIFLText(outBytes.Count + 0x18, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach6);
                AddNIFLText(outBytes.Count + 0x1C, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach7);
                AddNIFLText(outBytes.Count + 0x20, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach8);
                AddNIFLText(outBytes.Count + 0x24, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach9);
            }

            outBytes.AddRange(AquaGeneralMethods.ConvertStruct(acce.acce));
            if (mode >= 1)
            {
                AddNIFLText(outBytes.Count + 0x0, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach9);
                AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach10);

                AddNIFLText(outBytes.Count + 0x8, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach11);
                AddNIFLText(outBytes.Count + 0xC, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach12);
                AddNIFLText(outBytes.Count + 0x10, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach13);
                AddNIFLText(outBytes.Count + 0x14, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach14);

                AddNIFLText(outBytes.Count + 0x18, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach15);
                outBytes.AddRange(AquaGeneralMethods.ConvertStruct(acce.acceFeb8_22));
            }
            outBytes.AddRange(AquaGeneralMethods.ConvertStruct(acce.acceB));
            outBytes.AddRange(AquaGeneralMethods.ConvertStruct(acce.acce2a));
            if (mode >= 1)
            {
                outBytes.AddRange(BitConverter.GetBytes(acce.flt_54));
            }
            outBytes.AddRange(AquaGeneralMethods.ConvertStruct(acce.acce2b));
            if (mode >= 1)
            {
                for (int i = 0; i < 3; i++)
                {
                    var acce12 = acce.acce12List[i];

                    outBytes.AddRange(BitConverter.GetBytes(acce12.unkFloat0));
                    outBytes.AddRange(BitConverter.GetBytes(acce12.unkFloat1));
                    outBytes.AddRange(BitConverter.GetBytes(acce12.unkInt0));
                    outBytes.AddRange(BitConverter.GetBytes(acce12.unkInt1));

                    outBytes.AddRange(BitConverter.GetBytes(acce12.unkIntFeb822_0));

                    outBytes.AddRange(BitConverter.GetBytes(acce12.unkShort0));
                    outBytes.AddRange(BitConverter.GetBytes(acce12.unkShort1));
                    outBytes.AddRange(BitConverter.GetBytes(acce12.unkShort2));
                    outBytes.AddRange(BitConverter.GetBytes(acce12.unkShort3));

                    outBytes.AddRange(BitConverter.GetBytes(acce12.unkIntFeb822_1));
                }
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    var acce12 = acce.acce12List[i];

                    outBytes.AddRange(BitConverter.GetBytes(acce12.unkFloat0));
                    outBytes.AddRange(BitConverter.GetBytes(acce12.unkFloat1));
                    outBytes.AddRange(BitConverter.GetBytes(acce12.unkInt0));
                    outBytes.AddRange(BitConverter.GetBytes(acce12.unkInt1));

                    outBytes.AddRange(BitConverter.GetBytes(acce12.unkShort0));
                    outBytes.AddRange(BitConverter.GetBytes(acce12.unkShort1));
                    outBytes.AddRange(BitConverter.GetBytes(acce12.unkShort2));
                    outBytes.AddRange(BitConverter.GetBytes(acce12.unkShort3));
                }
            }
            if(mode >= 1)
            {
                outBytes.AddRange(BitConverter.GetBytes(acce.flt_90));
            }
        }

        private static void AddFCPBytes(FCPObject face, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, face.texString1);
            AddNIFLText(outBytes.Count + 0x8, nof0PointerLocations, textAddressDict, textList, face.texString2);
            AddNIFLText(outBytes.Count + 0xC, nof0PointerLocations, textAddressDict, textList, face.texString3);
            AddNIFLText(outBytes.Count + 0x10, nof0PointerLocations, textAddressDict, textList, face.texString4);

            outBytes.AddRange(AquaGeneralMethods.ConvertStruct(face.fcp));
        }

        private static void AddFaceTexturesBytes(FaceTextureObject face, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, face.texString1);
            AddNIFLText(outBytes.Count + 0x8, nof0PointerLocations, textAddressDict, textList, face.texString2);
            AddNIFLText(outBytes.Count + 0xC, nof0PointerLocations, textAddressDict, textList, face.texString3);
            AddNIFLText(outBytes.Count + 0x10, nof0PointerLocations, textAddressDict, textList, face.texString4);

            outBytes.AddRange(AquaGeneralMethods.ConvertStruct(face.ngsFace));
        }

        private static void AddFCMNBytes(FCMNObject face, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, face.proportionAnim);
            AddNIFLText(outBytes.Count + 0x8, nof0PointerLocations, textAddressDict, textList, face.faceAnim1);
            AddNIFLText(outBytes.Count + 0xC, nof0PointerLocations, textAddressDict, textList, face.faceAnim2);
            AddNIFLText(outBytes.Count + 0x10, nof0PointerLocations, textAddressDict, textList, face.faceAnim3);
            AddNIFLText(outBytes.Count + 0x14, nof0PointerLocations, textAddressDict, textList, face.faceAnim4);
            AddNIFLText(outBytes.Count + 0x18, nof0PointerLocations, textAddressDict, textList, face.faceAnim5);
            AddNIFLText(outBytes.Count + 0x1C, nof0PointerLocations, textAddressDict, textList, face.faceAnim6);
            AddNIFLText(outBytes.Count + 0x20, nof0PointerLocations, textAddressDict, textList, face.faceAnim7);
            AddNIFLText(outBytes.Count + 0x24, nof0PointerLocations, textAddressDict, textList, face.faceAnim8);
            AddNIFLText(outBytes.Count + 0x28, nof0PointerLocations, textAddressDict, textList, face.faceAnim9);
            AddNIFLText(outBytes.Count + 0x2C, nof0PointerLocations, textAddressDict, textList, face.faceAnim10);

            outBytes.AddRange(AquaGeneralMethods.ConvertStruct(face.fcmn));
        }

        private static void AddFACEBytes(FACEObject face, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, face.dataString);
            AddNIFLText(outBytes.Count + 0x8, nof0PointerLocations, textAddressDict, textList, face.texString1);
            AddNIFLText(outBytes.Count + 0xC, nof0PointerLocations, textAddressDict, textList, face.texString2);
            AddNIFLText(outBytes.Count + 0x10, nof0PointerLocations, textAddressDict, textList, face.texString3);
            AddNIFLText(outBytes.Count + 0x14, nof0PointerLocations, textAddressDict, textList, face.texString4);
            AddNIFLText(outBytes.Count + 0x18, nof0PointerLocations, textAddressDict, textList, face.texString5);
            AddNIFLText(outBytes.Count + 0x1C, nof0PointerLocations, textAddressDict, textList, face.texString6);

            outBytes.AddRange(AquaGeneralMethods.ConvertStruct(face.face));
            if (mode >= 1)
            {
                outBytes.AddRange(AquaGeneralMethods.ConvertStruct(face.faceRitem));
            }
            var face2Temp = face.face2;

            //Backwards compatibility
            if(mode == 0)
            {
                face2Temp.unkInt0 = face.faceRitem.unkIntRT0;
                face2Temp.unkInt1 = face.faceRitem.unkIntRT1 + (face.face2.unkInt0 * 0x10000); 
            }
            outBytes.AddRange(AquaGeneralMethods.ConvertStruct(face2Temp));
            if (mode >= 1)
            {
                outBytes.AddRange(BitConverter.GetBytes(face.unkFloatRitem));
            }
        }

        private static void AddStickerBytes(StickerObject sticker, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, sticker.texString);
            outBytes.AddRange(AquaGeneralMethods.ConvertStruct(sticker.sticker));
        }

        private static void AddBDPBytes(BBLYObject bbly, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, bbly.texString1);
            AddNIFLText(outBytes.Count + 0x8, nof0PointerLocations, textAddressDict, textList, bbly.texString2);
            AddNIFLText(outBytes.Count + 0xC, nof0PointerLocations, textAddressDict, textList, bbly.texString3);
            AddNIFLText(outBytes.Count + 0x10, nof0PointerLocations, textAddressDict, textList, bbly.texString4);
            AddNIFLText(outBytes.Count + 0x14, nof0PointerLocations, textAddressDict, textList, bbly.texString5);
            outBytes.AddRange(AquaGeneralMethods.ConvertStruct(bbly.bbly));
        }

        private static void AddBodyBytes(BODYObject body, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, body.dataString);
            AddNIFLText(outBytes.Count + 0x8, nof0PointerLocations, textAddressDict, textList, body.texString1);
            AddNIFLText(outBytes.Count + 0xC, nof0PointerLocations, textAddressDict, textList, body.texString2);
            AddNIFLText(outBytes.Count + 0x10, nof0PointerLocations, textAddressDict, textList, body.texString3);
            AddNIFLText(outBytes.Count + 0x14, nof0PointerLocations, textAddressDict, textList, body.texString4);
            AddNIFLText(outBytes.Count + 0x18, nof0PointerLocations, textAddressDict, textList, body.texString5);
            AddNIFLText(outBytes.Count + 0x1C, nof0PointerLocations, textAddressDict, textList, body.texString6);
            outBytes.AddRange(AquaGeneralMethods.ConvertStruct(body.body));
            if (mode >= 1)
            {
                outBytes.AddRange(AquaGeneralMethods.ConvertStruct(body.bodyRitem));
            }
            outBytes.AddRange(AquaGeneralMethods.ConvertStruct(body.body2));
            if (mode >= 1)
            {
                outBytes.AddRange(AquaGeneralMethods.ConvertStruct(body.body40cap));
            }
        }

        //Collects pointers locations relative each struct 
        private static void CalcCMXPointers(CMXTable table, AquaCommon.NOF0 nof0, int rel0DataStart, CharacterMakingIndex cmx)
        {
            long position = 0;
            int count = 0;
            int start = 0;
            List<int> BODYPtrs = new List<int>();
            List<int> BODYRitemPtrs = new List<int>();
            List<int> BODY2Ptrs = new List<int>();
            List<int> BODY40CapPtrs = new List<int>();

            List<int> BBLYPtrs = new List<int>();

            List<int> StickerPtrs = new List<int>();

            List<int> FacePtrs = new List<int>();
            List<int> FaceRitemPtrs = new List<int>();
            List<int> Face2Ptrs = new List<int>();

            List<int> FcmnPtrs = new List<int>();

            List<int> FaceTexPtrs = new List<int>();

            List<int> FcpPtrs = new List<int>();

            List<int> AccePtrs = new List<int>();
            List<int> AcceBPtrs = new List<int>();
            List<int> AcceFeb822Ptrs = new List<int>();
            List<int> Acce2Ptrs = new List<int>();
            List<int> Acce12Ptrs = new List<int>();

            List<int> EyePtrs = new List<int>();

            List<int> EarPtrs = new List<int>();

            List<int> TeethPtr = new List<int>();

            List<int> HornPtr = new List<int>();

            List<int> SkinPtr = new List<int>();

            List<int> EyeBPtr = new List<int>(); //For EYEL too

            List<int> HairPtr = new List<int>();

            List<int> NIFL_COLPtr = new List<int>();

            List<int> Unk_IntFieldPtr = new List<int>();

            List<int> BCLNPtr = new List<int>();
            List<int> BCLNRitemPtr = new List<int>();
            List<int> BCLNRitem2Ptr = new List<int>();

            for (int b = 0; b < 5; b++)
            {
                switch (b)
                {
                    case 0:
                        position = table.bodyAddress;
                        count = table.bodyCount;
                        break;
                    case 1:
                        position = table.carmAddress;
                        count = table.carmCount;
                        break;
                    case 2:
                        position = table.clegAddress;
                        count = table.clegCount;
                        break;
                    case 3:
                        position = table.bodyOuterAddress;
                        count = table.bodyOuterCount;
                        break;
                    case 4:
                        position = table.baseWearAddress;
                        count = table.baseWearCount;
                        break;
                }
                start = getPointrStart(nof0.relAddresses, position);
                for (int i = 0; i < count; i++)
                {
                    pointerCheck(position, nof0.relAddresses, sizeof(BODY), BODYPtrs, start);
                    position += sizeof(BODY);
                    if (rel0DataStart >= dec14_21TableAddressInt)
                    {
                        pointerCheck(position, nof0.relAddresses, sizeof(BODYRitem), BODYRitemPtrs, start);
                        position += sizeof(BODYRitem);
                    }
                    pointerCheck(position, nof0.relAddresses, sizeof(BODY2), BODY2Ptrs, start);
                    position += sizeof(BODY2);
                    if (rel0DataStart >= feb8_22TableAddressInt)
                    {
                        pointerCheck(position, nof0.relAddresses, sizeof(BODY40Cap), BODY40CapPtrs, start);
                        position += sizeof(BODY40Cap);
                    }
                }
            }
            BODYPtrs.Sort();
            BODYRitemPtrs.Sort();
            BODY2Ptrs.Sort();
            BODY40CapPtrs.Sort();

            for (int b = 0; b < 2; b++)
            {
                switch (b)
                {
                    case 0:
                        position = table.innerWearAddress;
                        count = table.innerWearCount;
                        break;
                    case 1:
                        position = table.bodyPaintAddress;
                        count = table.bodyPaintCount;
                        break;
                }
                start = getPointrStart(nof0.relAddresses, position);
                for (int i = 0; i < count; i++)
                {
                    pointerCheck(position, nof0.relAddresses, sizeof(BBLY), BBLYPtrs, start);
                    position += sizeof(BBLY);
                }
            }
            BBLYPtrs.Sort();

            position = table.stickerAddress;
            count = table.stickerCount;
            start = getPointrStart(nof0.relAddresses, position);
            for (int i = 0; i < count; i++)
            {
                pointerCheck(position, nof0.relAddresses, sizeof(Sticker), StickerPtrs, start);
                position += sizeof(Sticker);
            }
            StickerPtrs.Sort();

            position = table.faceAddress;
            count = table.faceCount;
            start = getPointrStart(nof0.relAddresses, position);
            for (int i = 0; i < count; i++)
            {
                pointerCheck(position, nof0.relAddresses, sizeof(FACE), FacePtrs, start);
                position += sizeof(FACE);

                if (rel0DataStart >= dec14_21TableAddressInt)
                {
                    pointerCheck(position, nof0.relAddresses, sizeof(FACERitem), FaceRitemPtrs, start);
                    position += sizeof(FACERitem);
                }
                pointerCheck(position, nof0.relAddresses, sizeof(FACE2), Face2Ptrs, start);
                position += sizeof(FACE2);

                if (rel0DataStart >= dec14_21TableAddressInt)
                {
                    position += sizeof(float);
                }
            }
            FacePtrs.Sort();

            position = table.faceMotionAddress;
            count = table.faceMotionCount;
            start = getPointrStart(nof0.relAddresses, position);
            for (int i = 0; i < count; i++)
            {
                pointerCheck(position, nof0.relAddresses, sizeof(FCMN), FcmnPtrs, start);
                position += sizeof(FCMN);
            }
            FcmnPtrs.Sort();

            position = table.faceTextureAddress;
            count = table.faceTextureCount;
            start = getPointrStart(nof0.relAddresses, position);
            for (int i = 0; i < count; i++)
            {
                pointerCheck(position, nof0.relAddresses, sizeof(FaceTextures), FaceTexPtrs, start);
                position += sizeof(FaceTextures);
            }
            FaceTexPtrs.Sort();

            position = table.faceTexturesAddress;
            count = table.faceTexturesCount;
            start = getPointrStart(nof0.relAddresses, position);
            for (int i = 0; i < count; i++)
            {
                pointerCheck(position, nof0.relAddresses, sizeof(FCP), FcpPtrs, start);
                position += sizeof(FCP);
            }
            FcpPtrs.Sort();

            position = table.accessoryAddress;
            count = table.accessoryCount;
            start = getPointrStart(nof0.relAddresses, position);
            for (int i = 0; i < count; i++)
            {
                pointerCheck(position, nof0.relAddresses, sizeof(ACCE), AccePtrs, start);
                position += sizeof(ACCE);
                if (rel0DataStart >= feb8_22TableAddressInt)
                {
                    pointerCheck(position, nof0.relAddresses, sizeof(ACCE_Feb8_22), AcceFeb822Ptrs, start);
                    position += sizeof(ACCE_Feb8_22);
                }
                pointerCheck(position, nof0.relAddresses, sizeof(ACCE_B), AcceBPtrs, start);
                position += sizeof(ACCE_B);
                pointerCheck(position, nof0.relAddresses, sizeof(ACCE2A), Acce2Ptrs, start);
                position += sizeof(ACCE2A);
                if (rel0DataStart >= oct21TableAddressInt)
                {
                    position += sizeof(int);
                }
                pointerCheck(position, nof0.relAddresses, sizeof(ACCE2B), Acce2Ptrs, start);
                position += sizeof(ACCE2B);
                if (rel0DataStart >= feb8_22TableAddressInt)
                {
                    pointerCheck(position, nof0.relAddresses, 0x60, Acce12Ptrs, start, true);
                    position += 0x60;
                }
                else
                {
                    pointerCheck(position, nof0.relAddresses, 0x48, Acce12Ptrs, start);
                    position += 0x48;
                }
            }
            AccePtrs.Sort();
            AcceBPtrs.Sort();
            AcceFeb822Ptrs.Sort();
            Acce2Ptrs.Sort();
            Acce12Ptrs.Sort();

            position = table.eyeTextureAddress;
            count = table.eyeTextureCount;
            start = getPointrStart(nof0.relAddresses, position);
            for (int i = 0; i < count; i++)
            {
                pointerCheck(position, nof0.relAddresses, sizeof(EYE), EyePtrs, start);
                position += sizeof(EYE);
            }
            EyePtrs.Sort();

            for (int b = 0; b < 2; b++)
            {
                switch (b)
                {
                    case 0:
                        position = table.eyebrowAddress;
                        count = table.eyebrowCount;
                        break;
                    case 1:
                        position = table.eyelashAddress;
                        count = table.eyelashCount;
                        break;
                }
                start = getPointrStart(nof0.relAddresses, position);
                for (int i = 0; i < count; i++)
                {
                    pointerCheck(position, nof0.relAddresses, sizeof(EYEB), EyeBPtr, start);
                    position += sizeof(EYEB);
                }
            }
            EyeBPtr.Sort();

            position = table.earAddress;
            count = table.earCount;
            start = getPointrStart(nof0.relAddresses, position);
            for (int i = 0; i < count; i++)
            {
                pointerCheck(position, nof0.relAddresses, sizeof(NGS_Ear), EarPtrs, start);
                position += sizeof(NGS_Ear);
            }
            EarPtrs.Sort();

            position = table.hornAddress;
            count = table.hornCount;
            start = getPointrStart(nof0.relAddresses, position);
            for (int i = 0; i < count; i++)
            {
                pointerCheck(position, nof0.relAddresses, sizeof(NGS_Horn), HornPtr, start);
                position += sizeof(NGS_Horn);
            }
            HornPtr.Sort();

            position = table.teethAddress;
            count = table.teethCount;
            start = getPointrStart(nof0.relAddresses, position);
            for (int i = 0; i < count; i++)
            {
                pointerCheck(position, nof0.relAddresses, sizeof(NGS_Teeth), TeethPtr, start);
                position += sizeof(NGS_Teeth);
            }
            TeethPtr.Sort();

            position = table.skinAddress;
            count = table.skinCount;
            start = getPointrStart(nof0.relAddresses, position);
            for (int i = 0; i < count; i++)
            {
                pointerCheck(position, nof0.relAddresses, sizeof(NGS_Skin), SkinPtr, start);
                position += sizeof(NGS_Skin);
            }
            SkinPtr.Sort();

            position = table.hairAddress;
            count = table.hairCount;
            start = getPointrStart(nof0.relAddresses, position);
            for (int i = 0; i < count; i++)
            {
                pointerCheck(position, nof0.relAddresses, sizeof(HAIR), HairPtr, start);
                position += sizeof(HAIR);
            }
            HairPtr.Sort();

            position = table.colAddress;
            count = table.colCount;
            start = getPointrStart(nof0.relAddresses, position);
            for (int i = 0; i < count; i++)
            {
                pointerCheck(position, nof0.relAddresses, sizeof(NIFL_COL), NIFL_COLPtr, start);
                position += sizeof(NIFL_COL);
            }
            NIFL_COLPtr.Sort();

            position = table.unkAddress;
            count = table.unkCount;
            start = getPointrStart(nof0.relAddresses, position);
            for (int i = 0; i < count; i++)
            {
                pointerCheck(position, nof0.relAddresses, sizeof(Unk_IntField), Unk_IntFieldPtr, start);
                position += sizeof(Unk_IntField);
            }
            Unk_IntFieldPtr.Sort();

            if (table.oct21UnkCount > 0)
            {
                position = table.oct21UnkAddress;
                count = table.oct21UnkCount;
                start = getPointrStart(nof0.relAddresses, position);
                for (int i = 0; i < count; i++)
                {
                    pointerCheck(position, nof0.relAddresses, sizeof(BCLN), BCLNPtr, start);
                    position += sizeof(BCLN);
                    if (rel0DataStart >= dec14_21TableAddressInt)
                    {
                        pointerCheck(position, nof0.relAddresses, sizeof(BCLNRitem), BCLNRitemPtr, start);
                        position += sizeof(BCLNRitem);
                        if (rel0DataStart >= feb8_22TableAddressInt)
                        {
                            pointerCheck(position, nof0.relAddresses, sizeof(BCLNRitem2), BCLNRitem2Ptr, start);
                            position += sizeof(BCLNRitem2);
                        }
                    }
                }
            }

            if (table.feb8_22UnkCount > 0)
            {
                position = table.feb8_22UnkAddress;
                count = table.feb8_22UnkCount;
                start = getPointrStart(nof0.relAddresses, position);
                for (int i = 0; i < count; i++)
                {
                    pointerCheck(position, nof0.relAddresses, sizeof(BCLN), BCLNPtr, start);
                    position += sizeof(BCLN);
                    if (rel0DataStart >= dec14_21TableAddressInt)
                    {
                        pointerCheck(position, nof0.relAddresses, sizeof(BCLNRitem), BCLNRitemPtr, start);
                        position += sizeof(BCLNRitem);
                        if (rel0DataStart >= feb8_22TableAddressInt)
                        {
                            pointerCheck(position, nof0.relAddresses, sizeof(BCLNRitem2), BCLNRitem2Ptr, start);
                            position += sizeof(BCLNRitem2);
                        }
                    }
                }
            }

            for (int b = 0; b < 6; b++)
            {
                switch (b)
                {
                    case 0:
                        position = table.costumeIdLinkAddress;
                        count = table.costumeIdLinkCount;
                        break;
                    case 1:
                        position = table.castArmIdLinkAddress;
                        count = table.castArmIdLinkCount;
                        break;
                    case 2:
                        position = table.castLegIdLinkAddress;
                        count = table.castLegIdLinkCount;
                        break;
                    case 3:
                        position = table.outerIdLinkAddress;
                        count = table.outerIdLinkCount;
                        break;
                    case 4:
                        position = table.baseWearIdLinkAddress;
                        count = table.baseWearIdLinkCount;
                        break;
                    case 5:
                        position = table.innerWearIdLinkAddress;
                        count = table.innerWearIdLinkCount;
                        break;
                }
                start = getPointrStart(nof0.relAddresses, position);
                for (int i = 0; i < count; i++)
                {
                    pointerCheck(position, nof0.relAddresses, sizeof(BCLN), BCLNPtr, start);
                    position += sizeof(BCLN);
                    if (rel0DataStart >= dec14_21TableAddressInt)
                    {
                        pointerCheck(position, nof0.relAddresses, sizeof(BCLNRitem), BCLNRitemPtr, start);
                        position += sizeof(BCLNRitem);
                        if (rel0DataStart >= feb8_22TableAddressInt)
                        {
                            pointerCheck(position, nof0.relAddresses, sizeof(BCLNRitem2), BCLNRitem2Ptr, start);
                            position += sizeof(BCLNRitem2);
                        }
                    }
                }
            }

            BCLNPtr.Sort();
            BCLNRitemPtr.Sort();
            BCLNRitem2Ptr.Sort();

        }

        private static int getPointrStart(List<int> nof0, long position)
        {
            int i = 0;
            while (nof0[i] < position && i < nof0.Count)
            {
                i++;
            }

            return i;
        }

        private static bool specialContains(List<int> nof0, long position, int i)
        {
            for (int index = i; index < nof0.Count; index++)
            {
                if (nof0[index] == position)
                {
                    return true;
                }
            }
            return false;
        }

        private static void pointerCheck(long position, List<int> nof0, int lenToCheck, List<int> ptrList, int start, bool log = false)
        {
            for (int i = 0; i < lenToCheck; i += 4)
            {
                if (!ptrList.Contains(i))
                {
                    if (specialContains(nof0, (position + i), start))
                    {
                        ptrList.Add(i);
                        if (log == true)
                        {
                            Debug.WriteLine((position + i).ToString("X"));
                        }
                    }
                }
            }
        }

        private static CMXTable ReadCMXTable(BufferedStreamReader streamReader, int headerOffset)
        {
            CMXTable cmxTable = new CMXTable();

            cmxTable.bodyAddress = streamReader.Read<int>(); //BODY Costumes
            cmxTable.carmAddress = streamReader.Read<int>(); //CARM Cast Arms
            cmxTable.clegAddress = streamReader.Read<int>(); //CLEG Cast Legs
            cmxTable.bodyOuterAddress = streamReader.Read<int>(); //BODY Outer Wear
            //0x10
            cmxTable.baseWearAddress = streamReader.Read<int>(); //BCLN Base Wear
            cmxTable.innerWearAddress = streamReader.Read<int>(); //BBLY Inner Wear
            cmxTable.bodyPaintAddress = streamReader.Read<int>(); //BDP1 Body Paint 
            cmxTable.stickerAddress = streamReader.Read<int>(); //BDP2 Stickers
            //0x20
            cmxTable.faceAddress = streamReader.Read<int>(); //FACE All heads
            cmxTable.faceMotionAddress = streamReader.Read<int>(); //Face motions
            cmxTable.faceTextureAddress = streamReader.Read<int>(); //NGS Faces?
            cmxTable.faceTexturesAddress = streamReader.Read<int>(); //Face textures and face paint
            //0x30
            cmxTable.accessoryAddress = streamReader.Read<int>(); //ACCE Accessories
            cmxTable.eyeTextureAddress = streamReader.Read<int>(); //EYE eye textures
            cmxTable.earAddress = streamReader.Read<int>(); //reboot ears
            cmxTable.teethAddress = streamReader.Read<int>(); //reboot mouths
            //0x40
            cmxTable.hornAddress = streamReader.Read<int>(); //reboot horns
            cmxTable.skinAddress = streamReader.Read<int>(); //reboot and maybe classic skin?
            cmxTable.eyebrowAddress = streamReader.Read<int>(); //EYEB eyebrows
            cmxTable.eyelashAddress = streamReader.Read<int>(); //EYEL eyelashes
            //0x50
            cmxTable.hairAddress = streamReader.Read<int>(); //HAIR 
            cmxTable.colAddress = streamReader.Read<int>(); //COL, for color chart textures
            cmxTable.unkAddress = streamReader.Read<int>(); //Unknown arrays
            cmxTable.costumeIdLinkAddress = streamReader.Read<int>(); //BCLN Costume ids for recolors
            //0x60
            cmxTable.castArmIdLinkAddress = streamReader.Read<int>(); //BCLN Cast arm ids for recolors
            cmxTable.castLegIdLinkAddress = streamReader.Read<int>(); //BCLN Cast leg ids for recolors
            cmxTable.outerIdLinkAddress = streamReader.Read<int>(); //BCLN Outer ids for recolors
            cmxTable.baseWearIdLinkAddress = streamReader.Read<int>(); //BCLN basewear ids for recolors
            //0x70
            cmxTable.innerWearIdLinkAddress = streamReader.Read<int>(); //BCLN innerwear ids for recolors

            if (headerOffset >= oct21TableAddressInt)
            {
                cmxTable.oct21UnkAddress = streamReader.Read<int>(); //Only in October 12, 2021 builds and forward
            }
            if (headerOffset >= jun7_22TableAddressInt)
            {
                cmxTable.jun7_22Address = streamReader.Read<int>(); //Only in feb 8, 2022 builds and forwared
            }
            if (headerOffset >= feb8_22TableAddressInt)
            {
                cmxTable.feb8_22UnkAddress = streamReader.Read<int>(); //Only in feb 8, 2022 builds and forwared
            }

            cmxTable.bodyCount = streamReader.Read<int>();
            cmxTable.carmCount = streamReader.Read<int>();
            cmxTable.clegCount = streamReader.Read<int>();
            cmxTable.bodyOuterCount = streamReader.Read<int>();

            cmxTable.baseWearCount = streamReader.Read<int>();
            cmxTable.innerWearCount = streamReader.Read<int>();
            cmxTable.bodyPaintCount = streamReader.Read<int>();
            cmxTable.stickerCount = streamReader.Read<int>();

            cmxTable.faceCount = streamReader.Read<int>();
            cmxTable.faceMotionCount = streamReader.Read<int>();
            cmxTable.faceTextureCount = streamReader.Read<int>();
            cmxTable.faceTexturesCount = streamReader.Read<int>();

            cmxTable.accessoryCount = streamReader.Read<int>();
            cmxTable.eyeTextureCount = streamReader.Read<int>();
            cmxTable.earCount = streamReader.Read<int>();
            cmxTable.teethCount = streamReader.Read<int>();

            cmxTable.hornCount = streamReader.Read<int>();
            cmxTable.skinCount = streamReader.Read<int>();
            cmxTable.eyebrowCount = streamReader.Read<int>();
            cmxTable.eyelashCount = streamReader.Read<int>();

            cmxTable.hairCount = streamReader.Read<int>();
            cmxTable.colCount = streamReader.Read<int>();
            cmxTable.unkCount = streamReader.Read<int>();
            cmxTable.costumeIdLinkCount = streamReader.Read<int>();

            cmxTable.castArmIdLinkCount = streamReader.Read<int>();
            cmxTable.castLegIdLinkCount = streamReader.Read<int>();
            cmxTable.outerIdLinkCount = streamReader.Read<int>();
            cmxTable.baseWearIdLinkCount = streamReader.Read<int>();

            cmxTable.innerWearIdLinkCount = streamReader.Read<int>();
            if (headerOffset >= oct21TableAddressInt)
            {
                cmxTable.oct21UnkCount = streamReader.Read<int>(); //Only in October 12, 2021 builds and forward
            }
            if (headerOffset >= jun7_22TableAddressInt)
            {
                cmxTable.jun7_22Count = streamReader.Read<int>(); //Only in feb 8, 2022 builds and forwared
            }
            if (headerOffset >= feb8_22TableAddressInt)
            {
                cmxTable.feb8_22UnkCount = streamReader.Read<int>(); //Only in feb 8, 2022 builds and forwared
            }

            return cmxTable;
        }

        public static bool IsValidOffset(int offset)
        {
            return offset > 0x10;
        }

        private static void ReadNGSHorn(BufferedStreamReader streamReader, int offset, CharacterMakingIndex cmx)
        {
            //NGS Horns
            streamReader.Seek(cmx.cmxTable.hornAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < cmx.cmxTable.hornCount; i++)
            {
                var ngsHornObj = new NGS_HornObject();
                ngsHornObj.num = i;
                ngsHornObj.originalOffset = streamReader.Position();
                ngsHornObj.ngsHorn = streamReader.Read<NGS_Horn>();
                long bookmark = streamReader.Position();

                streamReader.Seek(ngsHornObj.ngsHorn.dataStringPtr + offset, SeekOrigin.Begin);
                ngsHornObj.dataString = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(bookmark, SeekOrigin.Begin);

                cmx.ngsHornDict.Add(ngsHornObj.ngsHorn.id, ngsHornObj);
            }
        }

        private static void ReadNGSTEETH(BufferedStreamReader streamReader, int offset, CharacterMakingIndex cmx)
        {
            //NGS Teeth
            streamReader.Seek(cmx.cmxTable.teethAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < cmx.cmxTable.teethCount; i++)
            {
                var ngsTeethObj = new NGS_TeethObject();
                ngsTeethObj.num = i;
                ngsTeethObj.originalOffset = streamReader.Position();
                ngsTeethObj.ngsTeeth = streamReader.Read<NGS_Teeth>();
                long bookmark = streamReader.Position();

                streamReader.Seek(ngsTeethObj.ngsTeeth.dataStringPtr + offset, SeekOrigin.Begin);
                ngsTeethObj.dataString = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(ngsTeethObj.ngsTeeth.texString1Ptr + offset, SeekOrigin.Begin);
                ngsTeethObj.texString1 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(ngsTeethObj.ngsTeeth.texString2Ptr + offset, SeekOrigin.Begin);
                ngsTeethObj.texString2 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(ngsTeethObj.ngsTeeth.texString3Ptr + offset, SeekOrigin.Begin);
                ngsTeethObj.texString3 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(ngsTeethObj.ngsTeeth.texString4Ptr + offset, SeekOrigin.Begin);
                ngsTeethObj.texString4 = AquaGeneralMethods.ReadCString(streamReader);

                cmx.ngsTeethDict.Add(ngsTeethObj.ngsTeeth.id, ngsTeethObj);
                streamReader.Seek(bookmark, SeekOrigin.Begin);
            }
        }

        private static void ReadNGSEAR(BufferedStreamReader streamReader, int offset, CharacterMakingIndex cmx)
        {
            //NGS ears
            streamReader.Seek(cmx.cmxTable.earAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < cmx.cmxTable.earCount; i++)
            {
                var ngsEarObj = new NGS_EarObject();
                ngsEarObj.num = i;
                ngsEarObj.originalOffset = streamReader.Position();
                ngsEarObj.ngsEar = streamReader.Read<NGS_Ear>();
                long bookmark = streamReader.Position();

                streamReader.Seek(ngsEarObj.ngsEar.dataStringPtr + offset, SeekOrigin.Begin);
                ngsEarObj.dataString = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(ngsEarObj.ngsEar.texString1Ptr + offset, SeekOrigin.Begin);
                ngsEarObj.texString1 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(ngsEarObj.ngsEar.texString2Ptr + offset, SeekOrigin.Begin);
                ngsEarObj.texString2 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(ngsEarObj.ngsEar.texString3Ptr + offset, SeekOrigin.Begin);
                ngsEarObj.texString3 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(ngsEarObj.ngsEar.texString4Ptr + offset, SeekOrigin.Begin);
                ngsEarObj.texString4 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(ngsEarObj.ngsEar.texString5Ptr + offset, SeekOrigin.Begin);
                ngsEarObj.texString5 = AquaGeneralMethods.ReadCString(streamReader);

                cmx.ngsEarDict.Add(ngsEarObj.ngsEar.id, ngsEarObj);
                streamReader.Seek(bookmark, SeekOrigin.Begin);
            }
        }

        private static void ReadBODY(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, BODYObject> dict, int rel0DataStart)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                BODYObject body = new BODYObject();
                body.num = i;
                body.originalOffset = streamReader.Position();
                body.body = streamReader.Read<BODY>();

                if (rel0DataStart >= dec14_21TableAddressInt)
                {
                    body.bodyRitem = streamReader.Read<BODYRitem>();
                }

                body.body2 = streamReader.Read<BODY2>();
                if (rel0DataStart >= feb8_22TableAddressInt)
                {
                    body.body40cap = streamReader.Read<BODY40Cap>();
                }

                if(rel0DataStart >= jan25_23TableAddressInt)
                {
                    body.body2023_1 = streamReader.Read<BODY2023_1>();
                }

                long temp = streamReader.Position();

                if (IsValidOffset(body.body.dataStringPtr))
                {
                    streamReader.Seek(body.body.dataStringPtr + offset, SeekOrigin.Begin);
                    body.dataString = AquaGeneralMethods.ReadCString(streamReader);
                }
                if (IsValidOffset(body.body.texString1Ptr))
                {
                    streamReader.Seek(body.body.texString1Ptr + offset, SeekOrigin.Begin);
                    body.texString1 = AquaGeneralMethods.ReadCString(streamReader);
                }
                if (IsValidOffset(body.body.texString2Ptr))
                {
                    streamReader.Seek(body.body.texString2Ptr + offset, SeekOrigin.Begin);
                    body.texString2 = AquaGeneralMethods.ReadCString(streamReader);
                }
                if (IsValidOffset(body.body.texString3Ptr))
                {
                    streamReader.Seek(body.body.texString3Ptr + offset, SeekOrigin.Begin);
                    body.texString3 = AquaGeneralMethods.ReadCString(streamReader);
                }
                if (IsValidOffset(body.body.texString4Ptr))
                {
                    streamReader.Seek(body.body.texString4Ptr + offset, SeekOrigin.Begin);
                    body.texString4 = AquaGeneralMethods.ReadCString(streamReader);
                }
                if (IsValidOffset(body.body.texString5Ptr))
                {
                    streamReader.Seek(body.body.texString5Ptr + offset, SeekOrigin.Begin);
                    body.texString5 = AquaGeneralMethods.ReadCString(streamReader);
                }
                if (IsValidOffset(body.body.texString6Ptr))
                {
                    streamReader.Seek(body.body.texString6Ptr + offset, SeekOrigin.Begin);
                    body.texString6 = AquaGeneralMethods.ReadCString(streamReader);
                }
                if (IsValidOffset(body.body2023_1.nodeStrPtr_0))
                {
                    streamReader.Seek(body.body2023_1.nodeStrPtr_0 + offset, SeekOrigin.Begin);
                    body.nodeString0 = AquaGeneralMethods.ReadCString(streamReader);
                }
                if (IsValidOffset(body.body2023_1.nodeStrPtr_1))
                {
                    streamReader.Seek(body.body2023_1.nodeStrPtr_1 + offset, SeekOrigin.Begin);
                    body.nodeString1 = AquaGeneralMethods.ReadCString(streamReader);
                }

                streamReader.Seek(temp, SeekOrigin.Begin);

                if (dict.ContainsKey(body.body.id))
                {
                    continue;
                }
                dict.Add(body.body.id, body); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadBBLY(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, BBLYObject> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                BBLYObject bbly = new BBLYObject();
                bbly.num = i;
                bbly.originalOffset = streamReader.Position();
                bbly.bbly = streamReader.Read<BBLY>();
                long temp = streamReader.Position();

                streamReader.Seek(bbly.bbly.texString1Ptr + offset, SeekOrigin.Begin);
                bbly.texString1 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(bbly.bbly.texString2Ptr + offset, SeekOrigin.Begin);
                bbly.texString2 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(bbly.bbly.texString3Ptr + offset, SeekOrigin.Begin);
                bbly.texString3 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(bbly.bbly.texString4Ptr + offset, SeekOrigin.Begin);
                bbly.texString4 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(bbly.bbly.texString5Ptr + offset, SeekOrigin.Begin);
                bbly.texString5 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(temp, SeekOrigin.Begin);

                dict.Add(bbly.bbly.id, bbly); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadSticker(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, StickerObject> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                StickerObject sticker = new StickerObject();
                sticker.num = i;
                sticker.originalOffset = streamReader.Position();
                sticker.sticker = streamReader.Read<Sticker>();
                long temp = streamReader.Position();

                streamReader.Seek(sticker.sticker.texStringPtr + offset, SeekOrigin.Begin);
                sticker.texString = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(temp, SeekOrigin.Begin);

                dict.Add(sticker.sticker.id, sticker); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadFACE(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, FACEObject> dict, int rel0DataStart)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                FACEObject face = new FACEObject();
                face.num = i;
                face.originalOffset = streamReader.Position();
                face.face = streamReader.Read<FACE>();
                if (rel0DataStart > dec14_21TableAddressInt)
                {
                    face.faceRitem = streamReader.Read<FACERitem>();
                }
                face.face2 = streamReader.Read<FACE2>();

                //Handle backwards compatibility
                if (rel0DataStart < dec14_21TableAddressInt)
                {
                    face.faceRitem = new FACERitem();
                    face.faceRitem.unkIntRT0 = face.face2.unkInt0;
                    var bytes = BitConverter.GetBytes(face.face2.unkInt1);
                    face.faceRitem.unkIntRT1 = BitConverter.ToInt16(bytes, 0);
                    face.face2.unkInt0 = BitConverter.ToInt16(bytes, 2);
                    face.face2.unkInt1 = 0;
                }
                if (rel0DataStart > dec14_21TableAddressInt)
                {
                    face.unkFloatRitem = streamReader.Read<float>();
                }
                long temp = streamReader.Position();

                if (face.face.dataStringPtr + offset > 0)
                {
                    streamReader.Seek(face.face.dataStringPtr + offset, SeekOrigin.Begin);
                    face.dataString = AquaGeneralMethods.ReadCString(streamReader);
                }

                if (face.face.texString1Ptr + offset > 0)
                {
                    streamReader.Seek(face.face.texString1Ptr + offset, SeekOrigin.Begin);
                    face.texString1 = AquaGeneralMethods.ReadCString(streamReader);
                }

                if (face.face.texString2Ptr + offset > 0)
                {
                    streamReader.Seek(face.face.texString2Ptr + offset, SeekOrigin.Begin);
                    face.texString2 = AquaGeneralMethods.ReadCString(streamReader);
                }

                if (face.face.texString3Ptr + offset > 0)
                {
                    streamReader.Seek(face.face.texString3Ptr + offset, SeekOrigin.Begin);
                    face.texString3 = AquaGeneralMethods.ReadCString(streamReader);
                }

                if (face.face.texString4Ptr + offset > 0)
                {
                    streamReader.Seek(face.face.texString4Ptr + offset, SeekOrigin.Begin);
                    face.texString4 = AquaGeneralMethods.ReadCString(streamReader);
                }

                if (face.face.texString5Ptr + offset > 0)
                {
                    streamReader.Seek(face.face.texString5Ptr + offset, SeekOrigin.Begin);
                    face.texString5 = AquaGeneralMethods.ReadCString(streamReader);
                }

                if (face.face.texString6Ptr + offset > 0)
                {
                    streamReader.Seek(face.face.texString6Ptr + offset, SeekOrigin.Begin);
                    face.texString6 = AquaGeneralMethods.ReadCString(streamReader);
                }

                streamReader.Seek(temp, SeekOrigin.Begin);

                if (!dict.ContainsKey(face.face.id))
                {
                    dict.Add(face.face.id, face); //Set like this so we can access it by id later if we want. 
                }
            }
        }

        private static void ReadFCMN(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, FCMNObject> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                FCMNObject fcmn = new FCMNObject();
                fcmn.num = i;
                fcmn.originalOffset = streamReader.Position();
                fcmn.fcmn = streamReader.Read<FCMN>();
                long temp = streamReader.Position();

                streamReader.Seek(fcmn.fcmn.proportionAnimPtr + offset, SeekOrigin.Begin);
                fcmn.proportionAnim = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(fcmn.fcmn.faceAnim1Ptr + offset, SeekOrigin.Begin);
                fcmn.faceAnim1 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(fcmn.fcmn.faceAnim2Ptr + offset, SeekOrigin.Begin);
                fcmn.faceAnim2 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(fcmn.fcmn.faceAnim3Ptr + offset, SeekOrigin.Begin);
                fcmn.faceAnim3 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(fcmn.fcmn.faceAnim4Ptr + offset, SeekOrigin.Begin);
                fcmn.faceAnim4 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(fcmn.fcmn.faceAnim5Ptr + offset, SeekOrigin.Begin);
                fcmn.faceAnim5 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(fcmn.fcmn.faceAnim6Ptr + offset, SeekOrigin.Begin);
                fcmn.faceAnim6 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(fcmn.fcmn.faceAnim7Ptr + offset, SeekOrigin.Begin);
                fcmn.faceAnim7 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(fcmn.fcmn.faceAnim8Ptr + offset, SeekOrigin.Begin);
                fcmn.faceAnim8 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(fcmn.fcmn.faceAnim9Ptr + offset, SeekOrigin.Begin);
                fcmn.faceAnim9 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(fcmn.fcmn.faceAnim10Ptr + offset, SeekOrigin.Begin);
                fcmn.faceAnim10 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(temp, SeekOrigin.Begin);

                dict.Add(fcmn.fcmn.id, fcmn); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadFCP(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, FCPObject> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                FCPObject fcp = new FCPObject();
                fcp.num = i;
                fcp.originalOffset = streamReader.Position();
                fcp.fcp = streamReader.Read<FCP>();

                long temp = streamReader.Position();

                streamReader.Seek(fcp.fcp.texString1Ptr + offset, SeekOrigin.Begin);
                fcp.texString1 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(fcp.fcp.texString2Ptr + offset, SeekOrigin.Begin);
                fcp.texString2 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(fcp.fcp.texString3Ptr + offset, SeekOrigin.Begin);
                fcp.texString3 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(fcp.fcp.texString4Ptr + offset, SeekOrigin.Begin);
                fcp.texString4 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(temp, SeekOrigin.Begin);
                if (dict.ContainsKey(fcp.fcp.id))
                {
                    Console.WriteLine(fcp.fcp.id);
                }
                dict.Add(fcp.fcp.id, fcp); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadFaceTextures(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, FaceTextureObject> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                FaceTextureObject ngsFace = new FaceTextureObject();
                ngsFace.num = i;
                ngsFace.originalOffset = streamReader.Position();
                ngsFace.ngsFace = streamReader.Read<FaceTextures>();
                long temp = streamReader.Position();

                streamReader.Seek(ngsFace.ngsFace.texString1Ptr + offset, SeekOrigin.Begin);
                ngsFace.texString1 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(ngsFace.ngsFace.texString2Ptr + offset, SeekOrigin.Begin);
                ngsFace.texString2 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(ngsFace.ngsFace.texString3Ptr + offset, SeekOrigin.Begin);
                ngsFace.texString3 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(ngsFace.ngsFace.texString4Ptr + offset, SeekOrigin.Begin);
                ngsFace.texString4 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(temp, SeekOrigin.Begin);

                dict.Add(ngsFace.ngsFace.id, ngsFace); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadACCE(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, ACCEObject> dict, int cmxDateSize)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                ACCEObject acce = new ACCEObject();
                acce.num = i;
                acce.originalOffset = streamReader.Position();
                acce.acce = streamReader.Read<ACCE>();                      //0x28
                if (cmxDateSize >= feb8_22TableAddressInt)
                {
                    acce.acceFeb8_22 = streamReader.Read<ACCE_Feb8_22>();   //0x1C
                }
                acce.acceB = streamReader.Read<ACCE_B>();                   //0x1C
                acce.acce2a = streamReader.Read<ACCE2A>();                  //0x1C
                //This int was added to the middle of these in the Aug_3_2021 patch
                if (count >= 5977)
                {
                    acce.flt_54 = streamReader.Read<float>();                 //0x4
                }
                acce.acce2b = streamReader.Read<ACCE2B>();                  //0xC
                for (int j = 0; j < 3; j++)                                 //0x60
                { 
                    acce.acce12List.Add(ReadAcce12Object(streamReader, count));
                }

                if(cmxDateSize >= oct4_22TableAddressInt)
                {
                    acce.effectNamePtr = streamReader.Read<int>();
                }

                if (cmxDateSize >= aug17_22TableAddressInt)
                {
                    acce.flt_90 = streamReader.Read<float>();
                }

                long temp = streamReader.Position();

                if (acce.acce.dataStringPtr + offset > 0)
                {
                    streamReader.Seek(acce.acce.dataStringPtr + offset, SeekOrigin.Begin);
                    acce.dataString = AquaGeneralMethods.ReadCString(streamReader);
                }
                if (acce.acce.nodeAttach1Ptr + offset > 0)
                {
                    streamReader.Seek(acce.acce.nodeAttach1Ptr + offset, SeekOrigin.Begin);
                    acce.nodeAttach1 = AquaGeneralMethods.ReadCString(streamReader);
                }
                if (acce.acce.nodeAttach2Ptr + offset > 0)
                {
                    streamReader.Seek(acce.acce.nodeAttach2Ptr + offset, SeekOrigin.Begin);
                    acce.nodeAttach2 = AquaGeneralMethods.ReadCString(streamReader);
                }
                if (acce.acce.nodeAttach3Ptr + offset > 0)
                {
                    streamReader.Seek(acce.acce.nodeAttach3Ptr + offset, SeekOrigin.Begin);
                    acce.nodeAttach3 = AquaGeneralMethods.ReadCString(streamReader);
                }
                if (acce.acce.nodeAttach4Ptr + offset > 0)
                {
                    streamReader.Seek(acce.acce.nodeAttach4Ptr + offset, SeekOrigin.Begin);
                    acce.nodeAttach4 = AquaGeneralMethods.ReadCString(streamReader);
                }
                if (acce.acce.nodeAttach5Ptr + offset > 0)
                {
                    streamReader.Seek(acce.acce.nodeAttach5Ptr + offset, SeekOrigin.Begin);
                    acce.nodeAttach5 = AquaGeneralMethods.ReadCString(streamReader);
                }
                if (acce.acce.nodeAttach6Ptr + offset > 0)
                {
                    streamReader.Seek(acce.acce.nodeAttach6Ptr + offset, SeekOrigin.Begin);
                    acce.nodeAttach6 = AquaGeneralMethods.ReadCString(streamReader);
                }
                if (acce.acce.nodeAttach7Ptr + offset > 0)
                {
                    streamReader.Seek(acce.acce.nodeAttach7Ptr + offset, SeekOrigin.Begin);
                    acce.nodeAttach7 = AquaGeneralMethods.ReadCString(streamReader);
                }
                if (acce.acce.nodeAttach8Ptr + offset > 0)
                {
                    streamReader.Seek(acce.acce.nodeAttach8Ptr + offset, SeekOrigin.Begin);
                    acce.nodeAttach8 = AquaGeneralMethods.ReadCString(streamReader);
                }
                if (cmxDateSize >= feb8_22TableAddressInt)
                {
                    if (acce.acceFeb8_22.acceString9Ptr + offset > 0)
                    {
                        streamReader.Seek(acce.acceFeb8_22.acceString9Ptr + offset, SeekOrigin.Begin);
                        acce.nodeAttach9 = AquaGeneralMethods.ReadCString(streamReader);
                    }
                    if (acce.acceFeb8_22.acceString10Ptr + offset > 0)
                    {
                        streamReader.Seek(acce.acceFeb8_22.acceString10Ptr + offset, SeekOrigin.Begin);
                        acce.nodeAttach10 = AquaGeneralMethods.ReadCString(streamReader);
                    }
                    if (acce.acceFeb8_22.acceString11Ptr + offset > 0)
                    {
                        streamReader.Seek(acce.acceFeb8_22.acceString11Ptr + offset, SeekOrigin.Begin);
                        acce.nodeAttach11 = AquaGeneralMethods.ReadCString(streamReader);
                    }
                    if (acce.acceFeb8_22.acceString12Ptr + offset > 0)
                    {
                        streamReader.Seek(acce.acceFeb8_22.acceString12Ptr + offset, SeekOrigin.Begin);
                        acce.nodeAttach12 = AquaGeneralMethods.ReadCString(streamReader);
                    }
                    if (acce.acceFeb8_22.acceString13Ptr + offset > 0)
                    {
                        streamReader.Seek(acce.acceFeb8_22.acceString13Ptr + offset, SeekOrigin.Begin);
                        acce.nodeAttach13 = AquaGeneralMethods.ReadCString(streamReader);
                    }
                    if (acce.acceFeb8_22.acceString14Ptr + offset > 0)
                    {
                        streamReader.Seek(acce.acceFeb8_22.acceString14Ptr + offset, SeekOrigin.Begin);
                        acce.nodeAttach14 = AquaGeneralMethods.ReadCString(streamReader);
                    }
                    if (acce.acceFeb8_22.acceString15Ptr + offset > 0)
                    {
                        streamReader.Seek(acce.acceFeb8_22.acceString15Ptr + offset, SeekOrigin.Begin);
                        acce.nodeAttach15 = AquaGeneralMethods.ReadCString(streamReader);
                    }

                    if (cmxDateSize >= oct4_22TableAddressInt)
                    {
                        if (acce.effectNamePtr + offset > 0)
                        {
                            streamReader.Seek(acce.effectNamePtr + offset, SeekOrigin.Begin);
                            acce.effectName = AquaGeneralMethods.ReadCString(streamReader);
                        }

                    }
                }

                streamReader.Seek(temp, SeekOrigin.Begin);

                if (!dict.ContainsKey(acce.acce.id))
                {
                    dict.Add(acce.acce.id, acce); //Set like this so we can access it by id later if we want. 
                }
                else
                {
                    Console.WriteLine($"Duplicate acce id: {acce.acce.id}");
                }
            }
        }

        private static ACCE_12Object ReadAcce12Object(BufferedStreamReader streamReader, int count)
        {
            ACCE_12Object acce12Obj = new ACCE_12Object();
            acce12Obj.unkFloat0 = streamReader.Read<float>();
            acce12Obj.unkFloat1 = streamReader.Read<float>();
            acce12Obj.unkInt0 = streamReader.Read<int>();
            acce12Obj.unkInt1 = streamReader.Read<int>();

            if (count >= 6249)
            {
                acce12Obj.unkIntFeb822_0 = streamReader.Read<int>();
            }

            acce12Obj.unkShort0 = streamReader.Read<short>();
            acce12Obj.unkShort1 = streamReader.Read<short>();
            acce12Obj.unkShort2 = streamReader.Read<short>();
            acce12Obj.unkShort3 = streamReader.Read<short>();

            if (count >= 6249)
            {
                acce12Obj.unkIntFeb822_1 = streamReader.Read<int>();
            }

            return acce12Obj;
        }

        private static void ReadEYE(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, EYEObject> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                EYEObject eye = new EYEObject();
                eye.num = i;
                eye.originalOffset = streamReader.Position();
                eye.eye = streamReader.Read<EYE>();
                long temp = streamReader.Position();

                streamReader.Seek(eye.eye.texString1Ptr + offset, SeekOrigin.Begin);
                eye.texString1 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(eye.eye.texString2Ptr + offset, SeekOrigin.Begin);
                eye.texString2 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(eye.eye.texString3Ptr + offset, SeekOrigin.Begin);
                eye.texString3 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(eye.eye.texString4Ptr + offset, SeekOrigin.Begin);
                eye.texString4 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(eye.eye.texString5Ptr + offset, SeekOrigin.Begin);
                eye.texString5 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(temp, SeekOrigin.Begin);

                dict.Add(eye.eye.id, eye); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadNGSSKIN(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, NGS_SKINObject> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                NGS_SKINObject ngsSkin = new NGS_SKINObject();
                ngsSkin.num = i;
                ngsSkin.originalOffset = streamReader.Position();
                ngsSkin.ngsSkin = streamReader.Read<NGS_Skin>();
                long temp = streamReader.Position();

                streamReader.Seek(ngsSkin.ngsSkin.texString1Ptr + offset, SeekOrigin.Begin);
                ngsSkin.texString1 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(ngsSkin.ngsSkin.texString2Ptr + offset, SeekOrigin.Begin);
                ngsSkin.texString2 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(ngsSkin.ngsSkin.texString3Ptr + offset, SeekOrigin.Begin);
                ngsSkin.texString3 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(ngsSkin.ngsSkin.texString4Ptr + offset, SeekOrigin.Begin);
                ngsSkin.texString4 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(ngsSkin.ngsSkin.texString6Ptr + offset, SeekOrigin.Begin);
                ngsSkin.texString5 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(ngsSkin.ngsSkin.texString6Ptr + offset, SeekOrigin.Begin);
                ngsSkin.texString6 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(ngsSkin.ngsSkin.texString7Ptr + offset, SeekOrigin.Begin);
                ngsSkin.texString7 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(ngsSkin.ngsSkin.texString8Ptr + offset, SeekOrigin.Begin);
                ngsSkin.texString8 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(ngsSkin.ngsSkin.texString9Ptr + offset, SeekOrigin.Begin);
                ngsSkin.texString9 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(ngsSkin.ngsSkin.texString10Ptr + offset, SeekOrigin.Begin);
                ngsSkin.texString10 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(temp, SeekOrigin.Begin);

                dict.Add(ngsSkin.ngsSkin.id, ngsSkin); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadEYEB(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, EYEBObject> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                EYEBObject eyeb = new EYEBObject();
                eyeb.num = i;
                eyeb.originalOffset = streamReader.Position();
                eyeb.eyeb = streamReader.Read<EYEB>();
                long temp = streamReader.Position();

                streamReader.Seek(eyeb.eyeb.texString1Ptr + offset, SeekOrigin.Begin);
                eyeb.texString1 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(eyeb.eyeb.texString2Ptr + offset, SeekOrigin.Begin);
                eyeb.texString2 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(eyeb.eyeb.texString3Ptr + offset, SeekOrigin.Begin);
                eyeb.texString3 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(eyeb.eyeb.texString4Ptr + offset, SeekOrigin.Begin);
                eyeb.texString4 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(temp, SeekOrigin.Begin);

                dict.Add(eyeb.eyeb.id, eyeb); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadHAIR(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, HAIRObject> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                HAIRObject hair = new HAIRObject();
                hair.num = i;
                hair.originalOffset = streamReader.Position();
                hair.hair = streamReader.Read<HAIR>();
                long temp = streamReader.Position();

                streamReader.Seek(hair.hair.dataStringPtr + offset, SeekOrigin.Begin);
                hair.dataString = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(hair.hair.texString1Ptr + offset, SeekOrigin.Begin);
                hair.texString1 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(hair.hair.texString2Ptr + offset, SeekOrigin.Begin);
                hair.texString2 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(hair.hair.texString3Ptr + offset, SeekOrigin.Begin);
                hair.texString3 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(hair.hair.texString4Ptr + offset, SeekOrigin.Begin);
                hair.texString4 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(hair.hair.texString5Ptr + offset, SeekOrigin.Begin);
                hair.texString5 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(hair.hair.texString6Ptr + offset, SeekOrigin.Begin);
                hair.texString6 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(hair.hair.texString7Ptr + offset, SeekOrigin.Begin);
                hair.texString7 = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(temp, SeekOrigin.Begin);

                dict.Add(hair.hair.id, hair); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadNIFLCOL(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, NIFL_COLObject> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                NIFL_COLObject col = new NIFL_COLObject();
                col.num = i;
                col.originalOffset = streamReader.Position();
                col.niflCol = streamReader.Read<NIFL_COL>();
                long temp = streamReader.Position();

                streamReader.Seek(col.niflCol.textStringPtr + offset, SeekOrigin.Begin);
                col.textString = AquaGeneralMethods.ReadCString(streamReader);

                streamReader.Seek(temp, SeekOrigin.Begin);

                dict.Add(col.niflCol.id, col); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadPart6_7_22(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, Part6_7_22Obj> dict, int rel0Start)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                Part6_7_22Obj part = new Part6_7_22Obj();
                part.partStruct = streamReader.Read<Part6_7_22>();
                if (!dict.ContainsKey(part.partStruct.id))
                {
                    dict.Add(part.partStruct.id, part); //Set like this so we can access it by id later if we want. 
                }
                else
                {
                    Console.WriteLine($"Duplicate key {part.partStruct.id} at {(streamReader.Position() - offset).ToString("X")}");
                }
            }
        }

        private static void ReadIndexLinks(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, BCLNObject> dict, int rel0Start)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                BCLNObject bcln = new BCLNObject();
                bcln.bcln = streamReader.Read<BCLN>();
                if (rel0Start >= dec14_21TableAddressInt)
                {
                    bcln.bclnRitem = streamReader.Read<BCLNRitem>();
                    if (rel0Start >= feb8_22TableAddressInt)
                    {
                        bcln.bclnRitem2 = streamReader.Read<BCLNRitem2>();
                    }
                }
                if (!dict.ContainsKey(bcln.bcln.id))
                {
                    dict.Add(bcln.bcln.id, bcln); //Set like this so we can access it by id later if we want. 
                }
                else
                {
                    Console.WriteLine($"Duplicate key {bcln.bcln.id} at {(streamReader.Position() - offset).ToString("X")}");
                }
            }
        }

        public static CharacterMakingIndex ExtractCMX(string pso2_binDir, CharacterMakingIndex aquaCMX = null)
        {
            //Load CMX
            string cmxPath = Path.Combine(pso2_binDir, dataDir, GetFileHash(classicCMX));

            if (File.Exists(cmxPath))
            {
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
                        aquaCMX = ReadCMX(file, aquaCMX);
                    }
                }

                cmxIce = null;
            }

            return aquaCMX;
        }

        //Generates a CCO that defaults part counts to 1
        public static byte[] GenerateAccessoryCCO(CharacterMakingIndex cmx)
        {
            List<byte> outBytes = new List<byte>();
            List<int> nof0PointerLocations = new List<int>(); //Used for the NOF0 section

            int rel0SizeOffset = 0;

            //REL0
            outBytes.AddRange(Encoding.UTF8.GetBytes("REL0"));
            rel0SizeOffset = outBytes.Count; //We'll fill this later
            outBytes.AddRange(BitConverter.GetBytes(0));
            outBytes.AddRange(BitConverter.GetBytes(0));
            outBytes.AddRange(BitConverter.GetBytes(0));

            outBytes.AddRange(BitConverter.GetBytes(-1));

            //Write data
            foreach (var acce in cmx.accessoryDict.Keys)
            {
                outBytes.AddRange(BitConverter.GetBytes(acce));
                outBytes.AddRange(BitConverter.GetBytes(1));
            }

            //Write header data
            SetByteListInt(outBytes, rel0SizeOffset + 4, outBytes.Count);
            outBytes.AddRange(BitConverter.GetBytes(cmx.accessoryDict.Count));
            NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(0x14));
            AlignWriter(outBytes, 0x10);

            //Write REL0 Size
            SetByteListInt(outBytes, rel0SizeOffset, outBytes.Count - 0x8);

            //Write NOF0
            int NOF0Offset = outBytes.Count;
            int NOF0Size = (nof0PointerLocations.Count + 2) * 4;
            int NOF0FullSize = NOF0Size + 0x8;
            outBytes.AddRange(Encoding.UTF8.GetBytes("NOF0"));
            outBytes.AddRange(BitConverter.GetBytes(NOF0Size));
            outBytes.AddRange(BitConverter.GetBytes(nof0PointerLocations.Count));
            outBytes.AddRange(BitConverter.GetBytes(0x10));//Write pointer offsets

            for (int i = 0; i < nof0PointerLocations.Count; i++)
            {
                outBytes.AddRange(BitConverter.GetBytes(nof0PointerLocations[i]));
            }
            NOF0FullSize += AlignWriter(outBytes, 0x10);

            //NEND
            outBytes.AddRange(Encoding.UTF8.GetBytes("NEND"));
            outBytes.AddRange(BitConverter.GetBytes(0x8));
            outBytes.AddRange(BitConverter.GetBytes(0));
            outBytes.AddRange(BitConverter.GetBytes(0));

            //Generate NIFL
            AquaCommon.NIFL nifl = new AquaCommon.NIFL();
            nifl.magic = BitConverter.ToInt32(Encoding.UTF8.GetBytes("NIFL"), 0);
            nifl.NIFLLength = 0x18;
            nifl.unkInt0 = 1;
            nifl.offsetAddition = 0x20;

            nifl.NOF0Offset = NOF0Offset;
            nifl.NOF0OffsetFull = NOF0Offset + 0x20;
            nifl.NOF0BlockSize = NOF0FullSize;
            nifl.padding0 = 0;

            //Write NIFL
            outBytes.InsertRange(0, ConvertStruct(nifl));

            return outBytes.ToArray();
        }
    }
}
