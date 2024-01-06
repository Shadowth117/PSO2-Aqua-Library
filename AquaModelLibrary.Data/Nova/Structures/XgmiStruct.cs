namespace AquaModelLibrary.Data.Nova.Structures
{
    public class XgmiStruct
    {
        public string stamCombinedId = null; //Id of the fuller texture this mip belongs to
        public string stamUniqueId = null; //Unique id of the texture without mip relation

        public int magic;
        public int len;
        public int int_08;
        public int paddingLen;

        public byte mipIdByte; //bit *0000000 defines a DIFFERENT texture id. This results in valid ranges of 0x0-0x7F and 0x80-0xFF for mips, seemingly
        public byte idByte1;
        public byte idByte2;
        public byte idByte3;
        public int texCatId; //Shared between mips, sometimes shared with unrelated textures
        public int int_18;
        public int int_1C;

        public byte dxtType;
        public byte bt_21;
        public byte bt_22;
        public byte bt_23;
        public int int_24;
        public ushort width;
        public ushort height;
        public byte bt_2C;
        public byte bt_2D;
        public byte alphaTesting;
        public byte bt_2F;

        public int int_30;
        public int int_34;
        public int int_38;
        public int int_3C;

        public long md5_1;
        public long md5_2;

        public int int_50;
        public int int_54;
        public int int_58;
        public int int_5C;

        public int int_60;
        public int int_64;
        public int int_68;
        public int int_6C;

        public byte[] GetBytes()
        {
            List<byte> outBytes = new List<byte>();

            outBytes.AddRange(BitConverter.GetBytes(magic));
            outBytes.AddRange(BitConverter.GetBytes(len));
            outBytes.AddRange(BitConverter.GetBytes(int_08));
            outBytes.AddRange(BitConverter.GetBytes(paddingLen));

            outBytes.Add(mipIdByte); //bit *0000000 defines a DIFFERENT texture id. This results in valid ranges of 0x0-0x7F and 0x80-0xFF for mips, seemingly
            outBytes.Add(idByte1);
            outBytes.Add(idByte2);
            outBytes.Add(idByte3);
            outBytes.AddRange(BitConverter.GetBytes(texCatId)); //Shared between mips, sometimes shared with unrelated textures
            outBytes.AddRange(BitConverter.GetBytes(int_18));
            outBytes.AddRange(BitConverter.GetBytes(int_1C));

            outBytes.Add(dxtType);
            outBytes.Add(bt_21);
            outBytes.Add(bt_22);
            outBytes.Add(bt_23);
            outBytes.AddRange(BitConverter.GetBytes(int_24));
            outBytes.AddRange(BitConverter.GetBytes(width));
            outBytes.AddRange(BitConverter.GetBytes(height));
            outBytes.Add(bt_2C);
            outBytes.Add(bt_2D);
            outBytes.Add(alphaTesting);
            outBytes.Add(bt_2F);

            outBytes.AddRange(BitConverter.GetBytes(int_30));
            outBytes.AddRange(BitConverter.GetBytes(int_34));
            outBytes.AddRange(BitConverter.GetBytes(int_38));
            outBytes.AddRange(BitConverter.GetBytes(int_3C));

            outBytes.AddRange(BitConverter.GetBytes(md5_1));
            outBytes.AddRange(BitConverter.GetBytes(md5_2));

            outBytes.AddRange(BitConverter.GetBytes(int_50));
            outBytes.AddRange(BitConverter.GetBytes(int_54));
            outBytes.AddRange(BitConverter.GetBytes(int_58));
            outBytes.AddRange(BitConverter.GetBytes(int_5C));

            outBytes.AddRange(BitConverter.GetBytes(int_60));
            outBytes.AddRange(BitConverter.GetBytes(int_64));
            outBytes.AddRange(BitConverter.GetBytes(int_68));
            outBytes.AddRange(BitConverter.GetBytes(int_6C));

            return outBytes.ToArray();
        }
    }
}
