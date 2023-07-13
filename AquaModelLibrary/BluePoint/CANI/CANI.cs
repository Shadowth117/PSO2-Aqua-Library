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
        public List<CANIFrameData> caniFrameData = new List<CANIFrameData>();
        //public List<CANIMainData> caniMainData = new List<CANIMainData>();
        public CFooter footerData;

        public CANI(BufferedStreamReader sr)
        {
            header = new CANIHeader(sr);
            foreach(var set in header.info)
            {
                sr.Seek(set.frameDataSetPointer, System.IO.SeekOrigin.Begin);
                caniFrameData.Add(new CANIFrameData(sr));

                //Large data
            }
            footerData = sr.Read<CFooter>();
        }
    }
}
