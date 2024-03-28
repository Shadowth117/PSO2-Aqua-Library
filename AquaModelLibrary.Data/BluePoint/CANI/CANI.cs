using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BluePoint.CANI
{
    public class CANI
    {
        public CANIHeader header = null;
        public List<List<CANIFrameData>> caniFrameDataList = new List<List<CANIFrameData>>();
        //public List<CANIMainData> caniMainData = new List<CANIMainData>();
        public CANIFooter caniFooterData = null;
        public CFooter footerData;

        public CANI(byte[] file)
        {
            file = CompressionHandler.CheckCompression(file);
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        private void Read(BufferedStreamReaderBE<MemoryStream> sr)
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
                while (sr.Peek<int>() != 0) //Seemingly no note of count??
                {
                    caniFrameData.Add(new CANIFrameData(sr, set.frameDataSetPointer));
                }
                caniFrameDataList.Add(caniFrameData);

                //Large data, under some conditions, this will be empty and simply point to the CANIFooter
                sr.Seek(set.largeDataPointer, System.IO.SeekOrigin.Begin);
            }
        }
    }
}
