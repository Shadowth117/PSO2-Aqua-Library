using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.BluePoint.CAWS
{
    public class CGPRObject
    {
        public uint magic;
        public byte structSize;
        public byte unkByte;
        public short count;

        public CGPRObject()
        {

        }

        public CGPRObject(BufferedStreamReader sr)
        {
            magic = sr.Read<uint>();

            structSize = sr.Read<byte>();
            if ((structSize & 0x80) > 0)
            {
                unkByte = sr.Read<byte>(); //Only shows up in these weird conditions? 
            }
            count = sr.Read<short>();


        }
    }
}
