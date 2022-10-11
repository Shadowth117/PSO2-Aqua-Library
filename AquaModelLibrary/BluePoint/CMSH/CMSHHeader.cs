using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.BluePoint.CMSH
{
    public class CMSHHeader
    {
        public byte variantFlag; //1 for skinned stuff typically, 0 for more basic things
        public int sectionCount; //Sections being things such as vertexData, FaceData, etc.
        public int crc;          //Hash for file?
        public int matCount;     //Material reference count
        public List<CMSHMatReference> matList = new List<CMSHMatReference>();
        public int endInt;

        public CMSHHeader()
        {

        }

        public CMSHHeader(BufferedStreamReader sr)
        {
            variantFlag = sr.Read<byte>();
            sectionCount = sr.Read<int>();
            crc = sr.Read<int>();
            matCount = sr.Read<int>();
            
            for(int i = 0; i < matCount; i++)
            {
                CMSHMatReference matRef = new CMSHMatReference();
                matRef.texNameHash = sr.ReadBytes(sr.Position(), 0x18);
                sr.Seek(0x18, System.IO.SeekOrigin.Current);
                matRef.matNameLength = sr.Read<byte>();
                matRef.matName = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position(), matRef.matNameLength));
                if(variantFlag == 0x1 && (i + 1) != matCount)
                {
                    matRef.unkInt0 = sr.Read<int>();
                    matRef.unkVertexAddress = sr.Read<int>();
                }
                sr.Seek(matRef.matNameLength, System.IO.SeekOrigin.Current);
                matList.Add(matRef);
            }

            endInt = sr.Read<int>();
        }
    }

}
