namespace AquaModelLibrary.Data.Nova.Structures
{
    public class ffubStruct : BaseMeshData
    {
        //public int magic;
        //public int structSize;
        public int int_08;
        //public int toTagStruct; //Size of this struct? 0 When moving to a non tag struct

        //public int dataSize; //Size of the total vertex or face data
        public int int_14;
        //public int dataStartOffset; //Offset from start of meshdata to this data
        public int int_1C;

        public int int_20;
        public int int_24;
        public int int_28;
        //public int dataType; //2 - unknown, 3 vertex data, 7 face data

        //More follows, but it might be variable so it's left out
    }
}
