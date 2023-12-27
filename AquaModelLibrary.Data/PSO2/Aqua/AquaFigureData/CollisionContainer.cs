using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaFigureData
{
    public class CollisionContainerObject
    {
        public CollisionContainer colContainerStruct;
        public string collisionName = null;
        public List<int> colliderPtrs = new List<int>();
        public List<ColliderObject> colliders = new List<ColliderObject>();

        public CollisionContainerObject() { }

        public CollisionContainerObject(int offset, int address, BufferedStreamReaderBE<MemoryStream> streamReader)
        {
            streamReader.Seek(offset + address, SeekOrigin.Begin);
            colContainerStruct = streamReader.Read<CollisionContainer>();
            collisionName = streamReader.ReadCStringValidOffset(colContainerStruct.textPtr0, offset);

            streamReader.Seek(offset + colContainerStruct.subStructPtr, SeekOrigin.Begin);
            for (int i = 0; i < colContainerStruct.subStructCount; i++)
            {
                colliderPtrs.Add(streamReader.Read<int>());
            }

            colliders = new List<ColliderObject>();
            foreach (int ptr in colliderPtrs)
            {
                colliders.Add(ptr > 0x10 ? new ColliderObject(streamReader, ptr, offset) : null);
            }
        }
    }

    public struct CollisionContainer
    {
        public int textPtr0;
        public int subStructPtr;
        public int subStructCount;
    }
}
