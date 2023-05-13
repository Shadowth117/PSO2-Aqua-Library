using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary
{
    //PSO2 Implementation of Glitter particle effect files. Shares striking similarity to Project Diva variations.
    //May be entirely different than the NIFL variation 
    //Seemingly, the file seems to be an efct header followed by an emit nodes, their animations and particle nodes with their animations.
    //There should be at least one EFCT, one EMIT, and one PTCL per file while they must all have ANIMs, null or not.
    public unsafe class AquaEffectReboot : AquaCommon
    {
        public EFCTNGSObject efct;

        public class EFCTNGSObject
        {
            public EFCTNGS efct;
            public List<int> unkIntArray = new List<int>();
            public RootSettingsObject root = new RootSettingsObject();
        }

        public class RootSettingsObject : AquaEffect.AnimObject
        {
            public List<RootSettingsStruct0> rootSettingStruct0s = new List<RootSettingsStruct0>();
            public List<RootSettingsStruct1> rootSettingStruct1s = new List<RootSettingsStruct1>();
            public List<RootSettingsStruct2> rootSettingStruct2s = new List<RootSettingsStruct2>();
            public RootSettings root;
        }

        public struct EFCTNGS
        {
            public ushort flags;
            public ushort usht_02;
            public int offset_04;
            public int int_08;
            public int unkIntArrayCount;

            public int unkIntArrayOffset;
            public int int_14;
            public int int_18;
            public float float_1C; //End frame/framecount?

            public float float_20; //End frame/framecount? Same as above typically
            public int int_24;
            public PSO2Stringx30 soundName;
            public int int_58;
            public float unk_rate; // Typically -0.01666667 or something. Unclear what this is for, especially since it tends to be negative. Occasionally 0.

            //Padding? 
            public int int_60;
            public int int_64;
            public int int_68;
            public int int_6C;

            public int int_70;
            public int int_74;
            public int int_78;
            public int int_7C;
        }

        public struct RootSettingsStruct0
        {
            public int int_00;
            public int int_04;
            public int int_08;
            public int int_0C;
        }

        public struct RootSettingsStruct1
        {
            public Vector4 vec4_00;
            public int int_10;
            public int int_14;
            public int int_18;
            public int int_1C;

            public int int_20;
            public int int_24;
            public int int_28;
            public int int_2C;

            public int int_30;
            public int int_34;
            public int int_38;
            public int int_3C;

            public int int_40;
            public Vector3 vec3_44;

            public int int_50;
            public int int_54;
            public int int_58;
            public int int_5C;

            public int int_60;
            public int int_64;
            public int int_68;
            public int int_6C;
        }

        public struct RootSettingsStruct2
        {
            public int int_00;
            public int int_04;
            public int int_08;
            public int int_0C;

            public int int_10;
            public int int_14;
            public int int_18;
            public int int_1C;

            public int int_20;
            public int int_24;
            public int int_28;
            public int int_2C;

            public int int_30;
            public int int_34;
            public int int_38;
            public int int_3C;

            public int int_40;
            public int int_44;
            public int int_48;
            public int int_4C;
        }

        //Yes, this is really all one fixed size structure
        public struct RootSettings
        {
            public Vector3 position;
            public int reserve0;
            public Vector3 rotation;
            public int reserve1;
            public Vector3 scale;
            public float flt_2C;

            public Vector4 vec4_30; //30 - 80 often 0s
            public Vector4 vec4_40;
            public Vector4 vec4_50;
            public Vector4 vec4_60;
            public Vector4 vec4_70;
            public Vector4 vec4_80;

            public Vector4 vec4_90; //90 is often 1, 1, 1, 1
            public Vector4 vec4_A0;

            public int int_B0;
            public int int_B4;
            public int int_B8;
            public int int_BC;

            public float flt_C0;
            public float flt_C4;
            public int int_C8;
            public int int_CC;

            public float flt_D0;
            public float flt_D4;
            public int int_D8;
            public int int_DC;

            public Vector3 vec3_E0;
            public int int_EC;
            public Vector3 vec3_F0;
            public int int_FC;

            public int int_100;
            public int int_104;
            public int int_108;
            public int RootSettingsStruct0Count;

            public int RootSettingsStruct0Ptr;
            public int int_114;
            public float flt_118;
            public int int_11C;

            public float flt_120;
            public int int_124;
            public int int_128;
            public int int_12C;

            public int int_130;
            public int int_134;
            public int int_138;
            public int int_13C;

            public int int_140; // often -1
            public float flt_144;
            public int int_148;
            public int int_14C;

            public int int_150;
            public int int_154;
            public int int_158;
            public int int_15C;

            public int int_160;
            public int int_164;
            public int int_168;
            public int int_16C;

            public byte bt_170;
            public byte bt_171;
            public byte bt_172;
            public byte bt_173;
            public int int_174;
            public int int_178;
            public int int_17C;

            public int int_180;
            public int int_184;
            public int int_188;
            public int int_18C;

            public int RootSettingStruct1Count;
            public int RootSettingStruct1Ptr;
            public int int_198;
            public int RootSettingsStruct2Count;

            public int RootSettingsStruct2Ptr;
            public int int_1A4;
            public int int_1A8;
            public int int_1AC;

            public Vector3 vec3_1B0;
            public int int_1BC;

            public int int_1C0;
            public int int_1C4;
            public int int_1C8;
            public int int_1CC;

            public int int_1D0;
            public int int_1D4;
            public int int_1D8;
            public int int_1DC;

            public int int_1E0;
            public int int_1E4;
            public int int_1E8;
            public int int_1EC;

            public int int_1F0;
            public int int_1F4;
            public int int_1F8;
            public int int_1FC;

            public int int_200;
            public int int_204;
            public int int_208;
            public int int_20C;

            public int int_210;
            public int int_214;
            public int int_218;
            public int int_21C;

            public int int_220;
            public int int_224;
            public int int_228;
            public int int_22C;

            public int int_230;
            public int int_234;
            public int int_238;
            public int int_23C;

            public int int_240;
            public int int_244;
            public int int_248;
            public int int_24C;

            public int int_250;
            public int int_254;
            public int int_258;
            public int int_25C;

            public int int_260;
            public float flt_264;
            public int int_268;
            public ushort usht_26C;
            public ushort usht_26E;

            public int int_270;
            public int int_274;
            public int int_278;
            public int int_27C;

            public int int_280;
            public Vector3 vec3_284;

            public float flt_290;
            public Vector3 vec3_294;

            public float flt_2A0;
            public int int_2A4;
            public int int_2A8;
            public int int_2AC;

            public int int_2B0;
            public int int_2B4;
            public int int_2B8;
            public int int_2BC;

            public int int_2C0;
            public int int_2C4;
            public int int_2C8;
            public int int_2CC;

            public int int_2D0;
            public int int_2D4;
            public int int_2D8;
            public int int_2DC;

            public int int_2E0;
            public int int_2E4;
            public int int_2E8;
            public int int_2EC;

            public int int_2F0;
            public int int_2F4;
            public int int_2F8;
            public int int_2FC;

            public int int_300;
            public int int_304;
            public int int_308;
            public int int_30C;

            public int int_310;
            public int int_314;
            public int int_318;
            public int offset_320Count;

            public int offset_320;
            public int int_324;
            public int int_328;
            public int int_32C;

            public int int_330;
            public int int_334;
            public int int_338;
            public int int_33C;

            public int int_340;
            public int int_344;
            public int int_348;
            public int int_34C;

            public int int_350;
            public int int_354;
            public int int_358;
            public int int_35C;
        }

    }
}
