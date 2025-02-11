using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    public class Event : ARC
    {
        public Event() { }
        public Event(byte[] file)
        {
            Read(file);
        }

        public Event(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }
        public void Read(byte[] file)
        {
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            sr._BEReadActive = true;
            base.Read(sr);
            sr.Seek(0x20, SeekOrigin.Begin);

            var scriptCount = sr.ReadBE<int>();
            var nextSectionOffset = sr.ReadBE<int>();

            List<int> scriptRootOffsets = new List<int>();
            for (int i = 0; i < scriptCount; i++)
            {
                scriptRootOffsets.Add(sr.ReadBE<int>());
            }
            for (int i = 0; i < scriptRootOffsets.Count; i++)
            {
                sr.Seek(0x20 + scriptRootOffsets[i], SeekOrigin.Begin);
            }


        }
    }

    public class EventScript
    {
        public int int_00;
        public int scriptValue0;
        public int int_08;
        public int subStructType;

        public int subStructOffsetOffset;
        public int subStructOffset;

    }

    public class EventScriptSubstruct1
    {
        public int scriptNameOffset;
        public int scriptInt_04;
        public int scriptShortOffset;

        public short scriptShortValue;
    }
}
