using AquaModelLibrary.Extra;
using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.PSU
{
    //Phantasy Star Universe player character animation 
    public class NOM
    {
        public class NomFrame
        {
            public byte frame;
            public byte type;
            public byte type2;
            public List<float> data = new List<float>();
            public List<short> rawData = new List<short>();
            public int filePosition;

            public override string ToString()
            {
                string item = $"Frame {frame.ToString("D3")} ({type} {type2}):";
                //Add the float values to the string
                for (int j = 0; j < data.Count; j++)
                {
                    string temp = data[j].ToString();
                    item += " " + temp;

                    if (j != data.Count - 1)
                    {
                        item += ",";
                    }
                }
                item += " / Raw: ";
                for (int j = 0; j < rawData.Count; j++)
                {
                    string temp = rawData[j].ToString("X4");
                    item += " " + temp;

                    if (j != rawData.Count - 1)
                    {
                        item += ",";
                    }
                }
                item += " at offset ";
                item += filePosition.ToString("X");
                return item;
            }
        }

        private byte[] fileContents;

        public List<List<NomFrame>> rotationFrameList = new List<List<NomFrame>>();
        public List<List<NomFrame>> xPositionFrameList = new List<List<NomFrame>>();
        public List<List<NomFrame>> yPositionFrameList = new List<List<NomFrame>>();
        public List<List<NomFrame>> zPositionFrameList = new List<List<NomFrame>>();
        public ushort frameCount;
        public float frameRate;

        public string[] boneNames = new string[]
        {
            "Root",           //0
            "Navel",          //1
            "Pelvis",         //2
            "L_thigh",        //3
            "L_calf",         //4
            "L_foot",         //5
            "R_thigh",        //6
            "R_calf",         //7
            "R_foot",         //8
            "Spine",          //9
            "Spine1",         //10
            "Neck_root",      //11
            "Neck",           //12
            "Head",           //13
            "L_clavicle",     //14
            "L_upperarm",     //15
            "L_forearm",      //16
            "L_hand",         //17
            "L_weapon",       //18
            "R_clavicle",     //19
            "R_upperarm",     //20
            "R_forearm",      //21
            "R_hand",         //22
            "R_weapon",       //23
            "L_breast",       //24
            "R_breast",       //25
            "Belly",          //26
            "Body"            //27
        };

        public Dictionary<int, int> toPSO2BoneDict = new Dictionary<int, int>()
        {
            { 0, 0 }, //Root
            { 1, 2 }, //Navel->hip
            { 2, 3 }, //Pelvis->pelvis
            { 3, 4 }, //L_thigh->l_thigh
            { 4, 6 }, //L_calf->l_calf
            { 5, 7 }, //L_foot->l_foot_effl
            { 6, 11 }, //R_thigh->r_thigh
            { 7, 13 }, //R_calf->r_calf
            { 8, 14 }, //R_foot->r_foot
            { 9, 19 }, //Spine->navel_4aiming
            { 10, 21 }, //Spine1->spine2
            { 11, 45 }, //NeckRoot->neck0
        };

        //Conversion
        public List<string> nodeNames = new List<string>(); //Default node names for NGS player models
        public Dictionary<int, Quaternion> defaultRots = new Dictionary<int, Quaternion>(); //Bind pose rotations for animations
        public Dictionary<int, Vector4> defaultPos = new Dictionary<int, Vector4>(); //Bind pose translations for animations
        public int endRange = 171; //camera_target

        //Takes the file, splits it based upon which pointer accesses it.
        //Also modifies all pointers to be 0-based!
        public NOM(byte[] rawData)
        {
            List<int> rotationOffsets = new List<int>();
            List<int> positionOffsets = new List<int>();
            List<int> list3Offsets = new List<int>();
            List<int> list4Offsets = new List<int>();

            fileContents = rawData;
            using (Stream stream = (Stream)new MemoryStream(rawData))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                //Skip some meta data looking bytes for now since we don't understand them.
                streamReader.Seek(0x6, SeekOrigin.Begin);
                frameCount = streamReader.Read<ushort>();
                frameRate = streamReader.Read<float>();

                //Skip past initial pointer data since it's redundant for our purposes
                streamReader.Seek(0x40, SeekOrigin.Begin);

                //Populate offset lists. These should always have a set length and a set amount. All observed so far have anyways
                for (int i = 0; i < 28; i++) { rotationOffsets.Add(streamReader.Read<int>()); }
                for (int i = 0; i < 28; i++) { positionOffsets.Add(streamReader.Read<int>()); }
                for (int i = 0; i < 28; i++) { list3Offsets.Add(streamReader.Read<int>()); }
                for (int i = 0; i < 28; i++) { list4Offsets.Add(streamReader.Read<int>()); }

                //Populate the actual frame data lists

                //Read Rotation frame list
                ReadNomList(rotationOffsets, rotationFrameList, streamReader, true);
                ReadNomList(positionOffsets, xPositionFrameList, streamReader, false);
                ReadNomList(list3Offsets, yPositionFrameList, streamReader, false);
                ReadNomList(list4Offsets, zPositionFrameList, streamReader, false);
            }
        }

        private void ReadNomList(List<int> frameOffsets, List<List<NomFrame>> frameList, BufferedStreamReader streamReader, bool isRotList = false)
        {
            for (int i = 0; i < frameOffsets.Count; i++)
            {
                if (frameOffsets[i] != 0)
                {
                    streamReader.Seek(frameOffsets[i], SeekOrigin.Begin);
                    bool continueLoop = true;
                    int sanityCheck = 257; //Really shouldn't trigger, but in the case something is broken it's there.
                    List<NomFrame> frameValues = new List<NomFrame>();

                    //Read frames for the node until there aren't any
                    while (continueLoop)
                    {
                        NomFrame nomFrame = new NomFrame();
                        nomFrame.filePosition = (int)streamReader.Position();
                        nomFrame.frame = streamReader.Read<byte>();
                        nomFrame.type = streamReader.Read<byte>();
                        nomFrame.type2 = (byte)(nomFrame.type % 0x10);
                        nomFrame.type /= 0x10;
                        byte examinedType = nomFrame.type;

                        //Check if we should exit
                        if (nomFrame.frame == frameCount)
                        {
                            if (isRotList && nomFrame.type2 != 0x8)
                            {
                                Console.WriteLine("Unexpected rot frame set end...");
                            }
                            else if (!isRotList)
                            {
                                examinedType -= 0x2;
                            }
                            continueLoop = false;
                        }

                        //Handle different key types. Rotations and other data types handle this differently.
                        int typeCount = 0;
                        if (isRotList)
                        {
                            switch (examinedType)
                            {
                                case 0x0: //quats?
                                    typeCount = 0x4;
                                    break;
                                case 0x5: // interpolate X
                                case 0x6: // interpolate Y
                                case 0x7: // interpolate Z
                                    typeCount = 0x2;
                                    break;
                                case 0x8: // reset all
                                case 0x9: // reset X
                                case 0xA: // reset Y
                                case 0xB: // reset Z
                                    break;
                                default:
                                    Console.WriteLine("Unknown type " + examinedType + " detected at " + streamReader.Position().ToString("X") + " in iteration " + i);
                                    break;
                            }
                        }
                        else
                        {
                            switch (examinedType)
                            {
                                case 0x0: // value
                                    typeCount = 0x1;
                                    break;
                                case 0x4: // interpolate
                                    typeCount = 0x3;
                                    break;
                                case 0x8: // reset
                                    break;
                                default:
                                    Console.WriteLine("Unknown type " + examinedType + " detected at " + streamReader.Position().ToString("X") + " in iteration " + i);
                                    break;
                            }
                        }

                        //Read and store data
                        for (int j = 0; j < typeCount; j++)
                        {
                            short rawValue = streamReader.Read<short>();
                            nomFrame.rawData.Add(rawValue);
                            nomFrame.data.Add(convertValue(rawValue, isRotList));
                        }
                        frameValues.Add(nomFrame);

                        sanityCheck--;
                        if (sanityCheck < 0) { continueLoop = false; }
                    }

                    //Don't add it if it's garbage
                    if (sanityCheck >= 0)
                    {
                        frameList.Add(frameValues);
                    }
                    else
                    {
                        Console.WriteLine($"Bad frame count. Check node {i}, data address {frameOffsets[i].ToString("X")} in file for more info.");
                        frameList.Add(null);
                    }
                }
                else
                {
                    frameList.Add(null);
                }
            }
        }

        //Deobfuscates animation frame data
        private float convertValue(short initialValue, bool isRotValue = false)
        {
            //This value is different for rotation frames
            int finalAddition = 0x37800000;
            if (isRotValue)
            {
                finalAddition = 0x30000000;
            }

            int signum = Math.Sign(initialValue);
            int shifted = (initialValue & 0xFFFF) << 13;
            int initialValue1 = shifted & 0x0F800000;

            //Exit early if 0
            if (initialValue1 == 0)
            {
                return 0.0f;
            }

            int value2 = shifted & 0x007FE000;
            int finalValue1 = initialValue1 + finalAddition;
            int finalFloat = finalValue1 | value2;
            float result = signum * BitConverter.ToSingle(BitConverter.GetBytes(finalFloat), 0);

            return result;
        }

        //Basically, bone nodes have a matrix with world coordinates, but we need coords local to the parent. Therefore, we grab these here.
        public void GetDefaultTransformsFromBones(AquaNode bones, int? endRangeLocal = null)
        {
            if(endRangeLocal == null)
            {
                endRangeLocal = endRange;
            }
            nodeNames.Clear();
            defaultRots.Clear();
            for (int i = 0; i <= endRangeLocal; i++) //<= since we do want to hit that last one
            {
                var bone = bones.nodeList[i];
                Matrix4x4 inverseWorldMatrix = new Matrix4x4(bone.m1.X, bone.m1.Y, bone.m1.Z, bone.m1.W, bone.m2.X, bone.m2.Y, bone.m2.Z, bone.m2.W,
                    bone.m3.X, bone.m3.Y, bone.m3.Z, bone.m3.W, bone.m4.X, bone.m4.Y, bone.m4.Z, bone.m4.W);
                Matrix4x4.Invert(inverseWorldMatrix, out Matrix4x4 worldMatrix);

                Quaternion localRot;
                Vector4 localPos;
                if (bone.parentId == -1)
                {
                    localRot = Quaternion.Inverse(Quaternion.CreateFromRotationMatrix(inverseWorldMatrix));
                    localPos = new Vector4(inverseWorldMatrix.M41, inverseWorldMatrix.M42, inverseWorldMatrix.M43, inverseWorldMatrix.M44);
                }
                else
                {
                    var boneParent = bones.nodeList[bone.parentId];
                    Matrix4x4 parentInverseWorldMatrix = new Matrix4x4(boneParent.m1.X, boneParent.m1.Y, boneParent.m1.Z, boneParent.m1.W, boneParent.m2.X,
                        boneParent.m2.Y, boneParent.m2.Z, boneParent.m2.W, boneParent.m3.X, boneParent.m3.Y, boneParent.m3.Z, boneParent.m3.W, boneParent.m4.X,
                        boneParent.m4.Y, boneParent.m4.Z, boneParent.m4.W);

                    var localMatrix = Matrix4x4.Multiply(worldMatrix, parentInverseWorldMatrix);

                    localRot = Quaternion.CreateFromRotationMatrix(localMatrix);
                    localPos = new Vector4(localMatrix.M41, localMatrix.M42, localMatrix.M43, localMatrix.M44);
                }

                nodeNames.Add(bone.boneName.GetString());
                defaultRots.Add(i, localRot);
                defaultPos.Add(i, localPos);
            }

        }

        public AquaMotion GetPSO2MotionPSUBody(AquaNode bones)
        {
            GetDefaultTransformsFromBones(bones, bones.nodeList.Count - 1);
            AquaMotion aqm = new AquaMotion();

            aqm.moHeader = new AquaMotion.MOHeader();
            aqm.moHeader.frameSpeed = 30;
            aqm.moHeader.endFrame = frameCount;
            aqm.moHeader.unkInt0 = 2;
            aqm.moHeader.variant = 0x2;
            aqm.moHeader.testString.SetString("test");

            //Go through and add NGS nodes 
            for (int i = 0; i < 28; i++)
            {
                var keySet = new AquaMotion.KeyData();
                keySet.mseg.nodeDataCount = 3;
                keySet.mseg.nodeId = i;
                keySet.mseg.nodeType = 2;
                keySet.mseg.nodeName = AquaCommon.PSO2String.GeneratePSO2String(nodeNames[i]);

                var pos = new AquaMotion.MKEY();
                var rot = new AquaMotion.MKEY();
                var scale = new AquaMotion.MKEY();

                // position
                pos.dataType = 1;
                pos.keyCount = 1;
                pos.keyType = 1;
                pos.vector4Keys.Add(defaultPos[i]);

                // rotation
                rot.dataType = 3;
                rot.keyCount = 1;
                rot.keyType = 2;
                rot.vector4Keys.Add(new Vector4(defaultRots[i].X, defaultRots[i].Y, defaultRots[i].Z, defaultRots[i].W));

                // scale
                scale.dataType = 1;
                scale.keyCount = 1;
                scale.keyType = 3;
                scale.vector4Keys.Add(new Vector4(1.0f, 1.0f, 1.0f, 0));


                keySet.keyData.Add(pos);
                keySet.keyData.Add(rot);
                keySet.keyData.Add(scale);

                aqm.motionKeys.Add(keySet);
            }

            for (int i = 0; i < rotationFrameList.Count; i++)
            {
                if (rotationFrameList[i] == null)
                {
                    continue;
                }
                var motionKey = aqm.motionKeys[i];
                var rotKeys = aqm.motionKeys[i].keyData[1];

                var rotBone = rotationFrameList[i];
                Vector4 lastQuat = new Vector4();

                if (rotBone != null && rotBone.Count > 0)
                {
                    rotKeys.keyCount = rotBone.Count;
                    rotKeys.vector4Keys.Clear();
                }

                for (int f = 0; f < rotBone.Count; f++)
                {
                    var rotFrame = rotBone[f];
                    Vector4 quat;
                    bool skip = false;
                    switch (rotFrame.type)
                    {
                        case 0: //4 values, full quaternion data
                            quat = new Vector4(rotFrame.data[0], rotFrame.data[1], rotFrame.data[2], rotFrame.data[3]);
                            break;
                        case 5: //2 values in quaternion with 0s for other quat values
                            quat = new Vector4(rotFrame.data[0], 0, 0, rotFrame.data[1]);
                            break;
                        case 6:
                            quat = new Vector4(0, rotFrame.data[0], 0, rotFrame.data[1]);
                            break;
                        case 7:
                            quat = new Vector4(0, 0, rotFrame.data[0], rotFrame.data[1]);
                            break;
                        case 8: //Use and alter previous value
                            quat = lastQuat;
                            /*
                            quat.Y = -quat.Y;
                            quat.Z = -quat.Z;
                            rotKeys.vector4Keys[f - 1] = quat;
                            if(rotBone.Count - 1 > f)
                            {
                                quat = lastQuat;
                            } 
                            else
                            {
                                skip = true;
                            }*/
                            break;
                        case 9:
                            quat = lastQuat;
                            /*
                            quat.X = -quat.X;
                            quat.Y = -quat.Y;
                            rotKeys.vector4Keys[f - 1] = quat;
                            if (rotBone.Count - 1 > f)
                            {
                                quat = lastQuat;
                            }
                            else
                            {
                                skip = true;
                            }*/
                            break;
                        case 10:
                            quat = lastQuat;
                            /*
                            quat.X = -quat.X;
                            quat.Z = -quat.Z;
                            rotKeys.vector4Keys[f - 1] = quat;
                            if (rotBone.Count - 1 > f)
                            {
                                quat = lastQuat;
                            }
                            else
                            {
                                skip = true;
                            }*/
                            break;
                        default:
                            Debug.WriteLine($"Unexpected type {rotFrame.type}");
                            throw new Exception();
                    }
                    lastQuat = quat;
                   
                    //Assign to pso2 bone
                    int flag = 0;
                    if(f == 0)
                    {
                        flag = 1;
                    } else if(f == rotBone.Count - 1)
                    {
                        flag = 2;
                    }
                    
                    if(skip == true) //Skip being true means this is the last frame for this bone and key type
                    {
                        rotKeys.keyCount--;
                        //Add end flag to previous frame time
                        if (rotKeys.frameTimings != null && rotKeys.frameTimings.Count > 1)
                        {
                            rotKeys.frameTimings[f - 1] += 2;
                        } else if(rotKeys.frameTimings.Count == 1)
                        {
                            rotKeys.frameTimings.Clear();
                        }
                        continue;
                    }
                    rotKeys.vector4Keys.Add(quat);
                    rotKeys.frameTimings.Add((uint)((rotFrame.frame * 0x10) + flag));
                }
            }
            /*
            for(int i = 0; i < rotationFrameList.Count; i++)
            {
                var motionKey = aqm.motionKeys[i];
                var rotKeys = motionKey.keyData[1];
                Debug.WriteLine($"{i} - {boneNames[i]}:");
                for(int k = 0; k < rotKeys.vector4Keys.Count; k++)
                {
                    var euler = MathExtras.QuaternionToEuler(rotKeys.vector4Keys[k].ToQuat());
                    if(rotKeys.frameTimings.Count > 0)
                    {
                        Debug.WriteLine($"{k} Time ({rotKeys.frameTimings[k] / 0x10}): {rotKeys.vector4Keys[k]} Euler: {euler.X} {euler.Y} {euler.Z} Euler PI {euler.X * Math.PI / 180} {euler.Y * Math.PI / 180} {euler.Z * Math.PI / 180}");
                    } else
                    {
                        Debug.WriteLine($"{k}: {rotKeys.vector4Keys[k]} Euler: {euler.X} {euler.Y} {euler.Z} Euler PI {euler.X * Math.PI / 180} {euler.Y * Math.PI / 180} {euler.Z * Math.PI / 180}");
                    }
                }
            }*/

            /*
            for (int i = rotationFrameList.Count - 1; i > 0; i--)
            {
                var motionKey = aqm.motionKeys[i];
                var rotKeys = motionKey.keyData[1];
                var parMotionKey = aqm.motionKeys[bones.nodeList[i].parentId];
                var parRotKeys = parMotionKey.keyData[1];

                for (int f = 0; f < rotKeys.vector4Keys.Count; f++)
                {
                    var quat = rotKeys.vector4Keys[f];
                    var trueQuat = new Quaternion(quat.X, quat.Y, quat.Z, quat.W);

                    double time = 0;
                    if (rotKeys.frameTimings.Count > 0)
                    {
                        time = rotKeys.frameTimings[f];
                    }
                    var parQuat = parRotKeys.GetLinearInterpolatedVec4Key(time);
                    var parTrueQuat = new Quaternion(parQuat.X, parQuat.Y, parQuat.Z, parQuat.W);

                    trueQuat = trueQuat * parTrueQuat; // Quaternion.Inverse(parTrueQuat);

                    rotKeys.vector4Keys[f] = new Vector4(trueQuat.X, trueQuat.Y, trueQuat.Z, trueQuat.W);
                }
            }*/

            //Remove parent influence
            /*
            for(int i = rotationFrameList.Count - 1; i > 0; i--)
            {
                var motionKey = aqm.motionKeys[i];
                var rotKeys = motionKey.keyData[1];
                var parMotionKey = aqm.motionKeys[bones.nodeList[i].parentId];
                var parRotKeys = parMotionKey.keyData[1];

                for (int f = 0; f < rotKeys.vector4Keys.Count; f++)
                {
                    var quat = rotKeys.vector4Keys[f];
                    var trueQuat = new Quaternion(quat.X, quat.Y, quat.Z, quat.W);

                    double time = 0;
                    if(rotKeys.frameTimings.Count > 0)
                    {
                        time = rotKeys.frameTimings[f];
                    }
                    var parQuat = parRotKeys.GetLinearInterpolatedVec4Key(time);
                    var parTrueQuat = new Quaternion(parQuat.X, parQuat.Y, parQuat.Z, parQuat.W);

                    trueQuat = trueQuat * Quaternion.Inverse(parTrueQuat);

                    rotKeys.vector4Keys[f] = new Vector4(trueQuat.X, trueQuat.Y, trueQuat.Z, trueQuat.W);
                }
            }
            */
            aqm.moHeader.nodeCount = aqm.motionKeys.Count;

            return aqm;
        }

        public AquaMotion GetPSO2Motion(AquaNode bones)
        {
            GetDefaultTransformsFromBones(bones);
            AquaMotion aqm = new AquaMotion();

            aqm.moHeader = new AquaMotion.MOHeader();
            aqm.moHeader.frameSpeed = 30;
            aqm.moHeader.endFrame = frameCount;
            aqm.moHeader.unkInt0 = 2;
            aqm.moHeader.variant = 0x2;
            aqm.moHeader.testString.SetString("test");

            //Go through and add NGS nodes 
            for (int i = 0; i < endRange + 1; i++)
            {
                var keySet = new AquaMotion.KeyData();
                keySet.mseg.nodeDataCount = 3;
                keySet.mseg.nodeId = i;
                keySet.mseg.nodeType = 2;
                keySet.mseg.nodeName = AquaCommon.PSO2String.GeneratePSO2String(nodeNames[i]);

                var pos = new AquaMotion.MKEY();
                var rot = new AquaMotion.MKEY();
                var scale = new AquaMotion.MKEY();

                // position
                pos.dataType = 1;
                pos.keyCount = 1;
                pos.keyType = 1;
                pos.vector4Keys.Add(defaultPos[i]);

                // rotation
                rot.dataType = 3;
                rot.keyCount = 1;
                rot.keyType = 2;
                rot.vector4Keys.Add(new Vector4(defaultRots[i].X, defaultRots[i].Y, defaultRots[i].Z, defaultRots[i].W));

                // scale
                scale.dataType = 1;
                scale.keyCount = 1;
                scale.keyType = 3;
                scale.vector4Keys.Add(new Vector4(1.0f, 1.0f, 1.0f, 0));
                

                keySet.keyData.Add(pos);
                keySet.keyData.Add(rot);
                keySet.keyData.Add(scale);

                aqm.motionKeys.Add(keySet);
            }
            
            for(int i = 0; i < rotationFrameList.Count; i++)
            {
                if (rotationFrameList[i] == null)
                {
                    continue;
                }
                var rotBone = rotationFrameList[i];
                Vector4 lastQuat = new Vector4();
                for(int f = 0; f < rotBone.Count; f++)
                {
                    var rotFrame = rotBone[f];
                    Vector4 quat;
                    switch(rotFrame.type)
                    {
                        case 0: //4 values, full quaternion data
                            quat = new Vector4(rotFrame.data[0], rotFrame.data[1], rotFrame.data[2], rotFrame.data[3]);
                            break;
                        case 5: //2 values in quaternion with 0s for other quat values
                        case 6:
                            quat = new Vector4(rotFrame.data[0], rotFrame.data[1], 0, 0);
                            break;
                        case 8: //Use previous value
                        case 9:  
                        case 10:
                            quat = lastQuat;
                            break;
                        default:
                            Debug.WriteLine($"Unexpected type {rotFrame.type}");
                            throw new Exception();
                    }
                    lastQuat = quat;

                    //Assign to pso2 bone
                }
            }

            aqm.moHeader.nodeCount = aqm.motionKeys.Count;

            return aqm;
        }
    }
}
