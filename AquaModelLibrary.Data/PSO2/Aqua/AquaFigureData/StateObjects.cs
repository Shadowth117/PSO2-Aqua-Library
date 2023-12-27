using AquaModelLibrary.Helpers.Readers;

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
            text = streamReader.ReadCStringValidOffset(rawStruct.textPtr, offset);

            struct0 = rawStruct.FS1UnkStruct0Ptr > 0x10 ? new FS1UnkStruct0Object(offset, rawStruct.FS1UnkStruct0Ptr, streamReader) : null;
            collision = rawStruct.collisionPtr > 0x10 ? new CollisionContainerObject(offset, rawStruct.collisionPtr, streamReader) : null;
            stateMap = rawStruct.stateMappingPtr > 0x10 ? new StateMappingObject(offset, rawStruct.stateMappingPtr, streamReader) : null;
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
