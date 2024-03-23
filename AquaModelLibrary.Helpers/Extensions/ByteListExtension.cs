namespace AquaModelLibrary.Helpers.Extensions
{
    public static class ByteListExtension
    {
        public static bool AddAsBigEndian = false;
        private static Dictionary<string, int> reserveIntDict = new Dictionary<string, int>();

        public static void Reset()
        {
            AddAsBigEndian = false;
            reserveIntDict.Clear();
        }

        public static int ReserveInt(this List<byte> outBytes, string key)
        {
            reserveIntDict[key] = outBytes.Count;
            outBytes.AddRange(BitConverter.GetBytes(0));

            return reserveIntDict[key];
        }

        public static int FillInt(this List<byte> outBytes, string key, int value)
        {
            var ptrLocation = reserveIntDict[key];
            var newBytes = BitConverter.GetBytes(value);
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            for (int i = 0; i < 4; i++)
            {
                outBytes[ptrLocation + i] = newBytes[i];
            }
            reserveIntDict.Remove(key);

            return ptrLocation;
        }
        private static Dictionary<string, uint> reserveUintDict = new Dictionary<string, uint>();
        public static uint ReserveUint(this List<byte> outBytes, string key)
        {
            reserveUintDict[key] = (uint)outBytes.Count;
            outBytes.AddRange(BitConverter.GetBytes((uint)0));

            return reserveUintDict[key];
        }

        public static uint FillUint(this List<byte> outBytes, string key, uint value)
        {
            var ptrLocation = reserveUintDict[key];
            var newBytes = BitConverter.GetBytes(value);
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            for (int i = 0; i < 4; i++)
            {
                outBytes[(int)ptrLocation + i] = newBytes[i];
            }
            reserveUintDict.Remove(key);

            return ptrLocation;
        }
        private static Dictionary<string, int> reserveInt16Dict = new Dictionary<string, int>();
        public static int ReserveInt16(this List<byte> outBytes, string key)
        {
            reserveInt16Dict[key] = outBytes.Count;
            outBytes.AddRange(BitConverter.GetBytes((short)0));

            return reserveInt16Dict[key];
        }

        public static int FillInt16(this List<byte> outBytes, string key, short value)
        {
            var ptrLocation = reserveInt16Dict[key];
            var newBytes = BitConverter.GetBytes(value);
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            for (int i = 0; i < 2; i++)
            {
                outBytes[ptrLocation + i] = newBytes[i];
            }
            reserveInt16Dict.Remove(key);

            return ptrLocation;
        }
        private static Dictionary<string, uint> reserveUint16Dict = new Dictionary<string, uint>();
        public static uint ReserveUint16(this List<byte> outBytes, string key)
        {
            reserveUint16Dict[key] = (uint)outBytes.Count;
            outBytes.AddRange(BitConverter.GetBytes((ushort)0));

            return reserveUint16Dict[key];
        }

        public static uint FillUint16(this List<byte> outBytes, string key, ushort value)
        {
            var ptrLocation = reserveUint16Dict[key];
            var newBytes = BitConverter.GetBytes(value);
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            for (int i = 0; i < 2; i++)
            {
                outBytes[(int)ptrLocation + i] = newBytes[i];
            }
            reserveUint16Dict.Remove(key);

            return ptrLocation;
        }
        private static Dictionary<string, long> reserveLongDict = new Dictionary<string, long>();
        public static long ReserveLong(this List<byte> outBytes, string key)
        {
            reserveLongDict[key] = outBytes.Count;
            outBytes.AddRange(BitConverter.GetBytes((long)0));

            return reserveLongDict[key];
        }

        public static long FillLong(this List<byte> outBytes, string key, long value)
        {
            var ptrLocation = reserveLongDict[key];
            var newBytes = BitConverter.GetBytes(value);
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            for (int i = 0; i < 8; i++)
            {
                outBytes[(int)ptrLocation + i] = newBytes[i];
            }
            reserveLongDict.Remove(key);

            return ptrLocation;
        }
        private static Dictionary<string, ulong> reserveUlongDict = new Dictionary<string, ulong>();
        public static ulong ReserveUlong(this List<byte> outBytes, string key)
        {
            reserveUlongDict[key] = (uint)outBytes.Count;
            outBytes.AddRange(BitConverter.GetBytes((ulong)0));

            return reserveUlongDict[key];
        }

        public static ulong FillUlong(this List<byte> outBytes, string key, uint value)
        {
            var ptrLocation = reserveUlongDict[key];
            var newBytes = BitConverter.GetBytes(value);
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            for (int i = 0; i < 8; i++)
            {
                outBytes[(int)ptrLocation + i] = newBytes[i];
            }
            reserveUlongDict.Remove(key);

            return ptrLocation;
        }

        public static void AddValue(this List<byte> outBytes, ulong value)
        {
            var newBytes = BitConverter.GetBytes(value);
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            outBytes.AddRange(newBytes);
        }

        public static void AddValue(this List<byte> outBytes, long value)
        {
            var newBytes = BitConverter.GetBytes(value);
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            outBytes.AddRange(newBytes);
        }

        public static void AddValue(this List<byte> outBytes, uint value)
        {
            var newBytes = BitConverter.GetBytes(value);
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            outBytes.AddRange(newBytes);
        }

        public static void AddValue(this List<byte> outBytes, int value)
        {
            var newBytes = BitConverter.GetBytes(value);
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            outBytes.AddRange(newBytes);
        }

        public static void AddValue(this List<byte> outBytes, ushort value)
        {
            var newBytes = BitConverter.GetBytes(value);
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            outBytes.AddRange(newBytes);
        }

        public static void AddValue(this List<byte> outBytes, short value)
        {
            var newBytes = BitConverter.GetBytes(value);
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            outBytes.AddRange(newBytes);
        }

        public static void AddValue(this List<byte> outBytes, float value)
        {
            var newBytes = BitConverter.GetBytes(value);
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            outBytes.AddRange(newBytes);
        }

        public static void AddValue(this List<byte> outBytes, double value)
        {
            var newBytes = BitConverter.GetBytes(value);
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            outBytes.AddRange(newBytes);
        }

        public static void AddValue(this List<byte> outBytes, byte value)
        {
            var newBytes = BitConverter.GetBytes(value);
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            outBytes.AddRange(newBytes);
        }

        public static void AddValue(this List<byte> outBytes, sbyte value)
        {
            var newBytes = BitConverter.GetBytes(value);
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            outBytes.AddRange(newBytes);
        }

        public static int AlignWriter(this List<byte> outBytes, int alignmentValue, byte fillValue = 0)
        {
            //Align to int align
            int currentCount = outBytes.Count % alignmentValue;
            if (currentCount > 0)
            {
                int additions = alignmentValue - currentCount;
                var bytes = new byte[additions];

                //Fill with whatever is in fillValue that's not 0
                if (fillValue != 0)
                {
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        bytes[i] = fillValue;
                    }
                }
                outBytes.AddRange(bytes);

                return additions;
            }

            return 0;
        }

        public static int AlignFileEndWriter(this List<byte> outBytes, int alignmentValue, byte fillValue = 0)
        {
            //Align to int align
            int currentCount = outBytes.Count % alignmentValue;
            if (currentCount > 0)
            {
                int additions = alignmentValue - currentCount;
                var bytes = new byte[additions];

                //Fill with whatever is in fillValue that's not 0
                if (fillValue != 0)
                {
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        bytes[i] = fillValue;
                    }
                }
                outBytes.AddRange(bytes);

                return additions;
            }
            else
            {
                for (int i = 0; i < 0x10; i++)
                {
                    outBytes.Add(0);
                }
            }

            return 0;
        }

        /// <summary>
        /// Mainly for handling pointer offsets. Better handled by Reserve and Fill extensions in most cases. 
        /// </summary>
        public static int SetByteListInt(this List<byte> outBytes, int offset, int value)
        {
            if (offset != -1)
            {
                var newBytes = BitConverter.GetBytes(value);
                for (int i = 0; i < 4; i++)
                {
                    outBytes[offset + i] = newBytes[i];
                }

                return value;
            }

            return -1;
        }
    }
}
