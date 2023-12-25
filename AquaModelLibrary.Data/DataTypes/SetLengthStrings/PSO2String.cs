using System.Text;
using System.Text.Json.Serialization;
using static AquaModelLibrary.Data.DataTypes.SetLengthStrings.SetLengthHelper;

namespace AquaModelLibrary.Data.DataTypes.SetLengthStrings
{
    public unsafe struct PSO2String
    {
        [JsonIgnore]
        public fixed byte stringArray[0x20];
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

        public PSO2String(byte[] bytes)
        {
            SetBytes(bytes);
        }

        public PSO2String(string newString)
        {
            SetString(newString);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static PSO2String GeneratePSO2String(byte[] bytes)
        {
            var str = new PSO2String();
            str.SetBytes(bytes);

            return str;
        }

        public static PSO2String GeneratePSO2String(string newString)
        {
            var str = new PSO2String();
            str.SetString(newString);

            return str;
        }

#pragma warning disable CS8765
        public override bool Equals(object o)
        {
            return Equals((PSO2String)o);
        }
#pragma warning restore CS8765

        public bool Equals(PSO2String c)
        {
            var cArr = c.GetBytes();

            // Optimization for a common success case.
#pragma warning disable CA2013
            if (ReferenceEquals(this, c))
            {
                return true;
            }
#pragma warning restore CA2013

            // If run-time types are not exactly the same, return false.
            if (GetType() != c.GetType())
            {
                return false;
            }

            for (int i = 0; i < 0x20; i++)
            {
                if (stringArray[i] != cArr[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool operator ==(PSO2String lhs, PSO2String rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(PSO2String lhs, PSO2String rhs) => !(lhs == rhs);

        //Sometimes strings don't convert to the expected character set (Possibly sega setting in Unicode chars without warning?) This can help deal with that
        public int GetLength()
        {
            for (int j = 0; j < 0x20; j++)
            {
                if (stringArray[j] == 0)
                {
                    return j;
                }
            }

            return 0x20;
        }

        public byte[] GetBytes()
        {
            byte[] unfixedBytes = new byte[0x20];
            for (int i = 0; i < 0x20; i++)
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
            if (newBytes == null)
            {
                newBytes = new byte[0];
            }
            for (int i = 0; i < 0x20; i++)
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
            if (str == null)
            {
                str = "";
            }
            byte[] strArr = Encoding.UTF8.GetBytes(str);
            for (int i = 0; i < 0x20; i++)
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
