using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaMotionData;
using AquaModelLibrary.Helpers.MathHelpers;
using System.Numerics;

namespace AquaModelLibrary.Data.Ninja.Motion
{
    public class NinjaMotionConvert
    {
        public static List<AquaMotion> NJMToAqm(List<NJSMotion> njmlist)
        {
            List<AquaMotion> aqmList = new List<AquaMotion>();
            foreach(var njm in njmlist)
            {
                AquaMotion aqm = new AquaMotion();
                aqm.moHeader = new MOHeader();
                aqm.moHeader.endFrame = (int)njm.GetFinalFrameTime();
                aqm.moHeader.nodeCount = njm.KeyDataList.Count;
                aqm.motionKeys = new List<KeyData>();
                for (int ks = 0; ks < njm.KeyDataList.Count; ks++)
                {
                    var keySet = njm.KeyDataList[ks];
                    KeyData keyData = new KeyData();
                    keyData.mseg.nodeName.SetString($"bone{ks}");
                    keyData.mseg.nodeId = ks;
                    keyData.keyData = new List<MKEY>();

                    if (keySet.Position.Count > 0)
                    {
                        MKEY mkey = new MKEY();
                        mkey.dataType = 1;
                        mkey.keyType = 1;
                        mkey.keyCount = keySet.Position.Count;

                        var keyTimes = keySet.Position.Keys.ToList();
                        keyTimes.Sort();
                        for (int k = 0; k < keySet.Position.Count; k++)
                        {
                            mkey.frameTimings.Add((uint)keyTimes[k] * 0x10 + GetPSO2FrameFlag(keyTimes, k));
                            mkey.vector4Keys.Add(new Vector4(keySet.Position[keyTimes[k]], 0));
                        }
                        keyData.keyData.Add(mkey);
                    }
                    //Euler rotations quaternions are stored separately
                    if (keySet.RotationData.Count > 0)
                    {
                        MKEY mkey = new MKEY();
                        mkey.dataType = 3;
                        mkey.keyType = 2;
                        mkey.keyCount = keySet.RotationData.Count;

                        var keyTimes = keySet.RotationData.Keys.ToList();
                        keyTimes.Sort();
                        for (int k = 0; k < keySet.RotationData.Count; k++)
                        {
                            mkey.frameTimings.Add((uint)keyTimes[k] * 0x10 + GetPSO2FrameFlag(keyTimes, k));
                            var quat = MathExtras.EulerToQuaternion(keySet.RotationData[keyTimes[k]].Deg);
                            mkey.vector4Keys.Add(quat.ToVec4());
                        }
                        keyData.keyData.Add(mkey);
                    } else if(keySet.Quaternion.Count > 0)
                    {
                        MKEY mkey = new MKEY();
                        mkey.dataType = 3;
                        mkey.keyType = 2;
                        mkey.keyCount = keySet.Quaternion.Count;

                        var keyTimes = keySet.Quaternion.Keys.ToList();
                        keyTimes.Sort();
                        for (int k = 0; k < keySet.Quaternion.Count; k++)
                        {
                            mkey.frameTimings.Add((uint)keyTimes[k] * 0x10 + GetPSO2FrameFlag(keyTimes, k));
                            mkey.vector4Keys.Add(new Vector4(keySet.Quaternion[keyTimes[k]][0], keySet.Quaternion[keyTimes[k]][1], keySet.Quaternion[keyTimes[k]][2], keySet.Quaternion[keyTimes[k]][3]));
                        }
                        keyData.keyData.Add(mkey);
                    }
                    if (keySet.Scale.Count > 0)
                    {
                        MKEY mkey = new MKEY();
                        mkey.dataType = 1;
                        mkey.keyType = 3;
                        mkey.keyCount = keySet.Scale.Count;

                        var keyTimes = keySet.Scale.Keys.ToList();
                        keyTimes.Sort();
                        for (int k = 0; k < keySet.Scale.Count; k++)
                        {
                            mkey.frameTimings.Add((uint)keyTimes[k] * 0x10 + GetPSO2FrameFlag(keyTimes, k));
                            mkey.vector4Keys.Add(new Vector4(keySet.Scale[keyTimes[k]], 0));
                        }
                        keyData.keyData.Add(mkey);
                    }
                    aqm.motionKeys.Add(keyData);
                }
                aqmList.Add(aqm);
            }

            return aqmList;
        }
        private static uint GetPSO2FrameFlag(List<int> frameList, int frameIndex)
        {
            uint flag = 0;
            if (frameIndex == 0)
            {
                flag = 1;
            }
            else if (frameIndex == frameList.Count - 1)
            {
                flag = 2;
            }

            return flag;
        }
    }
}
