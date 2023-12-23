using System.Numerics;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    public class CharacterMakingOffsets : AquaCommon
    {
        public List<NodeDataInfo> nodeDataInfo = new List<NodeDataInfo>();
        public List<NodeData> nodeData = new List<NodeData>();

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
