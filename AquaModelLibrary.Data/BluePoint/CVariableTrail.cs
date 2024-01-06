using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BluePoint
{
    public class CVariableTrail
    {
        public List<byte> data = new List<byte>();

        public CVariableTrail() { }

        public CVariableTrail(BufferedStreamReaderBE<MemoryStream> sr, int limit = 4)
        {
            byte? current = null;
            for (int i = 0; i < limit; i++)
            {
                current = sr.Read<byte>();
                if (current == 0)
                {
                    break;
                }
                data.Add((byte)current);
            }
        }
    }
}
