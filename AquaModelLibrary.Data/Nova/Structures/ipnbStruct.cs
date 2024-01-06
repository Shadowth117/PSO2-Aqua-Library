namespace AquaModelLibrary.Data.Nova.Structures
{
    public class ipnbStruct
    {
        public int magic;
        public int len;
        public int int_08;
        public int paddedLen;

        public List<short> shortList = new List<short>(); //List is ((len - 0x10) / 2) long.
    }
}
