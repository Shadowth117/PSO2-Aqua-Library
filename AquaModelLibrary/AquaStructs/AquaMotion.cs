using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary
{
    //Cameras, UV, and standard motions are essentially the same format.
    public class AquaMotion
    {
        public const int CAMO = 0x4F4D4143; //Camera animation
        public const int SPMO = 0x4F4D5053; //UV animation
        public const int NDMO = 0x4F4D444E; //Motion animation
        public AquaPackage.AFPBase afp;
        public AquaCommon.NIFL nifl;
        public AquaCommon.REL0 rel0;
        public AquaCommon.NOF0 nof0;
        public AquaCommon.NEND nend;

        //MO Header
        public struct MOheader
        {

        }

        //Motion segment. Denotes a new node's animations
        public struct MSEG
        {

        }

        //Motion Key
        public struct MKEY
        {

        }

        public class MotionData
        {
            MSEG mseg;
            List<MKEY> posKeys = new List<MKEY>();
            List<MKEY> rotKeys = new List<MKEY>();
            List<MKEY> sclKeys = new List<MKEY>();
        }

        public class CameraData
        {
            MSEG mseg;
            List<MKEY> unkKeys0 = new List<MKEY>();
            List<MKEY> unkKeys1 = new List<MKEY>();
            List<MKEY> unkKeys2 = new List<MKEY>();
            List<MKEY> unkKeys3 = new List<MKEY>();
        }

        public class UVData
        {
            MSEG mseg;
            List<MKEY> unkKeys0 = new List<MKEY>();
            List<MKEY> unkKeys1 = new List<MKEY>();
            List<MKEY> unkKeys2 = new List<MKEY>();
            List<MKEY> unkKeys3 = new List<MKEY>();
            List<MKEY> unkKeys4 = new List<MKEY>();
            List<MKEY> unkKeys5 = new List<MKEY>();
        }
    }
}
