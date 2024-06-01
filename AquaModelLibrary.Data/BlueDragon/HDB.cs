namespace AquaModelLibrary.Data.BlueDragon
{
    public class HDB
    {
        public int magic;
        /// <summary>
        /// Always 0x10000
        /// </summary>
        public int int_04;
        public int fileSize;
        /// <summary>
        /// Always 0x4?
        /// </summary>
        public int int_0C;

        public int int_10;
        /// <summary>
        /// Always 0xC?
        /// </summary>
        public int int_14;
        public int int_18;
        public int int_1C;

        //Counts number of 0x10 length rows with data pointers 
        public int dataRowCount;

    }
}
