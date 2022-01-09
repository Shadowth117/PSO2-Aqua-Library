using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary
{
    //Credit to DeathCream for doing a first pass on documenting the format
    public class AquaFigure : AquaCommon
    {
        public FigHeader figHeader;
        public List<int> attachTransformPtrList = new List<int>();
        public List<int> unkPtr1List = new List<int>();
        public List<AttachTransformObject> attachTransforms = new List<AttachTransformObject>();
        public List<FigStruct1Object> figStruct1s = new List<FigStruct1Object>();
        public struct FigHeader
        {
            public int magic; //Should be fig/0 as plaintext
            public int int_04;
            public int version;
            public int int_08;

            public int attachTransofrmPtr;
            public int unkPtr1;
            public int attachTransformCount;
            public int unkPtr1Count;
        }

        public class AttachTransformObject
        {
            public AttachTransform attach;
            public string name;
            public string unkText;
            public string attachNode;
        }

        public struct AttachTransform
        {
            public int namePtr;
            public int unkTextPtr;
            public int attachNodePtr;
            public Vector3 pos;
            public Vector3 rot;
            public float scale;
            public int unkInt;
        }

        public class FigStruct1Object
        {
            public FigStruct1 rawStruct;
            public string text;
            FS1UnkStruct0Object struct0 = null;
            FS1UnkStruct1Object struct1 = null;
            StateMappingObject struct2 = null;
        }

        //Pointers of value 0x10 are null
        public struct FigStruct1
        {
            public int textPtr;
            public int FS1UnkStruct0Ptr;
            public int FS1UnkStruct1Ptr;
            public int StateMappingPtr;

            public int int_10;
        }

        public class FS1UnkStruct0Object
        {
            public FS1UnkStruct0 fs1struct0;
            public string text0;
            public string text1;
            public string text2;
            public string text3;
        }

        public struct FS1UnkStruct0
        {
            public int text0Ptr;
            public int text1Ptr;
            public int text2Ptr;
            public int text3Ptr;

            public int int_10;
            public float float_14;
            public ushort ushort_18;
            public ushort ushort_1C;
        }

        public class FS1UnkStruct1Object
        {
            public FS1UnkStruct1 unkStruct1;
            public string text0;
            public List<FS1_US1_SubstructObject> subStructs = new List<FS1_US1_SubstructObject>();
            public string text1;
            public string text2;
        }

        public struct FS1UnkStruct1
        {
            public int textPtr0;
            public int subStructPtr;
            public int subStructCount;
            public int textPtr1;
            public int textPtr2;
        }

        public class FS1_US1_SubstructObject
        {
            public FS1_US1_Substruct unkStruct;
            public string name;
            public string text1;
        }

        public struct FS1_US1_Substruct
        {
            public int namePtr;
            public int shape; //0 - Sphere?, 1 - Cylinder?
            public float shapeParam0;
            public float shapeParam1;
            
            public float shapeParam2;
            public int text1Ptr;
            public float shapeParam3;
            public float shapeParam4;

            public float shapeParam5;
            public float shapeParam6;
            public float shapeParam7;
            public float shapeParam8;
        }

        public class StateMappingObject
        {
            public StateMapping stateMappingStruct;
            public string name;
            public List<CommandObject> commands = new List<CommandObject>();
            public List<EffectMapObject> effects = new List<EffectMapObject>();
            public List<AnimMapObject> anims = new List<AnimMapObject>();
        }

        public struct StateMapping
        {
            public int namePtr;
            public int commandPtr;
            public int effectMapPtr;
            public int animMapPtr;
            public int commandCount;
            public int effectMapCount;
            public int animMapCount;
        }

        public class CommandObject
        {
            public CommandStruct cmdStruct;
            public string type;
            public string text1;
            public string text2;
            public string text3;
        }

        public struct CommandStruct
        {
            public int typePtr;
            public int text1Ptr;
            public int text2Ptr;
            public int text3Ptr;

            public int int_10;
            public int int_14;
        }

        //Effect mappings have numerous structures based on the type id. 
        //For sanity purposes, these will be stored in lists since there are only 3 distinct data types known to exist in them, though more may actually be used.
        //For writeback, note that data is read in by order. 
        public class EffectMapObject
        {
            public int type;
            public List<int> intList = new List<int>();
            public List<float> fltList = new List<float>();
            public List<string> strList = new List<string>();
            public List<int> colorList = new List<int>();
        }

        public class AnimMapObject
        {
            public AnimMapStruct animStruct;
            public string name;
            public string followUp;
            public string type;
            public string anim;
            public List<AnimFrameInfo> frameInfoList = new List<AnimFrameInfo>();
        }

        public struct AnimMapStruct
        {
            public int namePtr;
            public int followUpPtr;
            public int typePtr;
            public int animPtr;

            public float flt_10;
            public float flt_14;
            public float flt_18;
            public float flt_1C;

            public int int_20;
            public int frameInfoPtr;
            public int frameInfoPtrCount;
        }

        public struct AnimFrameInfo
        {
            public float flt_0;
            public float flt_1;
            public int frame;
        }
    }
}
