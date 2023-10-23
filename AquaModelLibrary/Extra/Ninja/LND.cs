using Reloaded.Memory.Streams;
using System.Collections.Generic;

namespace AquaModelLibrary.Extra.Ninja
{
    public class LND
    {
        public LND() { }

        public LND(BufferedStreamReader sr)
        {
            BigEndianHelper._active = true;
            var magicTest = sr.ReadBytes(0, 3);

            if (magicTest[0] == 0x4C && magicTest[1] == 0x4E && magicTest[0] == 0x44)
            {
                ReadAltLND(sr);
            } else {
                ReadLND(sr);
            }
        }

        /// <summary>
        /// Specifically for 
        /// </summary>
        public void ReadAltLND(BufferedStreamReader sr)
        {

        }

        public void ReadLND(BufferedStreamReader sr)
        {

        }
    }
}
