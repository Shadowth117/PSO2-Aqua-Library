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
        public byte[] extraFlags;          //Hash for file?
        public int matCount;     //Material reference count
        public List<CMSHMatReference> matList = new List<CMSHMatReference>();
        public int endInt;

        //Demon's Souls check
        public bool hasExtraFlags; //Used if extra flags are detected.

        //0x0, 0x2 data
        public byte unk0002Byte;
        public int unk0002Int0;
        public int unk0002Int1;

        //0x8C, 0xA data
        public int unk8C0AInt0;

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
                //For certain SOTC models
                var crcCheck = sr.ReadBytes(sr.Position(), 4);
                hasExtraFlags = crcCheck[1] > 0 || crcCheck[2] > 0 || crcCheck[3] > 0;
                if(variantFlag != 0 && variantFlag2 != 2 || hasExtraFlags)
                {
                    extraFlags = sr.ReadBytes(sr.Position(), 4);
                    sr.Seek(4, System.IO.SeekOrigin.Current);
                }

                //Read 8C and 0A specific value
                if(variantFlag == 0x8C && variantFlag2 == 0xA)
                {
                    unk8C0AInt0 = sr.Read<int>();
                }
                matCount = sr.Read<int>();
                
                //For 0x8C types, extraFlag 0
                if(variantFlag != 0x8C || (extraFlags[0] & 8) > 0)
                {
                    for (int i = 0; i < matCount; i++)
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
                }

                endInt = sr.Read<int>();

                if (variantFlag == 0 && variantFlag2 == 2 && hasExtraFlags == false)
                {
                    unk0002Byte = sr.Read<byte>();
                    unk0002Int0 = sr.Read<int>();
                    unk0002Int1 = sr.Read<int>();
                }
            } else
            {
                var mdlLen = sr.Read<byte>();
                OtherModelName = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position(), mdlLen));
            }
        }
    }

}
