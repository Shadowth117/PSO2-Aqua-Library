using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Reloaded.Memory;
using Reloaded.Memory.Streams;

namespace AquaModelLibrary
{
    public static class BigEndianHelper
    {
        public static bool _active = false;
        public static Vector2 ReadBEV2(this BufferedStreamReader streamReader, bool active)
        {
            return new Vector2(streamReader.ReadBE<float>(active), streamReader.ReadBE<float>(active));
        }

        public static Vector3 ReadBEV3(this BufferedStreamReader streamReader, bool active)
        {
            return new Vector3(streamReader.ReadBE<float>(active), streamReader.ReadBE<float>(active), streamReader.ReadBE<float>(active));
        }

        public static Vector4 ReadBEV4(this BufferedStreamReader streamReader, bool active)
        {
            return new Vector4(streamReader.ReadBE<float>(active), streamReader.ReadBE<float>(active), streamReader.ReadBE<float>(active), streamReader.ReadBE<float>(active));
        }

        public static T ReadBE<T>(this BufferedStreamReader streamReader, bool active) where T : unmanaged
        {
            if (active)
            {
                return streamReader.ReadBigEndianPrimitive<T>();
            }
            else
            {
                return streamReader.Read<T>();
            }
        }
        public static Vector2 ReadBEV2(this BufferedStreamReader streamReader)
        {
            return new Vector2(streamReader.ReadBE<float>(), streamReader.ReadBE<float>());
        }

        public static Vector3 ReadBEV3(this BufferedStreamReader streamReader)
        {
            return new Vector3(streamReader.ReadBE<float>(), streamReader.ReadBE<float>(), streamReader.ReadBE<float>());
        }

        public static Vector4 ReadBEV4(this BufferedStreamReader streamReader)
        {
            return new Vector4(streamReader.ReadBE<float>(), streamReader.ReadBE<float>(), streamReader.ReadBE<float>(), streamReader.ReadBE<float>());
        }

        public static T ReadBE<T>(this BufferedStreamReader streamReader) where T : unmanaged
        {
            if (_active)
            {
                return streamReader.ReadBigEndianPrimitive<T>();
            }
            else
            {
                return streamReader.Read<T>();
            }
        }
    }
}
