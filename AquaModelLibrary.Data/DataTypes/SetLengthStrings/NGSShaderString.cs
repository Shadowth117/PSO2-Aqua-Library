using System.Text;
using System.Text.Json.Serialization;
using static AquaModelLibrary.Data.DataTypes.SetLengthStrings.SetLengthHelper;

namespace AquaModelLibrary.Data.DataTypes.SetLengthStrings
{

    public unsafe struct NGSShaderString
    {
        [JsonIgnore]
        public fixed byte stringArray[0xA];
        public string curString
        {
            get
            {
                return GetString();
            }
            set
            {
                SetString(value);
            }
        }

        //Sometimes strings don't convert to the expected character set (Possibly sega setting in Unicode chars without warning?) This can help deal with that
        public int GetLength()
        {
            for (int j = 0; j < 0xA; j++)
            {
                if (stringArray[j] == 0)
                {
                    return j;
                }
            }

            return 0xA;
        }

        public byte[] GetBytes()
        {
            byte[] unfixedBytes = new byte[0xA];
            for (int i = 0; i < 0xA; i++)
            {
                unfixedBytes[i] = stringArray[i];
            }
            return unfixedBytes;
        }

        public unsafe string GetString()
        {
            fixed (byte* arr = stringArray)
            {
                string finalText;

                int end = GetLength();
                byte[] text = new byte[end];
                for (int i = 0; i < end; i++)
                {
                    text[i] = stringArray[i];
                }
                finalText = Encoding.UTF8.GetString(text);
                return GetPSO2String(arr, GetLength());
            }
        }

        public void SetBytes(byte[] newBytes)
        {
            for (int i = 0; i < 0xA; i++)
            {
                if (i < newBytes.Length)
                {
                    stringArray[i] = newBytes[i];
                }
                else
                {
                    stringArray[i] = 0;
                }
            }
        }

        public void SetString(string str)
        {
            byte[] strArr = Encoding.UTF8.GetBytes(str);
            for (int i = 0; i < 0xA; i++)
            {
                if (i < strArr.Length)
                {
                    stringArray[i] = strArr[i];
                }
                else
                {
                    stringArray[i] = 0;
                }
            }
        }
    }
}
