using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AquaModelLibrary.AquaCommon;

namespace AquaModelLibrary
{
    //Though the NIFL format is used for storage, VTBF format tag references for data will be commented where appropriate. Some offset/reserve related things are NIFL only, however.
    public unsafe class CharacterMakingIndex : AquaCommon
    {
        public static string dataDir = $"data\\win32\\";
        public static string dataNADir = $"data\\win32_na\\";
        public static string dataReboot = $"data\\win32reboot\\";
        public static string dataRebootNA = $"data\\win32reboot_na\\";
        public static string classicStart = $"character/making/pl_";
        public static string rebootStart = $"character/making_reboot/pl_";
        public static string rebootExStart = $"character/making_reboot_ex/pl_";
        public static string playerVoiceStart = $"player_voice/";
        public static string icon = $"large_icon/ui_making_";
        public static string lobbyActionStart = $"actor/lobby_action/";
        public static string lobbyActionStartReboot = $"character/motion/lobby_action/";
        public static string substituteMotion = $"character/motion/substitute_motion/pl_sb_";

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
        public static string eyebrowsIcon = $"eyebrows01_";
        public static string eyelashesIcon = $"eyelashes01_";
        public static string faceIcon = $"face01_";             //aka facepaint01
        public static string facepainticon = $"facepaint02_";
        public static string hairIcon = $"hair01_";
        public static string hornIcon = $"horn01_";
        public static string innerwearIcon = $"innerwear01_";
        public static string outerwearIcon = $"outerwear01_";
        public static string teethIcon = $"dental01_";

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
        public static string classicPartText = $"ui_charamake_text.ice";
        public static string classicAcceText = $"ui_fashion_text.ice";
        public static string classicCharCreate = $"ui_character_making.ice";
        public static string classicCommon = $"sy_common_text.ice";
        public static string classicLobbyAction = $"actor/lobby_action/pl_lobby_action_setting.ice";
        public static string rebootLobbyAction = $"f94e8bfb6ee674e39fa6bc1aa697bf82";

        public static string partsTextName = "ui_charamake_parts.text";
        public static string acceTextName = "ui_accessories_text.text";
        public static string commonTextName = "common.text";
        public static string faceVarName = "face_variation.cmp.lua";
        public static string lacName = "lobby_action_setting.lac";

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
        public Dictionary<int, NGS_FACEObject> ngsFaceDict = new Dictionary<int, NGS_FACEObject>();
        public Dictionary<int, FCPObject> fcpDict = new Dictionary<int, FCPObject>();

        public Dictionary<int, ACCEObject> accessoryDict = new Dictionary<int, ACCEObject>();
        public Dictionary<int, EYEObject> eyeDict = new Dictionary<int, EYEObject>();
        public Dictionary<int, NGS_EarObject> ngsEarDict = new Dictionary<int, NGS_EarObject>();
        public Dictionary<int, NGS_TeethObject> ngsTeethDict = new Dictionary<int, NGS_TeethObject>();

        public List<NGS_HornObject> ngsHornList = new List<NGS_HornObject>(); //Necessary since there are unindexable structs in these. Unfortunate.
        public Dictionary<int, NGS_HornObject> ngsHornDict = new Dictionary<int, NGS_HornObject>();
        public Dictionary<int, NGS_SKINObject> ngsSkinDict = new Dictionary<int, NGS_SKINObject>();
        public Dictionary<int, EYEBObject> eyebrowDict = new Dictionary<int, EYEBObject>();
        public Dictionary<int, EYEBObject> eyelashDict = new Dictionary<int, EYEBObject>();

        public Dictionary<int, HAIRObject> hairDict = new Dictionary<int, HAIRObject>();
        public Dictionary<int, NIFL_COLObject> colDict = new Dictionary<int, NIFL_COLObject>();
        public List<Unk_IntField> unkList = new List<Unk_IntField>();
        public Dictionary<int, BCLN> costumeIdLink = new Dictionary<int, BCLN>();

        public Dictionary<int, BCLN> castArmIdLink = new Dictionary<int, BCLN>();
        public Dictionary<int, BCLN> clegIdLink = new Dictionary<int, BCLN>();
        public Dictionary<int, BCLN> outerWearIdLink = new Dictionary<int, BCLN>();
        public Dictionary<int, BCLN> baseWearIdLink = new Dictionary<int, BCLN>();

        public Dictionary<int, BCLN> innerWearIdLink = new Dictionary<int, BCLN>();

        public CMXTable cmxTable;

        public class BODYObject
        {
            public BODY body;
            public PSO2String dataString;
            public PSO2String texString1;
            public PSO2String texString2;
            public PSO2String texString3;
            public PSO2String texString4;
            public PSO2String texString5;
            public PSO2String texString6;
        }

        //Used for BODY, CARM, CLEG, Outer Wear (BODY), BCLN
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

            public int unkInt0;
            public int int_0x9_0x9;   //0x9, 0x9
            public int unkInt1;
            public int unkInt2;

            public int int_0xA_0x8;   //0xA, 0x8
            public int reference_id;  //0xD, 0x8
            public int fSet0;
            public int fSet1;

            public int fSet2;
            public int fSet3;
            public float flt_0x8;   //0x8, 0xA
            public float flt_0xB;   //0xB, 0xA

            public int unkInt3;
            public float unkFloat0;
            public float unkFloat1;
            public float unkFloat2;

            public float unkFloat3;
            public int unkInt4;
        }

        public class BBLYObject
        {
            public BBLY bbly;
            public PSO2String texString1;
            public PSO2String texString2;
            public PSO2String texString3;

            public PSO2String texString4;
            public PSO2String texString5;
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
        public class StickerObject
        {
            public Sticker sticker;
            public PSO2String texString;
        }

        public struct Sticker
        {
            public int id;
            public int texStringPtr;
            public int reserve0;
        }

        public class FACEObject
        {
            public FACE face;
            public PSO2String dataString;
            public PSO2String texString1;
            public PSO2String texString2;

            public PSO2String texString3;
            public PSO2String texString4;
            public PSO2String texString5;
            public PSO2String texString6;
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

        public class FCMNObject
        {
            public FCMN fcmn;
            public PSO2String proportionAnim;
            public PSO2String faceAnim1;
            public PSO2String faceAnim2;

            public PSO2String faceAnim3;
            public PSO2String faceAnim4;
            public PSO2String faceAnim5;
            public PSO2String faceAnim6;

            public PSO2String faceAnim7;
            public PSO2String faceAnim8;
            public PSO2String faceAnim9;
            public PSO2String faceAnim10;
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

        public class NGS_FACEObject
        {
            public NGS_FACE ngsFace;
            public PSO2String texString1;
            public PSO2String texString2;
            public PSO2String texString3;

            public PSO2String texString4;
        }

        public struct NGS_FACE
        {
            public int id;
            public int texString1Ptr;
            public int texString2Ptr;
            public int texString3Ptr;

            public int texString4Ptr;
        }

        public class FCPObject
        {
            public FCP fcp;
            public PSO2String texString1;
            public PSO2String texString2;
            public PSO2String texString3;

            public PSO2String texString4;
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

        public class ACCEObject
        {
            public ACCE acce;
            public PSO2String dataString;
            public PSO2String nodeAttach1;
            public PSO2String nodeAttach2;

            public PSO2String nodeAttach3;
            public PSO2String nodeAttach4;
            public PSO2String nodeAttach5;
            public PSO2String nodeAttach6;

            public PSO2String nodeAttach7;
            public PSO2String nodeAttach8;
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
            public int unkInt0;           //Often 0x1
            public int unkInt1;           //Often 0x64

            public int unkInt2;
            public int unkInt3;
            public int unkInt4;
            public int unkInt5;

            public int unkInt6;
            public int unkInt7;
            public float unkFloat0;
            public float unkFloat1;

            public float unkFloat2;
            public float unkFloat3;
            public float unkFloat4;
            public float unkFloat5;

            public int unkInt8;
            public int unkInt9;
            public int unkInt10;
            public float unkFloat6;

            public int unkInt11;
            public int unkInt12;
            public int unkInt13;
            public short unkShort0;
            public short unkShort_D7; //0xD7, 0x6

            public short unkShort2;
            public short unkShort3;
            public short unkShort4;
            public short unkshort5;
            public int unktIn14;
            public int unkInt15;

            public int unkInt16;
            public short unkShort6;
            public short unkShort_C7; //0xC7, 0x6
            public short unkShort8;
            public short unkShort9;
            public float unkFloat7;

            public float unkFloat8;
            public int unkInt18;
            public int unkInt19;
            public short unkShort10;
            public short unkShort11;

            public short unkShort12;
            public short unkShort13;
        }

        public class EYEObject
        {
            public EYE eye;
            public PSO2String texString1;
            public PSO2String texString2;
            public PSO2String texString3;

            public PSO2String texString4;
            public PSO2String texString5;
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

        public class NGS_EarObject
        {
            public NGS_Ear ngsEar;
            public NGS_Unk_Substruct subStruct;

            public PSO2String dataString;
            public PSO2String texString1;
            public PSO2String texString2;
            public PSO2String texString3;

            public PSO2String texString4;
            public PSO2String texString5;
        }

        public struct NGS_Ear
        {
            public int unkSubStructPtr; //Not sure what this is at the moment.
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

        //Weird 0xA long substruct for NGS_Ear and NGS_Horn
        public struct NGS_Unk_Substruct
        {
            public short unkShort0;
            public short unkShort1;
            public short unkShort2;
            public short unkShort3;
            public short unkShort4;
        }

        public class NGS_TeethObject
        {
            public NGS_Teeth ngsTeeth;
            public NGS_Unk_Substruct substruct;

            public PSO2String dataString;
            public PSO2String texString1;
            public PSO2String texString2;
            public PSO2String texString3;

            public PSO2String texString4;
        }

        public struct NGS_Teeth
        {
            public int unkSubStructPtr; //Not sure what this is at the moment.
            public int dataStringPtr;
            public int texString1Ptr;
            public int texString2Ptr;

            public int texString3Ptr;
            public int texString4Ptr;
        }

        public class NGS_HornObject
        {
            public NGS_Horn ngsHorn;
            public NGS_Unk_Substruct substruct;

            public PSO2String dataString;
        }

        public struct NGS_Horn
        {
            public int unkSubStructPtr; //Not sure what this is at the moment. Shared with NGS_Ear versions? Assumedly not used when 0.
            public int dataStringPtr; //Name of the aqp, aqn, fltd, etc.
            public int reserve0;      //Always 0 so far.
        }

        public class NGS_SKINObject
        {
            public NGS_Skin ngsSkin;
            public PSO2String texString1;
            public PSO2String texString2;
            public PSO2String texString3;

            public PSO2String texString4;
            public PSO2String texString5;
            public PSO2String texString6;
            public PSO2String texString7;
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

        public class EYEBObject
        {
            public EYEB eyeb;
            public PSO2String texString1;
            public PSO2String texString2;
            public PSO2String texString3;

            public PSO2String texString4;
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

        public class HAIRObject
        {
            public HAIR hair;
            public PSO2String dataString;
            public PSO2String texString1;
            public PSO2String texString2;

            public PSO2String texString3;
            public PSO2String texString4;
            public PSO2String texString5;
            public PSO2String texString6;

            public PSO2String texString7;
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

        public class NIFL_COLObject
        {
            public NIFL_COL niflCol;
            public PSO2String textString;
        }

        //The color data here seems to be totally different than before so data just won't be compatible with the old format, not that we understood it before.
        public unsafe struct NIFL_COL
        {
            public int id;
            public int textStringPtr;
            public fixed int colorData[42];
        }

        //This maybe color related. But I have no idea what it's supposed to do.
        public unsafe struct Unk_IntField
        {
            public fixed int unkIntField[79];
        }

        //Also for LCLN, ACLN, and ICLN. Id remapping info. Recolor outfits have multiple ids, but only one id that will actually correlate to a file.
        public struct BCLN
        {
            public int id;
            public int fileId;
            public int unkInt;
        }

        public struct CMXTable
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
            public int rebootFaceAddress; //NGS Faces?
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
            public int outerIdLinkAddress; //BCLN Outer ids for recolors
            public int baseWearIdLinkAddress; //BCLN basewear ids for recolors

            public int innerWearIdLinkAddress; //BCLN innerwear ids for recolors

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
            public int rebootFaceCount;
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
        }
    }
}
