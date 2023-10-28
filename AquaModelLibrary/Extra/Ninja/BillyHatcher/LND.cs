using AquaModelLibrary.AquaMethods;
using Reloaded.Memory.Streams;
using System.Collections.Generic;

namespace AquaModelLibrary.Extra.Ninja
{
    public class LND
    {
        public List<byte> gvmBytes = new List<byte>();
        public LND() { }

        public LND(BufferedStreamReader sr)
        {
            BigEndianHelper._active = true;
            var magicTest = sr.ReadBytes(0, 3);

            if (magicTest[0] == 0x4C && magicTest[1] == 0x4E && magicTest[0] == 0x44)
            {
                ReadAltLND(sr);
                ReadGVMBytes(sr);
            } else {
                ReadLND(sr);
                ReadGVMBytes(sr);
            }
        }

        /// <summary>
        /// This seems to be mainly for older LND archives. They have an actual LND magic unlike the more common type
        /// </summary>
        public void ReadAltLND(BufferedStreamReader sr)
        {
            var magic = sr.Read<int>();
            var LndSize = sr.Read<int>();
            sr.Seek(LndSize + 0x8, System.IO.SeekOrigin.Begin);
        }

        public void ReadLND(BufferedStreamReader sr)
        {
            sr.Seek(0x34, System.IO.SeekOrigin.Begin);
            sr.Seek(sr.ReadBE<int>() + 0x20, System.IO.SeekOrigin.Begin);
        }

        public void ReadGVMBytes(BufferedStreamReader sr)
        {
            var magic = sr.Read<int>();
            var gvmFirstEntryOffset = sr.Read<int>();
            var flags = sr.ReadBE<ushort>();
            var entryCount = sr.ReadBE<ushort>();
            gvmBytes.AddRange(sr.ReadBytes(sr.Position() - 0xC, gvmFirstEntryOffset + 0x8));
            var gvrt0Offset = sr.Position() + gvmFirstEntryOffset - 4;
            sr.Seek(gvrt0Offset, System.IO.SeekOrigin.Begin);

            for (int i = 0; i < entryCount; i++)
            {
                var currentFileOffset = sr.Position();
                sr.Seek(4, System.IO.SeekOrigin.Current);
                var fileSize = sr.Read<int>(); //little endian
                if (i == entryCount - 1)
                {
                    fileSize += 0x10;
                }
                gvmBytes.AddRange(sr.ReadBytes(currentFileOffset, 8 + fileSize));
                AquaGeneralMethods.AlignWriter(gvmBytes, 0x10);
                sr.Seek(fileSize, System.IO.SeekOrigin.Current);
            }
        }
    }
}
