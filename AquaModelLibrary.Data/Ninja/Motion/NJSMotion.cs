using AquaModelLibrary.Helpers.Readers;

//Addapted from SA Tools
namespace AquaModelLibrary.Data.Ninja.Motion
{
    public class NJSMotion
    {
        public int frameCount = 0;
        public AnimFlags animType = 0;
        public InterpolationMode interpoMode = 0;
        public Dictionary<int, AnimModelData> KeyDataList = new Dictionary<int, AnimModelData>();

        public NJSMotion() { }

        public NJSMotion(byte[] file, int offset = 0, bool shortRot = false)
        {
            Read(file, offset, shortRot);
        }

        public NJSMotion(BufferedStreamReaderBE<MemoryStream> sr, int offset = 0, bool shortRot = false)
        {
            Read(sr, offset, shortRot);
        }

        public void Read(byte[] file, int offset = 0, bool shortRot = false)
        {
            using (var ms = new MemoryStream(file))
            using (var sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr, offset, shortRot);
            }
        }

        public void Read(BufferedStreamReaderBE<MemoryStream> sr, int offset = 0, bool shortRot = false)
        {
            bool shortRotCheck = true;
            var nodeCount = CalculateNodeCount(sr, offset);

            var initialPointer = sr.Read<int>();
            frameCount = sr.Read<int>();
            animType = sr.Read<AnimFlags>();
            var interpolationFrameSizeCombo = sr.Read<ushort>();
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

            var dataStart = sr.Position;
            if(ReadAnimData(sr, offset, nodeCount, shortRot, shortRotCheck) == false)
            {
                KeyDataList.Clear();
                sr.Seek(dataStart, SeekOrigin.Begin);
                ReadAnimData(sr, offset, nodeCount, true, false);
            }
        }

        /// <summary>
        /// Reads anim frame data. Returns false if rotation anim data seems to be out of range. 
        /// Ninja motions do not provide a natural way of finding this and can feature both types of rotation within the same game so this brute forcing is needed.
        /// </summary>
        private bool ReadAnimData(BufferedStreamReaderBE<MemoryStream> sr, int offset, int nodeCount, bool shortRot, bool shortRotCheck)
        {
            for (int i = 0; i < nodeCount; i++)
            {
                AnimModelData data = null;
                uint posoff = animType.HasFlag(AnimFlags.Position) ? sr.ReadBE<uint>() : 0;
                uint rotoff = animType.HasFlag(AnimFlags.Rotation) ? sr.ReadBE<uint>() : 0;
                uint scloff = animType.HasFlag(AnimFlags.Scale) ? sr.ReadBE<uint>() : 0;
                uint vecoff = animType.HasFlag(AnimFlags.Vector) ? sr.ReadBE<uint>() : 0;
                uint vertoff = animType.HasFlag(AnimFlags.Vertex) ? sr.ReadBE<uint>() : 0;
                uint normoff = animType.HasFlag(AnimFlags.Normal) ? sr.ReadBE<uint>() : 0;
                uint targoff = animType.HasFlag(AnimFlags.Target) ? sr.ReadBE<uint>() : 0;
                uint rolloff = animType.HasFlag(AnimFlags.Roll) ? sr.ReadBE<uint>() : 0;
                uint angoff = animType.HasFlag(AnimFlags.Angle) ? sr.ReadBE<uint>() : 0;
                uint coloff = animType.HasFlag(AnimFlags.Color) ? sr.ReadBE<uint>() : 0;
                uint intoff = animType.HasFlag(AnimFlags.Intensity) ? sr.ReadBE<uint>() : 0;
                uint spotoff = animType.HasFlag(AnimFlags.Spot) ? sr.ReadBE<uint>() : 0;
                uint pntoff = animType.HasFlag(AnimFlags.Point) ? sr.ReadBE<uint>() : 0;
                uint quatoff = animType.HasFlag(AnimFlags.Quaternion) ? sr.ReadBE<uint>() : 0;

                if (animType.HasFlag(AnimFlags.Position))
                {
                    int frames = sr.ReadBE<int>();
                    if (posoff != 0 && frames > 0)
                    {
                        var bookmark = sr.Position;
                        sr.Seek(posoff + offset, SeekOrigin.Begin);
                        data = data ?? new AnimModelData();
                        for (int j = 0; j < frames; j++)
                        {
                            data.Position.Add(sr.ReadBE<int>(), sr.ReadBEV3());
                        }
                        sr.Seek(bookmark, SeekOrigin.Begin);
                    }
                }
                if (animType.HasFlag(AnimFlags.Rotation))
                {
                    int frames = sr.ReadBE<int>();
                    if (rotoff != 0 && frames > 0)
                    {
                        var bookmark = sr.Position;
                        sr.Seek(rotoff + offset, SeekOrigin.Begin);
                        if (shortRot == false && shortRotCheck == true)
                        {
                            // Check if the animation uses short rotation or not
                            for (int j = 0; j < frames; j++)
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
                        }
                        // Read rotation values
                        for (int j = 0; j < frames; j++)
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
                        sr.Seek(bookmark, SeekOrigin.Begin);
                    }
                }
                /*
                if (animType.HasFlag(AnimFlags.Scale))
                {
                    int frames = sr.ReadBE<int>();
                    if (scloff != 0 && frames > 0)
                    {
                        hasdata = true;
                        tmpaddr = (int)scloff;
                        if (labels != null && labels.ContainsKey(tmpaddr))
                            data.ScaleName = labels[tmpaddr];
                        else data.ScaleName = Name + "_mkey_" + i.ToString() + "_scl_" + tmpaddr.ToString("X8");
                        for (int j = 0; j < frames; j++)
                        {
                            data.Scale.Add(ByteConverter.ToInt32(file, tmpaddr), new Vertex(file, tmpaddr + 4));
                            tmpaddr += 16;
                        }
                    }
                    address += 4;
                }
                if (animType.HasFlag(AnimFlags.Vector))
                {
                    int frames = sr.ReadBE<int>();
                    if (vecoff != 0 && frames > 0)
                    {
                        hasdata = true;
                        tmpaddr = (int)vecoff;
                        if (labels != null && labels.ContainsKey(tmpaddr))
                            data.VectorName = labels[tmpaddr];
                        else data.VectorName = Name + "_mkey_" + i.ToString() + "_vec_" + tmpaddr.ToString("X8");
                        for (int j = 0; j < frames; j++)
                        {
                            data.Vector.Add(ByteConverter.ToInt32(file, tmpaddr), new Vertex(file, tmpaddr + 4));
                            tmpaddr += 16;
                        }
                    }
                    address += 4;
                }
                int vtxcount = -1;
                if (animType.HasFlag(AnimFlags.Vertex))
                {
                    int frames = sr.ReadBE<int>();
                    if (vertoff != 0 && frames > 0)
                    {
                        hasdata = true;
                        tmpaddr = (int)vertoff;
                        if (labels != null && labels.ContainsKey(tmpaddr))
                            data.VertexName = labels[tmpaddr];
                        else data.VertexName = Name + "_mkey_" + i.ToString() + "_vert_" + tmpaddr.ToString("X8");
                        List<int> ptrs = new List<int>();
                        data.VertexItemName = new string[frames];
                        for (int j = 0; j < frames; j++)
                        {
                            ptrs.AddUnique((int)(ByteConverter.ToUInt32(file, tmpaddr + 4) - imageBase));
                            int itemaddr = (int)(ByteConverter.ToUInt32(file, tmpaddr + 4) - imageBase);
                            if (labels != null && labels.ContainsKey(itemaddr))
                                data.VertexItemName[j] = labels[itemaddr];
                            else data.VertexItemName[j] = Name + "_" + i.ToString() + "_vtx_" + j.ToString() + "_" + itemaddr.ToString("X8");
                            tmpaddr += 8;
                        }
                        // Use vertex counts specified in split if available
                        if (numverts != null && numverts.Length > 0)
                            vtxcount = numverts[i];
                        else
                        {
                            if (ptrs.Count > 1)
                            {
                                ptrs.Sort();
                                vtxcount = (ptrs[1] - ptrs[0]) / Vertex.Size;
                            }
                            else
                                vtxcount = ((int)vertoff - ptrs[0]) / Vertex.Size;
                        }
                        tmpaddr = (int)vertoff;
                        for (int j = 0; j < frames; j++)
                        {
                            Vertex[] verts = new Vertex[vtxcount];
                            int newaddr = (int)(ByteConverter.ToUInt32(file, tmpaddr + 4) - imageBase);
                            for (int k = 0; k < verts.Length; k++)
                            {
                                verts[k] = new Vertex(file, newaddr);
                                newaddr += Vertex.Size;
                            }
                            if (!data.Vertex.ContainsKey(ByteConverter.ToInt32(file, tmpaddr))) data.Vertex.Add(ByteConverter.ToInt32(file, tmpaddr), verts);
                            tmpaddr += 8;
                        }
                    }
                    address += 4;
                }
                if (animType.HasFlag(AnimFlags.Normal))
                {
                    int frames = sr.ReadBE<int>();
                    if (normoff != 0 && frames > 0)
                    {
                        hasdata = true;
                        data.NormalItemName = new string[frames];
                        // Use vertex counts specified in split if available
                        if (numverts != null && numverts.Length > 0)
                            vtxcount = numverts[i];
                        else if (vtxcount < 0)
                        {
                            tmpaddr = (int)normoff;
                            List<int> ptrs = new List<int>();
                            for (int j = 0; j < frames; j++)
                            {
                                ptrs.AddUnique((int)(ByteConverter.ToUInt32(file, tmpaddr + 4) - imageBase));
                                tmpaddr += 8;
                            }
                            if (ptrs.Count > 1)
                            {
                                ptrs.Sort();
                                vtxcount = (ptrs[1] - ptrs[0]) / Vertex.Size;
                            }
                            else
                                vtxcount = ((int)normoff - ptrs[0]) / Vertex.Size;
                        }
                        tmpaddr = (int)normoff;
                        if (labels != null && labels.ContainsKey(tmpaddr))
                            data.NormalName = labels[tmpaddr];
                        else data.NormalName = Name + "_mkey_" + i.ToString() + "_norm_" + tmpaddr.ToString("X8");
                        for (int j = 0; j < frames; j++)
                        {
                            Vertex[] verts = new Vertex[vtxcount];
                            int newaddr = (int)(ByteConverter.ToUInt32(file, tmpaddr + 4) - imageBase);
                            if (labels != null && labels.ContainsKey(newaddr))
                                data.NormalItemName[j] = labels[newaddr];
                            else data.NormalItemName[j] = Name + "_" + i.ToString() + "_nrm_" + j.ToString() + "_" + newaddr.ToString("X8");
                            for (int k = 0; k < verts.Length; k++)
                            {
                                verts[k] = new Vertex(file, newaddr);
                                newaddr += Vertex.Size;
                            }
                            if (!data.Normal.ContainsKey(ByteConverter.ToInt32(file, tmpaddr))) data.Normal.Add(ByteConverter.ToInt32(file, tmpaddr), verts);
                            tmpaddr += 8;
                        }
                    }
                    address += 4;
                }
                if (animType.HasFlag(AnimFlags.Target))
                {
                    int frames = sr.ReadBE<int>();
                    if (targoff != 0 && frames > 0)
                    {
                        hasdata = true;
                        tmpaddr = (int)targoff;
                        if (labels != null && labels.ContainsKey(tmpaddr))
                            data.TargetName = labels[tmpaddr];
                        else data.TargetName = Name + "_mkey_" + i.ToString() + "_target_" + tmpaddr.ToString("X8");
                        for (int j = 0; j < frames; j++)
                        {
                            data.Target.Add(ByteConverter.ToInt32(file, tmpaddr), new Vertex(file, tmpaddr + 4));
                            tmpaddr += 16;
                        }
                    }
                    address += 4;
                }
                if (animType.HasFlag(AnimFlags.Roll))
                {
                    int frames = sr.ReadBE<int>();
                    if (rolloff != 0 && frames > 0)
                    {
                        hasdata = true;
                        tmpaddr = (int)rolloff;
                        if (labels != null && labels.ContainsKey(tmpaddr))
                            data.RollName = labels[tmpaddr];
                        else data.RollName = Name + "_mkey_" + i.ToString() + "_roll_" + tmpaddr.ToString("X8");
                        for (int j = 0; j < frames; j++)
                        {
                            data.Roll.Add(ByteConverter.ToInt32(file, tmpaddr), ByteConverter.ToInt32(file, tmpaddr + 4));
                            tmpaddr += 8;
                        }
                    }
                    address += 4;
                }
                if (animType.HasFlag(AnimFlags.Angle))
                {
                    int frames = sr.ReadBE<int>();
                    if (angoff != 0 && frames > 0)
                    {
                        hasdata = true;
                        tmpaddr = (int)angoff;
                        if (labels != null && labels.ContainsKey(tmpaddr))
                            data.AngleName = labels[tmpaddr];
                        else data.AngleName = Name + "_mkey_" + i.ToString() + "_ang_" + tmpaddr.ToString("X8");
                        for (int j = 0; j < frames; j++)
                        {
                            data.Angle.Add(ByteConverter.ToInt32(file, tmpaddr), ByteConverter.ToInt32(file, tmpaddr + 4));
                            tmpaddr += 8;
                        }
                    }
                    address += 4;
                }
                if (animType.HasFlag(AnimFlags.Color))
                {
                    int frames = sr.ReadBE<int>();
                    if (coloff != 0 && frames > 0)
                    {
                        hasdata = true;
                        tmpaddr = (int)coloff;
                        if (labels != null && labels.ContainsKey(tmpaddr))
                            data.ColorName = labels[tmpaddr];
                        else data.ColorName = Name + "_mkey_" + i.ToString() + "_col_" + tmpaddr.ToString("X8");
                        for (int j = 0; j < frames; j++)
                        {
                            data.Color.Add(ByteConverter.ToInt32(file, tmpaddr), ByteConverter.ToUInt32(file, tmpaddr + 4));
                            tmpaddr += 8;
                        }
                    }
                    address += 4;
                }
                if (animType.HasFlag(AnimFlags.Intensity))
                {
                    int frames = sr.ReadBE<int>();
                    if (intoff != 0 && frames > 0)
                    {
                        hasdata = true;
                        tmpaddr = (int)intoff;
                        if (labels != null && labels.ContainsKey(tmpaddr))
                            data.IntensityName = labels[tmpaddr];
                        else data.IntensityName = Name + "_mkey_" + i.ToString() + "_int_" + tmpaddr.ToString("X8");
                        for (int j = 0; j < frames; j++)
                        {
                            data.Intensity.Add(ByteConverter.ToInt32(file, tmpaddr), ByteConverter.ToSingle(file, tmpaddr + 4));
                            tmpaddr += 8;
                        }
                    }
                    address += 4;
                }
                if (animType.HasFlag(AnimFlags.Spot))
                {
                    int frames = sr.ReadBE<int>();
                    if (spotoff != 0 && frames > 0)
                    {
                        hasdata = true;
                        tmpaddr = (int)spotoff;
                        if (labels != null && labels.ContainsKey(tmpaddr))
                            data.SpotName = labels[tmpaddr];
                        else data.SpotName = Name + "_mkey_" + i.ToString() + "_spot_" + tmpaddr.ToString("X8");
                        for (int j = 0; j < frames; j++)
                        {
                            data.Spot.Add(ByteConverter.ToInt32(file, tmpaddr), new Spotlight(file, tmpaddr + 4));
                            tmpaddr += 20;
                        }
                    }
                    address += 4;
                }
                if (animType.HasFlag(AnimFlags.Point))
                {
                    int frames = sr.ReadBE<int>();
                    if (pntoff != 0 && frames > 0)
                    {
                        hasdata = true;
                        tmpaddr = (int)pntoff;
                        if (labels != null && labels.ContainsKey(tmpaddr))
                            data.PointName = labels[tmpaddr];
                        else data.PointName = Name + "_mkey_" + i.ToString() + "_point_" + tmpaddr.ToString("X8");
                        for (int j = 0; j < frames; j++)
                        {
                            data.Point.Add(ByteConverter.ToInt32(file, tmpaddr), new float[] { ByteConverter.ToSingle(file, tmpaddr + 4), ByteConverter.ToSingle(file, tmpaddr + 8) });
                            tmpaddr += 12;
                        }
                    }
                    address += 4;
                }
                if (animType.HasFlag(AnimFlags.Quaternion))
                {
                    int frames = sr.ReadBE<int>();
                    if (quatoff != 0 && frames > 0)
                    {
                        hasdata = true;
                        tmpaddr = (int)quatoff;
                        if (labels != null && labels.ContainsKey(tmpaddr))
                            data.QuaternionName = labels[tmpaddr];
                        else data.QuaternionName = Name + "_mkey_" + i.ToString() + "_quat_" + tmpaddr.ToString("X8");
                        for (int j = 0; j < frames; j++)
                        {
                            //WXYZ order
                            data.Quaternion.Add(ByteConverter.ToInt32(file, tmpaddr), new float[] { ByteConverter.ToSingle(file, tmpaddr + 4), ByteConverter.ToSingle(file, tmpaddr + 8), ByteConverter.ToSingle(file, tmpaddr + 12), ByteConverter.ToSingle(file, tmpaddr + 16) });
                            tmpaddr += 20;
                        }
                    }
                    address += 4;
                }
                */
            }

            return true;
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
