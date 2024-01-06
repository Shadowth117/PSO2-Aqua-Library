using AquaModelLibrary.Data.DataTypes.SetLengthStrings;
using System.Numerics;

namespace AquaModelLibrary.Data.Nova.Structures
{
    public class rttaStruct
    {
        public int magic;
        public int len;
        public int int_08;
        public int trueLen;

        public PSO2String nodeName;
        public int int_30; //Child node?
        public int meshNodePtr; //Pointer to a mesh group for some nodes
        public int int_38;
        public int parentNodeId; //Id is based on order of all nodes and not entirely on hierarchy. If it's 0 and there's no existing nodes, there is no parent. Can also be -1.

        public short childNodeCount;

        public Vector3 pos;
        public float posEndFloat;
        public Quaternion quatRot;
        public Vector3 scale;

        //Extra
        public Matrix4x4 nodeMatrix;
    }
}
