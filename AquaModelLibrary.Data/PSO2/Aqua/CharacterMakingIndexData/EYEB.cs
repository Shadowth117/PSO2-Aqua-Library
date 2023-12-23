using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData
{
    public class EYEBObject : BaseCMXObject
    {
        public EYEB eyeb;
        public string texString1 = null;
        public string texString2 = null;
        public string texString3 = null;

        public string texString4 = null;
    }

    //Also for EYEL
    public struct EYEB
    {
        public int id;
        public int texString1Ptr;
        public int texString2Ptr;
        public int texString3Ptr;

        public int texString4Ptr;
    }
}
