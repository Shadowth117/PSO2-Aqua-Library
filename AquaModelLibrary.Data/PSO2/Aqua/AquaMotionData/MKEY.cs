using AquaModelLibrary.Helpers;
using AquaModelLibrary.Helpers.MathHelpers;
using AquaModelLibrary.Helpers.PSO2;
using System.Numerics;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaMotionData
{
    //Motion Key
    public class MKEY
    {
        public int keyType;  //0xEB, type 0x9 
        public int dataType; //0xEC, type 0x9
        public int unkInt0;  //0xF0, type 0x9
        public int keyCount; //0xED, type 0x9
        public int frameAddress;
        public int timeAddress;
        public List<Vector4> vector4Keys = new List<Vector4>(); //0xEE, type 0x4A or 0xCA if multiple
        public List<uint> frameTimings = new List<uint>(); //0xEF, type 0x06 or 0x86 if multiple //Frame timings start with 0 + 0x1 to represent the first frame.
                                                           //Subsequent frames are multiplied by 0x10 and the final frame will have 0x2 added.
                                                           //Ex. frame 0x14 would be stored as 0x140 (0x142 if it's the final frame)
        public List<float> floatKeys = new List<float>(); //0xF1, type 0xA or 0x8A if multiple
        public List<byte> byteKeys = new List<byte>();   //0xF2. Only observed in Alpha PSO2 animations. Appear to be int arrays rendered in bytes... for some reason.
                                                         //Also uses the designator for int keys in newer iterations. Combine every 4 to convert to an int array.
        public List<int> intKeys = new List<int>(); //0xF3, type 0x8 or 0x88 if multiple

        public MKEY() { }
        public MKEY(List<Dictionary<int, object>> mkeyRaw)
        {
            keyType = (int)mkeyRaw[0][0xEB];
            dataType = (int)mkeyRaw[0][0xEC];
            unkInt0 = (int)mkeyRaw[0][0xF0];
            keyCount = (int)mkeyRaw[0][0xED];

            //Get frame timings. Seemingly may not store a frame timing if there's only one frame.
            if (keyCount > 1)
            {
                for (int j = 0; j < keyCount; j++)
                {
                    frameTimings.Add(((ushort[])mkeyRaw[0][0xEF])[j]);
                }
            }
            else if (mkeyRaw[0].ContainsKey(0xEF))
            {
                frameTimings.Add((ushort)mkeyRaw[0][0xEF]);
            }

            //Get frames. The data types stored are different depending on the key count.
            switch (dataType)
            {
                //0x1 and 0x3 are Vector4 arrays essentially. 0x1 is seemingly a Vector3 with alignment padding, but could potentially have things.
                case 0x1:
                case 0x2:
                case 0x3:
                    if (keyCount > 1)
                    {
                        for (int j = 0; j < keyCount; j++)
                        {
                            vector4Keys.Add(((Vector4[])mkeyRaw[0][0xEE])[j]);
                        }
                    }
                    else
                    {
                        vector4Keys.Add((Vector4)mkeyRaw[0][0xEE]);
                    }
                    break;
                case 0x5:
                    if (keyCount > 1)
                    {
                        if (mkeyRaw[0].ContainsKey(0xF3))
                        {
                            for (int j = 0; j < keyCount; j++)
                            {
                                intKeys.Add(((int[])mkeyRaw[0][0xF3])[j]);
                            }
                        }
                        else
                        {
                            for (int j = 0; j < keyCount * 4; j += 4)
                            {
                                intKeys.Add(BitConverter.ToInt32(((byte[])mkeyRaw[0][0xF2]), j));
                            }
                        }
                    }
                    else
                    {
                        if (mkeyRaw[0].ContainsKey(0xF3))
                        {
                            intKeys.Add((int)mkeyRaw[0][0xF3]);
                        }
                        else
                        {
                            for (int j = 0; j < keyCount * 4; j += 4)
                            {
                                intKeys.Add(BitConverter.ToInt32(((byte[])mkeyRaw[0][0xF2]), j));
                            }
                        }
                    }
                    break;
                //0x4 is texture/uv related, 0x6 is Camera related - Array of floats. 0x4 seems to be used for every .aqv frame set interestingly
                case 0x4:
                case 0x6:
                    if (keyCount > 1)
                    {
                        for (int j = 0; j < keyCount; j++)
                        {
                            floatKeys.Add(((float[])mkeyRaw[0][0xF1])[j]);
                        }
                    }
                    else
                    {
                        floatKeys.Add((float)mkeyRaw[0][0xF1]);
                    }
                    break;
                default:
                    throw new Exception($"Unexpected data type: {dataType}");
            }
        }

        public byte[] GetBytesVTBF()
        {
            List<byte> outBytes = new List<byte>();

            VTBFMethods.AddBytes(outBytes, 0xEB, 0x9, BitConverter.GetBytes(keyType));
            VTBFMethods.AddBytes(outBytes, 0xEC, 0x9, BitConverter.GetBytes(dataType));
            VTBFMethods.AddBytes(outBytes, 0xF0, 0x9, BitConverter.GetBytes(unkInt0));
            VTBFMethods.AddBytes(outBytes, 0xED, 0x9, BitConverter.GetBytes(keyCount));

            //Set frame timings. The data types stored are different depending on the key count
            VTBFMethods.HandleOptionalArrayHeader(outBytes, 0xEF, keyCount, 0x06);
            //Write the actual timings
            for (int j = 0; j < frameTimings.Count; j++)
            {
                outBytes.AddRange(BitConverter.GetBytes((ushort)frameTimings[j]));
            }

            //Write frame data. Types will vary.
            switch (dataType)
            {
                //0x1, 0x2, and 0x3 are Vector4 arrays essentially. 0x1 is seemingly a Vector3 with alignment padding, but could potentially have things.
                case 0x1:
                case 0x2:
                case 0x3:
                    VTBFMethods.HandleOptionalArrayHeader(outBytes, 0xEE, keyCount, 0x4A);
                    for (int j = 0; j < frameTimings.Count; j++)
                    {
                        outBytes.AddRange(DataHelpers.ConvertStruct(vector4Keys[j]));
                    }
                    break;
                case 0x5:
                    VTBFMethods.HandleOptionalArrayHeader(outBytes, 0xF3, keyCount, 0x48);
                    if (intKeys.Count > 0)
                    {
                        for (int j = 0; j < frameTimings.Count; j++)
                        {

                            outBytes.AddRange(BitConverter.GetBytes(intKeys[j]));
                        }
                    }
                    else
                    {
                        outBytes.AddRange(byteKeys);
                    }
                    break;
                //0x4 is texture/uv related, 0x6 is Camera related - Array of floats. 0x4 seems to be used for every .aqv frame set interestingly
                case 0x4:
                case 0x6:
                    VTBFMethods.HandleOptionalArrayHeader(outBytes, 0xF1, keyCount, 0xA);
                    for (int j = 0; j < frameTimings.Count; j++)
                    {
                        outBytes.AddRange(BitConverter.GetBytes(floatKeys[j]));
                    }
                    break;
                default:
                    throw new Exception("Unexpected data type!");
            }

            return outBytes.ToArray();
        }

        public int GetTimeMultiplier()
        {
            if ((dataType & 0x80) > 0)
            {
                return 0x100;
            }
            else
            {
                return 0x10;
            }
        }

        //Expects only to be used on a populated MKEY with vector4Keys.
        public Vector4 GetLinearInterpolatedVec4Key(double time)
        {
            if (frameTimings.Count == 0 || time == frameTimings[0] || vector4Keys.Count == 1)
            {
                return vector4Keys[0];
            }
            if (time > frameTimings[frameTimings.Count - 1])
            {
                return vector4Keys[vector4Keys.Count - 1];
            }
            else if (time < 0)
            {
                throw new System.Exception("Time out of range");
            }


            Vector4 lowValue, highValue;
            uint lowTime, highTime;
            GetVec4HighLowTimes(time, out lowValue, out lowTime, out highValue, out highTime);
            if (lowTime == time)
            {
                return lowValue;
            }
            else if (highTime == time)
            {
                return highValue;
            }

            double ratio = GetTimeRatio(ref time, ref lowTime, ref highTime);

            return Vector4.Lerp(lowValue, highValue, (float)ratio);
        }

        //Expects only to be used on a populated MKEY with vector4Keys, particularly a rotation.
        public Vector4 GetSphericalInterpolatedVec4Key(double time)
        {
            if (frameTimings.Count == 0 || time == frameTimings[0] || vector4Keys.Count == 1)
            {
                return vector4Keys[0];
            }
            if (time > frameTimings[frameTimings.Count - 1] || time < 0)
            {
                throw new System.Exception("Time out of range");
            }

            Vector4 lowValue, highValue;
            uint lowTime, highTime;
            GetVec4HighLowTimes(time, out lowValue, out lowTime, out highValue, out highTime);
            if (lowTime == time)
            {
                return lowValue;
            }
            else if (highTime == time)
            {
                return highValue;
            }

            double ratio = GetTimeRatio(ref time, ref lowTime, ref highTime);

            return Quaternion.Slerp(lowValue.ToQuat(), highValue.ToQuat(), (float)ratio).ToVec4();
        }

        private static double GetTimeRatio(ref double time, ref uint lowTime, ref uint highTime)
        {
            //Interpolate based on results
            time /= 0x10;
            highTime /= 0x10;
            lowTime /= 0x10;
            double ratio = (time - lowTime) / (highTime - lowTime);
            return ratio;
        }

        private void GetVec4HighLowTimes(double time, out Vector4 lowValue, out uint lowTime, out Vector4 highValue, out uint highTime)
        {
            //Get high and low times
            lowValue = vector4Keys[0];
            lowTime = 1;
            highValue = vector4Keys[vector4Keys.Count - 1];
            highTime = frameTimings[frameTimings.Count - 1];
            for (int i = 0; i < frameTimings.Count; i++)
            {
                uint frameTime = (uint)frameTimings[i];
                if (frameTime <= time)
                {
                    lowTime = frameTime;
                    lowValue = vector4Keys[i];
                }
                if (frameTime >= time)
                {
                    highTime = frameTime;
                    highValue = vector4Keys[i];
                }
            }
        }

        public void CreateVec4KeysAtTimes(List<uint> timesToAdd)
        {
            int t = 0;
            int timings = frameTimings.Count;
            for (int i = 0; i < timings; i++)
            {
                if (t >= timesToAdd.Count)
                {
                    break;
                }
                //Don't add if it's there already
                if (frameTimings[i] == timesToAdd[t])
                {
                    continue;
                    t++;
                }

                //Add our missing frames when we reach the first frame that exists after them
                if (frameTimings[i] > timesToAdd[t])
                {
                    Vector4 vec4;
                    if (keyType == 2)
                    {
                        vec4 = GetSphericalInterpolatedVec4Key(timesToAdd[t]);
                    }
                    else
                    {
                        vec4 = GetLinearInterpolatedVec4Key(timesToAdd[t]);
                    }
                    vector4Keys.Insert(i, vec4);
                    frameTimings.Insert(i, timesToAdd[t]);
                    t++;
                    timings++;
                }
            }
        }

        public void RemoveParentScaleInfluenceAtTime(uint time, int mode, Vector4 value)
        {
            for (int i = 0; i < frameTimings.Count; i++)
            {
                if (frameTimings[i] == time)
                {
                    var vec4 = vector4Keys[i];
                    /*switch (mode)
                    {
                        case 1: //ZXY order - rotates forward
                            var temp = vec4.X;
                            vec4.X = vec4.Z;
                            vec4.Z = vec4.Y;
                            vec4.Y = temp;
                            break;
                        default:
                            vec4 /= value;
                            break;
                    }*/

                    vec4 /= value;
                    vector4Keys[i] = vec4;
                    break;
                }
            }
        }
    }
}
