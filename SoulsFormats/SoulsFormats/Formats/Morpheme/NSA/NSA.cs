using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Morpheme.NSA
{
    public class NSA
    {
        public NSAHeader header;
        public IndexList staticTranslationIndices;
        public IndexList staticRotationIndices;
        public IndexList dynamicTranslationIndicies;
        public IndexList dynamicRotationIndicies;
        public List<DequantizationFactor> translationAnimDequantizationFactors = new List<DequantizationFactor>();
        public List<DequantizationFactor> rotationAnimDequantizationFactors = new List<DequantizationFactor>();
        public StaticSegment staticSegment;
        public DynamicSegment dynamicSegment;
        public RootMotionSegment rootMotionSegment;

        public List<AnimFrame> keyframes = new List<AnimFrame>();

        public NSA() { }
        public NSA(BinaryReaderEx br)
        {
            header = new NSAHeader(br);

            br.Position = header.pStaticTranslationBoneIndices;
            staticTranslationIndices = new IndexList(br);

            br.Position = header.pStaticRotationBoneIndices;
            staticRotationIndices = new IndexList(br);

            br.Position = header.ppDynamicTranslationBoneIndices;
            br.Position = br.ReadVarint();
            dynamicTranslationIndicies = new IndexList(br);

            br.Position = header.ppDynamicRotationBoneIndices;
            br.Position = br.ReadVarint();
            dynamicRotationIndicies = new IndexList(br);

            br.Position = header.pTranslationDequantizationFactors;
            for(int i = 0; i < header.translationDequantizationCount; i++)
            {
                translationAnimDequantizationFactors.Add(new DequantizationFactor(br));
            }

            br.Position = header.pRotationDequantizationFactors;
            for (int i = 0; i < header.rotationDequantizationCount; i++)
            {
                rotationAnimDequantizationFactors.Add(new DequantizationFactor(br));
            }

            br.Position = header.pStaticSegment;
            staticSegment = new StaticSegment(br);

            br.Position = header.pDynamicSegment;
            dynamicSegment = new DynamicSegment(br);

            br.Position = header.pRootMotionSegment;
            rootMotionSegment = new RootMotionSegment(br);
        }
    }
}
