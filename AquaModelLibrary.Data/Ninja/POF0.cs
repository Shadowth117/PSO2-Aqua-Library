using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AquaModelLibrary.Extra.Ninja
{
    /// <summary>
    /// NOF0 precursor. Compresses offsets via masking. Likely abandoned due to shrinking concerns on size and rising concerns on speed.
    /// </summary>
    public class POF0
    {
        public static byte mask = 0xC0;

        /// <summary>
        /// Incoming pointers MUST be aligned to 4 bytes! Setting align to false will ensure the POF0 byte array itself is not aligned.
        /// </summary>
        public static byte[] GeneratePOF0(List<uint> offsets, bool align = true)
        {
            List<byte> bytes = new List<byte>
            {
                0x50,
                0x4F,
                0x46,
                0x30
            };

            var data = GenerateRawPOF0(offsets, align);
            bytes.AddRange(BitConverter.GetBytes(data.Length));
            bytes.AddRange(data);

            return bytes.ToArray();
        }

        /// <summary>
        /// Incoming pointers MUST be aligned to 4 bytes! Setting align to false will ensure the POF0 byte array itself is not aligned.
        /// </summary>
        public static byte[] GeneratePOF0(List<int> offsets, bool align = true)
        {
            List<byte> bytes = new List<byte>
            {
                0x50,
                0x4F,
                0x46,
                0x30
            };

            var data = GenerateRawPOF0(offsets, align);
            bytes.AddRange(BitConverter.GetBytes(data.Length));
            bytes.AddRange(data);

            return bytes.ToArray();
        }

        /// <summary>
        /// Incoming pointers MUST be aligned to 4 bytes! Setting align to false will ensure the POF0 byte array itself is not aligned.
        /// </summary>
        public static byte[] GenerateRawPOF0(List<uint> offsets, bool align = true)
        {
            List<byte> pofBytes = new List<byte>();
            uint lastPof = 0;
            foreach(var offset in offsets)
            {
                pofBytes.AddRange(CalcPOF0Pointer(lastPof, offset));
                lastPof = offset;
            }

            if(align == true)
            {
                while(pofBytes.Count % 4 != 0)
                {
                    pofBytes.Add(0);
                }
            }

            return pofBytes.ToArray();
        }

        /// <summary>
        /// Incoming pointers MUST be aligned to 4 bytes! Setting align to false will ensure the POF0 byte array itself is not aligned.
        /// </summary>
        public static byte[] GenerateRawPOF0(List<int> offsets, bool align = true)
        {
            List<byte> pofBytes = new List<byte>();
            int lastPof = 0;
            foreach (var offset in offsets)
            {
                pofBytes.AddRange(CalcPOF0Pointer((uint)lastPof, (uint)offset));
                lastPof = offset;
            }

            if (align == true)
            {
                while (pofBytes.Count % 4 != 0)
                {
                    pofBytes.Add(0);
                }
            }

            return pofBytes.ToArray();
        }

        /// <summary>
        /// Takes in the previous POF0 address (before it's compressed) and the current address. As POF0 is made up of relative addresses, we need both to calculate the return here.
        /// </summary>
        private static byte[] CalcPOF0Pointer(uint lastPOF, uint currentAddress)
        {
            byte[] finalPOF;
            uint offsetDiff = currentAddress - lastPOF;
            uint offsetDiv = offsetDiff / 4;

            if (offsetDiff > 0xFF)
            {
                if (offsetDiff > 0xFFFF)
                {
                    var bytes = BitConverter.GetBytes(offsetDiff / 4);
                    finalPOF = new byte[] { (byte)(0xC0 + bytes[3]), bytes[2], bytes[1], bytes[0] };
                }
                else
                {
                    short shortCalc = (short)(offsetDiv);
                    var bytes = BitConverter.GetBytes(shortCalc);
                    finalPOF = new byte[] { (byte)(0x80 + bytes[1]), bytes[0] };
                }
            }
            else
            {
                byte byteCalc = (byte)offsetDiv;
                byteCalc += 0x40;
                finalPOF = new byte[] { byteCalc };
            }

            return finalPOF;
        }

        /// <summary>
        /// For POF0 data which includes the magic and size
        /// </summary>
        public static List<uint> GetPof0Offsets(byte[] pof0Bytes)
        {
            var magic = Encoding.UTF8.GetString(pof0Bytes, 0, 4);
            if (magic == "POF0")
            {
                var size = BitConverter.ToInt32(pof0Bytes, 4);
                byte[] arr = new byte[size];
                Array.Copy(pof0Bytes, 8, arr, 0, pof0Bytes.Length - 8);
                return GetRawPOF0Offsets(arr);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// For POF0 data which does not have the magic and size
        /// </summary>
        public static List<uint> GetRawPOF0Offsets(byte[] pof0Bytes)
        {
            uint currentOffset = 0;
            List<uint> pof = new List<uint>();
            int index = 0;
            while (index < pof0Bytes.Length)
            {
                uint pointer = pof0Bytes[index++];

                switch (pointer & mask)
                {
                    case 0x40:
                        pointer -= 0x40;
                        break;
                    case 0x80:
                        pointer -= 0x80;
                        pointer *= 0x100;
                        pointer += pof0Bytes[index++];
                        break;
                    case 0xC0:
                        pointer -= 0xC0;
                        pointer *= 0x1000000;
                        pointer += pof0Bytes[index++] * (uint)0x10000;
                        pointer += pof0Bytes[index++] * (uint)0x100;
                        pointer += pof0Bytes[index++];
                        break;
                }
                currentOffset += 4 * pointer;
                pof.Add(currentOffset);
            }

            return pof;
        }
    }
}
