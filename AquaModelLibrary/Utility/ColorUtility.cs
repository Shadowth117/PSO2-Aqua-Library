using System.Drawing;
using System.Numerics;

namespace AquaModelLibrary.Utility
{
    public static class ColorUtility
    {
        public static Color ARGBFromRGBAVector4(Vector4 vec4)
        {
            //Limit input. It can technically be higher than this in theory, but usually it wouldn't be.
            if (vec4.X > 1.0f)
            {
                vec4.X = 1.0f;
            }
            if (vec4.Y > 1.0f)
            {
                vec4.Y = 1.0f;
            }
            if (vec4.Z > 1.0f)
            {
                vec4.Z = 1.0f;
            }
            return Color.FromArgb(1, (int)(vec4.X * 255), (int)(vec4.Y * 255), (int)(vec4.Z * 255));
        }
    }
}
