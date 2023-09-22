using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Morpheme.MorphemeBundle
{
    /// <summary>
    /// Header for Morpheme files.
    /// </summary>
    public class MorphemeFileHeader : MorphemeBundle_Base
    {
        /// <summary>
        /// Header variable 0
        /// </summary>
        public long iVar0 { get; set; }

        /// <summary>
        /// Header variable 1
        /// </summary>
        public long iVar1 { get; set; }

        /// <summary>
        /// Header variable 2
        /// </summary>
        public long iVar2 { get; set; }

        /// <summary>
        /// Header variable 3
        /// </summary>
        public long iVar3 { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MorphemeFileHeader() { }

        /// <summary>
        /// Constructor for reading a MorphemeBundle
        /// </summary>
        /// <param name="br"></param>
        public MorphemeFileHeader(BinaryReaderEx br)
        {
            Read(br);
        }

        /// <summary>
        /// Method for getting size of this MorphemeBundle
        /// </summary>
        public override long CalculateBundleSize()
        {
            return isX64 ? 0x20 : 0x10;
        }

        /// <summary>
        /// Method for reading a MorphemeBundle
        /// </summary>
        public override void Read(BinaryReaderEx br)
        {
            base.Read(br);
            iVar0 = br.ReadVarint();
            iVar1 = br.ReadVarint();
            iVar2 = br.ReadVarint();
            iVar3 = br.ReadVarint();
        }

        /// <summary>
        /// Method for writing a MorphemeBundle
        /// </summary>
        public override void Write(BinaryWriterEx bw)
        {
            base.Write(bw);
            bw.WriteVarint(iVar0);
            bw.WriteVarint(iVar1);
            bw.WriteVarint(iVar2);
            bw.WriteVarint(iVar3);
        }
    }
}
