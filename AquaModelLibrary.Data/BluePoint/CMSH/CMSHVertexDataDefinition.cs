namespace AquaModelLibrary.Data.BluePoint.CMSH
{
    public struct CMSHVertexDataDefinition
    {
        public VertexMagic dataMagic; //0SOP for POS0 for position data, etc.
        public ushort dataFormat; //Formal definition of above?
        /// <summary>
        /// Always 2?
        /// </summary>
        public ushort usht_06; 
        public int dataStart; //Based from where the size starts counting for VertexData
        public int int_0C;

        public int dataSize; //Size of this data's buffer
        public int int_14;
    }
}
