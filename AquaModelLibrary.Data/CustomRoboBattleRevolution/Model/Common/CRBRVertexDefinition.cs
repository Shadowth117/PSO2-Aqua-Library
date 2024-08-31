namespace AquaModelLibrary.Data.CustomRoboBattleRevolution.Model.Common
{
    public struct CRBRVertexDefinition
    {
        public CRBRVertexType dataType;
        /// <summary>
        /// 2 for 8 bit, 3 for 16 bit index
        /// </summary>
        public int size;
        /// <summary>
        /// 1 for position and UVs, 0 for others
        /// </summary>
        public int int_08;
        /// <summary>
        /// Always 4?
        /// </summary>
        public int int_0C;

        public int strideInBytes;
        public int dataOffset;
    }
}
