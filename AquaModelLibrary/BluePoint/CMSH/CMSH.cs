using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.BluePoint.CMSH
{
    public class CMSH
    {
        public CMSHHeader header;
        public CMSHVertexData vertData;
        public CMSHFaceData faceData;
        public CMSHBoneData boneData;
        public CFooter footerData;

        public CMSH(BufferedStreamReader sr)
        {
            header = new CMSHHeader(sr);
            vertData = new CMSHVertexData(sr);
            faceData = new CMSHFaceData(sr, vertData.positionList.Count);
            var position = sr.Position();
            if(header.variantFlag == 0x1)
            {
                boneData = new CMSHBoneData(sr);
            }
            footerData = sr.Read<CFooter>();
        }
    }
}
