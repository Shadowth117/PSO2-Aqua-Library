using AquaModelLibrary.Data.PSO2.Aqua.AquaCommonData;
using AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData;
using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.PSO2;
using System.Text;
using System.IO;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    //Though the NIFL format is used for storage, VTBF format tag references for data will be commented where appropriate. Some offset/reserve related things are NIFL only, however.
    public unsafe class CharacterMakingIndex : AquaCommon
    {
        public static bool pcDirectory = true;

        public static string dataDir
        {
            get { return pcDirectory ? dataDirPC : dataDirConsole; }
        }
        public static string dataNADir
        {
            get { return pcDirectory ? dataNADirPC : dataNADirConsole; }
        }
        public static string dataReboot
        {
            get { return pcDirectory ? dataRebootPC : dataRebootConsole; }
        }
        public static string dataRebootNA
        {
            get { return pcDirectory ? dataRebootNAPC : dataRebootNAConsole; }
        }

        public static string dataDirPC = $"data\\win32\\";
        public static string dataDirConsole = $"data\\";
        public static string dataNADirPC = $"data\\win32_na\\";
        public static string dataNADirConsole = $"data_na\\";
        public static string dataRebootPC = $"data\\win32reboot\\";
        public static string dataRebootConsole = $"datareboot\\";
        public static string dataRebootNAPC = $"data\\win32reboot_na\\";
        public static string dataRebootNAConsole = $"datareboot_na\\";

        public Dictionary<int, BODYObject> costumeDict = new Dictionary<int, BODYObject>();
        public Dictionary<int, BODYObject> carmDict = new Dictionary<int, BODYObject>();
        public Dictionary<int, BODYObject> clegDict = new Dictionary<int, BODYObject>();
        public Dictionary<int, BODYObject> outerDict = new Dictionary<int, BODYObject>();

        public Dictionary<int, BODYObject> baseWearDict = new Dictionary<int, BODYObject>();
        public Dictionary<int, BBLYObject> innerWearDict = new Dictionary<int, BBLYObject>();
        public Dictionary<int, BBLYObject> bodyPaintDict = new Dictionary<int, BBLYObject>();
        public Dictionary<int, StickerObject> stickerDict = new Dictionary<int, StickerObject>();

        public Dictionary<int, FACEObject> faceDict = new Dictionary<int, FACEObject>();
        public Dictionary<int, FCMNObject> fcmnDict = new Dictionary<int, FCMNObject>();
        public Dictionary<int, FaceTextureObject> faceTextureDict = new Dictionary<int, FaceTextureObject>();
        public Dictionary<int, FCPObject> fcpDict = new Dictionary<int, FCPObject>();

        public Dictionary<int, ACCEObject> accessoryDict = new Dictionary<int, ACCEObject>();
        public Dictionary<int, EYEObject> eyeDict = new Dictionary<int, EYEObject>();
        public Dictionary<int, NGS_EarObject> ngsEarDict = new Dictionary<int, NGS_EarObject>();
        public Dictionary<int, NGS_TeethObject> ngsTeethDict = new Dictionary<int, NGS_TeethObject>();

        public Dictionary<int, NGS_HornObject> ngsHornDict = new Dictionary<int, NGS_HornObject>();
        public Dictionary<int, NGS_SKINObject> ngsSkinDict = new Dictionary<int, NGS_SKINObject>();
        public Dictionary<int, EYEBObject> eyebrowDict = new Dictionary<int, EYEBObject>();
        public Dictionary<int, EYEBObject> eyelashDict = new Dictionary<int, EYEBObject>();

        public Dictionary<int, HAIRObject> hairDict = new Dictionary<int, HAIRObject>();
        public Dictionary<int, NIFL_COLObject> colDict = new Dictionary<int, NIFL_COLObject>();
        public Dictionary<int, VTBF_COLObject> legacyColDict = new Dictionary<int, VTBF_COLObject>();

        public List<Unk_IntField> unkList = new List<Unk_IntField>();
        public Dictionary<int, BCLNObject> costumeIdLink = new Dictionary<int, BCLNObject>();

        public Dictionary<int, BCLNObject> castArmIdLink = new Dictionary<int, BCLNObject>();
        public Dictionary<int, BCLNObject> clegIdLink = new Dictionary<int, BCLNObject>();
        public Dictionary<int, BCLNObject> outerWearIdLink = new Dictionary<int, BCLNObject>();
        public Dictionary<int, BCLNObject> baseWearIdLink = new Dictionary<int, BCLNObject>();

        public Dictionary<int, BCLNObject> innerWearIdLink = new Dictionary<int, BCLNObject>();
        public Dictionary<int, BCLNObject> castHeadIdLink = new Dictionary<int, BCLNObject>();
        public Dictionary<int, BCLNObject> accessoryIdLink = new Dictionary<int, BCLNObject>();

        public Dictionary<int, Part6_7_22Obj> part6_7_22Dict = new Dictionary<int, Part6_7_22Obj>();

        public CMXTable cmxTable = null;

        /// <summary>
        /// Mode 1 will be latest. Mode 0 will be the first NGS benchmark's.
        /// </summary>
        public int WriteMode = 1;
        public override string[] GetEnvelopeTypes()
        {
            return new string[] {
            "cmx\0"
            };
        }
        public CharacterMakingIndex() { }

        public CharacterMakingIndex(byte[] file, string _ext)
        {
            Read(file, _ext);
        }

        public CharacterMakingIndex(BufferedStreamReaderBE<MemoryStream> streamReader, string _ext)
        {
            Read(streamReader, _ext);
        }

        public CharacterMakingIndex(byte[] file) : base(file) { }

        public CharacterMakingIndex(BufferedStreamReaderBE<MemoryStream> streamReader) : base(streamReader) { }

        #region ReadNIFL
        public override void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> streamReader, int offset)
        {
            streamReader.Seek(rel0.REL0DataStart + offset, SeekOrigin.Begin);
            cmxTable = new CMXTable(streamReader, rel0.REL0DataStart);

            ReadBODY(streamReader, offset, cmxTable.bodyAddress, cmxTable.bodyCount, costumeDict, rel0.REL0DataStart);
            ReadBODY(streamReader, offset, cmxTable.carmAddress, cmxTable.carmCount, carmDict, rel0.REL0DataStart);
            ReadBODY(streamReader, offset, cmxTable.clegAddress, cmxTable.clegCount, clegDict, rel0.REL0DataStart);
            ReadBODY(streamReader, offset, cmxTable.bodyOuterAddress, cmxTable.bodyOuterCount, outerDict, rel0.REL0DataStart);

            ReadBODY(streamReader, offset, cmxTable.baseWearAddress, cmxTable.baseWearCount, baseWearDict, rel0.REL0DataStart);
            ReadBBLY(streamReader, offset, cmxTable.innerWearAddress, cmxTable.innerWearCount, innerWearDict);
            ReadBBLY(streamReader, offset, cmxTable.bodyPaintAddress, cmxTable.bodyPaintCount, bodyPaintDict);
            ReadSticker(streamReader, offset, cmxTable.stickerAddress, cmxTable.stickerCount, stickerDict);

            ReadFACE(streamReader, offset, cmxTable.faceAddress, cmxTable.faceCount, faceDict, rel0.REL0DataStart);
            ReadFCMN(streamReader, offset, cmxTable.faceMotionAddress, cmxTable.faceMotionCount, fcmnDict);
            ReadFaceTextures(streamReader, offset, cmxTable.faceTextureAddress, cmxTable.faceTextureCount, faceTextureDict);
            ReadFCP(streamReader, offset, cmxTable.faceTexturesAddress, cmxTable.faceTexturesCount, fcpDict);

            ReadACCE(streamReader, offset, cmxTable.accessoryAddress, cmxTable.accessoryCount, accessoryDict, rel0.REL0DataStart);
            ReadEYE(streamReader, offset, cmxTable.eyeTextureAddress, cmxTable.eyeTextureCount, eyeDict);

            ReadNGSEAR(streamReader, offset, this);
            ReadNGSTEETH(streamReader, offset, this);
            ReadNGSHorn(streamReader, offset, this);

            ReadNGSSKIN(streamReader, offset, cmxTable.skinAddress, cmxTable.skinCount, ngsSkinDict);
            ReadEYEB(streamReader, offset, cmxTable.eyebrowAddress, cmxTable.eyebrowCount, eyebrowDict);
            ReadEYEB(streamReader, offset, cmxTable.eyelashAddress, cmxTable.eyelashCount, eyelashDict);

            ReadHAIR(streamReader, offset, cmxTable.hairAddress, cmxTable.hairCount, hairDict);
            ReadNIFLCOL(streamReader, offset, cmxTable.colAddress, cmxTable.colCount, colDict);

            //Unk
            streamReader.Seek(cmxTable.unkAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < cmxTable.unkCount; i++)
            {
                unkList.Add(streamReader.Read<Unk_IntField>());
            }

            ReadIndexLinks(streamReader, offset, cmxTable.costumeIdLinkAddress, cmxTable.costumeIdLinkCount, costumeIdLink, rel0.REL0DataStart);
            ReadIndexLinks(streamReader, offset, cmxTable.castArmIdLinkAddress, cmxTable.castArmIdLinkCount, castArmIdLink, rel0.REL0DataStart);
            ReadIndexLinks(streamReader, offset, cmxTable.castLegIdLinkAddress, cmxTable.castLegIdLinkCount, clegIdLink, rel0.REL0DataStart);

            //If after a oct 21, the order is changed and we need to read things differently as the addresses get shifted down
            if (cmxTable.oct21UnkAddress != 0)
            {
                ReadIndexLinks(streamReader, offset, cmxTable.outerIdLinkAddress, cmxTable.outerIdLinkCount, castHeadIdLink, rel0.REL0DataStart);
                ReadIndexLinks(streamReader, offset, cmxTable.baseWearIdLinkAddress, cmxTable.baseWearIdLinkCount, outerWearIdLink, rel0.REL0DataStart);
                ReadIndexLinks(streamReader, offset, cmxTable.innerWearIdLinkAddress, cmxTable.innerWearIdLinkCount, baseWearIdLink, rel0.REL0DataStart);
                ReadIndexLinks(streamReader, offset, cmxTable.oct21UnkAddress, cmxTable.oct21UnkCount, innerWearIdLink, rel0.REL0DataStart);
                if (cmxTable.jun7_22Address != 0)
                {
                    ReadPart6_7_22(streamReader, offset, cmxTable.jun7_22Address, cmxTable.jun7_22Count, part6_7_22Dict, rel0.REL0DataStart);
                }
            }
            else
            {
                ReadIndexLinks(streamReader, offset, cmxTable.outerIdLinkAddress, cmxTable.outerIdLinkCount, outerWearIdLink, rel0.REL0DataStart);
                ReadIndexLinks(streamReader, offset, cmxTable.baseWearIdLinkAddress, cmxTable.baseWearIdLinkCount, baseWearIdLink, rel0.REL0DataStart);
                ReadIndexLinks(streamReader, offset, cmxTable.innerWearIdLinkAddress, cmxTable.innerWearIdLinkCount, innerWearIdLink, rel0.REL0DataStart);
            }
            if (cmxTable.feb8_22UnkAddress != 0)
            {
                ReadIndexLinks(streamReader, offset, cmxTable.feb8_22UnkAddress, cmxTable.feb8_22UnkCount, accessoryIdLink, rel0.REL0DataStart);
            }
        }

        private static bool IsValidOffset(int offset)
        {
            return offset > 0x10;
        }

        private static void ReadNGSHorn(BufferedStreamReaderBE<MemoryStream> streamReader, int offset, CharacterMakingIndex cmx)
        {
            //NGS Horns
            streamReader.Seek(cmx.cmxTable.hornAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < cmx.cmxTable.hornCount; i++)
            {
                var ngsHornObj = new NGS_HornObject();
                ngsHornObj.num = i;
                ngsHornObj.originalOffset = streamReader.Position;
                ngsHornObj.ngsHorn = streamReader.Read<NGS_Horn>();
                long bookmark = streamReader.Position;

                streamReader.Seek(ngsHornObj.ngsHorn.dataStringPtr + offset, SeekOrigin.Begin);
                ngsHornObj.dataString = streamReader.ReadCString();

                streamReader.Seek(bookmark, SeekOrigin.Begin);

                cmx.ngsHornDict.Add(ngsHornObj.ngsHorn.id, ngsHornObj);
            }
        }

        private static void ReadNGSTEETH(BufferedStreamReaderBE<MemoryStream> streamReader, int offset, CharacterMakingIndex cmx)
        {
            //NGS Teeth
            streamReader.Seek(cmx.cmxTable.teethAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < cmx.cmxTable.teethCount; i++)
            {
                var ngsTeethObj = new NGS_TeethObject();
                ngsTeethObj.num = i;
                ngsTeethObj.originalOffset = streamReader.Position;
                ngsTeethObj.ngsTeeth = streamReader.Read<NGS_Teeth>();
                long bookmark = streamReader.Position;

                streamReader.Seek(ngsTeethObj.ngsTeeth.dataStringPtr + offset, SeekOrigin.Begin);
                ngsTeethObj.dataString = streamReader.ReadCString();

                streamReader.Seek(ngsTeethObj.ngsTeeth.texString1Ptr + offset, SeekOrigin.Begin);
                ngsTeethObj.texString1 = streamReader.ReadCString();

                streamReader.Seek(ngsTeethObj.ngsTeeth.texString2Ptr + offset, SeekOrigin.Begin);
                ngsTeethObj.texString2 = streamReader.ReadCString();

                streamReader.Seek(ngsTeethObj.ngsTeeth.texString3Ptr + offset, SeekOrigin.Begin);
                ngsTeethObj.texString3 = streamReader.ReadCString();

                streamReader.Seek(ngsTeethObj.ngsTeeth.texString4Ptr + offset, SeekOrigin.Begin);
                ngsTeethObj.texString4 = streamReader.ReadCString();

                cmx.ngsTeethDict.Add(ngsTeethObj.ngsTeeth.id, ngsTeethObj);
                streamReader.Seek(bookmark, SeekOrigin.Begin);
            }
        }

        private static void ReadNGSEAR(BufferedStreamReaderBE<MemoryStream> streamReader, int offset, CharacterMakingIndex cmx)
        {
            //NGS ears
            streamReader.Seek(cmx.cmxTable.earAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < cmx.cmxTable.earCount; i++)
            {
                var ngsEarObj = new NGS_EarObject();
                ngsEarObj.num = i;
                ngsEarObj.originalOffset = streamReader.Position;
                ngsEarObj.ngsEar = streamReader.Read<NGS_Ear>();
                long bookmark = streamReader.Position;

                streamReader.Seek(ngsEarObj.ngsEar.dataStringPtr + offset, SeekOrigin.Begin);
                ngsEarObj.dataString = streamReader.ReadCString();

                streamReader.Seek(ngsEarObj.ngsEar.texString1Ptr + offset, SeekOrigin.Begin);
                ngsEarObj.texString1 = streamReader.ReadCString();

                streamReader.Seek(ngsEarObj.ngsEar.texString2Ptr + offset, SeekOrigin.Begin);
                ngsEarObj.texString2 = streamReader.ReadCString();

                streamReader.Seek(ngsEarObj.ngsEar.texString3Ptr + offset, SeekOrigin.Begin);
                ngsEarObj.texString3 = streamReader.ReadCString();

                streamReader.Seek(ngsEarObj.ngsEar.texString4Ptr + offset, SeekOrigin.Begin);
                ngsEarObj.texString4 = streamReader.ReadCString();

                streamReader.Seek(ngsEarObj.ngsEar.texString5Ptr + offset, SeekOrigin.Begin);
                ngsEarObj.texString5 = streamReader.ReadCString();

                cmx.ngsEarDict.Add(ngsEarObj.ngsEar.id, ngsEarObj);
                streamReader.Seek(bookmark, SeekOrigin.Begin);
            }
        }

        private static void ReadBODY(BufferedStreamReaderBE<MemoryStream> streamReader, int offset, int baseAddress, int count, Dictionary<int, BODYObject> dict, int rel0DataStart)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                BODYObject body = new BODYObject();
                body.num = i;
                body.originalOffset = streamReader.Position;
                body.body = streamReader.Read<BODY>();

                if (rel0DataStart >= CMXConstants.dec14_21TableAddressInt)
                {
                    body.bodyMaskColorMapping = streamReader.Read<BODYMaskColorMapping>();
                }

                body.body2 = streamReader.Read<BODY2>();
                if (rel0DataStart >= CMXConstants.feb8_22TableAddressInt)
                {
                    body.body40cap = streamReader.Read<BODY40Cap>();
                }

                if (rel0DataStart >= CMXConstants.jan25_23TableAddressInt)
                {
                    body.body2023_1 = streamReader.Read<BODY2023_1>();
                }

                if (rel0DataStart >= CMXConstants.ver2TableAddressInt)
                {
                    body.bodyVer2 = streamReader.Read<BODYVer2>();
                }

                long temp = streamReader.Position;

                if (IsValidOffset(body.body.dataStringPtr))
                {
                    streamReader.Seek(body.body.dataStringPtr + offset, SeekOrigin.Begin);
                    body.dataString = streamReader.ReadCString();
                }
                if (IsValidOffset(body.body.texString1Ptr))
                {
                    streamReader.Seek(body.body.texString1Ptr + offset, SeekOrigin.Begin);
                    body.texString1 = streamReader.ReadCString();
                }
                if (IsValidOffset(body.body.texString2Ptr))
                {
                    streamReader.Seek(body.body.texString2Ptr + offset, SeekOrigin.Begin);
                    body.texString2 = streamReader.ReadCString();
                }
                if (IsValidOffset(body.body.texString3Ptr))
                {
                    streamReader.Seek(body.body.texString3Ptr + offset, SeekOrigin.Begin);
                    body.texString3 = streamReader.ReadCString();
                }
                if (IsValidOffset(body.body.texString4Ptr))
                {
                    streamReader.Seek(body.body.texString4Ptr + offset, SeekOrigin.Begin);
                    body.texString4 = streamReader.ReadCString();
                }
                if (IsValidOffset(body.body.texString5Ptr))
                {
                    streamReader.Seek(body.body.texString5Ptr + offset, SeekOrigin.Begin);
                    body.texString5 = streamReader.ReadCString();
                }
                if (IsValidOffset(body.body.texString6Ptr))
                {
                    streamReader.Seek(body.body.texString6Ptr + offset, SeekOrigin.Begin);
                    body.texString6 = streamReader.ReadCString();
                }
                if (IsValidOffset(body.body2023_1.nodeStrPtr_0))
                {
                    streamReader.Seek(body.body2023_1.nodeStrPtr_0 + offset, SeekOrigin.Begin);
                    body.nodeString0 = streamReader.ReadCString();
                }
                if (IsValidOffset(body.body2023_1.nodeStrPtr_1))
                {
                    streamReader.Seek(body.body2023_1.nodeStrPtr_1 + offset, SeekOrigin.Begin);
                    body.nodeString1 = streamReader.ReadCString();
                }
                if (IsValidOffset(body.bodyVer2.nodeStrPtr_2))
                {
                    streamReader.Seek(body.bodyVer2.nodeStrPtr_2 + offset, SeekOrigin.Begin);
                    body.nodeString2 = streamReader.ReadCString();
                }

                streamReader.Seek(temp, SeekOrigin.Begin);

                if (dict.ContainsKey(body.body.id))
                {
                    continue;
                }
                dict.Add(body.body.id, body); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadBBLY(BufferedStreamReaderBE<MemoryStream> streamReader, int offset, int baseAddress, int count, Dictionary<int, BBLYObject> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                BBLYObject bbly = new BBLYObject();
                bbly.num = i;
                bbly.originalOffset = streamReader.Position;
                bbly.bbly = streamReader.Read<BBLY>();
                long temp = streamReader.Position;

                streamReader.Seek(bbly.bbly.texString1Ptr + offset, SeekOrigin.Begin);
                bbly.texString1 = streamReader.ReadCString();

                streamReader.Seek(bbly.bbly.texString2Ptr + offset, SeekOrigin.Begin);
                bbly.texString2 = streamReader.ReadCString();

                streamReader.Seek(bbly.bbly.texString3Ptr + offset, SeekOrigin.Begin);
                bbly.texString3 = streamReader.ReadCString();

                streamReader.Seek(bbly.bbly.texString4Ptr + offset, SeekOrigin.Begin);
                bbly.texString4 = streamReader.ReadCString();

                streamReader.Seek(bbly.bbly.texString5Ptr + offset, SeekOrigin.Begin);
                bbly.texString5 = streamReader.ReadCString();

                streamReader.Seek(temp, SeekOrigin.Begin);

                dict.Add(bbly.bbly.id, bbly); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadSticker(BufferedStreamReaderBE<MemoryStream> streamReader, int offset, int baseAddress, int count, Dictionary<int, StickerObject> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                StickerObject sticker = new StickerObject();
                sticker.num = i;
                sticker.originalOffset = streamReader.Position;
                sticker.sticker = streamReader.Read<Sticker>();
                long temp = streamReader.Position;

                streamReader.Seek(sticker.sticker.texStringPtr + offset, SeekOrigin.Begin);
                sticker.texString = streamReader.ReadCString();

                streamReader.Seek(temp, SeekOrigin.Begin);

                dict.Add(sticker.sticker.id, sticker); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadFACE(BufferedStreamReaderBE<MemoryStream> streamReader, int offset, int baseAddress, int count, Dictionary<int, FACEObject> dict, int rel0DataStart)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                FACEObject face = new FACEObject();
                face.num = i;
                face.originalOffset = streamReader.Position;
                face.face = streamReader.Read<FACE>();
                if (rel0DataStart > CMXConstants.dec14_21TableAddressInt)
                {
                    face.faceRitem = streamReader.Read<FACERitem>();
                }
                face.face2 = streamReader.Read<FACE2>();

                //Handle backwards compatibility
                if (rel0DataStart < CMXConstants.dec14_21TableAddressInt)
                {
                    face.faceRitem = new FACERitem();
                    face.faceRitem.unkIntRT0 = face.face2.unkInt0;
                    var bytes = BitConverter.GetBytes(face.face2.unkInt1);
                    face.faceRitem.unkIntRT1 = BitConverter.ToInt16(bytes, 0);
                    face.face2.unkInt0 = BitConverter.ToInt16(bytes, 2);
                    face.face2.unkInt1 = 0;
                }
                if (rel0DataStart > CMXConstants.dec14_21TableAddressInt)
                {
                    face.unkFloatRitem = streamReader.Read<float>();
                }
                if (rel0DataStart >= CMXConstants.ver2TableAddressInt)
                {
                    face.unkVer2Int = streamReader.Read<int>();
                }
                long temp = streamReader.Position;

                if (face.face.dataStringPtr + offset > 0)
                {
                    streamReader.Seek(face.face.dataStringPtr + offset, SeekOrigin.Begin);
                    face.dataString = streamReader.ReadCString();
                }

                if (face.face.texString1Ptr + offset > 0)
                {
                    streamReader.Seek(face.face.texString1Ptr + offset, SeekOrigin.Begin);
                    face.texString1 = streamReader.ReadCString();
                }

                if (face.face.texString2Ptr + offset > 0)
                {
                    streamReader.Seek(face.face.texString2Ptr + offset, SeekOrigin.Begin);
                    face.texString2 = streamReader.ReadCString();
                }

                if (face.face.texString3Ptr + offset > 0)
                {
                    streamReader.Seek(face.face.texString3Ptr + offset, SeekOrigin.Begin);
                    face.texString3 = streamReader.ReadCString();
                }

                if (face.face.texString4Ptr + offset > 0)
                {
                    streamReader.Seek(face.face.texString4Ptr + offset, SeekOrigin.Begin);
                    face.texString4 = streamReader.ReadCString();
                }

                if (face.face.texString5Ptr + offset > 0)
                {
                    streamReader.Seek(face.face.texString5Ptr + offset, SeekOrigin.Begin);
                    face.texString5 = streamReader.ReadCString();
                }

                if (face.face.texString6Ptr + offset > 0)
                {
                    streamReader.Seek(face.face.texString6Ptr + offset, SeekOrigin.Begin);
                    face.texString6 = streamReader.ReadCString();
                }

                streamReader.Seek(temp, SeekOrigin.Begin);

                if (!dict.ContainsKey(face.face.id))
                {
                    dict.Add(face.face.id, face); //Set like this so we can access it by id later if we want. 
                }
            }
        }

        private static void ReadFCMN(BufferedStreamReaderBE<MemoryStream> streamReader, int offset, int baseAddress, int count, Dictionary<int, FCMNObject> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                FCMNObject fcmn = new FCMNObject();
                fcmn.num = i;
                fcmn.originalOffset = streamReader.Position;
                fcmn.fcmn = streamReader.Read<FCMN>();
                long temp = streamReader.Position;

                streamReader.Seek(fcmn.fcmn.proportionAnimPtr + offset, SeekOrigin.Begin);
                fcmn.proportionAnim = streamReader.ReadCString();

                streamReader.Seek(fcmn.fcmn.faceAnim1Ptr + offset, SeekOrigin.Begin);
                fcmn.faceAnim1 = streamReader.ReadCString();

                streamReader.Seek(fcmn.fcmn.faceAnim2Ptr + offset, SeekOrigin.Begin);
                fcmn.faceAnim2 = streamReader.ReadCString();

                streamReader.Seek(fcmn.fcmn.faceAnim3Ptr + offset, SeekOrigin.Begin);
                fcmn.faceAnim3 = streamReader.ReadCString();

                streamReader.Seek(fcmn.fcmn.faceAnim4Ptr + offset, SeekOrigin.Begin);
                fcmn.faceAnim4 = streamReader.ReadCString();

                streamReader.Seek(fcmn.fcmn.faceAnim5Ptr + offset, SeekOrigin.Begin);
                fcmn.faceAnim5 = streamReader.ReadCString();

                streamReader.Seek(fcmn.fcmn.faceAnim6Ptr + offset, SeekOrigin.Begin);
                fcmn.faceAnim6 = streamReader.ReadCString();

                streamReader.Seek(fcmn.fcmn.faceAnim7Ptr + offset, SeekOrigin.Begin);
                fcmn.faceAnim7 = streamReader.ReadCString();

                streamReader.Seek(fcmn.fcmn.faceAnim8Ptr + offset, SeekOrigin.Begin);
                fcmn.faceAnim8 = streamReader.ReadCString();

                streamReader.Seek(fcmn.fcmn.faceAnim9Ptr + offset, SeekOrigin.Begin);
                fcmn.faceAnim9 = streamReader.ReadCString();

                streamReader.Seek(fcmn.fcmn.faceAnim10Ptr + offset, SeekOrigin.Begin);
                fcmn.faceAnim10 = streamReader.ReadCString();

                streamReader.Seek(temp, SeekOrigin.Begin);

                dict.Add(fcmn.fcmn.id, fcmn); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadFCP(BufferedStreamReaderBE<MemoryStream> streamReader, int offset, int baseAddress, int count, Dictionary<int, FCPObject> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                FCPObject fcp = new FCPObject();
                fcp.num = i;
                fcp.originalOffset = streamReader.Position;
                fcp.fcp = streamReader.Read<FCP>();

                long temp = streamReader.Position;

                streamReader.Seek(fcp.fcp.texString1Ptr + offset, SeekOrigin.Begin);
                fcp.texString1 = streamReader.ReadCString();

                streamReader.Seek(fcp.fcp.texString2Ptr + offset, SeekOrigin.Begin);
                fcp.texString2 = streamReader.ReadCString();

                streamReader.Seek(fcp.fcp.texString3Ptr + offset, SeekOrigin.Begin);
                fcp.texString3 = streamReader.ReadCString();

                streamReader.Seek(fcp.fcp.texString4Ptr + offset, SeekOrigin.Begin);
                fcp.texString4 = streamReader.ReadCString();

                streamReader.Seek(temp, SeekOrigin.Begin);
                if (dict.ContainsKey(fcp.fcp.id))
                {
                    Console.WriteLine(fcp.fcp.id);
                }
                dict.Add(fcp.fcp.id, fcp); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadFaceTextures(BufferedStreamReaderBE<MemoryStream> streamReader, int offset, int baseAddress, int count, Dictionary<int, FaceTextureObject> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                FaceTextureObject ngsFace = new FaceTextureObject();
                ngsFace.num = i;
                ngsFace.originalOffset = streamReader.Position;
                ngsFace.ngsFace = streamReader.Read<FaceTextures>();
                long temp = streamReader.Position;

                streamReader.Seek(ngsFace.ngsFace.texString1Ptr + offset, SeekOrigin.Begin);
                ngsFace.texString1 = streamReader.ReadCString();

                streamReader.Seek(ngsFace.ngsFace.texString2Ptr + offset, SeekOrigin.Begin);
                ngsFace.texString2 = streamReader.ReadCString();

                streamReader.Seek(ngsFace.ngsFace.texString3Ptr + offset, SeekOrigin.Begin);
                ngsFace.texString3 = streamReader.ReadCString();

                streamReader.Seek(ngsFace.ngsFace.texString4Ptr + offset, SeekOrigin.Begin);
                ngsFace.texString4 = streamReader.ReadCString();

                streamReader.Seek(temp, SeekOrigin.Begin);

                dict.Add(ngsFace.ngsFace.id, ngsFace); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadACCE(BufferedStreamReaderBE<MemoryStream> streamReader, int offset, int baseAddress, int count, Dictionary<int, ACCEObject> dict, int cmxDateSize)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                ACCEObject acce = new ACCEObject();
                acce.num = i;
                acce.originalOffset = streamReader.Position;
                acce.acce = streamReader.Read<ACCE>();                      //0x28
                if (cmxDateSize >= CMXConstants.feb8_22TableAddressInt)
                {
                    acce.acceFeb8_22 = streamReader.Read<ACCE_Feb8_22>();   //0x1C
                }
                acce.acceB = streamReader.Read<ACCE_B>();                   //0x1C
                acce.acce2a = streamReader.Read<ACCE2A>();                  //0x1C
                //This float was added to the middle of these in the Aug_3_2021 patch
                if (count >= 5977)
                {
                    acce.flt_54 = streamReader.Read<float>();                 //0x4
                }
                acce.acce2b = streamReader.Read<ACCE2B>();                  //0xC
                for (int j = 0; j < 3; j++)                                 //0x60
                {
                    acce.acce12List.Add(ReadAcce12Object(streamReader, count));
                }
                if (cmxDateSize >= CMXConstants.oct4_22TableAddressInt)
                {
                    acce.effectNamePtr = streamReader.Read<int>();
                }
                if (cmxDateSize >= CMXConstants.ver2TableAddressInt)
                {
                    acce.accev2 = streamReader.Read<ACCEV2>();
                }
                if (cmxDateSize >= CMXConstants.aug17_22TableAddressInt)
                {
                    acce.flt_90 = streamReader.Read<float>();
                }

                long temp = streamReader.Position;

                if (acce.acce.dataStringPtr + offset > 0)
                {
                    streamReader.Seek(acce.acce.dataStringPtr + offset, SeekOrigin.Begin);
                    acce.dataString = streamReader.ReadCString();
                }
                if (acce.acce.nodeAttach1Ptr + offset > 0)
                {
                    streamReader.Seek(acce.acce.nodeAttach1Ptr + offset, SeekOrigin.Begin);
                    acce.nodeAttach1 = streamReader.ReadCString();
                }
                if (acce.acce.nodeAttach2Ptr + offset > 0)
                {
                    streamReader.Seek(acce.acce.nodeAttach2Ptr + offset, SeekOrigin.Begin);
                    acce.nodeAttach2 = streamReader.ReadCString();
                }
                if (acce.acce.nodeAttach3Ptr + offset > 0)
                {
                    streamReader.Seek(acce.acce.nodeAttach3Ptr + offset, SeekOrigin.Begin);
                    acce.nodeAttach3 = streamReader.ReadCString();
                }
                if (acce.acce.nodeAttach4Ptr + offset > 0)
                {
                    streamReader.Seek(acce.acce.nodeAttach4Ptr + offset, SeekOrigin.Begin);
                    acce.nodeAttach4 = streamReader.ReadCString();
                }
                if (acce.acce.nodeAttach5Ptr + offset > 0)
                {
                    streamReader.Seek(acce.acce.nodeAttach5Ptr + offset, SeekOrigin.Begin);
                    acce.nodeAttach5 = streamReader.ReadCString();
                }
                if (acce.acce.nodeAttach6Ptr + offset > 0)
                {
                    streamReader.Seek(acce.acce.nodeAttach6Ptr + offset, SeekOrigin.Begin);
                    acce.nodeAttach6 = streamReader.ReadCString();
                }
                if (acce.acce.nodeAttach7Ptr + offset > 0)
                {
                    streamReader.Seek(acce.acce.nodeAttach7Ptr + offset, SeekOrigin.Begin);
                    acce.nodeAttach7 = streamReader.ReadCString();
                }
                if (acce.acce.nodeAttach8Ptr + offset > 0)
                {
                    streamReader.Seek(acce.acce.nodeAttach8Ptr + offset, SeekOrigin.Begin);
                    acce.nodeAttach8 = streamReader.ReadCString();
                }
                if (cmxDateSize >= CMXConstants.feb8_22TableAddressInt)
                {
                    if (acce.acceFeb8_22.acceString9Ptr + offset > 0)
                    {
                        streamReader.Seek(acce.acceFeb8_22.acceString9Ptr + offset, SeekOrigin.Begin);
                        acce.nodeAttach9 = streamReader.ReadCString();
                    }
                    if (acce.acceFeb8_22.acceString10Ptr + offset > 0)
                    {
                        streamReader.Seek(acce.acceFeb8_22.acceString10Ptr + offset, SeekOrigin.Begin);
                        acce.nodeAttach10 = streamReader.ReadCString();
                    }
                    if (acce.acceFeb8_22.acceString11Ptr + offset > 0)
                    {
                        streamReader.Seek(acce.acceFeb8_22.acceString11Ptr + offset, SeekOrigin.Begin);
                        acce.nodeAttach11 = streamReader.ReadCString();
                    }
                    if (acce.acceFeb8_22.acceString12Ptr + offset > 0)
                    {
                        streamReader.Seek(acce.acceFeb8_22.acceString12Ptr + offset, SeekOrigin.Begin);
                        acce.nodeAttach12 = streamReader.ReadCString();
                    }
                    if (acce.acceFeb8_22.acceString13Ptr + offset > 0)
                    {
                        streamReader.Seek(acce.acceFeb8_22.acceString13Ptr + offset, SeekOrigin.Begin);
                        acce.nodeAttach13 = streamReader.ReadCString();
                    }
                    if (acce.acceFeb8_22.acceString14Ptr + offset > 0)
                    {
                        streamReader.Seek(acce.acceFeb8_22.acceString14Ptr + offset, SeekOrigin.Begin);
                        acce.nodeAttach14 = streamReader.ReadCString();
                    }
                    if (acce.acceFeb8_22.acceString15Ptr + offset > 0)
                    {
                        streamReader.Seek(acce.acceFeb8_22.acceString15Ptr + offset, SeekOrigin.Begin);
                        acce.nodeAttach15 = streamReader.ReadCString();
                    }

                    if (cmxDateSize >= CMXConstants.oct4_22TableAddressInt)
                    {
                        if (acce.effectNamePtr + offset > 0)
                        {
                            streamReader.Seek(acce.effectNamePtr + offset, SeekOrigin.Begin);
                            acce.effectName = streamReader.ReadCString();
                        }

                        if (cmxDateSize >= CMXConstants.ver2TableAddressInt)
                        {
                            if (acce.accev2.acceString16Ptr + offset > 0)
                            {
                                streamReader.Seek(acce.accev2.acceString16Ptr + offset, SeekOrigin.Begin);
                                acce.nodeAttach16 = streamReader.ReadCString();
                            }
                            if (acce.accev2.acceString17Ptr + offset > 0)
                            {
                                streamReader.Seek(acce.accev2.acceString17Ptr + offset, SeekOrigin.Begin);
                                acce.nodeAttach17 = streamReader.ReadCString();
                            }
                            if (acce.accev2.acceString18Ptr + offset > 0)
                            {
                                streamReader.Seek(acce.accev2.acceString18Ptr + offset, SeekOrigin.Begin);
                                acce.nodeAttach18 = streamReader.ReadCString();
                            }
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

        private static ACCE_12Object ReadAcce12Object(BufferedStreamReaderBE<MemoryStream> streamReader, int count)
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

        private static void ReadEYE(BufferedStreamReaderBE<MemoryStream> streamReader, int offset, int baseAddress, int count, Dictionary<int, EYEObject> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                EYEObject eye = new EYEObject();
                eye.num = i;
                eye.originalOffset = streamReader.Position;
                eye.eye = streamReader.Read<EYE>();
                long temp = streamReader.Position;

                streamReader.Seek(eye.eye.texString1Ptr + offset, SeekOrigin.Begin);
                eye.texString1 = streamReader.ReadCString();

                streamReader.Seek(eye.eye.texString2Ptr + offset, SeekOrigin.Begin);
                eye.texString2 = streamReader.ReadCString();

                streamReader.Seek(eye.eye.texString3Ptr + offset, SeekOrigin.Begin);
                eye.texString3 = streamReader.ReadCString();

                streamReader.Seek(eye.eye.texString4Ptr + offset, SeekOrigin.Begin);
                eye.texString4 = streamReader.ReadCString();

                streamReader.Seek(eye.eye.texString5Ptr + offset, SeekOrigin.Begin);
                eye.texString5 = streamReader.ReadCString();

                streamReader.Seek(temp, SeekOrigin.Begin);

                dict.Add(eye.eye.id, eye); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadNGSSKIN(BufferedStreamReaderBE<MemoryStream> streamReader, int offset, int baseAddress, int count, Dictionary<int, NGS_SKINObject> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                NGS_SKINObject ngsSkin = new NGS_SKINObject();
                ngsSkin.num = i;
                ngsSkin.originalOffset = streamReader.Position;
                ngsSkin.ngsSkin = streamReader.Read<NGS_Skin>();
                long temp = streamReader.Position;

                streamReader.Seek(ngsSkin.ngsSkin.texString1Ptr + offset, SeekOrigin.Begin);
                ngsSkin.texString1 = streamReader.ReadCString();

                streamReader.Seek(ngsSkin.ngsSkin.texString2Ptr + offset, SeekOrigin.Begin);
                ngsSkin.texString2 = streamReader.ReadCString();

                streamReader.Seek(ngsSkin.ngsSkin.texString3Ptr + offset, SeekOrigin.Begin);
                ngsSkin.texString3 = streamReader.ReadCString();

                streamReader.Seek(ngsSkin.ngsSkin.texString4Ptr + offset, SeekOrigin.Begin);
                ngsSkin.texString4 = streamReader.ReadCString();

                streamReader.Seek(ngsSkin.ngsSkin.texString6Ptr + offset, SeekOrigin.Begin);
                ngsSkin.texString5 = streamReader.ReadCString();

                streamReader.Seek(ngsSkin.ngsSkin.texString6Ptr + offset, SeekOrigin.Begin);
                ngsSkin.texString6 = streamReader.ReadCString();

                streamReader.Seek(ngsSkin.ngsSkin.texString7Ptr + offset, SeekOrigin.Begin);
                ngsSkin.texString7 = streamReader.ReadCString();

                streamReader.Seek(ngsSkin.ngsSkin.texString8Ptr + offset, SeekOrigin.Begin);
                ngsSkin.texString8 = streamReader.ReadCString();

                streamReader.Seek(ngsSkin.ngsSkin.texString9Ptr + offset, SeekOrigin.Begin);
                ngsSkin.texString9 = streamReader.ReadCString();

                streamReader.Seek(ngsSkin.ngsSkin.texString10Ptr + offset, SeekOrigin.Begin);
                ngsSkin.texString10 = streamReader.ReadCString();

                streamReader.Seek(temp, SeekOrigin.Begin);

                dict.Add(ngsSkin.ngsSkin.id, ngsSkin); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadEYEB(BufferedStreamReaderBE<MemoryStream> streamReader, int offset, int baseAddress, int count, Dictionary<int, EYEBObject> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                EYEBObject eyeb = new EYEBObject();
                eyeb.num = i;
                eyeb.originalOffset = streamReader.Position;
                eyeb.eyeb = streamReader.Read<EYEB>();
                long temp = streamReader.Position;

                streamReader.Seek(eyeb.eyeb.texString1Ptr + offset, SeekOrigin.Begin);
                eyeb.texString1 = streamReader.ReadCString();

                streamReader.Seek(eyeb.eyeb.texString2Ptr + offset, SeekOrigin.Begin);
                eyeb.texString2 = streamReader.ReadCString();

                streamReader.Seek(eyeb.eyeb.texString3Ptr + offset, SeekOrigin.Begin);
                eyeb.texString3 = streamReader.ReadCString();

                streamReader.Seek(eyeb.eyeb.texString4Ptr + offset, SeekOrigin.Begin);
                eyeb.texString4 = streamReader.ReadCString();

                streamReader.Seek(temp, SeekOrigin.Begin);

                dict.Add(eyeb.eyeb.id, eyeb); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadHAIR(BufferedStreamReaderBE<MemoryStream> streamReader, int offset, int baseAddress, int count, Dictionary<int, HAIRObject> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                HAIRObject hair = new HAIRObject();
                hair.num = i;
                hair.originalOffset = streamReader.Position;
                hair.hair = streamReader.Read<HAIR>();
                long temp = streamReader.Position;

                streamReader.Seek(hair.hair.dataStringPtr + offset, SeekOrigin.Begin);
                hair.dataString = streamReader.ReadCString();

                streamReader.Seek(hair.hair.texString1Ptr + offset, SeekOrigin.Begin);
                hair.texString1 = streamReader.ReadCString();

                streamReader.Seek(hair.hair.texString2Ptr + offset, SeekOrigin.Begin);
                hair.texString2 = streamReader.ReadCString();

                streamReader.Seek(hair.hair.texString3Ptr + offset, SeekOrigin.Begin);
                hair.texString3 = streamReader.ReadCString();

                streamReader.Seek(hair.hair.texString4Ptr + offset, SeekOrigin.Begin);
                hair.texString4 = streamReader.ReadCString();

                streamReader.Seek(hair.hair.texString5Ptr + offset, SeekOrigin.Begin);
                hair.texString5 = streamReader.ReadCString();

                streamReader.Seek(hair.hair.texString6Ptr + offset, SeekOrigin.Begin);
                hair.texString6 = streamReader.ReadCString();

                streamReader.Seek(hair.hair.texString7Ptr + offset, SeekOrigin.Begin);
                hair.texString7 = streamReader.ReadCString();

                streamReader.Seek(temp, SeekOrigin.Begin);

                dict.Add(hair.hair.id, hair); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadNIFLCOL(BufferedStreamReaderBE<MemoryStream> streamReader, int offset, int baseAddress, int count, Dictionary<int, NIFL_COLObject> dict)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                NIFL_COLObject col = new NIFL_COLObject();
                col.num = i;
                col.originalOffset = streamReader.Position;
                col.niflCol = streamReader.Read<NIFL_COL>();
                long temp = streamReader.Position;

                streamReader.Seek(col.niflCol.textStringPtr + offset, SeekOrigin.Begin);
                col.textString = streamReader.ReadCString();

                streamReader.Seek(temp, SeekOrigin.Begin);

                dict.Add(col.niflCol.id, col); //Set like this so we can access it by id later if we want. 
            }
        }

        private static void ReadPart6_7_22(BufferedStreamReaderBE<MemoryStream> streamReader, int offset, int baseAddress, int count, Dictionary<int, Part6_7_22Obj> dict, int rel0Start)
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
                    Console.WriteLine($"Duplicate key {part.partStruct.id} at {(streamReader.Position - offset).ToString("X")}");
                }
            }
        }

        private static void ReadIndexLinks(BufferedStreamReaderBE<MemoryStream> streamReader, int offset, int baseAddress, int count, Dictionary<int, BCLNObject> dict, int rel0Start)
        {
            streamReader.Seek(baseAddress + offset, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                BCLNObject bcln = new BCLNObject();
                bcln.bcln = streamReader.Read<BCLN>();
                if (rel0Start >= CMXConstants.dec14_21TableAddressInt)
                {
                    bcln.bclnRitem = streamReader.Read<BCLNRitem>();
                    if (rel0Start >= CMXConstants.feb8_22TableAddressInt)
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
                    Console.WriteLine($"Duplicate key {bcln.bcln.id} at {(streamReader.Position - offset).ToString("X")}");
                }
            }
        }
        #endregion

        #region ReadVTBF
        public void ReadVTBFCMX(byte[] file)
        {
            using (MemoryStream stream = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                ReadVTBFFile(sr);
            }
        }

        public override void ReadVTBFFile(BufferedStreamReaderBE<MemoryStream> streamReader)
        {
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
                List<Dictionary<int, object>> data = VTBFMethods.ReadVTBFTag(streamReader, out string tagType, out int ptrCount, out int entryCount);
                switch (tagType)
                {
                    case "ACCE":
                        accessoryDict.Add((int)data[0][0xFF], new ACCEObject(data));
                        break;
                    case "BODY":
                        if (pastBody0 == false)
                        {
                            costumeDict.Add((int)data[0][0xFF], new BODYObject(data));
                        }
                        else
                        {
                            baseWearDict.Add((int)data[0][0xFF], new BODYObject(data));
                        }
                        break;
                    case "CARM":
                        pastBody0 = true;
                        if (pastCarm0 == false)
                        {
                            carmDict.Add((int)data[0][0xFF], new BODYObject(data));
                        }
                        else
                        {
                            outerDict.Add((int)data[0][0xFF], new BODYObject(data));
                        }
                        break;
                    case "CLEG":
                        pastCarm0 = true;
                        clegDict.Add((int)data[0][0xFF], new BODYObject(data));
                        break;
                    case "BDP1":
                        bodyPaintDict.Add((int)data[0][0xFF], new BBLYObject(data));
                        break;
                    case "BDP2":
                        stickerDict.Add((int)data[0][0xFF], new StickerObject(data));
                        break;
                    case "FACE":
                        faceDict.Add((int)data[0][0xFF], new FACEObject(data));
                        break;
                    case "FCMN":
                        fcmnDict.Add((int)data[0][0xFF], new FCMNObject(data));
                        break;
                    case "FCP1":
                        //These were kinda redundant with the FACE structs so SEGA yeeted them for NIFL. Not much reason to read them.
                        //cmx.fcpDict.Add((int)data[0][0xFF], parseFCP1(data));
                        break;
                    case "FCP2":
                        fcpDict.Add((int)data[0][0xFF], new FCPObject(data));
                        break;
                    case "EYE ":
                        eyeDict.Add((int)data[0][0xFF], new EYEObject(data));
                        break;
                    case "EYEB":
                        eyebrowDict.Add((int)data[0][0xFF], new EYEBObject(data));
                        break;
                    case "EYEL":
                        eyelashDict.Add((int)data[0][0xFF], new EYEBObject(data));
                        break;
                    case "HAIR":
                        hairDict.Add((int)data[0][0xFF], new HAIRObject(data));
                        break;
                    case "COL ":
                        legacyColDict.Add((int)data[0][0xFF], new VTBF_COLObject(data));
                        break;
                    case "BBLY":
                        innerWearDict.Add((int)data[0][0xFF], new BBLYObject(data));
                        break;
                    case "BCLN":
                        if (pastBcln0 == false)
                        {
                            costumeIdLink.Add((int)data[0][0xFF], new BCLNObject(data));
                        }
                        else
                        {
                            baseWearIdLink.Add((int)data[0][0xFF], new BCLNObject(data));
                        }
                        break;
                    case "LCLN":
                        pastBcln0 = true;
                        clegIdLink.Add((int)data[0][0xFF], new BCLNObject(data));
                        break;
                    case "ACLN":
                        castArmIdLink.Add((int)data[0][0xFF], new BCLNObject(data));
                        break;
                    case "ICLN":
                        innerWearIdLink.Add((int)data[0][0xFF], new BCLNObject(data));
                        break;
                    default:
                        throw new Exception($"Unexpected tag type {tagType}");
                }
            }
        }
        #endregion

        #region WriteMethods
        public override byte[] GetBytesNIFL()
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
            var costList = costumeDict.Keys.ToList();
            costList.Sort();
            foreach (var key in costList)
            {
                AddBodyBytes(costumeDict[key], WriteMode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //Cast Arms
            int carmAddress = outBytes.Count;
            var carmList = carmDict.Keys.ToList();
            carmList.Sort();
            foreach (var key in carmList)
            {
                AddBodyBytes(carmDict[key], WriteMode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //Cast Legs
            int clegAddress = outBytes.Count;
            var clegList = clegDict.Keys.ToList();
            clegList.Sort();
            foreach (var key in clegList)
            {
                AddBodyBytes(clegDict[key], WriteMode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //BDP1
            //Body Paint
            int bodyPaintAddress = outBytes.Count;
            var bdpList = bodyPaintDict.Keys.ToList();
            bdpList.Sort();
            foreach (var key in bdpList)
            {
                AddBDPBytes(bodyPaintDict[key], WriteMode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //Stickers
            int stickerAddress = outBytes.Count;
            var stickerList = stickerDict.Keys.ToList();
            stickerList.Sort();
            foreach (var key in stickerList)
            {
                AddStickerBytes(stickerDict[key], WriteMode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //FACE
            int faceAddress = outBytes.Count;
            var faceList = faceDict.Keys.ToList();
            faceList.Sort();
            foreach (var key in faceList)
            {
                AddFACEBytes(faceDict[key], WriteMode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //FCMN
            int fcmnAddress = outBytes.Count;
            var fcmnList = fcmnDict.Keys.ToList();
            fcmnList.Sort();
            foreach (var key in fcmnList)
            {
                AddFCMNBytes(fcmnDict[key], WriteMode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //FaceTextures
            int ftexAddress = outBytes.Count;
            var ftexList = faceTextureDict.Keys.ToList();
            ftexList.Sort();
            foreach (var key in ftexList)
            {
                AddFaceTexturesBytes(faceTextureDict[key], WriteMode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //FCP
            int fcpAddress = outBytes.Count;
            var fcpList = fcpDict.Keys.ToList();
            fcpList.Sort();
            foreach (var key in fcpList)
            {
                AddFCPBytes(fcpDict[key], WriteMode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //ACCE
            int acceAddress = outBytes.Count;
            var acceList = accessoryDict.Keys.ToList();
            acceList.Sort();
            foreach (var key in acceList)
            {
                AddACCEBytes(accessoryDict[key], WriteMode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //EYE
            int eyeAddress = outBytes.Count;
            var eyeList = eyeDict.Keys.ToList();
            eyeList.Sort();
            foreach (var key in eyeList)
            {
                AddEYEBytes(eyeDict[key], WriteMode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //EAR
            int earAddress = outBytes.Count;
            var earList = ngsEarDict.Keys.ToList();
            earList.Sort();
            foreach (var key in earList)
            {
                AddEARBytes(ngsEarDict[key], WriteMode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //Teeth
            int teethAddress = outBytes.Count;
            var teethList = ngsTeethDict.Keys.ToList();
            teethList.Sort();
            foreach (var key in teethList)
            {
                AddTeethBytes(ngsTeethDict[key], WriteMode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //Horn
            int hornAddress = outBytes.Count;
            var hornList = ngsHornDict.Keys.ToList();
            hornList.Sort();
            foreach (var key in hornList)
            {
                AddHornBytes(ngsHornDict[key], WriteMode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //Skin
            int skinAddress = outBytes.Count;
            var skinList = ngsSkinDict.Keys.ToList();
            skinList.Sort();
            foreach (var key in skinList)
            {
                AddSkinBytes(ngsSkinDict[key], WriteMode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //EYEB
            //Eyebrow
            int eyeBAddress = outBytes.Count;
            var eyeBList = eyebrowDict.Keys.ToList();
            eyeBList.Sort();
            foreach (var key in eyeBList)
            {
                AddEYEBBytes(eyebrowDict[key], WriteMode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //Eyelash
            int eyeLAddress = outBytes.Count;
            var eyeLList = eyelashDict.Keys.ToList();
            eyeLList.Sort();
            foreach (var key in eyeLList)
            {
                AddEYEBBytes(eyelashDict[key], WriteMode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //Hair
            int hairAddress = outBytes.Count;
            var hairList = hairDict.Keys.ToList();
            hairList.Sort();
            foreach (var key in hairList)
            {
                AddHAIRBytes(hairDict[key], WriteMode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //NIFL COL
            int colAddress = outBytes.Count;
            var colList = colDict.Keys.ToList();
            colList.Sort();
            foreach (var key in colList)
            {
                AddNIFLCOLBytes(colDict[key], WriteMode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //Unk
            int unkAddress = outBytes.Count;
            foreach (var unkArr in unkList)
            {
                outBytes.AddRange(unkArr.GetBytes());
            }
            //Outer
            int bodyOuterAddress = outBytes.Count;
            var outerList = outerDict.Keys.ToList();
            outerList.Sort();
            foreach (var key in outerList)
            {
                AddBodyBytes(outerDict[key], WriteMode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //Basewear
            int baseWearAddress = outBytes.Count;
            var baseList = baseWearDict.Keys.ToList();
            baseList.Sort();
            foreach (var key in baseList)
            {
                AddBodyBytes(baseWearDict[key], WriteMode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //BBLY
            //Inner wear 
            int innerWearAddress = outBytes.Count;
            var innerList = innerWearDict.Keys.ToList();
            innerList.Sort();
            foreach (var key in innerList)
            {
                AddBDPBytes(innerWearDict[key], WriteMode, outBytes, nof0PointerLocations, textAddressDict, textList);
            }

            //Index Links
            //Costume/Body
            int bodyLinkAddress = outBytes.Count;
            var bodyLinkList = costumeIdLink.Keys.ToList();
            bodyLinkList.Sort();
            foreach (var key in bodyLinkList)
            {
                outBytes.AddRange(costumeIdLink[key].GetBytes(WriteMode));
            }
            //Cast Leg
            int clegLinkAddress = outBytes.Count;
            var clegLinkList = clegIdLink.Keys.ToList();
            clegLinkList.Sort();
            foreach (var key in clegLinkList)
            {
                outBytes.AddRange(clegIdLink[key].GetBytes(WriteMode));
            }
            //Cast Arm
            int carmLinkAddress = outBytes.Count;
            var carmLinkList = castArmIdLink.Keys.ToList();
            carmLinkList.Sort();
            foreach (var key in carmLinkList)
            {
                outBytes.AddRange(castArmIdLink[key].GetBytes(WriteMode));
            }

            int castHeadLinkAddress = -1;
            if (WriteMode >= 1)
            {
                //Cast Head
                castHeadLinkAddress = outBytes.Count;
                var cHeadLinkList = castHeadIdLink.Keys.ToList();
                cHeadLinkList.Sort();
                foreach (var key in cHeadLinkList)
                {
                    outBytes.AddRange(castHeadIdLink[key].GetBytes(WriteMode));
                }
            }
            //Outer
            int outerLinkAddress = outBytes.Count;
            var outerLinkList = outerWearIdLink.Keys.ToList();
            outerLinkList.Sort();
            foreach (var key in outerLinkList)
            {
                outBytes.AddRange(outerWearIdLink[key].GetBytes(WriteMode));
            }
            //Base
            int baseLinkAddress = outBytes.Count;
            var baseLinkList = baseWearIdLink.Keys.ToList();
            baseLinkList.Sort();
            foreach (var key in baseLinkList)
            {
                outBytes.AddRange(baseWearIdLink[key].GetBytes(WriteMode));
            }
            //Inner
            int innerLinkAddress = outBytes.Count;
            var innerLinkList = innerWearIdLink.Keys.ToList();
            innerLinkList.Sort();
            foreach (var key in innerLinkList)
            {
                outBytes.AddRange(innerWearIdLink[key].GetBytes(WriteMode));
            }

            int accessoryIdLinkAddress = -1;
            if (WriteMode >= 1)
            {
                //Accessory
                accessoryIdLinkAddress = outBytes.Count;
                var acceLinkList = accessoryIdLink.Keys.ToList();
                acceLinkList.Sort();
                foreach (var key in acceLinkList)
                {
                    outBytes.AddRange(accessoryIdLink[key].GetBytes(WriteMode));
                }
            }
            //Write header data
            outBytes.SetByteListInt(rel0SizeOffset + 4, outBytes.Count);
            //CMX Table
            //Addresses
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(bodyAddress));
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(carmAddress));
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(clegAddress));
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(bodyOuterAddress));
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(baseWearAddress));
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(innerWearAddress));
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(bodyPaintAddress));
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(stickerAddress));
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(faceAddress));
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(fcmnAddress));
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(ftexAddress));
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(fcpAddress));
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(acceAddress));
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(eyeAddress));
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(earAddress));
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(teethAddress));
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(hornAddress));
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(skinAddress));
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(eyeBAddress));
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(eyeLAddress));
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(hairAddress));
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(colAddress));
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(unkAddress));

            //ID Links
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(bodyLinkAddress));
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(carmLinkAddress));
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(clegLinkAddress));

            if (WriteMode >= 1)
            {
                DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
                outBytes.AddRange(BitConverter.GetBytes(castHeadLinkAddress));
            }
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(outerLinkAddress));
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(baseLinkAddress));
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(innerLinkAddress));

            if (WriteMode >= 1)
            {
                DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
                outBytes.AddRange(BitConverter.GetBytes(accessoryIdLinkAddress));
            }

            //Counts
            outBytes.AddRange(BitConverter.GetBytes(costumeDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(carmDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(clegDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(outerDict.Count));

            outBytes.AddRange(BitConverter.GetBytes(baseWearDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(innerWearDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(bodyPaintDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(stickerDict.Count));

            outBytes.AddRange(BitConverter.GetBytes(faceDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(fcmnDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(faceTextureDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(fcpDict.Count));

            outBytes.AddRange(BitConverter.GetBytes(accessoryDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(eyeDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(ngsEarDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(ngsTeethDict.Count));

            outBytes.AddRange(BitConverter.GetBytes(ngsHornDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(ngsSkinDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(eyebrowDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(eyelashDict.Count));

            outBytes.AddRange(BitConverter.GetBytes(hairDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(colDict.Count));
            outBytes.AddRange(BitConverter.GetBytes(unkList.Count));

            outBytes.AddRange(BitConverter.GetBytes(costumeIdLink.Count));
            outBytes.AddRange(BitConverter.GetBytes(castArmIdLink.Count));
            outBytes.AddRange(BitConverter.GetBytes(clegIdLink.Count));

            if (WriteMode >= 1)
            {
                outBytes.AddRange(BitConverter.GetBytes(castHeadIdLink.Count));
                outBytes.AddRange(BitConverter.GetBytes(outerWearIdLink.Count));
                outBytes.AddRange(BitConverter.GetBytes(baseWearIdLink.Count));
                outBytes.AddRange(BitConverter.GetBytes(innerWearIdLink.Count));
                outBytes.AddRange(BitConverter.GetBytes(accessoryIdLink.Count));
            }
            else
            {
                outBytes.AddRange(BitConverter.GetBytes(outerWearIdLink.Count));
                outBytes.AddRange(BitConverter.GetBytes(baseWearIdLink.Count));
                outBytes.AddRange(BitConverter.GetBytes(innerWearIdLink.Count));
            }

            //Write text
            for (int i = 0; i < textList.Count; i++)
            {
                var offsetList = textAddressDict[textList[i]];
                for (int j = 0; j < offsetList.Count; j++)
                {
                    outBytes.SetByteListInt(offsetList[j], outBytes.Count);
                }
                outBytes.AddRange(Encoding.UTF8.GetBytes(textList[i]));
                var count = outBytes.Count;
                outBytes.AlignWriter(0x4);
                if (count == outBytes.Count)
                {
                    outBytes.AddRange(new byte[4]);
                }
            }

            outBytes.AlignWriter(0x10);

            //Write REL0 Size
            outBytes.SetByteListInt(rel0SizeOffset, outBytes.Count - 0x8);

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
            NOF0FullSize += outBytes.AlignWriter(0x10);

            //NEND
            outBytes.AddRange(Encoding.UTF8.GetBytes("NEND"));
            outBytes.AddRange(BitConverter.GetBytes(0x8));
            outBytes.AddRange(BitConverter.GetBytes(0));
            outBytes.AddRange(BitConverter.GetBytes(0));

            //Generate NIFL
            NIFL nifl = new NIFL();
            nifl.magic = BitConverter.ToInt32(Encoding.UTF8.GetBytes("NIFL"), 0);
            nifl.NIFLLength = 0x18;
            nifl.unkInt0 = 1;
            nifl.offsetAddition = 0x20;

            nifl.NOF0Offset = NOF0Offset;
            nifl.NOF0OffsetFull = NOF0Offset + 0x20;
            nifl.NOF0BlockSize = NOF0FullSize;
            nifl.padding0 = 0;

            //Write NIFL
            outBytes.InsertRange(0, DataHelpers.ConvertStruct(nifl));

            return outBytes.ToArray();
        }

        private static void AddNIFLCOLBytes(NIFL_COLObject col, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            DataHelpers.AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, col.textString);

            outBytes.AddRange(DataHelpers.ConvertStruct(col.niflCol));
        }

        private static void AddHAIRBytes(HAIRObject hair, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            DataHelpers.AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, hair.dataString);
            DataHelpers.AddNIFLText(outBytes.Count + 0x8, nof0PointerLocations, textAddressDict, textList, hair.texString1);
            DataHelpers.AddNIFLText(outBytes.Count + 0xC, nof0PointerLocations, textAddressDict, textList, hair.texString2);
            DataHelpers.AddNIFLText(outBytes.Count + 0x10, nof0PointerLocations, textAddressDict, textList, hair.texString3);
            DataHelpers.AddNIFLText(outBytes.Count + 0x14, nof0PointerLocations, textAddressDict, textList, hair.texString4);
            DataHelpers.AddNIFLText(outBytes.Count + 0x18, nof0PointerLocations, textAddressDict, textList, hair.texString5);
            DataHelpers.AddNIFLText(outBytes.Count + 0x1C, nof0PointerLocations, textAddressDict, textList, hair.texString6);
            DataHelpers.AddNIFLText(outBytes.Count + 0x20, nof0PointerLocations, textAddressDict, textList, hair.texString7);

            outBytes.AddRange(DataHelpers.ConvertStruct(hair.hair));
        }

        private static void AddEYEBBytes(EYEBObject eyeB, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            DataHelpers.AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, eyeB.texString1);
            DataHelpers.AddNIFLText(outBytes.Count + 0x8, nof0PointerLocations, textAddressDict, textList, eyeB.texString2);
            DataHelpers.AddNIFLText(outBytes.Count + 0xC, nof0PointerLocations, textAddressDict, textList, eyeB.texString3);
            DataHelpers.AddNIFLText(outBytes.Count + 0x10, nof0PointerLocations, textAddressDict, textList, eyeB.texString4);

            outBytes.AddRange(DataHelpers.ConvertStruct(eyeB.eyeb));
        }

        private static void AddSkinBytes(NGS_SKINObject skin, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            DataHelpers.AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, skin.texString1);
            DataHelpers.AddNIFLText(outBytes.Count + 0x8, nof0PointerLocations, textAddressDict, textList, skin.texString2);
            DataHelpers.AddNIFLText(outBytes.Count + 0xC, nof0PointerLocations, textAddressDict, textList, skin.texString3);
            DataHelpers.AddNIFLText(outBytes.Count + 0x10, nof0PointerLocations, textAddressDict, textList, skin.texString4);
            DataHelpers.AddNIFLText(outBytes.Count + 0x14, nof0PointerLocations, textAddressDict, textList, skin.texString5);
            DataHelpers.AddNIFLText(outBytes.Count + 0x18, nof0PointerLocations, textAddressDict, textList, skin.texString6);
            DataHelpers.AddNIFLText(outBytes.Count + 0x1C, nof0PointerLocations, textAddressDict, textList, skin.texString7);
            DataHelpers.AddNIFLText(outBytes.Count + 0x20, nof0PointerLocations, textAddressDict, textList, skin.texString8);
            DataHelpers.AddNIFLText(outBytes.Count + 0x24, nof0PointerLocations, textAddressDict, textList, skin.texString9);
            DataHelpers.AddNIFLText(outBytes.Count + 0x28, nof0PointerLocations, textAddressDict, textList, skin.texString10);

            outBytes.AddRange(DataHelpers.ConvertStruct(skin.ngsSkin));
        }

        private static void AddHornBytes(NGS_HornObject horn, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            DataHelpers.AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, horn.dataString);

            outBytes.AddRange(DataHelpers.ConvertStruct(horn.ngsHorn));
        }

        private static void AddTeethBytes(NGS_TeethObject teeth, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            DataHelpers.AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, teeth.dataString);
            DataHelpers.AddNIFLText(outBytes.Count + 0x8, nof0PointerLocations, textAddressDict, textList, teeth.texString1);
            DataHelpers.AddNIFLText(outBytes.Count + 0xC, nof0PointerLocations, textAddressDict, textList, teeth.texString2);
            DataHelpers.AddNIFLText(outBytes.Count + 0x10, nof0PointerLocations, textAddressDict, textList, teeth.texString3);
            DataHelpers.AddNIFLText(outBytes.Count + 0x14, nof0PointerLocations, textAddressDict, textList, teeth.texString4);

            outBytes.AddRange(DataHelpers.ConvertStruct(teeth.ngsTeeth));
        }

        private static void AddEARBytes(NGS_EarObject ear, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            DataHelpers.AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, ear.dataString);
            DataHelpers.AddNIFLText(outBytes.Count + 0x8, nof0PointerLocations, textAddressDict, textList, ear.texString1);
            DataHelpers.AddNIFLText(outBytes.Count + 0xC, nof0PointerLocations, textAddressDict, textList, ear.texString2);
            DataHelpers.AddNIFLText(outBytes.Count + 0x10, nof0PointerLocations, textAddressDict, textList, ear.texString3);
            DataHelpers.AddNIFLText(outBytes.Count + 0x14, nof0PointerLocations, textAddressDict, textList, ear.texString4);
            DataHelpers.AddNIFLText(outBytes.Count + 0x18, nof0PointerLocations, textAddressDict, textList, ear.texString5);

            outBytes.AddRange(DataHelpers.ConvertStruct(ear.ngsEar));
        }

        private static void AddEYEBytes(EYEObject eye, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            DataHelpers.AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, eye.texString1);
            DataHelpers.AddNIFLText(outBytes.Count + 0x8, nof0PointerLocations, textAddressDict, textList, eye.texString2);
            DataHelpers.AddNIFLText(outBytes.Count + 0xC, nof0PointerLocations, textAddressDict, textList, eye.texString3);
            DataHelpers.AddNIFLText(outBytes.Count + 0x10, nof0PointerLocations, textAddressDict, textList, eye.texString4);
            DataHelpers.AddNIFLText(outBytes.Count + 0x14, nof0PointerLocations, textAddressDict, textList, eye.texString5);

            outBytes.AddRange(DataHelpers.ConvertStruct(eye.eye));
        }

        private static void AddACCEBytes(ACCEObject acce, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            DataHelpers.AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, acce.dataString);
            DataHelpers.AddNIFLText(outBytes.Count + 0x8, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach1);
            DataHelpers.AddNIFLText(outBytes.Count + 0xC, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach2);
            DataHelpers.AddNIFLText(outBytes.Count + 0x10, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach3);
            DataHelpers.AddNIFLText(outBytes.Count + 0x14, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach4);
            if (mode >= 1)
            {
                DataHelpers.AddNIFLText(outBytes.Count + 0x18, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach5);
                DataHelpers.AddNIFLText(outBytes.Count + 0x1C, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach6);
                DataHelpers.AddNIFLText(outBytes.Count + 0x20, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach7);
                DataHelpers.AddNIFLText(outBytes.Count + 0x24, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach8);
            }
            else
            {
                //Reordering happened at some point
                DataHelpers.AddNIFLText(outBytes.Count + 0x18, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach6);
                DataHelpers.AddNIFLText(outBytes.Count + 0x1C, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach7);
                DataHelpers.AddNIFLText(outBytes.Count + 0x20, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach8);
                DataHelpers.AddNIFLText(outBytes.Count + 0x24, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach9);
            }

            outBytes.AddRange(DataHelpers.ConvertStruct(acce.acce));
            if (mode >= 1)
            {
                DataHelpers.AddNIFLText(outBytes.Count + 0x0, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach9);
                DataHelpers.AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach10);

                DataHelpers.AddNIFLText(outBytes.Count + 0x8, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach11);
                DataHelpers.AddNIFLText(outBytes.Count + 0xC, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach12);
                DataHelpers.AddNIFLText(outBytes.Count + 0x10, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach13);
                DataHelpers.AddNIFLText(outBytes.Count + 0x14, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach14);

                DataHelpers.AddNIFLText(outBytes.Count + 0x18, nof0PointerLocations, textAddressDict, textList, acce.nodeAttach15);
                outBytes.AddRange(DataHelpers.ConvertStruct(acce.acceFeb8_22));
            }
            outBytes.AddRange(DataHelpers.ConvertStruct(acce.acceB));
            outBytes.AddRange(DataHelpers.ConvertStruct(acce.acce2a));
            if (mode >= 1)
            {
                outBytes.AddRange(BitConverter.GetBytes(acce.flt_54));
            }
            outBytes.AddRange(DataHelpers.ConvertStruct(acce.acce2b));
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
            if (mode >= 1)
            {
                outBytes.AddRange(BitConverter.GetBytes(acce.flt_90));
            }
        }

        private static void AddFCPBytes(FCPObject face, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            DataHelpers.AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, face.texString1);
            DataHelpers.AddNIFLText(outBytes.Count + 0x8, nof0PointerLocations, textAddressDict, textList, face.texString2);
            DataHelpers.AddNIFLText(outBytes.Count + 0xC, nof0PointerLocations, textAddressDict, textList, face.texString3);
            DataHelpers.AddNIFLText(outBytes.Count + 0x10, nof0PointerLocations, textAddressDict, textList, face.texString4);

            outBytes.AddRange(DataHelpers.ConvertStruct(face.fcp));
        }

        private static void AddFaceTexturesBytes(FaceTextureObject face, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            DataHelpers.AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, face.texString1);
            DataHelpers.AddNIFLText(outBytes.Count + 0x8, nof0PointerLocations, textAddressDict, textList, face.texString2);
            DataHelpers.AddNIFLText(outBytes.Count + 0xC, nof0PointerLocations, textAddressDict, textList, face.texString3);
            DataHelpers.AddNIFLText(outBytes.Count + 0x10, nof0PointerLocations, textAddressDict, textList, face.texString4);

            outBytes.AddRange(DataHelpers.ConvertStruct(face.ngsFace));
        }

        private static void AddFCMNBytes(FCMNObject face, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            DataHelpers.AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, face.proportionAnim);
            DataHelpers.AddNIFLText(outBytes.Count + 0x8, nof0PointerLocations, textAddressDict, textList, face.faceAnim1);
            DataHelpers.AddNIFLText(outBytes.Count + 0xC, nof0PointerLocations, textAddressDict, textList, face.faceAnim2);
            DataHelpers.AddNIFLText(outBytes.Count + 0x10, nof0PointerLocations, textAddressDict, textList, face.faceAnim3);
            DataHelpers.AddNIFLText(outBytes.Count + 0x14, nof0PointerLocations, textAddressDict, textList, face.faceAnim4);
            DataHelpers.AddNIFLText(outBytes.Count + 0x18, nof0PointerLocations, textAddressDict, textList, face.faceAnim5);
            DataHelpers.AddNIFLText(outBytes.Count + 0x1C, nof0PointerLocations, textAddressDict, textList, face.faceAnim6);
            DataHelpers.AddNIFLText(outBytes.Count + 0x20, nof0PointerLocations, textAddressDict, textList, face.faceAnim7);
            DataHelpers.AddNIFLText(outBytes.Count + 0x24, nof0PointerLocations, textAddressDict, textList, face.faceAnim8);
            DataHelpers.AddNIFLText(outBytes.Count + 0x28, nof0PointerLocations, textAddressDict, textList, face.faceAnim9);
            DataHelpers.AddNIFLText(outBytes.Count + 0x2C, nof0PointerLocations, textAddressDict, textList, face.faceAnim10);

            outBytes.AddRange(DataHelpers.ConvertStruct(face.fcmn));
        }

        private static void AddFACEBytes(FACEObject face, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            DataHelpers.AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, face.dataString);
            DataHelpers.AddNIFLText(outBytes.Count + 0x8, nof0PointerLocations, textAddressDict, textList, face.texString1);
            DataHelpers.AddNIFLText(outBytes.Count + 0xC, nof0PointerLocations, textAddressDict, textList, face.texString2);
            DataHelpers.AddNIFLText(outBytes.Count + 0x10, nof0PointerLocations, textAddressDict, textList, face.texString3);
            DataHelpers.AddNIFLText(outBytes.Count + 0x14, nof0PointerLocations, textAddressDict, textList, face.texString4);
            DataHelpers.AddNIFLText(outBytes.Count + 0x18, nof0PointerLocations, textAddressDict, textList, face.texString5);
            DataHelpers.AddNIFLText(outBytes.Count + 0x1C, nof0PointerLocations, textAddressDict, textList, face.texString6);

            outBytes.AddRange(DataHelpers.ConvertStruct(face.face));
            if (mode >= 1)
            {
                outBytes.AddRange(DataHelpers.ConvertStruct(face.faceRitem));
            }
            var face2Temp = face.face2;

            //Backwards compatibility
            if (mode == 0)
            {
                face2Temp.unkInt0 = face.faceRitem.unkIntRT0;
                face2Temp.unkInt1 = face.faceRitem.unkIntRT1 + (face.face2.unkInt0 * 0x10000);
            }
            outBytes.AddRange(DataHelpers.ConvertStruct(face2Temp));
            if (mode >= 1)
            {
                outBytes.AddRange(BitConverter.GetBytes(face.unkFloatRitem));
            }
        }

        private static void AddStickerBytes(StickerObject sticker, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            DataHelpers.AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, sticker.texString);
            outBytes.AddRange(DataHelpers.ConvertStruct(sticker.sticker));
        }

        private static void AddBDPBytes(BBLYObject bbly, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            DataHelpers.AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, bbly.texString1);
            DataHelpers.AddNIFLText(outBytes.Count + 0x8, nof0PointerLocations, textAddressDict, textList, bbly.texString2);
            DataHelpers.AddNIFLText(outBytes.Count + 0xC, nof0PointerLocations, textAddressDict, textList, bbly.texString3);
            DataHelpers.AddNIFLText(outBytes.Count + 0x10, nof0PointerLocations, textAddressDict, textList, bbly.texString4);
            DataHelpers.AddNIFLText(outBytes.Count + 0x14, nof0PointerLocations, textAddressDict, textList, bbly.texString5);
            outBytes.AddRange(DataHelpers.ConvertStruct(bbly.bbly));
        }

        private static void AddBodyBytes(BODYObject body, int mode, List<byte> outBytes, List<int> nof0PointerLocations, Dictionary<string, List<int>> textAddressDict, List<string> textList)
        {
            DataHelpers.AddNIFLText(outBytes.Count + 0x4, nof0PointerLocations, textAddressDict, textList, body.dataString);
            DataHelpers.AddNIFLText(outBytes.Count + 0x8, nof0PointerLocations, textAddressDict, textList, body.texString1);
            DataHelpers.AddNIFLText(outBytes.Count + 0xC, nof0PointerLocations, textAddressDict, textList, body.texString2);
            DataHelpers.AddNIFLText(outBytes.Count + 0x10, nof0PointerLocations, textAddressDict, textList, body.texString3);
            DataHelpers.AddNIFLText(outBytes.Count + 0x14, nof0PointerLocations, textAddressDict, textList, body.texString4);
            DataHelpers.AddNIFLText(outBytes.Count + 0x18, nof0PointerLocations, textAddressDict, textList, body.texString5);
            DataHelpers.AddNIFLText(outBytes.Count + 0x1C, nof0PointerLocations, textAddressDict, textList, body.texString6);
            outBytes.AddRange(DataHelpers.ConvertStruct(body.body));
            if (mode >= 1)
            {
                outBytes.AddRange(DataHelpers.ConvertStruct(body.bodyMaskColorMapping));
            }
            outBytes.AddRange(DataHelpers.ConvertStruct(body.body2));
            if (mode >= 1)
            {
                outBytes.AddRange(DataHelpers.ConvertStruct(body.body40cap));
            }
        }
        #endregion
    }
}
