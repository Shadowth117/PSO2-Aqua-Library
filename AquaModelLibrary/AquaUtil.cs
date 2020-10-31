using Reloaded.Memory.Streams;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using AquaModelLibrary;
using System.Numerics;
using static AquaModelLibrary.AquaMethods.AquaObjectMethods;
using static AquaModelLibrary.AquaMethods.VTBFMethods;
using AquaModelLibrary.OtherStructs;
using System;
using System.Text;
using System.Diagnostics;

namespace AquaLibrary
{
    public class AquaUtil
    {
        public List<TCBTerrainConvex> tcbModels = new List<TCBTerrainConvex>();
        public List<ModelSet> aquaModels = new List<ModelSet>();
        public List<TPNTexturePattern> tpnFiles = new List<TPNTexturePattern>();
        public List<AquaNode> aquaBones = new List<AquaNode>();
        public struct ModelSet
        {
            public AquaPackage.AFPMain afp;
            public List<AquaObject> models;
        }

        public void ReadModel(string inFilename)
        {
            using (Stream stream = (Stream)new FileStream(inFilename, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                ModelSet set = new ModelSet();
                int type = streamReader.Peek<int>();
                int offset = 0x20; //Base offset due to NIFL header

                //Deal with deicer's extra header nonsense
                if (type.Equals(0x707161) || type.Equals(0x707274))
                {
                    streamReader.Seek(0xC, SeekOrigin.Begin);
                    //Basically always 0x60, but some deicer files from the Alpha have 0x50... 
                    int headJunkSize = streamReader.Read<int>();

                    streamReader.Seek(headJunkSize - 0x10, SeekOrigin.Current);
                    type = streamReader.Peek<int>();
                    offset += headJunkSize;
                }

                //Deal with afp header or aqo. prefixing as needed
                if (type.Equals(0x706661) || type.Equals(0x707274))
                {
                    set.afp = streamReader.Read<AquaPackage.AFPMain>();
                    type = streamReader.Peek<int>();
                    offset += 0x40;
                } else if(type.Equals(0x6F7161) || type.Equals(0x6F7274))
                {
                    streamReader.Seek(0x4, SeekOrigin.Current);
                    type = streamReader.Peek<int>();
                    offset += 0x4;
                }

                if(set.afp.fileCount == 0)
                {
                    set.afp.fileCount = 1;
                }

                //Proceed based on file variant
                if (type.Equals(0x4C46494E))
                {
                    set.models = ReadNIFLModel(streamReader, set.afp.fileCount, offset);
                    aquaModels.Add(set);
                } else if (type.Equals(0x46425456))
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
                AquaObject model = new AquaObject();

                if(modelIndex > 0)
                {
                    streamReader.Seek(0x10, SeekOrigin.Current);
                    model.afp = streamReader.Read<AquaPackage.AFPBase>();
                    offset = (int)streamReader.Position() + 0x20;
                }

                int fileMagic = streamReader.Peek<int>();
                if (fileMagic == 0x6E7074)
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
                    model.nifl = streamReader.Read<AquaCommon.NIFL>();
                    model.rel0 = streamReader.Read<AquaCommon.REL0>();
                    model.objc = streamReader.Read<AquaObject.OBJC>();
                    streamReader.Seek(model.objc.vsetOffset + offset, SeekOrigin.Begin);
                    //Read VSETS
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
                        for (int vtxeIndex = 0; vtxeIndex < model.vsetList[vsetIndex].vertTypesCount; vtxeIndex++)
                        {
                            vtxeSet.vertDataTypes.Add(streamReader.Read<AquaObject.VTXEElement>());
                        }
                        model.vtxeList.Add(vtxeSet);

                        streamReader.Seek(model.vsetList[vsetIndex].vtxlOffset + offset, SeekOrigin.Begin);
                        //VTXL
                        AquaObject.VTXL vtxl = new AquaObject.VTXL();
                        ReadVTXL(streamReader, vtxeSet, vtxl, model.vsetList[vsetIndex].vtxlCount, model.vsetList[vsetIndex].vertTypesCount);

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


                    streamReader.Seek(model.objc.psetOffset + offset, SeekOrigin.Begin);
                    //PSET
                    for (int psetIndex = 0; psetIndex < model.objc.psetCount; psetIndex++)
                    {
                        model.psetList.Add(streamReader.Read<AquaObject.PSET>());
                    }
                    //AlignReader(streamReader, 0x10);

                    //Read faces
                    for (int psetIndex = 0; psetIndex < model.objc.psetCount; psetIndex++)
                    {
                        streamReader.Seek(model.psetList[psetIndex].faceCountOffset + offset, SeekOrigin.Begin);
                        AquaObject.stripData stripData = new AquaObject.stripData();
                        stripData.triCount = streamReader.Read<int>();
                        stripData.reserve0 = streamReader.Read<int>();
                        stripData.reserve1 = streamReader.Read<int>();
                        stripData.reserve2 = streamReader.Read<int>();

                        streamReader.Seek(model.psetList[psetIndex].faceOffset + offset, SeekOrigin.Begin);
                        //Read strip vert indices
                        for (int triId = 0; triId < stripData.triCount; triId++)
                        {
                            stripData.triStrips.Add(streamReader.Read<ushort>());
                        }

                        model.strips.Add(stripData);

                        AlignReader(streamReader, 0x10);
                    }

                    streamReader.Seek(model.objc.meshOffset + offset, SeekOrigin.Begin);
                    //MESH
                    for (int meshIndex = 0; meshIndex < model.objc.meshCount; meshIndex++)
                    {
                        model.meshList.Add(streamReader.Read<AquaObject.MESH>());
                    }

                    streamReader.Seek(model.objc.mateOffset + offset, SeekOrigin.Begin);
                    //MATE
                    for (int mateIndex = 0; mateIndex < model.objc.mateCount; mateIndex++)
                    {
                        model.mateList.Add(streamReader.Read<AquaObject.MATE>());
                    }
                    //AlignReader(streamReader, 0x10);

                    streamReader.Seek(model.objc.rendOffset + offset, SeekOrigin.Begin);
                    //REND
                    for (int rendIndex = 0; rendIndex < model.objc.rendCount; rendIndex++)
                    {
                        model.rendList.Add(streamReader.Read<AquaObject.REND>());
                    }
                    //AlignReader(streamReader, 010);

                    streamReader.Seek(model.objc.shadOffset + offset, SeekOrigin.Begin);
                    //SHAD
                    for (int shadIndex = 0; shadIndex < model.objc.shadCount; shadIndex++)
                    {
                        model.shadList.Add(streamReader.Read<AquaObject.SHAD>());
                    }
                    //AlignReader(streamReader, 010);

                    streamReader.Seek(model.objc.tstaOffset + offset, SeekOrigin.Begin);
                    //TSTA
                    for (int tstaIndex = 0; tstaIndex < model.objc.tstaCount; tstaIndex++)
                    {
                        model.tstaList.Add(streamReader.Read<AquaObject.TSTA>());
                    }
                    //AlignReader(streamReader, 010);

                    streamReader.Seek(model.objc.tsetOffset + offset, SeekOrigin.Begin);
                    //TSET
                    for (int tsetIndex = 0; tsetIndex < model.objc.tsetCount; tsetIndex++)
                    {
                        model.tsetList.Add(streamReader.Read<AquaObject.TSET>());
                    }
                    //AlignReader(streamReader, 010);

                    streamReader.Seek(model.objc.texfOffset + offset, SeekOrigin.Begin);
                    //TEXF
                    for (int texfIndex = 0; texfIndex < model.objc.texfCount; texfIndex++)
                    {
                        model.texfList.Add(streamReader.Read<AquaObject.TEXF>());
                    }
                    //AlignReader(streamReader, 0x10);

                    //UNRM
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

                    //NOF0
                    model.nof0 = AquaCommon.readNOF0(streamReader);
                    AlignReader(streamReader, 0x10);

                    //NEND
                    model.nend = streamReader.Read<AquaCommon.NEND>();

                    aquaModels.Add(model);
                }
            }

            return aquaModels;
        }

        public List<AquaObject> ReadVTBFModel(BufferedStreamReader streamReader, int fileCount, int firstFileSize)
        {
            List<AquaObject> aquaModels = new List<AquaObject>();
            int fileSize = firstFileSize; 
            
            //Handle .aqo/tro
            if(fileSize == 0)
            {
                fileSize = (int)streamReader.BaseStream().Length;

                //Handle the weird aqo/tro with aqo. in front of the rest of the file needlessly
                int type = BitConverter.ToInt32(streamReader.ReadBytes(0, 4), 0);
                if (type.Equals(0x6F7161) || type.Equals(0x6F7274))
                {
                    fileSize -= 0x4;
                }
            }

            for(int modelIndex = 0; modelIndex < fileCount; modelIndex++ )
            {
                AquaObject model = new AquaObject();
                TPNTexturePattern tpn = new TPNTexturePattern();
                int objcCount = 0;
                List<List<ushort>> bp = null;
                List<List<ushort>> ev = null;

                if (modelIndex > 0)
                {
                    //There's 0x10 of padding present following the last model if it ends aligned to 0x10 already. Otherwise, padding to alignment.
                    if(streamReader.Position() % 0x10 == 0)
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
                    } else
                    {
                        model.afp = afp;
                    }
                    fileSize = model.afp.paddingOffset;
                }
                int dataEnd = (int)streamReader.Position() + fileSize;

                //TPN files are uncommon, but are sometimes at the end of afp archives. 
                if (tpn.tpnAFPBase.fileTypeCString == 0x6E7074)
                {
                    tpn.header = streamReader.Read<TPNTexturePattern.tpnHeader>();
                    for(int i = 0; i < tpn.header.count; i++)
                    {
                        tpn.texSets.Add(streamReader.Read<TPNTexturePattern.texSet>());
                    }
                    tpnFiles.Add(tpn);
                } else
                {
                    streamReader.Seek(0x10, SeekOrigin.Current); //Skip the header and move to the tags
                    while(streamReader.Position() < dataEnd)
                    {
                        var data = ReadVTBFTag(streamReader, out string tagType, out int entryCount);
                        switch(tagType)
                        {
                            case "ROOT":
                                //We don't do anything with this right now.
                                break;
                            case "OBJC":
                                objcCount++;
                                if(objcCount > 1)
                                {
                                    throw new System.Exception("More than one OBJC! Weird things are going on!");
                                }
                                model.objc = parseOBJC(data);
                                break;
                            case "VSET":
                                model.vsetList = parseVSET(data, out List<List<ushort>> bonePalettes, out List<List<ushort>> edgeVertsLists);
                                bp = bonePalettes;
                                ev = edgeVertsLists;
                                break;
                            case "VTXE":
                                var data2 = ReadVTBFTag(streamReader, out string tagType2, out int entryCount2);
                                if(tagType2 != "VTXL")
                                {
                                    throw new System.Exception("VTXE without paired VTXL! Please report!");
                                }
                                parseVTXE_VTXL(data, data2, out var vtxe, out var vtxl);
                                model.vtxeList.Add(vtxe);
                                model.vtxlList.Add(vtxl);
                                break;
                            case "PSET":
                                parsePSET(data, out var psetList, out var tris);
                                model.psetList = psetList;
                                model.strips = tris;
                                break;
                            case "MESH":
                                model.meshList = parseMESH(data);
                                break;
                            case "MATE":
                                model.mateList = parseMATE(data);
                                break;
                            case "REND":
                                model.rendList = parseREND(data);
                                break;
                            case "SHAD":
                                model.shadList = parseSHAD(data);
                                break;
                            case "TSTA":
                                model.tstaList = parseTSTA(data);
                                break;
                            case "TSET":
                                model.tsetList = parseTSET(data);
                                break;
                            case "TEXF":
                                model.texfList = parseTEXF(data);
                                break;
                            case "UNRM":
                                model.unrms = parseUNRM(data);
                                break;
                            case "SHAP":
                                //Poor SHAP. It's empty and useless. If it's not, we should probably do something though.
                                if(entryCount > 2)
                                {
                                    throw new System.Exception("SHAP has more than 2 entries! Please report!");
                                }
                                break;
                            default:
                                throw new System.Exception($"Unexpected tag at {streamReader.Position().ToString("X")}! {tagType} Please report!");
                        }
                    }
                    //Assign edgeverts and bone palettes. Assign them here in case of weird ordering shenanigans
                    if (bp != null)
                    {
                        for (int i = 0; i < bp.Count; i++)
                        {
                            model.vtxlList[i].bonePalette = bp[i];
                        }
                    }
                    if (ev != null)
                    {
                        for (int i = 0; i < ev.Count; i++)
                        {
                            model.vtxlList[i].edgeVerts = ev[i];
                        }
                    }
                    aquaModels.Add(model);
                }
            }

            return aquaModels;
        }

        public void WriteVTBFModel(string ogFileName, string outFileName)
        {
            List<byte> finalOutBytes = new List<byte>();
            finalOutBytes.AddRange(new byte[] { 0x61, 0x66, 0x70, 0});
            finalOutBytes.AddRange(BitConverter.GetBytes(aquaModels[0].models.Count + tpnFiles.Count));
            finalOutBytes.AddRange(BitConverter.GetBytes((int)0));
            finalOutBytes.AddRange(BitConverter.GetBytes((int)1));

            for(int i = 0; i < aquaModels[0].models.Count; i++)
            {
                int bonusBytes = 0;
                if(i == 0)
                {
                    bonusBytes = 0x10;
                }

                List<byte> outBytes = new List<byte>();

                outBytes.AddRange(toAQGFVTBF());
                outBytes.AddRange(toROOT());
                outBytes.AddRange(toOBJC(aquaModels[0].models[i].objc, aquaModels[0].models[i].unrms != null));
                outBytes.AddRange(toVSETList(aquaModels[0].models[i].vsetList, aquaModels[0].models[i].vtxlList));
                for(int j = 0; j < aquaModels[0].models[i].vtxlList.Count; j++)
                {
                    outBytes.AddRange(toVTXE_VTXL(aquaModels[0].models[i].vtxeList[j], aquaModels[0].models[i].vtxlList[j]));
                }
                outBytes.AddRange(toPSET(aquaModels[0].models[i].psetList, aquaModels[0].models[i].strips));
                outBytes.AddRange(toMESH(aquaModels[0].models[i].meshList));
                outBytes.AddRange(toMATE(aquaModels[0].models[i].mateList));
                outBytes.AddRange(toREND(aquaModels[0].models[i].rendList));
                outBytes.AddRange(toSHAD(aquaModels[0].models[i].shadList)); //Handles SHAP as well
                outBytes.AddRange(toTSTA(aquaModels[0].models[i].tstaList));
                outBytes.AddRange(toTSET(aquaModels[0].models[i].tsetList));
                outBytes.AddRange(toTEXF(aquaModels[0].models[i].texfList));
                if(aquaModels[0].models[i].unrms != null)
                {
                    outBytes.AddRange(toUNRM(aquaModels[0].models[i].unrms));
                }

                //Header info
                int size = outBytes.Count;
                int difference;
                if(size % 0x10 == 0)
                {
                    difference = 0x10;
                } else
                {
                    difference = 0x10 - (size % 0x10);
                }

                //Handle filename text
                byte[] fName = new byte[0x20];
                string modelCount = ".";
                if(aquaModels[0].models.Count > 1)
                {
                    modelCount = $"_l{i + 1}.";
                }
                byte[] outFileNameBytes = Encoding.UTF8.GetBytes(Path.GetFileName(ogFileName).Replace(Path.GetExtension(ogFileName), 
                    modelCount + Encoding.UTF8.GetString(returnModelType(ogFileName))));
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

                AlignVTBFEndWrite(outBytes, 0x10);

                finalOutBytes.AddRange(outBytes);
            }
            
            //Write texture patterns
            for(int i = 0; i < tpnFiles.Count; i++)
            {
                finalOutBytes.AddRange(Reloaded.Memory.Struct.GetBytes(tpnFiles[i].tpnAFPBase));
                finalOutBytes.AddRange(Reloaded.Memory.Struct.GetBytes(tpnFiles[i].header));
                for(int j = 0; j < tpnFiles[i].header.count; j++)
                {
                    if (i > 0)
                    {
                        finalOutBytes.AddRange(BitConverter.GetBytes((double)0.0));
                    }
                    finalOutBytes.AddRange(Reloaded.Memory.Struct.GetBytes(tpnFiles[i].texSets[j]));
                }
                AlignVTBFEndWrite(finalOutBytes, 0x10);
            }

            File.WriteAllBytes(outFileName, finalOutBytes.ToArray());
        }

        public void WriteNIFLModel(string outFileName)
        {

        }

        public void ReadBones(string inFilename)
        {
            using (Stream stream = (Stream)new FileStream(inFilename, FileMode.Open))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                int type = streamReader.Peek<int>();
                int offset = 0x20; //Base offset due to NIFL header

                //Deal with deicer's extra header nonsense
                if (type.Equals(0x6E7161) || type.Equals(0x6E7274))
                {
                    streamReader.Seek(0xC, SeekOrigin.Begin);
                    //Basically always 0x60, but some deicer files from the Alpha have 0x50... 
                    int headJunkSize = streamReader.Read<int>();

                    streamReader.Seek(headJunkSize - 0x10, SeekOrigin.Current);
                    type = streamReader.Peek<int>();
                    offset += headJunkSize;
                }

                //Proceed based on file variant
                if (type.Equals(0x4C46494E))
                {
                    aquaBones.Add(ReadNIFLBones(streamReader));
                }
                else if (type.Equals(0x46425456))
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
            for(int i = 0; i < bones.ndtr.boneCount; i++)
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
            
            //Seek past vtbf tag
            streamReader.Seek(0x10, SeekOrigin.Current);          //VTBF + AQGF + vtc0 tags

            for(int i = 0; i < 4; i++)
            {
                var data = ReadVTBFTag(streamReader, out string tagType, out int entryCount);
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
                        throw new System.Exception($"Unexpected tag at {streamReader.Position().ToString("X")}! {tagType} Please report!");
                }
            }

            return bones;
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
                    for(int i = 0; i < tcbModel.tcbInfo.vertexCount; i++)
                    {
                        verts.Add(streamReader.Read<Vector3>());
                    }

                    //Read main TCB faces
                    streamReader.Seek(tcbModel.tcbInfo.faceDataOffset + offset, SeekOrigin.Begin);
                    List<TCBTerrainConvex.TCBFace> faces = new List<TCBTerrainConvex.TCBFace>();
                    for (int i = 0; i < tcbModel.tcbInfo.faceCount; i++)
                    {
                        faces.Add(streamReader.Read<TCBTerrainConvex.TCBFace>());
                    }

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
            int offset = 0x20;
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
            for(int i = 0; i < tcbModel.vertices.Count; i++)
            {
                outBytes.AddRange(Reloaded.Memory.Struct.GetBytes(tcbModel.vertices[i]));
            }

            //Write faces
            tcbModel.tcbInfo.faceDataOffset = outBytes.Count + 0x10;
            for (int i = 0; i < tcbModel.faces.Count; i++)
            {
                outBytes.AddRange(Reloaded.Memory.Struct.GetBytes(tcbModel.faces[i]));
            }

            //Write materials
            tcbModel.tcbInfo.materialDataOFfset = outBytes.Count + 0x10;
            for(int i = 0; i < tcbModel.materials.Count; i++)
            {
                outBytes.AddRange(Reloaded.Memory.Struct.GetBytes(tcbModel.materials[i]));
            }

            //Write Nexus Mesh
            tcbModel.tcbInfo.nxsMeshOffset = outBytes.Count + 0x10;
            List<byte> nxsBytes = new List<byte>();
            WriteNXSMesh(nxsBytes);
            tcbModel.tcbInfo.nxsMeshSize = nxsBytes.Count;
            outBytes.AddRange(nxsBytes);

            //Write tcb
            outBytes.AddRange(Reloaded.Memory.Struct.GetBytes(tcbModel.tcbInfo));

            //Write NIFL, REL0, NOF0, NEND
        }

        public void WriteNXSMesh(List<byte> outBytes)
        {
            List<byte> nxsMesh = new List<byte>();



            outBytes.AddRange(nxsMesh);
        }

        public byte[] returnModelType(string fileName)
        {
            string ext = Path.GetExtension(fileName);
            if(ext.Equals(".aqp") || ext.Equals(".aqo"))
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

        private static void AlignReader(BufferedStreamReader streamReader, int align)
        {
            //Align to 0x10
            while (streamReader.Position() % align > 0)
            {
                streamReader.Read<byte>();
            }
        }

        public void AlignVTBFEndWrite(List<byte> outBytes, int align)
        {
            if (outBytes.Count % align == 0)
            {
                for (int i = 0; i < 0x10; i++)
                {
                    outBytes.Add(0);
                }
            }
            else
            {
                //Align to 0x10
                while (outBytes.Count % align > 0)
                {
                    outBytes.Add(0);
                }
            }
        }
    }
}
 