using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.PSO2.Aqua.MusFileRebootData.Composition
{
    //Movement (Shuffle, skip operations etc.)
    public class Movement
    {
        public List<Phrase> phrases = new List<Phrase>();
        public MovementStruct movementStruct;

        public Movement()
        {

        }

        public Movement(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            movementStruct = sr.Read<MovementStruct>();

            var bookmark = sr.Position;

            sr.Seek(offset + movementStruct.phraseOffset, SeekOrigin.Begin);
            for (int i = 0; i < movementStruct.phraseCount; i++)
            {
                phrases.Add(new Phrase(sr, offset));
            }

            sr.Seek(bookmark, SeekOrigin.Begin);
        }

    }

    public struct MovementStruct
    {
        public int phraseOffset;
        public int phraseCount;
    }
}
