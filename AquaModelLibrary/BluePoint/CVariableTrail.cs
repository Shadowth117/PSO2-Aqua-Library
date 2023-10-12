using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.BluePoint
{
    public class CVariableTrail
    {
        public List<byte> data = new List<byte>();

        public CVariableTrail() { }

        public CVariableTrail(BufferedStreamReader sr, int limit = 4)
        {
            byte? current = null;
            for(int i = 0; i < limit; i++)
            {
                current = sr.Read<byte>();
                if(current == 0)
                {
                    break;
                }
                data.Add((byte)current);
            }
        }
    }
}
