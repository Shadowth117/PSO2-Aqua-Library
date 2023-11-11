using System;
using System.Collections.Generic;

namespace AquaModelLibrary.Extra
{
    public static class ByteListExtension
    {
        public static bool AddAsBigEndian = false;
        private static Dictionary<string, int> reserveIntDict = new Dictionary<string, int>();
        public static void ReserveInt(this List<byte> outBytes, string key)
        {
            reserveIntDict[key] = outBytes.Count;
            outBytes.AddRange(BitConverter.GetBytes((int)0));
        }

        public static void FillInt(this List<byte> outBytes, string key, int value)
        {
            var newBytes = BitConverter.GetBytes(value);
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            for (int i = 0; i < 4; i++)
            {
                outBytes[reserveIntDict[key] + i] = newBytes[i];
            }
        }
        private static Dictionary<string, uint> reserveUintDict = new Dictionary<string, uint>();
        public static void ReserveUint(this List<byte> outBytes, string key)
        {
            reserveUintDict[key] = (uint)outBytes.Count;
            outBytes.AddRange(BitConverter.GetBytes((uint)0));
        }

        public static void FillUint(this List<byte> outBytes, string key, uint value)
        {
            var newBytes = BitConverter.GetBytes(value);
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            for (int i = 0; i < 4; i++)
            {
                outBytes[(int)reserveUintDict[key] + i] = newBytes[i];
            }
        }
        private static Dictionary<string, int> reserveInt16Dict = new Dictionary<string, int>();
        public static void ReserveInt16(this List<byte> outBytes, string key)
        {
            reserveInt16Dict[key] = outBytes.Count;
            outBytes.AddRange(BitConverter.GetBytes((short)0));
        }

        public static void FillInt16(this List<byte> outBytes, string key, short value)
        {
            var newBytes = BitConverter.GetBytes(value);
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            for (int i = 0; i < 2; i++)
            {
                outBytes[reserveInt16Dict[key] + i] = newBytes[i];
            }
        }
        private static Dictionary<string, uint> reserveUint16Dict = new Dictionary<string, uint>();
        public static void ReserveUint16(this List<byte> outBytes, string key)
        {
            reserveUint16Dict[key] = (uint)outBytes.Count;
            outBytes.AddRange(BitConverter.GetBytes((ushort)0));
        }

        public static void FillUint16(this List<byte> outBytes, string key, ushort value)
        {
            var newBytes = BitConverter.GetBytes(value);
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            for (int i = 0; i < 2; i++)
            {
                outBytes[(int)reserveUint16Dict[key] + i] = newBytes[i];
            }
        }
        private static Dictionary<string, long> reserveLongDict = new Dictionary<string, long>();
        public static void ReserveLong(this List<byte> outBytes, string key)
        {
            reserveLongDict[key] = outBytes.Count;
            outBytes.AddRange(BitConverter.GetBytes((long)0));
        }

        public static void FillLong(this List<byte> outBytes, string key, long value)
        {
            var newBytes = BitConverter.GetBytes(value);
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            for (int i = 0; i < 8; i++)
            {
                outBytes[(int)reserveLongDict[key] + i] = newBytes[i];
            }
        }
        private static Dictionary<string, ulong> reserveUlongDict = new Dictionary<string, ulong>();
        public static void ReserveUlong(this List<byte> outBytes, string key)
        {
            reserveUintDict[key] = (uint)outBytes.Count;
            outBytes.AddRange(BitConverter.GetBytes((ulong)0));
        }

        public static void FillUlong(this List<byte> outBytes, string key, uint value)
        {
            var newBytes = BitConverter.GetBytes(value);
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            for (int i = 0; i < 8; i++)
            {
                outBytes[(int)reserveUintDict[key] + i] = newBytes[i];
            }
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

        public static int AlignWrite(this List<byte> outBytes, int alignmentValue, byte fillValue = 0)
        {
            //Align to int align
            int currentCount = outBytes.Count % alignmentValue;
            if(currentCount > 0)
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
    }
}
