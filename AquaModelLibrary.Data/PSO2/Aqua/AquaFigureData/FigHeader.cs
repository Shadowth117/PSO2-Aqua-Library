namespace AquaModelLibrary.Data.PSO2.Aqua.AquaFigureData
{
    public struct FigHeader
    {
        public int magic; //Should be fig/0 as plaintext
        public int int_04;
        public int version;
        public int int_08;

        public int attachTransformPtr;
        public int statePtr;
        public int attachTransformCount;
        public int stateCount;
    }
}
