using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using UnluacNET.IO;

namespace UnluacNET
{
    public class BHeader
    {
        private static readonly int m_signature = 0x61754C1B; // '\x1B\Lua'

        private static readonly byte[] m_luacTail = {
            0x19, 0x93, 0x0D, 0x0A, 0x1A, 0x0A
        };

        public bool Debug { get; set; }

        public bool BigEndian { get; set; }

        public Version Version { get; set; }

        public BIntegerType Integer { get; set; }
        public BSizeTType SizeT { get; set; }
        public LBooleanType Bool { get; set; }
        public LNumberType Number { get; set; }
        public LStringType String { get; set; }
        public LConstantType Constant { get; set; }
        public LLocalType Local { get; set; }
        public LUpvalueType UpValue { get; set; }
        public LFunctionType Function { get; set; }

        public BHeader(Stream stream)
        {
            // 4-byte Lua signature
            if (stream.ReadInt32() != m_signature)
                throw new InvalidOperationException("The input file does not have the signature of a valid Lua file.");

            // 1-byte Lua version
            var version = stream.ReadByte();

            switch (version)
            {
            case 0x51:
                Version = Version.LUA51;
                break;
            case 0x52:
                Version = Version.LUA52;
                break;
            default:
                {
                    var major = version >> 4;
                    var minor = version & 0x0F;

                    var error = System.String.Format("The input chunk's Lua version is {0}.{1}; unluac can only handle Lua 5.1 and Lua 5.2.",
                        major, minor);

                    throw new InvalidOperationException(error);
                }
            }

            if (Debug)
                Console.WriteLine("-- version: 0x{0:X}", version);

            // 1-byte Lua "format"
            var format = stream.ReadByte();

            if (format != 0)
                throw new InvalidOperationException("The input chunk reports a non-standard lua format: " + format);

            if (Debug)
                Console.WriteLine("-- format: {0}", format);

            // 1-byte endianness
            var endianness = stream.ReadByte();

            if (endianness > 1)
                throw new InvalidOperationException("The input chunk reports an invalid endianness: " + endianness);

            BigEndian = (endianness == 0);

            if (Debug)
                Console.WriteLine("-- endianness: {0}", (BigEndian) ? "0 (big)" : "1 (little)");

            // 1-byte int size
            var intSize = stream.ReadByte();

            if (Debug)
                Console.WriteLine("-- int size: {0}", intSize);

            Integer = new BIntegerType(intSize);

            // 1-byte sizeT size
            var sizeTSize = stream.ReadByte();

            if (Debug)
                Console.WriteLine("-- size_t size: {0}", sizeTSize);

            SizeT = new BSizeTType(sizeTSize);

            // 1-byte instruction size
            var instructionSize = stream.ReadByte();

            if (Debug)
                Console.WriteLine("-- instruction size: {0}", instructionSize);

            if (instructionSize != 4)
                throw new InvalidOperationException("The input chunk reports an unsupported instruction size: " + instructionSize + " bytes");

            var lNumberSize = stream.ReadByte();

            if (Debug)
                Console.WriteLine("-- Lua number size: {0}", lNumberSize);

            var lNumberIntegralCode = stream.ReadByte();

            if (Debug)
                Console.WriteLine("-- Lua number integral code: {0}", lNumberIntegralCode);

            if (lNumberIntegralCode > 1)
                throw new InvalidOperationException("The input chunk reports an invalid code for lua number integralness: " + lNumberIntegralCode);

            var lNumberIntegral = (lNumberIntegralCode == 1);
            
            Number   = new LNumberType(lNumberSize, lNumberIntegral);
            Bool     = new LBooleanType();
            String   = new LStringType();
            Constant = new LConstantType();
            Local    = new LLocalType();
            UpValue  = new LUpvalueType();

            Function = Version.GetLFunctionType();

            if (Version.HasHeaderTail)
            {
                for (int i = 0; i < m_luacTail.Length; i++)
                    if (stream.ReadByte() != m_luacTail[i])
                        throw new InvalidOperationException("The input chunk does not have the header tail of a valid Lua file.");
            }
        }
    }
}
