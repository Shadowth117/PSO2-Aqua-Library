using System.Collections.Generic;

namespace AquaModelLibrary.AquaStructs
{
    public class CharacterMakingTemplate : AquaCommon
    {
        //Currently only planning to support the NGS versions of this. Could support specific versions of the old CMT files if desired, but they differ in part categories and use int16s vs int32s
        //Essentially, this whole file is a large table of character part ids for character creation with bitflags that enable and disable various things. 
        public CMTTable cmtTable;
        public List<Dictionary<int, int>> cmtData = new List<Dictionary<int, int>>();
        public class CMTTable
        {
            public List<int> counts = new List<int>();
            public List<int> addresses = new List<int>();
        }
    }
}
