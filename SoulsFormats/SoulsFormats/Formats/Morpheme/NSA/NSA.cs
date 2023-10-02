using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
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
            staticSegment = header.pStaticSegment > 0 ? new StaticSegment(br) : new StaticSegment();

            br.Position = header.pDynamicSegment;
            dynamicSegment = header.pDynamicSegment > 0 ? new DynamicSegment(br) : new DynamicSegment();

            br.Position = header.pRootMotionSegment;
            rootMotionSegment = header.pRootMotionSegment > 0 ? new RootMotionSegment(br) : new RootMotionSegment();
        }

        /// <summary>
        /// Decompresses the animation data back to something we can use. Details can be found here: https://github.com/ryanjsims/warpgate/blob/75c60875cd61aa275672b741e9ef472c4bb5b309/doc/formats/README.md?plain=1#L24
        /// </summary>
        public void DequantizeAnimation()
        {
            DequantizeRootSegment();        //Anim data for the root bone.
            DequantizeStaticSegment();      //Anim data for transforms that don't change during the animation. Does not include root bone.
            DequantizeDynamicSegment();     //Anim data for transforms that change during the animation. Does not include root bone.
        }

        public void DequantizeRootSegment()
        {

        }

        public void DequantizeStaticSegment()
        {
            if(header.pStaticSegment != 0)
            {

            }
        }

        public void DequantizeDynamicSegment()
        {
            if(header.pDynamicSegment != 0)
            {

            }
        }

        /// <summary>
        /// Dequantizes translation values for use.
        /// </summary>
        public Vector3 UnpackTranslation(NSAVec3 nsaVec3, DequantizationInfo deqInfo, List<DequantizationFactor> deqFactors, DequantizationFactor initFactor)
        {
            float xQuantFactor, xQuantMin, yQuantFactor, yQuantMin, zQuantFactor, zQuantMin;
            RetrieveFactorData(deqInfo, deqFactors, out xQuantFactor, out xQuantMin, out yQuantFactor, out yQuantMin, out zQuantFactor, out zQuantMin);

            var initX = initFactor.scaledExtent.X * deqInfo.init[0] + initFactor.min.X;
            var initY = initFactor.scaledExtent.Y * deqInfo.init[1] + initFactor.min.Y;
            var initZ = initFactor.scaledExtent.Z * deqInfo.init[2] + initFactor.min.Z;

            return new Vector3(xQuantFactor * nsaVec3.X + xQuantMin + initX,
                               yQuantFactor * nsaVec3.Y + yQuantMin + initY,
                               zQuantFactor * nsaVec3.Z + zQuantMin + initZ);
        }

        private static void RetrieveFactorData(DequantizationInfo deqInfo, List<DequantizationFactor> deqFactors, out float xQuantFactor, out float xQuantMin, out float yQuantFactor, out float yQuantMin, out float zQuantFactor, out float zQuantMin)
        {
            var xFactors = deqFactors[deqInfo.factorIdx[0]];
            xQuantFactor = xFactors.scaledExtent.X;
            xQuantMin = xFactors.min.X;
            var yFactors = deqFactors[deqInfo.factorIdx[1]];
            yQuantFactor = yFactors.scaledExtent.Y;
            yQuantMin = yFactors.min.Y;
            var zFactors = deqFactors[deqInfo.factorIdx[2]];
            zQuantFactor = zFactors.scaledExtent.Z;
            zQuantMin = zFactors.min.Z;
        }

        /// <summary>
        /// Dequantizes rotation values for use. Note, if too lossy feeling, do calcs in doubles and convert to float at end. Game probably just uses floats the whole time, but who knows.
        /// </summary>
        public Quaternion UnpackQuaternion(NSAVec3 nsaVec3, DequantizationInfo deqInfo, List<DequantizationFactor> deqFactors)
        {
            //Quat components are stored as 3 values, then get the 4th generated from the other 3.
            float xQuantFactor, xQuantMin, yQuantFactor, yQuantMin, zQuantFactor, zQuantMin;
            RetrieveFactorData(deqInfo, deqFactors, out xQuantFactor, out xQuantMin, out yQuantFactor, out yQuantMin, out zQuantFactor, out zQuantMin);

            var initialRotationVector = new List<float>();
            foreach (var init in deqInfo.init)
            {
                initialRotationVector.Add(init / 128.0f - 1);
            }

            float squareMagnitude = 0;
            foreach (var value in initialRotationVector)
            {
                squareMagnitude += value * value;
            }
            float scalar = 2 / (squareMagnitude + 1);

            var initialQuat = new Quaternion((scalar * initialRotationVector[0]), (scalar * initialRotationVector[1]), (scalar * initialRotationVector[2]), ((1 - squareMagnitude) / (1 + squareMagnitude)));

            Vector3 deqRotationVector = new Vector3(nsaVec3.X * xQuantFactor - xQuantMin, nsaVec3.Y * yQuantFactor - yQuantMin, nsaVec3.Z * zQuantFactor - zQuantMin);
            squareMagnitude = deqRotationVector.X * deqRotationVector.X + deqRotationVector.Y * deqRotationVector.Y + deqRotationVector.Z * deqRotationVector.Z;
            scalar = 2 / (squareMagnitude + 1);

            var deqQuaternion = new Quaternion(scalar * deqRotationVector.X, scalar * deqRotationVector.Y, scalar * deqRotationVector.Z, (1 - squareMagnitude) / (1 + squareMagnitude));

            return initialQuat * deqQuaternion;
        }

        /// <summary>
        /// A check for 64 bit values and the file's endianness. BinaryReader is adjusted as needed for this.
        /// </summary>
        public static void Set64BitAndEndianness(BinaryReaderEx br)
        {
            br.VarintLong = true;
            br.StepIn(0x8);

            //This should be reliable since there's no Big Endian platform that's 64 bit which DS2 is on.
            var endTest = br.ReadUInt32();
            br.BigEndian = true;
            br.Position -= 4;
            var endTest2 = br.ReadUInt32();
            if (endTest < endTest2)
            {
                br.BigEndian = false;
            }

            var test = br.ReadUInt32();
            if (test > 0)
            {
                br.VarintLong = false;
            }
            br.StepOut();
        }
    }
}
