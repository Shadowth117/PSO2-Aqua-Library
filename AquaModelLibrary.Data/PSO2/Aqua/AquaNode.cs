using AquaModelLibrary.Data.PSO2.Aqua.AquaNodeData;
using AquaModelLibrary.Extensions.Readers;
using AquaModelLibrary.Helpers.PSO2;
using System.Diagnostics;
using System.IO;
using System.Numerics;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    //Though the NIFL format is used for storage, VTBF format tag references for data will be commented where appropriate. Some offset/reserve related things are NIFL only, however.
    public unsafe class AquaNode : AquaCommon
    {
        public NDTR ndtr = new NDTR();
        public List<NODE> nodeList = new List<NODE>();
        public List<NODO> nodoList = new List<NODO>();
        public static readonly string[] fileExtensions = new string[]
        {
            "aqn\0",
            "trn\0",
        };

        public AquaNode() { }

        public AquaNode(byte[] file)
        {
            Read(file, fileExtensions);
        }

        public AquaNode(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr, fileExtensions);
        }

        public override void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            ndtr = sr.Read<NDTR>();
            for (int i = 0; i < ndtr.boneCount; i++)
            {
                nodeList.Add(sr.Read<NODE>());
            }
            for (int i = 0; i < ndtr.effCount; i++)
            {
                nodoList.Add(sr.Read<NODO>());
            }
        }

        public override void ReadVTBFFile(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {            
            //Seek past vtbf tag
            sr.Seek(0x10, SeekOrigin.Current);          //VTBF + AQGF tags

            while (sr.Position < sr.BaseStream.Length)
            {
                var data = VTBFMethods.ReadVTBFTag(sr, out string tagType, out int ptrCount, out int entryCount);
                switch (tagType)
                {
                    case "ROOT":
                        //We don't do anything with this right now.
                        break;
                    case "NDTR":
                        ndtr = new NDTR(data);
                        break;
                    case "NODE":
                        nodeList = parseNODE(data);
                        break;
                    case "NODO":
                        nodoList = parseNODO(data);
                        break;
                    default:                        
                        //Should mean it's done.
                        Debug.WriteLine($"Defaulted tag was: {tagType}");
                        break;
                }
            }
        }

        public AquaNode(string name = "Root Node")
        {
            GenerateBasicAQNInternal(name, this);
        }

        public static AquaNode GenerateBasicAQN(string name = "Root Node")
        {
            AquaNode aqn = new AquaNode();
            GenerateBasicAQNInternal(name, aqn);

            return aqn;
        }

        private static void GenerateBasicAQNInternal(string name, AquaNode aqn)
        {
            aqn.ndtr = new NDTR();
            aqn.ndtr.boneCount = 1;
            NODE node = new NODE();
            node.animatedFlag = 1;
            node.boneName.SetString(name);
            node.m1 = new Vector4(1, 0, 0, 0);
            node.m2 = new Vector4(0, 1, 0, 0);
            node.m3 = new Vector4(0, 0, 1, 0);
            node.m4 = new Vector4(0, 0, 0, 1);
            node.pos = new Vector3();
            node.parentId = -1;
            node.nextSibling = -1;
            node.firstChild = -1;
            node.unkNode = -1;

            aqn.nodeList.Add(node);
        }

        public Dictionary<int, int> GetNodeToParentIdDict()
        {
            Dictionary<int, int> dict = new Dictionary<int, int>();
            for (int i = 0; i < nodeList.Count; i++)
            {
                var node = nodeList[i];
                dict.Add(i, node.parentId);
            }

            return dict;
        }
    }
}
