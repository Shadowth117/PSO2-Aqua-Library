using AquaModelLibrary.Data.PSO2.Aqua.AquaCommonData;
using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers;
using AquaModelLibrary.Helpers.Extensions;
using System.Numerics;
using System.Text;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    public class CharacterMakingOffsets : AquaCommon
    {
        public List<NodeDataInfo> nodeDataInfo = new List<NodeDataInfo>();
        public List<NodeData> nodeData = new List<NodeData>();
        public override string[] GetEnvelopeTypes()
        {
            return new string[] {
            "cmo\0"
            };
        }

        public CharacterMakingOffsets() { }

        public CharacterMakingOffsets(byte[] file)
        {
            Read(file);
        }

        public CharacterMakingOffsets(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }

        public override void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            //Read offsets for
            int count = sr.Read<int>();
            int listOffset = sr.Read<int>();
            sr.Seek(offset + listOffset, SeekOrigin.Begin);

            for (int i = 0; i < count; i++)
            {
                nodeDataInfo.Add(sr.Read<CharacterMakingOffsets.NodeDataInfo>());
            }

            foreach (var info in nodeDataInfo)
            {
                CharacterMakingOffsets.NodeData data = new CharacterMakingOffsets.NodeData();

                //Read strings
                sr.Seek(offset + info.strOffsetList, SeekOrigin.Begin);
                for (int i = 0; i < info.count; i++)
                {
                    int strOffset = sr.Read<int>();
                    long bookmark = sr.Position;

                    sr.Seek(offset + strOffset, SeekOrigin.Begin);
                    data.nodeStrings.Add(sr.ReadCString());
                    sr.Seek(bookmark, SeekOrigin.Begin);
                }

                //Read data
                sr.Seek(offset + info.vectorListOffset, SeekOrigin.Begin);
                for (int i = 0; i < info.count; i++)
                {
                    data.nodeVectors.Add(sr.Read<Vector4>());
                }

                nodeData.Add(data);
            }

            /*
#if DEBUG
            StringBuilder output = new StringBuilder();
            output.AppendLine("CMO Data");
            for (int i = 0; i < cmo.nodeData.Count; i++)
            {
                var node = cmo.nodeData[i];
                output.AppendLine($"Set {i + 1}");
                for (int j = 0; j < node.nodeStrings.Count; j++)
                {
                    output.AppendLine($"{node.nodeStrings[j]} - {node.nodeVectors[j]}");
                }

                output.AppendLine("");
            }
            File.WriteAllText(inFilename + "_dump.txt", output.ToString());
#endif 
            */
        }
        public struct NodeDataInfo
        {
            public int id;
            public int count;
            public int strOffsetList;
            public int vectorListOffset;
        }
        public class NodeData
        {
            public List<string> nodeStrings = new List<string>();
            public List<Vector4> nodeVectors = new List<Vector4>();
        }

        /// <summary>
        /// Generates a CCO that defaults part counts to 1
        /// </summary>
        public static byte[] GenerateAccessoryCCO(CharacterMakingIndex cmx)
        {
            List<byte> outBytes = new List<byte>();
            List<int> nof0PointerLocations = new List<int>(); //Used for the NOF0 section

            int rel0SizeOffset = 0;

            //REL0
            outBytes.AddRange(Encoding.UTF8.GetBytes("REL0"));
            rel0SizeOffset = outBytes.Count; //We'll fill this later
            outBytes.AddRange(BitConverter.GetBytes(0));
            outBytes.AddRange(BitConverter.GetBytes(0));
            outBytes.AddRange(BitConverter.GetBytes(0));

            outBytes.AddRange(BitConverter.GetBytes(-1));

            //Write data
            foreach (var acce in cmx.accessoryDict.Keys)
            {
                outBytes.AddRange(BitConverter.GetBytes(acce));
                outBytes.AddRange(BitConverter.GetBytes(1));
            }

            //Write header data
            outBytes.SetByteListInt(rel0SizeOffset + 4, outBytes.Count);
            outBytes.AddRange(BitConverter.GetBytes(cmx.accessoryDict.Count));
            DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
            outBytes.AddRange(BitConverter.GetBytes(0x14));
            outBytes.AlignWriter(0x10);

            //Write REL0 Size
            outBytes.SetByteListInt(rel0SizeOffset, outBytes.Count - 0x8);

            //Write NOF0
            int NOF0Offset = outBytes.Count;
            int NOF0Size = (nof0PointerLocations.Count + 2) * 4;
            int NOF0FullSize = NOF0Size + 0x8;
            outBytes.AddRange(Encoding.UTF8.GetBytes("NOF0"));
            outBytes.AddRange(BitConverter.GetBytes(NOF0Size));
            outBytes.AddRange(BitConverter.GetBytes(nof0PointerLocations.Count));
            outBytes.AddRange(BitConverter.GetBytes(0x10));//Write pointer offsets

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
    }
}
