//THANK YOU HYPER AND KNUX https://github.com/Big-Endian-32/Marathon/blob/03a2812cc903241ab65fd21d2270c0680044bc09/Marathon/Formats/Mesh/Ninja/NinjaSubMotion.cs
using AquaModelLibrary;
using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;

namespace Marathon.Formats.Mesh.Ninja
{
    /// <summary>
    /// Structure of a Ninja Sub Motion entry.
    /// </summary>
    public class NinjaSubMotion
    {
        public SubMotionType Type { get; set; }

        public SubMotionInterpolationType InterpolationType { get; set; }

        public int NodeIndex { get; set; }

        public float StartFrame { get; set; }

        public float EndFrame { get; set; }

        public float StartKeyframe { get; set; }

        public float EndKeyframe { get; set; }

        public List<object> Keyframes { get; set; } = new List<object>();

        /// <summary>
        /// Reads a Ninja Sub Motion entry from a file.
        /// </summary>
        /// <param name="reader">The binary reader for this SegaNN file.</param>
        public void Read(BufferedStreamReader reader, bool be)
        {
            // Read the main data for this Sub Motion.
            Type = (SubMotionType)reader.ReadBE<uint>(be);
            InterpolationType = (SubMotionInterpolationType)reader.ReadBE<uint>(be);
            NodeIndex = reader.ReadBE<int>(be);
            StartFrame = reader.ReadBE<float>(be);
            EndFrame = reader.ReadBE<float>(be);
            StartKeyframe = reader.ReadBE<float>(be);
            EndKeyframe = reader.ReadBE<float>(be);
            uint KeyFrameCount = reader.ReadBE<uint>(be);
            uint KeyFrameSize = reader.ReadBE<uint>(be);
            uint KeyFrameOffset = reader.ReadBE<uint>(be);

            // Save our current position so we can jump back afterwards.
            long pos = reader.Position();

            // Jump to the list of Keyframes for this sub motion.
            reader.Seek(KeyFrameOffset, System.IO.SeekOrigin.Begin);

            // Loop through and read the keyframes based on the Type flag.
            for (int i = 0; i < KeyFrameCount; i++)
            {
                if
                (
                    Type.HasFlag(SubMotionType.NND_SMOTTYPE_TRANSLATION_MASK) ||
                    Type.HasFlag(SubMotionType.NND_SMOTTYPE_SCALING_MASK) ||
                    Type.HasFlag(SubMotionType.NND_SMOTTYPE_AMBIENT_MASK) ||
                    Type.HasFlag(SubMotionType.NND_SMOTTYPE_DIFFUSE_MASK) ||
                    Type.HasFlag(SubMotionType.NND_SMOTTYPE_SPECULAR_MASK) ||
                    Type.HasFlag(SubMotionType.NND_SMOTTYPE_LIGHT_COLOR_MASK)
                )
                {
                    NinjaKeyframe.NNS_MOTION_KEY_VECTOR Keyframe = new NinjaKeyframe.NNS_MOTION_KEY_VECTOR();
                    Keyframe.Read(reader, be);
                    Keyframes.Add(Keyframe);
                }
                else if (Type.HasFlag(SubMotionType.NND_SMOTTYPE_ROTATION_XYZ))
                {
                    NinjaKeyframe.NNS_MOTION_KEY_ROTATE_A16 Keyframe = new NinjaKeyframe.NNS_MOTION_KEY_ROTATE_A16();
                    Keyframe.Read(reader, be);
                    Keyframes.Add(Keyframe);
                }

                /* (Knuxfan24): Generic Handling, these could go tits up. */

                else if (Type.HasFlag(SubMotionType.NND_SMOTTYPE_FRAME_FLOAT) && KeyFrameSize == 8)
                {
                    NinjaKeyframe.NNS_MOTION_KEY_FLOAT Keyframe = new NinjaKeyframe.NNS_MOTION_KEY_FLOAT();
                    Keyframe.Read(reader, be);
                    Keyframes.Add(Keyframe);
                }
                else if (Type.HasFlag(SubMotionType.NND_SMOTTYPE_FRAME_SINT16) && KeyFrameSize == 4)
                {
                    NinjaKeyframe.NNS_MOTION_KEY_SINT16 Keyframe = new NinjaKeyframe.NNS_MOTION_KEY_SINT16();
                    Keyframe.Read(reader, be);
                    Keyframes.Add(Keyframe);
                }
                else
                {
                    // All else has failed, give up.
                    throw new NotImplementedException();
                }
            }

            // Jump back to where we were.
            reader.Seek(pos, System.IO.SeekOrigin.Begin);
        }
    }
}