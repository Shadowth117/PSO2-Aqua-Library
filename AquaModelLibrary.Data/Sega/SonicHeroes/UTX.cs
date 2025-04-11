using System.Text;

namespace AquaModelLibrary.Data.Sega.SonicHeroes
{
    public class UTX
    {
        public static List<List<string>> DumpUTX(byte[] utx)
        {
            byte[] test = new byte[4];
            Array.Copy(utx, 0, test, 0, 4);
            var leIntTest = BitConverter.ToInt32(test, 0);
            Array.Reverse(test);
            var beIntTest = BitConverter.ToInt32(test, 0);
            bool isBigEndian = leIntTest > beIntTest;

            var categoryCount = ReadInt32(utx, 0, isBigEndian);
            int address = 4;
            List<int> categoryCounts = new List<int>();
            for (int i = 0; i < categoryCount; i++)
            {
                categoryCounts.Add(ReadInt32(utx, address, isBigEndian));
                address += 4;
            }

            List<List<string>> utxStringListList = new();
            for (int i = 0; i < categoryCounts.Count; i++)
            {
                List<string> utxStringList = new();
                utxStringListList.Add(utxStringList);
                for (int j = 0; j < categoryCounts[i]; j++)
                {
                    var ptr = ReadInt32(utx, address, isBigEndian);
                    int textAddress = ptr;

                    while (true)
                    {
                        var value = ReadInt16(utx, textAddress, isBigEndian);
                        textAddress += 2;
                        if (value == 0)
                        {
                            //Align
                            if (textAddress % 4 != 0)
                            {
                                textAddress += 2;
                            }
                            break;
                        }
                    }
                    var byteLen = textAddress - ptr;
                    var unicodeBytes = new byte[byteLen];
                    Array.Copy(utx, ptr, unicodeBytes, 0, byteLen);
                    if(isBigEndian)
                    {
                        for(int k = 0; k < unicodeBytes.Length - 2; k += 2)
                        {
                            var temp = unicodeBytes[k];
                            unicodeBytes[k] = unicodeBytes[k + 1];
                            unicodeBytes[k + 1] = temp;
                        }
                    }
                    utxStringList.Add(Encoding.Unicode.GetString(unicodeBytes, 0, byteLen));
                    address += 4;
                }
            }

            return utxStringListList;
        }

        private static int ReadInt32(byte[] bytes, int address, bool isBigEndian)
        {
            byte[] intBytes = new byte[4];
            Array.Copy(bytes, address, intBytes, 0, 4);
            if(isBigEndian)
            {
                Array.Reverse(intBytes);
            }
            return BitConverter.ToInt32(intBytes, 0);
        }

        private static int ReadInt16(byte[] bytes, int address, bool isBigEndian)
        {
            byte[] intBytes = new byte[2];
            Array.Copy(bytes, address, intBytes, 0, 2);
            if (isBigEndian)
            {
                Array.Reverse(intBytes);
            }
            return BitConverter.ToInt16(intBytes, 0);
        }

        public static byte[] CompileUTX(List<List<string>> utxStrings, bool isBigEndian)
        {
            List<byte> outBytes = new();
            AddInt(outBytes, utxStrings.Count, isBigEndian);
            int totalStringCount = 0;
            for(int i = 0; i < utxStrings.Count; i++)
            {
                AddInt(outBytes, utxStrings[i].Count, isBigEndian);
                totalStringCount += utxStrings[i].Count;
            }

            List<byte> pointers = new();
            List<byte> strings = new();
            for(int i = 0; i < utxStrings.Count; i++)
            {
                for(int j = 0; j < utxStrings[i].Count; j++)
                {
                    AddInt(pointers, strings.Count + outBytes.Count + totalStringCount * 4, isBigEndian);
                    var utf16String = Encoding.Unicode.GetBytes(utxStrings[i][j]);
                    if (isBigEndian)
                    {
                        for (int k = 0; k < utf16String.Length - 2; k += 2)
                        {
                            var temp = utf16String[k];
                            utf16String[k] = utf16String[k + 1];
                            utf16String[k + 1] = temp;
                        }
                    }
                    strings.AddRange(utf16String);
                    if(strings.Count % 4 != 0)
                    {
                        strings.Add(0);
                        strings.Add(0);
                    }
                }
            }
            outBytes.AddRange(pointers);
            outBytes.AddRange(strings);

            return outBytes.ToArray();
        }

        private static void AddInt(List<byte> outBytes, int value, bool isBigEndian)
        {
            var valueBytes = BitConverter.GetBytes(value);
            if(isBigEndian)
            {
                Array.Reverse(valueBytes);
            }
            outBytes.AddRange(valueBytes);
        }
    }
}
