using System.Numerics;

namespace AquaModelLibrary.Helpers
{
    public class MiscHelpers
    {
        public static Vector2 UshortsToVector2(ushort[] ushorts)
        {
            return new Vector2((float)((double)ushorts[0] / ushort.MaxValue), (float)((double)ushorts[1] / ushort.MaxValue));
        }

        public static float[] VectorAsArray(Vector3 vec3)
        {
            return new float[] { vec3.X, vec3.Y, vec3.Z };
        }
    }
}
