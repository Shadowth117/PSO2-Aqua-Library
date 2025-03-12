using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

namespace AquaModelLibrary.Data.Capcom.MonsterHunter
{
    /// <summary>
    /// .amo and .ahi files are a similar schema with different sections.
    /// They all start with a MainSection type, a subsection count, and a size of the section including this 0xC size header.
    /// Subsections are the same except they seem to have a 0ed int16 and then be defined by a secondary int16 for the first 4 bytes.
    /// </summary>
    public class MHTagFile
    {
        public MHSection rootSection = null;

        public MHTagFile() { }
        public MHTagFile(byte[] bytes)
        {
            Read(bytes);
        }
        public MHTagFile(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }
        private void Read(byte[] file)
        {
            using (MemoryStream stream = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(stream))
            {
                Read(sr);
            }
        }
        public void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            rootSection = ReadMHSection(sr);
        }
        public static MHSection ReadMHSection(BufferedStreamReaderBE<MemoryStream> sr)
        {
            var hd = new MHSectionHeader() { type0 = sr.ReadBE<ushort>(), type1 = sr.ReadBE<ushort>(), subCount = sr.ReadBE<int>(), size = sr.ReadBE<int>() };
            switch (hd.type1)
            {
                case 0:
                    switch ((MHMainSectionType0)hd.type0)
                    {
                        case MHMainSectionType0.IdList:
                            return new MHIdList(hd, sr);
                        case MHMainSectionType0.ModelRoot:
                            return new AMOModelRoot(hd, sr);
                        case MHMainSectionType0.Model:
                            return new AMOModel(hd, sr);
                        case MHMainSectionType0.Mesh:
                            return new AMOMesh(hd, sr);
                        case MHMainSectionType0.StripSetList:
                            return new AMOStripSetList(hd, sr);
                        case MHMainSectionType0.Material:
                            return new AMOMaterialList(hd, sr);
                        case MHMainSectionType0.TextureInfo:
                            return new AMOTextureInfo(hd, sr);
                        default:
                            throw new NotImplementedException();
                    }
                case 2:
                    return new MHModelFlags(hd, sr);
                case 3:
                case 4:
                    return new MHStripSet(hd, sr);
                case 5:
                    return new MHMaterialIdPalette(hd, sr);
                case 6:
                    return new MHMaterialStripMapping(hd, sr);
                case 7:
                    return new MHVertPositions(hd, sr);
                case 8:
                    return new MHVertNormals(hd, sr);
                case 0xA:
                    return new MHVertUVs(hd, sr);
                case 0xB:
                    return new MHVertColors(hd, sr);
                case 0xC:
                    return new MHVertWeights(hd, sr);
                case 0xE:
                    return new MHUnkEValues(hd, sr);
                case 0xF:
                    return new MHUnkFValues(hd, sr);
                case 0x10:
                    return new MHBonePalette(hd, sr);
                case 0x11:
                    return new MHUnk11Values(hd, sr);
                case 0x4000:
                    switch (hd.type0)
                    {
                        case 0x1:
                        case 0x2:
                            return new MHNode(hd, sr);
                        default:
                            throw new NotImplementedException();
                    }
                case 0xC000:
                    return new AHINodeList(hd, sr);
                default:
                    throw new NotImplementedException();
            }
        }

        public static T GetFirstSection<T>(IEnumerable<MHSection> sections) where T : MHSection
        {
            foreach(var section in sections)
            {
                if(section is T)
                {
                    return (T)section;
                }
            }

            return null;
        }

        public static List<T> GetSections<T>(IEnumerable<MHSection> sections) where T : MHSection
        {
            List<T> selectedSections = new List<T>();
            foreach (var section in sections)
            {
                if (section is T)
                {
                    selectedSections.Add((T)section);
                }
            }

            return selectedSections;
        }
    }
    public enum MHMainSectionType0 : ushort
    {
        IdList = 0x0,
        ModelRoot = 0x1,
        Model = 0x2,
        Mesh = 0x4,
        StripSetList = 0x5,
        Material = 0x9,
        TextureInfo = 0xA,
    }

    public enum MHMainSectionType1 : ushort
    {
        Skeleton = 0xC000,
    }

    public enum MHSubSectionType0 : ushort
    {
        NodeVariant1 = 0x1,
        NodeVariant2 = 0x2,
    }

    public enum MHSubSectionType1 : ushort
    {

        ModelFlags = 0x2,
        /// <summary>
        /// Count value is number of distinct strips.
        /// Strips start with an int32 count, then contain an int32 for each vertex id
        /// Unknown how 0x3 and 0x4 are different
        /// </summary>
        TristripsSet0 = 0x3,
        TristripsSet1 = 0x4,
        /// <summary>
        /// Material id mappings to the global material list
        /// </summary>
        MaterialPalette = 0x5,
        /// <summary>
        /// Material ids for each strip
        /// </summary>
        MaterialStripMapping = 0x6,
        /// <summary>
        /// Vertex positions, three float32s
        /// </summary>
        Position = 0x7,
        /// <summary>
        /// Vertex Normals, three float32s
        /// </summary>
        Normal = 0x8,
        /// <summary>
        /// Vertex UV coordinates, two float32s
        /// </summary>
        UV = 0xA,
        /// <summary>
        /// RGBA Colors, colors are stored as 4 float32s in range 0-255.0F
        /// </summary>
        Color = 0xB,
        /// <summary>
        /// Values have an int count and then an array of int index + float pairs for the count per vertex
        /// Weights are totaled to 100.0f
        /// </summary>
        Weights = 0xC,
        /// <summary>
        /// No idea
        /// </summary>
        UnkEValues = 0xE,
        /// <summary>
        /// Always 0x54 long and unclear what it is. Doesn't seem to appear in skinned meshes?
        /// </summary>
        UnkFValues = 0xF,
        /// <summary>
        /// The mesh's bonepalette. Vertex weight ids map to this
        /// </summary>
        BonePalette = 0x10,
        /// <summary>
        /// An int and a vector3 seemingly
        /// </summary>
        Unk11Values = 0x11,
        /// <summary>
        /// Skeletal Node
        /// </summary>
        Node = 0x4000,
    }

    public class AMOModelRoot : MHMainSection
    {
        public List<MHSection> sections = new List<MHSection>();
        public AMOModelRoot(MHSectionHeader hd) : base(hd) { }
        public AMOModelRoot(MHSectionHeader hd, BufferedStreamReaderBE<MemoryStream> sr) : base(hd, sr)
        {
            for (int i = 0; i < hd.subCount; i++)
            {
                sections.Add(MHTagFile.ReadMHSection(sr));
            }
        }
    }
    public class AMOModel : MHMainSection
    {
        public List<MHSection> sections = new List<MHSection>();
        public AMOModel(MHSectionHeader hd) : base(hd) { }
        public AMOModel(MHSectionHeader hd, BufferedStreamReaderBE<MemoryStream> sr) : base(hd, sr)
        {
            for (int i = 0; i < hd.subCount; i++)
            {
                sections.Add(MHTagFile.ReadMHSection(sr));
            }
        }
    }
    public class AMOMesh : MHMainSection
    {
        public List<MHSection> sections = new List<MHSection>();
        public AMOMesh(MHSectionHeader hd) : base(hd) { }
        public AMOMesh(MHSectionHeader hd, BufferedStreamReaderBE<MemoryStream> sr) : base(hd, sr)
        {
            for (int i = 0; i < hd.subCount; i++)
            {
                sections.Add(MHTagFile.ReadMHSection(sr));
            }
        }
    }
    public class AMOStripSetList : MHMainSection
    {
        public List<MHSection> sections = new List<MHSection>();
        public AMOStripSetList(MHSectionHeader hd) : base(hd) { }
        public AMOStripSetList(MHSectionHeader hd, BufferedStreamReaderBE<MemoryStream> sr) : base(hd, sr)
        {
            for (int i = 0; i < hd.subCount; i++)
            {
                sections.Add(MHTagFile.ReadMHSection(sr));
            }
        }
    }
    public class AMOMaterialList : MHMainSection
    {
        public List<AMOMaterial> materials = new List<AMOMaterial>();
        public AMOMaterialList(MHSectionHeader hd) : base(hd) { }
        public AMOMaterialList(MHSectionHeader hd, BufferedStreamReaderBE<MemoryStream> sr) : base(hd, sr)
        {
            for (int i = 0; i < hd.subCount; i++)
            {
                var mat = new AMOMaterial()
                {
                    header = new MHSectionHeader() { type0 = sr.ReadBE<ushort>(), type1 = sr.ReadBE<ushort>(), subCount = sr.ReadBE<int>(), size = sr.ReadBE<int>() },
                    DiffuseColor = sr.ReadBEV3(),
                    int_18 = sr.ReadBE<int>(),
                    AmbientColor = sr.ReadBEV3(),
                    flt_28 = sr.ReadBE<float>(),
                    UnkColor = sr.ReadBEV3(),
                    int_38 = sr.ReadBE<int>(),
                    flt_3C = sr.ReadBE<float>(),
                    int_40 = sr.ReadBE<int>(),
                };
                for (int j = 0; j < 0x33; j++)
                {
                    mat.unkValues.Add(sr.ReadBE<int>());
                }
                materials.Add(mat);
            }
        }
        public class AMOMaterial
        {
            public MHSectionHeader header;

            public Vector3 DiffuseColor;
            public int int_18;
            public Vector3 AmbientColor;
            public float flt_28;
            public Vector3 UnkColor;
            public int int_38;
            public float flt_3C;
            public int int_40;

            /// <summary>
            /// 0xCC worth of int values
            /// </summary>
            public List<int> unkValues = new List<int>();
        }
    }
    public class AMOTextureInfo : MHMainSection
    {
        public List<AMOUnkAObject> texInfoList = new List<AMOUnkAObject>();
        public AMOTextureInfo(MHSectionHeader hd) : base(hd) { }
        public AMOTextureInfo(MHSectionHeader hd, BufferedStreamReaderBE<MemoryStream> sr) : base(hd, sr)
        {
            for (int i = 0; i < hd.subCount; i++)
            {
                var mat = new AMOUnkAObject()
                {
                    header = new MHSectionHeader() { type0 = sr.ReadBE<ushort>(), type1 = sr.ReadBE<ushort>(), subCount = sr.ReadBE<int>(), size = sr.ReadBE<int>() },
                    textureId = sr.ReadBE<int>(),
                    int_04 = sr.ReadBE<int>(),
                    usht_08 = sr.ReadBE<ushort>(),
                    usht_0A = sr.ReadBE<ushort>(),
                };
                for (int j = 0; j < 0x3D; j++)
                {
                    mat.unkValues.Add(sr.ReadBE<int>());
                }
                texInfoList.Add(mat);
            }
        }
        public class AMOUnkAObject
        {
            public MHSectionHeader header;

            public int textureId;
            public int int_04;
            public ushort usht_08;
            public ushort usht_0A;

            /// <summary>
            /// 0xF4 worth of int values
            /// </summary>
            public List<int> unkValues = new List<int>();
        }
    }
    public class AHINodeList : MHMainSection
    {
        public List<MHSection> sections = new List<MHSection>();
        public AHINodeList(MHSectionHeader hd) : base(hd) { }
        public AHINodeList(MHSectionHeader hd, BufferedStreamReaderBE<MemoryStream> sr) : base(hd, sr)
        {
            for (int i = 0; i < hd.subCount; i++)
            {
                sections.Add(MHTagFile.ReadMHSection(sr));
            }
        }
    }
    public class MHMainSection : MHSection
    {
        public MHMainSectionType0 type0 { get { return (MHMainSectionType0)header.type0; } set { header.type0 = (ushort)value; } }
        public MHMainSectionType1 type1 { get { return (MHMainSectionType1)header.type0; } set { header.type0 = (ushort)value; } }
        public MHMainSection(MHSectionHeader hd) : base(hd) { }
        public MHMainSection(MHSectionHeader hd, BufferedStreamReaderBE<MemoryStream> sr) : base(hd, sr) { }
    }
    public class MHNode : MHSubSection
    {

        public AHINode node;
        public MHNode(MHSectionHeader hd) : base(hd) { }
        public MHNode(MHSectionHeader hd, BufferedStreamReaderBE<MemoryStream> sr) : base(hd, sr)
        {
            node = new AHINode()
            {
                nodeId = sr.ReadBE<int>(),
                parentId = sr.ReadBE<int>(),
                firstChildId = sr.ReadBE<int>(),
                nextSiblingId = sr.ReadBE<int>(),
                scale = sr.ReadBEV3(),
                matrixRow0 = sr.ReadBEV3(),
                matrixRow1 = sr.ReadBEV3(),
                matrixRow2 = sr.ReadBEV3(),
            };
            for (int i = 0; i < 0x30; i++)
            {
                node.unkValues.Add(sr.ReadBE<int>());
            }
        }
        public class AHINode
        {
            public int nodeId;
            /// <summary>
            /// Parent node id, -1 if no parent node.
            /// </summary>
            public int parentId;
            /// <summary>
            /// First child node id, -1 if no children.
            /// </summary>
            public int firstChildId;
            /// <summary>
            /// Id of next lowest sibling after current node, -1 if none.
            /// Root nodes are not considered siblings of root nodes.
            /// </summary>
            public int nextSiblingId;

            public Vector3 scale;
            public Vector3 matrixRow0;
            public Vector3 matrixRow1;
            public Vector3 matrixRow2;

            /// <summary>
            /// 0xC0 worth of int values
            /// </summary>
            public List<int> unkValues = new List<int>();
        }
    }
    public class MHIdList : MHSubSection
    {

        public List<int> stripValues = new List<int>();
        public MHIdList(MHSectionHeader hd) : base(hd) { }
        public MHIdList(MHSectionHeader hd, BufferedStreamReaderBE<MemoryStream> sr) : base(hd, sr)
        {
            for (int i = 0; i < hd.subCount; i++)
            {
                stripValues.Add(sr.ReadBE<int>());
            }
        }
    }
    public class MHModelFlags : MHSubSection
    {
        public byte bt_0;
        public byte bt_1;
        public byte bt_2;
        public byte bt_3;
        public MHModelFlags(MHSectionHeader hd) : base(hd) { }
        public MHModelFlags(MHSectionHeader hd, BufferedStreamReaderBE<MemoryStream> sr) : base(hd, sr)
        {
            bt_0 = sr.ReadBE<byte>();
            bt_1 = sr.ReadBE<byte>();
            bt_2 = sr.ReadBE<byte>();
            bt_3 = sr.ReadBE<byte>();
        }
    }
    public class MHStripSet : MHSubSection
    {
        public List<List<int>> strips = new List<List<int>>();
        public MHStripSet(MHSectionHeader hd) : base(hd) { }
        public MHStripSet(MHSectionHeader hd, BufferedStreamReaderBE<MemoryStream> sr) : base(hd, sr)
        {
            for (int i = 0; i < hd.subCount; i++)
            {
                var count = sr.ReadBE<ushort>();
                var unk = sr.ReadBE<ushort>();
                var strips = new List<int>();
                for (int j = 0; j < count; j++)
                {
                    strips.Add(sr.ReadBE<int>());
                }
                this.strips.Add(strips);
            }
        }
    }

    /// <summary>
    /// Values map to the model's full material list. A MHMaterialStripMapping associates this with each tristrip
    /// </summary>
    public class MHMaterialIdPalette : MHSubSection
    {
        public List<int> materialids = new List<int>();
        public MHMaterialIdPalette(MHSectionHeader hd) : base(hd) { }
        public MHMaterialIdPalette(MHSectionHeader hd, BufferedStreamReaderBE<MemoryStream> sr) : base(hd, sr)
        {
            for (int i = 0; i < hd.subCount; i++)
            {
                materialids.Add(sr.ReadBE<int>());
            }
        }
    }

    /// <summary>
    /// Values map based on their index to a strip and the value at a given index in stripMapping maps to a value in the MHMaterialIdPalette. 
    /// </summary>
    public class MHMaterialStripMapping : MHSubSection
    {
        public List<int> stripMapping = new List<int>();
        public MHMaterialStripMapping(MHSectionHeader hd) : base(hd) { }
        public MHMaterialStripMapping(MHSectionHeader hd, BufferedStreamReaderBE<MemoryStream> sr) : base(hd, sr)
        {
            for (int i = 0; i < hd.subCount; i++)
            {
                stripMapping.Add(sr.ReadBE<int>());
            }
        }
    }
    public class MHVertPositions : MHSubSection
    {
        public List<Vector3> positions = new List<Vector3>();
        public MHVertPositions(MHSectionHeader hd) : base(hd) { }
        public MHVertPositions(MHSectionHeader hd, BufferedStreamReaderBE<MemoryStream> sr) : base(hd, sr)
        {
            for (int i = 0; i < hd.subCount; i++)
            {
                positions.Add(sr.ReadBEV3());
            }
        }
    }
    public class MHVertNormals : MHSubSection
    {
        public List<Vector3> normals = new List<Vector3>();
        public MHVertNormals(MHSectionHeader hd) : base(hd) { }
        public MHVertNormals(MHSectionHeader hd, BufferedStreamReaderBE<MemoryStream> sr) : base(hd, sr)
        {
            for (int i = 0; i < hd.subCount; i++)
            {
                normals.Add(sr.ReadBEV3());
            }
        }
    }
    public class MHVertUVs : MHSubSection
    {
        public List<Vector2> uvs = new List<Vector2>();
        public MHVertUVs(MHSectionHeader hd) : base(hd) { }
        public MHVertUVs(MHSectionHeader hd, BufferedStreamReaderBE<MemoryStream> sr) : base(hd, sr)
        {
            for (int i = 0; i < hd.subCount; i++)
            {
                uvs.Add(sr.ReadBEV2());
            }
        }
    }
    public class MHVertColors : MHSubSection
    {
        public List<Vector4> colors = new List<Vector4>();
        public MHVertColors(MHSectionHeader hd) : base(hd) { }
        public MHVertColors(MHSectionHeader hd, BufferedStreamReaderBE<MemoryStream> sr) : base(hd, sr)
        {
            for (int i = 0; i < hd.subCount; i++)
            {
                colors.Add(sr.ReadBEV4());
            }
        }
    }
    public class MHVertWeights : MHSubSection
    {
        public List<List<MHWeightPair>> weightsPairs = new List<List<MHWeightPair>>();

        public MHVertWeights(MHSectionHeader hd) : base(hd) { }
        public MHVertWeights(MHSectionHeader hd, BufferedStreamReaderBE<MemoryStream> sr) : base(hd, sr)
        {
            for (int i = 0; i < hd.subCount; i++)
            {
                var count = sr.ReadBE<int>();
                var pairs = new List<MHWeightPair>();
                for (int j = 0; j < count; j++)
                {
                    pairs.Add(new MHWeightPair() { boneIndex = sr.ReadBE<int>(), boneWeight = sr.ReadBE<float>() });
                }
                weightsPairs.Add(pairs);
            }
        }

        public struct MHWeightPair
        {
            public int boneIndex;
            public float boneWeight;
        }
    }
    public class MHUnkEValues : MHSubSection
    {

        public List<List<int>> unkValuesList = new List<List<int>>();
        public MHUnkEValues(MHSectionHeader hd) : base(hd) { }
        public MHUnkEValues(MHSectionHeader hd, BufferedStreamReaderBE<MemoryStream> sr) : base(hd, sr)
        {
            for(int l = 0; l < hd.subCount; l++)
            {
                var count = sr.ReadBE<int>();
                var unkValues = new List<int>();
                for (int i = 0; i < count; i++)
                {
                    unkValues.Add(sr.ReadBE<int>());
                }
                unkValuesList.Add(unkValues);
            }
        }
    }
    public class MHUnkFValues : MHSubSection
    {

        public List<int> unkValues = new List<int>();
        public MHUnkFValues(MHSectionHeader hd) : base(hd) { }
        public MHUnkFValues(MHSectionHeader hd, BufferedStreamReaderBE<MemoryStream> sr) : base(hd, sr)
        {
            for (int i = 0; i < ((hd.size - 0xC) / 4); i++)
            {
                unkValues.Add(sr.ReadBE<int>());
            }
        }
    }
    public class MHBonePalette : MHSubSection
    {

        public List<int> bonePalette = new List<int>();
        public MHBonePalette(MHSectionHeader hd) : base(hd) { }
        public MHBonePalette(MHSectionHeader hd, BufferedStreamReaderBE<MemoryStream> sr) : base(hd, sr)
        {
            for (int i = 0; i < hd.subCount; i++)
            {
                bonePalette.Add(sr.ReadBE<int>());
            }
        }
    }
    public class MHUnk11Values : MHSubSection
    {

        public int int_00;
        public Vector3 vec3_04;
        public MHUnk11Values(MHSectionHeader hd) : base(hd) { }
        public MHUnk11Values(MHSectionHeader hd, BufferedStreamReaderBE<MemoryStream> sr) : base(hd, sr)
        {
            int_00 = sr.ReadBE<int>();
            vec3_04 = sr.ReadBEV3();
        }
    }

    public class MHSubSection : MHSection
    {
        public MHSubSectionType0 type0 { get { return (MHSubSectionType0)header.type0; } set { header.type0 = (ushort)value; } }
        public MHSubSectionType1 type1 { get { return (MHSubSectionType1)header.type0; } set { header.type0 = (ushort)value; } }

        public MHSubSection(MHSectionHeader hd) : base(hd) { }
        public MHSubSection(MHSectionHeader hd, BufferedStreamReaderBE<MemoryStream> sr) : base(hd, sr) { }
    }

    public class MHSection
    {
        public long position;
        public MHSectionHeader header;
        public MHSection(MHSectionHeader hd) { header = hd; }
        public MHSection(MHSectionHeader hd, BufferedStreamReaderBE<MemoryStream> sr) { position = sr.Position - 0xC; header = hd; }
    }

    public struct MHSectionHeader
    {
        public ushort type0;
        public ushort type1;
        public int subCount;
        public int size;
    }
}
