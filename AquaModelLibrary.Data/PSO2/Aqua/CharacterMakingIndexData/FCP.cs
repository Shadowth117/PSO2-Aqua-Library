﻿namespace AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData
{
    public class FCPObject : BaseCMXObject
    {
        public FCP fcp;
        public string texString1 = null;
        public string texString2 = null;
        public string texString3 = null;

        public string texString4 = null;
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
}
