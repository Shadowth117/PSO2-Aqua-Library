using AquaModelLibrary.Helpers.Readers;
using Reloaded.Memory.Streams;

namespace AquaModelLibrary.BluePoint.CMSH
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

        public CMSH(BufferedStreamReaderBE<MemoryStream> sr)
        {
            header = new CMSHHeader(sr);
            if (header.variantFlag2 != 0x41)
            {
                vertData = new CMSHVertexData(sr, header, header.hasExtraFlags);
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
    }
}
