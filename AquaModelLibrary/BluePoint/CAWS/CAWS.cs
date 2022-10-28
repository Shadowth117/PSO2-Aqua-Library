using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.BluePoint.CAWS
{
    public class CAWS
    {
        public string name = null;
        public int cawsObjectCount;
        
        /*
        public ulong hashThing;
        public int const27B; //Always 0x27B???

        public int flags;
        public int unkCount1;
        public byte unkByte;

        public uint cawsConst;     //Always 0x394B87F0
        public ulong hashThing1;
        public ulong hashThing2;
        public ulong unkLong;      //Always 0?

        public ulong cawsConst2;
        public ushort cawsConst3;
        */

        public CAWS(BufferedStreamReader sr)
        {
            var len = sr.Read<byte>();
            name = Encoding.UTF8.GetString(sr.ReadBytes(sr.Position(), len));
            sr.Seek(len, System.IO.SeekOrigin.Current);

            cawsObjectCount = sr.Read<int>();

            /*
            hashThing = sr.Read<ulong>();
            const27B = sr.Read<int>();
            flags = sr.Read<int>();
            unkCount1 = sr.Read<int>();
            unkByte = sr.Read<byte>();

            cawsConst = sr.Read<uint>();
            hashThing1 = sr.Read<ulong>();
            hashThing2 = sr.Read<ulong>();
            unkLong = sr.Read<ulong>();

            cawsConst2 = sr.Read<ulong>();
            cawsConst3 = sr.Read<ushort>();
            */

        }
    }
}
