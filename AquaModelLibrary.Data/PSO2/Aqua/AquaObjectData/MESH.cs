﻿namespace AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData
{
    public struct MESH
    {
        public short flags; //0xB0, type 0x9, byte 0 and byte 1 //0x17 usually, 0x11 usually in 0xC33
        public short unkShort0; //0xB0, type 0x9, byte 2 and byte 3 //0, 0x9, sometimes 0x10
        public byte unkByte0; //0xC7, type 0x9, byte 0 //0x80 or very close. Unknown what it affects
        public byte unkByte1; //0xC7, type 0x9, byte 1 //0x64 or sometimes 0x63
        public short unkShort1; //0xC7, type 0x9, byte 2 and byte 3 //always 0?
        public int mateIndex;   //0xB1, type 0x8
        public int rendIndex;   //0xB2, type 0x8

        public int shadIndex;   //0xB3, type 0x8
        public int tsetIndex;   //0xB4, type 0x8
        public int baseMeshNodeId; //0xB5, type 0x8 //Used for assigning rigid weights in absence of vertex weights. 0 for basewear. 
                                   //Otherwise, takes the value of the dummy bone auto generated per mesh exported. 
                                   //Said bones are stored after regular bones, but before physics bones.
        public int vsetIndex; //0xC0, type 0x8  //Same as mesh index

        public int psetIndex; //0xC1, type 0x8 //Same as mesh index
        public int baseMeshDummyId; //0xC2, type 0x9  //Autogenerated mesh dummy bones have associated ids based on the order they were created. 
                                    //ie. basemesh0, basemesh1, etc. This stores that number or is 0 for basewear.
        public int unkInt0; //0xCD, 0x8 //Usually 0;
        public int reserve0; //0
    }
}
