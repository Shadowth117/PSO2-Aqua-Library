using AquaModelLibrary.Data.DataTypes.SetLengthStrings;
using AquaModelLibrary.Data.PSO2.Aqua.AquaCommonData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaNodeData;
using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers;
using AquaModelLibrary.Helpers.PSO2;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using static AquaModelLibrary.Helpers.Extensions.ByteListExtension;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    //Though the NIFL format is used for storage, VTBF format tag references for data will be commented where appropriate. Some offset/reserve related things are NIFL only, however.
    public unsafe class AquaNode : AquaCommon
    {
        public NDTR ndtr = new NDTR();
        public List<NODE> nodeList = new List<NODE>();
        public List<NODO> nodoList = new List<NODO>();

        public override string[] GetEnvelopeTypes()
        {
            return new string[] {
            "aqn\0",
            "trn\0",
            };
        }

        public AquaNode() { }

        public AquaNode(byte[] file) : base(file) { }

        public AquaNode(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }

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

        public override void ReadVTBFFile(BufferedStreamReaderBE<MemoryStream> sr)
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
                        ParseNODE(data);
                        break;
                    case "NODO":
                        ParseNODO(data);
                        break;
                    default:
                        //Should mean it's done.
                        Debug.WriteLine($"Defaulted tag was: {tagType}");
                        break;
                }
            }
        }

        public void ParseNODE(List<Dictionary<int, object>> nodeRaw)
        {
            for (int i = 0; i < nodeRaw.Count; i++)
            {
                NODE node = new NODE();

                byte[] shorts = BitConverter.GetBytes((int)nodeRaw[i][0x03]);
                node.boneShort1 = (ushort)(shorts[0] * 0x100 + shorts[1]);
                node.boneShort2 = (ushort)(shorts[2] * 0x100 + shorts[3]);
                node.animatedFlag = (int)nodeRaw[i][0x0B];
                node.parentId = (int)nodeRaw[i][0x04];
                node.unkNode = (int)nodeRaw[i][0x0F];
                node.firstChild = (int)nodeRaw[i][0x05];
                node.nextSibling = (int)nodeRaw[i][0x06];
                node.const0_2 = (int)nodeRaw[i][0x0C];
                node.pos = (Vector3)nodeRaw[i][0x07];
                node.eulRot = (Vector3)nodeRaw[i][0x08];
                node.scale = (Vector3)nodeRaw[i][0x09];
                node.m1 = ((Vector4[])nodeRaw[i][0x0A])[0];
                node.m2 = ((Vector4[])nodeRaw[i][0x0A])[1];
                node.m3 = ((Vector4[])nodeRaw[i][0x0A])[2];
                node.m4 = ((Vector4[])nodeRaw[i][0x0A])[3];
                node.boneName = new PSO2String((byte[])nodeRaw[i][0x0D]);

                nodeList.Add(node);
            }
        }

        public byte[] toNODE()
        {
            List<byte> outBytes = new List<byte>();

            for (int i = 0; i < nodeList.Count; i++)
            {
                NODE node = nodeList[i];
                if (i == 0)
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFC));
                }
                else
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFE));
                }
                VTBFMethods.AddBytes(outBytes, 0x3, 0x9, BitConverter.GetBytes(node.boneShort1 * 0x10000 + node.boneShort2));
                VTBFMethods.AddBytes(outBytes, 0x4, 0x8, BitConverter.GetBytes(node.parentId));
                VTBFMethods.AddBytes(outBytes, 0xF, 0x8, BitConverter.GetBytes(node.unkNode));
                VTBFMethods.AddBytes(outBytes, 0x5, 0x8, BitConverter.GetBytes(node.firstChild));
                VTBFMethods.AddBytes(outBytes, 0x6, 0x8, BitConverter.GetBytes(node.nextSibling));
                VTBFMethods.AddBytes(outBytes, 0x7, 0x4A, 0x1, DataHelpers.ConvertStruct(node.pos));
                VTBFMethods.AddBytes(outBytes, 0x8, 0x4A, 0x1, DataHelpers.ConvertStruct(node.eulRot));
                VTBFMethods.AddBytes(outBytes, 0x9, 0x4A, 0x1, DataHelpers.ConvertStruct(node.scale));
                VTBFMethods.AddBytes(outBytes, 0xA, 0xCA, 0xA, 0x3, DataHelpers.ConvertStruct(node.m1));
                outBytes.AddRange(DataHelpers.ConvertStruct(node.m2));
                outBytes.AddRange(DataHelpers.ConvertStruct(node.m3));
                outBytes.AddRange(DataHelpers.ConvertStruct(node.m4));
                VTBFMethods.AddBytes(outBytes, 0xB, 0x9, BitConverter.GetBytes(node.animatedFlag));
                VTBFMethods.AddBytes(outBytes, 0xC, 0x8, BitConverter.GetBytes(node.const0_2));

                //Bone Name String
                string boneNameStr = node.boneName.GetString();
                VTBFMethods.AddBytes(outBytes, 0x80, 0x02, (byte)boneNameStr.Length, Encoding.UTF8.GetBytes(boneNameStr));
            }
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));

            VTBFMethods.WriteTagHeader(outBytes, "NODE", 0, (ushort)(nodeList.Count * 0xC + 1));

            return outBytes.ToArray();
        }

        public void ParseNODO(List<Dictionary<int, object>> nodoRaw)
        {
            if (nodoRaw[0].Keys.Count > 1)
            {
                for (int i = 0; i < nodoRaw.Count; i++)
                {
                    NODO nodo = new NODO();

                    byte[] shorts = BitConverter.GetBytes((int)nodoRaw[i][0x03]);
                    nodo.boneShort1 = (ushort)(shorts[0] * 0x100 + shorts[1]);
                    nodo.boneShort2 = (ushort)(shorts[2] * 0x100 + shorts[3]);
                    nodo.animatedFlag = (int)nodoRaw[i][0x0B];
                    nodo.parentId = (int)nodoRaw[i][0x04];
                    nodo.pos = (Vector3)nodoRaw[i][0x07];
                    nodo.eulRot = (Vector3)nodoRaw[i][0x08];
                    nodo.boneName = new PSO2String((byte[])nodoRaw[i][0x0D]);

                    nodoList.Add(nodo);
                }
            }
        }

        public byte[] toNODO()
        {
            List<byte> outBytes = new List<byte>();

            for (int i = 0; i < nodoList.Count; i++)
            {
                NODO nodo = nodoList[i];
                if (i == 0)
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFC));
                }
                else
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFE));
                }
                VTBFMethods.AddBytes(outBytes, 0x3, 0x9, BitConverter.GetBytes(nodo.boneShort1 * 0x10000 + nodo.boneShort2));
                VTBFMethods.AddBytes(outBytes, 0x4, 0x8, BitConverter.GetBytes(nodo.parentId));
                VTBFMethods.AddBytes(outBytes, 0x7, 0x4A, 0x1, DataHelpers.ConvertStruct(nodo.pos));
                VTBFMethods.AddBytes(outBytes, 0x8, 0x4A, 0x1, DataHelpers.ConvertStruct(nodo.eulRot));
                VTBFMethods.AddBytes(outBytes, 0xB, 0x9, BitConverter.GetBytes(nodo.animatedFlag));

                //Bone Name String
                string boneNameStr = nodo.boneName.GetString();
                VTBFMethods.AddBytes(outBytes, 0x80, 0x02, (byte)boneNameStr.Length, Encoding.UTF8.GetBytes(boneNameStr));
            }
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));

            VTBFMethods.WriteTagHeader(outBytes, "NODO", 0, (ushort)(nodoList.Count * 7 + 1));

            return outBytes.ToArray();
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

        public override byte[] GetBytesNIFL()
        {
            List<byte> outBytes = new List<byte>();
            List<int> nof0PointerLocations = new List<int>(); //Used for the NOF0 section
            int rel0SizeOffset;
            int nodeOffset = DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0x14, nodeList.Count);
            int effOffset = DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0x24, nodoList.Count);

            //REL0
            outBytes.AddRange(Encoding.UTF8.GetBytes("REL0"));
            rel0SizeOffset = outBytes.Count; //We'll fill this later
            outBytes.AddRange(BitConverter.GetBytes(0));
            outBytes.AddRange(BitConverter.GetBytes(0x10));
            outBytes.AddRange(BitConverter.GetBytes(0));

            ndtr.boneCount = nodeList.Count;
            ndtr.effCount = nodoList.Count;
            outBytes.AddRange(DataHelpers.ConvertStruct(ndtr));
            //Write nodes
            if (nodeList.Count > 0)
            {
                outBytes.SetByteListInt(nodeOffset, outBytes.Count);
                for (int i = 0; i < nodeList.Count; i++)
                {
                    outBytes.AddRange(DataHelpers.ConvertStruct(nodeList[i]));
                }
            }

            //Write effect nodes
            if (nodoList.Count > 0)
            {
                outBytes.SetByteListInt(effOffset, outBytes.Count);
                for (int i = 0; i < nodoList.Count; i++)
                {
                    outBytes.AddRange(DataHelpers.ConvertStruct(nodoList[i]));
                }
            }

            //Write REL0 Size
            outBytes.SetByteListInt(rel0SizeOffset, outBytes.Count - 0x8);

            //NOF0
            int NOF0Offset = outBytes.Count;
            int NOF0Size = (nof0PointerLocations.Count + 2) * 4;
            int NOF0FullSize = NOF0Size + 0x8;
            outBytes.AddRange(Encoding.UTF8.GetBytes("NOF0"));
            outBytes.AddRange(BitConverter.GetBytes(NOF0Size));
            outBytes.AddRange(BitConverter.GetBytes(nof0PointerLocations.Count));
            outBytes.AddRange(BitConverter.GetBytes(0));

            //Write pointer offsets
            for (int i = 0; i < nof0PointerLocations.Count; i++)
            {
                outBytes.AddRange(BitConverter.GetBytes(nof0PointerLocations[i]));
            }
            NOF0FullSize += outBytes.AlignWriter(0x10);

            //NEND
            outBytes.AddRange(Encoding.UTF8.GetBytes("NEND"));
            outBytes.AddRange(BitConverter.GetBytes(0x8));
            outBytes.AddRange(BitConverter.GetBytes(0));
            outBytes.AddRange(BitConverter.GetBytes(0));

            //Generate NIFL
            NIFL nifl = new NIFL();
            nifl.magic = BitConverter.ToInt32(Encoding.UTF8.GetBytes("NIFL"), 0);
            nifl.NIFLLength = 0x18;
            nifl.unkInt0 = 1;
            nifl.offsetAddition = 0x20;

            nifl.NOF0Offset = NOF0Offset;
            nifl.NOF0OffsetFull = NOF0Offset + 0x20;
            nifl.NOF0BlockSize = NOF0FullSize;
            nifl.padding0 = 0;

            //Write NIFL
            outBytes.InsertRange(0, DataHelpers.ConvertStruct(nifl));

            return outBytes.ToArray();
        }

        public override byte[] GetBytesVTBF()
        {
            List<byte> outBytes = new List<byte>();
            outBytes.AddRange(VTBFMethods.ToAQGFVTBF());
            outBytes.AddRange(VTBFMethods.ToROOT());
            ndtr.boneCount = nodeList.Count;
            ndtr.effCount = nodoList.Count;
            outBytes.AddRange(ndtr.GetBytesVTBF());
            outBytes.AddRange(toNODE());
            outBytes.AddRange(toNODO());

            return outBytes.ToArray();
        }

        private Matrix4x4 DefaultNewMatrix = Matrix4x4.Identity; 
        public void AddRootNode(string rootNodeName = "Root_Node", Matrix4x4? matrix = null)
        {
            for(int i = 0; i < nodeList.Count; i++)
            {
                var oldNode = nodeList[i];
                
                if(oldNode.parentId != -1)
                {
                    oldNode.parentId++;
                } else if (oldNode.parentId == -1)
                {
                    //We want to reparent this to the new node
                    oldNode.parentId = 0;
                }
                if (oldNode.firstChild != -1)
                {
                    oldNode.firstChild++;
                }
                if (oldNode.nextSibling != -1)
                {
                    oldNode.nextSibling++;
                }
                if (oldNode.unkNode != -1)
                {
                    oldNode.unkNode++;
                }

                nodeList[i] = oldNode;
            }

            for (int i = 0; i < nodoList.Count; i++)
            {
                var oldNodo = nodoList[i];
                if(oldNodo.parentId != -1)
                {
                    oldNodo.parentId++;
                }
                else if (oldNodo.parentId == -1)
                {
                    //We want to reparent this to the new node
                    oldNodo.parentId = 0;
                }

                nodoList[i] = oldNodo;
            }

            NODE node = new NODE();
            node.animatedFlag = 1;
            node.boneName.SetString(rootNodeName);
            if (matrix != null)
            {
                node.SetInverseBindPoseMatrixFromUninvertedMatrix((Matrix4x4)matrix);
                //Set "local" transforms one day if the odd transformation is ever figured out for pso2.
            }
            else
            {
                node.m1 = new Vector4(1, 0, 0, 0);
                node.m2 = new Vector4(0, 1, 0, 0);
                node.m3 = new Vector4(0, 0, 1, 0);
                node.m4 = new Vector4(0, 0, 0, 1);
            }
            node.parentId = -1;
            node.firstChild = 1;
            node.nextSibling = -1;
            node.unkNode = -1;

            nodeList.Insert(0, node);
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
