using AquaModelLibrary.Data.DataTypes.SetLengthStrings;
using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaMotionData;
using AquaModelLibrary.Helpers.MathHelpers;
using SoulsFormats;
using SoulsFormats.Formats.Morpheme.NSA;
using System.Numerics;

namespace AquaModelLibrary.Core.Morpheme
{
    public class NSAConvert
    {
        public static AquaMotion GetAquaMotionFromNSA(NSA nsa, IFlver flv)
        {
            GetNSAKeyframes(nsa, flv, out var translationKeyFrameListList, out var rotationkeyFrameListList);

            AquaMotion aqm = new AquaMotion();
            aqm.moHeader = new MOHeader();
            aqm.moHeader.frameSpeed = nsa.header.fps;
            aqm.moHeader.endFrame = (int)(nsa.rootMotionSegment.sampleCount - 1);
            aqm.moHeader.unkInt0 = 2;
            aqm.moHeader.variant = 0x2;
            aqm.moHeader.testString.SetString("test");

            for (int i = 0; i < translationKeyFrameListList.Count; i++)
            {
                var keySet = new KeyData();
                keySet.mseg.nodeId = i;
                keySet.mseg.nodeType = 2;
                keySet.mseg.nodeName = new PSO2String(flv.Bones[i].Name);

                var pos = new MKEY();
                var rot = new MKEY();

                //Position
                var frameSet = translationKeyFrameListList[i];
                pos.dataType = 1;
                pos.keyCount = frameSet.Count;
                pos.keyType = 1;
                pos.vector4Keys = frameSet.ConvertAll(frame => new Vector4(frame.X, frame.Y, frame.Z, 0));

                //Position Timings
                for (int f = 0; f < pos.vector4Keys.Count; f++)
                {
                    int flag = GetFrameTimeFlag(pos, f);
                    pos.frameTimings.Add((uint)((f * 0x10) + flag));
                }

                //Rotation
                var rotFrameSet = rotationkeyFrameListList[i];
                rot.dataType = 3;
                rot.keyCount = rotFrameSet.Count;
                rot.keyType = 2;
                rot.vector4Keys = rotFrameSet;

                //Rotation Timings
                for (int f = 0; f < rot.vector4Keys.Count; f++)
                {
                    int flag = GetFrameTimeFlag(rot, f);
                    rot.frameTimings.Add((uint)((f * 0x10) + flag));
                }

                keySet.keyData.Add(pos);
                keySet.keyData.Add(rot);
                keySet.mseg.nodeDataCount = keySet.keyData.Count;
                aqm.motionKeys.Add(keySet);
            }

            aqm.moHeader.nodeCount = aqm.motionKeys.Count;
            return aqm;
        }

        private static int GetFrameTimeFlag(MKEY rot, int f)
        {
            int flag = 0;
            if (f == 0)
            {
                flag = 1;
            }
            else if (f == rot.vector4Keys.Count - 1)
            {
                flag = 2;
            }

            return flag;
        }

        /// <summary>
        /// There are some dummy bones that get exported related to each mesh in the model. We want to skip these until we get to the first real bone in the hierarchy.
        /// </summary>
        /// <returns></returns>
        public static int GetFlverTrueRoot(IFlver flv)
        {
            int bone = -1;
            for (int i = 0; i < flv.Bones.Count; i++)
            {
                if (flv.Bones[i].ParentIndex != -1)
                {
                    return bone;
                }
                bone++;
            }

            return bone;
        }

        public static void GetNSAKeyframes(NSA nsa, IFlver flv, out List<List<Vector3>> translationKeyframeListList, out List<List<Vector4>> rotationKeyframeListList)
        {
            int trueRoot = GetFlverTrueRoot(flv);

            translationKeyframeListList = new List<List<Vector3>>();
            rotationKeyframeListList = new List<List<Vector4>>();

            translationKeyframeListList.Add(nsa.rootMotionSegment.translationFrames.Count > 0 ? nsa.rootMotionSegment.translationFrames : new List<Vector3>() { new Vector3(0, 0, 0) });
            rotationKeyframeListList.Add(nsa.rootMotionSegment.rotationFrames.Count > 0 ? nsa.rootMotionSegment.rotationFrames.ConvertAll(frame => new Vector4(frame.X, frame.Y, frame.Z, frame.W)) : new List<Vector4>() { nsa.rootMotionSegment.rotation.ToVec4() });

            //Fill in dummy bones
            for (int i = 1; i < trueRoot; i++)
            {
                translationKeyframeListList.Add(new List<Vector3>() { new Vector3() });
                rotationKeyframeListList.Add(new List<Vector4>() { new Vector4(0, 0, 0, 1) });
            }

            for (ushort i = 0; i < nsa.header.boneCount; i++)
            {
                //Translation keys
                var translationDynamicIndex = nsa.dynamicTranslationIndices.IndexOf(i);
                var translationStaticIndex = nsa.staticTranslationIndices.IndexOf(i);
                if (translationDynamicIndex != -1)
                {
                    List<Vector3> translationKeys = new List<Vector3>();
                    foreach (var list in nsa.dynamicSegment.translationFrameLists)
                    {
                        translationKeys.Add(list[translationDynamicIndex] + flv.Bones[translationKeyframeListList.Count].Translation);
                    }
                    translationKeyframeListList.Add(translationKeys);
                }
                else if (translationStaticIndex != -1)
                {
                    translationKeyframeListList.Add(new List<Vector3>() { nsa.staticSegment.translationFrames[translationStaticIndex] + flv.Bones[translationKeyframeListList.Count].Translation });
                }
                else //Default - in theory we never reach here
                {
                    translationKeyframeListList.Add(new List<Vector3>());
                }

                //Rotation keys
                var rotationDynamicIndex = nsa.dynamicRotationIndicies.IndexOf(i);
                var rotationStaticIndex = nsa.staticRotationIndices.IndexOf(i);
                if (rotationDynamicIndex != -1)
                {
                    List<Vector4> rotationKeys = new List<Vector4>();
                    foreach (var list in nsa.dynamicSegment.rotationFrameLists)
                    {
                        rotationKeys.Add(list[rotationDynamicIndex].ToVec4() + new Vector4(flv.Bones[rotationKeyframeListList.Count].Rotation.X, flv.Bones[rotationKeyframeListList.Count].Rotation.Y, flv.Bones[rotationKeyframeListList.Count].Rotation.Z, 0));
                    }
                    rotationKeyframeListList.Add(rotationKeys);
                }
                else if (rotationStaticIndex != -1)
                {
                    rotationKeyframeListList.Add(new List<Vector4>() { nsa.staticSegment.rotationFrames[rotationStaticIndex].ToVec4() + new Vector4(flv.Bones[rotationKeyframeListList.Count].Rotation.X, flv.Bones[rotationKeyframeListList.Count].Rotation.Y, flv.Bones[rotationKeyframeListList.Count].Rotation.Z, 0) });
                }
                else //Default - in theory we never reach here
                {
                    rotationKeyframeListList.Add(new List<Vector4>());
                }
            }
        }
    }
}
