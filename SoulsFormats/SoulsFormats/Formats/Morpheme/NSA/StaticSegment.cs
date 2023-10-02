using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Morpheme.NSA
{
    public class StaticSegment
    {
        public int translationBoneCount;
        public int rotationBoneCount;
        public DequantizationFactor translationBoneDequantizationFactor;
        public DequantizationFactor rotationBoneDequantizationFactor;
        public List<NSAVec3> compressedTranslations = new List<NSAVec3>();
        public List<NSAVec3> compressedRotations = new List<NSAVec3>();

        //Dequantized Lists
        public List<Vector3> translationFrames = new List<Vector3>();
        public List<Quaternion> rotationFrames = new List<Quaternion>();

        public StaticSegment() { }

        public StaticSegment(BinaryReaderEx br)
        {
            var dataStart = br.Position;
            translationBoneCount = br.ReadInt32();
            rotationBoneCount = br.ReadInt32();
            translationBoneDequantizationFactor = new DequantizationFactor(br);
            rotationBoneDequantizationFactor = new DequantizationFactor(br);

            var compressedTranslationOffset = br.ReadVarint();
            var compressedRotationOffset = br.ReadVarint();

            br.StepIn(dataStart + compressedTranslationOffset);
            for(int i = 0; i < translationBoneCount; i++)
            {
                compressedTranslations.Add(new NSAVec3(br));
            }
            br.StepOut();
            br.StepIn(dataStart + compressedRotationOffset);
            for (int i = 0; i < rotationBoneCount; i++)
            {
                compressedRotations.Add(new NSAVec3(br));
            }
            br.StepOut();
        }
    }
}
