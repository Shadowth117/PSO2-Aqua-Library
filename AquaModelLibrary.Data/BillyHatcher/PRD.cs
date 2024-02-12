using AquaModelLibrary.Helpers;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using System.Text;

namespace AquaModelLibrary.Data.BillyHatcher
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

        public PRD(string filePath)
        {
            using (var stream = new MemoryStream(File.ReadAllBytes(filePath)))
            using (var sr = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                if (Path.GetExtension(filePath) == ".nrc")
                {
                    ReadNRC(sr);
                }
                else
                {
                    Read(sr);
                }
            }
        }

        public PRD(byte[] bytes, bool isNRC = false)
        {
            using (var stream = new MemoryStream(bytes))
            using (var sr = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                if (isNRC)
                {
                    ReadNRC(sr);
                }
                else
                {
                    Read(sr);
                }
            }
        }

        public PRD(BufferedStreamReaderBE<MemoryStream> initialSR, bool isNRC = false)
        {
            if (isNRC)
            {
                ReadNRC(initialSR);
            }
            else
            {
                Read(initialSR);
            }
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> initialSR)
        {
            header = new PRDHeader();
            initialSR._BEReadActive = initialSR.Peek<int>() > 0;
            header.endianness = initialSR.ReadBE<int>();
            header.totalBufferSize = initialSR.ReadBE<int>();
            header.totalBufferDifferenceFromCompressed = initialSR.ReadBE<int>();
            header.uncompressedDataSize = initialSR.ReadBE<int>();

            header.compressedDataSize = initialSR.ReadBE<int>();
            header.reserve0 = initialSR.ReadBE<int>();
            header.reserve1 = initialSR.ReadBE<int>();
            header.reserve2 = initialSR.ReadBE<int>();

            var archiveData = PRSHelpers.PRSDecompress(initialSR.ReadBytes(0x20, header.compressedDataSize));
            using (var stream = new MemoryStream(archiveData))
            using (var sr = new BufferedStreamReaderBE<MemoryStream>(stream, 8192))
            {
                ReadNRC(sr);
            }
        }

        public void ReadNRC(BufferedStreamReaderBE<MemoryStream> sr)
        {
            sr._BEReadActive = true;
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
            var entriesEndOffset = sr.Position;
            for (int i = 0; i < fileEntryHeader.nullTerminatorCount - 1; i++)
            {
                var entry = fileEntries[i];
                sr.Seek(entry.fileNameStringRelativeOffset + entriesEndOffset, SeekOrigin.Begin);
                fileNames.Add(sr.ReadCString());
                files.Add(sr.ReadBytes(entry.fileOffset, entry.fileSize));
            }
        }

        public unsafe byte[] GetBytes()
        {
            ByteListExtension.AddAsBigEndian = true;
            List<byte> outBytes = new List<byte>();
            outBytes.AddValue(1);
            outBytes.ReserveInt("totalBufferSize");
            outBytes.ReserveInt("totalBufferDifferenceFromCompressed");
            outBytes.ReserveInt("uncompressedDataSize");

            outBytes.ReserveInt("compressedDataSize");
            outBytes.AddValue(0);
            outBytes.AddValue(0);
            outBytes.AddValue(0);

            List<byte> innerBytes = NRCGetBytes();
            var compressedBytes = PRSHelpers.PRSCompress(innerBytes.ToArray());
            outBytes.AddRange(compressedBytes);
            outBytes.FillInt("uncompressedDataSize", innerBytes.Count);
            outBytes.FillInt("compressedDataSize", compressedBytes.Length);
            var bufferAddition = 0x20 - (innerBytes.Count - compressedBytes.Length) % 0x20;
            //Hack to ensure the buffersize is enough
            bufferAddition += 0x40;
            var compressionDifference = innerBytes.Count - compressedBytes.Length;
            var finalDifference = compressionDifference + bufferAddition;
            var finalTotalBufferSize = finalDifference + compressedBytes.Length;
            outBytes.FillInt("totalBufferSize", finalTotalBufferSize);
            outBytes.FillInt("totalBufferDifferenceFromCompressed", finalDifference);

            return outBytes.ToArray();
        }

        public List<byte> NRCGetBytes()
        {
            ByteListExtension.AddAsBigEndian = true;
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

            innerBytes.Add(0x1);
            innerBytes.Add(0x0);
            innerBytes.Add(0x0);
            innerBytes.Add(0x0);
            innerBytes.AddValue(0);
            innerBytes.AddValue(fileNames.Count + 1);

            for (int i = 0; i < fileNames.Count; i++)
            {
                innerBytes.ReserveInt($"nameOffset{i}");
                innerBytes.ReserveInt($"fileOffset{i}");
                innerBytes.AddValue(files[i].Length);
            }
            var namesStart = innerBytes.Count;
            innerBytes.Add(0);
            for (int i = 0; i < fileNames.Count; i++)
            {
                innerBytes.FillInt($"nameOffset{i}", innerBytes.Count - namesStart);
                innerBytes.AddRange(Encoding.UTF8.GetBytes(fileNames[i]));
                innerBytes.Add(0);
            }
            innerBytes.FillInt("fileEntriesSize", innerBytes.Count - 0x20);
            innerBytes.AlignWriter(0x20);
            innerBytes.FillInt("fullSize", innerBytes.Count);

            for (int i = 0; i < files.Count; i++)
            {
                innerBytes.FillInt($"fileOffset{i}", innerBytes.Count);
                innerBytes.AddRange(files[i]);
                if (i != files.Count - 1)
                {
                    innerBytes.AlignWriter(0x20);
                }
            }

            return innerBytes;
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
