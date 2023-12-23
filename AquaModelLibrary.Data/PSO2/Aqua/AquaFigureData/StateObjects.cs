namespace AquaModelLibrary.Data.PSO2.Aqua.AquaFigureData
{
    public class StateObjects
    {
        public StateStruct rawStruct;
        public string text = null;
        public FS1UnkStruct0Object struct0 = null;
        public CollisionContainerObject collision = null;
        public StateMappingObject stateMap = null;
    }

    //Pointers of value 0x10 are null
    public struct StateStruct
    {
        public int textPtr;
        public int FS1UnkStruct0Ptr;
        public int collisionPtr;
        public int stateMappingPtr;

        public int int_10;
    }
}
