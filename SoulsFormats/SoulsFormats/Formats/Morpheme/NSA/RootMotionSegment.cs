using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Morpheme.NSA
{
    public class RootMotionSegment
    {
        public float fps;
        public long sampleCount;
        DequantizationFactor dequantizationFactor;
        Quaternion rotation;
        List<TranslationSample> translationSamples = new List<TranslationSample>();
        List<RotationSample> rotationSamples = new List<RotationSample>();

        public RootMotionSegment() { }

        public RootMotionSegment(BinaryReaderEx br)
        {
            var dataStart = br.Position;
            br.Position += 0x20;
            fps = br.ReadSingle(); 
            sampleCount = br.ReadInt32();
            dequantizationFactor = new DequantizationFactor(br);
            rotation = new Quaternion(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

            br.Position += 0x8;

            var pTranslationSample = br.ReadVarint();
            var pRotationSample = br.ReadVarint();

            if(pTranslationSample != 0)
            {
                br.Position += dataStart + pTranslationSample;

                for(int i = 0; i < sampleCount; i++)
                {
                    translationSamples.Add(new TranslationSample(br));
                }
            }

            if(pRotationSample != 0)
            {
                br.Position += dataStart + pRotationSample;

                for (int i = 0; i < sampleCount; i++)
                {
                    rotationSamples.Add(new RotationSample(br));
                }
            }
        }
    }
}
