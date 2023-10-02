using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Morpheme.NSA
{
    public class DynamicSegment
    {
        public int sampleCount;
        public int translationBoneCount;
        public int rotationBoneCount;
        public List<TranslationSample> translationSamples = new List<TranslationSample>();
        public List<DequantizationInfo> translationDequantizationInfo = new List<DequantizationInfo>();
        public List<RotationSample> rotationSamples = new List<RotationSample>();
        public List<DequantizationInfo> rotationDequantizationInfo = new List<DequantizationInfo>();

        //Dequantized Lists
        public List<Vector3> translationFrames = new List<Vector3>();
        public List<Quaternion> rotationFrames = new List<Quaternion>();

        public DynamicSegment() { }

        public DynamicSegment(BinaryReaderEx br) 
        {
            var dataStart = br.Position;
            sampleCount = br.ReadInt32();
            translationBoneCount = br.ReadInt32();
            rotationBoneCount = br.ReadInt32();

            var dataCurrent = br.Position;
            br.Position = dataCurrent + 4;

            var pTranslationSample = br.ReadVarint();
            var pTranslationDeqInfo = br.ReadVarint();
            var pRotationSample = br.ReadVarint();
            var pRotationDeqInfo = br.ReadVarint();

            var translDeqCount = GetDeqInfoCount(translationBoneCount);
            var rotDeqCount = GetDeqInfoCount(rotationBoneCount);

            br.Position = dataStart + pTranslationSample;
            for(int i = 0; i < sampleCount * translationBoneCount; i++)
            {
                translationSamples.Add(new TranslationSample(br));
            }

            br.Position = dataStart + pTranslationDeqInfo;
            for (int i = 0; i < translDeqCount; i++)
            {
                translationDequantizationInfo.Add(new DequantizationInfo(br));
            }

            br.Position = dataStart + pRotationSample;
            for (int i = 0; i < sampleCount * rotationBoneCount; i++)
            {
                rotationSamples.Add(new RotationSample(br));
            }

            br.Position = dataStart + pRotationDeqInfo;
            for (int i = 0; i < rotDeqCount; i++)
            {
                rotationDequantizationInfo.Add(new DequantizationInfo(br));
            }
        }

        public int GetDeqInfoCount(int bone_count)
        {
            int multiple = bone_count / 4;

            if (bone_count % 4 != 0)
                multiple = multiple + 1;

            return multiple * 4;
        }
    }
}
