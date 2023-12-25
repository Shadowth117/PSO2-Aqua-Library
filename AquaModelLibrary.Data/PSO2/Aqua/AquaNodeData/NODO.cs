using AquaModelLibrary.Data.DataTypes.SetLengthStrings;
using System.Numerics;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaNodeData
{
    //A stripped down variant of NODE used for effect nodes. 
    public struct NODO
    {
        public ushort boneShort1;  //0x3, type 0x9 //First 2 bytes of this.
        public ushort boneShort2;  //Latter 2 bytes of above.
        public int animatedFlag;   //0xB, type 0x9 //Should generally be 1. I assume this is what it is based on PSU's bone format
        public int parentId;       //0x4, type 0x8
        public int const_0_2;      //0xC, type 0x9?
        public Vector3 pos;        //0x7, type 0x4A, 0x1
        public int const0_4;
        public Vector3 eulRot;     //0x8, type 0x4A, 0x1
        public int const0_5;
        public PSO2String boneName; //0xD, type 0x2

        //We don't do this for NODE since the result doesn't always match the expected result for unknown reasons
        public Matrix4x4 GetLocalTransformMatrix()
        {
            var matrix = Matrix4x4.Identity;
            var rotation = Matrix4x4.CreateRotationX((float)(eulRot.X * Math.PI / 180)) *
               Matrix4x4.CreateRotationY((float)(eulRot.Y * Math.PI / 180)) *
               Matrix4x4.CreateRotationZ((float)(eulRot.Z * Math.PI / 180));

            matrix *= rotation;
            matrix *= Matrix4x4.CreateTranslation(pos);

            return matrix;
        }
    }
}
