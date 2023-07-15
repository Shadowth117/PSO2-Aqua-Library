using AquaModelLibrary.AquaMethods;
using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SystemHalf;

namespace AquaModelLibrary.BluePoint.CANI
{
    public enum CANIFrameType : ushort
    {
        Position = 0x315,   //Vector3
        Scale = 0x316,      //Scale? Vector3 of Halfs?
        Rotation = 0x317,
        x318 = 0x318,      //Another Vector3 of halfs
        x319 = 0x319,      //Yet Another Vector3 of halfs
        vertMorphData0 = 0x60B,
        vertMorphData1 = 0x41B, //Seems to always follow 0x60B
        x41C = 0x41C,
        x41D = 0x41D,
        x41E = 0x41E,
        x40F = 0x40F,
        x410 = 0x410,
        x60C = 0x610,
        x411 = 0x411, //Comparable to 0x41B for 0x60C instead. Camera only?
    }

    public class CANIFrameData
    {
        public List<ushort> frameDataUshorts = new List<ushort>();
        public List<ushort> frameDataUshorts2 = new List<ushort>();
        public List<Vector3> frameDataVec3s = new List<Vector3>(); 
        public List<Quaternion> frameDataQuats = new List<Quaternion>();
        public List<Half> frameDataHalfs = new List<Half>();  
        public int _baseOffset = -1;

        public ushort variant;    //Type? 
        public ushort entryCount;
        public ushort unkShort1;  //Almost always 3?? Idk
        public ushort dataOffset;     //Relative to start of CANIFrameInfos

        //Extended types
        public ushort userData0;
        public ushort userData1;
        public ushort userData2;

        public CANIFrameData()
        {

        }

        public CANIFrameData(BufferedStreamReader sr, int baseOffset)
        {
            _baseOffset = baseOffset;
            variant = sr.Read<ushort>();
            entryCount = sr.Read<ushort>();
            unkShort1 = sr.Read<ushort>();
            dataOffset = sr.Read<ushort>(); //Offset is relative to the start of the frameData definitions

            long bookmark = -1;
            long bookmark2 = -1;
            switch (variant)
            {
                case (ushort)CANIFrameType.Position:
                case (ushort)CANIFrameType.Scale:
                case (ushort)CANIFrameType.Rotation:
                case (ushort)CANIFrameType.x318:
                case (ushort)CANIFrameType.x319:
                    bookmark = sr.Position();
                    break;
                case (ushort)CANIFrameType.vertMorphData0:
                    userData0 = sr.Read<ushort>();
                    userData1 = sr.Read<ushort>();
                    userData2 = sr.Read<ushort>();
                    bookmark = sr.Position();
                    break;
                case (ushort)CANIFrameType.vertMorphData1:
                case (ushort)CANIFrameType.x41C:
                case (ushort)CANIFrameType.x41D:
                case (ushort)CANIFrameType.x41E:
                case (ushort)CANIFrameType.x40F:
                case (ushort)CANIFrameType.x410:
                    userData0 = sr.Read<ushort>();
                    bookmark = sr.Position();
                    break;
                default:
                    Debug.WriteLine($"new Variant detected: {variant:X}");
                    throw new Exception();
            }

            sr.Seek(dataOffset + _baseOffset, System.IO.SeekOrigin.Begin);
            switch (variant)
            {
                case (ushort)CANIFrameType.Position:
                case (ushort)CANIFrameType.Scale:
                    for(int i = 0; i < entryCount; i++)
                    {
                        frameDataUshorts.Add(sr.Read<ushort>());
                    }
                    AquaGeneralMethods.AlignReader(sr, 0x10);

                    //A little unclear what this actually is. What we can say is the count is the buffer length is entryCount * 6, ie 3 16 bit values per entry.
                    //The odd thing is that these buffers have clear separations. The first half is almost always all 003C, 1 as a half. Near the middle, there are USUALLY padding esque values of 0000 for less than 0x10 of data.
                    //Lastly is another set of values which it is not readily apparent if these are even Half values or simply 16 bit integers. Either way, there is clear confusion here.
                    //In some files, such as very small ones for static or nearly static entities, the first type of data is all that can be found.
                    bookmark2 = sr.Position();
                    for(int i = 0; i < entryCount; i++)
                    {
                        frameDataVec3s.Add(new Vector3(sr.Read<Half>(), sr.Read<Half>(), sr.Read<Half>()));
                    }
                    //Read a second pass as shorts for analysis
                    sr.Seek(bookmark2, System.IO.SeekOrigin.Begin);
                    for(int i = 0; i < entryCount; i++)
                    {
                        frameDataUshorts2.Add(sr.Read<ushort>());
                        frameDataUshorts2.Add(sr.Read<ushort>());
                        frameDataUshorts2.Add(sr.Read<ushort>());
                    }
                    break;
                case (ushort)CANIFrameType.Rotation:
                case (ushort)CANIFrameType.x318:
                    for (int i = 0; i < entryCount; i++)
                    {
                        frameDataUshorts.Add(sr.Read<ushort>());
                    }
                    AquaGeneralMethods.AlignReader(sr, 0x10);

                    //Similar confusion
                    bookmark2 = sr.Position();
                    for (int i = 0; i < entryCount; i++)
                    {
                        frameDataVec3s.Add(new Vector3(sr.Read<Half>(), sr.Read<Half>(), sr.Read<Half>()));
                    }
                    //Read a second pass as shorts for analysis
                    sr.Seek(bookmark2, System.IO.SeekOrigin.Begin);
                    for (int i = 0; i < entryCount; i++)
                    {
                        frameDataUshorts2.Add(sr.Read<ushort>());
                        frameDataUshorts2.Add(sr.Read<ushort>());
                        frameDataUshorts2.Add(sr.Read<ushort>());
                    }
                    break;
                case (ushort)CANIFrameType.x319:
                case (ushort)CANIFrameType.vertMorphData0:
                case (ushort)CANIFrameType.vertMorphData1:
                case (ushort)CANIFrameType.x41C:
                case (ushort)CANIFrameType.x41D:
                case (ushort)CANIFrameType.x41E:
                case (ushort)CANIFrameType.x40F:
                case (ushort)CANIFrameType.x410:
                default:
                    throw new Exception();
            }
            sr.Seek(bookmark, System.IO.SeekOrigin.Begin);
        }

    }
}
