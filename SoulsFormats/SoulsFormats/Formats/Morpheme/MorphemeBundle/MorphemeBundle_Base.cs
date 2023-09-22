using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Morpheme.MorphemeBundle
{    
     /// <summary>
     /// Interface for MorphemeBundle format used in Morpheme files in Dark Souls 2.
     /// </summary>
    public abstract class MorphemeBundle_Base
    {
        /// <summary>
        /// Enumerator used to determine what kind of data the packet holds
        /// </summary>
        public eBundleType bundleType { get; set; }

        /// <summary>
        /// Signature that can be used by a packet to reference another one.
        /// </summary>
        public uint signature { get; set; }

        /// <summary>
        /// Morpheme Bundle Header. Usually all 0s.
        /// </summary>
        public MorphemeBundleHeader header { get; set; }

        /// <summary>
        /// Size of this bundle structure
        /// </summary>
        public ulong dataSize { get; set; }

        /// <summary>
        /// Alignment for this bundle structure. Only observed 0x4 and 0x10.
        /// </summary>
        public ulong dataAlignment { get; set; }

        /// <summary>
        /// Method for verifying data is a MorphemeBundle
        /// </summary>
        public bool Is(BinaryReaderEx br)
        {
            if (br.Length < 0xC)
                return false;

            var tempMagic0 = br.ReadInt32();
            var tempMagic1 = br.ReadInt32();

            return (tempMagic0 == 0x18) && (tempMagic1 == 0xA);
        }

        /// <summary>
        /// Method for reading a MorphemeBundle
        /// </summary>
        public virtual void Read(BinaryReaderEx br)
        {
            br.AssertInt32(0x18);
            br.AssertInt32(0xA);
            bundleType = br.ReadEnum32<eBundleType>();
            signature = br.ReadUInt32();
            header = new MorphemeBundleHeader
            {
                int_00 = br.ReadInt32(),
                int_04 = br.ReadInt32(),
                int_08 = br.ReadInt32(),
                int_0C = br.ReadInt32()
            };
            dataSize = br.ReadUInt64();
            dataAlignment = br.ReadUInt64();
            br.Pad((int)dataAlignment);
        }

        /// <summary>
        /// Method for writing a MorphemeBundle
        /// </summary>
        public virtual void Write(BinaryWriterEx bw)
        {
            bw.WriteUInt32(0x18);
            bw.WriteUInt32(0xA);
            bw.WriteInt32((int)bundleType);
            bw.WriteUInt32(signature);
            bw.WriteInt32(header.int_00);
            bw.WriteInt32(header.int_04);
            bw.WriteInt32(header.int_08);
            bw.WriteInt32(header.int_0C);
            bw.WriteUInt64(dataSize);
            bw.WriteUInt64((uint)CalculateBundleSize());
            bw.Pad((int)dataAlignment);
        }

        /// <summary>
        /// Method for getting size of this MorphemeBundle
        /// </summary>
        public abstract ulong CalculateBundleSize();
    }
}
