using AquaModelLibrary.Data.PSO2.Aqua.FCLData;
using AquaModelLibrary.Helpers.Readers;
using System.IO;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    public class FacialFCL : AquaCommon
    {
        public FCLHeader header;
        public List<int> unkIntList = new List<int>();
        public List<FCLFrameObject> frames = new List<FCLFrameObject>();

        public override string[] GetEnvelopeTypes()
        {
            return new string[] {
            "fcl\0"
            };
        }

        public FacialFCL() { }

        public FacialFCL(byte[] file) : base(file) { }

        public FacialFCL(BufferedStreamReaderBE<MemoryStream> streamReader) : base(streamReader) { }

        public override void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            header = sr.Read<FCLHeader>();

            sr.Seek(offset + header.unkIntListOffset, SeekOrigin.Begin);
            for (int i = 0; i < header.unkIntListCount; i++)
            {
                unkIntList.Add(sr.Read<int>());
            }

            sr.Seek(offset + header.frameListOffset, SeekOrigin.Begin);
            for (int i = 0; i < header.frameCount; i++)
            {
                var frame = new FCLFrameObject();
                frame.fclFrameStruct = sr.Read<FCLFrame>();

                var tempPos = sr.Position;
                sr.Seek(offset + frame.fclFrameStruct.frameValueOffset, SeekOrigin.Begin);
                frame.frameValue = sr.Read<float>();
                sr.Seek(tempPos, SeekOrigin.Begin);

                frames.Add(frame);
            }
        }
    }
}
