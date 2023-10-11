using System.Collections.Generic;
using System.IO;

namespace SoulsFormats.Formats
{
    public class DEV : SoulsFile<DEV>
    {
        public List<ArchiveHeader> archiveHeaders = new List<ArchiveHeader>();
        public List<List<FileHeader>> fileHeaderSets = new List<List<FileHeader>>();
        public List<List<byte[]>> fileSets = new List<List<byte[]>>();

        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 4)
                return false;

            string magic = br.GetASCII(0, 4);
            return magic == "DEV\0";
        }

        protected override void Read(BinaryReaderEx br)
        {
            br.AssertASCII("DEV\0");
            br.AssertUInt16(0xFFFF);
            br.AssertUInt16(0);
            br.AssertUInt32(0x3F947AE1);
            br.ReadInt32(); //File size

            br.ReadInt32(); //Data size, file size above -0x20
            var archiveFileCount = br.ReadInt32(); //Archive file count (Should only be 1 or 2 for Metal Wolf Chaos)
            br.AssertUInt32(0);
            br.AssertInt16(0x10);
            br.AssertInt16(0x5);

            for(int i = 0; i < archiveFileCount; i++)
            {
                ArchiveHeader archiveHeader = new ArchiveHeader();
                archiveHeader.id = br.ReadUInt16();
                archiveHeader.unk0 = br.ReadUInt16();
                archiveHeader.fileCount = br.ReadUInt32();
                archiveHeader.archiveSize = br.ReadUInt32();
                archiveHeader.fileHeadersOffset = br.ReadUInt32();

                archiveHeaders.Add(archiveHeader);
            }

            foreach(var archiveHeader in archiveHeaders)
            {
                var fileHeaderSet = new List<FileHeader>();
                for(int i = 0; i < archiveHeader.fileCount; i++)
                {
                    FileHeader fileHeader = new FileHeader();
                    fileHeader.id = br.ReadUInt16();
                    fileHeader.archiveId = br.ReadUInt16();
                    fileHeader.dataOffset = br.ReadUInt32();
                    fileHeader.dataSize = br.ReadUInt32();
                    fileHeader.nameOffset = br.ReadUInt32();

                    br.StepIn(fileHeader.nameOffset);
                    fileHeader.fileName = br.ReadASCII();
                    br.StepOut();

                    fileHeaderSet.Add(fileHeader);
                }
                fileHeaderSets.Add(fileHeaderSet);
            }
        }

        public void ReadData(string filePath)
        {
            for(int i = 0; i < fileHeaderSets.Count; i++)
            {
                var fileHeaderSet = fileHeaderSets[i];
                var dataPath = filePath.Replace("_header.dev", $"_data.00{i}");
                BinaryReaderEx br = new BinaryReaderEx(false, File.ReadAllBytes(dataPath));
                var fileSet = new List<byte[]>();

                for(int j = 0; j < fileHeaderSet.Count; j++)
                {
                    var fileHeader = fileHeaderSet[j];
                    br.Position = fileHeader.dataOffset;
                    fileSet.Add(br.ReadBytes((int)fileHeader.dataSize));
                }

                fileSets.Add(fileSet);
            }
        }

        public class ArchiveHeader
        {
            public ushort id;
            public ushort unk0; 
            public uint fileCount; //Offset in this DEV file where file headers start
            public uint archiveSize;       //Size of actual archive file
            public uint fileHeadersOffset;
        }

        public class FileHeader
        {
            public ushort id;
            public ushort archiveId; //Some DEVs reference multiple archives. This denotes the archive id
            public uint dataOffset;
            public uint dataSize;
            public uint nameOffset;

            public string fileName;
        }

    }
}
