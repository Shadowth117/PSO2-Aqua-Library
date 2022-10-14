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
        public byte variantFlag2; //Sections being things such as vertexData, FaceData, etc.
        public byte unk0;
        public ushort unk1;
        public int crc;          //Hash for file?
        public int matCount;     //Material reference count
        public List<CMSHMatReference> matList = new List<CMSHMatReference>();
        public int endInt;

        public string OtherModelName = null; //For variantFlag2 being 0x41

        public CMSHHeader()
        {

        }

        public CMSHHeader(BufferedStreamReader sr)
        {
            variantFlag = sr.Read<byte>();
            variantFlag2 = sr.Read<byte>();
            unk0 = sr.Read<byte>();
            unk1 = sr.Read<ushort>();

            if(variantFlag2 != 0x41)
            {
                crc = sr.Read<int>();
                matCount = sr.Read<int>();
            
                for(int i = 0; i < matCount; i++)
                {
                    CMSHMatReference matRef = new CMSHMatReference();
                    matRef.texNameHash = sr.ReadBytes(sr.Position(), 0x18);
                    sr.Seek(0x18, System.IO.SeekOrigin.Current);
                    matRef.matNameLength = sr.Read<byte>();
                    matRef.matName = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position(), matRef.matNameLength));
                    sr.Seek(matRef.matNameLength, System.IO.SeekOrigin.Current);
                    if (matCount > 1 && (i + 1 != matCount))
                    {
                        matRef.startingFaceIndex = sr.Read<int>();
                        matRef.endingFaceIndex = sr.Read<int>();
                    }
                    matList.Add(matRef);
                }

                endInt = sr.Read<int>();
            } else
            {
                var mdlLen = sr.Read<byte>();
                OtherModelName = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position(), mdlLen));
            }
        }
    }

}
