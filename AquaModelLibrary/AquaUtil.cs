using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;
using System.Windows;
using static AquaModelLibrary.AquaMiscMethods;
using static AquaModelLibrary.AquaObjectMethods;
using static AquaModelLibrary.CharacterMakingIndexMethods;
using static AquaModelLibrary.VTBFMethods;

namespace AquaModelLibrary
{
    public class AquaUtil
    {
        public string pso2_binDir = null;
        public CharacterMakingIndex aquaCMX = null;
        public PSO2Text aquaText = null;
        public List<TCBTerrainConvex> tcbModels = new List<TCBTerrainConvex>();
        public List<PRMModel> prmModels = new List<PRMModel>();
        public List<ModelSet> aquaModels = new List<ModelSet>();
        public List<TPNTexturePattern> tpnFiles = new List<TPNTexturePattern>();
        public List<AquaNode> aquaBones = new List<AquaNode>();
        public List<AquaEffect> aquaEffect = new List<AquaEffect>();
        public List<AnimSet> aquaMotions = new List<AnimSet>();
        public List<SetLayout> aquaSets = new List<SetLayout>();
        public List<AquaBTI_MotionConfig> aquaMotionConfigs = new List<AquaBTI_MotionConfig>();

        public class ModelSet
        {
            public AquaPackage.AFPMain afp = new AquaPackage.AFPMain();
            public List<AquaObject> models = new List<AquaObject>();
        }

        public class AnimSet
        {
            public AquaPackage.AFPMain afp = new AquaPackage.AFPMain();
            public List<AquaMotion> anims = new List<AquaMotion>();
        }

        public void ReadModel(string inFilename)
        {
            using (Stream stream = (Stream)new FileStream(inFilename, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                ModelSet set = new ModelSet();
                string type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
                int offset = 0x20; //Base offset due to NIFL header

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
                } else if (type.Equals("aqo\0") || type.Equals("tro\0"))
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
                    set.models = ReadNIFLModel(streamReader, set.afp.fileCount, offset);
                    aquaModels.Add(set);
                } else if (type.Equals("VTBF"))
                {
                    set.models = ReadVTBFModel(streamReader, set.afp.fileCount, set.afp.afpBase.paddingOffset);
                    aquaModels.Add(set);
                } else
                {
                    MessageBox.Show("Improper File Format!");
                }
            }
        }

        public List<AquaObject> ReadNIFLModel(BufferedStreamReader streamReader, int fileCount, int offset)
        {
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
                }
                else
                {
                    int magic = BitConverter.ToInt32(streamReader.ReadBytes(streamReader.Position() + 0x30, 0x4), 0);

                    switch (magic)
                    {
                        case 0xC32:
                        case 0xC33:
                            ReadNGSNIFLModel(streamReader, new NGSAquaObject(), offset, aquaModels);
                            break;
                        default:
                            ReadClassicNIFLModel(streamReader, new ClassicAquaObject(), offset, aquaModels);
                            break;
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

            if (model.objc.vsetOffset > 0)
            {
                streamReader.Seek(model.objc.vsetOffset + offset, SeekOrigin.Begin);
                //Read VSETs
                for (int vsetIndex = 0; vsetIndex < model.objc.vsetCount; vsetIndex++)
                {
                    model.vsetList.Add(streamReader.Read<AquaObject.VSET>());

                    //Get edge verts if needed. Bone palette is linked elsewhere in 0xC33 Aqua Objects.
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

                    //0xC33 Aqua Objects use a global vertex array. To separate it into something more normally usable, we need to loop through the VSETs
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
                    model.shadList.Add(NGSAquaObject.ReadNGSSHAD(streamReader, offset));
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
                for(int id = 0; id < model.objc.globalStrip3LengthCount; id++)
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
            
            if(model.objc.unkPointArray1Offset != 0)
            {
                if(model.objc.unkPointArray2Offset == 0)
                {
                    Console.WriteLine("unkPointArray2 was null. Cannot reliably calculate size");
                    throw new Exception();
                }
                int point1Count = (model.objc.unkPointArray2Offset - model.objc.unkPointArray1Offset) / 0xC;

                streamReader.Seek(model.objc.unkPointArray1Offset + offset, SeekOrigin.Begin);
                for(int i = 0; i < point1Count; i++)
                {
                    model.unkPointArray1.Add(streamReader.Read<Vector3>());
                }
            }

            if(model.objc.unkPointArray2Offset != 0)
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

        public List<AquaObject> ReadVTBFModel(BufferedStreamReader streamReader, int fileCount, int firstFileSize)
        {
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
                    } else
                    {
                        AlignReader(streamReader, 0x10);
                    }
                    //Decide whether this is a tpn or not
                    var afp = streamReader.Read<AquaPackage.AFPBase>();
                    if (afp.fileTypeCString == 0x6E7074)
                    {
                        tpn.tpnAFPBase = afp;
                    } else if (afp.fileTypeCString == 0x6F7161 || afp.fileTypeCString == 0x6F7274)
                    {
                        ((ClassicAquaObject)model).afp = afp;
                    } else
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
                    tpnFiles.Add(tpn);
                } else
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

        //Temp material, vtxlList data or tempTri vertex data, and temptris are expected to be populated prior to this process. This should ALWAYS be run before any write attempts.
        public void ConvertToNGSPSO2Mesh(bool useUnrms, bool useFaceNormals, bool baHack, bool useBiTangent, bool zeroBounds, bool useRigid, bool splitVerts = true)
        {
            for (int msI = 0; msI < aquaModels.Count; msI++)
            {
                for (int aqI = 0; aqI < aquaModels[msI].models.Count; aqI++)
                {
                    int totalStripsShorts = 0;
                    int totalVerts = 0;
                    AquaObject matModelSplit = new NGSAquaObject();
                    AquaObject outModel = new NGSAquaObject();

                    //Assemble vtxlList
                    if (aquaModels[msI].models[aqI].vtxlList == null || aquaModels[msI].models[aqI].vtxlList.Count == 0)
                    {
                        VTXLFromFaceVerts(aquaModels[msI].models[aqI]);
                    }
                    //Fix weights
                    if(useRigid == false)
                    {
                        foreach (var vtxl in aquaModels[msI].models[aqI].vtxlList)
                        {
                            vtxl.processToPSO2Weights(true);
                        }
                    } else
                    {
                        aquaModels[msI].models[aqI].bonePalette.Clear();
                        for (int v = 0; v < aquaModels[msI].models[aqI].vtxlList.Count; v++)
                        {
                            aquaModels[msI].models[aqI].vtxlList[v].bonePalette.Clear();
                            aquaModels[msI].models[aqI].vtxlList[v].vertWeights.Clear();
                            aquaModels[msI].models[aqI].vtxlList[v].vertWeightsNGS.Clear();
                            aquaModels[msI].models[aqI].vtxlList[v].vertWeightIndices.Clear();
                        }
                    }

                    //Reindex materials if needed
                    for (int mesh = 0; mesh < aquaModels[msI].models[aqI].tempTris.Count; mesh++)
                    {
                        var tempMesh = aquaModels[msI].models[aqI].tempTris[mesh];
                        if (tempMesh.matIdDict.Count > 0)
                        {
                            for (int face = 0; face < tempMesh.matIdList.Count; face++)
                            {
                                tempMesh.matIdList[face] = tempMesh.matIdDict[tempMesh.matIdList[face]];
                            }
                        }
                    }

                    SplitMeshByMaterial(aquaModels[msI].models[aqI], matModelSplit);
                    outModel = matModelSplit;
                    if (useRigid == false)
                    {
                        //BatchSplitByBoneCount(matModelSplit, outModel, 255);
                        //RemoveAllUnusedBones(outModel);
                        GenerateGlobalBonePalette(outModel);
                    }
                    if (splitVerts)
                    {
                        CalcUNRMs(outModel, aquaModels[msI].models[aqI].applyNormalAveraging, useUnrms);
                    }

                    //Set up materials and related data
                    for (int mat = 0; mat < aquaModels[msI].models[aqI].tempMats.Count; mat++)
                    {
                        GenerateMaterial(outModel, aquaModels[msI].models[aqI].tempMats[mat], true);
                    }
                                            outModel = matModelSplit;
                    //Set up PSETs and strips, and other per mesh data
                    for (int i = 0; i < outModel.tempTris.Count; i++)
                    {
                        //strips
                        var strips = new AquaObject.stripData();
                        strips.format0xC33 = true;
                        strips.triStrips = new List<ushort>(outModel.tempTris[i].toUshortArray());
                        strips.triIdCount = strips.triStrips.Count;
                        strips.faceGroups.Add(strips.triStrips.Count);
                        outModel.strips.Add(strips);

                        //PSET
                        var pset = new AquaObject.PSET();
                        pset.tag = 0x1000;
                        pset.faceGroupCount = 0x1;
                        pset.psetFaceCount = strips.triIdCount;
                        pset.stripStartCount = totalStripsShorts;
                        outModel.psetList.Add(pset);
                        totalStripsShorts += strips.triIdCount;   //Update this *after* setting the strip start count so that we don't direct to bad data.

                        //MESH
                        var mesh = new AquaObject.MESH();
                        mesh.flags = 0x17; //No idea what this really does. Seems to vary a lot, but also not matter a lot.
                        mesh.unkShort0 = 0x0;
                        mesh.unkByte0 = 0x80;
                        mesh.unkByte1 = 0x64;
                        mesh.unkShort1 = 0;
                        mesh.mateIndex = outModel.tempTris[i].matIdList[0];
                        mesh.rendIndex = mesh.mateIndex;
                        mesh.shadIndex = mesh.mateIndex;
                        mesh.tsetIndex = mesh.mateIndex;
                        mesh.baseMeshNodeId = outModel.tempTris[i].baseMeshNodeId;
                        mesh.vsetIndex = i;
                        mesh.psetIndex = i;
                        if (baHack)
                        {
                            mesh.baseMeshDummyId = 0;
                        }
                        else
                        {
                            mesh.baseMeshDummyId = outModel.tempTris[i].baseMeshDummyId;
                        }
                        mesh.unkInt0 = 0;
                        mesh.reserve0 = 0;
                        outModel.meshList.Add(mesh);
                    }

                    //Generate VTXEs and VSETs
                    int largestVertSize = 0;
                    int vertCounter = 0;
                    for (int i = 0; i < outModel.vtxlList.Count; i++)
                    {
                        totalVerts += outModel.vtxlList[i].vertPositions.Count;
                        AquaObject.VTXE vtxe = ConstructClassicVTXE(outModel.vtxlList[i], out int size);
                        outModel.vtxeList.Add(vtxe);

                        //Track this for objc
                        if (size > largestVertSize)
                        {
                            largestVertSize = size;
                        }

                        AquaObject.VSET vset = new AquaObject.VSET();
                        vset.vtxeCount = outModel.vtxeList.Count -  1;
                        vset.vtxlCount = outModel.vtxlList[i].vertPositions.Count;
                        vset.vtxlStartVert = vertCounter;
                        vertCounter += vset.vtxlCount;
                        outModel.vtxlList[i].bonePalette.Sort();
                        if(useRigid == true)
                        {
                            vset.bonePaletteCount = 0;
                        } else
                        {
                            vset.bonePaletteCount = -1; //Needs more research. This maybe works as a catch all for now?
                            //vset.bonePaletteCount = -(outModel.vtxlList[i].bonePalette[outModel.vtxlList[i].bonePalette.Count - 1] + 1); //This value seems to be the largest index in the used indices + 1 and then made negative.
                        }
                        vset.edgeVertsCount = outModel.vtxlList[i].edgeVerts.Count;
                        outModel.vsetList.Add(vset);
                    }

                    //Set sizes based on VTXE results
                    for(int i = 0; i < outModel.vsetList.Count; i++)
                    {
                        var vset = outModel.vsetList[i];
                        vset.vertDataSize = largestVertSize;
                        outModel.vsetList[i] = vset;
                    }

                    //Generate OBJC
                    AquaObject.OBJC objc = new AquaObject.OBJC();
                    objc.type = 0xC33;
                    objc.size = 0xF0;
                    objc.unkMeshValue = 0x30053; //Taken from pl_rbd_100000
                    objc.largetsVtxl = largestVertSize;
                    objc.totalStripFaces = totalStripsShorts;
                    objc.totalVTXLCount = totalVerts;
                    objc.unkStructCount = outModel.vtxlList.Count;
                    objc.vsetCount = outModel.vsetList.Count;
                    objc.psetCount = outModel.psetList.Count;
                    objc.meshCount = outModel.meshList.Count;
                    objc.mateCount = outModel.mateList.Count;
                    objc.rendCount = outModel.rendList.Count;
                    objc.shadCount = outModel.shadList.Count;
                    objc.tstaCount = outModel.tstaList.Count;
                    objc.tsetCount = outModel.tsetList.Count;
                    objc.texfCount = outModel.texfList.Count;
                    objc.vtxeCount = outModel.vtxeList.Count;
                    objc.fBlock0 = -1;
                    objc.fBlock1 = -1;
                    objc.fBlock2 = -1;
                    objc.fBlock3 = -1;
                    objc.globalStrip3LengthCount = 1;
                    objc.unkCount3 = 1;
                    if (!zeroBounds)
                    {
                        objc.bounds = GenerateBounding(outModel.vtxlList);
                    }
                    outModel.objc = objc;

                    if(useBiTangent)
                    {
                        ComputeTangentSpace(outModel, false, true);
                    }
                    aquaModels[msI].models[aqI] = outModel;
                }
            }
        }

        //Temp material, vtxlList data or tempTri vertex data, and temptris are expected to be populated prior to this process. This should ALWAYS be run before any write attempts.
        public void ConvertToClassicPSO2Mesh(bool useUnrms, bool useFaceNormals, bool baHack, bool useBiTangent, bool zeroBounds, bool useRigid, bool splitVerts = true)
        {
            for (int msI = 0; msI < aquaModels.Count; msI++)
            {
                for (int aqI = 0; aqI < aquaModels[msI].models.Count; aqI++)
                {
                    int totalStripsShorts = 0;
                    int totalVerts = 0;
                    AquaObject matModelSplit = new ClassicAquaObject();
                    AquaObject outModel = new ClassicAquaObject();

                    //Assemble vtxlList
                    if (aquaModels[msI].models[aqI].vtxlList == null || aquaModels[msI].models[aqI].vtxlList.Count == 0)
                    {
                        VTXLFromFaceVerts(aquaModels[msI].models[aqI]);
                    }
                    //Fix weights
                    //Fix weights
                    if (useRigid == false)
                    {
                        foreach (var vtxl in aquaModels[msI].models[aqI].vtxlList)
                        {
                            vtxl.processToPSO2Weights(true);
                        }
                    }
                    else
                    {
                        aquaModels[msI].models[aqI].bonePalette.Clear();
                        for (int v = 0; v < aquaModels[msI].models[aqI].vtxlList.Count; v++)
                        {
                            aquaModels[msI].models[aqI].vtxlList[v].bonePalette.Clear();
                            aquaModels[msI].models[aqI].vtxlList[v].vertWeights.Clear();
                            aquaModels[msI].models[aqI].vtxlList[v].vertWeightsNGS.Clear();
                            aquaModels[msI].models[aqI].vtxlList[v].vertWeightIndices.Clear();
                        }
                    }

                    //Reindex materials if needed
                    for (int mesh = 0; mesh < aquaModels[msI].models[aqI].tempTris.Count; mesh++)
                    {
                        var tempMesh = aquaModels[msI].models[aqI].tempTris[mesh];
                        if (tempMesh.matIdDict.Count > 0)
                        {
                            for (int face = 0; face < tempMesh.matIdList.Count; face++)
                            {
                                tempMesh.matIdList[face] = tempMesh.matIdDict[tempMesh.matIdList[face]];
                            }
                        }
                    }

                    SplitMeshByMaterial(aquaModels[msI].models[aqI], matModelSplit);
                    if (useRigid == false)
                    {
                        BatchSplitByBoneCount(matModelSplit, outModel, 16);
                        RemoveAllUnusedBones(outModel);
                    } else
                    {
                        outModel = matModelSplit;
                    }
                    if (splitVerts)
                    {
                        CalcUNRMs(outModel, aquaModels[msI].models[aqI].applyNormalAveraging, useUnrms);
                    }

                    //Set up materials and related data
                    for (int mat = 0; mat < aquaModels[msI].models[aqI].tempMats.Count; mat++)
                    {
                        GenerateMaterial(outModel, aquaModels[msI].models[aqI].tempMats[mat]);
                    }

                    //Set up PSETs and strips, and other per mesh data
                    for (int i = 0; i < outModel.tempTris.Count; i++)
                    {
                        //strips
                        var strips = new AquaObject.stripData(outModel.tempTris[i].toUshortArray());
                        outModel.strips.Add(strips);

                        //PSET
                        var pset = new AquaObject.PSET();
                        pset.tag = 0x2100;
                        pset.faceGroupCount = 0x1;
                        pset.psetFaceCount = strips.triIdCount;
                        outModel.psetList.Add(pset);
                        totalStripsShorts += strips.triIdCount;

                        //MESH
                        var mesh = new AquaObject.MESH();
                        mesh.flags = 0x17; //No idea what this really does. Seems to vary a lot, but also not matter a lot.
                        mesh.unkShort0 = 0x0;
                        mesh.unkByte0 = 0x80;
                        mesh.unkByte1 = 0x64;
                        mesh.unkShort1 = 0;
                        mesh.mateIndex = outModel.tempTris[i].matIdList[0];
                        mesh.rendIndex = mesh.mateIndex;
                        mesh.shadIndex = mesh.mateIndex;
                        mesh.tsetIndex = mesh.mateIndex;
                        if (baHack)
                        {
                            mesh.baseMeshNodeId = 0;
                        } else
                        {
                            mesh.baseMeshNodeId = outModel.tempTris[i].baseMeshNodeId;
                        }
                        mesh.vsetIndex = i;
                        mesh.psetIndex = i;
                        if (baHack)
                        {
                            mesh.baseMeshDummyId = 0;
                        } else
                        {
                            mesh.baseMeshDummyId = outModel.tempTris[i].baseMeshDummyId;
                        }
                        mesh.unkInt0 = 0;
                        mesh.reserve0 = 0;
                        outModel.meshList.Add(mesh);
                    }

                    //Generate VTXEs and VSETs
                    int largestVertSize = 0;
                    for (int i = 0; i < outModel.vtxlList.Count; i++)
                    {
                        totalVerts += outModel.vtxlList[i].vertPositions.Count;
                        AquaObject.VTXE vtxe = ConstructClassicVTXE(outModel.vtxlList[i], out int size);
                        outModel.vtxeList.Add(vtxe);

                        //Track this for objc
                        if (size > largestVertSize)
                        {
                            largestVertSize = size;
                        }

                        AquaObject.VSET vset = new AquaObject.VSET();
                        vset.vertDataSize = size;
                        vset.vtxeCount = vtxe.vertDataTypes.Count;
                        vset.vtxlCount = outModel.vtxlList[i].vertPositions.Count;
                        if (useRigid == true)
                        {
                            vset.bonePaletteCount = 0;
                        }
                        else
                        {
                            vset.bonePaletteCount = outModel.vtxlList[i].bonePalette.Count;
                        }
                        vset.edgeVertsCount = outModel.vtxlList[i].edgeVerts.Count;
                        outModel.vsetList.Add(vset);
                    }

                    //Generate OBJC
                    AquaObject.OBJC objc = new AquaObject.OBJC();
                    objc.type = 0xC2A;
                    objc.size = 0xA4;
                    objc.unkMeshValue = 0x17; //This seems to work. Do I know what it does? NOPE
                    objc.largetsVtxl = largestVertSize;
                    objc.totalStripFaces = totalStripsShorts;
                    objc.totalVTXLCount = totalVerts;
                    objc.unkStructCount = outModel.vtxlList.Count;
                    objc.vsetCount = outModel.vsetList.Count;
                    objc.psetCount = outModel.psetList.Count;
                    objc.meshCount = outModel.meshList.Count;
                    objc.mateCount = outModel.mateList.Count;
                    objc.rendCount = outModel.rendList.Count;
                    objc.shadCount = outModel.shadList.Count;
                    objc.tstaCount = outModel.tstaList.Count;
                    objc.tsetCount = outModel.tsetList.Count;
                    objc.texfCount = outModel.texfList.Count;
                    if (!zeroBounds)
                    {
                        objc.bounds = GenerateBounding(outModel.vtxlList);
                    }
                    outModel.objc = objc;

                    if (useBiTangent)
                    {
                        ComputeTangentSpace(outModel, false, true);
                    }
                    aquaModels[msI].models[aqI] = outModel;
                }
            }
        }

        public void WriteVTBFModel(string ogFileName, string outFileName)
        {
            bool package = outFileName.Contains(".aqp") || outFileName.Contains(".trp"); //If we're doing .aqo/.tro instead, we write only the first model and no aqp header
            int modelCount = aquaModels[0].models.Count;
            List<byte> finalOutBytes = new List<byte>();
            if (package)
            {
                finalOutBytes.AddRange(new byte[] { 0x61, 0x66, 0x70, 0 });
                finalOutBytes.AddRange(BitConverter.GetBytes(aquaModels[0].models.Count + tpnFiles.Count));
                finalOutBytes.AddRange(BitConverter.GetBytes((int)0));
                finalOutBytes.AddRange(BitConverter.GetBytes((int)1));
            }
            else
            {
                modelCount = 1;
            }

            for (int i = 0; i < modelCount; i++)
            {
                int bonusBytes = 0;
                if (i == 0)
                {
                    bonusBytes = 0x10;
                }

                List<byte> outBytes = new List<byte>();
                ClassicAquaObject model = (ClassicAquaObject)aquaModels[0].models[i];
                outBytes.AddRange(toAQGFVTBF());
                outBytes.AddRange(toROOT());
                outBytes.AddRange(toOBJC(model.objc, model.unrms != null));
                outBytes.AddRange(toVSETList(model.vsetList, model.vtxlList));
                for (int j = 0; j < model.vtxlList.Count; j++)
                {
                    outBytes.AddRange(toVTXE_VTXL(model.vtxeList[j], model.vtxlList[j]));
                }
                outBytes.AddRange(toPSET(model.psetList, model.strips));
                outBytes.AddRange(toMESH(model.meshList));
                outBytes.AddRange(toMATE(model.mateList));
                outBytes.AddRange(toREND(model.rendList));
                outBytes.AddRange(toSHAD(model.shadList)); //Handles SHAP as well
                outBytes.AddRange(toTSTA(model.tstaList));
                outBytes.AddRange(toTSET(model.tsetList));
                outBytes.AddRange(toTEXF(model.texfList));
                if (model.unrms != null)
                {
                    outBytes.AddRange(toUNRM(model.unrms));
                }

                //Header info
                int size = outBytes.Count;
                WriteAFPBase(ogFileName, package, i, bonusBytes, outBytes, size);

                finalOutBytes.AddRange(outBytes);

                AlignFileEndWrite(finalOutBytes, 0x10);
            }
            WriteTPN(package, finalOutBytes);

            File.WriteAllBytes(outFileName, finalOutBytes.ToArray());
        }

        private void WriteAFPBase(string ogFileName, bool package, int i, int bonusBytes, List<byte> outBytes, int size)
        {
            if (package)
            {
                int difference;
                if (size % 0x10 == 0)
                {
                    difference = 0x10;
                }
                else
                {
                    difference = 0x10 - (size % 0x10);
                }

                //Handle filename text
                byte[] fName = new byte[0x20];
                string modelCounter = ".";
                if (aquaModels[0].models.Count > 1)
                {
                    modelCounter = $"_l{i + 1}.";
                }
                byte[] outFileNameBytes = Encoding.UTF8.GetBytes(Path.GetFileName(ogFileName).Replace(Path.GetExtension(ogFileName),
                    modelCounter + Encoding.UTF8.GetString(returnModelType(ogFileName))));
                int nameCount = outFileNameBytes.Length < 0x20 ? outFileNameBytes.Length : 0x20;
                for (int j = 0; j < nameCount; j++)
                {
                    fName[j] = outFileNameBytes[j];
                }

                outBytes.InsertRange(0, returnModelType(ogFileName));
                outBytes.InsertRange(0, BitConverter.GetBytes(size + difference + 0x30 + bonusBytes));
                outBytes.InsertRange(0, BitConverter.GetBytes(0x30));
                outBytes.InsertRange(0, BitConverter.GetBytes(size));
                outBytes.InsertRange(0, fName);
            }
        }

        private void WriteTPN(bool package, List<byte> finalOutBytes)
        {

            //Write texture patterns
            if (package)
            {
                for (int i = 0; i < tpnFiles.Count; i++)
                {
                    finalOutBytes.AddRange(ConvertStruct(tpnFiles[i].tpnAFPBase));
                    finalOutBytes.AddRange(ConvertStruct(tpnFiles[i].header));
                    for (int j = 0; j < tpnFiles[i].header.count; j++)
                    {
                        if (i > 0)
                        {
                            finalOutBytes.AddRange(BitConverter.GetBytes((double)0.0));
                        }
                        finalOutBytes.AddRange(ConvertStruct(tpnFiles[i].texSets[j]));
                    }
                    AlignFileEndWrite(finalOutBytes, 0x10);
                }
            }
        }

        //Note, this method assumes the data has already been processed to work with this format
        public void WriteNGSNIFLModel(string ogFileName, string outFileName)
        {
            bool package = outFileName.Contains(".aqp") || outFileName.Contains(".trp"); //If we're doing .aqo/.tro instead, we write only the first model and no aqp header
            int modelCount = aquaModels[0].models.Count;
            List<byte> finalOutBytes = new List<byte>();
            if (package)
            {
                finalOutBytes.AddRange(new byte[] { 0x61, 0x66, 0x70, 0 });
                finalOutBytes.AddRange(BitConverter.GetBytes(aquaModels[0].models.Count + tpnFiles.Count));
                finalOutBytes.AddRange(BitConverter.GetBytes((int)0));
                finalOutBytes.AddRange(BitConverter.GetBytes((int)1));
            }
            else
            {
                modelCount = 1;
            }

            //Write model data out
            for (int modelId = 0; modelId < modelCount; modelId++)
            {
                int bonusBytes = 0; //Should be 0 for NIFL
                NGSAquaObject model = (NGSAquaObject)aquaModels[0].models[modelId];

                //Pointer data offsets for filling in later
                int rel0SizeOffset;

                //OBJC Offsets
                int objcGlobalTriOffset;
                int objcGlobalVtxlOffset;
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
                objcGlobalTriOffset = NOF0Append(nof0PointerLocations, outBytes.Count + 0x14, model.strips.Count);
                objcGlobalVtxlOffset = NOF0Append(nof0PointerLocations, outBytes.Count + 0x1C, model.vtxlList.Count);
                objcVsetOffset = NOF0Append(nof0PointerLocations, outBytes.Count + 0x28, model.objc.vsetCount);
                objcPsetOffset = NOF0Append(nof0PointerLocations, outBytes.Count + 0x30, model.objc.psetCount);
                objcMeshOffset = NOF0Append(nof0PointerLocations, outBytes.Count + 0x38, model.objc.meshCount);
                objcMateOffset = NOF0Append(nof0PointerLocations, outBytes.Count + 0x40, model.objc.mateCount);

                objcRendOffset = NOF0Append(nof0PointerLocations, outBytes.Count + 0x48, model.objc.rendCount);
                objcShadOffset = NOF0Append(nof0PointerLocations, outBytes.Count + 0x50, model.objc.shadCount);
                objcTstaOffset = NOF0Append(nof0PointerLocations, outBytes.Count + 0x58, model.objc.tstaCount);
                objcTsetOffset = NOF0Append(nof0PointerLocations, outBytes.Count + 0x60, model.objc.tsetCount);

                objcTexfOffset = NOF0Append(nof0PointerLocations, outBytes.Count + 0x68, model.objc.texfCount);
                if (model.unrms != null)
                {
                    objcUnrmOffset = NOF0Append(nof0PointerLocations, outBytes.Count + 0xA0, 1);
                }
                else
                {
                    objcUnrmOffset = -1;
                }
                objcVtxeOffset = NOF0Append(nof0PointerLocations, outBytes.Count + 0xA8, model.objc.vtxeCount);
                objcBonePaletteOffset = NOF0Append(nof0PointerLocations, outBytes.Count + 0xAC, model.bonePalette.Count);

                objcUnkStruct1Offset = NOF0Append(nof0PointerLocations, outBytes.Count + 0xC4, model.unkStruct1List.Count);
                objcPset2Offset = NOF0Append(nof0PointerLocations, outBytes.Count + 0xCC, model.pset2List.Count);
                objcMesh2Offset = NOF0Append(nof0PointerLocations, outBytes.Count + 0xD4, model.mesh2List.Count);
                objcGlobalStrip3Offset = NOF0Append(nof0PointerLocations, outBytes.Count + 0xD8, model.strips3.Count);

                objcGlobalStrip3LengthOffset3 = NOF0Append(nof0PointerLocations, outBytes.Count + 0xE0, model.strips3Lengths.Count);
                objcUnkPointArray1Offset = NOF0Append(nof0PointerLocations, outBytes.Count + 0xE4, model.unkPointArray1.Count);
                objcUnkPointArray2Offset = NOF0Append(nof0PointerLocations, outBytes.Count + 0xE8, model.unkPointArray2.Count);

                //Write OBJC block
                outBytes.AddRange(BitConverter.GetBytes(model.objc.type));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.size));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.unkMeshValue));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.largetsVtxl));

                outBytes.AddRange(BitConverter.GetBytes(model.objc.totalStripFaces));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.globalStripOffset));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.totalVTXLCount));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.vtxlStartOffset));

                outBytes.AddRange(BitConverter.GetBytes(model.objc.unkStructCount));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.vsetCount));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.vsetOffset));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.psetCount));

                outBytes.AddRange(BitConverter.GetBytes(model.objc.psetOffset));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.meshCount));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.meshOffset));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.mateCount));

                outBytes.AddRange(BitConverter.GetBytes(model.objc.mateOffset));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.rendCount));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.rendOffset));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.shadCount));

                outBytes.AddRange(BitConverter.GetBytes(model.objc.shadOffset));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.tstaCount));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.tstaOffset));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.tsetCount));

                outBytes.AddRange(BitConverter.GetBytes(model.objc.tsetOffset));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.texfCount));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.texfOffset));

                outBytes.AddRange(ConvertStruct(model.objc.bounds));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.unkCount0));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.unrmOffset));

                //0xC33 sections
                outBytes.AddRange(BitConverter.GetBytes(model.objc.vtxeCount));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.vtxeOffset));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.bonePaletteOffset));

                outBytes.AddRange(BitConverter.GetBytes(model.objc.fBlock0));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.fBlock1));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.fBlock2));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.fBlock3));

                outBytes.AddRange(BitConverter.GetBytes(model.objc.unkStruct1Count));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.unkStruct1Offset));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.pset2Count));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.pset2Offset));

                outBytes.AddRange(BitConverter.GetBytes(model.objc.mesh2Count));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.mesh2Offset));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.globalStrip3Offset));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.globalStrip3LengthCount));   //At least 1

                outBytes.AddRange(BitConverter.GetBytes(model.objc.globalStrip3LengthOffset));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.unkPointArray1Offset));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.unkPointArray2Offset));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.unkCount3));   //Default to 1?

                AlignWriter(outBytes, 0x10);

                //Write triangles. NGS doesn't use tristrips anymore
                //Assumes the ushorts are already laid out as triangles
                SetByteListInt(outBytes, objcGlobalTriOffset, outBytes.Count);
                for (int i = 0; i < model.strips.Count; i++)
                {
                    foreach(var id in model.strips[i].triStrips)
                    {
                        outBytes.AddRange(BitConverter.GetBytes(id));
                    }
                }
                AlignWriter(outBytes, 0x10);

                //Write NGS VTXL
                SetByteListInt(outBytes, objcGlobalVtxlOffset, outBytes.Count);
                for (int i = 0; i < model.vtxlList.Count; i++)
                {
                    WriteVTXL(model.vtxeList[model.vsetList[i].vtxeCount], model.vtxlList[i], outBytes, model.objc.largetsVtxl);
                }
                AlignWriter(outBytes, 0x10);

                //VSET
                //Write VSET pointer
                SetByteListInt(outBytes, objcVsetOffset, outBytes.Count);

                //Write VSET
                for (int vsetId = 0; vsetId < model.vsetList.Count; vsetId++)
                {
                    vsetEdgeVertOffsets.Add(NOF0Append(nof0PointerLocations, outBytes.Count + 0x30, model.vsetList[vsetId].edgeVertsCount));

                    outBytes.AddRange(ConvertStruct(model.vsetList[vsetId]));
                }
                AlignWriter(outBytes, 0x10);

                //Write Edge Verts
                for (int vsetId = 0; vsetId < model.vsetList.Count; vsetId++)
                {
                    if (model.vtxlList[vsetId].edgeVerts != null && model.vtxlList[vsetId].edgeVerts.Count > 0)
                    {
                        //Write edge verts pointer
                        SetByteListInt(outBytes, vsetEdgeVertOffsets[vsetId], outBytes.Count);
                        //Write edge verts
                        for (int evId = 0; evId < model.vtxlList[vsetId].edgeVerts.Count; evId++)
                        {
                            outBytes.AddRange(BitConverter.GetBytes(model.vtxlList[vsetId].edgeVerts[evId]));
                        }
                        AlignWriter(outBytes, 0x10);
                    }
                }

                //PSET

                //Write PSET pointer
                SetByteListInt(outBytes, objcPsetOffset, outBytes.Count);

                //Write PSET
                for (int psetId = 0; psetId < model.psetList.Count; psetId++)
                {
                    psetfirstOffsets.Add(NOF0Append(nof0PointerLocations, outBytes.Count + 0x8));
                    outBytes.AddRange(ConvertStruct(model.psetList[psetId]));
                }
                AlignWriter(outBytes, 0x10);

                //Write triangle groups
                for (int stripId = 0; stripId < model.strips.Count; stripId++)
                {
                    SetByteListInt(outBytes, psetfirstOffsets[stripId], outBytes.Count);

                    for(int id = 0; id < model.strips[stripId].faceGroups.Count; id++)
                    {
                        outBytes.AddRange(BitConverter.GetBytes(model.strips[stripId].faceGroups[id]));
                    }
                    AlignWriter(outBytes, 0x10); //These should all align already, but just in case
                }

                //MESH

                //Write MESH pointer
                SetByteListInt(outBytes, objcMeshOffset, outBytes.Count);

                //Write MESH
                for (int meshId = 0; meshId < model.meshList.Count; meshId++)
                {
                    outBytes.AddRange(ConvertStruct(model.meshList[meshId]));
                }

                //MATE

                //Write MATE pointer
                SetByteListInt(outBytes, objcMateOffset, outBytes.Count);

                //Write MATE
                for (int mateId = 0; mateId < model.mateList.Count; mateId++)
                {
                    outBytes.AddRange(ConvertStruct(model.mateList[mateId]));
                }
                AlignWriter(outBytes, 0x10);

                //REND

                //Write REND pointer
                SetByteListInt(outBytes, objcRendOffset, outBytes.Count);

                //Write REND
                for (int rendId = 0; rendId < model.rendList.Count; rendId++)
                {
                    outBytes.AddRange(ConvertStruct(model.rendList[rendId]));
                }
                AlignWriter(outBytes, 0x10);

                //SHAD

                //Write SHAD pointer
                SetByteListInt(outBytes, objcShadOffset, outBytes.Count);

                //Write SHAD
                for (int shadId = 0; shadId < model.shadList.Count; shadId++)
                {
                    outBytes.AddRange(BitConverter.GetBytes(model.shadList[shadId].unk0));
                    outBytes.AddRange(model.shadList[shadId].pixelShader.GetBytes());
                    outBytes.AddRange(model.shadList[shadId].vertexShader.GetBytes());

                    shadDetailOffsets.Add(NOF0Append(nof0PointerLocations, outBytes.Count, model.shadList[shadId].shadDetailOffset));
                    shadExtraOffsets.Add(NOF0Append(nof0PointerLocations, outBytes.Count + 4, model.shadList[shadId].shadExtraOffset));
                    outBytes.AddRange(BitConverter.GetBytes(model.shadList[shadId].shadDetailOffset));
                    outBytes.AddRange(BitConverter.GetBytes(model.shadList[shadId].shadExtraOffset));
                }
                AlignWriter(outBytes, 0x10);

                //Write SHAD sub structs
                for (int shadId = 0; shadId < model.shadList.Count; shadId++)
                {
                    var shad = (NGSAquaObject.NGSSHAD)model.shadList[shadId];

                    if(shad.shadDetailOffset != 0) 
                    {
                        SetByteListInt(outBytes, shadDetailOffsets[shadId], outBytes.Count);
                        outBytes.AddRange(ConvertStruct(shad.shadDetail));
                    }

                    if(shad.shadExtra.Count > 0)
                    {
                        SetByteListInt(outBytes, shadExtraOffsets[shadId], outBytes.Count);
                        foreach (var extra in shad.shadExtra)
                        {
                            outBytes.AddRange(ConvertStruct(extra));
                        }
                    }
                }

                //TSTA
                if (model.tstaList.Count > 0)
                {
                    //Write TSTA pointer
                    SetByteListInt(outBytes, objcTstaOffset, outBytes.Count);

                    //Write TSTA
                    for (int tstaId = 0; tstaId < model.tstaList.Count; tstaId++)
                    {
                        outBytes.AddRange(ConvertStruct(model.tstaList[tstaId]));
                    }
                    AlignWriter(outBytes, 0x10);
                }

                //TSET

                //Write TSET pointer
                SetByteListInt(outBytes, objcTsetOffset, outBytes.Count);

                //Write TSET
                for (int tsetId = 0; tsetId < model.tsetList.Count; tsetId++)
                {
                    outBytes.AddRange(BitConverter.GetBytes(model.tsetList[tsetId].unkInt0));
                    outBytes.AddRange(BitConverter.GetBytes(model.tsetList[tsetId].texCount));
                    outBytes.AddRange(BitConverter.GetBytes(model.tsetList[tsetId].unkInt1));
                    outBytes.AddRange(BitConverter.GetBytes(model.tsetList[tsetId].unkInt2));

                    outBytes.AddRange(BitConverter.GetBytes(model.tsetList[tsetId].unkInt3));

                    //Do as bytes for greater than 4, ints if 4 or less. Remainder of the 0x10 area should be 0xFF.
                    if (model.tsetList[tsetId].texCount > 4)
                    {
                        for (int i = 0; i < model.tsetList[tsetId].texCount; i++)
                        {
                            outBytes.Add((byte)model.tsetList[tsetId].tstaTexIDs[i]);
                        }
                        for (int i = 0; i < (0x10 - model.tsetList[tsetId].texCount); i++)
                        {
                            outBytes.Add(0xFF);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < model.tsetList[tsetId].texCount; i++)
                        {
                            outBytes.AddRange(BitConverter.GetBytes(model.tsetList[tsetId].tstaTexIDs[i]));
                        }
                        for (int i = 0; i < (0x10 - (model.tsetList[tsetId].texCount * sizeof(int))); i++)
                        {
                            outBytes.Add(0xFF);
                        }
                    }
                }
                AlignWriter(outBytes, 0x10);

                //TEXF
                if (model.tstaList.Count > 0)
                {
                    //Write TEXF pointer
                    SetByteListInt(outBytes, objcTexfOffset, outBytes.Count);

                    //Write TEXF
                    for (int texfId = 0; texfId < model.texfList.Count; texfId++)
                    {
                        outBytes.AddRange(ConvertStruct(model.texfList[texfId]));
                    }
                    AlignWriter(outBytes, 0x10);
                }

                //UNRM
                if (model.unrms != null)
                {
                    int meshIDPointerOffset = 0;
                    int vertIDPointerOffset = 0;

                    //Write UNRM pointer
                    SetByteListInt(outBytes, objcUnrmOffset, outBytes.Count);

                    //Write UNRM
                    outBytes.AddRange(BitConverter.GetBytes(model.unrms.vertGroupCountCount));
                    NOF0Append(nof0PointerLocations, outBytes.Count, 1);
                    outBytes.AddRange(BitConverter.GetBytes(outBytes.Count + 0x1C)); //Should always start a set amount after here
                    outBytes.AddRange(BitConverter.GetBytes(model.unrms.vertCount));
                    meshIDPointerOffset = NOF0Append(nof0PointerLocations, outBytes.Count, 1);
                    outBytes.AddRange(BitConverter.GetBytes(model.unrms.meshIdOffset));
                    vertIDPointerOffset = NOF0Append(nof0PointerLocations, outBytes.Count, 1);
                    outBytes.AddRange(BitConverter.GetBytes(model.unrms.vertIDOffset));
                    outBytes.AddRange(BitConverter.GetBytes(model.unrms.padding0));
                    outBytes.AddRange(BitConverter.GetBytes(model.unrms.padding1));

                    //Write group counts
                    for (int i = 0; i < model.unrms.unrmVertGroups.Count; i++)
                    {
                        outBytes.AddRange(BitConverter.GetBytes(model.unrms.unrmVertGroups[i]));
                    }
                    AlignWriter(outBytes, 0x10);

                    //Write Mesh Ids
                    SetByteListInt(outBytes, meshIDPointerOffset, outBytes.Count);
                    for (int i = 0; i < model.unrms.unrmMeshIds.Count; i++)
                    {
                        for (int j = 0; j < model.unrms.unrmMeshIds[i].Count; j++)
                        {
                            outBytes.AddRange(BitConverter.GetBytes(model.unrms.unrmMeshIds[i][j]));
                        }
                    }
                    AlignWriter(outBytes, 0x10);

                    //Write Vert Ids
                    SetByteListInt(outBytes, vertIDPointerOffset, outBytes.Count);
                    for (int i = 0; i < model.unrms.unrmMeshIds.Count; i++)
                    {
                        for (int j = 0; j < model.unrms.unrmMeshIds[i].Count; j++)
                        {
                            outBytes.AddRange(BitConverter.GetBytes(model.unrms.unrmVertIds[i][j]));
                        }
                    }
                    AlignWriter(outBytes, 0x10);
                }

                //VTXE
                if(model.vtxeList.Count > 0)
                {
                    SetByteListInt(outBytes, objcVtxeOffset, outBytes.Count);
                    for (int vt = 0; vt < model.vtxeList.Count; vt++)
                    {
                        vtxeOffsets.Add(NOF0Append(nof0PointerLocations, outBytes.Count + 4));
                        outBytes.AddRange(BitConverter.GetBytes(model.vtxeList[vt].vertDataTypes.Count));
                        outBytes.AddRange(BitConverter.GetBytes(0));
                    }
                    AlignWriter(outBytes, 0x10);
                    for (int vt = 0; vt < model.vtxeList.Count; vt++)
                    {
                        SetByteListInt(outBytes, vtxeOffsets[vt], outBytes.Count);
                        for (int xe = 0; xe < model.vtxeList[vt].vertDataTypes.Count; xe++)
                        {
                            outBytes.AddRange(ConvertStruct(model.vtxeList[vt].vertDataTypes[xe]));
                        }
                    }
                }

                //BonePalette
                if(model.bonePalette.Count > 0)
                {
                    SetByteListInt(outBytes, objcBonePaletteOffset, outBytes.Count);
                    NOF0Append(nof0PointerLocations, outBytes.Count + 4);
                    outBytes.AddRange(BitConverter.GetBytes(model.bonePalette.Count));
                    outBytes.AddRange(BitConverter.GetBytes(outBytes.Count + 4));

                    //Write bones
                    for (int bn = 0; bn < model.bonePalette.Count; bn++)
                    {
                        outBytes.AddRange(BitConverter.GetBytes(model.bonePalette[bn]));
                    }
                }
                AlignWriter(outBytes, 0x10);

                //UnkStruct1
                if (model.unkStruct1List.Count > 0)
                {
                    SetByteListInt(outBytes, objcUnkStruct1Offset, outBytes.Count);
                    for(int unk = 0; unk < model.unkStruct1List.Count; unk++)
                    {
                        outBytes.AddRange(ConvertStruct(model.unkStruct1List[unk]));
                    }
                }

                //PSET 2

                if(model.pset2List.Count > 0)
                {                
                    //Write PSET 2 pointer
                    SetByteListInt(outBytes, objcPset2Offset, outBytes.Count);

                    //Write PSET 2
                    for (int psetId = 0; psetId < model.pset2List.Count; psetId++)
                    {
                        pset2firstOffsets.Add(NOF0Append(nof0PointerLocations, outBytes.Count + 0x8));
                        outBytes.AddRange(ConvertStruct(model.pset2List[psetId]));
                    }
                    AlignWriter(outBytes, 0x10);

                    //Write triangle groups
                    for (int stripId = 0; stripId < model.strips2.Count; stripId++)
                    {
                        SetByteListInt(outBytes, pset2firstOffsets[stripId], outBytes.Count);

                        for (int id = 0; id < model.strips2[stripId].faceGroups.Count; id++)
                        {
                            outBytes.AddRange(BitConverter.GetBytes(model.strips2[stripId].faceGroups[id]));
                        }
                        AlignWriter(outBytes, 0x10); //These should all align already, but just in case
                    }
                }

                //MESH 2
                if (model.mesh2List.Count > 0)
                {
                    //Write MESH 2 pointer
                    SetByteListInt(outBytes, objcMesh2Offset, outBytes.Count);

                    //Write MESH 2
                    for (int meshId = 0; meshId < model.mesh2List.Count; meshId++)
                    {
                        outBytes.AddRange(ConvertStruct(model.mesh2List[meshId]));
                    }
                    AlignWriter(outBytes, 0x10);
                }

                //Strip data 2, used for strip set 3 (strip set 2 pulls from the main index set)
                if(model.strips3.Count > 0)
                {
                    SetByteListInt(outBytes, objcGlobalStrip3Offset, outBytes.Count);

                    //Write triangles. NGS doesn't use tristrips anymore
                    //Assumes the ushorts are already laid out as triangles
                    for (int i = 0; i < model.strips3.Count; i++)
                    {
                        foreach (var id in model.strips3[i].triStrips)
                        {
                            outBytes.AddRange(BitConverter.GetBytes(id));
                        }
                    }
                    AlignWriter(outBytes, 0x10);
                }

                //Strip lengths (Strip set 3 seemingly uses a simplified method for storing these)
                if(model.strips3Lengths.Count > 0)
                {
                    SetByteListInt(outBytes, objcGlobalStrip3LengthOffset3, outBytes.Count);
                    for(int i = 0; i < model.strips3Lengths.Count; i++)
                    {
                        outBytes.AddRange(BitConverter.GetBytes(model.strips3Lengths[i]));
                    }
                    AlignWriter(outBytes, 0x10);
                }

                //Point arrays
                if(model.unkPointArray1.Count > 0)
                {
                    SetByteListInt(outBytes, objcUnkPointArray1Offset, outBytes.Count);
                    for(int i = 0; i < model.unkPointArray1.Count; i++)
                    {
                        outBytes.AddRange(ConvertStruct(model.unkPointArray1[i]));
                    }
                }
                if (model.unkPointArray2.Count > 0)
                {
                    SetByteListInt(outBytes, objcUnkPointArray2Offset, outBytes.Count);
                    for (int i = 0; i < model.unkPointArray2.Count; i++)
                    {
                        outBytes.AddRange(ConvertStruct(model.unkPointArray2[i]));
                    }
                }

                //Write REL0 Size
                SetByteListInt(outBytes, rel0SizeOffset, outBytes.Count - 0x8);

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
                NOF0FullSize += AlignWriter(outBytes, 0x10);

                //NEND
                outBytes.AddRange(Encoding.UTF8.GetBytes("NEND"));
                outBytes.AddRange(BitConverter.GetBytes(0x8));
                outBytes.AddRange(BitConverter.GetBytes(0));
                outBytes.AddRange(BitConverter.GetBytes(0));

                //Generate NIFL
                AquaCommon.NIFL nifl = new AquaCommon.NIFL();
                nifl.magic = BitConverter.ToInt32(Encoding.UTF8.GetBytes("NIFL"), 0);
                nifl.NIFLLength = 0x18;
                nifl.unkInt0 = 1;
                nifl.offsetAddition = 0x20;

                nifl.NOF0Offset = NOF0Offset;
                nifl.NOF0OffsetFull = NOF0Offset + 0x20;
                nifl.NOF0BlockSize = NOF0FullSize;
                nifl.padding0 = 0;

                //Write NIFL
                outBytes.InsertRange(0, ConvertStruct(nifl));
                AlignFileEndWrite(outBytes, 0x10);

                //Write AFP Base
                int size = outBytes.Count - 0x10; //Size is 0x10 less than what it would be for VTBF afp headers for some reason
                WriteAFPBase(ogFileName, package, modelId, bonusBytes, outBytes, size);

                finalOutBytes.AddRange(outBytes);
            }
            WriteTPN(package, finalOutBytes);

            File.WriteAllBytes(outFileName, finalOutBytes.ToArray());
        }

        public void WriteClassicNIFLModel(string ogFileName, string outFileName)
        {
            bool package = outFileName.Contains(".aqp") || outFileName.Contains(".trp"); //If we're doing .aqo/.tro instead, we write only the first model and no aqp header
            int modelCount = aquaModels[0].models.Count;
            List<byte> finalOutBytes = new List<byte>();
            if (package)
            {
                finalOutBytes.AddRange(new byte[] { 0x61, 0x66, 0x70, 0 });
                finalOutBytes.AddRange(BitConverter.GetBytes(aquaModels[0].models.Count + tpnFiles.Count));
                finalOutBytes.AddRange(BitConverter.GetBytes((int)0));
                finalOutBytes.AddRange(BitConverter.GetBytes((int)1));
            } else
            {
                modelCount = 1;
            }

            //Write model data out
            for (int modelId = 0; modelId < modelCount; modelId++)
            {
                int bonusBytes = 0; //Should be 0 for NIFL
                ClassicAquaObject model = (ClassicAquaObject)aquaModels[0].models[modelId];

                //Pointer data offsets for filling in later
                int rel0SizeOffset;

                //OBJC Offsets
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

                List<int> vsetVtxeOffsets = new List<int>();
                List<int> vsetVtxlOffsets = new List<int>();
                List<int> vsetBonePaletteOffsets = new List<int>();
                List<int> vsetEdgeVertOffsets = new List<int>();

                List<int> psetfirstOffsets = new List<int>();

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
                objcVsetOffset = NOF0Append(nof0PointerLocations, outBytes.Count + 0x28, model.objc.vsetCount);
                objcPsetOffset = NOF0Append(nof0PointerLocations, outBytes.Count + 0x30, model.objc.psetCount);
                objcMeshOffset = NOF0Append(nof0PointerLocations, outBytes.Count + 0x38, model.objc.meshCount);
                objcMateOffset = NOF0Append(nof0PointerLocations, outBytes.Count + 0x40, model.objc.mateCount);

                objcRendOffset = NOF0Append(nof0PointerLocations, outBytes.Count + 0x48, model.objc.rendCount);
                objcShadOffset = NOF0Append(nof0PointerLocations, outBytes.Count + 0x50, model.objc.shadCount);
                objcTstaOffset = NOF0Append(nof0PointerLocations, outBytes.Count + 0x58, model.objc.tstaCount);
                objcTsetOffset = NOF0Append(nof0PointerLocations, outBytes.Count + 0x60, model.objc.tsetCount);

                objcTexfOffset = NOF0Append(nof0PointerLocations, outBytes.Count + 0x68, model.objc.texfCount);
                if (model.unrms != null)
                {
                    objcUnrmOffset = NOF0Append(nof0PointerLocations, outBytes.Count + 0xA0, 1);
                } else
                {
                    objcUnrmOffset = -1;
                }

                //Write OBJC block
                outBytes.AddRange(BitConverter.GetBytes(model.objc.type));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.size));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.unkMeshValue));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.largetsVtxl));

                outBytes.AddRange(BitConverter.GetBytes(model.objc.totalStripFaces));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.globalStripOffset));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.totalVTXLCount));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.vtxlStartOffset));

                outBytes.AddRange(BitConverter.GetBytes(model.objc.unkStructCount));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.vsetCount));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.vsetOffset));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.psetCount));

                outBytes.AddRange(BitConverter.GetBytes(model.objc.psetOffset));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.meshCount));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.meshOffset));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.mateCount));

                outBytes.AddRange(BitConverter.GetBytes(model.objc.mateOffset));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.rendCount));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.rendOffset));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.shadCount));

                outBytes.AddRange(BitConverter.GetBytes(model.objc.shadOffset));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.tstaCount));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.tstaOffset));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.tsetCount));

                outBytes.AddRange(BitConverter.GetBytes(model.objc.tsetOffset));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.texfCount));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.texfOffset));

                outBytes.AddRange(ConvertStruct(model.objc.bounds));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.unkCount0));
                outBytes.AddRange(BitConverter.GetBytes(model.objc.unrmOffset));

                AlignWriter(outBytes, 0x10);

                //VSET
                //Write VSET pointer
                SetByteListInt(outBytes, objcVsetOffset, outBytes.Count);

                //Write VSET
                for (int vsetId = 0; vsetId < model.vsetList.Count; vsetId++)
                {
                    vsetVtxeOffsets.Add(NOF0Append(nof0PointerLocations, outBytes.Count + 0x8, model.vsetList[vsetId].vtxeCount));
                    vsetVtxlOffsets.Add(NOF0Append(nof0PointerLocations, outBytes.Count + 0x10, model.vsetList[vsetId].vtxlCount));
                    vsetBonePaletteOffsets.Add(NOF0Append(nof0PointerLocations, outBytes.Count + 0x1C, model.vsetList[vsetId].bonePaletteCount));
                    vsetEdgeVertOffsets.Add(NOF0Append(nof0PointerLocations, outBytes.Count + 0x30, model.vsetList[vsetId].edgeVertsCount));

                    outBytes.AddRange(ConvertStruct(model.vsetList[vsetId]));
                }
                AlignWriter(outBytes, 0x10);

                //VTXE + VTXL
                for (int vertListId = 0; vertListId < model.vtxlList.Count; vertListId++)
                {
                    //Write VTXE pointer
                    SetByteListInt(outBytes, vsetVtxeOffsets[vertListId], outBytes.Count);
                    //Write current VTXE array
                    for (int vtxeId = 0; vtxeId < model.vtxeList[vertListId].vertDataTypes.Count; vtxeId++)
                    {
                        outBytes.AddRange(ConvertStruct(model.vtxeList[vertListId].vertDataTypes[vtxeId]));
                    }

                    //Write VTXL pointer
                    SetByteListInt(outBytes, vsetVtxlOffsets[vertListId], outBytes.Count);
                    //Write current VTXL array
                    WriteVTXL(model.vtxeList[vertListId], model.vtxlList[vertListId], outBytes);
                    AlignWriter(outBytes, 0x10);

                    if (model.vtxlList[vertListId].bonePalette != null)
                    {
                        //Write bone palette pointer
                        SetByteListInt(outBytes, vsetBonePaletteOffsets[vertListId], outBytes.Count);
                        //Write bone palette
                        for (int bpId = 0; bpId < model.vtxlList[vertListId].bonePalette.Count; bpId++)
                        {
                            outBytes.AddRange(BitConverter.GetBytes(model.vtxlList[vertListId].bonePalette[bpId]));
                        }
                        AlignWriter(outBytes, 0x10);
                    }

                    if (model.vtxlList[vertListId].edgeVerts != null)
                    {
                        //Write edge verts pointer
                        SetByteListInt(outBytes, vsetEdgeVertOffsets[vertListId], outBytes.Count);
                        //Write edge verts
                        for (int evId = 0; evId < model.vtxlList[vertListId].edgeVerts.Count; evId++)
                        {
                            outBytes.AddRange(BitConverter.GetBytes(model.vtxlList[vertListId].edgeVerts[evId]));
                        }
                        AlignWriter(outBytes, 0x10);
                    }
                }

                //PSET

                //Write PSET pointer
                SetByteListInt(outBytes, objcPsetOffset, outBytes.Count);

                //Write PSET
                for (int psetId = 0; psetId < model.psetList.Count; psetId++)
                {
                    psetfirstOffsets.Add(NOF0Append(nof0PointerLocations, outBytes.Count + 0x8));
                    nof0PointerLocations.Add(outBytes.Count + 0x10);
                    outBytes.AddRange(ConvertStruct(model.psetList[psetId]));
                }
                AlignWriter(outBytes, 0x10);

                //Write tristrip data
                for (int stripId = 0; stripId < model.strips.Count; stripId++)
                {
                    SetByteListInt(outBytes, psetfirstOffsets[stripId], outBytes.Count);
                    SetByteListInt(outBytes, psetfirstOffsets[stripId] + 0x8, outBytes.Count + 0x10); //Strip indices offset; always a set distance

                    outBytes.AddRange(BitConverter.GetBytes(model.strips[stripId].triIdCount));
                    outBytes.AddRange(BitConverter.GetBytes(model.strips[stripId].reserve0));
                    outBytes.AddRange(BitConverter.GetBytes(model.strips[stripId].reserve1));
                    outBytes.AddRange(BitConverter.GetBytes(model.strips[stripId].reserve2));

                    for (int faceId = 0; faceId < model.strips[stripId].triStrips.Count; faceId++)
                    {
                        outBytes.AddRange(BitConverter.GetBytes(model.strips[stripId].triStrips[faceId]));
                    }
                    AlignWriter(outBytes, 0x10); //Intentionally aligned inside, unlike the basic PSET array's alignment
                }

                //MESH

                //Write MESH pointer
                SetByteListInt(outBytes, objcMeshOffset, outBytes.Count);

                //Write MESH
                for (int meshId = 0; meshId < model.meshList.Count; meshId++)
                {
                    outBytes.AddRange(ConvertStruct(model.meshList[meshId]));
                }

                //MATE

                //Write MATE pointer
                SetByteListInt(outBytes, objcMateOffset, outBytes.Count);

                //Write MATE
                for (int mateId = 0; mateId < model.mateList.Count; mateId++)
                {
                    outBytes.AddRange(ConvertStruct(model.mateList[mateId]));
                }
                AlignWriter(outBytes, 0x10);

                //REND

                //Write REND pointer
                SetByteListInt(outBytes, objcRendOffset, outBytes.Count);

                //Write REND
                for (int rendId = 0; rendId < model.rendList.Count; rendId++)
                {
                    outBytes.AddRange(ConvertStruct(model.rendList[rendId]));
                }
                AlignWriter(outBytes, 0x10);

                //SHAD

                //Write SHAD pointer
                SetByteListInt(outBytes, objcShadOffset, outBytes.Count);

                //Write SHAD
                for (int shadId = 0; shadId < model.shadList.Count; shadId++)
                {
                    outBytes.AddRange(BitConverter.GetBytes(model.shadList[shadId].unk0));
                    outBytes.AddRange(model.shadList[shadId].pixelShader.GetBytes());
                    outBytes.AddRange(model.shadList[shadId].vertexShader.GetBytes());

                    outBytes.AddRange(BitConverter.GetBytes(model.shadList[shadId].shadDetailOffset));
                    outBytes.AddRange(BitConverter.GetBytes(model.shadList[shadId].shadExtraOffset));
                }
                AlignWriter(outBytes, 0x10);

                //TSTA
                if (model.tstaList.Count > 0)
                {
                    //Write TSTA pointer
                    SetByteListInt(outBytes, objcTstaOffset, outBytes.Count);

                    //Write TSTA
                    for (int tstaId = 0; tstaId < model.tstaList.Count; tstaId++)
                    {
                        outBytes.AddRange(ConvertStruct(model.tstaList[tstaId]));
                    }
                    AlignWriter(outBytes, 0x10);
                }

                //TSET

                //Write TSET pointer
                SetByteListInt(outBytes, objcTsetOffset, outBytes.Count);

                //Write TSET
                for (int tsetId = 0; tsetId < model.tsetList.Count; tsetId++)
                {
                    outBytes.AddRange(BitConverter.GetBytes(model.tsetList[tsetId].unkInt0));
                    outBytes.AddRange(BitConverter.GetBytes(model.tsetList[tsetId].texCount));
                    outBytes.AddRange(BitConverter.GetBytes(model.tsetList[tsetId].unkInt1));
                    outBytes.AddRange(BitConverter.GetBytes(model.tsetList[tsetId].unkInt2));

                    outBytes.AddRange(BitConverter.GetBytes(model.tsetList[tsetId].unkInt3));

                    //Do as bytes for greater than 4, ints if 4 or less. Remainder of the 0x10 area should be 0xFF.
                    if (model.tsetList[tsetId].texCount > 4)
                    {
                        for (int i = 0; i < model.tsetList[tsetId].texCount; i++)
                        {
                            outBytes.Add((byte)model.tsetList[tsetId].tstaTexIDs[i]);
                        }
                        for (int i = 0; i < (0x10 - model.tsetList[tsetId].texCount); i++)
                        {
                            outBytes.Add(0xFF);
                        }
                    } else
                    {
                        for (int i = 0; i < model.tsetList[tsetId].texCount; i++)
                        {
                            outBytes.AddRange(BitConverter.GetBytes(model.tsetList[tsetId].tstaTexIDs[i]));
                        }
                        for (int i = 0; i < (0x10 - (model.tsetList[tsetId].texCount * sizeof(int))); i++)
                        {
                            outBytes.Add(0xFF);
                        }
                    }
                }
                AlignWriter(outBytes, 0x10);

                //TEXF
                if (model.tstaList.Count > 0)
                {
                    //Write TEXF pointer
                    SetByteListInt(outBytes, objcTexfOffset, outBytes.Count);

                    //Write TEXF
                    for (int texfId = 0; texfId < model.texfList.Count; texfId++)
                    {
                        outBytes.AddRange(ConvertStruct(model.texfList[texfId]));
                    }
                    AlignWriter(outBytes, 0x10);
                }

                //UNRM
                if (model.unrms != null)
                {
                    int meshIDPointerOffset = 0;
                    int vertIDPointerOffset = 0;

                    //Write UNRM pointer
                    SetByteListInt(outBytes, objcUnrmOffset, outBytes.Count);

                    //Write UNRM
                    outBytes.AddRange(BitConverter.GetBytes(model.unrms.vertGroupCountCount));
                    NOF0Append(nof0PointerLocations, outBytes.Count, 1);
                    outBytes.AddRange(BitConverter.GetBytes(outBytes.Count + 0x1C)); //Should always start a set amount after here
                    outBytes.AddRange(BitConverter.GetBytes(model.unrms.vertCount));
                    meshIDPointerOffset = NOF0Append(nof0PointerLocations, outBytes.Count, 1);
                    outBytes.AddRange(BitConverter.GetBytes(model.unrms.meshIdOffset));
                    vertIDPointerOffset = NOF0Append(nof0PointerLocations, outBytes.Count, 1);
                    outBytes.AddRange(BitConverter.GetBytes(model.unrms.vertIDOffset));
                    outBytes.AddRange(BitConverter.GetBytes(model.unrms.padding0));
                    outBytes.AddRange(BitConverter.GetBytes(model.unrms.padding1));

                    //Write group counts
                    for (int i = 0; i < model.unrms.unrmVertGroups.Count; i++)
                    {
                        outBytes.AddRange(BitConverter.GetBytes(model.unrms.unrmVertGroups[i]));
                    }
                    AlignWriter(outBytes, 0x10);

                    //Write Mesh Ids
                    SetByteListInt(outBytes, meshIDPointerOffset, outBytes.Count);
                    for (int i = 0; i < model.unrms.unrmMeshIds.Count; i++)
                    {
                        for (int j = 0; j < model.unrms.unrmMeshIds[i].Count; j++)
                        {
                            outBytes.AddRange(BitConverter.GetBytes(model.unrms.unrmMeshIds[i][j]));
                        }
                    }
                    AlignWriter(outBytes, 0x10);

                    //Write Vert Ids
                    SetByteListInt(outBytes, vertIDPointerOffset, outBytes.Count);
                    for (int i = 0; i < model.unrms.unrmMeshIds.Count; i++)
                    {
                        for (int j = 0; j < model.unrms.unrmMeshIds[i].Count; j++)
                        {
                            outBytes.AddRange(BitConverter.GetBytes(model.unrms.unrmVertIds[i][j]));
                        }
                    }
                    AlignWriter(outBytes, 0x10);
                }

                //Write REL0 Size
                SetByteListInt(outBytes, rel0SizeOffset, outBytes.Count - 0x8);

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
                NOF0FullSize += AlignWriter(outBytes, 0x10);

                //NEND
                outBytes.AddRange(Encoding.UTF8.GetBytes("NEND"));
                outBytes.AddRange(BitConverter.GetBytes(0x8));
                outBytes.AddRange(BitConverter.GetBytes(0));
                outBytes.AddRange(BitConverter.GetBytes(0));

                //Generate NIFL
                AquaCommon.NIFL nifl = new AquaCommon.NIFL();
                nifl.magic = BitConverter.ToInt32(Encoding.UTF8.GetBytes("NIFL"), 0);
                nifl.NIFLLength = 0x18;
                nifl.unkInt0 = 1;
                nifl.offsetAddition = 0x20;

                nifl.NOF0Offset = NOF0Offset;
                nifl.NOF0OffsetFull = NOF0Offset + 0x20;
                nifl.NOF0BlockSize = NOF0FullSize;
                nifl.padding0 = 0;

                //Write NIFL
                outBytes.InsertRange(0, ConvertStruct(nifl));
                AlignFileEndWrite(outBytes, 0x10);

                //Write AFP Base
                int size = outBytes.Count - 0x10; //Size is 0x10 less than what it would be for VTBF afp headers for some reason
                WriteAFPBase(ogFileName, package, modelId, bonusBytes, outBytes, size);

                finalOutBytes.AddRange(outBytes);
            }
            WriteTPN(package, finalOutBytes);

            File.WriteAllBytes(outFileName, finalOutBytes.ToArray());
        }

        public static void WriteBones(string outFileName, AquaNode bones)
        {
            List<byte> outBytes = new List<byte>();
            List<int> nof0PointerLocations = new List<int>(); //Used for the NOF0 section
            int rel0SizeOffset;
            int nodeOffset = NOF0Append(nof0PointerLocations, outBytes.Count + 0x14, bones.nodeList.Count);
            int effOffset = NOF0Append(nof0PointerLocations, outBytes.Count + 0x24, bones.nodoList.Count);

            //REL0
            outBytes.AddRange(Encoding.UTF8.GetBytes("REL0"));
            rel0SizeOffset = outBytes.Count; //We'll fill this later
            outBytes.AddRange(BitConverter.GetBytes(0));
            outBytes.AddRange(BitConverter.GetBytes(0x10));
            outBytes.AddRange(BitConverter.GetBytes(0));

            outBytes.AddRange(ConvertStruct(bones.ndtr));
            //Write nodes
            if(bones.nodeList.Count > 0)
            {
                SetByteListInt(outBytes, nodeOffset, outBytes.Count);
                for (int i = 0; i < bones.nodeList.Count; i++)
                {
                    outBytes.AddRange(ConvertStruct(bones.nodeList[i]));
                }
            }

            //Write effect nodes
            if (bones.nodoList.Count > 0)
            {
                SetByteListInt(outBytes, effOffset, outBytes.Count);
                for (int i = 0; i < bones.nodoList.Count; i++)
                {
                    outBytes.AddRange(ConvertStruct(bones.nodoList[i]));
                }
            }

            //Write REL0 Size
            SetByteListInt(outBytes, rel0SizeOffset, outBytes.Count - 0x8);

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
            NOF0FullSize += AlignWriter(outBytes, 0x10);

            //NEND
            outBytes.AddRange(Encoding.UTF8.GetBytes("NEND"));
            outBytes.AddRange(BitConverter.GetBytes(0x8));
            outBytes.AddRange(BitConverter.GetBytes(0));
            outBytes.AddRange(BitConverter.GetBytes(0));

            //Generate NIFL
            AquaCommon.NIFL nifl = new AquaCommon.NIFL();
            nifl.magic = BitConverter.ToInt32(Encoding.UTF8.GetBytes("NIFL"), 0);
            nifl.NIFLLength = 0x18;
            nifl.unkInt0 = 1;
            nifl.offsetAddition = 0x20;

            nifl.NOF0Offset = NOF0Offset;
            nifl.NOF0OffsetFull = NOF0Offset + 0x20;
            nifl.NOF0BlockSize = NOF0FullSize;
            nifl.padding0 = 0;

            //Write NIFL
            outBytes.InsertRange(0, ConvertStruct(nifl));

            File.WriteAllBytes(outFileName, outBytes.ToArray());
        }

        public void ReadBones(string inFilename)
        {
            using (Stream stream = (Stream)new FileStream(inFilename, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                string type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
                int offset = 0x20; //Base offset due to NIFL header

                //Deal with deicer's extra header nonsense
                if (type.Equals("aqn\0") || type.Equals("trn\0"))
                {
                    streamReader.Seek(0xC, SeekOrigin.Begin);
                    //Basically always 0x60, but some deicer files from the Alpha have 0x50... 
                    int headJunkSize = streamReader.Read<int>();

                    streamReader.Seek(headJunkSize - 0x10, SeekOrigin.Current);
                    type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
                    offset += headJunkSize;
                }

                //Proceed based on file variant
                if (type.Equals("NIFL"))
                {
                    aquaBones.Add(ReadNIFLBones(streamReader));
                }
                else if (type.Equals("VTBF"))
                {
                    aquaBones.Add(ReadVTBFBones(streamReader));
                }
                else
                {
                    MessageBox.Show("Improper File Format!");
                }

            }
        }

        public AquaNode ReadNIFLBones(BufferedStreamReader streamReader)
        {
            AquaNode bones = new AquaNode();

            bones.nifl = streamReader.Read<AquaCommon.NIFL>();
            bones.rel0 = streamReader.Read<AquaCommon.REL0>();
            bones.ndtr = streamReader.Read<AquaNode.NDTR>();
            for (int i = 0; i < bones.ndtr.boneCount; i++)
            {
                bones.nodeList.Add(streamReader.Read<AquaNode.NODE>());
            }
            for (int i = 0; i < bones.ndtr.effCount; i++)
            {
                bones.nodoList.Add(streamReader.Read<AquaNode.NODO>());
            }
            bones.nof0 = AquaCommon.readNOF0(streamReader);
            AlignReader(streamReader, 0x10);
            bones.nend = streamReader.Read<AquaCommon.NEND>();

            return bones;
        }

        public AquaNode ReadVTBFBones(BufferedStreamReader streamReader)
        {
            AquaNode bones = new AquaNode();

            int dataEnd = (int)streamReader.BaseStream().Length;

            //Seek past vtbf tag
            streamReader.Seek(0x10, SeekOrigin.Current);          //VTBF + AQGF tags

            while (streamReader.Position() < dataEnd)
            {
                var data = ReadVTBFTag(streamReader, out string tagType, out int ptrCount, out int entryCount);
                switch (tagType)
                {
                    case "ROOT":
                        //We don't do anything with this right now.
                        break;
                    case "NDTR":
                        bones.ndtr = parseNDTR(data);
                        break;
                    case "NODE":
                        bones.nodeList = parseNODE(data);
                        break;
                    case "NODO":
                        bones.nodoList = parseNODO(data);
                        break;
                    default:
                        //Data being null signfies that the last thing read wasn't a proper tag. This should mean the end of the VTBF stream if nothing else.
                        if (data == null)
                        {
                            return bones;
                        }
                        throw new System.Exception($"Unexpected tag at {streamReader.Position().ToString("X")}! {tagType} Please report!");
                }
            }

            return bones;
        }

        public void ReadMotion(string inFilename)
        {
            using (Stream stream = (Stream)new FileStream(inFilename, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                AnimSet set = new AnimSet();
                string type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
                int offset = 0x20; //Base offset due to NIFL header

                //Deal with deicer's extra header nonsense
                if (type.Equals("aqm\0") || type.Equals("aqv\0") || type.Equals("aqw\0") || type.Equals("aqc\0") || type.Equals("trm\0") || type.Equals("trv\0") || type.Equals("trw\0"))
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

                if (set.afp.fileCount == 0)
                {
                    set.afp.fileCount = 1;
                }

                //Proceed based on file variant
                if (type.Equals("NIFL"))
                {
                    set.anims = ReadNIFLMotion(streamReader, set.afp.fileCount, offset);
                    aquaMotions.Add(set);
                }
                else if (type.Equals("VTBF"))
                {
                    set.anims = ReadVTBFMotion(streamReader, set.afp.fileCount, set.afp.afpBase.paddingOffset);
                    aquaMotions.Add(set);
                }
                else
                {
                    MessageBox.Show("Improper File Format!");
                }

            }
        }

        public List<AquaMotion> ReadVTBFMotion(BufferedStreamReader streamReader, int fileCount, int firstFileSize)
        {
            List<AquaMotion> aquaMotions = new List<AquaMotion>();
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

            for (int animIndex = 0; animIndex < fileCount; animIndex++)
            {
                AquaMotion motion = new AquaMotion();

                if (animIndex > 0)
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
                    motion.afp = afp;

                    fileSize = motion.afp.paddingOffset;
                }
                int dataEnd = (int)streamReader.Position() + fileSize;

                //Seek past vtbf tag
                streamReader.Seek(0x10, SeekOrigin.Current);          //VTBF + AQGF tags

                while (streamReader.Position() < dataEnd)
                {
                    var data = ReadVTBFTag(streamReader, out string tagType, out int ptrCount, out int entryCount);
                    switch (tagType)
                    {
                        case "ROOT":
                            //We don't do anything with this right now.
                            break;
                        case "NDMO":
                            //Signifies a 3d motion
                            motion.moHeader = parseNDMO(data);
                            break;
                        case "SPMO":
                            //Signifies a material animation
                            motion.moHeader = parseSPMO(data);
                            break;
                        case "CAMO":
                            //Signifies a camera motion
                            motion.moHeader = parseCAMO(data);
                            break;
                        case "MSEG":
                            //Motion segment - Signifies the start a node's animation data
                            motion.motionKeys.Add(new AquaMotion.KeyData());
                            motion.motionKeys[motion.motionKeys.Count - 1].mseg = parseMSEG(data);
                            break;
                        case "MKEY":
                            //Motion key - These contain frame data for the various animation types and always follow the MSEG for the node they apply to.
                            motion.motionKeys[motion.motionKeys.Count - 1].keyData.Add(parseMKEY(data));
                            break;
                        default:
                            //Data being null signfies that the last thing read wasn't a proper tag. This should mean the end of the VTBF stream if nothing else.
                            if (firstFileSize == 0)
                            {
                                aquaMotions.Add(motion);
                                return aquaMotions;
                            }
                            throw new System.Exception($"Unexpected tag at {streamReader.Position().ToString("X")}! {tagType} Please report!");
                    }
                }

                aquaMotions.Add(motion);
            }

            return aquaMotions;
        }

        public List<AquaMotion> ReadNIFLMotion(BufferedStreamReader streamReader, int fileCount, int offset)
        {
            List<AquaMotion> aquaMotions = new List<AquaMotion>();

            for (int animIndex = 0; animIndex < fileCount; animIndex++)
            {
                AquaMotion motion = new AquaMotion();

                if (animIndex > 0)
                {
                    streamReader.Seek(0x10, SeekOrigin.Current);
                    motion.afp = streamReader.Read<AquaPackage.AFPBase>();
                    offset = (int)streamReader.Position() + 0x20;
                }

                motion.nifl = streamReader.Read<AquaCommon.NIFL>();
                motion.rel0 = streamReader.Read<AquaCommon.REL0>();
                motion.moHeader = streamReader.Read<AquaMotion.MOHeader>();

                //Read MSEG data
                for (int i = 0; i < motion.moHeader.nodeCount; i++)
                {
                    AquaMotion.KeyData data = new AquaMotion.KeyData();
                    data.mseg = streamReader.Read<AquaMotion.MSEG>();
                    motion.motionKeys.Add(data);
                }

                //Read MKEY
                for (int i = 0; i < motion.motionKeys.Count; i++)
                {
                    streamReader.Seek(motion.motionKeys[i].mseg.nodeOffset + offset, SeekOrigin.Begin);
                    for (int j = 0; j < motion.motionKeys[i].mseg.nodeDataCount; j++)
                    {
                        AquaMotion.MKEY mkey = new AquaMotion.MKEY();
                        mkey.keyType = streamReader.Read<int>();
                        mkey.dataType = streamReader.Read<int>();
                        mkey.unkInt0 = streamReader.Read<int>();
                        mkey.keyCount = streamReader.Read<int>();
                        mkey.frameAddress = streamReader.Read<int>();
                        mkey.timeAddress = streamReader.Read<int>();
                        motion.motionKeys[i].keyData.Add(mkey);
                    }
                    //Odd amounts of MKEYs will pad 8 bytes in NIFL

                    //Loop through what was gathered and get the actual data
                    for (int j = 0; j < motion.motionKeys[i].mseg.nodeDataCount; j++)
                    {
                        streamReader.Seek(motion.motionKeys[i].keyData[j].timeAddress + offset, SeekOrigin.Begin);

                        if (motion.motionKeys[i].keyData[j].keyCount > 1)
                        {
                            for (int m = 0; m < motion.motionKeys[i].keyData[j].keyCount; m++)
                            {
                                motion.motionKeys[i].keyData[j].frameTimings.Add(streamReader.Read<ushort>());
                            }
                        }

                        //Stream aligns to 0x10 after timings.
                        streamReader.Seek(motion.motionKeys[i].keyData[j].frameAddress + offset, SeekOrigin.Begin);

                        switch (motion.motionKeys[i].keyData[j].dataType)
                        {
                            //0x1 and 0x3 are Vector4 arrays essentially. 0x1 is seemingly a Vector3 with alignment padding, but could potentially have things.
                            case 0x1:
                            case 0x2:
                            case 0x3:
                                for (int m = 0; m < motion.motionKeys[i].keyData[j].keyCount; m++)
                                {
                                    motion.motionKeys[i].keyData[j].vector4Keys.Add(streamReader.Read<Vector4>());
                                }
                                break;

                            case 0x5:
                                for (int m = 0; m < motion.motionKeys[i].keyData[j].keyCount; m++)
                                {
                                    motion.motionKeys[i].keyData[j].intKeys.Add(streamReader.Read<int>());
                                }
                                break;

                            //0x4 is texture/uv related, 0x6 is Camera related - Array of floats. 0x4 seems to be used for every .aqv frame set interestingly
                            case 0x4:
                            case 0x6:
                                for (int m = 0; m < motion.motionKeys[i].keyData[j].keyCount; m++)
                                {
                                    motion.motionKeys[i].keyData[j].floatKeys.Add(streamReader.Read<float>());
                                }
                                break;
                            default:
                                MessageBox.Show($"Unexpected (keytype {motion.motionKeys[i].keyData[j].keyType.ToString("X")}) type {motion.motionKeys[i].keyData[j].dataType.ToString("X")} at {streamReader.Position().ToString("X")}");
                                throw new Exception();
                        }
                        //Stream aligns to 0x10 again after frames.

                    }
                }
                AlignReader(streamReader, 0x10);
                motion.nof0 = AquaCommon.readNOF0(streamReader);
                AlignReader(streamReader, 0x10);
                motion.nend = streamReader.Read<AquaCommon.NEND>();

                aquaMotions.Add(motion);
            }

            return aquaMotions;
        }

        public void WriteVTBFMotion(string outFileName)
        {
            bool package = outFileName.Contains(".aqw") || outFileName.Contains(".trw"); //If we're doing .aqv/.trv or other motions instead, we write only the first motion and no aqp header
            int motionCount = aquaMotions[0].anims.Count;
            List<byte> finalOutBytes = new List<byte>();
            if (package)
            {
                finalOutBytes.AddRange(new byte[] { 0x61, 0x66, 0x70, 0 });
                finalOutBytes.AddRange(BitConverter.GetBytes(aquaMotions[0].anims.Count));
                finalOutBytes.AddRange(BitConverter.GetBytes((int)0));
                finalOutBytes.AddRange(BitConverter.GetBytes((int)1));
            }
            else
            {
                motionCount = 1;
            }

            for (int i = 0; i < motionCount; i++)
            {
                int bonusBytes = 0;
                if (i == 0)
                {
                    bonusBytes = 0x10;
                }

                List<byte> outBytes = new List<byte>();
                AquaMotion motion = aquaMotions[0].anims[i];
                outBytes.AddRange(toAQGFVTBF());
                outBytes.AddRange(toROOT());
                switch (motion.moHeader.variant)
                {
                    case AquaMotion.stdAnim:
                    case AquaMotion.stdPlayerAnim:
                        outBytes.AddRange(toNDMO(motion.moHeader));
                        break;
                    case AquaMotion.materialAnim:
                        outBytes.AddRange(toSPMO(motion.moHeader));
                        break;
                    case AquaMotion.cameraAnim:
                        outBytes.AddRange(toCAMO(motion.moHeader));
                        break;
                }
                for (int mseg = 0; mseg < motion.motionKeys.Count; mseg++)
                {
                    outBytes.AddRange(toMSEG(motion.motionKeys[mseg].mseg));
                    for (int keys = 0; keys < motion.motionKeys[mseg].keyData.Count; keys++)
                    {
                        outBytes.AddRange(toMKEY(motion.motionKeys[mseg].keyData[keys]));
                    }
                }

                if (package)
                {
                    //Header info
                    int size = outBytes.Count;
                    WriteAFPBase(outFileName, package, i, bonusBytes, outBytes, size);

                    finalOutBytes.AddRange(outBytes);
                    AlignFileEndWrite(finalOutBytes, 0x10);
                }
            }

            File.WriteAllBytes(outFileName, finalOutBytes.ToArray());
        }

        public void WriteNIFLMotion(string outFileName)
        {
            bool package = outFileName.Contains(".aqw") || outFileName.Contains(".trw"); //If we're doing .aqv/.trv or other motions instead, we write only the first motion and no aqp header
            int motionCount = aquaMotions[0].anims.Count;
            List<byte> finalOutBytes = new List<byte>();
            if (package)
            {
                finalOutBytes.AddRange(new byte[] { 0x61, 0x66, 0x70, 0 });
                finalOutBytes.AddRange(BitConverter.GetBytes(aquaMotions[0].anims.Count));
                finalOutBytes.AddRange(BitConverter.GetBytes((int)0));
                finalOutBytes.AddRange(BitConverter.GetBytes((int)1));
            }
            else
            {
                motionCount = 1;
            }

            for (int motionId = 0; motionId < motionCount; motionId++)
            {
                var motion = aquaMotions[0].anims[motionId];
                int bonusBytes = 0;

                int rel0SizeOffset = 0;
                int boneTableOffset = 0;

                List<int> boneOffAddresses = new List<int>();

                List<byte> outBytes = new List<byte>();
                List<int> nof0PointerLocations = new List<int>(); //Used for the NOF0 section

                //REL0
                outBytes.AddRange(Encoding.UTF8.GetBytes("REL0"));
                rel0SizeOffset = outBytes.Count; //We'll fill this later
                outBytes.AddRange(BitConverter.GetBytes(0));
                outBytes.AddRange(BitConverter.GetBytes(0x10));
                outBytes.AddRange(BitConverter.GetBytes(0));

                //MoHeader
                outBytes.AddRange(BitConverter.GetBytes(motion.moHeader.variant));
                outBytes.AddRange(BitConverter.GetBytes(motion.moHeader.loopPoint));
                outBytes.AddRange(BitConverter.GetBytes(motion.moHeader.endFrame));
                outBytes.AddRange(BitConverter.GetBytes(motion.moHeader.frameSpeed));

                outBytes.AddRange(BitConverter.GetBytes((int)0x2));
                outBytes.AddRange(BitConverter.GetBytes(motion.moHeader.nodeCount));
                boneTableOffset = NOF0Append(nof0PointerLocations, outBytes.Count);
                outBytes.AddRange(BitConverter.GetBytes((int)0x50));
                outBytes.AddRange(motion.moHeader.testString.GetBytes());

                //Padding
                outBytes.AddRange(BitConverter.GetBytes((int)0x0));

                //Bonelist
                for (int i = 0; i < motion.motionKeys.Count; i++)
                {
                    boneOffAddresses.Add(NOF0Append(nof0PointerLocations, outBytes.Count + 0x8));
                    outBytes.AddRange(ConvertStruct(motion.motionKeys[i].mseg));
                }

                //BoneAnims
                for (int i = 0; i < motion.motionKeys.Count; i++)
                {
                    int[] dataOffsets = new int[motion.motionKeys[i].keyData.Count];
                    int[] timeOffsets = new int[motion.motionKeys[i].keyData.Count];

                    SetByteListInt(outBytes, boneOffAddresses[i], outBytes.Count);
                    //Write keyset info
                    for (int keySet = 0; keySet < motion.motionKeys[i].keyData.Count; keySet++)
                    {
                        dataOffsets[keySet] = NOF0Append(nof0PointerLocations, outBytes.Count + 0x10);
                        if (motion.motionKeys[i].keyData[keySet].keyCount > 1)
                        {
                            timeOffsets[keySet] = NOF0Append(nof0PointerLocations, outBytes.Count + 0x14);
                        }
                        outBytes.AddRange(BitConverter.GetBytes(motion.motionKeys[i].keyData[keySet].keyType));
                        outBytes.AddRange(BitConverter.GetBytes(motion.motionKeys[i].keyData[keySet].dataType));
                        outBytes.AddRange(BitConverter.GetBytes(motion.motionKeys[i].keyData[keySet].unkInt0));
                        outBytes.AddRange(BitConverter.GetBytes(motion.motionKeys[i].keyData[keySet].keyCount));

                        outBytes.AddRange(BitConverter.GetBytes((int)0));
                        outBytes.AddRange(BitConverter.GetBytes((int)0));
                    }
                    AlignWriter(outBytes, 0x10);

                    //Write timings and frame data
                    for (int keySet = 0; keySet < motion.motionKeys[i].keyData.Count; keySet++)
                    {
                        if (motion.motionKeys[i].keyData[keySet].keyCount > 1)
                        {
                            SetByteListInt(outBytes, timeOffsets[keySet], outBytes.Count);
                            for (int time = 0; time < motion.motionKeys[i].keyData[keySet].keyCount; time++)
                            {
                                //Beginning time should internally be 0x1 while ending time should have a 2 in the ones decimal place
                                outBytes.AddRange(BitConverter.GetBytes(motion.motionKeys[i].keyData[keySet].frameTimings[time]));
                            }
                            AlignWriter(outBytes, 0x10);
                        }

                        SetByteListInt(outBytes, dataOffsets[keySet], outBytes.Count);
                        for (int data = 0; data < motion.motionKeys[i].keyData[keySet].keyCount; data++)
                        {
                            switch (motion.motionKeys[i].keyData[keySet].dataType)
                            {
                                //0x1, 0x2, and 0x3 are Vector4 arrays essentially. 0x1 is seemingly a Vector3 with alignment padding, but could potentially have things.
                                case 0x1:
                                case 0x2:
                                case 0x3:
                                    outBytes.AddRange(ConvertStruct(motion.motionKeys[i].keyData[keySet].vector4Keys[data]));
                                    break;

                                case 0x5:
                                    outBytes.AddRange(ConvertStruct(motion.motionKeys[i].keyData[keySet].intKeys[data]));
                                    break;

                                //0x4 is texture/uv related, 0x6 is Camera related - Array of floats. 0x4 seems to be used for every .aqv frame set interestingly
                                case 0x4:
                                case 0x6:
                                    outBytes.AddRange(ConvertStruct(motion.motionKeys[i].keyData[keySet].floatKeys[data]));
                                    break;
                                default:
                                    throw new Exception($"Unexpected data type {motion.motionKeys[i].keyData[keySet].dataType}!");
                            }
                        }
                        AlignWriter(outBytes, 0x10);
                    }
                }


                //Write REL0 Size
                SetByteListInt(outBytes, rel0SizeOffset, outBytes.Count - 0x8);

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
                NOF0FullSize += AlignWriter(outBytes, 0x10);

                //NEND
                outBytes.AddRange(Encoding.UTF8.GetBytes("NEND"));
                outBytes.AddRange(BitConverter.GetBytes(0x8));
                outBytes.AddRange(BitConverter.GetBytes(0));
                outBytes.AddRange(BitConverter.GetBytes(0));

                //Generate NIFL
                AquaCommon.NIFL nifl = new AquaCommon.NIFL();
                nifl.magic = BitConverter.ToInt32(Encoding.UTF8.GetBytes("NIFL"), 0);
                nifl.NIFLLength = 0x18;
                nifl.unkInt0 = 1;
                nifl.offsetAddition = 0x20;

                nifl.NOF0Offset = NOF0Offset;
                nifl.NOF0OffsetFull = NOF0Offset + 0x20;
                nifl.NOF0BlockSize = NOF0FullSize;
                nifl.padding0 = 0;

                //Write NIFL
                outBytes.InsertRange(0, ConvertStruct(nifl));
                AlignFileEndWrite(outBytes, 0x10);


                if (package)
                {
                    //Write AFP Base
                    int size = outBytes.Count - 0x10; //Size is 0x10 less than what it would be for VTBF afp headers for some reason
                    WriteAFPBase(outFileName, package, motionId, bonusBytes, outBytes, size);

                    finalOutBytes.AddRange(outBytes);
                    AlignFileEndWrite(finalOutBytes, 0x10);
                }

                finalOutBytes.AddRange(outBytes);
            }

            File.WriteAllBytes(outFileName, finalOutBytes.ToArray());
        }

        public void ReadCollision(string inFilename)
        {
            using (Stream stream = (Stream)new FileStream(inFilename, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                tcbModels = new List<TCBTerrainConvex>();
                TCBTerrainConvex tcbModel = new TCBTerrainConvex();
                int type = streamReader.Peek<int>();
                int offset = 0x20; //Base offset due to NIFL header

                //Deal with deicer's extra header nonsense
                if (type.Equals(0x626374))
                {
                    streamReader.Seek(0x60, SeekOrigin.Current);
                    type = streamReader.Peek<int>();
                    offset += 0x60;
                }

                streamReader.Seek(0x28, SeekOrigin.Current);
                int tcbPointer = streamReader.Read<int>() + offset;
                streamReader.Seek(tcbPointer, SeekOrigin.Begin);
                type = streamReader.Peek<int>();

                //Proceed based on file variant
                if (type.Equals(0x626374))
                {
                    tcbModel.tcbInfo = streamReader.Read<TCBTerrainConvex.TCB>();

                    //Read main TCB verts
                    streamReader.Seek(tcbModel.tcbInfo.vertexDataOffset + offset, SeekOrigin.Begin);
                    List<Vector3> verts = new List<Vector3>();
                    for (int i = 0; i < tcbModel.tcbInfo.vertexCount; i++)
                    {
                        verts.Add(streamReader.Read<Vector3>());
                    }
                    tcbModel.vertices = verts;

                    //Read main TCB faces
                    streamReader.Seek(tcbModel.tcbInfo.faceDataOffset + offset, SeekOrigin.Begin);
                    List<TCBTerrainConvex.TCBFace> faces = new List<TCBTerrainConvex.TCBFace>();
                    for (int i = 0; i < tcbModel.tcbInfo.faceCount; i++)
                    {
                        faces.Add(streamReader.Read<TCBTerrainConvex.TCBFace>());
                    }
                    tcbModel.faces = faces;

                    //Read main TCB materials

                    tcbModels.Add(tcbModel);
                }
                else
                {
                    MessageBox.Show("Improper File Format!");
                }

            }
        }

        //tcbModel components should be written before this
        public void WriteCollision(string outFilename)
        {
            //int offset = 0x20; Needed for NXSMesh part
            TCBTerrainConvex tcbModel = tcbModels[0];
            List<byte> outBytes = new List<byte>();

            //Initial tcb section setup
            tcbModel.tcbInfo = new TCBTerrainConvex.TCB();
            tcbModel.tcbInfo.magic = 0x626374;
            tcbModel.tcbInfo.flag0 = 0xD;
            tcbModel.tcbInfo.flag1 = 0x1;
            tcbModel.tcbInfo.flag2 = 0x4;
            tcbModel.tcbInfo.flag3 = 0x3;
            tcbModel.tcbInfo.vertexCount = tcbModel.vertices.Count;
            tcbModel.tcbInfo.rel0DataStart = 0x10;
            tcbModel.tcbInfo.faceCount = tcbModel.faces.Count;
            tcbModel.tcbInfo.materialCount = tcbModel.materials.Count;
            tcbModel.tcbInfo.unkInt3 = 0x1;

            //Data area starts with 0xFFFFFFFF
            for (int i = 0; i < 4; i++) { outBytes.Add(0xFF); }

            //Write vertices
            tcbModel.tcbInfo.vertexDataOffset = outBytes.Count + 0x10;
            for (int i = 0; i < tcbModel.vertices.Count; i++)
            {
                outBytes.AddRange(ConvertStruct(tcbModel.vertices[i]));
            }

            //Write faces
            tcbModel.tcbInfo.faceDataOffset = outBytes.Count + 0x10;
            for (int i = 0; i < tcbModel.faces.Count; i++)
            {
                outBytes.AddRange(ConvertStruct(tcbModel.faces[i]));
            }

            //Write materials
            tcbModel.tcbInfo.materialDataOFfset = outBytes.Count + 0x10;
            for (int i = 0; i < tcbModel.materials.Count; i++)
            {
                outBytes.AddRange(ConvertStruct(tcbModel.materials[i]));
            }

            //Write Nexus Mesh
            tcbModel.tcbInfo.nxsMeshOffset = outBytes.Count + 0x10;
            List<byte> nxsBytes = new List<byte>();
            WriteNXSMesh(nxsBytes);
            tcbModel.tcbInfo.nxsMeshSize = nxsBytes.Count;
            outBytes.AddRange(nxsBytes);

            //Write tcb
            outBytes.AddRange(ConvertStruct(tcbModel.tcbInfo));

            //Write NIFL, REL0, NOF0, NEND
        }

        public void WriteNXSMesh(List<byte> outBytes)
        {
            List<byte> nxsMesh = new List<byte>();



            outBytes.AddRange(nxsMesh);
        }

        public void LoadPRM(string inFilename)
        {
            using (Stream stream = (Stream)new FileStream(inFilename, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                PRMModel prmModel;
                ReadPRM(streamReader, out prmModel);
                if(prmModel == null)
                {
                    return;
                }

                prmModels.Add(prmModel);
            }
        }

        public static void ReadPRM(BufferedStreamReader streamReader, out PRMModel prmModel)
        {
            int offset = 0x0; //No NIFL header
            prmModel = new PRMModel();
            streamReader.Seek(0xC, SeekOrigin.Begin);
            int type = streamReader.Peek<int>();
            string prmText = "prm\0";

            //Deal with deicer's extra header nonsense
            if (type > 0x10)
            {
                ReadIceEnvelope(streamReader, Encoding.UTF8.GetString(BitConverter.GetBytes(type)), ref offset, ref prmText);
            }
            streamReader.Seek(0x0 + offset, SeekOrigin.Begin);

            prmModel.header = streamReader.Read<PRMModel.PRMHeader>();

            int faceCount;
            switch (prmModel.header.entryVersion)
            {
                case 1:
                    for (int i = 0; i < prmModel.header.entryCount; i++)
                    {
                        prmModel.vertices.Add(new PRMModel.PRMVert(streamReader.Read<PRMModel.PRMType01Vert>()));
                    }

                    if (prmModel.header.groupIndexCount > 0)
                    {
                        faceCount = prmModel.header.groupIndexCount / 3;

                        for (int i = 0; i < faceCount; i++)
                        {

                            prmModel.faces.Add(new Vector3(streamReader.Read<ushort>(), streamReader.Read<ushort>(), streamReader.Read<ushort>()));
                        }
                    }
                    else
                    {
                        faceCount = prmModel.header.entryCount;
                        for (int i = 0; i < faceCount; i += 3)
                        {
                            prmModel.faces.Add(new Vector3(i, i + 1, i + 2));
                        }
                    }
                    break;
                case 2:
                    MessageBox.Show("Unimplemented PRM version! Please report if found!");
                    return;
                case 3:
                    for (int i = 0; i < prmModel.header.entryCount; i++)
                    {
                        prmModel.vertices.Add(new PRMModel.PRMVert(streamReader.Read<PRMModel.PRMType03Vert>()));
                    }

                    faceCount = prmModel.header.groupIndexCount / 3;
                    for (int i = 0; i < faceCount; i++)
                    {
                        prmModel.faces.Add(new Vector3(streamReader.Read<ushort>(), streamReader.Read<ushort>(), streamReader.Read<ushort>()));
                    }
                    break;
                case 4:
                    for (int i = 0; i < prmModel.header.entryCount; i++)
                    {
                        prmModel.vertices.Add(new PRMModel.PRMVert(streamReader.Read<PRMModel.PRMType04Vert>()));
                    }

                    faceCount = prmModel.header.groupIndexCount / 3;
                    for (int i = 0; i < faceCount; i++)
                    {
                        prmModel.faces.Add(new Vector3(streamReader.Read<ushort>(), streamReader.Read<ushort>(), streamReader.Read<ushort>()));
                    }
                    break;
                default:
                    MessageBox.Show("Unknown PRM version! Please report!");
                    break;
            }
        }

        //vtxlList data or tempTri vertex data, and temptris are expected to be populated in an AquaObject prior to this process. This should ALWAYS be run before any write attempts.
        //PRM is very simple and can only take in: Vertex positions, vertex normals, vert colors, and 2 UV mappings along with a list of triangles at best. It also expects only one object. 
        //The main purpose of this function is to fix UV and vert color conflicts upon conversion. While you can just do this logic yourself, this will do it for you as needed.
        public void ConvertToPRM()
        {
            //Assemble vtxlList
            if (aquaModels[0].models[0].vtxlList == null || aquaModels[0].models[0].vtxlList.Count == 0)
            {
                VTXLFromFaceVerts(aquaModels[0].models[0]);
            }

            PRMModel prmModel = new PRMModel();
            for (int i = 0; i < aquaModels[0].models[0].vtxlList[0].vertPositions.Count; i++)
            {
                PRMModel.PRMVert prmVert = new PRMModel.PRMVert();

                prmVert.pos = aquaModels[0].models[0].vtxlList[0].vertPositions[i];

                if (aquaModels[0].models[0].vtxlList[0].vertNormals.Count > 0)
                {
                    prmVert.normal = aquaModels[0].models[0].vtxlList[0].vertNormals[i];
                }
                if (aquaModels[0].models[0].vtxlList[0].vertColors.Count > 0)
                {
                    prmVert.color = aquaModels[0].models[0].vtxlList[0].vertColors[i];
                }

                if (aquaModels[0].models[0].vtxlList[0].uv1List.Count > 0)
                {
                    prmVert.uv1 = aquaModels[0].models[0].vtxlList[0].uv1List[i];
                }
                if (aquaModels[0].models[0].vtxlList[0].uv2List.Count > 0)
                {
                    prmVert.uv2 = aquaModels[0].models[0].vtxlList[0].uv2List[i];
                }

                prmModel.vertices.Add(prmVert);
            }
            prmModel.faces = aquaModels[0].models[0].tempTris[0].triList;

            prmModels.Add(prmModel);
        }

        //Version 1 is the most basic and was seen in alpha pso2. Version 3 was used in PSO2 Classic for most of its life. Version 4 was used some in PSO2 and in NGS.
        //Version 1 requires some extra work to convert to since it doesn't have a face array and so is not supported at this time.
        public unsafe void WritePRM(string outFileName, int version)
        {
            var prm = prmModels[0];
            WritePRMToFile(prm, outFileName, version);
        }

        public unsafe static void WritePRMToFile(PRMModel prm, string outFileName, int version)
        {
            List<byte> finalOutBytes = new List<byte>();
            finalOutBytes.AddRange(Encoding.UTF8.GetBytes("prm\0"));
            finalOutBytes.AddRange(BitConverter.GetBytes(prm.vertices.Count));
            switch (version)
            {
                case 1:
                    MessageBox.Show("Version 1 unsupported at this time!");
                    return;
                case 2:
                    MessageBox.Show("Version 2 unsupported at this time!");
                    return;
                case 3:
                case 4:
                    finalOutBytes.AddRange(BitConverter.GetBytes(prm.faces.Count * 3));
                    break;
                default:
                    MessageBox.Show($"Version {version} unsupported at this time!");
                    return;
            }
            finalOutBytes.AddRange(BitConverter.GetBytes(version));

            for (int i = 0; i < prm.vertices.Count; i++)
            {
                switch (version)
                {
                    case 3:
                        var vert3 = prm.vertices[i].GetType03Vert();
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert3.pos.X));
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert3.pos.Y));
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert3.pos.Z));
                        finalOutBytes.Add(vert3.color[0]);
                        finalOutBytes.Add(vert3.color[1]);
                        finalOutBytes.Add(vert3.color[2]);
                        finalOutBytes.Add(vert3.color[3]);
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert3.uv1.X));
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert3.uv1.Y));
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert3.uv2.X));
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert3.uv2.Y));
                        break;
                    case 4:
                        var vert4 = prm.vertices[i].GetType04Vert();
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert4.pos.X));
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert4.pos.Y));
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert4.pos.Z));
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert4.normal.X));
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert4.normal.Y));
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert4.normal.Z));
                        finalOutBytes.Add(vert4.color[0]);
                        finalOutBytes.Add(vert4.color[1]);
                        finalOutBytes.Add(vert4.color[2]);
                        finalOutBytes.Add(vert4.color[3]);
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert4.uv1.X));
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert4.uv1.Y));
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert4.uv2.X));
                        finalOutBytes.AddRange(BitConverter.GetBytes(vert4.uv2.Y));
                        break;
                    default:
                        return;
                }
            }

            for (int i = 0; i < prm.faces.Count; i++)
            {
                finalOutBytes.AddRange(BitConverter.GetBytes((ushort)prm.faces[i].X));
                finalOutBytes.AddRange(BitConverter.GetBytes((ushort)prm.faces[i].Y));
                finalOutBytes.AddRange(BitConverter.GetBytes((ushort)prm.faces[i].Z));
            }

            AlignWriter(finalOutBytes, 0x10); //Should be padded at the end

            File.WriteAllBytes(outFileName, finalOutBytes.ToArray());
        }

        public byte[] returnModelType(string fileName)
        {
            string ext = Path.GetExtension(fileName);
            if (ext.Equals(".aqp") || ext.Equals(".aqo"))
            {
                return new byte[] { 0x61, 0x71, 0x6F, 0 };
            }
            else if (ext.Equals(".trp") || ext.Equals(".tro"))
            {
                return new byte[] { 0x74, 0x72, 0x6F, 0 };
            } else
            {
                throw new Exception("Invalid type");
            }
        }

        public static void AnalyzeVTBF(string fileName)
        {
            using (Stream stream = (Stream)new FileStream(fileName, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                string type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
                int offset = 0x20; //Base offset due to NIFL header

                //Deal with deicer's extra header nonsense
                if (!type.Equals("NIFL") && !type.Equals("VTBF"))
                {
                    streamReader.Seek(0xC, SeekOrigin.Begin);
                    //Basically always 0x60, but some deicer files from the Alpha have 0x50... 
                    int headJunkSize = streamReader.Read<int>();

                    streamReader.Seek(headJunkSize - 0x10, SeekOrigin.Current);
                    type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
                    offset += headJunkSize;
                }

                //Proceed based on file variant
                if (type.Equals("NIFL"))
                {
                    return;
                }
                else if (type.Equals("VTBF"))
                {
                    int dataEnd = (int)streamReader.BaseStream().Length;
                    StringBuilder output = new StringBuilder();
                    Dictionary<string, List<int>> tagTracker = new Dictionary<string, List<int>>();

                    //Seek past vtbf tag
                    streamReader.Seek(0x8, SeekOrigin.Current);          //VTBF 
                    output.AppendLine(Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Read<int>())) + ":"); //Type
                    streamReader.Seek(0x4, SeekOrigin.Current); //0x1 short and 0x4C00 short. Seem to be constants.

                    while (streamReader.Position() < dataEnd)
                    {
                        var data = ReadVTBFTag(streamReader, out string tagType, out int ptrCount, out int entryCount);

                        switch (tagType)
                        {
                            default:
                                //Data being null signfies that the last thing read wasn't a proper tag. This should mean the end of the VTBF stream if nothing else.
                                if (data == null)
                                {
                                    goto FINISH;
                                } else
                                {
                                    if (!tagTracker.ContainsKey(tagType))
                                    {
                                        tagTracker.Add(tagType, new List<int>());
                                    }
                                    output.AppendLine("");
                                    output.AppendLine($"{tagType} Pointers: {ptrCount} # of entries: {entryCount}");

                                    //Loop through data
                                    for (int dictId = 0; dictId < data.Count; dictId++)
                                    {
                                        var dict = data[dictId];

                                        //Many VTBF tags contain arrays of their data, but many don't. If it does we want to count those ids. We store both as a list here either way.
                                        if (data.Count > 1)
                                        {
                                            output.AppendLine("");
                                            output.AppendLine($"Set {dictId}:");
                                        }

                                        //Loop through values
                                        foreach (var pair in dict)
                                        {
                                            if (!tagTracker[tagType].Contains(pair.Key))
                                            {
                                                tagTracker[tagType].Add(pair.Key);
                                            }

                                            output.Append(pair.Key.ToString("X") + ": ");

                                            //VTBF data here can be either an array or a value of an arbitrary type. We check here if we have to iterate or not.
                                            if (pair.Value is System.Collections.ICollection)
                                            {
                                                output.Append("");
                                                string line = "";
                                                dynamic arr = pair.Value;
                                                bool first = true;
                                                foreach (var obj in arr)
                                                {
                                                    if (first == true)
                                                    {
                                                        first = false;
                                                    }
                                                    else
                                                    {
                                                        if (line == "")
                                                        {
                                                            output.AppendLine(",");
                                                        }
                                                        else
                                                        {
                                                            line += ", ";
                                                        }
                                                    }
                                                    if (obj is System.Collections.ICollection)
                                                    {
                                                        line += "<";
                                                        for (int num = 0; num < obj.Length; num++)
                                                        {
                                                            line += obj[num];
                                                            if (num + 1 != obj.Length)
                                                            {
                                                                line += ", ";
                                                            }
                                                        }
                                                        line += ">";
                                                    } else
                                                    {
                                                        line += obj.ToString();
                                                    }

                                                    if (line.Length > 80)
                                                    {
                                                        output.Append(line);
                                                        line = "";
                                                    }
                                                }

                                                //Flush the rest here if we haven't yet
                                                if (line.Length < 80 && line != "")
                                                {
                                                    output.AppendLine(line);
                                                } else if (line == "")
                                                {
                                                    output.AppendLine("");
                                                }

                                                //Handle for potential strings since we can't easily differentiate them (Sometimes they tried to convert unicode to strings and it gets weird)
                                                if (pair.Value is byte[] && ((byte[])pair.Value).Length <= 0x30)
                                                {
                                                    output.AppendLine(pair.Key.ToString("X") + "(string): " + Encoding.UTF8.GetString(((byte[])pair.Value)));
                                                }
                                            } else
                                            {
                                                output.AppendLine(pair.Value.ToString());
                                            }
                                        }
                                    }
                                }
                                break;
                        }
                    }

                FINISH:
                    output.AppendLine("");
                    output.AppendLine("*******************************");
                    output.AppendLine("Sub tag tracking per tag");
                    output.AppendLine("");
                    foreach (var pair in tagTracker)
                    {
                        output.Append(pair.Key + ": ");
                        string line = "";
                        bool first = true;
                        foreach (var num in pair.Value)
                        {
                            if (first == true)
                            {
                                first = false;
                            }
                            else
                            {
                                if (line == "")
                                {
                                    output.AppendLine(",");
                                } else
                                {
                                    line += ", ";
                                }
                            }
                            line += num.ToString("X");

                            if (line.Length > 80)
                            {
                                output.Append(line);
                                line = "";
                            }
                        }
                        //Flush the rest here if we haven't yet
                        if (line.Length < 80 && line != "")
                        {
                            output.AppendLine(line);
                        }
                        else if (line == "")
                        {
                            output.AppendLine("");
                        }

                    }

                    File.WriteAllText(fileName + ".txt", output.ToString());
                }
                else
                {
                    File.WriteAllText(fileName + ".txt", $"{fileName} was not a VTBF...");
                }
            }

        }

        public void LoadCMX(string fileName)
        {
            aquaCMX = ReadCMX(fileName);
        }

        public void LoadPSO2Text(string fileName)
        {
            aquaText = ReadPSO2Text(fileName);
        }

        public static void ConvertPSO2Text(string outName, string fileName)
        {
            WritePSO2Text(outName, fileName);
        }

        public void GenerateCharacterFileList(string pso2_binDir, string outputDirectory)
        {
            OutputCharacterFileList(pso2_binDir, outputDirectory);
        }

        public void ReadItNameCacheAppendix(string fileName)
        {
            using (Stream stream = (Stream)new FileStream(fileName, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                string type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
                int offset = 0x20; //Base offset due to NIFL header

                //Deal with deicer's extra header nonsense
                if (type.Equals("inca"))
                {
                    streamReader.Seek(0xC, SeekOrigin.Begin);
                    //Basically always 0x60, but some deicer files from the Alpha have 0x50... 
                    int headJunkSize = streamReader.Read<int>();

                    streamReader.Seek(headJunkSize - 0x10, SeekOrigin.Current);
                    type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
                    offset += headJunkSize;
                }

                //Proceed based on file variant
                if (type.Equals("NIFL"))
                {
                    //NIFL
                    ReadInca(streamReader, offset, fileName);
                }
                else if (type.Equals("VTBF"))
                {
                    //Should really never be VTBF...
                }
                else
                {
                    MessageBox.Show("Improper File Format!");
                }
            }
            return;
        }

        public void ReadInca(BufferedStreamReader streamReader, int offset, string fileName)
        {
            var nifl = streamReader.Read<AquaCommon.NIFL>();
            var end = nifl.NOF0Offset + offset;
            var rel0 = streamReader.Read<AquaCommon.REL0>();

            streamReader.Seek(rel0.REL0DataStart + offset, SeekOrigin.Begin);
            streamReader.Seek(streamReader.Read<int>() + offset, SeekOrigin.Begin);

            StringBuilder output = new StringBuilder();
            while (true)
            {
                int category = streamReader.Read<int>(); //Category?
                int id = streamReader.Read<int>(); //id
                if (category + id == 0)
                {
                    break;
                }
                int strPointer = streamReader.Read<int>();
                long bookmark = streamReader.Position();
                streamReader.Seek(strPointer + offset, SeekOrigin.Begin);
                output.AppendLine($"{category.ToString("X")} {id.ToString("X")} " + ReadUTF16String(streamReader, end));

                streamReader.Seek(bookmark, SeekOrigin.Begin);
            }

            File.WriteAllText(fileName + ".txt", output.ToString());
        }

        public void ReadMus(string fileName)
        {
            using (Stream stream = (Stream)new FileStream(fileName, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                string type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
                int offset = 0x20; //Base offset due to NIFL header

                //Deal with deicer's extra header nonsense
                if (type.Equals("mus\0"))
                {
                    streamReader.Seek(0xC, SeekOrigin.Begin);
                    //Basically always 0x60, but some deicer files from the Alpha have 0x50... 
                    int headJunkSize = streamReader.Read<int>();

                    streamReader.Seek(headJunkSize - 0x10, SeekOrigin.Current);
                    type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
                    offset += headJunkSize;
                }

                //Proceed based on file variant
                if (type.Equals("NIFL"))
                {
                    //NIFL
                    ReadNIFLMus(streamReader, offset, fileName);
                }
                else if (type.Equals("VTBF"))
                {
                    //Should really never be VTBF...
                }
                else
                {
                    MessageBox.Show("Improper File Format!");
                }
            }
            return;
        }

        public void ReadNIFLMus(BufferedStreamReader streamReader, int offset, string fileName)
        {
            AquaCommon.NIFL nifl = streamReader.Read<AquaCommon.NIFL>();
            AquaCommon.REL0 rel0 = streamReader.Read<AquaCommon.REL0>();

            streamReader.Seek(rel0.REL0DataStart + offset, SeekOrigin.Begin);
            int offsetOffset = streamReader.Read<int>();
            streamReader.Seek(offsetOffset + offset, SeekOrigin.Begin);
        }

        public void ReadEffect(string inFilename)
        {
            using (Stream stream = (Stream)new FileStream(inFilename, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                string type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
                int offset = 0x20; //Base offset due to NIFL header

                //Deal with deicer's extra header nonsense
                if (type.Equals("aqe\0"))
                {
                    streamReader.Seek(0xC, SeekOrigin.Begin);
                    //Basically always 0x60, but some deicer files from the Alpha have 0x50... 
                    int headJunkSize = streamReader.Read<int>();

                    streamReader.Seek(headJunkSize - 0x10, SeekOrigin.Current);
                    type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
                    offset += headJunkSize;
                }

                //Proceed based on file variant
                if (type.Equals("NIFL"))
                {
                    aquaEffect.Add(ReadNIFLEffect(streamReader, offset));
                }
                else if (type.Equals("VTBF"))
                {
                    aquaEffect.Add(ReadVTBFEffect(streamReader));
                }
                else
                {
                    MessageBox.Show("Improper File Format!");
                }

            }
        }
        public void WriteClassicNIFLEffect(string outFileName)
        {
            List<byte> finalOutBytes = new List<byte>();

            int rel0SizeOffset = 0;
            int efctCurvOffset = 0;
            int efctEmitOffset = 0;

            List<int> emitCurvOffsets = new List<int>();
            List<int> emitPtclOffsets = new List<int>();

            List<byte> outBytes = new List<byte>();
            List<int> nof0PointerLocations = new List<int>(); //Used for the NOF0 section

            //REL0
            outBytes.AddRange(Encoding.UTF8.GetBytes("REL0"));
            rel0SizeOffset = outBytes.Count; //We'll fill this later
            outBytes.AddRange(BitConverter.GetBytes(0));
            outBytes.AddRange(BitConverter.GetBytes(0x10));
            outBytes.AddRange(BitConverter.GetBytes(0));

            //EFCT
            efctCurvOffset = NOF0Append(nof0PointerLocations, outBytes.Count + 0x34, aquaEffect[0].efct.efct.curvCount);
            efctEmitOffset = NOF0Append(nof0PointerLocations, outBytes.Count + 0x98, aquaEffect[0].efct.efct.emitCount);

            outBytes.AddRange(ConvertStruct(aquaEffect[0].efct.efct));

            //Write anim
            if (aquaEffect[0].efct.efct.curvCount > 0)
            {
                SetByteListInt(outBytes, efctCurvOffset, outBytes.Count);
                WriteAQEAnim(outBytes, aquaEffect[0].efct, nof0PointerLocations);
            }

            //EMIT
            if (aquaEffect[0].efct.emits.Count > 0)
            {
                SetByteListInt(outBytes, efctEmitOffset, outBytes.Count);
                for (int emit = 0; emit < aquaEffect[0].efct.emits.Count; emit++)
                {
                    emitCurvOffsets.Add(NOF0Append(nof0PointerLocations, outBytes.Count + 0x34, aquaEffect[0].efct.emits[emit].curvs.Count));
                    emitPtclOffsets.Add(NOF0Append(nof0PointerLocations, outBytes.Count + 0xF0, aquaEffect[0].efct.emits[emit].ptcls.Count));
                    outBytes.AddRange(ConvertStruct(aquaEffect[0].efct.emits[emit].emit));
                }

                //The substructs are written after the set so we follow this here too
                for (int emit = 0; emit < aquaEffect[0].efct.emits.Count; emit++)
                {
                    List<int> ptclCurvOffsets = new List<int>();
                    List<int> ptclStringOffsets = new List<int>();

                    //Write anim
                    if (aquaEffect[0].efct.emits[emit].curvs.Count > 0)
                    {
                        SetByteListInt(outBytes, emitCurvOffsets[emit], outBytes.Count);
                        WriteAQEAnim(outBytes, aquaEffect[0].efct.emits[emit], nof0PointerLocations);
                    }

                    //PTCL
                    if (aquaEffect[0].efct.emits[emit].ptcls.Count > 0)
                    {
                        SetByteListInt(outBytes, emitPtclOffsets[emit], outBytes.Count);
                        for (int ptcl = 0; ptcl < aquaEffect[0].efct.emits[emit].ptcls.Count; ptcl++)
                        {
                            ptclStringOffsets.Add(NOF0Append(nof0PointerLocations, outBytes.Count + 0x140, aquaEffect[0].efct.emits[emit].ptcls[ptcl].ptcl.ptclStringsOffset));
                            ptclCurvOffsets.Add(NOF0Append(nof0PointerLocations, outBytes.Count + 0x144, aquaEffect[0].efct.emits[emit].ptcls[ptcl].curvs.Count));
                            outBytes.AddRange(ConvertStruct(aquaEffect[0].efct.emits[emit].ptcls[ptcl].ptcl));
                        }

                        //The substructs are written after the set so we follow this here too
                        for (int ptcl = 0; ptcl < aquaEffect[0].efct.emits[emit].ptcls.Count; ptcl++)
                        {
                            //Write strings
                            if (aquaEffect[0].efct.emits[emit].ptcls[ptcl].ptcl.ptclStringsOffset != 0)
                            {
                                SetByteListInt(outBytes, ptclStringOffsets[ptcl], outBytes.Count);
                                outBytes.AddRange(ConvertStruct(aquaEffect[0].efct.emits[emit].ptcls[ptcl].strings));
                            }

                            //Write anim
                            if (aquaEffect[0].efct.emits[emit].ptcls[ptcl].curvs.Count > 0)
                            {
                                SetByteListInt(outBytes, ptclCurvOffsets[ptcl], outBytes.Count);
                                WriteAQEAnim(outBytes, aquaEffect[0].efct.emits[emit].ptcls[ptcl], nof0PointerLocations);
                            }
                        }

                    }
                }
            }


            //Write REL0 Size
            SetByteListInt(outBytes, rel0SizeOffset, outBytes.Count - 0x8);

            //NOF0
            nof0PointerLocations.Sort();
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
            NOF0FullSize += AlignWriter(outBytes, 0x10);

            //NEND
            outBytes.AddRange(Encoding.UTF8.GetBytes("NEND"));
            outBytes.AddRange(BitConverter.GetBytes(0x8));
            outBytes.AddRange(BitConverter.GetBytes(0));
            outBytes.AddRange(BitConverter.GetBytes(0));

            //Generate NIFL
            AquaCommon.NIFL nifl = new AquaCommon.NIFL();
            nifl.magic = BitConverter.ToInt32(Encoding.UTF8.GetBytes("NIFL"), 0);
            nifl.NIFLLength = 0x18;
            nifl.unkInt0 = 1;
            nifl.offsetAddition = 0x20;

            nifl.NOF0Offset = NOF0Offset;
            nifl.NOF0OffsetFull = NOF0Offset + 0x20;
            nifl.NOF0BlockSize = NOF0FullSize;
            nifl.padding0 = 0;

            //Write NIFL
            outBytes.InsertRange(0, ConvertStruct(nifl));

            finalOutBytes.AddRange(outBytes);

            File.WriteAllBytes(outFileName, finalOutBytes.ToArray());
        }

        public void WriteAQEAnim(List<byte> outBytes, AquaEffect.AnimObject anim, List<int> nof0PointerLocations)
        {
            List<int> keysOffsets = new List<int>();
            List<int> timeOffsets = new List<int>();
            //CURV
            for (int i = 0; i < anim.curvs.Count; i++)
            {

                keysOffsets.Add(NOF0Append(nof0PointerLocations, outBytes.Count + 0x18, anim.curvs[i].curv.keysCount));
                timeOffsets.Add(NOF0Append(nof0PointerLocations, outBytes.Count + 0x20, anim.curvs[i].curv.timeCount));
                outBytes.AddRange(ConvertStruct(anim.curvs[i].curv));
            }
            AlignWriter(outBytes, 0x10);

            //Write substructs
            for (int i = 0; i < anim.curvs.Count; i++)
            {
                List<float> times = new List<float>();
                //KEYS
                if (anim.curvs[i].keys.Count > 0)
                {
                    SetByteListInt(outBytes, keysOffsets[i], outBytes.Count);
                    for (int key = 0; key < anim.curvs[i].keys.Count; key++)
                    {
                        times.Add(anim.curvs[i].keys[key].time);
                        outBytes.AddRange(ConvertStruct(anim.curvs[i].keys[key]));
                    }
                }
                AlignWriter(outBytes, 0x10);

                //NIFL Times
                if (anim.curvs[i].keys.Count > 0)
                {
                    times.Sort();
                    SetByteListInt(outBytes, timeOffsets[i], outBytes.Count);
                    for (int time = 0; time < times.Count; time++)
                    {
                        outBytes.AddRange(BitConverter.GetBytes(times[time]));
                    }
                }
                AlignWriter(outBytes, 0x10);
            }
        }

        public AquaEffect ReadNIFLEffect(BufferedStreamReader streamReader, int offset)
        {
            AquaEffect effect = new AquaEffect();
            effect.nifl = streamReader.Read<AquaCommon.NIFL>();
            effect.rel0 = streamReader.Read<AquaCommon.REL0>();

            var efct = new AquaEffect.EFCTObject();
            efct.efct = streamReader.Read<AquaEffect.EFCT>();
            effect.efct = efct;
            ReadNIFLAQECurves(streamReader, efct, efct.efct.curvCount, efct.efct.curvOffset, offset);

            if (efct.efct.emitCount > 0)
            {
                streamReader.Seek(efct.efct.emitOffset + offset, SeekOrigin.Begin);

                for (int i = 0; i < efct.efct.emitCount; i++)
                {
                    var emit = new AquaEffect.EMITObject();
                    emit.emit = streamReader.Read<AquaEffect.EMIT>();
                    long emitBookMark = streamReader.Position();
                    ReadNIFLAQECurves(streamReader, emit, emit.emit.curvCount, emit.emit.curvOffset, offset);

                    if (emit.emit.ptclCount > 0)
                    {
                        streamReader.Seek(emit.emit.ptclOffset + offset, SeekOrigin.Begin);

                        for (int pt = 0; pt < emit.emit.ptclCount; pt++)
                        {
                            var ptcl = new AquaEffect.PTCLObject();
                            ptcl.ptcl = streamReader.Read<AquaEffect.PTCL>();
                            ReadNIFLAQECurves(streamReader, ptcl, ptcl.ptcl.curvCount, ptcl.ptcl.curvOffset, offset);

                            long bookmark = streamReader.Position();
                            if (ptcl.ptcl.ptclStringsOffset > 0)
                            {
                                streamReader.Seek(ptcl.ptcl.ptclStringsOffset + offset, SeekOrigin.Begin);
                                ptcl.strings = streamReader.Read<AquaEffect.PTCLStrings>();
                            }
                            streamReader.Seek(bookmark, SeekOrigin.Begin);

                            emit.ptcls.Add(ptcl);
                        }
                    }
                    efct.emits.Add(emit);
                    streamReader.Seek(emitBookMark, SeekOrigin.Begin);
                }
            }

            streamReader.Seek(effect.nifl.NOF0Offset + offset, SeekOrigin.Begin);
            effect.nof0 = AquaCommon.readNOF0(streamReader);
            effect.nend = streamReader.Read<AquaCommon.NEND>();

            return effect;
        }

        public void ReadNIFLAQECurves(BufferedStreamReader streamReader, AquaEffect.AnimObject efct, int curvCount, int curvOffset, int offset)
        {
            long bookmark = streamReader.Position();

            if (curvCount > 0)
            {
                streamReader.Seek(curvOffset + offset, SeekOrigin.Begin);
                for (int i = 0; i < curvCount; i++)
                {
                    var curv = new AquaEffect.CURVObject();
                    curv.curv = streamReader.Read<AquaEffect.CURV>();
                    efct.curvs.Add(curv);
                }
                for (int i = 0; i < curvCount; i++)
                {
                    if (efct.curvs[i].curv.keysCount > 0)
                    {
                        streamReader.Seek(efct.curvs[i].curv.keysOffset + offset, SeekOrigin.Begin);
                        for (int key = 0; key < efct.curvs[i].curv.keysCount; key++)
                        {
                            efct.curvs[i].keys.Add(streamReader.Read<AquaEffect.KEYS>());
                        }
                    }
                    if (efct.curvs[i].curv.timeCount > 0)
                    {
                        streamReader.Seek(efct.curvs[i].curv.timeOffset + offset, SeekOrigin.Begin);
                        for (int key = 0; key < efct.curvs[i].curv.timeCount; key++)
                        {
                            efct.curvs[i].times.Add(streamReader.Read<float>());
                        }
                    }
                }
            }

            streamReader.Seek(bookmark, SeekOrigin.Begin);

            return;
        }

        public AquaEffect ReadVTBFEffect(BufferedStreamReader streamReader)
        {
            AquaEffect effect = new AquaEffect();

            int dataEnd = (int)streamReader.BaseStream().Length;

            //Seek past vtbf tag
            streamReader.Seek(0x10, SeekOrigin.Current);          //VTBF + GLIT tags

            var efct = new AquaEffect.EFCTObject();
            AquaEffect.AnimObject obj = new AquaEffect.AnimObject();

            while (streamReader.Position() < dataEnd)
            {
                var data = ReadVTBFTag(streamReader, out string tagType, out int ptrCount, out int entryCount);
                switch (tagType)
                {
                    case "DOC ":
                        break;
                    case "EFCT":
                        efct.efct = parseEFCT(data);
                        break;
                    case "EMIT":
                        obj = parseEMIT(data);
                        efct.emits.Add((AquaEffect.EMITObject)obj);
                        break;
                    case "PTCL":
                        obj = parsePTCL(data);
                        efct.emits[efct.emits.Count - 1].ptcls.Add((AquaEffect.PTCLObject)obj);
                        break;
                    case "ANIM":
                        break;
                    case "CURV":
                        obj.curvs.Add(parseCURV(data));
                        break;
                    case "KEYS":
                        obj.curvs[obj.curvs.Count - 1].keys = parseKEYS(data); //KEYS tags lump all of the keys into one tag
                        break;
                    default:
                        //Data being null signfies that the last thing read wasn't a proper tag. This should mean the end of the VTBF stream if nothing else.
                        if (data == null)
                        {
                            effect.efct = efct;

                            return effect;
                        }
                        throw new System.Exception($"Unexpected tag at {streamReader.Position().ToString("X")}! {tagType} Please report!");
                }
            }
            effect.efct = efct;

            return effect;
        }

        public void ReadNN(string filePath)
        {
            using (Stream stream = (Stream)new FileStream(filePath, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                int dataEnd = (int)streamReader.BaseStream().Length;

                while (streamReader.Position() < dataEnd)
                {
                    string type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Read<int>()));

                    switch(type)
                    {

                        case NNObject.NXIF:
                            streamReader.Seek(streamReader.Read<int>(), SeekOrigin.Current);
                            break;
                        case NNObject.NXTL:

                            break;
                    }
                }
                
            }
        }

        public void ReadSet(string fileName)
        {
            using (Stream stream = (Stream)new FileStream(fileName, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                var end = stream.Length;
                var set = new SetLayout();
                set.fileName = Path.GetFileNameWithoutExtension(fileName);
                set.header = streamReader.Read<SetLayout.SetHeader>();

                //Read strings
                for(int i = 0; i < set.header.entityStringCount; i++)
                {
                    var entityStr = new SetLayout.EntityString();
                    entityStr.size = streamReader.Read<int>();
                    var rawStr = Encoding.UTF8.GetString(streamReader.ReadBytes(streamReader.Position(), entityStr.size - 4));
                    rawStr = rawStr.Remove(rawStr.IndexOf(char.MinValue));

                    //Entity strings are comma delimited
                    var rawArray = rawStr.Split(',');
                    for (int sub = 0; sub < rawArray.Length; sub++)
                    {
                        entityStr.subStrings.Add(rawArray[sub]);
                    }

                    set.entityStrings.Add(entityStr);
                    streamReader.Seek(entityStr.size - 4, SeekOrigin.Current);
                }

                //Read entities
                for(int i = 0; i < set.header.entityCount; i++)
                {
                    var entityStart = streamReader.Position();

                    var entity = new SetLayout.SetEntity();
                    entity.size = streamReader.Read<int>();
                    entity.entity_variant_string0 = streamReader.Read<AquaCommon.PSO2String>();
                    entity.int_str1Sum = streamReader.Read<int>();

                    var strCount = streamReader.Read<int>();
                    entity.entity_variant_string1 = Encoding.UTF8.GetString(streamReader.ReadBytes(streamReader.Position(), strCount));
                    streamReader.Seek(strCount, SeekOrigin.Current);

                    strCount = streamReader.Read<int>();
                    entity.entity_variant_stringJP = Encoding.Unicode.GetString(streamReader.ReadBytes(streamReader.Position(), strCount));
                    streamReader.Seek(strCount, SeekOrigin.Current);

                    entity.subObjectCount = streamReader.Read<int>();

                    int trueCount = entity.subObjectCount;
                    //Gather variables
                    for (int obj = 0; obj < trueCount; obj++)
                    {
                        var type = streamReader.Read<int>();
                        int length; //Used for some types
                        object data;
                        switch(type)
                        {
                            case 0: //Int
                                data = streamReader.Read<int>();
                                break;
                            case 1: //Float
                                data = streamReader.Read<float>();
                                break;
                            case 2: //Utf8 String with size
                                length = streamReader.Read<int>();
                                data = Encoding.UTF8.GetString(streamReader.ReadBytes(streamReader.Position(), length));
                                streamReader.Seek(length, SeekOrigin.Current);
                                break;
                            case 3: //Unicode-16 String with size
                                length = streamReader.Read<int>();
                                data = Encoding.Unicode.GetString(streamReader.ReadBytes(streamReader.Position(), length));
                                streamReader.Seek(length, SeekOrigin.Current);
                                break;
                            case 4: //Null terminated, comma delimited string list
                                string str = Encoding.UTF8.GetString(streamReader.ReadBytes(streamReader.Position(), (int)(end - streamReader.Position()))); //Yeah idk if this has a limit. I tried.
                                length = str.IndexOf(char.MinValue);
                                data = str.Remove(length);
                                streamReader.Seek(length + 1, SeekOrigin.Current);
                                break;
                            default:
                                Console.WriteLine($"Unknown set type: {type} at position {streamReader.Position().ToString("X")}");
                                throw new Exception();
                                break;
                        }

                        //Name is always a utf8 string right after with a predefined length
                        int nameLength = streamReader.Read<int>();
                        string name = Encoding.UTF8.GetString(streamReader.ReadBytes(streamReader.Position(), nameLength));
                        streamReader.Seek(nameLength, SeekOrigin.Current);

                        //Some things can denote further objects
                        if(name == "edit")
                        {
                            trueCount += streamReader.Read<int>();
                        }
                        
                        //I don't know if it's possible for there to be a dupe within these, but if it is, we'll check for it and note it
                        if(entity.variables.ContainsKey(name))
                        {
                            Console.WriteLine($"Duplicate key: {name} at position {streamReader.Position().ToString("X")}");
                            entity.variables.Add(name + $"({obj})", data);
                        } else
                        {
                            entity.variables.Add(name, data);
                        }
                        
                    }
                    set.setEntities.Add(entity);
                    //Make sure we move to the end properly
                    if (streamReader.Position() != entityStart + entity.size)
                    {
                        streamReader.Seek(entityStart + entity.size, SeekOrigin.Begin);
                    }
                }

                aquaSets.Add(set);
            }
        }

        public static CharacterMakingOffsets LoadCMO(string inFilename)
        {
            CharacterMakingOffsets cmo = new CharacterMakingOffsets();

            using (Stream stream = (Stream)new FileStream(inFilename, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                string variant = ReadAquaHeader(streamReader, Path.GetExtension(inFilename), "", out int offset);

                if (variant == "NIFL")
                {
                    var nifl = streamReader.Read<AquaCommon.NIFL>();
                    var rel = streamReader.Read<AquaCommon.REL0>();
                    streamReader.Seek(offset + rel.REL0DataStart, SeekOrigin.Begin);

                    //Read offsets for
                    int count = streamReader.Read<int>();
                    int listOffset = streamReader.Read<int>();
                    streamReader.Seek(offset + listOffset, SeekOrigin.Begin);

                    for(int i = 0; i < count; i++)
                    {
                        cmo.nodeDataInfo.Add(streamReader.Read<CharacterMakingOffsets.NodeDataInfo>());
                    }

                    foreach(var info in cmo.nodeDataInfo)
                    {
                        CharacterMakingOffsets.NodeData data = new CharacterMakingOffsets.NodeData();

                        //Read strings
                        streamReader.Seek(offset + info.strOffsetList, SeekOrigin.Begin);
                        for(int i = 0; i < info.count; i++)
                        {
                            int strOffset = streamReader.Read<int>();
                            long bookmark = streamReader.Position();

                            streamReader.Seek(offset + strOffset, SeekOrigin.Begin);
                            data.nodeStrings.Add(ReadCString(streamReader));
                            streamReader.Seek(bookmark, SeekOrigin.Begin);
                        }

                        //Read data
                        streamReader.Seek(offset + info.vectorListOffset, SeekOrigin.Begin);
                        for(int i = 0; i < info.count; i++)
                        {
                            data.nodeVectors.Add(streamReader.Read<Vector4>());
                        }

                        cmo.nodeData.Add(data);
                    }

#if DEBUG
                    StringBuilder output = new StringBuilder();
                    output.AppendLine("CMO Data");
                    for(int i = 0; i < cmo.nodeData.Count; i++)
                    {
                        var node = cmo.nodeData[i];
                        output.AppendLine($"Set {i + 1}");
                        for(int j = 0; j < node.nodeStrings.Count; j++)
                        {
                            output.AppendLine($"{node.nodeStrings[j]} - {node.nodeVectors[j]}");
                        }

                        output.AppendLine("");
                    }
                    File.WriteAllText(inFilename + "_dump.txt", output.ToString());
#endif 

                    return cmo;
                }
            }

            return null;
        }

        public void ReadBTI(string inFilename)
        {
            aquaMotionConfigs.Add(LoadBTI(inFilename));
        }

        public static AquaBTI_MotionConfig LoadBTI(string inFilename)
        {
            AquaBTI_MotionConfig bti = new AquaBTI_MotionConfig();

            AquaPackage.AFPMain afp = new AquaPackage.AFPMain();
            string ext = Path.GetExtension(inFilename);
            string variant = "";
            int offset;
            if (ext.Length > 4)
            {
                ext = ext.Substring(0, 4);
            }

            using (Stream stream = (Stream)new FileStream(inFilename, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                variant = ReadAquaHeader(streamReader, ext, variant, out offset, afp);

                if (variant == "NIFL")
                {
                    var nifl = streamReader.Read<AquaCommon.NIFL>();
                    var rel = streamReader.Read<AquaCommon.REL0>();
                    streamReader.Seek(offset + rel.REL0DataStart, SeekOrigin.Begin);
                    bti.header = streamReader.Read<AquaBTI_MotionConfig.BTIHeader>();

                    for (int i = 0; i < bti.header.entryCount; i++)
                    {
                        streamReader.Seek(offset + bti.header.entryPtr + AquaBTI_MotionConfig.btiEntrySize * i, SeekOrigin.Begin);

                        AquaBTI_MotionConfig.BTIEntryObject btiEntry = new AquaBTI_MotionConfig.BTIEntryObject();
                        btiEntry.entry = streamReader.Read<AquaBTI_MotionConfig.BTIEntry>();

                        //Get strings
                        streamReader.Seek(offset + btiEntry.entry.additionPtr, SeekOrigin.Begin);
                        btiEntry.addition = ReadCString(streamReader);

                        streamReader.Seek(offset + btiEntry.entry.nodePtr, SeekOrigin.Begin);
                        btiEntry.node = ReadCString(streamReader);

                        streamReader.Seek(offset + btiEntry.entry.unkStringPtr, SeekOrigin.Begin);
                        btiEntry.unkString = ReadCString(streamReader);

                        bti.btiEntries.Add(btiEntry);
                    }
                }
            }

            return bti;
        }

        public static void DumpNOF0(string inFilename)
        {
            AquaPackage.AFPMain afp = new AquaPackage.AFPMain();
            string ext = Path.GetExtension(inFilename);
            string variant = "";
            int offset;
            if (ext.Length > 4)
            {
                ext = ext.Substring(0, 4);
            }

            using (Stream stream = (Stream)new FileStream(inFilename, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                variant = ReadAquaHeader(streamReader, ext, variant, out offset, afp);

                if (variant == "NIFL")
                {
                    var nifl = streamReader.Read<AquaCommon.NIFL>();
                    var rel = streamReader.Read<AquaCommon.REL0>();
                    streamReader.Seek(nifl.NOF0OffsetFull, SeekOrigin.Begin);
                    Trace.WriteLine(streamReader.Position());
                    AlignReader(streamReader, 0x10);
                    var nof0 = AquaCommon.readNOF0(streamReader);

                    List<string> output = new List<string>();
                    output.Add(Path.GetFileName(inFilename));
                    output.Add("");
                    output.Add($"{Encoding.UTF8.GetString(BitConverter.GetBytes(rel.magic))} {rel.REL0Size:X} {rel.REL0DataStart:X} {rel.padding0:X}");
                    output.Add("");
                    output.Add($"{Encoding.UTF8.GetString(BitConverter.GetBytes(nof0.magic))} {nof0.NOF0Size:X} {nof0.NOF0EntryCount:X} {nof0.NOF0DataSizeStart:X}");
                    foreach(var entry in nof0.relAddresses)
                    {
                        streamReader.Seek(entry + offset, SeekOrigin.Begin);
                        int ptr = streamReader.Read<int>();
                        output.Add($"{entry:X} - {ptr:X}");
                    }

                    File.WriteAllLines(inFilename + "_nof0.txt",output);
                }
            }
        }

        public static string ReadAquaHeader(BufferedStreamReader streamReader, string ext, string variant, out int offset, AquaPackage.AFPMain afp = new AquaPackage.AFPMain())
        {
            string type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
            offset = 0x20; //Base offset due to NIFL header

            ReadIceEnvelope(streamReader, ext, ref offset, ref type);

            //Deal with afp header or aqo. prefixing as needed
            if (type.Equals("afp\0"))
            {
                afp = streamReader.Read<AquaPackage.AFPMain>();
                type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
                offset += 0x40;
            }
            else if (type.Equals("aqo\0") || type.Equals("tro\0"))
            {
                streamReader.Seek(0x4, SeekOrigin.Current);
                type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
                offset += 0x4;
            }

            if (afp.fileCount == 0)
            {
                afp.fileCount = 1;
            }

            //Proceed based on file variant
            if (type.Equals("NIFL"))
            {
                variant = "NIFL";
            }
            else if (type.Equals("VTBF"))
            {
                variant = "VTBF";
            }
            else
            {
                MessageBox.Show("Improper File Format!");
            }

            return variant;
        }

        public static void ReadIceEnvelope(BufferedStreamReader streamReader, string ext, ref int offset, ref string type)
        {
            //Deal with deicer's extra header nonsense
            if (type.Equals(ext))
            {
                streamReader.Seek(0xC, SeekOrigin.Begin);
                //Basically always 0x60, but some deicer files from the Alpha have 0x50... 
                int headJunkSize = streamReader.Read<int>();

                streamReader.Seek(headJunkSize - 0x10, SeekOrigin.Current);
                type = Encoding.UTF8.GetString(BitConverter.GetBytes(streamReader.Peek<int>()));
                offset += headJunkSize;
            }
        }
    }
}
 