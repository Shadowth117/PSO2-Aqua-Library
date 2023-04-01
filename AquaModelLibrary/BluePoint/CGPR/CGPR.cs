using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.BluePoint.CAWS
{
    public class CGPR
    {
        public CFooter cfooter;
        public int cgprObjCount;
        public int int_04; //For 0 count cgpr

        public CGPR()
        {

        }

        public CGPR(BufferedStreamReader sr)
        {
            var cgprObjCount = sr.Read<int>(); //Doesn't always line up right. Correlates, but needs more research

            //Should just be an empty cgpr
            if(cgprObjCount == 0)
            {
                int_04 = sr.Read<int>();
                cfooter = sr.Read<CFooter>();
            }

            int type0 = sr.Peek<int>();
            while(!(type0 == 0 || type0 == 0x47505250))
            {
                switch(type0)
                {
                    default:
                        throw new Exception($"Unexpected object {type0.ToString("X")} discovered");
                }
                type0 = sr.Peek<int>();

                //Try to account for weird scenarios where sizes don't align? Idk wtf the game is doing
                if(sr.Peek<byte>() == 0)
                {
                    sr.Seek(1, System.IO.SeekOrigin.Current);
                    type0 = sr.Peek<int>();
                }
            }
        }
    }
}
