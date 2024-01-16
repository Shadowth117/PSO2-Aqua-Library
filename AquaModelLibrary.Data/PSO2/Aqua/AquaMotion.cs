using AquaModelLibrary.Data.PSO2.Aqua.AquaCommonData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaMotionData;
using AquaModelLibrary.Helpers;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.PSO2;
using AquaModelLibrary.Helpers.Readers;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    //Though the NIFL format is used for storage, VTBF format tag references for data will be commented where appropriate. Some offset/reserve related things are NIFL only, however.

    //Cameras, UV, and standard motions are essentially the same format.
    public unsafe class AquaMotion : AquaCommon
    {
        public MOHeader moHeader;
        public List<KeyData> motionKeys = new List<KeyData>();

        public override string[] GetEnvelopeTypes()
        {
            return new string[] {
            "aqm\0",
            "aqv\0",
            "aqw\0",
            "aqc\0",
            "trm\0",
            "trv\0",
            "trw\0"
            };
        }

        public AquaMotion() { }

        public AquaMotion(byte[] file) : base(file) { }

        public AquaMotion(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }

        public AquaMotion(byte[] file, string _ext)
        {
            Read(file, _ext);
        }

        public AquaMotion(BufferedStreamReaderBE<MemoryStream> sr, string _ext)
        {
            Read(sr, _ext);
        }

        #region ReadMethods
        public override void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            moHeader = sr.Read<MOHeader>();

            //Read MSEG data
            for (int i = 0; i < moHeader.nodeCount; i++)
            {
                KeyData data = new KeyData();
                data.mseg = sr.Read<MSEG>();
                motionKeys.Add(data);
            }

            //Read MKEY
            for (int i = 0; i < motionKeys.Count; i++)
            {
                sr.Seek(motionKeys[i].mseg.nodeOffset + offset, SeekOrigin.Begin);
                for (int j = 0; j < motionKeys[i].mseg.nodeDataCount; j++)
                {
                    MKEY mkey = new MKEY();
                    mkey.keyType = sr.Read<int>();
                    mkey.dataType = sr.Read<int>();
                    mkey.unkInt0 = sr.Read<int>();
                    mkey.keyCount = sr.Read<int>();
                    mkey.frameAddress = sr.Read<int>();
                    mkey.timeAddress = sr.Read<int>();
                    motionKeys[i].keyData.Add(mkey);
                }
                //Odd amounts of MKEYs will pad 8 bytes in NIFL

                //Loop through what was gathered and get the actual data
                for (int j = 0; j < motionKeys[i].mseg.nodeDataCount; j++)
                {
                    sr.Seek(motionKeys[i].keyData[j].timeAddress + offset, SeekOrigin.Begin);

                    if (motionKeys[i].keyData[j].keyCount > 1)
                    {
                        for (int m = 0; m < motionKeys[i].keyData[j].keyCount; m++)
                        {
                            if ((motionKeys[i].keyData[j].dataType & 0x80) > 0)
                            {
                                motionKeys[i].keyData[j].frameTimings.Add(sr.Read<uint>());
                            }
                            else
                            {
                                motionKeys[i].keyData[j].frameTimings.Add(sr.Read<ushort>());
                            }
                        }
                    }

                    //Stream aligns to 0x10 after timings.
                    sr.Seek(motionKeys[i].keyData[j].frameAddress + offset, SeekOrigin.Begin);

                    var dataType = motionKeys[i].keyData[j].dataType;
                    if ((dataType & 0x80) > 0)
                    {
                        dataType -= 0x80;
                    }
                    switch (dataType)
                    {
                        //0x1 and 0x3 are Vector4 arrays essentially. 0x1 is seemingly a Vector3 with alignment padding, but could potentially have things.
                        case 0x1:
                        case 0x2:
                        case 0x3:
                            for (int m = 0; m < motionKeys[i].keyData[j].keyCount; m++)
                            {
                                motionKeys[i].keyData[j].vector4Keys.Add(sr.Read<Vector4>());
                            }
                            break;

                        case 0x5:
                            for (int m = 0; m < motionKeys[i].keyData[j].keyCount; m++)
                            {
                                motionKeys[i].keyData[j].intKeys.Add(sr.Read<int>());
                            }
                            break;

                        //0x4 is texture/uv related, 0x6 is Camera related - Array of floats. 0x4 seems to be used for every .aqv frame set interestingly
                        case 0x4:
                        case 0x6:
                            for (int m = 0; m < motionKeys[i].keyData[j].keyCount; m++)
                            {
                                motionKeys[i].keyData[j].floatKeys.Add(sr.Read<float>());
                            }
                            break;
                        default:
                            Debug.WriteLine($"Unexpected (keytype {motionKeys[i].keyData[j].keyType.ToString("X")}) type {motionKeys[i].keyData[j].dataType.ToString("X")} at {sr.Position.ToString("X")}");
                            throw new Exception();
                    }
                    //Stream aligns to 0x10 again after frames.
                }
            }
        }

        public override void ReadVTBFFile(BufferedStreamReaderBE<MemoryStream> sr)
        {
            //Seek past vtbf tag
            sr.Seek(0x10, SeekOrigin.Current); //VTBF + AQGF tags

            while (sr.Position < sr.BaseStream.Length)
            {
                var data = VTBFMethods.ReadVTBFTag(sr, out string tagType, out int ptrCount, out int entryCount);
                switch (tagType)
                {
                    case "ROOT":
                        //We don't do anything with this right now.
                        break;
                    case "NDMO":
                        //Signifies a 3d motion
                        moHeader = new MOHeader(data);
                        break;
                    case "SPMO":
                        //Signifies a material animation
                        moHeader = new MOHeader(data);
                        break;
                    case "CAMO":
                        //Signifies a camera motion
                        moHeader = new MOHeader(data);
                        break;
                    case "MSEG":
                        //Motion segment - Signifies the start a node's animation data
                        motionKeys.Add(new KeyData());
                        motionKeys[motionKeys.Count - 1].mseg = new MSEG(data);
                        break;
                    case "MKEY":
                        //Motion key - These contain frame data for the various animation types and always follow the MSEG for the node they apply to.
                        motionKeys[motionKeys.Count - 1].keyData.Add(new MKEY(data));
                        break;
                    default:
                        //Should mean it's done.
                        Debug.WriteLine($"Defaulted tag was: {tagType}");
                        break;
                }
            }
        }
        #endregion

        #region WriteMethods

        public override byte[] GetBytesNIFL()
        {
            int rel0SizeOffset = 0;
            int boneTableOffset = 0;

            List<int> boneOffAddresses = new List<int>();

            List<byte> outBytes = new List<byte>();
            List<int> nof0PointerLocations = new List<int>(); //Used for the NOF0 section

            //REL0
            outBytes.AddRange(Encoding.UTF8.GetBytes("REL0"));
            rel0SizeOffset = outBytes.Count; //We'll fill this later
            outBytes.AddRange(BitConverter.GetBytes(0));
            outBytes.AddRange(BitConverter.GetBytes(0x10));
            outBytes.AddRange(BitConverter.GetBytes(0));

            //MoHeader
            outBytes.AddRange(BitConverter.GetBytes(moHeader.variant));
            outBytes.AddRange(BitConverter.GetBytes(moHeader.loopPoint));
            outBytes.AddRange(BitConverter.GetBytes(moHeader.endFrame));
            outBytes.AddRange(BitConverter.GetBytes(moHeader.frameSpeed));

            outBytes.AddRange(BitConverter.GetBytes((int)0x2));
            outBytes.AddRange(BitConverter.GetBytes(moHeader.nodeCount));
            boneTableOffset = DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count);
            outBytes.AddRange(BitConverter.GetBytes((int)0x50));
            outBytes.AddRange(moHeader.testString.GetBytes());

            //Padding
            outBytes.AddRange(BitConverter.GetBytes((int)0x0));

            //Bonelist
            for (int i = 0; i < motionKeys.Count; i++)
            {
                boneOffAddresses.Add(DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0x8));
                outBytes.AddRange(DataHelpers.ConvertStruct(motionKeys[i].mseg));
            }

            //BoneAnims
            for (int i = 0; i < motionKeys.Count; i++)
            {
                int[] dataOffsets = new int[motionKeys[i].keyData.Count];
                int[] timeOffsets = new int[motionKeys[i].keyData.Count];

                outBytes.SetByteListInt(boneOffAddresses[i], outBytes.Count);
                //Write keyset info
                for (int keySet = 0; keySet < motionKeys[i].keyData.Count; keySet++)
                {
                    dataOffsets[keySet] = DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0x10);
                    if (motionKeys[i].keyData[keySet].keyCount > 1)
                    {
                        timeOffsets[keySet] = DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0x14);
                    }
                    if (moHeader.endFrame > MotionConstants.UshortThreshold)
                    {
                        motionKeys[i].keyData[keySet].dataType |= 0x80;
                    }
                    outBytes.AddRange(BitConverter.GetBytes(motionKeys[i].keyData[keySet].keyType));
                    outBytes.AddRange(BitConverter.GetBytes(motionKeys[i].keyData[keySet].dataType));
                    outBytes.AddRange(BitConverter.GetBytes(motionKeys[i].keyData[keySet].unkInt0));
                    outBytes.AddRange(BitConverter.GetBytes(motionKeys[i].keyData[keySet].keyCount));

                    outBytes.AddRange(BitConverter.GetBytes((int)0));
                    outBytes.AddRange(BitConverter.GetBytes((int)0));
                }
                outBytes.AlignWriter(0x10);

                //Write timings and frame data
                for (int keySet = 0; keySet < motionKeys[i].keyData.Count; keySet++)
                {
                    if (motionKeys[i].keyData[keySet].keyCount > 1)
                    {
                        outBytes.SetByteListInt(timeOffsets[keySet], outBytes.Count);
                        for (int time = 0; time < motionKeys[i].keyData[keySet].keyCount; time++)
                        {
                            //Beginning time should internally be 0x1 while ending time should have a 2 in the ones decimal place
                            if (moHeader.endFrame > MotionConstants.UshortThreshold)
                            {
                                outBytes.AddRange(BitConverter.GetBytes(motionKeys[i].keyData[keySet].frameTimings[time]));
                            }
                            else
                            {
                                outBytes.AddRange(BitConverter.GetBytes((ushort)motionKeys[i].keyData[keySet].frameTimings[time]));
                            }
                        }
                        outBytes.AlignWriter(0x10);
                    }

                    outBytes.SetByteListInt(dataOffsets[keySet], outBytes.Count);
                    for (int data = 0; data < motionKeys[i].keyData[keySet].keyCount; data++)
                    {
                        var dataType = motionKeys[i].keyData[keySet].dataType;
                        if ((dataType & 0x80) > 0)
                        {
                            dataType -= 0x80;
                        }
                        switch (dataType)
                        {
                            //0x1, 0x2, and 0x3 are Vector4 arrays essentially. 0x1 is seemingly a Vector3 with alignment padding, but could potentially have things.
                            case 0x1:
                            case 0x2:
                            case 0x3:
                                outBytes.AddRange(DataHelpers.ConvertStruct(motionKeys[i].keyData[keySet].vector4Keys[data]));
                                break;

                            case 0x5:
                                outBytes.AddRange(DataHelpers.ConvertStruct(motionKeys[i].keyData[keySet].intKeys[data]));
                                break;

                            //0x4 is texture/uv related, 0x6 is Camera related - Array of floats. 0x4 seems to be used for every .aqv frame set interestingly
                            case 0x4:
                            case 0x6:
                                outBytes.AddRange(DataHelpers.ConvertStruct(motionKeys[i].keyData[keySet].floatKeys[data]));
                                break;
                            default:
                                throw new Exception($"Unexpected data type {motionKeys[i].keyData[keySet].dataType}!");
                        }
                    }
                    outBytes.AlignWriter(0x10);
                }
            }

            //Write REL0 Size
            outBytes.SetByteListInt(rel0SizeOffset, outBytes.Count - 0x8);

            //NOF0
            int NOF0Offset = outBytes.Count;
            int NOF0Size = (nof0PointerLocations.Count + 2) * 4;
            int NOF0FullSize = NOF0Size + 0x8;
            outBytes.AddRange(Encoding.UTF8.GetBytes("NOF0"));
            outBytes.AddRange(BitConverter.GetBytes(NOF0Size));
            outBytes.AddRange(BitConverter.GetBytes(nof0PointerLocations.Count));
            outBytes.AddRange(BitConverter.GetBytes(0));

            //Write pointer offsets
            for (int i = 0; i < nof0PointerLocations.Count; i++)
            {
                outBytes.AddRange(BitConverter.GetBytes(nof0PointerLocations[i]));
            }
            NOF0FullSize += outBytes.AlignWriter(0x10);

            //NEND
            outBytes.AddRange(Encoding.UTF8.GetBytes("NEND"));
            outBytes.AddRange(BitConverter.GetBytes(0x8));
            outBytes.AddRange(BitConverter.GetBytes(0));
            outBytes.AddRange(BitConverter.GetBytes(0));

            //Generate NIFL
            NIFL nifl = new NIFL();
            nifl.magic = BitConverter.ToInt32(Encoding.UTF8.GetBytes("NIFL"), 0);
            nifl.NIFLLength = 0x18;
            nifl.unkInt0 = 1;
            nifl.offsetAddition = 0x20;

            nifl.NOF0Offset = NOF0Offset;
            nifl.NOF0OffsetFull = NOF0Offset + 0x20;
            nifl.NOF0BlockSize = NOF0FullSize;
            nifl.padding0 = 0;

            //Write NIFL
            outBytes.InsertRange(0, DataHelpers.ConvertStruct(nifl));

            return outBytes.ToArray();
        }

        public override byte[] GetBytesVTBF()
        {
            List<byte> outBytes = new List<byte>();
            outBytes.AddRange(VTBFMethods.ToAQGFVTBF());
            outBytes.AddRange(VTBFMethods.ToROOT());
            switch (moHeader.variant)
            {
                case MotionConstants.stdAnim:
                case MotionConstants.stdPlayerAnim:
                    outBytes.AddRange(moHeader.GetBytesVTBF("NDMO"));
                    break;
                case MotionConstants.materialAnim:
                    outBytes.AddRange(moHeader.GetBytesVTBF("SPMO"));
                    break;
                case MotionConstants.cameraAnim:
                    outBytes.AddRange(moHeader.GetBytesVTBF("CAMO"));
                    break;
            }
            for (int mseg = 0; mseg < motionKeys.Count; mseg++)
            {
                outBytes.AddRange(motionKeys[mseg].mseg.GetBytesVTBF());
                for (int keys = 0; keys < motionKeys[mseg].keyData.Count; keys++)
                {
                    outBytes.AddRange(motionKeys[mseg].keyData[keys].GetBytesVTBF());
                }
            }

            return outBytes.ToArray();
        }

        #endregion

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
            for (int i = (boneCount - 1); i > 0; i--)
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

                    if (nodeScale != null)
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
