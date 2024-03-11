using AquaModelLibrary.Helpers.Readers;
using System.Drawing.Drawing2D;
using System.Numerics;

//Addapted from SA Tools
namespace AquaModelLibrary.Data.Ninja.Motion
{
    public class NJSMotion
    {
        public bool BillyMode = false;
        public int frameCount = 0;
        public AnimFlags animType = 0;
        public InterpolationMode interpoMode = 0;
        public Dictionary<int, AnimModelData> KeyDataList = new Dictionary<int, AnimModelData>();

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
                        data = data ?? new AnimModelData();
                        var bookmark = sr.Position;
                        sr.Seek(posoff + offset, SeekOrigin.Begin);
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
                        data = data ?? new AnimModelData();
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
                if (animType.HasFlag(AnimFlags.Scale))
                {
                    int frames = sr.ReadBE<int>();
                    if (scloff != 0 && frames > 0)
                    {
                        data = data ?? new AnimModelData();
                        var bookmark = sr.Position;
                        sr.Seek(scloff + offset, SeekOrigin.Begin);
                        for (int j = 0; j < frames; j++)
                        {
                            if(BillyMode)
                            {
                                data.Scale.Add(sr.ReadBE<short>(), new Vector3(sr.ReadBE<ushort>(), sr.ReadBE<ushort>(), sr.ReadBE<ushort>()));
                            } else
                            {
                                data.Scale.Add(sr.ReadBE<int>(), sr.ReadBEV3());
                            }
                        }
                        sr.Seek(bookmark, SeekOrigin.Begin);
                    }
                }
                if (animType.HasFlag(AnimFlags.Vector))
                {
                    int frames = sr.ReadBE<int>();
                    if (vecoff != 0 && frames > 0)
                    {
                        data = data ?? new AnimModelData();
                        var bookmark = sr.Position;
                        sr.Seek(vecoff + offset, SeekOrigin.Begin);
                        for (int j = 0; j < frames; j++)
                        {
                            data.Vector.Add(sr.ReadBE<int>(), sr.ReadBEV3());
                        }
                        sr.Seek(bookmark, SeekOrigin.Begin);
                    }
                }
                int vtxcount = -1;
                if (animType.HasFlag(AnimFlags.Vertex))
                {
                    int frames = sr.ReadBE<int>();
                    if (vertoff != 0 && frames > 0)
                    {
                        data = data ?? new AnimModelData();
                        var bookmark = sr.Position;
                        sr.Seek(vertoff + offset, SeekOrigin.Begin);

                        vtxcount = vtxcount < 0 ? GetVertexCount(sr, vertoff, frames) : vtxcount;

                        sr.Seek(vertoff + offset, SeekOrigin.Begin);
                        for (int j = 0; j < frames; j++)
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
                        sr.Seek(bookmark, SeekOrigin.Begin);
                    }
                }
                if (animType.HasFlag(AnimFlags.Normal))
                {
                    int frames = sr.ReadBE<int>();
                    if (normoff != 0 && frames > 0)
                    {
                        data = data ?? new AnimModelData();
                        var bookmark = sr.Position;
                        sr.Seek(normoff + offset, SeekOrigin.Begin);
                        vtxcount = vtxcount < 0 ? GetVertexCount(sr, normoff, frames) : vtxcount;

                        sr.Seek(normoff + offset, SeekOrigin.Begin);
                        for (int j = 0; j < frames; j++)
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

                        sr.Seek(bookmark, SeekOrigin.Begin);
                    }
                }
                if (animType.HasFlag(AnimFlags.Target))
                {
                    int frames = sr.ReadBE<int>();
                    if (targoff != 0 && frames > 0)
                    {
                        data = data ?? new AnimModelData();
                        var bookmark = sr.Position;
                        sr.Seek(targoff + offset, SeekOrigin.Begin);
                        for (int j = 0; j < frames; j++)
                        {
                            data.Target.Add(sr.ReadBE<int>(), sr.ReadBEV3());
                        }
                        sr.Seek(bookmark, SeekOrigin.Begin);
                    }
                }
                if (animType.HasFlag(AnimFlags.Roll))
                {
                    int frames = sr.ReadBE<int>();
                    if (rolloff != 0 && frames > 0)
                    {
                        data = data ?? new AnimModelData();
                        var bookmark = sr.Position;
                        sr.Seek(rolloff + offset, SeekOrigin.Begin);
                        for (int j = 0; j < frames; j++)
                        {
                            data.Roll.Add(sr.ReadBE<int>(), sr.ReadBE<int>());
                        }
                        sr.Seek(bookmark, SeekOrigin.Begin);
                    }
                }
                if (animType.HasFlag(AnimFlags.Angle))
                {
                    int frames = sr.ReadBE<int>();
                    if (angoff != 0 && frames > 0)
                    {
                        data = data ?? new AnimModelData();
                        var bookmark = sr.Position;
                        sr.Seek(angoff + offset, SeekOrigin.Begin);
                        for (int j = 0; j < frames; j++)
                        {
                            data.Angle.Add(sr.ReadBE<int>(), sr.ReadBE<int>());
                        }
                        sr.Seek(bookmark, SeekOrigin.Begin);
                    }
                }
                if (animType.HasFlag(AnimFlags.Color))
                {
                    int frames = sr.ReadBE<int>();
                    if (coloff != 0 && frames > 0)
                    {
                        data = data ?? new AnimModelData();
                        var bookmark = sr.Position;
                        sr.Seek(coloff + offset, SeekOrigin.Begin);
                        for (int j = 0; j < frames; j++)
                        {
                            data.Color.Add(sr.ReadBE<int>(), sr.ReadBE<uint>());
                        }
                        sr.Seek(bookmark, SeekOrigin.Begin);
                    }
                }
                if (animType.HasFlag(AnimFlags.Intensity))
                {
                    int frames = sr.ReadBE<int>();
                    if (intoff != 0 && frames > 0)
                    {
                        data = data ?? new AnimModelData();
                        var bookmark = sr.Position;
                        sr.Seek(intoff + offset, SeekOrigin.Begin);
                        for (int j = 0; j < frames; j++)
                        {
                            data.Intensity.Add(sr.ReadBE<int>(), sr.ReadBE<float>());
                        }
                        sr.Seek(bookmark, SeekOrigin.Begin);
                    }
                }
                if (animType.HasFlag(AnimFlags.Spot))
                {
                    int frames = sr.ReadBE<int>();
                    if (spotoff != 0 && frames > 0)
                    {
                        data = data ?? new AnimModelData();
                        var bookmark = sr.Position;
                        sr.Seek(spotoff + offset, SeekOrigin.Begin);
                        for (int j = 0; j < frames; j++)
                        {
                            data.Spot.Add(sr.ReadBE<int>(), new Spotlight(sr));
                        }
                        sr.Seek(bookmark, SeekOrigin.Begin);
                    }
                }
                if (animType.HasFlag(AnimFlags.Point))
                {
                    int frames = sr.ReadBE<int>();
                    if (pntoff != 0 && frames > 0)
                    {
                        data = data ?? new AnimModelData();
                        var bookmark = sr.Position;
                        sr.Seek(pntoff + offset, SeekOrigin.Begin);
                        for (int j = 0; j < frames; j++)
                        {
                            data.Point.Add(sr.ReadBE<int>(), sr.ReadBEV2());
                        }
                        sr.Seek(bookmark, SeekOrigin.Begin);
                    }
                }
                if (animType.HasFlag(AnimFlags.Quaternion))
                {
                    int frames = sr.ReadBE<int>();
                    if (quatoff != 0 && frames > 0)
                    {
                        data = data ?? new AnimModelData();
                        var bookmark = sr.Position;
                        sr.Seek(quatoff + offset, SeekOrigin.Begin);
                        for (int j = 0; j < frames; j++)
                        {
                            //WXYZ order
                            data.Quaternion.Add(sr.ReadBE<int>(), new float[] { sr.ReadBE<float>(), sr.ReadBE<float>(), sr.ReadBE<float>(), sr.ReadBE<float>() });
                        }
                        sr.Seek(bookmark, SeekOrigin.Begin);
                    }
                }

                if(data != null)
                {
                    KeyDataList.Add(i, data);
                }
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
