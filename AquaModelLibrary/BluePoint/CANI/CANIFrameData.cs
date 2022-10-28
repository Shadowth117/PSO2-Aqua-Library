using Reloaded.Memory.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AquaModelLibrary.BluePoint.CANI
{
    public enum CANIFrameType : ushort
    {
        Position = 0x315,
        Scale = 0x316,      //Scale?
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
        public List<ushort> frameTimeUshorts = new List<ushort>(); //Usually frame time related?? None of the repeated ushorts at the end are counted
        public List<ushort> frameDataUshorts = new List<ushort>(); 
        public List<Vector3> frameDataVec3s = new List<Vector3>(); 
        public List<Quaternion> frameDataQuats = new List<Quaternion>(); 

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

        public CANIFrameData(BufferedStreamReader sr)
        {
            variant = sr.Read<ushort>();
            entryCount = sr.Read<ushort>();
            unkShort1 = sr.Read<ushort>();
            dataOffset = sr.Read<ushort>();

            switch (variant)
            {
                case (ushort)CANIFrameType.Position:
                case (ushort)CANIFrameType.Scale:
                case (ushort)CANIFrameType.Rotation:
                case (ushort)CANIFrameType.x318:
                case (ushort)CANIFrameType.x319:
                    break;
                case (ushort)CANIFrameType.vertMorphData0:
                    userData0 = sr.Read<ushort>();
                    userData1 = sr.Read<ushort>();
                    userData2 = sr.Read<ushort>();
                    break;
                case (ushort)CANIFrameType.vertMorphData1:
                case (ushort)CANIFrameType.x41C:
                case (ushort)CANIFrameType.x41D:
                case (ushort)CANIFrameType.x41E:
                case (ushort)CANIFrameType.x40F:
                case (ushort)CANIFrameType.x410:
                    userData0 = sr.Read<ushort>();
                    break;
                default:
                    Debug.WriteLine($"new Variant detected: {variant:X}");
                    break;
            }

        }
    }
}
