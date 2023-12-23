using AquaModelLibrary.Data.PSO2.Aqua.AquaEffectData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaEffectData.Reboot;
using AquaModelLibrary.Data.PSO2.Aqua.SetLengthStrings;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    //PSO2 Implementation of Glitter particle effect files. Shares striking similarity to Project Diva variations.
    //May be entirely different than the NIFL variation 
    //Seemingly, the file seems to be an efct header followed by an emit nodes, their animations and particle nodes with their animations.
    //There should be at least one EFCT, one EMIT, and one PTCL per file while they must all have ANIMs, null or not.
    public unsafe class AquaEffectReboot : AquaCommon
    {
        public EFCTNGSObject efct = null;

        public class EFCTNGSObject
        {
            public EFCTNGS efct;
            public List<int> unkIntArray = new List<int>();
            public RootSettingsObject root = new RootSettingsObject();
        }

        public class RootSettingsObject : AnimObject
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
    }
}
