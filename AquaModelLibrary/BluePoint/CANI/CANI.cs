using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.BluePoint.CANI
{
    public class CANI
    {
        public CANIHeader header = null;
        public CFooter footerData;

        public CANI(BufferedStreamReader sr)
        {
            header = new CANIHeader(sr);
            footerData = sr.Read<CFooter>();
        }
    }
}
