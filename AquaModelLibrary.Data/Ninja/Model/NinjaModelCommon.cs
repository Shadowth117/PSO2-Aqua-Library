using System.Drawing;

namespace AquaModelLibrary.Data.Ninja.Model
{
    public class NinjaModelCommon
    {
        public static Color ReadColor(bool bigEndian, bool GCColorReverse, byte[] colorBytes)
        {
            switch (bigEndian)
            {
                case true:
                    switch (GCColorReverse)
                    {
                        case true:
                            return Color.FromArgb(colorBytes[3], colorBytes[0], colorBytes[1], colorBytes[2]);
                        case false:
                            return Color.FromArgb(colorBytes[0], colorBytes[1], colorBytes[2], colorBytes[3]);
                    }
                case false:
                    switch (GCColorReverse)
                    {
                        case true:
                            return Color.FromArgb(colorBytes[0], colorBytes[3], colorBytes[2], colorBytes[1]);
                        case false:
                            return Color.FromArgb(colorBytes[3], colorBytes[2], colorBytes[1], colorBytes[0]);
                    }
            }
        }
    }
}
