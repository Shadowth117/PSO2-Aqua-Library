﻿using Reloaded.Memory.Extensions;
using Reloaded.Memory.Streams;
using Reloaded.Memory.Utilities;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace AquaModelLibrary.Helpers.Readers
{
    public class BufferedStreamReaderBE<TStream> : BufferedStreamReader<TStream> where TStream : Stream
    {
        public bool _BEReadActive = false;

        public BufferedStreamReaderBE(TStream stream, int bufferSize = 65536) : base(stream, bufferSize) { }

        /// <summary>
        /// Aligns the position of the stream to the designated alignment value
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AlignReader(int align)
        {
            //Align to int align
            while (Position % align > 0)
            {
                Read<byte>();
            }
        }

        /// <summary>
        /// Reads a Vector2. If active = true, it will be read as Big Endian.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 ReadBEV2(bool active)
        {
            return new Vector2(ReadBE<float>(active), ReadBE<float>(active));
        }

        /// <summary>
        /// Reads a Vector3. If active = true, it will be read as Big Endian.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 ReadBEV3(bool active)
        {
            return new Vector3(ReadBE<float>(active), ReadBE<float>(active), ReadBE<float>(active));
        }

        /// <summary>
        /// Reads a Vector4. If active = true, it will be read as Big Endian.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ReadBEV4(bool active)
        {
            return new Vector4(ReadBE<float>(active), ReadBE<float>(active), ReadBE<float>(active), ReadBE<float>(active));
        }

        /// <summary>
        /// Reads a Matrix4x4. If active = true, it will be read as Big Endian.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix4x4 ReadBEMatrix4(bool active)
        {
            return new Matrix4x4(ReadBE<float>(active), ReadBE<float>(active), ReadBE<float>(active), ReadBE<float>(active),
                                 ReadBE<float>(active), ReadBE<float>(active), ReadBE<float>(active), ReadBE<float>(active),
                                 ReadBE<float>(active), ReadBE<float>(active), ReadBE<float>(active), ReadBE<float>(active),
                                 ReadBE<float>(active), ReadBE<float>(active), ReadBE<float>(active), ReadBE<float>(active));
        }

        /// <summary>
        /// Reads a non string primitive type. If active = true, it will be read as Big Endian.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadBE<T>(bool active) where T : unmanaged
        {
            if (active)
            {
                return ReadBigEndian<T>();
            }
            else
            {
                return Read<T>();
            }
        }

        /// <summary>
        /// Reads a Vector2. If this reader's _BEReadActive = true, it will be read as Big Endian.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 ReadBEV2()
        {
            return new Vector2(ReadBE<float>(), ReadBE<float>());
        }

        /// <summary>
        /// Reads a Vector3. If this reader's _BEReadActive = true, it will be read as Big Endian.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 ReadBEV3()
        {
            return new Vector3(ReadBE<float>(), ReadBE<float>(), ReadBE<float>());
        }

        /// <summary>
        /// Reads a Vector4. If this reader's _BEReadActive = true, it will be read as Big Endian.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ReadBEV4()
        {
            return new Vector4(ReadBE<float>(), ReadBE<float>(), ReadBE<float>(), ReadBE<float>());
        }

        /// <summary>
        /// Reads a Vector4. If this reader's _BEReadActive = true, it will be read as Big Endian.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Quaternion ReadBEQuat()
        {
            return new Quaternion(ReadBE<float>(), ReadBE<float>(), ReadBE<float>(), ReadBE<float>());
        }

        /// <summary>
        /// Reads a Matrix4x4. If this reader's _BEReadActive = true, it will be read as Big Endian.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix4x4 ReadBEMatrix4()
        {
            return new Matrix4x4(ReadBE<float>(), ReadBE<float>(), ReadBE<float>(), ReadBE<float>(),
                                 ReadBE<float>(), ReadBE<float>(), ReadBE<float>(), ReadBE<float>(),
                                 ReadBE<float>(), ReadBE<float>(), ReadBE<float>(), ReadBE<float>(),
                                 ReadBE<float>(), ReadBE<float>(), ReadBE<float>(), ReadBE<float>());
        }

        /// <summary>
        /// Reads a non string primitive type. If this reader's _BEReadActive = true, it will be read as Big Endian.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadBE<T>() where T : unmanaged
        {
            if (_BEReadActive)
            {
                return ReadBigEndian<T>();
            }
            else
            {
                return Read<T>();
            }
        }

        /// <summary>
        ///     [Big Endian] Reads an unmanaged value from the stream.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T ReadBigEndian<T>() where T : unmanaged
        {
            return Endian.Reverse(Read<T>());
        }

        /// <summary>
        /// Returns an array of bytes without optimizations. Best used for large sets.
        /// </summary>
        public byte[] ReadBytes(long offset, int blockSize)
        {
            Span<byte> bytes = new byte[blockSize];
            ReadBytesUnbuffered(offset, bytes);
            return bytes.ToArray();
        }

        /// <summary>
        /// Returns an array of bytes without optimizations. Best used for large sets.
        /// </summary>
        public byte[] ReadBytesSeek(int blockSize)
        {
            Span<byte> bytes = new byte[blockSize];
            ReadBytesUnbuffered(Position, bytes);
            Seek(Position + blockSize, SeekOrigin.Begin);
            return bytes.ToArray();
        }

        /// <summary>
        /// Reads 4 bytes at the current position and returns them as an array of ints.
        /// </summary>
        public int[] Read4BytesToIntArray()
        {
            var bytes = Read4Bytes();
            return new int[] { bytes[0], bytes[1], bytes[2], bytes[3] };
        }

        /// <summary>
        /// Reads 4 bytes at the current position.
        /// </summary>
        public byte[] Read4Bytes()
        {
            byte[] bytes = new byte[4];
            for (int byteIndex = 0; byteIndex < 4; byteIndex++) { bytes[byteIndex] = Read<byte>(); }

            return bytes;
        }

        /// <summary>
        /// Reads 4 shorts at the current position.
        /// </summary>
        public short[] ReadBE4Shorts()
        {
            short[] shorts = new short[4];
            for (int shortIndex = 0; shortIndex < 4; shortIndex++) { shorts[shortIndex] = Read<short>(); }

            return shorts;
        }

        /// <summary>
        /// Reads 4 ushorts at the current position.
        /// </summary>
        public ushort[] ReadBE4UShorts()
        {
            ushort[] ushorts = new ushort[4];
            for (int ushortIndex = 0; ushortIndex < 4; ushortIndex++) { ushorts[ushortIndex] = Read<ushort>(); }

            return ushorts;
        }

        /// <summary>
        /// Reads 2 shorts at the current position.
        /// </summary>
        public short[] ReadBE2Shorts()
        {
            short[] shorts = new short[2];
            for (int shortIndex = 0; shortIndex < 2; shortIndex++) { shorts[shortIndex] = ReadBE<short>(); }

            return shorts;
        }

        /// <summary>
        /// Reads 2 ushorts at the current position.
        /// </summary>
        public ushort[] ReadBE2Ushorts()
        {
            ushort[] shorts = new ushort[2];
            for (int shortIndex = 0; shortIndex < 2; shortIndex++) { shorts[shortIndex] = ReadBE<ushort>(); }

            return shorts;
        }

        /// <summary>
        /// Attempts to terminate read at terminatingCharacter provided if a standard null character is not found from the current stream position. Christened from Phantasy Star Zero.
        /// </summary>
        public string ReadCStringPSZ(int terminatingCharacter = 0xA, int blockSize = 0x100)
        {
            var pos = Position;
            var strLen = BaseStream.Length;
            if (strLen <= pos + blockSize)
            {
                blockSize = (int)(strLen - pos);
            }
            //Past end of file
            if (blockSize <= 0)
            {
                return null;
            }

            string str = Encoding.ASCII.GetString(ReadBytes(Position, blockSize));
            if (str.IndexOf(char.MinValue) == -1)
            {
                return str.Remove(str.IndexOf((char)terminatingCharacter));
            }
            return str.Remove(str.IndexOf(char.MinValue));
        }

        /// <summary>
        /// Attempts to read a null terminated terminated CString from the provided stream position.
        /// </summary>
        public string ReadCString(long address, int blockSize = 0x100)
        {
            long bookmark = Position;

            Seek(address, SeekOrigin.Begin);
            var str = ReadCString(blockSize);
            Seek(bookmark, SeekOrigin.Begin);

            return str;
        }

        /// <summary>
        /// Attempts to read a null terminated terminated CString from the provided stream position.
        /// </summary>
        public string ReadCStringValidOffset(long address, int offset, int blockSize = 0x100, int addressThreshold = 0x10)
        {
            string str = null;
            if((address + offset) > addressThreshold)
            {
                long bookmark = Position;

                Seek(address + offset, SeekOrigin.Begin);
                str = ReadCString(blockSize);
                Seek(bookmark, SeekOrigin.Begin);
            }

            return str;
        }

        /// <summary>
        /// Attempts to read a null terminated terminated UTF8 String from the provided stream position.
        /// </summary>
        public string ReadUTF8String(long address, int blockSize = 0x100)
        {
            long bookmark = Position;

            Seek(address, SeekOrigin.Begin);
            var str = ReadUTF8String(blockSize);
            Seek(bookmark, SeekOrigin.Begin);

            return str;
        }

        /// <summary>
        /// Attempts to read a null terminated terminated UTF8 String from the provided stream position.
        /// </summary>
        public string ReadUTF16String(long address, int blockSize = 0x100)
        {
            long bookmark = Position;

            Seek(address, SeekOrigin.Begin);
            var str = ReadUTF16String(blockSize);
            Seek(bookmark, SeekOrigin.Begin);

            return str;
        }

        /// <summary>
        /// Attempts to read a null terminated terminated CString from the current stream position.
        /// </summary>
        public string ReadCString(int blockSize = 0x100)
        {
            var pos = Position;
            var strLen = BaseStream.Length;
            if (strLen <= pos + blockSize)
            {
                blockSize = (int)(strLen - pos);
            }
            //Past end of file
            if (blockSize <= 0)
            {
                return null;
            }
            string str = Encoding.ASCII.GetString(ReadBytes(pos, blockSize));
            var minVal = str.IndexOf(char.MinValue);
            if(minVal == -1)
            {
                return str;
            }
            return str.Remove(minVal);
        }

        /// <summary>
        /// Attempts to read a null terminated terminated CString and advances the stream to just after the null terminator or block end from the current stream position.
        /// </summary>
        public string ReadCStringSeek(int blockSize = 0x100)
        {
            var pos = Position;
            var strLen = BaseStream.Length;
            if (strLen <= pos + blockSize)
            {
                blockSize = (int)(strLen - pos);
            }
            //Past end of file
            if (blockSize <= 0)
            {
                return null;
            }
            string str = Encoding.ASCII.GetString(ReadBytes(Position, blockSize));
            var minVal = str.IndexOf(char.MinValue);
            if (minVal == -1)
            {
                Seek(blockSize, SeekOrigin.Current);
                return str;
            }
            else
            {
                Seek(minVal + 1, SeekOrigin.Current);
            }
            return str.Remove(minVal);
        }

        /// <summary>
        /// Attempts to read a null terminated terminated of the provided encoding from the current stream position.
        /// </summary>
        public string Read8bitEncodedString(Encoding encoding, int blockSize = 0x100)
        {
            var pos = Position;
            var strLen = BaseStream.Length;
            if (strLen <= pos + blockSize)
            {
                blockSize = (int)(strLen - pos);
            }
            //Past end of file
            if (blockSize <= 0)
            {
                return null;
            }
            string str = encoding.GetString(ReadBytes(pos, blockSize));
            var minVal = str.IndexOf(char.MinValue);
            if (minVal == -1)
            {
                return str;
            }
            return str.Remove(minVal);
        }

        /// <summary>
        /// Attempts to read a null terminated terminated CString and advances the stream to just after the null terminator or block end from the current stream position.
        /// </summary>
        public string Read8bitEncodedStringSeek(Encoding encoding, int blockSize = 0x100)
        {
            var pos = Position;
            var strLen = BaseStream.Length;
            if (strLen <= pos + blockSize)
            {
                blockSize = (int)(strLen - pos);
            }
            //Past end of file
            if (blockSize <= 0)
            {
                return null;
            }
            string str = encoding.GetString(ReadBytes(Position, blockSize));
            var minVal = str.IndexOf(char.MinValue);
            if (minVal == -1)
            {
                Seek(blockSize, SeekOrigin.Current);
                return str;
            }
            else
            {
                Seek(minVal + 1, SeekOrigin.Current);
            }
            return str.Remove(minVal);
        }

        /// <summary>
        /// Attempts to read a null terminated terminated UTF8 String from the current stream position.
        /// </summary>
        public string ReadUTF8String(int blockSize = 0x100)
        {
            var pos = Position;
            var strLen = BaseStream.Length;
            if (strLen <= pos + blockSize)
            {
                blockSize = (int)(strLen - pos);
            }
            //Past end of file
            if (blockSize <= 0)
            {
                return null;
            }
            string str = Encoding.UTF8.GetString(ReadBytes(Position, blockSize)); //Shouldn't ever be more than 0x60... in theory
            var minVal = str.IndexOf(char.MinValue);
            if (minVal == -1)
            {
                return str;
            }
            return str.Remove(minVal);
        }

        /// <summary>
        /// Attempts to read a null terminated terminated UTF16 String from the current stream position.
        /// </summary>
        public string ReadUTF16String(int blockSize = 0x100)
        {
            if (Position >= BaseStream.Length)
            {
                // Past end of file
                return null;
            }

            var sb = new StringBuilder();
            var decoder = Encoding.Unicode.GetDecoder();
            unsafe
            {
                char[] dest = new char[blockSize / 2];

                var foundEnd = false;
                do
                {
                    var shorts = ReadRaw<ushort>(blockSize, out var len);
                    var source = new ReadOnlySpan<ushort>(shorts, len);

                    var endIndex = source.IndexOf(char.MinValue);
                    if (endIndex >= 0)
                    {
                        foundEnd = true;
                        source = source.Slice(0, endIndex);
                        Seek(Position + endIndex - (len * 2), SeekOrigin.Begin);
                    }

                    decoder.Convert(source.AsBytes(), dest, foundEnd, out var bytesUsed, out var charsUsed, out var completed);
                    sb.Append(new string(dest, 0, charsUsed));
                } while (!foundEnd);
            }
            return sb.ToString();
        }
    }
}
