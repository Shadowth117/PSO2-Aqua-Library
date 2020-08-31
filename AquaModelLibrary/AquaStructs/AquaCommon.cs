using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.AquaStructs
{
    public class AquaCommon
    {
        public struct VTBF
        {
            public int magicVTBF;
            public int size; //VTBF Size?
            public int magicAQGF; //AQGF, presumably AQua Game File
            public short unkShort0;
            public short unkShort1;
            public int magicVTC0; //special tag preceeding true tags in VTBF format. Always followed by a size int indicating the lenghh of the true tag and its data
            public int vtc0Size;
            public int magicROOT;
            public short unkTagShort;
            public short tagDataCount;
            public short tagDataSet0;
            public short tagDataSet0Length;
            public byte[] ROOTString; //String in ROOT of length tagDataSet0Length. In final, seemingly always says "hnd2aqg ver.1.61 Build: Feb 28 2012 18:46:06". Alphas note earlier dates.
        }

        public struct NIFL
        {
            public int magic;
            public int NIFLLength; //Length of NIFL after first 0x8
            public int unkInt0; //Always 1
            public int NIFLLengthFull; //Full size of NIFL
            public int NOF0Offset; //Offset of NOF0 from NIFL header end
            public int NOF0OffsetFull; //Offset of NOF0 from NIFL header start
            public int NOF0BlockSize; //Size of NOF0 struct
            public int padding0;
        }

        public struct REL0
        {
            public int magic;
            public int REL0Size; //REL0 is the container of general data in NIFL format
            public int REL0DataStart; //Always 0x10 for models, skeletons, and anims. Matters most for other filetypes where the REL structure is used more directly.
            public int padding0;
        }

        public struct NOF0
        {
            public int magic;
            public int NOF0Size; //Size of NOF0 data
            public int NOF0EntryCount; //Number of entries in NOF0 data
            public int NOF0DataSizeStart;
            public List<int> relAddresses;
            public List<int> paddingToAlign;
        }

        public struct NEND
        {
            public int magic;
            public int size; //Size of NEND data; Always 0x8
            public double padding0;
        }
    }
}
