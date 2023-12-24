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

        public static VSET ParseVSET(Dictionary<int, object> vsetRaw, out List<ushort> bonePalette, out List<ushort> edgeVertsList)
        {
            bonePalette = null;
            edgeVertsList = null;
            VSET vset = new VSET();
            vset.vertDataSize = (int)(vsetRaw[0xB6]);
            vset.vtxeCount = (int)(vsetRaw[0xBF]);
            vset.vtxlCount = (int)(vsetRaw[0xB9]);
            vset.vtxlStartVert = (int)(vsetRaw[0xC4]);

            //BonePalette
            vset.bonePaletteCount = (int)(vsetRaw[0xBD]);
            if (vsetRaw.ContainsKey(0xBE))
            {
                var rawBP = (vsetRaw[0xBE]);
                if (rawBP is ushort)
                {
                    bonePalette = new List<ushort> { (ushort)rawBP };
                }
                else if (rawBP is ushort[])
                {
                    bonePalette = ((ushort[])rawBP).ToList();
                }
                else if (rawBP is short)
                {
                    bonePalette = new List<ushort> { (ushort)((short)rawBP) };
                }
                else if (rawBP is short[])
                {
                    var rawBPUshort = new List<ushort>();
                    var BPArr = (short[])rawBP;
                    for (int s = 0; s < BPArr.Length; s++)
                    {
                        rawBPUshort.Add((ushort)BPArr[s]);
                    }
                    bonePalette = rawBPUshort;
                }
            }

            //Not sure on these, but I don't know that unk0-2 get used normally
            vset.unk0 = (int)(vsetRaw[0xC8]);
            vset.unk1 = (int)(vsetRaw[0xCC]);

            //EdgeVerts
            vset.edgeVertsCount = (int)(vsetRaw[0xC9]);
            if (vsetRaw.ContainsKey(0xCA))
            {
                var rawEV = (vsetRaw[0xCA]);
                if (rawEV is ushort)
                {
                    edgeVertsList = new List<ushort> { (ushort)rawEV };
                }
                else if (rawEV is short[])
                {
                    edgeVertsList = ((ushort[])rawEV).ToList();
                }
                else if (rawEV is short)
                {
                    edgeVertsList = new List<ushort> { (ushort)((short)rawEV) };
                }
                else if (rawEV is short[])
                {
                    var rawEshort = new List<ushort>();
                    var EArr = (short[])rawEV;
                    for (int s = 0; s < EArr.Length; s++)
                    {
                        rawEshort.Add((ushort)EArr[s]);
                    }
                    edgeVertsList = rawEshort;
                }
            }

            return vset;
        }
    }
}
