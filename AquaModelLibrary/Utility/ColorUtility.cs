using System.Drawing;
using System.Numerics;

namespace AquaModelLibrary.Utility
{
    public static class ColorUtility
    {
        public static Color ARGBFromRGBAVector3(float x, float y, float z)
        {
            return ARGBFromRGBAVector3(new Vector3(x, y, z));
        }

        public static Color ARGBFromRGBAVector3(Vector3 vec3)
        {
            //Limit input. It can technically be higher than this in theory, but usually it wouldn't be.
            if (vec3.X > 1.0f || vec3.X < 0)
            {
                vec3.X = 1.0f;
            }
            if (vec3.Y > 1.0f || vec3.Y < 0)
            {
                vec3.Y = 1.0f;
            }
            if (vec3.Z > 1.0f || vec3.Z < 0)
            {
                vec3.Z = 1.0f;
            }
            return Color.FromArgb(1, (int)(vec3.X * 255), (int)(vec3.Y * 255), (int)(vec3.Z * 255));
        }
        public static Color ARGBFromRGBAVector4(Vector4 vec4)
        {
            //Limit input. It can technically be higher than this in theory, but usually it wouldn't be.
            if (vec4.X > 1.0f || vec4.X < 0)
            {
                vec4.X = 1.0f;
            }
            if (vec4.Y > 1.0f || vec4.Y < 0)
            {
                vec4.Y = 1.0f;
            }
            if (vec4.Z > 1.0f || vec4.Z < 0)
            {
                vec4.Z = 1.0f;
            }
            return Color.FromArgb(1, (int)(vec4.X * 255), (int)(vec4.Y * 255), (int)(vec4.Z * 255));
        }
    }
}
