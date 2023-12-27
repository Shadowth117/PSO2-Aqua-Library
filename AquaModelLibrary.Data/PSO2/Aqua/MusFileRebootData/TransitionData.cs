using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.PSO2.Aqua.MusFileRebootData
{
    public class TransitionData
    {
        public TransitionDataStruct transStruct;
        public TransitionDataSubStruct transSubStruct;
        public string transitionClipName = null;
        public string subStructStr_04 = null;

        public TransitionData()
        {

        }

        public TransitionData(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            transStruct = sr.Read<TransitionDataStruct>();

            var bookmark = sr.Position;
            if (transStruct.transitionClipNameOffset != 0x10 && transStruct.transitionClipNameOffset != 0)
            {
                sr.Seek(offset + transStruct.transitionClipNameOffset, SeekOrigin.Begin);
                transitionClipName = sr.ReadCString();
            }

            if (transStruct.transitionDataSubstructOffset != 0x10 && transStruct.transitionDataSubstructOffset > 0)
            {
                sr.Seek(offset + transStruct.transitionDataSubstructOffset, SeekOrigin.Begin);
                transSubStruct = sr.Read<TransitionDataSubStruct>();

                if (transSubStruct.str_04 != 0x10 && transSubStruct.str_04 != 0)
                {
                    sr.Seek(offset + transSubStruct.str_04, SeekOrigin.Begin);
                    subStructStr_04 = sr.ReadCString();
                }
            }

            sr.Seek(bookmark, SeekOrigin.Begin);
        }
    }

    public struct TransitionDataStruct
    {
        public int int_00;
        public int transitionDataSubstructOffset;
        public float flt_08;
        public int transitionClipNameOffset;
    }

    public struct TransitionDataSubStruct
    {
        public int int_00;
        public int str_04;
    }
}
