using System.Collections.Generic;
using System.Numerics;
using static AquaModelLibrary.AquaCommon;

namespace AquaModelLibrary
{
    //Cameras, UV, and standard motions are essentially the same format.
    public unsafe class AquaMotion
    {
        public const int CAMO = 0x4F4D4143; //Camera animation
        public const int SPMO = 0x4F4D5053; //UV animation
        public const int NDMO = 0x4F4D444E; //Motion animation
        public const int stdAnim = 0x10002;
        public const int stdPlayerAnim = 0x10012;
        public const int cameraAnim = 0x10004;
        public const int materialAnim = 0x20;
        public AquaPackage.AFPBase afp;
        public NIFL nifl;
        public REL0 rel0;
        public MOheader moHeader;
        public List<KeyData> motionKeys;
        public NOF0 nof0;
        public NEND nend;

        //MO Header
        public struct MOheader
        {
            public int variant; //0xE0, type 0x9 //Seems to be different pending type of anim. 0x10002 standard anims, 0x10012 player anims, 0x10004 camera, 0x20 texture anims
            public int loopPoint; //0xE1, type 0x9 //Loop point? Only seen in live dance aqms and lower than anim range. Otherwise, 0.
            public int endFrame; //0xE2, type 0x9 //Last frame of the animation (0 based)
            public float frameSpeed; //0xE3, type 0xA //FPS of the animation. PSO2's default is 30.0f
            public int unkInt0;      //0xE4, type 0x9 //Always 0x4 for VTBF, always 0x2 for NIFL anims
            public int nodeCount;    //0xE5, type 0x9 //Number of animated nodes (effect type nodes not included) plus one for NodeTreeFlag stuff if a player anim.
            public int boneTableOffset; //NIFL only, always 0x50. Points to start of the bone name table in NIFL anims
            public PSO2String testString; //0xE6, type 0x2 //Always a string of just "test". Technically a standard 0x20 length PSO2 string, but always observed as "test"
            public int reserve0;
        }

        //Motion segment. Denotes a node's animations. Camera files will only have one of these. 
        public unsafe struct MSEG
        {
            public int nodeType; //0xE7, type 0x9 
            public int nodeDataSet; //0xE8, type 0x9 
            public int nodeOffset;
            public PSO2String boneName; //0xE9, type 0x2  
            public int nodeId; //0xEA, type 0x9           //0 on material entries
        }

        //Motion Key
        public class MKEY
        {
            public int keyType;  //0xEB, type 0x9 
            public int dataType; //0xEC, type 0x9
            public int unkInt0;  //0xF0, type 0x9
            public int keyCount; //0xED, type 0x9
            public int frameAddress;
            public int timeAddress;
            public List<Vector4> vector4Keys; //0xEE, type 0x4A or 0xCA if multiple
            public List<ushort> frameCounters; //0xEF, type 0x06 or 0x86 if multiple
            public List<float> floatKeys; //0xF1, type 0xA or 0x8A if multiple
                                          //0xF2. Theoretical. 
            public List<int> intKeys; //0xF3, type 0x8 or 0x88 if multiple
        }

        public class KeyData
        {
            public MSEG mseg = new MSEG();
        }

        public class NodeData : KeyData
        {
            public MKEY posKeys = new MKEY();
            public MKEY rotKeys = new MKEY();
            public MKEY sclKeys = new MKEY();
        }

        public class CameraData : KeyData
        {
            public MKEY unkKeys0 = new MKEY();
            public MKEY unkKeys1 = new MKEY();
            public MKEY unkKeys2 = new MKEY();
            public MKEY unkKeys3 = new MKEY();
        }

        public class MaterialData : KeyData
        {
            public MKEY unkKeys0 = new MKEY();
            public MKEY unkKeys1 = new MKEY();
            public MKEY unkKeys2 = new MKEY();
            public MKEY unkKeys3 = new MKEY();
            public MKEY unkKeys4 = new MKEY();
            public MKEY unkKeys5 = new MKEY();
        }
    }
}
