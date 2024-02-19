using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaObjectData.Intermediary;
using AquaModelLibrary.Helpers.Readers;
using AquaModelLibrary.Helpers;
using AquaModelLibrary.Helpers.PSO2;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Data.PSO2.Aqua.AquaCommonData;

namespace AquaModelLibrary.Data.PSO2.Aqua

{    //Though the NIFL format is used for storage, VTBF format tag references for data will be commented where appropriate. Some offset/reserve related things are NIFL only, however.
    public unsafe partial class AquaObject : AquaCommon
    {
        /// <summary>
        /// Checks the objc for if the type is in NGS range. Default to NGS at this point.
        /// </summary>
        public bool IsNGS { get { return !(objc.type < 0xC32); } }

        public OBJC objc;
        public List<VSET> vsetList = new List<VSET>();
        public List<VTXE> vtxeList = new List<VTXE>();
        public List<VTXL> vtxlList = new List<VTXL>();
        public List<PSET> psetList = new List<PSET>();
        public List<MESH> meshList = new List<MESH>();
        public List<MATE> mateList = new List<MATE>();
        public List<REND> rendList = new List<REND>();
        public List<SHAD> shadList = new List<SHAD>();
        public List<TSTA> tstaList = new List<TSTA>();
        public List<TSET> tsetList = new List<TSET>();
        public List<TEXF> texfList = new List<TEXF>();
        public UNRM unrms = null;
        public List<StripData> strips = new List<StripData>();

        //*** 0xC33 only
        public List<uint> bonePalette = new List<uint>();

        //Unclear the purpose of these, but when present they have a smaller count than initial mesh and psets. 
        public List<UnkStruct1> unkStruct1List = new List<UnkStruct1>();
        public List<MESH> mesh2List = new List<MESH>();
        public List<PSET> pset2List = new List<PSET>();
        public List<StripData> strips2 = new List<StripData>(); //Strip set 2 is from the same array as the first, just split differently, potentially.

        public List<int> strips3Lengths = new List<int>();
        public List<StripData> strips3 = new List<StripData>();
        public List<Vector3> unkPointArray1 = new List<Vector3>(); //Noooooooo idea what these are. Count matches the strips3Lengths count
        public List<Vector3> unkPointArray2 = new List<Vector3>();
        //***

        public bool applyNormalAveraging = false;

        //Custom model related data
        public List<GenericTriangles> tempTris = new List<GenericTriangles>();
        public List<GenericMaterial> tempMats = new List<GenericMaterial>();

        //Extra
        public List<string> meshNames = new List<string>();
        public List<string> matUnicodeNames = new List<string>();
        public List<string> texFUnicodeNames = new List<string>();

        public override string[] GetEnvelopeTypes()
        {
            return new string[] {
            "aqp\0",
            "trp\0",
            "aqo\0",
            "tro\0"
            };
        }

        /// <summary>
        /// Defaults to 0xC33, base NGS model type
        /// </summary>
        public AquaObject() 
        {
            objc.type = 0xC33;
        }
        public AquaObject(bool createNGSObj) 
        {
            objc.type = createNGSObj ? 0xC33 : 0xC2A;
        }
        public AquaObject(int objcType)
        {
            objc.type = objcType;
        }

        public AquaObject(byte[] bytes) : base(bytes) { }

        public AquaObject(BufferedStreamReaderBE<MemoryStream> sr) : base(sr) { }

        public override void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            objc = new OBJC();
            objc.Read(sr);

            //Read Bone Palette
            if (objc.bonePaletteOffset > 0)
            {
                sr.Seek(objc.bonePaletteOffset + offset, SeekOrigin.Begin);
                int boneCount = sr.Read<int>();
                sr.Seek(sr.Read<int>() + offset, SeekOrigin.Begin); //Should start literally right after this anyways, but in case it changes or w/e
                for (int boneIndex = 0; boneIndex < boneCount; boneIndex++)
                {
                    bonePalette.Add(sr.Read<uint>());
                }
            }

            if (IsNGS)
            {
                List<List<ushort>> edgeVertsTemp = new List<List<ushort>>();
                if (objc.vsetOffset > 0)
                {
                    sr.Seek(objc.vsetOffset + offset, SeekOrigin.Begin);
                    //Read VSETs
                    for (int vsetIndex = 0; vsetIndex < objc.vsetCount; vsetIndex++)
                    {
                        vsetList.Add(sr.Read<VSET>());

                        //Get edge verts if needed. Bone palette is linked elsewhere in 0xC32+ Aqua Objects.
                        List<ushort> edgeVerts = new List<ushort>();
                        if (vsetList[vsetIndex].edgeVertsCount > 0)
                        {
                            long bookmark = sr.Position;
                            sr.Seek(vsetList[vsetIndex].edgeVertsOffset + offset, SeekOrigin.Begin);
                            for (int edge = 0; edge < vsetList[vsetIndex].edgeVertsCount; edge++)
                            {
                                edgeVerts.Add(sr.Read<ushort>());
                            }
                            sr.Seek(bookmark, SeekOrigin.Begin);
                        }
                        edgeVertsTemp.Add(edgeVerts);
                    }
                }

                if (objc.vtxeOffset > 0)
                {
                    //Read VTXEs
                    sr.Seek(objc.vtxeOffset + offset, SeekOrigin.Begin);
                    for (int vtxeIndex = 0; vtxeIndex < objc.vtxeCount; vtxeIndex++)
                    {
                        VTXE vtxeSet = new VTXE();
                        int vtxeSubCount = sr.Read<int>();
                        long bookmark = sr.Position + 4;
                        sr.Seek(sr.Read<int>() + offset, SeekOrigin.Begin);

                        for (int vtxeEleIndex = 0; vtxeEleIndex < vtxeSubCount; vtxeEleIndex++)
                        {
                            vtxeSet.vertDataTypes.Add(sr.Read<VTXEElement>());
                        }
                        vtxeList.Add(vtxeSet);
                        sr.Seek(bookmark, SeekOrigin.Begin);
                    }

                    //Read VTXL
                    if (objc.vtxlStartOffset > 0)
                    {
                        sr.Seek(objc.vtxlStartOffset + offset, SeekOrigin.Begin);

                        //0xC32+ Aqua Objects use a global vertex array. To separate it into something more normally usable, we need to loop through the VSETs
                        //To accurately dump all model parts, materials and all without having isolated vertices, we'll want to split this again later when we have face data.
                        for (int vset = 0; vset < vsetList.Count; vset++)
                        {
                            VTXL vtxl = new VTXL(sr, vtxeList[vsetList[vset].vtxeCount], vsetList[vset].vtxlCount, objc.largetsVtxl);
                            vtxl.edgeVerts = edgeVertsTemp[vset];
                            vtxlList.Add(vtxl);
                        }
                    }
                }
            } else
            {
                //Read VSETs
                if (objc.vsetOffset > 0)
                {
                    sr.Seek(objc.vsetOffset + offset, SeekOrigin.Begin);
                    for (int vsetIndex = 0; vsetIndex < objc.vsetCount; vsetIndex++)
                    {
                        vsetList.Add(sr.Read<VSET>());
                    }
                    //Read VTXE+VTXL+BonePalette+MeshEdgeVerts
                    for (int vsetIndex = 0; vsetIndex < objc.vsetCount; vsetIndex++)
                    {
                        sr.Seek(vsetList[vsetIndex].vtxeOffset + offset, SeekOrigin.Begin);
                        //VTXE
                        VTXE vtxeSet = new VTXE();
                        for (int vtxeIndex = 0; vtxeIndex < vsetList[vsetIndex].vtxeCount; vtxeIndex++)
                        {
                            vtxeSet.vertDataTypes.Add(sr.Read<VTXEElement>());
                        }
                        vtxeList.Add(vtxeSet);

                        sr.Seek(vsetList[vsetIndex].vtxlOffset + offset, SeekOrigin.Begin);
                        //VTXL
                        VTXL vtxl = new VTXL(sr, vtxeSet, vsetList[vsetIndex].vtxlCount);
                        sr.AlignReader(0x10);

                        //Bone Palette
                        if (vsetList[vsetIndex].bonePaletteCount > 0)
                        {
                            sr.Seek(vsetList[vsetIndex].bonePaletteOffset + offset, SeekOrigin.Begin);
                            for (int boneId = 0; boneId < vsetList[vsetIndex].bonePaletteCount; boneId++)
                            {
                                vtxl.bonePalette.Add(sr.Read<ushort>());
                            }
                            sr.AlignReader(0x10);
                        }

                        //Edge Verts
                        if (vsetList[vsetIndex].edgeVertsCount > 0)
                        {
                            sr.Seek(vsetList[vsetIndex].edgeVertsOffset + offset, SeekOrigin.Begin);
                            for (int boneId = 0; boneId < vsetList[vsetIndex].edgeVertsCount; boneId++)
                            {
                                vtxl.edgeVerts.Add(sr.Read<ushort>());
                            }
                            sr.AlignReader(0x10);
                        }
                        vtxlList.Add(vtxl);
                    }
                }
            }

            //PSET
            if (objc.psetOffset > 0)
            {
                sr.Seek(objc.psetOffset + offset, SeekOrigin.Begin);
                for (int psetIndex = 0; psetIndex < objc.psetCount; psetIndex++)
                {
                    psetList.Add(sr.Read<PSET>());
                }

                //Read faces
                for (int psetIndex = 0; psetIndex < objc.psetCount; psetIndex++)
                {
                    sr.Seek(psetList[psetIndex].faceCountOffset + offset, SeekOrigin.Begin);
                    StripData stripData = new StripData();


                    //Sega is pretty consistent with these, but for 0xC31
                    stripData.format0xC31 = psetList[psetIndex].tag != 0x2100;

                    //We want to read 0xC32+ files differently.
                    //While 0xC31 uses triangle lists over tristrips, it should still be read the same as 0xC2A type models
                    if (IsNGS)
                    {
                        stripData.triIdCount = psetList[psetIndex].psetFaceCount;

                        //Read face groups. For most models, this will be 1
                        for (int i = 0; i < psetList[psetIndex].faceGroupCount; i++)
                        {
                            stripData.faceGroups.Add(sr.Read<int>());
                        }

                        sr.Seek(objc.globalStripOffset + (psetList[psetIndex].stripStartCount * 2) + offset, SeekOrigin.Begin);

                        //Read strip vert indices
                        for (int triId = 0; triId < psetList[psetIndex].psetFaceCount; triId++)
                        {
                            stripData.triStrips.Add(sr.Read<ushort>());
                        }
                    }
                    else
                    {
                        stripData.triIdCount = sr.Read<int>();
                        stripData.reserve0 = sr.Read<int>();
                        stripData.reserve1 = sr.Read<int>();
                        stripData.reserve2 = sr.Read<int>();
                        sr.Seek(psetList[psetIndex].faceOffset + offset, SeekOrigin.Begin);

                        //Read strip vert indices
                        for (int triId = 0; triId < stripData.triIdCount; triId++)
                        {
                            stripData.triStrips.Add(sr.Read<ushort>());
                        }
                    }
                    strips.Add(stripData);
                    sr.AlignReader(0x10);
                }
            }

            //Read MESH
            if (objc.meshOffset > 0)
            {
                sr.Seek(objc.meshOffset + offset, SeekOrigin.Begin);
                for (int meshIndex = 0; meshIndex < objc.meshCount; meshIndex++)
                {
                    meshList.Add(sr.Read<MESH>());
                }
            }

            //Read MATE
            if (objc.mateOffset > 0)
            {
                sr.Seek(objc.mateOffset + offset, SeekOrigin.Begin);
                for (int mateIndex = 0; mateIndex < objc.mateCount; mateIndex++)
                {
                    mateList.Add(sr.Read<MATE>());
                }
            }

            //Read REND
            if (objc.rendOffset > 0)
            {
                sr.Seek(objc.rendOffset + offset, SeekOrigin.Begin);
                for (int rendIndex = 0; rendIndex < objc.rendCount; rendIndex++)
                {
                    rendList.Add(sr.Read<REND>());
                }
            }

            //Read SHAD
            if (objc.shadOffset > 0)
            {
                sr.Seek(objc.shadOffset + offset, SeekOrigin.Begin);
                for (int shadIndex = 0; shadIndex < objc.shadCount; shadIndex++)
                {
                    shadList.Add(new SHAD(sr, offset));
                }
            }

            //Read TSTA
            if (objc.tstaOffset > 0)
            {
                sr.Seek(objc.tstaOffset + offset, SeekOrigin.Begin);
                for (int tstaIndex = 0; tstaIndex < objc.tstaCount; tstaIndex++)
                {
                    tstaList.Add(sr.Read<TSTA>());
                }
            }

            //Read TSET
            if (objc.tsetOffset > 0)
            {
                sr.Seek(objc.tsetOffset + offset, SeekOrigin.Begin);
                for (int tsetIndex = 0; tsetIndex < objc.tsetCount; tsetIndex++)
                {
                    tsetList.Add(new TSET(sr));
                }
            }

            //Read TEXF
            if (objc.texfOffset > 0)
            {
                sr.Seek(objc.texfOffset + offset, SeekOrigin.Begin);
                for (int texfIndex = 0; texfIndex < objc.texfCount; texfIndex++)
                {
                    texfList.Add(sr.Read<TEXF>());
                    texFUnicodeNames.Add(texfList[texfList.Count - 1].texName.GetString());
                }
            }

            //Read UNRM
            if (objc.unrmOffset > 0)
            {
                sr.Seek(objc.unrmOffset + offset, SeekOrigin.Begin);
                unrms = new UNRM();
                unrms.vertGroupCountCount = sr.Read<int>();
                unrms.vertGroupCountOffset = sr.Read<int>();
                unrms.vertCount = sr.Read<int>();
                unrms.meshIdOffset = sr.Read<int>();
                unrms.vertIDOffset = sr.Read<int>();
                unrms.padding0 = sr.Read<double>();
                unrms.padding1 = sr.Read<int>();
                unrms.unrmVertGroups = new List<int>();
                unrms.unrmMeshIds = new List<List<int>>();
                unrms.unrmVertIds = new List<List<int>>();

                //GroupCounts
                for (int vertId = 0; vertId < unrms.vertGroupCountCount; vertId++)
                {
                    unrms.unrmVertGroups.Add(sr.Read<int>());
                }
                sr.AlignReader(0x10);

                //Mesh IDs
                for (int vertGroup = 0; vertGroup < unrms.vertGroupCountCount; vertGroup++)
                {
                    List<int> vertGroupMeshList = new List<int>();

                    for (int i = 0; i < unrms.unrmVertGroups[vertGroup]; i++)
                    {
                        vertGroupMeshList.Add(sr.Read<int>());
                    }

                    unrms.unrmMeshIds.Add(vertGroupMeshList);
                }
                sr.AlignReader(0x10);

                //Vert IDs
                for (int vertGroup = 0; vertGroup < unrms.vertGroupCountCount; vertGroup++)
                {
                    List<int> vertGroupVertList = new List<int>();

                    for (int i = 0; i < unrms.unrmVertGroups[vertGroup]; i++)
                    {
                        vertGroupVertList.Add(sr.Read<int>());
                    }

                    unrms.unrmVertIds.Add(vertGroupVertList);
                }
                sr.AlignReader(0x10);
            }

            //Read PSET2
            if (objc.pset2Offset > 0)
            {
                sr.Seek(objc.pset2Offset + offset, SeekOrigin.Begin);
                for (int psetIndex = 0; psetIndex < objc.pset2Count; psetIndex++)
                {
                    pset2List.Add(sr.Read<PSET>());
                }

                //Read faces
                for (int psetIndex = 0; psetIndex < objc.pset2Count; psetIndex++)
                {
                    sr.Seek(pset2List[psetIndex].faceCountOffset + offset, SeekOrigin.Begin);
                    StripData stripData = new StripData();


                    //Sega is pretty consistent with these, but for 0xC31
                    stripData.format0xC31 = pset2List[psetIndex].tag != 0x2100;

                    if (IsNGS)
                    {
                        stripData.triIdCount = pset2List[psetIndex].psetFaceCount;

                        //Read face groups. For most models, this will be 1
                        for (int i = 0; i < pset2List[psetIndex].faceGroupCount; i++)
                        {
                            stripData.faceGroups.Add(sr.Read<int>());
                        }

                        sr.Seek(objc.globalStripOffset + (pset2List[psetIndex].stripStartCount * 2) + offset, SeekOrigin.Begin);

                        //Read strip vert indices
                        for (int triId = 0; triId < pset2List[psetIndex].psetFaceCount; triId++)
                        {
                            stripData.triStrips.Add(sr.Read<ushort>());
                        }
                    }
                    else
                    {
                        stripData.triIdCount = sr.Read<int>();
                        stripData.reserve0 = sr.Read<int>();
                        stripData.reserve1 = sr.Read<int>();
                        stripData.reserve2 = sr.Read<int>();
                        sr.Seek(pset2List[psetIndex].faceOffset + offset, SeekOrigin.Begin);

                        //Read strip vert indices
                        for (int triId = 0; triId < stripData.triIdCount; triId++)
                        {
                            stripData.triStrips.Add(sr.Read<ushort>());
                        }
                    }
                    strips2.Add(stripData);
                    sr.AlignReader(0x10);
                }
            }

            //Read MESH2
            if (objc.mesh2Offset > 0)
            {
                sr.Seek(objc.mesh2Offset + offset, SeekOrigin.Begin);
                for (int meshIndex = 0; meshIndex < objc.mesh2Count; meshIndex++)
                {
                    mesh2List.Add(sr.Read<MESH>());
                }
            }

            // Weird trp nonsense from here
            if (objc.unkStruct1Count > 0)
            {
                sr.Seek(objc.unkStruct1Offset + offset, SeekOrigin.Begin);
                for (int strIndex = 0; strIndex < objc.unkStruct1Count; strIndex++)
                {
                    unkStruct1List.Add(sr.Read<UnkStruct1>());
                }
            }

            //Get 3rd strip set. This actually has its own separate strip array... for some reason. The count seems to always be 1 regardless, but doesn't always have anything at the offset. Quite odd.
            if (objc.globalStrip3LengthCount > 0 && objc.globalStrip3LengthOffset != 0 && objc.globalStrip3Offset != 0)
            {
                //Get the lengths
                sr.Seek(objc.globalStrip3LengthOffset + offset, SeekOrigin.Begin);
                for (int id = 0; id < objc.globalStrip3LengthCount; id++)
                {
                    strips3Lengths.Add(sr.Read<int>());
                }

                sr.Seek(objc.globalStrip3Offset + offset, SeekOrigin.Begin);
                //Read strip vert indices
                for (int id = 0; id < strips3Lengths.Count; id++)
                {
                    StripData stripData = new StripData();
                    stripData.format0xC31 = true;
                    stripData.triIdCount = strips3Lengths[id];

                    //These can potentially be 0 sometimes
                    for (int triId = 0; triId < strips3Lengths[id]; triId++)
                    {
                        stripData.triStrips.Add(sr.Read<ushort>());
                    }

                    strips3.Add(stripData);
                }
            }

            if (objc.unkPointArray1Offset != 0)
            {
                if (objc.unkPointArray2Offset == 0)
                {
                    Debug.WriteLine("unkPointArray2 was null. Cannot reliably calculate size");
                    throw new Exception();
                }
                int point1Count = (objc.unkPointArray2Offset - objc.unkPointArray1Offset) / 0xC;

                sr.Seek(objc.unkPointArray1Offset + offset, SeekOrigin.Begin);
                for (int i = 0; i < point1Count; i++)
                {
                    unkPointArray1.Add(sr.Read<Vector3>());
                }
            }

            if (objc.unkPointArray2Offset != 0)
            {
                if (objc.unkPointArray1Offset == 0)
                {
                    Debug.WriteLine("unkPointArray1 was null. Cannot reliably calculate size");
                    throw new Exception();
                }
                int point2Count = (objc.unkPointArray2Offset - objc.unkPointArray1Offset) / 0xC;

                sr.Seek(objc.unkPointArray2Offset + offset, SeekOrigin.Begin);
                for (int i = 0; i < point2Count; i++)
                {
                    unkPointArray2.Add(sr.Read<Vector3>());
                }
            }
        }

        #region NIFLWriting
        public override byte[] GetBytesNIFL()
        {
            //Pointer data offsets for filling in later
            int rel0SizeOffset;

            //OBJC Offsets
            int objcGlobalTriOffset = 0;
            int objcGlobalVtxlOffset = 0;
            int objcVsetOffset;
            int objcPsetOffset;
            int objcMeshOffset;
            int objcMateOffset;

            int objcRendOffset;
            int objcShadOffset;
            int objcTstaOffset;
            int objcTsetOffset;

            int objcTexfOffset;
            int objcUnrmOffset;
            int objcVtxeOffset;
            int objcBonePaletteOffset;

            int objcUnkStruct1Offset;
            int objcPset2Offset;
            int objcMesh2Offset;
            int objcGlobalStrip3Offset;

            int objcGlobalStrip3LengthOffset3;
            int objcUnkPointArray1Offset;
            int objcUnkPointArray2Offset;

            List<int> vsetEdgeVertOffsets = new List<int>();
            List<int> psetfirstOffsets = new List<int>();
            List<int> pset2firstOffsets = new List<int>();
            List<int> shadDetailOffsets = new List<int>();
            List<int> shadExtraOffsets = new List<int>();

            List<int> vtxeOffsets = new List<int>();

            List<byte> outBytes = new List<byte>();
            List<int> nof0PointerLocations = new List<int>(); //Used for the NOF0 section

            //REL0
            outBytes.AddRange(Encoding.UTF8.GetBytes("REL0"));
            rel0SizeOffset = outBytes.Count; //We'll fill this later
            outBytes.AddRange(BitConverter.GetBytes(0));
            outBytes.AddRange(BitConverter.GetBytes(0x10));
            outBytes.AddRange(BitConverter.GetBytes(0));

            //OBJC

            //Set up OBJC pointers
            if (IsNGS)
            {
                objcGlobalTriOffset = DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0x14, strips.Count);
                objcGlobalVtxlOffset = DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0x1C, vtxlList.Count);
            }
            objcVsetOffset = DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0x28, objc.vsetCount);
            objcPsetOffset = DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0x30, objc.psetCount);
            objcMeshOffset = DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0x38, objc.meshCount);
            objcMateOffset = DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0x40, objc.mateCount);
                            
            objcRendOffset = DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0x48, objc.rendCount);
            objcShadOffset = DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0x50, objc.shadCount);
            objcTstaOffset = DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0x58, objc.tstaCount);
            objcTsetOffset = DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0x60, objc.tsetCount);

            objcTexfOffset = DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0x68, objc.texfCount);
            if (unrms != null)
            {
                objcUnrmOffset = DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0xA0, 1);
            }
            else
            {
                objcUnrmOffset = -1;
            }
            objcVtxeOffset = DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0xA8, objc.vtxeCount);
            objcBonePaletteOffset = DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0xAC, bonePalette.Count);

            objcUnkStruct1Offset = DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0xC4, unkStruct1List.Count);
            objcPset2Offset = DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0xCC, pset2List.Count);
            objcMesh2Offset = DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0xD4, mesh2List.Count);
            objcGlobalStrip3Offset = DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0xD8, strips3.Count);

            objcGlobalStrip3LengthOffset3 = DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0xE0, strips3Lengths.Count);
            objcUnkPointArray1Offset = DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0xE4, unkPointArray1.Count);
            objcUnkPointArray2Offset = DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0xE8, unkPointArray2.Count);

            //Write OBJC block
            outBytes.AddRange(BitConverter.GetBytes(objc.type));
            outBytes.AddRange(BitConverter.GetBytes(objc.size));
            outBytes.AddRange(BitConverter.GetBytes(objc.unkMeshValue));
            outBytes.AddRange(BitConverter.GetBytes(objc.largetsVtxl));

            outBytes.AddRange(BitConverter.GetBytes(objc.totalStripFaces));
            outBytes.AddRange(BitConverter.GetBytes(objc.globalStripOffset));
            outBytes.AddRange(BitConverter.GetBytes(objc.totalVTXLCount));
            outBytes.AddRange(BitConverter.GetBytes(objc.vtxlStartOffset));

            outBytes.AddRange(BitConverter.GetBytes(objc.unkStructCount));
            outBytes.AddRange(BitConverter.GetBytes(objc.vsetCount));
            outBytes.AddRange(BitConverter.GetBytes(objc.vsetOffset));
            outBytes.AddRange(BitConverter.GetBytes(objc.psetCount));

            outBytes.AddRange(BitConverter.GetBytes(objc.psetOffset));
            outBytes.AddRange(BitConverter.GetBytes(objc.meshCount));
            outBytes.AddRange(BitConverter.GetBytes(objc.meshOffset));
            outBytes.AddRange(BitConverter.GetBytes(objc.mateCount));

            outBytes.AddRange(BitConverter.GetBytes(objc.mateOffset));
            outBytes.AddRange(BitConverter.GetBytes(objc.rendCount));
            outBytes.AddRange(BitConverter.GetBytes(objc.rendOffset));
            outBytes.AddRange(BitConverter.GetBytes(objc.shadCount));

            outBytes.AddRange(BitConverter.GetBytes(objc.shadOffset));
            outBytes.AddRange(BitConverter.GetBytes(objc.tstaCount));
            outBytes.AddRange(BitConverter.GetBytes(objc.tstaOffset));
            outBytes.AddRange(BitConverter.GetBytes(objc.tsetCount));

            outBytes.AddRange(BitConverter.GetBytes(objc.tsetOffset));
            outBytes.AddRange(BitConverter.GetBytes(objc.texfCount));
            outBytes.AddRange(BitConverter.GetBytes(objc.texfOffset));

            outBytes.AddRange(DataHelpers.ConvertStruct(objc.bounds));
            outBytes.AddRange(BitConverter.GetBytes(objc.unkCount0));
            outBytes.AddRange(BitConverter.GetBytes(objc.unrmOffset));

            //NGS sections
            if(objc.type >= 0xC32)
            {
                outBytes.AddRange(BitConverter.GetBytes(objc.vtxeCount));
                outBytes.AddRange(BitConverter.GetBytes(objc.vtxeOffset));
                outBytes.AddRange(BitConverter.GetBytes(objc.bonePaletteOffset));

                outBytes.AddRange(BitConverter.GetBytes(objc.fBlock0));
                outBytes.AddRange(BitConverter.GetBytes(objc.fBlock1));
                outBytes.AddRange(BitConverter.GetBytes(objc.fBlock2));
                outBytes.AddRange(BitConverter.GetBytes(objc.fBlock3));

                outBytes.AddRange(BitConverter.GetBytes(objc.unkStruct1Count));
                outBytes.AddRange(BitConverter.GetBytes(objc.unkStruct1Offset));
                outBytes.AddRange(BitConverter.GetBytes(objc.pset2Count));
                outBytes.AddRange(BitConverter.GetBytes(objc.pset2Offset));

                if(objc.type >= 0xC33)
                {
                    outBytes.AddRange(BitConverter.GetBytes(objc.mesh2Count));
                    outBytes.AddRange(BitConverter.GetBytes(objc.mesh2Offset));
                    outBytes.AddRange(BitConverter.GetBytes(objc.globalStrip3Offset));
                    outBytes.AddRange(BitConverter.GetBytes(objc.globalStrip3LengthCount));   //At least 1

                    outBytes.AddRange(BitConverter.GetBytes(objc.globalStrip3LengthOffset));
                    outBytes.AddRange(BitConverter.GetBytes(objc.unkPointArray1Offset));
                    outBytes.AddRange(BitConverter.GetBytes(objc.unkPointArray2Offset));
                    outBytes.AddRange(BitConverter.GetBytes(objc.unkCount3));   //Default to 1?
                }
            }

            outBytes.AlignWriter(0x10);

            if(objc.type >= 0xC32)
            {
                //Write triangles. NGS doesn't use tristrips anymore
                //Assumes the ushorts are already laid out as triangles
                outBytes.SetByteListInt(objcGlobalTriOffset, outBytes.Count);
                for (int i = 0; i < strips.Count; i++)
                {
                    foreach (var id in strips[i].triStrips)
                    {
                        outBytes.AddRange(BitConverter.GetBytes(id));
                    }
                }
                outBytes.AlignWriter(0x10);
            }

            if(objc.type >= 0xC32)
            {
                //Write NGS VTXL
                outBytes.SetByteListInt(objcGlobalVtxlOffset, outBytes.Count);
                for (int i = 0; i < vtxlList.Count; i++)
                {
                    vtxlList[i].GetBytes(vtxeList[vsetList[i].vtxeCount], outBytes, objc.largetsVtxl);
                }
                outBytes.AlignWriter(0x10);

                //VSET
                //Write VSET pointer
                outBytes.SetByteListInt(objcVsetOffset, outBytes.Count);

                //Write VSET
                for (int vsetId = 0; vsetId < vsetList.Count; vsetId++)
                {
                    vsetEdgeVertOffsets.Add(DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0x30, vsetList[vsetId].edgeVertsCount));

                    outBytes.AddRange(DataHelpers.ConvertStruct(vsetList[vsetId]));
                }
                outBytes.AlignWriter(0x10);

                //Write Edge Verts
                for (int vsetId = 0; vsetId < vsetList.Count; vsetId++)
                {
                    if (vtxlList[vsetId].edgeVerts != null && vtxlList[vsetId].edgeVerts.Count > 0)
                    {
                        //Write edge verts pointer
                        outBytes.SetByteListInt(vsetEdgeVertOffsets[vsetId], outBytes.Count);
                        //Write edge verts
                        for (int evId = 0; evId < vtxlList[vsetId].edgeVerts.Count; evId++)
                        {
                            outBytes.AddRange(BitConverter.GetBytes(vtxlList[vsetId].edgeVerts[evId]));
                        }
                        outBytes.AlignWriter(0x10);
                    }
                }
            } else
            {
                List<int> vsetVtxeOffsets = new List<int>();
                List<int> vsetVtxlOffsets = new List<int>();
                List<int> vsetBonePaletteOffsets = new List<int>();
                //VSET
                //Write VSET pointer
                outBytes.SetByteListInt(objcVsetOffset, outBytes.Count);

                //Write VSET
                for (int vsetId = 0; vsetId < vsetList.Count; vsetId++)
                {
                    vsetVtxeOffsets.Add(DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0x8, vsetList[vsetId].vtxeCount));
                    vsetVtxlOffsets.Add(DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0x10, vsetList[vsetId].vtxlCount));
                    vsetBonePaletteOffsets.Add(DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0x1C, vsetList[vsetId].bonePaletteCount));
                    vsetEdgeVertOffsets.Add(DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0x30, vsetList[vsetId].edgeVertsCount));

                    outBytes.AddRange(DataHelpers.ConvertStruct(vsetList[vsetId]));
                }
                outBytes.AlignWriter(0x10);

                //VTXE + VTXL
                for (int vertListId = 0; vertListId < vtxlList.Count; vertListId++)
                {
                    //Write VTXE pointer
                    outBytes.SetByteListInt(vsetVtxeOffsets[vertListId], outBytes.Count);
                    //Write current VTXE array
                    for (int vtxeId = 0; vtxeId < vtxeList[vertListId].vertDataTypes.Count; vtxeId++)
                    {
                        outBytes.AddRange(DataHelpers.ConvertStruct(vtxeList[vertListId].vertDataTypes[vtxeId]));
                    }

                    //Write VTXL pointer
                    outBytes.SetByteListInt(vsetVtxlOffsets[vertListId], outBytes.Count);
                    //Write current VTXL array
                    vtxlList[vertListId].GetBytes(vtxeList[vertListId], outBytes);
                    outBytes.AlignWriter(0x10);

                    if (vtxlList[vertListId].bonePalette != null)
                    {
                        //Write bone palette pointer
                        outBytes.SetByteListInt(vsetBonePaletteOffsets[vertListId], outBytes.Count);
                        //Write bone palette
                        for (int bpId = 0; bpId < vtxlList[vertListId].bonePalette.Count; bpId++)
                        {
                            outBytes.AddRange(BitConverter.GetBytes(vtxlList[vertListId].bonePalette[bpId]));
                        }
                        outBytes.AlignWriter(0x10);
                    }

                    if (vtxlList[vertListId].edgeVerts != null)
                    {
                        //Write edge verts pointer
                        outBytes.SetByteListInt(vsetEdgeVertOffsets[vertListId], outBytes.Count);
                        //Write edge verts
                        for (int evId = 0; evId < vtxlList[vertListId].edgeVerts.Count; evId++)
                        {
                            outBytes.AddRange(BitConverter.GetBytes(vtxlList[vertListId].edgeVerts[evId]));
                        }
                        outBytes.AlignWriter(0x10);
                    }
                }
            }

            //PSET

            //Write PSET pointer
            outBytes.SetByteListInt(objcPsetOffset, outBytes.Count);

            //Write PSET
            for (int psetId = 0; psetId < psetList.Count; psetId++)
            {
                psetfirstOffsets.Add(DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0x8));
                if(objc.type < 0xC32)
                {
                    nof0PointerLocations.Add(outBytes.Count + 0x10);
                }
                outBytes.AddRange(DataHelpers.ConvertStruct(psetList[psetId]));
            }
            outBytes.AlignWriter(0x10);

            //0xC31 is closer to 0xC2A type models and so should be written like it
            if(objc.type >= 0xC32)
            {
                //Write triangle groups
                for (int stripId = 0; stripId < strips.Count; stripId++)
                {
                    outBytes.SetByteListInt(psetfirstOffsets[stripId], outBytes.Count);

                    for (int id = 0; id < strips[stripId].faceGroups.Count; id++)
                    {
                        outBytes.AddRange(BitConverter.GetBytes(strips[stripId].faceGroups[id]));
                    }
                    outBytes.AlignWriter(0x10); //These should all align already, but just in case
                }
            } else
            {
                //Write tristrip data
                for (int stripId = 0; stripId < strips.Count; stripId++)
                {
                    outBytes.SetByteListInt(psetfirstOffsets[stripId], outBytes.Count);
                    outBytes.SetByteListInt(psetfirstOffsets[stripId] + 0x8, outBytes.Count + 0x10); //Strip indices offset; always a set distance

                    outBytes.AddRange(BitConverter.GetBytes(strips[stripId].triIdCount));
                    outBytes.AddRange(BitConverter.GetBytes(strips[stripId].reserve0));
                    outBytes.AddRange(BitConverter.GetBytes(strips[stripId].reserve1));
                    outBytes.AddRange(BitConverter.GetBytes(strips[stripId].reserve2));

                    for (int faceId = 0; faceId < strips[stripId].triStrips.Count; faceId++)
                    {
                        outBytes.AddRange(BitConverter.GetBytes(strips[stripId].triStrips[faceId]));
                    }
                    outBytes.AlignWriter(0x10); //Intentionally aligned inside, unlike the basic PSET array's alignment
                }
            }

            //MESH

            //Write MESH pointer
            outBytes.SetByteListInt(objcMeshOffset, outBytes.Count);

            //Write MESH
            for (int meshId = 0; meshId < meshList.Count; meshId++)
            {
                outBytes.AddRange(DataHelpers.ConvertStruct(meshList[meshId]));
            }

            //MATE

            //Write MATE pointer
            outBytes.SetByteListInt(objcMateOffset, outBytes.Count);

            //Write MATE
            for (int mateId = 0; mateId < mateList.Count; mateId++)
            {
                outBytes.AddRange(DataHelpers.ConvertStruct(mateList[mateId]));
            }
            outBytes.AlignWriter(0x10);

            //REND

            //Write REND pointer
            outBytes.SetByteListInt(objcRendOffset, outBytes.Count);

            //Write REND
            for (int rendId = 0; rendId < rendList.Count; rendId++)
            {
                outBytes.AddRange(DataHelpers.ConvertStruct(rendList[rendId]));
            }
            outBytes.AlignWriter(0x10);

            //SHAD

            //Write SHAD pointer
            outBytes.SetByteListInt(objcShadOffset, outBytes.Count);

            //Write SHAD
            for (int shadId = 0; shadId < shadList.Count; shadId++)
            {
                outBytes.AddRange(BitConverter.GetBytes(shadList[shadId].unk0));
                outBytes.AddRange(shadList[shadId].pixelShader.GetBytes());
                outBytes.AddRange(shadList[shadId].vertexShader.GetBytes());

                shadDetailOffsets.Add(DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, shadList[shadId].shadDetailOffset));
                shadExtraOffsets.Add(DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 4, shadList[shadId].shadExtraOffset));
                outBytes.AddRange(BitConverter.GetBytes(shadList[shadId].shadDetailOffset));
                outBytes.AddRange(BitConverter.GetBytes(shadList[shadId].shadExtraOffset));
            }
            outBytes.AlignWriter(0x10);

            //Write SHAD sub structs
            for (int shadId = 0; shadId < shadList.Count; shadId++)
            {
                var shad = shadList[shadId];

                if (shad.shadDetailOffset != 0)
                {
                    outBytes.SetByteListInt(shadDetailOffsets[shadId], outBytes.Count);
                    outBytes.AddRange(DataHelpers.ConvertStruct(shad.shadDetail));
                }

                if (shad.shadExtra.Count > 0)
                {
                    outBytes.SetByteListInt(shadExtraOffsets[shadId], outBytes.Count);
                    foreach (var extra in shad.shadExtra)
                    {
                        outBytes.AddRange(DataHelpers.ConvertStruct(extra));
                    }
                }
            }

            //TSTA
            if (tstaList.Count > 0)
            {
                //Write TSTA pointer
                outBytes.SetByteListInt(objcTstaOffset, outBytes.Count);

                //Write TSTA
                for (int tstaId = 0; tstaId < tstaList.Count; tstaId++)
                {
                    outBytes.AddRange(DataHelpers.ConvertStruct(tstaList[tstaId]));
                }
                outBytes.AlignWriter(0x10);
            }

            //TSET

            //Write TSET pointer
            outBytes.SetByteListInt(objcTsetOffset, outBytes.Count);

            //Write TSET
            for (int tsetId = 0; tsetId < tsetList.Count; tsetId++)
            {
                outBytes.AddRange(BitConverter.GetBytes(tsetList[tsetId].unkInt0));
                outBytes.AddRange(BitConverter.GetBytes(tsetList[tsetId].texCount));
                outBytes.AddRange(BitConverter.GetBytes(tsetList[tsetId].unkInt1));
                outBytes.AddRange(BitConverter.GetBytes(tsetList[tsetId].unkInt2));

                outBytes.AddRange(BitConverter.GetBytes(tsetList[tsetId].unkInt3));

                //Do as bytes for greater than 4, ints if 4 or less. Remainder of the 0x10 area should be 0xFF.
                if (tsetList[tsetId].texCount > 4)
                {
                    for (int i = 0; i < tsetList[tsetId].texCount; i++)
                    {
                        outBytes.Add((byte)tsetList[tsetId].tstaTexIDs[i]);
                    }
                    for (int i = 0; i < (0x10 - tsetList[tsetId].texCount); i++)
                    {
                        outBytes.Add(0xFF);
                    }
                }
                else
                {
                    for (int i = 0; i < tsetList[tsetId].texCount; i++)
                    {
                        outBytes.AddRange(BitConverter.GetBytes(tsetList[tsetId].tstaTexIDs[i]));
                    }
                    for (int i = 0; i < (0x10 - (tsetList[tsetId].texCount * sizeof(int))); i++)
                    {
                        outBytes.Add(0xFF);
                    }
                }
            }
            outBytes.AlignWriter(0x10);

            //TEXF
            if (tstaList.Count > 0)
            {
                //Write TEXF pointer
                outBytes.SetByteListInt(objcTexfOffset, outBytes.Count);

                //Write TEXF
                for (int texfId = 0; texfId < texfList.Count; texfId++)
                {
                    outBytes.AddRange(DataHelpers.ConvertStruct(texfList[texfId]));
                }
                outBytes.AlignWriter(0x10);
            }

            //UNRM
            if (unrms != null)
            {
                int meshIDPointerOffset = 0;
                int vertIDPointerOffset = 0;

                //Write UNRM pointer
                outBytes.SetByteListInt(objcUnrmOffset, outBytes.Count);

                //Write UNRM
                outBytes.AddRange(BitConverter.GetBytes(unrms.vertGroupCountCount));
                DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
                outBytes.AddRange(BitConverter.GetBytes(outBytes.Count + 0x1C)); //Should always start a set amount after here
                outBytes.AddRange(BitConverter.GetBytes(unrms.vertCount));
                meshIDPointerOffset = DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
                outBytes.AddRange(BitConverter.GetBytes(unrms.meshIdOffset));
                vertIDPointerOffset = DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count, 1);
                outBytes.AddRange(BitConverter.GetBytes(unrms.vertIDOffset));
                outBytes.AddRange(BitConverter.GetBytes(unrms.padding0));
                outBytes.AddRange(BitConverter.GetBytes(unrms.padding1));

                //Write group counts
                for (int i = 0; i < unrms.unrmVertGroups.Count; i++)
                {
                    outBytes.AddRange(BitConverter.GetBytes(unrms.unrmVertGroups[i]));
                }
                outBytes.AlignWriter(0x10);

                //Write Mesh Ids
                outBytes.SetByteListInt(meshIDPointerOffset, outBytes.Count);
                for (int i = 0; i < unrms.unrmMeshIds.Count; i++)
                {
                    for (int j = 0; j < unrms.unrmMeshIds[i].Count; j++)
                    {
                        outBytes.AddRange(BitConverter.GetBytes(unrms.unrmMeshIds[i][j]));
                    }
                }
                outBytes.AlignWriter(0x10);

                //Write Vert Ids
                outBytes.SetByteListInt(vertIDPointerOffset, outBytes.Count);
                for (int i = 0; i < unrms.unrmMeshIds.Count; i++)
                {
                    for (int j = 0; j < unrms.unrmMeshIds[i].Count; j++)
                    {
                        outBytes.AddRange(BitConverter.GetBytes(unrms.unrmVertIds[i][j]));
                    }
                }
                outBytes.AlignWriter(0x10);
            }

            if(objc.type >= 0xC32)
            {
                //VTXE
                if (vtxeList.Count > 0)
                {
                    outBytes.SetByteListInt(objcVtxeOffset, outBytes.Count);
                    for (int vt = 0; vt < vtxeList.Count; vt++)
                    {
                        vtxeOffsets.Add(DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 4));
                        outBytes.AddRange(BitConverter.GetBytes(vtxeList[vt].vertDataTypes.Count));
                        outBytes.AddRange(BitConverter.GetBytes(0));
                    }
                    outBytes.AlignWriter(0x10);
                    for (int vt = 0; vt < vtxeList.Count; vt++)
                    {
                        outBytes.SetByteListInt(vtxeOffsets[vt], outBytes.Count);
                        for (int xe = 0; xe < vtxeList[vt].vertDataTypes.Count; xe++)
                        {
                            outBytes.AddRange(DataHelpers.ConvertStruct(vtxeList[vt].vertDataTypes[xe]));
                        }
                    }
                }

                //BonePalette
                if (bonePalette.Count > 0)
                {
                    outBytes.SetByteListInt(objcBonePaletteOffset, outBytes.Count);
                    DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 4);
                    outBytes.AddRange(BitConverter.GetBytes(bonePalette.Count));
                    outBytes.AddRange(BitConverter.GetBytes(outBytes.Count + 4));

                    //Write bones
                    for (int bn = 0; bn < bonePalette.Count; bn++)
                    {
                        outBytes.AddRange(BitConverter.GetBytes(bonePalette[bn]));
                    }
                }
                outBytes.AlignWriter(0x10);

                //UnkStruct1
                if (unkStruct1List.Count > 0)
                {
                    outBytes.SetByteListInt(objcUnkStruct1Offset, outBytes.Count);
                    for (int unk = 0; unk < unkStruct1List.Count; unk++)
                    {
                        outBytes.AddRange(DataHelpers.ConvertStruct(unkStruct1List[unk]));
                    }
                }

                //PSET 2
                if (pset2List.Count > 0)
                {
                    //Write PSET 2 pointer
                    outBytes.SetByteListInt(objcPset2Offset, outBytes.Count);

                    //Write PSET 2
                    for (int psetId = 0; psetId < pset2List.Count; psetId++)
                    {
                        pset2firstOffsets.Add(DataHelpers.NOF0Append(nof0PointerLocations, outBytes.Count + 0x8));
                        outBytes.AddRange(DataHelpers.ConvertStruct(pset2List[psetId]));
                    }
                    outBytes.AlignWriter(0x10);

                    //Write triangle groups
                    for (int stripId = 0; stripId < strips2.Count; stripId++)
                    {
                        outBytes.SetByteListInt(pset2firstOffsets[stripId], outBytes.Count);

                        for (int id = 0; id < strips2[stripId].faceGroups.Count; id++)
                        {
                            outBytes.AddRange(BitConverter.GetBytes(strips2[stripId].faceGroups[id]));
                        }
                        outBytes.AlignWriter(0x10); //These should all align already, but just in case
                    }
                }

                //MESH 2
                if (mesh2List.Count > 0)
                {
                    //Write MESH 2 pointer
                    outBytes.SetByteListInt(objcMesh2Offset, outBytes.Count);

                    //Write MESH 2
                    for (int meshId = 0; meshId < mesh2List.Count; meshId++)
                    {
                        outBytes.AddRange(DataHelpers.ConvertStruct(mesh2List[meshId]));
                    }
                    outBytes.AlignWriter(0x10);
                }

                //Strip data 2, used for strip set 3 (strip set 2 pulls from the main index set)
                if (strips3.Count > 0)
                {
                    outBytes.SetByteListInt(objcGlobalStrip3Offset, outBytes.Count);

                    //Write triangles. NGS doesn't use tristrips anymore
                    //Assumes the ushorts are already laid out as triangles
                    for (int i = 0; i < strips3.Count; i++)
                    {
                        foreach (var id in strips3[i].triStrips)
                        {
                            outBytes.AddRange(BitConverter.GetBytes(id));
                        }
                    }
                    outBytes.AlignWriter(0x10);
                }

                //Strip lengths (Strip set 3 seemingly uses a simplified method for storing these)
                if (strips3Lengths.Count > 0)
                {
                    outBytes.SetByteListInt(objcGlobalStrip3LengthOffset3, outBytes.Count);
                    for (int i = 0; i < strips3Lengths.Count; i++)
                    {
                        outBytes.AddRange(BitConverter.GetBytes(strips3Lengths[i]));
                    }
                    outBytes.AlignWriter(0x10);
                }

                //Point arrays
                if (unkPointArray1.Count > 0)
                {
                    outBytes.SetByteListInt(objcUnkPointArray1Offset, outBytes.Count);
                    for (int i = 0; i < unkPointArray1.Count; i++)
                    {
                        outBytes.AddRange(DataHelpers.ConvertStruct(unkPointArray1[i]));
                    }
                }
                if (unkPointArray2.Count > 0)
                {
                    outBytes.SetByteListInt(objcUnkPointArray2Offset, outBytes.Count);
                    for (int i = 0; i < unkPointArray2.Count; i++)
                    {
                        outBytes.AddRange(DataHelpers.ConvertStruct(unkPointArray2[i]));
                    }
                }
            }


            //Write REL0 Size
            outBytes.SetByteListInt(rel0SizeOffset, outBytes.Count - 0x8);

            //NOF0
            int NOF0Offset = outBytes.Count;
            int NOF0Size = (nof0PointerLocations.Count + 2) * 4;
            int NOF0FullSize = NOF0Size + 0x8;
            outBytes.AddRange(Encoding.UTF8.GetBytes("NOF0"));
            outBytes.AddRange(BitConverter.GetBytes(NOF0Size));
            outBytes.AddRange(BitConverter.GetBytes(nof0PointerLocations.Count));
            outBytes.AddRange(BitConverter.GetBytes(0));

            //Write pointer offsets
            for (int i = 0; i < nof0PointerLocations.Count; i++)
            {
                outBytes.AddRange(BitConverter.GetBytes(nof0PointerLocations[i]));
            }
            NOF0FullSize += outBytes.AlignWriter(0x10);

            //NEND
            outBytes.AddRange(Encoding.UTF8.GetBytes("NEND"));
            outBytes.AddRange(BitConverter.GetBytes(0x8));
            outBytes.AddRange(BitConverter.GetBytes(0));
            outBytes.AddRange(BitConverter.GetBytes(0));

            //Generate NIFL
            NIFL nifl = new NIFL();
            nifl.magic = BitConverter.ToInt32(Encoding.UTF8.GetBytes("NIFL"), 0);
            nifl.NIFLLength = 0x18;
            nifl.unkInt0 = 1;
            nifl.offsetAddition = 0x20;

            nifl.NOF0Offset = NOF0Offset;
            nifl.NOF0OffsetFull = NOF0Offset + 0x20;
            nifl.NOF0BlockSize = NOF0FullSize;
            nifl.padding0 = 0;

            //Write NIFL
            outBytes.InsertRange(0, DataHelpers.ConvertStruct(nifl));

            return outBytes.ToArray();
        }
        #endregion

        #region VTBFReading
        public override void ReadVTBFFile(BufferedStreamReaderBE<MemoryStream> sr)
        {
            int objcCount = 0;
            sr.Seek(0x10, SeekOrigin.Current); //Skip the header and move to the tags
            List<List<ushort>> bp = null;
            List<List<ushort>> ev = null;
            while (sr.Position < sr.BaseStream.Length)
            {
                var data = VTBFMethods.ReadVTBFTag(sr, out string tagType, out int ptrCount, out int entryCount);

                //Force stop if we've already hit the end
                if (data == null)
                {
                    break;
                }
                switch (tagType)
                {
                    case "ROOT":
                        //We don't do anything with this right now.
                        break;
                    case "OBJC":
                        objcCount++;
                        if (objcCount > 1)
                        {
                            throw new System.Exception("More than one OBJC! Weird things are going on!");
                        }
                        objc = new OBJC();
                        objc.ReadVTBF(data);
                        break;
                    case "VSET":
                        vsetList = ParseVSET(data, out List<List<ushort>> bonePalettes, out List<List<ushort>> edgeVertsLists);
                        bp = bonePalettes;
                        ev = edgeVertsLists;
                        break;
                    case "VTXE":
                        var data2 = VTBFMethods.ReadVTBFTag(sr, out string tagType2, out int ptrCount2, out int entryCount2);
                        if (tagType2 != "VTXL")
                        {
                            throw new System.Exception("VTXE without paired VTXL! Please report!");
                        }
                        VTXE vtxe = new VTXE(data);
                        vtxeList.Add(vtxe);
                        vtxlList.Add(new VTXL(data2, vtxe));
                        break;
                    case "PSET":
                        ParsePSET(data);
                        break;
                    case "MESH":
                        ParseMESH(data);
                        break;
                    case "MATE":
                        ParseMATE(data);
                        break;
                    case "REND":
                        ParseREND(data);
                        break;
                    case "SHAD":
                        ParseSHAD(data);
                        break;
                    case "TSTA":
                        ParseTSTA(data);
                        break;
                    case "TSET":
                        ParseTSET(data);
                        break;
                    case "TEXF":
                        ParseTEXF(data);
                        break;
                    case "UNRM":
                        ParseUNRM(data);
                        break;
                    case "SHAP":
                        //Poor SHAP. It's empty and useless. If it's not, we should probably do something though.
                        if (entryCount > 2)
                        {
                            throw new System.Exception("SHAP has more than 2 entries! Please report!");
                        }
                        break;
                    default:
                        //Should mean it's done.
                        Debug.WriteLine($"Defaulted tag was: {tagType}");
                        break;
                }
            }
            //Assign edgeverts and bone palettes. Assign them here in case of weird ordering shenanigans
            if (bp != null)
            {
                for (int i = 0; i < bp.Count; i++)
                {
                    vtxlList[i].bonePalette = bp[i];
                }
            }
            if (ev != null)
            {
                for (int i = 0; i < ev.Count; i++)
                {
                    vtxlList[i].edgeVerts = ev[i];
                }
            }
        }

        public static unsafe List<TEXF> ParseTEXF(List<Dictionary<int, object>> texfRaw)
        {
            List<TEXF> texfList = new List<TEXF>();

            //Make sure there are texture refs to get
            if (texfRaw[0].Keys.Count > 1)
            {
                for (int i = 0; i < texfRaw.Count; i++)
                {
                    texfList.Add(new TEXF((byte[])texfRaw[i][0x80]));
                }
            }
            return texfList;
        }

        //Technically, this data is written out as a list, but should only ever have one entry.
        public unsafe void ParseUNRM(List<Dictionary<int, object>> unrmRaw)
        {
            unrms = new UNRM(unrmRaw);
        }

        public unsafe void ParseTSET(List<Dictionary<int, object>> tsetRaw)
        {
            for (int i = 0; i < tsetRaw.Count; i++)
            {
                tsetList.Add(new TSET(tsetRaw[i]));
            }
        }

        public unsafe void ParseTSTA(List<Dictionary<int, object>> tstaRaw)
        {
            //Make sure there are actually textures
            if (tstaRaw[0].Keys.Count > 1)
            {
                for (int i = 0; i < tstaRaw.Count; i++)
                {
                    tstaList.Add(new TSTA(tstaRaw[i]));
                }
            }
        }

        public static unsafe List<SHAD> ParseSHAD(List<Dictionary<int, object>> shadRaw)
        {
            List<SHAD> shadList = new List<SHAD>();

            for (int i = 0; i < shadRaw.Count; i++)
            {
                shadList.Add(new SHAD(shadRaw[i]));
            }

            return shadList;
        }

        public static unsafe List<REND> ParseREND(List<Dictionary<int, object>> rendRaw)
        {
            List<REND> rendList = new List<REND>();

            for (int i = 0; i < rendRaw.Count; i++)
            {
                rendList.Add(new REND(rendRaw[i]));
            }

            return rendList;
        }

        public static unsafe List<MATE> ParseMATE(List<Dictionary<int, object>> mateRaw)
        {
            List<MATE> mateList = new List<MATE>();
            for (int i = 0; i < mateRaw.Count; i++)
            {
                mateList.Add(new MATE(mateRaw[i]));
            }

            return mateList;
        }


        public void ParseMESH(List<Dictionary<int, object>> meshRaw)
        {
            for (int i = 0; i < meshRaw.Count; i++)
            {
                meshList.Add(new MESH(meshRaw[i]));
            }
        }


        public void ParsePSET(List<Dictionary<int, object>> psetRaw)
        {
            for (int i = 0; i < psetRaw.Count; i++)
            {
                PSET pset = new PSET(psetRaw[i]);
                StripData strip = new StripData(psetRaw[i]);

                psetList.Add(pset);
                strips.Add(strip);
            }
        }

        public static List<VSET> ParseVSET(List<Dictionary<int, object>> vsetRaw, out List<List<ushort>> bonePalettes, out List<List<ushort>> edgeVertsLists)
        {
            List<VSET> vsetList = new List<VSET>();
            bonePalettes = new List<List<ushort>>();
            edgeVertsLists = new List<List<ushort>>();

            for (int i = 0; i < vsetRaw.Count; i++)
            {

                vsetList.Add(VSET.ParseVSET(vsetRaw[i], out var bonePalette, out var edgeVertsList));
                bonePalettes.Add(bonePalette);
                edgeVertsLists.Add(edgeVertsList);
            }

            return vsetList;
        }
        #endregion

        #region VTBFWriting
        public override byte[] GetBytesVTBF()
        {
            List<byte> outBytes = new List<byte>();
            outBytes.AddRange(VTBFMethods.ToAQGFVTBF());
            outBytes.AddRange(VTBFMethods.ToROOT());
            outBytes.AddRange(objc.GetBytesVTBF(unrms != null));
            outBytes.AddRange(ToVSETList());
            for (int j = 0; j < meshList.Count; j++)
            {
                var vtxe = vtxeList[meshList[j].vsetIndex];
                outBytes.AddRange(vtxe.GetBytesVTBF());
                outBytes.AddRange(vtxlList[meshList[j].vsetIndex].GetBytesVTBF(vtxe));
            }
            outBytes.AddRange(ToPSET());
            outBytes.AddRange(ToMESH());
            outBytes.AddRange(ToMATE());
            outBytes.AddRange(ToREND());
            outBytes.AddRange(ToSHAD()); //Handles SHAP as well
            outBytes.AddRange(ToTSTA());
            outBytes.AddRange(ToTSET());
            outBytes.AddRange(ToTEXF());
            if (unrms != null)
            {
                outBytes.AddRange(ToUNRM());
            }

            return outBytes.ToArray();
        }

        public unsafe byte[] ToUNRM()
        {
            return unrms.GetVTBFBytes();
        }

        public unsafe byte[] ToTEXF()
        {
            List<byte> outBytes = new List<byte>();

            //Normally the FC tag is included in the count of the rest of these, but when there's no tags we account for it here.
            int emptyArray = 0;
            if (texfList.Count == 0)
            {
                emptyArray++;
            }

            outBytes.AddRange(BitConverter.GetBytes((short)0xFC));
            for (int i = 0; i < texfList.Count; i++)
            {
                TEXF texf = texfList[i];
                if (i != 0)
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFE));
                }

                //TexName String
                string texNameStr = texf.texName.GetString();
                VTBFMethods.AddBytes(outBytes, 0x80, 0x02, (byte)texNameStr.Length, Encoding.UTF8.GetBytes(texNameStr));
            }
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));

            //Pointer count. Always 0 on TEXF
            //Subtag count. 2 for each TEXF + 1 for the end tag, always.
            VTBFMethods.WriteTagHeader(outBytes, "TEXF", 0, (ushort)(texfList.Count * 0x2 + 0x1 + emptyArray));

            return outBytes.ToArray();
        }

        public unsafe byte[] ToTSTA()
        {
            List<byte> outBytes = new List<byte>();

            //Normally the FC tag is included in the count of the rest of these, but when there's no tags we account for it here.
            int emptyArray = 0;
            if (tstaList.Count == 0)
            {
                emptyArray++;
            }

            outBytes.AddRange(BitConverter.GetBytes((short)0xFC)); //Always there, even if there's nothing in the list
            for (int i = 0; i < tstaList.Count; i++)
            {
                TSTA tsta = tstaList[i];
                if (i != 0)
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFE));
                }

                VTBFMethods.AddBytes(outBytes, 0x60, 0x9, BitConverter.GetBytes(tstaList[i].tag));
                VTBFMethods.AddBytes(outBytes, 0x61, 0x9, BitConverter.GetBytes(tstaList[i].texUsageOrder));
                VTBFMethods.AddBytes(outBytes, 0x62, 0x9, BitConverter.GetBytes(tstaList[i].modelUVSet));
                VTBFMethods.AddBytes(outBytes, 0x63, 0x4A, 0x1, DataHelpers.ConvertStruct(tstaList[i].unkVector0));
                VTBFMethods.AddBytes(outBytes, 0x64, 0x9, BitConverter.GetBytes((int)tstaList[i].unkFloat2));
                VTBFMethods.AddBytes(outBytes, 0x65, 0x9, BitConverter.GetBytes((int)tstaList[i].unkFloat3));
                VTBFMethods.AddBytes(outBytes, 0x66, 0x9, BitConverter.GetBytes((int)tstaList[i].unkFloat4));
                VTBFMethods.AddBytes(outBytes, 0x67, 0x9, BitConverter.GetBytes(tstaList[i].unkInt3));
                VTBFMethods.AddBytes(outBytes, 0x68, 0x9, BitConverter.GetBytes(tstaList[i].unkInt4));
                VTBFMethods.AddBytes(outBytes, 0x69, 0x9, BitConverter.GetBytes(tstaList[i].unkInt5));
                VTBFMethods.AddBytes(outBytes, 0x6A, 0xA, BitConverter.GetBytes(tstaList[i].unkFloat0));
                VTBFMethods.AddBytes(outBytes, 0x6B, 0xA, BitConverter.GetBytes(tstaList[i].unkFloat1));

                //TexName String
                string texNameStr = tsta.texName.GetString();
                VTBFMethods.AddBytes(outBytes, 0x6C, 0x02, (byte)texNameStr.Length, Encoding.UTF8.GetBytes(texNameStr));
            }
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));
            VTBFMethods.WriteTagHeader(outBytes, "TSTA", 0, (ushort)(tstaList.Count * 0xE + 0x1 + emptyArray));

            return outBytes.ToArray();
        }


        public unsafe byte[] ToTSET()
        {
            List<byte> outBytes = new List<byte>();

            for (int i = 0; i < tsetList.Count; i++)
            {
                TSET tset = tsetList[i];
                if (i == 0)
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFC));
                }
                else
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFE));
                }

                VTBFMethods.AddBytes(outBytes, 0x70, 0x9, BitConverter.GetBytes(tset.unkInt0));
                VTBFMethods.AddBytes(outBytes, 0x71, 0x8, BitConverter.GetBytes(tset.texCount));
                VTBFMethods.AddBytes(outBytes, 0x72, 0x9, BitConverter.GetBytes(tset.unkInt1));
                VTBFMethods.AddBytes(outBytes, 0x73, 0x9, BitConverter.GetBytes(tset.unkInt2));
                VTBFMethods.AddBytes(outBytes, 0x74, 0x9, BitConverter.GetBytes(tset.unkInt3));
                VTBFMethods.AddBytes(outBytes, 0x75, 0x88, 0x8, 0x3, BitConverter.GetBytes(tset.tstaTexIDs[0]));
                for (int j = 1; j < 4; j++)
                {
                    if (tset.tstaTexIDs.Count > j)
                    {
                        outBytes.AddRange(BitConverter.GetBytes(tset.tstaTexIDs[j]));
                    }
                    else
                    {
                        outBytes.AddRange(BitConverter.GetBytes((int)-1));
                    }
                }
            }
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));

            //Pointer count. Always 0 on TSET
            //Subtag count. 7 for each TSET + 1 for the end tag, always.
            VTBFMethods.WriteTagHeader(outBytes, "TSET", 0, (ushort)(tsetList.Count * 0x7 + 0x1));

            return outBytes.ToArray();
        }

        public unsafe byte[] ToSHAD()
        {
            List<byte> outBytes = new List<byte>();


            for (int i = 0; i < shadList.Count; i++)
            {
                SHAD shad = shadList[i];
                if (i == 0)
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFC));
                }
                else
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFE));
                }

                VTBFMethods.AddBytes(outBytes, 0x90, 0x9, BitConverter.GetBytes(shad.unk0));

                //Pixel Shader String
                string pixelStr = shad.pixelShader.GetString();
                VTBFMethods.AddBytes(outBytes, 0x91, 0x02, (byte)pixelStr.Length, Encoding.UTF8.GetBytes(pixelStr));

                //Vertex Shader String
                string vertStr = shad.vertexShader.GetString();
                VTBFMethods.AddBytes(outBytes, 0x92, 0x02, (byte)vertStr.Length, Encoding.UTF8.GetBytes(vertStr));

                VTBFMethods.AddBytes(outBytes, 0x93, 0x9, BitConverter.GetBytes(shad.shadDetailOffset));

            }
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));

            //Pointer count. SHAD struct count on SHAD.
            //Subtag count. 18 for each SHAD + 1 for the end tag, always.
            VTBFMethods.WriteTagHeader(outBytes, "SHAD", (ushort)shadList.Count, (ushort)(shadList.Count * 0x5 + 0x1));

            //There's one of these for each SHAD, but they don't seem to have any meaningful contents in observed files
            for (int i = 0; i < shadList.Count; i++)
            {
                outBytes.AddRange(Encoding.UTF8.GetBytes("vtc0"));
                outBytes.AddRange(BitConverter.GetBytes(0xC));
                outBytes.AddRange(Encoding.UTF8.GetBytes("SHAP"));
                outBytes.AddRange(BitConverter.GetBytes((short)0));
                outBytes.AddRange(BitConverter.GetBytes((short)2));
                outBytes.AddRange(BitConverter.GetBytes((short)0xFC));
                outBytes.AddRange(BitConverter.GetBytes((short)0xFD));
            }

            return outBytes.ToArray();
        }

        public unsafe byte[] ToREND()
        {
            List<byte> outBytes = new List<byte>();

            for (int i = 0; i < rendList.Count; i++)
            {
                if (i == 0)
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFC));
                }
                else
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFE));
                }

                VTBFMethods.AddBytes(outBytes, 0x40, 0x9, BitConverter.GetBytes(rendList[i].tag));
                VTBFMethods.AddBytes(outBytes, 0x41, 0x9, BitConverter.GetBytes(rendList[i].unk0));
                VTBFMethods.AddBytes(outBytes, 0x42, 0x9, BitConverter.GetBytes(rendList[i].twosided));
                VTBFMethods.AddBytes(outBytes, 0x43, 0x9, BitConverter.GetBytes(rendList[i].int_0C));

                VTBFMethods.AddBytes(outBytes, 0x44, 0x9, BitConverter.GetBytes(rendList[i].sourceAlpha));
                VTBFMethods.AddBytes(outBytes, 0x45, 0x9, BitConverter.GetBytes(rendList[i].destinationAlpha));
                VTBFMethods.AddBytes(outBytes, 0x46, 0x9, BitConverter.GetBytes(rendList[i].unk3));
                VTBFMethods.AddBytes(outBytes, 0x47, 0x9, BitConverter.GetBytes(rendList[i].unk4));

                VTBFMethods.AddBytes(outBytes, 0x48, 0x9, BitConverter.GetBytes(rendList[i].unk5));
                VTBFMethods.AddBytes(outBytes, 0x49, 0x9, BitConverter.GetBytes(rendList[i].unk6));
                VTBFMethods.AddBytes(outBytes, 0x4A, 0x9, BitConverter.GetBytes(rendList[i].unk7));
                VTBFMethods.AddBytes(outBytes, 0x4B, 0x9, BitConverter.GetBytes(rendList[i].unk8));

                VTBFMethods.AddBytes(outBytes, 0x4C, 0x9, BitConverter.GetBytes(rendList[i].unk9));
                VTBFMethods.AddBytes(outBytes, 0x4D, 0x9, BitConverter.GetBytes(rendList[i].alphaCutoff));
                VTBFMethods.AddBytes(outBytes, 0x4E, 0x9, BitConverter.GetBytes(rendList[i].unk11));
                VTBFMethods.AddBytes(outBytes, 0x4F, 0x9, BitConverter.GetBytes(rendList[i].unk12));

                VTBFMethods.AddBytes(outBytes, 0x50, 0x9, BitConverter.GetBytes(rendList[i].unk13));
            }
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));

            //Pointer count. Always 0 on REND
            //Subtag count. 18 for each REND + 1 for the end tag, always.
            VTBFMethods.WriteTagHeader(outBytes, "REND", 0, (ushort)(rendList.Count * 0x12 + 0x1));

            return outBytes.ToArray();
        }


        public unsafe byte[] ToMATE()
        {
            List<byte> outBytes = new List<byte>();

            for (int i = 0; i < mateList.Count; i++)
            {
                //Gotta make a local accessor for fixed arrays
                MATE mate = mateList[i];
                if (i == 0)
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFC));
                }
                else
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFE));
                }

                VTBFMethods.AddBytes(outBytes, 0x30, 0x4A, 0x2, DataHelpers.ConvertStruct(mate.diffuseRGBA));
                VTBFMethods.AddBytes(outBytes, 0x31, 0x4A, 0x2, DataHelpers.ConvertStruct(mate.unkRGBA0));
                VTBFMethods.AddBytes(outBytes, 0x32, 0x4A, 0x2, DataHelpers.ConvertStruct(mate._sRGBA));
                VTBFMethods.AddBytes(outBytes, 0x33, 0x4A, 0x2, DataHelpers.ConvertStruct(mate.unkRGBA1));
                VTBFMethods.AddBytes(outBytes, 0x34, 0x9, BitConverter.GetBytes(mate.reserve0));
                VTBFMethods.AddBytes(outBytes, 0x35, 0xA, BitConverter.GetBytes(mate.unkFloat0));
                VTBFMethods.AddBytes(outBytes, 0x36, 0xA, BitConverter.GetBytes(mate.unkFloat1));
                VTBFMethods.AddBytes(outBytes, 0x37, 0x9, BitConverter.GetBytes(mate.unkInt0));
                VTBFMethods.AddBytes(outBytes, 0x38, 0x9, BitConverter.GetBytes(mate.unkInt1));

                //Alpha Type String
                string alphaStr = mate.alphaType.GetString();
                VTBFMethods.AddBytes(outBytes, 0x3A, 0x02, (byte)alphaStr.Length, Encoding.UTF8.GetBytes(alphaStr));

                //Mat Name String. Do it this way in case of names that would break when encoded to utf8 again
                int matLen = mate.matName.GetLength();
                byte[] matBytes = new byte[matLen];
                byte[] tempMatBytes = mate.matName.GetBytes();
                for (int strIndex = 0; strIndex < matLen; strIndex++)
                {
                    matBytes[strIndex] = tempMatBytes[strIndex];
                }
                VTBFMethods.AddBytes(outBytes, 0x39, 0x02, (byte)matLen, matBytes);
            }
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));

            //Pointer count. Always 0 on MATE
            //Subtag count. 12 for each MATE + 1 for the end tag, always.
            VTBFMethods.WriteTagHeader(outBytes, "MATE", 0, (ushort)(mateList.Count * 0xC + 0x1));

            return outBytes.ToArray();
        }

        public byte[] ToMESH()
        {
            List<byte> outBytes = new List<byte>();

            for (int i = 0; i < meshList.Count; i++)
            {
                if (i == 0)
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFC));
                }
                else
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFE));
                }
                int shorts = meshList[i].flags + (meshList[i].unkShort0 * 0x10000);
                int bytes = meshList[i].unkByte0 + (meshList[i].unkByte1 * 0x100) + (meshList[i].unkShort1 * 0x10000);
                VTBFMethods.AddBytes(outBytes, 0xB0, 0x9, BitConverter.GetBytes(shorts));
                VTBFMethods.AddBytes(outBytes, 0xC7, 0x9, BitConverter.GetBytes(bytes));
                VTBFMethods.AddBytes(outBytes, 0xB1, 0x8, BitConverter.GetBytes(meshList[i].mateIndex));
                VTBFMethods.AddBytes(outBytes, 0xB2, 0x8, BitConverter.GetBytes(meshList[i].rendIndex));
                VTBFMethods.AddBytes(outBytes, 0xB3, 0x8, BitConverter.GetBytes(meshList[i].shadIndex));
                VTBFMethods.AddBytes(outBytes, 0xB4, 0x8, BitConverter.GetBytes(meshList[i].tsetIndex));
                VTBFMethods.AddBytes(outBytes, 0xB5, 0x8, BitConverter.GetBytes(meshList[i].baseMeshNodeId));
                VTBFMethods.AddBytes(outBytes, 0xC0, 0x8, BitConverter.GetBytes(meshList[i].vsetIndex));
                VTBFMethods.AddBytes(outBytes, 0xC1, 0x8, BitConverter.GetBytes(meshList[i].psetIndex));
                VTBFMethods.AddBytes(outBytes, 0xCD, 0x8, BitConverter.GetBytes(meshList[i].unkInt0));
                VTBFMethods.AddBytes(outBytes, 0xC2, 0x9, BitConverter.GetBytes(meshList[i].baseMeshDummyId));
            }

            //Pointer count. Always 0 on MESH
            //Subtag count. 11 for each MESH + 1 for the end tag, always.
            VTBFMethods.WriteTagHeader(outBytes, "MESH", 0, (ushort)(meshList.Count * 0xC));

            return outBytes.ToArray();
        }

        public byte[] ToPSET()
        {
            List<byte> outBytes = new List<byte>();
            for (int i = 0; i < psetList.Count; i++)
            {
                if (i == 0)
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFC));
                }
                else
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFE));
                }
                VTBFMethods.AddBytes(outBytes, 0xC6, 0x9, BitConverter.GetBytes(psetList[i].tag));
                VTBFMethods.AddBytes(outBytes, 0xBB, 0x9, BitConverter.GetBytes(psetList[i].faceGroupCount));
                VTBFMethods.AddBytes(outBytes, 0xBC, 0x9, BitConverter.GetBytes(psetList[i].psetFaceCount));
                VTBFMethods.AddBytes(outBytes, 0xB7, 0x9, BitConverter.GetBytes(strips[i].triIdCount));

                outBytes.Add(0xB8);
                outBytes.Add(0x86);
                if (strips[i].triIdCount - 1 > byte.MaxValue)
                {
                    if (strips[i].triIdCount - 1 > ushort.MaxValue)
                    {
                        outBytes.Add(0x18);
                        outBytes.AddRange(BitConverter.GetBytes(strips[i].triStrips.Count - 1));
                    }
                    else
                    {
                        outBytes.Add(0x10);
                        outBytes.AddRange(BitConverter.GetBytes((short)(strips[i].triStrips.Count - 1)));
                    }
                }
                else
                {
                    outBytes.Add(0x8);
                    outBytes.Add((byte)(strips[i].triStrips.Count - 1));
                }
                for (int j = 0; j < strips[i].triStrips.Count; j++)
                {
                    outBytes.AddRange(BitConverter.GetBytes(strips[i].triStrips[j]));
                }

                VTBFMethods.AddBytes(outBytes, 0xC5, 0x9, BitConverter.GetBytes(psetList[i].stripStartCount));
            }
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));

            //Pointer count. Always 0 on PSET
            //Subtag count. 7 for each PSET + 1 for the end tag, always.
            VTBFMethods.WriteTagHeader(outBytes, "PSET", 0, (ushort)(psetList.Count * 0x7 + 0x1));

            return outBytes.ToArray();
        }

        public byte[] ToVSETList()
        {
            List<byte> outBytes = new List<byte>();
            int subTagCount = 0;

            for (int i = 0; i < vsetList.Count; i++)
            {
                subTagCount += 0x9; //Each vset substruct adds this many sub tags every time.
                if (i == 0)
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFC));
                }
                else
                {
                    outBytes.AddRange(BitConverter.GetBytes((short)0xFE));
                }
                VTBFMethods.AddBytes(outBytes, 0xB6, 0x9, BitConverter.GetBytes(vsetList[i].vertDataSize));
                VTBFMethods.AddBytes(outBytes, 0xBF, 0x9, BitConverter.GetBytes(vsetList[i].vtxeCount));
                VTBFMethods.AddBytes(outBytes, 0xB9, 0x9, BitConverter.GetBytes(vsetList[i].vtxlCount));
                VTBFMethods.AddBytes(outBytes, 0xC4, 0x9, BitConverter.GetBytes(vsetList[i].vtxlStartVert));

                if (vtxlList[i].bonePalette != null)
                {
                    if (vtxlList[i].bonePalette.Count > 0)
                    {
                        VTBFMethods.AddBytes(outBytes, 0xBD, 0x9, BitConverter.GetBytes(vtxlList[i].bonePalette.Count));
                        subTagCount++;

                        outBytes.Add(0xBE);
                        outBytes.Add(0x86);
                        outBytes.Add(0x8);
                        outBytes.Add((byte)(vtxlList[i].bonePalette.Count - 1));
                        for (int j = 0; j < vtxlList[i].bonePalette.Count; j++)
                        {
                            outBytes.AddRange(BitConverter.GetBytes(vtxlList[i].bonePalette[j]));
                        }
                    }
                    else
                    {
                        VTBFMethods.AddBytes(outBytes, 0xBD, 0x9, BitConverter.GetBytes((int)0));
                    }

                }
                else
                {
                    VTBFMethods.AddBytes(outBytes, 0xBD, 0x9, BitConverter.GetBytes((int)0));
                }

                VTBFMethods.AddBytes(outBytes, 0xC8, 0x9, BitConverter.GetBytes(vsetList[i].unk0));
                VTBFMethods.AddBytes(outBytes, 0xCC, 0x9, BitConverter.GetBytes(vsetList[i].unk1));

                if (vtxlList[i].edgeVerts != null)
                {
                    if (vtxlList[i].edgeVerts.Count > 0)
                    {

                        VTBFMethods.AddBytes(outBytes, 0xC9, 0x9, BitConverter.GetBytes(vtxlList[i].edgeVerts.Count));
                        subTagCount++;

                        outBytes.Add(0xCA);
                        outBytes.Add(0x86);
                        outBytes.Add(0x8);
                        outBytes.Add((byte)(vtxlList[i].edgeVerts.Count - 1));
                        for (int j = 0; j < vtxlList[i].edgeVerts.Count; j++)
                        {
                            outBytes.AddRange(BitConverter.GetBytes(vtxlList[i].edgeVerts[j]));
                        }
                    }
                    else
                    {
                        VTBFMethods.AddBytes(outBytes, 0xC9, 0x9, BitConverter.GetBytes((int)0));
                    }

                }
                else
                {
                    VTBFMethods.AddBytes(outBytes, 0xC9, 0x9, BitConverter.GetBytes((int)0));
                }

            }
            outBytes.AddRange(BitConverter.GetBytes((short)0xFD));
            subTagCount++;

            //In VTBF, VSETS are all treated as part of the same struct

            //Pointer count. In this case, 0x2 times the VSET count.
            //Subtag count
            VTBFMethods.WriteTagHeader(outBytes, "VSET", (ushort)(0x2 * vsetList.Count), (ushort)(subTagCount));

            return outBytes.ToArray();
        }
        #endregion

        public void CreateTrueVertWeights()
        {
            foreach (var vtxl in vtxlList)
            {
                vtxl.createTrueVertWeights();
            }
        }

        public void ChangeTexExtension(string ext)
        {
            for (int i = 0; i < texfList.Count; i++)
            {
                texFUnicodeNames[i] = Path.ChangeExtension(texFUnicodeNames[i], ext);
            }
            for (int i = 0; i < texfList.Count; i++)
            {
                texfList[i].texName.SetString(Path.ChangeExtension(texfList[i].texName.GetString(), ext));
            }
            for (int i = 0; i < tstaList.Count; i++)
            {
                tstaList[i].texName.SetString(Path.ChangeExtension(tstaList[i].texName.GetString(), ext));
            }
        }

        //0xC33 variations of the format can recycle vtxl lists for multiple meshes. Neat, but not helpful for conversion purposes.
        public int splitVSETPerMesh()
        {
            if(meshList.Count < 2 || objc.type < 0xC32)
            {
                return vsetList.Count;
            }
            bool continueSplitting = false;
            Dictionary<int, List<int>> vsetTracker = new Dictionary<int, List<int>>(); //Key int is a VSET, value is a list of indices for each mesh that uses said VSET
            for (int meshId = 0; meshId < meshList.Count; meshId++)
            {
                if (!vsetTracker.ContainsKey(meshList[meshId].vsetIndex))
                {
                    vsetTracker.Add(meshList[meshId].vsetIndex, new List<int>() { meshId });
                }
                else
                {
                    continueSplitting = true;
                    vsetTracker[meshList[meshId].vsetIndex].Add(meshId);
                }
            }

            if (continueSplitting)
            {
                VSET[] newVsetArray = new VSET[meshList.Count];
                VTXL[] newVtxlArray = new VTXL[meshList.Count];

                //Handle instances in which there are multiple of the same VSET used.
                //VTXL and VSETs should be cloned and updated as necessary while strips should be updated to match new vertex ids (strips using the same VTXL continue from old Ids, typically)
                foreach (var key in vsetTracker.Keys)
                {
                    if (vsetTracker[key].Count > 1)
                    {
                        foreach (int meshId in vsetTracker[key])
                        {
                            if (meshList[meshId].vsetIndex >= 0 && meshList[meshId].psetIndex >= 0)
                            {
                                Dictionary<int, int> usedVerts = new Dictionary<int, int>();
                                VSET newVset = new VSET();
                                VTXL newVtxl = new VTXL();

                                int counter = 0;
                                for (int stripIndex = 0; stripIndex < strips[meshId].triStrips.Count; stripIndex++)
                                {
                                    ushort id = strips[meshId].triStrips[stripIndex];
                                    if (!usedVerts.ContainsKey(id))
                                    {
                                        VTXL.AppendVertex(vtxlList[meshList[meshId].vsetIndex], newVtxl, id);
                                        usedVerts.Add(id, counter);
                                        counter++;
                                    }
                                    strips[meshId].triStrips[stripIndex] = (ushort)usedVerts[id];
                                }
                                var tempMesh = meshList[meshId];
                                tempMesh.vsetIndex = meshId;
                                meshList[meshId] = tempMesh;
                                newVsetArray[meshId] = newVset;
                                newVtxlArray[meshId] = newVtxl;
                            }
                        }
                    }
                    else
                    {
                        int meshId = vsetTracker[key][0];
                        newVsetArray[meshId] = vsetList[meshList[meshId].vsetIndex];
                        newVtxlArray[meshId] = vtxlList[meshList[meshId].vsetIndex];
                        var tempMesh = meshList[meshId];
                        tempMesh.vsetIndex = meshId;
                        meshList[meshId] = tempMesh;
                    }
                }
                vsetList = newVsetArray.ToList();
                vtxlList = newVtxlArray.ToList();
            }

            List<int> badIds = new List<int>();
            for (int i = 0; i < meshList.Count; i++)
            {
                if (meshList[i].vsetIndex < 0 || meshList[i].psetIndex < 0)
                {
                    badIds.Add(i);
                }
            }

            int badCounter = 0;
            foreach (var id in badIds)
            {
                meshList.RemoveAt(id - badCounter);
                badCounter++;
            }
            objc.meshCount = meshList.Count;
            objc.vsetCount = vsetList.Count;

            return vsetList.Count;
        }

        public int getStripIndexCount()
        {
            int indexCount = 0;
            for (int i = 0; i < strips.Count; i++)
            {
                indexCount += strips[i].triIdCount;
            }
            return indexCount;
        }

        public int getVertexCount()
        {
            int vertCount = 0;
            for (int i = 0; i < vtxlList.Count; i++)
            {
                vertCount += vtxlList[i].vertPositions.Count;
            }
            return vertCount;
        }

        /// <summary>
        /// Gets a list of GenericMaterials representing all possible material component combinations used by this model based on its MESH structs.
        /// The idea would be condensing a list of traditional materials for external usage.
        /// A list of integers which map mesh indices to GenericMaterial ids is also output.
        /// </summary>
        /// <returns>List<GenericMaterial>, out List<int></returns>
        public List<GenericMaterial> GetUniqueMaterials(out List<int> meshMatMapping)
        {
            List<GenericMaterial> mats = new List<GenericMaterial>();
            meshMatMapping = new List<int>();

            for (int i = 0; i < meshList.Count; i++)
            {
                var curMesh = meshList[i];
                for (int msh = 0; msh < meshMatMapping.Count; msh++)
                {
                    var checkMesh = meshList[msh];
                    //Mate, rend, shad, and tset define what would make up a traditional material in a 3d program
                    if (curMesh.mateIndex == checkMesh.mateIndex && curMesh.rendIndex == checkMesh.rendIndex && curMesh.shadIndex == checkMesh.shadIndex && curMesh.tsetIndex == checkMesh.tsetIndex)
                    {
                        meshMatMapping.Add(meshMatMapping[msh]);
                        break;
                    }
                }

                if (meshMatMapping.Count - 1 != i)
                {
                    var curMate = mateList[curMesh.mateIndex];
                    var curRend = rendList[curMesh.rendIndex];
                    var shadNames = GetShaderNames(curMesh.shadIndex);
                    var texNames = GetTexListNamesUnicode(curMesh.tsetIndex);
                    var texUvSets = GetTexListUVChannels(curMesh.tsetIndex);
                    GenericMaterial mat = new GenericMaterial();
                    mat.texNames = texNames;
                    mat.texUVSets = texUvSets;
                    mat.shaderNames = shadNames;
                    mat.blendType = curMate.alphaType.GetString();
                    mat.specialType = GetSpecialMatType(texNames);
                    if (matUnicodeNames.Count > curMesh.mateIndex)
                    {
                        mat.matName = matUnicodeNames[curMesh.mateIndex];
                    }
                    else
                    {
                        mat.matName = curMate.matName.GetString();
                    }
                    mat.twoSided = curRend.twosided;
                    mat.alphaCutoff = curRend.alphaCutoff;
                    mat.srcAlpha = curRend.sourceAlpha;
                    mat.destAlpha = curRend.destinationAlpha;

                    mat.diffuseRGBA = curMate.diffuseRGBA;
                    mat.unkRGBA0 = curMate.unkRGBA0;
                    mat._sRGBA = curMate._sRGBA;
                    mat.unkRGBA1 = curMate.unkRGBA1;

                    mat.reserve0 = curMate.reserve0;
                    mat.unkFloat0 = curMate.unkFloat0;
                    mat.unkFloat1 = curMate.unkFloat1;
                    mat.unkInt0 = curMate.unkInt0;
                    mat.unkInt1 = curMate.unkInt1;

                    mats.Add(mat);
                    meshMatMapping.Add(mats.Count - 1);
                }
            }

            return mats;
        }

        public void FixWeightsFromBoneCount(int maxBone = int.MaxValue)
        {
            for (int i = 0; i < vtxlList.Count; i++)
            {
                vtxlList[i].fixWeightsFromBoneCount(maxBone);
            }
        }
        public void ConvertToLegacyTypes()
        {
            for (int i = 0; i < vtxlList.Count; i++)
            {
                vtxlList[i].convertToLegacyTypes();
            }
        }

        public void AddUnfilledUVs()
        {
            for (int i = 0; i < vtxlList.Count; i++)
            {
                var vtxl = vtxlList[i];
                bool addUV1 = !(vtxl.uv1List.Count > 0);
                bool addUV2 = !(vtxl.uv2List.Count > 0);
                bool addUV3 = !(vtxl.uv3List.Count > 0);
                bool addUV4 = !(vtxl.uv4List.Count > 0);
                bool addUV5 = !(vtxl.vert0x22.Count > 0);
                bool addUV6 = !(vtxl.vert0x23.Count > 0);
                bool addUV7 = !(vtxl.vert0x24.Count > 0);
                bool addUV8 = !(vtxl.vert0x25.Count > 0);

                for (int v = 0; v < vtxl.vertPositions.Count; v++)
                {
                    if (addUV1)
                    {
                        vtxl.uv1List.Add(new Vector2());
                    }
                    if (addUV2)
                    {
                        vtxl.uv2List.Add(new Vector2());
                    }
                    if (addUV3)
                    {
                        vtxl.uv3List.Add(new Vector2());
                    }
                    if (addUV4)
                    {
                        vtxl.uv4List.Add(new Vector2());
                    }
                    if (addUV5)
                    {
                        vtxl.vert0x22.Add(new short[2]);
                    }
                    if (addUV6)
                    {
                        vtxl.vert0x23.Add(new short[2]);
                    }
                    if (addUV7)
                    {
                        vtxl.vert0x24.Add(new short[2]);
                    }
                    if (addUV8)
                    {
                        vtxl.vert0x25.Add(new short[2]);
                    }
                }
            }
        }

        /// <summary>
        /// PSO2 has a distinct rendering it does for hollow vs blendalpha materials, but sometimes the render data is changed in post to reflect something different.
        /// This method checks all instances of this and fixes them, making a new REND instance as needed.
        /// </summary>
        public void FixHollowMatNaming()
        {
            //Set wrongly assigned blendalphas to hollow for reexport purposes
            for (int i = 0; i < meshList.Count; i++)
            {
                var mesh = meshList[i];
                var mate = mateList[mesh.mateIndex];
                var rend = rendList[mesh.rendIndex];
                if ((mate.alphaType.GetString() == "opaque" || mate.alphaType.GetString() == "blendalpha") && rend.int_0C == 0 && rend.unk8 == 1 && rend.twosided == 2)
                {
                    //Account for recycled material, but separate rend
                    for (int j = 0; j < meshList.Count; j++)
                    {
                        var mesh2 = meshList[i];
                        if (mesh.mateIndex == mesh2.mateIndex && mesh.rendIndex != mesh2.rendIndex)
                        {
                            mesh.mateIndex = mateList.Count;
                            mateList.Add(mate);
                            mate = mateList[mateList.Count - 1];
                            objc.mateCount += 1;
                            meshList[i] = mesh;
                            break;
                        }
                    }

                    mate.alphaType.SetString("hollow");
                    mateList[mesh.mateIndex] = mate;
                }
            }
        }

        public static string GetSpecialMatType(List<string> names)
        {
            if (names.Count > 0)
            {
                var name = names[0];
                switch (name)
                {
                    case "pl_eye_diffuse.dds":
                        return "ey";
                    case "pl_hair_diffuse.dds":
                        switch (names[names.Count - 1])
                        {
                            case "pl_hair_noise.dds":
                                return "rhr";
                            default:
                                return "hr";
                        }
                    case "pl_face_diffuse.dds":
                        return "fc";
                    case "pl_body_diffuse.dds":
                        return "pl";
                    case "pl_body_base_diffuse.dds":
                        switch (names[names.Count - 1])
                        {
                            case "pl_body_decal.dds":
                                return "rbd_d";
                            default:
                                return "rbd";
                        }
                    case "pl_body_skin_diffuse.dds":
                        return "rbd_sk";
                    case "pl_body_outer_diffuse.dds":
                        switch (names[names.Count - 1])
                        {
                            case "pl_body_decal.dds":
                                return "rbd_ou_d";
                            default:
                                return "rbd_ou";
                        }
                    default:
                        return "";
                }

            }
            else
            {
                return "";
            }
        }

        public static List<TSTA> GetTexListTSTAs(AquaObject model, int tsetIndex)
        {
            List<TSTA> textureList = new List<TSTA>();

            //Don't try to read what's not there
            if (model.tstaList.Count == 0 || model.tstaList == null)
            {
                return textureList;
            }
            TSET tset = model.tsetList[tsetIndex];

            for (int index = 0; index < tset.tstaTexIDs.Count; index++)
            {
                int texIndex = tset.tstaTexIDs[index];
                if (texIndex != -1)
                {
                    TSTA tsta = model.tstaList[texIndex];

                    textureList.Add(tsta);
                }
            }

            return textureList;
        }

        public List<string> GetTexListNames(int tsetIndex)
        {
            List<string> textureList = new List<string>();

            //Don't try to read what's not there
            if (tstaList.Count == 0 || tstaList == null)
            {
                return textureList;
            }
            TSET tset = tsetList[tsetIndex];

            for (int index = 0; index < tset.tstaTexIDs.Count; index++)
            {
                int texIndex = tset.tstaTexIDs[index];
                if (texIndex != -1)
                {
                    TSTA tsta = tstaList[texIndex];

                    textureList.Add(tsta.texName.GetString());
                }
            }

            return textureList;
        }

        public List<string> GetTexListNamesUnicode(int tsetIndex)
        {
            List<string> textureList = new List<string>();

            //Don't try to read what's not there
            if (tstaList.Count == 0 || tstaList == null)
            {
                return textureList;
            }
            TSET tset = tsetList[tsetIndex];

            for (int index = 0; index < tset.tstaTexIDs.Count; index++)
            {
                int texIndex = tset.tstaTexIDs[index];
                if (texIndex != -1)
                {
                    TSTA tsta = tstaList[texIndex];
                    var name = tsta.texName.GetString();

                    bool skip = false;
                    foreach (var str in texFUnicodeNames)
                    {
                        if (str.StartsWith(name))
                        {
                            textureList.Add(str);
                            skip = true;
                            break;
                        }
                    }
                    if (skip == true)
                    {
                        continue;
                    }
                    textureList.Add(name);
                }
            }

            return textureList;
        }

        public List<int> GetTexListUVChannels(int tsetIndex)
        {
            List<int> uvList = new List<int>();

            //Don't try to read what's not there
            if (tstaList.Count == 0 || tstaList == null)
            {
                return uvList;
            }
            TSET tset = tsetList[tsetIndex];

            for (int index = 0; index < tset.tstaTexIDs.Count; index++)
            {
                int texIndex = tset.tstaTexIDs[index];
                if (texIndex != -1)
                {
                    TSTA tsta = tstaList[texIndex];

                    uvList.Add(tsta.modelUVSet);
                }
            }

            return uvList;
        }

        public List<string> GetShaderNames(int shadIndex)
        {
            List<string> shaderList = new List<string>();

            SHAD shad = shadList[shadIndex];

            shaderList.Add(shad.pixelShader.GetString());
            shaderList.Add(shad.vertexShader.GetString());

            return shaderList;
        }

        //To be honest I don't really know what these actually do, but this seems to generate the structure roughly the way the game's exporter does.
        //Essentially, vertices between different meshes are linked together 
        public void CalcUNRMs(bool applyNormalAveraging, bool useUNRMs)
        {
            UNRM unrm = new UNRM();
            if (useUNRMs == false && applyNormalAveraging == false)
            {
                return;
            }

            //Set up a boolean array for tracking what we've gone through for this
            bool[][] meshCheckArr = new bool[vtxlList.Count][];
            for (int m = 0; m < vtxlList.Count; m++)
            {
                meshCheckArr[m] = new bool[vtxlList[m].vertPositions.Count];
            }

            for (int m = 0; m < vtxlList.Count; m++)
            {
                for (int v = 0; v < vtxlList[m].vertPositions.Count; v++)
                {
                    Vector3 normals = new Vector3();
                    if (vtxlList[m].vertNormals.Count > 0)
                    {
                        normals = vtxlList[m].vertNormals[v];
                    }

                    List<int> meshNum = new List<int>() { m };
                    List<int> vertId = new List<int>() { v };
                    //Loop through the other vertices to match them up
                    for (int n = 0; n < vtxlList.Count; n++)
                    {
                        for (int w = 0; w < vtxlList[n].vertPositions.Count; w++)
                        {
                            bool sameVert = (m == n && v == w);
                            if (!sameVert && vtxlList[n].vertPositions[w].Equals(vtxlList[m].vertPositions[v]) && !meshCheckArr[n][w])
                            {
                                meshCheckArr[n][w] = true;
                                meshNum.Add(n);
                                vertId.Add(w);
                                if (applyNormalAveraging && vtxlList[n].vertNormals.Count > 0)
                                {
                                    normals += vtxlList[n].vertNormals[w];
                                }
                            }
                        }
                    }
                    meshCheckArr[m][v] = true;

                    //UNRM groups are only valid if there's more than 1, ie more than one vertex linked by position.
                    if (meshNum.Count > 1)
                    {
                        unrm.vertGroupCountCount++;
                        unrm.vertCount += meshNum.Count;
                        unrm.unrmVertGroups.Add(meshNum.Count);
                        unrm.unrmMeshIds.Add(meshNum);
                        unrm.unrmVertIds.Add(vertId);
                        if (applyNormalAveraging)
                        {
                            normals = Vector3.Normalize(normals);
                            for (int i = 0; i < meshNum.Count; i++)
                            {
                                vtxlList[meshNum[i]].vertNormals[vertId[i]] = normals;
                            }
                        }
                    }
                }


            }

            //Only actually apply them if we choose to. This function may just be used for averaging normals.
            if (useUNRMs)
            {
                unrms = unrm;
            }
        }

        public void VTXLFromFaceVerts()
        {
            vtxlList = new List<VTXL>();

            for (int mesh = 0; mesh < tempTris.Count; mesh++)
            {
                //Set up a new VTXL based on an existing sample in order to figure optimize a bit for later.
                //For the sake of simplicity, we assume that vertex IDs for this start from 0 and end at the vertex count - 1. 
                VTXL vtxl = new VTXL(tempTris[mesh].vertCount, tempTris[mesh].faceVerts[0]);
                List<bool> vtxlCheck = new List<bool>(new bool[tempTris[mesh].vertCount]);

                //Set up classic bone palette
                for (int b = 0; b < tempTris[mesh].bonePalette.Count; b++)
                {
                    vtxl.bonePalette.Add((ushort)tempTris[mesh].bonePalette[b]);
                }

                //Go through the faces, set vertices in at their index unless they're a duplicate index with different data. 
                for (int face = 0; face < tempTris[mesh].triList.Count; face++)
                {
                    for (int faceVert = 0; faceVert < 3; faceVert++)
                    {
                        int vertIndex = tempTris[mesh].faceVerts[face].rawVertId[faceVert];

                        //Handle if for whatever reason we have more vertices than expected
                        if (vertIndex > (vtxlCheck.Count - 1))
                        {
                            vtxlCheck.AddRange(new bool[vertIndex - (vtxlCheck.Count - 1)]);
                        }

                        if (vertIndex > (vtxl.vertPositions.Count - 1))
                        {
                            vtxl.InsertBlankVerts(vertIndex - (vtxl.vertPositions.Count - 1), vtxl);
                        }

                        if (vtxlCheck[vertIndex] == true && !VTXL.IsSameVertex(vtxl, vertIndex, tempTris[mesh].faceVerts[face], faceVert))
                        {
                            //If this really needs to be split to a new vertex, add it to the end of the new VTXL list
                            VTXL.AppendVertex(tempTris[mesh].faceVerts[face], vtxl, faceVert);

                            var tri = tempTris[mesh].triList[face];
                            switch (faceVert)
                            {
                                case 0:
                                    tri.X = vtxl.vertPositions.Count - 1;
                                    break;
                                case 1:
                                    tri.Y = vtxl.vertPositions.Count - 1;
                                    break;
                                case 2:
                                    tri.Z = vtxl.vertPositions.Count - 1;
                                    break;
                            }
                            tempTris[mesh].triList[face] = tri;
                        }
                        else if (vtxlCheck[vertIndex] == false)
                        {
                            VTXL.CopyVertex(tempTris[mesh].faceVerts[face], vtxl, faceVert, vertIndex);
                            vtxlCheck[vertIndex] = true;
                        }
                    }

                }

                //Loop through and check for missing vertices/isolated vertices. Proceed to dummy these out as a failsafe for later access.
                for (int i = 0; i < vtxl.vertPositions.Count; i++)
                {
                    /*
                    if (vtxl.vertNormals.Count > 0)
                    {
                        vtxl.vertNormals[i] = new Vector3();
                    }
                    */
                    if (vtxl.vertNormalsNGS.Count > 0 && vtxl.vertNormalsNGS[i] == null)
                    {
                        vtxl.vertNormalsNGS[i] = new short[4];
                    }
                    if (vtxl.vertColors.Count > 0 && vtxl.vertColors[i] == null)
                    {
                        vtxl.vertColors[i] = new byte[4];
                    }
                    if (vtxl.vertColor2s.Count > 0 && vtxl.vertColor2s[i] == null)
                    {
                        vtxl.vertColor2s[i] = new byte[4];
                    }
                    /*
                    if (vtxl.uv1List.Count > 0)
                    {
                        vtxl.uv1List[i] = new Vector2();
                    }*/
                    if (vtxl.uv1ListNGS.Count > 0 && vtxl.uv1ListNGS[i] == null)
                    {
                        vtxl.uv1ListNGS[i] = new short[2];
                    }
                    if (vtxl.uv2ListNGS.Count > 0 && vtxl.uv2ListNGS[i] == null)
                    {
                        vtxl.uv2ListNGS[i] = new short[2];
                    }
                    if (vtxl.uv3ListNGS.Count > 0 && vtxl.uv3ListNGS[i] == null)
                    {
                        vtxl.uv3ListNGS[i] = new short[2];
                    }
                    if (vtxl.uv4ListNGS.Count > 0 && vtxl.uv4ListNGS[i] == null)
                    {
                        vtxl.uv4ListNGS[i] = new short[2];
                    }
                    /*
                    if (vtxl.uv2List.Count > 0)
                    {
                        vtxl.uv2List[i] = new Vector2();
                    }
                    if (vtxl.uv3List.Count > 0)
                    {
                        vtxl.uv3List[i] = new Vector2();
                    }
                    if (vtxl.uv4List.Count > 0)
                    {
                        vtxl.uv4List[i] = new Vector2();
                    }
                    */
                    if (vtxl.vert0x22.Count > 0 && vtxl.vert0x22[i] == null)
                    {
                        vtxl.vert0x22[i] = new short[2];
                    }
                    if (vtxl.vert0x23.Count > 0 && vtxl.vert0x23[i] == null)
                    {
                        vtxl.vert0x23[i] = new short[2];
                    }
                    if (vtxl.vert0x24.Count > 0 && vtxl.vert0x24[i] == null)
                    {
                        vtxl.vert0x24[i] = new short[2];
                    }
                    if (vtxl.vert0x25.Count > 0 && vtxl.vert0x25[i] == null)
                    {
                        vtxl.vert0x25[i] = new short[2];
                    }
                    if (vtxl.rawVertWeights.Count > 0 && vtxl.rawVertWeights[i] == null)
                    {
                        vtxl.rawVertWeights[i] = new List<float>();
                    }
                    if (vtxl.rawVertWeightIds.Count > 0 && vtxl.rawVertWeightIds[i] == null)
                    {
                        vtxl.rawVertWeightIds[i] = new List<int>();
                    }
                }

                vtxlList.Add(vtxl);
            }
        }

        public void ProcessToPSO2Weights()
        {
            foreach(var vtxl in vtxlList)
            {
                vtxl.ProcessToPSO2Weights();
            }
        }

        public AquaObject Clone()
        {
            AquaObject aqp = new AquaObject();
            aqp.objc = objc;
            aqp.vsetList = new List<VSET>(vsetList);
            aqp.vtxeList = vtxeList.ConvertAll(vtxe => vtxe.Clone()).ToList();
            aqp.vtxlList = vtxlList.ConvertAll(vtxl => vtxl.Clone()).ToList();
            aqp.psetList = new List<PSET>(psetList);
            aqp.meshList = new List<MESH>(meshList);
            aqp.mateList = new List<MATE>(mateList);
            aqp.rendList = new List<REND>(rendList);
            aqp.shadList = shadList.ConvertAll(shad => shad.Clone()).ToList();
            aqp.tstaList = new List<TSTA>(tstaList);
            aqp.tsetList = tsetList.ConvertAll(tset => tset.Clone()).ToList();
            aqp.texfList = new List<TEXF>(texfList);
            if (aqp.unrms != null)
            {
                aqp.unrms = unrms.Clone();
            }
            aqp.strips = strips.ConvertAll(stp => stp.Clone()).ToList();

            //*** 0xC33 only
            aqp.bonePalette = new List<uint>(bonePalette);

            //Unclear the purpose of these, but when present they have a smaller count than initial mesh and psets. 
            aqp.unkStruct1List = new List<UnkStruct1>(unkStruct1List);
            aqp.mesh2List = new List<MESH>(mesh2List);
            aqp.pset2List = new List<PSET>(pset2List);
            aqp.strips2 = strips2.ConvertAll(stp => stp.Clone()).ToList();

            aqp.strips3Lengths = new List<int>(strips3Lengths);
            aqp.strips3 = strips3.ConvertAll(stp => stp.Clone()).ToList();
            aqp.unkPointArray1 = new List<Vector3>(unkPointArray1); //Noooooooo idea what these are. Count matches the strips3Lengths count
            aqp.unkPointArray2 = new List<Vector3>(unkPointArray2);
            //***

            aqp.applyNormalAveraging = applyNormalAveraging;

            //Custom model related data
            aqp.tempTris = tempTris.ConvertAll(tri => tri.Clone()).ToList();
            aqp.tempMats = tempMats.ConvertAll(mat => mat.Clone()).ToList();
            aqp.texFUnicodeNames = texFUnicodeNames.ConvertAll(texf => $"{texf}").ToList();
            aqp.matUnicodeNames = texFUnicodeNames.ConvertAll(mat => $"{mat}").ToList();

            return aqp;
        }

    }
}
