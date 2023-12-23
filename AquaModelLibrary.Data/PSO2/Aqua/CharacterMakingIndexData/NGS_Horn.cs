namespace AquaModelLibrary.Data.PSO2.Aqua.CharacterMakingIndexData
{
    public class NGS_HornObject : BaseCMXObject
    {
        public NGS_Horn ngsHorn;

        public string dataString = null;
    }

    public struct NGS_Horn
    {
        public int id;
        public int dataStringPtr; //Name of the aqp, aqn, fltd, etc.
        public int reserve0;      //Always 0 so far.
    }
}
