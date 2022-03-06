using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static AquaModelLibrary.AquaCommon;

namespace AquaModelLibrary.Nova.Structures
{
    public class rttaStruct
    {
        public int magic;
        public int len;
        public int int_08;
        public int trueLen;
        
        public PSO2String nodeName;
        public int int_30; //Child node?
        public int int_34;
        public int int_38;
        public int parentNodeId; //Id is based on order of all nodes and not entirely on hierarchy. If it's 0 and there's no existing nodes, there is no parent. Can also be -1.

        public short childNodeCount;

        public Vector3 pos;
        public float posEndFloat;
        public Quaternion quatRot;
        public Vector3 scale;
    }
}
