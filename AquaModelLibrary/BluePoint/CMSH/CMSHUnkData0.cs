using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.BluePoint.CMSH
{
    public class CMSHUnkData0
    {
        public int unk0;
        public int flags;
        public int unk1;
        public int count;
        public List<CMSHUNKData0Data> unkList = new List<CMSHUNKData0Data>();

        public struct CMSHUNKData0Data
        {
            int unk0;
            int unk1;
        }

        public CMSHUnkData0()
        {

        }

        public CMSHUnkData0(BufferedStreamReader sr)
        {
            unk0 = sr.Read<int>();
            flags = sr.Read<int>();
            unk1 = sr.Read<int>();
            count = sr.Read<int>();
            var pos = sr.Position();
            for (int i = 0; i < count; i++)
            {
                unkList.Add(sr.Read<CMSHUNKData0Data>());
            }
        }
    }
}
