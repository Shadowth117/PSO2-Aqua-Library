using AquaModelLibrary.Data.DataTypes.SetLengthStrings;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaEffectData.Reboot
{
    public class EFCTNGSObject
    {
        public EFCTNGS efctNGS;
        public List<int> unkIntArray = new List<int>();
        public RootSettingsObject root = new RootSettingsObject();
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
