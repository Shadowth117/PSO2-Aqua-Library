using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

//Addapted from SA Tools
namespace AquaModelLibrary.Data.Ninja.Motion
{
    public class NJSMotion
    {
        public enum MotionWriteMode
        {
            Standard = 0,
            ShortRot = 1,
            BillyMode = 2,
        }

        public static List<AnimFlags> GetAnimFlagListFromEnum(AnimFlags animType, bool BillyMode)
        {
            List<AnimFlags> animFlagList = new List<AnimFlags>();
            if (BillyMode)
            {
                if ((animType & AnimFlags.Position) > 0)
                {
                    animFlagList.Add(AnimFlags.Position);
                }
                if ((animType & AnimFlags.Rotation) > 0)
                {
                    animFlagList.Add(AnimFlags.Rotation);
                }
                //Normal is the shortrot rotation type in Billy
                if ((animType & AnimFlags.Normal) > 0)
                {
                    animFlagList.Add(AnimFlags.Rotation);
                }
                if ((animType & AnimFlags.Quaternion) > 0)
                {
                    animFlagList.Add(AnimFlags.Quaternion);
                }
                if ((animType & AnimFlags.Scale) > 0)
                {
                    animFlagList.Add(AnimFlags.Scale);
                }
                if ((animType & AnimFlags.Vector) > 0)
                {
                    animFlagList.Add(AnimFlags.Vector);
                }
                if ((animType & AnimFlags.Vertex) > 0)
                {
                    animFlagList.Add(AnimFlags.Vertex);
                }
                if ((animType & AnimFlags.Target) > 0)
                {
                    animFlagList.Add(AnimFlags.Target);
                }
                if ((animType & AnimFlags.Roll) > 0)
                {
                    animFlagList.Add(AnimFlags.Roll);
                }
                if ((animType & AnimFlags.Angle) > 0)
                {
                    animFlagList.Add(AnimFlags.Angle);
                }
                if ((animType & AnimFlags.Color) > 0)
                {
                    animFlagList.Add(AnimFlags.Color);
                }
                if ((animType & AnimFlags.Intensity) > 0)
                {
                    animFlagList.Add(AnimFlags.Intensity);
                }
                if ((animType & AnimFlags.Spot) > 0)
                {
                    animFlagList.Add(AnimFlags.Spot);
                }
                if ((animType & AnimFlags.Point) > 0)
                {
                    animFlagList.Add(AnimFlags.Point);
                }
            }
            else
            {
                //Default just has these in order, seemingly
                foreach (var flag in Enum.GetValues(typeof(AnimFlags)))
                {
                    var enFlag = (AnimFlags)flag;
                    if ((animType & enFlag) > 0)
                    {
                        animFlagList.Add(enFlag);
                    }
                }
            }

            return animFlagList;
        }

        public bool BillyMode = false;
        public int frameCount = 0;
        public AnimFlags animType = 0;
        public InterpolationMode interpoMode = 0;
        public List<AnimModelData> KeyDataList = new List<AnimModelData>();

        public NJSMotion() { }
        public NJSMotion(byte[] file, bool billyMode, int offset = 0, bool shortRot = false, int nodeCount = 0)
        {
            BillyMode = billyMode;
            Read(file, offset, shortRot, nodeCount);
        }

        public NJSMotion(BufferedStreamReaderBE<MemoryStream> sr, bool billyMode, int offset = 0, bool shortRot = false, int nodeCount = 0)
        {
            BillyMode = billyMode;
            Read(sr, offset, shortRot, nodeCount);
        }

        public NJSMotion(byte[] file, int offset = 0, bool shortRot = false, int nodeCount = 0)
        {
            Read(file, offset, shortRot, nodeCount);
        }

        public NJSMotion(BufferedStreamReaderBE<MemoryStream> sr, int offset = 0, bool shortRot = false, int nodeCount = 0)
        {
            Read(sr, offset, shortRot, nodeCount);
        }

        public byte[] GetBytesNJM(bool writeBE, MotionWriteMode writeMode)
        {
            ByteListExtension.AddAsBigEndian = writeBE;
            List<byte> outBytes = new List<byte>();
            List<int> pofSets = new List<int>();

            Write(outBytes, pofSets, writeMode);

            List<byte> headerBytes = new List<byte> { 0x4E, 0x4D, 0x44, 0x4D };
            headerBytes.AddRange(BitConverter.GetBytes(outBytes.Count));
            outBytes.InsertRange(0, headerBytes);
            outBytes.AddRange(POF0.GeneratePOF0(pofSets));

            return outBytes.ToArray();
        }

        public void Write(List<byte> outBytes, List<int> POF0Offsets, MotionWriteMode writeMode)
        {
            POF0Offsets.Add(outBytes.Count);
            outBytes.ReserveInt("MotionStart");
            outBytes.AddValue(frameCount);

            AnimFlags flags = new AnimFlags();
            List<AnimFlags> keyDataAnimFlagsList = new List<AnimFlags>();
            foreach (var data in KeyDataList)
            {
                var dataFlags = data.GetAnimFlags(writeMode == MotionWriteMode.BillyMode);
                flags |= dataFlags;
                keyDataAnimFlagsList.Add(dataFlags);
            }

            ushort dataCount = 0;
            foreach (AnimFlags flag in Enum.GetValues(typeof(AnimFlags)))
            {
                if ((flag & flags) > 0)
                {
                    dataCount++;
                }
            }

            outBytes.AddValue((ushort)flags);
            outBytes.AddValue((ushort)((ushort)interpoMode | dataCount));

            var dataTypeList = GetAnimFlagListFromEnum(flags, writeMode == MotionWriteMode.BillyMode);
            outBytes.FillInt("MotionStart", outBytes.Count);
            for (int i = 0; i < KeyDataList.Count; i++)
            {
                var data = KeyDataList[i];
                outBytes.AlignWriter(0x4);
                //Reserve frame set offsets
                for (int j = 0; j < dataTypeList.Count; j++)
                {
                    outBytes.ReserveInt($"MotionOffset_{i}_{j}");
                }

                //Write frame set counts
                for (int j = 0; j < dataTypeList.Count; j++)
                {
                    var type = dataTypeList[j];
                    switch (type)
                    {
                        case AnimFlags.Position:
                            outBytes.AddValue(data.Position.Count);
                            break;
                        case AnimFlags.Rotation:
                            outBytes.AddValue(data.RotationData.Count);
                            break;
                        case AnimFlags.Scale:
                            outBytes.AddValue(data.Scale.Count);
                            break;
                        case AnimFlags.Vector:
                            outBytes.AddValue(data.Vector.Count);
                            break;
                        case AnimFlags.Vertex:
                            outBytes.AddValue(data.Vertex.Count);
                            break;
                        case AnimFlags.Normal:
                            if (writeMode == MotionWriteMode.BillyMode)
                            {
                                outBytes.AddValue(data.RotationData.Count);
                            }
                            else
                            {
                                outBytes.AddValue(data.Normal.Count);
                            }
                            break;
                        case AnimFlags.Target:
                            outBytes.AddValue(data.Target.Count);
                            break;
                        case AnimFlags.Roll:
                            outBytes.AddValue(data.Roll.Count);
                            break;
                        case AnimFlags.Angle:
                            outBytes.AddValue(data.Angle.Count);
                            break;
                        case AnimFlags.Color:
                            outBytes.AddValue(data.Color.Count);
                            break;
                        case AnimFlags.Intensity:
                            outBytes.AddValue(data.Intensity.Count);
                            break;
                        case AnimFlags.Spot:
                            outBytes.AddValue(data.Spot.Count);
                            break;
                        case AnimFlags.Point:
                            outBytes.AddValue(data.Point.Count);
                            break;
                        case AnimFlags.Quaternion:
                            outBytes.AddValue(data.Quaternion.Count);
                            break;
                    }
                }
            }

            for (int i = 0; i < KeyDataList.Count; i++)
            {
                var data = KeyDataList[i];
                outBytes.AlignWriter(0x4);
                //Write frame sets, fill reserved offsets if used
                for (int j = 0; j < dataTypeList.Count; j++)
                {
                    var type = dataTypeList[j];
                    switch (type)
                    {
                        case AnimFlags.Position:
                            if(data.Position.Count > 0)
                            {
                                POF0Offsets.Add(outBytes.FillInt($"MotionOffset_{i}_{j}", outBytes.Count));
                            }
                            foreach(var set in data.Position)
                            {
                                outBytes.AddValue(set.Key);
                                outBytes.AddValue(set.Value.X);
                                outBytes.AddValue(set.Value.Y);
                                outBytes.AddValue(set.Value.Z);
                            }
                            break;
                        case AnimFlags.Rotation:
                            if (data.RotationData.Count > 0)
                            {
                                POF0Offsets.Add(outBytes.FillInt($"MotionOffset_{i}_{j}", outBytes.Count));
                            }
                            foreach (var set in data.RotationData)
                            {
                                switch(writeMode)
                                {
                                    case MotionWriteMode.Standard:
                                        outBytes.AddValue(set.Key);
                                        outBytes.AddValue(set.Value.X);
                                        outBytes.AddValue(set.Value.Y);
                                        outBytes.AddValue(set.Value.Z);
                                        break;
                                    case MotionWriteMode.ShortRot:
                                    case MotionWriteMode.BillyMode:
                                        //Check origianl flags and use normal rotation ints if needed
                                        if((flags & AnimFlags.Rotation) > 0)
                                        {
                                            outBytes.AddValue(set.Key);
                                            outBytes.AddValue(set.Value.X);
                                            outBytes.AddValue(set.Value.Y);
                                            outBytes.AddValue(set.Value.Z);
                                        } else
                                        {
                                            outBytes.AddValue((short)set.Key);
                                            outBytes.AddValue((short)set.Value.X);
                                            outBytes.AddValue((short)set.Value.Y);
                                            outBytes.AddValue((short)set.Value.Z);
                                        }
                                        break;
                                }
                            }
                            break;
                        case AnimFlags.Scale:
                            if (data.Scale.Count > 0)
                            {
                                POF0Offsets.Add(outBytes.FillInt($"MotionOffset_{i}_{j}", outBytes.Count));
                            }
                            foreach (var set in data.Scale)
                            {
                                outBytes.AddValue(set.Key);
                                outBytes.AddValue(set.Value.X);
                                outBytes.AddValue(set.Value.Y);
                                outBytes.AddValue(set.Value.Z);
                            }
                            break;
                        case AnimFlags.Vector:
                            if (data.Vector.Count > 0)
                            {
                                POF0Offsets.Add(outBytes.FillInt($"MotionOffset_{i}_{j}", outBytes.Count));
                            }
                            foreach (var set in data.Vector)
                            {
                                outBytes.AddValue(set.Key);
                                outBytes.AddValue(set.Value.X);
                                outBytes.AddValue(set.Value.Y);
                                outBytes.AddValue(set.Value.Z);
                            }
                            break;
                        case AnimFlags.Vertex:
                            if (data.Vertex.Count > 0)
                            {
                                POF0Offsets.Add(outBytes.FillInt($"MotionOffset_{i}_{j}", outBytes.Count));
                            }
                            foreach (var set in data.Vertex)
                            {
                                outBytes.AddValue(set.Key);
                                POF0Offsets.Add(outBytes.Count);
                                outBytes.ReserveInt($"VertSetOffset_{i}_{j}");
                            }
                            foreach (var set in data.Vertex)
                            {
                                outBytes.FillInt($"VertSetOffset_{i}_{j}", outBytes.Count);
                                foreach(var setValue in set.Value)
                                {
                                    outBytes.AddValue(setValue.X);
                                    outBytes.AddValue(setValue.Y);
                                    outBytes.AddValue(setValue.Z);
                                }
                            }
                            break;
                        case AnimFlags.Normal: //Billy stuff should already be handled as rotation data so we just write the Normal flag data here
                            if (data.Normal.Count > 0)
                            {
                                POF0Offsets.Add(outBytes.FillInt($"MotionOffset_{i}_{j}", outBytes.Count));
                            }
                            foreach (var set in data.Normal)
                            {
                                outBytes.AddValue(set.Key);
                                POF0Offsets.Add(outBytes.Count);
                                outBytes.ReserveInt($"NrmSetOffset_{i}_{j}");
                            }
                            foreach (var set in data.Normal)
                            {
                                outBytes.FillInt($"NrmSetOffset_{i}_{j}", outBytes.Count);
                                foreach (var setValue in set.Value)
                                {
                                    outBytes.AddValue(setValue.X);
                                    outBytes.AddValue(setValue.Y);
                                    outBytes.AddValue(setValue.Z);
                                }
                            }
                            break;
                        case AnimFlags.Target:
                            if (data.Target.Count > 0)
                            {
                                POF0Offsets.Add(outBytes.FillInt($"MotionOffset_{i}_{j}", outBytes.Count));
                            }
                            foreach (var set in data.Target)
                            {
                                outBytes.AddValue(set.Key);
                                outBytes.AddValue(set.Value.X);
                                outBytes.AddValue(set.Value.Y);
                                outBytes.AddValue(set.Value.Z);
                            }
                            break;
                        case AnimFlags.Roll:
                            if (data.Roll.Count > 0)
                            {
                                POF0Offsets.Add(outBytes.FillInt($"MotionOffset_{i}_{j}", outBytes.Count));
                            }
                            foreach (var set in data.Roll)
                            {
                                outBytes.AddValue(set.Key);
                                outBytes.AddValue(set.Value);
                            }
                            break;
                        case AnimFlags.Angle:
                            if (data.Angle.Count > 0)
                            {
                                POF0Offsets.Add(outBytes.FillInt($"MotionOffset_{i}_{j}", outBytes.Count));
                            }
                            foreach (var set in data.Angle)
                            {
                                outBytes.AddValue(set.Key);
                                outBytes.AddValue(set.Value);
                            }
                            break;
                        case AnimFlags.Color:
                            if (data.Color.Count > 0)
                            {
                                POF0Offsets.Add(outBytes.FillInt($"MotionOffset_{i}_{j}", outBytes.Count));
                            }
                            foreach (var set in data.Color)
                            {
                                outBytes.AddValue(set.Key);
                                outBytes.AddValue(set.Value);
                            }
                            break;
                        case AnimFlags.Intensity:
                            if (data.Intensity.Count > 0)
                            {
                                POF0Offsets.Add(outBytes.FillInt($"MotionOffset_{i}_{j}", outBytes.Count));
                            }
                            foreach (var set in data.Intensity)
                            {
                                outBytes.AddValue(set.Key);
                                outBytes.AddValue(set.Value);
                            }
                            break;
                        case AnimFlags.Spot:
                            if (data.Spot.Count > 0)
                            {
                                POF0Offsets.Add(outBytes.FillInt($"MotionOffset_{i}_{j}", outBytes.Count));
                            }
                            foreach (var set in data.Spot)
                            {
                                outBytes.AddValue(set.Key);
                                outBytes.AddRange(set.Value.GetBytes(ByteListExtension.AddAsBigEndian));
                            }
                            break;
                        case AnimFlags.Point:
                            if (data.Point.Count > 0)
                            {
                                POF0Offsets.Add(outBytes.FillInt($"MotionOffset_{i}_{j}", outBytes.Count));
                            }
                            foreach (var set in data.Point)
                            {
                                outBytes.AddValue(set.Key);
                                outBytes.AddValue(set.Value.X);
                                outBytes.AddValue(set.Value.Y);
                            }
                            break;
                        case AnimFlags.Quaternion:
                            if (data.Quaternion.Count > 0)
                            {
                                POF0Offsets.Add(outBytes.FillInt($"MotionOffset_{i}_{j}", outBytes.Count));
                            }
                            foreach (var set in data.Quaternion)
                            {
                                outBytes.AddValue(set.Key);
                                outBytes.AddValue(set.Value[0]);
                                outBytes.AddValue(set.Value[1]);
                                outBytes.AddValue(set.Value[2]);
                                outBytes.AddValue(set.Value[3]);
                            }
                            break;
                    }
                }
            }
            outBytes.AlignWriter(0x4);
        }

        public void Read(byte[] file, int offset = 0, bool shortRot = false, int nodeCount = 0)
        {
            using (var ms = new MemoryStream(file))
            using (var sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr, offset, shortRot, nodeCount);
            }
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr, int offset = 0, bool shortRot = false, int nodeCount = 0)
        {
            bool shortRotCheck = true;
            nodeCount = nodeCount == 0 ? CalculateNodeCount(sr, offset) : nodeCount;

            var initialPointer = sr.ReadBE<int>();
            frameCount = sr.ReadBE<int>();
            animType = sr.ReadBE<AnimFlags>();

            List<AnimFlags> animFlagList = GetAnimFlagListFromEnum(animType, BillyMode);
            if (BillyMode)
            {
                shortRot = true;
                shortRotCheck = false;
            }

            var interpolationFrameSizeCombo = sr.ReadBE<ushort>();
            switch ((NJD_MTYPE_FN)interpolationFrameSizeCombo & NJD_MTYPE_FN.NJD_MTYPE_MASK)
            {
                case NJD_MTYPE_FN.NJD_MTYPE_LINER:
                    interpoMode = InterpolationMode.Linear;
                    break;
                case NJD_MTYPE_FN.NJD_MTYPE_SPLINE:
                    interpoMode = InterpolationMode.Spline;
                    break;
                case NJD_MTYPE_FN.NJD_MTYPE_USER:
                    interpoMode = InterpolationMode.User;
                    break;
            }
            int frameSize = (interpolationFrameSizeCombo & 0xF) * 8;

            sr.Seek(initialPointer + offset, SeekOrigin.Begin);
            var dataStart = sr.Position;

            /// Ninja motions do not provide a natural way of finding if rotations use shorts in Sonic Adventure 2/Battle and can feature both types of rotation within the same game so we brute force a bit if that goes wrong.
            if (ReadAnimData(sr, offset, animFlagList, nodeCount, shortRot, shortRotCheck) == false)
            {
                KeyDataList.Clear();
                sr.Seek(dataStart, SeekOrigin.Begin);
                ReadAnimData(sr, offset, animFlagList, nodeCount, true, false);
            }
        }

        /// <summary>
        /// Reads anim frame data. Returns false if rotation anim data seems to be out of range. 
        /// </summary>
        private bool ReadAnimData(BufferedStreamReaderBE<MemoryStream> sr, int offset, List<AnimFlags> animFlagList, int nodeCount, bool shortRot, bool shortRotCheck)
        {
            for (int i = 0; i < nodeCount; i++)
            {
                AnimModelData data = new AnimModelData();

                //Read frameData offsets and counts
                List<uint> offsets = new List<uint>();
                List<int> counts = new List<int>();
                foreach (var flag in animFlagList)
                {
                    offsets.Add(sr.ReadBE<uint>());
                }
                foreach (var flag in animFlagList)
                {
                    counts.Add(sr.ReadBE<int>());
                }
                var bookmark = sr.Position;

                //Handle based on anim flag order
                for (int f = 0; f < animFlagList.Count; f++)
                {
                    var framesOffset = offsets[f];
                    if (framesOffset != 0)
                    {
                        int vtxcount = -1;
                        sr.Seek(framesOffset + offset, SeekOrigin.Begin);
                        switch (animFlagList[f])
                        {
                            case AnimFlags.Position:
                                for (int j = 0; j < counts[f]; j++)
                                {
                                    data.Position.Add(sr.ReadBE<int>(), sr.ReadBEV3());
                                }
                                break;
                            case AnimFlags.Rotation:
                                if (shortRot == false && shortRotCheck == true)
                                {
                                    var shortBookmark = sr.Position;
                                    // Check if the animation uses short rotation or not
                                    for (int j = 0; j < counts[f]; j++)
                                    {
                                        // If any of the rotation frames go outside the file, assume it uses shorts
                                        if (sr.Position + 4 + 12 > sr.BaseStream.Length)
                                        {
                                            return false;
                                        }
                                        // If any of the rotation frames isn't in the range from -65535 to 65535, assume it uses shorts
                                        Rotation rot = new Rotation(sr);
                                        if (rot.X > 65535 || rot.X < -65535 ||
                                            rot.Y > 65535 || rot.Y < -65535 ||
                                            rot.Z > 65535 || rot.Z < -65535)
                                        {
                                            return false;
                                        }
                                    }
                                    sr.Seek(shortBookmark, SeekOrigin.Begin);
                                }
                                // Read rotation values
                                for (int j = 0; j < counts[f]; j++)
                                {
                                    if (shortRot)
                                    {
                                        var frame = sr.ReadBE<short>();
                                        if (!data.RotationData.ContainsKey(frame))
                                            data.RotationData.Add(frame, new Rotation(sr.ReadBE<short>(), sr.ReadBE<short>(), sr.ReadBE<short>()));
                                    }
                                    else
                                    {
                                        var frame = sr.ReadBE<int>();
                                        if (!data.RotationData.ContainsKey(frame))
                                            data.RotationData.Add(frame, new Rotation(sr));
                                    }
                                }
                                break;
                            case AnimFlags.Scale:
                                for (int j = 0; j < counts[f]; j++)
                                {
                                    data.Scale.Add(sr.ReadBE<int>(), sr.ReadBEV3());
                                }
                                break;
                            case AnimFlags.Vector:
                                for (int j = 0; j < counts[f]; j++)
                                {
                                    data.Vector.Add(sr.ReadBE<int>(), sr.ReadBEV3());
                                }
                                break;
                            case AnimFlags.Vertex:
                                vtxcount = vtxcount < 0 ? GetVertexCount(sr, framesOffset, counts[f]) : vtxcount;

                                sr.Seek(framesOffset + offset, SeekOrigin.Begin);
                                for (int j = 0; j < counts[f]; j++)
                                {
                                    Vector3[] verts = new Vector3[vtxcount];
                                    int frame = sr.ReadBE<int>();
                                    int vertSetOffset = sr.ReadBE<int>();

                                    var vertSetBookmark = sr.Position;
                                    sr.Seek(vertSetOffset + offset, SeekOrigin.Begin);
                                    for (int k = 0; k < verts.Length; k++)
                                    {
                                        verts[k] = sr.ReadBEV3();
                                    }

                                    if (!data.Vertex.ContainsKey(frame))
                                    {
                                        data.Vertex.Add(frame, verts);
                                    }
                                    sr.Seek(vertSetBookmark, SeekOrigin.Begin);
                                }
                                break;
                            case AnimFlags.Normal:
                                vtxcount = vtxcount < 0 ? GetVertexCount(sr, framesOffset, counts[f]) : vtxcount;

                                sr.Seek(framesOffset + offset, SeekOrigin.Begin);
                                for (int j = 0; j < counts[f]; j++)
                                {
                                    Vector3[] verts = new Vector3[vtxcount];
                                    int frame = sr.ReadBE<int>();
                                    int vertSetOffset = sr.ReadBE<int>();

                                    var vertSetBookmark = sr.Position;
                                    sr.Seek(vertSetOffset + offset, SeekOrigin.Begin);
                                    for (int k = 0; k < verts.Length; k++)
                                    {
                                        verts[k] = sr.ReadBEV3();
                                    }

                                    if (!data.Normal.ContainsKey(frame))
                                    {
                                        data.Normal.Add(frame, verts);
                                    }
                                    sr.Seek(vertSetBookmark, SeekOrigin.Begin);
                                }
                                break;
                            case AnimFlags.Target:
                                for (int j = 0; j < counts[f]; j++)
                                {
                                    data.Target.Add(sr.ReadBE<int>(), sr.ReadBEV3());
                                }
                                break;
                            case AnimFlags.Roll:
                                for (int j = 0; j < counts[f]; j++)
                                {
                                    data.Roll.Add(sr.ReadBE<int>(), sr.ReadBE<int>());
                                }
                                break;
                            case AnimFlags.Angle:
                                for (int j = 0; j < counts[f]; j++)
                                {
                                    data.Angle.Add(sr.ReadBE<int>(), sr.ReadBE<int>());
                                }
                                break;
                            case AnimFlags.Color:
                                for (int j = 0; j < counts[f]; j++)
                                {
                                    data.Color.Add(sr.ReadBE<int>(), sr.ReadBE<uint>());
                                }
                                break;
                            case AnimFlags.Intensity:
                                for (int j = 0; j < counts[f]; j++)
                                {
                                    data.Intensity.Add(sr.ReadBE<int>(), sr.ReadBE<float>());
                                }
                                break;
                            case AnimFlags.Spot:
                                for (int j = 0; j < counts[f]; j++)
                                {
                                    data.Spot.Add(sr.ReadBE<int>(), new Spotlight(sr));
                                }
                                break;
                            case AnimFlags.Point:
                                for (int j = 0; j < counts[f]; j++)
                                {
                                    data.Point.Add(sr.ReadBE<int>(), sr.ReadBEV2());
                                }
                                break;
                            case AnimFlags.Quaternion:
                                for (int j = 0; j < counts[f]; j++)
                                {
                                    //WXYZ order
                                    data.Quaternion.Add(sr.ReadBE<int>(), new float[] { sr.ReadBE<float>(), sr.ReadBE<float>(), sr.ReadBE<float>(), sr.ReadBE<float>() });
                                }
                                break;
                        }
                    }
                }
                sr.Seek(bookmark, SeekOrigin.Begin);

                KeyDataList.Add(data);
            }

            return true;
        }

        private static int GetVertexCount(BufferedStreamReaderBE<MemoryStream> sr, uint vertoff, int frames)
        {
            int vtxcount;
            //Gather pointer data to dynamically determine vert count. Normally you should have the model's data here to do this, but we're going without.
            List<int> ptrs = new List<int>();
            for (int j = 0; j < frames; j++)
            {
                var frame = sr.ReadBE<int>();
                var ptr = sr.ReadBE<int>();
                if (!ptrs.Contains(ptr))
                {
                    ptrs.Add(ptr);
                }
            }

            //Determine vert count
            if (ptrs.Count > 1)
            {
                ptrs.Sort();
                vtxcount = (ptrs[1] - ptrs[0]) / 0xC;
            }
            else
            {
                vtxcount = ((int)vertoff - ptrs[0]) / 0xC;
            }

            return vtxcount;
        }

        /// <summary>
        /// This should be called while the streamreader is at the start of the motion data.
        /// </summary>
        public int CalculateNodeCount(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            var bookmark = sr.Position;
            int mdatap = sr.ReadBE<int>();
            sr.ReadBE<int>();
            AnimFlags animtype = sr.ReadBE<AnimFlags>();
            if (animtype == 0) return 0;
            int mdata = 0;
            if (animtype.HasFlag(AnimFlags.Position)) mdata++;
            if (animtype.HasFlag(AnimFlags.Rotation)) mdata++;
            if (animtype.HasFlag(AnimFlags.Scale)) mdata++;
            if (animtype.HasFlag(AnimFlags.Vector)) mdata++;
            if (animtype.HasFlag(AnimFlags.Vertex)) mdata++;
            if (animtype.HasFlag(AnimFlags.Normal)) mdata++;
            if (animtype.HasFlag(AnimFlags.Color)) mdata++;
            if (animtype.HasFlag(AnimFlags.Intensity)) mdata++;
            if (animtype.HasFlag(AnimFlags.Target)) mdata++;
            if (animtype.HasFlag(AnimFlags.Spot)) mdata++;
            if (animtype.HasFlag(AnimFlags.Point)) mdata++;
            if (animtype.HasFlag(AnimFlags.Roll)) mdata++;
            if (animtype.HasFlag(AnimFlags.Quaternion)) mdata++;
            int mdatasize = 0;
            bool lost = false;
            switch (mdata)
            {
                case 1:
                case 2:
                    mdatasize = 16;
                    break;
                case 3:
                    mdatasize = 24;
                    break;
                case 4:
                    mdatasize = 32;
                    break;
                case 5:
                    mdatasize = 40;
                    break;
                default:
                    lost = true;
                    break;
            }
            if (lost) return 0;
            // Check MKEY pointers
            int mdatas = 0;
            for (int u = 0; u < 255; u++)
            {
                for (int m = 0; m < mdata; m++)
                {
                    if (lost) continue;
                    sr.Seek(mdatap + offset + mdatasize * u + 4 * m, SeekOrigin.Begin);
                    uint pointer = sr.ReadBE<uint>();
                    if (pointer != 0 && pointer + offset >= sr.BaseStream.Length - 36)
                        lost = true;
                    if (!lost)
                    {
                        sr.Seek(mdatap + offset + mdatasize * u + 4 * mdata + 4 * m, SeekOrigin.Begin);
                        int framecount = sr.ReadBE<int>();
                        if (framecount < 0 || framecount > 100 || pointer == 0 && framecount != 0)
                            lost = true;
                    }
                }
                if (!lost)
                    mdatas++;
            }
            sr.Seek(bookmark, SeekOrigin.Begin);
            return mdatas;
        }
    }
}
