using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaFigureData
{
    public class AnimMapObject
    {
        public AnimMapStruct animStruct;
        public string name = null;
        public string followUp = null;
        public string type = null;
        public string anim = null;
        public List<AnimFrameInfo> frameInfoList = new List<AnimFrameInfo>();

        public AnimMapObject() { }

        public AnimMapObject(int offset, long address, BufferedStreamReaderBE<MemoryStream> sr)
        {
            sr.Seek(offset + address, SeekOrigin.Begin);
            animStruct = sr.Read<AnimMapStruct>();
            name = sr.ReadCStringValidOffset(animStruct.namePtr, offset);
            followUp = sr.ReadCStringValidOffset(animStruct.followUpPtr, offset);
            type = sr.ReadCStringValidOffset(animStruct.typePtr, offset);
            anim = sr.ReadCStringValidOffset(animStruct.animPtr, offset);

            List<int> frameInfoPtrs = new List<int>();
            if (animStruct.frameInfoPtr > 0x10)
            {
                sr.Seek(offset + animStruct.frameInfoPtr, SeekOrigin.Begin);
                for (int fi = 0; fi < animStruct.frameInfoPtrCount; fi++)
                {
                    frameInfoPtrs.Add(sr.Read<int>());
                }

                for (int fi = 0; fi < animStruct.frameInfoPtrCount; fi++)
                {
                    sr.Seek(offset + frameInfoPtrs[fi], SeekOrigin.Begin);
                    frameInfoList.Add(sr.Read<AnimFrameInfo>());
                }
            }
        }
    }

    public struct AnimMapStruct
    {
        public int namePtr;
        public int followUpPtr;
        public int typePtr;
        public int animPtr;

        public float flt_10;
        public float flt_14;
        public float flt_18;
        public float flt_1C;

        public int int_20;
        public int frameInfoPtr;
        public int frameInfoPtrCount;
    }

    //Seemingly, the ones with default frames are set specially somehow. -1, -1 may play on transition.
    public struct AnimFrameInfo
    {
        public float startFrame; //-1 if default
        public float endFrame;   //9999 if default
        public int effectId;
    }
}
