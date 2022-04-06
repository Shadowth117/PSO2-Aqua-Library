namespace AquaModelLibrary.AquaStructs
{
    public class MusicFileReboot : AquaCommon
    {
        public int musHeaderOffset;

        public class MusHeader
        {
            public MusHeaderStruct mus;
            public string musString1C = null;
            public string customVariables = null;
        }

        public struct MusHeaderStruct
        {
            public int subStrOffset;
            public byte bt_4;
            public byte bt_5;
            public byte bt_6;
            public byte bt_7;
            public int int_08;
            public int int_0C;

            public byte bt_10;
            public byte bt_11;
            public byte bt_12;
            public byte transitionDataCount;
            public int transitionDataOffset;
            public byte bt_18;
            public byte bt_19;
            public byte bt_1A;
            public byte bt_1B;
            public int string_1C;

            public byte bt_20;
            public byte bt_21;
            public byte bt_22;
            public byte bt_23;
            public int conditionDataOffset;
            public int customVariableStringOffset;
        }

        public class SymphonyData
        {

        }

        public struct SymphonyDataStruct
        {
        }

        public class TransitionData
        {
            public TransitionDataStruct transStruct;
            public TransitionDataSubStruct transSubStruct;
            public string transitionClipName;
            public string subStructStr_04;
        }

        public struct TransitionDataStruct
        {
            public int int_00;
            public int substructOffset;
            public float flt_08;
            public int transitionClipNameOffset;
        }

        public struct TransitionDataSubStruct
        {
            public int int_00;
            public int str_04;
        }

        public class Conditions
        {
            public ConditionsStruct conditionstr;
            public string description;
            public string str_14;
            public string str_18;
            public string str_20;
            public string str_28;
        }

        public struct ConditionsStruct
        {
            public int int_00;
            public int descriptionStrOffset;
            public int valueType;
            public float valueOne;

            public float valueTwo;
            public int str_14;
            public int str_18;
            public int int_1C;

            public int str_20;
            public byte bt_24;
            public byte bt_25;
            public byte bt_26;
            public byte bt_27;
            public int str_28;
            public int int_2C;
        }

    }
}
