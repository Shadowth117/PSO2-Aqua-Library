using AquaModelLibrary.AquaMethods;
using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.Extra.AM2
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

        public FARC (BufferedStreamReader streamReader)
        {
            header = streamReader.Read<FARCHeader>();

            for(int i = 0; i < header.fileCount; i++)
            {
                FARCEntryObject farcEntry = new FARCEntryObject();
                farcEntry.entryStruct = streamReader.Read<FARCEntry>();

                var pos = streamReader.Position();

                streamReader.Seek(farcEntry.entryStruct.nameOffset, System.IO.SeekOrigin.Begin);
                farcEntry.fileName = AquaGeneralMethods.ReadCString(streamReader);
                farcEntry.fileData = streamReader.ReadBytes(farcEntry.entryStruct.fileOffset, farcEntry.entryStruct.size0);

                streamReader.Seek(pos, System.IO.SeekOrigin.Begin);
                fileEntries.Add(farcEntry);
            }
        }
    }
}
