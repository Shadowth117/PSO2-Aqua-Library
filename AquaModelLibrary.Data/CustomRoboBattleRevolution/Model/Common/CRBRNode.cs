using System.Numerics;

namespace AquaModelLibrary.Data.CustomRoboBattleRevolution.Model.Common
{
    public class CRBRNode
    {
        public int int_00;
        /// <summary>
        /// Value of 0x10 means there's a child, 0 means no child
        /// </summary>
        public byte childEnabledFlag;
        /// <summary>
        /// ?
        /// </summary>
        public byte unkFlag1;
        /// <summary>
        /// ?
        /// </summary>
        public byte unkFlag2;
        /// <summary>
        /// Value of 0x80 means there's a mesh attached
        /// </summary>
        public byte meshFlag;
        public int childOffset;
        public int siblingOffset;

        public int meshOffset;
        /// <summary>
        /// Stored in radians
        /// </summary>
        public Vector3 eulerRotation;
        public Vector3 scale;
        public Vector3 translation;

        //Padding to 0x20?
        public int int_38;
        public int int_3C;
    }
}
