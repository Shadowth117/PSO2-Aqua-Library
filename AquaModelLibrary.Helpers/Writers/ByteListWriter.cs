using System.Numerics;

namespace AquaModelLibrary.Helpers.Writers
{
    public class ByteListWriter : List<byte>
    {
        public bool AddAsBigEndian = false;
        public Dictionary<string, bool> writeChecks = new Dictionary<string, bool>();

        public void Reset()
        {
            AddAsBigEndian = false;
            reserveIntDict.Clear();
            reserveUintDict.Clear();
            reserveInt16Dict.Clear();
            reserveUint16Dict.Clear();
            reserveLongDict.Clear();
            reserveUlongDict.Clear();
            writeChecks.Clear();
        }

        #region ReserveDictionaries
        private Dictionary<string, int> reserveIntDict = new Dictionary<string, int>();
        private Dictionary<string, int> reserveUintDict = new Dictionary<string, int>();
        private Dictionary<string, int> reserveInt16Dict = new Dictionary<string, int>();
        private Dictionary<string, int> reserveUint16Dict = new Dictionary<string, int>();
        private Dictionary<string, int> reserveLongDict = new Dictionary<string, int>();
        private Dictionary<string, int> reserveUlongDict = new Dictionary<string, int>();
        private int ReserveValue(string key, int offset, Dictionary<string, int> reserveDict, int valueSize)
        {
            reserveDict[key] = offset;
            AddRange(new byte[valueSize]);

            return offset;
        }
        private int FillValue(string key, byte[] newBytes, Dictionary<string, int> reserveDict)
        {
            var ptrLocation = reserveDict[key];
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            for (int i = 0; i < 4; i++)
            {
                this[ptrLocation + i] = newBytes[i];
            }
            reserveIntDict.Remove(key);

            return ptrLocation;
        }

        public int ReserveInt(string key)
        {
            return ReserveInt(key, Count);
        }

        public int ReserveInt(string key, int offset)
        {
            return ReserveValue(key, offset, reserveIntDict, 4);
        }

        public int FillInt(string key, int value)
        {
            return FillValue(key, BitConverter.GetBytes(value), reserveIntDict);
        }

        public int ReserveUint(string key)
        {
            return ReserveUint(key, Count);
        }

        public int ReserveUint(string key, int offset)
        {
            return ReserveValue(key, offset, reserveUintDict, 4);
        }

        public int FillUint(string key, uint value)
        {
            return FillValue(key, BitConverter.GetBytes(value), reserveUintDict);
        }
        public int ReserveInt16(string key)
        {
            return ReserveInt16(key, Count);
        }

        public int ReserveInt16(string key, int offset)
        {
            return ReserveValue(key, offset, reserveInt16Dict, 2);
        }

        public int FillInt16(string key, short value)
        {
            return FillValue(key, BitConverter.GetBytes(value), reserveInt16Dict);
        }
        public int ReserveUint16(string key)
        {
            return ReserveUint16(key, Count);
        }

        public int ReserveUint16(string key, int offset)
        {
            return ReserveValue(key, offset, reserveUint16Dict, 2);
        }

        public int FillUint16(string key, ushort value)
        {
            return FillValue(key, BitConverter.GetBytes(value), reserveUint16Dict);
        }
        public int ReserveLong(string key)
        {
            return ReserveLong(key, Count);
        }

        private int ReserveLong(string key, int offset)
        {
            return ReserveValue(key, offset, reserveLongDict, 8);
        }

        public int FillLong(string key, long value)
        {
            return FillValue(key, BitConverter.GetBytes(value), reserveLongDict);
        }
        public int ReserveUlong(string key)
        {
            return ReserveUlong(key, Count);
        }

        private int ReserveUlong(string key, int offset)
        {
            return ReserveValue(key, offset, reserveUlongDict, 8);
        }

        public int FillUlong(string key, uint value)
        {
            return FillValue(key, BitConverter.GetBytes(value), reserveUlongDict);
        }
        #endregion

        public void AddValue(byte[] value)
        {
            AddRange(value);
        }

        public void AddValue(Vector2 value)
        {
            AddValue(value.X);
            AddValue(value.Y);
        }

        public void AddValue(Vector3 value)
        {
            AddValue(value.X);
            AddValue(value.Y);
            AddValue(value.Z);
        }

        public void AddValue(Vector4 value)
        {
            AddValue(value.X);
            AddValue(value.Y);
            AddValue(value.Z);
            AddValue(value.W);
        }

        public void AddValue(ulong value)
        {
            var newBytes = BitConverter.GetBytes(value);
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            AddRange(newBytes);
        }

        public void AddValue(long value)
        {
            var newBytes = BitConverter.GetBytes(value);
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            AddRange(newBytes);
        }

        public void AddValue(uint value)
        {
            var newBytes = BitConverter.GetBytes(value);
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            AddRange(newBytes);
        }

        public void AddValue(int value)
        {
            var newBytes = BitConverter.GetBytes(value);
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            AddRange(newBytes);
        }

        public void AddValue(ushort value)
        {
            var newBytes = BitConverter.GetBytes(value);
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            AddRange(newBytes);
        }

        public void AddValue(short value)
        {
            var newBytes = BitConverter.GetBytes(value);
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            AddRange(newBytes);
        }

        public void AddValue(float value)
        {
            var newBytes = BitConverter.GetBytes(value);
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            AddRange(newBytes);
        }

        public void AddValue(double value)
        {
            var newBytes = BitConverter.GetBytes(value);
            if (AddAsBigEndian)
            {
                Array.Reverse(newBytes);
            }
            AddRange(newBytes);
        }

        public void AddValue(byte value)
        {
            Add(value);
        }

        public void AddValue(sbyte value)
        {
            Add((byte)value);
        }

        /// <summary>
        /// Aligns byte list to the alignment value ex. 0xB with alignmentValue 0x4 becomes 0xC. fillValue is the value of each byte added in 
        /// </summary>
        public int AlignWriter(int alignmentValue, byte fillValue = 0)
        {
            //Align to int align
            int currentCount = Count % alignmentValue;
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
                AddRange(bytes);

                return additions;
            }

            return 0;
        }

        /// <summary>
        /// Like AlignWriter, aligns byte list to the alignment value ex. 0xB with alignmentValue 0x4 becomes 0xC. fillValue is the value of each byte added in 
        /// This version is for PSO2 and accounts for bad modulo operation on their part and adds extra bytes to the file in the case that the size mod the alignment value is 0
		/// </summary>
		public int AlignFileEndWriter(int alignmentValue, byte fillValue = 0)
        {
            //Align to int align
            int currentCount = Count % alignmentValue;
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
                AddRange(bytes);

                return additions;
            }
            else
            {
                for (int i = 0; i < 0x10; i++)
                {
                    Add(0);
                }
            }

            return 0;
        }

        /// <summary>
        /// Mainly for handling pointer offsets. Better handled by Reserve and Fill extensions in most cases. 
        /// </summary>
        public int SetByteListInt(int offset, int value)
        {
            if (offset != -1)
            {
                var newBytes = BitConverter.GetBytes(value);
                for (int i = 0; i < 4; i++)
                {
                    this[offset + i] = newBytes[i];
                }

                return value;
            }

            return -1;
        }
    }
}
