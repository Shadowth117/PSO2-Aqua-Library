using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using zamboni;
using static AquaModelLibrary.CharacterMakingIndex;
using static AquaModelLibrary.VTBFMethods;
using static AquaModelLibrary.AquaMiscMethods;

namespace AquaModelLibrary
{
    public static class CharacterMakingIndexMethods
    {
        public static CharacterMakingIndex ReadCMX(string fileName, CharacterMakingIndex cmx = null)
        {
            using (Stream stream = (Stream)new FileStream(fileName, FileMode.Open))
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
            if(cmx == null)
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
                        if(pastBody0 == false)
                        {
                            cmx.costumeDict.Add((int)data[0][0xFF], parseBODY(data));
                        } else
                        {
                            cmx.baseWearDict.Add((int)data[0][0xFF], parseBODY(data));
                        }
                        break;
                    case "CARM":
                        pastBody0 = true;
                        if(pastCarm0 == false)
                        {
                            cmx.carmDict.Add((int)data[0][0xFF], parseCARM(data));
                        } else
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
                        //These are entirely different for VTBF. Not really sure what to do with them for now.
                        //parts.colTags.Add((int)data[0][0xFF], data);
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

            streamReader.Seek(cmx.rel0.REL0DataStart + offset, SeekOrigin.Begin);
            cmx.cmxTable = streamReader.Read<CMXTable>();

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
            ReadIndexLinks(streamReader, offset, cmx.cmxTable.costumeIdLinkAddress, cmx.cmxTable.costumeIdLinkCount, cmx.costumeIdLink);

            ReadIndexLinks(streamReader, offset, cmx.cmxTable.castArmIdLinkAddress, cmx.cmxTable.castArmIdLinkCount, cmx.castArmIdLink);
            ReadIndexLinks(streamReader, offset, cmx.cmxTable.castLegIdLinkAddress, cmx.cmxTable.castLegIdLinkCount, cmx.clegIdLink);
            ReadIndexLinks(streamReader, offset, cmx.cmxTable.outerIdLinkAddress, cmx.cmxTable.outerIdLinkCount, cmx.outerWearIdLink);
            ReadIndexLinks(streamReader, offset, cmx.cmxTable.baseWearIdLinkAddress, cmx.cmxTable.baseWearIdLinkCount, cmx.baseWearIdLink);

            ReadIndexLinks(streamReader, offset, cmx.cmxTable.innerWearIdLinkAddress, cmx.cmxTable.innerWearIdLinkCount, cmx.innerWearIdLink);

            return cmx;
        }

        private static void ReadNGSHorn(BufferedStreamReader streamReader, int offset, CharacterMakingIndex cmx)
        {
            //NGS Horns
            streamReader.Seek(cmx.cmxTable.hornAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < cmx.cmxTable.hornCount; i++)
            {
                var ngsHornObj = new NGS_HornObject();
                ngsHornObj.ngsHorn = streamReader.Read<NGS_Horn>();
                long bookmark = streamReader.Position();

                streamReader.Seek(ngsHornObj.ngsHorn.unkSubStructPtr + offset, SeekOrigin.Begin);
                ngsHornObj.substruct = streamReader.Read<NGS_Unk_Substruct>();

                streamReader.Seek(ngsHornObj.ngsHorn.dataStringPtr + offset, SeekOrigin.Begin);
                ngsHornObj.dataString.SetString(AquaObjectMethods.ReadCString(streamReader));

                cmx.ngsHornList.Add(ngsHornObj);

                //Some ids can't be parsed and indexed properly. In this case, skip them. They seemingly don't have associated files anyways. 
                if (!ngsHornObj.dataString.GetString().Contains(rebootHornDataStr))
                {
                    continue;    
                }

                //Hackily get the id from the strings. This only works because NGS uses proper ids in the asset filenames and wouldn't work in classic pso2.
                int id = Int32.Parse(ngsHornObj.dataString.GetString().Replace(rebootHornDataStr, ""));

                //There aren't texture strings to double check this like with the teeth and ears so we have to assume this is correct. Thankfully, the game doesn't appear to have any conflicts here so far.

                cmx.ngsHornDict.Add(id, ngsHornObj);
                streamReader.Seek(bookmark, SeekOrigin.Begin);
            }
        }

        private static void ReadNGSTEETH(BufferedStreamReader streamReader, int offset, CharacterMakingIndex cmx)
        {
            //NGS Teeth
            streamReader.Seek(cmx.cmxTable.teethAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < cmx.cmxTable.teethCount; i++)
            {
                var ngsTeethObj = new NGS_TeethObject();
                ngsTeethObj.ngsTeeth = streamReader.Read<NGS_Teeth>();
                long bookmark = streamReader.Position();

                streamReader.Seek(ngsTeethObj.ngsTeeth.unkSubStructPtr + offset, SeekOrigin.Begin);
                ngsTeethObj.substruct = streamReader.Read<NGS_Unk_Substruct>();

                streamReader.Seek(ngsTeethObj.ngsTeeth.dataStringPtr + offset, SeekOrigin.Begin);
                ngsTeethObj.dataString.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(ngsTeethObj.ngsTeeth.texString1Ptr + offset, SeekOrigin.Begin);
                ngsTeethObj.texString1.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(ngsTeethObj.ngsTeeth.texString2Ptr + offset, SeekOrigin.Begin);
                ngsTeethObj.texString2.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(ngsTeethObj.ngsTeeth.texString3Ptr + offset, SeekOrigin.Begin);
                ngsTeethObj.texString3.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(ngsTeethObj.ngsTeeth.texString4Ptr + offset, SeekOrigin.Begin);
                ngsTeethObj.texString4.SetString(AquaObjectMethods.ReadCString(streamReader));

                //Hackily get the id from the strings. This only works because NGS uses proper ids in the asset filenames and wouldn't work in classic pso2.
                int id = Int32.Parse(ngsTeethObj.dataString.GetString().Replace(rebootTeethDataStr, ""));
                string tempId2 = ngsTeethObj.texString1.GetString().Replace(rebootTeethDataStr, "");
                int id2 = Int32.Parse(tempId2.Replace("_d.dds", ""));

                //Some of the more debug looking stuff recycles the data names. This is hacky, but getting the id from these strings is already hacky.
                if (id != id2)
                {
                    id = id2;
                }
                cmx.ngsTeethDict.Add(id, ngsTeethObj);
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
                ngsEarObj.ngsEar = streamReader.Read<NGS_Ear>();
                long bookmark = streamReader.Position();

                streamReader.Seek(ngsEarObj.ngsEar.unkSubStructPtr + offset, SeekOrigin.Begin);
                ngsEarObj.subStruct = streamReader.Read<NGS_Unk_Substruct>();

                streamReader.Seek(ngsEarObj.ngsEar.dataStringPtr + offset, SeekOrigin.Begin);
                ngsEarObj.dataString.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(ngsEarObj.ngsEar.texString1Ptr + offset, SeekOrigin.Begin);
                ngsEarObj.texString1.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(ngsEarObj.ngsEar.texString2Ptr + offset, SeekOrigin.Begin);
                ngsEarObj.texString2.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(ngsEarObj.ngsEar.texString3Ptr + offset, SeekOrigin.Begin);
                ngsEarObj.texString3.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(ngsEarObj.ngsEar.texString4Ptr + offset, SeekOrigin.Begin);
                ngsEarObj.texString4.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(ngsEarObj.ngsEar.texString5Ptr + offset, SeekOrigin.Begin);
                ngsEarObj.texString5.SetString(AquaObjectMethods.ReadCString(streamReader));

                //Hackily get the id from the strings. This only works because NGS uses proper ids in the asset filenames and wouldn't work in classic pso2.
                int id = Int32.Parse(ngsEarObj.dataString.GetString().Replace(rebootEarDataStr, ""));
                string tempId2 = ngsEarObj.texString1.GetString().Replace(rebootEarDataStr, "");
                int id2 = Int32.Parse(tempId2.Replace("_d.dds", ""));
                
                //Some of the more debug looking stuff recycles the data names. This is hacky, but getting the id from these strings is already hacky.
                if(id != id2)
                {
                    id = id2;
                }
                cmx.ngsEarDict.Add(id, ngsEarObj);
                streamReader.Seek(bookmark, SeekOrigin.Begin);
            }
        }

        private static void ReadBODY(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, BODYObject> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                BODYObject body = new BODYObject();
                body.body = streamReader.Read<BODY>();
                long temp = streamReader.Position();

                streamReader.Seek(body.body.dataStringPtr + offset, SeekOrigin.Begin);
                body.dataString.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(body.body.texString1Ptr + offset, SeekOrigin.Begin);
                body.texString1.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(body.body.texString2Ptr + offset, SeekOrigin.Begin);
                body.texString2.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(body.body.texString3Ptr + offset, SeekOrigin.Begin);
                body.texString3.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(body.body.texString4Ptr + offset, SeekOrigin.Begin);
                body.texString4.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(body.body.texString5Ptr + offset, SeekOrigin.Begin);
                body.texString5.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(body.body.texString6Ptr + offset, SeekOrigin.Begin);
                body.texString6.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(temp, SeekOrigin.Begin);

                dict.Add(body.body.id, body); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadBBLY(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, BBLYObject> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                BBLYObject bbly = new BBLYObject();
                bbly.bbly = streamReader.Read<BBLY>();
                long temp = streamReader.Position();

                streamReader.Seek(bbly.bbly.texString1Ptr + offset, SeekOrigin.Begin);
                bbly.texString1.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(bbly.bbly.texString2Ptr + offset, SeekOrigin.Begin);
                bbly.texString2.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(bbly.bbly.texString3Ptr + offset, SeekOrigin.Begin);
                bbly.texString3.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(bbly.bbly.texString4Ptr + offset, SeekOrigin.Begin);
                bbly.texString4.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(bbly.bbly.texString5Ptr + offset, SeekOrigin.Begin);
                bbly.texString5.SetString(AquaObjectMethods.ReadCString(streamReader));

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
                sticker.sticker = streamReader.Read<Sticker>();
                long temp = streamReader.Position();

                streamReader.Seek(sticker.sticker.texStringPtr + offset, SeekOrigin.Begin);
                sticker.texString.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(temp, SeekOrigin.Begin);

                dict.Add(sticker.sticker.id, sticker); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadFACE(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, FACEObject> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                FACEObject face = new FACEObject();
                face.face = streamReader.Read<FACE>();
                long temp = streamReader.Position();

                streamReader.Seek(face.face.dataStringPtr + offset, SeekOrigin.Begin);
                face.dataString.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(face.face.texString1Ptr + offset, SeekOrigin.Begin);
                face.texString1.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(face.face.texString2Ptr + offset, SeekOrigin.Begin);
                face.texString2.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(face.face.texString3Ptr + offset, SeekOrigin.Begin);
                face.texString3.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(face.face.texString4Ptr + offset, SeekOrigin.Begin);
                face.texString4.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(face.face.texString5Ptr + offset, SeekOrigin.Begin);
                face.texString5.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(face.face.texString6Ptr + offset, SeekOrigin.Begin);
                face.texString6.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(temp, SeekOrigin.Begin);

                dict.Add(face.face.id, face); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadFCMN(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, FCMNObject> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                FCMNObject fcmn = new FCMNObject();
                fcmn.fcmn = streamReader.Read<FCMN>();
                long temp = streamReader.Position();

                streamReader.Seek(fcmn.fcmn.proportionAnimPtr + offset, SeekOrigin.Begin);
                fcmn.proportionAnim.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(fcmn.fcmn.faceAnim1Ptr + offset, SeekOrigin.Begin);
                fcmn.faceAnim1.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(fcmn.fcmn.faceAnim2Ptr + offset, SeekOrigin.Begin);
                fcmn.faceAnim2.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(fcmn.fcmn.faceAnim3Ptr + offset, SeekOrigin.Begin);
                fcmn.faceAnim3.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(fcmn.fcmn.faceAnim4Ptr + offset, SeekOrigin.Begin);
                fcmn.faceAnim4.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(fcmn.fcmn.faceAnim5Ptr + offset, SeekOrigin.Begin);
                fcmn.faceAnim5.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(fcmn.fcmn.faceAnim6Ptr + offset, SeekOrigin.Begin);
                fcmn.faceAnim6.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(fcmn.fcmn.faceAnim7Ptr + offset, SeekOrigin.Begin);
                fcmn.faceAnim7.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(fcmn.fcmn.faceAnim8Ptr + offset, SeekOrigin.Begin);
                fcmn.faceAnim8.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(fcmn.fcmn.faceAnim9Ptr + offset, SeekOrigin.Begin);
                fcmn.faceAnim9.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(fcmn.fcmn.faceAnim10Ptr + offset, SeekOrigin.Begin);
                fcmn.faceAnim10.SetString(AquaObjectMethods.ReadCString(streamReader));

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
                fcp.fcp = streamReader.Read<FCP>();

                long temp = streamReader.Position();

                streamReader.Seek(fcp.fcp.texString1Ptr + offset, SeekOrigin.Begin);
                fcp.texString1.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(fcp.fcp.texString2Ptr + offset, SeekOrigin.Begin);
                fcp.texString2.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(fcp.fcp.texString3Ptr + offset, SeekOrigin.Begin);
                fcp.texString3.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(fcp.fcp.texString4Ptr + offset, SeekOrigin.Begin);
                fcp.texString4.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(temp, SeekOrigin.Begin);

                dict.Add(fcp.fcp.id, fcp); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadNGSFACE(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, NGS_FACEObject> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                NGS_FACEObject ngsFace = new NGS_FACEObject();
                ngsFace.ngsFace = streamReader.Read<NGS_FACE>();
                long temp = streamReader.Position();

                streamReader.Seek(ngsFace.ngsFace.texString1Ptr + offset, SeekOrigin.Begin);
                ngsFace.texString1.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(ngsFace.ngsFace.texString2Ptr + offset, SeekOrigin.Begin);
                ngsFace.texString2.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(ngsFace.ngsFace.texString3Ptr + offset, SeekOrigin.Begin);
                ngsFace.texString3.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(ngsFace.ngsFace.texString4Ptr + offset, SeekOrigin.Begin);
                ngsFace.texString4.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(temp, SeekOrigin.Begin);

                dict.Add(ngsFace.ngsFace.id, ngsFace); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadACCE(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, ACCEObject> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                ACCEObject acce = new ACCEObject();
                acce.acce = streamReader.Read<ACCE>();
                long temp = streamReader.Position();

                streamReader.Seek(acce.acce.dataStringPtr + offset, SeekOrigin.Begin);
                acce.dataString.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(acce.acce.nodeAttach1Ptr + offset, SeekOrigin.Begin);
                acce.nodeAttach1.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(acce.acce.nodeAttach2Ptr + offset, SeekOrigin.Begin);
                acce.nodeAttach2.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(acce.acce.nodeAttach3Ptr + offset, SeekOrigin.Begin);
                acce.nodeAttach3.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(acce.acce.nodeAttach4Ptr + offset, SeekOrigin.Begin);
                acce.nodeAttach4.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(acce.acce.nodeAttach5Ptr + offset, SeekOrigin.Begin);
                acce.nodeAttach5.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(acce.acce.nodeAttach6Ptr + offset, SeekOrigin.Begin);
                acce.nodeAttach6.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(acce.acce.nodeAttach7Ptr + offset, SeekOrigin.Begin);
                acce.nodeAttach7.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(acce.acce.nodeAttach8Ptr + offset, SeekOrigin.Begin);
                acce.nodeAttach8.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(temp, SeekOrigin.Begin);

                dict.Add(acce.acce.id, acce); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadEYE(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, EYEObject> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                EYEObject eye = new EYEObject();
                eye.eye = streamReader.Read<EYE>();
                long temp = streamReader.Position();

                streamReader.Seek(eye.eye.texString1Ptr + offset, SeekOrigin.Begin);
                eye.texString1.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(eye.eye.texString2Ptr + offset, SeekOrigin.Begin);
                eye.texString2.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(eye.eye.texString3Ptr + offset, SeekOrigin.Begin);
                eye.texString3.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(eye.eye.texString4Ptr + offset, SeekOrigin.Begin);
                eye.texString4.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(eye.eye.texString5Ptr + offset, SeekOrigin.Begin);
                eye.texString5.SetString(AquaObjectMethods.ReadCString(streamReader));

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
                ngsSkin.ngsSkin = streamReader.Read<NGS_Skin>();
                long temp = streamReader.Position();

                streamReader.Seek(ngsSkin.ngsSkin.texString1Ptr + offset, SeekOrigin.Begin);
                ngsSkin.texString1.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(ngsSkin.ngsSkin.texString2Ptr + offset, SeekOrigin.Begin);
                ngsSkin.texString2.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(ngsSkin.ngsSkin.texString3Ptr + offset, SeekOrigin.Begin);
                ngsSkin.texString3.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(ngsSkin.ngsSkin.texString4Ptr + offset, SeekOrigin.Begin);
                ngsSkin.texString4.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(ngsSkin.ngsSkin.texString5Ptr + offset, SeekOrigin.Begin);
                ngsSkin.texString5.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(ngsSkin.ngsSkin.texString6Ptr + offset, SeekOrigin.Begin);
                ngsSkin.texString6.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(ngsSkin.ngsSkin.texString7Ptr + offset, SeekOrigin.Begin);
                ngsSkin.texString7.SetString(AquaObjectMethods.ReadCString(streamReader));

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
                eyeb.eyeb = streamReader.Read<EYEB>();
                long temp = streamReader.Position();

                streamReader.Seek(eyeb.eyeb.texString1Ptr + offset, SeekOrigin.Begin);
                eyeb.texString1.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(eyeb.eyeb.texString2Ptr + offset, SeekOrigin.Begin);
                eyeb.texString2.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(eyeb.eyeb.texString3Ptr + offset, SeekOrigin.Begin);
                eyeb.texString3.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(eyeb.eyeb.texString4Ptr + offset, SeekOrigin.Begin);
                eyeb.texString4.SetString(AquaObjectMethods.ReadCString(streamReader));

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
                hair.hair = streamReader.Read<HAIR>();
                long temp = streamReader.Position();

                streamReader.Seek(hair.hair.dataStringPtr + offset, SeekOrigin.Begin);
                hair.dataString.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(hair.hair.texString1Ptr + offset, SeekOrigin.Begin);
                hair.texString1.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(hair.hair.texString2Ptr + offset, SeekOrigin.Begin);
                hair.texString2.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(hair.hair.texString3Ptr + offset, SeekOrigin.Begin);
                hair.texString3.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(hair.hair.texString4Ptr + offset, SeekOrigin.Begin);
                hair.texString4.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(hair.hair.texString5Ptr + offset, SeekOrigin.Begin);
                hair.texString5.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(hair.hair.texString6Ptr + offset, SeekOrigin.Begin);
                hair.texString6.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(hair.hair.texString7Ptr + offset, SeekOrigin.Begin);
                hair.texString7.SetString(AquaObjectMethods.ReadCString(streamReader));

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
                col.niflCol = streamReader.Read<NIFL_COL>();
                long temp = streamReader.Position();

                streamReader.Seek(col.niflCol.textStringPtr + offset, SeekOrigin.Begin);
                col.textString.SetString(AquaObjectMethods.ReadCString(streamReader));

                streamReader.Seek(temp, SeekOrigin.Begin);

                dict.Add(col.niflCol.id, col); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadIndexLinks(BufferedStreamReader streamReader, int offset, int baseAddress, int count, Dictionary<int, BCLN> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                var bcln = streamReader.Read<BCLN>();
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

        //Takes in pso2_bin directory and outputDirectory. From there, it will read to memory various files in order to determine namings. 
        //As win32_na is a patching folder, if it exists in the pso2_bin it will be prioritized for text related items.
        public static void OutputCharacterFileList(string pso2_binDir, string outputDirectory)
        {
            var aquaCMX = new CharacterMakingIndex();
            PSO2Text partsText = null;
            PSO2Text acceText = null;
            PSO2Text commonText = null;
            PSO2Text commonTextReboot = null;
            LobbyActionCommon lac = null;
            Dictionary<int, string> faceIds = new Dictionary<int, string>();

            aquaCMX = ExtractCMX(pso2_binDir, aquaCMX);

            ReadCMXText(pso2_binDir, out partsText, out acceText, out commonText, out commonTextReboot);

            faceIds = GetFaceVariationLuaNameDict(pso2_binDir, faceIds);

            //Load lac
            string lacPath = Path.Combine(pso2_binDir, dataDir, GetFileHash(classicLobbyAction));
            string lacPathRe = Path.Combine(pso2_binDir, dataReboot, GetFileHash(rebootLobbyAction));
            string lacTruePath = null;
            if (File.Exists(lacPath))
            {
                lacTruePath = lacPath;
            }
            else if (File.Exists(lacPath))
            {
                //lacTruePath = lacPathRe;
            }
            if (lacTruePath != null)
            {

                var strm = new MemoryStream(File.ReadAllBytes(lacTruePath));
                var fVarIce = IceFile.LoadIceFile(strm);
                strm.Dispose();

                List<byte[]> files = (new List<byte[]>(fVarIce.groupOneFiles));
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

            //Since we have an idea of what should be there and what we're interested in parsing out, throw these into a dictionary and go
            Dictionary<string, List<List<PSO2Text.textPair>>> textByCat = new Dictionary<string, List<List<PSO2Text.textPair>>>();
            Dictionary<string, List<List<PSO2Text.textPair>>> commByCat = new Dictionary<string, List<List<PSO2Text.textPair>>>();
            Dictionary<string, List<List<PSO2Text.textPair>>> commRebootByCat = new Dictionary<string, List<List<PSO2Text.textPair>>>();
            for (int i = 0; i < partsText.text.Count; i++)
            {
                textByCat.Add(partsText.categoryNames[i], partsText.text[i]);
            }
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
            for (int i = 0; i < commonText.text.Count; i++)
            {
                commByCat.Add(commonText.categoryNames[i], commonText.text[i]);
            }
            if(commonTextReboot != null)
            {
                for (int i = 0; i < commonTextReboot.text.Count; i++)
                {
                    commRebootByCat.Add(commonTextReboot.categoryNames[i], commonTextReboot.text[i]);
                }
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
            StringBuilder outputUnknownWearables = new StringBuilder();

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
                output += $"{id},";

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
                else if (aquaCMX.outerWearIdLink.ContainsKey(id))
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
                    string reb = $"{rebootStart}{typeString}{adjustedId}.ice";
                    string rebEx = $"{rebootExStart}{typeString}{adjustedId}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    output += "," + GetCostumeOuterIconString(pso2_binDir, id.ToString());

                    output += "\n";
                    output = AddBodyExtraFiles(output, reb, pso2_binDir, "_" + typeString, false);


                    output += ",[HQ Texture Ice]," + rebExHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, rebExHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";
                    output = AddBodyExtraFiles(output, rebEx, pso2_binDir, "_" + typeString, false);

                }
                else
                {
                    string finalId = ToFive(adjustedId);
                    string finalIdIcon = ToFive(id);
                    string classic = $"{classicStart}{typeString}{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    output += "," + GetCostumeOuterIconString(pso2_binDir, finalIdIcon);

                    output += "\n";
                    output = AddBodyExtraFiles(output, classic, pso2_binDir, "_" + typeString, true);
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
                    outputUnknownWearables.Append(output);
                }
            }
            WriteCSV(outputDirectory, "MaleCostumes.csv", outputCostumeMale);
            WriteCSV(outputDirectory, "FemaleCostumes.csv", outputCostumeFemale);
            WriteCSV(outputDirectory, "MaleOuters.csv", outputOuterMale); 
            WriteCSV(outputDirectory, "FemaleOuters.csv", outputOuterFemale); 
            WriteCSV(outputDirectory, "CastBodies.csv", outputCastBody);
            WriteCSV(outputDirectory, "CasealBodies.csv", outputCasealBody); 
            WriteCSV(outputDirectory, "MaleNGSOuters.csv", outputNGSOuterMale);
            WriteCSV(outputDirectory, "FemaleNGSOuters.csv", outputNGSOuterFemale); 
            WriteCSV(outputDirectory, "CastNGSBodies.csv", outputNGSCastBody);
            WriteCSV(outputDirectory, "CasealNGSBodies.csv", outputNGSCasealBody);
            //WriteCSV(outputDirectory, "MaleNGSCostumes.csv", outputNGSCostumeMale);
            //WriteCSV(outputDirectory, "FemaleNGSCostumes.csv", outputNGSCostumeFemale);
            if (outputUnknownWearables.Length > 0)
            {
                WriteCSV(outputDirectory, "UnknownOutfits.csv", outputUnknownWearables);
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
                output += $"{id},";

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
                    string reb = $"{rebootStart}bw_{adjustedId}.ice";
                    string rebEx = $"{rebootExStart}bw_{adjustedId}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    var iconStr = GetBasewearIconString(id.ToString());
                    output += "," + iconStr;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";
                    output = AddBasewearExtraFiles(output, reb, pso2_binDir, false);

                    output += ",[HQ Texture Ice]," + rebExHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, rebExHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";
                    output = AddBasewearExtraFiles(output, rebEx, pso2_binDir, false);

                }
                else
                {
                    string finalId = ToFive(adjustedId);
                    string finalIdIcon = ToFive(id);
                    string classic = $"{classicStart}bw_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    var iconStr = GetBasewearIconString(finalIdIcon);
                    output += "," + iconStr;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";
                    output = AddBasewearExtraFiles(output, classic, pso2_binDir, true);

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
            WriteCSV(outputDirectory, "MaleBasewear.csv", outputBasewearMale);
            WriteCSV(outputDirectory, "FemaleBasewear.csv", outputBasewearFemale);
            WriteCSV(outputDirectory, "MaleNGSBasewear.csv", outputNGSBasewearMale);
            WriteCSV(outputDirectory, "FemaleNGSBasewear.csv", outputNGSBasewearFemale);
            WriteCSV(outputDirectory, "GenderlessNGSBasewear.csv", outputNGSGenderlessBasewear);

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
                output += $"{id},";

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
                    string reb = $"{rebootStart}iw_{adjustedId}.ice";
                    string rebEx = $"{rebootExStart}iw_{adjustedId}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }
                    string iconStr = GetInnerwearIconString(id.ToString());
                    output += "," + iconStr;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                    output += ",[HQ Texture Ice]," + rebExHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, rebExHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }
                else
                {
                    string finalId = ToFive(adjustedId);
                    string finalIdIcon = ToFive(id);
                    string classic = $"{classicStart}iw_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    var iconStr = GetInnerwearIconString(finalIdIcon);
                    output += "," + iconStr;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
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
            WriteCSV(outputDirectory, "MaleInnerwear.csv", outputInnerwearMale);
            WriteCSV(outputDirectory, "FemaleInnerwear.csv", outputInnerwearFemale);
            WriteCSV(outputDirectory, "MaleNGSInnerwear.csv", outputNGSInnerwearMale);
            WriteCSV(outputDirectory, "FemaleNGSInnerwear.csv", outputNGSInnerwearFemale);

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
                output += $"{id},";

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
                    string reb = $"{rebootStart}am_{adjustedId}.ice";
                    string rebEx = $"{rebootExStart}am_{adjustedId}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }
                    string iconStr = GetCastArmIconString(id.ToString());
                    output += "," + iconStr;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                    output += ",[HQ Texture Ice]," + rebExHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, rebExHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }
                else
                {
                    string finalId = ToFive(adjustedId);
                    string finalIdIcon = ToFive(id);
                    string classic = $"{classicStart}am_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    var iconStr = GetCastArmIconString(finalIdIcon);
                    output += "," + iconStr;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
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
            WriteCSV(outputDirectory, "CastArms.csv", outputCastArmMale);
            WriteCSV(outputDirectory, "CasealArms.csv", outputCastArmFemale);
            WriteCSV(outputDirectory, "NGSCastArms.csv", outputNGSCastArmMale);
            WriteCSV(outputDirectory, "NGSCasealArms.csv", outputNGSCastArmFemale);

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
                output += $"{id},";

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
                    string reb = $"{rebootStart}lg_{adjustedId}.ice";
                    string rebEx = $"{rebootExStart}lg_{adjustedId}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }
                    string iconStr = GetCastLegIconString(id.ToString());
                    output += "," + iconStr;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                    output += ",[HQ Texture Ice]," + rebExHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, rebExHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }
                else
                {
                    string finalId = ToFive(adjustedId);
                    string finalIdIcon = ToFive(id);
                    string classic = $"{classicStart}lg_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    var iconStr = GetCastLegIconString(finalIdIcon);
                    output += "," + iconStr;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
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
            WriteCSV(outputDirectory, "CastLegs.csv", outputCastLegMale);
            WriteCSV(outputDirectory, "CasealLegs.csv", outputCastLegFemale);
            WriteCSV(outputDirectory, "NGSCastLegs.csv", outputNGSCastLegMale);
            WriteCSV(outputDirectory, "NGSCasealLegs.csv", outputNGSCastLegFemale);

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
                output += $"{id},";

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    output = $"[Unnamed {id}]" + output;
                }

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{rebootStart}b1_{id}.ice";
                    string rebEx = $"{rebootExStart}b1_{id}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + bodyPaintIcon + id + ".ice");
                    output += "," + iconStr;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                    output += ",[HQ Texture Ice]," + rebExHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, rebExHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }
                else
                {
                    string finalId = ToFive(id);
                    string classic = $"{classicStart}b1_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + bodyPaintIcon + finalId + ".ice");
                    output += "," + iconStr;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
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
            WriteCSV(outputDirectory, "MaleBodyPaint.csv", outputMaleBodyPaint);
            WriteCSV(outputDirectory, "FemaleBodyPaint.csv", outputFemaleBodyPaint);
            WriteCSV(outputDirectory, "MaleLayeredBodyPaint.csv", outputMaleLayeredBodyPaint);
            WriteCSV(outputDirectory, "FemaleLayeredBodyPaint.csv", outputFemaleLayeredBodyPaint);
            WriteCSV(outputDirectory, "NGSMaleBodyPaint.csv", outputNGSMaleBodyPaint);
            WriteCSV(outputDirectory, "NGSFemaleBodyPaint.csv", outputNGSFemaleBodyPaint);
            WriteCSV(outputDirectory, "NGSCastBodyPaint.csv", outputNGSCastMaleBodyPaint);
            WriteCSV(outputDirectory, "NGSCasealBodyPaint.csv", outputNGSCastFemaleBodyPaint);
            WriteCSV(outputDirectory, "NGSGenderlessBodyPaint.csv", outputNGSGenderlessBodyPaint);

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
                output += $"{id},";

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    output = $"[Unnamed {id}]" + output;
                }

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{rebootStart}b2_{id}.ice";
                    string rebEx = $"{rebootExStart}b2_{id}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                    output += ",[HQ Texture Ice]," + rebExHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, rebExHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + stickerIcon + id + ".ice");
                    output += "," + iconStr;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }
                else
                {
                    string finalId = ToFive(id);
                    string classic = $"{classicStart}b2_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + stickerIcon + finalId + ".ice");
                    output += "," + iconStr;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }

                outputStickers.Append(output);
            }
            WriteCSV(outputDirectory, "Stickers.csv", outputStickers);

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
                output += $"{id},";

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    output = $"[Unnamed {id}]" + output;
                }

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{rebootStart}hr_{id}.ice";
                    string rebEx = $"{rebootExStart}hr_{id}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    var maleIconStr = GetFileHash(icon + hairIcon + iconMale + id + ".ice");
                    var femaleIconStr = GetFileHash(icon + hairIcon + iconFemale + id + ".ice");
                    var castIconStr = GetFileHash(icon + hairIcon + iconCast + id + ".ice");
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, maleIconStr)))
                    {
                        output += "," + maleIconStr;
                    }
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, femaleIconStr)))
                    {
                        output += "," + femaleIconStr;
                    }
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, castIconStr)))
                    {
                        output += "," + castIconStr;
                    }

                    output += "\n";
                }
                else
                {
                    string finalId = ToFive(id);
                    string classic = $"{classicStart}hr_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    var maleIconStr = GetFileHash(icon + hairIcon + iconMale + finalId + ".ice");
                    var femaleIconStr = GetFileHash(icon + hairIcon + iconFemale + finalId + ".ice");
                    var castIconStr = GetFileHash(icon + hairIcon + iconCast + finalId + ".ice");
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, maleIconStr)))
                    {
                        output += "," + maleIconStr;
                    }
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, femaleIconStr)))
                    {
                        output += "," + femaleIconStr;
                    }
                    if (File.Exists(Path.Combine(pso2_binDir, dataDir, castIconStr)))
                    {
                        output += "," + castIconStr;
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
            WriteCSV(outputDirectory, "MaleHair.csv", outputMaleHair);
            WriteCSV(outputDirectory, "FemaleHair.csv", outputFemaleHair);
            WriteCSV(outputDirectory, "CasealHair.csv", outputCasealHair);
            WriteCSV(outputDirectory, "NGSHair.csv", outputNGSHair);

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
                output += $"{id},";

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    output = $"[Unnamed {id}]" + output;
                }

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{rebootStart}ey_{id}.ice";
                    string rebEx = $"{rebootExStart}ey_{id}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + eyeIcon + id + ".ice");
                    output += "," + iconStr;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";
                }
                else
                {
                    string finalId = ToFive(id);
                    string classic = $"{classicStart}ey_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + eyeIcon + finalId + ".ice");
                    output += "," + iconStr;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }

                if (id <= 100000)
                {
                    outputEyes.Append(output);
                }
                else
                {
                    outputNGSEyes.Append(output);
                }
            }
            WriteCSV(outputDirectory, "Eyes.csv", outputEyes);
            WriteCSV(outputDirectory, "NGSEyes.csv", outputNGSEyes);

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
                output += $"{id},";

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    output = $"[Unnamed {id}]" + output;
                }

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{rebootStart}eb_{id}.ice";
                    string rebEx = $"{rebootExStart}eb_{id}_ex.ice";
                    string rebHash = GetFileHash(reb);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + eyebrowsIcon + id + ".ice");
                    output += "," + iconStr;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";
                }
                else
                {
                    string finalId = ToFive(id);
                    string classic = $"{classicStart}eb_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + eyebrowsIcon + finalId + ".ice");
                    output += "," + iconStr;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }

                if (id <= 100000)
                {
                    outputEyebrows.Append(output);
                }
                else
                {
                    outputNGSEyebrows.Append(output);
                }
            }
            WriteCSV(outputDirectory, "Eyebrows.csv", outputEyebrows);
            WriteCSV(outputDirectory, "NGSEyebrows.csv", outputNGSEyebrows);

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
                output += $"{id},";

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    output = $"[Unnamed {id}]" + output;
                }

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{rebootStart}el_{id}.ice";
                    string rebEx = $"{rebootExStart}el_{id}_ex.ice";
                    string rebHash = GetFileHash(reb);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + eyelashesIcon + id + ".ice");
                    output += "," + iconStr;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";
                }
                else
                {
                    string finalId = ToFive(id);
                    string classic = $"{classicStart}el_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + eyelashesIcon + finalId + ".ice");
                    output += "," + iconStr;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }

                if (id <= 100000)
                {
                    outputEyelashes.Append(output);
                }
                else
                {
                    outputNGSEyelashes.Append(output);
                }
            }
            WriteCSV(outputDirectory, "Eyelashes.csv", outputEyelashes);
            WriteCSV(outputDirectory, "NGSEyelashes.csv", outputNGSEyelashes);

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
                output += $"{id},";

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    output = $"[Unnamed {id}]" + output;
                }

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{rebootStart}ac_{id}.ice";
                    string rebEx = $"{rebootExStart}ac_{id}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + accessoryIcon + id + ".ice");
                    output += "," + iconStr;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        output += ", (Not found)";
                    }
                }
                else
                {
                    string finalId = ToFive(id);
                    string classic = $"{classicStart}ac_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + accessoryIcon + finalId + ".ice");
                    output += "," + iconStr;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        output += ", (Not found)";
                    }
                }

                output += "\n";

                //Add linked character nodes
                if (aquaCMX.accessoryDict.ContainsKey(id))
                {
                    var acce = aquaCMX.accessoryDict[id];

                    output += $",{acce.nodeAttach1.GetString()},{acce.nodeAttach2.GetString()},{acce.nodeAttach3.GetString()},{acce.nodeAttach4.GetString()},{acce.nodeAttach5.GetString()}," +
                        $"{acce.nodeAttach6.GetString()},{acce.nodeAttach7.GetString()},{acce.nodeAttach8.GetString()}";
                }

                output += "\n";

                outputAccessories.Append(output);
            }
            WriteCSV(outputDirectory, "Accessories.csv", outputAccessories);

            //---------------------------Parse out skin
            StringBuilder outputSkin = new StringBuilder();
            StringBuilder outputNGSSkin = new StringBuilder();

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "skin", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.ngsSkinDict.Keys);

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
                output += $"{id},";

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    output = $"[Unnamed {id}]" + output;
                }

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{rebootStart}sk_{id}.ice";
                    string rebEx = $"{rebootExStart}sk_{id}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                    output += ",[HQ Texture Ice]," + rebExHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, rebExHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }
                else
                {
                    string finalId = ToFive(id);
                    string classic = $"{classicStart}sk_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }
                    output += "\n";

                }

                if (id < 100000)
                {
                    outputSkin.Append(output);
                }
                else
                {
                    outputNGSSkin.Append(output);
                }
            }
            WriteCSV(outputDirectory, "Skins.csv", outputSkin);
            WriteCSV(outputDirectory, "NGSSkins.csv", outputNGSSkin);

            //---------------------------Parse out FCP1, Face Textures
            StringBuilder outputFCP1 = new StringBuilder();
            StringBuilder outputNGSFCP1 = new StringBuilder();

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "facepaint1", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.fcpDict.Keys);

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
                output += $"{id},";

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    output = $"[Unnamed {id}]" + output;
                }

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{rebootStart}f1_{id}.ice";
                    string rebEx = $"{rebootExStart}f1_{id}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + faceIcon + id + ".ice");
                    output += "," + iconStr;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                    output += ",[HQ Texture Ice]," + rebExHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, rebExHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }
                else
                {
                    string finalId = ToFive(id);
                    string classic = $"{classicStart}f1_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + faceIcon + finalId + ".ice");
                    output += "," + iconStr;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }
                if (id <= 100000)
                {
                    outputFCP1.Append(output);
                }
                else
                {
                    outputNGSFCP1.Append(output);
                }
            }
            WriteCSV(outputDirectory, "FaceTextures.csv", outputFCP1);
            if (outputNGSFCP1.Length > 0)
            {
                WriteCSV(outputDirectory, "NGSFaceTextures.csv", outputNGSFCP1);
            }

            //---------------------------Parse out FCP2
            StringBuilder outputFCP2 = new StringBuilder();
            StringBuilder outputNGSFCP2 = new StringBuilder();

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "facepaint2", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.fcpDict.Keys);

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
                output += $"{id},";

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    output = $"[Unnamed {id}]" + output;
                }

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{rebootStart}f2_{id}.ice";
                    string rebEx = $"{rebootExStart}f2_{id}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + facepainticon + id + ".ice");
                    output += "," + iconStr;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                    output += ",[HQ Texture Ice]," + rebExHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, rebExHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }
                else
                {
                    string finalId = ToFive(id);
                    string classic = $"{classicStart}f2_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + facepainticon + finalId + ".ice");
                    output += "," + iconStr;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }

                if (id <= 100000)
                {
                    outputFCP2.Append(output);
                }
                else
                {
                    outputNGSFCP2.Append(output);
                }
            }
            WriteCSV(outputDirectory, "FacePaint.csv", outputFCP2);
            if (outputNGSFCP2.Length > 0)
            {
                WriteCSV(outputDirectory, "NGSFacePaint.csv", outputNGSFCP2);
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

            masterIdList.Clear();
            nameDicts.Clear();

            List<string> masterNameList = new List<string>();
            List<Dictionary<string, string>> strNameDicts = new List<Dictionary<string, string>>();
            GatherTextIdsStringRef(textByCat, masterNameList, strNameDicts, "facevariation", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.faceDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                string output = "";
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

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{rebootStart}fc_{id}.ice";
                    string rebEx = $"{rebootExStart}fc_{id}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                    output += ",[HQ Texture Ice]," + rebExHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, rebExHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }
                else
                {
                    string finalId = ToFive(id);
                    string classic = $"{classicStart}fc_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }

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
            WriteCSV(outputDirectory, "MaleHumanFaces.csv", outputHumanMaleFace);
            WriteCSV(outputDirectory, "FemaleHumanFaces.csv", outputHumanFemaleFace);
            WriteCSV(outputDirectory, "MaleNewmanFaces.csv", outputNewmanMaleFace);
            WriteCSV(outputDirectory, "FemaleNewmanFaces.csv", outputNewmanFemaleFace);
            WriteCSV(outputDirectory, "CastFaces_Heads.csv", outputCastMaleFace);
            WriteCSV(outputDirectory, "CasealFaces_Heads.csv", outputCastFemaleFace);
            WriteCSV(outputDirectory, "MaleDeumanFaces.csv", outputDewmanMaleFace);
            WriteCSV(outputDirectory, "FemaleDeumanFaces.csv", outputDewmanFemaleFace);
            WriteCSV(outputDirectory, "NGSFaces.csv", outputNGSFace);

            //---------------------------------------------------------------------------------------//End CMX related ids

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
            WriteCSV(outputDirectory, "MaleVoices.csv", outputMaleVoices);
            WriteCSV(outputDirectory, "FemaleVoices.csv", outputFemaleVoices);
            WriteCSV(outputDirectory, "CastVoices.csv", outputCastVoices);
            WriteCSV(outputDirectory, "CasealVoices.csv", outputCasealVoices);

            //---------------------------Parse out Lobby Action files -- in lobby_action_setting.lac within defaa92bd5435c84af0da0302544b811 and common.text in a1d84c3c748cebdb6fc42f66b73d2e57
            if (lacTruePath != null)
            {
                StringBuilder lobbyActions = new StringBuilder();
                strNameDicts.Clear();
                masterNameList.Clear();
                List<string> iceTracker = new List<string>();
                GatherTextIdsStringRef(commByCat, masterNameList, strNameDicts, "LobbyAction", true);

                lobbyActions.AppendLine("Files are layed out as: PSO2File NGSfile NGSCastFile NGSCasealFile NGSFigFile");
                lobbyActions.AppendLine("NGS Lobby Actions are in win32reboot, unlike most NGS player files");
                lobbyActions.AppendLine("");

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

                    if (lac.dataBlocks[i].iceName.Contains("_m.ice"))
                    {
                        output += (", Male");
                    }
                    else if (lac.dataBlocks[i].iceName.Contains("_f.ice"))
                    {
                        output += (", Female");
                    }

                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                    lobbyActions.Append(output);
                }

                WriteCSV(outputDirectory, "LobbyActions.csv", lobbyActions);
            }
            //---------------------------Parse out NGS ears //The cmx has ear data, but no ids. Maybe it's done by order? Same for teeth and horns
            StringBuilder outputNGSEars = new StringBuilder();

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "ears", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.ngsEarDict.Keys);

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
                output += $"{id},";

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    output = $"[Unnamed {id}]" + output;
                }

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{rebootStart}ea_{id}.ice";
                    string rebEx = $"{rebootExStart}ea_{id}_ex.ice";
                    string rebHash = GetFileHash(reb);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + earIcon + id + ".ice");
                    output += "," + iconStr;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }
                else
                {
                    string finalId = ToFive(id);
                    string classic = $"{classicStart}ea_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + earIcon + finalId + ".ice");
                    output += "," + iconStr;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }

                outputNGSEars.Append(output);

            }
            WriteCSV(outputDirectory, "NGSEars.csv", outputNGSEars);

            //---------------------------Parse out NGS teeth 
            StringBuilder outputNGSTeeth = new StringBuilder();

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "dental", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.ngsTeethDict.Keys);

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
                output += $"{id},";

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    output = $"[Unnamed {id}]" + output;
                }

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{rebootStart}de_{id}.ice";
                    string rebEx = $"{rebootExStart}de_{id}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + teethIcon + id + ".ice");
                    output += "," + iconStr;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";
                }
                else
                {
                    string finalId = ToFive(id);
                    string classic = $"{classicStart}de_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + teethIcon + finalId + ".ice");
                    output += "," + iconStr;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }

                outputNGSTeeth.Append(output);

            }
            WriteCSV(outputDirectory, "NGSTeeth.csv", outputNGSTeeth);

            //---------------------------Parse out NGS horns 
            StringBuilder outputNGSHorns = new StringBuilder();

            masterIdList.Clear();
            nameDicts.Clear();
            GatherTextIds(textByCat, masterIdList, nameDicts, "horn", true);

            //Add potential cmx ids that wouldn't be stored in
            GatherDictKeys(masterIdList, aquaCMX.ngsTeethDict.Keys);

            masterIdList.Sort();

            //Loop through master id list, generate filenames, and link name strings if applicable. Use IDLink dicts in cmx to get proper filenames for colored outfits
            foreach (int id in masterIdList)
            {
                //Skip the なし horn entry. I'm not even sure why that's in there.
                if (id == 0)
                {
                    continue;
                }
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
                output += $"{id},";

                //Account for lack of a name on an outfit
                if (named == false)
                {
                    output = $"[Unnamed {id}]" + output;
                }

                //Decide if it needs to be handled as a reboot file or not
                if (id >= 100000)
                {
                    string reb = $"{rebootStart}hn_{id}.ice";
                    string rebEx = $"{rebootExStart}hn_{id}_ex.ice";
                    string rebHash = GetFileHash(reb);
                    string rebExHash = GetFileHash(rebEx);

                    output += rebHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, rebHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + hornIcon + id + ".ice");
                    output += "," + iconStr;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";
                }
                else
                {
                    string finalId = ToFive(id);
                    string classic = $"{classicStart}hn_{finalId}.ice";

                    var classicHash = GetFileHash(classic);

                    output += classicHash;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, classicHash)))
                    {
                        output += ", (Not found)";
                    }
                    //Set icon string
                    var iconStr = GetFileHash(icon + hornIcon + finalId + ".ice");
                    output += "," + iconStr;
                    if (!File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
                    {
                        output += ", (Not found)";
                    }

                    output += "\n";

                }

                outputNGSHorns.Append(output);

            }
            WriteCSV(outputDirectory, "NGSHorns.csv", outputNGSHorns);

            //---------------------------Get Substitute Motion files -- 
            if (commonTextReboot != null)
            {
                List<string> subCatList = new List<string>() { subSwim, subGlide, subJump, subLanding, subMove, subSprint, subIdle };
                List<StringBuilder> subMotions = new List<StringBuilder>();
                for(int i = 0; i < subCatList.Count; i++)
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
                        humanHash = GetFileHash(humanHash);
                        if (!File.Exists(Path.Combine(pso2_binDir, dataReboot, GetRebootHash(humanHash))))
                        {
                            continue;
                        }

                        //These should all exist if humanHash does
                        string castHash = GetFileHash(humanHash.Replace($"{rebootLAHuman}.ice", rebootLACastMale + ".ice"));
                        string casealHash = GetFileHash(humanHash.Replace($"{rebootLAHuman}.ice", rebootLACastFemale + ".ice"));
                        string figHash = GetFileHash(humanHash.Replace($"{rebootLAHuman}.ice", rebootFig + ".ice"));

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

                        output += ToThree(i) + ",";
                        output += ", " + humanHash;
                        output += ", " + castHash;
                        output += ", " + casealHash;
                        output += ", " + figHash;

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

                    WriteCSV(outputDirectory, $"SubstituteMotion{sub}.csv", subMotions[cat]);
                }

            }
        }

        public static string GetCastLegIconString(string id)
        {
            //Set icon string
            return GetFileHash(icon + castPartIcon + castLegIcon + id + ".ice");
        }

        public static string GetCastArmIconString(string id)
        {
            //Set icon string
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

        public static string GetCostumeOuterIconString(string pso2_binDir, string finalId)
        {
            var iconStr = GetFileHash(icon + costumeIcon + finalId + ".ice");
            var iconStr2 = GetFileHash(icon + castPartIcon + finalId + ".ice");
            if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr)))
            {
                return iconStr;
            }
            else if (File.Exists(Path.Combine(pso2_binDir, dataDir, iconStr2)))
            {
                return iconStr2;
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
            if(Directory.Exists(Path.Combine(pso2_binDir, dataReboot)))
            {
                string commonTextRebootPath = Path.Combine(pso2_binDir, dataReboot, GetRebootHash(GetFileHash(classicCommon)));
                string commonTextRebootPathhNA = Path.Combine(pso2_binDir, dataRebootNA, GetRebootHash(GetFileHash(classicCommon)));

                commonTextReboot = GetTextConditional(commonTextRebootPath, commonTextRebootPathhNA, commonTextName);
            } else
            {
                commonTextReboot = null;
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

        private static string GetIconGender(int id)
        {
            //NGS ids
            if(id >= 100000)
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

            } else //Classic ids
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
                File.WriteAllText(Path.Combine(outputDirectory, fileName), output.ToString());
            }
            if(nullSB == true)
            {
                output = null;
            }
        }

        private static string AddBodyExtraFiles(string output, string fname, string pso2_binDir, string typeString, bool isClassic)
        {
            string rpCheck = GetFileHash(fname.Replace(".ice", "_rp.ice"));
            string bmCheck = GetFileHash(fname.Replace(typeString, "_bm_"));
            string hnCheck = GetFileHash(fname.Replace(typeString, "_hn_")); //If not basewear, hn. If basewear, ho

            //_rp alt model
            if (File.Exists(Path.Combine(pso2_binDir, dataDir, rpCheck)))
            {
                output += ",[Alt Model]," + rpCheck + "\n";
            }
            //Aqv archive
            if (File.Exists(Path.Combine(pso2_binDir, dataDir, bmCheck)))
            {
                output += ",[Aqv]," + bmCheck + "\n";
            }

            //NGS doesn't have these sorts of files
            if (isClassic)
            {
                //Hand textures
                if (File.Exists(Path.Combine(pso2_binDir, dataDir, hnCheck)))
                {
                    output += ",[Hand Textures]," + hnCheck + "\n";
                }
            }

            return output;
        }

        private static string AddBasewearExtraFiles(string output, string fname, string pso2_binDir, bool isClassic)
        {
            string rpCheck = GetFileHash(fname.Replace(".ice", "_rp.ice"));
            string hnCheck = GetFileHash(fname.Replace("bw", "ho")); //If not basewear, hn. If basewear, ho

            //_rp alt model
            if (File.Exists(Path.Combine(pso2_binDir, dataDir, rpCheck)))
            {
                output += ",[Alt Model]," + rpCheck + "\n";
            }

            //NGS doesn't have these sorts of files
            if(isClassic)
            {
                //Hand textures
                if (File.Exists(Path.Combine(pso2_binDir, dataDir, hnCheck)))
                {
                    output += ",[Hand Textures]," + hnCheck + "\n";
                }
            }

            return output;
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
            if(textByCat.ContainsKey(category))
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

            } else
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
                    if(!masterNameList.Contains(pair.name))
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

            for(int sub = 0; sub < textByCat["Substitute"].Count; sub++)
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

        public static string ToFive(int num)
        {
            string numString = num.ToString();
            while (numString.Length < 5)
            {
                numString = numString.Insert(0, "0");
            }

            return numString;
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

        public static string GetFileHash(string str)
        {
            if (str == null)
            {
                return "";
            }
            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(new UTF8Encoding().GetBytes(str));
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
        }

        public static string GetFileDataHash(string fileName)
        {
            if (fileName == null)
            {
                return "";
            }
            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(File.ReadAllBytes(fileName));
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
        }

        public static string GetRebootHash(string fileName)
        {
            return fileName.Substring(0, 2) + "\\" + fileName.Substring(2, fileName.Length - 2);
        }
    }
}
