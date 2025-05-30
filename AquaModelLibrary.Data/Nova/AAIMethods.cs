using AquaModelLibrary.Data.DataTypes.SetLengthStrings;
using AquaModelLibrary.Data.PSO2.Aqua;
using AquaModelLibrary.Data.Nova.Structures;
using Reloaded.Memory.Streams;
using System.Diagnostics;
using static AquaModelLibrary.Data.Nova.AXSConstants;
using AquaModelLibrary.Helpers.MathHelpers;

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
                            case 0x24: 
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
                List<List<RotationKey>> rotFramesListList = new List<List<RotationKey>>();
                

                for (int i = 0; i < offsetTimes.Count; i++)
                {
                    List<RotationKey> rotFrameList = new List<RotationKey>();
                    streamReader.Seek(offsetTimes[i].offset + offsetTimesStart + i * 8, SeekOrigin.Begin);
                    var position = streamReader.Position;
                    //Debug.WriteLine($"OffsetTime {i} start: {position:X}");
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
                        rotFrameList.Add(streamReader.Read<RotationKey>());
                        //Debug.WriteLine($"U {rotFrameList[j].usht_0 / 65535.0} {rotFrameList[j].usht_1 / 65535.0} {rotFrameList[j].usht_2 / 65535.0}  {rotFrameList[j].usht_3 / 65535.0}");
                        Debug.WriteLine($"{i} {j}");
                        PrintRotKey(rotFrameList[j]);
                        //Debug.WriteLine($"U {GetBAMSHigh(rotFrameList[j].usht_2)} {GetBAMSHigh(rotFrameList[j].usht_3)} {GetBAMSGimbal(rotFrameList[j].usht_0)} ");
                        streamReader.Seek(-0x8, SeekOrigin.Current);
                        //Debug.WriteLine($"S {streamReader.Read<short>() / 32767.0} {streamReader.Read<short>() / 32767.0} {streamReader.Read<short>() / 32767.0}  {streamReader.Read<short>() / 32767.0}");
                    }
                    rotFramesListList.Add(rotFrameList);
                }
            }
            return null;
        }

        public static void PrintRotKey(RotationKey rot)
        {
            Debug.WriteLine($"{GetBAMSHigh(rot.usht_2)} {GetBAMSHigh(rot.usht_3)} {GetBAMSGimbal(rot.usht_0)} ");
            Debug.WriteLine($"{GetBAMSHigh(rot.usht_2)} {GetBAMSGimbal(rot.usht_0)} {GetBAMSHigh(rot.usht_3)} ");
            Debug.WriteLine($"{GetBAMSHigh(rot.usht_3)} {GetBAMSHigh(rot.usht_2)} {GetBAMSGimbal(rot.usht_0)} ");
            Debug.WriteLine($"{GetBAMSHigh(rot.usht_3)} {GetBAMSGimbal(rot.usht_0)} {GetBAMSHigh(rot.usht_2)} ");
            Debug.WriteLine($"{GetBAMSGimbal(rot.usht_0)} {GetBAMSHigh(rot.usht_2)} {GetBAMSHigh(rot.usht_3)}");
            Debug.WriteLine($"{GetBAMSGimbal(rot.usht_0)} {GetBAMSHigh(rot.usht_3)} {GetBAMSHigh(rot.usht_2)} ");

            Debug.WriteLine("Q");

            var quat0 = MathExtras.EulerToQuaternion(new System.Numerics.Vector3((float)GetBAMSHigh(rot.usht_2), (float)GetBAMSHigh(rot.usht_3), (float)GetBAMSGimbal(rot.usht_0)), RotationOrder.XYZ);
            var quat1 = MathExtras.EulerToQuaternion(new System.Numerics.Vector3((float)GetBAMSHigh(rot.usht_2), (float)GetBAMSHigh(rot.usht_3), (float)GetBAMSGimbal(rot.usht_0)), RotationOrder.XZY);
            var quat2 = MathExtras.EulerToQuaternion(new System.Numerics.Vector3((float)GetBAMSHigh(rot.usht_2), (float)GetBAMSHigh(rot.usht_3), (float)GetBAMSGimbal(rot.usht_0)), RotationOrder.YXZ);
            var quat3 = MathExtras.EulerToQuaternion(new System.Numerics.Vector3((float)GetBAMSHigh(rot.usht_2), (float)GetBAMSHigh(rot.usht_3), (float)GetBAMSGimbal(rot.usht_0)), RotationOrder.YZX);
            var quat4 = MathExtras.EulerToQuaternion(new System.Numerics.Vector3((float)GetBAMSHigh(rot.usht_2), (float)GetBAMSHigh(rot.usht_3), (float)GetBAMSGimbal(rot.usht_0)), RotationOrder.ZXY);
            var quat5 = MathExtras.EulerToQuaternion(new System.Numerics.Vector3((float)GetBAMSHigh(rot.usht_2), (float)GetBAMSHigh(rot.usht_3), (float)GetBAMSGimbal(rot.usht_0)), RotationOrder.ZYX);

            var quat01 = MathExtras.EulerToQuaternion(new System.Numerics.Vector3((float)GetBAMSHigh(rot.usht_3), (float)GetBAMSHigh(rot.usht_2), (float)GetBAMSGimbal(rot.usht_0)), RotationOrder.XYZ);
            var quat02 = MathExtras.EulerToQuaternion(new System.Numerics.Vector3((float)GetBAMSHigh(rot.usht_3), (float)GetBAMSHigh(rot.usht_2), (float)GetBAMSGimbal(rot.usht_0)), RotationOrder.XZY);
            var quat03 = MathExtras.EulerToQuaternion(new System.Numerics.Vector3((float)GetBAMSHigh(rot.usht_3), (float)GetBAMSHigh(rot.usht_2), (float)GetBAMSGimbal(rot.usht_0)), RotationOrder.YXZ);
            var quat04 = MathExtras.EulerToQuaternion(new System.Numerics.Vector3((float)GetBAMSHigh(rot.usht_3), (float)GetBAMSHigh(rot.usht_2), (float)GetBAMSGimbal(rot.usht_0)), RotationOrder.YXZ);
            var quat05 = MathExtras.EulerToQuaternion(new System.Numerics.Vector3((float)GetBAMSHigh(rot.usht_3), (float)GetBAMSHigh(rot.usht_2), (float)GetBAMSGimbal(rot.usht_0)), RotationOrder.YXZ);
            var quat06 = MathExtras.EulerToQuaternion(new System.Numerics.Vector3((float)GetBAMSHigh(rot.usht_3), (float)GetBAMSHigh(rot.usht_2), (float)GetBAMSGimbal(rot.usht_0)), RotationOrder.YXZ);

            Debug.WriteLine($"{quat0.X} {quat0.Y} {quat0.Z} {quat0.W}");
            Debug.WriteLine($"{quat1.X} {quat1.Y} {quat1.Z} {quat1.W}");
            Debug.WriteLine($"{quat2.X} {quat2.Y} {quat2.Z} {quat2.W}");
            Debug.WriteLine($"{quat3.X} {quat3.Y} {quat3.Z} {quat3.W}");
            Debug.WriteLine($"{quat4.X} {quat4.Y} {quat4.Z} {quat4.W}");
            Debug.WriteLine($"{quat5.X} {quat5.Y} {quat5.Z} {quat5.W}");
            Debug.WriteLine($"");
            Debug.WriteLine($"{quat01.X} {quat01.Y} {quat01.Z} {quat01.W}");
            Debug.WriteLine($"{quat02.X} {quat02.Y} {quat02.Z} {quat02.W}");
            Debug.WriteLine($"{quat03.X} {quat03.Y} {quat03.Z} {quat03.W}");
            Debug.WriteLine($"{quat04.X} {quat04.Y} {quat04.Z} {quat04.W}");
            Debug.WriteLine($"{quat05.X} {quat05.Y} {quat05.Z} {quat05.W}");
            Debug.WriteLine($"{quat06.X} {quat06.Y} {quat06.Z} {quat06.W}");
        }

        public static double GetBAMSHigh(ushort value)
        {
            return (value / 65535.0) * 360.0 - 180.0;
        }

        public static double GetBAMSGimbal(ushort value)
        {
            return (value / 65535.0) * 180.0 - 90.0;
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
