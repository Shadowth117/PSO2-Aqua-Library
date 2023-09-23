using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Morpheme.NSA
{
    public class TranslationSample
    {
        public ulong sample;
        public int x;
        public int y;
        public int z;


        public TranslationSample() { }

        public TranslationSample(BinaryReaderEx br)
        {
            sample = br.ReadUInt64();
            x = ExtractBits((int)sample, 0, 11);
            y = ExtractBits((int)sample, 11, 11);
            z = ExtractBits((int)sample, 22, 10);
        }

        public int ExtractBits(int value, int startBit, int numBits)
        {
            uint mask = ((1u << numBits) - 1u) << startBit;

            uint extractedBits = (uint)((value & mask) >> startBit);

            return (int)extractedBits;
        }
    }
}
