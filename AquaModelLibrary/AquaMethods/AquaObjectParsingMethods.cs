using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using System.Windows;
using static AquaModelLibrary.AquaMethods.AquaGeneralMethods;
using static AquaModelLibrary.AquaObjectMethods;
using static AquaModelLibrary.Utility.AquaUtilData;
using static AquaModelLibrary.VTBFMethods;

namespace AquaModelLibrary
{
    public static class AquaObjectParsingMethods
    {
        public static List<AquaObject> ReadAQOModel(BufferedStreamReader streamReader)
        {
            List<AquaObject> aquaObjects = new List<AquaObject>();
            List<TPNTexturePattern> tpns = new List<TPNTexturePattern>();
            ReadAQOModel(streamReader, new ModelSet(), ref aquaObjects, ref tpns);

            return aquaObjects;
        }

        public static bool ReadAQOModel(BufferedStreamReader streamReader, ModelSet set, ref List<AquaObject> models, ref List<TPNTexturePattern> tpns)
        {
            string type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
            int offset = 0x20; //Base offset due to NIFL header
            bool success = false;

            //Deal with deicer's extra header nonsense
            if (type.Equals("aqp\0") || type.Equals("trp\0"))
            {
                streamReader.Seek(0xC, SeekOrigin.Begin);
                //Basically always 0x60, but some deicer files from the Alpha have 0x50... 
                int headJunkSize = streamReader.Read<int>();

                streamReader.Seek(headJunkSize - 0x10, SeekOrigin.Current);
                type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
                offset += headJunkSize;
            }

            //Deal with afp header or aqo. prefixing as needed
            if (type.Equals("afp\0"))
            {
                set.afp = streamReader.Read<AquaPackage.AFPMain>();
                type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
                offset += 0x40;
            }
            else if (type.Equals("aqo\0") || type.Equals("tro\0"))
            {
                streamReader.Seek(0x4, SeekOrigin.Current);
                type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
                offset += 0x4;
            }

            if (set.afp.fileCount == 0)
            {
                set.afp.fileCount = 1;
            }

            //Proceed based on file variant
            if (type.Equals("NIFL"))
            {
                models = ReadNIFLModel(streamReader, set.afp.fileCount, offset, out tpns);
                success = true;
            }
            else if (type.Equals("VTBF"))
            {
                models = ReadVTBFModel(streamReader, set.afp.fileCount, set.afp.afpBase.paddingOffset, out tpns);
                success = true;
            }

            return success;
        }

        public static List<AquaObject> ReadNIFLModel(BufferedStreamReader streamReader, int fileCount, int offset, out List<TPNTexturePattern> tpnList)
        {
            tpnList = new List<TPNTexturePattern>();
            List<AquaObject> aquaModels = new List<AquaObject>();
            for (int modelIndex = 0; modelIndex < fileCount; modelIndex++)
            {
                AquaPackage.AFPBase afp = new AquaPackage.AFPBase();
                if (modelIndex > 0)
                {
                    streamReader.Seek(0x10, SeekOrigin.Current);
                    afp = streamReader.Read<AquaPackage.AFPBase>();
                    offset = (int)streamReader.Position() + 0x20;
                }

                string tpnCheck = Encoding.UTF8.GetString(BitConverter.GetBytes(afp.fileTypeCString));
                if (tpnCheck == "tpn\0")
                {
                    TPNTexturePattern tpn = new TPNTexturePattern();
                    tpn.header = streamReader.Read<TPNTexturePattern.tpnHeader>();
                    for (int i = 0; i < tpn.header.count; i++)
                    {
                        tpn.texSets.Add(streamReader.Read<TPNTexturePattern.texSet>());
                    }
                    tpnList.Add(tpn);
                }
                else
                {
                    int objcVersion = BitConverter.ToInt32(streamReader.ReadBytes(streamReader.Position() + 0x30, 0x4), 0);

                    if (objcVersion >= 0xC32)
                    {
                        ReadNGSNIFLModel(streamReader, new NGSAquaObject(), offset, aquaModels);
                    }
                    else
                    {
                        ReadClassicNIFLModel(streamReader, new ClassicAquaObject(), offset, aquaModels);
                    }

                }
            }
            return aquaModels;
        }

        private static void ReadNGSNIFLModel(BufferedStreamReader streamReader, NGSAquaObject model, int offset, List<AquaObject> aquaModels)
        {
            List<List<ushort>> edgeVertsTemp = new List<List<ushort>>();

            model.nifl = streamReader.Read<AquaCommon.NIFL>();
            model.rel0 = streamReader.Read<AquaCommon.REL0>();
            model.objc = AquaObject.ReadOBJC(streamReader);

            //Read Bone Palette
            if (model.objc.bonePaletteOffset > 0)
            {
                streamReader.Seek(model.objc.bonePaletteOffset + offset, SeekOrigin.Begin);
                int boneCount = streamReader.Read<int>();
                streamReader.Seek(streamReader.Read<int>() + offset, SeekOrigin.Begin); //Should start literally right after this anyways, but in case it changes or w/e
                for (int boneIndex = 0; boneIndex < boneCount; boneIndex++)
                {
                    model.bonePalette.Add(streamReader.Read<uint>());
                }
            }

            if (model.objc.vsetOffset > 0)
            {
                streamReader.Seek(model.objc.vsetOffset + offset, SeekOrigin.Begin);
                //Read VSETs
                for (int vsetIndex = 0; vsetIndex < model.objc.vsetCount; vsetIndex++)
                {
                    model.vsetList.Add(streamReader.Read<AquaObject.VSET>());

                    //Get edge verts if needed. Bone palette is linked elsewhere in 0xC32+ Aqua Objects.
                    List<ushort> edgeVerts = new List<ushort>();
                    if (model.vsetList[vsetIndex].edgeVertsCount > 0)
                    {
                        long bookmark = streamReader.Position();
                        streamReader.Seek(model.vsetList[vsetIndex].edgeVertsOffset + offset, SeekOrigin.Begin);
                        for (int edge = 0; edge < model.vsetList[vsetIndex].edgeVertsCount; edge++)
                        {
                            edgeVerts.Add(streamReader.Read<ushort>());
                        }
                        streamReader.Seek(bookmark, SeekOrigin.Begin);
                    }
                    edgeVertsTemp.Add(edgeVerts);
                }
            }

            if (model.objc.vtxeOffset > 0)
            {
                //Create master VTXE dictionary to record which data types are used in this particular model. 
                //Create dictionaries per group to track which particular ones each has.
                Dictionary<int, bool> vtxeCheck = new Dictionary<int, bool>();
                List<Dictionary<int, bool>> vtxeGroupCheck = new List<Dictionary<int, bool>>();

                //Set up group dictionary
                for (int group = 0; group < model.objc.vtxeCount; group++)
                {
                    vtxeGroupCheck.Add(new Dictionary<int, bool>());
                }
                foreach (int entry in Enum.GetValues(typeof(NGSAquaObject.NGSVertFlags)))
                {
                    vtxeCheck.Add(entry, false);
                    for (int group = 0; group < vtxeGroupCheck.Count; group++)
                    {
                        vtxeGroupCheck[group].Add(entry, false);
                    }
                }

                //Read VTXEs

                streamReader.Seek(model.objc.vtxeOffset + offset, SeekOrigin.Begin);
                for (int vtxeIndex = 0; vtxeIndex < model.objc.vtxeCount; vtxeIndex++)
                {
                    AquaObject.VTXE vtxeSet = new AquaObject.VTXE();
                    vtxeSet.vertDataTypes = new List<AquaObject.VTXEElement>();

                    int vtxeSubCount = streamReader.Read<int>();
                    long bookmark = streamReader.Position() + 4;
                    streamReader.Seek(streamReader.Read<int>() + offset, SeekOrigin.Begin);

                    for (int vtxeEleIndex = 0; vtxeEleIndex < vtxeSubCount; vtxeEleIndex++)
                    {
                        AquaObject.VTXEElement vtxeEle = streamReader.Read<AquaObject.VTXEElement>();
                        vtxeSet.vertDataTypes.Add(vtxeEle);
                        try
                        {
                            vtxeCheck[vtxeEle.dataType] = true;
                            vtxeGroupCheck[vtxeIndex][vtxeEle.dataType] = true;
                        }
                        catch
                        {
                            MessageBox.Show($"Crashed on unknown vert data type: {vtxeSet.vertDataTypes[vtxeEleIndex].dataType} \n Please report!");
                            throw new Exception();
                        }
                    }
                    model.vtxeList.Add(vtxeSet);
                    streamReader.Seek(bookmark, SeekOrigin.Begin);
                }


                //Read VTXL
                if (model.objc.vtxlStartOffset > 0)
                {
                    streamReader.Seek(model.objc.vtxlStartOffset + offset, SeekOrigin.Begin);

                    //0xC32+ Aqua Objects use a global vertex array. To separate it into something more normally usable, we need to loop through the VSETs
                    //To accurately dump all model parts, materials and all without having isolated vertices, we'll want to split this again later when we have face data.
                    for (int vset = 0; vset < model.vsetList.Count; vset++)
                    {
                        AquaObject.VTXL vtxl = new AquaObject.VTXL();
                        ReadVTXL(streamReader, model.vtxeList[model.vsetList[vset].vtxeCount], vtxl, model.vsetList[vset].vtxlCount,
                            model.vtxeList[model.vsetList[vset].vtxeCount].vertDataTypes.Count, model.objc.largetsVtxl);
                        vtxl.edgeVerts = edgeVertsTemp[vset];
                        model.vtxlList.Add(vtxl);
                    }
                }
            }

            //Read PSET
            if (model.objc.psetOffset > 0)
            {
                streamReader.Seek(model.objc.psetOffset + offset, SeekOrigin.Begin);
                for (int psetIndex = 0; psetIndex < model.objc.psetCount; psetIndex++)
                {
                    model.psetList.Add(streamReader.Read<AquaObject.PSET>());
                }

                //Read faces
                for (int psetIndex = 0; psetIndex < model.objc.psetCount; psetIndex++)
                {
                    streamReader.Seek(model.psetList[psetIndex].faceCountOffset + offset, SeekOrigin.Begin);
                    AquaObject.stripData stripData = new AquaObject.stripData();
                    stripData.triIdCount = model.psetList[psetIndex].psetFaceCount;

                    //Read face groups. For most models, this will be 1
                    for (int i = 0; i < model.psetList[psetIndex].faceGroupCount; i++)
                    {
                        stripData.faceGroups.Add(streamReader.Read<int>());
                    }

                    streamReader.Seek(model.objc.globalStripOffset + (model.psetList[psetIndex].stripStartCount * 2) + offset, SeekOrigin.Begin);
                    //Read strip vert indices
                    for (int triId = 0; triId < model.psetList[psetIndex].psetFaceCount; triId++)
                    {
                        stripData.triStrips.Add(streamReader.Read<ushort>());
                    }
                    stripData.format0xC33 = true;

                    model.strips.Add(stripData);
                }
            }

            //Read MESH
            if (model.objc.meshOffset > 0)
            {
                streamReader.Seek(model.objc.meshOffset + offset, SeekOrigin.Begin);
                for (int meshIndex = 0; meshIndex < model.objc.meshCount; meshIndex++)
                {
                    model.meshList.Add(streamReader.Read<AquaObject.MESH>());
                }
            }

            //Read MATE
            if (model.objc.mateOffset > 0)
            {
                streamReader.Seek(model.objc.mateOffset + offset, SeekOrigin.Begin);
                for (int mateIndex = 0; mateIndex < model.objc.mateCount; mateIndex++)
                {
                    model.mateList.Add(streamReader.Read<AquaObject.MATE>());
                }
            }

            //Read REND
            if (model.objc.rendOffset > 0)
            {
                streamReader.Seek(model.objc.rendOffset + offset, SeekOrigin.Begin);
                for (int rendIndex = 0; rendIndex < model.objc.rendCount; rendIndex++)
                {
                    model.rendList.Add(streamReader.Read<AquaObject.REND>());
                }
            }

            //Read SHAD
            if (model.objc.shadOffset > 0)
            {
                streamReader.Seek(model.objc.shadOffset + offset, SeekOrigin.Begin);
                for (int shadIndex = 0; shadIndex < model.objc.shadCount; shadIndex++)
                {
                    model.shadList.Add(NGSAquaObject.ReadNGSSHAD(streamReader, offset, true));
                }
            }

            //Read TSTA
            if (model.objc.tstaOffset > 0)
            {
                streamReader.Seek(model.objc.tstaOffset + offset, SeekOrigin.Begin);
                for (int tstaIndex = 0; tstaIndex < model.objc.tstaCount; tstaIndex++)
                {
                    model.tstaList.Add(streamReader.Read<AquaObject.TSTA>());
                }
            }

            //Read TSET
            if (model.objc.tsetOffset > 0)
            {
                streamReader.Seek(model.objc.tsetOffset + offset, SeekOrigin.Begin);
                for (int tsetIndex = 0; tsetIndex < model.objc.tsetCount; tsetIndex++)
                {
                    model.tsetList.Add(AquaObject.ReadTSET(streamReader));
                }
            }

            //Read TEXF
            if (model.objc.texfOffset > 0)
            {
                streamReader.Seek(model.objc.texfOffset + offset, SeekOrigin.Begin);
                for (int texfIndex = 0; texfIndex < model.objc.texfCount; texfIndex++)
                {
                    model.texfList.Add(streamReader.Read<AquaObject.TEXF>());
                }
            }

            //Read UNRM
            if (model.objc.unrmOffset > 0)
            {
                streamReader.Seek(model.objc.unrmOffset + offset, SeekOrigin.Begin);
                model.unrms = new AquaObject.UNRM();
                model.unrms.vertGroupCountCount = streamReader.Read<int>();
                model.unrms.vertGroupCountOffset = streamReader.Read<int>();
                model.unrms.vertCount = streamReader.Read<int>();
                model.unrms.meshIdOffset = streamReader.Read<int>();
                model.unrms.vertIDOffset = streamReader.Read<int>();
                model.unrms.padding0 = streamReader.Read<double>();
                model.unrms.padding1 = streamReader.Read<int>();
                model.unrms.unrmVertGroups = new List<int>();
                model.unrms.unrmMeshIds = new List<List<int>>();
                model.unrms.unrmVertIds = new List<List<int>>();

                //GroupCounts
                for (int vertId = 0; vertId < model.unrms.vertGroupCountCount; vertId++)
                {
                    model.unrms.unrmVertGroups.Add(streamReader.Read<int>());
                }
                AlignReader(streamReader, 0x10);

                //Mesh IDs
                for (int vertGroup = 0; vertGroup < model.unrms.vertGroupCountCount; vertGroup++)
                {
                    List<int> vertGroupMeshList = new List<int>();

                    for (int i = 0; i < model.unrms.unrmVertGroups[vertGroup]; i++)
                    {
                        vertGroupMeshList.Add(streamReader.Read<int>());
                    }

                    model.unrms.unrmMeshIds.Add(vertGroupMeshList);
                }
                AlignReader(streamReader, 0x10);

                //Vert IDs
                for (int vertGroup = 0; vertGroup < model.unrms.vertGroupCountCount; vertGroup++)
                {
                    List<int> vertGroupVertList = new List<int>();

                    for (int i = 0; i < model.unrms.unrmVertGroups[vertGroup]; i++)
                    {
                        vertGroupVertList.Add(streamReader.Read<int>());
                    }

                    model.unrms.unrmVertIds.Add(vertGroupVertList);
                }
                AlignReader(streamReader, 0x10);
            }

            //Read PSET2
            if (model.objc.pset2Offset > 0)
            {
                streamReader.Seek(model.objc.pset2Offset + offset, SeekOrigin.Begin);
                for (int psetIndex = 0; psetIndex < model.objc.pset2Count; psetIndex++)
                {
                    model.pset2List.Add(streamReader.Read<AquaObject.PSET>());
                }

                //Read faces
                for (int psetIndex = 0; psetIndex < model.objc.pset2Count; psetIndex++)
                {
                    streamReader.Seek(model.pset2List[psetIndex].faceCountOffset + offset, SeekOrigin.Begin);
                    AquaObject.stripData stripData = new AquaObject.stripData();
                    stripData.triIdCount = model.pset2List[psetIndex].psetFaceCount;

                    //Read face groups. For most models, this will be 1
                    for (int i = 0; i < model.pset2List[psetIndex].faceGroupCount; i++)
                    {
                        stripData.faceGroups.Add(streamReader.Read<int>());
                    }

                    streamReader.Seek(model.objc.globalStripOffset + (model.pset2List[psetIndex].stripStartCount * 2) + offset, SeekOrigin.Begin);
                    //Read strip vert indices
                    for (int triId = 0; triId < model.pset2List[psetIndex].psetFaceCount; triId++)
                    {
                        stripData.triStrips.Add(streamReader.Read<ushort>());
                    }
                    stripData.format0xC33 = true;

                    model.strips2.Add(stripData);
                }
            }

            //Read MESH2
            if (model.objc.mesh2Offset > 0)
            {
                streamReader.Seek(model.objc.mesh2Offset + offset, SeekOrigin.Begin);
                for (int meshIndex = 0; meshIndex < model.objc.mesh2Count; meshIndex++)
                {
                    model.mesh2List.Add(streamReader.Read<AquaObject.MESH>());
                }
            }

            // Weird trp nonsense from here
            if (model.objc.unkStruct1Count > 0)
            {
                streamReader.Seek(model.objc.unkStruct1Offset + offset, SeekOrigin.Begin);
                for (int strIndex = 0; strIndex < model.objc.unkStruct1Count; strIndex++)
                {
                    model.unkStruct1List.Add(streamReader.Read<NGSAquaObject.unkStruct1>());
                }
            }

            //Get 3rd strip set. This actually has its own separate strip array... for some reason. The count seems to always be 1 regardless, but doesn't always have anything at the offset. Quite odd.
            if (model.objc.globalStrip3LengthCount > 0 && model.objc.globalStrip3LengthOffset != 0 && model.objc.globalStrip3Offset != 0)
            {
                //Get the lengths
                streamReader.Seek(model.objc.globalStrip3LengthOffset + offset, SeekOrigin.Begin);
                for (int id = 0; id < model.objc.globalStrip3LengthCount; id++)
                {
                    model.strips3Lengths.Add(streamReader.Read<int>());
                }

                streamReader.Seek(model.objc.globalStrip3Offset + offset, SeekOrigin.Begin);
                //Read strip vert indices
                for (int id = 0; id < model.strips3Lengths.Count; id++)
                {
                    AquaObject.stripData stripData = new AquaObject.stripData();
                    stripData.format0xC33 = true;
                    stripData.triIdCount = model.strips3Lengths[id];

                    //These can potentially be 0 sometimes
                    for (int triId = 0; triId < model.strips3Lengths[id]; triId++)
                    {
                        stripData.triStrips.Add(streamReader.Read<ushort>());
                    }

                    model.strips3.Add(stripData);
                }
            }

            if (model.objc.unkPointArray1Offset != 0)
            {
                if (model.objc.unkPointArray2Offset == 0)
                {
                    Console.WriteLine("unkPointArray2 was null. Cannot reliably calculate size");
                    throw new Exception();
                }
                int point1Count = (model.objc.unkPointArray2Offset - model.objc.unkPointArray1Offset) / 0xC;

                streamReader.Seek(model.objc.unkPointArray1Offset + offset, SeekOrigin.Begin);
                for (int i = 0; i < point1Count; i++)
                {
                    model.unkPointArray1.Add(streamReader.Read<Vector3>());
                }
            }

            if (model.objc.unkPointArray2Offset != 0)
            {
                if (model.objc.unkPointArray1Offset == 0)
                {
                    Console.WriteLine("unkPointArray1 was null. Cannot reliably calculate size");
                    throw new Exception();
                }
                int point2Count = (model.objc.unkPointArray2Offset - model.objc.unkPointArray1Offset) / 0xC;

                streamReader.Seek(model.objc.unkPointArray2Offset + offset, SeekOrigin.Begin);
                for (int i = 0; i < point2Count; i++)
                {
                    model.unkPointArray2.Add(streamReader.Read<Vector3>());
                }
            }

            //Read NOF0
            streamReader.Seek(model.rel0.REL0Size + 0x8 + offset, SeekOrigin.Begin);
            model.nof0 = AquaCommon.readNOF0(streamReader);
            AlignReader(streamReader, 0x10);

            //Read NEND
            model.nend = streamReader.Read<AquaCommon.NEND>();
            aquaModels.Add(model);
        }

        private static void ReadClassicNIFLModel(BufferedStreamReader streamReader, ClassicAquaObject model, int offset, List<AquaObject> aquaModels)
        {
            model.nifl = streamReader.Read<AquaCommon.NIFL>();
            model.rel0 = streamReader.Read<AquaCommon.REL0>();
            model.objc = AquaObject.ReadOBJC(streamReader);

            //Read VSETs
            if (model.objc.vsetOffset > 0)
            {
                streamReader.Seek(model.objc.vsetOffset + offset, SeekOrigin.Begin);

                for (int vsetIndex = 0; vsetIndex < model.objc.vsetCount; vsetIndex++)
                {
                    model.vsetList.Add(streamReader.Read<AquaObject.VSET>());
                }
                //Read VTXE+VTXL+BonePalette+MeshEdgeVerts
                for (int vsetIndex = 0; vsetIndex < model.objc.vsetCount; vsetIndex++)
                {
                    streamReader.Seek(model.vsetList[vsetIndex].vtxeOffset + offset, SeekOrigin.Begin);
                    //VTXE
                    AquaObject.VTXE vtxeSet = new AquaObject.VTXE();
                    vtxeSet.vertDataTypes = new List<AquaObject.VTXEElement>();
                    for (int vtxeIndex = 0; vtxeIndex < model.vsetList[vsetIndex].vtxeCount; vtxeIndex++)
                    {
                        vtxeSet.vertDataTypes.Add(streamReader.Read<AquaObject.VTXEElement>());
                    }
                    model.vtxeList.Add(vtxeSet);

                    streamReader.Seek(model.vsetList[vsetIndex].vtxlOffset + offset, SeekOrigin.Begin);
                    //VTXL
                    AquaObject.VTXL vtxl = new AquaObject.VTXL();
                    ReadVTXL(streamReader, vtxeSet, vtxl, model.vsetList[vsetIndex].vtxlCount, vtxeSet.vertDataTypes.Count);

                    AlignReader(streamReader, 0x10);

                    //Bone Palette
                    if (model.vsetList[vsetIndex].bonePaletteCount > 0)
                    {
                        streamReader.Seek(model.vsetList[vsetIndex].bonePaletteOffset + offset, SeekOrigin.Begin);
                        for (int boneId = 0; boneId < model.vsetList[vsetIndex].bonePaletteCount; boneId++)
                        {
                            vtxl.bonePalette.Add(streamReader.Read<ushort>());
                        }
                        AlignReader(streamReader, 0x10);
                    }

                    //Edge Verts
                    if (model.vsetList[vsetIndex].edgeVertsCount > 0)
                    {
                        streamReader.Seek(model.vsetList[vsetIndex].edgeVertsOffset + offset, SeekOrigin.Begin);
                        for (int boneId = 0; boneId < model.vsetList[vsetIndex].edgeVertsCount; boneId++)
                        {
                            vtxl.edgeVerts.Add(streamReader.Read<ushort>());
                        }
                        AlignReader(streamReader, 0x10);
                    }
                    model.vtxlList.Add(vtxl);
                }
            }

            //PSET
            if (model.objc.psetOffset > 0)
            {
                streamReader.Seek(model.objc.psetOffset + offset, SeekOrigin.Begin);
                for (int psetIndex = 0; psetIndex < model.objc.psetCount; psetIndex++)
                {
                    model.psetList.Add(streamReader.Read<AquaObject.PSET>());
                }

                //Read faces
                for (int psetIndex = 0; psetIndex < model.objc.psetCount; psetIndex++)
                {
                    streamReader.Seek(model.psetList[psetIndex].faceCountOffset + offset, SeekOrigin.Begin);
                    AquaObject.stripData stripData = new AquaObject.stripData();
                    stripData.triIdCount = streamReader.Read<int>();
                    stripData.reserve0 = streamReader.Read<int>();
                    stripData.reserve1 = streamReader.Read<int>();
                    stripData.reserve2 = streamReader.Read<int>();
                    if (model.objc.type == 0xC31 && model.psetList[psetIndex].tag != 0x2100)
                    {
                        stripData.format0xC33 = true;
                    }

                    streamReader.Seek(model.psetList[psetIndex].faceOffset + offset, SeekOrigin.Begin);
                    //Read strip vert indices
                    for (int triId = 0; triId < stripData.triIdCount; triId++)
                    {
                        stripData.triStrips.Add(streamReader.Read<ushort>());
                    }

                    model.strips.Add(stripData);

                    AlignReader(streamReader, 0x10);
                }
            }

            //MESH
            if (model.objc.meshOffset > 0)
            {
                streamReader.Seek(model.objc.meshOffset + offset, SeekOrigin.Begin);
                for (int meshIndex = 0; meshIndex < model.objc.meshCount; meshIndex++)
                {
                    model.meshList.Add(streamReader.Read<AquaObject.MESH>());
                }
            }

            //MATE
            if (model.objc.mateOffset > 0)
            {
                streamReader.Seek(model.objc.mateOffset + offset, SeekOrigin.Begin);
                for (int mateIndex = 0; mateIndex < model.objc.mateCount; mateIndex++)
                {
                    model.mateList.Add(streamReader.Read<AquaObject.MATE>());
                }
            }

            //REND
            if (model.objc.rendOffset > 0)
            {
                streamReader.Seek(model.objc.rendOffset + offset, SeekOrigin.Begin);
                for (int rendIndex = 0; rendIndex < model.objc.rendCount; rendIndex++)
                {
                    model.rendList.Add(streamReader.Read<AquaObject.REND>());
                }
            }

            //SHAD
            if (model.objc.shadOffset > 0)
            {
                streamReader.Seek(model.objc.shadOffset + offset, SeekOrigin.Begin);
                for (int shadIndex = 0; shadIndex < model.objc.shadCount; shadIndex++)
                {
                    model.shadList.Add(AquaObject.ReadSHAD(streamReader));
                }
            }

            //TSTA
            if (model.objc.tstaOffset > 0)
            {
                streamReader.Seek(model.objc.tstaOffset + offset, SeekOrigin.Begin);
                for (int tstaIndex = 0; tstaIndex < model.objc.tstaCount; tstaIndex++)
                {
                    model.tstaList.Add(streamReader.Read<AquaObject.TSTA>());
                }
            }

            //TSET
            if (model.objc.tsetOffset > 0)
            {
                streamReader.Seek(model.objc.tsetOffset + offset, SeekOrigin.Begin);
                for (int tsetIndex = 0; tsetIndex < model.objc.tsetCount; tsetIndex++)
                {
                    model.tsetList.Add(AquaObject.ReadTSET(streamReader));
                }
            }

            //TEXF
            if (model.objc.texfOffset > 0)
            {
                streamReader.Seek(model.objc.texfOffset + offset, SeekOrigin.Begin);
                for (int texfIndex = 0; texfIndex < model.objc.texfCount; texfIndex++)
                {
                    model.texfList.Add(streamReader.Read<AquaObject.TEXF>());
                }
            }

            //UNRM
            if (model.objc.unrmOffset > 0)
            {
                streamReader.Seek(model.objc.unrmOffset + offset, SeekOrigin.Begin);
                model.unrms = new ClassicAquaObject.UNRM();
                model.unrms.vertGroupCountCount = streamReader.Read<int>();
                model.unrms.vertGroupCountOffset = streamReader.Read<int>();
                model.unrms.vertCount = streamReader.Read<int>();
                model.unrms.meshIdOffset = streamReader.Read<int>();
                model.unrms.vertIDOffset = streamReader.Read<int>();
                model.unrms.padding0 = streamReader.Read<double>();
                model.unrms.padding1 = streamReader.Read<int>();
                model.unrms.unrmVertGroups = new List<int>();
                model.unrms.unrmMeshIds = new List<List<int>>();
                model.unrms.unrmVertIds = new List<List<int>>();

                //GroupCounts
                for (int vertId = 0; vertId < model.unrms.vertGroupCountCount; vertId++)
                {
                    model.unrms.unrmVertGroups.Add(streamReader.Read<int>());
                }
                AlignReader(streamReader, 0x10);

                //Mesh IDs
                for (int vertGroup = 0; vertGroup < model.unrms.vertGroupCountCount; vertGroup++)
                {
                    List<int> vertGroupMeshList = new List<int>();

                    for (int i = 0; i < model.unrms.unrmVertGroups[vertGroup]; i++)
                    {
                        vertGroupMeshList.Add(streamReader.Read<int>());
                    }

                    model.unrms.unrmMeshIds.Add(vertGroupMeshList);
                }
                AlignReader(streamReader, 0x10);

                //Vert IDs
                for (int vertGroup = 0; vertGroup < model.unrms.vertGroupCountCount; vertGroup++)
                {
                    List<int> vertGroupVertList = new List<int>();

                    for (int i = 0; i < model.unrms.unrmVertGroups[vertGroup]; i++)
                    {
                        vertGroupVertList.Add(streamReader.Read<int>());
                    }

                    model.unrms.unrmVertIds.Add(vertGroupVertList);
                }
                AlignReader(streamReader, 0x10);
            }

            //NOF0
            streamReader.Seek(model.rel0.REL0Size + 0x8 + offset, SeekOrigin.Begin);
            model.nof0 = AquaCommon.readNOF0(streamReader);
            AlignReader(streamReader, 0x10);

            //NEND
            model.nend = streamReader.Read<AquaCommon.NEND>();

            aquaModels.Add(model);
        }

        public static List<AquaObject> ReadVTBFModel(BufferedStreamReader streamReader, int fileCount, int firstFileSize, out List<TPNTexturePattern> tpnList)
        {
            tpnList = new List<TPNTexturePattern>();
            List<AquaObject> aquaModels = new List<AquaObject>();
            int fileSize = firstFileSize;

            //Handle .aqo/tro
            if (fileSize == 0)
            {
                fileSize = (int)streamReader.BaseStream().Length;

                //Handle the weird aqo/tro with aqo. in front of the rest of the file needlessly
                int type = BitConverter.ToInt32(streamReader.ReadBytes(0, 4), 0);
                if (type.Equals(0x6F7161) || type.Equals(0x6F7274))
                {
                    fileSize -= 0x4;
                }
            }

            for (int modelIndex = 0; modelIndex < fileCount; modelIndex++)
            {
                AquaObject model = new ClassicAquaObject();
                TPNTexturePattern tpn = new TPNTexturePattern();
                int objcCount = 0;
                List<List<ushort>> bp = null;
                List<List<ushort>> ev = null;

                if (modelIndex > 0)
                {
                    //There's 0x10 of padding present following the last model if it ends aligned to 0x10 already. Otherwise, padding to alignment.
                    if (streamReader.Position() % 0x10 == 0)
                    {
                        streamReader.Seek(0x10, SeekOrigin.Current);
                    }
                    else
                    {
                        AlignReader(streamReader, 0x10);
                    }
                    //Decide whether this is a tpn or not
                    var afp = streamReader.Read<AquaPackage.AFPBase>();
                    if (afp.fileTypeCString == 0x6E7074)
                    {
                        tpn.tpnAFPBase = afp;
                    }
                    else if (afp.fileTypeCString == 0x6F7161 || afp.fileTypeCString == 0x6F7274)
                    {
                        ((ClassicAquaObject)model).afp = afp;
                    }
                    else
                    {
                        break;
                    }
                    fileSize = ((ClassicAquaObject)model).afp.paddingOffset;
                }
                int dataEnd = (int)streamReader.Position() + fileSize;

                //TPN files are uncommon, but are sometimes at the end of afp archives. 
                if (tpn.tpnAFPBase.fileTypeCString == 0x6E7074)
                {
                    tpn.header = streamReader.Read<TPNTexturePattern.tpnHeader>();
                    for (int i = 0; i < tpn.header.count; i++)
                    {
                        tpn.texSets.Add(streamReader.Read<TPNTexturePattern.texSet>());
                    }
                    tpnList.Add(tpn);
                }
                else
                {
                    streamReader.Seek(0x10, SeekOrigin.Current); //Skip the header and move to the tags
                    while (streamReader.Position() < dataEnd)
                    {
                        var data = ReadVTBFTag(streamReader, out string tagType, out int ptrCount, out int entryCount);

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
                                ((ClassicAquaObject)model).objc = parseOBJC(data);
                                break;
                            case "VSET":
                                ((ClassicAquaObject)model).vsetList = parseVSET(data, out List<List<ushort>> bonePalettes, out List<List<ushort>> edgeVertsLists);
                                bp = bonePalettes;
                                ev = edgeVertsLists;
                                break;
                            case "VTXE":
                                var data2 = ReadVTBFTag(streamReader, out string tagType2, out int ptrCount2, out int entryCount2);
                                if (tagType2 != "VTXL")
                                {
                                    throw new System.Exception("VTXE without paired VTXL! Please report!");
                                }
                                parseVTXE_VTXL(data, data2, out var vtxe, out var vtxl);
                                ((ClassicAquaObject)model).vtxeList.Add(vtxe);
                                ((ClassicAquaObject)model).vtxlList.Add(vtxl);
                                break;
                            case "PSET":
                                parsePSET(data, out var psetList, out var tris);
                                ((ClassicAquaObject)model).psetList = psetList;
                                ((ClassicAquaObject)model).strips = tris;
                                break;
                            case "MESH":
                                ((ClassicAquaObject)model).meshList = parseMESH(data);
                                break;
                            case "MATE":
                                ((ClassicAquaObject)model).mateList = parseMATE(data);
                                break;
                            case "REND":
                                ((ClassicAquaObject)model).rendList = parseREND(data);
                                break;
                            case "SHAD":
                                ((ClassicAquaObject)model).shadList = parseSHAD(data);
                                break;
                            case "TSTA":
                                ((ClassicAquaObject)model).tstaList = parseTSTA(data);
                                break;
                            case "TSET":
                                ((ClassicAquaObject)model).tsetList = parseTSET(data);
                                break;
                            case "TEXF":
                                ((ClassicAquaObject)model).texfList = parseTEXF(data);
                                break;
                            case "UNRM":
                                ((ClassicAquaObject)model).unrms = parseUNRM(data);
                                break;
                            case "SHAP":
                                //Poor SHAP. It's empty and useless. If it's not, we should probably do something though.
                                if (entryCount > 2)
                                {
                                    throw new System.Exception("SHAP has more than 2 entries! Please report!");
                                }
                                break;
                            default:
                                //Data being null signfies that the last thing read wasn't a proper tag. This should mean the end of the VTBF stream if nothing else.
                                if (firstFileSize == 0)
                                {
                                    aquaModels.Add(model);
                                    return aquaModels;
                                }
                                throw new System.Exception($"Unexpected tag at {streamReader.Position().ToString("X")}! {tagType} Please report!");
                        }
                    }
                    //Assign edgeverts and bone palettes. Assign them here in case of weird ordering shenanigans
                    if (bp != null)
                    {
                        for (int i = 0; i < bp.Count; i++)
                        {
                            ((ClassicAquaObject)model).vtxlList[i].bonePalette = bp[i];
                        }
                    }
                    if (ev != null)
                    {
                        for (int i = 0; i < ev.Count; i++)
                        {
                            ((ClassicAquaObject)model).vtxlList[i].edgeVerts = ev[i];
                        }
                    }
                    aquaModels.Add(model);
                }
            }

            return aquaModels;
        }

    }
}
