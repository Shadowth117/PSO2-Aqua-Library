using AquaModelLibrary.Helpers.MathHelpers;
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
