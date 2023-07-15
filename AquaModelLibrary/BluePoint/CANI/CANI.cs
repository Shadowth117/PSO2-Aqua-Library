﻿using Reloaded.Memory.Streams;
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
        public List<List<CANIFrameData>> caniFrameDataList = new List<List<CANIFrameData>>();
        //public List<CANIMainData> caniMainData = new List<CANIMainData>();
        public CANIFooter caniFooterData = null;
        public CFooter footerData;

        public CANI(BufferedStreamReader sr)
        {
            header = new CANIHeader(sr);

            //Read CANI specific footer and main footer.
            sr.Seek(header.caniHeader.caniFooterPtr, System.IO.SeekOrigin.Begin);
            caniFooterData = new CANIFooter(sr);
            footerData = sr.Read<CFooter>();

            foreach (var set in header.info)
            {
                List<CANIFrameData> caniFrameData = new List<CANIFrameData>();
                sr.Seek(set.frameDataSetPointer, System.IO.SeekOrigin.Begin);
                while(sr.Peek<int>() != 0) //Seemingly no note of count??
                {
                    caniFrameData.Add(new CANIFrameData(sr, set.frameDataSetPointer));
                }
                caniFrameDataList.Add(caniFrameData);

                //Large data
                sr.Seek(set.largeDataPointer, System.IO.SeekOrigin.Begin);
            }
        }
    }
}
