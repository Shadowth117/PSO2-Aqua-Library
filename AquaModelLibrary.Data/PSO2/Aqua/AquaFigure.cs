using AquaModelLibrary.Data.PSO2.Aqua.AquaFigureData;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    //Credit to DeathCream for doing a first pass on documenting the format
    public class AquaFigure : AquaCommon
    {
        public FigHeader figHeader;
        public List<int> attachTransformPtrList = new List<int>();
        public List<int> unkPtr1List = new List<int>();
        public List<AttachTransformObject> attachTransforms = new List<AttachTransformObject>();
        public List<AttachTransformObject> attachTransformsExtra = new List<AttachTransformObject>();
        public List<StateObjects> stateStructs = new List<StateObjects>();
    }
}
