using AquaModelLibrary.AquaStructs.AquaFigure;
using Reloaded.Memory.Streams;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using static AquaModelLibrary.AquaMethods.AquaGeneralMethods;

namespace AquaModelLibrary.AquaMethods
{
    public static class AquaFigMethods
    {
        public static AquaFigure LoadFig(string inFilename)
        {
            AquaFigure fig = new AquaFigure();

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
                    fig.figHeader = streamReader.Read<AquaFigure.FigHeader>();

                    //Read Attach Transforms
                    if (fig.figHeader.attachTransformPtr > 0x10)
                    {
                        streamReader.Seek(offset + fig.figHeader.attachTransformPtr, SeekOrigin.Begin);
                        for (int i = 0; i < fig.figHeader.attachTransformCount; i++)
                        {
                            int address = streamReader.Read<int>();
                            long bookmark = streamReader.Position();
                            streamReader.Seek(offset + address, SeekOrigin.Begin);
                            fig.attachTransforms.Add(ReadAttachTransform(offset, streamReader));

                            streamReader.Seek(bookmark, SeekOrigin.Begin);
                        }
                    }

                    //Read unk structs
                    if (fig.figHeader.statePtr > 0x10)
                    {
                        streamReader.Seek(offset + fig.figHeader.statePtr, SeekOrigin.Begin);
                        for (int i = 0; i < fig.figHeader.stateCount; i++)
                        {
                            int address = streamReader.Read<int>();
                            long bookmark = streamReader.Position();
                            streamReader.Seek(offset + address, SeekOrigin.Begin);
                            fig.stateStructs.Add(ReadStateStruct(offset, streamReader));

                            streamReader.Seek(bookmark, SeekOrigin.Begin);
                        }
                    }
                }
            }

            return fig;
        }

        private static AquaFigure.AttachTransformObject ReadAttachTransform(int offset, BufferedStreamReader streamReader)
        {
            AquaFigure.AttachTransformObject attach = new AquaFigure.AttachTransformObject();
            attach.attach = streamReader.Read<AquaFigure.AttachTransform>();
            attach.name = GetFigString(streamReader, attach.attach.namePtr, offset);
            attach.unkText = GetFigString(streamReader, attach.attach.unkTextPtr, offset);
            attach.attachNode = GetFigString(streamReader, attach.attach.attachNodePtr, offset);
            return attach;
        }

        private static AquaFigure.StateObjects ReadStateStruct(int offset, BufferedStreamReader streamReader)
        {
            AquaFigure.StateObjects fs1 = new AquaFigure.StateObjects();
            fs1.rawStruct = streamReader.Read<AquaFigure.StateStruct>();
            fs1.text = GetFigString(streamReader, fs1.rawStruct.textPtr, offset);

            fs1.struct0 = ReadFS1UnkStruct0Object(offset, fs1.rawStruct.FS1UnkStruct0Ptr, streamReader);
            fs1.collision = ReadCollisionData(offset, fs1.rawStruct.collisionPtr, streamReader);
            fs1.stateMap = ReadStateMappingObject(offset, fs1.rawStruct.stateMappingPtr, streamReader);

            return fs1;
        }

        private static AquaFigure.FS1UnkStruct0Object ReadFS1UnkStruct0Object(int offset, int address, BufferedStreamReader streamReader)
        {
            if (address <= 0x10)
            {
                return null;
            }
            streamReader.Seek(offset + address, SeekOrigin.Begin);
            AquaFigure.FS1UnkStruct0Object fs1unk0 = new AquaFigure.FS1UnkStruct0Object();
            fs1unk0.fs1struct0 = streamReader.Read<AquaFigure.FS1UnkStruct0>();
            fs1unk0.text0 = GetFigString(streamReader, fs1unk0.fs1struct0.text0Ptr, offset);
            fs1unk0.text1 = GetFigString(streamReader, fs1unk0.fs1struct0.text1Ptr, offset);
            fs1unk0.text2 = GetFigString(streamReader, fs1unk0.fs1struct0.text2Ptr, offset);
            fs1unk0.text3 = GetFigString(streamReader, fs1unk0.fs1struct0.text3Ptr, offset);

            return fs1unk0;
        }

        private static AquaFigure.CollisionContainerObject ReadCollisionData(int offset, int address, BufferedStreamReader streamReader)
        {
            if (address <= 0x10)
            {
                return null;
            }
            streamReader.Seek(offset + address, SeekOrigin.Begin);
            AquaFigure.CollisionContainerObject colContainers = new AquaFigure.CollisionContainerObject();
            colContainers.colContainerStruct = streamReader.Read<AquaFigure.CollisionContainer>();
            colContainers.collisionName = GetFigString(streamReader, colContainers.colContainerStruct.textPtr0, offset);

            streamReader.Seek(offset + colContainers.colContainerStruct.subStructPtr, SeekOrigin.Begin);
            for (int i = 0; i < colContainers.colContainerStruct.subStructCount; i++)
            {
                colContainers.colliderPtrs.Add(streamReader.Read<int>());
            }

            colContainers.colliders = new List<AquaFigure.ColliderObject>();
            foreach (int ptr in colContainers.colliderPtrs)
            {
                streamReader.Seek(offset + ptr, SeekOrigin.Begin);
                AquaFigure.ColliderObject substruct = new AquaFigure.ColliderObject();
                substruct.colStruct = streamReader.Read<AquaFigure.Collider>();
                substruct.name = GetFigString(streamReader, substruct.colStruct.namePtr, offset);
                substruct.text1 = GetFigString(streamReader, substruct.colStruct.text1Ptr, offset);

                colContainers.colliders.Add(substruct);
            }

            return colContainers;
        }

        private static AquaFigure.StateMappingObject ReadStateMappingObject(int offset, int address, BufferedStreamReader streamReader)
        {
            if (address <= 0x10)
            {
                return null;
            }
            streamReader.Seek(offset + address, SeekOrigin.Begin);
            AquaFigure.StateMappingObject smObj = new AquaFigure.StateMappingObject();
            smObj.stateMappingStruct = streamReader.Read<AquaFigure.StateMapping>();
            smObj.name = GetFigString(streamReader, smObj.stateMappingStruct.namePtr, offset);
            smObj.commands = ReadSMCommands(offset, smObj.stateMappingStruct.commandPtr, smObj.stateMappingStruct.commandCount, streamReader);
            smObj.effects = ReadSMEffects(offset, smObj.stateMappingStruct.effectMapPtr, smObj.stateMappingStruct.effectMapCount, streamReader);
            smObj.anims = ReadSMAnims(offset, smObj.stateMappingStruct.animMapPtr, smObj.stateMappingStruct.animMapCount, streamReader);

            return smObj;
        }

        private static List<AquaFigure.CommandObject> ReadSMCommands(int offset, int address, int count, BufferedStreamReader streamReader)
        {
            if (address <= 0x10 || count == 0)
            {
                return null;
            }

            List<int> cmdPtrList = new List<int>();
            streamReader.Seek(offset + address, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                cmdPtrList.Add(streamReader.Read<int>());
            }

            List<AquaFigure.CommandObject> cmdList = new List<AquaFigure.CommandObject>();
            for (int i = 0; i < count; i++)
            {
                streamReader.Seek(offset + cmdPtrList[i], SeekOrigin.Begin);
                AquaFigure.CommandObject cmd = new AquaFigure.CommandObject();
                cmd.cmdStruct = streamReader.Read<AquaFigure.CommandStruct>();
                cmd.type = GetFigString(streamReader, cmd.cmdStruct.typePtr, offset);
                cmd.text1 = GetFigString(streamReader, cmd.cmdStruct.text1Ptr, offset);
                cmd.text2 = GetFigString(streamReader, cmd.cmdStruct.text2Ptr, offset);
                cmd.text3 = GetFigString(streamReader, cmd.cmdStruct.text3Ptr, offset);

                cmdList.Add(cmd);
            }

            return cmdList;
        }

        private static List<AquaFigure.EffectMapObject> ReadSMEffects(int offset, int address, int count, BufferedStreamReader streamReader)
        {
            if (address <= 0x10 || count == 0)
            {
                return null;
            }

            List<AquaFigure.EffectMapObject> cmdList = new List<AquaFigure.EffectMapObject>();
            List<int> effPtrList = new List<int>();
            streamReader.Seek(offset + address, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                effPtrList.Add(streamReader.Read<int>());
            }

            for (int i = 0; i < count; i++)
            {
                streamReader.Seek(offset + effPtrList[i], SeekOrigin.Begin);
                AquaFigure.EffectMapObject effMapObject = new AquaFigure.EffectMapObject();
                effMapObject.type = streamReader.Read<int>();

                //All of these are mapped a bit arbitrarily and need externally defined mappings to be read
                if (FigEffectMapStructs.effectMappings.ContainsKey(effMapObject.type))
                {
                    int[] map = FigEffectMapStructs.effectMappings[effMapObject.type];
                    foreach (int str in map)
                    {
                        switch (str)
                        {
                            case 0:
                                effMapObject.intList.Add(streamReader.Read<int>());
                                break;
                            case 1:
                                effMapObject.fltList.Add(streamReader.Read<float>());
                                break;
                            case 2:
                                int stringPtr = streamReader.Read<int>();
                                string effString = GetFigString(streamReader, stringPtr, offset);
                                effMapObject.strList.Add(effString);
                                break;
                            case 3: //Unsure if an actual type
                                effMapObject.colorList.Add(streamReader.Read<int>());
                                break;
                        }
                    }
                    effMapObject.knownType = true;
                }
                else
                {
                    effMapObject.knownType = false;
                    Debug.WriteLine($"Undefined effect type '{effMapObject.type}' found! Index {i}, address {effPtrList[i]:X}");
                }

                cmdList.Add(effMapObject);
            }

            
            return cmdList;
        }

        private static List<AquaFigure.AnimMapObject> ReadSMAnims(int offset, int address, int count, BufferedStreamReader streamReader)
        {
            if (address <= 0x10 || count == 0)
            {
                return null;
            }

            List<AquaFigure.AnimMapObject> animList = new List<AquaFigure.AnimMapObject>();
            List<int> animPtrList = new List<int>();
            streamReader.Seek(offset + address, SeekOrigin.Begin);
            for (int i = 0; i < count; i++)
            {
                animPtrList.Add(streamReader.Read<int>());
            }

            for (int i = 0; i < count; i++)
            {
                streamReader.Seek(offset + animPtrList[i], SeekOrigin.Begin);
                AquaFigure.AnimMapObject animData = new AquaFigure.AnimMapObject();
                animData.animStruct = streamReader.Read<AquaFigure.AnimMapStruct>();
                animData.name = GetFigString(streamReader, animData.animStruct.namePtr, offset);
                animData.followUp = GetFigString(streamReader, animData.animStruct.followUpPtr, offset);
                animData.type = GetFigString(streamReader, animData.animStruct.typePtr, offset);
                animData.anim = GetFigString(streamReader, animData.animStruct.animPtr, offset);

                List<int> frameInfoPtrs = new List<int>();
                if (animData.animStruct.frameInfoPtr > 0x10 && animData.animStruct.frameInfoPtrCount > 0)
                {
                    streamReader.Seek(offset + animData.animStruct.frameInfoPtr, SeekOrigin.Begin);
                    for (int fi = 0; fi < animData.animStruct.frameInfoPtrCount; fi++)
                    {
                        frameInfoPtrs.Add(streamReader.Read<int>());
                    }

                    for (int fi = 0; fi < animData.animStruct.frameInfoPtrCount; fi++)
                    {
                        streamReader.Seek(offset + frameInfoPtrs[fi], SeekOrigin.Begin);
                        animData.frameInfoList.Add(streamReader.Read<AquaFigure.AnimFrameInfo>());
                    }
                }

                animList.Add(animData);
            }

            return animList;
        }

        public static string GetFigString(BufferedStreamReader streamReader, long address, int offset)
        {
            string str = null;
            if (address > 0x10)
            {
                long bookmark = streamReader.Position();

                streamReader.Seek(offset + address, SeekOrigin.Begin);
                str = ReadCString(streamReader);

                streamReader.Seek(bookmark, SeekOrigin.Begin);
            }

            return str;
        }

        public static StringBuilder CheckFigEffectMaps(string inFilename, List<int> allTypes)
        {
            if (File.ReadAllBytes(inFilename).Length < 0x11)
            {
                return new StringBuilder();
            }
            StringBuilder dump = new StringBuilder();
            dump.AppendLine(inFilename);

            AquaFigure fig = new AquaFigure();

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
                    fig.figHeader = streamReader.Read<AquaFigure.FigHeader>();

                    streamReader.Seek(offset + fig.figHeader.statePtr, SeekOrigin.Begin);
                    for (int i = 0; i < fig.figHeader.stateCount; i++)
                    {
                        int unk1Address = streamReader.Read<int>();
                        long bookmark = streamReader.Position();

                        if (unk1Address != 0x10 && unk1Address != 0)
                        {
                            streamReader.Seek(offset + unk1Address, SeekOrigin.Begin);
                            var unk1 = streamReader.Read<AquaFigure.StateStruct>();

                            if (unk1.stateMappingPtr != 0x10 && unk1.stateMappingPtr != 0)
                            {
                                streamReader.Seek(offset + unk1.stateMappingPtr, SeekOrigin.Begin);
                                var stateMap = streamReader.Read<AquaFigure.StateMapping>();

                                if (stateMap.effectMapPtr != 0x10 && stateMap.effectMapPtr != 0)
                                {
                                    streamReader.Seek(offset + stateMap.effectMapPtr, SeekOrigin.Begin);

                                    for (int j = 0; j < stateMap.effectMapCount; j++)
                                    {
                                        int mapAddress = streamReader.Read<int>();
                                        long bookmark2 = streamReader.Position();

                                        if (mapAddress != 0x10 && mapAddress != 0)
                                        {
                                            streamReader.Seek(offset + mapAddress, SeekOrigin.Begin);
                                            int num = streamReader.Read<int>();
                                            dump.AppendLine(num.ToString() + " " + num.ToString("X"));
                                            if (!allTypes.Contains(num))
                                            {
                                                allTypes.Add(num);
                                            }
                                        }

                                        streamReader.Seek(bookmark2, SeekOrigin.Begin);
                                    }
                                }
                            }
                        }

                        streamReader.Seek(bookmark, SeekOrigin.Begin);
                    }
                }
            }

            return dump;
        }

    }
}
