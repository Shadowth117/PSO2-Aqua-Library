using System.Runtime.InteropServices;

namespace AquaModelLibrary.Data.PSO
{
    public class PlyMotionDataUtil
    {
        /// <summary>
        /// Adapted from https://github.com/fuzziqersoftware/newserv/blob/299e1873801c62b7cfc3a317f3f0c3fb15aa3fd1/src/PSOEncryption.cc
        /// </summary>
        public class PSOEncryption
        {
            // Most ciphers used by PSO are symmetric; alias decrypt to encrypt by default
            public void Decrypt(IntPtr data, int size, bool advance)
            {
                Encrypt(data, size, advance);
            }

            public virtual void Encrypt(IntPtr data, int size, bool advance) { }
        }

        public class PSOLFGEncryption : PSOEncryption
        {
            public uint[] stream;
            public int offset;
            public int endOffset;
            public uint initialSeed;
            public int cycles;

            public PSOLFGEncryption(uint seed, int streamLength, int endOffset)
            {
                stream = new uint[streamLength];
                offset = 0;
                this.endOffset = endOffset;
                initialSeed = seed;
                cycles = 0;
            }

            protected uint Next(bool advance)
            {
                if (offset == endOffset)
                {
                    UpdateStream();
                }

                uint ret = stream[offset];
                if (advance)
                {
                    offset++;
                }
                return ret;
            }

            protected void EncryptT(IntPtr vdata, int size, bool advance)
            {
                if (!advance && size != 4)
                {
                    throw new InvalidOperationException("Cannot peek-encrypt/decrypt with size > 4");
                }

                int uint32Count = size >> 2;
                int extraBytes = size & 3;

                uint[] data = new uint[uint32Count + 1];
                Marshal.Copy(vdata, (int[])(object)data, 0, uint32Count);

                for (int x = 0; x < uint32Count; x++)
                {
                    data[x] ^= Next(advance);
                }

                if (extraBytes > 0)
                {
                    /*
                    uint last = 0;
                    Buffer.BlockCopy(data, uint32Count * sizeof(uint), 0, 0, extraBytes);
                    last = Next(advance);
                    Buffer.BlockCopy(last, 0, data, uint32Count * sizeof(uint), extraBytes);
                    */
                    System.Diagnostics.Debug.WriteLine($"ExtraBytes count: {extraBytes}");
                }

                Marshal.Copy((int[])(object)data, 0, vdata, uint32Count + 1);
            }

            protected void EncryptMinusT(IntPtr vdata, int size, bool advance)
            {
                if (!advance && size != 4)
                {
                    throw new InvalidOperationException("Cannot peek-encrypt/decrypt with size > 4");
                }

                int uint32Count = size >> 2;
                int extraBytes = size & 3;

                uint[] data = new uint[uint32Count + 1];
                Marshal.Copy(vdata, (int[])(object)data, 0, uint32Count);

                for (int x = 0; x < uint32Count; x++)
                {
                    data[x] = Next(advance) - data[x];
                }

                if (extraBytes > 0)
                {
                    /*
                    uint last = 0;
                    Buffer.BlockCopy(data, uint32Count * sizeof(uint), ref last, 0, extraBytes);
                    last = Next(advance) - last;
                    Buffer.BlockCopy(ref last, 0, data, uint32Count * sizeof(uint), extraBytes);
                    */
                    System.Diagnostics.Debug.WriteLine($"ExtraBytes count: {extraBytes}");
                }

                Marshal.Copy((int[])(object)data, 0, vdata, uint32Count + 1);
            }

            public override void Encrypt(IntPtr vdata, int size, bool advance)
            {
                EncryptT(vdata, size, advance);
            }

            public void EncryptBigEndian(IntPtr vdata, int size, bool advance)
            {
                EncryptT(vdata, size, advance);
            }

            public void EncryptMinus(IntPtr vdata, int size, bool advance)
            {
                EncryptMinusT(vdata, size, advance);
            }

            public void EncryptBigEndianMinus(IntPtr vdata, int size, bool advance)
            {
                EncryptMinusT(vdata, size, advance);
            }

            public void EncryptBothEndian(IntPtr leVdata, IntPtr beVdata, int size, bool advance)
            {
                if (size % 4 != 0)
                {
                    throw new ArgumentException("Size must be a multiple of 4");
                }

                if (!advance && size != 4)
                {
                    throw new InvalidOperationException("Cannot peek-encrypt/decrypt with size > 4");
                }

                size >>= 2;

                uint[] leData = new uint[size];
                uint[] beData = new uint[size];

                Marshal.Copy(leVdata, (int[])(object)leData, 0, size);
                Marshal.Copy(beVdata, (int[])(object)beData, 0, size);

                for (int x = 0; x < size; x++)
                {
                    uint key = Next(advance);
                    leData[x] ^= key;
                    beData[x] ^= key;
                }

                Marshal.Copy((int[])(object)leData, 0, leVdata, size);
                Marshal.Copy((int[])(object)beData, 0, beVdata, size);
            }

            protected void UpdateStream()
            {
                for (int z = 1; z < 0x19; z++)
                {
                    stream[z] -= stream[z + 0x1F];
                }
                for (int z = 0x19; z < 0x38; z++)
                {
                    stream[z] -= stream[z - 0x18];
                }
                offset = 1;
                cycles++;
            }
        }

        public unsafe class PSOV2Encryption : PSOLFGEncryption
        {
            public const int STREAM_LENGTH = 0x38;
            public PSOV2Encryption(uint seed)
                : base(seed, STREAM_LENGTH + 1, STREAM_LENGTH)
            {
                uint a = 1, b = initialSeed;
                stream[0x37] = b;
                for (ushort virtualIndex = 0x15; virtualIndex <= 0x36 * 0x15; virtualIndex += 0x15)
                {
                    stream[virtualIndex % 0x37] = a;
                    uint c = b - a;
                    b = a;
                    a = c;
                }
                for (int x = 0; x < 5; x++)
                {
                    UpdateStream();
                }
                cycles = 0;
            }

            public byte[] Decrypt(byte[] data)
            {
                return Encrypt(data);
            }

            public byte[] Encrypt(byte[] data)
            {
                fixed (byte* dataPtr = data)
                {
                    Encrypt((IntPtr)dataPtr, data.Length, true);
                    byte[] outBytes = new byte[data.Length];
                    for (int i = 0; i < data.Length; i++)
                    {
                        outBytes[i] = dataPtr[i];
                    }

                    return outBytes;
                }
            }

            public override void Encrypt(IntPtr data, int size, bool advance)
            {
                EncryptT(data, size, advance);
            }

            public void EncryptBigEndian(IntPtr data, int size, bool advance)
            {
                EncryptT(data, size, advance);
            }
        }
    }
}
