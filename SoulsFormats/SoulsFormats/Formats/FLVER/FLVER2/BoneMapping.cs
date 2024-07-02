using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class FLVER2
    {
        /// <summary>
        /// For mapping skeletons and their controls in Sekiro forward.
        /// </summary>
        public class BoneMapping
        {
            /// <summary>
            /// Contains the standard skeleton hierarchy mapping.
            /// </summary>
            public List<BoneSet> BaseBoneMapping { get; set; }

            /// <summary>
            /// Contains the control skeleton hierarchy mapping.
            /// </summary>
            public List<BoneSet> ControlBoneMapping { get; set; }

            /// <summary>
            /// Creates an empty BoneMapping.
            /// </summary>
            public BoneMapping()
            {
                BaseBoneMapping = new List<BoneSet>();
                ControlBoneMapping = new List<BoneSet>();
            }

            internal BoneMapping(BinaryReaderEx br)
            {
                short count1 = br.ReadInt16();
                short count2 = br.ReadInt16();
                uint offset1 = br.ReadUInt32();
                uint offset2 = br.ReadUInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);

                br.StepIn(offset1);
                {
                    BaseBoneMapping = new List<BoneSet>(count1);
                    for (int i = 0; i < count1; i++)
                        BaseBoneMapping.Add(new BoneSet(br));
                }
                br.StepOut();

                br.StepIn(offset2);
                {
                    ControlBoneMapping = new List<BoneSet>(count2);
                    for (int i = 0; i < count2; i++)
                        ControlBoneMapping.Add(new BoneSet(br));
                }
                br.StepOut();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt16((short)BaseBoneMapping.Count);
                bw.WriteInt16((short)ControlBoneMapping.Count);
                bw.ReserveUInt32("BaseBoneMappingOffset");
                bw.ReserveUInt32("ControlBoneMappingOffset");
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);

                bw.FillUInt32("BaseBoneMappingOffset", (uint)bw.Position);
                foreach (BoneSet member in BaseBoneMapping)
                    member.Write(bw);

                bw.FillUInt32("ControlBoneMappingOffset", (uint)bw.Position);
                foreach (BoneSet member in ControlBoneMapping)
                    member.Write(bw);
            }

            /// <summary>
            /// Hierarchy assignments for a particular bone.
            /// </summary>
            public class BoneSet
            {
                /// <summary>
                /// Bone's parent
                /// </summary>
                public short Parent { get; set; }
                /// <summary>
                /// Bone's first child
                /// </summary>
                public short FirstChild { get; set; }
                /// <summary>
                /// Bone's next sibling
                /// </summary>
                public short NextSibling { get; set; }
                /// <summary>
                /// Bone's previous sibling
                /// </summary>
                public short PreviousSibling { get; set; }
                /// <summary>
                /// Bone's index
                /// </summary>
                public int Index { get; set; }

                /// <summary>
                /// Creates a Member with default values.
                /// </summary>
                public BoneSet()
                {
                }

                internal BoneSet(BinaryReaderEx br)
                {
                    Parent = br.ReadInt16();
                    FirstChild = br.ReadInt16();
                    NextSibling = br.ReadInt16();
                    PreviousSibling = br.ReadInt16();
                    Index = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt16(Parent);
                    bw.WriteInt16(FirstChild);
                    bw.WriteInt16(NextSibling);
                    bw.WriteInt16(PreviousSibling);
                    bw.WriteInt32(Index);
                    bw.WriteInt32(0);
                }
            }
        }
    }
}
