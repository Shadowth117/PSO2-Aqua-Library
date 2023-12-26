using AquaModelLibrary.Extensions.Readers;

namespace AquaModelLibrary.Data.PSO2.Aqua.AquaFigureData
{
    public class StateObjects
    {
        public StateStruct rawStruct;
        public string text = null;
        public FS1UnkStruct0Object struct0 = null;
        public CollisionContainerObject collision = null;
        public StateMappingObject stateMap = null;

        public StateObjects(int offset, BufferedStreamReaderBE<MemoryStream> streamReader)
        {
            rawStruct = streamReader.Read<StateStruct>();
            text = streamReader.ReadCStringValidOffset(rawStruct.textPtr);

            struct0 = rawStruct.FS1UnkStruct0Ptr > 0x10 ? new FS1UnkStruct0Object(rawStruct.FS1UnkStruct0Ptr + offset, streamReader) : null;
            collision = ReadCollisionData(offset, rawStruct.collisionPtr, streamReader);
            stateMap = ReadStateMappingObject(offset, rawStruct.stateMappingPtr, streamReader);
        }
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
