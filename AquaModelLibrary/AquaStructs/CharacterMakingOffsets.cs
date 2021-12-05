using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary
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
