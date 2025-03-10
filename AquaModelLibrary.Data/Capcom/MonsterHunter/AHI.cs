using AquaModelLibrary.Helpers.Readers;
using System.Numerics;

namespace AquaModelLibrary.Data.Capcom.MonsterHunter
{
    /// <summary>
    /// Model and skeletal node data. Seems to be one or the other for an AHI instance.
    /// </summary>
    public class AHI
    {
        public static uint AHIMagic = 0xC0000000;

        public List<AHINode> nodesList = new List<AHINode>();
        public AHI(byte[] file)
        {
            Read(file);
        }
        public AHI(BufferedStreamReaderBE<MemoryStream> sr)
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

        private void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            var flt_00 = sr.ReadBE<float>();
            var structureCount = sr.ReadBE<int>(); //Including header structure
            var fileSize = sr.ReadBE<int>();
            var unkShort0 = sr.ReadBE<short>();
            var meshCount = sr.ReadBE<short>();

            var rootNodeCount = sr.ReadBE<short>();
            var unkShort1 = sr.ReadBE<short>();
            var dataOffset = sr.ReadBE<int>(); //Offset to nodes following root node ids
            List<AHIRootDataThing> rootNodeThinglist = new List<AHIRootDataThing>();
            for(int i = 0; i < rootNodeCount; i++)
            {
                rootNodeThinglist.Add(sr.ReadBE<AHIRootDataThing>());
            }
            for(int i = 0; i < rootNodeCount -1; i++)
            {
                nodesList.Add(sr.ReadBE<AHINode>());
            }
        }
        
        public struct AHIRootDataThing
        {
            public byte bt_0;
            public byte bt_1;
            public byte bt_2;
            public byte bt_3;
        }

        public struct AHINode
        {
            public short unkShort0;
            public short unkShort1;
            /// <summary>
            /// Always 1?
            /// </summary>
            public int unkInt0;
            public short shortFlags0;
            public short shortFlags1;
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

            //0xC0 of padding?
            public Vector4 unkData0;
            public Vector4 unkData1;
            public Vector4 unkData2;
            public Vector4 unkData3;

            public Vector4 unkData4;
            public Vector4 unkData5;
            public Vector4 unkData6;
            public Vector4 unkData7;

            public Vector4 unkData8;
            public Vector4 unkData9;
            public Vector4 unkData10;
            public Vector4 unkData11;
        }
    }
}
