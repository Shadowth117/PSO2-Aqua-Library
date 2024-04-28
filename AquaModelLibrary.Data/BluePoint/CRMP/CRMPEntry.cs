using AquaModelLibrary.Helpers.Readers;
using System.Text;

namespace AquaModelLibrary.Data.BluePoint.CRMP
{
    public class CRMPEntry
    {
        public int idHash = -1;
        public string reference;

        public CRMPEntry() { }

        public CRMPEntry(BufferedStreamReaderBE<MemoryStream> sr) 
        {
            idHash = sr.ReadBE<int>();
            int refOffset = sr.ReadBE<int>();
            var position = sr.Position;
            sr.Seek(refOffset - 0x4, SeekOrigin.Current);
            reference = sr.ReadCString();
            sr.Seek(position, SeekOrigin.Begin);
        }
    }
}
