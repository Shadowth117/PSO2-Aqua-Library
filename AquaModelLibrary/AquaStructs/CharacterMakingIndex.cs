using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AquaModelLibrary.AquaCommon;

namespace AquaModelLibrary
{
    //Though the NIFL format is used for storage, VTBF format tag references for data will be commented where appropriate. Some offset/reserve related things are NIFL only, however.
    public unsafe class CharacterMakingIndex
    {
        public static string dataDir = $"data\\win32\\";
        public static string dataNADir = $"data\\win32_na\\";
        public static string classicStart = $"character/making/pl_";
        public static string rebootStart = $"character/making_reboot/pl_";
        public static string rebootExStart = $"character/making_reboot_ex/pl_";
        public static string playerVoiceStart = $"player_voice/";

        public static string voiceCman = "11_voice_cman";
        public static string voiceCwoman = "11_voice_cwoman";
        public static string voiceMan = "11_voice_man";
        public static string voiceWoman = "11_voice_woman";

        public Dictionary<int, BODY> costumeDict = new Dictionary<int, BODY>();
        public Dictionary<int, BODY> carmDict = new Dictionary<int, BODY>();
        public Dictionary<int, BODY> clegDict = new Dictionary<int, BODY>();
        public Dictionary<int, BODY> outerDict = new Dictionary<int, BODY>();

        public Dictionary<int, BODY> baseWearDict = new Dictionary<int, BODY>();
        public Dictionary<int, BBLY> innerWearDict = new Dictionary<int, BBLY>();
        public Dictionary<int, BBLY> bodyPaintDict = new Dictionary<int, BBLY>();
        public Dictionary<int, Sticker> stickerDict = new Dictionary<int, Sticker>();

        public Dictionary<int, FACE> faceDict = new Dictionary<int, FACE>();
        public Dictionary<int, FCMN> fcmnDict = new Dictionary<int, FCMN>();
        public Dictionary<int, NGS_FACE> ngsFaceDict = new Dictionary<int, NGS_FACE>();
        public Dictionary<int, FCP> fcpDict = new Dictionary<int, FCP>();

        public Dictionary<int, ACCE> accessoryDict = new Dictionary<int, ACCE>();
        public Dictionary<int, EYE> eyeDict = new Dictionary<int, EYE>();
        public List<NGS_Ear> ngsEarList = new List<NGS_Ear>();
        public List<NGS_Teeth> ngsTeethList = new List<NGS_Teeth>();

        public List<NGS_Horn> ngsHornList = new List<NGS_Horn>();
        public Dictionary<int, NGS_Skin> ngsSkinDict = new Dictionary<int, NGS_Skin>();
        public Dictionary<int, EYEB> eyebrowDict = new Dictionary<int, EYEB>();
        public Dictionary<int, EYEB> eyelashDict = new Dictionary<int, EYEB>();

        public Dictionary<int, HAIR> hairDict = new Dictionary<int, HAIR>();
        public Dictionary<int, NIFL_COL> colDict = new Dictionary<int, NIFL_COL>();
        public List<Unk_IntField> unkList = new List<Unk_IntField>();
        public Dictionary<int, BCLN> costumeIdLink = new Dictionary<int, BCLN>();

        public Dictionary<int, BCLN> castArmIdLink = new Dictionary<int, BCLN>();
        public Dictionary<int, BCLN> clegIdLink = new Dictionary<int, BCLN>();
        public Dictionary<int, BCLN> outerWearIdLink = new Dictionary<int, BCLN>();
        public Dictionary<int, BCLN> baseWearIdLink = new Dictionary<int, BCLN>();

        public Dictionary<int, BCLN> innerWearIdLink = new Dictionary<int, BCLN>();

        public NIFL nifl;
        public REL0 rel0;
        public CMXTable cmxTable;
        public NOF0 nof0;
        public NEND nend;

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
            public float flt_0x8_0xA;   //0x8, 0xA
            public float flt_0xB_0xA;   //0xB, 0xA

            public int unkInt3;
            public float unkFloat0;
            public float unkFloat1;
            public float unkFloat2;

            public float unkFloat3;
            public int unkInt4;
        }

        //BBLY, BDP1, BDP2(stickers)
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

        public struct Sticker
        {
            public int id;
            public int texStringPtr;
            public int reserve0;
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

        public struct NGS_FACE
        {
            public int id; 
            public int texString1Ptr;
            public int texString2Ptr;
            public int texString3Ptr;

            public int texString4Ptr;
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
            public short unkShort1;

            public short unkShort2;
            public short unkShort3;
            public short unkShort4;
            public short unkshort5;
            public int unktIn14;
            public int unkInt15;
            
            public int unkInt16;
            public short unkShort6;
            public short unkShort7;
            public short unkShort8;
            public short unkShort9;
            public float unkfloat7;

            public int unkInt17;
            public int unkInt18;
            public int unkInt19;
            public short unkShort10;
            public short unkShort11;

            public short unkShort12;
            public short unkShort13;
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

        public struct NGS_Ear
        {
            public int unkSubStructPtr; //Not sure what this is at the moment.
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

        public struct NGS_Teeth
        {
            public int unkSubStructPtr; //Not sure what this is at the moment.
            public int texString1Ptr;
            public int texString2Ptr;
            public int texString3Ptr;

            public int texString4Ptr;
            public int texString5Ptr;
        }

        public struct NGS_Horn
        {
            public int unkSubStructPtr; //Not sure what this is at the moment. Shared with NGS_Ear versions? Assumedly not used when 0.
            public int dataStringPtr; //Name of the aqp, aqn, fltd, etc.
            public int reserve0;      //Always 0 so far.
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

        //Also for EYEL
        public struct EYEB
        {
            public int id;
            public int texString1Ptr;
            public int texString2Ptr;
            public int texString3Ptr;

            public int texString4Ptr;
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
            public int unkInt0;
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

            public int unkInt21;
            public int unkInt22;
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

            public int castArmIdLinkAddress; //BCLN Cast body ids for recolors
            public int castLegIdLinkAddress; //BCLN Cast leg ids for recolors
            public int outerIdLinkAddress; //BCLN Cast arm ids for recolors
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
