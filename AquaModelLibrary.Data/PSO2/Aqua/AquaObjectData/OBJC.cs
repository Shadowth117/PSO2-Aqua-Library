using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers;
using System.Numerics;
using System.Text;
using static AquaModelLibrary.Helpers.PSO2.VTBFMethods;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData
{
    public struct OBJC
    {
        public int type;           //0x10, Type 0x8
        public int size;           //0x11, Type 0x8
        public int unkMeshValue;   //0x12, Type 0x9
        public int largetsVtxl;       //In 0xC33 objects, this is the largest possible entry. Smaller entries will fill and skip past the remainder.

        public int totalStripFaces;  //0x14, Type 0x9
        public int globalStripOffset; //Unused in classic. Always 0x100 in 0xC33 since it's directly after OBJC in this one.
        public int totalVTXLCount;   //0x15, Type 0x8
        public int vtxlStartOffset; //Unused in classic.

        public int unkStructCount;        //0x16, Type 0x8 //Same value as below in classic.
        public int vsetCount;       //0x24, Type 0x9
        public int vsetOffset;
        public int psetCount;       //0x25, Type 0x9

        public int psetOffset;
        public int meshCount;    //0x17, Type 0x9
        public int meshOffset;
        public int mateCount;    //0x18, Type 0x8

        public int mateOffset;
        public int rendCount;    //0x19, Type 0x8
        public int rendOffset;
        public int shadCount;    //0x1A, Type 0x8

        public int shadOffset;
        public int tstaCount;    //0x1B, Type 0x8
        public int tstaOffset;
        public int tsetCount;    //0x1C, Type 0x8

        public int tsetOffset;
        public int texfCount;    //0x1D, Type 0x8
        public int texfOffset;

        public BoundingVolume bounds; //0x1E, 0x1F, 0x20, 0x21
        public int unkCount0;

        public int unrmOffset; //Never set if unused. 

        //End of classic OBJC

        public int vtxeCount;
        public int vtxeOffset;
        public int bonePaletteOffset; //0xC33 only uses a single bone palette

        public int fBlock0; //These 4 seemingly do nothing in normal models, but have values in some trp/tro variations of the format.
        public int fBlock1; //Maybe these are all shorts?
        public int fBlock2;
        public int fBlock3;

        public int unkStruct1Count;
        public int unkStruct1Offset;
        public int pset2Count;
        public int pset2Offset;

        public int mesh2Count;
        public int mesh2Offset;
        public int globalStrip3Offset;
        public int globalStrip3LengthCount;

        public int globalStrip3LengthOffset;
        public int unkPointArray1Offset;
        public int unkPointArray2Offset;
        public int unkCount3;

        #region Readers
        public void ReadVTBF(List<Dictionary<int, object>> objcRaw)
        {
            type = (int)(objcRaw[0][0x10]);
            size = (int)(objcRaw[0][0x11]);
            unkMeshValue = (int)(objcRaw[0][0x12]);
            largetsVtxl = (int)(objcRaw[0][0x13]);
            totalStripFaces = (int)(objcRaw[0][0x14]);
            totalVTXLCount = (int)(objcRaw[0][0x15]);
            unkStructCount = (int)(objcRaw[0][0x16]);
            vsetCount = (int)(objcRaw[0][0x24]);
            psetCount = (int)(objcRaw[0][0x25]);
            meshCount = (int)(objcRaw[0][0x17]);
            mateCount = (int)(objcRaw[0][0x18]);
            rendCount = (int)(objcRaw[0][0x19]);
            shadCount = (int)(objcRaw[0][0x1A]);
            tstaCount = (int)(objcRaw[0][0x1B]);
            tsetCount = (int)(objcRaw[0][0x1C]);
            texfCount = (int)(objcRaw[0][0x1D]);

            BoundingVolume bounding = new BoundingVolume();
            bounding.modelCenter = ((Vector3)(objcRaw[0][0x1E]));

            if (objcRaw[0].ContainsKey(0x1F))
            {
                bounding.boundingRadius = (float)(objcRaw[0][0x1F]);
            }
            if (objcRaw[0].ContainsKey(0x20))
            {
                bounding.modelCenter2 = ((Vector3)(objcRaw[0][0x20]));
            }
            if (objcRaw[0].ContainsKey(0x21))
            {
                bounding.halfExtents = ((Vector3)(objcRaw[0][0x21]));
            }

            bounds = bounding;
        }

        public static OBJC ReadOBJC(BufferedStreamReaderBE<MemoryStream> streamReader)
        {
            OBJC objc = new OBJC();
            objc.Read(streamReader);
            return objc;
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> streamReader)
        {
            type = streamReader.Read<int>();
            size = streamReader.Read<int>();
            unkMeshValue = streamReader.Read<int>();
            largetsVtxl = streamReader.Read<int>();

            totalStripFaces = streamReader.Read<int>();
            globalStripOffset = streamReader.Read<int>();
            totalVTXLCount = streamReader.Read<int>();
            vtxlStartOffset = streamReader.Read<int>();

            unkStructCount = streamReader.Read<int>();
            vsetCount = streamReader.Read<int>();
            vsetOffset = streamReader.Read<int>();
            psetCount = streamReader.Read<int>();

            psetOffset = streamReader.Read<int>();
            meshCount = streamReader.Read<int>();
            meshOffset = streamReader.Read<int>();
            mateCount = streamReader.Read<int>();

            mateOffset = streamReader.Read<int>();
            rendCount = streamReader.Read<int>();
            rendOffset = streamReader.Read<int>();
            shadCount = streamReader.Read<int>();

            shadOffset = streamReader.Read<int>();
            tstaCount = streamReader.Read<int>();
            tstaOffset = streamReader.Read<int>();
            tsetCount = streamReader.Read<int>();

            tsetOffset = streamReader.Read<int>();
            texfCount = streamReader.Read<int>();
            texfOffset = streamReader.Read<int>();

            bounds = streamReader.Read<BoundingVolume>();
            unkCount0 = streamReader.Read<int>();
            unrmOffset = streamReader.Read<int>();

            if (type >= 0xC32)
            {

                vtxeCount = streamReader.Read<int>();
                vtxeOffset = streamReader.Read<int>();
                bonePaletteOffset = streamReader.Read<int>();

                fBlock0 = streamReader.Read<int>();
                fBlock1 = streamReader.Read<int>();
                fBlock2 = streamReader.Read<int>();
                fBlock3 = streamReader.Read<int>();

                unkStruct1Count = streamReader.Read<int>();
                unkStruct1Offset = streamReader.Read<int>();
                pset2Count = streamReader.Read<int>();
                pset2Offset = streamReader.Read<int>();

                if (type >= 0xC33)
                {
                    mesh2Count = streamReader.Read<int>();
                    mesh2Offset = streamReader.Read<int>();
                    globalStrip3Offset = streamReader.Read<int>();
                    globalStrip3LengthCount = streamReader.Read<int>();

                    globalStrip3LengthOffset = streamReader.Read<int>();
                    unkPointArray1Offset = streamReader.Read<int>();
                    unkPointArray2Offset = streamReader.Read<int>();
                    unkCount3 = streamReader.Read<int>();
                }
                //objc.type = 0xC33; //0xC33 is essentially the same as 32 so we can treat it as that from here. It's just that objc just doesn't have those last 2 fields or the associated arrays.
            }
        }
        #endregion

        #region Writing
        public byte[] GetBytesVTBF(bool useUNRMs)
        {
            List<byte> outBytes = new List<byte>();

            ushort pointerCount = 0;
            pointerCount += VTBFFlagCheck(vsetCount);
            pointerCount += VTBFFlagCheck(psetCount);
            pointerCount += VTBFFlagCheck(meshCount);
            pointerCount += VTBFFlagCheck(mateCount);
            pointerCount += VTBFFlagCheck(rendCount);
            pointerCount += VTBFFlagCheck(shadCount);
            pointerCount += VTBFFlagCheck(tstaCount);
            pointerCount += VTBFFlagCheck(tsetCount);
            pointerCount += VTBFFlagCheck(texfCount);
            if (useUNRMs)
            {
                pointerCount += 1;
            }

            outBytes.AddRange(Encoding.UTF8.GetBytes("vtc0"));
            outBytes.AddRange(BitConverter.GetBytes(0x9B));          //Data body size is always 0x9B for OBJC
            outBytes.AddRange(Encoding.UTF8.GetBytes("OBJC"));
            outBytes.AddRange(BitConverter.GetBytes(pointerCount));
            outBytes.AddRange(BitConverter.GetBytes((short)0x14)); //Subtag count, always 0x14 for OBJC

            AddBytes(outBytes, 0x10, 0x8, BitConverter.GetBytes(type)); //Should just always be 0xC2A. Perhaps some kind of header info?
            AddBytes(outBytes, 0x11, 0x8, BitConverter.GetBytes(size)); //Size of the final data struct, always 0xA4. This ends up being the exact size of the NIFL variation of OBJC.
            AddBytes(outBytes, 0x12, 0x9, BitConverter.GetBytes(unkMeshValue));
            AddBytes(outBytes, 0x13, 0x8, BitConverter.GetBytes(largetsVtxl));
            AddBytes(outBytes, 0x14, 0x9, BitConverter.GetBytes(totalStripFaces));
            AddBytes(outBytes, 0x15, 0x8, BitConverter.GetBytes(totalVTXLCount));
            AddBytes(outBytes, 0x16, 0x8, BitConverter.GetBytes(unkStructCount));
            AddBytes(outBytes, 0x24, 0x9, BitConverter.GetBytes(vsetCount));
            AddBytes(outBytes, 0x25, 0x9, BitConverter.GetBytes(psetCount));
            AddBytes(outBytes, 0x17, 0x9, BitConverter.GetBytes(meshCount));
            AddBytes(outBytes, 0x18, 0x8, BitConverter.GetBytes(mateCount));
            AddBytes(outBytes, 0x19, 0x8, BitConverter.GetBytes(rendCount));
            AddBytes(outBytes, 0x1A, 0x8, BitConverter.GetBytes(shadCount));
            AddBytes(outBytes, 0x1B, 0x8, BitConverter.GetBytes(tstaCount));
            AddBytes(outBytes, 0x1C, 0x8, BitConverter.GetBytes(tsetCount));
            AddBytes(outBytes, 0x1D, 0x8, BitConverter.GetBytes(texfCount));
            AddBytes(outBytes, 0x1E, 0x4A, 0x1, DataHelpers.ConvertStruct(bounds.modelCenter));
            AddBytes(outBytes, 0x1F, 0xA, BitConverter.GetBytes(bounds.boundingRadius));
            AddBytes(outBytes, 0x20, 0x4A, 0x1, DataHelpers.ConvertStruct(bounds.modelCenter2));
            AddBytes(outBytes, 0x21, 0x4A, 0x1, DataHelpers.ConvertStruct(bounds.halfExtents));

            return outBytes.ToArray();
        }

        #endregion
    }
}
