using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.BluePoint.CMDL
{
    public class CMDL_CMATMaterialMap
    {
        public ushort usht0;
        public int int0;
        public BPString cmshMaterialName = null;

        public int int1;
        public BPString cmatPath = null;
        public CMDL_CMATMaterialMap() { }

        public CMDL_CMATMaterialMap(BufferedStreamReader sr)
        {
            usht0 = sr.Read<ushort>();
            int0 = sr.Read<int>();
            cmshMaterialName = new BPString(sr);

            int1 = sr.Read<int>();
            cmatPath = new BPString(sr);
        }
    }
}
