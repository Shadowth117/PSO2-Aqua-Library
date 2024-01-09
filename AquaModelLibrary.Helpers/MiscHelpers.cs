using System.Numerics;
using System.Runtime.InteropServices;

namespace AquaModelLibrary.Helpers
{
    public class MiscHelpers
    {
        public static Vector2 UshortsToVector2(ushort[] ushorts)
        {
            return new Vector2((float)((double)ushorts[0] / ushort.MaxValue), (float)((double)ushorts[1] / ushort.MaxValue));
        }

        public static bool IsEqualByteArray(byte[] bArr0, byte[] bArr1)
        {
            if (bArr0.Length != bArr1.Length)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < bArr0.Length; i++)
                {
                    if (bArr0[i] != bArr1[i])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static bool IsEqualShortArray(short[] sArr0, short[] sArr1)
        {
            if (sArr0.Length != sArr1.Length)
            {
                return false;
            }
            else
            {
                for (int i = 0; i < sArr0.Length; i++)
                {
                    if (sArr0[i] != sArr1[i])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public List<Vector2> getUVFlipped(List<Vector2> uvList)
        {
            List<Vector2> uvs = uvList.ToList();

            for (int i = 0; i < uvs.Count; i++)
            {
                Vector2 uv = uvs[i];
                uv.Y = -uv.Y;
                uvs[i] = uv;
            }

            return uvs;
        }
    }
}
