using Newtonsoft.Json;
using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using static AquaModelLibrary.AquaObjectMethods;

namespace AquaModelLibrary
{
    public unsafe class AquaCommon
    {
        public VTBF vtbf;
        public NIFL nifl;
        public REL0 rel0;

        public NOF0 nof0;
        public NEND nend;

        public struct VTBF
        {
            public int magicVTBF;
            public int size; //VTBF Size?
            public int magicAQGF; //AQGF, presumably AQua Game File
            public short unkShort0;
            public short unkShort1;
            public int magicVTC0; //special tag preceeding true tags in VTBF format. Always followed by a size int indicating the lenghh of the true tag and its data
            public int vtc0Size;
            public int magicROOT;
            public short unkTagShort;
            public short tagDataCount;
            public short tagDataSet0;
            public short tagDataSet0Length;
            public byte[] ROOTString; //String in ROOT of length tagDataSet0Length. In final, seemingly always says "hnd2aqg ver.1.61 Build: Feb 28 2012 18:46:06". Alphas note earlier dates.
        }

        public struct NIFL
        {
            public int magic;
            public int NIFLLength; //Length of NIFL after first 0x8
            public int unkInt0; //Always 1
            public int offsetAddition; //Full size of NIFL

            public int NOF0Offset; //Offset of NOF0 from NIFL header end
            public int NOF0OffsetFull; //Offset of NOF0 from NIFL header start
            public int NOF0BlockSize; //Size of NOF0 struct
            public int padding0;
        }

        public struct REL0
        {
            public int magic;
            public int REL0Size; //REL0 is the container of general data in NIFL format
            public int REL0DataStart; //Always 0x10 for models, skeletons, and anims. Matters most for other filetypes where the REL structure is used more directly.
            public int version;
        }

        public struct NOF0
        {
            public int magic;
            public int NOF0Size; //Size of NOF0 data
            public int NOF0EntryCount; //Number of entries in NOF0 data
            public int NOF0DataSizeStart;
            public List<int> relAddresses;
            public List<int> paddingToAlign;
        }

        public struct NEND
        {
            public int magic;
            public int size; //Size of NEND data; Always 0x8
            public double padding0;
        }

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

                    int end = this.GetLength();
                    byte[] text = new byte[end];
                    for (int i = 0; i < end; i++)
                    {
                        text[i] = stringArray[i];
                    }
                    finalText = System.Text.Encoding.UTF8.GetString(text);
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
            /*
            public override bool Equals(object o)
            {
                return Equals((PSO2String)o);
            }*/

            public bool Equals(PSO2String c)
            {
                var cArr = c.GetBytes();
                // Optimization for a common success case.
                if (Object.ReferenceEquals(this, c))
                {
                    return true;
                }

                // If run-time types are not exactly the same, return false.
                if (this.GetType() != c.GetType())
                {
                    return false;
                }

                for(int i = 0; i < 0x20; i++)
                {
                    if(stringArray[i] != cArr[i])
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
                for(int i = 0; i < 0x20; i++)
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

                    int end = this.GetLength();
                    byte[] text = new byte[end];
                    for (int i = 0; i < end; i++)
                    {
                        text[i] = stringArray[i];
                    }
                    finalText = System.Text.Encoding.UTF8.GetString(text);
                    return GetPSO2String(arr, GetLength());
                }
            }

            public void SetBytes(byte[] newBytes)
            {
                if(newBytes == null)
                {
                    newBytes = new byte[0];
                }
                for (int i = 0; i < 0x20; i++)
                {
                    if(i < newBytes.Length)
                    {
                        stringArray[i] = newBytes[i];
                    } else
                    {
                        stringArray[i] = 0;
                    }
                }
            }

            public void SetString(string str)
            {
                if(str == null)
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

        public unsafe struct PSO2Stringx30
        {
            [JsonIgnore]
            public fixed byte stringArray[0x30];
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
            
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public static PSO2Stringx30 GeneratePSO2String(byte[] bytes)
            {
                var str = new PSO2Stringx30();
                str.SetBytes(bytes);

                return str;
            }

            public static PSO2Stringx30 GeneratePSO2String(string newString)
            {
                var str = new PSO2Stringx30();
                str.SetString(newString);

                return str;
            }

            public override bool Equals(object o)
            {
                return Equals((PSO2Stringx30)o);
            }

            public bool Equals(PSO2Stringx30 c)
            {
                var cArr = c.GetBytes();
                // Optimization for a common success case.
                if (Object.ReferenceEquals(this, c))
                {
                    return true;
                }

                // If run-time types are not exactly the same, return false.
                if (this.GetType() != c.GetType())
                {
                    return false;
                }

                for (int i = 0; i < 0x30; i++)
                {
                    if (stringArray[i] != cArr[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            public static bool operator ==(PSO2Stringx30 lhs, PSO2Stringx30 rhs)
            {
                return lhs.Equals(rhs);
            }

            public static bool operator !=(PSO2Stringx30 lhs, PSO2Stringx30 rhs) => !(lhs == rhs);

            //Sometimes strings don't convert to the expected character set (Possibly sega setting in Unicode chars without warning?) This can help deal with that
            public int GetLength()
            {
                for (int j = 0; j < 0x30; j++)
                {
                    if (stringArray[j] == 0)
                    {
                        return j;
                    }
                }

                return 0x30;
            }

            public byte[] GetBytes()
            {
                byte[] unfixedBytes = new byte[0x20];
                for (int i = 0; i < 0x30; i++)
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

                    int end = this.GetLength();
                    byte[] text = new byte[end];
                    for (int i = 0; i < end; i++)
                    {
                        text[i] = stringArray[i];
                    }
                    finalText = System.Text.Encoding.UTF8.GetString(text);
                    return GetPSO2String(arr, GetLength());
                }
            }

            public void SetBytes(byte[] newBytes)
            {
                if (newBytes == null)
                {
                    newBytes = new byte[0];
                }
                for (int i = 0; i < 0x30; i++)
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
                for (int i = 0; i < 0x30; i++)
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

        public static bool isEqualVec4(Vector4 a, Vector4 b)
        {
            if (a.X != b.X)
            {
                return false;
            }
            if (a.Y != b.Y)
            {
                return false;
            }
            if (a.Z != b.Z)
            {
                return false;
            }
            if (a.W != b.W)
            {
                return false;
            }

            return true;
        }

        public static NOF0 readNOF0(BufferedStreamReader streamReader)
        {
            NOF0 nof0 = new NOF0();
            nof0.magic = streamReader.Read<int>();
            nof0.NOF0Size = streamReader.Read<int>();
            nof0.NOF0EntryCount = streamReader.Read<int>();
            nof0.NOF0DataSizeStart = streamReader.Read<int>();
            nof0.relAddresses = new List<int>();

            for (int nofEntry = 0; nofEntry < nof0.NOF0EntryCount; nofEntry++)
            {
                nof0.relAddresses.Add(streamReader.Read<int>());
            }

            return nof0;
        }

        public static List<uint> GetNOF0PointedValues(NOF0 nof0, BufferedStreamReader streamReader, int offset)
        {
            List<uint> addresses = new List<uint>();

            for(int i = 0; i < nof0.relAddresses.Count; i++)
            {
                streamReader.Seek(nof0.relAddresses[i] + offset, System.IO.SeekOrigin.Begin);
                addresses.Add(streamReader.Read<uint>());
            }

            return addresses;
        }
    }
}
