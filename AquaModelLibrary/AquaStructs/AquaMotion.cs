using System.Collections.Generic;
using System.Numerics;

namespace AquaModelLibrary
{
    //Though the NIFL format is used for storage, VTBF format tag references for data will be commented where appropriate. Some offset/reserve related things are NIFL only, however.

    //Cameras, UV, and standard motions are essentially the same format.
    public unsafe class AquaMotion : AquaCommon
    {
        public const int CAMO = 0x4F4D4143; //Camera animation
        public const int SPMO = 0x4F4D5053; //UV animation
        public const int NDMO = 0x4F4D444E; //3d Motion animation
        public const int stdAnim = 0x10002;
        public const int stdPlayerAnim = 0x10012;
        public const int cameraAnim = 0x10004;
        public const int materialAnim = 0x20;
        public const int ushortThreshold = 4095; //Threshold to change over to uints for timing. Times are multiplied by 0x10 and so 65535 / 0x10 leaves 4095 as the integer sans remainder
      
        public Dictionary<int, string> keyTypeNames = new Dictionary<int, string>() 
        {
            {0x1, "0x1 Position" },  //Standard, Camera
            {0x2, "0x2 Rotation" },  //Standard
            {0x3, "0x3 Scale" },     //Standard
            {0x4, "0x4 Unk Floats" }, //Camera
            {0x8, "0x8 Unk Vector4s" }, //Alternate UV
            {0x9, "0x9 Unk Vector4s" }, //Alternate UV
            {0xA, "0xA Unk Vector4s" }, //Alternate UV
            {0xB, "0xB Unk Vector4s" }, //Alternate UV
            {0xC, "0xC Unk Vector4s" }, //Alternate UV
            {0xD, "0xD Unk Vector4s" }, //Alternate UV
            {0xE, "0xE Unk Vector4s" }, //Alternate UV
            //0x5-0x7 and 0xD-0x13 seemingly not used in this format. Effects seem to use values in this range for equivalent area there. 
            //In addition, the way those are constructed is akin to an alternative or early version of the overall motion format, based on namings and other bits.  
            {0x10, "0x10 NodeTreeFlag Ints 1" }, //Standard (Player)
            {0x11, "0x11 NodeTreeFlag Ints 2" }, //Standard (Player)
            {0x12, "0x12 NodeTreeFlag Ints 3" }, //Standard (Player)
            {0x14, "0x14 Unk Floats" }, //Camera
            {0x15, "0x15 Unk Floats" }, //Camera
            {0x16, "0x16 Unk Floats" }, //UV
            {0x17, "0x17 Unk Floats" }, //UV
            {0x18, "0x18 Unk Floats" }, //UV
            {0x19, "0x19 Unk Floats" }, //UV
            {0x1A, "0x1A Unk Floats" }, //UV
            {0x1B, "0x1B Unk Floats" }, //UV
            {0x1C, "0x1C Unk Floats" }, //UV
            {0x1D, "0x1D Unk Floats" }, //UV
            {0x1E, "0x1E Unk Floats" }, //Alternate UV
            {0x1F, "0x1F Unk Floats" }, //Alternate UV
            {0x20, "0x20 Unk Floats" }, //Alternate UV
            {0x21, "0x21 Unk Floats" } //Alternate UV
        };
        public List<int> standardTypes = new List<int>()
        {
            0x1,
            0x2,
            0x3,
            0x10,
            0x11,
            0x12
        };
        public List<int> cameraTypes = new List<int>()
        {
            0x1,
            0x4,
            0x14,
            0x15
        };
        public List<int> materialTypes = new List<int>()
        {
            0x8,
            0x9,
            0xA,
            0xB,
            0xC,
            0xD,
            0xE,
            0x16,
            0x17,
            0x18,
            0x19,
            0x1A,
            0x1B,
            0x1C,
            0x1D,
            0x1E,
            0x1F,
            0x20,
            0x21
        };

        public AquaPackage.AFPBase afp;
        public MOHeader moHeader;
        public List<KeyData> motionKeys = new List<KeyData>();

        //MO Header
        public struct MOHeader
        {
            public int variant; //0xE0, type 0x9 //Seems to be different pending type of anim. 0x10002 standard anims, 0x10012 player anims, 0x10004 camera, 0x20 material anims
            public int loopPoint; //0xE1, type 0x9 //Loop point? Only seen in live dance aqms and lower than anim range. Otherwise, 0.
            public int endFrame; //0xE2, type 0x9 //Last frame of the animation (0 based)
            public float frameSpeed; //0xE3, type 0xA //FPS of the animation. PSO2's default is 30.0f

            public int unkInt0;      //0xE4, type 0x9 //Always 0x4 for VTBF, always 0x2 for NIFL anims
            public int nodeCount;    //0xE5, type 0x9 //Number of animated nodes (effect type nodes not included) plus one for NodeTreeFlag stuff if a player anim.
            public int boneTableOffset; //NIFL only, always 0x50. Points to start of the bone name table in NIFL anims
            public PSO2String testString; //0xE6, type 0x2 //Always a string of just "test". Technically a standard 0x20 length PSO2 string, but always observed as "test"

            public int reserve0;
        }

        //Motion segment. Denotes a node's animations. Camera files will only have one of these. 
        public unsafe struct MSEG
        {
            public int nodeType; //0xE7, type 0x9 
            public int nodeDataCount; //0xE8, type 0x9 
            public int nodeOffset;
            public PSO2String nodeName; //0xE9, type 0x2  
            public int nodeId; //0xEA, type 0x9           //0 on material entries
        }

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
                if((dataType & 0x80) > 0)
                {
                    return 0x100;
                } else
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
                if (time > frameTimings[frameTimings.Count - 1] || time < 0)
                {
                    throw new System.Exception("Time out of range");
                }

                //Get high and low times
                Vector4 lowValue = vector4Keys[0];
                uint lowTime = 1;
                Vector4 highValue = vector4Keys[vector4Keys.Count - 1];
                uint highTime = frameTimings[frameTimings.Count - 1];
                for(int i = 0; i < frameTimings.Count; i++)
                {
                    uint frameTime = (uint)frameTimings[i];
                    if(frameTime <= time)
                    {
                        lowTime = frameTime;
                        lowValue = vector4Keys[i];
                    }
                    if(frameTime >= time)
                    {
                        highTime = frameTime;
                        highValue = vector4Keys[i];
                    }
                }
                if(lowTime == time)
                {
                    return lowValue;
                } else if(highTime == time)
                {
                    return highValue;
                }

                //Interpolate based on results
                time /= 0x10;
                highTime /= 0x10;
                lowTime /= 0x10;
                double ratio = (time - lowTime) / (highTime - lowTime);

                return Vector4.Lerp(lowValue, highValue, (float)ratio);
            }

            public void CreateVec4KeysAtTimes(List<uint> timesToAdd)
            {
                int t = 0;
                int timings = frameTimings.Count;
                for (int i = 0; i < timings; i++)
                {
                    if(t >= timesToAdd.Count)
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
                        var vec4 = GetLinearInterpolatedVec4Key(timesToAdd[t]);
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

        public class KeyData
        {
            public MSEG mseg = new MSEG();
            public List<MKEY> keyData = new List<MKEY>();
            //Typically, keyData is generally stored this way, but technically doesn't have to follow this convention:

            //Player/Standard animation data
            //Pos, Rot, Scale data

            //Camera animation Data - Unlike most animation data, only seems to ever contain one node. Seemingly just for fixed cameras. 
            //Pos, unk, unk, unk
            
            //Texture/UV anim data - Seems to contain many types of data, though somewhat untested
            //Seemingly 8 data sets. 

            //Node Tree Flag - Special subsection of data for player animations with an unknown purpose. Not necessary to include, but can be filled
            //with somewhat valid data if the user wishes
            //Pos, Rot data
            public MKEY GetMKEYofType(int type)
            {
                for(int i = 0; i < keyData.Count; i++)
                {
                    if (keyData[i].keyType == type)
                    {
                        return keyData[i];
                    }
                }

                return null;
            }
        }

        public static int GetKeyDataType(int keyType)
        {
            switch(keyType)
            {
                case 0x1:
                    return 0x1;
                case 0x2:
                    return 0x3;
                case 0x3:
                case 0x4:
                    return 0x1;
                case 0x8:
                case 0x9:
                    return 0x2;
                case 0x10:
                case 0x11:
                case 0x12:
                    return 0x5;
                case 0xA:
                    return 0x1;
                case 0xB:
                case 0xC:
                case 0xD:
                case 0xE:
                    return 0x2;
                case 0x14:
                case 0x15:
                    return 0x6;
                case 0x16:
                case 0x17:
                case 0x18:
                case 0x19:
                case 0x1A:
                case 0x1B:
                case 0x1C:
                case 0x1D:
                    return 0x4;
                default:
                    System.Console.WriteLine($"Unknown key type: {keyType}. Returning 1");
                    return 0x1;
            }
        }

        //Converts scale keys to the more typical absolute scaling so they can be edited in a nice way externally.
        public void PrepareScalingForExport(AquaNode aqn)
        {
            int boneCount = System.Math.Min(motionKeys.Count, aqn.nodeList.Count);
            for (int i = 0; i < boneCount; i++)
            {
                var node = aqn.nodeList[i];
                if (node.parentId >= 0)
                {
                    var nodeScale = motionKeys[i].GetMKEYofType(3);
                    var parentNodeScale = motionKeys[node.parentId].GetMKEYofType(3);
                    if (nodeScale == null)
                    {
                        continue;
                    }

                    //Fix up the keyset in case there's only one key
                    //Normally, there's no frametimings at all with single keys, so this simplifies the next part
                    if (parentNodeScale.frameTimings.Count > 1 && nodeScale.frameTimings.Count < 2)
                    {
                        nodeScale.frameTimings.Clear();
                        nodeScale.frameTimings.Add(0x1);
                        nodeScale.frameTimings.Add((ushort)(moHeader.endFrame * 0x10 + 0x2));
                        nodeScale.vector4Keys.Add(nodeScale.vector4Keys[0]); //If there's scale, there should always be at least one key for it
                    }

                    //Create keyframes for each parent key so we can cancel them all
                    List<uint> timingsToAdd = new List<uint>();
                    for (int t = 0; t < parentNodeScale.frameTimings.Count; t++)
                    {
                        if (!nodeScale.frameTimings.Contains(parentNodeScale.frameTimings[t]))
                        {
                            timingsToAdd.Add((uint)parentNodeScale.frameTimings[t]);
                        }
                    }
                    nodeScale.CreateVec4KeysAtTimes(timingsToAdd);
                }
            }

            //Loop backwards through and use the relative scaling of the parent to cancel the child scaling
            for(int i = (boneCount - 1); i > 0; i--)
            {
                var node = aqn.nodeList[i];
                if (node.parentId >= 0)
                {
                    var nodeScale = motionKeys[i].GetMKEYofType(3);
                    var parentNodeScale = motionKeys[node.parentId].GetMKEYofType(3);
                    int mode = 0;

                    //Check order flags
                    if ((node.boneShort1 & 1) > 0)
                    {
                        mode = 1;
                    }

                    if(nodeScale != null)
                    {
                        //Get rid of parental influence
                        for (int t = 0; t < nodeScale.frameTimings.Count; t++)
                        {
                            var currentTime = nodeScale.frameTimings[t];
                            var value = parentNodeScale.GetLinearInterpolatedVec4Key(currentTime);

                            nodeScale.RemoveParentScaleInfluenceAtTime(nodeScale.frameTimings[t], mode, value);
                        }
                    }
                }
            }
        }

    }
}
