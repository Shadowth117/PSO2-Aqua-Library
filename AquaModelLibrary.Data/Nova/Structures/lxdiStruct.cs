namespace AquaModelLibrary.Data.Nova.Structures
{
    public class lxdiStruct
    {
        public int magic;
        public int len;
        public int int_08;
        public int paddedLen;

        public int int_10;
        public int int_14;
        public int int_18;
        public int int_1C;

        public long md5_1;
        public long md5_2;

        public long md5_2_1; //Second instance of md5. Always the same?
        public long md5_2_2;
    }
}
