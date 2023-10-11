using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats
{
    /// <summary>
    /// BND file
    /// </summary>
    public class BND : SoulsFile<BND>, IBinder
    {
        /// <summary>
        /// BND version, seemingly.
        /// </summary>
        public int internalVersion;

        /// <summary>
        /// The first BND format definition
        /// </summary>
        public ushort format0;

        /// <summary>
        /// The second BND format definition.
        /// Bit 0 determines if filenames exist
        /// Bit 1 determines if the rootFilePath exists
        /// </summary>
        public ushort format1;

        /// <summary>
        /// Format for BND3+. Format is entirely different in BND.
        /// </summary>
        public Binder.Format Format { get => 0; set => Format = 0; }

        /// <summary>
        /// Interally, the version is an int for BND
        /// </summary>
        public string Version { get => internalVersion.ToString(); set => internalVersion = Int32.Parse(value); }

        /// <summary>
        /// The files in the BND
        /// </summary>
        public List<BinderFile> Files { get; set; }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 4)
                return false;

            string magic = br.GetASCII(0, 4);
            return magic == "BND\0";
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            List<BinderFileHeader> fileHeaders = ReadHeader(this, br);
            Files = new List<BinderFile>(fileHeaders.Count);
            foreach (BinderFileHeader fileHeader in fileHeaders)
                Files.Add(fileHeader.ReadFileData(br));
        }

        /// <summary>
        /// Reads the BND header
        /// </summary>
        /// <param name="bnd"></param>
        /// <param name="br"></param>
        /// <returns>BinderFileHeaders which contain everything needed to grab a file from the BinaryReaderEx</returns>
        internal static List<BinderFileHeader> ReadHeader(BND bnd, BinaryReaderEx br)
        {
            br.AssertASCII("BND\0");
            br.AssertUInt16(0xFFFF);
            br.AssertUInt16(0);
            bnd.internalVersion = br.ReadUInt16();
            br.ReadUInt32(); //Filesize
            var fileCount = br.ReadInt32();
            var rootFilePathOffset = br.ReadInt32(); //This will be 0 if there is no rootFilePath
            string rootFilePath = "";
            if(rootFilePathOffset != 0)
            {
                br.StepIn(rootFilePathOffset);
                rootFilePath = br.ReadASCII();
                br.StepOut();
            }

            bnd.format0 = br.ReadUInt16(); //Unknown
            bnd.format1 = br.ReadUInt16();
            br.AssertInt32(0);

            var fileHeaders = new List<BinderFileHeader>(fileCount);
            for (int i = 0; i < fileCount; i++)
            {
                fileHeaders.Add(ReadBinderFileHeader(br, rootFilePath));
            }

            return fileHeaders;
        }

        /// <summary>
        /// Reads a BND file header
        /// </summary>
        /// <param name="br"></param>
        /// <param name="rootFilePath"></param>
        /// <returns>BinderFileHeader which contains everything needed to grab a file from the BinaryReaderEx</returns>
        internal static BinderFileHeader ReadBinderFileHeader(BinaryReaderEx br, string rootFilePath)
        {
            var rootFilePathOffsetClean = rootFilePath.Substring(0, rootFilePath.Length - 1);
            var id = br.ReadInt32();
            var dataOffset = br.ReadUInt32();
            var fileSize = br.ReadUInt32();
            var nameOffset = br.ReadUInt32();

            string name = $"File_{id}";
            if(nameOffset != 0)
            {
                br.StepIn(nameOffset);
                name = rootFilePathOffsetClean + br.ReadASCII();
                br.StepOut();
            } 

            return new BinderFileHeader(id, name) { DataOffset = dataOffset, UncompressedSize = fileSize };
        }
    }
}
