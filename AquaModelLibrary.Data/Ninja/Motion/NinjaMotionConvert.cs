using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaMotionData;
using AquaModelLibrary.Helpers.MathHelpers;
using System.Numerics;

namespace AquaModelLibrary.Data.Ninja.Motion
{
    public class NinjaMotionConvert
    {
        /// <summary>
        /// For the older Ninja games, quaternion rotations aren't an option and so useQuaternion should be off so Euler rotations are used
        /// Billy Hatcher and PSO Blue Burst can both use quaternions
        /// </summary>
        public static NJSMotion AQMToNJM(AquaMotion aqm, bool useQuaternion = true)
        {
            NJSMotion njm = new NJSMotion();
            njm.interpoMode = NJD_MTYPE_FN.NJD_MTYPE_LINER;
            njm.frameCount = aqm.moHeader.endFrame + 1;

            njm.KeyDataList = new List<AnimModelData>();
            AnimFlags types = 0;
            for(int i = 0; i < aqm.motionKeys.Count; i++)
            {
                AnimModelData newKeyData = new AnimModelData();
                for(int j = 0; j < aqm.motionKeys[i].keyData.Count; j++)
                {
                    var keyType = aqm.motionKeys[i].keyData[j].keyType;
                    //Find out what types we have to support
                    switch (keyType)
                    {
                        case 1:
                            types = types | AnimFlags.Position;
                            if(aqm.motionKeys[i].keyData[j].vector4Keys.Count == 1)
                            {
                                var key = aqm.motionKeys[i].keyData[j].vector4Keys[0];
                                newKeyData.Position.Add(0, new Vector3(key.X, key.Y, key.Z));
                                newKeyData.Position.Add(njm.frameCount - 1, new Vector3(key.X, key.Y, key.Z));
                            } else
                            {
                                for (int k = 0; k < aqm.motionKeys[i].keyData[j].vector4Keys.Count; k++)
                                {
                                    var key = aqm.motionKeys[i].keyData[j].vector4Keys[k];
                                    var timing = aqm.motionKeys[i].keyData[j].frameTimings[k] / 0x10;
                                    newKeyData.Position.Add((int)timing, new Vector3(key.X, key.Y, key.Z));
                                }
                            }
                            break;
                        case 2:
                            if(useQuaternion)
                            {
                                types = types | AnimFlags.Quaternion;
                                if (aqm.motionKeys[i].keyData[j].vector4Keys.Count == 1)
                                {
                                    var key = aqm.motionKeys[i].keyData[j].vector4Keys[0];
                                    newKeyData.Quaternion.Add(0, new float[] { key.W, key.X, key.Y, key.Z });
                                    newKeyData.Quaternion.Add(njm.frameCount - 1, new float[] { key.W, key.X, key.Y, key.Z });
                                }
                                else
                                {
                                    for (int k = 0; k < aqm.motionKeys[i].keyData[j].vector4Keys.Count; k++)
                                    {
                                        var key = aqm.motionKeys[i].keyData[j].vector4Keys[k];
                                        var timing = aqm.motionKeys[i].keyData[j].frameTimings[k] / 0x10;
                                        newKeyData.Quaternion.Add((int)timing, new float[] { key.W, key.X, key.Y, key.Z });
                                    }
                                }
                            } else
                            {
                                types = types | AnimFlags.Rotation;
                                if (aqm.motionKeys[i].keyData[j].vector4Keys.Count == 1)
                                {
                                    var key = aqm.motionKeys[i].keyData[j].vector4Keys[0];
                                    newKeyData.RotationData.Add(0, new Rotation(MathExtras.QuaternionToEuler(key.ToQuat())));
                                    newKeyData.RotationData.Add(njm.frameCount - 1, new Rotation(MathExtras.QuaternionToEuler(key.ToQuat())));
                                }
                                else
                                {
                                    for (int k = 0; k < aqm.motionKeys[i].keyData[j].vector4Keys.Count; k++)
                                    {
                                        var key = aqm.motionKeys[i].keyData[j].vector4Keys[k];
                                        var timing = aqm.motionKeys[i].keyData[j].frameTimings[k] / 0x10;
                                        newKeyData.RotationData.Add((int)timing, new Rotation(MathExtras.QuaternionToEuler(key.ToQuat())));
                                    }
                                }
                            }
                            break;
                        case 3:
                            types = types | AnimFlags.Scale;
                            if (aqm.motionKeys[i].keyData[j].vector4Keys.Count == 1)
                            {
                                var key = aqm.motionKeys[i].keyData[j].vector4Keys[0];
                                newKeyData.Scale.Add(0, new Vector3(key.X, key.Y, key.Z));
                                newKeyData.Scale.Add(njm.frameCount - 1, new Vector3(key.X, key.Y, key.Z));
                            }
                            else
                            {
                                for (int k = 0; k < aqm.motionKeys[i].keyData[j].vector4Keys.Count; k++)
                                {
                                    var key = aqm.motionKeys[i].keyData[j].vector4Keys[k];
                                    var timing = aqm.motionKeys[i].keyData[j].frameTimings[k] / 0x10;
                                    newKeyData.Scale.Add((int)timing, new Vector3(key.X, key.Y, key.Z));
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }

                njm.KeyDataList.Add(newKeyData);
            }
            njm.animType = types; 

            return njm;
        }

        /// <summary>
        /// Does not account for node remapping required for certain animations in some Ninja games. Will need this for full compatibility
        /// </summary>
        public static List<AquaMotion> NJMToAqmList(List<NJSMotion> njmlist)
        {
            List<AquaMotion> aqmList = new List<AquaMotion>();
            foreach(var njm in njmlist)
            {
                if (njm == null)
                {
                    aqmList.Add(new AquaMotion());
                    continue;
                }
                aqmList.Add(NJMToAQM(njm));
            }

            return aqmList;
        }

        public static AquaMotion NJMToAQM(NJSMotion njm)
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
                }
                else if (keySet.Quaternion.Count > 0)
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
                        //Ninja stores this WXYZ order, unlike PSO2's XYZW
                        mkey.vector4Keys.Add(new Vector4(keySet.Quaternion[keyTimes[k]][1], keySet.Quaternion[keyTimes[k]][2], keySet.Quaternion[keyTimes[k]][3], keySet.Quaternion[keyTimes[k]][0]));
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
                // Light data has no aqm equivalent
                // Camera data? PSO2 equivalents haven't been tested or IDed so far

                aqm.motionKeys.Add(keyData);
            }

            return aqm;
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
