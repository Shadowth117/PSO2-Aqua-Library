using System.Drawing;
using System.Net;

namespace AquaModelLibrary.Data.Ninja.Model
{
    public class NinjaModelCommon
    {
        /// <summary>
        /// For ARGB8888_32
        /// </summary>
        public static Color ReadColorARGB8888_32(bool bigEndian, bool SADXColorReverse, byte[] colorBytes)
        {
            switch (bigEndian)
            {
                case true:
                    switch (SADXColorReverse)
                    {
                        case true:
                            return Color.FromArgb(colorBytes[3], colorBytes[0], colorBytes[1], colorBytes[2]);
                        case false:
                            return Color.FromArgb(colorBytes[0], colorBytes[1], colorBytes[2], colorBytes[3]);
                    }
                case false:
                    switch (SADXColorReverse)
                    {
                        case true:
                            return Color.FromArgb(colorBytes[0], colorBytes[3], colorBytes[2], colorBytes[1]);
                        case false:
                            return Color.FromArgb(colorBytes[3], colorBytes[2], colorBytes[1], colorBytes[0]);
                    }
            }
        }

        public static byte[] GetBytesColorARGB8888_32(bool bigEndian, bool SADXColorReverse, Color color)
        {
            switch (bigEndian)
            {
                case true:
                    switch (SADXColorReverse)
                    {
                        case true:
                            return [color.R, color.G, color.B, color.A];
                        case false:
                            return [color.A, color.R, color.G, color.B];
                    }
                case false:
                    switch (SADXColorReverse)
                    {
                        case true:
                            return [color.A, color.B, color.G, color.R];
                        case false:
                            return [color.B, color.G, color.R, color.A];
                    }
            }
        }

        public static Color ReadColorRGB565(bool bigEndian, ushort value)
        {
            var r = value >> 11;
            var g = (value >> 5) & 0x3F;
            var b = value & 0x1F;
            return Color.FromArgb(
                r << 3 | r >> 2,
                g << 2 | g >> 4,
                b << 3 | b >> 2
                );
        }

        public static ushort GetUshortColorRGB565(Color color)
        {
            return (ushort)(((color.R >> 3) << 11) | ((color.G >> 2) << 5) | (color.B >> 3));
        }

        public static Color ReadColorARGB4444(bool bigEndian, ushort value)
        {
            int a = value >> 12;
            int r = (value >> 8) & 0xF;
            int g = (value >> 4) & 0xF;
            int b = value & 0xF;
            return Color.FromArgb(
                a | (a << 4),
                r | (r << 4),
                g | (g << 4),
                b | (b << 4)
                );
        }

        public static ushort GetUshortColorARGB4444(Color color)
        {
            return (ushort)(((color.A >> 4) << 12) | ((color.R >> 4) << 8) | ((color.G >> 4) << 4) | (color.B >> 4));
        }
    }
}
