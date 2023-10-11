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

        public CVariableTrail(BufferedStreamReader sr)
        {
            byte? current = null;
            for(int i = 0; i < 4; i++)
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
