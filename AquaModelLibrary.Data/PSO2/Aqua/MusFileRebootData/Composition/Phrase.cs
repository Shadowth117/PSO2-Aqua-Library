using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.PSO2.Aqua.MusFileRebootData.Composition
{
    //Phrase
    public class Phrase
    {
        public List<Bar> bars = new List<Bar>();
        public PhraseStruct phraseStruct;

        public Phrase()
        {

        }

        public Phrase(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            phraseStruct = sr.Read<PhraseStruct>();

            var bookmark = sr.Position;

            sr.Seek(offset + phraseStruct.barOffset, SeekOrigin.Begin);
            for (int i = 0; i < phraseStruct.barCount; i++)
            {
                bars.Add(new Bar(sr, offset));
            }

            sr.Seek(bookmark, SeekOrigin.Begin);
        }
    }

    public struct PhraseStruct
    {
        public int barOffset;
        public int barCount;
    }
}
