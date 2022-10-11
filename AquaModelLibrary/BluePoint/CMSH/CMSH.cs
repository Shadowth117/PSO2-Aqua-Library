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

        public CMSH(BufferedStreamReader sr)
        {
            header = new CMSHHeader(sr);
            vertData = new CMSHVertexData(sr);
            faceData = new CMSHFaceData(sr, vertData.positionList.Count);
        }
    }
}
