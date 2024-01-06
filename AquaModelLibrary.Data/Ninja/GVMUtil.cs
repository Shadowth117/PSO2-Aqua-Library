using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.Ninja
{
    public class GVMUtil
    {
        [Flags]
        internal enum GvmFlags : ushort
        {
            /// <summary>
            /// Specifies global indexes are provided.
            /// </summary>
            GlobalIndexes = (1 << 0),

            /// <summary>
            /// Specifies texture dimensions are provided within the entry table.
            /// </summary>
            Dimensions = (1 << 1),

            /// <summary>
            /// Specifies pixel and data formats are provided within the entry table.
            /// </summary>
            Formats = (1 << 2),

            /// <summary>
            /// Specifies filenames are present within the entry table.
            /// </summary>
            Filenames = (1 << 3),
        }

        /// <summary>
        /// GVM unfortunately does not give a full filesize and so when it's embedded within other files, we need to seek through it to get everything.
        /// GVR is a bit of an involved format as well so it's best to extract and leave the rest to puyo tools, frankly.
        /// </summary>
        public static byte[] ReadGVMBytes(BufferedStreamReaderBE<MemoryStream> sr, bool isArcGVM = false)
        {
            List<byte> gvmBytes = new List<byte>();
            var magic = sr.Read<int>();
            var gvmFirstEntryOffset = sr.Read<int>();
            var flags = sr.ReadBE<ushort>();
            var entryCount = sr.ReadBE<ushort>();
            gvmBytes.AddRange(sr.ReadBytes(sr.Position - 0xC, gvmFirstEntryOffset + 0x8));
            var gvrt0Offset = sr.Position + gvmFirstEntryOffset - 4;
            sr.Seek(gvrt0Offset, System.IO.SeekOrigin.Begin);

            for (int i = 0; i < entryCount; i++)
            {
                var currentFileOffset = sr.Position;
                sr.Seek(4, System.IO.SeekOrigin.Current);
                var fileSize = sr.Read<int>(); //little endian
                if (isArcGVM == false && i == entryCount - 1 && sr.BaseStream.Length > fileSize + sr.Position + 9)
                {
                    fileSize += 0x10;
                }
                else if (isArcGVM && i == entryCount - 1)
                {
                    fileSize -= 0x10;
                }
                gvmBytes.AddRange(sr.ReadBytes(currentFileOffset, 8 + fileSize));
                gvmBytes.AlignWriter(0x10);
                sr.Seek(fileSize, System.IO.SeekOrigin.Current);
            }

            return gvmBytes.ToArray();
        }


        public static List<string> ReadGVMFileNames(BufferedStreamReaderBE<MemoryStream> sr)
        {
            sr._BEReadActive = true;
            var magic = sr.Read<int>();
            var gvmFirstEntryOffset = sr.Read<int>();
            GvmFlags properties = (GvmFlags)sr.ReadBE<ushort>();
            var _hasFilenames = (properties & GvmFlags.Filenames) != 0;
            var _hasFormats = (properties & GvmFlags.Formats) != 0;
            var _hasDimensions = (properties & GvmFlags.Dimensions) != 0;
            var _hasGlobalIndexes = (properties & GvmFlags.GlobalIndexes) != 0;

            // Calculate the size of each entry in the entry table.
            var _tableEntryLength = 2;
            int _globalIndexOffset = -1;
            if (_hasFilenames)
            {
                _tableEntryLength += 28;
            }
            if (_hasFormats)
            {
                _tableEntryLength += 2;
            }
            if (_hasDimensions)
            {
                _tableEntryLength += 2;
            }
            if (_hasGlobalIndexes)
            {
                _globalIndexOffset = _tableEntryLength;
                _tableEntryLength += 4;
            }

            var entryCount = sr.ReadBE<ushort>();
            List<string> gvmNames = new List<string>();
            // Loop through all of the entries.
            for (int i = 0; i < entryCount; i++)
            {
                var pos = sr.Position;
                var id = sr.ReadBE<ushort>();
                if (_hasFilenames)
                {
                    gvmNames.Add(sr.ReadCString(0x1C));
                    sr.Seek(0x1C, System.IO.SeekOrigin.Current);
                }
                else
                {
                    gvmNames.Add($"tex_{i}");
                }
                if (_hasFormats)
                {
                    var format0 = sr.Read<byte>();
                    var format1 = sr.Read<byte>();
                }
                if (_hasDimensions)
                {
                    var dim0 = sr.Read<byte>();
                    var dim1 = sr.Read<byte>();
                }
                if (_hasGlobalIndexes)
                {
                    var globalIndex = sr.ReadBE<int>();
                }
            }

            sr._BEReadActive = false;
            return gvmNames;
        }
    }
}
