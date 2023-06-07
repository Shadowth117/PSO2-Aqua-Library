using System.Collections.Generic;

namespace AquaModelLibrary
{
    //Though the NIFL format is used for storage, VTBF format tag references for data will be commented where appropriate. Some offset/reserve related things are NIFL only, however.
    public unsafe class CharacterMakingIndex : AquaCommon
    {
        public static int oct21TableAddressInt = 0x2318B4; //Used for checking the version of the cmx in order to maintain legacy support
        public static int dec14_21TableAddressInt = 0x26B66C; //Ritem Update cmx. Some structs were reordered for this update.
        public static int feb8_22TableAddressInt = 0x2DAFD0; //Lv 40 update cmx
        public static int jun7_22TableAddressInt = 0x2F6C44; //Kvaris update cmx
        public static int aug17_22TableAddressInt = 0x307D6C; //August 17th 2022 update cmx
        public static int oct4_22TableAddressInt = 0x00320b1C; //October 5th 2022 update cmx
        public static int jan25_23TableAddressInt = 0x0034689C; //January 25th 2023 update cmx
        public static int ver2TableAddressInt = 0x0039B5EC; //Version 2, June 6th 2023 update cmx

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
        public static string classicStart = $"character/making/pl_";
        public static string rebootStart = $"character/making_reboot/pl_";
        public static string rebootExStart = $"character/making_reboot_ex/pl_";
        public static string playerVoiceStart = $"player_voice/";
        public static string icon = $"large_icon/ui_making_";
        public static string lobbyActionStart = $"actor/lobby_action/";
        public static string characterStart = $"character/";
        public static string pvpStart = $"character_poka/";
        public static string lobbyActionStartReboot = $"character/motion/lobby_action/";
        public static string substituteMotion = $"character/motion/substitute_motion/pl_sb_";
        public static string magItem = $"item/mag/it_mg_";

        public static string basewearIcon = $"basewear01_";
        public static string bodyPaintIcon = $"bodypaint01_";
        public static string stickerIcon = $"bodypaint02_";
        public static string costumeIcon = $"costume01_";
        public static string castPartIcon = $"costume02_"; //No extra string for body. Best to check if both costume01 and costume02 exist for costumes since they're not
                                                           //always differentiated well (cast only costumes etc.)
        public static string castArmIcon = $"arm_";
        public static string castLegIcon = $"leg_";
        public static string accessoryIcon = $"decoy01_";
        public static string earIcon = $"ears01_";
        public static string eyeIcon = $"eye01_";
        public static string eyeBrowsIcon = $"eyebrows01_";
        public static string eyelashesIcon = $"eyelashes01_";
        public static string faceIcon = $"face01_";             //aka facepaint01
        public static string facePaintIcon = $"facepaint02_";
        public static string hairIcon = $"hair01_";
        public static string hornIcon = $"horn01_";
        public static string innerwearIcon = $"innerwear01_";
        public static string outerwearIcon = $"outerwear01_";
        public static string teethIcon = $"dental01_";
        public static string skinIcon = $"skin01_";

        public static string iconCast = "cast_"; //Only used for hair? Idk
        public static string iconMale = "man_";
        public static string iconFemale = "woman_";

        public static string voiceCman = "11_voice_cman";
        public static string voiceCwoman = "11_voice_cwoman";
        public static string voiceMan = "11_voice_man";
        public static string voiceWoman = "11_voice_woman";

        public static string rebootHornDataStr = "pl_rhn_";
        public static string rebootTeethDataStr = "pl_rdt_";
        public static string rebootEarDataStr = "pl_rea_";

        public static string rebootLAHuman = "_std";
        public static string rebootLACastMale = "_cam";
        public static string rebootLACastFemale = "_caf";
        public static string rebootFig = "_base";

        public static string subSwim = "swim_";
        public static string subGlide = "glide_";
        public static string subJump = "jump_";
        public static string subLanding = "landing_";
        public static string subMove = "mov_";
        public static string subSprint = "sprint_";
        public static string subIdle = "idle_";

        public static string classicCMX = $"character/making/pl_system.ice";
        public static string classicColor = $"character/making/pl_system_color.ice";
        public static string classicCollect = $"character/making/pl_system_collect.ice";
        public static string classicPartText = $"ui_charamake_text.ice";
        public static string classicAcceText = $"ui_fashion_text.ice";
        public static string classicCharCreate = $"ui_character_making.ice";
        public static string classicCommon = $"sy_common_text.ice";
        public static string classicLobbyAction = $"actor/lobby_action/pl_lobby_action_setting.ice";
        public static string classicActorName = $"actor/ac_name_text.ice";
        public static string rebootActorName = $"actor/ac_name_text.ice";
        public static string rebootActorNameNPC = $"npc/np_npc_actor_name.ice"; //Use this for actual cml names. Other one
        public static string rebootLobbyAction = $"lobby_action/pl_lobby_action_setting.ice";

        public static string partsTextName = "ui_charamake_parts.text";
        public static string acceTextName = "ui_accessories_text.text";
        public static string commonTextName = "common.text";
        public static string actorNameName = "actor_name.text";
        public static string actorNameNPCName = "actor_name_npc.text";
        public static string faceVarName = "face_variation.cmp.lua";
        public static string lacName = "lobby_action_setting.lac";
        public static string accessoryCostName = "pl_accessory.cco";
        public static string cmtName = "pl_making_table.cmt";
        public static string cmxName = "pl_data_info.cmx";

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

        public CMXTable cmxTable;

        public class BaseCMXObject
        {
            public int num;
            public long originalOffset;
        }

        public class BODYObject : BaseCMXObject
        {
            public BODY body;
            public BODYRitem bodyRitem;
            public BODY2 body2;
            public BODY40Cap body40cap;
            public BODY2023_1 body2023_1;
            public BODYVer2 bodyVer2;
            public string dataString;
            public string texString1;
            public string texString2;
            public string texString3;
            public string texString4;
            public string texString5;
            public string texString6;
            public string nodeString0;
            public string nodeString1;
            public string nodeString2;
        }

        public struct BODY
        {
            public int id; //0xFF, 0x8
            public int dataStringPtr; //Name of the aqp, aqn, fltd, etc.
            public int texString1Ptr;
            public int texString2Ptr;

            public int texString3Ptr;
            public int texString4Ptr;
            public int texString5Ptr;
            public int texString6Ptr;

            public int int_20;
        }

        public struct BODYRitem //Body struct section addition added
        {
            public int int_0;
            public int int_4;
            public int int_8;
            public int int_C;
        }

        public struct BODY2
        {
            public int int_24_0x9_0x9;   //0x9, 0x9
            public int int_28;
            public int int_2C;           //One byte of this is a set of bitflags for parts to hide if it's outer wear. They follow the order of basewear ids.

            public int costumeSoundId;   //0xA, 0x8
            public int reference_id;  //0xD, 0x8 //Contains the id for a linked part, such as madoka's hair or bask repca's helmet
            public int int_38;         //Usually -1
            public int int_3C;         //Usually -1

            public int linkedInnerId;         //Usually -1
            public int int_44;         //Usually -1
            public float legLength;   //0x8, 0xA
            public float float_4C_0xB;   //0xB, 0xA

            public float float_50;
            public float float_54;
            public float float_58;
            public float float_5C;

            public float float_60;
            public int int_64;
        }

        public struct BODY40Cap //Added 2/9 update
        {
            public float float_78;
            public float float_7C;
        }

        public struct BODY2023_1
        {
            public int nodeStrPtr_0;
            public int nodeStrPtr_1;
        }

        public struct BODYVer2
        {
            public int nodeStrPtr_2;
            public float flt_8C;
            public float flt_90;
            public float flt_94;
            public float flt_98;
        }

        public class BBLYObject : BaseCMXObject
        {
            public BBLY bbly;
            public string texString1;
            public string texString2;
            public string texString3;

            public string texString4;
            public string texString5;
        }

        //BBLY, BDP1
        public struct BBLY
        {
            public int id; //0xFF, 0x8
            public int texString1Ptr;
            public int texString2Ptr;
            public int texString3Ptr;

            public int texString4Ptr;
            public int texString5Ptr;
            public int unkInt0;       //This value forward may just be junk or new functionality in this version. Old BBLY only had the id and texture strings. Seems to correlate to BODY stuff though.
            public int unkInt1;

            public int unkInt2;
            public int unkInt3;
            public float unkFloat0;
            public float unkFloat1;

            public int unkInt4;
            public int unkInt5;
            public float unkFloat2;
            public float unkFloat3;

            public float unkFloat4;
            public float unkFloat5;
        }

        //BDP2(stickers)
        public class StickerObject : BaseCMXObject
        {
            public Sticker sticker;
            public string texString;
        }

        public struct Sticker
        {
            public int id;
            public int texStringPtr;
            public int reserve0;
        }

        public class FACEObject : BaseCMXObject
        {
            public FACE face;
            public FACERitem faceRitem;
            public FACE2 face2;
            public float unkFloatRitem;
            public int unkVer2Int;

            public string dataString;
            public string texString1;
            public string texString2;

            public string texString3;
            public string texString4;
            public string texString5;
            public string texString6;
        }

        public struct FACE
        {
            public int id; //0xFF, 0x8
            public int dataStringPtr; //Name of the aqp, aqn, fltd, etc.
            public int texString1Ptr;
            public int texString2Ptr;

            public int texString3Ptr;
            public int texString4Ptr;
            public int texString5Ptr;
            public int texString6Ptr;
        }

        public struct FACERitem
        {
            public int unkIntRT0;
            public int unkIntRT1;
        }

        public struct FACE2
        {
            public int unkInt0;
            public int unkInt1;
            public int unkInt2;
            public int unkInt3;

            public int unkInt4;
            public float unkFloat0;
            public float unkFloat1;
            public float unkFloat2;

            public float unkFloat3;
            public int unkInt5;
            public int unkInt6;
            public int unkInt7;

            public int unkInt8;
            public int unkInt9;
            public int unkInt10;
            public int unkInt11;
        }

        public class FCMNObject : BaseCMXObject
        {
            public FCMN fcmn;
            public string proportionAnim;
            public string faceAnim1;
            public string faceAnim2;

            public string faceAnim3;
            public string faceAnim4;
            public string faceAnim5;
            public string faceAnim6;

            public string faceAnim7;
            public string faceAnim8;
            public string faceAnim9;
            public string faceAnim10;
        }

        public struct FCMN
        {
            public int id; //FF, 0x8
            public int proportionAnimPtr;
            public int faceAnim1Ptr;
            public int faceAnim2Ptr;

            public int faceAnim3Ptr;
            public int faceAnim4Ptr;
            public int faceAnim5Ptr;
            public int faceAnim6Ptr;

            public int faceAnim7Ptr;
            public int faceAnim8Ptr;
            public int faceAnim9Ptr;
            public int faceAnim10Ptr;
        }

        public class FaceTextureObject : BaseCMXObject
        {
            public FaceTextures ngsFace;
            public string texString1;
            public string texString2;
            public string texString3;

            public string texString4;
        }

        public struct FaceTextures
        {
            public int id;
            public int texString1Ptr;
            public int texString2Ptr;
            public int texString3Ptr;

            public int texString4Ptr;
        }

        public class FCPObject : BaseCMXObject
        {
            public FCP fcp;
            public string texString1;
            public string texString2;
            public string texString3;

            public string texString4;
        }

        public struct FCP
        {
            public int id;
            public int texString1Ptr;
            public int texString2Ptr;
            public int texString3Ptr;

            public int texString4Ptr;
            public int unkInt0;
            public int unkInt1;
            public int unkInt2;

            public int unkInt3;
            public int unkInt4;
            public int unkInt5;
        }

        public class ACCEObject : BaseCMXObject
        {
            public ACCE acce;
            public ACCE_B acceB;
            public ACCE_Feb8_22 acceFeb8_22;
            public ACCE2A acce2a;
            public float flt_54;
            public ACCE2B acce2b;
            public List<ACCE_12Object> acce12List = new List<ACCE_12Object>();
            public ACCEV2 accev2;
            public int effectNamePtr;
            public float flt_90;

            public string dataString;
            public string nodeAttach1;
            public string nodeAttach2;

            public string nodeAttach3;
            public string nodeAttach4;
            public string nodeAttach5;
            public string nodeAttach6;

            public string nodeAttach7;
            public string nodeAttach8;

            //Feb 8 2022 strings
            public string nodeAttach9;
            public string nodeAttach10;

            public string nodeAttach11;
            public string nodeAttach12;
            public string nodeAttach13;
            public string nodeAttach14;

            public string nodeAttach15;

            //Ver2 strings
            public string nodeAttach16;
            public string nodeAttach17;
            public string nodeAttach18;

            public string effectName;
        }

        public struct ACCE
        {
            public int id;
            public int dataStringPtr; //Name of the aqp, aqn, fltd, etc.
            public int nodeAttach1Ptr; //These pointers are to bone names the accessory uses when attaching by default in PSO2 Classic seemingly
            public int nodeAttach2Ptr;

            public int nodeAttach3Ptr;
            public int nodeAttach4Ptr;
            public int nodeAttach5Ptr;
            public int nodeAttach6Ptr;

            public int nodeAttach7Ptr;
            public int nodeAttach8Ptr;
        }

        public struct ACCE_Feb8_22
        {
            public int acceString9Ptr;
            public int acceString10Ptr;

            public int acceString11Ptr;
            public int acceString12Ptr;
            public int acceString13Ptr;
            public int acceString14Ptr;

            public int acceString15Ptr;
        }

        public struct ACCE_B
        {
            public int unkInt0;           //Often 0x1
            public int unkInt1;           //Often 0x64

            public int unkInt2;
            public int unkInt3;
            public int unkInt4;
            public int unkInt5;

            public int unkInt6;
        }

        public struct ACCE2A
        {
            public float unkFloatNeg1;
            public float unkFloat0;
            public float unkFloat1;

            public float unkFloat2;
            public float unkFloat3;
            public float unkFloat4;
            public float unkFloat5;
        }
        public struct ACCE2B
        {
            public int unkInt8;
            public int unkInt9;
            public int unkInt10;
        }

        public struct ACCEV2
        {
            public int acceString16Ptr;
            public int acceString17Ptr;
            public int acceString18Ptr;
        }

        public class ACCE_12Object
        {
            public float unkFloat0;
            public float unkFloat1;
            public int unkInt0;
            public int unkInt1;

            public int unkIntFeb822_0;

            public short unkShort0;
            public short unkShort1; //0xD7, 0x6 on 1st, 0xC7, 0x6
            public short unkShort2;
            public short unkShort3;

            public int unkIntFeb822_1;
        }

        public class EYEObject : BaseCMXObject
        {
            public EYE eye;
            public string texString1;
            public string texString2;
            public string texString3;

            public string texString4;
            public string texString5;
        }

        public struct EYE
        {
            public int id;
            public int texString1Ptr;
            public int texString2Ptr;
            public int texString3Ptr;

            public int texString4Ptr;
            public int texString5Ptr;
            public float unkFloat0;
            public float unkFloat1;
        }

        public class NGS_EarObject : BaseCMXObject
        {
            public NGS_Ear ngsEar;

            public string dataString;
            public string texString1;
            public string texString2;
            public string texString3;

            public string texString4;
            public string texString5;
        }

        public struct NGS_Ear
        {
            public int id;
            public int dataStringPtr;
            public int texString1Ptr;
            public int texString2Ptr;

            public int texString3Ptr;
            public int texString4Ptr;
            public int texString5Ptr;
            public int unkInt0;

            public int unkInt1;
            public int unkInt2;
            public int unkInt3;
            public int unkInt4;
        }

        public class NGS_TeethObject : BaseCMXObject
        {
            public NGS_Teeth ngsTeeth;

            public string dataString;
            public string texString1;
            public string texString2;
            public string texString3;

            public string texString4;
        }

        public struct NGS_Teeth
        {
            public int id;
            public int dataStringPtr;
            public int texString1Ptr;
            public int texString2Ptr;

            public int texString3Ptr;
            public int texString4Ptr;
        }

        public class NGS_HornObject : BaseCMXObject
        {
            public NGS_Horn ngsHorn;

            public string dataString;
        }

        public struct NGS_Horn
        {
            public int id;
            public int dataStringPtr; //Name of the aqp, aqn, fltd, etc.
            public int reserve0;      //Always 0 so far.
        }

        public class NGS_SKINObject : BaseCMXObject
        {
            public NGS_Skin ngsSkin;
            public string texString1;
            public string texString2;
            public string texString3;

            public string texString4;
            public string texString5;
            public string texString6;
            public string texString7;
            public string texString8;
            public string texString9;
            public string texString10;
        }

        public struct NGS_Skin
        {
            public int id;
            public int texString1Ptr;
            public int texString2Ptr;
            public int texString3Ptr;

            public int texString4Ptr;
            public int texString5Ptr;
            public int texString6Ptr;
            public int texString7Ptr;

            public int texString8Ptr;
            public int texString9Ptr;
            public int texString10Ptr;
            public byte unkByte0;
            public byte unkByte1;
            public byte unkByte2;
            public byte unkByte3;
        }

        public class EYEBObject : BaseCMXObject
        {
            public EYEB eyeb;
            public string texString1;
            public string texString2;
            public string texString3;

            public string texString4;
        }

        //Also for EYEL
        public struct EYEB
        {
            public int id;
            public int texString1Ptr;
            public int texString2Ptr;
            public int texString3Ptr;

            public int texString4Ptr;
        }

        public class HAIRObject : BaseCMXObject
        {
            public HAIR hair;
            public string dataString;
            public string texString1;
            public string texString2;

            public string texString3;
            public string texString4;
            public string texString5;
            public string texString6;

            public string texString7;
        }

        public struct HAIR
        {
            public int id;
            public int dataStringPtr; //Name of the aqp, aqn, fltd, etc.
            public int texString1Ptr;
            public int texString2Ptr;

            public int texString3Ptr;
            public int texString4Ptr;
            public int texString5Ptr;
            public int texString6Ptr;

            public int texString7Ptr;
            public int unkIntB1;      //B0, 0x9
            public int unkInt1;
            public int unkInt2;

            public float unkFloat0;
            public float unkFloat1;
            public float unkFloat2;
            public float unkFloat3;

            public int unkInt3;
            public int unkInt4;
            public int unkInt5;
            public int unkInt6;

            public int unkInt7;
            public int unkInt8;
            public float unkFloat4;
            public float unkFloat5;

            public float unkFloat6;
            public int unkInt9;
            public int unkInt10;
            public int unkInt11;

            public int unkInt12;
            public int unkInt13;
            public int unkInt14;
            public float unkFloat7;

            public float unkFloat8;
            public float unkFloat9;
            public int unkInt15;
            public int unkInt16;

            public int unkInt17;
            public int unkInt18;
            public int unkInt19;
            public int unkInt20;

            public short unkShortB1; //0xB1, 0x6
            public short unkShortB2; //0xB2, 0x6
            public short unkShortB3; //0xB3, 0x6
            public short unkShort0;
        }

        public class VTBF_COLObject : BaseCMXObject
        {
            public VTBF_COL vtbfCol;
            public string utf8Name;
            public string utf16Name;
        }

        //Character color data ranges as PixelFormat.Format32bppRgb. 21 x 6 pixels. Every 3 columns is one area, but only the middle column seems used ingame.
        //Left and right columns seem like they may have been related to either ingame shading, as the deuman one does not go nearly as dark as the others, but it's hard to say.
        public class VTBF_COL
        {
            public int id;
            public byte[] utf8String;
            public byte[] utf16String;
            public byte[] colorData;   //Should be 0x1F8 bytes. 
        }

        public class NIFL_COLObject : BaseCMXObject
        {
            public NIFL_COL niflCol;
            public string textString;
        }

        //Character color data ranges as PixelFormat.Format32bppRgb. 7 x 6 pixels. Same main data as old VTBF_COL, just without the vestigial ranges.
        public unsafe struct NIFL_COL
        {
            public int id;
            public int textStringPtr;
            public fixed byte colorData[0xA8];
        }

        //This may be color related. But I have no idea what it's supposed to do.
        public unsafe struct Unk_IntField
        {
            public fixed int unkIntField[0x79];
        }

        public class BCLNObject
        {
            public BCLN bcln;
            public BCLNRitem bclnRitem;
            public BCLNRitem2 bclnRitem2;
        }

        //Also for LCLN, ACLN, and ICLN. Id remapping info. Recolor outfits have multiple ids, but only one id that will actually correlate to a file.
        public struct BCLN
        {
            public int id;
            public int fileId;
            public int unkInt;
        }

        public struct BCLNRitem
        {
            public int int_00;
            public int int_04;
            public int int_08;
            public int int_0C;
        }

        //Added sometime before April 3 2022
        public struct BCLNRitem2
        {
            public int int_00;
            public int int_04;
        }

        public class Part6_7_22Obj
        {
            public Part6_7_22 partStruct;
        }

        public struct Part6_7_22
        {
            public int id;
            public int modelId; //Guess on this
            public int int_08;
            public int int_0C; //The next 4 are always 1000 as of writing

            public int int_10;
            public int int_14;
            public int int_18; //End 1000
            public int int_1C;

            public int int_20;
        }

        public class CMXTable
        {
            public int bodyAddress; //BODY Costumes
            public int carmAddress; //CARM Cast Arms
            public int clegAddress; //CLEG Cast Legs
            public int bodyOuterAddress; //BODY Outer Wear

            public int baseWearAddress; //BCLN Base Wear
            public int innerWearAddress; //BBLY Inner Wear
            public int bodyPaintAddress; //BDP1 Body Paint 
            public int stickerAddress; //BDP2 Stickers

            public int faceAddress; //FACE All heads
            public int faceMotionAddress; //Face motions
            public int faceTextureAddress; //NGS Faces?
            public int faceTexturesAddress; //Face textures and face paint

            public int accessoryAddress; //ACCE Accessories
            public int eyeTextureAddress; //EYE eye textures
            public int earAddress; //reboot ears
            public int teethAddress; //reboot mouths

            public int hornAddress; //reboot horns
            public int skinAddress; //reboot and maybe classic skin?
            public int eyebrowAddress; //EYEB eyebrows
            public int eyelashAddress; //EYEL eyelashes

            public int hairAddress; //HAIR 
            public int colAddress; //COL, for color chart textures
            public int unkAddress; //Unknown arrays
            public int costumeIdLinkAddress; //BCLN Costume ids for recolors

            public int castArmIdLinkAddress; //BCLN Cast arm ids for recolors
            public int castLegIdLinkAddress; //BCLN Cast leg ids for recolors
            public int outerIdLinkAddress; //BCLN Outer ids for recolors                  Outer, basewear, and inner links are shifted down after oct 21 2021. This becomes CastHeadLinks
            public int baseWearIdLinkAddress; //BCLN basewear ids for recolors

            public int innerWearIdLinkAddress; //BCLN innerwear ids for recolors
            public int oct21UnkAddress; //Only in October 12, 2021 builds and forward
            public int jun7_22Address; //Only in June 7, 2022 builds and forward
            public int feb8_22UnkAddress; //Only in feb 8, 2022 builds and forwared

            public int bodyCount;
            public int carmCount;
            public int clegCount;
            public int bodyOuterCount;

            public int baseWearCount;
            public int innerWearCount;
            public int bodyPaintCount;
            public int stickerCount;

            public int faceCount;
            public int faceMotionCount;
            public int faceTextureCount;
            public int faceTexturesCount;

            public int accessoryCount;
            public int eyeTextureCount;
            public int earCount;
            public int teethCount;

            public int hornCount;
            public int skinCount;
            public int eyebrowCount;
            public int eyelashCount;

            public int hairCount;
            public int colCount;
            public int unkCount;
            public int costumeIdLinkCount;

            public int castArmIdLinkCount;
            public int castLegIdLinkCount;
            public int outerIdLinkCount;
            public int baseWearIdLinkCount;

            public int innerWearIdLinkCount;
            public int oct21UnkCount; //Only in October 12, 2021 builds and forward
            public int jun7_22Count; //Only in June 7, 2022 builds and forward
            public int feb8_22UnkCount; //Only in feb 8, 2022 builds and forwared
        }
    }
}
