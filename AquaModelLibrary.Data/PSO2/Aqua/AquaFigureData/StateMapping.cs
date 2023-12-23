namespace AquaModelLibrary.Data.PSO2.Aqua.AquaFigureData
{
    public class StateMappingObject
    {
        public StateMapping stateMappingStruct;
        public string name = null;
        public List<CommandObject> commands = new List<CommandObject>();
        public List<EffectMapObject> effects = new List<EffectMapObject>();
        public List<AnimMapObject> anims = new List<AnimMapObject>();
    }
    //Effect mappings have numerous structures based on the type id. 
    //For sanity purposes, these will be stored in lists since there are only 3 distinct data types known to exist in them, though more may actually be used.
    //For writeback, note that data is read in by order. 
    public class EffectMapObject
    {
        public int type;
        public List<int> intList = new List<int>();
        public List<float> fltList = new List<float>();
        public List<string> strList = new List<string>();
        public List<int> colorList = new List<int>();

        //Extra
        public bool knownType;
    }
    public struct StateMapping
    {
        public int namePtr;
        public int commandPtr;
        public int effectMapPtr;
        public int animMapPtr;
        public int commandCount;
        public int effectMapCount;
        public int animMapCount;
    }
}
