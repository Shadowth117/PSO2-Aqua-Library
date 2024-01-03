using AquaModelLibrary.Data.PSO2.Aqua.AquaEffectData;
using AquaModelLibrary.Data.PSO2.Aqua.AquaEffectData.Reboot;
using AquaModelLibrary.Helpers.PSO2;
using AquaModelLibrary.Helpers.Readers;

namespace AquaModelLibrary.Data.PSO2.Aqua
{
    //PSO2 Implementation of Glitter particle effect files. Shares striking similarity to Project Diva variations.
    //May be entirely different than the NIFL variation 
    //Seemingly, the file seems to be an efct header followed by an emit nodes, their animations and particle nodes with their animations.
    //There should be at least one EFCT, one EMIT, and one PTCL per file while they must all have ANIMs, null or not.
    public unsafe class AquaEffect : AquaCommon
    {
        public EFCTObject efct = null;
        public EFCTNGSObject efctNGSObj = null;

        public bool IsNGS { get { return rel0.version > 0; } }

        public AquaEffect() { }

        public AquaEffect(BufferedStreamReaderBE<MemoryStream> sr, string _ext)
        {
            Read(sr, _ext);
        }

        public AquaEffect(byte[] file, string _ext)
        {
            Read(file, _ext);
        }

        public override void ReadNIFLFile(BufferedStreamReaderBE<MemoryStream> sr, int offset)
        {
            switch (rel0.version)
            {
                case 0:
                    ReadClassicNIFLEffect(sr, offset);
                    break;
                case 2:
                    ReadRebootNIFLEffect(sr, offset);
                    break;
                default:
                    throw new Exception($"Unknown effect version {rel0.version}");
            }
        }

        /// <summary>
        /// Currently unfinished, needs more research, but the subsequent structs are 0x300 long each and don't match up super well with the old format.
        /// </summary>
        public void ReadRebootNIFLEffect(BufferedStreamReaderBE<MemoryStream> streamReader, int offset)
        {
            efctNGSObj = new EFCTNGSObject();

            efctNGSObj.efctNGS = streamReader.Read<EFCTNGS>();
            streamReader.Seek(offset + efctNGSObj.efctNGS.unkIntArrayOffset, SeekOrigin.Begin);
            for (int i = 0; i < efctNGSObj.efctNGS.unkIntArrayCount; i++)
            {
                efctNGSObj.unkIntArray.Add(streamReader.Read<int>());
            }

            streamReader.Seek(offset + efctNGSObj.efctNGS.offset_04, SeekOrigin.Begin);
            var root = new RootSettingsObject();
            efctNGSObj.root = root;
            root.root = streamReader.Read<RootSettings>();

            streamReader.Seek(offset + root.root.RootSettingsStruct0Ptr, SeekOrigin.Begin);
            for (int i = 0; i < root.root.RootSettingsStruct0Count; i++)
            {
                root.rootSettingStruct0s.Add(streamReader.Read<RootSettingsStruct0>());
            }

            if (root.root.RootSettingsStruct0Count != 2)
            {
                throw new Exception("RootSettingsStruct0Count is not count of 2");
            }

            if (root.root.RootSettingStruct1Count > 1)
            {
                throw new Exception("offset_194Count is greater than 1");
            }

            if (root.root.RootSettingsStruct2Count > 1)
            {
                throw new Exception("offset_194Count is greater than 1");
            }

            if (root.root.offset_320Count != 2)
            {
                throw new Exception("offset_320Count is greater than 1");
            }
        }

        public void ReadClassicNIFLEffect(BufferedStreamReaderBE<MemoryStream> streamReader, int offset)
        {
            efct = new EFCTObject();
            efct.efct = streamReader.Read<EFCT>();
            ReadClassicNIFLAQECurves(streamReader, efct, efct.efct.curvCount, efct.efct.curvOffset, offset);

            if (efct.efct.emitCount > 0)
            {
                streamReader.Seek(efct.efct.emitOffset + offset, SeekOrigin.Begin);

                for (int i = 0; i < efct.efct.emitCount; i++)
                {
                    var emit = new EMITObject();
                    emit.emit = streamReader.Read<EMIT>();
                    long emitBookMark = streamReader.Position;
                    ReadClassicNIFLAQECurves(streamReader, emit, emit.emit.curvCount, emit.emit.curvOffset, offset);

                    if (emit.emit.ptclCount > 0)
                    {
                        streamReader.Seek(emit.emit.ptclOffset + offset, SeekOrigin.Begin);

                        for (int pt = 0; pt < emit.emit.ptclCount; pt++)
                        {
                            var ptcl = new PTCLObject();
                            ptcl.ptcl = streamReader.Read<PTCL>();
                            ReadClassicNIFLAQECurves(streamReader, ptcl, ptcl.ptcl.curvCount, ptcl.ptcl.curvOffset, offset);

                            long bookmark = streamReader.Position;
                            if (ptcl.ptcl.ptclStringsOffset > 0)
                            {
                                streamReader.Seek(ptcl.ptcl.ptclStringsOffset + offset, SeekOrigin.Begin);
                                ptcl.strings = streamReader.Read<PTCLStrings>();
                            }
                            streamReader.Seek(bookmark, SeekOrigin.Begin);

                            emit.ptcls.Add(ptcl);
                        }
                    }
                    efct.emits.Add(emit);
                    streamReader.Seek(emitBookMark, SeekOrigin.Begin);
                }
            }
        }

        public void ReadClassicNIFLAQECurves(BufferedStreamReaderBE<MemoryStream> streamReader, AnimObject efct, int curvCount, int curvOffset, int offset)
        {
            long bookmark = streamReader.Position;

            if (curvCount > 0)
            {
                streamReader.Seek(curvOffset + offset, SeekOrigin.Begin);
                for (int i = 0; i < curvCount; i++)
                {
                    var curv = new CURVObject();
                    curv.curv = streamReader.Read<CURV>();
                    efct.curvs.Add(curv);
                }
                for (int i = 0; i < curvCount; i++)
                {
                    if (efct.curvs[i].curv.keysCount > 0)
                    {
                        streamReader.Seek(efct.curvs[i].curv.keysOffset + offset, SeekOrigin.Begin);
                        for (int key = 0; key < efct.curvs[i].curv.keysCount; key++)
                        {
                            efct.curvs[i].keys.Add(streamReader.Read<KEYS>());
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

        public override void ReadVTBFFile(BufferedStreamReaderBE<MemoryStream> sr)
        {
            int dataEnd = (int)sr.BaseStream.Length;

            //Seek past vtbf tag
            sr.Seek(0x10, SeekOrigin.Current);          //VTBF + GLIT tags

            efct = new EFCTObject();
            AnimObject obj = new AnimObject();

            while (sr.Position < dataEnd)
            {
                var data = VTBFMethods.ReadVTBFTag(sr, out string tagType, out int ptrCount, out int entryCount);
                switch (tagType)
                {
                    case "DOC ":
                        break;
                    case "EFCT":
                        efct.efct = new EFCT(data);
                        break;
                    case "EMIT":
                        obj = new EMITObject(data);
                        efct.emits.Add((EMITObject)obj);
                        break;
                    case "PTCL":
                        obj = new PTCLObject(data);
                        efct.emits[efct.emits.Count - 1].ptcls.Add((PTCLObject)obj);
                        break;
                    case "ANIM":
                        break;
                    case "CURV":
                        obj.curvs.Add(new CURVObject(data));
                        break;
                    case "KEYS":
                        obj.curvs[obj.curvs.Count - 1].keys = parseKEYS(data); //KEYS tags lump all of the keys into one tag
                        break;
                    default:
                        //Data being null signfies that the last thing read wasn't a proper tag. This should mean the end of the VTBF stream if nothing else.
                        return;
                }
            }
        }

        public static List<KEYS> parseKEYS(List<Dictionary<int, object>> keysRaw)
        {
            List<KEYS> keyList = new List<KEYS>();

            for (int i = 0; i < keysRaw.Count; i++)
            {
                var keys = new KEYS();

                keys.type = VTBFMethods.GetObject<byte>(keysRaw[i], 0x72);
                if (keysRaw[i].TryGetValue(0x78, out object value) == true)
                {
                    if (value is float)
                    {
                        keys.time = (float)value;
                    }
                    else if (value is short)
                    {
                        keys.time = (short)value;
                    }
                }
                keys.value = VTBFMethods.GetObject<float>(keysRaw[i], 0x79);
                keys.inParam = VTBFMethods.GetObject<float>(keysRaw[i], 0x7A);

                keys.outParam = VTBFMethods.GetObject<float>(keysRaw[i], 0x7B);

                keyList.Add(keys);
            }

            return keyList;
        }
    }
}
