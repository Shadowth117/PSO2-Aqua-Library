using AquaModelLibrary.Helpers.Readers;

//THANK YOU HYPER AND KNUX: https://github.com/Big-Endian-32/Marathon/blob/03a2812cc903241ab65fd21d2270c0680044bc09/Marathon/Formats/Mesh/Ninja/NinjaMotion.cs
namespace Marathon.Formats.Mesh.Ninja
{
    /// <summary>
    /// Structure of the main Ninja Motion data.
    /// </summary>
    public class NinjaMotion
    {
        /// <summary>
        /// NinjaMotion is used by a lot of things, so we store the Chunk ID so we know what to write back.
        /// </summary>
        public string ChunkID { get; set; }
        public MotionType Type { get; set; }
        public float StartFrame { get; set; }
        public float EndFrame { get; set; }
        public List<NinjaSubMotion> SubMotions { get; set; } = new List<NinjaSubMotion>();
        public float Framerate { get; set; }
        public uint Reserved0 { get; set; }
        public uint Reserved1 { get; set; }

        /// <summary>
        /// Reads the Ninja Motion data based on a filename
        /// </summary>
        /// <param name="bytes"></param>
        public void Read(string filePath)
        {
            Read(File.ReadAllBytes(filePath));
        }

        /// <summary>
        /// Reads the Ninja Motion data from a file.
        /// </summary>
        /// <param name="reader">The binary reader for this SegaNN file.</param>
        public void Read(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            using (BufferedStreamReaderBE<MemoryStream> reader = new(ms))
            {
                int magic = reader.Read<int>();
                int fileSize = reader.Read<int>();

                bool be = reader.Peek<int>() < 0;
                // Read the offset to the actual Ninja Motion data.
                uint dataOffset = reader.ReadBE<uint>(be);

                // Jump to the actual Ninja Motion data.
                reader.Seek(dataOffset, System.IO.SeekOrigin.Begin);

                // Read all of the data from the Ninja Motion data.
                Type = (MotionType)reader.ReadBE<uint>(be);
                StartFrame = reader.ReadBE<float>(be);
                EndFrame = reader.ReadBE<float>(be);
                uint SubMotionCount = reader.ReadBE<uint>(be);
                uint SubMotionsOffset = reader.ReadBE<uint>(be);
                Framerate = reader.ReadBE<float>(be);
                Reserved0 = reader.ReadBE<uint>(be);
                Reserved1 = reader.ReadBE<uint>(be);

                // Jump to the offset for this motion data's sub motions.
                reader.Seek(SubMotionsOffset, System.IO.SeekOrigin.Begin);

                // Loop through and read all the sub motions.
                for (int i = 0; i < SubMotionCount; i++)
                {
                    NinjaSubMotion subMotion = new NinjaSubMotion();
                    subMotion.Read(reader, be);
                    SubMotions.Add(subMotion);
                }
            }
        }

        /// <summary>
        /// Write the Ninja Motion data to a file.
        /// </summary>
        /// <param name="writer">The binary writer for this SegaNN file.</param>
        /*
        public byte[] GetBytes()
        {
            
            // Set up a list of offsets for earlier points in the chunk.
            Dictionary<string, uint> MotionOffsets = new Dictionary<string, uint>();

            // Write chunk header.
            writer.Write(ChunkID);
            writer.Write("SIZE"); // Temporary entry, is filled in later once we know this chunk's size.
            long HeaderSizePosition = writer.BaseStream.Position;
            writer.AddOffset("dataOffset");
            writer.FixPadding(0x10);

            // Keyframes.
            for (int i = 0; i < SubMotions.Count; i++)
            {
                // Add an offset to our list so we know where this sub motion's keyframes are.
                MotionOffsets.Add($"SubMotion{i}KeyframesOffset", (uint)writer.BaseStream.Position);

                // Loop through this sub motions keyframes and write different data depending on the Type flag.
                for (int k = 0; k < SubMotions[i].Keyframes.Count; k++)
                {
                    if
                    (
                        SubMotions[i].Type.HasFlag(SubMotionType.NND_SMOTTYPE_TRANSLATION_MASK) ||
                        SubMotions[i].Type.HasFlag(SubMotionType.NND_SMOTTYPE_SCALING_MASK) ||
                        SubMotions[i].Type.HasFlag(SubMotionType.NND_SMOTTYPE_AMBIENT_MASK) ||
                        SubMotions[i].Type.HasFlag(SubMotionType.NND_SMOTTYPE_DIFFUSE_MASK) ||
                        SubMotions[i].Type.HasFlag(SubMotionType.NND_SMOTTYPE_SPECULAR_MASK) ||
                        SubMotions[i].Type.HasFlag(SubMotionType.NND_SMOTTYPE_LIGHT_COLOR_MASK)
                    )
                    {
                        writer.Write((SubMotions[i].Keyframes[k] as NinjaKeyframe.NNS_MOTION_KEY_VECTOR).Frame);
                        writer.Write((SubMotions[i].Keyframes[k] as NinjaKeyframe.NNS_MOTION_KEY_VECTOR).Value);
                    }
                    else if (SubMotions[i].Type.HasFlag(SubMotionType.NND_SMOTTYPE_ROTATION_XYZ))
                    {
                        writer.Write((SubMotions[i].Keyframes[k] as NinjaKeyframe.NNS_MOTION_KEY_ROTATE_A16).Frame);
                        writer.Write((SubMotions[i].Keyframes[k] as NinjaKeyframe.NNS_MOTION_KEY_ROTATE_A16).Value1);
                        writer.Write((SubMotions[i].Keyframes[k] as NinjaKeyframe.NNS_MOTION_KEY_ROTATE_A16).Value2);
                        writer.Write((SubMotions[i].Keyframes[k] as NinjaKeyframe.NNS_MOTION_KEY_ROTATE_A16).Value3);
                    }
                    else if (SubMotions[i].Type.HasFlag(SubMotionType.NND_SMOTTYPE_FRAME_FLOAT))
                    {
                        writer.Write((SubMotions[i].Keyframes[k] as NinjaKeyframe.NNS_MOTION_KEY_FLOAT).Frame);
                        writer.Write((SubMotions[i].Keyframes[k] as NinjaKeyframe.NNS_MOTION_KEY_FLOAT).Value);
                    }
                    else if (SubMotions[i].Type.HasFlag(SubMotionType.NND_SMOTTYPE_FRAME_SINT16))
                    {
                        writer.Write((SubMotions[i].Keyframes[k] as NinjaKeyframe.NNS_MOTION_KEY_SINT16).Frame);
                        writer.Write((SubMotions[i].Keyframes[k] as NinjaKeyframe.NNS_MOTION_KEY_SINT16).Value);
                    }
                    else
                    {
                        // If none of those flags are found, error out.
                        throw new NotImplementedException();
                    }
                }
            }

            // Write sub motions.
            //   Add an offset to our list so we know where the sub motions are. 
            MotionOffsets.Add($"SubMotionTable", (uint)writer.BaseStream.Position);
            for (int i = 0; i < SubMotions.Count; i++)
            {
                // Write most of the data for this sub motion.
                writer.Write((uint)SubMotions[i].Type);
                writer.Write((uint)SubMotions[i].InterpolationType);
                writer.Write(SubMotions[i].NodeIndex);
                writer.Write(SubMotions[i].StartFrame);
                writer.Write(SubMotions[i].EndFrame);
                writer.Write(SubMotions[i].StartKeyframe);
                writer.Write(SubMotions[i].EndKeyframe);
                writer.Write(SubMotions[i].Keyframes.Count);

                // Figure out the size value to write based on the flags.
                //   Error out if none of them are found. 
                if
                (
                    SubMotions[i].Type.HasFlag(SubMotionType.NND_SMOTTYPE_TRANSLATION_MASK) ||
                    SubMotions[i].Type.HasFlag(SubMotionType.NND_SMOTTYPE_SCALING_MASK) ||
                    SubMotions[i].Type.HasFlag(SubMotionType.NND_SMOTTYPE_AMBIENT_MASK) ||
                    SubMotions[i].Type.HasFlag(SubMotionType.NND_SMOTTYPE_DIFFUSE_MASK) ||
                    SubMotions[i].Type.HasFlag(SubMotionType.NND_SMOTTYPE_SPECULAR_MASK) ||
                    SubMotions[i].Type.HasFlag(SubMotionType.NND_SMOTTYPE_LIGHT_COLOR_MASK)
                )
                {
                    writer.Write(16);
                }
                else if (SubMotions[i].Type.HasFlag(SubMotionType.NND_SMOTTYPE_ROTATION_XYZ))
                {
                    writer.Write(8);
                }
                else if (SubMotions[i].Type.HasFlag(SubMotionType.NND_SMOTTYPE_FRAME_FLOAT))
                {
                    writer.Write(8);
                }
                else if (SubMotions[i].Type.HasFlag(SubMotionType.NND_SMOTTYPE_FRAME_SINT16))
                {
                    writer.Write(4);
                }
                else
                {
                    throw new NotImplementedException();
                }

                // Add an offset to fill in later with the NOF0 chunk.
                writer.AddOffset($"SubMotion{i}KeyframesOffset", 0);

                // Write the previously saved position for this sub motion's keyframes.
                writer.Write(MotionOffsets[$"SubMotion{i}KeyframesOffset"] - writer.Offset);
            }

            // Write chunk data.
            writer.FillOffset("dataOffset", true);
            writer.Write((uint)Type);
            writer.Write(StartFrame);
            writer.Write(EndFrame);
            writer.Write(SubMotions.Count);
            writer.AddOffset($"SubMotionTable", 0);
            writer.Write(MotionOffsets[$"SubMotionTable"] - writer.Offset);
            writer.Write(Framerate);
            writer.Write(Reserved0);
            writer.Write(Reserved1);

            // Alignment.
            writer.FixPadding(0x10);

            // Write chunk size.
            long ChunkEndPosition = writer.BaseStream.Position;
            uint ChunkSize = (uint)(ChunkEndPosition - HeaderSizePosition);
            writer.BaseStream.Position = HeaderSizePosition - 4;
            writer.Write(ChunkSize);
            writer.BaseStream.Position = ChunkEndPosition;
        }
        */
    }
}