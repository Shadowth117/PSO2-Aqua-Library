using csharp_prs;
using Reloaded.Memory.Streams;
using System.Collections.Generic;
using System.IO;

namespace AquaModelLibrary.Extra.Ninja.BillyHatcher
{
    public class PRD
    {
        public PRDHeader header;
        public PRDArchiveHeader archiveHeader;
        public PRDFileEntryHeader fileEntryHeader;
        public List<PRDFileEntry> fileEntries = new List<PRDFileEntry>();
        public List<byte[]> files = new List<byte[]>();
        public List<string> fileNames = new List<string>();

        public PRD() { }

        public PRD(BufferedStreamReader initialSR)
        {
            header = new PRDHeader();
            BigEndianHelper._active = initialSR.Peek<int>() > 0;
            header.endianness = initialSR.ReadBE<int>();
            header.int04 = initialSR.ReadBE<int>();
            header.int08 = initialSR.ReadBE<int>();
            header.uncompressedDataSize = initialSR.ReadBE<int>();

            header.compressedDataSize = initialSR.ReadBE<int>();
            header.reserve0 = initialSR.ReadBE<int>();
            header.reserve1 = initialSR.ReadBE<int>();
            header.reserve2 = initialSR.ReadBE<int>();

            var archiveData = Prs.Decompress(initialSR.ReadBytes(0x20, header.compressedDataSize));
            using (var stream = new MemoryStream(archiveData))
            using (var sr = new BufferedStreamReader(stream, 8192))
            {
                archiveHeader = new PRDArchiveHeader();
                archiveHeader.magic = sr.ReadBE<int>();
                archiveHeader.baseSize = sr.ReadBE<int>();
                archiveHeader.fileEntriesSize = sr.ReadBE<int>();
                archiveHeader.fullSize = sr.ReadBE<int>();

                archiveHeader.ccInt0 = sr.ReadBE<int>();
                archiveHeader.ccInt1 = sr.ReadBE<int>();
                archiveHeader.ccInt2 = sr.ReadBE<int>();
                archiveHeader.ccInt3 = sr.ReadBE<int>();

                fileEntryHeader = new PRDFileEntryHeader();
                fileEntryHeader.int00 = sr.ReadBE<int>();
                fileEntryHeader.reserve0 = sr.ReadBE<int>();
                fileEntryHeader.nullTerminatorCount = sr.ReadBE<int>();

                for (int i = 0; i < fileEntryHeader.nullTerminatorCount - 1; i++)
                {
                    var entry = new PRDFileEntry();
                    entry.fileNameStringRelativeOffset = sr.ReadBE<int>();
                    entry.fileOffset = sr.ReadBE<int>();
                    entry.fileSize = sr.ReadBE<int>();
                    fileEntries.Add(entry);
                }
                var entriesEndOffset = sr.Position();
                for (int i = 0; i < fileEntryHeader.nullTerminatorCount - 1; i++)
                {
                    var entry = fileEntries[i];
                    sr.Seek(entry.fileNameStringRelativeOffset + entriesEndOffset, SeekOrigin.Begin);
                    fileNames.Add(AquaMethods.AquaGeneralMethods.ReadCString(sr));
                    files.Add(sr.ReadBytes(entry.fileOffset, entry.fileSize));
                }
            }
        }

        public struct PRDHeader
        {
            public int endianness; //Endianness check?
            public int int04; //Size of int08 + compressedSize
            public int int08; //Unknown, size above compressed data size needed for memory allocation?
            public int uncompressedDataSize;

            public int compressedDataSize;
            public int reserve0;
            public int reserve1;
            public int reserve2;
        }

        public struct PRDArchiveHeader
        {
            public int magic;
            public int baseSize;
            public int fileEntriesSize;
            public int fullSize; //Includes padding

            public int ccInt0;
            public int ccInt1;
            public int ccInt2;
            public int ccInt3;
        }

        public struct PRDFileEntryHeader
        {
            public int int00; //Endianness check again?
            public int reserve0;
            public int nullTerminatorCount; //File name string count + 1, there seems to always be a single 0 at the start of the file name strings
        }

        public struct PRDFileEntry
        {
            public int fileNameStringRelativeOffset; //Relative to end of PRDFileEntryHeader list
            public int fileOffset;
            public int fileSize;
        }
    }
}
