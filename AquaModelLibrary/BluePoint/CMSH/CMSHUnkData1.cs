using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.BluePoint.CMSH
{
    public class CMSHUnkData1
    {
        public int size;
        public List<byte> buffer = new List<byte>();

        public CMSHUnkData1()
        {

        }
        public CMSHUnkData1(BufferedStreamReader sr)
        {
            size = sr.Read<int>();
            buffer.AddRange(sr.ReadBytes(sr.Position(), size));
            sr.Seek(size, System.IO.SeekOrigin.Current);
        }
    }
}
