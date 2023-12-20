using System.Collections.Generic;
using static AquaModelLibrary.AquaCommon;

namespace AquaModelLibrary.AquaStructs
{
    public class AddOnIndex
    {
        public List<ADDO> addonList = new List<ADDO>();
        public class ADDO
        {
            public int id;
            public PSO2String leftName;
            public PSO2String leftBoneAttach;
            public PSO2String rightName;
            public PSO2String rightBoneAttach;
            public PSO2String unusedLeftName2;
            public PSO2String unusedLeftBoneAttach2;
            public PSO2String unusedRightName2;
            public PSO2String unusedRightBoneAttach2;
            public byte F8;
            public PSO2String leftEffectName;
            public PSO2String rightEffectName;
            public PSO2String leftEffectAttach;
            public PSO2String rightEffectAttach;
            public byte FD;
            public byte FE;
            public byte E0;
            public PSO2String extraAttach;
        }

    }
}
