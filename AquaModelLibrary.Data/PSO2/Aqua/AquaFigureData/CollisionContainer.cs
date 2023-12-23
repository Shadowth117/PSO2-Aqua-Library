namespace AquaModelLibrary.Data.PSO2.Aqua.AquaFigureData
{
    public class CollisionContainerObject
    {
        public CollisionContainer colContainerStruct;
        public string collisionName = null;
        public List<int> colliderPtrs = new List<int>();
        public List<ColliderObject> colliders = new List<ColliderObject>();
    }

    public struct CollisionContainer
    {
        public int textPtr0;
        public int subStructPtr;
        public int subStructCount;
    }
}
