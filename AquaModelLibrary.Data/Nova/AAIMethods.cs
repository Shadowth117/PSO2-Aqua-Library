using AquaModelLibrary.Data.DataTypes.SetLengthStrings;
using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.Nova.Structures;
using Reloaded.Memory.Streams;
using System.Diagnostics;
using static AquaModelLibrary.Data.Nova.AXSConstants;

namespace AquaModelLibrary.Data.Nova
{
    public static class AAIMethods
    {
        public static AquaMotion ReadAAI(string filePath)
        {

            using (MemoryStream stream = new MemoryStream(File.ReadAllBytes(filePath)))
            using (var streamReader = new BufferedStreamReader<MemoryStream>(stream, 8192))
            {
                int fType = streamReader.Read<int>();
                if (fType != FAA)
                {
                    return null;
                }
                streamReader.Seek(0xC, SeekOrigin.Current);

                int innerFtype = streamReader.Read<int>();
                int len = streamReader.Read<int>();
                streamReader.Seek(len - 0x8, SeekOrigin.Current);

                int animDataMagic = streamReader.Read<int>();
                ushort nodeCount = streamReader.Read<ushort>();
                ushort sht_06 = streamReader.Read<ushort>();
                ushort sht_08 = streamReader.Read<ushort>();
                ushort sht_0C = streamReader.Read<ushort>();
                int timeCount = streamReader.Read<int>();

                float finalFrame = streamReader.Read<float>();
                int unkAddress0 = streamReader.Read<int>();
                int unkAddress1 = streamReader.Read<int>();
                int unkAddress2 = streamReader.Read<int>();

                int unkAddress3 = streamReader.Read<int>();
                int unkAddress4 = streamReader.Read<int>();
                int unkAddress5 = streamReader.Read<int>();
                int unkAddress6 = streamReader.Read<int>();

                int unkAddress7 = streamReader.Read<int>();
                int unkAddress8 = streamReader.Read<int>();
                int nodeDefEndAddress = streamReader.Read<int>();
                int unkAddress9 = streamReader.Read<int>();

                int unkAddress10 = streamReader.Read<int>();
                int int_44 = streamReader.Read<int>();

                int clumpCount = 0;
                List<AnimDefinitionNode> nodes = new List<AnimDefinitionNode>();
                int ct14 = 0;
                int ct24 = 0;
                int ct30 = 0;
                int ct34 = 0;
                int ct44 = 0;
                for (int i = 0; i < nodeCount; i++)
                {
                    AnimDefinitionNode node = new AnimDefinitionNode();
                    node.header0 = streamReader.Read<ushort>();
                    node.dataCount = streamReader.Read<ushort>();
                    node.len = streamReader.Read<int>();
                    node.nameData = streamReader.Read<PSO2String>();
                    node.name = node.nameData.GetString();
                    for (int j = 0; j < node.dataCount; j++)
                    {
                        clumpCount++;
                        var test = streamReader.Position.ToString("X");
                        DataClump dc = new DataClump();
                        dc.dcStart = streamReader.Read<DataClumpStart>();
                        switch (dc.dcStart.dcType)
                        {
                            case 0x14:
                                ct14++;
                                dc.dc = streamReader.Read<DataClump14>();
                                break;
                            case 0x24: //Should be for typical keyframe types
                                ct24++;
                                dc.dc = streamReader.Read<DataClump24>();
                                break;
                            case 0x30:
                                ct30++;
                                dc.dc = streamReader.Read<DataClump30>();
                                break;
                            case 0x34:
                                ct34++;
                                dc.dc = streamReader.Read<DataClump34>();
                                dc.dcString = ((DataClump34)dc.dc).clumpName.GetString();
                                break;
                            case 0x44:
                                ct44++;
                                dc.dc = streamReader.Read<DataClump44>();
                                dc.dcString = ((DataClump44)dc.dc).clumpName.GetString();
                                break;
                            default:
                                Debug.WriteLine($"clumpSize {dc.dcStart.dcType.ToString("X")} at {streamReader.Position.ToString("X")} is unexpected!");
                                break;
                        }
                        node.data.Add(dc);
                    }
                    nodes.Add(node);
                }

                //return null;

                var offsetTimesStart = streamReader.Position;
                var offsetTimes = streamReader.ReadOffsetTimeSets(timeCount);
                List<List<NodeOffsetSet>> setsList = new List<List<NodeOffsetSet>>();
                List<Dictionary<int, RotationKey>> framesDictList = new List<Dictionary<int, RotationKey>>();

                for (int i = 0; i < offsetTimes.Count; i++)
                {
                    streamReader.Seek(offsetTimes[i].offset + offsetTimesStart + i * 8, SeekOrigin.Begin);
                    var position = streamReader.Position;
                    Debug.WriteLine($"OffsetTime {i} start: {position:X}");
                    int keyNodeCount = streamReader.Read<int>();

                    //Sometimes, they just put the keydata right here? Read the keydata when this is figured out
                    if (keyNodeCount > 0xFF)
                    {
                        throw new Exception();
                    }

                    List<NodeOffsetSet> sets = new List<NodeOffsetSet>();
                    for (int j = 0; j < keyNodeCount; j++)
                    {
                        NodeOffsetSet set = new NodeOffsetSet() { nodeId = streamReader.Read<ushort>(), offset = streamReader.Read<ushort>() };
                        if (j < nodes.Count)
                        {
                            set.nodeName = nodes[j].name;
                        }
                        else
                        {
                            set.nodeName = $"node_{j}";
                        }
                        sets.Add(set);
                    }
                    setsList.Add(sets);

                    for (int j = 0; j < sets.Count; j++)
                    {

                    }
                }
            }
            return null;
        }

        public static List<OffsetTimeSet> ReadOffsetTimeSets(this BufferedStreamReader<MemoryStream> streamReader, int timeCount)
        {
            List<OffsetTimeSet> sets = new List<OffsetTimeSet>();
            OffsetTimeSet set0 = new OffsetTimeSet() { offset = streamReader.Read<int>(), time = streamReader.Read<float>() };

            //Note a potentially unintended read
            if (timeCount == 0)
            {
                Debug.WriteLine($"Warning, timeCount is {timeCount}, set0 values are offset:{set0.offset:X} time:{set0.time}");
            }
            sets.Add(set0);

            for (int i = 0; i < timeCount - 1; i++) //Already Reading one in before this
            {
                OffsetTimeSet set = new OffsetTimeSet() { offset = streamReader.Read<int>(), time = streamReader.Read<float>() };
                sets.Add(set);
            }

            return sets;
        }
    }
}
