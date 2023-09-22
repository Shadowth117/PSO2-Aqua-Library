using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Morpheme.MorphemeBundle
{
    /// <summary>
    /// Substruct from FileNameLookupTable.
    /// </summary>
    public class LookupTable
    {
        /// <summary>
        /// Number of indices and localOffsets.
        /// </summary>
        public int elemCount;

        /// <summary>
        /// Number of bytes at the string offset.
        /// </summary>
        public int stringSize;

        /// <summary>
        /// Indices list.
        /// </summary>
        public List<int> idxList = new List<int>(); 

        /// <summary>
        /// Local offsets.
        /// </summary>
        public List<int> localOffsets = new List<int>();

        /// <summary>
        /// Strings as bytes.
        /// </summary>
        public List<byte> strings = new List<byte>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public LookupTable() { }

        /// <summary>
        /// Constructsor to read a LookupTable.
        /// </summary>
        /// <param name="br">A BinaryReaderEx to read this strcture from.</param>
        public LookupTable(BinaryReaderEx br) 
        {
            Read(br);
        }

        /// <summary>
        /// Calc the size of the structure.
        /// </summary>
        /// <param name="isX64"></param>
        public int CalculateBundleSize(bool isX64)
        {
            return 8 + (isX64 ? 0x18 : 0xC) + idxList.Count * 4 + localOffsets.Count * 4 + strings.Count;
        }

        /// <summary>
        /// Reads the LookupTable.
        /// </summary>
        /// <param name="br">A BinaryReaderEx to read this strcture from.</param>
        public void Read(BinaryReaderEx br)
        {
            var dataStart = br.Position;
            elemCount = br.ReadInt32();
            stringSize = br.ReadInt32();
            var idxOffset = br.ReadVarint();
            var localOffsetOffset = br.ReadVarint();
            var stringsOffset = br.ReadVarint();

            br.Position = dataStart + idxOffset;
            for (int i = 0; i < elemCount; i++)
            {
                idxList.Add(br.ReadInt32());
            }

            br.Position = dataStart + localOffsetOffset;
            for (int i = 0; i < elemCount; i++)
            {
                localOffsets.Add(br.ReadInt32());
            }

            br.Position = dataStart + stringsOffset;
            for (int i = 0; i < stringSize; i++)
            {
                strings.Add(br.ReadByte());
            }
        }

        /// <summary>
        /// Writes the LookupTable.
        /// </summary>
        /// <param name="bw">A BinaryWriterEx to write this structure from.</param>
        public void Write(BinaryWriterEx bw)
        {
            var dataStart = bw.Position;
            bw.WriteInt32(idxList.Count);
            bw.WriteInt32(strings.Count);
            bw.ReserveVarint("LTIdxList");
            bw.ReserveVarint("LTLocalOffsets");
            bw.ReserveVarint("LTStrings");

            bw.FillVarint("LTIdxList", bw.Position - dataStart);
            for(int i = 0; i < idxList.Count; i++)
            {
                bw.WriteInt32(idxList[i]);
            }

            bw.FillVarint("LTLocalOffsets", bw.Position - dataStart);
            for (int i = 0; i < localOffsets.Count; i++)
            {
                bw.WriteInt32(localOffsets[i]);
            }

            bw.FillVarint("LTStrings", bw.Position - dataStart);
            for (int i = 0; i < strings.Count; i++)
            {
                bw.WriteByte(strings[i]);
            }
        }
    }
}
