﻿namespace AquaModelLibrary.Data.PSO2.Aqua.AquaFigureData
{
    public class CommandObject
    {
        public CommandStruct cmdStruct;
        public string type = null;
        public string text1 = null;
        public string text2 = null;
        public string text3 = null;
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
}