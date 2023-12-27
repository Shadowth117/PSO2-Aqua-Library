using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.AM2.BorderBreakPS4
{
    //Border Break PS4 .pfa files
    public class FARC
    {
        public FARCHeader header;
        public List<FARCEntryObject> fileEntries = new List<FARCEntryObject>();
        public struct FARCHeader
        {
            public int magic;
            public int fileCount;
            public int tableStartAddress;
            public int farcDataSize; //Minus initial header bytes
        }

        public class FARCEntryObject
        {
            public FARCEntry entryStruct;
            public string fileName;
            public byte[] fileData;
        }

        public struct FARCEntry
        {
            public int nameOffset;
            public int fileOffset;
            public int size0;
            public int size1; //Idk, we have two for w/e reason
        }

        public FARC()
        {

        }

        public FARC(BufferedStreamReaderBE<MemoryStream> streamReader)
        {
            header = streamReader.Read<FARCHeader>();

            for (int i = 0; i < header.fileCount; i++)
            {
                FARCEntryObject farcEntry = new FARCEntryObject();
                farcEntry.entryStruct = streamReader.Read<FARCEntry>();

                var pos = streamReader.Position;

                streamReader.Seek(farcEntry.entryStruct.nameOffset, SeekOrigin.Begin);
                farcEntry.fileName = streamReader.ReadCString();
                farcEntry.fileData = streamReader.ReadBytes(farcEntry.entryStruct.fileOffset, farcEntry.entryStruct.size0);

                streamReader.Seek(pos, SeekOrigin.Begin);
                fileEntries.Add(farcEntry);
            }
        }
    }
}
