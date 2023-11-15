using csharp_prs;
using Reloaded.Memory.Streams;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

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
            header.totalBufferSize = initialSR.ReadBE<int>();
            header.totalBufferDifferenceFromCompressed = initialSR.ReadBE<int>();
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

        public byte[] GetBytes()
        {
            ByteListExtension.AddAsBigEndian = true;
            List<byte> outBytes = new List<byte>();
            outBytes.AddValue((int)1);
            outBytes.ReserveInt("totalBufferSize");
            outBytes.ReserveInt("totalBufferDifferenceFromCompressed");
            outBytes.ReserveInt("uncompressedDataSize");

            outBytes.ReserveInt("compressedDataSize");
            outBytes.AddValue((int)0);
            outBytes.AddValue((int)0);
            outBytes.AddValue((int)0);

            List<byte> innerBytes = new List<byte>()
            {
                0x55,
                0xAA,
                0x38,
                0x2D,
                0x0,
                0x0,
                0x0,
                0x20,
            };
            innerBytes.ReserveInt("fileEntriesSize");
            innerBytes.ReserveInt("fullSize");

            innerBytes.AddValue(0xCCCCCCCCCCCCCCCC);
            innerBytes.AddValue(0xCCCCCCCCCCCCCCCC);

            innerBytes.Add((byte)0x1);
            innerBytes.Add((byte)0x0);
            innerBytes.Add((byte)0x0);
            innerBytes.Add((byte)0x0);
            innerBytes.AddValue((int)0);
            innerBytes.AddValue((int)fileNames.Count + 1);

            for (int i = 0; i < fileNames.Count; i++)
            {
                innerBytes.ReserveInt($"nameOffset{i}");
                innerBytes.ReserveInt($"fileOffset{i}");
                innerBytes.AddValue((int)files[i].Length);
            }
            var namesStart = innerBytes.Count;
            innerBytes.Add((byte)0);
            for (int i = 0; i < fileNames.Count; i++)
            {
                innerBytes.FillInt($"nameOffset{i}", innerBytes.Count - namesStart);
                innerBytes.AddRange(Encoding.UTF8.GetBytes(fileNames[i]));
                innerBytes.Add((byte)0);
            }
            innerBytes.FillInt("fileEntriesSize", innerBytes.Count - 0x20);
            innerBytes.AlignWrite(0x20);
            innerBytes.FillInt("fullSize", innerBytes.Count);

            for (int i = 0; i < files.Count; i++)
            {
                innerBytes.FillInt($"fileOffset{i}", innerBytes.Count);
                innerBytes.AddRange(files[i]);
                if (i != files.Count - 1)
                {
                    innerBytes.AlignWrite(0x20);
                }
            }

            var prs = Prs.Compress(innerBytes.ToArray(), 0x1FFF);
            outBytes.AddRange(prs);
            outBytes.FillInt("uncompressedDataSize", innerBytes.Count);
            outBytes.FillInt("compressedDataSize", prs.Length);
            var bufferAddition = 0x20 - ((innerBytes.Count - prs.Length) % 0x20);
            //Hack to ensure the buffersize is enough
            bufferAddition += 0x40;
            var compressionDifference = (innerBytes.Count - prs.Length);
            var finalDifference = compressionDifference + bufferAddition;
            var finalTotalBufferSize = finalDifference + prs.Length;
            outBytes.FillInt("totalBufferSize", finalTotalBufferSize);
            outBytes.FillInt("totalBufferDifferenceFromCompressed", finalDifference);

            return outBytes.ToArray();
        }

        public struct PRDHeader
        {
            public int endianness; //Endianness check?
            public int totalBufferSize; //Size of totalBufferDifferenceFromCompressed + compressedSize
            public int totalBufferDifferenceFromCompressed; //Uncompressed + compressed difference, rounded up to nearest multiple of 0x20. 0x20 is also added if modulo is 1 or 0, perhaps due to a bug.
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
