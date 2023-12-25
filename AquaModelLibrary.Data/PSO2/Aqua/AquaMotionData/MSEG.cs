using AquaModelLibrary.Data.DataTypes.SetLengthStrings;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaMotionData
{

    //Motion segment. Denotes a node's animations. Camera files will only have one of these. 
    public unsafe struct MSEG
    {
        public int nodeType; //0xE7, type 0x9 
        public int nodeDataCount; //0xE8, type 0x9 
        public int nodeOffset;
        public PSO2String nodeName; //0xE9, type 0x2  
        public int nodeId; //0xEA, type 0x9           //0 on material entries
    }
}
