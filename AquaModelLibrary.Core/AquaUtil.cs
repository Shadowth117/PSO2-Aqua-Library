using AquaModelLibrary.Core.Utility;
using AquaModelLibrary.Data.PSO2.Aqua;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using System.Text.Json;

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
        public List<AquaEffectReboot> aquaEffectReboot = new List<AquaEffectReboot>();
        public List<AnimSet> aquaMotions = new List<AnimSet>();
        public List<SetLayout> aquaSets = new List<SetLayout>();
        public List<AquaBTI_MotionConfig> aquaMotionConfigs = new List<AquaBTI_MotionConfig>();
        public List<FacialFCL> facials = new List<FacialFCL>();
        public List<AquaFigure> aquaFigures = new List<AquaFigure>();


        public void ReadCollision(byte[] file)
        {
            using (Stream stream = new MemoryStream(file))
            using (var streamReader = new BufferedStreamReader(stream, 8192))
            {
                tcbModels = new List<TCBTerrainConvex>();
                TCBTerrainConvex tcbModel = ReadTCB(streamReader);

                if (tcbModel == null)
                {
                    Debug.WriteLine("Improper File Format!");
                    return;
                }
                tcbModels.Add(tcbModel);
            }
        }

        public static TCBTerrainConvex ReadTCB(BufferedStreamReader streamReader)
        {
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
            }

            return tcbModel;
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

        public void GenerateFileReferenceSheets(string pso2_binDir, string outputDirectory)
        {
            ReferenceGenerator.OutputFileLists(pso2_binDir, outputDirectory);
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
                    Debug.WriteLine("Improper File Format!");
                }
            }
            return;
        }

        public void ReadNIFLMus(BufferedStreamReader streamReader, int offset, string fileName)
        {
            AquaCommon.NIFL nifl = streamReader.Read<AquaCommon.NIFL>();
            AquaCommon.REL0 rel0 = streamReader.Read<AquaCommon.REL0>();

            bool isAlpha = false;
            streamReader.Seek(rel0.REL0DataStart + offset, SeekOrigin.Begin);
            int offsetOffset = streamReader.Read<int>();
            streamReader.Seek(offsetOffset + offset, SeekOrigin.Begin);
            isAlpha = rel0.REL0DataStart == 0x10 && offsetOffset == 0x14;
            //AquaCommon musFile;
            if (isAlpha)
            {
                var mus = new MusicFileAlpha();
                mus.header = streamReader.Read<AquaStructs.MusicFileAlpha.musHeader>();
                for (int i = 0; i < mus.header.sympathyPartCount; i++)
                {
                    mus.parts.Add(streamReader.Read<MusicFileAlpha.sympathyPart>());
                }
            }
            else
            {

            }
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
                    var effect = ReadNIFLEffect(streamReader, offset);
                    if(effect is AquaEffect)
                    {
                        aquaEffect.Add((AquaEffect)effect);
                    } else
                    {
                        aquaEffectReboot.Add((AquaEffectReboot)effect);
                    }
                }
                else if (type.Equals("VTBF"))
                {
                    aquaEffect.Add(ReadVTBFEffect(streamReader));
                }
                else
                {
                    Debug.WriteLine("Improper File Format!");
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

        private void WriteAQEAnim(List<byte> outBytes, AquaEffect.AnimObject anim, List<int> nof0PointerLocations)
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




        public void ConvertToJson(string filePath)
        {
            var ext = Path.GetExtension(filePath);
            JsonSerializerOptions jss = new JsonSerializerOptions() { WriteIndented = true };
            string jsonData = "";
            switch (ext)
            {
                case ".aqo":
                case ".tro":
                case ".aqp":
                case ".trp":
                    ReadModel(filePath);
                    jsonData = JsonSerializer.Serialize(aquaModels[0], jss);
                    if (aquaModels[0].models[0].objc.type > 0xC32)
                    {
                        filePath = filePath.Replace(ext, $".ngs{ext}");
                    }
                    else
                    {
                        filePath = filePath.Replace(ext, $".classic{ext}");
                    }
                    aquaModels.Clear();
                    break;
                case ".aqn":
                case ".trn":
                    ReadBones(filePath);
                    jsonData = JsonSerializer.Serialize(aquaBones[0], jss);
                    aquaBones.Clear();
                    break;
                case ".aqm":
                case ".trm":
                    ReadMotion(filePath);
                    jsonData = JsonSerializer.Serialize(aquaMotions[0], jss);
                    aquaMotions.Clear();
                    break;
                case ".bti":
                    ReadBTI(filePath);
                    jsonData = JsonSerializer.Serialize(aquaMotionConfigs[0], jss);
                    aquaMotionConfigs.Clear();
                    break;
                case ".cmx":
                    var cmx = ReadCMX(filePath);
                    jsonData = JsonSerializer.Serialize(cmx, jss);
                    cmx = null;
                    break;
                case ".text":
                    var text = ReadPSO2Text(filePath);
                    jsonData = JsonSerializer.Serialize(text, jss);
                    text = null;
                    break;
                case ".aqe": //(Classic for now)
                    ReadEffect(filePath);
                    jsonData = JsonSerializer.Serialize(aquaEffect[0], jss);
                    aquaEffect.Clear();
                    break;
            }
            File.WriteAllText(filePath + ".json", jsonData);
        }

        public void ConvertFromJson(string filePath)
        {
            var ogName = filePath.Substring(0, filePath.Length - 5);
            var ext = Path.GetExtension(ogName); //GetFileNameWithoutExtension nixes the .json text
            var jsonData = File.ReadAllText(filePath);

            switch (ext)
            {
                case ".aqo":
                case ".tro":
                case ".aqp":
                case ".trp":
                    ModelSet aqp;
                    if(filePath.Contains(".ngs."))
                    {
                        aqp = JsonSerializer.Deserialize<NGSModelSet>(jsonData).GetModelSet();
                        aquaModels.Add(aqp);
                        WriteNGSNIFLModel(ogName, ogName);
                    } else if(filePath.Contains(".classic."))
                    {
                        aqp = JsonSerializer.Deserialize<ClassicModelSet>(jsonData).GetModelSet();
                        aquaModels.Add(aqp);
                        WriteClassicNIFLModel(ogName, ogName);
                    }
                    break;
                case ".aqn":
                case ".trn":
                    var aqn = JsonSerializer.Deserialize<AquaNode>(jsonData);
                    WriteBones(ogName, aqn);
                    break;
                case ".aqm":
                case ".trm":
                    var aqm = JsonSerializer.Deserialize<AnimSet>(jsonData);
                    aquaMotions.Add(aqm);
                    WriteNIFLMotion(ogName);
                    break;
                case ".bti":
                    var bti = JsonSerializer.Deserialize<AquaBTI_MotionConfig>(jsonData);
                    WriteBTI(bti, ogName);
                    break;
                case ".cmx":
                    var cmx = JsonSerializer.Deserialize<CharacterMakingIndex>(jsonData);
                    WriteCMX(ogName, cmx, 1);
                    break;
                case ".text":
                    var text = JsonSerializer.Deserialize<PSO2Text>(jsonData);
                    WritePSO2TextNIFL(ogName, text);
                    break;
                case ".aqe": //(Classic for now)
                    var aqe = JsonSerializer.Deserialize<AquaEffect>(jsonData);
                    aquaEffect.Add(aqe);
                    WriteClassicNIFLEffect(ogName);
                    aquaEffect.Clear();
                    break;
            }
        }
    }
}
