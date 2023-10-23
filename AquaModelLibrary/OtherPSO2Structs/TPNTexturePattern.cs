using System;
using System.Collections.Generic;
using static AquaModelLibrary.AquaPackage;

namespace AquaModelLibrary
{
    public unsafe class TPNTexturePattern
    {
        public AFPBase tpnAFPBase = new AFPBase();
        public tpnHeader header = new tpnHeader();
        public List<texSet> texSets = new List<texSet>();
        //Header
        public struct tpnHeader
        {
            public int magic; //tpn
            public int count; //Amount of texture sets in the file
        }
        //Texture Set
        public struct texSet
        {
            public fixed byte setName[0x20]; //Reference for this texture set
            public fixed byte tex0Name[0x20]; //Tex0
            public fixed byte tex1Name[0x20]; //Tex1
            public fixed byte tex2Name[0x20]; //Tex2
            public fixed byte tex3Name[0x20]; //Tex3
        }
    }
}
