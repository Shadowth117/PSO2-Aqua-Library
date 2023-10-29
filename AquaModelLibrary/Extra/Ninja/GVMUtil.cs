using AquaModelLibrary.AquaMethods;
using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.Extra.Ninja
{
    public class GVMUtil
    {
        /// <summary>
        /// GVM unfortunately does not give a full filesize and so when it's embedded within other files, we need to seek through it to get everything.
        /// GVR is a bit of an involved format as well so it's best to extract and leave the rest to puyo tools, frankly.
        /// </summary>
        public static byte[] ReadGVMBytes(BufferedStreamReader sr)
        {
            List<byte> gvmBytes = new List<byte>();
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
                if (i == entryCount - 1 && sr.BaseStream().Length > fileSize + sr.Position() + 9)
                {
                    fileSize += 0x10;
                }
                gvmBytes.AddRange(sr.ReadBytes(currentFileOffset, 8 + fileSize));
                AquaGeneralMethods.AlignWriter(gvmBytes, 0x10);
                sr.Seek(fileSize, System.IO.SeekOrigin.Current);
            }

            return gvmBytes.ToArray();
        }
    }
}
