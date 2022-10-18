using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.BluePoint.CANI
{
    //Thanks to Meowmaritus for their own notes
    public class CANIHeader
    {
        public int magic;
        public int unkConst;
        public int size; //Anim footer will be here
        public ushort usht_0C;
        public ushort usht_0E;

        public int int_10;
        public int fileFps;
        public int ptr_18;
        public float flt_1C;

        public ushort usht_20;
        public ushort usht_22;
        public float float_24;
        public float endFrame;
        public float frameDuration; //?

        public int ptr_30;
        public ushort usht_34;
        public ushort usht_36;
        public int ptr_38;
        public int int_3C;
    }
}
