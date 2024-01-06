namespace AquaModelLibrary.Data.Nova.Structures
{
    public class rddaStruct : BaseMeshData
    {
        //public int magic;
        //public int structSize;
        public int int_08;
        //public int toTagStruct; //Size of this struct? 0 When moving to a non tag struct

        public long md5_1; //Total md5 used as an id for other data. Unsure what the hash is made from, but it's stored here and in the linked area as a key.
        public long md5_2;

        //public int dataSize; //Size of the total vertex or face data
        public int int_24;
        //public int dataStartOffset; //Offset from start of meshdata to this data
        public int int_2C;

        public int int_30;
        public int int_34;
        public int int_38;
        //public short dataType; //2 - unknown, 3 vertex data, 7 face data
        public short short_3E;
        //More follows, but it might be variable so it's left out
    }
}
