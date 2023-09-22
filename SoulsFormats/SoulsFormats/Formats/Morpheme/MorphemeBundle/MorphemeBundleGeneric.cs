using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Morpheme.MorphemeBundle
{
    /// <summary>
    /// MorphemeBundle format used in Morpheme files in Dark Souls 2.
    /// </summary>
    public class MorphemeBundle : MorphemeBundle_Base
    {
        /// <summary>
        /// Raw data
        /// </summary>
        public byte[] data { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MorphemeBundle() { }

        /// <summary>
        /// Constructor for reading a MorphemeBundle
        /// </summary>
        /// <param name="br"></param>
        public MorphemeBundle(BinaryReaderEx br)
        {
            Read(br);
        }

        /// <summary>
        /// Method for reading a MorphemeBundle
        /// </summary>
        public override void Read(BinaryReaderEx br)
        {
            base.Read(br);
            data = br.ReadBytes((int)dataSize);
        }

        /// <summary>
        /// Method for writing a MorphemeBundle
        /// </summary>
        public override void Write(BinaryWriterEx bw)
        {
            base.Write(bw);
            bw.WriteBytes(data);
        }

        /// <summary>
        /// Method for getting size of this MorphemeBundle
        /// </summary>
        public override ulong CalculateBundleSize()
        {
            return dataSize;
        }
    }
}
