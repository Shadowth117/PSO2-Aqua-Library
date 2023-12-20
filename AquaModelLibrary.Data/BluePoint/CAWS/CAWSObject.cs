using Reloaded.Memory.Streams;

namespace AquaModelLibrary.BluePoint.CAWS
{
    public class CAWSObject
    {
        public uint magic;
        public byte structSize;
        public byte unkByte;
        public short count;

        public CAWSObject()
        {

        }

        public CAWSObject(BufferedStreamReader sr)
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
