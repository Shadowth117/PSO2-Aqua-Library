using AquaModelLibrary.Data.PSO2.Aqua.AquaCommonData;
using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers;
using AquaModelLibrary.Helpers.Extensions;
using System.Numerics;
using System.Text;
using System.IO;

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

        public CharacterMakingOffsets(byte[] file) : base(file) { }

        public CharacterMakingOffsets(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }

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
    }
}
