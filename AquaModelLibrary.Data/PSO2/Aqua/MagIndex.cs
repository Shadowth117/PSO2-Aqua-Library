using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers.PSO2;
using System.Diagnostics;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    public class MagIndex : AquaCommon
    {
        private List<int> mgxIds = new();

        public override string[] GetEnvelopeTypes()
        {
            return new string[] {
            "mgx\0"
            };
        }

        public MagIndex() { }

        public MagIndex(byte[] file)
        {
            Read(file);
        }

        public MagIndex(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        public List<int> GetMagIdList() => mgxIds;

        public override void ReadVTBFFile(BufferedStreamReaderBE<MemoryStream> sr)
        {
            //Seek past vtbf tag
            sr.Seek(0x10, SeekOrigin.Current);          //VTBF tags

            while (sr.Position < (int)sr.BaseStream.Length)
            {
                var data = VTBFMethods.ReadVTBFTag(sr, out string tagType, out int ptrCount, out int entryCount);
                switch (tagType)
                {
                    case "DOC ":
                        break;
                    case "MAGR":
                        mgxIds.Add((int)data[0][0xFF]);
                        break;
                    default:
                        //Should mean it's done.
                        Debug.WriteLine($"Defaulted tag was: {tagType}");
                        break;
                }
            }
        }
    }
}
