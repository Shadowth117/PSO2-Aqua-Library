using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace UnluacNET.IO
{
    public static class StreamExtensions
    {
        #region Read methods
        internal static int Read(this Stream stream, byte[] buffer)
        {
            stream.Read(buffer, 0, buffer.Length);
            return (buffer != null) ? 1 : -1;
        }
        
        public static char ReadChar(this Stream stream)
        {
            return (char)stream.ReadByte();
        }

        public static char[] ReadChars(this Stream stream, int count)
        {
            char[] chars = new char[count];

            for (int i = 0; i < count; i++)
                chars[i] = stream.ReadChar();

            return chars;
        }

        public static short ReadInt16(this Stream stream, bool bigEndian = false)
        {
            byte[] buffer = new byte[sizeof(short)];
            stream.Read(buffer);

            if (bigEndian)
                Array.Reverse(buffer);

            return BitConverter.ToInt16(buffer, 0);
        }

        public static ushort ReadUInt16(this Stream stream, bool bigEndian = false)
        {
            byte[] buffer = new byte[sizeof(ushort)];
            stream.Read(buffer);

            if (bigEndian)
                Array.Reverse(buffer);

            return BitConverter.ToUInt16(buffer, 0);
        }

        public static int ReadInt32(this Stream stream, bool bigEndian = false)
        {
            byte[] buffer = new byte[sizeof(int)];
            stream.Read(buffer);

            if (bigEndian)
                Array.Reverse(buffer);

            return BitConverter.ToInt32(buffer, 0);
        }

        public static uint ReadUInt32(this Stream stream, bool bigEndian = false)
        {
            byte[] buffer = new byte[sizeof(uint)];
            stream.Read(buffer);

            if (bigEndian)
                Array.Reverse(buffer);

            return BitConverter.ToUInt32(buffer, 0);
        }

        public static long ReadInt64(this Stream stream, bool bigEndian = false)
        {
            byte[] buffer = new byte[sizeof(long)];
            stream.Read(buffer);

            if (bigEndian)
                Array.Reverse(buffer);

            return BitConverter.ToInt64(buffer, 0);
        }

        public static ulong ReadUInt64(this Stream stream, bool bigEndian = false)
        {
            byte[] buffer = new byte[sizeof(ulong)];
            stream.Read(buffer);

            if (bigEndian)
                Array.Reverse(buffer);

            return BitConverter.ToUInt64(buffer, 0);
        }
        
        public static double ReadFloat(this Stream stream, bool bigEndian = false)
        {
            var val = (double)stream.ReadSingle(bigEndian);

            return Math.Round(val, 3);
        }

        public static float ReadSingle(this Stream stream, bool bigEndian = false)
        {
            byte[] buffer = new byte[sizeof(float)];
            stream.Read(buffer);

            if (bigEndian)
                Array.Reverse(buffer);

            return BitConverter.ToSingle(buffer, 0);
        }

        public static double ReadDouble(this Stream stream, bool bigEndian = false)
        {
            byte[] buffer = new byte[sizeof(double)];
            stream.Read(buffer);

            if (bigEndian)
                Array.Reverse(buffer);

            return BitConverter.ToDouble(buffer, 0);
        }
        #endregion
    }
}
