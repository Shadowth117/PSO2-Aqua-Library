using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AquaModelLibrary.BluePoint.CMSH
{
    public class CMSHHeader
    {
        //These define a lot of what's going to be in a particular model variant, but currently it's difficult to guess what each bit means.
        public ushort variantFlags;
        public byte variantFlag; //& 1 for skinned stuff typically, 0 for more basic things
        public byte variantFlag2; //Sections being things such as vertexData, FaceData, etc.
        public byte unk0;
        public ushort unk1;
        public byte[] extraFlags;          //Hash for file?
        public int matCount;     //Material reference count
        public List<CMSHMatReference> matList = new List<CMSHMatReference>();
        public int endInt;

        //Demon's Souls check
        public bool hasExtraFlags; //Used if extra flags are detected.
        public ulong dummyConstData = 7885087596553986582; //Certain 0x200 dummy models have 0ed extraFlags so we check this against address 0x25 to get around that.

        //0x8C, 0xA data
        public byte unk8C0ASize0Modifier;
        public ushort unk8C0ASize0;
        public byte subVariantFlag;
        public ushort unk8C0ASize1;
        public byte unk8C0AByte0;
        public ushort unk8C0AShort0;
        public byte unk8C0AInnerBigPtrByte; //Seems to be 0x1 for large pointers and 0x2 for larger ones
        public int unk8C0AModelDataSize; //\Distance from directly after the short value to the end of the face indices

        //Some cmshs have this
        public int modelType;

        public string OtherModelName = null; //For cmshs with a reference model. These often have a bit of their own data, but are LODs and so aren't a priority

        public CMSHHeader()
        {

        }

        public CMSHHeader(BufferedStreamReader sr)
        {
            variantFlags = sr.Peek<ushort>();
            variantFlag = sr.Read<byte>();
            variantFlag2 = sr.Read<byte>();

            switch(variantFlags)
            {
                //Reference mesh variants
                case 0x1100:
                case 0x4100:
                case 0x5100:
                case 0x4901:
                case 0x500:
                    if (variantFlags == 0x500)
                    {
                        modelType = 0x5;
                    }
                    unk0 = sr.Read<byte>();
                    unk1 = sr.Read<ushort>();
                    ReadReferenceModelPath(sr);
                    break;

                case 0x200:
                case 0xA01:
                case 0x2A01:
                case 0xAA01:
                    unk0 = sr.Read<byte>();
                    unk1 = sr.Read<ushort>();
                    CheckForExtraFlags(sr);
                    matCount = sr.Read<int>();
                    ReadMaterialList(sr);
                    
                    //Demon's Souls
                    if (hasExtraFlags)
                    {
                        endInt = sr.Read<int>();
                    }
                    break;

                case 0x68C:
                    Debug.WriteLine("0x68C unimplemented");
                    throw new Exception();
                    break;
                case 0xA8C:
                    ReadA8CHeaderExtras(sr);
                    ReadMaterialList(sr);
                    break;
                case 0xACC:
                    unk0 = sr.Read<byte>();
                    modelType = sr.Read<int>();
                    if(modelType == 0x5)
                    {
                        ReadReferenceModelPath(sr);
                    } else
                    {
                        matCount = sr.Read<int>();
                        ReadMaterialList(sr);
                    }
                    break;
                default:
                    Debug.WriteLine($"Unknown variant flags: {variantFlags:X}");
                    break;
            }
        }

        private void ReadMaterialList(BufferedStreamReader sr)
        {
            //For 0x8C types, check the bit which says if there should be material
            if (variantFlag != 0x8C || (extraFlags[0] & 8) > 0)
            {
                for (int i = 0; i < matCount; i++)
                {
                    CMSHMatReference matRef = new CMSHMatReference();
                    matRef.texNameHash = sr.ReadBytes(sr.Position(), 0x18);
                    sr.Seek(0x18, System.IO.SeekOrigin.Current);
                    matRef.matNameLength = sr.Read<byte>();
                    matRef.matName = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position(), matRef.matNameLength));
                    sr.Seek(matRef.matNameLength, System.IO.SeekOrigin.Current);
                    if (hasExtraFlags && matCount > 1 && (i + 1 != matCount))
                    {
                        matRef.startingFaceIndex = sr.Read<int>();
                        matRef.endingFaceIndex = sr.Read<int>();
                    }
                    else if (!hasExtraFlags) //SOTC
                    {
                        matRef.unkByte = sr.Read<byte>();
                        matRef.startingVertexIndex = sr.Read<int>();
                        matRef.vertexIndicesUsed = sr.Read<int>();
                        matRef.startingFaceVertIndex = sr.Read<int>();
                        matRef.faceVertIndicesUsed = sr.Read<int>();
                    }
                    matList.Add(matRef);
                }
            }
        }

        private void ReadReferenceModelPath(BufferedStreamReader sr)
        {
            var mdlLen = sr.Read<byte>();

            //Some strings seem to insert a 0x1 and then place the actual string after
            var test = sr.Peek<byte>();
            if (test == 0x1)
            {
                sr.Seek(1, System.IO.SeekOrigin.Current);
            }
            OtherModelName = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position(), mdlLen));
        }

        private void ReadA8CHeaderExtras(BufferedStreamReader sr)
        {
            unk8C0ASize0Modifier = sr.Read<byte>();
            unk8C0ASize0 = sr.Read<ushort>();
            subVariantFlag = sr.Read<byte>();
            unk8C0ASize1 = sr.Read<ushort>();
            unk8C0AByte0 = sr.Read<byte>();
            modelType = sr.Read<int>();
            matCount = sr.Read<byte>();
            unk8C0AShort0 = sr.Read<ushort>();
            unk8C0AInnerBigPtrByte = sr.Read<byte>();
            unk8C0AModelDataSize = sr.ReadBigEndianPrimitive<ushort>();

            //We need to transform unk8C0AModelDataSize based on unk8C0AInnerBigPtrByte's value. If 0, we can ignore this
            switch (unk8C0AInnerBigPtrByte)
            {
                case 1:
                    throw new NotImplementedException();
                    break;
                case 2:
                    throw new NotImplementedException();
                    break;
                case 0:
                default:
                    break;
            }

            //Unknown territory past here, needs research into the tokenization
        }

        private void CheckForExtraFlags(BufferedStreamReader sr)
        {
            //For certain SOTC models
            var crcCheck = sr.ReadBytes(sr.Position(), 4);
            hasExtraFlags = crcCheck[1] > 0 || crcCheck[2] > 0 || crcCheck[3] > 0;
            if (variantFlags == 0x200 && BitConverter.ToUInt64(sr.ReadBytes(0x25, 8), 0) == dummyConstData)
            {
                hasExtraFlags = true;
            }
            
            if (hasExtraFlags)
            {
                extraFlags = sr.ReadBytes(sr.Position(), 4);
                sr.Seek(4, System.IO.SeekOrigin.Current);
            }
        }
    }

}
