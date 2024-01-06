using AquaModelLibrary.Data.DataTypes.SetLengthStrings;
using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.PSO2.Aqua.AquaMotionData;
using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers.MathHelpers;
using Marathon.Formats.Mesh.Ninja;
using System.Diagnostics;
using System.Numerics;
using static AquaModelLibrary.Helpers.Extensions.ByteListExtension;
using static Marathon.Formats.Mesh.Ninja.NinjaKeyframe;

namespace AquaModelLibrary.Data.PSU
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
        public List<short> extraData = new List<short>();
        public ushort frameCount;
        public float frameRate;
        public int boneCount;

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
            { 0,  0 }, //Root
            { 1,  2 }, //Navel->hip
            { 2,  3 }, //Pelvis->pelvis
            { 3,  4 }, //L_thigh->l_thigh
            { 4,  6 }, //L_calf->l_calf
            { 5,  7 }, //L_foot->l_foot_effl
            { 6,  11 }, //R_thigh->r_thigh
            { 7,  13 }, //R_calf->r_calf
            { 8,  14 }, //R_foot->r_foot
            { 9,  19 }, //Spine->navel_4aiming
            { 10, 21 }, //Spine1->spine2
            //{11, ? }, //No direct compare?
            { 12, 45 }, //NeckRoot->neck0
            { 13, 47 }, //Head->head
            { 14, 22 }, //L_clavicle->l_clavicle
            { 15, 23 }, //L_upperarm->l_upperarm
            { 16, 25 }, //L_forearm->l_forearm
            { 17, 27 }, //L_hand->l_hand
            { 18, 28 }, //L_weapon->l_weapon_efhl
            { 19, 30 }, //R_clavicle->r_clavicle
            { 20, 31 }, //R_upperarm->r_upperarm
            { 21, 33 }, //R_forearm->r_forearm
            { 22, 35 }, //R_hand->r_hand
            { 23, 36 }, //R_weapon->r_weapon_efhl
            { 24, 41 }, //L_breast->l_breast
            { 25, 43 }, //R_breast->r_breast
            { 26, 18 }, //Belly->crotch
            //{ 27, ? }, //Body->?
        };

        public Dictionary<int, int> fromPSO2BoneDict = new Dictionary<int, int>()
        {
            { 0, 0 }, //Root
            { 2, 1 }, //Navel<-hip
            { 3, 2 }, //Pelvis<-pelvis
            { 4, 3 }, //L_thigh<-l_thigh
            { 6, 4 }, //L_calf<-l_calf
            { 7, 5 }, //L_foot<-l_foot_effl
            { 11, 6 }, //R_thigh<-r_thigh
            { 13, 7 }, //R_calf<-r_calf
            { 14, 8 }, //R_foot<-r_foot
            { 19, 9 }, //Spine<-navel_4aiming
            { 21,10 }, //Spine1<-spine2
            { 45,12 }, //NeckRoot<-neck0
            { 47,13 }, //Head<-head
            { 22,14 }, //L_clavicle<-l_clavicle
            { 23,15 }, //L_upperarm<-l_upperarm
            { 25,16 }, //L_forearm<-l_forearm
            { 27,17 }, //L_hand<-l_hand
            { 28,18 }, //L_weapon<-l_weapon_efhl
            { 30,19 }, //R_clavicle<-r_clavicle
            { 31,20 }, //R_upperarm<-r_upperarm
            { 33,21 }, //R_forearm<-r_forearm
            { 35,22 }, //R_hand<-r_hand
            { 36,23 }, //R_weapon<-r_weapon_efhl
            { 41,24 }, //L_breast<-l_breast
            { 43,25 }, //R_breast<-r_breast
            { 18,26 }, //Belly<-crotch
        };

        //Conversion
        public List<string> nodeNames = new List<string>(); //Default node names for NGS player models
        public Dictionary<int, Quaternion> defaultRots = new Dictionary<int, Quaternion>(); //Bind pose rotations for animations
        public Dictionary<int, Vector4> defaultPos = new Dictionary<int, Vector4>(); //Bind pose translations for animations
        public Dictionary<int, Quaternion> defaultRotsPSO2 = new Dictionary<int, Quaternion>(); //Bind pose rotations for animations
        public Dictionary<int, Vector4> defaultPosPSO2 = new Dictionary<int, Vector4>(); //Bind pose translations for animations
        public int endRange = 171; //camera_target

        public NOM()
        {

        }

        //Takes the file, splits it based upon which pointer accesses it.
        //Also modifies all pointers to be 0-based!
        public NOM(byte[] rawData)
        {
            List<int> rotationOffsets = new List<int>();
            List<int> positionOffsets = new List<int>();
            List<int> list3Offsets = new List<int>();
            List<int> list4Offsets = new List<int>();

            fileContents = rawData;
            using (MemoryStream stream = new MemoryStream(rawData))
            using (var streamReader = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                //Skip some meta data looking bytes for now since we don't understand them.
                streamReader.Seek(0x6, SeekOrigin.Begin);
                frameCount = streamReader.Read<ushort>();
                frameRate = streamReader.Read<float>();
                boneCount = streamReader.Read<int>();

                //Skip past initial pointer data since it's redundant for our purposes
                streamReader.Seek(0x40, SeekOrigin.Begin);

                //Populate offset lists. These should always have a set length and a set amount. All observed so far have anyways
                for (int i = 0; i < boneCount; i++) { rotationOffsets.Add(streamReader.Read<int>()); }
                for (int i = 0; i < boneCount; i++) { positionOffsets.Add(streamReader.Read<int>()); }
                for (int i = 0; i < boneCount; i++) { list3Offsets.Add(streamReader.Read<int>()); }
                for (int i = 0; i < boneCount; i++) { list4Offsets.Add(streamReader.Read<int>()); }

                //Populate the actual frame data lists

                //Read Rotation frame list
                ReadNomList(rotationOffsets, rotationFrameList, streamReader, true);
                ReadNomList(positionOffsets, xPositionFrameList, streamReader, false);
                ReadNomList(list3Offsets, yPositionFrameList, streamReader, false);
                ReadNomList(list4Offsets, zPositionFrameList, streamReader, false);

                while (streamReader.Position < rawData.Length)
                {
                    var pos = streamReader.Position;
                    extraData.Add(streamReader.Read<short>());
                    var dataPos = unpackValue(extraData[extraData.Count - 1], false);
                    var dataRot = unpackValue(extraData[extraData.Count - 1], true);
                    Debug.WriteLine($"Extra @ Address {pos:X} - pos {dataPos} - rot {dataRot}");
                }
            }
        }

        private void ReadNomList(List<int> frameOffsets, List<List<NomFrame>> frameList, BufferedStreamReaderBE<MemoryStream> streamReader, bool isRotList = false)
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
                        nomFrame.filePosition = (int)streamReader.Position;
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
                                Debug.WriteLine("Unexpected rot frame set end...");
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
                                    Debug.WriteLine("Unknown rotation type " + examinedType + " detected at " + streamReader.Position.ToString("X") + " in iteration " + i);
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
                                case 0x2:
                                    typeCount = 0x4;
                                    break;
                                case 0x4: // interpolate
                                    typeCount = 0x3;
                                    break;
                                case 0x6:
                                    typeCount = 0x3;
                                    break;
                                case 0x8: // reset
                                    break;
                                case 0xA:
                                    break;
                                default:
                                    Debug.WriteLine("Unknown position type " + examinedType + " detected at " + streamReader.Position.ToString("X") + " in iteration " + i);
                                    break;
                            }
                        }

                        //Read and store data
                        for (int j = 0; j < typeCount; j++)
                        {
                            short rawValue = streamReader.Read<short>();
                            nomFrame.rawData.Add(rawValue);
                            nomFrame.data.Add(unpackValue(rawValue, isRotList));
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
                        Debug.WriteLine($"Bad frame count. Check node {i}, data address {frameOffsets[i].ToString("X")} in file for more info.");
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
        private float unpackValue(short initialValue, bool isRotValue = false)
        {
            //This value is different for rotation frames
            int finalAddition = 0x37800000;
            if (isRotValue)
            {
                finalAddition = 0x30000000;
            }

            int signum = Math.Sign(initialValue);
            int initialValueAnd = (initialValue & 0xFFFF);
            int shifted = initialValueAnd << 13;
            int initialValue1 = shifted & 0x0F800000;

            //Exit early if 0
            if (initialValue1 == 0)
            {
                float zero = 0.0f;
                zero *= signum;
                return zero;
            }

            int value2 = shifted & 0x007FE000;
            int finalValue1 = initialValue1 + finalAddition;
            int finalFloat = finalValue1 | value2;
            float result = signum * BitConverter.ToSingle(BitConverter.GetBytes(finalFloat), 0);

            return result;
        }

        //Agra the GOAT. Praise this man.
        private short packValueAgra(float initialValue, bool isRotValue = false)
        {
            int topBits = isRotValue ? 0x30000000 : 0x37800000;
            int signum = Math.Sign(initialValue);
            byte[] bytes = BitConverter.GetBytes(Math.Abs(initialValue));
            int intRepresentation = BitConverter.ToInt32(bytes, 0);
            if (intRepresentation <= topBits)
            {
                return 0;
            }
            int extractedBits = intRepresentation - topBits;
            if ((extractedBits & 0xF800000) == 0)
            {
                return 0;
            }
            int isolatedBits = extractedBits & 0x0FFFE000;
            int resultValue = isolatedBits >> 13;
            if (signum != 1)
            {
                resultValue |= 0x8000;
            }
            return (short)(resultValue);
        }

        public byte[] GetBytes()
        {
            List<byte> outbytes = new List<byte>();
            List<int> pointerListOffsets = new List<int>();
            outbytes.Add(0x1);
            outbytes.Add(0x0);
            outbytes.Add(0x2);
            outbytes.Add(0x10);
            outbytes.Add(0x0);
            outbytes.Add(0x0);

            ushort? endFrame = null;
            for (int i = 0; i < rotationFrameList.Count; i++)
            {
                if (rotationFrameList[i] != null)
                {
                    endFrame = rotationFrameList[i][rotationFrameList[i].Count - 1].frame;
                    break;
                }
                if (xPositionFrameList[i] != null)
                {
                    endFrame = xPositionFrameList[i][xPositionFrameList[i].Count - 1].frame;
                    break;
                }
                if (yPositionFrameList[i] != null)
                {
                    endFrame = yPositionFrameList[i][yPositionFrameList[i].Count - 1].frame;
                    break;
                }
                if (zPositionFrameList[i] != null)
                {
                    endFrame = zPositionFrameList[i][zPositionFrameList[i].Count - 1].frame;
                    break;
                }
            }
            if (endFrame == null)
            {
                Debug.WriteLine("Tried to get bytes from an empty anim...");
                return null;
            }
            outbytes.AddRange(BitConverter.GetBytes((ushort)endFrame));
            outbytes.AddRange(BitConverter.GetBytes(frameRate));
            outbytes.AddRange(BitConverter.GetBytes(boneCount)); //Bone Count. Probably ALWAYS 0x1C

            outbytes.AddRange(BitConverter.GetBytes(0x0)); //Pointer to rotation data start
            outbytes.AddRange(BitConverter.GetBytes(0x0)); //Pointer to x position data start
            outbytes.AddRange(BitConverter.GetBytes(0x0)); //Pointer to y position data start
            outbytes.AddRange(BitConverter.GetBytes(0x0)); //Pointer to z position data start

            outbytes.AddRange(BitConverter.GetBytes(0x40)); //Pointer to rotation data pointers
            outbytes.AddRange(BitConverter.GetBytes(0xB0)); //Pointer to x position data pointers
            outbytes.AddRange(BitConverter.GetBytes(0x120)); //Pointer to y position data pointers
            outbytes.AddRange(BitConverter.GetBytes(0x190)); //Pointer to z position data pointers

            outbytes.AddRange(BitConverter.GetBytes(0)); //File size
            outbytes.AlignWriter(0x10);

            for (int a = 0; a < 4; a++)
            {
                pointerListOffsets.Add(outbytes.Count);
                for (int i = 0; i < boneCount; i++) //Pointers for all 
                {
                    outbytes.AddRange(BitConverter.GetBytes(0));
                }
                outbytes.AlignWriter(0x10);
            }

            //Write Rotation Data
            outbytes.SetByteListInt(0x10, outbytes.Count);
            WriteNOMList(outbytes, rotationFrameList, pointerListOffsets[0], true);

            //Write X Position Data
            outbytes.SetByteListInt(0x14, outbytes.Count);
            WriteNOMList(outbytes, xPositionFrameList, pointerListOffsets[1], false);

            //Write Y Position Data
            outbytes.SetByteListInt(0x18, outbytes.Count);
            WriteNOMList(outbytes, yPositionFrameList, pointerListOffsets[2], false);

            //Write Z Position Data
            outbytes.SetByteListInt(0x1C, outbytes.Count);
            WriteNOMList(outbytes, zPositionFrameList, pointerListOffsets[3], false);
            outbytes.AlignWriter(0x4);

            foreach (var edata in extraData)
            {
                outbytes.AddRange(BitConverter.GetBytes(edata));
            }
            outbytes.SetByteListInt(0x30, outbytes.Count);

            return outbytes.ToArray();
        }

        public void WriteNOMList(List<byte> outbytes, List<List<NomFrame>> nomList, int pointerListOffset, bool isRotValue = false)
        {
            for (int i = 0; i < nomList.Count; i++)
            {
                if (nomList[i] != null)
                {
                    outbytes.SetByteListInt(pointerListOffset + 0x4 * i, outbytes.Count);
                    var currentList = nomList[i];

                    //Read frames for the node until there aren't any
                    for (int f = 0; f < currentList.Count; f++)
                    {
                        NomFrame nomFrame = currentList[f];
                        outbytes.Add(nomFrame.frame);
                        outbytes.Add((byte)((nomFrame.type * 0x10) + (nomFrame.type2)));
                        foreach (var value in nomFrame.data)
                        {
                            outbytes.AddRange(BitConverter.GetBytes(packValueAgra(value, isRotValue)));
                        }
                    }
                }
            }
        }

        //Basically, bone nodes have a matrix with world coordinates, but we need coords local to the parent. Therefore, we grab these here.
        public void GetDefaultTransformsFromBones(AquaNode bones, int? endRangeLocal = null)
        {
            if (endRangeLocal == null)
            {
                endRangeLocal = endRange;
            }
            nodeNames.Clear();
            defaultRots.Clear();
            defaultPos.Clear();

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

        //Basically, bone nodes have a matrix with world coordinates, but we need coords local to the parent. Therefore, we grab these here.
        public void GetDefaultTransformsFromPSO2Bones(AquaNode bones, int? endRangeLocal = null)
        {
            if (endRangeLocal == null)
            {
                endRangeLocal = endRange;
            }
            nodeNames.Clear();
            defaultRotsPSO2.Clear();
            defaultPosPSO2.Clear();

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
                defaultRotsPSO2.Add(i, localRot);
                defaultPosPSO2.Add(i, localPos);
            }

        }

        public List<(Vector3 data, int frame)> GetPositionKeys(int boneNum)
        {
            List<(Vector3 data, int frame)> posFrames = new List<(Vector3 data, int frame)>();
            List<int> times = new List<int>();

            //Get all frame times
            for (int i = 0; i < xPositionFrameList.Count; i++)
            {
                GetTimes(xPositionFrameList[boneNum], times);
                GetTimes(yPositionFrameList[boneNum], times);
                GetTimes(zPositionFrameList[boneNum], times);
            }

            //Interpolate and combine frame data
            for (int i = 0; i < times.Count; i++)
            {
                var xVal = defaultPos[boneNum].X;
                var yVal = defaultPos[boneNum].Y;
                var zVal = defaultPos[boneNum].Z;

                if (xPositionFrameList[boneNum] != null)
                {
                    xVal = GetPosValueAtTime(xPositionFrameList[boneNum], times[i]);
                }
                if (yPositionFrameList[boneNum] != null)
                {
                    yVal = GetPosValueAtTime(yPositionFrameList[boneNum], times[i]);
                }
                if (zPositionFrameList[boneNum] != null)
                {
                    zVal = GetPosValueAtTime(zPositionFrameList[boneNum], times[i]);
                }

                posFrames.Add((new Vector3(xVal, yVal, zVal), times[i]));
            }

            return posFrames;
        }

        public static void GetTimes(List<NomFrame> bone, List<int> times)
        {
            if (bone == null)
            {
                return;
            }
            for (int f = 0; f < bone.Count; f++)
            {
                if (!times.Contains(bone[f].frame))
                {
                    times.Add(bone[f].frame);
                }
            }
        }

        public static float GetPosValue(List<NomFrame> bone, int index)
        {
            switch (bone[index].type)
            {
                case 8:
                case 9:
                case 10:
                    return bone[index - 1].data[0];
                default:
                    return bone[index].data[0];
            }

        }

        public static float GetPosValueAtTime(List<NomFrame> bone, double time)
        {
            if (bone.Count == 0 || time == bone[0].frame || bone.Count == 1)
            {
                return bone[0].data[0];
            }
            if (time > bone[bone.Count - 1].frame || time < 0)
            {
                throw new Exception("Time out of range");
            }

            //Get high and low times
            float lowValue = GetPosValue(bone, 0);
            uint lowTime = 1;
            float highValue = GetPosValue(bone, bone.Count - 1);
            uint highTime = bone[bone.Count - 1].frame;
            for (int i = 0; i < bone.Count; i++)
            {
                uint frameTime = (uint)bone[i].frame;
                if (frameTime <= time)
                {
                    lowTime = frameTime;
                    lowValue = GetPosValue(bone, i);
                }
                if (frameTime >= time)
                {
                    highTime = frameTime;
                    highValue = GetPosValue(bone, i);
                }
            }
            if (lowTime == time)
            {
                return lowValue;
            }
            else if (highTime == time)
            {
                return highValue;
            }

            //Interpolate based on results
            time /= 0x10;
            highTime /= 0x10;
            lowTime /= 0x10;
            double ratio = (time - lowTime) / (highTime - lowTime);
            var distance = (float)Math.Sqrt(Math.Pow(highValue - lowValue, 2));
            var finalValue = (float)(lowValue + (distance * ratio));

            return finalValue;
        }

        public void CreateFromNNMotion(NinjaMotion nm)
        {
            frameRate = nm.Framerate;
            boneCount = 28; //We're just gonna assume this is meant for player anims right now
            frameCount = (ushort)nm.EndFrame;

            rotationFrameList = new List<List<NomFrame>>();
            xPositionFrameList = new List<List<NomFrame>>();
            yPositionFrameList = new List<List<NomFrame>>();
            zPositionFrameList = new List<List<NomFrame>>();

            for (int i = 0; i < 28; i++)
            {
                rotationFrameList.Add(null);
                xPositionFrameList.Add(null);
                yPositionFrameList.Add(null);
                zPositionFrameList.Add(null);
            }

            foreach (var sub in nm.SubMotions)
            {
                if (sub.Type.HasFlag(SubMotionType.NND_SMOTTYPE_TRANSLATION_MASK))
                {
                    var posFramesX = new List<NomFrame>();
                    var posFramesY = new List<NomFrame>();
                    var posFramesZ = new List<NomFrame>();
                    for (int i = 0; i < sub.Keyframes.Count; i++)
                    {
                        var posFrameX = new NomFrame();
                        var posFrameY = new NomFrame();
                        var posFrameZ = new NomFrame();

                        var kf = (NNS_MOTION_KEY_VECTOR)sub.Keyframes[i];
                        posFrameX.frame = (byte)kf.Frame;
                        posFrameY.frame = (byte)kf.Frame;
                        posFrameZ.frame = (byte)kf.Frame;
                        posFrameX.data.Add(kf.Value.X);
                        posFrameY.data.Add(kf.Value.Y);
                        posFrameZ.data.Add(kf.Value.Z);

                        if (i == sub.Keyframes.Count - 1)
                        {
                            posFrameX.type2 = 8;
                        }

                        posFramesX.Add(posFrameX);
                        posFramesY.Add(posFrameY);
                        posFramesZ.Add(posFrameZ);
                    }

                    //Sanity check. If we have a frame, it needs to have at least one more to cap it off
                    if (posFramesX.Count == 1)
                    {
                        var posFrameXEnd = new NomFrame();
                        posFrameXEnd.type2 = 8;
                        posFrameXEnd.frame = (byte)nm.EndFrame;
                        posFrameXEnd.data = posFramesX[0].data;
                        posFramesX.Add(posFrameXEnd);
                    }
                    if (posFramesY.Count == 1)
                    {
                        var posFrameYEnd = new NomFrame();
                        posFrameYEnd.type2 = 8;
                        posFrameYEnd.frame = (byte)nm.EndFrame;
                        posFrameYEnd.data = posFramesY[0].data;
                        posFramesY.Add(posFrameYEnd);
                    }
                    if (posFramesZ.Count == 1)
                    {
                        var posFrameZEnd = new NomFrame();
                        posFrameZEnd.type2 = 8;
                        posFrameZEnd.frame = (byte)nm.EndFrame;
                        posFrameZEnd.data = posFramesZ[0].data;
                        posFramesZ.Add(posFrameZEnd);
                    }

                    xPositionFrameList[sub.NodeIndex] = posFramesX;
                    yPositionFrameList[sub.NodeIndex] = posFramesY;
                    zPositionFrameList[sub.NodeIndex] = posFramesZ;
                }
                else if (sub.Type.HasFlag(SubMotionType.NND_SMOTTYPE_ROTATION_XYZ))
                {
                    var rotFrames = new List<NomFrame>();
                    for (int i = 0; i < sub.Keyframes.Count; i++)
                    {
                        var rotFrame = new NomFrame();

                        var kf = (NNS_MOTION_KEY_ROTATE_A16)sub.Keyframes[i];
                        rotFrame.frame = (byte)kf.Frame;
                        var vec3 = kf.GetVec3();
                        var quat = MathExtras.EulerToQuaternion(vec3);

                        rotFrame.data.Add(quat.X);
                        rotFrame.data.Add(quat.Y);
                        rotFrame.data.Add(quat.Z);
                        rotFrame.data.Add(quat.W);

                        if (i == sub.Keyframes.Count - 1)
                        {
                            rotFrame.type2 = 8;
                        }

                        rotFrames.Add(rotFrame);
                    }

                    //Sanity check. If we have a frame, it needs to have at least one more to cap it off
                    if (rotFrames.Count == 1)
                    {
                        var rotFrameEnd = new NomFrame();
                        rotFrameEnd.type2 = 8;
                        rotFrameEnd.frame = (byte)nm.EndFrame;
                        rotFrameEnd.data = rotFrames[0].data;
                        rotFrames.Add(rotFrameEnd);
                    }
                    rotationFrameList[sub.NodeIndex] = rotFrames;
                }
                else
                {
                    throw new Exception("Unexpected frame type!");
                }
            }
        }

        public void CreateFromPSO2BodyMotion(AquaMotion aqm, AquaNode psuAqn, AquaNode pso2Aqn)
        {
            GetDefaultTransformsFromBones(psuAqn, 27);
            GetDefaultTransformsFromPSO2Bones(pso2Aqn);

            //Create a stripped down aqm with only the bones we need
            AquaMotion psuAqm = new AquaMotion();
            psuAqm.moHeader = new MOHeader();
            psuAqm.moHeader.frameSpeed = aqm.moHeader.frameSpeed;
            psuAqm.moHeader.endFrame = aqm.moHeader.endFrame;

            for (int i = 0; i < 28; i++)
            {
                if (toPSO2BoneDict.TryGetValue(i, out int key))
                {
                    var value = aqm.motionKeys[key];

                    //Position keys
                    for (int k = 0; k < value.keyData[0].vector4Keys.Count; k++)
                    {
                        //Strip original bone transform
                        var frameData = value.keyData[0].vector4Keys[k] - defaultPosPSO2[key];
                        frameData *= 10; //position values in pso2 are 1/10th of psu's

                        //Apply to PSU bone transform
                        value.keyData[0].vector4Keys[k] = frameData + defaultPos[i];
                    }

                    //Rotation Keys
                    for (int k = 0; k < value.keyData[1].vector4Keys.Count; k++)
                    {
                        //Strip original bone transform
                        var frameData = value.keyData[1].vector4Keys[k].ToQuat() * Quaternion.Inverse(defaultRotsPSO2[key]);
                        if (i == 9)
                        {
                            frameData = Quaternion.Inverse(frameData);
                        }
                        //Apply to PSU bone transform
                        value.keyData[1].vector4Keys[k] = (frameData * defaultRots[i]).ToVec4();
                    }

                    psuAqm.motionKeys.Add(value);
                }
                else
                {
                    psuAqm.motionKeys.Add(null);
                }
            }

            CreateFromPSO2Motion(psuAqm);
        }

        public void CreateFromPSO2Motion(AquaMotion aqm)
        {
            frameRate = aqm.moHeader.frameSpeed;
            boneCount = aqm.motionKeys.Count;

            if (aqm.motionKeys[aqm.motionKeys.Count - 1] != null && aqm.motionKeys[aqm.motionKeys.Count - 1].mseg.nodeName.GetString().ToLower().Contains("nodetreeflag"))
            {
                boneCount -= 1;
            }

            rotationFrameList = new List<List<NomFrame>>();
            xPositionFrameList = new List<List<NomFrame>>();
            yPositionFrameList = new List<List<NomFrame>>();
            zPositionFrameList = new List<List<NomFrame>>();

            for (int i = 0; i < boneCount; i++)
            {
                var rotKeys = new List<NomFrame>();
                var xPosKeys = new List<NomFrame>();
                var yPosKeys = new List<NomFrame>();
                var zPosKeys = new List<NomFrame>();

                if (aqm.motionKeys[i] == null)
                {
                    rotationFrameList.Add(null);
                    xPositionFrameList.Add(null);
                    yPositionFrameList.Add(null);
                    zPositionFrameList.Add(null);
                    continue;
                }

                var posFrames = aqm.motionKeys[i].keyData[0];
                var rotFrames = aqm.motionKeys[i].keyData[1];

                //Position
                //Precheck
                bool skipPosX = true;
                bool skipPosY = true;
                bool skipPosZ = true;
                var pos0 = posFrames.vector4Keys[0];
                for (int f = 0; f < posFrames.vector4Keys.Count; f++)
                {
                    var pos = posFrames.vector4Keys[f];
                    if (pos0.X != pos.X)
                    {
                        skipPosX = false;
                    }
                    if (pos0.Y != pos.Y)
                    {
                        skipPosY = false;
                    }
                    if (pos0.Z != pos.Z)
                    {
                        skipPosZ = false;
                    }
                }

                for (int f = 0; f < posFrames.vector4Keys.Count; f++)
                {
                    var pos = posFrames.vector4Keys[f];
                    var xPosNomFrom = new NomFrame();
                    xPosNomFrom.data = new List<float>() { pos.X };
                    var yPosNomFrom = new NomFrame();
                    yPosNomFrom.data = new List<float>() { pos.Y };
                    var zPosNomFrom = new NomFrame();
                    zPosNomFrom.data = new List<float>() { pos.Z };

                    //Handle lack of frameTimings
                    if (posFrames.frameTimings == null || posFrames.frameTimings.Count < 2)
                    {
                        xPosNomFrom.frame = 0;

                        var xPosNomEndFrame = new NomFrame();
                        xPosNomEndFrame.data = xPosNomFrom.data;
                        xPosNomEndFrame.frame = (byte)aqm.moHeader.endFrame;
                        xPosNomEndFrame.type2 = 8;
                        var yPosNomEndFrame = new NomFrame();
                        yPosNomEndFrame.data = yPosNomFrom.data;
                        yPosNomEndFrame.frame = (byte)aqm.moHeader.endFrame;
                        yPosNomEndFrame.type2 = 8;
                        var zPosNomEndFrame = new NomFrame();
                        zPosNomEndFrame.data = zPosNomFrom.data;
                        zPosNomEndFrame.frame = (byte)aqm.moHeader.endFrame;
                        zPosNomEndFrame.type2 = 8;

                        xPosKeys.Add(xPosNomFrom);
                        xPosKeys.Add(xPosNomEndFrame);
                        yPosKeys.Add(yPosNomFrom);
                        yPosKeys.Add(yPosNomEndFrame);
                        zPosKeys.Add(zPosNomFrom);
                        zPosKeys.Add(zPosNomEndFrame);
                        break;
                    }
                    else
                    {
                        xPosNomFrom.frame = (byte)(posFrames.frameTimings[f] / 0x10);
                        yPosNomFrom.frame = (byte)(posFrames.frameTimings[f] / 0x10);
                        zPosNomFrom.frame = (byte)(posFrames.frameTimings[f] / 0x10);
                    }
                    if (f == posFrames.vector4Keys.Count - 1)
                    {
                        xPosNomFrom.type2 = 8;
                        yPosNomFrom.type2 = 8;
                        zPosNomFrom.type2 = 8;
                    }

                    xPosKeys.Add(xPosNomFrom);
                    yPosKeys.Add(yPosNomFrom);
                    zPosKeys.Add(zPosNomFrom);
                }

                //Rotation
                //Precheck
                bool skipRot = true;
                var rot0 = rotFrames.vector4Keys[0];
                for (int f = 0; f < rotFrames.vector4Keys.Count; f++)
                {
                    var rot = rotFrames.vector4Keys[f];
                    if (rot0.Equals(rot))
                    {
                        skipRot = false;
                    }
                }

                for (int f = 0; f < rotFrames.vector4Keys.Count; f++)
                {
                    var rot = rotFrames.vector4Keys[f];
                    var rotNomFrame = new NomFrame();
                    rotNomFrame.data = new List<float>() { rot.X, rot.Y, rot.Z, rot.W };

                    //Handle lack of frameTimings
                    if (rotFrames.frameTimings == null || rotFrames.frameTimings.Count < 2)
                    {
                        rotNomFrame.frame = 0;

                        var rotEndFrame = new NomFrame();
                        rotEndFrame.data = rotNomFrame.data;
                        rotEndFrame.frame = (byte)aqm.moHeader.endFrame;
                        rotEndFrame.type2 = 8;

                        rotKeys.Add(rotNomFrame);
                        rotKeys.Add(rotEndFrame);
                        break;
                    }
                    else
                    {
                        rotNomFrame.frame = (byte)(rotFrames.frameTimings[f] / 0x10);
                    }
                    if (f == rotFrames.vector4Keys.Count - 1)
                    {
                        rotNomFrame.type2 = 8;
                    }

                    rotKeys.Add(rotNomFrame);
                }

                if (rotKeys.Count > 0 /*&& skipRot == false*/)
                {
                    rotationFrameList.Add(rotKeys);
                }
                else
                {
                    rotationFrameList.Add(null);
                }
                if (xPosKeys.Count > 0 && skipPosX == false)
                {
                    xPositionFrameList.Add(xPosKeys);
                }
                else
                {
                    xPositionFrameList.Add(null);
                }
                if (yPosKeys.Count > 0 && skipPosY == false)
                {
                    yPositionFrameList.Add(yPosKeys);
                }
                else
                {
                    yPositionFrameList.Add(null);
                }
                if (zPosKeys.Count > 0 && skipPosZ == false)
                {
                    zPositionFrameList.Add(zPosKeys);
                }
                else
                {
                    zPositionFrameList.Add(null);
                }
            }
        }

        public AquaMotion GetPSO2MotionPSUBody(AquaNode bones)
        {
            GetDefaultTransformsFromBones(bones, bones.nodeList.Count - 1);
            AquaMotion aqm = new AquaMotion();
            var posData = new List<(Vector3 data, int frame)>[rotationFrameList.Count];

            aqm.moHeader = new MOHeader();
            aqm.moHeader.frameSpeed = 30;
            aqm.moHeader.endFrame = frameCount;
            aqm.moHeader.unkInt0 = 2;
            aqm.moHeader.variant = 0x2;
            aqm.moHeader.testString.SetString("test");

            //Go through and add NGS nodes 
            for (int i = 0; i < rotationFrameList.Count; i++)
            {
                var keySet = new KeyData();
                keySet.mseg.nodeDataCount = 3;
                keySet.mseg.nodeId = i;
                keySet.mseg.nodeType = 2;
                keySet.mseg.nodeName = PSO2String.GeneratePSO2String(nodeNames[i]);

                var pos = new MKEY();
                var rot = new MKEY();
                var scale = new MKEY();

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

                posData[i] = GetPositionKeys(i);
            }

            for (int i = 0; i < rotationFrameList.Count; i++)
            {
                //Debug.WriteLine($"Bone {i}");
                //Positions
                var motionKey = aqm.motionKeys[i];
                var posKeys = motionKey.keyData[0];
                var posBone = posData[i];

                if (posBone != null && posBone.Count > 0)
                {
                    posKeys.keyCount = posBone.Count;
                    posKeys.vector4Keys.Clear();
                }

                for (int f = 0; f < posBone.Count; f++)
                {
                    var posFrame = posBone[f];

                    //Assign to pso2 bone
                    int flag = 0;
                    if (f == 0)
                    {
                        flag = 1;
                    }
                    else if (f == posBone.Count - 1)
                    {
                        flag = 2;
                    }
                    posKeys.vector4Keys.Add(new Vector4(posFrame.data, 0));
                    posKeys.frameTimings.Add((uint)((posFrame.frame * 0x10) + flag));
                }

                //Rotations
                if (rotationFrameList[i] == null)
                {
                    continue;
                }
                var rotKeys = motionKey.keyData[1];
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
                    if (f == 0)
                    {
                        flag = 1;
                    }
                    else if (f == rotBone.Count - 1)
                    {
                        flag = 2;
                    }

                    var euler = MathExtras.QuaternionToEuler(quat.ToQuat());
                    //Debug.WriteLine($"Frame ({rotFrame.frame}) Euler X {euler.X} Y {euler.Y} Z {euler.Z}  Quat {quat.X} {quat.Y} {quat.Z} {quat.W}");
                    rotKeys.vector4Keys.Add(quat);
                    rotKeys.frameTimings.Add((uint)((rotFrame.frame * 0x10) + flag));
                }
            }
            aqm.moHeader.nodeCount = aqm.motionKeys.Count;

            return aqm;
        }

        public AquaMotion GetPSO2Motion(AquaNode bones)
        {
            GetDefaultTransformsFromBones(bones);
            AquaMotion aqm = new AquaMotion();

            aqm.moHeader = new MOHeader();
            aqm.moHeader.frameSpeed = 30;
            aqm.moHeader.endFrame = frameCount;
            aqm.moHeader.unkInt0 = 2;
            aqm.moHeader.variant = 0x2;
            aqm.moHeader.testString.SetString("test");

            //Go through and add NGS nodes 
            for (int i = 0; i < endRange + 1; i++)
            {
                var keySet = new KeyData();
                keySet.mseg.nodeDataCount = 3;
                keySet.mseg.nodeId = i;
                keySet.mseg.nodeType = 2;
                keySet.mseg.nodeName = PSO2String.GeneratePSO2String(nodeNames[i]);

                var pos = new MKEY();
                var rot = new MKEY();
                var scale = new MKEY();

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
                var rotBone = rotationFrameList[i];
                Vector4 lastQuat = new Vector4();
                for (int f = 0; f < rotBone.Count; f++)
                {
                    var rotFrame = rotBone[f];
                    Vector4 quat;
                    switch (rotFrame.type)
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
