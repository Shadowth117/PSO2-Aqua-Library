using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    public class FieldPlayerObject : AquaCommon
    {
        public List<FPOEntryObject> fpoEntries = new List<FPOEntryObject>();
        public FPOHeader fpoHeader;

        public override string[] GetEnvelopeTypes()
        {
            return new string[] {
            "fpo\0"
            };
        }

        public FieldPlayerObject() { }

        public FieldPlayerObject(byte[] file) : base(file) { }

        public FieldPlayerObject(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }

        public override void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            fpoHeader = sr.Read<FPOHeader>();

            //Entries
            sr.Seek(offset + fpoHeader.entryOffset, SeekOrigin.Begin);
            for (int i = 0; i < fpoHeader.entryCount; i++)
            {
                FPOEntryObject fpoEntry = new FPOEntryObject();
                fpoEntry.fpoEntry = sr.Read<FPOEntry>();

                var bookmark = sr.Position;
                if (fpoEntry.fpoEntry.asciiNameOffset > 0x10)
                {
                    sr.Seek(offset + fpoEntry.fpoEntry.asciiNameOffset, SeekOrigin.Begin);
                    fpoEntry.asciiName = sr.ReadCString();
                }
                else
                {
                    fpoEntry.asciiName = "";
                }

                fpoEntries.Add(fpoEntry);
                sr.Seek(bookmark, SeekOrigin.Begin);
            }
        }

        public struct FPOHeader
        {
            public int unkInt0;
            public int entryCount;
            public int entryOffset;
        }

        public class FPOEntryObject
        {
            public FPOEntry fpoEntry;

            public string asciiName = null;
        }

        public struct FPOEntry
        {
            public int asciiNameOffset;
            public int id;
            public byte unkByte0;
            public byte unkByte1;
            public byte unkByte2;
            public byte unkByte3;
        }
    }
}
