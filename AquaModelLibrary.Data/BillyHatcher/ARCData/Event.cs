using AquaModelLibrary.Data.Ninja;
using AquaModelLibrary.Helpers.Extensions;
using AquaModelLibrary.Helpers.Readers;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;

namespace AquaModelLibrary.Data.BillyHatcher.ARCData
{
    public class Event : ARC
    {
        public List<EventScriptContainer> eventScriptContainers { get; set; } = new List<EventScriptContainer>();
        public Event() { }
        public Event(byte[] file)
        {
            Read(file);
        }

        public Event(BufferedStreamReaderBE<MemoryStream> sr)
        {
            Read(sr);
        }
        public override void Read(byte[] file)
        {
            using (MemoryStream ms = new MemoryStream(file))
            using (BufferedStreamReaderBE<MemoryStream> sr = new BufferedStreamReaderBE<MemoryStream>(ms))
            {
                Read(sr);
            }
        }

        public override void Read(BufferedStreamReaderBE<MemoryStream> sr)
        {
            sr._BEReadActive = true;
            base.Read(sr);
            sr.Seek(0x20, SeekOrigin.Begin);

            var scriptContainerCount = sr.ReadBE<int>();
            var scriptContainerArrayOffset = sr.ReadBE<int>();

            List<int> scriptRootOffsets = new List<int>();
            for (int i = 0; i < scriptContainerCount; i++)
            {
                scriptRootOffsets.Add(sr.ReadBE<int>());
            }
            for (int i = 0; i < scriptRootOffsets.Count; i++)
            {
                sr.Seek(0x20 + scriptRootOffsets[i], SeekOrigin.Begin);
                eventScriptContainers.Add(new EventScriptContainer()
                {
                    position = sr.Position,
                    int_00 = sr.ReadBE<int>(),
                    scriptValue0 = sr.ReadBE<int>(),
                    int_08 = sr.ReadBE<int>(),
                    scriptCount = sr.ReadBE<int>(),

                    scriptOffsets = sr.ReadBE<int>(),
                });
                if (eventScriptContainers[i].scriptOffsets != 0)
                {
                    sr.Seek(0x20 + eventScriptContainers[i].scriptOffsets, SeekOrigin.Begin);
                    List<int> scriptOffsetList = new List<int>();
                    for (int j = 0; j < eventScriptContainers[i].scriptCount; j++)
                    {
                        scriptOffsetList.Add(sr.ReadBE<int>());
                    }
                    for (int j = 0; j < scriptOffsetList.Count; j++)
                    {
                        if (scriptOffsetList[j] == 0)
                        {
                            continue;
                        }
                        sr.Seek(0x20 + scriptOffsetList[j], SeekOrigin.Begin);
                        eventScriptContainers[i].scripts.Add(new EventScript()
                        {
                            scriptNameOffset = sr.ReadBE<int>(),
                            scriptInt_04 = sr.ReadBE<int>(),
                            scriptDataOffset = sr.ReadBE<int>(),
                        });

                        eventScriptContainers[i].scripts[j].name = sr.ReadCStringValidOffset(eventScriptContainers[i].scripts[j].scriptNameOffset, 0x20);
                        switch (eventScriptContainers[i].scripts[j].name)
                        {
                            //1 int16
                            case "load_racer":
                            case "set_bgm":
                            case "msg":
                            case "bgm_play":
                                eventScriptContainers[i].scripts[j].int0_Data = sr.ReadBE<short>();
                                break;
                            //4 bytes
                            case "gold_save":
                            case "load_p":
                            case "load_prisoner":
                            case "sunrate":
                            case "sw_off":
                            case "sw_on":
                            case "sw_chk_off":
                            case "sw_chk_on":
                                eventScriptContainers[i].scripts[j].bytes_Data = sr.Read4Bytes().ToList();
                                break;
                            //8 bytes
                            case "set":
                                eventScriptContainers[i].scripts[j].bytes_Data = sr.ReadBytesSeek(0x8).ToList();
                                break;
                            //1 int32
                            case "set_rescue":
                            case "prison_save":
                            case "spore":
                            case "hoverleaf":
                            case "blizzard":
                            case "se":
                                eventScriptContainers[i].scripts[j].int0_Data = sr.ReadBE<int>();
                                break;
                            //1 float, 2 ints16s
                            case "bgm_vol":
                                eventScriptContainers[i].scripts[j].flt_Data = sr.ReadBE<float>();
                                eventScriptContainers[i].scripts[j].int0_Data = sr.ReadBE<short>();
                                eventScriptContainers[i].scripts[j].int1_Data = sr.ReadBE<short>();
                                break;
                            //2 int32s
                            case "talk":
                            case "load_stg_title":
                            case "set_talk_mode":
                                eventScriptContainers[i].scripts[j].int0_Data = sr.ReadBE<int>();
                                eventScriptContainers[i].scripts[j].int1_Data = sr.ReadBE<int>();
                                break;
                            //Float Vector3
                            case "sol":
                                eventScriptContainers[i].scripts[j].vec3_Data = new List<float>() { sr.ReadBE<float>(), sr.ReadBE<float>(), sr.ReadBE<float>()};
                                break;
                            //Unique structure for set_race
                            case "set_race":
                                eventScriptContainers[i].scripts[j].race_Data = new RaceData()
                                {
                                    aiSetting = sr.ReadBE<short>(),
                                    sht_02 = sr.ReadBE<short>(),
                                    animalSpeed = sr.ReadBE<float>(),
                                    preRaceMessageId = sr.ReadBE<short>(),
                                    winRaceMessageId = sr.ReadBE<short>(),
                                    loseRaceMessageId = sr.ReadBE<short>(),
                                    sht_0E = sr.ReadBE<short>(),
                                    animalScale = sr.ReadBE<float>(),
                                    animalPosition = new List<float> { sr.ReadBE<float>(), sr.ReadBE<float>(), sr.ReadBE<float>() },
                                };
                                break;
                            //Unique structure for minigame
                            case "minigame":
                                eventScriptContainers[i].scripts[j].minigame_Data = new MinigameData()
                                {
                                    bt_00 = sr.ReadBE<byte>(),
                                    bt_01 = sr.ReadBE<byte>(),
                                    bt_02 = sr.ReadBE<byte>(),
                                    bt_03 = sr.ReadBE<byte>(),

                                    timeInMinutes = sr.ReadBE<byte>(),
                                    timeInSeconds = sr.ReadBE<byte>(),
                                    bt_06 = sr.ReadBE<byte>(),
                                    emblemScoreToWin = sr.ReadBE<byte>(),
                                };
                                break;
                            //No data
                            case "takeoff_suit":
                            case "start_race":
                            case "ms_boot":
                            case "ms_fail":
                            case "ms_success":
                            case "load_green_ene_demo":
                            case "load_o_cannon":
                            case "load_darkgate":
                            case "load_chicken":
                            case "load_goal":
                            case "load_ring":
                            case "load_mgleader":
                            case "load_gold":
                            case "load_bomb":
                            case "load_egg_suit":
                            case "always_true":
                            case "race":
                                break;
                            //Unknown if there's data since unused
                            case "wait_fake":
                            case "ptcl_test":
                            case "send":
                            case "wait":
                            case "bgm_stop":
                            case "se_pos":
                            default:
                                break;
                        }
                    }
                }
            }
        }

        public byte[] GetBytes()
        {
            List<byte> outBytes = new List<byte>();
            List<int> pofSets = new List<int>();
            List<string> arcStringList = new List<string>();
            Dictionary<string, List<string>> arcStrings = new Dictionary<string, List<string>>();
            var tempBE = ByteListExtension.AddAsBigEndian;
            ByteListExtension.AddAsBigEndian = true;

            outBytes.AddValue((int)eventScriptContainers.Count);
            pofSets.Add(outBytes.Count);
            outBytes.AddValue((int)8);

            for (int i = 0; i < eventScriptContainers.Count; i++)
            {
                pofSets.Add(outBytes.Count);
                outBytes.ReserveInt($"Container{i}");
            }

            for (int i = 0; i < eventScriptContainers.Count; i++)
            {
                outBytes.AlignWriter(0x4);
                outBytes.FillInt($"Container{i}", outBytes.Count);
                var evt = eventScriptContainers[i];
                outBytes.AddValue(evt.int_00);
                outBytes.AddValue(evt.scriptValue0);
                outBytes.AddValue(evt.int_08);
                outBytes.AddValue((int)evt.scripts.Count);

                pofSets.Add(outBytes.Count);
                outBytes.ReserveInt($"ContainerScripts{i}");

                outBytes.FillInt($"ContainerScripts{i}", outBytes.Count);
                for (int j = 0; j < evt.scripts.Count; j++)
                {
                    pofSets.Add(outBytes.Count);
                    outBytes.ReserveInt($"ContainerScripts{i}{j}");
                }
                for (int j = 0; j < evt.scripts.Count; j++)
                {
                    //Aligns in case the last script's data wasn't aligned. We specifically don't want to do this for the final script
                    outBytes.AlignWriter(0x4);
                    outBytes.FillInt($"ContainerScripts{i}{j}", outBytes.Count);
                    string nameReserve = $"ContainerScripts{i}{j}Name";
                    if (!arcStrings.ContainsKey(evt.scripts[j].name))
                    {
                        arcStringList.Add(evt.scripts[j].name);
                        arcStrings[evt.scripts[j].name] = new List<string> { nameReserve };
                    }
                    else
                    {
                        arcStrings[evt.scripts[j].name].Add(nameReserve);
                    }
                    pofSets.Add(outBytes.Count);
                    outBytes.ReserveInt(nameReserve);
                    outBytes.AddValue(evt.scripts[j].scriptInt_04);
                    outBytes.ReserveInt($"ContainerScripts{i}{j}Data");

                    var dataBytes = evt.scripts[j].GetDataBytes(ByteListExtension.AddAsBigEndian);
                    if (dataBytes.Length > 0)
                    {
                        pofSets.Add(outBytes.Count - 4);
                        outBytes.FillInt($"ContainerScripts{i}{j}Data", outBytes.Count);
                        outBytes.AddRange(dataBytes);
                    }
                }
            }

            //Write strings
            for (int i = 0; i < arcStringList.Count; i++)
            {
                var stringOffsets = arcStrings[arcStringList[i]];
                for (int j = 0; j < stringOffsets.Count; j++)
                {
                    outBytes.FillInt(stringOffsets[j], outBytes.Count);
                }
                outBytes.AddRange(Encoding.ASCII.GetBytes(arcStringList[i]));
                outBytes.Add(0);
            }

            //ARC enveloping
            outBytes.AlignWriter(0x4);
            var pof0Offset = outBytes.Count;
            pofSets.Sort();
            var pof0 = POF0.GenerateRawPOF0(pofSets, true);
            outBytes.AddRange(pof0);

            var arcBytes = new List<byte>();
            arcBytes.AddValue(outBytes.Count + 0x20);
            arcBytes.AddValue(pof0Offset);
            arcBytes.AddValue(pof0.Length);
            arcBytes.AddValue(0);

            arcBytes.AddValue(0);
            arcBytes.Add(0x30);
            arcBytes.Add(0x31);
            arcBytes.Add(0x30);
            arcBytes.Add(0x30);
            arcBytes.AddValue(0);
            arcBytes.AddValue(0);

            outBytes.InsertRange(0, arcBytes);

            ByteListExtension.Reset();
            return outBytes.ToArray();
        }
    }

    public class EventScriptContainer
    {
        [JsonIgnore]
        public long position;

        public int int_00 { get; set; }
        public int scriptValue0 { get; set; }
        public int int_08 { get; set; }
        [JsonIgnore]
        public int scriptCount;

        [JsonIgnore]
        public int scriptOffsets;

        public List<EventScript> scripts { get; set; } = new List<EventScript>();
    }

    public class EventScript
    {
        public string name { get; set; }

        [JsonIgnore]
        public int scriptNameOffset;
        public int scriptInt_04 { get; set; }
        [JsonIgnore]
        public int scriptDataOffset;

        //Potential script data
        //Could be abstracted out cleanly, but we're doing it this way to avoid complication
        public List<byte> bytes_Data { get; set; } = null;
        public int? int0_Data { get; set; } = null;
        public int? int1_Data { get; set; } = null;
        public float? flt_Data { get; set; } = null;
        public List<float> vec3_Data { get; set; } = null;
        public RaceData? race_Data { get; set; } = null;
        public MinigameData? minigame_Data { get; set; } = null;

        public byte[] GetDataBytes(bool getBigEndian)
        {
            List<byte> outBytes = new List<byte>();
            var temp = ByteListExtension.AddAsBigEndian;
            ByteListExtension.AddAsBigEndian = getBigEndian;
            switch (name)
            {
                //1 int16
                case "load_racer":
                case "set_bgm":
                case "msg":
                case "bgm_play":
                    outBytes.AddValue((short)int0_Data);
                    break;
                //4 bytes
                case "gold_save":
                case "load_p":
                case "load_prisoner":
                case "sunrate":
                case "sw_off":
                case "sw_on":
                case "sw_chk_off":
                case "sw_chk_on":
                    outBytes.AddRange(bytes_Data);
                    break;
                //8 bytes
                case "set":
                    outBytes.AddRange(bytes_Data);
                    break;
                //1 int32
                case "set_rescue":
                case "prison_save":
                case "spore":
                case "hoverleaf":
                case "blizzard":
                case "se":
                    outBytes.AddValue((int)int0_Data);
                    break;
                //1 float, 2 int16s
                case "bgm_vol":
                    outBytes.AddValue((float)flt_Data);
                    outBytes.AddValue((short)int0_Data);
                    outBytes.AddValue((short)int1_Data);
                    break;
                //2 int32s
                case "talk":
                case "load_stg_title":
                case "set_talk_mode":
                    outBytes.AddValue((int)int0_Data);
                    outBytes.AddValue((int)int1_Data);
                    break;
                //Float Vector3
                case "sol":
                    outBytes.AddValue(vec3_Data[0]);
                    outBytes.AddValue(vec3_Data[1]);
                    outBytes.AddValue(vec3_Data[2]);
                    break;
                //Unique structure for set_race
                case "set_race":
                    var rData = (RaceData)race_Data;
                    outBytes.AddValue(rData.aiSetting);
                    outBytes.AddValue(rData.sht_02);
                    outBytes.AddValue(rData.animalSpeed);
                    outBytes.AddValue(rData.preRaceMessageId);
                    outBytes.AddValue(rData.winRaceMessageId);
                    outBytes.AddValue(rData.loseRaceMessageId);
                    outBytes.AddValue(rData.sht_0E);
                    outBytes.AddValue(rData.animalScale);
                    outBytes.AddValue(rData.animalPosition[0]);
                    outBytes.AddValue(rData.animalPosition[1]);
                    outBytes.AddValue(rData.animalPosition[2]);
                    break;
                //Unique structure for minigame
                case "minigame":
                    var minG = (MinigameData)minigame_Data;
                    outBytes.Add(minG.bt_00);
                    outBytes.Add(minG.bt_01);
                    outBytes.Add(minG.bt_02);
                    outBytes.Add(minG.bt_03);
                    outBytes.Add(minG.timeInMinutes);
                    outBytes.Add(minG.timeInSeconds);
                    outBytes.Add(minG.bt_06);
                    outBytes.Add(minG.emblemScoreToWin);
                    break;
                //No data
                case "takeoff_suit":
                case "start_race":
                case "ms_boot":
                case "ms_fail":
                case "ms_success":
                case "load_green_ene_demo":
                case "load_o_cannon":
                case "load_darkgate":
                case "load_chicken":
                case "load_goal":
                case "load_ring":
                case "load_mgleader":
                case "load_gold":
                case "load_bomb":
                case "load_egg_suit":
                case "always_true":
                case "race":
                    break;
                //Unknown if there's data since unused
                case "wait_fake":
                case "ptcl_test":
                case "send":
                case "wait":
                case "bgm_stop":
                case "se_pos":
                    break;
                default:
                    ByteListExtension.AddAsBigEndian = temp;
                    throw new NotImplementedException();
            }
            ByteListExtension.AddAsBigEndian = temp;
            return outBytes.ToArray();
        }
    }

    public struct RaceData
    {
        /// <summary>
        /// Unknown what this does, but the non default values make the AI move erratically.
        /// 0x8 is the default for King Clippen, 0x3 is the default for Queen Rabbish
        /// </summary>
        public short aiSetting { get; set; }
        public short sht_02 { get; set; }
        public float animalSpeed { get; set; }
        public short preRaceMessageId { get; set; }
        public short winRaceMessageId { get; set; }
        public short loseRaceMessageId { get; set; }
        public short sht_0E { get; set; }

        public float animalScale { get; set; }
        public List<float> animalPosition { get; set; }
    }

    public struct MinigameData
    {
        public byte bt_00 { get; set; }
        public byte bt_01 { get; set; }
        public byte bt_02 { get; set; }
        public byte bt_03 { get; set; }

        public byte timeInMinutes { get; set; }
        public byte timeInSeconds { get; set; }
        public byte bt_06 { get; set; }
        public byte emblemScoreToWin { get; set; }
    }
}
