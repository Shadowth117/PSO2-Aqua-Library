﻿using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.BluePoint
{
    public class CLength
    {
        public BPEra era = BPEra.None;
        private int length;
        private byte lengthAddition0 = 1;
        private byte lengthAddition1 = 1;
        private byte lengthAddition2 = 1;

        public CLength() { }

        public CLength(int trueLen)
        {
            var divCount = trueLen / 0x80;
            var rem = trueLen % 0x80;
            length = rem + 0x80;
            lengthAddition0 = (byte)divCount;
        }

        public CLength(BufferedStreamReaderBE<MemoryStream> sr, BPEra newEra)
        {
            era = newEra;
            switch(era)
            {
                case BPEra.DemonsSouls:
                    length = sr.Read<byte>();
                    if (length >= 0x80)
                    {
                        lengthAddition0 = sr.Read<byte>();
                        if (lengthAddition0 >= 0x80)
                        {
                            lengthAddition1 = sr.Read<byte>();
                            throw new NotImplementedException("Discovered CLength with unexpected length addition! Please review!");
                            if (lengthAddition1 >= 0x80)
                            {
                                lengthAddition2 = sr.Read<byte>();
                                throw new NotImplementedException("Discovered CLength with unexpected length addition! Please review!");
                            }
                        }
                    }
                    break;
                case BPEra.SOTC:
                    length = sr.ReadBE<ushort>();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public byte[] GetBytes(BPEra era)
        {
            switch (era)
            {
                case BPEra.DemonsSouls:
                    List<byte> outBytes = new List<byte>();
                    var trueLen = GetTrueLength();
                    if (trueLen >= 0x80)
                    {
                        var divCount = trueLen / 0x80;
                        var rem = trueLen % 0x80;
                        outBytes.Add((byte)(0x80 + rem));
                        outBytes.Add((byte)divCount);
                    } else
                    {
                        outBytes.Add((byte)trueLen);
                    }
                    return outBytes.ToArray();
                case BPEra.SOTC:
                    return BitConverter.GetBytes((ushort)GetTrueLength());
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Get full calculated variable length
        /// </summary>
        public int GetTrueLength()
        {
            switch (era)
            {
                case BPEra.DemonsSouls:
                    return length + (lengthAddition0 - 1) * 0x80;
                case BPEra.SOTC:
                    return length;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Determine the number of bytes this length data is composed of
        /// </summary>
        public int GetCLengthStructSize()
        {
            switch (era)
            {
                case BPEra.DemonsSouls:
                    return length >= 0x80 ? 2 : 1;
                case BPEra.SOTC:
                    return 2;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
