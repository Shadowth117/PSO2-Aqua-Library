using AquaModelLibrary.AquaMethods;
using Reloaded.Memory.Streams;
using System.Collections.Generic;

namespace AquaModelLibrary.Extra.Ninja
{
    public class LND
    {
        public byte[] gvmBytes = null;
        public LND() { }

        public LND(BufferedStreamReader sr)
        {
            BigEndianHelper._active = true;
            var magicTest = sr.ReadBytes(0, 3);

            if (magicTest[0] == 0x4C && magicTest[1] == 0x4E && magicTest[0] == 0x44)
            {
                ReadAltLND(sr);
                gvmBytes = GVMUtil.ReadGVMBytes(sr);
            } else {
                ReadLND(sr);
                gvmBytes = GVMUtil.ReadGVMBytes(sr);
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
    }
}
