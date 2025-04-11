using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BluePoint.CMSH
{
    public class CMSH
    {
        public CMSHHeader header = null;
        public CMSHVertexData vertData = null;
        public CMSHFaceData faceData = null;
        public CMSHUnkData0 unkdata0 = null;
        public CMSHUnkData1 unkdata1 = null;
        public CMSHUnkData2 unkdata2 = null;
        public CMSHBoneData boneData = null;
        public CFooter footerData;

        public CMSH()
        {

        }

        public CMSH(byte[] file)
        {
            file = CompressionHandler.CheckCompression(file);
            var test = BitConverter.ToUInt16(file, 0);
            if(test == 0x1500 || test == 0x500 || test == 0xD01 || test == 0x4100 || test == 0x1100 || test == 0x4901 || test == 0x5100)
            {
                //Maybe one day we'll support these, but they're kinda dumb and all LODs
                return;
            }
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        private void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            header = new CMSHHeader(sr);
            if (header.variantFlag2 != 0x41)
            {
                vertData = new CMSHVertexData(sr, header, header.hasSizeFloat);
                faceData = new CMSHFaceData(sr, header, vertData.positionList.Count);

                if ((header.variantFlag2 & 0x20) > 0)
                {
                    unkdata0 = new CMSHUnkData0(sr);
                    unkdata1 = new CMSHUnkData1(sr);
                }
                if ((header.variantFlag & 0x1) > 0)
                {
                    byte[] test = sr.ReadBytes(sr.Position + 1, 1);
                    if (test[0] != '$' && !(header.variantFlag == 0x1 && header.variantFlag2 == 0xA))
                    {
                        unkdata2 = new CMSHUnkData2(sr);
                    }
                    boneData = new CMSHBoneData(sr, header);
                }
                footerData = sr.Read<CFooter>();
            }
        }

        public byte[] GetBytes()
        {
            List<byte> outBytes = new List<byte>();
            outBytes.AddRange(header.GetBytes());
            switch(header.variantFlag2)
            {
                case 0x41:
                    break;
                default:
                    //outBytes.AddRange(vertData.GetBytes());
                    //outBytes.AddRange(faceData.GetBytes());
                    if((header.variantFlag2 & 0x20) > 0)
                    {
                      //  outBytes.AddRange(unkdata0.GetBytes());
                        //outBytes.AddRange(unkdata1.GetBytes());
                    }
                    if((header.variantFlag & 0x1) > 0)
                    {
                        if(unkdata2 != null)
                        {
                          //  outBytes.AddRange(unkdata2.GetBytes());
                        }
                        //outBytes.AddRange(boneData.GetBytes());
                    }
                    break;
            }
            return outBytes.ToArray();
        }
    }
}
