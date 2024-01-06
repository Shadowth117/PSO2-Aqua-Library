using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BluePoint.CANI
{

    //Thanks to Meowmaritus for their own notes
    public class CANIHeader
    {
        public CANIMainHeader caniHeader;
        public List<CANIFrameDataSetInfo> info = new List<CANIFrameDataSetInfo>();

        public CANIHeader()
        {

        }

        public CANIHeader(BufferedStreamReaderBE<MemoryStream> sr)
        {
            caniHeader = sr.Read<CANIMainHeader>();
            info = new List<CANIFrameDataSetInfo>();
            for (int i = 0; i < caniHeader.frameDataSetCount; i++)
            {
                info.Add(sr.Read<CANIFrameDataSetInfo>());
            }
        }
    }

    public struct CANIMainHeader
    {
        public int magic;
        public int unkConst;
        public int size; //Anim footer will be here
        public ushort frameDataSetCount;
        public ushort usht_0E;

        public int int_10;
        public int int_14; //Usually 0x30
        public int caniFooterPtr;
        public float fps;

        public ushort usht_20;
        public ushort usht_22;
        public float float_24;
        public float endFrame;
        public float frameDuration;
    }

    public struct CANIFrameDataSetInfo
    {
        public int frameDataSetPointer;
        public ushort unk0;
        public ushort largeDataFlags;
        public int largeDataPointer;
        public int largeDataUnk;
    }
}
