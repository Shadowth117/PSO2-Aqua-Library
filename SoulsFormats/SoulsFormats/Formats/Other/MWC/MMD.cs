using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Other.MWC
{
    /// <summary>
    /// Metal Wolf Chaos Map Model Data
    /// </summary>
    public class MMD : SoulsFile<MMD>
    {
        /// <summary>
        /// MMD Header data
        /// </summary>
        public MMDHeader header;
        /// <summary>
        /// Mesh headers List
        /// </summary>
        public List<MeshHeader> meshHeaders = new List<MeshHeader>();
        /// <summary>
        /// Vertices List
        /// </summary>
        public List<Vertex> vertices = new List<Vertex>();
        /// <summary>
        /// Face Indices List
        /// </summary>
        public List<ushort> faceIndices = new List<ushort>();
        /// <summary>
        /// Bones list
        /// </summary>
        public List<Bone> bones = new List<Bone>();
        /// <summary>
        /// Texture names list
        /// </summary>
        public List<string> texNames = new List<string>();
        /// <summary>
        /// MMD Base Constructor
        /// </summary>
        public MMD() { }

        /// <summary>
        /// MMD BinaryReaderEx Constructor
        /// </summary>
        public MMD(BinaryReaderEx br)
        {
            Read(br);
        }

        /// <summary>
        /// Method to determine if this is indeed an MMD
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 8)
                return false;

            br.ReadInt32();
            string magic = br.GetASCII(4);
            return magic == "MMD ";
        }

        /// <summary>
        /// Method to read MMD
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            header = new MMDHeader();
            br.ReadInt32();
            br.AssertASCII("MMD ");
            header.usht_08 = br.ReadUInt16();
            header.usht_0A = br.ReadUInt16();
            header.int_0C = br.ReadInt32();

            header.meshCount = br.ReadInt32();
            header.faceIndexCount = br.ReadInt32();
            header.vertexCount = br.ReadInt32();
            header.boneCount = br.ReadInt32();

            header.textureCount = br.ReadInt32();
            var meshHeaderOffset = br.ReadInt32();
            var faceIndicesOffset = br.ReadInt32();
            var vertexDataOffset = br.ReadInt32();

            var boneOffset = br.ReadInt32();
            var textureNameOffset = br.ReadInt32();
            header.unkCount3 = br.ReadInt32();
            header.unkCount4 = br.ReadInt32();

            br.Position = meshHeaderOffset;
            for (int i = 0; i < header.meshCount; i++)
            {
                MeshHeader msh = new MeshHeader();
                msh.materialId = br.ReadInt32();
                msh.int_04 = br.ReadInt32();
                msh.faceIndexCount = br.ReadInt32();
                msh.faceIndexStart = br.ReadInt32();
                meshHeaders.Add(msh);
            }

            br.Position = faceIndicesOffset;
            for (int i = 0; i < header.faceIndexCount; i++)
            {
                faceIndices.Add(br.ReadUInt16());
            }

            br.Position = vertexDataOffset;
            for (int i = 0; i < header.vertexCount; i++)
            {
                vertices.Add(new Vertex(br));
            }

            br.Position = boneOffset;
            for (int i = 0; i < header.boneCount; i++)
            {
                var bone = new Bone();
                bone.int00 = br.ReadInt32();
                bone.int04 = br.ReadInt32();
                bone.int08 = br.ReadInt32();
                bone.int0C = br.ReadInt32();

                bone.int10 = br.ReadInt32();
                bone.int14 = br.ReadInt32();
                bone.int18 = br.ReadInt32();
                bone.int1C = br.ReadInt32();

                bone.translation = br.ReadVector3();
                bone.extraFloat0 = br.ReadSingle();
                bone.rotation = br.ReadQuaternion();
                bone.scale = br.ReadVector3();
                bone.extraFloat1 = br.ReadSingle();

                bone.int50 = br.ReadInt32();
                bone.int54 = br.ReadInt32();
                bone.int58 = br.ReadInt32();
                bone.int5C = br.ReadInt32();

                bone.flt60 = br.ReadInt32();
                bone.int64 = br.ReadInt32();
                bone.int68 = br.ReadInt32();
                bone.int6C = br.ReadInt32();

                bone.int70 = br.ReadInt32();
                bone.int74 = br.ReadInt32();
                bone.int78 = br.ReadInt32();
                bone.int7C = br.ReadInt32();

                bones.Add(bone);
            }

            br.Position = textureNameOffset;
            for (int i = 0; i < header.textureCount; i++)
            {
                texNames.Add(br.ReadASCII());
            }
        }

        /// <summary>
        /// Structure of the MMDHeader
        /// </summary>
        public struct MMDHeader
        {
            //public int fileSize;
            //public int magic;
            public ushort usht_08;
            public ushort usht_0A;
            public int int_0C;

            public int meshCount;
            public int faceIndexCount;
            public int vertexCount;
            public int boneCount;

            public int textureCount;
            //public int meshHeaderOffset;
            //public int faceIndicesOffset;
            //public int vertexDataOffset;

            //public int boneOffset;
            //public int textureNameOffset;
            public int unkCount3;
            public int unkCount4;
        }

        /// <summary>
        /// Structure of a mesh header in an MMD
        /// </summary>
        public struct MeshHeader
        {
            public int materialId;
            public int int_04;
            public int faceIndexCount;
            public int faceIndexStart;
        }

        /// <summary>
        /// Vertex in an MMD
        /// </summary>
        public class Vertex
        {
            /// <summary>
            /// These positions are all VERY large for some reason. 
            /// Based on the white house model in m000, the first mmd model, we can estimate that these positions are about 2000x the scale they should be.
            /// </summary>
            public virtual Vector3 Position { get; set; }
            public virtual Vector3 Normal { get; set; }
            public byte[] WeightIndices { get; set; }     //Always 0? Unknown
            public byte[] Weights { get; set; }           //Always 0? Unknown

            public Vector2[] UVs;
            public Color Color;

            public Vertex()
            {
                UVs = new Vector2[4];
            }

            internal Vertex(BinaryReaderEx br)
            {
                Position = br.ReadVector3();
                Normal = br.Read11_11_10Vector3();
                WeightIndices = br.ReadBytes(4);
                Weights = br.ReadBytes(4);
                UVs = new Vector2[4];
                for (int i = 0; i < 4; i++)
                    UVs[i] = br.ReadVector2();
                Color = br.ReadRGBA();

            }
        }

        /// <summary>
        /// Structure of a node in an MMD
        /// </summary>
        public struct Bone
        {
            public int int00;
            public int int04;
            public int int08;
            public int int0C;

            public int int10;
            public int int14;
            public int int18;
            public int int1C; //Always 0xFFFFFFFF?

            public Vector3 translation;
            public float extraFloat0;
            public Quaternion rotation;
            public Vector3 scale;
            public float extraFloat1;

            public int int50;
            public int int54;
            public int int58;
            public int int5C;

            public float flt60;
            public int int64;
            public int int68;
            public int int6C;

            public int int70;
            public int int74;
            public int int78;
            public int int7C;
        }
    }
}
