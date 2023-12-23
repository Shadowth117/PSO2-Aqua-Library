namespace AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData
{
    public struct VSET
    {
        public int vertDataSize;   //0xB6, Type 0x9 //In 0xC33, should always be OBJC largest size due to VTXL structure changes.
        public int vtxeCount; //0xBF, Type 0x9 //Number of data struct types per vertex in classic. Index of VTXE struct in 0xC33
        public int vtxeOffset;      //Unused in 0xC33
        public int vtxlCount;      //0xB9, Type 0x9 //Number of VTXL structs/Vertices

        public int vtxlOffset;     //Unused in 0xC33
        public int vtxlStartVert;       //0xC4, Type 0x9 //Unused in classic. In 0xC33, specifies starting vertex for VSET within global VTXL list
        public int bonePaletteCount; //0xBD, Type 0x8 //Vertex groups can't have more than 15 bones. 
                                     //Unknown value in 0xC33. Replaces bone palette count and seems to be 0x??FFFFFF always. Perhaps a negative bonecount?
                                     //Still set to 0 if model does not use bones in 0xC33

        public int bonePaletteOffset;
        //In VTBF, VSET also contains bonePalette. 
        //0xBE. Entire entry omitted if count was 0. Type is 06 if single bone, 86 if multiple. Next is usually 0x8 or 0x6 (unknown what this really is), 
        //last is 0 based count as a byte.

        public int unk0;         //0xC8, Type 0x9 //Unknown
        public int unk1;         //0xCC, Type 0x9 //Unknown
        public int unk2;         //Likely an offset related to above as it's not present in VTBF.
                                 //Edge verts are what I christened the set of vertex ids seemingly split along where the mesh
                                 //had to be separated due to bone count limitations.
        public int edgeVertsCount;  //0xC9, Type 0x9

        public int edgeVertsOffset;
        //In VTBF, VSET also contains Edge Verts. 
        //0xCA. Entire entry omitted if count was 0. Type is 06 if single vert, 86 if multiple. Next is usually 0x8 or 0x6 (unknown what this really is), 
        //last is 0 based count as a byte.
    }
}
