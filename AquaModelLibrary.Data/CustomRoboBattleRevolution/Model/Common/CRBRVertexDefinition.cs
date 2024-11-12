using AquaModelLibrary.Data.Gamecube;

namespace AquaModelLibrary.Data.CustomRoboBattleRevolution.Model.Common
{
    public struct CRBRVertexDefinition
    {
        public CRBRVertexType dataType;
        /// <summary>
        /// 2 for 8 bit, 3 for 16 bit index
        /// </summary>
        public int indexSize;
        /// <summary>
        /// 1 for position and UVs, 0 for others
        /// </summary>
        public int int_08;
        public GCDataType dataFormat;

        public short sht_10;
        public short strideInBytes;
        public int dataOffset;
    }
}
